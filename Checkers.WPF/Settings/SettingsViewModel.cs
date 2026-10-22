using System;
using System.Collections.Generic;
using System.Windows;
using Checkers.Tools;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.WPF.Commands;
using Checkers.WPF.Localization;

namespace Checkers.WPF.Settings;

/// <summary>Applies each setting immediately and persists it.</summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly AppSettings _settings;

    public List<Language> Languages { get; } = new((Language[])Enum.GetValues(typeof(Language)));
    public UpdateViewCommand UpdateViewCommand { get; }

    public SettingsViewModel(UpdateViewCommand updateViewCommand, ISettingsService settingsService)
    {
        UpdateViewCommand = updateViewCommand;
        _settingsService = settingsService;
        _settings = settingsService.Load();
        Loc.Instance.Language = _settings.Language;
        ThemeManager.Apply(_settings.IsDarkTheme);
    }

    public Language SelectedLanguage
    {
        get => _settings.Language;
        set
        {
            if (value == _settings.Language) return;
            _settings.Language = value;
            Loc.Instance.Language = value;
            OnPropertyChanged();
            Save();
        }
    }

    public bool IsDarkTheme
    {
        get => _settings.IsDarkTheme;
        set
        {
            if (value == _settings.IsDarkTheme) return;
            _settings.IsDarkTheme = value;
            ThemeManager.Apply(value);
            OnPropertyChanged();
            Save();
        }
    }

    public bool IsMaximized
    {
        get => _settings.IsMaximized;
        set
        {
            if (value == _settings.IsMaximized) return;
            _settings.IsMaximized = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WindowState));
            Save();
        }
    }

    public WindowState WindowState => _settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;

    private void Save() => _settingsService.TrySave(_settings);
}
