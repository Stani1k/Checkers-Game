using Checkers.Tools;
using Checkers.WPF.Commands;

namespace Checkers.WPF.Game;

public class GameViewModel : ViewModelBase
{
    public UpdateViewCommand UpdateViewCommand { get; }

    public GameViewModel(UpdateViewCommand updateViewCommand)
    {
        UpdateViewCommand = updateViewCommand;
    }
}