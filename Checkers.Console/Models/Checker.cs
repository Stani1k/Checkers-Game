using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Console.Models
{
    public class Checker
    {
        public ConsoleColor Color { get; }

        public Position CurrentPosition { get; set; }

        public int PossibleJumpCount { get; set; }

        public bool IsQueen { get; set; }

        public List<Position> Moves { get; set; }

        public Checker(ConsoleColor color, Position currentPosition)
        {
            Color = color;
            CurrentPosition = currentPosition;
        }

        public Checker() {}

        public bool IsFieldAlreadyOccupied(Player player, Position destination)
        {
            return player.Checkers.Any(checker => destination.Equals(checker.CurrentPosition));
        }

        public void TransformCheckerToQueen()
        {
            IsQueen = true;
        }

        public bool IsNextPositionValid(Position destination)
        {
            if (CurrentPosition.XRight == destination.X ||
                CurrentPosition.XLeft == destination.X)
            {
                if (CurrentPosition.NextYCoordinate == destination.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAmericanQueenMoveValid(Position destination)
        {
            // American Checkers: Queen can only move one field forward and backwards
            if (CurrentPosition.XRight == destination.X || CurrentPosition.XLeft == destination.X)
            {
                if (CurrentPosition.NextYCoordinate == destination.Y || CurrentPosition.NextYCoordinateBackwards == destination.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsEuropeanQueenMoveValid(Position destination)
        {
            // European Checkers: Queen can move unlimited fields forward and backwards
            return Moves
                .Where(currentPosition => currentPosition.X == destination.X)
                .Any(position => position.Y == destination.Y);
        }
    }
}