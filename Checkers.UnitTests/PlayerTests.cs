using Checkers.Tools;
using Checkers.Tools.Models;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class PlayerTests
    {
        private Player _player;
        private Mock<IView> _view;
        private bool result;

        [SetUp]
        public void SetUp()
        {
            _view = new Mock<IView>();
            _player = new Player("Max", 1, _view.Object);
            _player.CreateCheckers(new List<Checker> { new Checker(ConsoleColor.Blue, new Position(1, 0, _player.GameDirection)),
                new Checker(ConsoleColor.Blue, new Position(2, 1, _player.GameDirection)) });
        }

        [Test]
        public void IsPositionOccupied_IsOccupied_ReturnTrue()
        {
            // Act
            result = _player.IsPositionOccupied(new Position(1, 0, _player.GameDirection));

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetCheckerWithMatchingCoordinates_PositionContainsChecker_ReturnChecker()
        { 
            var result = _player.GetCheckerWithMatchingCoordinates(new Position(1, 0, _player.GameDirection));

            Assert.That(result, Is.EqualTo(_player.Checkers[0]));
        }

        [Test]
        public void GetCheckerWithMatchingCoordinates_NoCheckerAtPosition_ReturnNull()
        {
            var result = _player.GetCheckerWithMatchingCoordinates(new Position(3, 0, _player.GameDirection));

            Assert.That(result, Is.Null);
        }
    }
}