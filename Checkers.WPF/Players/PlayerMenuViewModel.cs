using System.Collections.Generic;
using Checkers.Tools;
using Checkers.WPF.Commands;

namespace Checkers.WPF.Players;

public class PlayerMenuViewModel : ViewModelBase
{
    private string _timeLeft;

    public UpdateViewCommand UpdateViewCommand { get; }
    public List<PlayerViewModel> Players { get; set; }
    
    public PlayerMenuViewModel(UpdateViewCommand updateViewCommand)
    {
        UpdateViewCommand = updateViewCommand;
        _timeLeft = string.Empty;
        Players = new List<PlayerViewModel> { new(1), new(2) };
    }

    public string TimeLeft
    {
        get => _timeLeft;
        set
        {
            if (value == _timeLeft) return;
            _timeLeft = value;
            OnPropertyChanged();
        }
    }
}