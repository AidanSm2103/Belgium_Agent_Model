using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Simulation;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class SimulationEngineTests
    {
        private static SimulationEngine CreateEngine(int agentCount = 10, int? seed = 42)
        {
            var settings = new SimulationSettings
            {
                AgentCount = agentCount,
                WorldWidth = 100,
                WorldHeight = 100,
                StepSize = 2.0,
                MaxTurnDegrees = 25,
                Seed = seed
            };
            return new SimulationEngine(settings);
        }

        [Fact]
        public void Setup_CreatesCorrectNumberOfAgents()
        {
            var engine = CreateEngine(agentCount: 15);

            engine.Setup();

            Assert.Equal(15, engine.World.Agents.Count);
        }

        [Fact]
        public void Setup_ResetsTickCountToZero()
        {
            var engine = CreateEngine();
            engine.Setup();
            engine.Tick();
            engine.Tick();

            engine.Setup();

            Assert.Equal(0, engine.TickCount);
        }

        [Fact]
        public void Tick_IncrementsTickCount()
        {
            var engine = CreateEngine();
            engine.Setup();

            engine.Tick();
            engine.Tick();
            engine.Tick();

            Assert.Equal(3, engine.TickCount);
        }

        [Fact]
        public void Tick_MovesAgents()
        {
            var engine = CreateEngine(agentCount: 1);
            engine.Setup();
            var agent = engine.World.Agents[0];
            var originalX = agent.X;
            var originalY = agent.Y;

            engine.Tick();

            Assert.False(agent.X == originalX && agent.Y == originalY);
        }

        [Fact]
        public void Tick_RaisesTickedEvent()
        {
            var engine = CreateEngine();
            engine.Setup();
            bool eventRaised = false;
            engine.Ticked += (sender, args) => eventRaised = true;

            engine.Tick();

            Assert.True(eventRaised);
        }

        [Fact]
        public void Start_SetsIsRunningTrue()
        {
            var engine = CreateEngine();

            engine.Start();

            Assert.True(engine.IsRunning);
        }

        [Fact]
        public void Stop_SetsIsRunningFalse()
        {
            var engine = CreateEngine();
            engine.Start();

            engine.Stop();

            Assert.False(engine.IsRunning);
        }
    }
}
