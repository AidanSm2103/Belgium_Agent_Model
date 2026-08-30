using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Agents;
using AgentSim.Core.Worlds;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class WorldTests
    {
        [Fact]
        public void AddAgent_AddsToAgentsList()
        {
            var world = new World(100, 100);
            var agent = new Agent(1, 10, 10, 0, new RandomWalkBehavior());

            world.AddAgent(agent);

            Assert.Single(world.Agents);
            Assert.Contains(agent, world.Agents);
        }

        [Fact]
        public void RemoveAgent_RemovesFromAgentsList()
        {
            var world = new World(100, 100);
            var agent = new Agent(1, 10, 10, 0, new RandomWalkBehavior());
            world.AddAgent(agent);

            world.RemoveAgent(agent);

            Assert.Empty(world.Agents);
        }

        [Fact]
        public void Wrap_WrapsXCoordinateAroundWidth()
        {
            var world = new World(100, 100);

            var (x, _) = world.Wrap(110, 50);

            Assert.Equal(10, x, precision: 5);
        }

        [Fact]
        public void Wrap_WrapsYCoordinateAroundHeight()
        {
            var world = new World(100, 100);

            var (_, y) = world.Wrap(50, 105);

            Assert.Equal(5, y, precision: 5);
        }

        [Fact]
        public void Wrap_HandlesNegativeCoordinates()
        {
            var world = new World(100, 100);

            var (x, y) = world.Wrap(-10, -10);

            Assert.Equal(90, x, precision: 5);
            Assert.Equal(90, y, precision: 5);
        }
    }
}
