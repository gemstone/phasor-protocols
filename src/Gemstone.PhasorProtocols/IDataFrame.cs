//******************************************************************************************************
//  IDataFrame.cs - Gbtc
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


using Gemstone.Timeseries;

namespace Gemstone.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any kind of data frame that contains
    /// a collection of <see cref="IDataCell"/> objects.
    /// </summary>
    public interface IDataFrame : IChannelFrame
    {
        /// <summary>
        /// Gets or sets <see cref="IConfigurationFrame"/> associated with this <see cref="IDataFrame"/>.
        /// </summary>
        IConfigurationFrame ConfigurationFrame { get; set; }

        /// <summary>
        /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="IDataFrame"/>.
        /// </summary>
        new DataCellCollection Cells { get; }

        /// <summary>
        /// Gets or sets the parsing state for this <see cref="IDataFrame"/>.
        /// </summary>
        new IDataFrameParsingState? State { get; set; }

        /// <summary>
        /// Gets or sets protocol specific quality flags for this <see cref="IDataFrame"/>.
        /// </summary>
        uint QualityFlags { get; set; }

        /// <summary>
        /// Gets the quality flags of the <see cref="IDataFrame"/> as a measurement value.
        /// </summary>
        /// <returns>Quality flags of the <see cref="IDataFrame"/> as a measurement value.</returns>
        IMeasurement GetQualityFlagsMeasurement();
    }
}
