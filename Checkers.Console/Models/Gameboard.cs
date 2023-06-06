namespace Checkers.Console.Models
{
    public class Gameboard
    {
        private readonly Settings _settings;

        public Gameboard(Settings settings)
        {
            _settings = settings;
        }

        public int ColumnCount => _settings.Variant == Variant.Amerikanisch ? 10 : 8;
        public int RowCount => _settings.Variant == Variant.Amerikanisch ? 10 : 8;

        /// <summary>
        /// Starting checker number for upper row
        /// </summary>
        public int StartingUpperRow => _settings.Variant == Variant.Amerikanisch ? 4 : 3;

        /// <summary>
        /// Starting checker number for lower row
        /// </summary>
        public int StartingLowerRow => _settings.Variant == Variant.Amerikanisch ? 5 : 4;

        public int LastPlayableRowNumber => _settings.Variant == Variant.Amerikanisch ? 9 : 7;
        public int FirstPlayableRowNumber => 0;

        public bool IsDiagonalFieldEmpty(int xCoordinate, int yCoordinate, Player player)
        {
            foreach (var checker in player.Checkers)
            {
                // When x- or y-Coordinates are outside the game area they are marked as not empty
                if (xCoordinate > LastPlayableRowNumber || xCoordinate < FirstPlayableRowNumber)
                {
                    return false;
                }
                if (yCoordinate > LastPlayableRowNumber || yCoordinate < FirstPlayableRowNumber)
                {
                    return false;
                }
                // Check if provided position is already occupied by another Checker
                if (checker.CurrentPosition.Equals(new Position(xCoordinate, yCoordinate, player.GameDirection)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}