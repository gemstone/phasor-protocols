﻿//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace Gemstone.PhasorProtocols.IEEEC37_118
{
    #region [ Enumerations ]

    /// <summary>
    /// IEEE C37.118 frame types enumeration.
    /// </summary>
    [Serializable]
    public enum FrameType : ushort
    {
        /// <summary>
        /// 000 Data frame.
        /// </summary>
        DataFrame = (ushort)Bits.Nil,
        /// <summary>
        /// 001 Header frame.
        /// </summary>
        HeaderFrame = (ushort)Bits.Bit04,
        /// <summary>
        /// 010 Configuration frame 1.
        /// </summary>
        ConfigurationFrame1 = (ushort)Bits.Bit05,
        /// <summary>
        /// 011 Configuration frame 2.
        /// </summary>
        ConfigurationFrame2 = (ushort)(Bits.Bit04 | Bits.Bit05),
        /// <summary>
        /// 101 Configuration frame 3.
        /// </summary>
        ConfigurationFrame3 = (ushort)(Bits.Bit06 | Bits.Bit04),
        /// <summary>
        /// 100 Command frame.
        /// </summary>
        CommandFrame = (ushort)Bits.Bit06,
        /// <summary>.
        /// Reserved bit.
        /// </summary>
        Reserved = (ushort)Bits.Bit07,
        /// <summary>
        /// Version number mask.
        /// </summary>
        VersionNumberMask = (ushort)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03)
    }

    /// <summary>
    /// Protocol draft revision numbers enumeration.
    /// </summary>
    [Serializable]
    public enum DraftRevision : byte
    {
        /// <summary>
        /// Draft 6.0.
        /// </summary>
        Draft6 = 0,
        /// <summary>
        /// Draft 7.0 (i.e., IEEE Std C37.118-2005).
        /// </summary>
        Draft7 = 1,
        /// <summary>
        /// IEEE Std C37.118-2005.
        /// </summary>
        Std2005 = 1,
        /// <summary>
        /// IEEE Std C37.118-2011.
        /// </summary>
        Std2011 = 2,
        /// <summary>
        /// Defines the latest known IEEE C37.118 version number.
        /// </summary>
        LatestVersion = Std2011
    }

    /// <summary>
    /// Data format flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum FormatFlags : ushort
    {
        /// <summary>
        /// Frequency value format (Set = float, Clear = integer).
        /// </summary>
        Frequency = (ushort)Bits.Bit03,
        /// <summary>
        /// Analog value format (Set = float, Clear = integer).
        /// </summary>
        Analog = (ushort)Bits.Bit02,
        /// <summary>
        /// Phasor value format (Set = float, Clear = integer).
        /// </summary>
        Phasors = (ushort)Bits.Bit01,
        /// <summary>
        /// Phasor coordinate format (Set = polar, Clear = rectangular).
        /// </summary>
        Coordinates = (ushort)Bits.Bit00,
        /// <summary>
        /// Unused format bits mask.
        /// </summary>
        UnusedMask = unchecked(ushort.MaxValue & (ushort)~(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03)),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (ushort)Bits.Nil
    }

    /// <summary>
    /// Time quality flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum TimeQualityFlags : uint
    {
        /// <summary>
        /// Reserved bit.
        /// </summary>
        Reserved = (uint)Bits.Bit31,
        /// <summary>
        /// Leap second direction – 0 for add, 1 for delete.
        /// </summary>
        LeapSecondDirection = (uint)Bits.Bit30,
        /// <summary>
        /// Leap second occurred – set in the first second after the leap second occurs and remains set for 24 hours.
        /// </summary>
        LeapSecondOccurred = (uint)Bits.Bit29,
        /// <summary>
        /// Leap second pending – set before a leap second occurs and cleared in the second after the leap second occurs.
        /// </summary>
        LeapSecondPending = (uint)Bits.Bit28,
        /// <summary>
        /// Time quality indicator code mask.
        /// </summary>
        TimeQualityIndicatorCodeMask = (uint)(Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (uint)Bits.Nil
    }

    /// <summary>
    /// Time quality indicator code enumeration.
    /// </summary>
    [Serializable]
    public enum TimeQualityIndicatorCode : uint
    {
        /// <summary>
        /// 1111 - 0xF:	Fault--clock failure, time not reliable.
        /// </summary>
        Failure = (uint)(Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 1011 - 0xB:	Clock unlocked, time within 10^1 s.
        /// </summary>
        UnlockedWithin10Seconds = (uint)(Bits.Bit27 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 1010 - 0xA:	Clock unlocked, time within 10^0 s.
        /// </summary>
        UnlockedWithin1Second = (uint)(Bits.Bit27 | Bits.Bit25),
        /// <summary>
        /// 1001 - 0x9: Clock unlocked, time within 10^-1 s.
        /// </summary>
        UnlockedWithinPoint1Seconds = (uint)(Bits.Bit27 | Bits.Bit24),
        /// <summary>
        /// 1000 - 0x8: Clock unlocked, time within 10^-2 s.
        /// </summary>
        UnlockedWithinPoint01Seconds = (uint)Bits.Bit27,
        /// <summary>
        /// 0111 - 0x7: Clock unlocked, time within 10^-3 s.
        /// </summary>
        UnlockedWithinPoint001Seconds = (uint)(Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 0110 - 0x6: Clock unlocked, time within 10^-4 s.
        /// </summary>
        UnlockedWithinPoint0001Seconds = (uint)(Bits.Bit26 | Bits.Bit25),
        /// <summary>
        /// 0101 - 0x5: Clock unlocked, time within 10^-5 s.
        /// </summary>
        UnlockedWithinPoint00001Seconds = (uint)(Bits.Bit26 | Bits.Bit24),
        /// <summary>
        /// 0100 - 0x4: Clock unlocked, time within 10^-6 s.
        /// </summary>
        UnlockedWithinPoint000001Seconds = (uint)Bits.Bit26,
        /// <summary>
        /// 0011 - 0x3: Clock unlocked, time within 10^-7 s.
        /// </summary>
        UnlockedWithinPoint0000001Seconds = (uint)(Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 0010 - 0x2: Clock unlocked, time within 10^-8 s.
        /// </summary>
        UnlockedWithinPoint00000001Seconds = (uint)Bits.Bit25,
        /// <summary>
        /// 0001 - 0x1: Clock unlocked, time within 10^-9 s.
        /// </summary>
        UnlockedWithinPoint000000001Seconds = (uint)Bits.Bit24,
        /// <summary>
        /// 0000 - 0x0: Normal operation, clock locked.
        /// </summary>
        Locked = 0
    }

    /// <summary>
    /// Status flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum StatusFlags : ushort
    {
        /// <summary>
        /// Data is valid (0 when device data is valid, 1 when invalid or device is in test mode).
        /// </summary>
        DataIsValid = (ushort)Bits.Bit15,
        /// <summary>
        /// Device error including configuration error, 0 when no error.
        /// </summary>
        DeviceError = (ushort)Bits.Bit14,
        /// <summary>
        /// Device synchronization error, 0 when in sync.
        /// </summary>
        DeviceSynchronizationError = (ushort)Bits.Bit13,
        /// <summary>
        /// Data sorting type, 0 by timestamp, 1 by arrival.
        /// </summary>
        DataSortingType = (ushort)Bits.Bit12,
        /// <summary>
        /// Device trigger detected, 0 when no trigger.
        /// </summary>
        DeviceTriggerDetected = (ushort)Bits.Bit11,
        /// <summary>
        /// Configuration changed, set to 1 for one minute when configuration changed.
        /// </summary>
        ConfigurationChanged = (ushort)Bits.Bit10,
        /// <summary>
        /// Data modified indicator, set to 1 when data is modified by a post-processing
        /// device such as a PDC, else 0 to indicate no modifications.
        /// </summary>
        DataModified = (ushort)Bits.Bit09,
        /// <summary>
        /// Time quality mask.
        /// </summary>
        TimeQualityMask = (ushort)(Bits.Bit08 | Bits.Bit07 | Bits.Bit06),
        /// <summary>
        /// Unlocked time mask.
        /// </summary>
        UnlockedTimeMask = (ushort)(Bits.Bit05 | Bits.Bit04),
        /// <summary>
        /// Trigger reason mask.
        /// </summary>
        TriggerReasonMask = (ushort)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (ushort)Bits.Nil
    }

    /// <summary>
    /// Unlocked time enumeration.
    /// </summary>
    [Serializable]
    public enum TimeQuality : ushort
    {
        /// <summary>
        /// Estimated maximum time error &gt; 10ms or time error unknown.
        /// </summary>
        ErrorGreaterThan10ms = (ushort)(Bits.Bit08 | Bits.Bit07 | Bits.Bit06),
        /// <summary>
        /// Estimated maximum time error &lt; 10ms.
        /// </summary>
        ErrorLessThan10ms = (ushort)(Bits.Bit08 | Bits.Bit07),
        /// <summary>
        /// Estimated maximum time error &lt; 1ms.
        /// </summary>
        ErrorLessThan1ms = (ushort)(Bits.Bit08 | Bits.Bit06),
        /// <summary>
        /// Estimated maximum time error &lt; 100μs.
        /// </summary>
        ErrorLessThan100μs = (ushort)(Bits.Bit08),
        /// <summary>
        /// Estimated maximum time error &lt; 10μs.
        /// </summary>
        ErrorLessThan10μs = (ushort)(Bits.Bit07 | Bits.Bit06),
        /// <summary>
        /// Estimated maximum time error &lt; 1μs.
        /// </summary>
        ErrorLessThan1μs = (ushort)(Bits.Bit07),
        /// <summary>
        /// Estimated maximum time error &lt; 100ns.
        /// </summary>
        ErrorLessThan100ns = (ushort)(Bits.Bit06),
        /// <summary>
        /// Not used (indicates code from previous version of profile)
        /// </summary>
        Unavailable = (ushort)Bits.Nil
    }

    /// <summary>
    /// Unlocked time enumeration.
    /// </summary>
    [Serializable]
    public enum UnlockedTime : byte
    {
        /// <summary>
        /// Sync locked, best quality.
        /// </summary>
        SyncLocked = (byte)Bits.Nil,
        /// <summary>
        /// Unlocked for 10 seconds.
        /// </summary>
        UnlockedFor10Seconds = (byte)Bits.Bit04,
        /// <summary>
        /// Unlocked for 100 seconds.
        /// </summary>
        UnlockedFor100Seconds = (byte)Bits.Bit05,
        /// <summary>
        /// Unlocked for over 1000 seconds.
        /// </summary>
        UnlockedForOver1000Seconds = (byte)(Bits.Bit05 | Bits.Bit04)
    }

    /// <summary>
    /// Trigger reason enumeration.
    /// </summary>
    [Serializable]
    public enum TriggerReason : byte
    {
        /// <summary>
        /// 1111 Vendor defined trigger 8.
        /// </summary>
        VendorDefinedTrigger8 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 1110 Vendor defined trigger 7.
        /// </summary>
        VendorDefinedTrigger7 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01),
        /// <summary>
        /// 1101 Vendor defined trigger 6.
        /// </summary>
        VendorDefinedTrigger6 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit00),
        /// <summary>
        /// 1100 Vendor defined trigger 5.
        /// </summary>
        VendorDefinedTrigger5 = (byte)(Bits.Bit03 | Bits.Bit02),
        /// <summary>
        /// 1011 Vendor defined trigger 4.
        /// </summary>
        VendorDefinedTrigger4 = (byte)(Bits.Bit03 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 1010 Vendor defined trigger 3.
        /// </summary>
        VendorDefinedTrigger3 = (byte)(Bits.Bit03 | Bits.Bit01),
        /// <summary>
        /// 1001 Vendor defined trigger 2.
        /// </summary>
        VendorDefinedTrigger2 = (byte)(Bits.Bit03 | Bits.Bit00),
        /// <summary>
        /// 1000 Vendor defined trigger 1.
        /// </summary>
        VendorDefinedTrigger1 = (byte)Bits.Bit03,
        /// <summary>
        /// 0111 Digital.
        /// </summary>
        Digital = (byte)(Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 0101 df/dt high.
        /// </summary>
        DfDtHigh = (byte)(Bits.Bit02 | Bits.Bit00),
        /// <summary>
        /// 0011 Phase angle difference.
        /// </summary>
        PhaseAngleDifference = (byte)(Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 0001 Magnitude low.
        /// </summary>
        MagnitudeLow = (byte)Bits.Bit00,
        /// <summary>
        /// 0110 Reserved.
        /// </summary>
        Reserved = (byte)(Bits.Bit02 | Bits.Bit01),
        /// <summary>
        /// 0100 Frequency high/low.
        /// </summary>
        FrequencyHighOrLow = (byte)Bits.Bit02,
        /// <summary>
        /// 0010 Magnitude high.
        /// </summary>
        MagnitudeHigh = (byte)Bits.Bit01,
        /// <summary>
        /// 0000 Manual.
        /// </summary>
        Manual = (byte)Bits.Nil
    }

    /// <summary>
    /// Phasor data modification flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum PhasorDataModifications : ushort
    {
        /// <summary>
        /// No modifications.
        /// </summary>
        NoModifications = (ushort)Bits.Nil,
        /// <summary>
        /// Up-sampled with interpolation.
        /// </summary>
        UpSampledWithInterpolation = (ushort)Bits.Bit01,
        /// <summary>
        /// Up-sampled with extrapolation.
        /// </summary>
        UpSampledWithExtrapolation = (ushort)Bits.Bit02,
        /// <summary>
        /// Down-sampled by re-selection (selecting every Nth sample).
        /// </summary>
        DownSampledByReselection = (ushort)Bits.Bit03,
        /// <summary>
        /// Down sampled with FIR filter.
        /// </summary>
        DownSampledWithFIRFilter = (ushort)Bits.Bit04,
        /// <summary>
        /// Down-sampled with non-FIR filter.
        /// </summary>
        DownSampledWithNonFIRFilter = (ushort)Bits.Bit05,
        /// <summary>
        /// Filtered without changing sampling.
        /// </summary>
        FilteredNoChangeToSampling = (ushort)Bits.Bit06,
        /// <summary>
        /// Phasor magnitude adjusted for calibration.
        /// </summary>
        MagnitudeCalibrationAdjustment = (ushort)Bits.Bit07,
        /// <summary>
        /// Phasor phase adjusted for calibration.
        /// </summary>
        AngleCalibrationAdjustment = (ushort)Bits.Bit08,
        /// <summary>
        /// Phasor phase adjusted for rotation ( ±30º, ±120º, etc.).
        /// </summary>
        AngleRotationAdjustment = (ushort)Bits.Bit09,
        /// <summary>
        /// Pseudo-phasor value (combined from other phasors).
        /// </summary>
        PseudoPhasorValue = (ushort)Bits.Bit10,
        /// <summary>
        /// Modification applied, type not here defined.
        /// </summary>
        OtherModificationApplied = (ushort)Bits.Bit15,
        /// <summary>
        /// Reserved bits.
        /// </summary>
        Reserved = (ushort)(Bits.Bit00 | Bits.Bit11 | Bits.Bit12 | Bits.Bit13 | Bits.Bit14)
    }

    /// <summary>
    /// Phasor type enumeration.
    /// </summary>
    [Serializable]
    public enum PhasorTypeIndication : byte
    {
        /// <summary>
        /// Phasor type, 0 for voltage, 1 for current.
        /// </summary>
        Type = (byte)Bits.Bit03,
        /// <summary>
        /// Phasor component mask.
        /// </summary>
        ComponentMask = (byte)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02),
        /// <summary>
        /// Reserved bits.
        /// </summary>
        Reserved = (byte)(Bits.Bit04 | Bits.Bit05 | Bits.Bit06 | Bits.Bit07)
    }

    /// <summary>
    /// Phasor component enumeration.
    /// </summary>
    [Serializable]
    public enum PhasorComponent : byte
    {
        /// <summary>
        /// Zero sequence phasor.
        /// </summary>
        ZeroSequence = (byte)Bits.Nil,
        /// <summary>
        /// Positive sequence phasor.
        /// </summary>
        PositiveSequence = (byte)Bits.Bit00,
        /// <summary>
        /// Negative sequence phasor.
        /// </summary>
        NegativeSequence = (byte)Bits.Bit01,
        /// <summary>
        /// Reserved sequence phasor.
        /// </summary>
        ReservedSequence = (byte)(Bits.Bit00 | Bits.Bit01),
        /// <summary>
        /// Phase-A phasor.
        /// </summary>
        PhaseA = (byte)Bits.Bit02,
        /// <summary>
        /// Phase-B phasor.
        /// </summary>
        PhaseB = (byte)(Bits.Bit00 | Bits.Bit02),
        /// <summary>
        /// Phase-C phasor.
        /// </summary>
        PhaseC = (byte)(Bits.Bit01 | Bits.Bit02),
        /// <summary>
        /// Reserved phase phasor.
        /// </summary>
        ReservedPhase = (byte)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02)
    }

    #endregion

    /// <summary>
    /// Common IEEE C37.118 declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumPhasorValues = ushort.MaxValue / 4 - CommonFrameHeader.FixedLength - 8;

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumAnalogValues = ushort.MaxValue / 2 - CommonFrameHeader.FixedLength - 8;

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumDigitalValues = ushort.MaxValue / 2 - CommonFrameHeader.FixedLength - 8;

        /// <summary>
        /// Absolute maximum data length (in bytes) that could fit into any frame.
        /// </summary>
        public const ushort MaximumDataLength = ushort.MaxValue - CommonFrameHeader.FixedLength - 2;

        /// <summary>
        /// Absolute maximum number of bytes of extended data that could fit into a command frame.
        /// </summary>
        public const ushort MaximumExtendedDataLength = MaximumDataLength - 2;

        /// <summary>
        /// Time quality flags mask.
        /// </summary>
        public const uint TimeQualityFlagsMask = (uint)(Bits.Bit31 | Bits.Bit30 | Bits.Bit29 | Bits.Bit28 | Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24);

        /// <summary>
        /// Gets the version number for a given <see cref="DraftRevision"/>.
        /// </summary>
        /// <param name="revision">Target <see cref="DraftRevision"/>.</param>
        /// <returns>Version number for the specified <paramref name="revision"/>.</returns>
        public static string ToVersionString(this DraftRevision revision)
        {
            return revision switch
            {
                DraftRevision.Draft6 => "Draft 6",
                DraftRevision.Std2005 => "2005",
                DraftRevision.Std2011 => "2011",
                _ => "Unknown",
            };
        }
    }
}
