// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable

using System;
using Gemstone.Numeric.EE;

namespace Gemstone.PhasorProtocols.SelCWS;

/// <summary>
/// Test program demonstrating the RollingPhaseEstimator with IEEE C37.118-2018 Annex D algorithm.
/// </summary>
internal class Program
{
    private static void Main(string[] args)
    {
        System.Console.WriteLine("RollingPhaseEstimator Test Program (IEEE C37.118-2018 Annex D)");
        System.Console.WriteLine("==============================================================\n");

        // Test 1: Basic operation at nominal frequency (P-class)
        TestNominalFrequency();

        // Test 2: Frequency deviation detection
        TestFrequencyDeviation();

        // Test 3: Magnitude measurement
        TestMagnitudeMeasurement();

        // Test 4: Phase angle relationships
        TestPhaseAngles();

        // Test 5: P-class vs M-class comparison
        TestPClassVsMClass();

        // Test 6: Decimation count verification
        TestDecimationCount();

        System.Console.WriteLine("\nAll tests completed.");
    }

    private static void TestNominalFrequency()
    {
        System.Console.WriteLine("Test 1: Nominal Frequency Operation (60 Hz, P-class)");
        System.Console.WriteLine("-----------------------------------------------------");

        const double sampleRate = 960.0;
        const double outputRate = 60.0;
        const double nominalFreq = 60.0;
        const double amplitude = 100.0;

        RollingPhaseEstimator estimator = new(sampleRate, outputRate, LineFrequency.Hz60, FilterClass.P);

        // Generate samples for 50 cycles worth of data
        int totalSamples = (int)(sampleRate / nominalFreq * 50);
        long samplePeriodNs = (long)(1e9 / sampleRate);
        double omega = 2.0 * Math.PI * nominalFreq;
        long epochNs = 0;

        double lastFrequency = 0;
        double lastRocof = 0;
        double lastVAMagnitude = 0;
        bool gotEstimate = false;
        int estimateCount = 0;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = i / sampleRate;

            // Generate balanced three-phase signals (120° apart)
            double ia = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t);
            double ib = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0);
            double ic = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0);
            double va = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t);
            double vb = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0);

            if (estimator.Step(ia, ib, ic, va, vb, vc, epochNs, (in PhaseEstimate estimate) =>
            {
                lastFrequency = estimate.Frequency;
                lastRocof = estimate.dFdt;
                lastVAMagnitude = estimate.Magnitudes[(int)PhaseChannel.VA];
                gotEstimate = true;
                estimateCount++;
            }))
            {
                // Step returned true, meaning an estimate was produced
            }

            epochNs += samplePeriodNs;
        }

        if (gotEstimate)
        {
            double expectedMag = amplitude / Math.Sqrt(2);
            System.Console.WriteLine($"  Filter order: {estimator.FilterOrder}");
            System.Console.WriteLine($"  Total estimates: {estimateCount}");
            System.Console.WriteLine($"  Measured Frequency: {lastFrequency:F4} Hz (expected: {nominalFreq} Hz)");
            System.Console.WriteLine($"  ROCOF: {lastRocof:F4} Hz/s (expected: ~0)");
            System.Console.WriteLine($"  VA Magnitude: {lastVAMagnitude:F4} (expected: ~{expectedMag:F4} RMS)");
            System.Console.WriteLine($"  Frequency error: {Math.Abs(lastFrequency - nominalFreq):F6} Hz");
        }
        System.Console.WriteLine();
    }

    private static void TestFrequencyDeviation()
    {
        System.Console.WriteLine("Test 2: Frequency Deviation Detection (61 Hz vs 60 Hz nominal)");
        System.Console.WriteLine("--------------------------------------------------------------");

        const double sampleRate = 960.0;
        const double outputRate = 60.0;
        const double actualFreq = 61.0;
        const double amplitude = 100.0;

        RollingPhaseEstimator estimator = new(sampleRate, outputRate, LineFrequency.Hz60, FilterClass.P);

        int totalSamples = (int)(sampleRate / 60.0 * 30); // 30 nominal cycles for convergence
        long samplePeriodNs = (long)(1e9 / sampleRate);
        long epochNs = 0;

        double lastFrequency = 0;
        bool gotEstimate = false;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = i / sampleRate;

            double ia = amplitude * Math.Cos(2.0 * Math.PI * actualFreq * t);
            double ib = amplitude * Math.Cos(2.0 * Math.PI * actualFreq * t - 2.0 * Math.PI / 3.0);
            double ic = amplitude * Math.Cos(2.0 * Math.PI * actualFreq * t + 2.0 * Math.PI / 3.0);
            double va = amplitude * Math.Cos(2.0 * Math.PI * actualFreq * t);
            double vb = amplitude * Math.Cos(2.0 * Math.PI * actualFreq * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Cos(2.0 * Math.PI * actualFreq * t + 2.0 * Math.PI / 3.0);

            estimator.Step(ia, ib, ic, va, vb, vc, epochNs, (in PhaseEstimate estimate) =>
            {
                lastFrequency = estimate.Frequency;
                gotEstimate = true;
            });

            epochNs += samplePeriodNs;
        }

        if (gotEstimate)
        {
            System.Console.WriteLine($"  Measured Frequency: {lastFrequency:F4} Hz (actual: {actualFreq} Hz)");
            System.Console.WriteLine($"  Frequency error: {Math.Abs(lastFrequency - actualFreq):F4} Hz");
        }

        System.Console.WriteLine();
    }

    private static void TestMagnitudeMeasurement()
    {
        System.Console.WriteLine("Test 3: Magnitude Measurement (various amplitudes)");
        System.Console.WriteLine("--------------------------------------------------");

        const double sampleRate = 960.0;
        const double outputRate = 60.0;
        const double nominalFreq = 60.0;

        RollingPhaseEstimator estimator = new(sampleRate, outputRate, LineFrequency.Hz60, FilterClass.P);

        // Different amplitudes for each channel
        double[] peakAmplitudes = [100.0, 150.0, 200.0, 120.0, 180.0, 90.0];
        double[] expectedRms = new double[6];

        for (int i = 0; i < 6; i++)
            expectedRms[i] = peakAmplitudes[i] / Math.Sqrt(2);

        int totalSamples = (int)(sampleRate / nominalFreq * 20); // 20 cycles
        long samplePeriodNs = (long)(1e9 / sampleRate);

        long epochNs = 0;

        double[] lastMagnitudes = new double[6];
        bool gotEstimate = false;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = i / sampleRate;

            double ia = peakAmplitudes[0] * Math.Cos(2.0 * Math.PI * nominalFreq * t);
            double ib = peakAmplitudes[1] * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0);
            double ic = peakAmplitudes[2] * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0);
            double va = peakAmplitudes[3] * Math.Cos(2.0 * Math.PI * nominalFreq * t);
            double vb = peakAmplitudes[4] * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0);
            double vc = peakAmplitudes[5] * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0);

            estimator.Step(ia, ib, ic, va, vb, vc, epochNs, (in PhaseEstimate estimate) =>
            {
                estimate.Magnitudes.CopyTo(lastMagnitudes);
                gotEstimate = true;
            });

            epochNs += samplePeriodNs;
        }

        if (gotEstimate)
        {
            string[] names = ["IA", "IB", "IC", "VA", "VB", "VC"];
            for (int i = 0; i < 6; i++)
            {
                double error = Math.Abs(lastMagnitudes[i] - expectedRms[i]);
                double errorPct = error / expectedRms[i] * 100;
                System.Console.WriteLine($"  {names[i]}: Measured={lastMagnitudes[i]:F4}, Expected={expectedRms[i]:F4}, Error={errorPct:F2}%");
            }
        }
        System.Console.WriteLine();
    }

    private static void TestPhaseAngles()
    {
        System.Console.WriteLine("Test 4: Phase Angle Relationships");
        System.Console.WriteLine("----------------------------------");

        const double sampleRate = 960.0;
        const double outputRate = 60.0;
        const double nominalFreq = 60.0;
        const double amplitude = 100.0;

        RollingPhaseEstimator estimator = new(sampleRate, outputRate, LineFrequency.Hz60, FilterClass.P);

        // Currents lead voltages by 30 degrees (capacitive load)
        const double CurrentLeadRad = 30.0 * Math.PI / 180.0;
        int totalSamples = (int)(sampleRate / nominalFreq * 20);
        long samplePeriodNs = (long)(1e9 / sampleRate);
        long epochNs = 0;

        double[] lastAngles = new double[6];
        bool gotEstimate = false;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = i / sampleRate;

            // Currents with 30° lead
            double ia = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t + CurrentLeadRad);
            double ib = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0 + CurrentLeadRad);
            double ic = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0 + CurrentLeadRad);

            // Voltages (reference)
            double va = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t);
            double vb = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0);

            estimator.Step(ia, ib, ic, va, vb, vc, epochNs, (in PhaseEstimate estimate) =>
            {
                for (int j = 0; j < 6; j++)
                    lastAngles[j] = estimate.Angles[j].ToDegrees();
                gotEstimate = true;
            });

            epochNs += samplePeriodNs;
        }

        if (gotEstimate)
        {
            string[] names = ["IA", "IB", "IC", "VA", "VB", "VC"];

            System.Console.WriteLine("  Angles:");

            for (int i = 0; i < 6; i++)
                System.Console.WriteLine($"    {names[i]}: {lastAngles[i]:F2}°");

            System.Console.WriteLine($"\n  IA-VA angle difference: {WrapAngleDeg(lastAngles[(int)PhaseChannel.IA] - lastAngles[(int)PhaseChannel.VA]):F2}° (expected: 30°)");
        }

        System.Console.WriteLine();
    }

    private static void TestPClassVsMClass()
    {
        System.Console.WriteLine("Test 5: P-class vs M-class Comparison");
        System.Console.WriteLine("--------------------------------------");

        const double sampleRate = 960.0;
        const double outputRate = 60.0;
        const double nominalFreq = 60.0;
        const double amplitude = 100.0;

        RollingPhaseEstimator pEstimator = new(sampleRate, outputRate, LineFrequency.Hz60, FilterClass.P);
        RollingPhaseEstimator mEstimator = new(sampleRate, outputRate, LineFrequency.Hz60, FilterClass.M);

        System.Console.WriteLine($"  P-class filter order: {pEstimator.FilterOrder}");
        System.Console.WriteLine($"  M-class filter order: {mEstimator.FilterOrder}");

        int totalSamples = (int)(sampleRate / nominalFreq * 50);
        long samplePeriodNs = (long)(1e9 / sampleRate);
        long epochNs = 0;

        double pFreq = 0, mFreq = 0;
        double pMag = 0, mMag = 0;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = i / sampleRate;

            double va = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t);
            double vb = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0);
            double ia = va, ib = vb, ic = vc;

            pEstimator.Step(ia, ib, ic, va, vb, vc, epochNs, (in PhaseEstimate estimate) =>
            {
                pFreq = estimate.Frequency;
                pMag = estimate.Magnitudes[(int)PhaseChannel.VA];
            });

            mEstimator.Step(ia, ib, ic, va, vb, vc, epochNs, (in PhaseEstimate estimate) =>
            {
                mFreq = estimate.Frequency;
                mMag = estimate.Magnitudes[(int)PhaseChannel.VA];
            });

            epochNs += samplePeriodNs;
        }

        double expectedRms = amplitude / Math.Sqrt(2);
        System.Console.WriteLine($"  P-class: Freq={pFreq:F4} Hz, VA Mag={pMag:F4} (expected RMS: {expectedRms:F4})");
        System.Console.WriteLine($"  M-class: Freq={mFreq:F4} Hz, VA Mag={mMag:F4} (expected RMS: {expectedRms:F4})");

        System.Console.WriteLine();
    }

    private static void TestDecimationCount()
    {
        System.Console.WriteLine("Test 6: Decimation Count Verification");
        System.Console.WriteLine("--------------------------------------");

        const double sampleRate = 960.0;
        const double outputRate = 60.0;
        const double nominalFreq = 60.0;
        const double amplitude = 100.0;
        const double testDurationSeconds = 1.0; // exactly 1 second

        RollingPhaseEstimator estimator = new(sampleRate, outputRate, LineFrequency.Hz60, FilterClass.P);

        int totalSamples = (int)(sampleRate * testDurationSeconds);
        long samplePeriodNs = (long)(1e9 / sampleRate);
        long epochNs = 0;
        int estimateCount = 0;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = i / sampleRate;

            double va = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t);
            double vb = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Cos(2.0 * Math.PI * nominalFreq * t + 2.0 * Math.PI / 3.0);

            estimator.Step(va, vb, vc, va, vb, vc, epochNs, (in PhaseEstimate estimate) =>
            {
                estimateCount++;
            });

            epochNs += samplePeriodNs;
        }

        // Expected: total outputs = (totalSamples - WindowSamples) / decimationStep + some
        // At 960 Hz with filter order ~28, about 960-29=931 valid samples, 931/16=58 ish
        // Exact count depends on filter order
        int samplesPerReport = (int)(sampleRate / outputRate);
        System.Console.WriteLine($"  Total samples: {totalSamples}");
        System.Console.WriteLine($"  Samples per report: {samplesPerReport}");
        System.Console.WriteLine($"  Filter startup samples: {estimator.WindowSamples}");
        System.Console.WriteLine($"  Total estimates produced: {estimateCount}");
        System.Console.WriteLine($"  Expected approx: {(totalSamples - estimator.WindowSamples) / samplesPerReport}");

        System.Console.WriteLine();
    }

    private static double WrapAngleDeg(double degrees)
    {
        double result = (degrees + 180.0) % 360.0;

        if (result < 0)
            result += 360.0;

        return result - 180.0;
    }
}
