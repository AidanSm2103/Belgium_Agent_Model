using System.Collections.Generic;
using System.Linq;
using AgentSim.Core.Agents;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;

namespace AgentSim.Core.Persistence
{
    // Copies the live state of a simulation (agents, patches, tick count,
    // applied scripts) into a SimulationSaveData and back again.
    public static class SimulationSnapshot
    {
        public static void Capture(SimulationEngine engine, SimulationSaveData data)
        {
            var scripts = new List<string>();
            var agents = new List<AgentSaveData>();

            foreach (var agent in engine.Worlds.Agents)
            {
                int scriptIndex = -1;

                if (agent.Behavior is ScriptedBehavior scripted && !string.IsNullOrEmpty(scripted.SourceCode))
                {
                    scriptIndex = scripts.IndexOf(scripted.SourceCode);
                    if (scriptIndex < 0)
                    {
                        scripts.Add(scripted.SourceCode);
                        scriptIndex = scripts.Count - 1;
                    }
                }

                agents.Add(new AgentSaveData
                {
                    Id = agent.Id,
                    X = agent.X,
                    Y = agent.Y,
                    Heading = agent.Heading,
                    IsActive = agent.IsActive,
                    Species = agent.Species,
                    ScriptIndex = scriptIndex
                });
            }

            var (patchValues, patchTimers) = engine.Worlds.CapturePatchState();

            data.TickCount = engine.TickCount;
            data.Scripts = scripts;
            data.Agents = agents;
            data.PatchValues = patchValues;
            data.PatchTimers = patchTimers;
            data.ScriptAssignments = engine.ScriptAssignments.ToList();
        }

        // Rebuilds the simulation from a save file. Engine.Settings must
        // already hold the saved settings. Returns a warning message if some
        // scripts couldn't be recompiled (those agents fall back to the
        // default behavior), or null if everything restored cleanly.
        public static string? Restore(SimulationEngine engine, SimulationSaveData data)
        {
            // Older save file with settings only: nothing to restore, start fresh.
            if (data.Agents == null)
            {
                engine.Setup();
                return null;
            }

            var warnings = new List<string>();

            var behaviors = new List<IAgentBehavior?>();
            foreach (var source in data.Scripts ?? new List<string>())
            {
                var result = BehaviorCompiler.Compile(source);
                if (result.Success)
                {
                    behaviors.Add(result.Behavior);
                }
                else
                {
                    behaviors.Add(null);
                    warnings.Add("A saved script no longer compiles and was replaced by the default behavior: " + result.ErrorMessage);
                }
            }

            var agents = data.Agents.Select(saved =>
            {
                IAgentBehavior behavior =
                    saved.ScriptIndex >= 0 && saved.ScriptIndex < behaviors.Count && behaviors[saved.ScriptIndex] != null
                        ? behaviors[saved.ScriptIndex]!
                        : new RandomWalkBehavior(engine.Settings.MaxTurnDegrees, engine.Settings.StepSize);

                return new Agent(saved.Id, saved.X, saved.Y, saved.Heading, behavior)
                {
                    Species = saved.Species,
                    IsActive = saved.IsActive
                };
            }).ToList();

            engine.RestoreState(
                data.TickCount ?? 0,
                agents,
                data.PatchValues,
                data.PatchTimers,
                data.ScriptAssignments);

            return warnings.Count == 0 ? null : string.Join("\n", warnings);
        }
    }
}