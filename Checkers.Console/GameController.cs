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
                if (CheckerFromEnemy(opponent, startPosition))
                {
                    _view.TryMovingEnemyChecker();
                    continue;
                }
                if (checkerToMove == null)
                {
                    continue;
                }
                if (SelectWhiteField(checkerToMove))
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

        private int ZähleAnzahlSprünge(Checker checker, Position checkerPosition)
        {
            if (!checker.IsQueen)
            {
                return CalculateJumpAmount(checker, checkerPosition);
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.American)
            {
                return GetJumpsForAmericanQueen(checker, checkerPosition);
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.European)
            {
                return BerechneSprüngeEuropäischerDame(checker, checkerPosition);
            }
            return 0;
        }

        private int CalculateJumpAmount(Checker checker, Position position)
        {
            var zielPos1 = new Position(position.SecondXRight,
                position.SecondNextYCoordinate, _player1.GameDirection);
            var zielPos2 = new Position(position.SecondXLeft,
                position.SecondNextYCoordinate, _player1.GameDirection);
            var istSprungMöglichRechts = JumpIsAvailable(new JumpDestination(position.XRight, position.NextYCoordinate, zielPos1.X, zielPos1.Y));
            var istSprungMöglichLinks = JumpIsAvailable(new JumpDestination(position.XLeft, position.NextYCoordinate, zielPos2.X, zielPos2.Y));

            int anzahlSprüngeRechts;
            if (istSprungMöglichRechts)
            {
                anzahlSprüngeRechts = 1 + CalculateJumpAmount(checker, zielPos1);
            }
            else
            {
                anzahlSprüngeRechts = 0;
            }
            int anzahlSprüngeLinks;
            if (istSprungMöglichLinks)
            {
                anzahlSprüngeLinks = 1 + CalculateJumpAmount(checker, zielPos2);
            }
            else
            {
                anzahlSprüngeLinks = 0;
            }

            return Math.Max(anzahlSprüngeRechts, anzahlSprüngeLinks);
        }

        private int GetJumpsForAmericanQueen(Checker checker, Position position)
        {
            // ToDo - FRAGE: Kann man, und wenn ja wie am besten, eine rekursive Methode wie diese hier, verkürzen? (Clean Code)
            var zielPos1 = new Position(position.SecondXRight,
                position.SecondNextYCoordinate, _player1.GameDirection);
            var zielPos2 = new Position(position.SecondXLeft,
                position.SecondNextYCoordinate, _player1.GameDirection);
            var istSprungMöglichRechts = JumpIsAvailable(new JumpDestination(position.XRight, position.NextYCoordinate, zielPos1.X, zielPos1.Y));
            var istSprungMöglichLinks = JumpIsAvailable(new JumpDestination(position.XLeft, position.NextYCoordinate, zielPos2.X, zielPos2.Y));
            var zielPos1Rückwärts = new Position(position.SecondXRight,
                position.SecondYCoordinateBackwards, _player1.GameDirection);
            var zielPos2Rückwärts = new Position(position.SecondXLeft,
                position.SecondYCoordinateBackwards, _player1.GameDirection);
            // Möglichkeiten um rückwärts zu springen, falls der Spielstein eine Dame ist
            var istSprungMöglichRechtsRückwärts = JumpIsAvailable(new JumpDestination(position.XRight, position.NextYCoordinateBackwards, zielPos1Rückwärts.X, zielPos1Rückwärts.Y));
            var istSprungMöglichLinksRückwärts = JumpIsAvailable(new JumpDestination(position.XLeft, position.NextYCoordinateBackwards, zielPos2Rückwärts.X, zielPos2Rückwärts.Y));

            int anzahlSprüngeRechts;
            if (istSprungMöglichRechts)
            {
                anzahlSprüngeRechts = 1 + GetJumpsForAmericanQueen(checker, zielPos1);
            }
            else if (istSprungMöglichRechtsRückwärts)
            {
                anzahlSprüngeRechts = 1 + CalculateBackwardsJumpAmount(checker, zielPos1Rückwärts);
            }
            else
            {
                anzahlSprüngeRechts = 0;
            }

            int anzahlSprüngeLinks;
            if (istSprungMöglichLinks)
            {
                anzahlSprüngeLinks = 1 + GetJumpsForAmericanQueen(checker, zielPos2);
            }
            else if (istSprungMöglichLinksRückwärts)
            {
                anzahlSprüngeLinks = 1 + CalculateBackwardsJumpAmount(checker, zielPos2Rückwärts);
            }
            else
            {
                anzahlSprüngeLinks = 0;
            }

            return Math.Max(anzahlSprüngeRechts, anzahlSprüngeLinks);
        }

        private int BerechneSprüngeEuropäischerDame(Checker checker, Position position)
        {
            Position jumpDestination = null;

            _queenPossiblePositionCount.Clear();
            var jumpRight = GetJumpDestination(position, position.XRight, position.SecondXRight, position.X + 1, position.NextYCoordinate, position.SecondNextYCoordinate, position.Y + 1);
            if (jumpRight != null)
            {
                jumpDestination = GetQueenJumpPosition(checker, jumpRight, new Position(jumpRight.X - 1, jumpRight.Y - 1, _player1.GameDirection));
            }

            var jumpLeft = GetJumpDestination(position, position.XLeft, position.SecondXLeft, position.X - 1, position.NextYCoordinate, position.SecondNextYCoordinate, position.Y + 1);
            if (jumpLeft != null)
            {
                jumpDestination = GetQueenJumpPosition(checker, jumpLeft, new Position(jumpLeft.X + 1, jumpLeft.Y - 1, _player1.GameDirection));
            }

            var jumpRightBackwards = GetJumpDestination(position, position.XLeft, position.SecondXLeft, position.X + 1, position.NextYCoordinateBackwards, 
                position.SecondYCoordinateBackwards, position.Y - 1);

            if (jumpRightBackwards != null)
            {
                jumpDestination = GetQueenJumpPosition(checker, jumpRightBackwards, new Position(jumpRightBackwards.X - 1, jumpRightBackwards.Y + 1, _player1.GameDirection));
            }

            var jumpLeftBackwards = GetJumpDestination(position, position.XLeft, position.SecondXLeft, position.X - 1, position.NextYCoordinateBackwards,
    position.SecondYCoordinateBackwards, position.Y - 1);

            if (jumpLeftBackwards != null)
            {
                jumpDestination = GetQueenJumpPosition(checker, jumpLeftBackwards, new Position(jumpLeftBackwards.X + 1, jumpLeftBackwards.Y + 1, _player1.GameDirection));
            }

            // Falls der Spielstein hier noch null ist, hat er keine Sprünge und wir geben den Wert 0 zurück
            if (jumpDestination == null)
            {
                return 0;
            }

            _skippedCheckers.Clear();
            return _queenPossiblePositionCount.Max();
        }

        private Position GetQueenJumpPosition(Checker checker, Position destination, Position checkerToHit)
        {
            if (destination != null)
            {
                if (!_skippedCheckers.Contains(checkerToHit))
                {
                    _skippedCheckers.Add(checkerToHit);
                    _queenPossiblePositionCount.Add(1 + BerechneSprüngeEuropäischerDame(checker, destination));
                }
                else
                {
                    destination = null;
                }
            }

            return destination;
        }

        private Position GetJumpDestination(Position destination, int xNext, int secondXNext, int xDestination, int yNext, int secondYNext, int yDestination)
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

        private int CalculateBackwardsJumpAmount(Checker checker, Position destination)
        {
            int rightBackwardsJump;
            int leftBackwardsJump;

            var rightDestination = new Position(destination.SecondXRight, destination.SecondYCoordinateBackwards, _player1.GameDirection);
            var leftDestination = new Position(destination.SecondXLeft, destination.SecondYCoordinateBackwards, _player1.GameDirection);

            var rightBackwardsJumpIsPossible = JumpIsAvailable(new JumpDestination(destination.XRight, destination.NextYCoordinateBackwards, rightDestination.X, rightDestination.Y));
            var leftBackwardsJumpIsPossible = JumpIsAvailable(new JumpDestination(destination.XLeft, destination.NextYCoordinateBackwards, leftDestination.X, leftDestination.Y));

            if (rightBackwardsJumpIsPossible)
            {
                rightBackwardsJump = 1 + CalculateBackwardsJumpAmount(checker, rightDestination);
            }
            else
                rightBackwardsJump = 0;

            if (leftBackwardsJumpIsPossible)
            {
                leftBackwardsJump = 1 + CalculateBackwardsJumpAmount(checker, leftDestination);
            }
            else
                leftBackwardsJump = 0;

            return Math.Max(rightBackwardsJump, leftBackwardsJump);
        }

        private static bool SelectWhiteField(Checker selektierterSpielstein)
        {
            return selektierterSpielstein == null;
        }

        private static bool CheckerFromEnemy(Player player2, Position startPosition)
        {
            return player2.Checkers.Any(spielstein => startPosition.Equals(spielstein.CurrentPosition));
        }

        private bool MoveIsValid(Checker checker, Position zielPosition)
        {
            if (!checker.IsQueen)
            {
                if (checker.IsNextPositionValid(zielPosition)) return true;
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.American)
            {
                if (checker.IsAmericanQueenMoveValid(zielPosition)) return true;
            }
            if (checker.IsQueen && _gameBoard    .Variant == Variant.European)
            {
                if (checker.IsEuropeanQueenMoveValid(zielPosition)) return true;
            }

            return false;
        }

        private void DetermineQueenMoves(Checker dame)
        {
            // Deklariert eine neue Liste für alle Zugmöglichkeiten einer europäischen Dame
            dame.Moves = new List<Position>();
            var zielPosition = new Position(dame.CurrentPosition.XRight, dame.CurrentPosition.NextYCoordinate, _player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS VORNE
            while (IsQueenMovePossible(new Position(zielPosition.X, zielPosition.Y, _player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X + 1, zielPosition.Y + 1, _player1.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XLeft, dame.CurrentPosition.NextYCoordinate, _player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS VORNE
            while (IsQueenMovePossible(new Position(zielPosition.X, zielPosition.Y, _player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X - 1, zielPosition.Y + 1, _player1.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XRight, dame.CurrentPosition.NextYCoordinateBackwards, _player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS HINTEN
            while (IsQueenMovePossible(new Position(zielPosition.X, zielPosition.Y, _player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X + 1, zielPosition.Y - 1, _player1.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XLeft, dame.CurrentPosition.NextYCoordinateBackwards, _player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS HINTEN
            while (IsQueenMovePossible(new Position(zielPosition.X, zielPosition.Y, _player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X - 1, zielPosition.Y - 1, _player1.GameDirection);
            }
        }

        private bool MovingPossible(Checker checker)
        {
            var spielfigur = new Position(checker.CurrentPosition.X, checker.CurrentPosition.Y, _player1.GameDirection);
            if (!checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!_gameBoard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, _player1) || !_gameBoard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, _player2))
                    && (!_gameBoard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, _player1) || !_gameBoard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, _player2)))
                {
                    return false;
                }
            }

            if (checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach OBEN mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!_gameBoard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, _player1) ||
                     !_gameBoard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, _player2))
                    && (!_gameBoard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, _player1) ||
                        !_gameBoard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, _player2)))
                {
                    // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach UNTEN ebenfalls mit eigenen oder gegnerischen Spielsteinen besetzt sind
                    if ((!_gameBoard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinateBackwards, _player1) ||
                         !_gameBoard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinateBackwards, _player2))
                        && (!_gameBoard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinateBackwards, _player1) ||
                            !_gameBoard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinateBackwards, _player2)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CoordinatesOutOfFieldRange(int xKoordinate, int yKoordinate = 0)
        {
            if (xKoordinate > _gameBoard.StartingUpperRow || xKoordinate < _gameBoard.StartingLowerRow)
            {
                return true;
            }

            if (yKoordinate > _gameBoard.StartingUpperRow || yKoordinate < _gameBoard.StartingLowerRow)
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

        private List<Checker> GetCheckersWithMostJumps(List<Checker> spielsteine)
        {
            var spielsteineGleichVieleSprünge = spielsteine.Where(c => c.JumpCount == spielsteine.Max(s => s.JumpCount)).ToList();
            ZeigeUmgewandelteKoordinatenAn(spielsteineGleichVieleSprünge);
            _view.ShowPossibleJumpCount(spielsteineGleichVieleSprünge);

            return spielsteineGleichVieleSprünge;
        }

        private bool JumpIsAvailable(Checker checker)
        {
            var sprungMöglichkeit3 = false;
            var sprungMöglichkeit4 = false;
            var sprungMöglichkeit5 = false;
            var sprungMöglichkeit6 = false;
            var sprungMöglichkeit7 = false;
            var sprungMöglichkeit8 = false;

            var sprungMöglichkeit1 = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.LeftJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate,
                _player1.GameDirection));
            var sprungMöglichkeit2 = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight,
                checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection));

            if (checker.IsQueen && _gameBoard.Variant == Variant.American)
            {
                sprungMöglichkeit3 = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft,
                    checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection));
                sprungMöglichkeit4 = IsJumpOnNextFieldPossible(checker, checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight,
                    checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection));
            }
            if (checker.IsQueen && _gameBoard.Variant == Variant.European)
            {
                sprungMöglichkeit5 = IsAnyJumpPossible(checker.CurrentPosition.LeftJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection), checker);
                sprungMöglichkeit6 = IsAnyJumpPossible(checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondNextYCoordinate, _player1.GameDirection), checker);
                sprungMöglichkeit7 = IsAnyJumpPossible(checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection), checker);
                sprungMöglichkeit8 = IsAnyJumpPossible(checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondYCoordinateBackwards, _player1.GameDirection), checker);
            }

            if (sprungMöglichkeit1 || sprungMöglichkeit2 || sprungMöglichkeit3 || sprungMöglichkeit4 || sprungMöglichkeit5 ||
                sprungMöglichkeit6 || sprungMöglichkeit7 || sprungMöglichkeit8)
            {
                return true;
            }

            return false;
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

        private void ZeigeUmgewandelteKoordinatenAn(List<Checker> spielsteine)
        {
            // Umgewandelte x und y Koordinaten des ERSTEN gefundenen Spielstein, welcher über einen Sprung verfügt
            var xKoordinateAlsBuchstabe = Convert.ToChar(spielsteine[0].CurrentPosition.X + 97);
            var yKoordinateUmgewandelt = (spielsteine[0].CurrentPosition.Y - _gameBoard.ColumnCount) * -1;

            if (spielsteine.Count == 1)
            {
                _view.ShowPossibleJump(xKoordinateAlsBuchstabe, yKoordinateUmgewandelt);
            }
            else if (spielsteine.Count == 2)
            {
                // Umgewandelte x und y Koordinaten des ZWEITEN gefundenen Spielstein, welcher über einen Sprung verfügt
                var xKoordinate2AlsBuchstabe = Convert.ToChar(spielsteine[1].CurrentPosition.X + 97);
                var yKoordinate2Umgewandelt = (spielsteine[1].CurrentPosition.Y - _gameBoard.ColumnCount) * -1;
                _view.ShowTwoPossibleJumps(xKoordinateAlsBuchstabe, yKoordinateUmgewandelt, xKoordinate2AlsBuchstabe, yKoordinate2Umgewandelt);
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