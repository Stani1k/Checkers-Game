using System.Collections.Generic;
using System.Windows.Input;
using Checkers.Tools;
using Checkers.Tools.Models;
using Checkers.Tools.Online;
using Checkers.WPF.Commands;
using Checkers.WPF.Game;
using Checkers.WPF.Localization;
using Checkers.WPF.Online;
using Serilog;

namespace Checkers.WPF.Players;

public class OnlineMenuViewModel : ViewModelBase
{
    private readonly IOnlineGameClient _client;
    private readonly GameViewModel _game;
    private string _serverUrl = "http://localhost:5000";
    private string _playerName = string.Empty;
    private string _joinCode = string.Empty;
    private string _gameCode = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private bool _isWaiting;

    public UpdateViewCommand UpdateViewCommand { get; }
    public IReadOnlyList<Variant> Variants { get; } = new[] { Variant.American, Variant.European };
    public Variant SelectedVariant { get; set; } = Variant.American;
    public ICommand CreateCommand { get; }
    public ICommand JoinCommand { get; }
    public ICommand CancelCommand { get; }

    public OnlineMenuViewModel(UpdateViewCommand updateViewCommand, IOnlineGameClient client, GameViewModel game)
    {
        UpdateViewCommand = updateViewCommand;
        _client = client;
        _game = game;
        CreateCommand = new RelayCommand(Create);
        JoinCommand = new RelayCommand(Join);
        CancelCommand = new RelayCommand(Cancel);
        _client.GameStarted += OnGameStarted;
        _client.ConnectionLost += OnConnectionLost;
    }

    public string ServerUrl
    {
        get => _serverUrl;
        set => Set(ref _serverUrl, value);
    }

    public string PlayerName
    {
        get => _playerName;
        set => Set(ref _playerName, value);
    }

    public string JoinCode
    {
        get => _joinCode;
        set => Set(ref _joinCode, value);
    }

    public string GameCode
    {
        get => _gameCode;
        private set => Set(ref _gameCode, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (!Set(ref _errorMessage, value)) return;
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => _errorMessage.Length > 0;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!Set(ref _isBusy, value)) return;
            OnPropertyChanged(nameof(CanEdit));
        }
    }

    public bool IsWaiting
    {
        get => _isWaiting;
        private set
        {
            if (!Set(ref _isWaiting, value)) return;
            OnPropertyChanged(nameof(CanEdit));
        }
    }

    /// <summary>The connection form is editable unless a request is running or a game is waiting for an opponent.</summary>
    public bool CanEdit => !_isBusy && !_isWaiting;

    /// <summary>Clears state from a previous visit when the menu is opened.</summary>
    public void Reset()
    {
        ErrorMessage = string.Empty;
        GameCode = string.Empty;
        IsWaiting = false;
        IsBusy = false;
    }

    private async void Create()
    {
        if (!CanEdit) return;
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            await _client.ConnectAsync(ServerUrl);
            GameCode = await _client.CreateGameAsync(SelectedVariant, PlayerName);
            IsWaiting = true;
        }
        catch (OnlineException ex)
        {
            await FailAsync(ex.Code);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void Join()
    {
        if (!CanEdit) return;
        var code = OnlineProtocol.NormalizeCode(JoinCode);
        if (code.Length != OnlineProtocol.CodeLength)
        {
            ErrorMessage = ErrorText("InvalidCode");
            return;
        }

        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            await _client.ConnectAsync(ServerUrl);
            await _client.JoinGameAsync(code, PlayerName);
        }
        catch (OnlineException ex)
        {
            await FailAsync(ex.Code);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void Cancel()
    {
        await _client.DisconnectAsync();
        Reset();
        UpdateViewCommand.Execute(MenuOption.MainMenu.ToString());
    }

    private async System.Threading.Tasks.Task FailAsync(string code)
    {
        Log.Information("Online action failed: {Code}", code);
        ErrorMessage = ErrorText(code);
        IsWaiting = false;
        await _client.DisconnectAsync();
    }

    private void OnGameStarted(GameStartedDto started)
    {
        _game.StartOnlineGame(started);
        Reset();
        UpdateViewCommand.Execute(MenuOption.OnlineGame.ToString());
    }

    private void OnConnectionLost()
    {
        if (!IsWaiting) return;
        IsWaiting = false;
        GameCode = string.Empty;
        ErrorMessage = ErrorText("ConnectionLost");
    }

    private static string ErrorText(string code)
    {
        var key = $"Online.Error.{code}";
        var text = Loc.Get(key);
        return text == key ? Loc.Get("Online.Error.Generic") : text;
    }

    private bool Set(ref string field, string value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        if (field == value) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private bool Set(ref bool field, bool value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        if (field == value) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
