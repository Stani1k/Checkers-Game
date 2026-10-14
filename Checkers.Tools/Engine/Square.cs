namespace Checkers.Tools.Engine;

/// <summary>
/// A board square. Row 0 is the top row of the board (the side that starts at the top).
/// </summary>
public readonly record struct Square(int Col, int Row)
{
    public Square Offset(int dCol, int dRow) => new(Col + dCol, Row + dRow);
}
