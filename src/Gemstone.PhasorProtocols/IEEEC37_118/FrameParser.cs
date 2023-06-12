﻿//******************************************************************************************************
//  FrameParser.cs - Gbtc
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
using System.Text;
using Gemstone.IO.Parsing;
using Gemstone.StringExtensions;

namespace Gemstone.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents a frame parser for an IEEE C37.118 binary data stream and returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<FrameType>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="ConfigurationFrame1"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame1"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame1>>? ReceivedConfigurationFrame1;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="ConfigurationFrame2"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame2"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame2>>? ReceivedConfigurationFrame2;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="ConfigurationFrame2"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame3"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame3>>? ReceivedConfigurationFrame3;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>>? ReceivedDataFrame;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="HeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="HeaderFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<HeaderFrame>>? ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="CommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="CommandFrame"/> that was received.
        /// <para>
        /// Command frames are normally sent, not received, but there is nothing that prevents this.
        /// </para>
        /// </remarks>
        public new event EventHandler<EventArgs<CommandFrame>>? ReceivedCommandFrame;

        // Fields
        private ConfigurationFrame2 m_configurationFrame2;
        private ConfigurationFrame3 m_configurationFrame3;
        private FrameImageCollector m_configFrame3Images;
        private bool m_configurationChangeHandled;
        private long m_unexpectedCommandFrames;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/>.
        /// </summary>
        /// <param name="checkSumValidationFrameTypes">Frame types that should perform check-sum validation; default to <see cref="CheckSumValidationFrameTypes.AllFrames"/></param>
        /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
        /// <param name="draftRevision">The <see cref="IEEEC37_118.DraftRevision"/> of this <see cref="FrameParser"/>.</param>
        public FrameParser(CheckSumValidationFrameTypes checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames, bool trustHeaderLength = true, DraftRevision draftRevision = DraftRevision.Std2005)
            : base(checkSumValidationFrameTypes, trustHeaderLength)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new[] { PhasorProtocols.Common.SyncByte };

            DraftRevision = draftRevision;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
        /// </summary>
        /// <remarks>
        /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame. Consumer can manually assign a
        /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
        /// </remarks>
        public override IConfigurationFrame ConfigurationFrame
        {
            get => (IConfigurationFrame)m_configurationFrame3 ?? m_configurationFrame2;
            set
            {
                IConfigurationFrame configuration = CastToDerivedConfigurationFrame(value, DraftRevision);
                m_configurationFrame2 = configuration as ConfigurationFrame2;
                m_configurationFrame3 = configuration as ConfigurationFrame3;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEEEC37_118.DraftRevision"/> of this <see cref="FrameParser"/>.
        /// </summary>
        public DraftRevision DraftRevision { get; set; }

        /// <summary>
        /// Gets the IEEE C37.118 resolution of fractional timestamps of the current <see cref="ConfigurationFrame"/>, if one has been parsed.
        /// </summary>
        public uint Timebase => (ConfigurationFrame as ConfigurationFrame1)?.Timebase ?? 0;

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes => true;

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new();

                status.AppendLine($"     IEEE C37.118 revision: {DraftRevision}");
                status.AppendLine($"         Current time base: {Timebase}");
                status.AppendLine($" Unexpected command frames: {m_unexpectedCommandFrames:N0}");
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the data parser.
        /// </summary>
        public override void Start()
        {
            m_configurationChangeHandled = false;
            m_unexpectedCommandFrames = 0;

            // We narrow down parsing types to just those needed...
            switch (DraftRevision)
            {
                case DraftRevision.Draft6:
                    base.Start(new[] { typeof(DataFrame), typeof(ConfigurationFrame1Draft6), typeof(ConfigurationFrame2Draft6), typeof(HeaderFrame) });
                    break;
                case DraftRevision.Std2005:
                    base.Start(new[] { typeof(DataFrame), typeof(ConfigurationFrame1), typeof(ConfigurationFrame2), typeof(HeaderFrame) });
                    break;
                case DraftRevision.Std2011:
                    base.Start(new[] { typeof(DataFrame), typeof(ConfigurationFrame1), typeof(ConfigurationFrame2), typeof(ConfigurationFrame3), typeof(HeaderFrame) });
                    break;
            }
        }

        /// <summary>
        /// Parses a common header instance that implements <see cref="ICommonHeader{TTypeIdentifier}"/> for the output type represented
        /// in the binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <returns>The <see cref="ICommonHeader{TTypeIdentifier}"/> which includes a type ID for the <see cref="Type"/> to be parsed.</returns>
        /// <remarks>
        /// <para>
        /// Derived classes need to provide a common header instance (i.e., class that implements <see cref="ICommonHeader{TTypeIdentifier}"/>)
        /// for the output types; this will primarily include an ID of the <see cref="Type"/> that the data image represents.  This parsing is
        /// only for common header information, actual parsing will be handled by output type via its <see cref="ISupportBinaryImage.ParseBinaryImage"/>
        /// method. This header image should also be used to add needed complex state information about the output type being parsed if needed.
        /// </para>
        /// <para>
        /// If there is not enough buffer available to parse common header (as determined by <paramref name="length"/>), return null.  Also, if
        /// the protocol allows frame length to be determined at the time common header is being parsed and there is not enough buffer to parse
        /// the entire frame, it will be optimal to prevent further parsing by returning null.
        /// </para>
        /// </remarks>
        protected override ICommonHeader<FrameType> ParseCommonHeader(byte[] buffer, int offset, int length)
        {
            // See if there is enough data in the buffer to parse the common frame header.
            if (length < CommonFrameHeader.FixedLength)
                return null;

            // Parse common frame header
            CommonFrameHeader parsedFrameHeader = new(ConfigurationFrame as ConfigurationFrame1, buffer, offset);

            // Look for probable misaligned bad frame header parse
            if (parsedFrameHeader.FrameType == FundamentalFrameType.Undetermined || parsedFrameHeader.Version > (byte)DraftRevision.LatestVersion)
                throw new InvalidOperationException("Probable frame misalignment detected, forcing scan ahead to next sync byte");

            // As an optimization, we also make sure entire frame buffer image is available to be parsed - by doing this
            // we eliminate the need to validate length on all subsequent data elements that comprise the frame
            if (length < parsedFrameHeader.FrameLength)
                return null;

            // Expose the frame buffer image in case client needs this data for any reason
            OnReceivedFrameBufferImage(parsedFrameHeader.FrameType, buffer, offset, parsedFrameHeader.FrameLength);

            // Handle special parsing states
            switch (parsedFrameHeader.TypeID)
            {
                case FrameType.DataFrame:
                    // Assign data frame parsing state
                    parsedFrameHeader.State = new DataFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationFrame, DataCell.CreateNewCell, TrustHeaderLength, ValidateDataFrameCheckSum);
                    break;
                case FrameType.ConfigurationFrame1:
                case FrameType.ConfigurationFrame2:
                    // Assign configuration frame parsing state
                    parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell.CreateNewCell, TrustHeaderLength, ValidateConfigurationFrameCheckSum);
                    break;
                case FrameType.ConfigurationFrame3:
                    // Safe to parse next two bytes for continuation index here since we know all bytes of frame are available (per optimization above)
                    parsedFrameHeader.ContinuationIndex = BigEndian.ToUInt16(buffer, offset + CommonFrameHeader.FixedLength);
                    parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell3.CreateNewCell, TrustHeaderLength, ValidateConfigurationFrameCheckSum);

                    // Any non-zero continuation index represents a frame in a series of frame fragments
                    if (parsedFrameHeader.ContinuationIndex > 0)
                    {
                        if (parsedFrameHeader.IsFirstFrame)
                            m_configFrame3Images = new FrameImageCollector(ValidateConfigurationFrameCheckSum);

                        try
                        {
                            if (m_configFrame3Images is null)
                                throw new NullReferenceException("No frame image collector defined to cumulate fragmented frames of configuration frame 3.");

                            m_configFrame3Images.AppendFrameImage(buffer, offset, parsedFrameHeader.FrameLength);
                        }
                        catch
                        {
                            m_configFrame3Images = null;
                            throw;
                        }

                        // Store a reference to frame image collection in common header state so configuration frame
                        // can be parsed from entire combined binary image collection when last frame is received
                        parsedFrameHeader.FrameImages = m_configFrame3Images;

                        // Clear local reference to frame image collection if this is the last frame
                        if (parsedFrameHeader.IsLastFrame)
                            m_configFrame3Images = null;
                    }

                    break;
                case FrameType.HeaderFrame:
                    // Assign header frame parsing state
                    parsedFrameHeader.State = new HeaderFrameParsingState(parsedFrameHeader.FrameLength, parsedFrameHeader.DataLength, TrustHeaderLength, ValidateHeaderFrameCheckSum);
                    break;
            }

            return parsedFrameHeader;

        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IConfigurationFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.</param>
        protected override void OnReceivedConfigurationFrame(IConfigurationFrame frame)
        {
            // Cache new configuration frame for parsing subsequent data frames...
            switch (frame)
            {
                case ConfigurationFrame3 configurationFrame3:
                    CommonFrameHeader header = configurationFrame3.CommonHeader;

                    // Configuration frame 3 can span multiple frame images, so do not raise this event until all frames have been assembled...
                    if (header.ContinuationIndex > 0 && !header.IsLastFrame)
                        return;

                    m_configurationFrame3 = configurationFrame3;
                    break;
                case ConfigurationFrame2 configurationFrame2:
                    m_configurationFrame2 = configurationFrame2;
                    break;
            }

            base.OnReceivedConfigurationFrame(frame);
        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedDataFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IDataFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedDataFrame"/> event.</param>
        protected override void OnReceivedDataFrame(IDataFrame frame)
        {
            // We override this method so we can detect and respond to a configuration change notification
            base.OnReceivedDataFrame(frame);

            bool configurationChangeDetected = false;

            if (frame.Cells is DataCellCollection dataCells)
            {
                // Check for a configuration change notification from any data cell
                for (int x = 0; x < dataCells.Count; x++)
                {
                    // Configuration change is only relevant if raw status flags <> FF (status undefined)
                    if (dataCells[x].ConfigurationChangeDetected && !dataCells[x].DeviceError && ((DataCellBase)dataCells[x]).StatusFlags != ushort.MaxValue)
                    {
                        configurationChangeDetected = true;

                        // Configuration change detection flag should terminate after one minute, but
                        // we only want to send a single notification
                        if (!m_configurationChangeHandled)
                        {
                            m_configurationChangeHandled = true;
                            base.OnConfigurationChanged();
                        }

                        break;
                    }
                }
            }

            if (!configurationChangeDetected)
                m_configurationChangeHandled = false;
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/>, <see cref="ConfigurationFrame"/>, <see cref="CommandFrame"/> or <see cref="HeaderFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise IEEE C37.118 specific channel frame events, if any have been subscribed
            if (frame is null || ReceivedDataFrame is null && ReceivedConfigurationFrame3 is null && ReceivedConfigurationFrame2 is null && ReceivedConfigurationFrame1 is null && ReceivedHeaderFrame is null && ReceivedCommandFrame is null)
                return;

            switch (frame)
            {
                case DataFrame dataFrame:
                {
                    ReceivedDataFrame?.Invoke(this, new EventArgs<DataFrame>(dataFrame));
                    break;
                }
                // Configuration frame type 3 has priority over type 2, so we check it first
                case ConfigurationFrame3 configFrame3:
                {
                    ReceivedConfigurationFrame3?.Invoke(this, new EventArgs<ConfigurationFrame3>(configFrame3));
                    break;
                }
                // Configuration frame type 2 is more specific than type 1 (and more common), so we check it next
                case ConfigurationFrame2 configFrame2:
                {
                    ReceivedConfigurationFrame2?.Invoke(this, new EventArgs<ConfigurationFrame2>(configFrame2));
                    break;
                }
                case ConfigurationFrame1 configFrame1:
                {
                    ReceivedConfigurationFrame1?.Invoke(this, new EventArgs<ConfigurationFrame1>(configFrame1));
                    break;
                }
                case HeaderFrame headerFrame:
                {
                    ReceivedHeaderFrame?.Invoke(this, new EventArgs<HeaderFrame>(headerFrame));
                    break;
                }
                case CommandFrame commandFrame:
                {
                    ReceivedCommandFrame?.Invoke(this, new EventArgs<CommandFrame>(commandFrame));
                    break;
                }
            }
        }

        /// <summary>
        /// Handles unknown frame types.
        /// </summary>
        /// <param name="frameType">Unknown frame ID.</param>
        protected override void OnUnknownFrameTypeEncountered(FrameType frameType)
        {
            // It is unusual, but not all that uncommon, for a device to send a command frame to its host. Normally the host sends to commands
            // to the device. However, since some devices seem to do this frequently we suppress reporting that the frame is "undefined".
            // Technically the frame is defined, but it is not part of the valid set of frames intended for reception.
            if (frameType is FrameType.CommandFrame)
                m_unexpectedCommandFrames++;
            else
                base.OnUnknownFrameTypeEncountered(frameType);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Attempts to cast given frame into an IEEE C37.118 configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static IConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame, DraftRevision draftRevision)
        {
            switch (sourceFrame)
            {
                // See if frame is already an IEEE C37.118 configuration frame 2 or 3
                case ConfigurationFrame3 configurationFrame3:
                    return configurationFrame3;
                case ConfigurationFrame2 configurationFrame2:
                    return configurationFrame2;
            }

            if (draftRevision == DraftRevision.Std2011)
            {
                // Handle configuration 3 frame as a special case:
                ConfigurationFrame3 derivedFrame3 = new(100000, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);

                foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
                {
                    // Create new derived configuration cell
                    ConfigurationCell3 derivedCell = new(derivedFrame3, sourceCell.IDCode, sourceCell.NominalFrequency);

                    string stationName = sourceCell.StationName;
                    string idLabel = sourceCell.IDLabel;

                    if (!string.IsNullOrWhiteSpace(stationName))
                        derivedCell.StationName = stationName.TruncateLeft(derivedCell.MaximumStationNameLength);

                    if (!string.IsNullOrWhiteSpace(idLabel))
                        derivedCell.IDLabel = idLabel.TruncateLeft(derivedCell.IDLabelLength);

                    derivedCell.PhasorCoordinateFormat = sourceCell.PhasorCoordinateFormat;
                    derivedCell.PhasorAngleFormat = sourceCell.PhasorAngleFormat;
                    derivedCell.PhasorDataFormat = sourceCell.PhasorDataFormat;
                    derivedCell.FrequencyDataFormat = sourceCell.FrequencyDataFormat;
                    derivedCell.AnalogDataFormat = sourceCell.AnalogDataFormat;

                    // Create equivalent derived phasor definitions
                    foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                        derivedCell.PhasorDefinitions.Add(new PhasorDefinition3(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.PhasorType, null, sourcePhasor is Anonymous.PhasorDefinition anonymousPhasor ? anonymousPhasor.Phase : '+'));

                    // Create equivalent derived frequency definition
                    IFrequencyDefinition sourceFrequency = sourceCell.FrequencyDefinition;

                    if (sourceFrequency is not null)
                        derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                    // Create equivalent derived analog definitions (assuming analog type = SinglePointOnWave)
                    foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                        derivedCell.AnalogDefinitions.Add(new AnalogDefinition3(derivedCell, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.Offset, sourceAnalog.AnalogType));

                    // Create equivalent derived digital definitions
                    foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                        derivedCell.DigitalDefinitions.Add(new DigitalDefinition3(derivedCell, sourceDigital.Label, 0, 0));

                    // Add cell to frame
                    derivedFrame3.Cells.Add(derivedCell);
                }

                return derivedFrame3;
            }

            ConfigurationFrame1 derivedFrame = draftRevision switch
            {
                // Assuming configuration frame 2 and timebase = 100000
                DraftRevision.Draft6 => new ConfigurationFrame2Draft6(100000, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate),
                DraftRevision.Std2005 => new ConfigurationFrame2(100000, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate),
                _ => throw new ArgumentOutOfRangeException(nameof(draftRevision), draftRevision, null),
            };

            foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
            {
                // Create new derived configuration cell
                ConfigurationCell derivedCell = new(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                string stationName = sourceCell.StationName;
                string idLabel = sourceCell.IDLabel;

                if (!string.IsNullOrWhiteSpace(stationName))
                    derivedCell.StationName = stationName.TruncateLeft(derivedCell.MaximumStationNameLength);

                if (!string.IsNullOrWhiteSpace(idLabel))
                    derivedCell.IDLabel = idLabel.TruncateLeft(derivedCell.IDLabelLength);

                derivedCell.PhasorCoordinateFormat = sourceCell.PhasorCoordinateFormat;
                derivedCell.PhasorAngleFormat = sourceCell.PhasorAngleFormat;
                derivedCell.PhasorDataFormat = sourceCell.PhasorDataFormat;
                derivedCell.FrequencyDataFormat = sourceCell.FrequencyDataFormat;
                derivedCell.AnalogDataFormat = sourceCell.AnalogDataFormat;

                // Create equivalent derived phasor definitions
                foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.PhasorType, null));

                // Create equivalent derived frequency definition
                IFrequencyDefinition sourceFrequency = sourceCell.FrequencyDefinition;

                if (sourceFrequency is not null)
                    derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                // Create equivalent derived analog definitions (assuming analog type = SinglePointOnWave)
                foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                    derivedCell.AnalogDefinitions.Add(new AnalogDefinition(derivedCell, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.Offset, sourceAnalog.AnalogType));

                // Create equivalent derived digital definitions
                foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                    derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label, 0, 0));

                // Add cell to frame
                derivedFrame.Cells.Add(derivedCell);
            }

            return derivedFrame;
        }

        #endregion
    }
}
