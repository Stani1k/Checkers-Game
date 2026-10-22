namespace Checkers.Tools.Engine;

public interface ISettingsService
{
    string FilePath { get; }
    AppSettings Load();
    bool TrySave(AppSettings settings);
}
