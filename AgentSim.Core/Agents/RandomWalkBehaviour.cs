using System;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;

namespace AgentSim.Core.Agents;

// Hardcoded behavior for the MVP demo: turn by a small random amount each tick, then move forward.
public class RandomWalkBehavior : IAgentBehavior
{
    private readonly double _maxTurnDegrees;
    private readonly double _stepSize;

    public RandomWalkBehavior(double maxTurnDegrees = 25, double stepSize = 2.0)
    {
        _maxTurnDegrees = maxTurnDegrees;
        _stepSize = stepSize;
    }

    public void Execute(Agent agent, World world, RandomProvider rng)
    {
        // Turn by a random amount within [-maxTurnDegrees, +maxTurnDegrees]
        double turn = rng.NextDouble(-_maxTurnDegrees, _maxTurnDegrees);
        agent.Heading = (agent.Heading + turn + 360) % 360;

        // Move forward in the direction of the new heading
        double radians = agent.Heading * Math.PI / 180.0;
        double newX = agent.X + Math.Sin(radians) * _stepSize;
        double newY = agent.Y - Math.Cos(radians) * _stepSize;

        (agent.X, agent.Y) = world.Wrap(newX, newY);
    }
}
