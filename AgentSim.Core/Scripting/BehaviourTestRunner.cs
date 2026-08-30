using AgentSim.Core.Agents;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Scripting
{
    // A compiled script can still fail the FIRST time it actually runs, even
    // though it compiled cleanly — e.g. a null reference or divide-by-zero
    // that only shows up at execution time. Rather than let that surprise
    // happen on a real agent mid-simulation, this runs the new behavior once
    // against a disposable "scratch" agent/world first.
    
    // Use this right after a successful BehaviorCompileResult, before
    // applying the behavior to real agents.
    public static class BehaviorTestRunner
    {
        public static bool TryDryRun(IAgentBehavior behavior, out string? failureReason)
        {
            try
            {
                var scratchWorld = new World(100, 100);
                var scratchAgent = new Agent(id: -1, x: 50, y: 50, heading: 0, behavior: behavior);
                var scratchRng = new RandomProvider(seed: 1); // fixed seed = repeatable test

                scratchAgent.Step(scratchWorld, scratchRng);

                // ScriptedBehavior swallows its own exceptions and sets
                // IsActive = false instead of throwing — so IsActive is the real signal a script failed or timed out on its first run.
                if (!scratchAgent.IsActive)
                {
                    failureReason = "The script failed or timed out on its first run.";
                    return false;
                }

                failureReason = null;
                return true;
            }
            catch (Exception ex)
            {
                // Defensive fallback for any future IAgentBehavior implementation that doesn't swallow its own exceptions the way ScriptedBehavior does
                failureReason = ex.Message;
                return false;
            }
        }
    }
}
