using System.Security.Cryptography;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.Tools.Online;

namespace Checkers.Server;

public sealed class GameRoom
{
    public GameRoom(string code, Variant variant, string hostConnection, string hostName)
    {
        Code = code;
        Variant = variant;
        HostConnection = hostConnection;
        HostName = hostName;
        Game = new CheckersGame(variant);
    }

    public string Code { get; }
    public Variant Variant { get; }
    public CheckersGame Game { get; }
    public string HostConnection { get; }
    public string HostName { get; }
    public string? GuestConnection { get; set; }
    public string GuestName { get; set; } = string.Empty;

    public bool HasStarted => GuestConnection != null;

    /// <summary>The host plays Bottom (moves first), the guest plays Top.</summary>
    public Side? SideOf(string connectionId) =>
        connectionId == HostConnection ? Side.Bottom : connectionId == GuestConnection ? Side.Top : null;

    public string? ConnectionOf(Side side) => side == Side.Bottom ? HostConnection : GuestConnection;
}

/// <summary>Authoritative in-memory state of all games. Independent of SignalR so it can be tested directly.</summary>
public sealed class GameRoomManager
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly object _lock = new();
    private readonly Dictionary<string, GameRoom> _rooms = new();
    private readonly Dictionary<string, GameRoom> _byConnection = new();

    public (GameRoom? Room, string? Error) Create(string connectionId, Variant variant, string? name)
    {
        lock (_lock)
        {
            if (_byConnection.ContainsKey(connectionId))
            {
                return (null, OnlineProtocol.ErrorAlreadyInGame);
            }

            string code;
            do
            {
                code = NewCode();
            } while (_rooms.ContainsKey(code));

            var room = new GameRoom(code, variant, connectionId, OnlineProtocol.NormalizeName(name));
            _rooms[code] = room;
            _byConnection[connectionId] = room;
            return (room, null);
        }
    }

    public (GameRoom? Room, string? Error) Join(string code, string connectionId, string? name)
    {
        lock (_lock)
        {
            if (_byConnection.ContainsKey(connectionId))
            {
                return (null, OnlineProtocol.ErrorAlreadyInGame);
            }

            if (!_rooms.TryGetValue(OnlineProtocol.NormalizeCode(code), out var room))
            {
                return (null, OnlineProtocol.ErrorNotFound);
            }

            if (room.HasStarted)
            {
                return (null, OnlineProtocol.ErrorFull);
            }

            room.GuestConnection = connectionId;
            room.GuestName = OnlineProtocol.NormalizeName(name);
            _byConnection[connectionId] = room;
            return (room, null);
        }
    }

    /// <summary>Validates and applies a move for the given connection. The room is removed when the game ends.</summary>
    public (GameRoom? Room, string? Error) ApplyMove(string connectionId, IReadOnlyList<Square> path)
    {
        lock (_lock)
        {
            if (!_byConnection.TryGetValue(connectionId, out var room))
            {
                return (null, OnlineProtocol.ErrorNotInGame);
            }

            if (!room.HasStarted)
            {
                return (room, OnlineProtocol.ErrorNotStarted);
            }

            var game = room.Game;
            if (game.Status != GameStatus.InProgress)
            {
                return (room, OnlineProtocol.ErrorGameOver);
            }

            if (room.SideOf(connectionId) != game.Turn)
            {
                return (room, OnlineProtocol.ErrorNotYourTurn);
            }

            var move = game.GetLegalMoves().FirstOrDefault(m => m.HasSamePath(path));
            if (move == null)
            {
                return (room, OnlineProtocol.ErrorIllegalMove);
            }

            game.Apply(move);
            if (game.Status != GameStatus.InProgress)
            {
                Remove(room);
            }

            return (room, null);
        }
    }

    /// <summary>Ends a started game; the caller's opponent wins.</summary>
    public (GameRoom? Room, string? Error) Resign(string connectionId)
    {
        lock (_lock)
        {
            if (!_byConnection.TryGetValue(connectionId, out var room))
            {
                return (null, OnlineProtocol.ErrorNotInGame);
            }

            if (!room.HasStarted)
            {
                return (room, OnlineProtocol.ErrorNotStarted);
            }

            Remove(room);
            return (room, null);
        }
    }

    /// <summary>Removes the connection's room (disconnect or cancel) and returns it, or null when it was in none.</summary>
    public GameRoom? Leave(string connectionId)
    {
        lock (_lock)
        {
            if (!_byConnection.TryGetValue(connectionId, out var room))
            {
                return null;
            }

            Remove(room);
            return room;
        }
    }

    public int RoomCount
    {
        get
        {
            lock (_lock)
            {
                return _rooms.Count;
            }
        }
    }

    private void Remove(GameRoom room)
    {
        _rooms.Remove(room.Code);
        _byConnection.Remove(room.HostConnection);
        if (room.GuestConnection != null)
        {
            _byConnection.Remove(room.GuestConnection);
        }
    }

    private static string NewCode()
    {
        var chars = new char[OnlineProtocol.CodeLength];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        }

        return new string(chars);
    }
}
