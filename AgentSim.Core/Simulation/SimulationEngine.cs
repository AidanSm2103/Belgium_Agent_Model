using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Utilities; 
using AgentSim.Core.Worlds;
using AgentSim.Core.Agents;
using System.Runtime.CompilerServices;

// This class owns the tick loop and the current state of the simulation
// It does not have a timer but rather the UI calls Tick() repeatedly once the user clicks "Start" for example
namespace AgentSim.Core.Simulation
{
    public class SimulationEngine
    {
        public World Worlds { get; private set; }
        public SimulationSettings Settings { get; }
        public int TickCount { get; private set; }
        public bool IsRunning { get; private set; }

        private RandomProvider _rng;

        // Changes queued by scripts during a tick, applied after the tick loop finishes 
        private readonly List<Agent> _pendingSpawns = new();
        private readonly List<Agent> _pendingKills = new();

        // The rendering side of the UI will be subscribed to this this is to know when it is necessary to redraw the canvas
        public event EventHandler? Ticked;

        public SimulationEngine(SimulationSettings settings)
        {
            Settings = settings;
            _rng = new RandomProvider(settings.Seed);
            Worlds = new World(settings.WorldWidth, settings.WorldHeight);
        }

        // Used to reinitialize the world and have the agents be spawned fresh
        public void Setup()
        {
            _rng = new RandomProvider(Settings.Seed);
            Worlds = new World(Settings.WorldWidth, Settings.WorldHeight);
            Worlds.InitializePatches(columns: 20, rows: 20);
            TickCount = 0;
            _pendingSpawns.Clear();
            _pendingKills.Clear();

            for (int i = 0; i < Settings.AgentCount; i++)
            {
                double x = _rng.NextDouble() * Settings.WorldWidth;
                double y = _rng.NextDouble() * Settings.WorldHeight;
                double heading = _rng.NextDouble() * 360;

                var behavior = new RandomWalkBehavior(Settings.MaxTurnDegrees, Settings.StepSize);
                var agent = new Agent(i, x, y, heading, behavior);

                // The last SecondaryGroupCount agents spawned belong to the secondary group, everyone else is primary.
                bool isSecondary = i >= Settings.AgentCount - Settings.SecondaryGroupCount;
                agent.Species = isSecondary ? Settings.SecondaryGroupSpecies : Settings.PrimaryGroupSpecies;

                Worlds.AddAgent(agent);
            }
            Ticked?.Invoke(this, EventArgs.Empty);
        }

        
        // This will advance our simulation by exactly one tick - every agent moves once
        public void Tick()
        {
            foreach (var agent in Worlds.Agents)
            {
                agent.Step(Worlds, _rng, this);
            }

            foreach (var agent in _pendingKills)
            {
                Worlds.RemoveAgent(agent);
            }
            _pendingKills.Clear();

            foreach (var agent in _pendingSpawns)
            {
                Worlds.AddAgent(agent);
            }
            _pendingSpawns.Clear();

            TickCount++;
            Ticked?.Invoke(this, EventArgs.Empty);
        }

        public void Start() => IsRunning = true;
        public void Stop() => IsRunning = false;

        // Takes effect after the current tick finishes, never mid-iteration.
        public void QueueSpawn(Agent agent) => _pendingSpawns.Add(agent);

        public void QueueKill(Agent agent)
        {
            agent.IsActive = false;
            _pendingKills.Add(agent);
        }

        // Applies a new behavior to every agent currently in the world. 
        public void ApplyBehaviorToAllAgents(IAgentBehavior behavior)
        {
            if (behavior == null) return;

            foreach (var agent in Worlds.Agents)
            {
                agent.Behavior = behavior;
            }
        }
    }   
}
