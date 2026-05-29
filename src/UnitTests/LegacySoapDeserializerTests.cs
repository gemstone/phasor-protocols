//******************************************************************************************************
//  LegacySoapDeserializerTests.cs - Gbtc
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
using System.IO;
using System.Linq;
using Gemstone.PhasorProtocols.IEEE1344;
using Gemstone.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.PhasorProtocols.UnitTests;

[TestClass]
public class LegacySoapDeserializerTests
{
    /// <summary>
    /// Folder of representative legacy SOAP-formatted configuration XML files captured from
    /// production openPDC / openHistorian / PMU Connection Tester deployments. Tests skip when
    /// the folder is not present (e.g., on CI machines without the captures).
    /// </summary>
    private static readonly string s_examplesFolder = @"C:\Users\rcarroll\Desktop\Captures\Examples";

    [TestMethod]
    public void Deserialize_ShelbyIeee1344_ProducesExpectedFrame()
    {
        string path = Path.Combine(s_examplesFolder, "Shelby IEEE1344.xml");
        SkipIfMissing(path);

        IConfigurationFrame frame = LoadConfigurationFrame(path);

        Assert.IsInstanceOfType(frame, typeof(ConfigurationFrame));
        Assert.AreEqual((ushort)2, frame.IDCode);
        Assert.AreEqual((ushort)30, frame.FrameRate);
        Assert.AreEqual(1, frame.Cells.Count);

        IConfigurationCell cell = frame.Cells[0];
        Assert.AreEqual("Shelby", cell.StationName.Trim());
        Assert.AreEqual(5, cell.PhasorDefinitions.Count);
        Assert.AreEqual(0, cell.AnalogDefinitions.Count);
        Assert.AreEqual(1, cell.DigitalDefinitions.Count);

        Assert.AreEqual("Bus 1", cell.PhasorDefinitions[0].Label.Trim());
        Assert.AreSame(cell, cell.PhasorDefinitions[0].Parent, "Phasor's parent must be the same cell instance — back-reference round-tripped.");
        Assert.AreSame(frame, cell.Parent, "Cell's parent must be the configuration frame instance — circular reference round-tripped.");
    }

    [TestMethod]
    public void Deserialize_RhodesC37118_ProducesExpectedFrame()
    {
        string path = Path.Combine(s_examplesFolder, "RHODES_DFR1.configuration.xml");
        SkipIfMissing(path);

        IConfigurationFrame frame = LoadConfigurationFrame(path);

        Assert.IsInstanceOfType(frame, typeof(ConfigurationFrame2));
        Assert.AreEqual((ushort)1, frame.IDCode);
        Assert.AreEqual((ushort)30, frame.FrameRate);
        Assert.AreEqual(1, frame.Cells.Count);

        IConfigurationCell cell = frame.Cells[0];
        Assert.AreEqual("RHODES SUBSTATIO", cell.StationName);
        Assert.AreEqual(24, cell.PhasorDefinitions.Count);
        Assert.AreEqual(4, cell.AnalogDefinitions.Count);
        Assert.AreEqual(2, cell.DigitalDefinitions.Count);

        Assert.AreEqual("NELSON_L850___V1", cell.PhasorDefinitions[0].Label.Trim());
    }

    [TestMethod]
    public void Deserialize_AllExampleFiles_DoesNotThrow()
    {
        if (!Directory.Exists(s_examplesFolder))
        {
            Assert.Inconclusive($"Examples folder not present: {s_examplesFolder}");
            return;
        }

        string[] files = Directory.GetFiles(s_examplesFolder, "*.xml");
        Assert.IsTrue(files.Length > 0, "Examples folder is empty.");

        int succeeded = 0;
        var failures = new System.Collections.Generic.List<string>();

        foreach (string file in files)
        {
            try
            {
                IConfigurationFrame frame = LoadConfigurationFrame(file);
                Assert.IsNotNull(frame, $"{Path.GetFileName(file)} deserialized to null.");
                Assert.IsTrue(frame.Cells.Count >= 1, $"{Path.GetFileName(file)} produced no cells.");
                succeeded++;
            }
            catch (Exception ex)
            {
                failures.Add($"{Path.GetFileName(file)}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        System.Console.WriteLine($"Deserialized {succeeded}/{files.Length} example files.");

        if (failures.Count > 0)
            Assert.Fail($"{failures.Count} file(s) failed:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    [TestMethod]
    public void Deserialize_LargeMultiCellFile_PreservesAllCells()
    {
        string path = Path.Combine(s_examplesFolder, "ISO_NE_Troubleshooting_Config.xml");
        SkipIfMissing(path);

        IConfigurationFrame frame = LoadConfigurationFrame(path);
        Assert.AreEqual(47, frame.Cells.Count, "Expected 47 cells in ISO-NE troubleshooting config.");

        // Every cell's Parent should resolve back to the same root frame instance
        foreach (IConfigurationCell cell in frame.Cells)
            Assert.AreSame(frame, cell.Parent, $"Cell '{cell.StationName}' parent ref did not resolve to root frame.");
    }

    private static IConfigurationFrame LoadConfigurationFrame(string path)
    {
        using FileStream stream = File.OpenRead(path);
        IConfigurationFrame frame = Common.DeserializeConfigurationFrame(stream);
        Assert.IsNotNull(frame, $"Deserialization returned null for {Path.GetFileName(path)}");
        return frame;
    }

    private static void SkipIfMissing(string path)
    {
        if (!File.Exists(path))
            Assert.Inconclusive($"Sample file not present: {path}");
    }
}
