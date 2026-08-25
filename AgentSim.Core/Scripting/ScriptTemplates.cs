using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Scripting
{
    // A small library of ready-made example scripts. Two purposes:
    //  1. Test input for the compiler — proving BehaviorCompiler.Compile()
    //     handles more than one example correctly.
    //  2. Starter content the UI can offer users (a "try one of these"
    //     dropdown) instead of a blank textbox.
    //
    // Each entry is valid input to BehaviorCompiler.Compile() — C# statements
    // only (no class/method wrapper), using the names Agent, World, and Rng
    // exposed by ScriptGlobals.
    public static class ScriptTemplates
    {
        // Matches the built-in RandomWalkBehavior's logic exactly.
        public const string RandomWalk = @"
            double turn = (Rng.NextDouble() - 0.5) * 20;
            Agent.Heading = (Agent.Heading + turn + 360) % 360;
            double rad = Agent.Heading * Math.PI / 180;
            double nx = Agent.X + Math.Sin(rad) * 2;
            double ny = Agent.Y - Math.Cos(rad) * 2;
            var (wx, wy) = World.Wrap(nx, ny);
            Agent.X = wx;
            Agent.Y = wy;
            ";

        // Agents drift toward the middle of the world instead of wandering visually distinct from RandomWalk, good for proving scripting is really changing behavior during a demo.
        public const string FleeToCenter = @"
            double centerX = World.Width / 2;
            double centerY = World.Height / 2;
            double dx = centerX - Agent.X;
            double dy = centerY - Agent.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
 
            if (distance > 1)
            {
                double stepSize = 2.0;
                Agent.X += (dx / distance) * stepSize;
                Agent.Y += (dy / distance) * stepSize;
            }
            ";

        // Agents move in a straight line and bounce off the edges instead of wrapping a third visually distinct option.
        public const string BounceOffWalls = @"
            double stepSize = 2.0;
            double rad = Agent.Heading * Math.PI / 180;
            double nx = Agent.X + Math.Sin(rad) * stepSize;
            double ny = Agent.Y - Math.Cos(rad) * stepSize;
 
            bool bounced = false;
 
            if (nx < 0 || nx > World.Width)
            {
                Agent.Heading = (360 - Agent.Heading) % 360;
                bounced = true;
            }
 
            if (ny < 0 || ny > World.Height)
            {
                Agent.Heading = (180 - Agent.Heading + 360) % 360;
                bounced = true;
            }
 
            if (!bounced)
            {
                Agent.X = nx;
                Agent.Y = ny;
            }
            ";

        // Exposes all templates by name, for the UI to list in a dropdown
        public static IReadOnlyDictionary<string, string> All => new Dictionary<string, string>
        {
            ["Random Walk"] = RandomWalk,
            ["Flee to Center"] = FleeToCenter,
            ["Bounce off Walls"] = BounceOffWalls,
        };
    }
}
