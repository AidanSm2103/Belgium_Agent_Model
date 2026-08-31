using AgentSim.Core.Agents;
using AgentSim.Core.Scripting;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using Xunit;

namespace AgentSim.Core.Tests.Scripting
{
    public class BehaviorCompilerTests
    {
        // TEST 1: Valid script
        [Fact]
        public void Compile_ValidScript_ReturnsWorkingBehavior()
        {
            // Arrange
            var script = @"
                Agent.X += 1;
            ";

            // Act
            var result = BehaviorCompiler.Compile(script);

            // Assert
            Assert.True(result.Success, result.ErrorMessage);
            Assert.NotNull(result.Behavior);

            var world = new World(100, 100);
            var agent = new Agent(
                id: 1,
                x: 50,
                y: 50,
                heading: 0,
                behavior: result.Behavior!
            );

            var rng = new RandomProvider(seed: 42);

            result.Behavior!.Execute(agent, world, rng);

            Assert.Equal(51, agent.X, precision: 5);
        }


        // TEST 2: Invalid script
        [Fact]
        public void Compile_InvalidScript_ReturnsUsefulError()
        {
            // Arrange
            var script = @"
                this is not valid C# code;
            ";

            // Act
            var result = BehaviorCompiler.Compile(script);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Behavior);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        }

        [Fact]
        public void Execute_InfiniteLoop_DeactivatesAgent()
        {
        // Arrange
            var script = @"
            while (true)
            {
            }
            ";

            var result = BehaviorCompiler.Compile(script);

            Assert.True(result.Success, result.ErrorMessage);
            Assert.NotNull(result.Behavior);

            var world = new World(100, 100);
            var agent = new Agent(
            id: 1,
            x: 50,
            y: 50,
            heading: 0,
            behavior: result.Behavior!
            );

            var rng = new RandomProvider(seed: 42);

        // Act
            result.Behavior!.Execute(agent, world, rng);

        // Assert
           Assert.False(agent.IsActive);
        }
    }
}