//******************************************************************************************************
//  LegacySoapSecurityTests.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/08/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Gemstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.PhasorProtocols.UnitTests;

/// <summary>
/// Verifies the deserialization-hardening controls on <see cref="LegacySoapDeserializer"/>:
/// the type allowlist (fail closed for gadget types) and the XXE/DTD protections.
/// </summary>
[TestClass]
public class LegacySoapSecurityTests
{
    // CLR nsassem namespace for a System.Collections gadget primitive that lives outside the protocol assemblies.
    private const string HashtableNsUri = "http://schemas.microsoft.com/clr/nsassem/System.Collections/System.Private.CoreLib";

    [TestMethod]
    public void SafeBinder_ResolvesLegitimateProtocolTypes_IncludingLegacyGsfNames()
    {
        // Native Gemstone name resolves.
        Assert.AreEqual(
            typeof(Gemstone.PhasorProtocols.IEEE1344.ConfigurationFrame),
            LegacySoapDeserializer.SafeBinder.BindToType("Gemstone.PhasorProtocols", "Gemstone.PhasorProtocols.IEEE1344.ConfigurationFrame"),
            "Native protocol type should resolve through the safe binder.");

        // Legacy GSF name still translates and resolves.
        Assert.AreEqual(
            typeof(Gemstone.PhasorProtocols.IEEE1344.ConfigurationFrame),
            LegacySoapDeserializer.SafeBinder.BindToType("GSF.PhasorProtocols", "GSF.PhasorProtocols.IEEE1344.ConfigurationFrame"),
            "Legacy GSF protocol name should translate and resolve through the safe binder.");

        // EE enum in the Gemstone.Numeric assembly resolves, including via its legacy GSF.Units.EE / GSF.Core name.
        Assert.AreEqual(
            typeof(Gemstone.Numeric.EE.LineFrequency),
            LegacySoapDeserializer.SafeBinder.BindToType("GSF.Core", "GSF.Units.EE.LineFrequency"),
            "EE enum should resolve through the safe binder via its legacy name.");
    }

    [TestMethod]
    public void SafeBinder_FailsClosed_ForTypesOutsideProtocolAssemblies()
    {
        // Sanity: the UNRESTRICTED legacy binder will happily resolve this gadget primitive, proving it is
        // reachable in the loaded app domain — i.e., the allowlist (not mere absence) is the active control.
        Assert.IsNotNull(
            Serialization.LegacyBinder.BindToType("System.Private.CoreLib", "System.Collections.Hashtable"),
            "Precondition: the broad legacy binder resolves Hashtable, so the allowlist is what must block it.");

        // The safe binder refuses it.
        Assert.IsNull(
            LegacySoapDeserializer.SafeBinder.BindToType("System.Private.CoreLib", "System.Collections.Hashtable"),
            "Safe binder must reject types outside the phasor-protocol assemblies.");
    }

    [TestMethod]
    public void Deserialize_FailsClosed_OnGadgetTypeBeforeInstantiation()
    {
        // A well-formed SOAP envelope whose root names a disallowed type (stand-in for a deserialization gadget).
        string malicious =
            "<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
            "xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
            "<SOAP-ENV:Body>" +
            $"<a1:Hashtable id=\"ref-1\" xmlns:a1=\"{HashtableNsUri}\">" +
            "<LoadFactor>0.72</LoadFactor>" +
            "</a1:Hashtable>" +
            "</SOAP-ENV:Body></SOAP-ENV:Envelope>";

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(malicious));

        SerializationException ex = Assert.ThrowsException<SerializationException>(
            () => LegacySoapDeserializer.Deserialize(stream),
            "Deserializing a disallowed root type must fail closed.");

        StringAssert.Contains(ex.Message, "Hashtable",
            "The failure should name the rejected type, confirming it was blocked at resolution (Pass 1) before any allocation.");
    }

    [TestMethod]
    public void Deserialize_WithExplicitBroadBinder_StillResolvesType_ProvingAllowlistIsTheControl()
    {
        // Same payload, but force the unrestricted binder. Resolution should now SUCCEED (get past Pass 1) and
        // fail later for a different reason (Hashtable lacks the expected graph shape / ctor handling) — never
        // with the "Could not resolve type" message. This isolates the allowlist as the security control.
        string payload =
            "<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
            "xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
            "<SOAP-ENV:Body>" +
            $"<a1:Hashtable id=\"ref-1\" xmlns:a1=\"{HashtableNsUri}\">" +
            "<LoadFactor>0.72</LoadFactor>" +
            "</a1:Hashtable>" +
            "</SOAP-ENV:Body></SOAP-ENV:Envelope>";

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(payload));

        try
        {
            LegacySoapDeserializer.Deserialize(stream, Serialization.LegacyBinder);
            // If it somehow succeeds, that's fine for this test's purpose — the point is it didn't fail at resolution.
        }
        catch (Exception ex)
        {
            Assert.IsFalse(ex.Message.Contains("Could not resolve type"),
                $"With the broad binder the type should resolve; failure came from resolution instead: {ex.Message}");
        }
    }

    [TestMethod]
    public void Deserialize_RejectsDtd_ToPreventXxeAndEntityExpansion()
    {
        // Classic XXE shape: a DTD declaring an external entity. With DtdProcessing.Prohibit the reader rejects
        // the document the moment it sees the DOCTYPE — the entity is never defined, let alone resolved.
        string xxe =
            "<?xml version=\"1.0\"?>" +
            "<!DOCTYPE foo [ <!ENTITY xxe SYSTEM \"file:///c:/windows/win.ini\"> ]>" +
            "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
            "<SOAP-ENV:Body>&xxe;</SOAP-ENV:Body></SOAP-ENV:Envelope>";

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xxe));

        Assert.ThrowsException<XmlException>(
            () => LegacySoapDeserializer.Deserialize(stream),
            "A document containing a DTD must be rejected outright (no XXE / entity-expansion processing).");
    }
}
