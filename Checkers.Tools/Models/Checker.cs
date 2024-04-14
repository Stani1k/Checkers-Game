namespace Checkers.Tools.Models
{
    public sealed class Checker : GamePiece
    {
        /// <summary>
        /// Contains all possible jump positions
        /// </summary>
        public List<Position> Jumps { get; }
        public int JumpCount => Jumps.Count;

        public bool IsQueen { get; private set; }

        public Checker(Colors color, Position currentPosition) : base(color)
        {
            CurrentPosition = currentPosition;
            Jumps = new List<Position>();
        }

        public void AddJump(Position jumpDestination)
        {
            if (Jumps.Contains(jumpDestination) == false)
            {
                Jumps.Add(jumpDestination);
            }
        }

        public void ClearJumps()
        {
            Jumps.Clear();
        }

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