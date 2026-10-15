using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AgentSim.Core.Simulation;

namespace AgentSim.Wpf.Views
{
    /// Live plot of the population over time: one line per species, plus a
    /// total line when there is more than one species.
    public partial class PlotControl : UserControl
    {
        private sealed class Sample
        {
            public int Tick;
            public int Total;
            public Dictionary<string, int> BySpecies = new();
        }

        // Cap so a very long run can't grow without limit.
        private const int MaxSamples = 20000;

        private readonly List<Sample> _history = new();
        private readonly List<string> _species = new();
        private SimulationEngine? _engine;

        public PlotControl()
        {
            InitializeComponent();
            SizeChanged += (s, e) => DrawPlot();
        }

        /// Connects the plot to the simulation engine.
        public void Connect(SimulationEngine engine)
        {
            if (_engine != null)
            {
                _engine.Ticked -= Engine_Ticked;
            }
            _engine = engine;
            _engine.Ticked += Engine_Ticked;
            UpdatePlot();
        }

        /// Called whenever the simulation produces a new tick.
        private void Engine_Ticked(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(UpdatePlot);
        }

        /// Records the current population and redraws.
        private void UpdatePlot()
        {
            if (_engine == null)
                return;

            int tick = _engine.TickCount;

            // A fresh Setup (tick 0), or a load that rewinds the clock, starts
            // a new history so points from a previous run never get mixed in.
            if (tick == 0 || (_history.Count > 0 && _history[^1].Tick > tick))
            {
                _history.Clear();
                _species.Clear();
            }

            var sample = new Sample { Tick = tick };
            foreach (var agent in _engine.Worlds.Agents)
            {
                if (!agent.IsActive) continue;

                sample.Total++;
                sample.BySpecies[agent.Species] = sample.BySpecies.GetValueOrDefault(agent.Species) + 1;
            }

            foreach (var name in sample.BySpecies.Keys)
            {
                if (!_species.Contains(name)) _species.Add(name);
            }

            // Setup can raise more than one event for the same tick.
            if (_history.Count > 0 && _history[^1].Tick == tick)
                _history[^1] = sample;
            else
                _history.Add(sample);

            if (_history.Count > MaxSamples)
                _history.RemoveRange(0, MaxSamples / 10);

            DrawPlot();
        }

        private Brush Themed(string key, Brush fallback) =>
            TryFindResource(key) as Brush ?? fallback;

        private void AddLegendItem(Brush colour, string text)
        {
            var item = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(16, 0, 0, 0) };
            item.Children.Add(new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = colour,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            item.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = Themed("TextSecondaryBrush", Brushes.Gray),
                VerticalAlignment = VerticalAlignment.Center
            });
            LegendPanel.Children.Add(item);
        }

        /// Draws the population plot.
        private void DrawPlot()
        {
            PlotCanvas.Children.Clear();
            LegendPanel.Children.Clear();

            if (_history.Count == 0)
            {
                EmptyText.Visibility = Visibility.Visible;
                return;
            }

            EmptyText.Visibility = Visibility.Collapsed;

            var latest = _history[^1];
            foreach (var name in _species)
            {
                AddLegendItem(SpeciesColors.BrushFor(name), $"{name}  {latest.BySpecies.GetValueOrDefault(name)}");
            }
            if (_species.Count > 1)
            {
                AddLegendItem(ChartDrawing.OtherBrush, $"Total  {latest.Total}");
            }

            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;
            if (width < 120 || height < 60)
                return;

            var plot = new Rect(40, 6, width - 40 - 14, height - 6 - 22);
            var yAxis = ChartAxis.Nice(0, Math.Max(1, _history.Max(s => s.Total)));

            // Gridlines and y labels
            int steps = (int)Math.Round((yAxis.Max - yAxis.Min) / yAxis.Step);
            for (int i = 0; i <= steps; i++)
            {
                double value = yAxis.Min + i * yAxis.Step;
                double y = ChartDrawing.MapY(plot, value, yAxis);
                ChartDrawing.AddLine(PlotCanvas, plot.Left, y, plot.Right, y, ChartDrawing.GridBrush);
                ChartDrawing.AddText(PlotCanvas, ChartDrawing.Format(value), plot.Left - 6, y - 8, 10, null, rightAligned: true);
            }

            // x axis: tick numbers at both ends
            int firstTick = _history[0].Tick;
            int lastTick = latest.Tick;
            ChartDrawing.AddText(PlotCanvas, firstTick.ToString(), plot.Left, plot.Bottom + 4, 10, null, centered: true);
            if (lastTick != firstTick)
            {
                ChartDrawing.AddText(PlotCanvas, $"Tick {lastTick}", plot.Right, plot.Bottom + 4, 10, null, centered: true);
            }

            double XFor(Sample s) =>
                lastTick == firstTick
                    ? plot.Left
                    : plot.Left + (s.Tick - firstTick) / (double)(lastTick - firstTick) * plot.Width;

            // Thin the data to roughly one point per pixel so long runs stay fast.
            int stride = Math.Max(1, _history.Count / Math.Max(1, (int)plot.Width));

            Point[] Series(Func<Sample, int> pick)
            {
                var pts = new List<Point>();
                for (int i = 0; i < _history.Count; i += stride)
                {
                    pts.Add(new Point(XFor(_history[i]), ChartDrawing.MapY(plot, pick(_history[i]), yAxis)));
                }
                if ((_history.Count - 1) % stride != 0)
                {
                    pts.Add(new Point(XFor(latest), ChartDrawing.MapY(plot, pick(latest), yAxis)));
                }
                return pts.ToArray();
            }

            // With several species the total is drawn first, thin and grey, behind them.
            if (_species.Count > 1)
            {
                AddLine(Series(s => s.Total), ChartDrawing.OtherBrush, 1.25, dashed: true);
            }

            foreach (var name in _species)
            {
                var points = Series(s => s.BySpecies.GetValueOrDefault(name));
                var colour = SpeciesColors.ColorFor(name);

                // A soft fill under the line when there is only one series.
                if (_species.Count == 1 && points.Length > 1)
                {
                    var area = new Polygon
                    {
                        Fill = new LinearGradientBrush(
                            Color.FromArgb(90, colour.R, colour.G, colour.B),
                            Color.FromArgb(0, colour.R, colour.G, colour.B),
                            90)
                    };
                    foreach (var p in points) area.Points.Add(p);
                    area.Points.Add(new Point(points[^1].X, plot.Bottom));
                    area.Points.Add(new Point(points[0].X, plot.Bottom));
                    PlotCanvas.Children.Add(area);
                }

                AddLine(points, SpeciesColors.BrushFor(name), 2, dashed: false);

                // Dot on the newest point
                if (points.Length > 0)
                {
                    var end = points[^1];
                    var dot = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = SpeciesColors.BrushFor(name),
                        Stroke = Themed("SurfaceBrush", Brushes.Black),
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(dot, end.X - 4);
                    Canvas.SetTop(dot, end.Y - 4);
                    PlotCanvas.Children.Add(dot);
                }
            }
        }

        private void AddLine(Point[] points, Brush stroke, double thickness, bool dashed)
        {
            if (points.Length < 2) return;

            var line = new Polyline
            {
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeLineJoin = PenLineJoin.Round,
                Points = new PointCollection(points)
            };
            if (dashed) line.StrokeDashArray = new DoubleCollection { 4, 3 };
            PlotCanvas.Children.Add(line);
        }
    }
}
