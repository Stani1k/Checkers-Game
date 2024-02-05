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
}