using System.Collections.Generic;
using System.Linq;
using AgentSim.Core.Agents;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;

namespace AgentSim.Core.Analysis
{
    // Runs a simulation many times over with different random seeds and
    // aggregates the outcomes — proves the engine's behavior statistically
    // rather than trusting any single run. Runs entirely headlessly: no
    // WPF references anywhere in this file or its dependencies, which is
    // the proof the engine works as a standalone, reusable library.
    public static class MonteCarloRunner
    {
        public static MonteCarloSummary Run(MonteCarloSettings settings, IAgentBehavior? behaviorOverride = null)
        {
            var random = new System.Random(settings.BaseSeed ?? System.Environment.TickCount);
            var trials = new List<TrialResult>();

            // Compile each assigned script once up front. The compiled
            // behaviors are reused by every trial (compiling per trial
            // would be far slower and gains nothing).
            var assignments = new List<(string? Species, IAgentBehavior Behavior)>();
            foreach (var assignment in settings.ScriptAssignments)
            {
                var compiled = BehaviorCompiler.Compile(assignment.Source);
                if (!compiled.Success)
                {
                    throw new System.InvalidOperationException(
                        "A script could not be compiled for the Monte Carlo run: " + compiled.ErrorMessage);
                }
                assignments.Add((assignment.Species, compiled.Behavior!));
            }

            for (int i = 0; i < settings.TrialCount; i++)
            {
                int seed = settings.BaseSeed.HasValue ? settings.BaseSeed.Value + i : random.Next();

                var trialSettings = CloneSettings(settings.BaseSettings, seed);
                var engine = new SimulationEngine(trialSettings);
                engine.Setup();

                if (behaviorOverride != null)
                {
                    engine.ApplyBehaviorToAllAgents(behaviorOverride);
                }

                // Same scripts, same order, same targets as the live simulation.
                foreach (var (species, behavior) in assignments)
                {
                    if (species == null)
                    {
                        engine.ApplyBehaviorToAllAgents(behavior);
                    }
                    else
                    {
                        engine.ApplyBehaviorToAgentsMatching(a => a.Species == species, behavior);
                    }
                }

                for (int t = 0; t < settings.TicksPerTrial; t++)
                {
                    engine.Tick();
                }

                trials.Add(new TrialResult
                {
                    TrialIndex = i,
                    Seed = seed,
                    FinalAgentCount = engine.Worlds.Agents.Count(a => a.IsActive),   // agents deactivated by a failing script don't count as alive
                    FinalTickCount = engine.TickCount
                });
            }

            var finalCounts = trials.Select(t => (double)t.FinalAgentCount).ToList();

            return new MonteCarloSummary
            {
                Trials = trials,
                MeanFinalAgentCount = StatisticsHelper.Mean(finalCounts),
                StdDevFinalAgentCount = StatisticsHelper.StdDev(finalCounts),
                MinFinalAgentCount = (int)StatisticsHelper.Min(finalCounts),
                MaxFinalAgentCount = (int)StatisticsHelper.Max(finalCounts)
            };
        }

        private static SimulationSettings CloneSettings(SimulationSettings baseSettings, int seed) => new()
        {
            AgentCount = baseSettings.AgentCount,
            WorldWidth = baseSettings.WorldWidth,
            WorldHeight = baseSettings.WorldHeight,
            StepSize = baseSettings.StepSize,
            MaxTurnDegrees = baseSettings.MaxTurnDegrees,
            SecondaryGroupCount = baseSettings.SecondaryGroupCount,
            PrimaryGroupSpecies = baseSettings.PrimaryGroupSpecies,
            SecondaryGroupSpecies = baseSettings.SecondaryGroupSpecies,
            PatchColumns = baseSettings.PatchColumns,
            PatchRows = baseSettings.PatchRows,
            PatchInitialValue = baseSettings.PatchInitialValue,
            PatchRegrowthTicks = baseSettings.PatchRegrowthTicks,
            Seed = seed
        };
    }
}
