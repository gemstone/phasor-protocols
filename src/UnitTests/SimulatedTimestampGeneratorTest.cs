//******************************************************************************************************
//  SimulatedTimestampGeneratorTest.cs - Gbtc
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
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.PhasorProtocols.UnitTests;

/// <summary>
/// Unit tests for <see cref="SimulatedTimestampGenerator"/> and the high frame rate
/// catch-up option of <see cref="ReplayTimer"/>.
/// </summary>
[TestClass]
public class SimulatedTimestampGeneratorTest
{
    // Arbitrary start time that is not on a second or frame boundary
    private static readonly long StartTicks = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc).Ticks + 1_234_567L;

    // Ideal wall-clock time of the i-th frame after start
    private static long IdealTime(long index, int frameRate) => StartTicks + index * Ticks.PerSecond / frameRate;

    // Asserts that timestamps hold every slot of every whole second exactly once, evenly spaced
    private static void AssertEvenlyDistributed(IReadOnlyList<long> timestamps, int frameRate)
    {
        long minSpacing = Ticks.PerSecond / frameRate;
        long maxSpacing = (Ticks.PerSecond + frameRate - 1) / frameRate;

        Assert.AreEqual(timestamps.Count, timestamps.Distinct().Count(), "Timestamps are not unique");

        for (int i = 0; i < timestamps.Count; i++)
        {
            long timestamp = timestamps[i];

            Assert.AreEqual(timestamp, Ticks.RoundToSubsecondDistribution(timestamp, frameRate).Value, $"Timestamp {i} is not on the {frameRate} fps grid");

            if (i == 0)
                continue;

            long spacing = timestamp - timestamps[i - 1];
            Assert.IsTrue(spacing >= minSpacing && spacing <= maxSpacing, $"Spacing {spacing} at frame {i} is outside [{minSpacing}, {maxSpacing}]");
        }

        // Every whole second fully covered by the sequence holds exactly frameRate timestamps, matching the grid
        HashSet<long> grid = [.. Enumerable.Range(0, frameRate).Select(slot => slot * Ticks.PerSecond / frameRate)];

        IEnumerable<IGrouping<long, long>> seconds = timestamps
            .GroupBy(timestamp => timestamp - timestamp % Ticks.PerSecond)
            .Skip(1)                                // First second is partial
            .SkipLast(1);                           // Last second is partial

        int checkedSeconds = 0;

        foreach (IGrouping<long, long> second in seconds)
        {
            HashSet<long> slots = [.. second.Select(timestamp => timestamp - second.Key)];
            Assert.AreEqual(frameRate, slots.Count, $"Second {second.Key} holds {slots.Count} distinct slots");
            Assert.IsTrue(slots.SetEquals(grid), $"Second {second.Key} slots do not match the {frameRate} fps grid");
            checkedSeconds++;
        }

        Assert.IsTrue(checkedSeconds > 0, "No whole seconds were checked");
    }

    [TestMethod]
    public void MillisecondAlignment_At3000Fps_LosesSlots()
    {
        // Documents the original defect: with perfect pacing, millisecond alignment cannot represent 3,000 slots per second
        const int FrameRate = 3000;

        HashSet<long> timestamps = [];

        for (int i = 0; i < FrameRate; i++)
            timestamps.Add(Ticks.AlignToMillisecondDistribution(IdealTime(i, FrameRate), FrameRate).Value);

        Assert.IsTrue(timestamps.Count < FrameRate * 3 / 4, $"Expected millisecond alignment to collapse slots, got {timestamps.Count:N0} distinct");
    }

    [TestMethod]
    public void Next_At3000FpsPerfectPacing_Produces3000DistinctEvenlySpacedSlotsPerSecond()
    {
        const int FrameRate = 3000;
        const int Seconds = 5;

        SimulatedTimestampGenerator generator = new(FrameRate);
        List<long> timestamps = [];

        for (int i = 0; i < FrameRate * Seconds; i++)
            timestamps.Add(generator.Next(IdealTime(i, FrameRate)));

        AssertEvenlyDistributed(timestamps, FrameRate);
        Assert.AreEqual(0L, generator.Resynchronizations);
    }

    [TestMethod]
    public void Next_At3000FpsWithJitterStallsAndBursts_StaysEvenlySpaced()
    {
        const int FrameRate = 3000;
        const int Seconds = 5;

        SimulatedTimestampGenerator generator = new(FrameRate);
        Random random = new(12345);
        List<long> timestamps = [];
        long stallTicks = Ticks.FromMilliseconds(50.0D).Value;
        long stallEndTicks = 0L;

        for (int i = 0; i < FrameRate * Seconds; i++)
        {
            // Mid-way through every second, the replay thread stalls for 50 ms, then delivers the late frames in a burst
            if (i % FrameRate == FrameRate / 2)
                stallEndTicks = IdealTime(i, FrameRate) + stallTicks;

            // Up to 1 ms of scheduling jitter on every frame
            long wallTicks = Math.Max(IdealTime(i, FrameRate), stallEndTicks) + random.Next(0, (int)Ticks.PerMillisecond);

            timestamps.Add(generator.Next(wallTicks));
        }

        AssertEvenlyDistributed(timestamps, FrameRate);
        Assert.AreEqual(0L, generator.Resynchronizations);
    }

    [DataTestMethod]
    [DataRow(30)]
    [DataRow(60)]
    [DataRow(1000)]
    [DataRow(1001)]
    [DataRow(2880)]
    [DataRow(3000)]
    [DataRow(3840)]
    [DataRow(7000)]
    public void Next_AtVariousFrameRates_ProducesEveryGridSlotOnce(int frameRate)
    {
        SimulatedTimestampGenerator generator = new(frameRate);
        List<long> timestamps = [];

        for (int i = 0; i < frameRate * 3; i++)
            timestamps.Add(generator.Next(IdealTime(i, frameRate)));

        AssertEvenlyDistributed(timestamps, frameRate);
    }

    [TestMethod]
    public void Next_FirstTimestamp_IsFloorOfWallClockSlot()
    {
        const int FrameRate = 3000;

        for (long offset = 0L; offset < Ticks.PerMillisecond * 2L; offset += 7L)
        {
            long wallTicks = StartTicks + offset;
            long timestamp = new SimulatedTimestampGenerator(FrameRate).Next(wallTicks);

            Assert.AreEqual(Ticks.AlignToSubsecondDistribution(wallTicks, FrameRate, 0L).Value, timestamp, $"Offset {offset}");
            Assert.IsTrue(timestamp <= wallTicks && wallTicks - timestamp < Ticks.PerSecond / FrameRate + 1L, $"Offset {offset}");
        }
    }

    [TestMethod]
    public void Next_DriftBeyondMaximum_ReanchorsToWallClock()
    {
        const int FrameRate = 3000;

        SimulatedTimestampGenerator generator = new(FrameRate, Ticks.FromMilliseconds(100.0D));
        List<long> timestamps = [];

        for (int i = 0; i < FrameRate; i++)
            timestamps.Add(generator.Next(IdealTime(i, FrameRate)));

        // Replay stalls for two seconds; the counter would lag the wall clock by about two seconds
        long resumeTicks = IdealTime(FrameRate, FrameRate) + 2L * Ticks.PerSecond;
        long resumed = generator.Next(resumeTicks);

        Assert.AreEqual(1L, generator.Resynchronizations);
        Assert.AreEqual(Ticks.AlignToSubsecondDistribution(resumeTicks, FrameRate, 0L).Value, resumed);
        Assert.IsTrue(resumed > timestamps[^1]);

        // Pacing continues from the new anchor
        timestamps.Clear();
        timestamps.Add(resumed);

        for (int i = 1; i < FrameRate * 3; i++)
            timestamps.Add(generator.Next(resumeTicks + i * Ticks.PerSecond / FrameRate));

        AssertEvenlyDistributed(timestamps, FrameRate);
        Assert.AreEqual(1L, generator.Resynchronizations);
    }

    [TestMethod]
    public void Next_DriftWithinMaximum_DoesNotReanchor()
    {
        const int FrameRate = 3000;

        SimulatedTimestampGenerator generator = new(FrameRate, Ticks.FromMilliseconds(100.0D));
        long first = generator.Next(StartTicks);

        // A 90 ms stall stays within tolerance, so the next frame takes the next slot
        long second = generator.Next(StartTicks + Ticks.FromMilliseconds(90.0D).Value);

        Assert.AreEqual(0L, generator.Resynchronizations);
        Assert.AreEqual(Ticks.RoundToSubsecondDistribution(first + Ticks.PerSecond / FrameRate + 1L, FrameRate).Value, second);
    }

    [TestMethod]
    public void Reset_ReanchorsOnNextFrame()
    {
        const int FrameRate = 3000;

        SimulatedTimestampGenerator generator = new(FrameRate);
        generator.Next(StartTicks);
        generator.Next(StartTicks + 3333L);

        generator.Reset();

        long wallTicks = StartTicks + Ticks.FromMilliseconds(10.0D).Value;
        Assert.AreEqual(Ticks.AlignToSubsecondDistribution(wallTicks, FrameRate, 0L).Value, generator.Next(wallTicks));
        Assert.AreEqual(0L, generator.Resynchronizations);
    }

    [TestMethod]
    public void Constructor_InvalidArguments_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SimulatedTimestampGenerator(0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SimulatedTimestampGenerator(-1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SimulatedTimestampGenerator(30, new Ticks(-1L)));
    }

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(30)]
    [DataRow(60)]
    [DataRow(240)]
    [DataRow(1000)]
    public void AlignToFrameRate_AtOrBelow1000Fps_MatchesMillisecondAlignment(int frameRate)
    {
        Random random = new(frameRate);

        for (int i = 0; i < 10_000; i++)
        {
            long ticks = StartTicks + random.NextInt64(0L, Ticks.PerSecond * 2L);
            Assert.AreEqual(Ticks.AlignToMillisecondDistribution(ticks, frameRate), SimulatedTimestampGenerator.AlignToFrameRate(ticks, frameRate));
        }
    }

    [TestMethod]
    public void AlignToFrameRate_At3000Fps_KeepsEverySlot()
    {
        const int FrameRate = 3000;

        List<long> timestamps = [];

        for (int i = 0; i < FrameRate * 3; i++)
            timestamps.Add(SimulatedTimestampGenerator.AlignToFrameRate(IdealTime(i, FrameRate), FrameRate).Value);

        AssertEvenlyDistributed(timestamps, FrameRate);
    }

    [TestMethod]
    public void ReplayTimer_WithCatchUpWindow_ReleasesLateFramesWithoutWaiting()
    {
        const int FrameRate = 1000;
        const int Frames = 20;

        // Owe about 50 frames, then time how long the next 20 take to release
        ReplayTimer catchUpTimer = new(FrameRate, TimeSpan.FromMilliseconds(100.0D));
        Thread.Sleep(50);

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < Frames; i++)
            catchUpTimer.WaitNext();

        double catchUpMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

        // Default timer resets cadence when late, so it paces all 20 frames
        ReplayTimer defaultTimer = new(FrameRate);
        Thread.Sleep(50);

        stopwatch.Restart();

        for (int i = 0; i < Frames; i++)
            defaultTimer.WaitNext();

        double defaultMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

        Assert.AreEqual(TimeSpan.Zero, defaultTimer.CatchUpWindow);
        Assert.IsTrue(catchUpMilliseconds < 10.0D, $"Catch-up timer took {catchUpMilliseconds:N2} ms for {Frames} owed frames");
        Assert.IsTrue(defaultMilliseconds >= Frames - 2, $"Default timer took {defaultMilliseconds:N2} ms for {Frames} frames");
    }
}
