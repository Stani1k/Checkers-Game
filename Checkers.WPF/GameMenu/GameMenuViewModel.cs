using System;
using System.Collections.Generic;
using System.Linq;
using Checkers.Tools;

namespace Checkers.WPF.GameMenu;

public class GameMenuViewModel : ViewModelBase
{
    private string _timeLeft;
    private string _playerName;

    public List<ConsoleColor> CheckerColors { get; }
    public List<PlayerViewModel> Players { get; set; }
    
    public GameMenuViewModel()
    {
        _timeLeft = string.Empty;
        Players = new List<PlayerViewModel>();
        CheckerColors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToList();
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

    public string PlayerName
    {
        get => _playerName;
        set
        {
            if (value == _playerName) return;
            _playerName = value;
            OnPropertyChanged();
        }
    }
}