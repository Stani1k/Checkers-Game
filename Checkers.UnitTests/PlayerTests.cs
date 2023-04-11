using System;
using System.Collections.Generic;
using Checkers.Console;
using Checkers.Console.Interfaces;
using Moq;
using NUnit.Framework;

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
            _player = new Player(_view.Object);
            _player.SpielRichtung = 1;
            _player.Spielsteine = new List<Checker> { new Checker(ConsoleColor.Blue, new Position(1, 0, _player.SpielRichtung)), 
                new Checker(ConsoleColor.Blue, new Position(2, 1, _player.SpielRichtung)) };
        }

        [Test]
        [TestCase("1", 1)]
        [TestCase("2", -1)]
        public void WähleSeite_BeimAufruf_SetzeSpielRichtungVonSpieler1(string spieler1Seitenwahl, int erwartung)
        {
            // Act
            _player.WähleSeite(spieler1Seitenwahl);

            // Assert
            Assert.That(_player.SpielRichtung, Is.EqualTo(erwartung));
        }

        [Test]
        public void SucheSpielsteinFürStartKoordinaten_StartPositionEntsprichtPositionEinemChecker_ReturnChecker()
        { 
            var result = _player.SucheSpielsteinFürStartKoordinaten(new Position(1, 0, _player.SpielRichtung));

            Assert.That(result, Is.EqualTo(_player.Spielsteine[0]));
        }

        [Test]
        public void SucheSpielsteinFürStartKoordinaten_StartPositionEntsprichtKeinemChecker_ReturnNull()
        {
            var result = _player.SucheSpielsteinFürStartKoordinaten(new Position(3, 0, _player.SpielRichtung));

            Assert.That(result, Is.Null);
        }
    }
}