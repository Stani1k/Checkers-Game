namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            // Creating the view model first loads the saved settings and applies theme and language.
            DataContext = new MainViewModel();
            InitializeComponent();
        }
    }
}
