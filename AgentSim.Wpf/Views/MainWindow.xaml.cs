using AgentSim.Wpf.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AgentSim.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Whenever the engine ticks (or Setup() runs, per the change
            // above), redraw the canvas. Wired here in code-behind rather
            // than inside the ViewModel, since the ViewModel shouldn't
            // reference WPF/View types directly.
            _viewModel.Simulation.Engine.Ticked += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                    WorldCanvasCtrl.DrawWorld(_viewModel.Simulation.Engine.Worlds));
            };
        }
    }
}
