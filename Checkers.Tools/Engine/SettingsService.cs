using System.Text.Json;
using System.Text.Json.Serialization;
using Checkers.Tools.Models;

namespace Checkers.Tools.Engine;

public sealed class AppSettings
{
    public Language Language { get; set; } = Language.English;
    public bool IsDarkTheme { get; set; } = true;
    public bool IsMaximized { get; set; } = true;
}

/// <summary>Stores the user settings as JSON. The default location is %AppData%\Checkers\settings.json.</summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string FilePath { get; }

    public SettingsService(string? filePath = null)
    {
        FilePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Checkers", "settings.json");
    }

    /// <summary>Returns the stored settings, or defaults when the file is missing, unreadable or invalid.</summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new AppSettings();
            }

            var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), Options);
            if (loaded == null || !Enum.IsDefined(loaded.Language))
            {
                return new AppSettings();
            }

            return loaded;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    /// <summary>Returns false when the settings could not be written.</summary>
    public bool TrySave(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var temp = FilePath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(settings, Options));
            File.Move(temp, FilePath, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
