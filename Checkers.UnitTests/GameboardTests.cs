using Checkers.Tools.Models;

namespace Checkers.UnitTests
{
    [TestFixture]
    internal class GameboardTests
    {
        private GameBoard board;

        [SetUp]
        public void SetUp()
        {
            board = new GameBoard(Variant.American);
        }

        [Test]
        [TestCase(Variant.American, Variant.American)]
        [TestCase(Variant.European, Variant.European)]
        public void SetGameVariant_WhenCalled_SetSelectedVariant(Variant selectedVariant, Variant expectedVariant)
        {
            // Act
            board.SetGameVariant(selectedVariant);

            // Assert
            Assert.That(board.Variant, Is.EqualTo(expectedVariant));
        }
    }
}
