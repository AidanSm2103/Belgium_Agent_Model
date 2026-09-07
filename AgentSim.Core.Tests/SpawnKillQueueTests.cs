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
        public void QueueSpawn_DoesNotAddAgentMidTick_AppliesAfterTick()
        {
            var settings = new SimulationSettings { AgentCount = 5 };
            var engine = new SimulationEngine(settings);
            engine.Setup();
            int initialCount = engine.Worlds.Agents.Count;

            engine.QueueSpawn(new Agent(999, 0, 0, 0, new RandomWalkBehavior()));

            Assert.Equal(initialCount, engine.Worlds.Agents.Count);

            engine.Tick();

            Assert.Equal(initialCount + 1, engine.Worlds.Agents.Count);
        }

        [Fact]
        public void QueueKill_DoesNotRemoveAgentMidTick_AppliesAfterTick()
        {
            var settings = new SimulationSettings { AgentCount = 5 };
            var engine = new SimulationEngine(settings);
            engine.Setup();
            var targetAgent = engine.Worlds.Agents.First();
            int initialCount = engine.Worlds.Agents.Count;

            engine.QueueKill(targetAgent);

            Assert.Equal(initialCount, engine.Worlds.Agents.Count);
            Assert.Contains(targetAgent, engine.Worlds.Agents);

            engine.Tick();

            Assert.Equal(initialCount - 1, engine.Worlds.Agents.Count);
            Assert.DoesNotContain(targetAgent, engine.Worlds.Agents);
        }

        [Fact]
        public void QueuedSpawnsAndKills_DuringScriptExecution_DoNotCrashTickLoop()
        {
            var settings = new SimulationSettings { AgentCount = 3 };
            var engine = new SimulationEngine(settings);
            engine.Setup();

            var spawningAndKillingBehavior = new CustomTestBehavior((agent, world, rng, simEngine) =>
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
            private readonly Action<Agent, World, RandomProvider, SimulationEngine> _action;

            public CustomTestBehavior(Action<Agent, World, RandomProvider, SimulationEngine> action)
            {
                _action = action;
            }

            public void Execute(Agent agent, World world, RandomProvider rng, SimulationEngine engine)
            {
                _action(agent, world, rng, engine);
            }
        }
    }
}