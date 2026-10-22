using System.Windows.Input;
using Checkers.Tools;
using Checkers.Tools.Engine;
using Checkers.WPF.Commands;

namespace Checkers.WPF.MainMenu;

public class MainMenuViewModel : ViewModelBase
{
    private readonly IGameSaveService _saveService;
    private bool _hasSavedGame;
    private string _errorMessage = string.Empty;

    public UpdateViewCommand UpdateViewCommand { get; set; }

    public bool HasSavedGame
    {
        get => _hasSavedGame;
        private set
        {
            if (_hasSavedGame == value) return;
            _hasSavedGame = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => _errorMessage.Length > 0;

    public MainMenuViewModel(UpdateViewCommand updateViewCommand, IGameSaveService saveService)
    {
        UpdateViewCommand = updateViewCommand;
        _saveService = saveService;
        RefreshSavedGame();
    }

    public void RefreshSavedGame() => HasSavedGame = _saveService.Exists;
}