using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.World
{
    internal class World
    {
        public double Width { get; }
        public double Height { get; }

        private readonly List<Agent> _agents = new();
        public IReadOnlyList<Agent> Agents => _agents;

        public Patch[,]? Patches { get; private set; }

        public World(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void AddAgent(Agent agent) => _agents.Add(agent);

        public void RemoveAgent(Agent agent) => _agents.Remove(agent);

        public void Clear() => _agents.Clear();

        public void InitializePatches(int columns, int rows)
        {
            Patches = new Patch[columns, rows];
            double patchWidth = Width / columns;
            double patchHeight = Height / rows;

            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    Patches[col, row] = new Patch(col, row, col * patchWidth, row * patchHeight);
                }
            }
        }

        public (double X, double Y) Wrap(double x, double y)
        {
            double wrappedX = ((x % Width) + Width) % Width;
            double wrappedY = ((y % Height) + Height) % Height;
            return (wrappedX, wrappedY);
        }
    }
}
