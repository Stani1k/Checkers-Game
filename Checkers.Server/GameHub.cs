using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.Tools.Online;
using Microsoft.AspNetCore.SignalR;

namespace Checkers.Server;

public sealed class GameHub : Hub
{
    private readonly GameRoomManager _rooms;

    public GameHub(GameRoomManager rooms)
    {
        _rooms = rooms;
    }

    public CreateGameResultDto CreateGame(Variant variant, string? name)
    {
        if (!Enum.IsDefined(variant))
        {
            throw new HubException(OnlineProtocol.ErrorIllegalMove);
        }

        var (room, error) = _rooms.Create(Context.ConnectionId, variant, name);
        return room != null ? new CreateGameResultDto(room.Code) : throw new HubException(error);
    }

    /// <summary>Joins a game by code; both players are then sent GameStarted. Returns null on success or an error code.</summary>
    public async Task<string?> JoinGame(string code, string? name)
    {
        var (room, error) = _rooms.Join(code, Context.ConnectionId, name);
        if (room == null)
        {
            return error;
        }

        await Clients.Client(room.HostConnection).SendAsync(OnlineProtocol.GameStartedEvent,
            new GameStartedDto(room.Variant, Side.Bottom, room.HostName, room.GuestName));
        await Clients.Client(room.GuestConnection!).SendAsync(OnlineProtocol.GameStartedEvent,
            new GameStartedDto(room.Variant, Side.Top, room.HostName, room.GuestName));
        return null;
    }

    /// <summary>Validates and applies a move, then forwards it to the opponent. Returns null on success or an error code.</summary>
    public async Task<string?> MakeMove(MoveDto move)
    {
        var path = move.Path.Select(p => p.ToSquare()).ToList();
        var (room, error) = _rooms.ApplyMove(Context.ConnectionId, path);
        if (error != null || room == null)
        {
            return error;
        }

        var opponent = room.SideOf(Context.ConnectionId) == Side.Bottom ? room.GuestConnection : room.HostConnection;
        await Clients.Client(opponent!).SendAsync(OnlineProtocol.MoveAppliedEvent, move);
        return null;
    }

    public async Task Resign()
    {
        var (room, error) = _rooms.Resign(Context.ConnectionId);
        if (room == null || error != null)
        {
            return;
        }

        var winner = room.SideOf(Context.ConnectionId) == Side.Bottom ? Side.Top : Side.Bottom;
        var ended = new GameEndedDto(winner, GameEndReason.Resigned);
        await Clients.Clients(room.HostConnection, room.GuestConnection!).SendAsync(OnlineProtocol.GameEndedEvent, ended);
    }

    /// <summary>Cancels a game that is waiting for an opponent, or forfeits a running one.</summary>
    public Task LeaveGame() => HandleLeave();

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await HandleLeave();
        await base.OnDisconnectedAsync(exception);
    }

    private async Task HandleLeave()
    {
        var room = _rooms.Leave(Context.ConnectionId);
        if (room is not { HasStarted: true })
        {
            return;
        }

        var leaver = room.SideOf(Context.ConnectionId)!.Value;
        var winner = leaver == Side.Bottom ? Side.Top : Side.Bottom;
        var remaining = room.ConnectionOf(winner)!;
        await Clients.Client(remaining).SendAsync(OnlineProtocol.GameEndedEvent,
            new GameEndedDto(winner, GameEndReason.OpponentLeft));
    }
}
