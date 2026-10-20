using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using Checkers.Tools.Localization;
using Checkers.Tools.Models;

namespace Checkers.WPF.Localization;

/// <summary>Holds the active UI language. Bindings to the indexer refresh when the language changes.</summary>
public sealed class Loc : INotifyPropertyChanged
{
    private Language _language = Language.English;
    private IReadOnlyDictionary<string, string> _texts = Translations.For(Language.English);

    public static Loc Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    public Language Language
    {
        get => _language;
        set
        {
            if (_language == value) return;
            _language = value;
            _texts = Translations.For(value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string this[string key] =>
        _texts.TryGetValue(key, out var text) ? text
        : Translations.For(Language.English).TryGetValue(key, out var fallback) ? fallback
        : key;

    public static string Get(string key) => Instance[key];

    public static string Format(string key, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, Instance[key], args);
}
