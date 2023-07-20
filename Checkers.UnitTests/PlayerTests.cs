namespace Checkers.UnitTests
{
    [TestFixture]
    public class PlayerTests
    {
        private Player _player;
        private Mock<IView> _view;
        [SetUp]
        public void SetUp()
        {
            _view = new Mock<IView>();
            _player = new Player("Max", 1, _view.Object);
            _player.CreateCheckers(new List<Checker> { new Checker(ConsoleColor.Blue, new Position(1, 0, _player.GameDirection)),
                new Checker(ConsoleColor.Blue, new Position(2, 1, _player.GameDirection)) });
        }

        [Test]
        public void IstEinCheckerBereitsAufDemFeld_FeldIstBesetzt_ReturnTrue()
        {
            // Act
            var ergebnis = _player.IsPositionOccupied(new Position(1, 0, _player.GameDirection));

            // Assert
            Assert.That(ergebnis, Is.True);
        }

        [Test]
        [TestCase("1", 1)]
        [TestCase("2", -1)]
        public void WähleSeite_BeimAufruf_SetzeSpielRichtungVonSpieler1(string spieler1Seitenwahl, int erwartung)
        {
            // Act
            _player.SetGameDirection(spieler1Seitenwahl);

            // Assert
            Assert.That(_player.GameDirection, Is.EqualTo(erwartung));
        }

        [Test]
        public void SucheSpielsteinFürStartKoordinaten_StartPositionEntsprichtPositionEinemChecker_ReturnChecker()
        { 
            var result = _player.GetCheckerWithMatchingCoordinates(new Position(1, 0, _player.GameDirection));

            Assert.That(result, Is.EqualTo(_player.Checkers[0]));
        }

        [Test]
        public void SucheSpielsteinFürStartKoordinaten_StartPositionEntsprichtKeinemChecker_ReturnNull()
        {
            var result = _player.GetCheckerWithMatchingCoordinates(new Position(3, 0, _player.GameDirection));

            Assert.That(result, Is.Null);
        }
    }
}