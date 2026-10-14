namespace Checkers.Tools.Engine;

/// <summary>
/// A complete move of one piece: the squares it visits (start, then every landing square) and the squares of captured pieces.
/// </summary>
public sealed record Move(IReadOnlyList<Square> Path, IReadOnlyList<Square> Captured)
{
    public Square From => Path[0];
    public Square To => Path[^1];
    public bool IsCapture => Captured.Count > 0;

    public bool HasSamePath(IReadOnlyList<Square> other) => Path.SequenceEqual(other);
}
