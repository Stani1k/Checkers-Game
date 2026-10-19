using Checkers.Tools;
using Checkers.Tools.Engine;

namespace Checkers.WPF.Game;

public sealed class SquareViewModel : ViewModelBase
{
    private Piece? _piece;
    private bool _isSelected;
    private bool _isTarget;
    private bool _isLastMove;
    private bool _isMovable;
    private bool _isWaiting;

    public SquareViewModel(int col, int row, bool isDark)
    {
        Square = new Square(col, row);
        IsDark = isDark;
    }

    public Square Square { get; }
    public bool IsDark { get; }

    public Piece? Piece
    {
        get => _piece;
        set
        {
            if (_piece == value) return;
            _piece = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasPiece));
            OnPropertyChanged(nameof(IsTopPiece));
            OnPropertyChanged(nameof(IsKing));
        }
    }

    public bool HasPiece => _piece != null;
    public bool IsTopPiece => _piece?.Side == Side.Top;
    public bool IsKing => _piece?.IsKing == true;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public bool IsTarget
    {
        get => _isTarget;
        set
        {
            if (_isTarget == value) return;
            _isTarget = value;
            OnPropertyChanged();
        }
    }

    public bool IsLastMove
    {
        get => _isLastMove;
        set
        {
            if (_isLastMove == value) return;
            _isLastMove = value;
            OnPropertyChanged();
        }
    }

    /// <summary>The piece belongs to the player on turn and has a legal move.</summary>
    public bool IsMovable
    {
        get => _isMovable;
        set
        {
            if (_isMovable == value) return;
            _isMovable = value;
            OnPropertyChanged();
        }
    }

    /// <summary>The piece belongs to the player who is not on turn.</summary>
    public bool IsWaiting
    {
        get => _isWaiting;
        set
        {
            if (_isWaiting == value) return;
            _isWaiting = value;
            OnPropertyChanged();
        }
    }
}