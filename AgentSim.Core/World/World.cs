using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Agents;

// The simulation space. Owns the agent collection, the patch grid, and bounds/wrapping logic

namespace AgentSim.Core.World
{
    internal class World
    {
        public double Width { get; }
        public double Height { get; }

        private readonly List<Agent> _agents = new();
        public IReadOnlyList<Agent> Agents => _agents;

        // Optional patch grid — not required for the random-walk MVP, available for later features (patch coloring, agent-environment interaction).
        public Patch[,]? Patches { get; private set; }

        public World(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void AddAgent(Agent agent) => _agents.Add(agent);

        public void RemoveAgent(Agent agent) => _agents.Remove(agent);

        public void Clear() => _agents.Clear();

        // Builds a patch grid of the given resolution over the world's bounds. Call this from SimulationEngine.Setup() only if/when patches are needed
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

        // Wraps a position around world bounds (torus topology)
        // Always route new agent positions through this so agents never leave the visible world
        public (double X, double Y) Wrap(double x, double y)
        {
            double wrappedX = ((x % Width) + Width) % Width;
            double wrappedY = ((y % Height) + Height) % Height;
            return (wrappedX, wrappedY);
        }
    }
}
