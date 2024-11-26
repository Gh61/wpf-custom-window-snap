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
            ResizeMode = ResizeMode.NoResize;

            WindowChrome.SetWindowChrome(this, CreateChrome());

            //BarTitle.MouseLeftButtonDown += (s, a) => DragMove();
        }

        private static WindowChrome CreateChrome()
        {
            return new WindowChrome()
            {
                CaptionHeight = 20,
                ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness,
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                UseAeroCaptionButtons = false
            };
        }

        private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState == WindowState.Normal
                ? WindowState.Maximized
                : WindowState.Normal;
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
