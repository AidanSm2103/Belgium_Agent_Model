using AgentSim.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Analysis
{
    // Parameters for a Monte Carlo batch run
    public class MonteCarloSettings
    {
        public int TrialCount { get; set; } = 30;
        public int TicksPerTrial { get; set; } = 200;
        public SimulationSettings BaseSettings { get; set; } = new();

        // Fixed BaseSeed = same trial seeds every run (reproducible for
        // testing/marking). Null = a fresh random seed per trial each run.
        public int? BaseSeed { get; set; } = null;
    }
}
