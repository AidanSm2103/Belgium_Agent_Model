using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AgentSim.Core.Agents;
using AgentSim.Core.Scripting;
using AgentSim.Core.Worlds;

namespace AgentSim.Wpf.Views
{
    public partial class WorldCanvasControl : UserControl
    {
        private World? _lastWorld;

        // Heading marks double the number of shapes drawn each tick, so they
        // are only drawn while the population is small enough to stay smooth.
        private const int MaxAgentsForHeadingMarks = 400;

        private static readonly Brush AgentOutline = Freeze(new SolidColorBrush(Color.FromArgb(200, 5, 7, 10)));
        private static readonly Brush HeadingBrush = Freeze(new SolidColorBrush(Color.FromArgb(190, 255, 255, 255)));
        private static readonly Brush WorldFrameBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x3A, 0x43, 0x54)));

        public WorldCanvasControl()
        {
            InitializeComponent();

            SizeChanged += (s, e) =>
            {
                // Rounded corners for the whole canvas, so patches never poke
                // out of the card's rounded edge.
                WorldCanvas.Clip = new RectangleGeometry(
                    new Rect(0, 0, WorldCanvas.ActualWidth, WorldCanvas.ActualHeight), 8, 8);

                if (_lastWorld != null) DrawWorld(_lastWorld);
            };
        }

        private static Brush Freeze(SolidColorBrush brush)
        {
            brush.Freeze();
            return brush;
        }

        private void ClearCanvas()
        {
            WorldCanvas.Children.Clear();
        }

        // Species colours come from SpeciesColors so the world, the live plot
        // and the Monte Carlo charts always agree.
        private static Brush FillFor(Agent agent)
        {
            if (agent.Species != "Default")
            {
                return SpeciesColors.BrushFor(agent.Species);
            }

            // No species set: fall back to the original
            // scripted-vs-default distinction from Milestone 2.
            return agent.Behavior is ScriptedBehavior
                ? SpeciesColors.ScriptedBrush
                : SpeciesColors.DefaultBrush;
        }

        // Draws a single agent
        private void DrawAgent(Agent agent, double scale, double offsetX, double offsetY, double diameter, bool headingMark)
        {
            if (agent == null || !agent.IsActive) return;

            var ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = FillFor(agent),
                Stroke = AgentOutline,
                StrokeThickness = 1
            };

            double cx = offsetX + agent.X * scale;
            double cy = offsetY + agent.Y * scale;

            Canvas.SetLeft(ellipse, cx - diameter / 2);
            Canvas.SetTop(ellipse, cy - diameter / 2);
            WorldCanvas.Children.Add(ellipse);

            if (headingMark)
            {
                // Heading 0 = up, increasing clockwise.
                double radians = agent.Heading * Math.PI / 180.0;
                double reach = diameter * 0.5 + 3;

                WorldCanvas.Children.Add(new Line
                {
                    X1 = cx,
                    Y1 = cy,
                    X2 = cx + Math.Sin(radians) * reach,
                    Y2 = cy - Math.Cos(radians) * reach,
                    Stroke = HeadingBrush,
                    StrokeThickness = 1.5,
                    StrokeEndLineCap = PenLineCap.Round
                });
            }
        }

        // Draws the patch grid as colored background cells, dark earth
        // (empty) to rich green (full) based on each patch's Value. Drawn
        // BEFORE agents so agents stay visible on top.
        private void DrawPatches(World world, double scale, double offsetX, double offsetY)
        {
            if (world.Patches == null) return;

            double cellWidth = (world.Width / world.Patches.GetLength(0)) * scale;
            double cellHeight = (world.Height / world.Patches.GetLength(1)) * scale;

            foreach (var patch in world.Patches)
            {
                // Scale against the world's configured full value instead of a
                // fixed 100, so custom starting values color correctly.
                double max = world.PatchMaxValue;
                double t = max > 0 ? Math.Clamp(patch.Value / max, 0.0, 1.0) : 0.0;

                // Interpolate dark earth (empty) -> deep green (full)
                byte r = (byte)(36 + (26 - 36) * t);
                byte g = (byte)(27 + (112 - 27) * t);
                byte b = (byte)(22 + (62 - 22) * t);

                var rect = new Rectangle
                {
                    // +0.5 closes the hairline gaps between neighbouring cells.
                    Width = cellWidth + 0.5,
                    Height = cellHeight + 0.5,
                    Fill = new SolidColorBrush(Color.FromRgb(r, g, b))
                };

                Canvas.SetLeft(rect, offsetX + patch.X * scale);
                Canvas.SetTop(rect, offsetY + patch.Y * scale);
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

            EmptyHint.Visibility = Visibility.Collapsed;

            // Keep the world's proportions: one scale for both axes, centred,
            // instead of stretching a square world into a wide rectangle.
            double scale = Math.Min(WorldCanvas.ActualWidth / world.Width, WorldCanvas.ActualHeight / world.Height);
            double offsetX = (WorldCanvas.ActualWidth - world.Width * scale) / 2;
            double offsetY = (WorldCanvas.ActualHeight - world.Height * scale) / 2;

            DrawPatches(world, scale, offsetX, offsetY);

            // Frame around the playable area, drawn over the patches.
            var frame = new Rectangle
            {
                Width = world.Width * scale,
                Height = world.Height * scale,
                Stroke = WorldFrameBrush,
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(frame, offsetX);
            Canvas.SetTop(frame, offsetY);
            WorldCanvas.Children.Add(frame);

            double diameter = Math.Clamp(scale * 4.5, 7, 13);
            bool headingMarks = world.Agents.Count <= MaxAgentsForHeadingMarks;

            foreach (Agent agent in world.Agents)
            {
                DrawAgent(agent, scale, offsetX, offsetY, diameter, headingMarks);
            }
        }
    }
}
