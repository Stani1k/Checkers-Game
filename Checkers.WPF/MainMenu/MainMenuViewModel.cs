using System.Windows.Input;
using Checkers.Tools;
using Checkers.WPF.Commands;

namespace Checkers.WPF.MainMenu;

public class MainMenuViewModel : ViewModelBase
{
    public UpdateViewCommand UpdateViewCommand { get; set; }

    public MainMenuViewModel(UpdateViewCommand updateViewCommand)
    {
        UpdateViewCommand = updateViewCommand;
    }
}