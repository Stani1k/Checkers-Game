using System;
using System.Windows;

namespace Checkers.WPF;

/// <summary>Swaps the colour dictionary used by the DynamicResource brushes.</summary>
public static class ThemeManager
{
    private static ResourceDictionary? _current;

    public static bool IsDark { get; private set; } = true;

    public static void Apply(bool dark)
    {
        var merged = Application.Current.Resources.MergedDictionaries;
        var next = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Checkers.WPF;component/Resources/Themes/{(dark ? "Dark" : "Light")}.xaml")
        };

        if (_current != null)
        {
            merged.Remove(_current);
        }

        merged.Add(next);
        _current = next;
        IsDark = dark;
    }
}

