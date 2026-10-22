namespace Checkers.Tools.Engine;

public interface IGameSaveService
{
    string FilePath { get; }
    bool Exists { get; }
    void Save(GameSaveData data);
    bool TryLoad(out GameSaveData? data, out CheckersGame? game, out string error);
}
