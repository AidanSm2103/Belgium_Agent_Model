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

        // Patch colours are quantised to 64 steps and cached as frozen brushes,
        // so a 50x50 grid no longer allocates 2500 brushes every tick.
        private static readonly Brush[] PatchBrushes = BuildPatchBrushes();

        private static Brush[] BuildPatchBrushes()
        {
            var brushes = new Brush[64];
            for (int i = 0; i < brushes.Length; i++)
            {
                double t = i / 63.0;
                byte r = (byte)(36 + (26 - 36) * t);
                byte g = (byte)(27 + (112 - 27) * t);
                byte b = (byte)(22 + (62 - 22) * t);
                brushes[i] = Freeze(new SolidColorBrush(Color.FromRgb(r, g, b)));
            }
            return brushes;
        }

        private static readonly Pen AgentPen = MakePen(AgentOutline, 1);
        private static readonly Pen HeadingPen = MakePen(HeadingBrush, 1.5);

        private static Pen MakePen(Brush brush, double thickness)
        {
            var pen = new Pen(brush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            pen.Freeze();
            return pen;
        }

        // Draws the whole world into ONE DrawingGroup shown by ONE Image.
        // WPF now keeps a single element instead of thousands of Rectangle /
        // Ellipse / Line objects, which is what made large runs lag.
        public void DrawWorld(World world)
        {
            _lastWorld = world;

            ClearCanvas();

            if (world == null) return;
            if (world.Width <= 0 || world.Height <= 0) return;
            if (WorldCanvas.ActualWidth <= 0 || WorldCanvas.ActualHeight <= 0) return;

            EmptyHint.Visibility = Visibility.Collapsed;

            double scale = Math.Min(WorldCanvas.ActualWidth / world.Width, WorldCanvas.ActualHeight / world.Height);
            double offsetX = (WorldCanvas.ActualWidth - world.Width * scale) / 2;
            double offsetY = (WorldCanvas.ActualHeight - world.Height * scale) / 2;

            var group = new DrawingGroup();
            using (DrawingContext dc = group.Open())
            {
                // Transparent full-canvas rect so the image bounds match the canvas.
                dc.DrawRectangle(Brushes.Transparent, null,
                    new Rect(0, 0, WorldCanvas.ActualWidth, WorldCanvas.ActualHeight));

                if (world.Patches != null)
                {
                    double cellWidth = (world.Width / world.Patches.GetLength(0)) * scale;
                    double cellHeight = (world.Height / world.Patches.GetLength(1)) * scale;
                    double max = world.PatchMaxValue;

                    foreach (var patch in world.Patches)
                    {
                        double t = max > 0 ? Math.Clamp(patch.Value / max, 0.0, 1.0) : 0.0;
                        var brush = PatchBrushes[(int)(t * 63 + 0.5)];
                        dc.DrawRectangle(brush, null,
                            new Rect(offsetX + patch.X * scale, offsetY + patch.Y * scale,
                                     cellWidth + 0.5, cellHeight + 0.5));
                    }
                }

                dc.DrawRectangle(Brushes.Transparent, new Pen(WorldFrameBrush, 1),
                    new Rect(offsetX, offsetY, world.Width * scale, world.Height * scale));

                double diameter = Math.Clamp(scale * 4.5, 7, 13);
                double radius = diameter / 2;
                bool headingMarks = world.Agents.Count <= MaxAgentsForHeadingMarks;

                foreach (Agent agent in world.Agents)
                {
                    if (agent == null || !agent.IsActive) continue;

                    double cx = offsetX + agent.X * scale;
                    double cy = offsetY + agent.Y * scale;
                    dc.DrawEllipse(FillFor(agent), AgentPen, new Point(cx, cy), radius, radius);

                    if (headingMarks)
                    {
                        double radians = agent.Heading * Math.PI / 180.0;
                        double reach = radius + 3;
                        dc.DrawLine(HeadingPen, new Point(cx, cy),
                            new Point(cx + Math.Sin(radians) * reach, cy - Math.Cos(radians) * reach));
                    }
                }
            }

            // Fix the drawing's bounds to the canvas. Without this, agents near
            // the edge stick out past it, the bounds change every tick, and
            // the Image re-aligns itself, which made the world jump up and down.
            group.ClipGeometry = new RectangleGeometry(
                new Rect(0, 0, WorldCanvas.ActualWidth, WorldCanvas.ActualHeight));

            var image = new Image
            {
                Source = new DrawingImage(group),
                Width = WorldCanvas.ActualWidth,
                Height = WorldCanvas.ActualHeight,
                Stretch = Stretch.Fill
            };
            WorldCanvas.Children.Add(image);
        }
    }
}