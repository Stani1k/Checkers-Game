using System.Collections.Generic;
using System.Linq;
using Checkers.Tools.Engine;

namespace Checkers.WPF.Game;

/// <summary>Tracks the squares a player has clicked so far while building a (possibly multi-jump) move.</summary>
public sealed class MoveSelection
{
    private readonly List<Square> _path = new();

    public IReadOnlyList<Square> Path => _path;

    public void Clear() => _path.Clear();

    public bool Contains(Square square) => _path.Contains(square);

    /// <summary>Squares the selected piece can continue to from the current partial path.</summary>
    public HashSet<Square> Targets(IReadOnlyList<Move> moves) =>
        _path.Count == 0
            ? new HashSet<Square>()
            : moves.Where(m => StartsWith(m.Path, _path) && m.Path.Count > _path.Count)
                   .Select(m => m.Path[_path.Count])
                   .ToHashSet();

    /// <summary>Handles a click and returns the completed move, if the click finished one.</summary>
    public Move? Click(Square square, IReadOnlyList<Move> moves)
    {
        if (_path.Count > 0 && Targets(moves).Contains(square))
        {
            _path.Add(square);
            var finished = moves.FirstOrDefault(m => m.HasSamePath(_path));
            if (finished != null)
            {
                _path.Clear();
            }

            return finished;
        }

        _path.Clear();
        if (moves.Any(m => m.From == square))
        {
            _path.Add(square);
        }

        return null;
    }

    private static bool StartsWith(IReadOnlyList<Square> path, IReadOnlyList<Square> prefix) =>
        path.Count >= prefix.Count && path.Take(prefix.Count).SequenceEqual(prefix);
}
