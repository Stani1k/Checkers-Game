namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            // The view model is created before the window so it can load the saved settings and apply theme and language.
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
