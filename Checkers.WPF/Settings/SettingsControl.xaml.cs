using System.Windows;
using System.Windows.Controls;

namespace Checkers.WPF.Settings;

public partial class SettingsControl : UserControl
{
    public SettingsControl()
    {
        InitializeComponent();
    }

    private void FullSizeCheckbox_OnChecked(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            parentWindow.WindowState = WindowState.Maximized;
        }
    }
}