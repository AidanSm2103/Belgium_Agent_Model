using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AgentSim.Wpf.Views
{
    public enum DialogKind { Info, Success, Warning, Error, Question }

    /// Themed replacements for MessageBox.Show. Messages only: file open/save
    /// still use the standard Windows dialogs.
    public static class Dialogs
    {
        private static Window? Owner
        {
            get
            {
                var w = Application.Current?.MainWindow;
                return w != null && w.IsLoaded && w.IsVisible ? w : null;
            }
        }

        public static void Info(string message, string title) => Show(message, title, DialogKind.Info, false, "OK");
        public static void Success(string message, string title) => Show(message, title, DialogKind.Success, false, "OK");
        public static void Warning(string message, string title) => Show(message, title, DialogKind.Warning, false, "OK");
        public static void Error(string message, string title) => Show(message, title, DialogKind.Error, false, "OK");

        public static bool Confirm(string message, string title, string okText = "Yes")
            => Show(message, title, DialogKind.Question, true, okText);

        private static bool Show(string message, string title, DialogKind kind, bool showCancel, string okText)
        {
            var window = new ThemedMessageWindow(title, message, kind, showCancel, okText);
            if (Owner != null) window.Owner = Owner;
            return window.ShowDialog() == true;
        }

        // ---- Dark title bar (same trick MainWindow uses) -------------------
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        public static void ApplyDarkTitleBar(Window window)
        {
            try
            {
                IntPtr handle = new WindowInteropHelper(window).Handle;
                int dark = 1;
                if (DwmSetWindowAttribute(handle, 20, ref dark, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(handle, 19, ref dark, sizeof(int));
                }
                int caption = 0x00140F0C; // #0C0F14 as 0x00BBGGRR
                DwmSetWindowAttribute(handle, 35, ref caption, sizeof(int));
            }
            catch
            {
                // Not supported on this Windows version: keep the default title bar.
            }
        }
    }
}
