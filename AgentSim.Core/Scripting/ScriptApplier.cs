using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Scripting
{
    // The entry point the UI calls to compile, test, and apply a user script
    public static class ScriptApplier
    {
        public static ScriptApplyResult ApplyScript(string userCode, SimulationEngine engine, Func<Agent, bool>? targetFilter = null)
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

            if (targetFilter != null)
            {
                engine.ApplyBehaviorToAgentsMatching(targetFilter, compileResult.Behavior!);
            }
            else
            {
                engine.ApplyBehaviorToAllAgents(compileResult.Behavior!);
            }

            return ScriptApplyResult.Ok();
        }
    }
}

