using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AgentSim.Core.Agents;
using AgentSim.Core.Worlds;
using AgentSim.Core.Scripting;

namespace AgentSim.Wpf.Views
{
    public partial class WorldCanvasControl : UserControl
    {
        private World? _lastWorld;

        // Deterministic species -> color mapping. No species names are
        // hardcoded as special cases — any string gets consistently
        // mapped to one of these colors via a hash, so it works for
        // however many species a user actually creates.
        private static readonly Brush[] SpeciesPalette =
        {
            Brushes.DodgerBlue,
            Brushes.OrangeRed,
            Brushes.MediumSeaGreen,
            Brushes.MediumPurple,
            Brushes.Goldenrod,
            Brushes.DeepPink
        };

        public WorldCanvasControl()
        {
            InitializeComponent();
            SizeChanged += (s, e) =>
            {
                if (_lastWorld != null) DrawWorld(_lastWorld);
            };
        }

        private void ClearCanvas()
        {
            WorldCanvas.Children.Clear();
        }

        // Creates the visual representation of an agent
        private Ellipse CreateAgentShape(Agent agent)
        {
            Brush fill;

            if (agent.Species != "Default")
            {
                // Multi-species: color consistently by species name.
                int index = Math.Abs(agent.Species.GetHashCode()) % SpeciesPalette.Length;
                fill = SpeciesPalette[index];
            }
            else
            {
                // No species set: fall back to the original
                // scripted-vs-default distinction from Milestone 2.
                bool isScripted = agent.Behavior is ScriptedBehavior;
                fill = isScripted ? Brushes.OrangeRed : Brushes.DodgerBlue;
            }

            return new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = fill,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
        }

        // Draws a single agent
        private void DrawAgent(Agent agent, double scaleX, double scaleY)
        {
            if (agent == null || !agent.IsActive) return;

            Ellipse ellipse = CreateAgentShape(agent);

            double left = (agent.X * scaleX) - (ellipse.Width / 2);
            double top = (agent.Y * scaleY) - (ellipse.Height / 2);

            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            WorldCanvas.Children.Add(ellipse);
        }

        // Draws the patch grid as colored background cells, brown (empty)
        // to green (full) based on each patch's Value. Drawn BEFORE
        // agents so agents stay visible on top.
        private void DrawPatches(World world, double scaleX, double scaleY)
        {
            if (world.Patches == null) return;

            double cellWidth = (world.Width / world.Patches.GetLength(0)) * scaleX;
            double cellHeight = (world.Height / world.Patches.GetLength(1)) * scaleY;

            foreach (var patch in world.Patches)
            {
                // Scale against the world's configured full value instead of a
                // fixed 100, so custom starting values color correctly.
                double max = world.PatchMaxValue;
                double t = max > 0 ? Math.Clamp(patch.Value / max, 0.0, 1.0) : 0.0;

                // Interpolate brown (0) -> green (100)
                byte r = (byte)(139 + (34 - 139) * t);
                byte g = (byte)(69 + (139 - 69) * t);
                byte b = (byte)(19 + (34 - 19) * t);

                var rect = new Rectangle
                {
                    Width = cellWidth,
                    Height = cellHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(r, g, b))
                };

                Canvas.SetLeft(rect, patch.X * scaleX);
                Canvas.SetTop(rect, patch.Y * scaleY);
                WorldCanvas.Children.Add(rect);
            }
        }

        // Draws all agents currently in the world
        public void DrawWorld(World world)
        {
            _lastWorld = world;

            ClearCanvas();

            if (world == null) return;
            if (world.Width <= 0 || world.Height <= 0) return;
            if (WorldCanvas.ActualWidth <= 0 || WorldCanvas.ActualHeight <= 0) return;

            double scaleX = WorldCanvas.ActualWidth / world.Width;
            double scaleY = WorldCanvas.ActualHeight / world.Height;

            DrawPatches(world, scaleX, scaleY);

            foreach (Agent agent in world.Agents)
            {
                DrawAgent(agent, scaleX, scaleY);
            }
        }
    }
}