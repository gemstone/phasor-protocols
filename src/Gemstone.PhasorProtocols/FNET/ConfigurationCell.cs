﻿//******************************************************************************************************
//  ConfigurationCell.cs - Gbtc
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
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Gemstone.Numeric.EE;

// ReSharper disable VirtualMemberCallInConstructor
namespace Gemstone.PhasorProtocols.FNET
{
    /// <summary>
    /// Represents the F-NET implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationCell : ConfigurationCellBase
    {
        #region [ Members ]

        // Fields
        private Ticks m_timeOffset;
        private double m_longitude;
        private double m_latitude;
        private int m_numberOfSatellites = 1; // We'll initially assume synchronization is good until told otherwise

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame"/> of this <see cref="ConfigurationCell"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.</param>
        /// <param name="timeOffset">The time offset of F-NET device in <see cref="Ticks"/>.</param>
        internal ConfigurationCell(ConfigurationFrame parent, LineFrequency nominalFrequency, Ticks timeOffset)
            : base(parent, parent.IDCode, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            NominalFrequency = nominalFrequency;
            m_timeOffset = timeOffset;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration cell
            m_timeOffset = info.GetInt64("timeOffset");
            m_longitude = info.GetDouble("longitude");
            m_latitude = info.GetDouble("latitude");
            m_numberOfSatellites = info.GetInt32("numberOfSatellites");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ConfigurationFrame"/> for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ConfigurationFrame Parent
        {
            get => base.Parent as ConfigurationFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override ushort IDCode
        {
            // F-NET protocol only allows one device, so we share ID code with parent frame...
            get => Parent.IDCode;
            set
            {
                Parent.IDCode = value;
                base.IDCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports floating-point data; F-NET doesn't transport scaled values.
        /// </remarks>
        /// <exception cref="NotSupportedException">F-NET only supports floating-point data.</exception>
        public override DataFormat PhasorDataFormat
        {
            get => DataFormat.FloatingPoint;
            set
            {
                if (value != DataFormat.FloatingPoint)
                    throw new NotSupportedException("F-NET only supports floating-point data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports polar phasor data; F-NET doesn't transport rectangular phasor values.
        /// </remarks>
        /// <exception cref="NotSupportedException">F-NET only supports polar phasor data.</exception>
        public override CoordinateFormat PhasorCoordinateFormat
        {
            get => CoordinateFormat.Polar;
            set
            {
                if (value != CoordinateFormat.Polar)
                    throw new NotSupportedException("F-NET only supports polar phasor data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports floating-point data; F-NET doesn't transport scaled values.
        /// </remarks>
        /// <exception cref="NotSupportedException">F-NET only supports floating-point data.</exception>
        public override DataFormat FrequencyDataFormat
        {
            get => DataFormat.FloatingPoint;
            set
            {
                if (value != DataFormat.FloatingPoint)
                    throw new NotSupportedException("F-NET only supports floating-point data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports floating-point data; F-NET doesn't transport scaled values.
        /// </remarks>
        /// <exception cref="NotSupportedException">F-NET only supports floating-point data.</exception>
        public override DataFormat AnalogDataFormat
        {
            get => DataFormat.FloatingPoint;
            set
            {
                if (value != DataFormat.FloatingPoint)
                    throw new NotSupportedException("F-NET only supports floating-point data");
            }
        }

        /// <summary>
        /// Gets or sets time offset of F-NET device in <see cref="Ticks"/>.
        /// </summary>
        /// <remarks>
        /// F-NET devices normally report time in 11 seconds past real-time, this property defines the offset for this artificial delay.
        /// Note that the parameter value is in ticks to allow a very high-resolution offset;  1 second = 10,000,000 ticks.
        /// </remarks>
        public Ticks TimeOffset
        {
            get => m_timeOffset;
            set => m_timeOffset = value;
        }

        /// <summary>
        /// Gets or sets the longitude (i.e., East/West geospatial position) of the device defined by this <see cref="ConfigurationCell"/>.
        /// </summary>
        public double Longitude
        {
            get => m_longitude;
            set => m_longitude = value;
        }

        /// <summary>
        /// Gets or sets the latitude (i.e., North/South geospatial position) of the device defined by this <see cref="ConfigurationCell"/>.
        /// </summary>
        public double Latitude
        {
            get => m_latitude;
            set => m_latitude = value;
        }

        /// <summary>
        /// Gets or sets the number of satellites visible to the GPS timing source of the device defined by this <see cref="ConfigurationCell"/>.
        /// </summary>
        public int NumberOfSatellites
        {
            get => m_numberOfSatellites;
            set => m_numberOfSatellites = value;
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ConfigurationCellBase.StationName"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override int MaximumStationNameLength => int.MaxValue; // The station name is defined external to the protocol, so there is no set limit

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Time Offset (ticks)", ((long)TimeOffset).ToString());
                baseAttributes.Add("Time Offset (seconds)", Ticks.ToSeconds(TimeOffset).ToString());
                baseAttributes.Add("Longitude", Longitude.ToString());
                baseAttributes.Add("Latitude", Latitude.ToString());
                baseAttributes.Add("Number of Satellites", NumberOfSatellites.ToString());

                return baseAttributes;
            }
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
            base.GetObjectData(info, context);

            // Serialize configuration cell
            info.AddValue("timeOffset", (long)m_timeOffset);
            info.AddValue("longitude", m_longitude);
            info.AddValue("latitude", m_latitude);
            info.AddValue("numberOfSatellites", m_numberOfSatellites);
        }

        #endregion
    }
}