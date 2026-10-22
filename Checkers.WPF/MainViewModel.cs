using System.Windows.Input;
using Checkers.Tools;
using Checkers.Tools.Engine;
using Checkers.WPF.Commands;
using Checkers.WPF.Game;
using Checkers.WPF.MainMenu;
using Checkers.WPF.Players;
using Checkers.WPF.Rules;
using Checkers.WPF.Settings;

namespace Checkers.WPF;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _selectedViewModel;

    public MainMenuViewModel MainMenuViewModel { get; set; }
    public SettingsViewModel SettingsViewModel { get; set; }
    public PlayerMenuViewModel PlayerMenuViewModel { get; set; }
    public GameViewModel GameViewModel { get; set; }
    public RulesViewModel RulesViewModel { get; set; }

    public MainViewModel(IGameSaveService saveService, ISettingsService settingsService)
    {
        var updateViewCommand = new UpdateViewCommand(this);
        MainMenuViewModel = new MainMenuViewModel(updateViewCommand, saveService);
        SettingsViewModel = new SettingsViewModel(updateViewCommand, settingsService);
        PlayerMenuViewModel = new PlayerMenuViewModel(updateViewCommand);
        GameViewModel = new GameViewModel(updateViewCommand, saveService);
        RulesViewModel = new RulesViewModel(updateViewCommand);
        _selectedViewModel = MainMenuViewModel;
    }
    
    public ViewModelBase? ViewBeforeRules { get; set; }

    public ViewModelBase? SelectedViewModel
    {
        get => _selectedViewModel;
        set
        {
            if (Equals(value, _selectedViewModel)) return;
            _selectedViewModel = value;
            OnPropertyChanged();
        }
    }
}