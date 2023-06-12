﻿//******************************************************************************************************
//  ConnectionParameters.cs - Gbtc
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
//  02/26/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Gemstone.Numeric.EE;

namespace Gemstone.PhasorProtocols.FNET
{
    /// <summary>
    /// Represents the extra connection parameters required for a connection to a F-NET device.
    /// </summary>
    /// <remarks>
    /// This class is designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.
    /// As a result the <see cref="CategoryAttribute"/> and <see cref="DescriptionAttribute"/> elements should be defined for
    /// each of the exposed properties.
    /// </remarks>
    [Serializable]
    public class ConnectionParameters : ConnectionParametersBase
    {
        #region [ Members ]

        // Fields
        private long m_timeOffset;
        private ushort m_frameRate;
        private LineFrequency m_nominalFrequency;
        private string m_stationName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/>.
        /// </summary>
        public ConnectionParameters()
        {
            m_timeOffset = Common.DefaultTimeOffset;
            m_frameRate = Common.DefaultFrameRate;
            m_nominalFrequency = Common.DefaultNominalFrequency;
            m_stationName = Common.DefaultStationName;
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConnectionParameters(SerializationInfo info, StreamingContext context)
        {
            // Deserialize connection parameters
            m_timeOffset = info.GetOrDefault("timeOffset", Common.DefaultTimeOffset);
            m_frameRate = info.GetOrDefault("frameRate", Common.DefaultFrameRate);
            m_nominalFrequency = info.GetOrDefault("nominalFrequency", Common.DefaultNominalFrequency);
            m_stationName = info.GetOrDefault("stationName", Common.DefaultStationName);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets time offset of the F-NET device in <see cref="Ticks"/>.
        /// </summary>
        /// <remarks>
        /// F-NET devices normally report time in 11 seconds past real-time, this property defines the offset for this artificial delay.
        /// Note that the parameter value is in ticks to allow a very high-resolution offset;  1 second = 10,000,000 ticks.
        /// </remarks>
        [Category("Optional Connection Parameters")]
        [Description("F-NET devices normally report time in 11 seconds past real-time, this parameter adjusts for this artificial delay.  Note parameter is in ticks (1 second = 10,000,000 ticks).")]
        [DefaultValue(Common.DefaultTimeOffset)]
        public long TimeOffset
        {
            get => m_timeOffset;
            set => m_timeOffset = value;
        }

        /// <summary>
        /// Gets or sets the configured frame rate for the F-NET device.
        /// </summary>
        /// <remarks>
        /// This is typically set to 10 frames per second.
        /// </remarks>
        [Category("Optional Connection Parameters")]
        [Description("Configured frame rate for F-NET device.")]
        [DefaultValue(Common.DefaultFrameRate)]
        public ushort FrameRate
        {
            get => m_frameRate;
            set => m_frameRate = value < 1 ? Common.DefaultFrameRate : value;
        }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of this F-NET device.
        /// </summary>
        [Category("Optional Connection Parameters")]
        [Description("Configured nominal frequency for F-NET device.")]
        [DefaultValue(typeof(LineFrequency), "Hz60")]
        public LineFrequency NominalFrequency
        {
            get => m_nominalFrequency;
            set => m_nominalFrequency = value;
        }

        /// <summary>
        /// Gets or sets the station name for the F-NET device.
        /// </summary>
        [Category("Optional Connection Parameters")]
        [Description("Station name to use for F-NET device.")]
        [DefaultValue(Common.DefaultStationName)]
        public string StationName
        {
            get => m_stationName;
            set => m_stationName = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize connection parameters
            info.AddValue("timeOffset", m_timeOffset);
            info.AddValue("frameRate", m_frameRate);
            info.AddValue("nominalFrequency", m_nominalFrequency, typeof(LineFrequency));
            info.AddValue("stationName", m_stationName);
        }

        #endregion
    }
}