using System;
using System.Windows;
using System.Windows.Media;

namespace AgentSim.Wpf.Views
{
    public partial class ThemedMessageWindow : Window
    {
        public ThemedMessageWindow(string title, string message, DialogKind kind, bool showCancel, string okText)
        {
            InitializeComponent();

            Title = title;
            TitleText.Text = title;
            MessageText.Text = message;
            OkButton.Content = okText;
            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;

            (string glyph, string brushKey, string dimKey) = kind switch
            {
                DialogKind.Success => ("", "SuccessBrush", "SuccessDimBrush"),
                DialogKind.Warning => ("", "GoldBrush", "GoldDimBrush"),
                DialogKind.Error => ("", "RedBrush", "RedDimBrush"),
                DialogKind.Question => ("", "GoldBrush", "GoldDimBrush"),
                _ => ("", "GoldBrush", "GoldDimBrush")
            };

            IconText.Text = glyph;
            IconText.Foreground = (Brush)FindResource(brushKey);
            IconBadge.Background = (Brush)FindResource(dimKey);

            if (kind == DialogKind.Error)
            {
                OkButton.Style = (Style)FindResource("DangerButton");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Dialogs.ApplyDarkTitleBar(this);
        }

        private void Ok_Click(object sender, RoutedEventArgs e) => DialogResult = true;
        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
