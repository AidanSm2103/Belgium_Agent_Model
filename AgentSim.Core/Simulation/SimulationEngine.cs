using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using AgentSim.Core.Agents;
using AgentSim.Core.Scripting;
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

        // Which scripts the user has applied, in order (see ScriptAssignment).
        // Monte Carlo trials and save/load use this to reproduce them.
        private readonly List<ScriptAssignment> _scriptAssignments = new();
        public IReadOnlyList<ScriptAssignment> ScriptAssignments => _scriptAssignments;

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
            CreateFreshWorld();

            TickCount = 0;
            _pendingSpawns.Clear();
            _pendingKills.Clear();
            _scriptAssignments.Clear();   // fresh agents all run the default behavior again

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
            // Snapshot with ToList() so the loop is safe even if something
            // mutates the live agent list directly instead of using the queue.
            foreach (var agent in Worlds.Agents.ToList())
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

            // Empty patches count down and regrow (no-op when
            // Settings.PatchRegrowthTicks is 0).
            Worlds.RegrowPatches(Settings.PatchRegrowthTicks);

            TickCount++;
            Ticked?.Invoke(this, EventArgs.Empty);
        }

        // Builds a new World and patch grid from Settings. Grid size and
        // starting value come from Settings; the grid dimensions are floored
        // at 1 so a bad value can't produce an empty grid (which would break GetPatchAt).
        private void CreateFreshWorld()
        {
            _rng = new RandomProvider(Settings.Seed);
            Worlds = new World(Settings.WorldWidth, Settings.WorldHeight);
            Worlds.InitializePatches(
                columns: Math.Max(1, Settings.PatchColumns),
                rows: Math.Max(1, Settings.PatchRows),
                initialValue: Settings.PatchInitialValue);
        }

        // Rebuilds the simulation from a saved snapshot instead of spawning
        // new agents: the given agents, patch values, tick count and script
        // record replace whatever is currently loaded. The random number
        // stream restarts from Settings.Seed (its position can't be saved),
        // so a restored run continues from the same STATE but not
        // necessarily the same random sequence.
        public void RestoreState(
            int tickCount,
            IEnumerable<Agent> agents,
            IReadOnlyList<double>? patchValues,
            IReadOnlyList<int>? patchTimers,
            IEnumerable<ScriptAssignment>? assignments)
        {
            CreateFreshWorld();
            Worlds.RestorePatchState(patchValues, patchTimers);

            foreach (var agent in agents)
            {
                Worlds.AddAgent(agent);
            }

            TickCount = tickCount;
            _pendingSpawns.Clear();
            _pendingKills.Clear();

            _scriptAssignments.Clear();
            if (assignments != null)
            {
                _scriptAssignments.AddRange(assignments);
            }

            Ticked?.Invoke(this, EventArgs.Empty);
        }

        // Remembers that a script was applied to everyone (species == null)
        // or to one species. Applying to everyone replaces everything
        // recorded so far; applying to a species replaces that species' entry.
        public void RecordScriptAssignment(string? species, string source)
        {
            if (species == null)
            {
                _scriptAssignments.Clear();
            }
            else
            {
                _scriptAssignments.RemoveAll(a => a.Species == species);
            }
            _scriptAssignments.Add(new ScriptAssignment(species, source));
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

        // Applies a behavior only to agents matching a predicate, e.g.
        // agent => agent.Species == "Prey", instead of all-or-nothing.
        public void ApplyBehaviorToAgentsMatching(Func<Agent, bool> predicate, IAgentBehavior behavior)
        {
            if (behavior == null || predicate == null) return;

            foreach (var agent in Worlds.Agents.Where(predicate))
            {
                agent.Behavior = behavior;
            }
        }
    }
}
