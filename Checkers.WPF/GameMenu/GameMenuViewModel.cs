using System;
using System.Collections.Generic;
using System.Linq;
using Checkers.Tools;
using Checkers.Tools.Models;
using Checkers.WPF.Commands;
using Colors = Checkers.Tools.Models.Colors;

namespace Checkers.WPF.GameMenu;

public class GameMenuViewModel : ViewModelBase
{
    private string _timeLeft;

    public UpdateViewCommand UpdateViewCommand { get; }
    public List<PlayerViewModel> Players { get; set; }
    
    public GameMenuViewModel(UpdateViewCommand updateViewCommand)
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