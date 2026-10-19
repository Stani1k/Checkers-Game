using Checkers.Tools.Models;

namespace Checkers.Tools.Engine;

/// <summary>
/// Rules engine for the two supported variants.
/// American: 8x8, men move and capture forward only, kings move/capture one square in every direction, capturing is mandatory.
/// European: 10x10, men also capture backwards, flying kings, capturing is mandatory and the longest sequence must be taken.
/// Pieces are removed after the whole capture sequence; a piece may not be jumped twice.
/// </summary>
public sealed class CheckersGame
{
    private static readonly (int DCol, int DRow)[] Directions = { (-1, -1), (1, -1), (-1, 1), (1, 1) };

    private readonly Piece?[,] _board;
    private IReadOnlyList<Move> _legalMoves = Array.Empty<Move>();
    private int _quietPlies;

    public Variant Variant { get; }
    public int Size => Variant == Variant.American ? 8 : 10;
    public Side Turn { get; private set; }
    public GameStatus Status { get; private set; }
    public Move? LastMove { get; private set; }

    /// <summary>Plies since the last capture or man move; drives the draw rule.</summary>
    public int QuietPlies => _quietPlies;

    /// <summary>All pieces on the board, for saving the position.</summary>
    public IEnumerable<(Square Square, Piece Piece)> GetPieces()
    {
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                if (_board[col, row] is { } piece)
                {
                    yield return (new Square(col, row), piece);
                }
            }
        }
    }

    private int DrawPlyLimit => Variant == Variant.American ? 80 : 50;

    public CheckersGame(Variant variant) : this(variant, Side.Bottom)
    {
        var rowsPerSide = Variant == Variant.American ? 3 : 4;
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                if (!IsDarkSquare(new Square(col, row)))
                {
                    continue;
                }

                if (row < rowsPerSide)
                {
                    _board[col, row] = new Piece(Side.Top, false);
                }
                else if (row >= Size - rowsPerSide)
                {
                    _board[col, row] = new Piece(Side.Bottom, false);
                }
            }
        }

        UpdateStatus();
    }

    private CheckersGame(Variant variant, Side turn)
    {
        Variant = variant;
        Turn = turn;
        _board = new Piece?[Size, Size];
    }

    /// <summary>Creates a game from a custom position (used for tests and puzzles).</summary>
    public static CheckersGame FromPieces(Variant variant, Side turn, IEnumerable<(Square Square, Piece Piece)> pieces,
        int quietPlies = 0, Move? lastMove = null)
    {
        if (quietPlies < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quietPlies));
        }

        var game = new CheckersGame(variant, turn) { _quietPlies = quietPlies, LastMove = lastMove };
        foreach (var (square, piece) in pieces)
        {
            if (!game.InBounds(square) || !game.IsDarkSquare(square))
            {
                throw new ArgumentException($"Invalid square {square}.", nameof(pieces));
            }

            game._board[square.Col, square.Row] = piece;
        }

        game.UpdateStatus();
        return game;
    }

    public bool InBounds(Square square) => square.Col >= 0 && square.Col < Size && square.Row >= 0 && square.Row < Size;

    /// <summary>Playable squares are the dark ones; the bottom-left square is dark.</summary>
    public bool IsDarkSquare(Square square) => (square.Col + square.Row) % 2 == 1;

    public Piece? PieceAt(Square square) => InBounds(square) ? _board[square.Col, square.Row] : null;

    public int CountPieces(Side side)
    {
        var count = 0;
        foreach (var piece in _board)
        {
            if (piece is { } p && p.Side == side)
            {
                count++;
            }
        }

        return count;
    }

    public IReadOnlyList<Move> GetLegalMoves() => _legalMoves;

    public string ToNotation(Square square) => $"{(char)('a' + square.Col)}{Size - square.Row}";

    public bool TryParseSquare(string text, out Square square)
    {
        square = default;
        text = text.Trim().ToLowerInvariant();
        if (text.Length < 2 || text[0] < 'a' || text[0] > 'z' || !int.TryParse(text.AsSpan(1), out var rank))
        {
            return false;
        }

        square = new Square(text[0] - 'a', Size - rank);
        return InBounds(square);
    }

    /// <summary>
    /// Finds the legal move for the squares typed by a player. The full path is always accepted; start and end alone are
    /// accepted when they identify exactly one legal move.
    /// </summary>
    public bool TryResolveMove(IReadOnlyList<Square> squares, out Move? move, out string error)
    {
        move = null;
        error = string.Empty;

        if (Status != GameStatus.InProgress)
        {
            error = "The game is over.";
            return false;
        }

        if (squares.Count < 2)
        {
            error = "Enter at least a start and a destination square, e.g. c3 d4.";
            return false;
        }

        var exact = _legalMoves.FirstOrDefault(m => m.HasSamePath(squares));
        if (exact != null)
        {
            move = exact;
            return true;
        }

        var candidates = _legalMoves.Where(m => m.From == squares[0] && m.To == squares[^1]).ToList();
        if (candidates.Count == 1)
        {
            move = candidates[0];
            return true;
        }

        if (candidates.Count > 1)
        {
            error = "That move is ambiguous. Enter every square the piece lands on, e.g. c3 e5 g7.";
            return false;
        }

        var piece = PieceAt(squares[0]);
        if (piece == null)
        {
            error = $"There is no piece on {ToNotation(squares[0])}.";
        }
        else if (piece.Value.Side != Turn)
        {
            error = $"The piece on {ToNotation(squares[0])} is not yours.";
        }
        else if (_legalMoves.Any(m => m.IsCapture))
        {
            error = Variant == Variant.European
                ? "You must capture, and you must take the longest capture sequence. Type 'moves' to see your options."
                : "You must capture. Type 'moves' to see your options.";
        }
        else
        {
            error = "Illegal move. Type 'moves' to see your options.";
        }

        return false;
    }

    public void Apply(Move move)
    {
        if (!_legalMoves.Any(m => m.HasSamePath(move.Path)))
        {
            throw new InvalidOperationException("The move is not legal in the current position.");
        }

        var piece = _board[move.From.Col, move.From.Row]!.Value;
        var wasMan = !piece.IsKing;

        _board[move.From.Col, move.From.Row] = null;
        foreach (var captured in move.Captured)
        {
            _board[captured.Col, captured.Row] = null;
        }

        if (wasMan && move.To.Row == KingRow(piece.Side))
        {
            piece = piece.Crowned();
        }

        _board[move.To.Col, move.To.Row] = piece;
        _quietPlies = move.IsCapture || wasMan ? 0 : _quietPlies + 1;
        LastMove = move;
        Turn = Turn == Side.Bottom ? Side.Top : Side.Bottom;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var moves = ComputeLegalMoves(Turn);
        if (moves.Count == 0)
        {
            Status = Turn == Side.Bottom ? GameStatus.TopWins : GameStatus.BottomWins;
            _legalMoves = Array.Empty<Move>();
        }
        else if (_quietPlies >= DrawPlyLimit)
        {
            Status = GameStatus.Draw;
            _legalMoves = Array.Empty<Move>();
        }
        else
        {
            Status = GameStatus.InProgress;
            _legalMoves = moves;
        }
    }

    private List<Move> ComputeLegalMoves(Side side)
    {
        var captures = new List<Move>();
        var simpleMoves = new List<Move>();

        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                if (_board[col, row] is not { } piece || piece.Side != side)
                {
                    continue;
                }

                var start = new Square(col, row);
                CollectCaptures(start, piece, start, piece.IsKing, new List<Square> { start }, new List<Square>(), captures);
                CollectSimpleMoves(start, piece, simpleMoves);
            }
        }

        if (captures.Count == 0)
        {
            return simpleMoves;
        }

        if (Variant == Variant.European)
        {
            var longest = captures.Max(m => m.Captured.Count);
            captures = captures.Where(m => m.Captured.Count == longest).ToList();
        }

        return captures;
    }

    private bool IsEmptyFor(Square square, Square start) =>
        InBounds(square) && (square == start || _board[square.Col, square.Row] == null);

    private int Forward(Side side) => side == Side.Bottom ? -1 : 1;

    private int KingRow(Side side) => side == Side.Bottom ? 0 : Size - 1;

    private void CollectSimpleMoves(Square start, Piece piece, List<Move> result)
    {
        foreach (var (dCol, dRow) in Directions)
        {
            if (!piece.IsKing && dRow != Forward(piece.Side))
            {
                continue;
            }

            var target = start.Offset(dCol, dRow);
            while (IsEmptyFor(target, start))
            {
                result.Add(new Move(new[] { start, target }, Array.Empty<Square>()));
                if (!(piece.IsKing && Variant == Variant.European))
                {
                    break;
                }

                target = target.Offset(dCol, dRow);
            }
        }
    }

    private void CollectCaptures(Square start, Piece piece, Square current, bool isKing, List<Square> path, List<Square> captured, List<Move> result)
    {
        var extended = false;

        foreach (var (dCol, dRow) in Directions)
        {
            if (!isKing && Variant == Variant.American && dRow != Forward(piece.Side))
            {
                continue;
            }

            var over = current.Offset(dCol, dRow);
            if (isKing && Variant == Variant.European)
            {
                while (IsEmptyFor(over, start))
                {
                    over = over.Offset(dCol, dRow);
                }
            }

            if (!InBounds(over) || _board[over.Col, over.Row] is not { } victim || victim.Side == piece.Side || captured.Contains(over))
            {
                continue;
            }

            var landing = over.Offset(dCol, dRow);
            while (IsEmptyFor(landing, start))
            {
                extended = true;
                captured.Add(over);
                path.Add(landing);

                // In the American variant a man that reaches the king row has to stop there.
                var crowned = !isKing && Variant == Variant.American && landing.Row == KingRow(piece.Side);
                if (crowned)
                {
                    result.Add(new Move(path.ToList(), captured.ToList()));
                }
                else
                {
                    CollectCaptures(start, piece, landing, isKing, path, captured, result);
                }

                path.RemoveAt(path.Count - 1);
                captured.RemoveAt(captured.Count - 1);

                if (!(isKing && Variant == Variant.European))
                {
                    break;
                }

                landing = landing.Offset(dCol, dRow);
            }
        }

        if (!extended && captured.Count > 0)
        {
            result.Add(new Move(path.ToList(), captured.ToList()));
        }
    }
}
