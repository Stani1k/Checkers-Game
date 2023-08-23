using System;
using System.Collections.Generic;

namespace Checkers.Console.Models
{
    public class GamePiece : Entity
    {
        public ConsoleColor Color { get; }

        public Position CurrentPosition { get; set; }

        public List<Position> Moves { get; set; }

        protected GamePiece(ConsoleColor color) : base(Guid.NewGuid())
        {
            Color = color;
        }
    }
}