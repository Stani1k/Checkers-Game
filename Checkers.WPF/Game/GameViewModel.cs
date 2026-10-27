using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Checkers.Tools;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.Tools.Online;
using Checkers.WPF.Commands;
using Checkers.WPF.Localization;
using Checkers.WPF.Online;
using Serilog;

namespace Checkers.WPF.Game;

public class GameViewModel : ViewModelBase
{
    private const int BoardPixels = 720;

    private CheckersGame _game = new(Variant.American);
    private readonly IGameSaveService _saveService;
    private string _saveMessageKey = string.Empty;
    private readonly MoveSelection _selection = new();
    private string _bottomPlayerName = string.Empty;
    private string _topPlayerName = string.Empty;
    private string _statusText = string.Empty;
    private string _scoreText = string.Empty;
    private int _boardSize;
    private bool _turnIsTop;
    private bool _isGameInProgress;
    private readonly IOnlineGameClient _client;
    private bool _isOnline;
    private bool _canResign;
    private Side _onlineSide;
    private OnlineEnd? _onlineEnd;

    /// <summary>How an online game ended outside the board rules; <see cref="Winner"/> is null when nobody won.</summary>
    private sealed record OnlineEnd(Side? Winner, string NoteKey);

    public UpdateViewCommand UpdateViewCommand { get; }
    public ICommand SquareClickCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand SaveGameCommand { get; }
    public ICommand ResignCommand { get; }
    public ICommand LeaveCommand { get; }
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

    public bool IsOffline => !_isOnline;

    public bool IsOnline
    {
        get => _isOnline;
        private set
        {
            if (_isOnline == value) return;
            _isOnline = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsOffline));
        }
    }

    public bool CanResign
    {
        get => _canResign;
        private set
        {
            if (_canResign == value) return;
            _canResign = value;
            OnPropertyChanged();
        }
    }

    public GameViewModel(UpdateViewCommand updateViewCommand, IGameSaveService saveService, IOnlineGameClient client)
    {
        UpdateViewCommand = updateViewCommand;
        _saveService = saveService;
        _client = client;
        _client.MoveApplied += OnOnlineMoveApplied;
        _client.GameEnded += OnOnlineGameEnded;
        _client.ConnectionLost += OnOnlineConnectionLost;
        SquareClickCommand = new RelayCommand<SquareViewModel>(OnSquareClicked);
        NewGameCommand = new RelayCommand(() => StartNewGame(_game.Variant, _bottomPlayerName, _topPlayerName));
        SaveGameCommand = new RelayCommand(SaveGame);
        ResignCommand = new RelayCommand(Resign);
        LeaveCommand = new RelayCommand(LeaveGame);
        StartNewGame(Variant.American, _bottomPlayerName, _topPlayerName);
        Loc.Instance.LanguageChanged += (_, _) => Refresh();
    }

    public void StartNewGame(Variant variant, string bottomPlayerName, string topPlayerName)
    {
        _bottomPlayerName = bottomPlayerName?.Trim() ?? string.Empty;
        _topPlayerName = topPlayerName?.Trim() ?? string.Empty;
        IsOnline = false;
        _onlineEnd = null;
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
        IsOnline = false;
        _onlineEnd = null;
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
        _selection.Clear();
        Squares.Clear();
        var flipped = IsOnline && _onlineSide == Side.Top;
        for (var row = 0; row < _game.Size; row++)
        {
            for (var col = 0; col < _game.Size; col++)
            {
                var square = flipped ? new Square(_game.Size - 1 - col, _game.Size - 1 - row) : new Square(col, row);
                Squares.Add(new SquareViewModel(square.Col, square.Row, _game.IsDarkSquare(square)));
            }
        }

        BoardSize = _game.Size;
        Refresh();
    }

    /// <summary>Starts a networked game; the board is shown from <see cref="GameStartedDto.YourSide"/>'s point of view.</summary>
    public void StartOnlineGame(GameStartedDto started)
    {
        _bottomPlayerName = started.BottomName;
        _topPlayerName = started.TopName;
        _onlineSide = started.YourSide;
        _onlineEnd = null;
        IsOnline = true;
        SetGame(new CheckersGame(started.Variant));
        Log.Information("Online game started as {Side}: {Variant}", started.YourSide, started.Variant);
    }

    private void OnOnlineMoveApplied(MoveDto dto)
    {
        if (!IsOnline || _onlineEnd != null) return;
        if (_game.Turn == _onlineSide || !_game.TryResolveMove(dto.ToSquares(), out var move, out _) || move == null)
        {
            EndOnline(null, "Online.Desync");
            return;
        }

        _game.Apply(move);
        Refresh();
    }

    private void OnOnlineGameEnded(GameEndedDto dto)
    {
        if (!IsOnline || _onlineEnd != null) return;
        EndOnline(dto.Winner, dto.Reason == GameEndReason.Resigned ? "Online.OpponentResigned" : "Online.OpponentLeft");
    }

    private void OnOnlineConnectionLost()
    {
        if (!IsOnline || _onlineEnd != null || _game.Status != GameStatus.InProgress) return;
        EndOnline(null, "Online.ConnectionLost");
    }

    private void EndOnline(Side? winner, string noteKey)
    {
        _onlineEnd = new OnlineEnd(winner, noteKey);
        _selection.Clear();
        Refresh();
    }

    private async void SendMove(Move move)
    {
        try
        {
            await _client.MakeMoveAsync(move);
        }
        catch (OnlineException ex)
        {
            Log.Warning("Move rejected or not delivered: {Code}", ex.Code);
            if (IsOnline && _onlineEnd == null)
            {
                EndOnline(null, ex.Code == "ConnectionLost" ? "Online.ConnectionLost" : "Online.Desync");
            }
        }
    }

    private async void Resign()
    {
        if (!IsOnline || _onlineEnd != null || _game.Status != GameStatus.InProgress) return;
        try
        {
            await _client.ResignAsync();
        }
        catch (OnlineException ex)
        {
            Log.Warning("Resign failed: {Code}", ex.Code);
        }
    }

    private async void LeaveGame()
    {
        if (IsOnline)
        {
            IsOnline = false;
            _onlineEnd = null;
            try
            {
                await _client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Disconnect failed");
            }
        }

        UpdateViewCommand.Execute(MenuOption.MainMenu.ToString());
    }

    private string NameOf(Side side)
    {
        var name = side == Side.Bottom ? _bottomPlayerName : _topPlayerName;
        return name.Length > 0 ? name : Loc.Format("Players.PlayerN", side == Side.Bottom ? 1 : 2);
    }

    private bool IsMyTurn => !IsOnline || (_onlineEnd == null && _game.Turn == _onlineSide);

    private void OnSquareClicked(SquareViewModel? clicked)
    {
        if (clicked == null || _game.Status != GameStatus.InProgress || !IsMyTurn) return;
        _saveMessageKey = string.Empty;

        var moves = _game.GetLegalMoves();
        var finished = _selection.Click(clicked.Square, moves);
        if (finished != null)
        {
            _game.Apply(finished);
            if (IsOnline)
            {
                SendMove(finished);
            }
        }

        Refresh();
    }

    private void Refresh()
    {
        var moves = _game.GetLegalMoves();
        var targets = _selection.Targets(moves);
        var lastMove = _game.LastMove?.Path.ToHashSet() ?? new HashSet<Square>();
        var inProgress = _game.Status == GameStatus.InProgress && _onlineEnd == null;
        var movable = IsMyTurn ? moves.Select(m => m.From).ToHashSet() : new HashSet<Square>();

        foreach (var vm in Squares)
        {
            vm.Piece = _game.PieceAt(vm.Square);
            vm.IsSelected = _selection.Contains(vm.Square);
            vm.IsTarget = targets.Contains(vm.Square);
            vm.IsLastMove = lastMove.Contains(vm.Square);
            vm.IsMovable = inProgress && movable.Contains(vm.Square);
            vm.IsWaiting = inProgress && vm.Piece is { } p && p.Side != _game.Turn;
        }

        TurnIsTop = _game.Turn == Side.Top;
        IsGameInProgress = inProgress;

        CanResign = IsOnline && inProgress;

        StatusText = _game.Status switch
        {
            GameStatus.BottomWins => Loc.Format("Game.Wins", NameOf(Side.Bottom)),
            GameStatus.TopWins => Loc.Format("Game.Wins", NameOf(Side.Top)),
            GameStatus.Draw => Loc.Get("Game.Draw"),
            _ when _onlineEnd is { Winner: { } winner } => Loc.Format("Game.Wins", NameOf(winner)),
            _ when _onlineEnd != null => Loc.Get(_onlineEnd.NoteKey),
            _ when IsOnline && IsMyTurn => Loc.Get(moves.Any(m => m.IsCapture) ? "Online.YourTurnCapture" : "Online.YourTurn"),
            _ => Loc.Format(moves.Any(m => m.IsCapture) ? "Game.TurnCapture" : "Game.Turn", NameOf(_game.Turn))
        };
        ScoreText = Loc.Format("Game.Score", NameOf(Side.Bottom), _game.CountPieces(Side.Bottom),
            NameOf(Side.Top), _game.CountPieces(Side.Top));
        if (_onlineEnd is { Winner: not null })
        {
            ScoreText += $"{Environment.NewLine}{Loc.Get(_onlineEnd.NoteKey)}";
        }

        if (_saveMessageKey.Length > 0)
        {
            ScoreText += $"{Environment.NewLine}{Loc.Get(_saveMessageKey)}";
        }
    }
}
