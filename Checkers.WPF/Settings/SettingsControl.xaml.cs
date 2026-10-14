using System.Windows;
using System.Windows.Controls;

namespace Checkers.WPF.Settings;

public partial class SettingsControl : UserControl
{
    public SettingsControl()
    {
        InitializeComponent();
        (ThemeManager.IsDark ? DarkMode : LightMode).IsChecked = true;
    }

    private void FullSizeCheckbox_OnChecked(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            parentWindow.WindowState = WindowState.Maximized;
        }
    }

    private void RadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        var themeName = (sender as RadioButton)?.Name;
        ThemeManager.Apply(dark: themeName == "DarkMode");
    }
}

