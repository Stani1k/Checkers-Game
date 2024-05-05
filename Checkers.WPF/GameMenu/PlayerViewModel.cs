using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Checkers.Tools;
using Checkers.Tools.Models;

namespace Checkers.WPF.GameMenu;

public class PlayerViewModel : ViewModelBase
{
    private string _playerName;
    private Colors _checkerColor;
    private GameDirection _gameDirection;
    private string _groupName;
    private readonly Player _player;

    public string PlayerNumberText { get; private set; }
    public IEnumerable<Colors> CheckerColors { get; }
    public ObservableCollection<Checker> Checkers { get; set; }

    public PlayerViewModel(int playerNumber)
    {
        _player = new Player();
        PlayerNumberText = $"Player {playerNumber}";
        CheckerColors = Enum.GetValues(typeof(Colors)).Cast<Colors>();
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
    
    public string GroupName
    {
        get => _groupName;
        set
        {
            if (value == _groupName) return;
            _groupName = value;
            OnPropertyChanged();
        }
    }
    
    public Colors CheckerColor
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