using System;
using System.Windows.Input;
using Checkers.Tools.Models;
using Checkers.WPF.Settings;

namespace Checkers.WPF.Commands;

public class UpdateViewCommand : ICommand
{
    private readonly MainViewModel _viewModel;

    public UpdateViewCommand(MainViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        if (parameter == null) return;
        if (parameter.ToString() == MenuOption.Settings.ToString())
        {
            _viewModel.SelectedViewModel = _viewModel.SettingsViewModel;
        }
        else if (parameter.ToString() == MenuOption.Play.ToString())
        {
            _viewModel.SelectedViewModel = _viewModel.PlayerMenuViewModel;
        }
        else if (parameter.ToString() == MenuOption.MainMenu.ToString())
        {
            _viewModel.MainMenuViewModel.ErrorMessage = string.Empty;
            _viewModel.MainMenuViewModel.RefreshSavedGame();
            _viewModel.SelectedViewModel = _viewModel.MainMenuViewModel;
        }
        else if (parameter.ToString() == MenuOption.ContinueGame.ToString())
        {
            if (_viewModel.GameViewModel.LoadSavedGame(out var error))
            {
                _viewModel.MainMenuViewModel.ErrorMessage = string.Empty;
                _viewModel.SelectedViewModel = _viewModel.GameViewModel;
            }
            else
            {
                _viewModel.MainMenuViewModel.ErrorMessage = error;
                _viewModel.MainMenuViewModel.RefreshSavedGame();
            }
        }
        else if (parameter.ToString() == MenuOption.StartGame.ToString())
        {
            var players = _viewModel.PlayerMenuViewModel;
            _viewModel.GameViewModel.StartNewGame(players.SelectedVariant, players.Players[0].PlayerName, players.Players[1].PlayerName);
            _viewModel.SelectedViewModel = _viewModel.GameViewModel;
        }
    }

    public event EventHandler? CanExecuteChanged;
}
