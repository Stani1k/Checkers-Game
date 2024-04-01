using System;
using System.Collections.Generic;
using System.Linq;
using Checkers.Tools;
using Checkers.Tools.Models;

namespace Checkers.WPF.GameMenu;

public class GameMenuViewModel : ViewModelBase
{
    private string _timeLeft;
    private string _playerName;
    private ConsoleColor _checkerColor;
    private GameDirection _gameDirection;

    public List<ConsoleColor> CheckerColors { get; }
    public List<PlayerViewModel> Players { get; set; }
    
    public GameMenuViewModel()
    {
        _playerName = string.Empty;
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

    public ConsoleColor CheckerColor
    {
        get => _checkerColor;
        set
        {
            if (value == _checkerColor) return;
            _checkerColor = value;
            OnPropertyChanged();
        }
    }

    public GameDirection GameDirection
    {
        get => _gameDirection;
        set
        {
            if (value == _gameDirection) return;
            _gameDirection = value;
            OnPropertyChanged();
        }
    }
}