using System;
using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;

namespace AgentSim.Core.Scripting;

public static class BehaviorTestRunner
{
    public static bool TryDryRun(IAgentBehavior behavior, out string? failureReason)
    {
        try
        {
            var scratchWorld = new World(100, 100);
            scratchWorld.InitializePatches(columns: 5, rows: 5);
            var scratchAgent = new Agent(id: -1, x: 50, y: 50, heading: 0, behavior: behavior);
            var scratchRng = new RandomProvider(seed: 1);

            // A throwaway engine — so if the script queues a spawn/kill during
            // this test run, it happens against a disposable scratch engine,
            // not the real simulation the user hasn't actually applied yet.
            var scratchEngine = new SimulationEngine(new SimulationSettings
            {
                WorldWidth = 100,
                WorldHeight = 100,
                AgentCount = 0
            });

            // First run of an unknown script: use the thread + timeout guard
            // (generous, covers JIT warm-up), then switch to the fast path.
            var scripted = behavior as ScriptedBehavior;
            if (scripted != null) scripted.UseTimeoutGuard = true;
            try
            {
                scratchAgent.Step(scratchWorld, scratchRng, scratchEngine);
            }
            finally
            {
                if (scripted != null) scripted.UseTimeoutGuard = false;
            }

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
            failureReason = ex.Message;
            return false;
        }
    }
}
