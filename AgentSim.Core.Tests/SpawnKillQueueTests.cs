using System;
using System.Linq;
using Xunit;
using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using AgentSim.Core.Worlds;
using AgentSim.Core.Utilities;

namespace AgentSim.Core.Tests
{
    public class SpawnKillQueueTests
    {
        [Fact]
        public void QueueToSpawn()
        {
            var settings = new SimulationSettings { AgentCount = 5 };
            var engine = new SimulationEngine(settings);
            engine.Setup();
            int count = engine.Worlds.Agents.Count;

            engine.QueueSpawn(new Agent(999, 0, 0, 0, new RandomWalkBehavior()));

            Assert.Equal(count, engine.Worlds.Agents.Count);

            engine.Tick();

            Assert.Equal(count + 1, engine.Worlds.Agents.Count);
        }

        [Fact]
        public void QueueToKill()
        {
            var settings = new SimulationSettings { AgentCount = 5 };
            var engine = new SimulationEngine(settings);
            engine.Setup();
            var targetedAgent = engine.Worlds.Agents.First();
            int count = engine.Worlds.Agents.Count;

            engine.QueueKill(targetedAgent);

            Assert.Equal(count, engine.Worlds.Agents.Count);
            Assert.Contains(targetedAgent, engine.Worlds.Agents);

            engine.Tick();

            Assert.Equal(count - 1, engine.Worlds.Agents.Count);
            Assert.DoesNotContain(targetedAgent, engine.Worlds.Agents);
        }

        [Fact]
        public void QueuedSpawnsAndKills()
        {
            var settings = new SimulationSettings { AgentCount = 3 };
            var engine = new SimulationEngine(settings);
            engine.Setup();

            var spawningAndKillingBehavior = new CustomTestBehavior((agent, world, random, simEngine) =>
            {
                simEngine.QueueSpawn(new Agent(888, 10, 10, 0, new RandomWalkBehavior()));
                simEngine.QueueKill(agent);
            });

            engine.ApplyBehaviorToAllAgents(spawningAndKillingBehavior);

            var exception = Record.Exception(() => engine.Tick());
            Assert.Null(exception);
        }

        private class CustomTestBehavior : IAgentBehavior
        {
            private readonly Action<Agent, World, RandomProvider, SimulationEngine> engine_action;

            public CustomTestBehavior(Action<Agent, World, RandomProvider, SimulationEngine> action)
            {
                engine_action = action;
            }

            public void Execute(Agent agent, World world, RandomProvider random, SimulationEngine engine)
            {
                engine_action(agent, world, random, engine);
            }
        }
    }
}