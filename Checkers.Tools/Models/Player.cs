namespace Checkers.Tools.Models
{
    public class Player
    {
        private readonly IView _view;

        public string Name { get; }
        public int Number { get; set; }
        public ConsoleColor CheckersColor { get; set; }
        public GameDirection GameDirection { get; private set; }
        public List<Checker> Checkers { get; }

        public Player(string name, GameDirection gameDirection, IView view = null)
        {
            _view = view;
            Name = name;
            GameDirection = gameDirection;
            Checkers = new List<Checker>();
        }

        public void SetGameDirection(string direction)
        {
            GameDirection = direction switch
            {
                "1" => GameDirection.Top,
                "2" => GameDirection.Bottom,
                _ => GameDirection
            };
        }

        public void CreateCheckers(GameBoard gameBoard)
        {
            for (var y = 0; y < gameBoard.RowCount; y++)
            {
                for (var x = 0; x < gameBoard.ColumnCount; x++)
                {
                    // Black square -> invalid Checker position
                    if ((GameDirection != GameDirection.Top || y >= gameBoard.StartingUpperRow) &&
                        (GameDirection != GameDirection.Bottom || y <= gameBoard.StartingLowerRow))
                    {
                        continue;
                    }
                    
                    if ((x + y) % 2 != 0)
                    {
                        Checker checker = new(CheckersColor, new Position(x, y, GameDirection));
                        Checkers.Add(checker);
                    }
                }
            }
        }

        public void CreateCheckers(List<Checker> checkers) 
        {
            foreach (var checker in checkers)
            {
                Checkers.Add(checker);
            }
        }

        public bool IsSurrendering(Position coordinates)
        {
            if (coordinates == null) return true;
            return false;
        }

        public bool IsPositionOccupied(Position destination)
        {
            return Checkers.Any(checker => destination.Equals(checker.CurrentPosition));
        }

        public Checker ChooseChecker(Player player, List<Checker> checkersWithMaxJumps)
        {
            Checker checker;
            do
            {
                var startingPosition = _view.GetPosition(true, player);
                checker = CheckSelectedChecker(checkersWithMaxJumps, startingPosition);
            } while (checker == null);

            return checker;
        }

        private Checker CheckSelectedChecker(List<Checker> checkers, Position startPosition)
        {
            foreach (var checker in checkers)
            {
                if (startPosition.Equals(checker.CurrentPosition))
                {
                    return checker;
                }
            }

            return null;
        }

        public Checker? GetCheckerWithMatchingCoordinates(Position startPosition)
        {
            foreach (var checker in Checkers)
            {
                if (checker.CurrentPosition.Equals(startPosition))
                {
                    return checker;
                }
            }
            return null;
        }
    }
}