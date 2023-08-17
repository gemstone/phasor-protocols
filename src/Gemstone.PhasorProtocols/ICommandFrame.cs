﻿//******************************************************************************************************
//  ICommandFrame.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace Gemstone.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Phasor enabled device commands enumeration.
    /// </summary>
    [Serializable]
    public enum DeviceCommand : ushort
    {
        /// <summary>
        /// 0000 0000 0000 0001 Turn off transmission of data frames.
        /// </summary>
        DisableRealTimeData = (ushort)Bits.Bit00,
        /// <summary>
        /// 0000 0000 0000 0010 Turn on transmission of data frames.
        /// </summary>
        EnableRealTimeData = (ushort)Bits.Bit01,
        /// <summary>
        /// 0000 0000 0000 0011 Send header file.
        /// </summary>
        SendHeaderFrame = (ushort)Bits.Bit00 | (ushort)Bits.Bit01,
        /// <summary>
        /// 0000 0000 0000 0100 Send configuration file 1.
        /// </summary>
        SendConfigurationFrame1 = (ushort)Bits.Bit02,
        /// <summary>
        /// 0000 0000 0000 0101 Send configuration file 2.
        /// </summary>
        SendConfigurationFrame2 = (ushort)Bits.Bit00 | (ushort)Bits.Bit02,
        /// <summary>
        /// 0000 0000 0000 0110 Send configuration file 3.
        /// </summary>
        SendConfigurationFrame3 = (ushort)Bits.Bit01 | (ushort)Bits.Bit02,
        /// <summary>
        /// 0000 0000 0000 1000 Receive extended frame for IEEE C37.118 / receive reference phasor for IEEE 1344.
        /// </summary>
        ReceiveExtendedFrame = (ushort)Bits.Bit03,
        /// <summary>
        /// 0000 1000 0000 0000 User designated command. Used to send latest configuration frame version.
        /// </summary>
        SendLatestConfigurationFrameVersion = (ushort)Bits.Bit11,
        /// <summary>
        /// Reserved bits.
        /// </summary>
        ReservedBits = ushort.MaxValue & ~((ushort)Bits.Bit00 | (ushort)Bits.Bit01 | (ushort)Bits.Bit02 | (ushort)Bits.Bit03)
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of any kind of command frame that contains
    /// a collection of <see cref="ICommandCell"/> objects.
    /// </summary>
    public interface ICommandFrame : IChannelFrame<ICommandCell>
    {
        /// <summary>
        /// Gets reference to the <see cref="CommandCellCollection"/> for this <see cref="ICommandFrame"/>.
        /// </summary>
        new CommandCellCollection Cells { get; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ICommandFrame"/>.
        /// </summary>
        new ICommandFrameParsingState? State { get; set; }
    
        /// <summary>
        /// Gets or sets <see cref="DeviceCommand"/> for this <see cref="ICommandFrame"/>.
        /// </summary>
        DeviceCommand Command { get; set; }

        /// <summary>
        /// Gets or sets extended binary image data for this <see cref="ICommandFrame"/>.
        /// </summary>
        byte[] ExtendedData { get; set; }
    }
}
