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
            _player = new Player("Max", 1);
            var checkersToTest = new List<Checker> { new Checker(ConsoleColor.Blue, new Position(1, 0, _player.GameDirection)), new Checker(ConsoleColor.Blue, new Position(2, 1, _player.GameDirection)) };
            _player.CreateCheckers(checkersToTest);

            _checker = new Checker(ConsoleColor.Blue, new Position(1, 2, _player.GameDirection)) {Moves = new List<Position> {new Position(2, 3, _player.GameDirection)}};
        }


        [Test]
        public void UmwandlungSpielfigurInDame_CheckerIstDame_PropertyIstEineDameIstTrue()
        {
            // Act
            _checker.SetQueen();

            // Assert
            Assert.That(_checker.IsQueen, Is.True);
        }

        [Test]
        public void PrüfeBewegung_ZielPositionIstGültig_ReturnTrue()
        {
            var ergebnis = _checker.IsNextPositionValid(new Position(2, 3, _player.GameDirection));

            Assert.That(ergebnis, Is.True);
        }

        [Test]
        public void PrüfeBewegung_ZielPositionIstUnGültig_ReturnFalse()
        {
            var ergebnis = _checker.IsNextPositionValid(new Position(1, 3, _player.GameDirection));

            Assert.That(ergebnis, Is.False);
        }

        [Test]
        public void PrüfeBewegungAmerikanischerDame_ZielPositionIstGültig_ReturnTrue()
        {
            // Arrange
            _checker.SetQueen();

            // Act
            var ergebnis = _checker.IsAmericanQueenMoveValid(new Position(0, 1, _player.GameDirection));

            // Assert
            Assert.That(ergebnis, Is.True);
        }

        [Test]
        public void PrüfeBewegungEuropäischerDame_ZielPositionIstGültig_ReturnTrue()
        {
            var ergebnis = _checker.IsEuropeanQueenMoveValid(new Position(2, 3, _player.GameDirection));

            Assert.That(ergebnis, Is.True);
        }
    }
}