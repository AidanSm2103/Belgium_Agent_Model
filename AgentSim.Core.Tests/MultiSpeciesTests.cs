using System;
using System.Linq;
using Xunit;
using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using AgentSim.Core.Worlds;
using AgentSim.Core.Utilities;

namespace AgentSim.Core.Tests
{
    public class MultiSpeciesTests
    {
        [Fact]
        public void Agent_Assigned_To_Default_Species()
        {
            var agent = new Agent(1, 10, 10, 0, new RandomWalkBehavior());
            Assert.Equal("Default", agent.Species);
        }

        [Fact]
        public void Agent_Assigned_To_Custom_Species()
        {
            var agent = new Agent(1, 10, 10, 0, new RandomWalkBehavior())
            {
                Species = "Dog"
            };

            Assert.Equal("Dog", agent.Species);
        }

        [Fact]
        public void Simulation_Test_Filter_Multiple_Species()
        {
            var settings = new SimulationSettings { AgentCount = 0 };
            var engine = new SimulationEngine(settings);
            engine.Setup();

            var dog = new Agent(1, 5, 5, 0, new RandomWalkBehavior()) { Species = "Dog" };
            var cat1 = new Agent(2, 10, 10, 0, new RandomWalkBehavior()) { Species = "Cat" };
            var cat2 = new Agent(3, 15, 15, 0, new RandomWalkBehavior()) { Species = "Cat" };

            engine.Worlds.AddAgent(dog);
            engine.Worlds.AddAgent(cat1);
            engine.Worlds.AddAgent(cat2);

            var dogs = engine.Worlds.Agents.Where(a => a.Species == "Dog").ToList();
            var cats = engine.Worlds.Agents.Where(a => a.Species == "Cat").ToList();

            Assert.Single(dogs);
            Assert.Equal(2, cats.Count);
        }

        [Fact]
        public void Behavior_To_Target_Specific_Species_Only()
        {
            var settings = new SimulationSettings { AgentCount = 0 };
            var engine = new SimulationEngine(settings);
            engine.Setup();

            int catHuntedCount = 0;

            var predatorBehavior = new CustomSpeciesBehavior((agent, world, random, simEngine) =>
            {
                var targetCat = world.Agents.FirstOrDefault(a => a.Species == "Cat" && a.IsActive);
                if (targetCat != null)
                {
                    catHuntedCount++;
                    targetCat.IsActive = false;
                }
            });

            var wolf = new Agent(1, 0, 0, 0, predatorBehavior) { Species = "Wolf" };
            var cat = new Agent(2, 5, 5, 0, new RandomWalkBehavior()) { Species = "Cat" };

            engine.Worlds.AddAgent(wolf);
            engine.Worlds.AddAgent(cat);

            engine.Tick();

            Assert.Equal(1, catHuntedCount);
            Assert.False(cat.IsActive);
        }

        private class CustomSpeciesBehavior : IAgentBehavior
        {
            private readonly Action<Agent, World, RandomProvider, SimulationEngine> engine_action;

            public CustomSpeciesBehavior(Action<Agent, World, RandomProvider, SimulationEngine> action)
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