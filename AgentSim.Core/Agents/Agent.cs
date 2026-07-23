using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;


// Base class for all simulated agents 
// Holds position, heading, and delegates per-tick logic to its Behavior

namespace AgentSim.Core.Agents
{
    public class Agent
    {
        public int Id { get; }
        public double X { get; set; }
        public double Y { get; set; }

        // Heading in degrees. 0 = facing "north" (up), increases clockwise
        public double Heading { get; set; }

        public bool IsActive { get; set; } = true;

        public IAgentBehavior Behavior { get; set; }

        public Agent(int id, double x, double y, double heading, IAgentBehavior behavior)
        {
            Id = id;
            X = x;
            Y = y;
            Heading = heading;
            Behavior = behavior;
        }

        // Advances this agent by exactly one tick. Called by SimulationEngine — never call this directly from UI code, go through SimulationEngine.Tick() instead.
        public void Step(World world, RandomProvider rng)
        {
            if (!IsActive) return;
            Behavior.Execute(this, world, rng);
        }
    }
}
