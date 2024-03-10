using System.Collections.Generic;
using Checkers.Tools;

namespace Checkers.WPF.GameMenu;

public class GameMenuViewModel : ViewModelBase
{
    private string _timeLeft;

    public List<PlayerViewModel> Players { get; set; }
    
    public GameMenuViewModel()
    {
        _timeLeft = string.Empty;
        Players = new List<PlayerViewModel>();
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