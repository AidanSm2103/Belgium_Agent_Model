using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Agents;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using Microsoft.CodeAnalysis.Scripting;

namespace AgentSim.Core.Scripting
{
    // <summary>
    /// Wraps a compiled Roslyn script so it can be used anywhere a normal
    /// IAgentBehavior is expected — Agent.Step() doesn't know or care that
    /// this one runs user code instead of a hardcoded C# class.
    /// </summary>
    public class ScriptedBehavior : IAgentBehavior
    {
        private readonly Script<object> _script;

        // Best-effort safety limit: if a user's script hasn't finished within
        // this window, we stop waiting on it and deactivate that agent rather
        // than let it hang the whole simulation. NOTE: this is a practical
        // safeguard against accidental slow/looping code, not a hard security
        // guarantee — true isolation from malicious code needs a separate
        // process, which is out of scope for this project.
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(50);

        public ScriptedBehavior(Script<object> script)
        {
            _script = script;
        }

        public void Execute(Agent agent, World world, RandomProvider rng)
        {
            var globals = new ScriptGlobals { Agent = agent, World = world, Rng = rng };

            try
            {
                var task = _script.RunAsync(globals);
                if (!task.Wait(Timeout))
                {
                    // Script took too long — don't let it hang the tick loop.
                    agent.IsActive = false;
                    return;
                }
            }
            catch (Exception)
            {
                // Script threw at runtime (null ref, divide by zero, etc.)
                // Deactivate this agent rather than crash the simulation.
                agent.IsActive = false;
            }
        }
    }
}
