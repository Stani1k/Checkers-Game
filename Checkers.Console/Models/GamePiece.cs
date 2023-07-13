using System;
using System.Collections.Generic;

namespace Checkers.Console.Models
{
    public class GamePiece
    {
        public ConsoleColor Color { get; }

        public Position CurrentPosition { get; set; }

        public List<Position> Moves { get; set; }

        public GamePiece(ConsoleColor color)
        {
            Color = color;
        }
    }
}