using System.Collections.Generic;
using Checkers.Tools;
using Checkers.Tools.Models;
using Checkers.WPF.Commands;

namespace Checkers.WPF.Players;

public class PlayerMenuViewModel : ViewModelBase
{
    private string _timeLeft;

    public UpdateViewCommand UpdateViewCommand { get; }
    public List<PlayerViewModel> Players { get; set; }
    public IReadOnlyList<Variant> Variants { get; } = new[] { Variant.American, Variant.European };

    public Variant SelectedVariant { get; set; } = Variant.American;
    
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
