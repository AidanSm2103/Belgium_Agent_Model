using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Analysis
{
    // Runs a simulation many times over with different random seeds and aggregates the outcomes 
    public static class MonteCarloRunner
    {
        public static MonteCarloSummary Run(MonteCarloSettings settings, IAgentBehavior? behaviorOverride = null)
        {
            var random = new System.Random(settings.BaseSeed ?? System.Environment.TickCount);
            var trials = new List<TrialResult>();

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

                for (int t = 0; t < settings.TicksPerTrial; t++)
                {
                    engine.Tick();
                }

                trials.Add(new TrialResult
                {
                    TrialIndex = i,
                    Seed = seed,
                    FinalAgentCount = engine.Worlds.Agents.Count,
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
            Seed = seed
        };
    }
}
