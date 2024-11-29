using System;
using System.Windows;
using System.Windows.Shell;

namespace WpfWindowTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.CanResize; // Needed for WindowChrome to work (AeroSnap then works too)

            WindowChrome.SetWindowChrome(this, CreateChrome());
            Loaded += SnapLayoutButton_Loaded;
        }

        private static WindowChrome CreateChrome(bool isMaximized = false)
        {
            return new WindowChrome()
            {
                CaptionHeight = 20,
                // when maximized, the resize border prevent to take the window "down" using the very last few pixels
                // https://stackoverflow.com/questions/19280016/windowchrome-resizeborderthickness-issue
                ResizeBorderThickness = isMaximized 
                    ? new Thickness(0)
                    : SystemParameters.WindowResizeBorderThickness,
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                UseAeroCaptionButtons = false
            };
        }

        private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal
                ? WindowState.Maximized
                : WindowState.Normal;
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            RefreshMaxHeight();

            WindowChrome.SetWindowChrome(this, CreateChrome(WindowState == WindowState.Maximized));
        }
    }
}
