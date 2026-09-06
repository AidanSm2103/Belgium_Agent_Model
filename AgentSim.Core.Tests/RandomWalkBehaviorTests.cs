using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class RandomWalkBehaviourTests
    {
        private static SimulationEngine CreateScratchEngine() =>
            new(new SimulationSettings { WorldWidth = 100, WorldHeight = 100, AgentCount = 0 });

        [Fact]
        public void Execute_MovesAgent()
        {
            // Arrange
            var world = new World(100, 100);
            var agent = new Agent(
                1,
                50,
                50,
                0,
                new RandomWalkBehavior(maxTurnDegrees: 0, stepSize: 5)
            );

            var rng = new RandomProvider(seed: 42);
            var engine = CreateScratchEngine();
            var originalX = agent.X;
            var originalY = agent.Y;

            // Act
            var behavior = new RandomWalkBehavior(maxTurnDegrees: 0, stepSize: 5);
            behavior.Execute(agent, world, rng, engine);

            // Assert
            Assert.True(agent.X != originalX || agent.Y != originalY);
        }

        [Fact]
        public void Execute_WithZeroTurn_KeepsHeadingUnchanged()
        {
            // Arrange
            var world = new World(100, 100);
            var agent = new Agent(
                1,
                50,
                50,
                90,
                new RandomWalkBehavior(maxTurnDegrees: 0, stepSize: 5)
            );

            var rng = new RandomProvider(seed: 42);
            var engine = CreateScratchEngine();
            var originalHeading = agent.Heading;

            // Act
            var behavior = new RandomWalkBehavior(maxTurnDegrees: 0, stepSize: 5);
            behavior.Execute(agent, world, rng, engine);

            // Assert
            Assert.Equal(originalHeading, agent.Heading, precision: 5);
        }

        [Fact]
        public void Execute_WithZeroStepSize_DoesNotMoveAgent()
        {
            // Arrange
            var world = new World(100, 100);
            var agent = new Agent(
                1,
                50,
                50,
                0,
                new RandomWalkBehavior(maxTurnDegrees: 25, stepSize: 0)
            );

            var rng = new RandomProvider(seed: 42);
            var engine = CreateScratchEngine();

            // Act
            var behavior = new RandomWalkBehavior(maxTurnDegrees: 25, stepSize: 0);
            behavior.Execute(agent, world, rng, engine);

            // Assert
            Assert.Equal(50, agent.X, precision: 5);
            Assert.Equal(50, agent.Y, precision: 5);
        }
    }
}
 