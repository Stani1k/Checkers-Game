using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace Checkers.WPF.Localization;

/// <summary>XAML usage: Text="{loc:T Menu.Play}". The text updates when the language changes.</summary>
public class TExtension : MarkupExtension
{
    public TExtension(string key)
    {
        Key = key;
    }

    [ConstructorArgument("key")]
    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = Loc.Instance,
            Mode = BindingMode.OneWay
        };
        return binding.ProvideValue(serviceProvider);
    }
}
