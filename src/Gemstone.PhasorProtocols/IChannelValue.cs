﻿//******************************************************************************************************
//  IChannelValue.cs - Gbtc
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Runtime.Serialization;
using Gemstone.Timeseries;

namespace Gemstone.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation any kind of <see cref="IChannel"/> data value.
    /// </summary>
    /// <remarks>
    /// Each instance of <see cref="IChannelValue{T}"/> will have a more specific derived implementation (e.g., <see cref="IDigitalValue"/> and <see cref="IPhasorValue"/>),
    /// these specific implementations of <see cref="IChannelValue{T}"/> will be referenced children of a <see cref="IDataCell"/>.<br/>
    /// The <see cref="IChannelValue{T}"/> uses the specified <see cref="IChannelDefinition"/> type to define its properties.
    /// </remarks>
    /// <typeparam name="T">Specific <see cref="IChannelDefinition"/> type that represents the <see cref="IChannelValue{T}"/> definition.</typeparam>
    public interface IChannelValue<T> : IChannel, ISerializable where T : IChannelDefinition
    {
        /// <summary>
        /// Gets the <see cref="IDataCell"/> parent of this <see cref="IChannelValue{T}"/>.
        /// </summary>
        IDataCell Parent { get; set; }

        /// <summary>
        /// Gets the <see cref="IChannelDefinition"/> associated with this <see cref="IChannelValue{T}"/>.
        /// </summary>
        T Definition { get; set; }

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="IChannelValue{T}"/> typically derived from <see cref="IChannelDefinition.DataFormat"/>.
        /// </summary>
        DataFormat DataFormat { get; }

        /// <summary>
        /// Gets text based label of this <see cref="IChannelValue{T}"/> typically derived from <see cref="IChannelDefinition.Label"/>.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="IChannelValue{T}"/> have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the composite values of this <see cref="IChannelValue{T}"/> as an array of <see cref="IMeasurement"/> values.
        /// </summary>
        IMeasurement[] Measurements { get; }

        /// <summary>
        /// Gets total number of composite values that this <see cref="IChannelValue{T}"/> provides.
        /// </summary>
        int CompositeValueCount { get; }

        /// <summary>
        /// Gets the specified composite value of this <see cref="IChannelValue{T}"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        /// <returns>A <see cref="double"/> representing the composite value.</returns>
        double GetCompositeValue(int index);
    }
}