using Checkers.Tools;
using Checkers.WPF.GameMenu;
using Checkers.WPF.MainMenu;

namespace Checkers.WPF;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        MainMenuViewModel = new MainMenuViewModel();
        GameMenuViewModel = new GameMenuViewModel();
    }

    public MainMenuViewModel MainMenuViewModel { get; set; }
    public GameMenuViewModel GameMenuViewModel { get; set; }
}