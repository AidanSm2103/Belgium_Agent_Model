using AgentSim.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Scripting
{
    // The one method the UI needs to call. Runs the full pipeline —
    // safety check (inside Compile) → Roslyn compile → dry run → apply to
    // all agents — and returns a single pass/fail result with an error
    // message ready to show the user directly.
    public static class ScriptApplier
    {
        public static ScriptApplyResult ApplyScript(string userCode, SimulationEngine engine)
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

            engine.ApplyBehaviorToAllAgents(compileResult.Behavior!);
            return ScriptApplyResult.Ok();
        }
    }
}
