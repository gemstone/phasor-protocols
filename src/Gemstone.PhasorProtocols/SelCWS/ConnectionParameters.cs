//******************************************************************************************************
//  ConnectionParameters.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2025 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Gemstone.Numeric.EE;
using static Gemstone.PhasorProtocols.SelCWS.RollingPhaseEstimator;

namespace Gemstone.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the extra connection parameters required for a connection to a SEL CWS device.
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

    // Constants

    /// <summary>
    /// Default value for <see cref="CalculatePhaseEstimates"/>.
    /// </summary>
    public const bool DefaultCalculatePhaseEstimates = true;

    /// <summary>
    /// Default value for <see cref="RepeatLastCalculatedValueWhenDownSampling"/>.
    /// </summary>
    public const bool DefaultRepeatLastCalculatedValueWhenDownSampling = true;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="ConnectionParameters"/>.
    /// </summary>
    public ConnectionParameters()
    {
        CalculatePhaseEstimates = DefaultCalculatePhaseEstimates;
        NominalFrequency = Common.DefaultNominalFrequency;
        CalculationFrameRate = Common.DefaultFramePerSecond;
        RepeatLastCalculatedValueWhenDownSampling = DefaultRepeatLastCalculatedValueWhenDownSampling;
        FilterClass = DefaultFilterClass;
    }

    /// <summary>
    /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected ConnectionParameters(SerializationInfo info, StreamingContext context)
    {
        // Deserialize connection parameters
        CalculatePhaseEstimates = info.GetOrDefault("calculatePhaseEstimates", DefaultCalculatePhaseEstimates);
        NominalFrequency = info.GetOrDefault("nominalFrequency", Common.DefaultNominalFrequency);
        CalculationFrameRate = info.GetOrDefault("calculationFrameRate", Common.DefaultFramePerSecond);
        RepeatLastCalculatedValueWhenDownSampling = info.GetOrDefault("repeatLastCalculatedValueWhenDownSampling", DefaultRepeatLastCalculatedValueWhenDownSampling);
        FilterClass = info.GetOrDefault("filterClass", DefaultFilterClass);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets flag that determines if current and voltage phase estimates, frequency and dF/dt should be
    /// calculated for PoW data.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("Determines if current and voltage phase estimates, frequency and dF/dt should be calculated for PoW data.")]
    [DefaultValue(DefaultCalculatePhaseEstimates)]
    public bool CalculatePhaseEstimates { get; set; }

    /// <summary>
    /// Gets or sets the nominal <see cref="LineFrequency"/> of this SEL CWS device.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("Configured nominal frequency for SEL CWS device.")]
    [DefaultValue(typeof(LineFrequency), "Hz60")]
    public LineFrequency NominalFrequency { get; set; }

    /// <summary>
    /// Gets or sets the configured frame rate for phase estimate calculations.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("Configured frame rate for phase estimate calculations.")]
    [DefaultValue(Common.DefaultFramePerSecond)]
    public ushort CalculationFrameRate { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if last value should be repeated when down-sampling, i.e.,
    /// when <see cref="CalculationFrameRate"/> is less than SEL CWS frame rate (commonly 3000Hz);
    /// otherwise <see cref="Double.NaN"/> will be used.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description(
        "Gets or sets flag that determines if last value should be repeated when down-sampling, i.e.," +
        "when 'CalculationFrameRate' is less than SEL CWS frame rate (commonly 3000Hz);" +
        "otherwise 'NaN' will be used."
    )]
    [DefaultValue(DefaultRepeatLastCalculatedValueWhenDownSampling)]
    public bool RepeatLastCalculatedValueWhenDownSampling { get; set; }

    /// <summary>
    /// Gets or sets the IEEE C37.118 filter class: P (Protection, fast response) or M (Measurement, better out-of-band rejection).
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("IEEE C37.118 filter class: P (Protection, fast response) or M (Measurement, better out-of-band rejection).")]
    [DefaultValue(DefaultFilterClass)]
    public FilterClass FilterClass { get; set; }

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
        info.AddValue("calculatePhaseEstimates", CalculatePhaseEstimates);
        info.AddValue("nominalFrequency", NominalFrequency, typeof(LineFrequency));
        info.AddValue("calculationFrameRate", CalculationFrameRate);
        info.AddValue("repeatLastCalculatedValueWhenDownSampling", RepeatLastCalculatedValueWhenDownSampling);
        info.AddValue("filterClass", FilterClass, typeof(FilterClass));
    }

    #endregion
}
