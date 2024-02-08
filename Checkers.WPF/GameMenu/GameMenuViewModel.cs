using Checkers.Tools;

namespace Checkers.WPF.GameMenu;

public class GameMenuViewModel : ViewModelBase
{
    private string _timeLeft;

    public GameMenuViewModel()
    {
        _timeLeft = string.Empty;
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