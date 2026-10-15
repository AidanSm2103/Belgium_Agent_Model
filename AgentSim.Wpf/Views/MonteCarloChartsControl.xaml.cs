using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AgentSim.Core.Analysis;
using AgentSim.Wpf.ViewModels;

namespace AgentSim.Wpf.Views
{
    // Four charts of the latest Monte Carlo run, drawn with plain Canvas
    // shapes (same approach as PlotControl). DataContext is the
    // MonteCarloViewModel; the charts redraw when its Summary changes or the
    // control is resized.
    public partial class MonteCarloChartsControl : UserControl
    {
        private MonteCarloViewModel? _viewModel;

        public MonteCarloChartsControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            PopulationCanvas.SizeChanged += (_, _) => Redraw();
            HistogramCanvas.SizeChanged += (_, _) => Redraw();
            ScatterCanvas.SizeChanged += (_, _) => Redraw();
            GroupsCanvas.SizeChanged += (_, _) => Redraw();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null) _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            _viewModel = e.NewValue as MonteCarloViewModel;

            if (_viewModel != null) _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            Redraw();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MonteCarloViewModel.Summary)) Redraw();
        }

        private void Redraw()
        {
            PopulationCanvas.Children.Clear();
            HistogramCanvas.Children.Clear();
            ScatterCanvas.Children.Clear();
            GroupsCanvas.Children.Clear();

            var summary = _viewModel?.Summary;
            if (summary == null || summary.Trials.Count == 0)
            {
                EmptyPanel.Visibility = Visibility.Visible;
                return;
            }

            EmptyPanel.Visibility = Visibility.Collapsed;

            DrawPopulationPerTrial(summary);
            DrawHistogram(summary);
            DrawStartVsFinal(summary);
            DrawGroupComparison(summary);
        }

        // Chart 1: final population of every trial, stacked by species,
        // with the mean as a dashed line.
        private void DrawPopulationPerTrial(MonteCarloSummary summary)
        {
            var canvas = PopulationCanvas;
            var trials = summary.Trials;

            var yAxis = ChartAxis.Nice(0, Math.Max(1, trials.Max(t => t.FinalAgentCount)));
            var plot = ChartDrawing.BeginChart(canvas, "Final population per trial", "Trial", "Agents", yAxis, null);
            if (plot.IsEmpty) return;

            var primaryBrush = SpeciesColors.BrushFor(summary.PrimarySpecies);
            var secondaryBrush = SpeciesColors.BrushFor(summary.SecondarySpecies);

            double slot = plot.Width / trials.Count;
            double barWidth = Math.Max(1, slot * 0.8);
            double perAgent = plot.Height / (yAxis.Max - yAxis.Min);

            for (int i = 0; i < trials.Count; i++)
            {
                var t = trials[i];
                double x = plot.Left + i * slot + (slot - barWidth) / 2;
                double bottom = plot.Bottom;

                int other = Math.Max(0, t.FinalAgentCount - t.FinalPrimaryCount - t.FinalSecondaryCount);
                var segments = new (int Count, System.Windows.Media.Brush Color)[]
                {
                    (t.FinalPrimaryCount, primaryBrush),
                    (t.FinalSecondaryCount, secondaryBrush),
                    (other, ChartDrawing.OtherBrush)
                };

                foreach (var (count, color) in segments)
                {
                    if (count <= 0) continue;
                    double h = count * perAgent;
                    ChartDrawing.AddRect(canvas, x, bottom - h, barWidth, h, color);
                    bottom -= h;
                }
            }

            // Trial numbers at the two ends only, so they never overlap.
            ChartDrawing.AddText(canvas, trials[0].TrialIndex.ToString(), plot.Left + slot / 2, plot.Bottom + 4, 10, null, centered: true);
            if (trials.Count > 1)
            {
                ChartDrawing.AddText(canvas, trials[^1].TrialIndex.ToString(), plot.Right - slot / 2, plot.Bottom + 4, 10, null, centered: true);
            }

            double meanY = ChartDrawing.MapY(plot, summary.FinalTotal.Mean, yAxis);
            ChartDrawing.AddLine(canvas, plot.Left, meanY, plot.Right, meanY, ChartDrawing.MeanBrush, 1.5, dashed: true);
            ChartDrawing.AddText(canvas, $"mean {ChartDrawing.Format(summary.FinalTotal.Mean)}",
                plot.Left + 4, meanY - 14, 10, ChartDrawing.MeanBrush);

            var legend = new List<(string, System.Windows.Media.Brush)> { (summary.PrimarySpecies, primaryBrush) };
            if (summary.HasSecondarySpecies) legend.Add((summary.SecondarySpecies, secondaryBrush));
            ChartDrawing.AddLegend(canvas, plot, legend.ToArray());
        }

        // Chart 2: how often each final population size occurred.
        private void DrawHistogram(MonteCarloSummary summary)
        {
            var canvas = HistogramCanvas;
            var values = summary.Trials.Select(t => t.FinalAgentCount).ToList();

            int lo = values.Min();
            int hi = values.Max();
            int range = hi - lo + 1;                       // every whole count between lo and hi
            int binCount = Math.Max(1, Math.Min(Math.Min(range, 15), (int)Math.Ceiling(Math.Sqrt(values.Count)) + 2));
            double binWidth = (double)range / binCount;

            var counts = new int[binCount];
            foreach (int v in values)
            {
                int bin = Math.Min(binCount - 1, (int)((v - lo) / binWidth));
                counts[bin]++;
            }

            var yAxis = ChartAxis.Nice(0, Math.Max(1, counts.Max()));
            var plot = ChartDrawing.BeginChart(canvas, "Distribution of final population", "Final agent count", "Trials", yAxis, null);
            if (plot.IsEmpty) return;

            // A whole count v occupies the slot [v, v + 1), so its centre is v + 0.5.
            double ToX(double v) => plot.Left + (v - lo) / range * plot.Width;

            for (int b = 0; b < binCount; b++)
            {
                double x0 = ToX(lo + b * binWidth);
                double x1 = ToX(lo + (b + 1) * binWidth);
                double top = ChartDrawing.MapY(plot, counts[b], yAxis);

                ChartDrawing.AddRect(canvas, x0 + 1, top, Math.Max(1, x1 - x0 - 2), plot.Bottom - top, ChartDrawing.AccentBrush);
                if (counts[b] > 0)
                {
                    ChartDrawing.AddText(canvas, counts[b].ToString(), (x0 + x1) / 2, top - 14, 10, null, centered: true);
                }
            }

            ChartDrawing.AddText(canvas, lo.ToString(), ToX(lo + 0.5), plot.Bottom + 4, 10, null, centered: true);
            if (hi != lo)
            {
                ChartDrawing.AddText(canvas, hi.ToString(), ToX(hi + 0.5), plot.Bottom + 4, 10, null, centered: true);
            }

            double meanX = ToX(summary.FinalTotal.Mean + 0.5);
            ChartDrawing.AddLine(canvas, meanX, plot.Top, meanX, plot.Bottom, ChartDrawing.MeanBrush, 1.5, dashed: true);
            ChartDrawing.AddText(canvas, $"mean {ChartDrawing.Format(summary.FinalTotal.Mean)}",
                meanX + 4, plot.Top + 2, 10, ChartDrawing.MeanBrush);
        }

        // Chart 3: each trial as a dot, starting count against final count.
        private void DrawStartVsFinal(MonteCarloSummary summary)
        {
            var canvas = ScatterCanvas;
            var trials = summary.Trials;

            double xLo = trials.Min(t => t.StartAgentCount);
            double xHi = trials.Max(t => t.StartAgentCount);
            double yLo = trials.Min(t => t.FinalAgentCount);
            double yHi = trials.Max(t => t.FinalAgentCount);

            double xPad = Math.Max(1, (xHi - xLo) * 0.05);
            double yPad = Math.Max(1, (yHi - yLo) * 0.05);
            var xAxis = ChartAxis.Nice(xLo - xPad, xHi + xPad);
            var yAxis = ChartAxis.Nice(yLo - yPad, yHi + yPad);

            var plot = ChartDrawing.BeginChart(canvas, "Starting vs final population", "Starting agents", "Final agents", yAxis, xAxis);
            if (plot.IsEmpty) return;

            // The y = x line: a dot on it means the trial ended where it started.
            double lineLo = Math.Max(xAxis.Min, yAxis.Min);
            double lineHi = Math.Min(xAxis.Max, yAxis.Max);
            if (lineLo < lineHi)
            {
                ChartDrawing.AddLine(canvas,
                    ChartDrawing.MapX(plot, lineLo, xAxis), ChartDrawing.MapY(plot, lineLo, yAxis),
                    ChartDrawing.MapX(plot, lineHi, xAxis), ChartDrawing.MapY(plot, lineHi, yAxis),
                    ChartDrawing.OtherBrush, 1, dashed: true);
            }

            foreach (var t in trials)
            {
                ChartDrawing.AddDot(canvas,
                    ChartDrawing.MapX(plot, t.StartAgentCount, xAxis),
                    ChartDrawing.MapY(plot, t.FinalAgentCount, yAxis),
                    8, ChartDrawing.PointBrush);
            }

            ChartDrawing.AddText(canvas, $"r = {summary.StartFinalCorrelation:F2}", plot.Right - 6, plot.Top + 2, 10, null, rightAligned: true);

            if (xLo == xHi)
            {
                ChartDrawing.AddText(canvas, "All trials started with the same count.\nSet a start count variance above 0 to spread them.",
                    plot.Left + plot.Width / 2, plot.Top + 20, 10, ChartDrawing.OtherBrush, centered: true);
            }
        }

        // Chart 4: mean starting vs mean final population for each species
        // (and the total), with the 95% confidence interval on the final mean.
        private void DrawGroupComparison(MonteCarloSummary summary)
        {
            var canvas = GroupsCanvas;
            var trials = summary.Trials;

            var groups = new List<(string Label, double StartMean, SeriesStats Final, System.Windows.Media.Brush Color)>
            {
                (summary.PrimarySpecies, trials.Average(t => t.StartPrimaryCount), summary.FinalPrimary, SpeciesColors.BrushFor(summary.PrimarySpecies))
            };
            if (summary.HasSecondarySpecies)
            {
                groups.Add((summary.SecondarySpecies, trials.Average(t => t.StartSecondaryCount), summary.FinalSecondary, SpeciesColors.BrushFor(summary.SecondarySpecies)));
                groups.Add(("Total", summary.StartTotal.Mean, summary.FinalTotal, ChartDrawing.OtherBrush));
            }

            double top = groups.Max(g => Math.Max(g.StartMean, Math.Max(g.Final.Mean, g.Final.CiHigh)));
            var yAxis = ChartAxis.Nice(0, Math.Max(1, top));
            var plot = ChartDrawing.BeginChart(canvas, "Mean start vs mean final (95% CI)", "", "Agents", yAxis, null);
            if (plot.IsEmpty) return;

            double slot = plot.Width / groups.Count;
            double barWidth = Math.Min(60, slot * 0.28);

            for (int i = 0; i < groups.Count; i++)
            {
                var g = groups[i];
                double centre = plot.Left + slot * (i + 0.5);

                double startTop = ChartDrawing.MapY(plot, g.StartMean, yAxis);
                double finalTop = ChartDrawing.MapY(plot, g.Final.Mean, yAxis);

                ChartDrawing.AddRect(canvas, centre - barWidth - 2, startTop, barWidth, plot.Bottom - startTop, ChartDrawing.StartBrush);
                ChartDrawing.AddRect(canvas, centre + 2, finalTop, barWidth, plot.Bottom - finalTop, g.Color);

                // 95% confidence interval whisker on the final mean.
                double whiskerX = centre + 2 + barWidth / 2;
                double ciTop = ChartDrawing.MapY(plot, Math.Min(g.Final.CiHigh, yAxis.Max), yAxis);
                double ciBottom = ChartDrawing.MapY(plot, Math.Max(g.Final.CiLow, yAxis.Min), yAxis);
                ChartDrawing.AddLine(canvas, whiskerX, ciTop, whiskerX, ciBottom, ChartDrawing.TextBrush, 1.5);
                ChartDrawing.AddLine(canvas, whiskerX - 4, ciTop, whiskerX + 4, ciTop, ChartDrawing.TextBrush, 1.5);
                ChartDrawing.AddLine(canvas, whiskerX - 4, ciBottom, whiskerX + 4, ciBottom, ChartDrawing.TextBrush, 1.5);

                ChartDrawing.AddText(canvas, ChartDrawing.Format(Math.Round(g.StartMean, 1)), centre - barWidth / 2 - 2, startTop - 14, 10, null, centered: true);
                ChartDrawing.AddText(canvas, ChartDrawing.Format(Math.Round(g.Final.Mean, 1)), whiskerX, Math.Min(finalTop, ciTop) - 14, 10, null, centered: true);

                ChartDrawing.AddText(canvas, g.Label, centre, plot.Bottom + 4, 11, null, centered: true, bold: true);
            }

            ChartDrawing.AddLegend(canvas, plot,
                ("Mean start", ChartDrawing.StartBrush),
                ("Mean final", SpeciesColors.BrushFor(summary.PrimarySpecies)));
        }
    }
}
