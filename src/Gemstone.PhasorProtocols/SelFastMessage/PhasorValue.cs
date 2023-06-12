﻿//******************************************************************************************************
//  PhasorValue.cs - Gbtc
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
//  04/27/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using Gemstone.Units;

namespace Gemstone.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="IPhasorValue"/>.
    /// </summary>
    [Serializable]
    public class PhasorValue : PhasorValueBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="IPhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        public PhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition)
            : base(parent, phasorDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        /// <param name="real">The real value of this <see cref="PhasorValue"/>.</param>
        /// <param name="imaginary">The imaginary value of this <see cref="PhasorValue"/>.</param>
        public PhasorValue(DataCell parent, PhasorDefinition phasorDefinition, double real, double imaginary)
            : base(parent, phasorDefinition, real, imaginary)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        /// <param name="angle">The <see cref="Angle"/> value (a.k.a., the argument) of this <see cref="PhasorValue"/>, in radians.</param>
        /// <param name="magnitude">The magnitude value (a.k.a., the absolute value or modulus) of this <see cref="PhasorValue"/>.</param>
        public PhasorValue(DataCell parent, PhasorDefinition phasorDefinition, Angle angle, double magnitude)
            : base(parent, phasorDefinition, angle, magnitude)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.
        /// </summary>
        public new virtual DataCell Parent
        {
            get => base.Parent as DataCell;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.
        /// </summary>
        public new virtual PhasorDefinition Definition
        {
            get => base.Definition as PhasorDefinition;
            set => base.Definition = value;
        }
        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => 8;

        /// <summary>
        /// Gets the binary body image of the <see cref="PhasorValueBase"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[8];

                BigEndian.CopyBytes((float)Magnitude, buffer, 0);
                BigEndian.CopyBytes((float)Angle.ToDegrees(), buffer, 4);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            // Parse magnitude and angle in degrees
            Magnitude = BigEndian.ToSingle(buffer, startIndex);
            Angle = Angle.FromDegrees(BigEndian.ToSingle(buffer, startIndex + 4));

            return 8;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new SEL Fast Message phasor value
        internal static IPhasorValue CreateNewValue(IDataCell parent, IPhasorDefinition definition, byte[] buffer, int startIndex, out int parsedLength)
        {
            IPhasorValue phasor = new PhasorValue(parent, definition);

            parsedLength = phasor.ParseBinaryImage(buffer, startIndex, 0);

            return phasor;
        }

        #endregion
    }
}