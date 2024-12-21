using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WpfWindowTest
{
    /*
     * This part of the code makes sure that the window expands correctly if it goes into fully maximized (Fullscreen) mode.
     * Without these modifications, the position of the upper left corner is negative (something like [-2;-2]) and the window is stretched below the taskbar (bottom bar of windows).
     */

    public partial class MainWindow
    {
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        /// <summary>
        /// This needs to be called in OnStateChanged - for fullscreen size to correctly work.
        /// </summary>
        private void RefreshMaxHeight()
        {
            if (WindowState == WindowState.Maximized)
            {
                // se the MaxHeight to the size of WorkingArea, where the window is
                var windowHandle = new WindowInteropHelper(this).Handle;
                var screen = System.Windows.Forms.Screen.FromHandle(windowHandle);

                // FullScreen is then limited by this and will not go behind the taskbar
                this.MaxHeight = screen.WorkingArea.Height / _lastScaleY;
            }
            else
            {
                // reset MaxHeight
                this.MaxHeight = double.PositiveInfinity;
            }
        }

        private double _lastScaleY;

        private IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFOEX monitorInfo = new MONITORINFOEX();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                    GetMonitorInfo(monitor, ref monitorInfo);

                    DEVMODE devMode = new DEVMODE();
                    devMode.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));
                    EnumDisplaySettings(monitorInfo.szDevice, -1 /*currentSettings*/, ref devMode);

                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;

                    var scaleX = Math.Abs((rcMonitorArea.Right - rcMonitorArea.Left) / (double)devMode.dmPelsWidth);
                    var scaleY = Math.Abs((rcMonitorArea.Top - rcMonitorArea.Bottom) / (double)devMode.dmPelsHeight);
                    _lastScaleY = scaleY;

                    mmi.ptMaxPosition.X = (int)(Math.Abs(rcWorkArea.Left - rcMonitorArea.Left) / scaleX);
                    mmi.ptMaxPosition.Y = (int)(Math.Abs(rcWorkArea.Top - rcMonitorArea.Top) / scaleY);
                    mmi.ptMaxSize.X = (int)(Math.Abs(rcWorkArea.Right - rcWorkArea.Left) / scaleX);
                    mmi.ptMaxSize.Y = (int)(Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top) / scaleY);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return IntPtr.Zero;
        }

        private const int WM_GETMINMAXINFO = 0x0024;

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hmonitor, ref MONITORINFOEX info);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplaySettings(char[] deviceName, int modeNum, ref DEVMODE devMode);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack=4)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;

            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }
    }
}
