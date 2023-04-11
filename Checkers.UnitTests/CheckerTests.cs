using System;
using System.Collections.Generic;
using Checkers.Console;
using Checkers.Console.Models;
using Checkers.Console.Views;
using Moq;
using NUnit.Framework;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class CheckerTests
    {
        private Checker _checker;
        private Player _player;
        private Mock<IView> _view;

        [SetUp]
        public void SetUp()
        {
            _view = new Mock<IView>();
            _player = new Player{SpielRichtung = 1};
            _player.Spielsteine = new List<Checker> { new Checker(ConsoleColor.Blue, new Position(1, 0, _player.SpielRichtung)), new Checker(ConsoleColor.Blue, new Position(2, 1, _player.SpielRichtung)) };

            _checker = new Checker(ConsoleColor.Blue, new Position(1, 2, _player.SpielRichtung)) {Moves = new List<Position> {new Position(2, 3, _player.SpielRichtung)}};
        }

        [Test]
        public void IstEinCheckerBereitsAufDemFeld_FeldIstBesetzt_ReturnTrue()
        {
            // Act
            var ergebnis = _checker.IsFieldAlreadyOccupied(_player, new Position(1, 0, _player.SpielRichtung));

            // Assert
            Assert.That(ergebnis, Is.True);
        }

        [Test]
        public void UmwandlungSpielfigurInDame_CheckerIstDame_PropertyIstEineDameIstTrue()
        {
            // Act
            _checker.TransformCheckerInQueen();

            // Assert
            Assert.That(_checker.IsQueen, Is.True);
        }

        [Test]
        public void PrüfeBewegung_ZielPositionIstGültig_ReturnTrue()
        {
            var ergebnis = _checker.IsNextPositionValid(new Position(2, 3, _player.SpielRichtung));

            Assert.That(ergebnis, Is.True);
        }

        [Test]
        public void PrüfeBewegung_ZielPositionIstUnGültig_ReturnFalse()
        {
            var ergebnis = _checker.IsNextPositionValid(new Position(1, 3, _player.SpielRichtung));

            Assert.That(ergebnis, Is.False);
        }

        [Test]
        public void PrüfeBewegungAmerikanischerDame_ZielPositionIstGültig_ReturnTrue()
        {
            // Arrange
            _checker.IsQueen = true;

            // Act
            var ergebnis = _checker.IsAmericanQueenMoveValid(new Position(0, 1, _player.SpielRichtung));

            // Assert
            Assert.That(ergebnis, Is.True);
        }

        [Test]
        public void PrüfeBewegungEuropäischerDame_ZielPositionIstGültig_ReturnTrue()
        {
            var ergebnis = _checker.IsEuropeanQueenMoveValid(new Position(2, 3, _player.SpielRichtung));

            Assert.That(ergebnis, Is.True);
        }
    }
}