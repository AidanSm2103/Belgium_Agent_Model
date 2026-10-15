using AgentSim.Wpf.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AgentSim.Wpf.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Whenever the engine ticks (or Setup() runs), redraw the canvas.
            _viewModel.Simulation.Engine.Ticked += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                    WorldCanvasCtrl.DrawWorld(_viewModel.Simulation.Engine.Worlds));
            };

            // Connects the plot's own Ticked subscription (handled inside
            // PlotControl.Connect) — without this call, PlotControl never
            // hears about ticks at all, which is why it was stuck at 0/0.
            PlotCtrl.Connect(_viewModel.Simulation.Engine);
        }

        // ---- Dark title bar -------------------------------------------------
        // Asks Windows to draw the title bar in dark mode (and, on Windows 11,
        // in the app's own background colour) so it blends into the window.
        // Purely cosmetic: if the call isn't supported it is silently ignored.

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;

                int dark = 1;
                // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (19 on older Windows 10 builds)
                if (DwmSetWindowAttribute(handle, 20, ref dark, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(handle, 19, ref dark, sizeof(int));
                }

                // 35 = DWMWA_CAPTION_COLOR (Windows 11). COLORREF is 0x00BBGGRR,
                // so this is the theme's #0C0F14 background.
                int caption = 0x00140F0C;
                DwmSetWindowAttribute(handle, 35, ref caption, sizeof(int));
            }
            catch
            {
                // Not supported on this version of Windows: keep the default title bar.
            }
        }
    }
}

