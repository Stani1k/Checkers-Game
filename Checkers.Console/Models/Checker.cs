using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Console.Models
{
    public class Checker
    {
        public ConsoleColor Color { get; }

        public Position CurrentPosition { get; set; }

        public int Jumps { get; set; }

        public bool IsQueen { get; private set; }

        public List<Position> Moves { get; set; }

        public Checker(ConsoleColor color, Position currentPosition)
        {
            Color = color;
            CurrentPosition = currentPosition;
        }

        public Checker() {}


        public void SetQueen()
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

        /// <summary>
        /// American Checkers => Queen can only move one field forward and backwards
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public bool IsAmericanQueenMoveValid(Position destination)
        {
            if (CurrentPosition.XRight == destination.X || CurrentPosition.XLeft == destination.X)
            {
                if (CurrentPosition.NextYCoordinate == destination.Y || CurrentPosition.NextYCoordinateBackwards == destination.Y)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// European Checkers => Queen can move unlimited fields forward and backwards
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public bool IsEuropeanQueenMoveValid(Position destination)
        {
            return Moves.Where(currentPosition => currentPosition.X == destination.X)
                .Any(position => position.Y == destination.Y);
        }
    }
}