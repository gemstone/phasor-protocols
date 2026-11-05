//******************************************************************************************************
//  SimulatedTimestampGenerator.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/05/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace Gemstone.PhasorProtocols;

/// <summary>
/// Generates simulated frame timestamps for paced (e.g., file replay) input by advancing a frame
/// counter across the sub-second distribution of a defined frame rate, instead of reading the
/// wall clock for every frame.
/// </summary>
/// <remarks>
/// <para>
/// Millisecond aligned wall-clock timestamps, i.e., <see cref="Ticks.AlignToMillisecondDistribution"/>,
/// cannot represent every frame slot when the frame rate exceeds 1,000 frames per second. For example, at
/// 3,000 frames per second only about 2,000 distinct timestamps per second are produced, some slots twice,
/// so consumers aligning to a 3,000-slot grid lose about a third of the frames. Reading the wall clock
/// per frame also misplaces frames when replay pacing stalls and then bursts.
/// </para>
/// <para>
/// This generator anchors to the wall clock on the first frame, then issues each following frame the
/// next slot of the <see cref="FrameRate"/> distribution at full tick resolution. Every timestamp lands
/// exactly on the grid used by <see cref="Ticks.RoundToSubsecondDistribution"/> and
/// <see cref="Ticks.AlignToSubsecondDistribution"/>, each slot is used once, and consecutive frames are
/// exactly one frame period apart. If the counter drifts from the wall clock by more than
/// <see cref="MaximumDrift"/>, for example after a long stall in replay, the generator re-anchors to the
/// wall clock so timestamps stay near real time.
/// </para>
/// </remarks>
public sealed class SimulatedTimestampGenerator
{
    #region [ Members ]

    // Constants

    /// <summary>
    /// Defines the highest frame rate for which millisecond aligned timestamps can represent every frame slot.
    /// </summary>
    public const int MaximumMillisecondFrameRate = 1000;

    /// <summary>
    /// Defines the default maximum drift between the frame counter and the wall clock before re-anchoring.
    /// </summary>
    public static readonly Ticks DefaultMaximumDrift = Ticks.FromMilliseconds(500.0D);

    // Fields
    private long m_secondTicks;  // Top of the second, in ticks, for the current slot
    private int m_slot;          // Current slot index within the second, 0 to FrameRate - 1
    private bool m_anchored;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="SimulatedTimestampGenerator"/>.
    /// </summary>
    /// <param name="frameRate">Defined frame rate, in frames per second.</param>
    /// <param name="maximumDrift">
    /// Maximum allowed difference between generated timestamps and the wall clock before the generator
    /// re-anchors to the wall clock; defaults to <see cref="DefaultMaximumDrift"/>.
    /// </param>
    public SimulatedTimestampGenerator(int frameRate, Ticks? maximumDrift = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameRate);

        Ticks drift = maximumDrift ?? DefaultMaximumDrift;
        ArgumentOutOfRangeException.ThrowIfNegative(drift.Value, nameof(maximumDrift));

        FrameRate = frameRate;
        MaximumDrift = drift;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the defined frame rate, in frames per second.
    /// </summary>
    public int FrameRate { get; }

    /// <summary>
    /// Gets the maximum allowed difference between generated timestamps and the wall clock before re-anchoring.
    /// </summary>
    public Ticks MaximumDrift { get; }

    /// <summary>
    /// Gets the number of times the generator re-anchored to the wall clock because drift exceeded <see cref="MaximumDrift"/>.
    /// </summary>
    public long Resynchronizations { get; private set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Gets the timestamp for the next frame, using the current UTC time as the wall-clock reference.
    /// </summary>
    /// <returns>Timestamp, in ticks, for the next frame.</returns>
    public long Next()
    {
        return Next(DateTime.UtcNow.Ticks);
    }

    /// <summary>
    /// Gets the timestamp for the next frame, using <paramref name="currentTicks"/> as the wall-clock reference.
    /// </summary>
    /// <param name="currentTicks">Current wall-clock time, in ticks.</param>
    /// <returns>Timestamp, in ticks, for the next frame.</returns>
    public long Next(long currentTicks)
    {
        if (!m_anchored)
        {
            Anchor(currentTicks);
            m_anchored = true;
            return CurrentTimestamp;
        }

        // Advance one frame slot, rolling over to the next second as needed
        if (++m_slot >= FrameRate)
        {
            m_slot = 0;
            m_secondTicks += Ticks.PerSecond;
        }

        long timestamp = CurrentTimestamp;

        if (Math.Abs(timestamp - currentTicks) <= MaximumDrift.Value)
            return timestamp;

        // Counter has drifted too far from the wall clock, e.g., after a long replay stall or a clock change
        Anchor(currentTicks);
        Resynchronizations++;

        return CurrentTimestamp;
    }

    /// <summary>
    /// Resets the generator so the next frame re-anchors to the wall clock.
    /// </summary>
    public void Reset()
    {
        m_anchored = false;
    }

    // Timestamp of the current slot, on the same grid as Ticks.RoundToSubsecondDistribution
    private long CurrentTimestamp => m_secondTicks + m_slot * Ticks.PerSecond / FrameRate;

    // Anchors to the slot at or before the specified time, i.e., floor alignment at tick resolution
    private void Anchor(long currentTicks)
    {
        long ticksBeyondSecond = currentTicks % Ticks.PerSecond;

        if (ticksBeyondSecond < 0L)
            ticksBeyondSecond += Ticks.PerSecond;

        m_secondTicks = currentTicks - ticksBeyondSecond;

        // Largest slot whose start, slot * PerSecond / FrameRate (floored), is at or before ticksBeyondSecond
        m_slot = (int)(((ticksBeyondSecond + 1L) * FrameRate - 1L) / Ticks.PerSecond);
    }

    #endregion

    #region [ Static ]

    // Static Methods

    /// <summary>
    /// Floor-aligns <paramref name="timestamp"/> to the sub-second distribution of <paramref name="frameRate"/>.
    /// </summary>
    /// <param name="timestamp">Timestamp to align.</param>
    /// <param name="frameRate">Frame rate, in frames per second.</param>
    /// <returns>Aligned timestamp.</returns>
    /// <remarks>
    /// Frame rates up to <see cref="MaximumMillisecondFrameRate"/> use millisecond alignment, unchanged from
    /// prior behavior, see <see cref="Ticks.AlignToMillisecondDistribution"/>. Higher frame rates align at
    /// full tick resolution so that every frame slot remains distinct.
    /// </remarks>
    public static Ticks AlignToFrameRate(Ticks timestamp, int frameRate)
    {
        return frameRate > MaximumMillisecondFrameRate ?
            Ticks.AlignToSubsecondDistribution(timestamp, frameRate, 0L) :
            Ticks.AlignToMillisecondDistribution(timestamp, frameRate);
    }

    #endregion
}
