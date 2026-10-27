using System;
using System.IO;
using System.Reflection;
using Checkers.Tools;
using Checkers.WPF.Commands;
using Serilog;

namespace Checkers.WPF.Notices;

public class NoticesViewModel : ViewModelBase
{
    private const string ResourceName = "THIRD-PARTY-NOTICES.md";

    public UpdateViewCommand UpdateViewCommand { get; }
    public string Text { get; }

    public NoticesViewModel(UpdateViewCommand updateViewCommand)
    {
        UpdateViewCommand = updateViewCommand;
        Text = LoadText();
    }

    private static string LoadText()
    {
        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            Log.Warning("Embedded resource {Name} not found", ResourceName);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            Log.Warning(ex, "Could not read {Name}", ResourceName);
        }

        return string.Empty;
    }
}
