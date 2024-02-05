using System.Windows.Input;
using Checkers.Tools;

namespace Checkers.WPF.MainMenu;

public class MainMenuViewModel : ViewModelBase
{
    public ICommand OpenSettingsCommand { get; set; }

    public MainMenuViewModel()
    {
        OpenSettingsCommand = new RoutedCommand();
    }
}