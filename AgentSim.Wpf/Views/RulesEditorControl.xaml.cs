using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AgentSim.Wpf.Views
{
    public partial class RulesEditorControl : UserControl
    {
        public RulesEditorControl()
        {
            InitializeComponent();

            // Center the popup over this control's position in the window,
            // rather than the screen default.
            ExpandedPopup.PlacementTarget = this;
            ExpandedPopup.Placement = PlacementMode.Center;
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandedPopup.IsOpen = true;
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandedPopup.IsOpen = false;
        }
    }
}