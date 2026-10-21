using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AgentSim.Core.Agents;
using AgentSim.Core.Simulation;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using Microsoft.CodeAnalysis.Scripting;

namespace AgentSim.Core.Scripting
{
    // Wraps a compiled Roslyn script so it can be used anywhere a normal IAgentBehavior is expected
    public class ScriptedBehavior : IAgentBehavior
    {
        private readonly Script<object> _script;

        // Compiled once into a plain delegate: calling it is a normal method
        // call instead of building a new script run per agent per tick.
        private readonly ScriptRunner<object> _runner;

        // A script slower than this deactivates its agent.
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(50);

        // The C# text this behavior was compiled from. Kept so a saved
        // simulation can recompile the same behavior when it is loaded.
        public string SourceCode { get; }

        // Only the first test run of a new script (BehaviourTestRunner) uses
        // the thread + timeout guard, so an infinite loop can't freeze the app.
        // Normal ticks run the script directly on the calling thread.
        public bool UseTimeoutGuard { get; set; }

        public ScriptedBehavior(Script<object> script, string sourceCode = "")
        {
            _script = script;
            _runner = script.CreateDelegate();
            SourceCode = sourceCode;
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
                _runner(globals).GetAwaiter().GetResult();

                if (Stopwatch.GetElapsedTime(start) > Timeout)
                {
                    agent.IsActive = false;
                }
            }
            catch (Exception)
            {
                // Deactivate this agent rather than crash the simulation.
                agent.IsActive = false;
            }
        }
    }
}
