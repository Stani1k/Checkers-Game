namespace Checkers.Tools.Engine;

public enum Side
{
    /// <summary>Starts on the lower rows, moves up the board and moves first.</summary>
    Bottom,
    /// <summary>Starts on the upper rows and moves down the board.</summary>
    Top
}

public enum GameStatus
{
    InProgress,
    BottomWins,
    TopWins,
    Draw
}

public readonly record struct Piece(Side Side, bool IsKing)
{
    public Piece Crowned() => this with { IsKing = true };
}
