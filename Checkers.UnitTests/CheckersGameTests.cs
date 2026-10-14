using System.Linq;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class CheckersGameTests
    {
        private static Square Sq(CheckersGame game, string notation)
        {
            Assert.That(game.TryParseSquare(notation, out var square), Is.True, notation);
            return square;
        }

        private static (Square, Piece) P(CheckersGame reference, string notation, Side side, bool king = false) =>
            (Sq(reference, notation), new Piece(side, king));

        private static CheckersGame Custom(Variant variant, Side turn, params (string Square, Side Side, bool King)[] pieces)
        {
            var reference = new CheckersGame(variant);
            return CheckersGame.FromPieces(variant, turn, pieces.Select(p => P(reference, p.Square, p.Side, p.King)));
        }

        private static List<string> Paths(CheckersGame game) =>
            game.GetLegalMoves().Select(m => string.Join(" ", m.Path.Select(game.ToNotation))).OrderBy(s => s).ToList();

        [TestCase(Variant.American, 8, 12)]
        [TestCase(Variant.European, 10, 20)]
        public void NewGame_HasStandardSetup(Variant variant, int size, int piecesPerSide)
        {
            var game = new CheckersGame(variant);

            Assert.That(game.Size, Is.EqualTo(size));
            Assert.That(game.CountPieces(Side.Bottom), Is.EqualTo(piecesPerSide));
            Assert.That(game.CountPieces(Side.Top), Is.EqualTo(piecesPerSide));
            Assert.That(game.Turn, Is.EqualTo(Side.Bottom));
            Assert.That(game.Status, Is.EqualTo(GameStatus.InProgress));
        }

        [Test]
        public void NewAmericanGame_BottomLeftSquareIsPlayableAndOccupied()
        {
            var game = new CheckersGame(Variant.American);

            Assert.That(game.PieceAt(Sq(game, "a1")), Is.EqualTo(new Piece(Side.Bottom, false)));
            Assert.That(game.PieceAt(Sq(game, "b8")), Is.EqualTo(new Piece(Side.Top, false)));
            Assert.That(game.PieceAt(Sq(game, "b1")), Is.Null);
        }

        [TestCase(Variant.American, 7)]
        [TestCase(Variant.European, 9)]
        public void NewGame_FirstPlayerHasLegalMoves(Variant variant, int moveCount)
        {
            var game = new CheckersGame(variant);

            Assert.That(game.GetLegalMoves(), Has.Count.EqualTo(moveCount));
        }

        [Test]
        public void Man_MovesOnlyForward()
        {
            var game = Custom(Variant.American, Side.Bottom, ("d4", Side.Bottom, false), ("h8", Side.Top, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "d4 c5", "d4 e5" }));
        }

        [Test]
        public void Capture_IsMandatory()
        {
            var game = Custom(Variant.American, Side.Bottom, ("c3", Side.Bottom, false), ("d4", Side.Top, false), ("h2", Side.Bottom, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "c3 e5" }));
        }

        [Test]
        public void Apply_Capture_RemovesCapturedPiece()
        {
            var game = Custom(Variant.American, Side.Bottom, ("c3", Side.Bottom, false), ("d4", Side.Top, false), ("h8", Side.Top, false));

            Assert.That(game.TryResolveMove(new[] { Sq(game, "c3"), Sq(game, "e5") }, out var move, out _), Is.True);
            game.Apply(move!);

            Assert.That(game.PieceAt(Sq(game, "d4")), Is.Null);
            Assert.That(game.PieceAt(Sq(game, "e5")), Is.EqualTo(new Piece(Side.Bottom, false)));
            Assert.That(game.CountPieces(Side.Top), Is.EqualTo(1));
            Assert.That(game.Turn, Is.EqualTo(Side.Top));
        }

        [Test]
        public void MultiJump_IsOneMove_AndCanBeGivenAsStartAndEnd()
        {
            var game = Custom(Variant.American, Side.Bottom,
                ("c3", Side.Bottom, false), ("d4", Side.Top, false), ("f6", Side.Top, false), ("h8", Side.Top, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "c3 e5 g7" }));
            Assert.That(game.TryResolveMove(new[] { Sq(game, "c3"), Sq(game, "g7") }, out var move, out _), Is.True);
            game.Apply(move!);

            Assert.That(game.CountPieces(Side.Top), Is.EqualTo(1));
        }

        [Test]
        public void MultiJump_CannotJumpSamePieceTwice()
        {
            // Square of 4 pieces around an empty centre would allow circular jumps if captured pieces could be re-jumped.
            var game = Custom(Variant.European, Side.Bottom,
                ("e1", Side.Bottom, true), ("f2", Side.Top, false), ("f4", Side.Top, false), ("d4", Side.Top, false), ("d2", Side.Top, false));

            var moves = game.GetLegalMoves();

            Assert.That(moves, Is.Not.Empty);
            foreach (var move in moves)
            {
                Assert.That(move.Captured.Distinct().Count(), Is.EqualTo(move.Captured.Count));
            }
        }

        [Test]
        public void American_ManCannotCaptureBackwards()
        {
            var game = Custom(Variant.American, Side.Bottom, ("d4", Side.Bottom, false), ("c3", Side.Top, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "d4 c5", "d4 e5" }));
        }

        [Test]
        public void European_ManCapturesBackwards()
        {
            var game = Custom(Variant.European, Side.Bottom, ("d4", Side.Bottom, false), ("c3", Side.Top, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "d4 b2" }));
        }

        [Test]
        public void American_ManReachingKingRow_BecomesKingAndTurnEnds()
        {
            var game = Custom(Variant.American, Side.Bottom, ("c7", Side.Bottom, false), ("h2", Side.Top, false));

            Assert.That(game.TryResolveMove(new[] { Sq(game, "c7"), Sq(game, "b8") }, out var move, out _), Is.True);
            game.Apply(move!);

            Assert.That(game.PieceAt(Sq(game, "b8")), Is.EqualTo(new Piece(Side.Bottom, true)));
        }

        [Test]
        public void American_CapturingManThatReachesKingRow_IsCrownedAndStops()
        {
            // After landing on h8 the new king could capture g5 backwards, but the turn must end.
            var game = Custom(Variant.American, Side.Bottom,
                ("f6", Side.Bottom, false), ("g7", Side.Top, false), ("g5", Side.Top, false), ("a1", Side.Top, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "f6 h8" }));
            game.Apply(game.GetLegalMoves()[0]);

            Assert.That(game.PieceAt(Sq(game, "h8")), Is.EqualTo(new Piece(Side.Bottom, true)));
            Assert.That(game.PieceAt(Sq(game, "g5")), Is.Not.Null);
        }
        [Test]
        public void European_KingIsFlying()
        {
            var game = Custom(Variant.European, Side.Bottom, ("a1", Side.Bottom, true), ("j10", Side.Top, false));

            var paths = Paths(game);

            Assert.That(paths, Does.Contain("a1 e5"));
            Assert.That(paths, Does.Contain("a1 i9"));
            Assert.That(paths, Has.Count.EqualTo(8));
        }

        [Test]
        public void European_FlyingKingCapturesFromDistance_AndLandsOnAnyFreeSquareBehind()
        {
            var game = Custom(Variant.European, Side.Bottom, ("a1", Side.Bottom, true), ("d4", Side.Top, false), ("j10", Side.Top, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "a1 e5", "a1 f6", "a1 g7", "a1 h8", "a1 i9" }));
        }

        [Test]
        public void European_MaximumCaptureRuleIsEnforced()
        {
            // The king on a1 can capture one piece on the long diagonal, but the man on a3 has a double capture available.
            var game = Custom(Variant.European, Side.Bottom,
                ("a1", Side.Bottom, true), ("e5", Side.Top, false),
                ("a3", Side.Bottom, false), ("b4", Side.Top, false), ("d6", Side.Top, false));

            var moves = game.GetLegalMoves();

            Assert.That(moves.All(m => m.Captured.Count == 2), Is.True);
            Assert.That(moves.Any(m => m.From == Sq(game, "a3")), Is.True);
        }

        [Test]
        public void American_NoMaximumCaptureRule()
        {
            var game = Custom(Variant.American, Side.Bottom,
                ("a1", Side.Bottom, false), ("b2", Side.Top, false),
                ("e3", Side.Bottom, false), ("f4", Side.Top, false), ("f6", Side.Top, false), ("h8", Side.Top, false));

            var captureCounts = game.GetLegalMoves().Select(m => m.Captured.Count).ToList();

            Assert.That(captureCounts, Does.Contain(1));
            Assert.That(captureCounts, Does.Contain(2));
        }

        [Test]
        public void American_KingMovesAndCapturesInAllDirections_OneStep()
        {
            var game = Custom(Variant.American, Side.Bottom, ("d4", Side.Bottom, true), ("h8", Side.Top, false));

            Assert.That(Paths(game), Is.EqualTo(new[] { "d4 c3", "d4 c5", "d4 e3", "d4 e5" }));
        }

        [Test]
        public void Status_PlayerWithoutPiecesLoses()
        {
            var game = Custom(Variant.American, Side.Bottom, ("c3", Side.Bottom, false), ("d4", Side.Top, false));

            game.Apply(game.GetLegalMoves().Single());

            Assert.That(game.Status, Is.EqualTo(GameStatus.BottomWins));
            Assert.That(game.GetLegalMoves(), Is.Empty);
        }

        [Test]
        public void Status_PlayerWithoutLegalMovesLoses()
        {
            // The top man on b8 is blocked by two bottom men and cannot jump.
            var game = Custom(Variant.American, Side.Top, ("b8", Side.Top, false), ("a7", Side.Bottom, false), ("c7", Side.Bottom, false), ("d6", Side.Bottom, false));

            Assert.That(game.Status, Is.EqualTo(GameStatus.BottomWins));
        }

        [Test]
        public void Apply_IllegalMove_Throws()
        {
            var game = new CheckersGame(Variant.American);
            var illegal = new Move(new[] { Sq(game, "a1"), Sq(game, "a2") }, Array.Empty<Square>());

            Assert.Throws<InvalidOperationException>(() => game.Apply(illegal));
        }

        [Test]
        public void TryResolveMove_GivesUsefulErrors()
        {
            var game = new CheckersGame(Variant.American);

            Assert.That(game.TryResolveMove(new[] { Sq(game, "c4"), Sq(game, "d5") }, out _, out var noPiece), Is.False);
            Assert.That(noPiece, Does.Contain("no piece"));
            Assert.That(game.TryResolveMove(new[] { Sq(game, "b6"), Sq(game, "a5") }, out _, out var notYours), Is.False);
            Assert.That(notYours, Does.Contain("not yours"));
            Assert.That(game.TryResolveMove(new[] { Sq(game, "a3"), Sq(game, "c5") }, out _, out var illegal), Is.False);
            Assert.That(illegal, Does.Contain("Illegal"));
        }

        [TestCase("a1", true)]
        [TestCase("h8", true)]
        [TestCase("i1", false)]
        [TestCase("a0", false)]
        [TestCase("", false)]
        [TestCase("xx", false)]
        public void TryParseSquare_ValidatesInput(string text, bool valid)
        {
            var game = new CheckersGame(Variant.American);

            Assert.That(game.TryParseSquare(text, out _), Is.EqualTo(valid));
        }

        [Test]
        public void Draw_AfterManyKingOnlyMovesWithoutCapture()
        {
            var game = Custom(Variant.American, Side.Bottom, ("a1", Side.Bottom, true), ("h8", Side.Top, true));

            var shuffle = new Dictionary<string, string> { ["a1"] = "b2", ["b2"] = "a1", ["h8"] = "g7", ["g7"] = "h8" };
            for (var i = 0; i < 200 && game.Status == GameStatus.InProgress; i++)
            {
                var move = game.GetLegalMoves().Single(m => shuffle[game.ToNotation(m.From)] == game.ToNotation(m.To));
                game.Apply(move);
            }
            Assert.That(game.Status, Is.EqualTo(GameStatus.Draw));
        }

        [Test]
        public void RandomPlayouts_NeverThrow_AndAlwaysEnd()
        {
            foreach (var variant in new[] { Variant.American, Variant.European })
            {
                for (var seed = 0; seed < 30; seed++)
                {
                    var random = new Random(seed);
                    var game = new CheckersGame(variant);
                    var plies = 0;
                    while (game.Status == GameStatus.InProgress && plies++ < 2000)
                    {
                        var moves = game.GetLegalMoves();
                        Assert.That(moves, Is.Not.Empty);
                        if (moves.Any(m => m.IsCapture))
                        {
                            Assert.That(moves.All(m => m.IsCapture), Is.True);
                        }

                        game.Apply(moves[random.Next(moves.Count)]);
                    }

                    Assert.That(game.Status, Is.Not.EqualTo(GameStatus.InProgress), $"{variant} seed {seed}");
                }
            }
        }
    }
}
