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
            _viewModel.SelectedViewModel = new SettingsViewModel();
        }
    }

    public event EventHandler? CanExecuteChanged;
}