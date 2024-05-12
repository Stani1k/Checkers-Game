using Checkers.Tools;
using Checkers.WPF.Commands;

namespace Checkers.WPF.Game;

public class GameViewModel : ViewModelBase
{
    public UpdateViewCommand UpdateViewCommand { get; }
    
    /// <summary>
    /// Value is calculated based on Square size (defined in Code behind)
    /// </summary>
    public int RowHeightAndWidth => 640;

    public GameViewModel(UpdateViewCommand updateViewCommand)
    {
        UpdateViewCommand = updateViewCommand;
    }
}