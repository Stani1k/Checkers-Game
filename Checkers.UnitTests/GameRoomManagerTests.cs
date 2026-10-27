using Checkers.Server;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;
using Checkers.Tools.Online;

namespace Checkers.UnitTests;

public class GameRoomManagerTests
{
    private static readonly Square[] BottomOpening = { new(0, 5), new(1, 4) };
    private static readonly Square[] TopReply = { new(1, 2), new(0, 3) };

    private GameRoomManager _manager = null!;

    [SetUp]
    public void SetUp() => _manager = new GameRoomManager();

    private GameRoom StartGame(Variant variant = Variant.American)
    {
        var (room, error) = _manager.Create("host", variant, "Alice");
        Assert.That(error, Is.Null);
        var (joined, joinError) = _manager.Join(room!.Code, "guest", "Bob");
        Assert.That(joinError, Is.Null);
        return joined!;
    }

    [Test]
    public void Create_ReturnsCodeOfExpectedShape()
    {
        var (room, _) = _manager.Create("host", Variant.European, "  Alice  ");

        Assert.That(room!.Code, Has.Length.EqualTo(OnlineProtocol.CodeLength));
        Assert.That(room.HostName, Is.EqualTo("Alice"));
        Assert.That(room.Variant, Is.EqualTo(Variant.European));
    }

    [Test]
    public void Join_AssignsSidesAndIsCaseInsensitive()
    {
        var (room, _) = _manager.Create("host", Variant.American, "Alice");

        var (joined, error) = _manager.Join(room!.Code.ToLowerInvariant(), "guest", "Bob");

        Assert.That(error, Is.Null);
        Assert.That(joined!.SideOf("host"), Is.EqualTo(Side.Bottom));
        Assert.That(joined.SideOf("guest"), Is.EqualTo(Side.Top));
        Assert.That(joined.GuestName, Is.EqualTo("Bob"));
    }

    [Test]
    public void Join_UnknownCode_Fails() =>
        Assert.That(_manager.Join("ZZZZZ", "guest", "Bob").Error, Is.EqualTo(OnlineProtocol.ErrorNotFound));

    [Test]
    public void Join_FullRoom_Fails()
    {
        var room = StartGame();

        Assert.That(_manager.Join(room.Code, "third", "Eve").Error, Is.EqualTo(OnlineProtocol.ErrorFull));
    }

    [Test]
    public void Create_WhileAlreadyInGame_Fails()
    {
        _manager.Create("host", Variant.American, "Alice");

        Assert.That(_manager.Create("host", Variant.American, "Alice").Error, Is.EqualTo(OnlineProtocol.ErrorAlreadyInGame));
    }

    [Test]
    public void Move_BeforeOpponentJoined_Fails()
    {
        _manager.Create("host", Variant.American, "Alice");

        Assert.That(_manager.ApplyMove("host", BottomOpening).Error, Is.EqualTo(OnlineProtocol.ErrorNotStarted));
    }

    [Test]
    public void Move_ByPlayerNotOnTurn_Fails()
    {
        StartGame();

        Assert.That(_manager.ApplyMove("guest", TopReply).Error, Is.EqualTo(OnlineProtocol.ErrorNotYourTurn));
    }

    [Test]
    public void Move_Illegal_IsRejectedAndDoesNotChangeTurn()
    {
        var room = StartGame();

        var result = _manager.ApplyMove("host", new[] { new Square(0, 5), new Square(5, 0) });

        Assert.That(result.Error, Is.EqualTo(OnlineProtocol.ErrorIllegalMove));
        Assert.That(room.Game.Turn, Is.EqualTo(Side.Bottom));
    }

    [Test]
    public void Move_Legal_IsAppliedAndTurnPasses()
    {
        var room = StartGame();

        Assert.That(_manager.ApplyMove("host", BottomOpening).Error, Is.Null);
        Assert.That(room.Game.Turn, Is.EqualTo(Side.Top));
        Assert.That(_manager.ApplyMove("guest", TopReply).Error, Is.Null);
        Assert.That(room.Game.Turn, Is.EqualTo(Side.Bottom));
    }

    [Test]
    public void Move_ByStranger_Fails() =>
        Assert.That(_manager.ApplyMove("nobody", BottomOpening).Error, Is.EqualTo(OnlineProtocol.ErrorNotInGame));

    [Test]
    public void Resign_RemovesRoom()
    {
        var room = StartGame();

        var (resigned, error) = _manager.Resign("guest");

        Assert.That(error, Is.Null);
        Assert.That(resigned, Is.SameAs(room));
        Assert.That(_manager.RoomCount, Is.EqualTo(0));
        Assert.That(_manager.ApplyMove("host", BottomOpening).Error, Is.EqualTo(OnlineProtocol.ErrorNotInGame));
    }

    [Test]
    public void Leave_WaitingHost_RemovesRoomSoCodeCannotBeJoined()
    {
        var (room, _) = _manager.Create("host", Variant.American, "Alice");

        var left = _manager.Leave("host");

        Assert.That(left!.HasStarted, Is.False);
        Assert.That(_manager.Join(room!.Code, "guest", "Bob").Error, Is.EqualTo(OnlineProtocol.ErrorNotFound));
    }

    [Test]
    public void Leave_UnknownConnection_ReturnsNull() => Assert.That(_manager.Leave("nobody"), Is.Null);

    [Test]
    public void NormalizeName_TruncatesLongNames() =>
        Assert.That(OnlineProtocol.NormalizeName(new string('x', 100)), Has.Length.EqualTo(OnlineProtocol.MaxNameLength));

    [Test]
    public void MoveDto_RoundTripsSquares()
    {
        var dto = MoveDto.From(new Move(new[] { new Square(0, 5), new Square(2, 3) }, new[] { new Square(1, 4) }));

        Assert.That(dto.ToSquares(), Is.EqualTo(new[] { new Square(0, 5), new Square(2, 3) }));
    }
}
