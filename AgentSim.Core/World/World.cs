using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Agents;
// The simulation space. Owns the agent collection, the patch grid, and
// bounds/wrapping logic

namespace AgentSim.Core.Worlds
{
    public class World
    {
        public double Width { get; }
        public double Height { get; }

        private readonly List<Agent> _agents = new();
        public IReadOnlyList<Agent> Agents => _agents;

        public Patch[,]? Patches { get; private set; }

        // The value patches start at, and the value an empty patch regrows
        // to. Also what the canvas treats as "fully grown" when coloring.
        public double PatchMaxValue { get; private set; } = 100;

        public World(double width, double height)
        {
            Width = width;
            Height = height;
        }

        //Adds an agent to the world
        public void AddAgent(Agent agent) => _agents.Add(agent);

        //Removes an agent from the world
        public void RemoveAgent(Agent agent) => _agents.Remove(agent);

        //Removes all agents from the world
        public void Clear() => _agents.Clear();

        // Builds a patch grid of the given resolution over the world's bounds.
        // Every patch starts at initialValue, which is also the value an
        // empty patch regrows to (see RegrowPatches).
        public void InitializePatches(int columns, int rows, double initialValue = 100)
        {
            PatchMaxValue = initialValue;
            Patches = new Patch[columns, rows];
            double patchWidth = Width / columns;
            double patchHeight = Height / rows;

            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    Patches[col, row] = new Patch(col, row, col * patchWidth, row * patchHeight)
                    {
                        Value = initialValue
                    };
                }
            }
        }

        // Ticks patch regrowth. A patch that is empty (Value <= 0) counts the
        // ticks it has been empty; once that reaches regrowthTicks it is
        // restored to PatchMaxValue. regrowthTicks <= 0 disables regrowth
        // entirely. Call once per simulation tick.
        public void RegrowPatches(int regrowthTicks)
        {
            if (Patches == null || regrowthTicks <= 0) return;

            foreach (var patch in Patches)
            {
                if (patch.Value <= 0)
                {
                    patch.RegrowthTimer++;
                    if (patch.RegrowthTimer >= regrowthTicks)
                    {
                        patch.Value = PatchMaxValue;
                        patch.RegrowthTimer = 0;
                    }
                }
                else
                {
                    patch.RegrowthTimer = 0;
                }
            }
        }

        //Returns the patch under the given world-space position, or null if patches haven't been initialized
        public Patch? GetPatchAt(double x, double y)
        {
            if (Patches == null) return null;

            var (wx, wy) = Wrap(x, y);
            int columns = Patches.GetLength(0);
            int rows = Patches.GetLength(1);

            int col = (int)(wx / Width * columns);
            int row = (int)(wy / Height * rows);
            col = Math.Clamp(col, 0, columns - 1);
            row = Math.Clamp(row, 0, rows - 1);

            return Patches[col, row];
        }

        // Wraps a position around world bounds (torus topology). Always route
        // new agent positions through this so agents never leave the visible world.
        public (double X, double Y) Wrap(double x, double y)
        {
            double wrappedX = ((x % Width) + Width) % Width;
            double wrappedY = ((y % Height) + Height) % Height;
            return (wrappedX, wrappedY);
        }
    }
}