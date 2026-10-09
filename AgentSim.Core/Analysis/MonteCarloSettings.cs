using System;
using System.Collections.Generic;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;

namespace AgentSim.Core.Analysis
{
    // Parameters for a Monte Carlo batch run: how many independent trials
    // to run and how long each one runs for.
    public class MonteCarloSettings
    {
        public int TrialCount { get; set; } = 30;
        public int TicksPerTrial { get; set; } = 200;
        public SimulationSettings BaseSettings { get; set; } = new();

        // Fixed BaseSeed = same trial seeds every run (reproducible for
        // testing/marking). Null = a fresh random seed per trial each run.
        public int? BaseSeed { get; set; } = null;

        // Scripts to apply to every trial, in order, right after Setup().
        // Pass engine.ScriptAssignments here to make the trials behave like
        // the simulation on screen. Empty = default random walk only.
        public IReadOnlyList<ScriptAssignment> ScriptAssignments { get; set; } = Array.Empty<ScriptAssignment>();
    }
}
