namespace Checkers.Tools.Models
{
    /// <summary>
    /// Contains all possible coordinates, which are needed for a normal movement or a jump
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Current X-Coordinate
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Current Y-Coordinate
        /// </summary>
        public int Y { get; set; }
        public int XRight => X + 1;
        public int XLeft => X - 1;
        public int SecondXRight => X + 2;
        public int SecondXLeft => X - 2;
        public int NextYCoordinate { get; set; }
        public int SecondNextYCoordinate { get; set; }
        public int NextYCoordinateBackwards { get; set; }
        public int SecondYCoordinateBackwards { get; set; }
        public JumpDestination RightBackwardsJump { get; set; }
        public JumpDestination LeftBackwardsJump { get; set; }
        public JumpDestination RightJump { get; set; }
        public JumpDestination LeftJump { get; set; }

        public Position(int x, int y, GameDirection gameDirection)
        {
            X = x;
            Y = y;
            var gameDirectionNumber = gameDirection switch
            {
                GameDirection.Top => 1,
                GameDirection.Bottom => 2,
                _ => 0
            };

            NextYCoordinate = y + gameDirectionNumber;
            SecondNextYCoordinate = y + 2 * gameDirectionNumber;
            NextYCoordinateBackwards = y - gameDirectionNumber;
            SecondYCoordinateBackwards = y - 2 * gameDirectionNumber;

            RightBackwardsJump = new JumpDestination(XRight, NextYCoordinateBackwards, SecondXRight, SecondYCoordinateBackwards);
            LeftBackwardsJump = new JumpDestination(XLeft, NextYCoordinateBackwards, SecondXLeft, SecondYCoordinateBackwards);
            RightJump = new JumpDestination(XRight, NextYCoordinate, SecondXRight, SecondNextYCoordinate);
            LeftJump = new JumpDestination(XLeft, NextYCoordinate, SecondXLeft, SecondNextYCoordinate);
        }

        protected bool Equals(Position other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Position) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public bool IsWhiteField(Position destination)
        {
            var isWhiteField = (destination.X + destination.Y) % 2 == 0;
            return isWhiteField;
        }
    }
}
