using System;
using System.Threading.Tasks;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.Tools.Online;

namespace Checkers.WPF.Online;

public sealed class OnlineException : Exception
{
    public OnlineException(string code) : base(code)
    {
        Code = code;
    }

    public string Code { get; }
}

public interface IOnlineGameClient : IAsyncDisposable
{
    event Action<GameStartedDto>? GameStarted;
    event Action<MoveDto>? MoveApplied;
    event Action<GameEndedDto>? GameEnded;

    /// <summary>Raised when the connection drops unexpectedly.</summary>
    event Action? ConnectionLost;

    Task ConnectAsync(string serverUrl);

    /// <summary>Creates a game and returns its join code.</summary>
    Task<string> CreateGameAsync(Variant variant, string playerName);

    Task JoinGameAsync(string code, string playerName);
    Task MakeMoveAsync(Move move);
    Task ResignAsync();

    /// <summary>Closes the connection; the server cancels or forfeits any game in progress.</summary>
    Task DisconnectAsync();
}
