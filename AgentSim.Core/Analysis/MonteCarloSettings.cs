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

        // How much the starting population varies from trial to trial, as a
        // percentage of each group's configured size. With a base of 30 and
        // 10, each trial starts with somewhere between 27 and 33 agents
        // (e.g. 28, 31, 30...). The primary and secondary groups are varied
        // independently. 0 = every trial starts with exactly the configured
        // count (the original behaviour). The variation is drawn from the
        // trial's seed, so a fixed BaseSeed still reproduces the same run.
        public double StartCountVariancePercent { get; set; } = 0;
    }
}

