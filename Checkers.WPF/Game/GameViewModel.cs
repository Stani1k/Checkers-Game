using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Checkers.Tools;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.WPF.Commands;
using Checkers.WPF.Localization;
using Serilog;

namespace Checkers.WPF.Game;

public class GameViewModel : ViewModelBase
{
    private const int BoardPixels = 720;

    private CheckersGame _game = new(Variant.American);
    private readonly GameSaveService _saveService;
    private string _saveMessageKey = string.Empty;
    private readonly List<Square> _path = new();
    private string _bottomPlayerName = string.Empty;
    private string _topPlayerName = string.Empty;
    private string _statusText = string.Empty;
    private string _scoreText = string.Empty;
    private int _boardSize;
    private bool _turnIsTop;
    private bool _isGameInProgress;

    public UpdateViewCommand UpdateViewCommand { get; }
    public ICommand SquareClickCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand SaveGameCommand { get; }
    public ObservableCollection<SquareViewModel> Squares { get; } = new();

    public int BoardWidthAndHeight => BoardPixels;

    public int BoardSize
    {
        get => _boardSize;
        private set
        {
            if (_boardSize == value) return;
            _boardSize = value;
            OnPropertyChanged();
        }
    }

    public bool TurnIsTop
    {
        get => _turnIsTop;
        private set
        {
            if (_turnIsTop == value) return;
            _turnIsTop = value;
            OnPropertyChanged();
        }
    }

    public bool IsGameInProgress
    {
        get => _isGameInProgress;
        private set
        {
            if (_isGameInProgress == value) return;
            _isGameInProgress = value;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (_statusText == value) return;
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public string ScoreText
    {
        get => _scoreText;
        private set
        {
            if (_scoreText == value) return;
            _scoreText = value;
            OnPropertyChanged();
        }
    }

    public GameViewModel(UpdateViewCommand updateViewCommand, GameSaveService saveService)
    {
        UpdateViewCommand = updateViewCommand;
        _saveService = saveService;
        SquareClickCommand = new RelayCommand<SquareViewModel>(OnSquareClicked);
        NewGameCommand = new RelayCommand(() => StartNewGame(_game.Variant, _bottomPlayerName, _topPlayerName));
        SaveGameCommand = new RelayCommand(SaveGame);
        StartNewGame(Variant.American, _bottomPlayerName, _topPlayerName);
        Loc.Instance.LanguageChanged += (_, _) => Refresh();
    }

    public void StartNewGame(Variant variant, string bottomPlayerName, string topPlayerName)
    {
        _bottomPlayerName = bottomPlayerName?.Trim() ?? string.Empty;
        _topPlayerName = topPlayerName?.Trim() ?? string.Empty;
        SetGame(new CheckersGame(variant));
        Log.Information("New game started: {Variant}, {Bottom} vs {Top}", variant, _bottomPlayerName, _topPlayerName);
    }

    /// <summary>Loads the saved game; returns false when there is no valid save.</summary>
    public bool LoadSavedGame(out string error)
    {
        if (!_saveService.TryLoad(out var data, out var game, out error) || data == null || game == null)
        {
            return false;
        }

        _bottomPlayerName = data.BottomPlayerName?.Trim() ?? string.Empty;
        _topPlayerName = data.TopPlayerName?.Trim() ?? string.Empty;
        SetGame(game);
        Log.Information("Saved game loaded: {Variant}", game.Variant);
        return true;
    }

    private void SaveGame()
    {
        if (_game.Status != GameStatus.InProgress)
        {
            return;
        }

        try
        {
            _saveService.Save(GameSaveData.From(_game, _bottomPlayerName, _topPlayerName));
            _saveMessageKey = "Game.Saved";
            Log.Information("Game saved");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Error(ex, "Failed to save game");
            _saveMessageKey = "Game.SaveFailed";
        }

        Refresh();
    }

    private void SetGame(CheckersGame game)
    {
        _game = game;
        _path.Clear();
        _saveMessageKey = string.Empty;

        Squares.Clear();
        for (var row = 0; row < _game.Size; row++)
        {
            for (var col = 0; col < _game.Size; col++)
            {
                Squares.Add(new SquareViewModel(col, row, _game.IsDarkSquare(new Square(col, row))));
            }
        }

        BoardSize = _game.Size;
        Refresh();
    }

    private SquareViewModel SquareAt(Square square) => Squares[square.Row * _game.Size + square.Col];

    private string NameOf(Side side)
    {
        var name = side == Side.Bottom ? _bottomPlayerName : _topPlayerName;
        return name.Length > 0 ? name : Loc.Format("Players.PlayerN", side == Side.Bottom ? 1 : 2);
    }

    private void OnSquareClicked(SquareViewModel? clicked)
    {
        if (clicked == null || _game.Status != GameStatus.InProgress) return;
        _saveMessageKey = string.Empty;

        var square = clicked.Square;
        var moves = _game.GetLegalMoves();

        if (_path.Count > 0)
        {
            var candidates = moves.Where(m => StartsWith(m.Path, _path)).ToList();
            if (candidates.Any(m => m.Path.Count > _path.Count && m.Path[_path.Count] == square))
            {
                _path.Add(square);
                var finished = moves.FirstOrDefault(m => m.HasSamePath(_path));
                if (finished != null)
                {
                    _path.Clear();
                    _game.Apply(finished);
                }

                Refresh();
                return;
            }
        }

        _path.Clear();
        if (moves.Any(m => m.From == square))
        {
            _path.Add(square);
        }

        Refresh();
    }

    private static bool StartsWith(IReadOnlyList<Square> path, IReadOnlyList<Square> prefix) =>
        path.Count >= prefix.Count && path.Take(prefix.Count).SequenceEqual(prefix);

    private void Refresh()
    {
        var moves = _game.GetLegalMoves();
        var targets = _path.Count == 0
            ? new HashSet<Square>()
            : moves.Where(m => StartsWith(m.Path, _path) && m.Path.Count > _path.Count)
                   .Select(m => m.Path[_path.Count])
                   .ToHashSet();
        var lastMove = _game.LastMove?.Path.ToHashSet() ?? new HashSet<Square>();
        var inProgress = _game.Status == GameStatus.InProgress;
        var movable = moves.Select(m => m.From).ToHashSet();

        foreach (var vm in Squares)
        {
            vm.Piece = _game.PieceAt(vm.Square);
            vm.IsSelected = _path.Contains(vm.Square);
            vm.IsTarget = targets.Contains(vm.Square);
            vm.IsLastMove = lastMove.Contains(vm.Square);
            vm.IsMovable = inProgress && movable.Contains(vm.Square);
            vm.IsWaiting = inProgress && vm.Piece is { } p && p.Side != _game.Turn;
        }

        TurnIsTop = _game.Turn == Side.Top;
        IsGameInProgress = inProgress;

        StatusText = _game.Status switch
        {
            GameStatus.BottomWins => Loc.Format("Game.Wins", NameOf(Side.Bottom)),
            GameStatus.TopWins => Loc.Format("Game.Wins", NameOf(Side.Top)),
            GameStatus.Draw => Loc.Get("Game.Draw"),
            _ => Loc.Format(moves.Any(m => m.IsCapture) ? "Game.TurnCapture" : "Game.Turn", NameOf(_game.Turn))
        };
        ScoreText = Loc.Format("Game.Score", NameOf(Side.Bottom), _game.CountPieces(Side.Bottom),
            NameOf(Side.Top), _game.CountPieces(Side.Top));
        if (_saveMessageKey.Length > 0)
        {
            ScoreText += $"{Environment.NewLine}{Loc.Get(_saveMessageKey)}";
        }
    }
}
