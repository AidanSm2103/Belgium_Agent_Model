using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AgentSim.Wpf.Views
{
    // A numeric axis: where it starts, where it ends, and the gap between
    // gridlines.
    internal readonly record struct ChartAxis(double Min, double Max, double Step)
    {
        // Builds an axis that covers lo..hi with round-numbered gridlines.
        public static ChartAxis Nice(double lo, double hi)
        {
            if (hi - lo < 1e-9)
            {
                lo -= 1;
                hi += 1;
            }

            double step = NiceStep((hi - lo) / 4.0);
            double niceLo = Math.Floor(lo / step) * step;
            double niceHi = Math.Ceiling(hi / step) * step;
            return new ChartAxis(niceLo, niceHi, step);
        }

        private static double NiceStep(double raw)
        {
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double f = raw / magnitude;
            double nice = f <= 1 ? 1 : f <= 2 ? 2 : f <= 2.5 ? 2.5 : f <= 5 ? 5 : 10;
            return nice * magnitude;
        }
    }

    // Minimal drawing helpers for the Monte Carlo charts. Everything is plain
    // WPF shapes on a Canvas (the same approach as PlotControl), so no chart
    // library is needed.
    internal static class ChartDrawing
    {
        // Colours for the dark theme. Species colours come from SpeciesColors;
        // the gold accent matches the app theme.
        public static readonly Brush TextBrush = Frozen(Color.FromRgb(0xC3, 0xCB, 0xD8));
        public static readonly Brush GridBrush = Frozen(Color.FromRgb(0x23, 0x2A, 0x35));
        public static readonly Brush AxisBrush = Frozen(Color.FromRgb(0x5A, 0x64, 0x75));
        public static readonly Brush PrimaryBrush = Frozen(Color.FromRgb(0x4F, 0xC3, 0xF7));
        public static readonly Brush SecondaryBrush = Frozen(Color.FromRgb(0xFF, 0x6B, 0x6B));
        public static readonly Brush OtherBrush = Frozen(Color.FromRgb(0x7B, 0x85, 0x96));
        public static readonly Brush StartBrush = Frozen(Color.FromRgb(0x59, 0x62, 0x73));
        public static readonly Brush AccentBrush = Frozen(Color.FromRgb(0xFD, 0xDA, 0x24));
        public static readonly Brush MeanBrush = Frozen(Color.FromRgb(0xEE, 0xF1, 0xF6));
        public static readonly Brush PointBrush = Frozen(Color.FromArgb(200, 0xFD, 0xDA, 0x24));

        private static Brush Frozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public static string Format(double value) => value.ToString("0.##", CultureInfo.CurrentCulture);

        public static double MapX(Rect plot, double value, ChartAxis axis) =>
            plot.Left + (value - axis.Min) / (axis.Max - axis.Min) * plot.Width;

        public static double MapY(Rect plot, double value, ChartAxis axis) =>
            plot.Bottom - (value - axis.Min) / (axis.Max - axis.Min) * plot.Height;

        public static TextBlock AddText(
            Canvas canvas, string text, double x, double y, double fontSize = 11,
            Brush? brush = null, bool centered = false, bool rightAligned = false, bool bold = false)
        {
            var block = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = brush ?? TextBrush,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
            };

            block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double width = block.DesiredSize.Width;
            if (centered) x -= width / 2;
            else if (rightAligned) x -= width;

            Canvas.SetLeft(block, x);
            Canvas.SetTop(block, y);
            canvas.Children.Add(block);
            return block;
        }

        public static void AddLine(
            Canvas canvas, double x1, double y1, double x2, double y2,
            Brush stroke, double thickness = 1, bool dashed = false)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = stroke,
                StrokeThickness = thickness
            };
            if (dashed) line.StrokeDashArray = new DoubleCollection { 4, 3 };
            canvas.Children.Add(line);
        }

        public static void AddRect(Canvas canvas, double x, double y, double width, double height, Brush fill)
        {
            if (height <= 0 || width <= 0) return;

            var rect = new Rectangle { Width = width, Height = height, Fill = fill };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            canvas.Children.Add(rect);
        }

        public static void AddDot(Canvas canvas, double cx, double cy, double diameter, Brush fill)
        {
            var dot = new Ellipse { Width = diameter, Height = diameter, Fill = fill };
            Canvas.SetLeft(dot, cx - diameter / 2);
            Canvas.SetTop(dot, cy - diameter / 2);
            canvas.Children.Add(dot);
        }

        // Draws the title, gridlines, axis labels and (optionally) numeric x
        // ticks, and returns the rectangle the data should be drawn inside.
        // Returns Rect.Empty when the canvas is too small to draw anything.
        public static Rect BeginChart(
            Canvas canvas, string title, string xLabel, string yLabel,
            ChartAxis yAxis, ChartAxis? xAxis)
        {
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width < 140 || height < 110) return Rect.Empty;

            var plot = new Rect(48, 26, width - 48 - 16, height - 26 - 44);

            AddText(canvas, title, width / 2, 4, 12, null, centered: true, bold: true);

            int ySteps = (int)Math.Round((yAxis.Max - yAxis.Min) / yAxis.Step);
            for (int i = 0; i <= ySteps; i++)
            {
                double value = yAxis.Min + i * yAxis.Step;
                double y = MapY(plot, value, yAxis);
                AddLine(canvas, plot.Left, y, plot.Right, y, GridBrush);
                AddText(canvas, Format(value), plot.Left - 4, y - 8, 10, null, rightAligned: true);
            }

            if (xAxis.HasValue)
            {
                var xa = xAxis.Value;
                int xSteps = (int)Math.Round((xa.Max - xa.Min) / xa.Step);
                for (int i = 0; i <= xSteps; i++)
                {
                    double value = xa.Min + i * xa.Step;
                    double x = MapX(plot, value, xa);
                    AddLine(canvas, x, plot.Bottom, x, plot.Bottom + 3, AxisBrush);
                    AddText(canvas, Format(value), x, plot.Bottom + 4, 10, null, centered: true);
                }
            }

            AddLine(canvas, plot.Left, plot.Top, plot.Left, plot.Bottom, AxisBrush);
            AddLine(canvas, plot.Left, plot.Bottom, plot.Right, plot.Bottom, AxisBrush);

            AddText(canvas, xLabel, plot.Left + plot.Width / 2, height - 18, 11, null, centered: true);

            // Rotated y-axis label, centred against the plot.
            var yText = new TextBlock
            {
                Text = yLabel,
                FontSize = 11,
                Foreground = TextBrush,
                LayoutTransform = new RotateTransform(-90)
            };
            yText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(yText, 2);
            Canvas.SetTop(yText, plot.Top + plot.Height / 2 - yText.DesiredSize.Height / 2);
            canvas.Children.Add(yText);

            return plot;
        }

        // A small colour key stacked in the top-right corner of the plot.
        public static void AddLegend(Canvas canvas, Rect plot, params (string Label, Brush Color)[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                double y = plot.Top + 2 + i * 15;
                var text = AddText(canvas, items[i].Label, plot.Right - 6, y, 10, null, rightAligned: true);
                AddRect(canvas, plot.Right - 6 - text.DesiredSize.Width - 14, y + 3, 9, 9, items[i].Color);
            }
        }
    }
}
