using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class AgentTests
    {
        private static SimulationEngine CreateScratchEngine() =>
            new(new SimulationSettings { WorldWidth = 100, WorldHeight = 100, AgentCount = 0 });

        [Fact]
        public void Step_WhenActive_MovesAccordingToBehavior()
        {
            var world = new World(100, 100);
            // maxTurnDegrees: 0 keeps heading fixed at 0 (straight "up"), so movement is predictable
            var agent = new Agent(1, 50, 50, 0, new RandomWalkBehavior(maxTurnDegrees: 0, stepSize: 5));
            var rng = new RandomProvider(seed: 1);
            var engine = CreateScratchEngine();

            agent.Step(world, rng, engine);

            Assert.Equal(50, agent.X, precision: 5);
            Assert.Equal(45, agent.Y, precision: 5);
        }

        [Fact]
        public void Step_WhenInactive_DoesNotMove()
        {
            var world = new World(100, 100);
            var agent = new Agent(1, 50, 50, 0, new RandomWalkBehavior())
            {
                IsActive = false
            };
            var rng = new RandomProvider();
            var engine = CreateScratchEngine();

            agent.Step(world, rng, engine);

            Assert.Equal(50, agent.X);
            Assert.Equal(50, agent.Y);
        }
    }
}
