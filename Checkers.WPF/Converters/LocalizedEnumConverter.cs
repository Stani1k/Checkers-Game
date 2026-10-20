using System;
using System.Globalization;
using System.Windows.Data;

namespace Checkers.WPF.Converters;

/// <summary>Shows an enum value as the translated text "{ConverterParameter}.{value}", e.g. Variant.American.</summary>
public class LocalizedEnumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        Localization.Loc.Get($"{parameter}.{value}");

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
}
