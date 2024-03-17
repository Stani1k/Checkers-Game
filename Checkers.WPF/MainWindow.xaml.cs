using System.Windows;
using Syncfusion.SfSkinManager;

namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            SfSkinManager.SetTheme(this, new Theme("Windows11Dark"));
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        
        private void OpenSettings_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings menu currently not implemented!", "Feature not implemented", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}