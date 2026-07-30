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
    /// <summary>
    /// Interaction logic for WorldCanvasControl.xaml
    /// Responsible for rendering simulation world.
    /// </summary>
    public partial class WorldCanvasControl : UserControl
    {
        public WorldCanvasControl()
        {
            InitializeComponent();
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
        private void DrawAgent(Agent agent)
        {
            Ellipse ellipse = CreateAgentShape();

            Canvas.SetLeft(ellipse, agent.X);
            Canvas.SetTop(ellipse, agent.Y);

            WorldCanvas.Children.Add(ellipse);
        }

        ///<summary>
        /// Draws all agents ccurrently in the world
        /// </summary>
        public void DrawWorld(World world)
        {
            ClearCanvas();

            foreach (Agent agent in world.Agents)
            {
                DrawAgent(agent);
            }
        }
    }
}
