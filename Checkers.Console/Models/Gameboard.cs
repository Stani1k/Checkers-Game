namespace Checkers.Console.Models
{
    public class GameBoard
    {
        private readonly Settings settings;

        public GameBoard(Settings settings)
        {
            this.settings = settings;
        }

        public int ColumnCount => settings.Variant == Variant.American ? 10 : 8;
        public int RowCount => settings.Variant == Variant.American ? 10 : 8;
        /// <summary>
        /// Row number for placing Checkers for Player 1
        /// </summary>
        public int StartingUpperRow => settings.Variant == Variant.American ? 4 : 3;
        /// <summary>
        /// Row number for placing Checkers for Player 2
        /// </summary>
        public int StartingLowerRow => settings.Variant == Variant.American ? 5 : 4;
        public int RowEndNumber => settings.Variant == Variant.American ? 9 : 7;
        public const int RowStartNumber = 0;

        public bool IsDiagonalFieldEmpty(int x, int y, Player player)
        {
            foreach (var checker in player.Checkers)
            {
                // Coordinates are out of boundary
                if (x > RowEndNumber || x < RowStartNumber || 
                    y > RowEndNumber || y < RowStartNumber)
                {
                    return false;
                }
                
                // Destination coordinates are occupied
                if (checker.CurrentPosition.Equals(new Position(x, y, player.GameDirection)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}