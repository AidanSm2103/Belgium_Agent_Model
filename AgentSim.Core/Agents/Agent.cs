using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Agents
{
    public class Agent
    {
        public int Id { get; }
        public double X { get; set; }
        public double Y { get; set; }

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

        public void Step(World world, RandomProvider rng)
        {
            if (!IsActive) return;
            Behavior.Execute(this, world, rng);
        }
    }
}
