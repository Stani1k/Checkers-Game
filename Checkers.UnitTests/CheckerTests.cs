namespace Checkers.UnitTests
{
    [TestFixture]
    public class CheckerTests
    {
        private Checker _checker;
        private Player _player;
        private Mock<IView> _view;
        private bool result;

        [SetUp]
        public void SetUp()
        {
            _view = new Mock<IView>();
            _player = new Player("Max", 1);
            var checkersToTest = new List<Checker> {
                new Checker(ConsoleColor.Blue, new Position(1, 0, _player.GameDirection)), 
                new Checker(ConsoleColor.Blue, new Position(2, 1, _player.GameDirection)) 
            };
            _player.CreateCheckers(checkersToTest);

            _checker = new Checker(ConsoleColor.Blue, new Position(1, 2, _player.GameDirection)) {Moves = new List<Position> {new Position(2, 3, _player.GameDirection)}};
        }


        [Test]
        public void SetQueen_IsQueen_PropertyEqualsTrue()
        {
            // Act
            _checker.SetQueen();

            // Assert
            Assert.That(_checker.IsQueen, Is.True);
        }

        [Test]
        public void IsNextPositionValid_PositionIsValid_ReturnTrue()
        {
            result = _checker.IsNextPositionValid(new Position(2, 3, _player.GameDirection));

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsNextPositionValid_PositionIsNotValid_ReturnFalse()
        {
            result = _checker.IsNextPositionValid(new Position(1, 3, _player.GameDirection));

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsAmericanQueenMoveValid_PositionIsValid_ReturnTrue()
        {
            _checker.SetQueen();

            result = _checker.IsAmericanQueenMoveValid(new Position(0, 1, _player.GameDirection));

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsEuropeanQueenMoveValid_PositionIsValid_ReturnTrue()
        {
            result = _checker.IsEuropeanQueenMoveValid(new Position(2, 3, _player.GameDirection));

            Assert.That(result, Is.True);
        }
    }
}