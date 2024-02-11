using System.Windows;

namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        
        private void Quit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenSettings_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings menu currently not implemented!", "Feature not implemented", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}