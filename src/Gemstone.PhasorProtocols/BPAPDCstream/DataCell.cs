﻿//******************************************************************************************************
//  DataCell.cs - Gbtc
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
//  05/20/2011 - J. Ritchie Carroll
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Gemstone.WordExtensions;

// ReSharper disable VirtualMemberCallInConstructor
namespace Gemstone.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IDataCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class DataCell : DataCellBase
    {
        #region [ Members ]

        // Fields
        private ChannelFlags m_channelFlags;
        private ReservedFlags m_reservedFlags;
        private ushort m_sampleNumber;
        private byte m_dataRate;
        private uint m_dataBuffer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
            : base(parent, configurationCell, 0xFFFF, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Define new parsing state which defines constructors for key data values
            State = new DataCellParsingState(
                configurationCell,
                PhasorValue.CreateNewValue,
                BPAPDCstream.FrequencyValue.CreateNewValue,
                AnalogValue.CreateNewValue,
                DigitalValue.CreateNewValue);
        }

        /// <summary>
        /// Creates a new <see cref="DataCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="DataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        /// <param name="addEmptyValues">If <c>true</c>, adds empty values for each defined configuration cell definition.</param>
        public DataCell(DataFrame parent, ConfigurationCell configurationCell, bool addEmptyValues)
            : this(parent, configurationCell)
        {
            if (!addEmptyValues)
                return;

            // Define needed phasor values
            foreach (IPhasorDefinition phasorDefinition in configurationCell.PhasorDefinitions)
                PhasorValues.Add(new PhasorValue(this, phasorDefinition));

            // Define a frequency and df/dt
            FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition);

            // Define any analog values
            foreach (IAnalogDefinition analogDefinition in configurationCell.AnalogDefinitions)
                AnalogValues.Add(new AnalogValue(this, analogDefinition));

            // Define any digital values
            foreach (IDigitalDefinition digitalDefinition in configurationCell.DigitalDefinitions)
                DigitalValues.Add(new DigitalValue(this, digitalDefinition));
        }

        /// <summary>
        /// Creates a new <see cref="DataCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data cell
            m_channelFlags = (ChannelFlags)info.GetValue("channelFlags", typeof(ChannelFlags));
            m_reservedFlags = (ReservedFlags)info.GetValue("reservedFlags", typeof(ReservedFlags));
            m_sampleNumber = info.GetUInt16("sampleNumber");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the reference to parent <see cref="DataFrame"/> of this <see cref="DataCell"/>.
        /// </summary>
        public new DataFrame Parent
        {
            get => base.Parent as DataFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.
        /// </summary>
        public new ConfigurationCell ConfigurationCell
        {
            get => base.ConfigurationCell as ConfigurationCell;
            set => base.ConfigurationCell = value;
        }

        /// <summary>
        /// Gets or sets channel flags for this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// These are bit flags, use properties to change basic values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ChannelFlags ChannelFlags
        {
            get => m_channelFlags;
            set => m_channelFlags = value;
        }

        /// <summary>
        /// Gets or sets reserved flags for this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// These are bit flags, use properties to change basic values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ReservedFlags ReservedFlags
        {
            get => m_reservedFlags;
            set => m_reservedFlags = value;
        }

        /// <summary>
        /// Gets or sets data rate of this <see cref="DataCell"/>.
        /// </summary>
        public byte DataRate
        {
            get => Parent.ConfigurationFrame.RevisionNumber >= RevisionNumber.Revision2 ? (byte)Parent.ConfigurationFrame.FrameRate : m_dataRate;
            set => m_dataRate = value;
        }

        /// <summary>
        /// Gets or sets <see cref="BPAPDCstream.FormatFlags"/> from <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.
        /// </summary>
        public FormatFlags FormatFlags
        {
            get => ConfigurationCell.FormatFlags;
            set => ConfigurationCell.FormatFlags = value;
        }

        /// <summary>
        /// Gets or sets sample number associated with this <see cref="DataCell"/>.
        /// </summary>
        public ushort SampleNumber
        {
            get => m_sampleNumber;
            set => m_sampleNumber = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if reserved flag zero is set.
        /// </summary>
        public bool ReservedFlag0IsSet
        {
            get => (m_reservedFlags & ReservedFlags.Reserved0) > 0;
            set
            {
                if (value)
                    m_reservedFlags |= ReservedFlags.Reserved0;
                else
                    m_reservedFlags &= ~ReservedFlags.Reserved0;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if reserved flag one is set.
        /// </summary>
        public bool ReservedFlag1IsSet
        {
            get => (m_reservedFlags & ReservedFlags.Reserved1) > 0;
            set
            {
                if (value)
                    m_reservedFlags |= ReservedFlags.Reserved1;
                else
                    m_reservedFlags &= ~ReservedFlags.Reserved1;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get => (m_channelFlags & ChannelFlags.DataIsValid) == 0;
            set
            {
                if (value)
                    m_channelFlags &= ~ChannelFlags.DataIsValid;
                else
                    m_channelFlags |= ChannelFlags.DataIsValid;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        public override bool SynchronizationIsValid
        {
            get => (m_channelFlags & ChannelFlags.PmuSynchronized) == 0;
            set
            {
                if (value)
                    m_channelFlags &= ~ChannelFlags.PmuSynchronized;
                else
                    m_channelFlags |= ChannelFlags.PmuSynchronized;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
        /// </summary>
        public override DataSortingType DataSortingType
        {
            get => (m_channelFlags & ChannelFlags.DataSortedByArrival) > 0 ? DataSortingType.ByArrival : DataSortingType.ByTimestamp;
            set
            {
                if (value == DataSortingType.ByArrival)
                    m_channelFlags |= ChannelFlags.DataSortedByArrival;
                else
                    m_channelFlags &= ~ChannelFlags.DataSortedByArrival;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        public override bool DeviceError
        {
            get => (m_channelFlags & ChannelFlags.TransmissionErrors) > 0;
            set
            {
                if (value)
                    m_channelFlags |= ChannelFlags.TransmissionErrors;
                else
                    m_channelFlags &= ~ChannelFlags.TransmissionErrors;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> is using the PDC exchange format.
        /// </summary>
        public bool UsingPdcExchangeFormat
        {
            get => (m_channelFlags & ChannelFlags.PdcExchangeFormat) > 0;
            set
            {
                if (value)
                    m_channelFlags |= ChannelFlags.PdcExchangeFormat;
                else
                    m_channelFlags &= ~ChannelFlags.PdcExchangeFormat;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> is using Macrodyne format.
        /// </summary>
        public bool UsingMacrodyneFormat
        {
            get => (m_channelFlags & ChannelFlags.MacrodyneFormat) > 0;
            set
            {
                if (value)
                    m_channelFlags |= ChannelFlags.MacrodyneFormat;
                else
                    m_channelFlags &= ~ChannelFlags.MacrodyneFormat;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> is using IEEE format.
        /// </summary>
        public bool UsingIeeeFormat
        {
            get => (m_channelFlags & ChannelFlags.MacrodyneFormat) == 0;
            set
            {
                if (value)
                    m_channelFlags &= ~ChannelFlags.MacrodyneFormat;
                else
                    m_channelFlags |= ChannelFlags.MacrodyneFormat;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> data is sorted by timestamp.
        /// </summary>
        [Obsolete("This bit definition is for obsolete uses that is no longer needed.", false)]
        public bool DataIsSortedByTimestamp
        {
            get => (m_channelFlags & ChannelFlags.DataSortedByTimestamp) == 0;
            set
            {
                if (value)
                    m_channelFlags &= ~ChannelFlags.DataSortedByTimestamp;
                else
                    m_channelFlags |= ChannelFlags.DataSortedByTimestamp;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp is included with this <see cref="DataCell"/>.
        /// </summary>
        [Obsolete("This bit definition is for obsolete uses that is no longer needed.", false)]
        public bool TimestampIsIncluded
        {
            get => (m_channelFlags & ChannelFlags.TimestampIncluded) == 0;
            set
            {
                if (value)
                    m_channelFlags &= ~ChannelFlags.TimestampIncluded;
                else
                    m_channelFlags |= ChannelFlags.TimestampIncluded;
            }
        }

        /// <summary>
        /// Gets flag that determines if source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public bool UsePhasorDataFileFormat => Parent.UsePhasorDataFileFormat;

        /// <summary>
        /// Gets or sets data buffer long word that prefixes each cell when source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public uint DataBuffer
        {
            get => m_dataBuffer;
            set => m_dataBuffer = value;
        }

        /// <summary>
        /// Gets the length of the <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// This property is overridden to extend length evenly at 4-byte intervals.
        /// </remarks>
        public override int BinaryLength => base.BinaryLength.AlignDoubleWord();

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => 6;

        /// <summary>
        /// Gets the binary header image of the <see cref="DataCell"/> object.
        /// </summary>
        /// <remarks>
        /// Although this BPA PDCstream implementation <see cref="DataCell"/> will correctly parse a PDCxchng style
        /// stream, one will not be produced. Only a fully formatted stream will ever be produced.
        /// </remarks>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];

                // Add standard PDCstream specific image. There is no major benefit to justify development
                // that would to allow production of a PDCExchangeFormat stream.
                buffer[0] = (byte)(m_channelFlags & ~ChannelFlags.PdcExchangeFormat);

                if (Parent.ConfigurationFrame.RevisionNumber >= RevisionNumber.Revision2)
                {
                    buffer[1] = (byte)((AnalogValues.Count & (int)ReservedFlags.AnalogWordsMask) | (int)ReservedFlags);
                    buffer[2] = (byte)((DigitalValues.Count & (int)FormatFlags.DigitalWordsMask) | (int)FormatFlags);
                    buffer[3] = (byte)PhasorValues.Count;
                }
                else
                {
                    buffer[1] = m_dataRate;
                    buffer[2] = (byte)DigitalValues.Count;
                    buffer[3] = (byte)PhasorValues.Count;
                }

                BigEndian.CopyBytes(m_sampleNumber, buffer, 4);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Channel Flags", $"{(int)ChannelFlags}: {ChannelFlags}");
                baseAttributes.Add("Reserved Flags", $"{(int)ReservedFlags}: {ReservedFlags}");
                baseAttributes.Add("Sample Number", SampleNumber.ToString());
                baseAttributes.Add("Using PDC Exchange Format", UsingPdcExchangeFormat.ToString());
                baseAttributes.Add("Using Macrodyne Format", UsingMacrodyneFormat.ToString());
                baseAttributes.Add("Using IEEE Format", UsingIeeeFormat.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This property is overridden to extend parsed length evenly at 4-byte intervals.
        /// </remarks>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            // We align data cells on 32-bit word boundaries (accounts for phantom digital)
            return base.ParseBinaryImage(buffer, startIndex, length).AlignDoubleWord();
        }

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            DataFrame parentFrame = Parent;
            DataFrameParsingState frameState = parentFrame.State;
            IDataCellParsingState state = State;
            RevisionNumber revision = parentFrame.ConfigurationFrame.RevisionNumber;
            int x, index = startIndex;
            byte digitals, phasors;

            // Read data buffer if using phasor data file format
            if (UsePhasorDataFileFormat && frameState.RemainingPdcBlockPmus == 0)
            {
                m_dataBuffer = BigEndian.ToUInt32(buffer, index);
                index += 4;
            }

            // Get data cell flags
            m_channelFlags = (ChannelFlags)buffer[index];
            byte analogs = buffer[index + 1];
            index += 2;

            // Parse PDCstream specific header image
            if (revision >= RevisionNumber.Revision2 && frameState.RemainingPdcBlockPmus == 0)
            {
                // Strip off reserved flags
                m_reservedFlags = (ReservedFlags)analogs & ~ReservedFlags.AnalogWordsMask;

                // Leave analog word count
                analogs &= (byte)ReservedFlags.AnalogWordsMask;
            }
            else
            {
                // Older revisions didn't allow analogs
                m_dataRate = analogs;
                analogs = 0;
            }

            if (frameState.RemainingPdcBlockPmus > 0)
            {
                // PDC Block PMU's contain exactly 2 phasors, 0 analogs and 1 digital
                phasors = 2;
                analogs = 0;
                digitals = 1;
                UsingPdcExchangeFormat = true;

                // Decrement remaining PDC block PMU's
                frameState.RemainingPdcBlockPmus--;
            }
            else
            {
                // Parse number of digitals and phasors for normal PMU cells
                digitals = buffer[index];
                phasors = buffer[index + 1];
                index += 2;

                if (revision >= RevisionNumber.Revision2)
                {
                    // Strip off IEEE flags
                    FormatFlags = (FormatFlags)digitals & ~FormatFlags.DigitalWordsMask;

                    // Leave digital word count
                    digitals &= (byte)FormatFlags.DigitalWordsMask;
                }

                // Check for PDC exchange format
                if (UsingPdcExchangeFormat)
                {
                    // In cases where we are using PDC exchange the phasor count is the number of PMU's in the PDC block
                    int pdcBlockPmus = phasors - 1; // <-- Current PMU counts as one
                    frameState.RemainingPdcBlockPmus = pdcBlockPmus;
                    frameState.CellCount += pdcBlockPmus;

                    // PDC Block PMU's contain exactly 2 phasors, 0 analogs and 1 digital
                    phasors = 2;
                    analogs = 0;
                    digitals = 1;

                    // Get data cell flags for PDC block PMU
                    m_channelFlags = (ChannelFlags)buffer[index];
                    UsingPdcExchangeFormat = true;
                    index += 2;
                }
                else
                {
                    // Parse PMU's sample number
                    m_sampleNumber = BigEndian.ToUInt16(buffer, index);
                    index += 2;
                }
            }

            // Algorithm Case: Determine best course of action when stream counts don't match counts defined in the
            // external INI based configuration file.  Think about what *will* happen when new data appears in the
            // stream that's not in the config file - you could raise an event notifying consumer about the mismatch
            // instead of raising an exception - could even make a boolean property that would allow either case.
            // The important thing to consider is that to parse the cell images you have to have a defined
            // definition (see base class method "DataCellBase.ParseBodyImage").  If you have more items defined
            // in the stream than you do in the config file then you won't get the new value, too few items and you
            // don't have enough definitions to correctly interpret the data (that would be bad) - either way the
            // definitions won't line up with the appropriate data value and you won't know which one is missing or
            // added.  I can't change the protocol so this is enough argument to just raise an error for config
            // file/stream mismatch.  So for now we'll just throw an exception and deal with consequences :)
            // Note that this only applies to BPA PDCstream protocol because of external configuration.

            // Addendum: After running this with several protocol implementations I noticed that if a device wasn't
            // reporting, the phasor count dropped to zero even if there were phasors defined in the configuration
            // file, so the only time an exception is thrown is if there are more phasors defined in the stream
            // than there are defined in the INI file...

            // At least this number of phasors should be already defined in BPA PDCstream configuration file
            if (phasors > ConfigurationCell.PhasorDefinitions.Count)
                throw new InvalidOperationException($"Stream/Config File Mismatch: Phasor value count in stream ({phasors}) does not match defined count in configuration file ({ConfigurationCell.PhasorDefinitions.Count}) for {ConfigurationCell.IDLabel}");

            // If analog values get a clear definition in INI file at some point, we can validate the number in the
            // stream to the number in the config file, in the mean time we dynamically add analog definitions to
            // configuration cell as needed (they are only defined in data frame of BPA PDCstream)
            if (analogs > ConfigurationCell.AnalogDefinitions.Count)
            {
                for (x = ConfigurationCell.AnalogDefinitions.Count; x < analogs; x++)
                    ConfigurationCell.AnalogDefinitions.Add(new AnalogDefinition(ConfigurationCell, $"Analog {(x + 1)}", 1, 0.0D, AnalogType.SinglePointOnWave));
            }

            // If digital values get a clear definition in INI file at some point, we can validate the number in the
            // stream to the number in the config file, in the mean time we dynamically add digital definitions to
            // configuration cell as needed (they are only defined in data frame of BPA PDCstream)
            if (digitals > ConfigurationCell.DigitalDefinitions.Count)
            {
                for (x = ConfigurationCell.DigitalDefinitions.Count; x < digitals; x++)
                    ConfigurationCell.DigitalDefinitions.Add(new DigitalDefinition(ConfigurationCell, $"Digital Word {(x + 1)}"));
            }

            // Unlike most all other protocols the counts defined for phasors, analogs and digitals in the data frame
            // may not exactly match what's defined in the configuration frame as these values are defined in an external
            // INI file for BPA PDCstream.  As a result, we manually assign the counts to the parsing state so that these
            // will be the counts used to parse values from data frame in the base class ParseBodyImage method
            state.PhasorCount = phasors;
            state.AnalogCount = analogs;
            state.DigitalCount = digitals;

            // Status flags and remaining data elements will parsed by base class in the ParseBodyImage method
            return index - startIndex;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data cell
            info.AddValue("channelFlags", m_channelFlags, typeof(ChannelFlags));
            info.AddValue("reservedFlags", m_reservedFlags, typeof(ReservedFlags));
            info.AddValue("sampleNumber", m_sampleNumber);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new BPA PDCstream data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new(parent as IDataFrame, (state as IDataFrameParsingState)?.ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.ParseBinaryImage(buffer, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}
