using System.Windows;
using System.Windows.Controls;
using Syncfusion.SfSkinManager;

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

    private void RadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        var themeName = (sender as RadioButton)?.Name;
        switch (themeName)
        {
            case "LightMode":
                SfSkinManager.SetVisualStyle(Application.Current.MainWindow, VisualStyles.Windows11Light);
                break;
            case "DarkMode":
                SfSkinManager.SetVisualStyle(Application.Current.MainWindow, VisualStyles.Windows11Dark);
                break;
        }
    }
}