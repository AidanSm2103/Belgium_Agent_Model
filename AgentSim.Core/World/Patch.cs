using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// A single cell in the world's optional patch grid 
// Not required for the random-walk MVP — World works fine with Patches left Null
// This exists so patch-based features (coloring, resource values, agent-environment interaction) can be added later without restructuring World or SimulationEngine

namespace AgentSim.Core.World
{
    public class Patch
    {
        public int Column { get; }
        public int Row { get; }

        // World-space X/Y of this patch's top-left corner
        public double X { get; }
        public double Y { get; }

        // Generic slot for a per-patch value(e.g.resource level, terrain type)
        // Left as a simple double for now — extend later if patches need richer state
        public double Value { get; set; }

        public Patch(int column, int row, double x, double y)
        {
            Column = column;
            Row = row;
            X = x;
            Y = y;
        }
    }
}
