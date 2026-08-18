//******************************************************************************************************
//  LegacySoapSerializerTests.cs - Gbtc
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
//  05/28/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gemstone.PhasorProtocols.IEEE1344;
using Gemstone.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.PhasorProtocols.UnitTests;

[TestClass]
public class LegacySoapSerializerTests
{
    private static readonly string s_examplesFolder = @"C:\Users\rcarroll\Desktop\Captures\Examples";

    [TestMethod]
    public void Serialize_ShelbyIeee1344_RoundTrips()
    {
        string path = Path.Combine(s_examplesFolder, "Shelby IEEE1344.xml");
        SkipIfMissing(path);

        IConfigurationFrame original = LoadConfigurationFrame(path);
        IConfigurationFrame roundTripped = RoundTrip(original);

        Assert.IsInstanceOfType(roundTripped, original.GetType());
        AssertFrameEquivalent(original, roundTripped);
    }

    [TestMethod]
    public void Serialize_RhodesC37118_RoundTrips()
    {
        string path = Path.Combine(s_examplesFolder, "RHODES_DFR1.configuration.xml");
        SkipIfMissing(path);

        IConfigurationFrame original = LoadConfigurationFrame(path);
        IConfigurationFrame roundTripped = RoundTrip(original);

        Assert.IsInstanceOfType(roundTripped, typeof(ConfigurationFrame2));
        AssertFrameEquivalent(original, roundTripped);
    }

    [TestMethod]
    public void Serialize_LargeMultiCellFile_RoundTrips()
    {
        string path = Path.Combine(s_examplesFolder, "ISO_NE_Troubleshooting_Config.xml");
        SkipIfMissing(path);

        IConfigurationFrame original = LoadConfigurationFrame(path);
        IConfigurationFrame roundTripped = RoundTrip(original);

        AssertFrameEquivalent(original, roundTripped);
        Assert.AreEqual(47, roundTripped.Cells.Count);

        // Verify that cross-references survived round-trip with stable identity
        foreach (IConfigurationCell cell in roundTripped.Cells)
            Assert.AreSame(roundTripped, cell.Parent, $"Cell '{cell.StationName}' parent did not survive round-trip.");
    }

    [TestMethod]
    public void Serialize_AllExampleFiles_RoundTrip()
    {
        if (!Directory.Exists(s_examplesFolder))
        {
            Assert.Inconclusive($"Examples folder not present: {s_examplesFolder}");
            return;
        }

        string[] files = Directory.GetFiles(s_examplesFolder, "*.xml");
        Assert.IsTrue(files.Length > 0, "Examples folder is empty.");

        int succeeded = 0;
        List<string> failures = [];

        foreach (string file in files)
        {
            try
            {
                IConfigurationFrame original = LoadConfigurationFrame(file);
                IConfigurationFrame roundTripped = RoundTrip(original);
                AssertFrameEquivalent(original, roundTripped);
                succeeded++;
            }
            catch (Exception ex)
            {
                failures.Add($"{Path.GetFileName(file)}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        System.Console.WriteLine($"Round-tripped {succeeded}/{files.Length} example files.");

        if (failures.Count > 0)
            Assert.Fail($"{failures.Count} file(s) failed:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    [TestMethod]
    public void Serialize_ShelbyToFile_ForVisualInspection()
    {
        string inputPath = Path.Combine(s_examplesFolder, "Shelby IEEE1344.xml");
        SkipIfMissing(inputPath);

        IConfigurationFrame frame = LoadConfigurationFrame(inputPath);

        string outputPath = Path.Combine(Path.GetTempPath(), "Shelby_RoundTripped.xml");
        using (FileStream fs = File.Create(outputPath))
            LegacySoapSerializer.Serialize(fs, frame);

        System.Console.WriteLine($"Round-tripped file written to: {outputPath}");
        System.Console.WriteLine($"Size: {new FileInfo(outputPath).Length} bytes (original: {new FileInfo(inputPath).Length} bytes)");
    }

    [TestMethod]
    public void Serialize_ProducesParseableSoapXml()
    {
        string path = Path.Combine(s_examplesFolder, "Shelby IEEE1344.xml");
        SkipIfMissing(path);

        IConfigurationFrame frame = LoadConfigurationFrame(path);

        using MemoryStream buffer = new();
        LegacySoapSerializer.Serialize(buffer, frame);

        string xml = System.Text.Encoding.UTF8.GetString(buffer.ToArray());

        StringAssert.StartsWith(xml, "<SOAP-ENV:Envelope", "Output should start with the SOAP envelope.");
        StringAssert.Contains(xml, "<SOAP-ENV:Body>", "Output should contain a SOAP body.");
        StringAssert.Contains(xml, "id=\"ref-1\"", "Root object should be ref-1.");
        StringAssert.Contains(xml, "href=\"#ref-", "At least one href reference should be emitted.");
        StringAssert.Contains(xml, "Shelby", "Station name should appear in output.");
    }

    [TestMethod]
    public void Serialize_AssignsUniqueGlobalPrefixPerNamespace_ForLegacySoapFormatterCompatibility()
    {
        // The .NET Framework SoapFormatter (PMU Connection Tester) binds xmlns:aN prefixes
        // document-wide rather than per-element. If the writer reuses the same prefix character for two
        // distinct namespace URIs, the legacy reader misbinds xsi:type and fails with
        // "no type associated with Xml key aN <ns> <asm>". This test guards against that regression.
        string path = Path.Combine(s_examplesFolder, "Shelby IEEE1344.xml");
        SkipIfMissing(path);

        IConfigurationFrame frame = LoadConfigurationFrame(path);

        using MemoryStream buffer = new();
        LegacySoapSerializer.Serialize(buffer, frame);
        string xml = System.Text.Encoding.UTF8.GetString(buffer.ToArray());

        // For every "xmlns:aN" declaration in the document, the bound URI must be the same.
        Dictionary<string, string> prefixBindings = new(StringComparer.Ordinal);
        System.Text.RegularExpressions.Regex pattern = new("xmlns:(a\\d+)=\"([^\"]+)\"");

        foreach (System.Text.RegularExpressions.Match match in pattern.Matches(xml))
        {
            string prefix = match.Groups[1].Value;
            string uri = match.Groups[2].Value;

            if (prefixBindings.TryGetValue(prefix, out string? existing))
            {
                Assert.AreEqual(existing, uri,
                    $"Prefix '{prefix}' is bound to two distinct URIs ('{existing}' and '{uri}') — legacy SoapFormatter would misbind xsi:type.");
            }
            else
            {
                prefixBindings[prefix] = uri;
            }
        }

        Assert.IsTrue(prefixBindings.Count >= 3, "Expected at least three distinct namespace URIs (IEEE1344, GSF.PhasorProtocols, GSF.Units.EE).");
    }

    [TestMethod]
    public void Serialize_EmitsGsfLegacyNamesByDefault_ForPmuConnectionTesterCompatibility()
    {
        string path = Path.Combine(s_examplesFolder, "RHODES_DFR1.configuration.xml");
        SkipIfMissing(path);

        IConfigurationFrame frame = LoadConfigurationFrame(path);

        using MemoryStream buffer = new();
        LegacySoapSerializer.Serialize(buffer, frame);
        string xml = System.Text.Encoding.UTF8.GetString(buffer.ToArray());

        // Generic Gemstone.* → GSF.* mapping
        StringAssert.Contains(xml, "GSF.PhasorProtocols.IEEEC37_118", "IEEE C37.118 namespace should be emitted with legacy GSF prefix.");
        StringAssert.Contains(xml, "GSF.PhasorProtocols/GSF.PhasorProtocols", "PhasorProtocols collection namespace+assembly should match GSF era.");
        Assert.IsFalse(xml.Contains("Gemstone.PhasorProtocols"), "Output should not contain current Gemstone.* namespace prefixes.");

        // Special-case: Gemstone.Numeric.EE.* (LineFrequency, PhasorType) → GSF.Units.EE.* in GSF.Core
        StringAssert.Contains(xml, "GSF.Units.EE/GSF.Core", "EE enum types must remap to GSF.Units.EE namespace inside the GSF.Core assembly.");
        Assert.IsFalse(xml.Contains("Gemstone.Numeric"), "Output should not contain the current Gemstone.Numeric assembly name.");

        // Verify the GSF-named output round-trips back through our Gemstone-aware reader
        buffer.Position = 0;
        IConfigurationFrame roundTripped = Common.DeserializeConfigurationFrame(buffer)
            ?? throw new InvalidOperationException("Round-trip deserialization returned null.");

        AssertFrameEquivalent(frame, roundTripped);
    }

    // -- helpers --------------------------------------------------------------

    private static IConfigurationFrame RoundTrip(IConfigurationFrame frame)
    {
        using MemoryStream buffer = new();
        LegacySoapSerializer.Serialize(buffer, frame);
        buffer.Position = 0;

        IConfigurationFrame deserialized = Common.DeserializeConfigurationFrame(buffer);
        Assert.IsNotNull(deserialized, "Round-trip deserialization returned null.");
        return deserialized;
    }

    private static IConfigurationFrame LoadConfigurationFrame(string path)
    {
        using FileStream stream = File.OpenRead(path);
        IConfigurationFrame frame = Common.DeserializeConfigurationFrame(stream);
        Assert.IsNotNull(frame, $"Deserialization returned null for {Path.GetFileName(path)}");
        return frame;
    }

    private static void AssertFrameEquivalent(IConfigurationFrame expected, IConfigurationFrame actual)
    {
        Assert.AreEqual(expected.GetType(), actual.GetType(), "Frame type mismatch.");
        Assert.AreEqual(expected.IDCode, actual.IDCode, "IDCode mismatch.");
        Assert.AreEqual(expected.FrameRate, actual.FrameRate, "FrameRate mismatch.");
        Assert.AreEqual(expected.Cells.Count, actual.Cells.Count, "Cell count mismatch.");

        for (int i = 0; i < expected.Cells.Count; i++)
            AssertCellEquivalent(expected.Cells[i], actual.Cells[i], i);
    }

    private static void AssertCellEquivalent(IConfigurationCell expected, IConfigurationCell actual, int index)
    {
        Assert.AreEqual(expected.GetType(), actual.GetType(), $"Cell[{index}] type mismatch.");
        Assert.AreEqual(expected.IDCode, actual.IDCode, $"Cell[{index}] IDCode mismatch.");
        Assert.AreEqual(expected.StationName, actual.StationName, $"Cell[{index}] StationName mismatch.");
        Assert.AreEqual(expected.NominalFrequency, actual.NominalFrequency, $"Cell[{index}] NominalFrequency mismatch.");
        Assert.AreEqual(expected.PhasorDefinitions.Count, actual.PhasorDefinitions.Count, $"Cell[{index}] PhasorDefinitions count mismatch.");
        Assert.AreEqual(expected.AnalogDefinitions.Count, actual.AnalogDefinitions.Count, $"Cell[{index}] AnalogDefinitions count mismatch.");
        Assert.AreEqual(expected.DigitalDefinitions.Count, actual.DigitalDefinitions.Count, $"Cell[{index}] DigitalDefinitions count mismatch.");

        for (int i = 0; i < expected.PhasorDefinitions.Count; i++)
        {
            Assert.AreEqual(expected.PhasorDefinitions[i].Label, actual.PhasorDefinitions[i].Label,
                $"Cell[{index}].PhasorDefinitions[{i}] Label mismatch.");
            Assert.AreEqual(expected.PhasorDefinitions[i].ScalingValue, actual.PhasorDefinitions[i].ScalingValue,
                $"Cell[{index}].PhasorDefinitions[{i}] ScalingValue mismatch.");
            Assert.AreEqual(expected.PhasorDefinitions[i].PhasorType, actual.PhasorDefinitions[i].PhasorType,
                $"Cell[{index}].PhasorDefinitions[{i}] PhasorType mismatch.");
        }

        for (int i = 0; i < expected.AnalogDefinitions.Count; i++)
        {
            Assert.AreEqual(expected.AnalogDefinitions[i].Label, actual.AnalogDefinitions[i].Label,
                $"Cell[{index}].AnalogDefinitions[{i}] Label mismatch.");
            Assert.AreEqual(expected.AnalogDefinitions[i].AnalogType, actual.AnalogDefinitions[i].AnalogType,
                $"Cell[{index}].AnalogDefinitions[{i}] AnalogType mismatch.");
        }
    }

    private static void SkipIfMissing(string path)
    {
        if (!File.Exists(path))
            Assert.Inconclusive($"Sample file not present: {path}");
    }
}
