using System.Windows;

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

            BarTitle.MouseLeftButtonDown += (s, a) => DragMove();
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
