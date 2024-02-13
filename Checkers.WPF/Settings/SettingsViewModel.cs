using System;
using System.Collections.Generic;
using Checkers.Tools;
using Checkers.Tools.Models;

namespace Checkers.WPF.Settings;

public class SettingsViewModel : ViewModelBase
{
    private Language _selectedLanguage;
    public List<Language> Languages { get; set; }
    
    public SettingsViewModel()
    {
        Languages = new List<Language>();
        var languages = Enum.GetValues(typeof(Language));
        foreach (var language in (Language[]) languages)
        {
            Languages.Add(language);
        }
    }
    
    public Language SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value == _selectedLanguage) return;
            _selectedLanguage = value;
            OnPropertyChanged();
        }
    }
}