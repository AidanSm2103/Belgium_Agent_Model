using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace AgentSim.Wpf.Views
{
    /// A Popup that stays inside this app's window stack. The normal WPF Popup
    /// is "always on top" of every program on the screen; this one is only
    /// above its own app, so opening Notepad (or anything else) covers it.
    public class NonTopmostPopup : Popup
    {
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint flags);

        private Window? _parentWindow;

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ClearTopmost();

            // WPF re-applies "topmost" when the parent window changes state,
            // so clear it again each time.
            _parentWindow = Window.GetWindow(PlacementTarget ?? this);
            if (_parentWindow != null)
            {
                _parentWindow.Activated += ParentChanged;
                _parentWindow.Deactivated += ParentChanged;
                _parentWindow.LocationChanged += ParentChanged;
                _parentWindow.SizeChanged += ParentSizeChanged;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.Activated -= ParentChanged;
                _parentWindow.Deactivated -= ParentChanged;
                _parentWindow.LocationChanged -= ParentChanged;
                _parentWindow.SizeChanged -= ParentSizeChanged;
                _parentWindow = null;
            }
            base.OnClosed(e);
        }

        private void ParentChanged(object? sender, EventArgs e) => ClearTopmost();
        private void ParentSizeChanged(object sender, SizeChangedEventArgs e) => ClearTopmost();

        private void ClearTopmost()
        {
            try
            {
                if (Child != null && PresentationSource.FromVisual(Child) is HwndSource source)
                {
                    SetWindowPos(source.Handle, HWND_NOTOPMOST, 0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }
            }
            catch
            {
                // Cosmetic only: if it fails the popup just behaves as before.
            }
        }
    }
}

