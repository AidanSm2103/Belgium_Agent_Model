using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;

// Defines how an agent behaves on each simulation tick.
// RandomWalkBehavior implements this for the MVP demo. Later, when
// the scripting subsystem lands, user-written rules will implement
// this same interface — Agent and SimulationEngine won't need to change.

namespace AgentSim.Core.Agents
{
    public interface IAgentBehaviour
    {
        void Execute(Agent agent, World world, RandomProvider rng);
    }
}
