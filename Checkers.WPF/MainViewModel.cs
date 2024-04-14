using System.Windows.Input;
using Checkers.Tools;
using Checkers.WPF.Commands;
using Checkers.WPF.GameMenu;
using Checkers.WPF.MainMenu;
using Checkers.WPF.Settings;

namespace Checkers.WPF;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _selectedViewModel;

    public MainMenuViewModel MainMenuViewModel { get; set; }
    public SettingsViewModel SettingsViewModel { get; set; }
    public GameMenuViewModel GameMenuViewModel { get; set; }

    public MainViewModel()
    {
        var updateViewCommand = new UpdateViewCommand(this);
        MainMenuViewModel = new MainMenuViewModel(updateViewCommand);
        SettingsViewModel = new SettingsViewModel(updateViewCommand);
        GameMenuViewModel = new GameMenuViewModel(updateViewCommand);
        _selectedViewModel = MainMenuViewModel;
    }
    
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