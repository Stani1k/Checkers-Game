using Checkers.Tools.Engine;
using Checkers.Tools.Models;

namespace Checkers.Tools.Online;

/// <summary>Names and payloads shared by the multiplayer server and the clients.</summary>
public static class OnlineProtocol
{
    public const string HubPath = "/checkers";
    public const int MaxNameLength = 24;
    public const int CodeLength = 5;

    public const string GameStartedEvent = "GameStarted";
    public const string MoveAppliedEvent = "MoveApplied";
    public const string GameEndedEvent = "GameEnded";

    public const string ErrorNotFound = "NotFound";
    public const string ErrorFull = "Full";
    public const string ErrorAlreadyInGame = "AlreadyInGame";
    public const string ErrorNotInGame = "NotInGame";
    public const string ErrorNotStarted = "NotStarted";
    public const string ErrorNotYourTurn = "NotYourTurn";
    public const string ErrorIllegalMove = "IllegalMove";
    public const string ErrorGameOver = "GameOver";

    public static string NormalizeName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        return trimmed.Length > MaxNameLength ? trimmed[..MaxNameLength] : trimmed;
    }

    public static string NormalizeCode(string? code) => code?.Trim().ToUpperInvariant() ?? string.Empty;
}

public sealed record SquareDto(int Col, int Row)
{
    public Square ToSquare() => new(Col, Row);
    public static SquareDto From(Square square) => new(square.Col, square.Row);
}

public sealed record CreateGameResultDto(string Code);

/// <summary>Sent to each player when the second player has joined; <see cref="YourSide"/> differs per recipient.</summary>
public sealed record GameStartedDto(Variant Variant, Side YourSide, string BottomName, string TopName);

public sealed record MoveDto(List<SquareDto> Path)
{
    public static MoveDto From(Move move) => new(move.Path.Select(SquareDto.From).ToList());
    public List<Square> ToSquares() => Path.Select(p => p.ToSquare()).ToList();
}

public enum GameEndReason
{
    Resigned,
    OpponentLeft
}

public sealed record GameEndedDto(Side Winner, GameEndReason Reason);
