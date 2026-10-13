using System;
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
            var random = new Random(settings.BaseSeed ?? Environment.TickCount);
            var trials = new List<TrialResult>();
            var baseSettings = settings.BaseSettings;

            // Compile each assigned script once up front. The compiled
            // behaviors are reused by every trial (compiling per trial
            // would be far slower and gains nothing).
            var assignments = new List<(string? Species, IAgentBehavior Behavior)>();
            foreach (var assignment in settings.ScriptAssignments)
            {
                var compiled = BehaviorCompiler.Compile(assignment.Source);
                if (!compiled.Success)
                {
                    throw new InvalidOperationException(
                        "A script could not be compiled for the Monte Carlo run: " + compiled.ErrorMessage);
                }
                assignments.Add((assignment.Species, compiled.Behavior!));
            }

            // The configured split between the two groups. SecondaryGroupCount
            // is part of AgentCount, so primary = the rest.
            int baseSecondary = Math.Clamp(baseSettings.SecondaryGroupCount, 0, Math.Max(0, baseSettings.AgentCount));
            int basePrimary = Math.Max(0, baseSettings.AgentCount - baseSecondary);
            bool hasSecondary = baseSecondary > 0;

            for (int i = 0; i < settings.TrialCount; i++)
            {
                int seed = settings.BaseSeed.HasValue ? settings.BaseSeed.Value + i : random.Next();

                // A separate generator for the start-count variation, derived
                // from the trial seed. It must not share the engine's random
                // numbers, otherwise changing the variance setting would also
                // change how the agents move.
                var variation = new Random(seed ^ 0x5F3759DF);
                int startPrimary = Jitter(basePrimary, settings.StartCountVariancePercent, variation);
                int startSecondary = Jitter(baseSecondary, settings.StartCountVariancePercent, variation);

                var trialSettings = CloneSettings(baseSettings, seed, startPrimary + startSecondary, startSecondary);
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

                // Agents deactivated by a failing script don't count as alive.
                var alive = engine.Worlds.Agents.Where(a => a.IsActive).ToList();

                int finalPrimary = alive.Count(a => a.Species == baseSettings.PrimaryGroupSpecies);
                int finalSecondary = hasSecondary && baseSettings.SecondaryGroupSpecies != baseSettings.PrimaryGroupSpecies
                    ? alive.Count(a => a.Species == baseSettings.SecondaryGroupSpecies)
                    : 0;

                trials.Add(new TrialResult
                {
                    TrialIndex = i,
                    Seed = seed,
                    FinalTickCount = engine.TickCount,
                    StartAgentCount = startPrimary + startSecondary,
                    StartPrimaryCount = startPrimary,
                    StartSecondaryCount = startSecondary,
                    FinalAgentCount = alive.Count,
                    FinalPrimaryCount = finalPrimary,
                    FinalSecondaryCount = finalSecondary,
                    FinalMeanPatchValue = MeanPatchValue(engine)
                });
            }

            var finalCounts = trials.Select(t => (double)t.FinalAgentCount).ToList();
            var startCounts = trials.Select(t => (double)t.StartAgentCount).ToList();

            return new MonteCarloSummary
            {
                Trials = trials,

                MeanFinalAgentCount = StatisticsHelper.Mean(finalCounts),
                StdDevFinalAgentCount = StatisticsHelper.StdDev(finalCounts),
                MinFinalAgentCount = (int)StatisticsHelper.Min(finalCounts),
                MaxFinalAgentCount = (int)StatisticsHelper.Max(finalCounts),

                PrimarySpecies = baseSettings.PrimaryGroupSpecies,
                SecondarySpecies = baseSettings.SecondaryGroupSpecies,
                HasSecondarySpecies = hasSecondary,
                BaseStartAgentCount = baseSettings.AgentCount,
                StartCountVariancePercent = settings.StartCountVariancePercent,

                StartTotal = SeriesStats.From(startCounts),
                FinalTotal = SeriesStats.From(finalCounts),
                FinalPrimary = SeriesStats.From(trials.Select(t => (double)t.FinalPrimaryCount)),
                FinalSecondary = SeriesStats.From(trials.Select(t => (double)t.FinalSecondaryCount)),
                SurvivalRate = SeriesStats.From(trials.Select(t => t.SurvivalRate)),
                FinalMeanPatchValue = SeriesStats.From(trials.Select(t => t.FinalMeanPatchValue)),

                StartFinalCorrelation = StatisticsHelper.Correlation(startCounts, finalCounts)
            };
        }

        // Moves a group size up or down by a random whole number of agents,
        // at most +/- the given percentage of the size (and at least +/- 1
        // whenever a percentage is set, so small groups still vary).
        // Never drops a group that existed to zero.
        private static int Jitter(int count, double percent, Random rng)
        {
            if (count <= 0 || percent <= 0) return Math.Max(0, count);

            int spread = Math.Max(1, (int)Math.Round(count * percent / 100.0));
            return Math.Max(1, count + rng.Next(-spread, spread + 1));
        }

        private static double MeanPatchValue(SimulationEngine engine)
        {
            var patches = engine.Worlds.Patches;
            if (patches == null) return 0;

            double total = 0;
            int count = 0;
            foreach (var patch in patches)
            {
                total += patch.Value;
                count++;
            }

            return count == 0 ? 0 : total / count;
        }

        private static SimulationSettings CloneSettings(
            SimulationSettings baseSettings, int seed, int agentCount, int secondaryCount) => new()
            {
                AgentCount = agentCount,
                WorldWidth = baseSettings.WorldWidth,
                WorldHeight = baseSettings.WorldHeight,
                StepSize = baseSettings.StepSize,
                MaxTurnDegrees = baseSettings.MaxTurnDegrees,
                SecondaryGroupCount = secondaryCount,
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