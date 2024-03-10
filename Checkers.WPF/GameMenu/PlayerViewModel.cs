using System.Collections.ObjectModel;
using Checkers.Tools;
using Checkers.Tools.Models;

namespace Checkers.WPF.GameMenu;

public class PlayerViewModel : ViewModelBase
{
    private string _playerName;
    private Player _player;

    public ObservableCollection<Checker> Checkers { get; set; }

    public PlayerViewModel(Player player)
    {
        _player = player;
        Checkers = new ObservableCollection<Checker>(_player.Checkers);
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