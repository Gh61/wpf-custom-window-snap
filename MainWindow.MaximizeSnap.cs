using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace WpfWindowTest
{
    public partial class MainWindow
    {
        // WINAPI:
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 161;
        private const int HTMAXBUTTON = 9;

        // internal dependency properties
        private static DependencyPropertyKey s_uiElementIsMouseOverPropertyKey =
            (DependencyPropertyKey)typeof(UIElement).GetField("IsMouseOverPropertyKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);

        private static DependencyPropertyKey s_buttonIsPressedPropertyKey =
            (DependencyPropertyKey)typeof(ButtonBase).GetField("IsPressedPropertyKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);

        private float _scaleFactor;
        private HwndSource _hwndSource;

        #region Init/Deinit

        /// <summary>
        /// This needs to be called in this.Loaded event.
        /// </summary>
        private void SnapLayoutButton_Loaded(object sender, RoutedEventArgs e)
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            var dpi = 96.0 * source.CompositionTarget.TransformToDevice.M11;

            _scaleFactor = (float)(dpi / 96.0);
            _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (_hwndSource != null)
            {
                _hwndSource.AddHook(HwndSourceHook);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            this._hwndSource?.RemoveHook(HwndSourceHook);
            this._hwndSource = null;

            base.OnClosed(e);
        }

        #endregion

        private bool IsMouseOverFromHook(IntPtr lparam, FrameworkElement button)
        {
            // Extract mouse coordinates from lparam
            int mouseX = (short)(lparam.ToInt32() & 0xFFFF);
            int mouseY = (short)((lparam.ToInt32() >> 16) & 0xFFFF);

            // Get button's actual dimensions and position
            var buttonPosition = button.PointToScreen(new Point(0, 0));

            // Check if mouse coordinates are within the button bounds using a single return statement
            return mouseX >= buttonPosition.X && mouseX <= buttonPosition.X + button.ActualWidth * _scaleFactor &&
                   mouseY >= buttonPosition.Y && mouseY <= buttonPosition.Y + button.ActualHeight * _scaleFactor;
        }

        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_NCLBUTTONDOWN)
            {
                if (IsMouseOverFromHook(lparam, BtnMaximize))
                {
                    SetButtonState(BtnMaximize, isPressed: true);

                    BtnMaximize.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }

            if (msg == WM_NCHITTEST)
            {
                if (IsMouseOverFromHook(lparam, BtnMaximize))
                {
                    SetButtonState(BtnMaximize, isMouseOver: true);

                    handled = true;

                    return new IntPtr(HTMAXBUTTON);
                }
                else
                {
                    SetButtonState(BtnMaximize, isMouseOver: false, isPressed: false);
                }
            }

            return IntPtr.Zero;
        }

        private static void SetButtonState(UIElement button, bool? isMouseOver = null, bool? isPressed = null)
        {
            if (isMouseOver.HasValue)
            {
                button.SetValue(s_uiElementIsMouseOverPropertyKey, isMouseOver.Value);
            }

            if (isPressed.HasValue && button is ButtonBase)
            {
                button.SetValue(s_buttonIsPressedPropertyKey, isPressed.Value);
            }
        }
    }
}