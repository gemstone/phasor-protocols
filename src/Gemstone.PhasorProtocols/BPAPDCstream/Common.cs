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
//  05/19/2011 - Ritchie
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace Gemstone.PhasorProtocols.BPAPDCstream
{
    #region [ Enumerations ]

    /// <summary>
    /// Stream type enueration.
    /// </summary>
    [Serializable]
    public enum StreamType : byte
    {
        /// <summary>
        /// Standard full data stream.
        /// </summary>
        Legacy = 0,
        /// <summary>
        /// Full data stream with PMU ID's and offsets removed from data packet.
        /// </summary>
        Compact = 1
    }

    /// <summary>
    /// File type enueration.
    /// </summary>
    [Serializable]
    public enum FileType : byte
    {
        /// <summary>
        /// PDC phasor data recording, NTP time stamp (SOC starts 1900).
        /// </summary>
        PdcNtp = 1,
        /// <summary>
        /// PDC phasor data recording, UNIX time stamp (SOC starts 1970).
        /// </summary>
        PdcUnix = 2,
        /// <summary>
        /// PPSM sample data recording.
        /// </summary>
        Ppsm = 4
    }

    /// <summary>
    /// Stream revision number enumeration.
    /// </summary>
    [Serializable]
    public enum RevisionNumber : byte
    {
        /// <summary>
        /// Original revision for all to June 2002, use NTP timetag (start count 1900).
        /// </summary>
        Revision0 = 0,
        /// <summary>
        /// July 2002 revision for std. 37.118, use UNIX timetag (start count 1970).
        /// </summary>
        Revision1 = 1,
        /// <summary>
        /// May 2005 revision for std. 37.118, change ChanFlag for added data types.
        /// </summary>
        Revision2 = 2
    }

    /// <summary>
    /// File version type.
    /// </summary>
    [Serializable]
    public enum FileVersion : byte
    {
        /// <summary>
        /// PDC phasor data recording, including Dbuf flag.
        /// </summary>
        PdcWithDbuf = 01,
        /// <summary>
        ///  PDC phasor data recording, excluding Dbuf flag.
        /// </summary>
        PdcWithoutDbuf = 02
    }

    /// <summary>
    /// Channel flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum ChannelFlags : byte
    {
        /// <summary>
        /// Valid if not set (yes = 0).
        /// </summary>
        DataIsValid = (byte)Bits.Bit07,
        /// <summary>
        /// Errors if set (yes = 1).
        /// </summary>
        TransmissionErrors = (byte)Bits.Bit06,
        /// <summary>
        /// Not sync'd if set (yes = 0).
        /// </summary>
        PmuSynchronized = (byte)Bits.Bit05,
        /// <summary>
        /// Data out of sync if set (yes = 1).
        /// </summary>
        DataSortedByArrival = (byte)Bits.Bit04,
        /// <summary>
        /// Sorted by timestamp if not set (yes = 0).
        /// </summary>
        DataSortedByTimestamp = (byte)Bits.Bit03,
        /// <summary>
        /// PDC format if set (yes = 1).
        /// </summary>
        PdcExchangeFormat = (byte)Bits.Bit02,
        /// <summary>
        /// Macrodyne or IEEE format (Macrodyne = 1).
        /// </summary>
        MacrodyneFormat = (byte)Bits.Bit01,
        /// <summary>
        /// Timestamp included if not set (yes = 0).
        /// </summary>
        TimestampIncluded = (byte)Bits.Bit00,
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (byte)Bits.Nil
    }

    /// <summary>
    /// Reserved flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum ReservedFlags : byte
    {
        /// <summary>
        /// Reserved bit 7.
        /// </summary>
        Reserved0 = (byte)Bits.Bit07,
        /// <summary>
        /// Reserved bit 6.
        /// </summary>
        Reserved1 = (byte)Bits.Bit06,
        /// <summary>
        /// Analog words mask.
        /// </summary>
        AnalogWordsMask = (byte)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03 | Bits.Bit04 | Bits.Bit05),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (byte)Bits.Nil
    }

    /// <summary>
    /// Format flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum FormatFlags : byte
    {
        /// <summary>
        /// Frequency data format: Set = float, Clear = integer.
        /// </summary>
        Frequency = (byte)Bits.Bit07,
        /// <summary>
        /// Analog data format: Set = float, Clear = integer.
        /// </summary>
        Analog = (byte)Bits.Bit06,
        /// <summary>
        /// Phasor data format: Set = float, Clear = integer.
        /// </summary>
        Phasors = (byte)Bits.Bit05,
        /// <summary>
        /// Phasor coordinate format: Set = polar, Clear = rectangular.
        /// </summary>
        Coordinates = (byte)Bits.Bit04,
        /// <summary>
        /// Digital words mask.
        /// </summary>
        DigitalWordsMask = (byte)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (byte)Bits.Nil
    }

    /// <summary>
    /// Frame type enumeration.
    /// </summary>
    [Serializable]
    public enum FrameType : byte
    {
        /// <summary>
        /// Configuration frame.
        /// </summary>
        ConfigurationFrame,
        /// <summary>
        /// Data frame.
        /// </summary>
        DataFrame
    }

    /// <summary>
    /// PMU status flags enumeration.
    /// </summary>
    [Flags]
    [Serializable]
    public enum PMUStatusFlags : byte
    {
        /// <summary>
        /// Synchronization is invalid.
        /// </summary>
        SyncInvalid = (byte)Bits.Bit00,
        /// <summary>
        /// Data is invalid.
        /// </summary>
        DataInvalid = (byte)Bits.Bit01,
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (byte)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Common BPA PDCstream declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Descriptor packet flag.
        /// </summary>
        public const byte DescriptorPacketFlag = 0x0;

        /// <summary>
        /// Phasor file format flag.
        /// </summary>
        public const byte PhasorFileFormatFlag = 0xCC;

        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumPhasorValues = byte.MaxValue;

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumAnalogValues = (ushort)ReservedFlags.AnalogWordsMask;

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumDigitalValues = (ushort)FormatFlags.DigitalWordsMask;

        /// <summary>
        /// Absolute maximum data length (in bytes) that could fit into any frame.
        /// </summary>
        public const ushort MaximumDataLength = ushort.MaxValue - CommonFrameHeader.FixedLength - 2;
    }
}
