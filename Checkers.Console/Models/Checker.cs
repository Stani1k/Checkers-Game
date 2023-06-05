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

        public void TransformCheckerInQueen()
        {
            IsQueen = true;
        }

        public bool IsNextPositionValid(Position destination)
        {
            if (CurrentPosition.XKoordinateRechts == destination.XKoordinate ||
                CurrentPosition.XKoordinateLinks == destination.XKoordinate)
            {
                if (CurrentPosition.NächsteYKoordinate == destination.YKoordinate)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAmericanQueenMoveValid(Position destination)
        {
            // American Checkers: Queen can only move one field forward and backwards
            if (CurrentPosition.XKoordinateRechts == destination.XKoordinate || CurrentPosition.XKoordinateLinks == destination.XKoordinate)
            {
                if (CurrentPosition.NächsteYKoordinate == destination.YKoordinate || CurrentPosition.NächsteYKoordinateRückwärts == destination.YKoordinate)
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
                .Where(currentPosition => currentPosition.XKoordinate == destination.XKoordinate)
                .Any(position => position.YKoordinate == destination.YKoordinate);
        }
    }
}