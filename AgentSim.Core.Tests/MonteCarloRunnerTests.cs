using System.Linq;
using Xunit;
using AgentSim.Core.Analysis;
using AgentSim.Core.Simulation;

namespace AgentSim.Core.Tests
{
    public class MonteCarloRunnerTests
    {
        [Fact]
        public void Executes_Specific_Number_Of_Trials_And_Ticks()
        {
            var settings = new MonteCarloSettings
            {
                TrialCount = 5,
                TicksPerTrial = 10,
                BaseSettings = new SimulationSettings { AgentCount = 3 },
                BaseSeed = 12345
            };

            var execute = MonteCarloRunner.Run(settings);

            Assert.Equal(5, execute.Trials.Count);
            Assert.All(execute.Trials, trial => Assert.Equal(10, trial.FinalTickCount));
        }

        [Fact]
        public void Run_Determined_Seed()
        {
            var settings = new MonteCarloSettings
            {
                TrialCount = 3,
                TicksPerTrial = 5,
                BaseSettings = new SimulationSettings { AgentCount = 10 },
                BaseSeed = 999
            };

            var runner1 = MonteCarloRunner.Run(settings);
            var runner2 = MonteCarloRunner.Run(settings);

            Assert.Equal(runner1.MeanFinalAgentCount, runner2.MeanFinalAgentCount);
            Assert.Equal(runner1.StdDevFinalAgentCount, runner2.StdDevFinalAgentCount);
            Assert.Equal(runner1.Trials.Select(t => t.Seed), runner2.Trials.Select(t => t.Seed));
        }

        [Fact]
        public void Calculates_Statistics()
        {
            var settings = new MonteCarloSettings
            {
                TrialCount = 4,
                TicksPerTrial = 2,
                BaseSettings = new SimulationSettings { AgentCount = 5 },
                BaseSeed = 42
            };

            var execute = MonteCarloRunner.Run(settings);

            Assert.Equal(5, execute.MeanFinalAgentCount);
            Assert.Equal(0, execute.StdDevFinalAgentCount);
            Assert.Equal(5, execute.MinFinalAgentCount);
            Assert.Equal(5, execute.MaxFinalAgentCount);
        }
    }
}