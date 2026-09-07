using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // If a user's script hasn't finished within this window, we stop waiting on it and deactivate that agent
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(50);

        public ScriptedBehavior(Script<object> script)
        {
            _script = script;
        }

        public void Execute(Agent agent, World world, RandomProvider rng, SimulationEngine engine)
        {
            var globals = new ScriptGlobals { Agent = agent, World = world, Rng = rng, Engine = engine };

            try
            {
                var task = Task.Run(() => _script.RunAsync(globals));
                if (!task.Wait(Timeout))
                {
                    // Script took too long — don't let it hang the tick loop.
                    agent.IsActive = false;
                    return;
                }

                if (task.IsFaulted)
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
