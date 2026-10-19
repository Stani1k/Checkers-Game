using Checkers.Tools;
using Checkers.WPF.Commands;

namespace Checkers.WPF.Rules;

public class RulesViewModel : ViewModelBase
{
    public UpdateViewCommand UpdateViewCommand { get; }

    public RulesViewModel(UpdateViewCommand updateViewCommand)
    {
        UpdateViewCommand = updateViewCommand;
    }
}
