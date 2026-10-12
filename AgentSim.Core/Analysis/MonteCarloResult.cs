using System.Collections.Generic;
using System.Linq;

namespace AgentSim.Core.Analysis
{
    // The outcome of a single trial within a Monte Carlo run.
    public class TrialResult
    {
        public int TrialIndex { get; set; }
        public int Seed { get; set; }
        public int FinalTickCount { get; set; }

        // Population at the start (after the per-trial variance was applied).
        public int StartAgentCount { get; set; }
        public int StartPrimaryCount { get; set; }
        public int StartSecondaryCount { get; set; }

        // Population at the end. Agents deactivated by a failing script or
        // killed by a rule are not counted.
        public int FinalAgentCount { get; set; }
        public int FinalPrimaryCount { get; set; }
        public int FinalSecondaryCount { get; set; }

        // Average patch value across the whole grid at the end (0 when the
        // world has no patches). Shows how much the agents grazed.
        public double FinalMeanPatchValue { get; set; }

        public int NetChange => FinalAgentCount - StartAgentCount;
        public double SurvivalRate => StartAgentCount > 0 ? (double)FinalAgentCount / StartAgentCount : 0;
    }

    // Descriptive statistics for one measurement across all trials.
    public class SeriesStats
    {
        public int Count { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double StdDev { get; set; }          // population, same as the original summary fields
        public double SampleStdDev { get; set; }
        public double StdError { get; set; }
        public double CiLow { get; set; }           // 95% confidence interval for the mean
        public double CiHigh { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        public static SeriesStats From(IEnumerable<double> values)
        {
            var list = values.ToList();
            var (low, high) = StatisticsHelper.ConfidenceInterval95(list);

            return new SeriesStats
            {
                Count = list.Count,
                Mean = StatisticsHelper.Mean(list),
                Median = StatisticsHelper.Median(list),
                StdDev = StatisticsHelper.StdDev(list),
                SampleStdDev = StatisticsHelper.SampleStdDev(list),
                StdError = StatisticsHelper.StandardError(list),
                CiLow = low,
                CiHigh = high,
                Min = StatisticsHelper.Min(list),
                Max = StatisticsHelper.Max(list)
            };
        }
    }

    // The aggregated outcome of a full Monte Carlo run — every individual
    // trial, plus summary statistics across all of them.
    public class MonteCarloSummary
    {
        public List<TrialResult> Trials { get; set; } = new();

        // Original summary fields (final total population).
        public double MeanFinalAgentCount { get; set; }
        public double StdDevFinalAgentCount { get; set; }
        public int MinFinalAgentCount { get; set; }
        public int MaxFinalAgentCount { get; set; }

        // What the run was configured with.
        public string PrimarySpecies { get; set; } = "";
        public string SecondarySpecies { get; set; } = "";
        public bool HasSecondarySpecies { get; set; }
        public int BaseStartAgentCount { get; set; }
        public double StartCountVariancePercent { get; set; }

        // Fuller statistics, one block per measurement.
        public SeriesStats StartTotal { get; set; } = new();
        public SeriesStats FinalTotal { get; set; } = new();
        public SeriesStats FinalPrimary { get; set; } = new();
        public SeriesStats FinalSecondary { get; set; } = new();
        public SeriesStats SurvivalRate { get; set; } = new();
        public SeriesStats FinalMeanPatchValue { get; set; } = new();

        // Pearson correlation between how many agents a trial started with
        // and how many it ended with (-1 to 1). Near 1 means the outcome
        // mostly follows the starting size; near 0 means it doesn't.
        public double StartFinalCorrelation { get; set; }
    }
}

