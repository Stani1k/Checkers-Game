using System.Windows;
using System.Windows.Threading;

namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exceptionMessage = "An unhandled exception just occurred: " + e.Exception.Message;
            MessageBox.Show(exceptionMessage, "Unhandled Application Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}