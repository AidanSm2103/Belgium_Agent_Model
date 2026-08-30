using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AgentSim.Core.Simulation;

namespace AgentSim.Wpf.Views
{
    /// Displays simulation monitors and a live plot of agent count over time.
    public partial class PlotControl : UserControl
    {
        private readonly List<int> _agentCounts = new();

        private SimulationEngine? _engine;

        public PlotControl()
        {
            InitializeComponent();
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

        /// Updates the monitor values and redraws the plot.
        private void UpdatePlot()
        {
            if (_engine == null)
                return;

            int tickCount = _engine.TickCount;
            int agentCount = _engine.Worlds.Agents.Count;

            TickCountText.Text = tickCount.ToString();
            AgentCountText.Text = agentCount.ToString();

            if (_agentCounts.Count == 0 ||
                _agentCounts[^1] != agentCount)
            {
                _agentCounts.Add(agentCount);
            }

            DrawPlot();
        }

        /// Draws the agent-count plot.
        private void DrawPlot()
        {
            PlotCanvas.Children.Clear();

            if (_agentCounts.Count < 1)
                return;

            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            int maxAgents = Math.Max(1, _engine?.Settings.AgentCount ?? 1);

            Polyline line = new Polyline
            {
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 2
            };

            for (int i = 0; i < _agentCounts.Count; i++)
            {
                double x;

                if (_agentCounts.Count == 1)
                {
                    x = 0;
                }
                else
                {
                    x = i * (width / (_agentCounts.Count - 1));
                }

                double y = height - (_agentCounts[i] / (double)maxAgents * height);

                line.Points.Add(new Point(x, y));
            }

            PlotCanvas.Children.Add(line);
        }
    }
}