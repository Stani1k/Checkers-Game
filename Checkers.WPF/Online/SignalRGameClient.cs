using System;
using System.Threading;
using System.Threading.Tasks;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.Tools.Online;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

namespace Checkers.WPF.Online;

public sealed class SignalRGameClient : IOnlineGameClient, IDisposable
{
    private readonly SynchronizationContext? _context = SynchronizationContext.Current;
    private HubConnection? _connection;

    public event Action<GameStartedDto>? GameStarted;
    public event Action<MoveDto>? MoveApplied;
    public event Action<GameEndedDto>? GameEnded;
    public event Action? ConnectionLost;

    public async Task ConnectAsync(string serverUrl)
    {
        await DisconnectAsync();

        if (!Uri.TryCreate(serverUrl.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new OnlineException("InvalidUrl");
        }

        var hubUri = new Uri(uri, OnlineProtocol.HubPath);
        var connection = new HubConnectionBuilder().WithUrl(hubUri).Build();
        connection.On<GameStartedDto>(OnlineProtocol.GameStartedEvent, dto => Raise(() => GameStarted?.Invoke(dto)));
        connection.On<MoveDto>(OnlineProtocol.MoveAppliedEvent, dto => Raise(() => MoveApplied?.Invoke(dto)));
        connection.On<GameEndedDto>(OnlineProtocol.GameEndedEvent, dto => Raise(() => GameEnded?.Invoke(dto)));
        connection.Closed += error =>
        {
            if (ReferenceEquals(_connection, connection))
            {
                _connection = null;
                Log.Warning(error, "Online connection closed");
                Raise(() => ConnectionLost?.Invoke());
            }

            return Task.CompletedTask;
        };

        try
        {
            await connection.StartAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not connect to {Url}", hubUri);
            await connection.DisposeAsync();
            throw new OnlineException("ConnectFailed");
        }

        _connection = connection;
    }

    public async Task<string> CreateGameAsync(Variant variant, string playerName)
    {
        var result = await InvokeAsync(c => c.InvokeAsync<CreateGameResultDto>("CreateGame", variant, playerName));
        return result.Code;
    }

    public async Task JoinGameAsync(string code, string playerName)
    {
        var error = await InvokeAsync(c => c.InvokeAsync<string?>("JoinGame", code, playerName));
        if (error != null)
        {
            throw new OnlineException(error);
        }
    }

    public async Task MakeMoveAsync(Move move)
    {
        var error = await InvokeAsync(c => c.InvokeAsync<string?>("MakeMove", MoveDto.From(move)));
        if (error != null)
        {
            throw new OnlineException(error);
        }
    }

    public Task ResignAsync() => InvokeAsync(c => c.InvokeAsync("Resign"));

    public async Task DisconnectAsync()
    {
        var connection = _connection;
        _connection = null;
        if (connection != null)
        {
            await connection.DisposeAsync();
        }
    }

    public ValueTask DisposeAsync() => new(DisconnectAsync());

    /// <summary>Used by the synchronous container disposal on exit; runs off the UI thread to avoid blocking on it.</summary>
    public void Dispose() => Task.Run(DisconnectAsync).Wait(TimeSpan.FromSeconds(2));

    private async Task<T> InvokeAsync<T>(Func<HubConnection, Task<T>> call)
    {
        var connection = _connection ?? throw new OnlineException("ConnectionLost");
        try
        {
            return await call(connection);
        }
        catch (HubException ex)
        {
            // The message ends with the error code thrown by the hub.
            throw new OnlineException(ex.Message[(ex.Message.LastIndexOf(' ') + 1)..]);
        }
        catch (Exception ex) when (ex is not OnlineException)
        {
            Log.Warning(ex, "Online call failed");
            throw new OnlineException("ConnectionLost");
        }
    }

    private Task InvokeAsync(Func<HubConnection, Task> call) =>
        InvokeAsync(async c =>
        {
            await call(c);
            return true;
        });

    private void Raise(Action action)
    {
        if (_context != null)
        {
            _context.Post(_ => action(), null);
        }
        else
        {
            action();
        }
    }
}
