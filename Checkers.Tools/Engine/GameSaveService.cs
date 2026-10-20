using System.Text.Json;
using System.Text.Json.Serialization;
using Checkers.Tools.Models;
using Serilog;

namespace Checkers.Tools.Engine;

public sealed class SavedSquare
{
    public int Col { get; set; }
    public int Row { get; set; }
}

public sealed class SavedPiece
{
    public int Col { get; set; }
    public int Row { get; set; }
    public Side Side { get; set; }
    public bool IsKing { get; set; }
}

/// <summary>Serializable state of a game in progress, including the player names.</summary>
public sealed class GameSaveData
{
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;
    public Variant Variant { get; set; }
    public Side Turn { get; set; }
    public int QuietPlies { get; set; }
    public string BottomPlayerName { get; set; } = string.Empty;
    public string TopPlayerName { get; set; } = string.Empty;
    public List<SavedPiece> Pieces { get; set; } = new();
    public List<SavedSquare> LastMovePath { get; set; } = new();
    public List<SavedSquare> LastMoveCaptured { get; set; } = new();

    public static GameSaveData From(CheckersGame game, string bottomPlayerName, string topPlayerName) => new()
    {
        Variant = game.Variant,
        Turn = game.Turn,
        QuietPlies = game.QuietPlies,
        BottomPlayerName = bottomPlayerName,
        TopPlayerName = topPlayerName,
        Pieces = game.GetPieces()
            .Select(p => new SavedPiece { Col = p.Square.Col, Row = p.Square.Row, Side = p.Piece.Side, IsKing = p.Piece.IsKing })
            .ToList(),
        LastMovePath = game.LastMove?.Path.Select(ToSaved).ToList() ?? new List<SavedSquare>(),
        LastMoveCaptured = game.LastMove?.Captured.Select(ToSaved).ToList() ?? new List<SavedSquare>()
    };

    /// <summary>Rebuilds the game; throws <see cref="ArgumentException"/> when the data describes an invalid position.</summary>
    public CheckersGame ToGame()
    {
        if (!Enum.IsDefined(Variant) || !Enum.IsDefined(Turn) || Pieces.Any(p => !Enum.IsDefined(p.Side)))
        {
            throw new ArgumentException("Unknown variant or side in save data.");
        }

        var squares = Pieces.Select(p => (p.Col, p.Row)).ToList();
        if (squares.Distinct().Count() != squares.Count)
        {
            throw new ArgumentException("Two pieces share a square.");
        }

        Move? lastMove = LastMovePath.Count >= 2
            ? new Move(LastMovePath.Select(FromSaved).ToList(), LastMoveCaptured.Select(FromSaved).ToList())
            : null;

        return CheckersGame.FromPieces(
            Variant,
            Turn,
            Pieces.Select(p => (new Square(p.Col, p.Row), new Piece(p.Side, p.IsKing))),
            QuietPlies,
            lastMove);
    }

    private static SavedSquare ToSaved(Square s) => new() { Col = s.Col, Row = s.Row };
    private static Square FromSaved(SavedSquare s) => new(s.Col, s.Row);
}

/// <summary>Stores a single saved game as JSON. The default location is %AppData%\Checkers\savegame.json.</summary>
public sealed class GameSaveService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string FilePath { get; }

    public GameSaveService(string? filePath = null)
    {
        FilePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Checkers", "savegame.json");
    }

    public bool Exists => File.Exists(FilePath);

    public void Save(GameSaveData data)
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, Options));
        File.Move(temp, FilePath, overwrite: true);
        Log.ForContext<GameSaveService>().Information("Game saved to {Path}", FilePath);
    }

    /// <summary>Returns false and a user-readable error when there is no save or it is unreadable or invalid.</summary>
    public bool TryLoad(out GameSaveData? data, out CheckersGame? game, out string error)
    {
        data = null;
        game = null;
        error = string.Empty;
        var log = Log.ForContext<GameSaveService>();
        try
        {
            if (!File.Exists(FilePath))
            {
                error = "No saved game was found.";
                log.Information("No saved game at {Path}", FilePath);
                return false;
            }

            var loaded = JsonSerializer.Deserialize<GameSaveData>(File.ReadAllText(FilePath), Options);
            if (loaded == null)
            {
                error = "The saved game file is empty.";
                return false;
            }

            if (loaded.Version != GameSaveData.CurrentVersion)
            {
                error = $"The saved game uses an unsupported format (version {loaded.Version}).";
                return false;
            }

            game = loaded.ToGame();
            data = loaded;
            return true;
        }
        catch (JsonException ex)
        {
            error = "The saved game file is corrupted and cannot be read.";
            log.Warning(ex, "Corrupted save file {Path}", FilePath);
        }
        catch (ArgumentException ex)
        {
            error = $"The saved game contains an invalid position: {ex.Message}";
            log.Warning(ex, "Invalid position in save file {Path}", FilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            error = $"The saved game could not be opened: {ex.Message}";
            log.Error(ex, "Could not open save file {Path}", FilePath);
        }

        return false;
    }
}