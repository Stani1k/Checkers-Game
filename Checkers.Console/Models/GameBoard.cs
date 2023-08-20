namespace Checkers.Console.Models
{
    public class GameBoard
    {
        private const int RowStartNumber = 0;
        public static int FieldWidth = 7;
        public static int FieldHeight = 3;

        public Variant Variant { get; private set; }

        public int ColumnCount => Variant == Variant.American ? 10 : 8;
        public int RowCount => Variant == Variant.American ? 10 : 8;
        /// <summary>
        /// Row number for placing Checkers for Player 1
        /// </summary>
        public int StartingUpperRow => Variant == Variant.American ? 4 : 3;
        /// <summary>
        /// Row number for placing Checkers for Player 2
        /// </summary>
        public int StartingLowerRow => Variant == Variant.American ? 5 : 4;
        public int RowEndNumber => Variant == Variant.American ? 9 : 7;

        public GameBoard(Variant variant)
        {
            Variant = variant;
        }

        public void SetGameVariant(Variant variant)
        {
            Variant = variant;
        }

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