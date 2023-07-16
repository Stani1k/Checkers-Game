using System;
using System.Collections.Generic;
using System.Linq;
using Checkers.Console.Views;

namespace Checkers.Console.Models
{
    public class Player
    {
        private readonly IView _view;
        public string Name { get; set; }
        public int Nummer { get; set; }
        public ConsoleColor CheckersColor { get; set; }
        // Todo: Is this property really needed? Refactor to Enum?
        public int GameDirection { get; set; }
        public List<Checker> Checkers { get; }

        public Player(IView view = null)
        {
            _view = view ?? new View();
            Checkers = new List<Checker>();
        }

        public void SetGameDirection(string direction)
        {
            switch (direction)
            {
                case "1":
                    GameDirection = 1;
                    break;

                case "2":
                    GameDirection = -1;
                    break;
            }
        }

        public void CreateCheckers(GameBoard gameBoard)
        {
            for (var y = 0; y < gameBoard.RowCount; y++)
            {
                for (var x = 0; x < gameBoard.ColumnCount; x++)
                {
                    if (GameDirection == 1 && y < gameBoard.StartingUpperRow || 
                        GameDirection == -1 && y > gameBoard.StartingLowerRow)
                    {
                        // "!=" means that the checkers are positioned on a black square
                        if ((x + y) % 2 != 0)
                        {
                            Checker checker = new(CheckersColor, new Position(x, y, GameDirection));
                            Checkers.Add(checker);
                        }
                    }
                }
            }
        }

        public bool IsSurrendering(Position coordinates)
        {
            if (coordinates == null) return true;
            return false;
        }

        public bool IsPositionOccupied(Position destination)
        {
            return Checkers.Any(checker => destination.Equals(checker.CurrentPosition));
        }

        public Checker ChooseChecker(Player player, List<Checker> checkersWithMaxJumps)
        {
            Checker checker;
            do
            {
                var startingPosition = _view.GetPosition(true, player);
                checker = CheckSelectedChecker(checkersWithMaxJumps, startingPosition);
            } while (checker == null);

            return checker;
        }

        private Checker CheckSelectedChecker(List<Checker> checkers, Position startPosition)
        {
            foreach (var checker in checkers)
            {
                if (startPosition.Equals(checker.CurrentPosition))
                {
                    return checker;
                }
            }

            return null;
        }

        public Checker GetCheckerWithMatchingCoordinates(Position startPosition)
        {
            foreach (var checker in Checkers)
            {
                if (checker.CurrentPosition.Equals(startPosition))
                {
                    return checker;
                }
            }
            return null;
        }
    }
}