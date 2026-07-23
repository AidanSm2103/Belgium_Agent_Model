using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Agents
{
    public interface IAgentBehaviour
    {
        void Execute(Agent agent, World world, RandomProvider rng);
    }
}
