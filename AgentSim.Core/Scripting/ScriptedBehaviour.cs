using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using Microsoft.CodeAnalysis.Scripting;

namespace AgentSim.Core.Scripting;

public class ScriptedBehavior : IAgentBehavior
{
    private readonly Script<object> _script;

    // Compiled once into a plain delegate. Calling it is just a method call,
    // instead of building a new script-run object for every agent every tick.
    private readonly ScriptRunner<object> _runner;

    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(50);

    // Only the dry-run (BehaviorTestRunner) uses the thread + timeout guard,
    // because that is where a brand-new, untrusted script runs for the first
    // time (an infinite loop must not freeze the app). After it has passed,
    // normal ticks run the script directly on the calling thread.
    public bool UseTimeoutGuard { get; set; }

    public ScriptedBehavior(Script<object> script)
    {
        _script = script;
        _runner = script.CreateDelegate();
    }

    public void Execute(Agent agent, World world, RandomProvider rng, SimulationEngine engine)
    {
        var globals = new ScriptGlobals { Agent = agent, World = world, Rng = rng, Engine = engine };

        try
        {
            if (UseTimeoutGuard)
            {
                var task = Task.Run(() => _runner(globals));
                if (!task.Wait(TimeSpan.FromMilliseconds(500)) || task.IsFaulted)
                {
                    agent.IsActive = false;
                }
                return;
            }

            long start = Stopwatch.GetTimestamp();

            // Scripts have no 'await', so this finishes synchronously.
            _runner(globals).GetAwaiter().GetResult();

            // Still switch off an agent whose script is far too slow.
            if (Stopwatch.GetElapsedTime(start) > Timeout)
            {
                agent.IsActive = false;
            }
        }
        catch (Exception)
        {
            agent.IsActive = false;
        }
    }
}
