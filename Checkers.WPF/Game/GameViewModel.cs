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

namespace Checkers.WPF.Game;

public class GameViewModel : ViewModelBase
{
    private const int BoardPixels = 720;

    private CheckersGame _game = new(Variant.American);
    private readonly GameSaveService _saveService;
    private string _saveMessage = string.Empty;
    private readonly List<Square> _path = new();
    private string _bottomPlayerName = "Player 1";
    private string _topPlayerName = "Player 2";
    private string _statusText = string.Empty;
    private string _scoreText = string.Empty;
    private int _boardSize;

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
    }

    public void StartNewGame(Variant variant, string bottomPlayerName, string topPlayerName)
    {
        _bottomPlayerName = string.IsNullOrWhiteSpace(bottomPlayerName) ? "Player 1" : bottomPlayerName.Trim();
        _topPlayerName = string.IsNullOrWhiteSpace(topPlayerName) ? "Player 2" : topPlayerName.Trim();
        SetGame(new CheckersGame(variant));
    }

    /// <summary>Loads the saved game; returns false when there is no valid save.</summary>
    public bool LoadSavedGame(out string error)
    {
        if (!_saveService.TryLoad(out var data, out var game, out error) || data == null || game == null)
        {
            return false;
        }

        _bottomPlayerName = string.IsNullOrWhiteSpace(data.BottomPlayerName) ? "Player 1" : data.BottomPlayerName.Trim();
        _topPlayerName = string.IsNullOrWhiteSpace(data.TopPlayerName) ? "Player 2" : data.TopPlayerName.Trim();
        SetGame(game);
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
            _saveMessage = "Game saved.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _saveMessage = "Could not save the game.";
        }

        Refresh();
    }

    private void SetGame(CheckersGame game)
    {
        _game = game;
        _path.Clear();
        _saveMessage = string.Empty;

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

    private string NameOf(Side side) => side == Side.Bottom ? _bottomPlayerName : _topPlayerName;

    private void OnSquareClicked(SquareViewModel? clicked)
    {
        if (clicked == null || _game.Status != GameStatus.InProgress) return;
        _saveMessage = string.Empty;

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

        foreach (var vm in Squares)
        {
            vm.Piece = _game.PieceAt(vm.Square);
            vm.IsSelected = _path.Contains(vm.Square);
            vm.IsTarget = targets.Contains(vm.Square);
            vm.IsLastMove = lastMove.Contains(vm.Square);
        }

        StatusText = _game.Status switch
        {
            GameStatus.BottomWins => $"{_bottomPlayerName} wins!",
            GameStatus.TopWins => $"{_topPlayerName} wins!",
            GameStatus.Draw => "Draw.",
            _ => moves.Any(m => m.IsCapture)
                ? $"{NameOf(_game.Turn)}'s turn - you must capture."
                : $"{NameOf(_game.Turn)}'s turn."
        };
        ScoreText = $"{_bottomPlayerName} (light): {_game.CountPieces(Side.Bottom)}    " +
                    $"{_topPlayerName} (dark): {_game.CountPieces(Side.Top)}";
        if (_saveMessage.Length > 0)
        {
            ScoreText += $"{Environment.NewLine}{_saveMessage}";
        }
    }
}
