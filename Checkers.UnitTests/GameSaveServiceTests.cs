using System.IO;
using System.Linq;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class GameSaveServiceTests
    {
        private string _directory = string.Empty;
        private GameSaveService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "checkers-tests-" + Guid.NewGuid().ToString("N"));
            _service = new GameSaveService(Path.Combine(_directory, "save.json"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        private static CheckersGame PlayedGame(Variant variant)
        {
            var game = new CheckersGame(variant);
            for (var i = 0; i < 3; i++)
            {
                game.Apply(game.GetLegalMoves()[0]);
            }

            return game;
        }

        [TestCase(Variant.American)]
        [TestCase(Variant.European)]
        public void SaveAndLoad_RestoresPosition(Variant variant)
        {
            var game = PlayedGame(variant);

            _service.Save(GameSaveData.From(game, "Ann", "Bob"));
            var loaded = _service.TryLoad(out var data, out var restored, out _);

            Assert.That(loaded, Is.True);
            Assert.That(data!.BottomPlayerName, Is.EqualTo("Ann"));
            Assert.That(data.TopPlayerName, Is.EqualTo("Bob"));
            Assert.That(restored!.Variant, Is.EqualTo(variant));
            Assert.That(restored.Turn, Is.EqualTo(game.Turn));
            Assert.That(restored.QuietPlies, Is.EqualTo(game.QuietPlies));
            Assert.That(restored.GetPieces().OrderBy(p => p.Square.Row).ThenBy(p => p.Square.Col),
                Is.EqualTo(game.GetPieces().OrderBy(p => p.Square.Row).ThenBy(p => p.Square.Col)));
            Assert.That(restored.LastMove!.Path, Is.EqualTo(game.LastMove!.Path));
            Assert.That(Paths(restored), Is.EqualTo(Paths(game)));
        }

        [Test]
        public void SaveAndLoad_KeepsKings()
        {
            var game = CheckersGame.FromPieces(Variant.American, Side.Top, new[]
            {
                (new Square(1, 0), new Piece(Side.Bottom, true)),
                (new Square(4, 3), new Piece(Side.Top, false))
            }, 7);

            _service.Save(GameSaveData.From(game, "A", "B"));
            _service.TryLoad(out _, out var restored, out _);

            Assert.That(restored!.PieceAt(new Square(1, 0)), Is.EqualTo(new Piece(Side.Bottom, true)));
            Assert.That(restored.QuietPlies, Is.EqualTo(7));
            Assert.That(restored.Turn, Is.EqualTo(Side.Top));
        }

        [Test]
        public void TryLoad_MissingFile_ReturnsFalse()
        {
            Assert.That(_service.Exists, Is.False);
            Assert.That(_service.TryLoad(out _, out _, out _), Is.False);
        }

        [TestCase("not json")]
        [TestCase("{\"Version\":1,\"Variant\":\"American\",\"Turn\":\"Bottom\",\"Pieces\":[{\"Col\":0,\"Row\":0,\"Side\":\"Top\"}]}")]
        [TestCase("{\"Version\":99}")]
        public void TryLoad_InvalidFile_ReturnsFalse(string content)
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_service.FilePath, content);

            Assert.That(_service.TryLoad(out var data, out var game, out var error), Is.False);
            Assert.That(data, Is.Null);
            Assert.That(game, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        private static List<string> Paths(CheckersGame game) =>
            game.GetLegalMoves().Select(m => string.Join(" ", m.Path.Select(game.ToNotation))).OrderBy(s => s).ToList();
    }
}
