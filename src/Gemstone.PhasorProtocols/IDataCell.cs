﻿//******************************************************************************************************
//  IDataCell.cs - Gbtc
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
//  04/16/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using Gemstone.Timeseries;

namespace Gemstone.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Data sorting types enumeration.
    /// </summary>
    [Serializable]
    public enum DataSortingType
    {
        /// <summary>
        /// Data sorted by timestamp (typical situation).
        /// </summary>
        ByTimestamp,
        /// <summary>
        /// Data sorted by arrival (bad timestamp).
        /// </summary>
        ByArrival
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of any kind of <see cref="IDataFrame"/> cell.
    /// </summary>
    /// <remarks>
    /// This phasor protocol implementation defines a "cell" as a portion of a "frame", i.e., a logical unit of data.
    /// For example, a <see cref="IDataCell"/> could be defined as a PMU within a <see cref="IDataFrame"/> that contains
    /// multiple PMU's coming from a PDC.
    /// </remarks>
    public interface IDataCell : IChannelCell
    {
        /// <summary>
        /// Gets reference to parent <see cref="IDataFrame"/> of this <see cref="IDataCell"/>.
        /// </summary>
        new IDataFrame Parent { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IConfigurationCell"/> associated with this <see cref="IDataCell"/>.
        /// </summary>
        IConfigurationCell ConfigurationCell { get; set; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="IDataCell"/>.
        /// </summary>
        new IDataCellParsingState? State { get; set; }

        /// <summary>
        /// Gets station name of this <see cref="IDataCell"/>.
        /// </summary>
        string StationName { get; }

        /// <summary>
        /// Gets ID label of this <see cref="IDataCell"/>.
        /// </summary>
        string IDLabel { get; }

        /// <summary>
        /// Gets or sets 16-bit status flags of this <see cref="IDataCell"/>.
        /// </summary>
        ushort StatusFlags { get; set; }

        /// <summary>
        /// Gets or sets command status flags of this <see cref="IDataCell"/>.
        /// </summary>
        uint CommonStatusFlags { get; set; }

        /// <summary>
        /// Gets flag that determines if all values of this <see cref="IDataCell"/> have been assigned.
        /// </summary>
        bool AllValuesAssigned { get; }

        /// <summary>
        /// Gets <see cref="PhasorValueCollection"/> of this <see cref="IDataCell"/>.
        /// </summary>
        PhasorValueCollection PhasorValues { get; }

        /// <summary>
        /// Gets <see cref="IFrequencyValue"/> of this <see cref="IDataCell"/>.
        /// </summary>
        IFrequencyValue FrequencyValue { get; set; }

        /// <summary>
        /// Gets <see cref="AnalogValueCollection"/>of this <see cref="IDataCell"/>.
        /// </summary>
        AnalogValueCollection AnalogValues { get; }

        /// <summary>
        /// Gets <see cref="DigitalValueCollection"/>of this <see cref="IDataCell"/>.
        /// </summary>
        DigitalValueCollection DigitalValues { get; }

        // These properties correspond to the CommonStatusFlags enumeration
        // allowing all protocols to implement a common set of status flags

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="IDataCell"/> is valid.
        /// </summary>
        bool DataIsValid { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="IDataCell"/> is valid based on GPS lock.
        /// </summary>
        bool SynchronizationIsValid { get; set; }

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.DataSortingType"/> of this <see cref="IDataCell"/>.
        /// </summary>
        DataSortingType DataSortingType { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="IDataCell"/> is reporting an error.
        /// </summary>
        bool DeviceError { get; set; }

        /// <summary>
        /// Gets the status flags of the <see cref="IDataCell"/> as a measurement value.
        /// </summary>
        /// <returns>Status flags of the <see cref="IDataCell"/> as a measurement value.</returns>
        IMeasurement GetStatusFlagsMeasurement();
    }
}
