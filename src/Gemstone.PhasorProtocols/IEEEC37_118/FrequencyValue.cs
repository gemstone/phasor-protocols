﻿//******************************************************************************************************
//  FrequencyValue.cs - Gbtc
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
using System.Runtime.Serialization;

namespace Gemstone.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IFrequencyValue"/>.
    /// </summary>
    [Serializable]
    public class FrequencyValue : FrequencyValueBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyValue"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="FrequencyValue"/>.</param>
        /// <param name="frequencyDefinition">The <see cref="IFrequencyDefinition"/> associated with this <see cref="FrequencyValue"/>.</param>
        public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition)
            : base(parent, frequencyDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="FrequencyValue"/>.</param>
        /// <param name="frequencyDefinition">The <see cref="FrequencyDefinition"/> associated with this <see cref="FrequencyValue"/>.</param>
        /// <param name="frequency">The floating point value that represents this <see cref="FrequencyValue"/>.</param>
        /// <param name="dfdt">The floating point value that represents the change in this <see cref="FrequencyValue"/> over time.</param>
        public FrequencyValue(DataCell parent, FrequencyDefinition frequencyDefinition, double frequency, double dfdt)
            : base(parent, frequencyDefinition, frequency, dfdt)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyValue"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DataCell"/> parent of this <see cref="FrequencyValue"/>.
        /// </summary>
        public new virtual DataCell Parent
        {
            get => base.Parent as DataCell;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="FrequencyDefinition"/> associated with this <see cref="FrequencyValue"/>.
        /// </summary>
        public new virtual FrequencyDefinition Definition
        {
            get => base.Definition as FrequencyDefinition;
            set => base.Definition = value;
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of this <see cref="FrequencyValue"/>.
        /// </summary>
        public override int UnscaledFrequency
        {
            get => double.IsNaN(Frequency) ? short.MinValue : base.UnscaledFrequency;
            set
            {
                if (value <= short.MinValue)
                    Frequency = double.NaN;
                else
                    base.UnscaledFrequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the change in this <see cref="FrequencyValue"/> over time.
        /// </summary>
        public override int UnscaledDfDt
        {
            get => double.IsNaN(DfDt) ? short.MinValue : base.UnscaledDfDt;
            set
            {
                if (value <= short.MinValue)
                    DfDt = double.NaN;
                else
                    base.UnscaledDfDt = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 frequency value
        internal static IFrequencyValue CreateNewValue(IDataCell parent, IFrequencyDefinition definition, byte[] buffer, int startIndex, out int parsedLength)
        {
            IFrequencyValue frequency = new FrequencyValue(parent, definition);

            parsedLength = frequency.ParseBinaryImage(buffer, startIndex, 0);

            return frequency;
        }

        #endregion
    }
}