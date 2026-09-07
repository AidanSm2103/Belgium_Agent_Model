using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Analysis
{
    // The outcome of a single trial within a Monte Carlo run.
    public class TrialResult
    {
        public int TrialIndex { get; set; }
        public int Seed { get; set; }
        public int FinalAgentCount { get; set; }
        public int FinalTickCount { get; set; }
    }

    // The aggregated outcome of a full Monte Carlo run — every individual
    // trial, plus summary statistics across all of them.
    public class MonteCarloSummary
    {
        public List<TrialResult> Trials { get; set; } = new();

        public double MeanFinalAgentCount { get; set; }
        public double StdDevFinalAgentCount { get; set; }
        public int MinFinalAgentCount { get; set; }
        public int MaxFinalAgentCount { get; set; }
    }
}
