using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// A single cell in the world's patch grid.
// Value is the patch's resource level (e.g. grass): above 0 means "has resource",
// 0 means it has been used up. RegrowthTimer is bookkeeping for World.RegrowPatches.

namespace AgentSim.Core.Worlds
{
    public class Patch
    {
        public int Column { get; }
        public int Row { get; }

        // World-space X/Y of this patch's top-left corner
        public double X { get; }
        public double Y { get; }

        // Per-patch resource level. Scripts read and write this directly.
        public double Value { get; set; }

        // Ticks this patch has spent empty (Value <= 0). Managed by
        // World.RegrowPatches — scripts don't need to touch it.
        public int RegrowthTimer { get; set; }

        public Patch(int column, int row, double x, double y)
        {
            Column = column;
            Row = row;
            X = x;
            Y = y;
        }
    }
}