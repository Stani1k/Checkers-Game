using System.Windows;
using System.Windows.Controls;

namespace Checkers.WPF.MainMenu;

public partial class MainMenuControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(MainMenuViewModel), typeof(MainMenuControl), new PropertyMetadata(default(MainMenuViewModel)));

    public MainMenuViewModel ViewModel
    {
        get => (MainMenuViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    
    public MainMenuControl()
    {
        InitializeComponent();
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