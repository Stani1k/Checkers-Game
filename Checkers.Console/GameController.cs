using Checkers.Console.Models;
using Checkers.Console.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Checkers.Console
{
    public class GameController
    {
        private const int AAsciiNumber = 97;
        private Player _player1;
        private Player _player2;

        private readonly IView _view;
        private readonly GameBoard _gameBoard;
        private readonly Stopwatch _stopwatch;
        private readonly List<int> _queenPossiblePositionCount= new();

        /// <summary>
        /// Contains all Checkers that have been skipped
        /// </summary>
        private readonly List<Position> _skippedCheckers = new();

        public GameController()
        {
            _stopwatch = new Stopwatch();
            _gameBoard = new GameBoard(Variant.American);
            _view = new View(_gameBoard);
        }
        
        public void RunApplication()
        {
            _view.SetUp();
            _view.ShowMainMenu();
            ExecuteMainMenuOptions();
            // Settings.SetGameLanguage(_view.ChooseLanguage());
        }

        private void ExecuteMainMenuOptions()
        {
            var selectedMenuOption = GetSelectedMenuOption();
            System.Console.Clear();
            switch (selectedMenuOption)
            {
                case MenuOption.Start:
                    _gameBoard.SetGameVariant(_view.GetSelectedGameVariant());
                    _view.CreateGameboard();
                    CreatePlayers();
                    SelectPlayerColors();
                    SelectSites();
                    _player1.CreateCheckers(_gameBoard);
                    _player2.CreateCheckers(_gameBoard);
                    PrintCheckers(_player1);
                    PrintCheckers(_player2);
                    StartGame();
                    SelectWinner();
                    break;
                case MenuOption.Settings:
                    break;
                case MenuOption.Help:
                    break;
                case MenuOption.About:
                    break;
                case MenuOption.Quit:
                    ExitApplication();
                    break;
                case MenuOption.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static MenuOption GetSelectedMenuOption()
        {
            var menuOption = MenuOption.None;
            while (menuOption == MenuOption.None)
            {
                var selectedOption = System.Console.ReadLine();
                switch (selectedOption)
                {
                    case "1":
                        menuOption = MenuOption.Start;
                        break;
                    case "2":
                        menuOption = MenuOption.Settings;
                        break;
                    case "3":
                        menuOption = MenuOption.Help;
                        break;
                    case "4":
                        menuOption = MenuOption.About;
                        break;
                    case "5":
                        menuOption = MenuOption.Quit;
                        break;
                    default:
                        menuOption = MenuOption.None;
                        System.Console.WriteLine("Please make a valid selection!");
                        break;
                }
            }

            return menuOption;
        }

        private void CreatePlayers()
        {
            string player1Name = _view.GetPlayerName(1);
            string player2Name = _view.GetPlayerName(2);
            while (_player1.Name.Equals(_player2.Name))
            {
                _view.EqualNamesNotAllowed();
                player1Name = _view.GetPlayerName(_player1.Number);
                player2Name = _view.GetPlayerName(_player2.Number);
            }

            _player1 = new Player(player1Name, 1, _view);
            _player2 = new Player(player2Name, 2, _view);
        }

        private void SelectPlayerColors()
        {
            _player1.CheckersColor = _view.GetSelectedColor(_player1.Name, _player1.Number);
            _player2.CheckersColor = _view.GetSelectedColor(_player2.Name, _player2.Number);
            while (_player1.CheckersColor.Equals(_player2.CheckersColor))
            {
                _view.EqualCheckerColorNotAllowed();
                _player1.CheckersColor = _view.GetSelectedColor(_player1.Name, _player1.Number);
                _player2.CheckersColor = _view.GetSelectedColor(_player2.Name, _player2.Number);
            }
        }

        private void SelectSites()
        {
            _player1.SetGameDirection(_view.GetStartingSite(_player1.Name));
            if (_player1.GameDirection == 1)
            {
                _player2.GameDirection = -1;
            }
            else if (_player1.GameDirection == -1)
            {
                _player2.GameDirection = 1;
            }
        }

        private void PrintCheckers(Player player)
        {
            foreach (var checker in player.Checkers)
            {
                _view.PrintChecker(checker);
            }
        }

        private Checker GetStartingCoordinates(Player player, Player opponent)
        {
            Checker? checkerToMove;
            while (true)
            {
                var startPosition = _view.GetPosition(true, player);
                if (player.IsSurrendering(startPosition))
                {
                    _view.PlayerHasSurrendered(player, opponent);
                    Environment.Exit(0);
                }
                checkerToMove = player.GetCheckerWithMatchingCoordinates(startPosition);
                if (EnemyCheckerSelected(opponent, startPosition))
                {
                    _view.TryMovingEnemyChecker();
                    continue;
                }
                if (checkerToMove == null)
                {
                    continue;
                }
                if (WhiteFieldSelected(checkerToMove))
                {
                    _view.SelectEmptyField();
                    continue;
                }
                if (!MovingPossible(checkerToMove))
                {
                    _view.MovingNotPossible();
                    continue;
                }
                if (MovingPossible(checkerToMove))
                {
                    if (checkerToMove.IsQueen && _gameBoard.Variant == Variant.European)
                    {
                        DetermineQueenMoves(checkerToMove);
                    }
                    break;
                }
            }
            return checkerToMove;
        }

        private void MoveChecker(Checker checker, Player player, Player enemy, bool jumpAvailable)
        {
            while (true)
            {
                var destination = _view.GetPosition(false, player);
                if (destination.IsWhiteField(destination))
                {
                    _view.MoveCheckerOnWhiteField();
                    continue;
                }
                if (player.IsPositionOccupied(destination) || enemy.IsPositionOccupied(destination))
                {
                    _view.MoveCheckerOnOccupiedField();
                    continue;
                }
                if (jumpAvailable)
                {
                    Jump(player, checker, destination);
                    break;
                }
                if (!MoveIsValid(checker, destination))
                {
                    _view.TryMovingCheckerToFarAway();
                    continue;
                }
                checker.CurrentPosition = destination;
                if (QueenTransformationAvailable(player, checker, destination))
                {
                    checker.SetQueen();
                    _view.TransformCheckerToQueen(checker.CurrentPosition);
                    _view.MoveChecker(checker);
                }
                else
                {
                    _view.MoveChecker(checker);
                }
                break;
            }
        }

        private void Jump(Player player, Checker checker, Position destination)
        {
            while (!CoordinatesAreValid(checker, destination))
            {
                _view.ForceJump(checker.Jumps);
                destination = _view.GetPosition(false, player);
            }
            _view.RemoveChecker(checker);
            checker.CurrentPosition = destination;
            if (QueenTransformationAvailable(player, checker, destination))
            {
                checker.SetQueen();
                _view.TransformCheckerToQueen(checker.CurrentPosition);
                _view.MoveChecker(checker);
            }
            else
            {
                _view.MoveChecker(checker);
            }
        }

        private void ChangePlayerTurn(Player currentSelectedPlayer)
        {
            while (true)
            {
                // At the beginning of a turn it is checked whether the player has one or more jumps
                var checkersToJump = SearchForJumps();
                if (checkersToJump.Any())
                {
                    var checkersWithMaxJumps = GetCheckersWithMostJumps(checkersToJump);
                    var selectedChecker = currentSelectedPlayer.ChooseChecker(currentSelectedPlayer, checkersWithMaxJumps);
                    _view.RemoveChecker(selectedChecker);

                    for (var i = 0; i < selectedChecker.JumpCount; i++)
                    {
                        if (i > 0)
                        {
                            if (JumpIsAvailable(selectedChecker)) { }
                        }
                        MoveChecker(selectedChecker, currentSelectedPlayer, _player2, true);
                        _view.ShowCurrentCheckerAmount(_player1.Name, _player1.Checkers.Count, _player2.Name, _player2.Checkers.Count);
                    }
                    break;
                }
                var checkerToMove = GetStartingCoordinates(currentSelectedPlayer, _player2);
                // If no jumps are available, check if checker can be moved diagonally
                if (MovingPossible(checkerToMove))
                {
                    _view.RemoveChecker(checkerToMove);
                    MoveChecker(checkerToMove, currentSelectedPlayer, _player2, false);
                    _view.ShowCurrentCheckerAmount(_player1.Name, _player1.Checkers.Count, _player2.Name, _player2.Checkers.Count);
                    break;
                }
            }
        }

        private bool QueenTransformationAvailable(Player currentPlayer, Checker checker, Position destination)
        {
            if (!checker.IsQueen)
            {
                if (destination.Equals(new Position(destination.X, _gameBoard.StartingLowerRow, currentPlayer.GameDirection)) ^
                    destination.Equals(new Position(destination.X, _gameBoard.StartingUpperRow, currentPlayer.GameDirection)))
                {
                    return true;
                }
            }

            return false;
        }

        private int CountJumps(Checker checker, Position checkerPosition)
        {
            _queenPossiblePositionCount.Clear();
            if (checker.IsQueen == false)
            {
                return CalculateJumpAmount(checkerPosition, checker.IsQueen);
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.American)
            {
                return CalculateJumpAmount(checkerPosition, checker.IsQueen);
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.European)
            {
                return CalculateJumpAmount(checkerPosition, checker.IsQueen);
            }
            return 0;
        }

        private int CalculateJumpAmount(Position position, bool isQueen)
        {
            var rightBackwardsJumpAvailable = false;
            var leftBackwardsJumpAvailable = false;
            
            Position destinationRight = null;
            Position? destinationRightBackwards = null;
            Position destinationLeft = null;
            Position? destinationLeftBackwards = null;
            
            destinationRight = new Position(position.SecondXRight, position.SecondNextYCoordinate, _player1.GameDirection);
            destinationLeft = new Position(position.SecondXLeft, position.SecondNextYCoordinate, _player1.GameDirection);
            var rightJumpAvailable = JumpIsAvailable(new JumpDestination(position.XRight, position.NextYCoordinate, destinationRight.X, destinationRight.Y));
            var leftJumpAvailable = JumpIsAvailable(new JumpDestination(position.XLeft, position.NextYCoordinate, destinationLeft.X, destinationLeft.Y));

            if (isQueen && _gameBoard.Variant == Variant.American)
            {
                destinationRightBackwards = new Position(position.SecondXRight, position.SecondYCoordinateBackwards, _player1.GameDirection);
                destinationLeftBackwards = new Position(position.SecondXLeft, position.SecondYCoordinateBackwards, _player1.GameDirection);
                rightBackwardsJumpAvailable = JumpIsAvailable(new JumpDestination(position.XRight, position.NextYCoordinateBackwards, destinationRightBackwards.X, destinationRightBackwards.Y));
                leftBackwardsJumpAvailable = JumpIsAvailable(new JumpDestination(position.XLeft, position.NextYCoordinateBackwards, destinationLeftBackwards.X, destinationLeftBackwards.Y));
            }
            else if (isQueen && _gameBoard.Variant == Variant.European)
            {
                destinationRightBackwards = GetJumpDestination(position, position.XLeft, position.SecondXLeft, position.X + 1, position.NextYCoordinateBackwards,
                    position.SecondYCoordinateBackwards, position.Y - 1);
                destinationLeftBackwards = GetJumpDestination(position, position.XLeft, position.SecondXLeft, position.X - 1, position.NextYCoordinateBackwards,
                    position.SecondYCoordinateBackwards, position.Y - 1);
                
                if (destinationRightBackwards != null)
                {
                    rightBackwardsJumpAvailable = true;
                }
                if (destinationLeftBackwards != null)
                {
                    leftBackwardsJumpAvailable = true;
                }
            }

            int rightJumpCount;
            if (rightJumpAvailable)
            {
                rightJumpCount = 1 + CalculateJumpAmount(destinationRight, isQueen);
            }
            else if (rightBackwardsJumpAvailable)
            {
                rightJumpCount = 1 + CalculateBackwardsJumpAmount(destinationRightBackwards);
            }
            else
            {
                rightJumpCount = 0;
            }

            int leftJumpCount;
            if (leftJumpAvailable)
            {
                leftJumpCount = 1 + CalculateJumpAmount(destinationLeft, isQueen);
            }
            else if (leftBackwardsJumpAvailable)
            {
                leftJumpCount = 1 + CalculateBackwardsJumpAmount(destinationLeftBackwards);
            }
            else
            {
                leftJumpCount = 0;
            }

            return Math.Max(rightJumpCount, leftJumpCount);
        }
        
        private Position? GetJumpDestination(Position destination, int xNext, int secondXNext, int xDestination, int yNext, int secondYNext, int yDestination)
        {
            while (!CoordinatesOutOfFieldRange(destination.X, destination.Y))
            {
                if (!_gameBoard.IsDiagonalFieldEmpty(xNext, yNext, _player1))
                {
                    return null;
                }
                if (!_gameBoard.IsDiagonalFieldEmpty(xNext, yNext, _player2) &&
                    !_gameBoard.IsDiagonalFieldEmpty(secondXNext, secondYNext, _player2))
                {
                    return null;
                }
                if (JumpIsAvailable(new JumpDestination(xNext, yNext, secondXNext, secondYNext)))
                {
                    return new Position(secondXNext, secondYNext, _player1.GameDirection);
                }

                destination = new Position(xDestination, yDestination, _player1.GameDirection);
            }

            return null;
        }

        private int CalculateBackwardsJumpAmount(Position? destination)
        {
            int rightBackwardsJump;
            int leftBackwardsJump;

            var rightDestination = new Position(destination.SecondXRight, destination.SecondYCoordinateBackwards, _player1.GameDirection);
            var leftDestination = new Position(destination.SecondXLeft, destination.SecondYCoordinateBackwards, _player1.GameDirection);

            var rightBackwardsJumpIsPossible = JumpIsAvailable(new JumpDestination(destination.XRight, destination.NextYCoordinateBackwards, rightDestination.X, rightDestination.Y));
            var leftBackwardsJumpIsPossible = JumpIsAvailable(new JumpDestination(destination.XLeft, destination.NextYCoordinateBackwards, leftDestination.X, leftDestination.Y));

            if (rightBackwardsJumpIsPossible)
            {
                rightBackwardsJump = 1 + CalculateBackwardsJumpAmount(rightDestination);
            }
            else
                rightBackwardsJump = 0;

            if (leftBackwardsJumpIsPossible)
            {
                leftBackwardsJump = 1 + CalculateBackwardsJumpAmount(leftDestination);
            }
            else
                leftBackwardsJump = 0;

            return Math.Max(rightBackwardsJump, leftBackwardsJump);
        }

        private static bool WhiteFieldSelected(Checker? checker)
        {
            return checker == null;
        }

        private static bool EnemyCheckerSelected(Player player, Position startPosition)
        {
            return player.Checkers.Any(checker => startPosition.Equals(checker.CurrentPosition));
        }

        private bool MoveIsValid(Checker checker, Position destination)
        {
            if (!checker.IsQueen)
            {
                if (checker.IsNextPositionValid(destination)) return true;
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.American)
            {
                if (checker.IsAmericanQueenMoveValid(destination)) return true;
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.European)
            {
                if (checker.IsEuropeanQueenMoveValid(destination)) return true;
            }

            return false;
        }

        private void DetermineQueenMoves(Checker queen)
        {
            queen.Moves = new List<Position>();
            var destination = new Position(queen.CurrentPosition.XRight, queen.CurrentPosition.NextYCoordinate, _player1.GameDirection);
            while (IsQueenMovePossible(new Position(destination.X, destination.Y, _player1.GameDirection)))
            {
                queen.Moves.Add(destination);
                destination = new Position(destination.X + 1, destination.Y + 1, _player1.GameDirection);
            }
            
            destination = new Position(queen.CurrentPosition.XLeft, queen.CurrentPosition.NextYCoordinate, _player1.GameDirection);
            while (IsQueenMovePossible(new Position(destination.X, destination.Y, _player1.GameDirection)))
            {
                queen.Moves.Add(destination);
                destination = new Position(destination.X - 1, destination.Y + 1, _player1.GameDirection);
            }
            
            destination = new Position(queen.CurrentPosition.XRight, queen.CurrentPosition.NextYCoordinateBackwards, _player1.GameDirection);
            while (IsQueenMovePossible(new Position(destination.X, destination.Y, _player1.GameDirection)))
            {
                queen.Moves.Add(destination);
                destination = new Position(destination.X + 1, destination.Y - 1, _player1.GameDirection);
            }
            
            destination = new Position(queen.CurrentPosition.XLeft, queen.CurrentPosition.NextYCoordinateBackwards, _player1.GameDirection);
            while (IsQueenMovePossible(new Position(destination.X, destination.Y, _player1.GameDirection)))
            {
                queen.Moves.Add(destination);
                destination = new Position(destination.X - 1, destination.Y - 1, _player1.GameDirection);
            }
        }

        private bool MovingPossible(Checker checker)
        {
            var position = checker.CurrentPosition;
            if (checker.IsQueen)
            {
                // Both diagonal fields UPWARDS are occupied by own or enemy checkers -> move not possible
                if ((!_gameBoard.IsDiagonalFieldEmpty(position.XRight, position.NextYCoordinate, _player1) || 
                     !_gameBoard.IsDiagonalFieldEmpty(position.XRight, position.NextYCoordinate, _player2)) && 
                    (!_gameBoard.IsDiagonalFieldEmpty(position.XLeft, position.NextYCoordinate, _player1) || 
                     !_gameBoard.IsDiagonalFieldEmpty(position.XLeft, position.NextYCoordinate, _player2)))
                {
                    return false;
                }
                
                // Both diagonal fields DOWNWARDS are occupied by own or enemy checkers -> move not possible
                if ((!_gameBoard.IsDiagonalFieldEmpty(position.XRight, position.NextYCoordinateBackwards, _player1) ||
                     !_gameBoard.IsDiagonalFieldEmpty(position.XRight, position.NextYCoordinateBackwards, _player2)) && 
                    (!_gameBoard.IsDiagonalFieldEmpty(position.XLeft, position.NextYCoordinateBackwards, _player1) || 
                     !_gameBoard.IsDiagonalFieldEmpty(position.XLeft, position.NextYCoordinateBackwards, _player2)))
                {
                    return false;
                }
            }
            else
            {
                // Checks if destination is occupied by own or enemy checker -> move not possible
                if ((!_gameBoard.IsDiagonalFieldEmpty(position.XRight, position.NextYCoordinate, _player1) || !_gameBoard.IsDiagonalFieldEmpty(position.XRight, position.NextYCoordinate, _player2))
                    && (!_gameBoard.IsDiagonalFieldEmpty(position.XLeft, position.NextYCoordinate, _player1) || !_gameBoard.IsDiagonalFieldEmpty(position.XLeft, position.NextYCoordinate, _player2)))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CoordinatesOutOfFieldRange(int x, int y = 0)
        {
            if (x > _gameBoard.StartingUpperRow || x < _gameBoard.StartingLowerRow)
            {
                return true;
            }

            if (y > _gameBoard.StartingUpperRow || y < _gameBoard.StartingLowerRow)
            {
                return true;
            }

            return false;
        }

        private bool IsQueenMovePossible(Position destination)
        {
            if (_gameBoard.IsDiagonalFieldEmpty(destination.X, destination.Y, _player1) && _gameBoard.IsDiagonalFieldEmpty(destination.X, destination.Y, _player2))
            {
                return true;
            }

            return false;
        }

        private bool JumpIsAvailable(JumpDestination destination)
        {
            if (!_gameBoard.IsDiagonalFieldEmpty(destination.X, destination.Y, _player2))
            {
                if (_gameBoard.IsDiagonalFieldEmpty(destination.XNext, destination.YNext, _player2) && 
                    _gameBoard.IsDiagonalFieldEmpty(destination.XNext, destination.YNext, _player1))
                {
                    return true;
                }
            }

            return false;
        }

        private List<Checker> SearchForJumps()
        {
            List<Checker> checkersToJump = new();
            foreach (var checker in _player1.Checkers)
            {
                if (JumpIsAvailable(checker))
                {
                    checkersToJump.Add(checker);
                }
            }

            return checkersToJump;
        }

        private List<Checker> GetCheckersWithMostJumps(IReadOnlyCollection<Checker> checker)
        {
            var checkersWithMostJumps = checker.Where(c => c.JumpCount == checker.Max(s => s.JumpCount)).ToList();
            ShowTransformedCoordinates(checkersWithMostJumps);
            _view.ShowPossibleJumpCount(checkersWithMostJumps);

            return checkersWithMostJumps;
        }

        private bool JumpIsAvailable(Checker checker)
        {
            var jumpIsAvailable = false;

            jumpIsAvailable = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.LeftJump, 
                new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection));
            if (jumpIsAvailable == false)
            {
                jumpIsAvailable = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight,
                    checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection));
            }

            if (checker.IsQueen && _gameBoard.Variant == Variant.American)
            {
                jumpIsAvailable = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft,
                    checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection));
                if (jumpIsAvailable == false)
                {
                    jumpIsAvailable = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight,
                        checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection));
                }
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.European)
            {
                jumpIsAvailable = IsAnyJumpPossible(checker.CurrentPosition.LeftJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection), checker);
                jumpIsAvailable = IsAnyJumpPossible(checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection), checker);
                jumpIsAvailable = IsAnyJumpPossible(checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection), checker);
                jumpIsAvailable = IsAnyJumpPossible(checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection), checker);
            }

            return jumpIsAvailable;
        }

        private bool IsJumpOnNextFieldPossible(Checker checker, JumpDestination richtung, Position jumpDestination)
        {
            if (JumpIsAvailable(richtung))
            {
                checker.AddJump(jumpDestination);
                return true;
            }

            return false;
        }

        private bool IsAnyJumpPossible(JumpDestination richtung, Position sprungPosition, Checker checker)
        {
            var startPosition = checker.CurrentPosition;
            while (!CoordinatesOutOfFieldRange(checker.CurrentPosition.X))
            {
                // Durchsucht alle diagonalen Felder, in eine bestimmte Richtung, nach einem Sprung
                if (IsJumpOnNextFieldPossible(checker, richtung, sprungPosition))
                {
                    checker.CurrentPosition = startPosition;
                    return true;
                }
                if (richtung.Equals(checker.CurrentPosition.LeftJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X - 1, checker.CurrentPosition.Y + _player1.GameDirection, _player1.GameDirection);
                    richtung = checker.CurrentPosition.LeftJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.RightJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X + 1, checker.CurrentPosition.Y + _player1.GameDirection, _player1.GameDirection);
                    richtung = checker.CurrentPosition.RightJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.LeftBackwardsJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X - 1, checker.CurrentPosition.Y - _player1.GameDirection, _player1.GameDirection);
                    richtung = checker.CurrentPosition.LeftBackwardsJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.RightBackwardsJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X + 1, checker.CurrentPosition.Y - _player1.GameDirection, _player1.GameDirection);
                    richtung = checker.CurrentPosition.RightBackwardsJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection);
                }
            }

            checker.CurrentPosition = startPosition;
            return false;
        }

        private void ShowTransformedCoordinates(IReadOnlyList<Checker> checkers)
        {
            if (checkers.Count == 0)
            {
                return;
            }
            
            var firstCheckerPosition = checkers[0].CurrentPosition;
            var xAsChar = Convert.ToChar(firstCheckerPosition.X + AAsciiNumber);
            var yAsInt = (firstCheckerPosition.Y - _gameBoard.ColumnCount) * -1;

            switch (checkers.Count)
            {
                case 1:
                    _view.ShowPossibleJump(xAsChar, yAsInt);
                    break;
                case 2:
                {
                    var secondCheckerPosition = checkers[1].CurrentPosition;
                    var xAsChar2 = Convert.ToChar(secondCheckerPosition.X + AAsciiNumber);
                    var yAsInt2 = (secondCheckerPosition.Y - _gameBoard.ColumnCount) * -1;
                    _view.ShowTwoPossibleJumps(xAsChar, yAsInt, xAsChar2, yAsInt2);
                    break;
                }
            }
        }

        private bool CoordinatesAreValid(Checker checker, Position zielPosition)
        {
            Position geschlagenerSpielstein;
            // Sprung nach rechts unten
            var sprungMöglichkeit1 = checker.Jumps.FirstOrDefault(p => p.X > checker.CurrentPosition.X && p.Y > checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit1))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit1.X - 1, sprungMöglichkeit1.Y - 1, _player1.GameDirection);
                RemoveEnemyChecker(_player2.Checkers, geschlagenerSpielstein);
                checker.ClearJumps();
                return true;
            }
            // Sprung nach links unten
            var sprungMöglichkeit2 = checker.Jumps.FirstOrDefault(p => p.X < checker.CurrentPosition.X && p.Y > checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit2))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit2.X + 1, sprungMöglichkeit2.Y - 1, _player1.GameDirection);
                RemoveEnemyChecker(_player2.Checkers, geschlagenerSpielstein);
                checker.ClearJumps();
                return true;
            }
            // Sprung nach rechts oben
            var sprungMöglichkeit3 = checker.Jumps.FirstOrDefault(p => p.X > checker.CurrentPosition.X && p.Y < checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit3))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit3.X - 1, sprungMöglichkeit3.Y + 1, _player1.GameDirection);
                RemoveEnemyChecker(_player2.Checkers, geschlagenerSpielstein);
                checker.ClearJumps();
                return true;
            }
            // Sprung nach links oben
            var sprungMöglichkeit4 = checker.Jumps.FirstOrDefault(p => p.X < checker.CurrentPosition.X && p.Y < checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit4))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit4.X + 1, sprungMöglichkeit4.Y + 1, _player1.GameDirection);
                RemoveEnemyChecker(_player2.Checkers, geschlagenerSpielstein);
                checker.ClearJumps();
                return true;
            }

            return false;
        }

        private void RemoveEnemyChecker(List<Checker> enemyCheckers, Position checkerToRemove)
        {
            for (var i = 0; i < enemyCheckers.Count; i++)
            {
                if (enemyCheckers[i].CurrentPosition.Equals(checkerToRemove))
                {
                    _view.RemoveChecker(enemyCheckers[i]);
                    enemyCheckers.Remove(enemyCheckers[i]);
                    _view.BeatChecker();
                    break;
                }
            }
        }

        // ToDo: mit Implementierung eines Timers fortsetzen (Idee: Den Timer z.B. jede 15 Sekunden aktualisieren lassen)
        private void StartTimeMeasurement()
        {
            _stopwatch.Start();
            if (_stopwatch.ElapsedMilliseconds >= 15000)
            {
                _view.ShowPassedTime(_stopwatch);
                Thread.Sleep(1000);
            }
        }

        private void StopTimeMeasurement()
        {
            _stopwatch.Stop();
        }

        private void StartGame()
        {
            _view.ShowSurrenderOption();
            var player1Turns = 1;
            var player2Turns = 0;

            // The game continues until one of the two players runs out of checkers
            while (_player1.Checkers.Count != 0 || _player2.Checkers.Count != 0)
            {
                if ((player1Turns + player2Turns) % 2 != 0)
                {
                    _view.ShowPlayerTurn(_player1.Name, _player1.CheckersColor);
                    ChangePlayerTurn(_player1);
                    player1Turns++;
                }
                else
                {
                    _view.ShowPlayerTurn(_player2.Name, _player2.CheckersColor);
                    ChangePlayerTurn(_player2);
                    player2Turns++;
                }
            }
        }

        private void SelectWinner()
        {
            if (_player1.Checkers.Count == 0)
            {
                _view.ShowWinner(_player2.Name);
            }
            else if (_player2.Checkers.Count == 0)
            {
                _view.ShowWinner(_player1.Name);
            }
        }

        private void ExitApplication()
        {
            _view.QuitGame();
            Environment.Exit(0);
        }
    }
}