using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Microsoft.Win32;

namespace WpfWindowTest
{
    /*
     * This part of code provides the Windows 11 snap menu, when hovering maximize button.
     */

    public partial class MainWindow
    {
        // WINAPI:
        private const int WM_NCHITTEST = 0x0084;//InteropValues
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_NCLBUTTONUP = 0x00A2;
        private const int HTMAXBUTTON = 9;

        private float _scaleFactor;
        private HwndSource _hwndSource;

        #region Init/Deinit

        /// <summary>
        /// This needs to be called in this.Loaded event.
        /// </summary>
        private void SnapLayoutButton_Loaded(object sender, RoutedEventArgs e)
        {
            // This feature is only available on Windows 11+
            if (!IsWindows11OrGreater())
                return;

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

        private bool IsCursorOnButton(IntPtr lparam, FrameworkElement button)
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
            // Only for WindowsStyle = None
            if (WindowStyle != WindowStyle.None)
                return IntPtr.Zero;

            //https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-snap-layout-menu
            //https://github.com/dotnet/wpf/issues/4825
            switch (msg)
            {
                case WM_NCHITTEST:
                    if (IsCursorOnButton(lparam, BtnMaximize))
                    {
                        SetButtonState(BtnMaximize, isMouseOver: true);

                        handled = true;

                        return new IntPtr(HTMAXBUTTON);
                    }
                    else
                    {
                        SetButtonState(BtnMaximize, isMouseOver: false, isPressed: false);
                    }
                    break;

                case WM_NCLBUTTONDOWN:
                    if (IsCursorOnButton(lparam, BtnMaximize))
                    {
                        SetButtonState(BtnMaximize, isPressed: true);

                        handled = true;
                    }
                    break;

                case WM_NCLBUTTONUP:
                    if (IsCursorOnButton(lparam, BtnMaximize))
                    {
                        GetButtonState(BtnMaximize, out _, out var wasPressed);

                        SetButtonState(BtnMaximize, isPressed: false);

                        handled = true;

                        if (wasPressed == true)
                        {
                            // Fire click
                            BtnMaximize.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        #region Helpers

        // internal dependency properties
        private static DependencyPropertyKey s_uiElementIsMouseOverPropertyKey =
            (DependencyPropertyKey)typeof(UIElement).GetField("IsMouseOverPropertyKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);

        private static DependencyPropertyKey s_buttonIsPressedPropertyKey =
            (DependencyPropertyKey)typeof(ButtonBase).GetField("IsPressedPropertyKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);

        private static void GetButtonState(UIElement button, out bool isMouseOver, out bool? isPressed)
        {
            isMouseOver = (bool)button.GetValue(s_uiElementIsMouseOverPropertyKey.DependencyProperty);
            isPressed = null;
            if (button is ButtonBase)
            {
                isPressed = (bool)button.GetValue(s_buttonIsPressedPropertyKey.DependencyProperty);
            }
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

            if (button is FrameworkElement element)
            {
                // refresh actual states
                GetButtonState(button, out var mouseOver, out isPressed);
                isMouseOver = mouseOver;

                string state;
                if (isPressed == true)
                {
                    state = "Pressed";
                }
                else if (isMouseOver == true)
                {
                    state = "MouseOver";
                }
                else
                {
                    state = "Normal";
                }

                // apply visual state (for styling to work)
                VisualStateManager.GoToState(element, state, true);
            }
        }

        private static bool IsWindows11OrGreater()
        {
            uint.TryParse(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", "").ToString(), out uint number);
            return number >= 22000; // Windows 11 Original release (21H2)
        }

        #endregion
    }
}