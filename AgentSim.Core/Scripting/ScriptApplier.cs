using System;
using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;

namespace AgentSim.Core.Scripting
{
    // The entry point the UI calls to compile, test, and apply a user script.
    public static class ScriptApplier
    {
        // Applies to every agent (targetFilter == null) or to agents matching
        // a predicate. A predicate can't be recorded by species name, so a
        // filtered apply is NOT added to engine.ScriptAssignments — use
        // ApplyScriptToSpecies when you want Monte Carlo / save-load to
        // know about it.
        public static ScriptApplyResult ApplyScript(string userCode, SimulationEngine engine, Func<Agent, bool>? targetFilter = null)
        {
            return ApplyInternal(userCode, engine, targetFilter, species: null, record: targetFilter == null);
        }

        // Applies to one species (or to everyone when species is null) and
        // records the assignment on the engine, so Monte Carlo trials and
        // saved simulations reproduce it.
        public static ScriptApplyResult ApplyScriptToSpecies(string userCode, SimulationEngine engine, string? species)
        {
            Func<Agent, bool>? filter = species == null ? null : (a => a.Species == species);
            return ApplyInternal(userCode, engine, filter, species, record: true);
        }

        private static ScriptApplyResult ApplyInternal(string userCode, SimulationEngine engine, Func<Agent, bool>? filter, string? species, bool record)
        {
            var compileResult = BehaviorCompiler.Compile(userCode);
            if (!compileResult.Success)
            {
                return ScriptApplyResult.Fail(compileResult.ErrorMessage ?? "Compilation failed.");
            }

            if (!BehaviorTestRunner.TryDryRun(compileResult.Behavior!, out var failureReason))
            {
                return ScriptApplyResult.Fail(failureReason ?? "Script failed on its first run.");
            }

            if (filter != null)
            {
                engine.ApplyBehaviorToAgentsMatching(filter, compileResult.Behavior!);
            }
            else
            {
                engine.ApplyBehaviorToAllAgents(compileResult.Behavior!);
            }

            if (record)
            {
                engine.RecordScriptAssignment(species, userCode);
            }

            return ScriptApplyResult.Ok();
        }
    }
}
