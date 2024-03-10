using System.Windows;
using System.Windows.Controls;

namespace Checkers.WPF.MainMenu;

public partial class MainMenuControl : UserControl
{
    public MainMenuControl()
    {
        InitializeComponent();
    }
    
    private void Quit_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}