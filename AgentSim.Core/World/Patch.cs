using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.World
{
    public class Patch
    {
        public int Column { get; }
        public int Row { get; }

        public double X { get; }
        public double Y { get; }

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
