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

namespace AgentSim.Wpf.Views
{
    public partial class WorldCanvasControl : UserControl
    {
        private World? _lastWorld;

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

        ///<summary>
        /// Creates the visual representation of an agent
        /// </summary>
        private Ellipse CreateAgentShape()
        {
            return new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.DodgerBlue,
                Stroke = Brushes.Black, 
                StrokeThickness = 1
            };
        }

        /// <summary>
        /// Draws a single agent
        /// </summary>
        private void DrawAgent(Agent agent, double scaleX, double scaleY)
        {
            if (agent == null) return;

            Ellipse ellipse = CreateAgentShape();

            double left = (agent.X * scaleX) - (ellipse.Width / 2);
            double top = (agent.Y * scaleY) - (ellipse.Height / 2);

            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            WorldCanvas.Children.Add(ellipse);
        }

        ///<summary>
        /// Draws all agents ccurrently in the world
        /// </summary>
        public void DrawWorld(World world)
        {
            _lastWorld = world;

            ClearCanvas();

            if (world == null) return;
            if (world.Width <= 0 || world.Height <= 0) return;
            if (WorldCanvas.ActualWidth <= 0 || WorldCanvas.ActualHeight <= 0) return;

            double scaleX = WorldCanvas.ActualWidth / world.Width;
            double scaleY = WorldCanvas.ActualHeight / world.Height;

            foreach (Agent agent in world.Agents)
            {
                DrawAgent(agent, scaleX, scaleY);
            }
        }
    }
}


