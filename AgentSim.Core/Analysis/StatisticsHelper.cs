using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentSim.Core.Analysis
{
    // Small, independently-testable statistics helpers used to summarize
    // Monte Carlo trial results. No dependency on the rest of the engine.
    public static class StatisticsHelper
    {
        public static double Mean(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;
            return list.Average();
        }

        // Population standard deviation (divides by n). Kept as-is because
        // existing tests and the summary's StdDev fields rely on it.
        public static double StdDev(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;

            double mean = Mean(list);
            double sumOfSquares = list.Sum(v => (v - mean) * (v - mean));
            return Math.Sqrt(sumOfSquares / list.Count);
        }

        // Sample standard deviation (divides by n - 1). This is the right one
        // for estimating the spread of the underlying process from a set of
        // trials, and is what the standard error and confidence interval use.
        public static double SampleStdDev(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count < 2) return 0;

            double mean = Mean(list);
            double sumOfSquares = list.Sum(v => (v - mean) * (v - mean));
            return Math.Sqrt(sumOfSquares / (list.Count - 1));
        }

        public static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0) return 0;

            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 1
                ? sorted[mid]
                : (sorted[mid - 1] + sorted[mid]) / 2.0;
        }

        // Standard error of the mean: how much the mean itself would wobble
        // if the whole Monte Carlo run were repeated.
        public static double StandardError(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count < 2) return 0;
            return SampleStdDev(list) / Math.Sqrt(list.Count);
        }

        // 95% confidence interval for the mean, using Student's t for small
        // trial counts (a plain 1.96 would make 10 trials look too certain).
        public static (double Low, double High) ConfidenceInterval95(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return (0, 0);

            double mean = Mean(list);
            if (list.Count < 2) return (mean, mean);

            double margin = TCritical95(list.Count - 1) * StandardError(list);
            return (mean - margin, mean + margin);
        }

        // Pearson correlation between two equally long series (-1 to 1).
        // Returns 0 when it is undefined (fewer than 2 points, or one of the
        // series never changes).
        public static double Correlation(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
        {
            int n = Math.Min(xs.Count, ys.Count);
            if (n < 2) return 0;

            double meanX = xs.Take(n).Average();
            double meanY = ys.Take(n).Average();

            double covariance = 0, varX = 0, varY = 0;
            for (int i = 0; i < n; i++)
            {
                double dx = xs[i] - meanX;
                double dy = ys[i] - meanY;
                covariance += dx * dy;
                varX += dx * dx;
                varY += dy * dy;
            }

            if (varX <= 0 || varY <= 0) return 0;
            return covariance / Math.Sqrt(varX * varY);
        }

        public static double Min(IEnumerable<double> values) => values.DefaultIfEmpty(0).Min();
        public static double Max(IEnumerable<double> values) => values.DefaultIfEmpty(0).Max();

        // Two-sided 95% critical values of Student's t distribution.
        private static readonly double[] TTable =
        {
            12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228,
            2.201, 2.179, 2.160, 2.145, 2.131, 2.120, 2.110, 2.101, 2.093, 2.086,
            2.080, 2.074, 2.069, 2.064, 2.060, 2.056, 2.052, 2.048, 2.045, 2.042
        };

        public static double TCritical95(int degreesOfFreedom)
        {
            if (degreesOfFreedom < 1) return 0;
            if (degreesOfFreedom <= TTable.Length) return TTable[degreesOfFreedom - 1];
            if (degreesOfFreedom <= 60) return 2.000;
            return 1.960;
        }
    }
}
