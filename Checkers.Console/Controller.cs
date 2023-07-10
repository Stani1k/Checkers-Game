using Checkers.Console.Models;
using Checkers.Console.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Checkers.Console
{
    public class Controller
    {
        private Player player1;
        private Player player2;

        private readonly IView _view;
        private readonly Settings _settings;
        private readonly GameBoard gameboard;
        private readonly Stopwatch stopwatch = new Stopwatch();

        public Controller(IView view = null)
        {
            _view = view ?? new View();
            _settings = new Settings();
            gameboard = new GameBoard(_settings);
        }

        /// <summary>
        /// Contains all possible jump positions (max. 4 per Checker)
        /// </summary>
        private readonly List<Position> jumpPositions = new List<Position>();
        /// <summary>
        /// Contains all Checkers that have been skipped
        /// </summary>
        private readonly List<Position> skippedCheckers = new List<Position>();
        private List<int> queenPossiblePositionCount= new List<int>();

        public void RunApplication()
        {
            _view.SetUp();
            _view.GreetPlayer();
            Settings.SetGameLanguage(_view.ChooseLanguage());
            _settings.SetGameVariant(_view.SelectGameVariant());
            _view.CreateGameboard();
            CreatePlayers();
            SelectPlayerColors();
            SelectSites();
            CreateCheckers(player1);
            CreateCheckers(player2);
            PrintCheckers(player1);
            PrintCheckers(player2);
            StartGame();
            SelectWinner();
            ExitApplication();
        }

        private void CreatePlayers()
        {
            player1 = new Player(_view) { Nummer = 1 };
            player2 = new Player(_view) { Nummer = 2 };
            player1.Name = _view.GetPlayerName(player1.Nummer);
            player2.Name = _view.GetPlayerName(player2.Nummer);
            while (player1.Name.Equals(player2.Name))
            {
                _view.EqualNamesNotAllowed();
                player1.Name = _view.GetPlayerName(player1.Nummer);
                player2.Name = _view.GetPlayerName(player2.Nummer);
            }
        }

        private void SelectPlayerColors()
        {
            player1.CheckersColor = _view.GetSelectedColor(player1.Name, player1.Nummer);
            player2.CheckersColor = _view.GetSelectedColor(player2.Name, player2.Nummer);
            while (player1.CheckersColor.Equals(player2.CheckersColor))
            {
                _view.EqualCheckerColorNotAllowed();
                player1.CheckersColor = _view.GetSelectedColor(player1.Name, player1.Nummer);
                player2.CheckersColor = _view.GetSelectedColor(player2.Name, player2.Nummer);
            }
        }

        private void SelectSites()
        {
            player1.SetGameDirection(_view.GetStartingSite(player1.Name));
            if (player1.GameDirection == 1)
            {
                player2.GameDirection = -1;
            }
            else if (player1.GameDirection == -1)
            {
                player2.GameDirection = 1;
            }
        }

        private void CreateCheckers(Player player)
        {
            player.Checkers = new List<Checker>();
            for (var y = 0; y < gameboard.RowCount; y++)
            {
                for (var x = 0; x < gameboard.ColumnCount; x++)
                {
                    if (player.GameDirection == 1 && y < gameboard.StartingUpperRow || player.GameDirection == -1 && y > gameboard.StartingLowerRow)
                    {
                        // "!=" means that the checkers are positioned on a black square
                        if ((x + y) % 2 != 0)
                        {
                            Checker checker = new(player.CheckersColor, new Position(x, y, player.GameDirection));
                            player.Checkers.Add(checker);
                        }
                    }
                }
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
            Checker checkerToMove;
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
                    if (checkerToMove.IsQueen && _settings.Variant == Variant.European)
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
                var zielPosition = _view.GetPosition(false, player);
                if (zielPosition.IsWhiteField(zielPosition))
                {
                    _view.MoveCheckerOnWhiteField();
                    continue;
                }
                if (checker.IsFieldAlreadyOccupied(player, zielPosition) || checker.IsFieldAlreadyOccupied(enemy, zielPosition))
                {
                    _view.MoveCheckerOnOccupiedField();
                    continue;
                }
                if (jumpAvailable)
                {
                    Jump(player, checker, zielPosition);
                    break;
                }
                if (!MoveIsValid(checker, zielPosition))
                {
                    _view.TryMovingCheckerToFarAway();
                    continue;
                }
                checker.CurrentPosition = zielPosition;
                if (QueenTransformationAvailable(player, checker, zielPosition))
                {
                    checker.TransformCheckerToQueen();
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
                _view.ForceJump(jumpPositions);
                destination = _view.GetPosition(false, player);
            }
            _view.RemoveChecker(checker);
            checker.CurrentPosition = destination;
            if (QueenTransformationAvailable(player, checker, destination))
            {
                checker.TransformCheckerToQueen();
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

                    for (var i = 0; i < selectedChecker.PossibleJumpCount; i++)
                    {
                        if (i > 0)
                        {
                            if (JumpIsAvailable(selectedChecker)) { }
                        }
                        MoveChecker(selectedChecker, currentSelectedPlayer, player2, true);
                        _view.ShowCurrentCheckerAmount(player1.Name, player1.Checkers.Count, player2.Name, player2.Checkers.Count);
                    }
                    break;
                }
                var checkerToMove = GetStartingCoordinates(currentSelectedPlayer, player2);
                // If no jumps are available, check if checker can be moved diagonally
                if (MovingPossible(checkerToMove))
                {
                    _view.RemoveChecker(checkerToMove);
                    MoveChecker(checkerToMove, currentSelectedPlayer, player2, false);
                    _view.ShowCurrentCheckerAmount(player1.Name, player1.Checkers.Count, player2.Name, player2.Checkers.Count);
                    break;
                }
            }
        }

        private bool QueenTransformationAvailable(Player currentPlayer, Checker checker, Position destination)
        {
            if (!checker.IsQueen)
            {
                if (destination.Equals(new Position(destination.X, gameboard.StartingLowerRow, currentPlayer.GameDirection)) ^
                    destination.Equals(new Position(destination.X, gameboard.StartingUpperRow, currentPlayer.GameDirection)))
                {
                    return true;
                }
            }

            return false;
        }

        // ToDo - FRAGE: Warum erhalte ich eine "StackOverflowException" wenn ich den Parameter "positionSpielstein" entferne und dafür checker.Position verwende??
        private int ZähleAnzahlSprünge(Checker checker, Position checkerPosition)
        {
            if (!checker.IsQueen)
            {
                return CalculateJumpAmount(checker, checkerPosition);
            }
            if (checker.IsQueen && _settings.Variant == Variant.American)
            {
                return GetJumpsForAmericanQueen(checker, checkerPosition);
            }
            if (checker.IsQueen && _settings.Variant == Variant.European)
            {
                return BerechneSprüngeEuropäischerDame(checker, checkerPosition);
            }
            return 0;
        }

        private int CalculateJumpAmount(Checker checker, Position position)
        {
            var zielPos1 = new Position(position.SecondXRight,
                position.SecondNextYCoordinate, player1.GameDirection);
            var zielPos2 = new Position(position.SecondXLeft,
                position.SecondNextYCoordinate, player1.GameDirection);
            var istSprungMöglichRechts = IstEinSprungVerfügbar(new JumpDestination(position.XRight, position.NextYCoordinate, zielPos1.X, zielPos1.Y));
            var istSprungMöglichLinks = IstEinSprungVerfügbar(new JumpDestination(position.XLeft, position.NextYCoordinate, zielPos2.X, zielPos2.Y));

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
                position.SecondNextYCoordinate, player1.GameDirection);
            var zielPos2 = new Position(position.SecondXLeft,
                position.SecondNextYCoordinate, player1.GameDirection);
            var istSprungMöglichRechts = IstEinSprungVerfügbar(new JumpDestination(position.XRight, position.NextYCoordinate, zielPos1.X, zielPos1.Y));
            var istSprungMöglichLinks = IstEinSprungVerfügbar(new JumpDestination(position.XLeft, position.NextYCoordinate, zielPos2.X, zielPos2.Y));
            var zielPos1Rückwärts = new Position(position.SecondXRight,
                position.SecondYCoordinateBackwards, player1.GameDirection);
            var zielPos2Rückwärts = new Position(position.SecondXLeft,
                position.SecondYCoordinateBackwards, player1.GameDirection);
            // Möglichkeiten um rückwärts zu springen, falls der Spielstein eine Dame ist
            var istSprungMöglichRechtsRückwärts = IstEinSprungVerfügbar(new JumpDestination(position.XRight, position.NextYCoordinateBackwards, zielPos1Rückwärts.X, zielPos1Rückwärts.Y));
            var istSprungMöglichLinksRückwärts = IstEinSprungVerfügbar(new JumpDestination(position.XLeft, position.NextYCoordinateBackwards, zielPos2Rückwärts.X, zielPos2Rückwärts.Y));

            int anzahlSprüngeRechts;
            if (istSprungMöglichRechts)
            {
                anzahlSprüngeRechts = 1 + GetJumpsForAmericanQueen(checker, zielPos1);
            }
            else if (istSprungMöglichRechtsRückwärts)
            {
                anzahlSprüngeRechts = 1 + BerechneAnzahlRückwärtsSprünge(checker, zielPos1Rückwärts);
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
                anzahlSprüngeLinks = 1 + BerechneAnzahlRückwärtsSprünge(checker, zielPos2Rückwärts);
            }
            else
            {
                anzahlSprüngeLinks = 0;
            }

            return Math.Max(anzahlSprüngeRechts, anzahlSprüngeLinks);
        }

        private int BerechneSprüngeEuropäischerDame(Checker checker, Position position)
        {
            queenPossiblePositionCount.Clear();
            var dameSprungRechts = SucheSprungNachRechts(position);
            Position sprungMöglichkeit = null;
            if (dameSprungRechts != null)
            {
                sprungMöglichkeit = GetQueenJumpPosition(checker, dameSprungRechts, new Position(dameSprungRechts.X - 1, dameSprungRechts.Y - 1, player1.GameDirection));
            }

            var dameSprungLinks = SucheSprungNachLinks(position);
            if (dameSprungLinks != null)
            {
                sprungMöglichkeit = GetQueenJumpPosition(checker, dameSprungLinks, new Position(dameSprungLinks.X + 1, dameSprungLinks.Y - 1, player1.GameDirection));
            }

            var dameSprungRechtsRückwärts = SucheSprungNachRechtsRückwärts(position);
            if (dameSprungRechtsRückwärts != null)
            {
                sprungMöglichkeit = GetQueenJumpPosition(checker, dameSprungRechtsRückwärts, new Position(dameSprungRechtsRückwärts.X - 1, dameSprungRechtsRückwärts.Y + 1, player1.GameDirection));
            }

            var dameSprungLinksRückwärts = SucheSprungNachLinksRückwärts(position);
            if (dameSprungLinksRückwärts != null)
            {
                sprungMöglichkeit = GetQueenJumpPosition(checker, dameSprungLinksRückwärts, new Position(dameSprungLinksRückwärts.X + 1, dameSprungLinksRückwärts.Y + 1, player1.GameDirection));
            }

            // Falls der Spielstein hier noch null ist, hat er keine Sprünge und wir geben den Wert 0 zurück
            if (sprungMöglichkeit == null)
            {
                return 0;
            }

            skippedCheckers.Clear();
            return queenPossiblePositionCount.Max();
        }

        private Position GetQueenJumpPosition(Checker checker, Position destination, Position checkerToHit)
        {
            if (destination != null)
            {
                if (!skippedCheckers.Contains(checkerToHit))
                {
                    skippedCheckers.Add(checkerToHit);
                    queenPossiblePositionCount.Add(1 + BerechneSprüngeEuropäischerDame(checker, destination));
                }
                else
                {
                    destination = null;
                }
            }

            return destination;
        }

        private Position SucheSprungNachRechts(Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinate, player1))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinate, player2) &&
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondNextYCoordinate, player2))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(new JumpDestination(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinate, neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondNextYCoordinate)))
                {
                    return new Position(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondNextYCoordinate, player1.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X + 1, neuPosSpielstein.Y + 1, player1.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachLinks(Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinate, player1))
                {
                    return null;
                }
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinate, player2) &&
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondNextYCoordinate, player2))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(new JumpDestination(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinate, neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondNextYCoordinate)))
                {
                    return new Position(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondNextYCoordinate, player1.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X - 1, neuPosSpielstein.Y + 1, player1.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachRechtsRückwärts(Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinateBackwards, player1))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinateBackwards, player2) &&
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondYCoordinateBackwards, player2))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(new JumpDestination(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinateBackwards, neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondYCoordinateBackwards)))
                {
                    return new Position(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondYCoordinateBackwards, player1.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X + 1, neuPosSpielstein.Y - 1, player1.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachLinksRückwärts(Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinateBackwards, player1))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinateBackwards, player2) &&
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondYCoordinateBackwards, player2))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(new JumpDestination(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinateBackwards, neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondYCoordinateBackwards)))
                {
                    return new Position(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondYCoordinateBackwards, player1.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X - 1, neuPosSpielstein.Y - 1, player1.GameDirection);
            }

            return null;
        }

        private int BerechneAnzahlRückwärtsSprünge(Checker checker, Position zielPositionChecker)
        {
            int anzahlSprüngeRechtsRückwärts;
            int anzahlSprüngeLinksRückwärts;

            var zielPosition1Rückwärts = new Position(zielPositionChecker.SecondXRight, zielPositionChecker.SecondYCoordinateBackwards, player1.GameDirection);
            var zielPosition2Rückwärts = new Position(zielPositionChecker.SecondXLeft, zielPositionChecker.SecondYCoordinateBackwards, player1.GameDirection);

            // Möglichkeiten um rückwärts zu springen
            var istSprungMöglichRechtsRückwärts = IstEinSprungVerfügbar(new JumpDestination(zielPositionChecker.XRight, zielPositionChecker.NextYCoordinateBackwards, zielPosition1Rückwärts.X, zielPosition1Rückwärts.Y));
            var istSprungMöglichLinksRückwärts = IstEinSprungVerfügbar(new JumpDestination(zielPositionChecker.XLeft, zielPositionChecker.NextYCoordinateBackwards, zielPosition2Rückwärts.X, zielPosition2Rückwärts.Y));

            if (istSprungMöglichRechtsRückwärts)
            {
                anzahlSprüngeRechtsRückwärts = 1 + BerechneAnzahlRückwärtsSprünge(checker, zielPosition1Rückwärts);
            }
            else
                anzahlSprüngeRechtsRückwärts = 0;

            if (istSprungMöglichLinksRückwärts)
            {
                anzahlSprüngeLinksRückwärts = 1 + BerechneAnzahlRückwärtsSprünge(checker, zielPosition2Rückwärts);
            }
            else
                anzahlSprüngeLinksRückwärts = 0;

            return Math.Max(anzahlSprüngeRechtsRückwärts, anzahlSprüngeLinksRückwärts);
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
            if (checker.IsQueen && _settings.Variant == Variant.American)
            {
                if (checker.IsAmericanQueenMoveValid(zielPosition)) return true;
            }
            if (checker.IsQueen && _settings.Variant == Variant.European)
            {
                if (checker.IsEuropeanQueenMoveValid(zielPosition)) return true;
            }

            return false;
        }

        private void DetermineQueenMoves(Checker dame)
        {
            // Deklariert eine neue Liste für alle Zugmöglichkeiten einer europäischen Dame
            dame.Moves = new List<Position>();
            var zielPosition = new Position(dame.CurrentPosition.XRight, dame.CurrentPosition.NextYCoordinate, player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS VORNE
            while (IstEineDameBewegungMöglich(new Position(zielPosition.X, zielPosition.Y, player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X + 1, zielPosition.Y + 1, player1.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XLeft, dame.CurrentPosition.NextYCoordinate, player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS VORNE
            while (IstEineDameBewegungMöglich(new Position(zielPosition.X, zielPosition.Y, player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X - 1, zielPosition.Y + 1, player1.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XRight, dame.CurrentPosition.NextYCoordinateBackwards, player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS HINTEN
            while (IstEineDameBewegungMöglich(new Position(zielPosition.X, zielPosition.Y, player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X + 1, zielPosition.Y - 1, player1.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XLeft, dame.CurrentPosition.NextYCoordinateBackwards, player1.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS HINTEN
            while (IstEineDameBewegungMöglich(new Position(zielPosition.X, zielPosition.Y, player1.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X - 1, zielPosition.Y - 1, player1.GameDirection);
            }
        }

        private bool MovingPossible(Checker checker)
        {
            var spielfigur = new Position(checker.CurrentPosition.X, checker.CurrentPosition.Y, player1.GameDirection);
            if (!checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, player1) || !gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, player2))
                    && (!gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, player1) || !gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, player2)))
                {
                    return false;
                }
            }

            if (checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach OBEN mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, player1) ||
                     !gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, player2))
                    && (!gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, player1) ||
                        !gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, player2)))
                {
                    // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach UNTEN ebenfalls mit eigenen oder gegnerischen Spielsteinen besetzt sind
                    if ((!gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinateBackwards, player1) ||
                         !gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinateBackwards, player2))
                        && (!gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinateBackwards, player1) ||
                            !gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinateBackwards, player2)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool SindKoordinatenAußerhalbSpielfeld(int xKoordinate, int yKoordinate = 0)
        {
            if (xKoordinate > gameboard.StartingUpperRow || xKoordinate < gameboard.StartingLowerRow)
            {
                return true;
            }

            if (yKoordinate > gameboard.StartingUpperRow || yKoordinate < gameboard.StartingLowerRow)
            {
                return true;
            }

            return false;
        }

        private bool IstEineDameBewegungMöglich(Position zielPosition)
        {
            // Prüft ob das diagonale Feld, mit den als Parameter übergebenen Koordinaten, frei ist
            if (gameboard.IsDiagonalFieldEmpty(zielPosition.X, zielPosition.Y, player1) && gameboard.IsDiagonalFieldEmpty(zielPosition.X, zielPosition.Y, player2))
            {
                return true;
            }

            return false;
        }
        private bool IstEinSprungVerfügbar(JumpDestination sprungRichtung)
        {
            // Prüft ob das nächste diagonale Feld besetzt ist
            if (!gameboard.IsDiagonalFieldEmpty(sprungRichtung.X, sprungRichtung.Y, player2))
            {
                // Prüft ob das übernächste diagonale Feld frei ist, d.h. es befindet sich auf diesem Feld weder ein gegnerischer, noch ein eigener Checker
                if (gameboard.IsDiagonalFieldEmpty(sprungRichtung.XNext, sprungRichtung.YNext, player2) && gameboard.IsDiagonalFieldEmpty(sprungRichtung.XNext, sprungRichtung.YNext, player1))
                {
                    return true;
                }
            }

            return false;
        }

        private List<Checker> SearchForJumps()
        {
            List<Checker> checkersToJump = new List<Checker>();
            foreach (var checker in player1.Checkers)
            {
                if (JumpIsAvailable(checker))
                {
                    checker.PossibleJumpCount = ZähleAnzahlSprünge(checker, checker.CurrentPosition);
                    checkersToJump.Add(checker);
                }
            }

            return checkersToJump;
        }

        private List<Checker> GetCheckersWithMostJumps(List<Checker> spielsteine)
        {
            var spielsteineGleichVieleSprünge = spielsteine.Where(c => c.PossibleJumpCount == spielsteine.Max(s => s.PossibleJumpCount)).ToList();
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

            var sprungMöglichkeit1 = SucheSprungAufNächstemDiagonalenFeld(checker.CurrentPosition.LeftJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate,
                player1.GameDirection));
            var sprungMöglichkeit2 = SucheSprungAufNächstemDiagonalenFeld(checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight,
                checker.CurrentPosition.SecondNextYCoordinate, player1.GameDirection));

            if (checker.IsQueen && _settings.Variant == Variant.American)
            {
                sprungMöglichkeit3 = SucheSprungAufNächstemDiagonalenFeld(checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft,
                    checker.CurrentPosition.SecondYCoordinateBackwards, player1.GameDirection));
                sprungMöglichkeit4 = SucheSprungAufNächstemDiagonalenFeld(checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight,
                    checker.CurrentPosition.SecondYCoordinateBackwards, player1.GameDirection));
            }
            if (checker.IsQueen && _settings.Variant == Variant.European)
            {
                sprungMöglichkeit5 = SucheSprungAufMehrerenDiagonalenFeldern(checker.CurrentPosition.LeftJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, player1.GameDirection), checker);
                sprungMöglichkeit6 = SucheSprungAufMehrerenDiagonalenFeldern(checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondNextYCoordinate, player1.GameDirection), checker);
                sprungMöglichkeit7 = SucheSprungAufMehrerenDiagonalenFeldern(checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondYCoordinateBackwards, player1.GameDirection), checker);
                sprungMöglichkeit8 = SucheSprungAufMehrerenDiagonalenFeldern(checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondYCoordinateBackwards, player1.GameDirection), checker);
            }

            if (sprungMöglichkeit1 || sprungMöglichkeit2 || sprungMöglichkeit3 || sprungMöglichkeit4 || sprungMöglichkeit5 ||
                sprungMöglichkeit6 || sprungMöglichkeit7 || sprungMöglichkeit8)
            {
                return true;
            }

            return false;
        }

        private bool SucheSprungAufNächstemDiagonalenFeld(JumpDestination richtung, Position sprungPosition)
        {
            if (IstEinSprungVerfügbar(richtung))
            {
                FügeSprungZurListeHinzu(sprungPosition);
                return true;
            }

            return false;
        }

        private bool SucheSprungAufMehrerenDiagonalenFeldern(JumpDestination richtung, Position sprungPosition, Checker checker)
        {
            var startPosition = checker.CurrentPosition;
            while (!SindKoordinatenAußerhalbSpielfeld(checker.CurrentPosition.X))
            {
                // Durchsucht alle diagonalen Felder, in eine bestimmte Richtung, nach einem Sprung
                if (SucheSprungAufNächstemDiagonalenFeld(richtung, sprungPosition))
                {
                    checker.CurrentPosition = startPosition;
                    return true;
                }
                if (richtung.Equals(checker.CurrentPosition.LeftJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X - 1, checker.CurrentPosition.Y + player1.GameDirection, player1.GameDirection);
                    richtung = checker.CurrentPosition.LeftJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, player1.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.RightJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X + 1, checker.CurrentPosition.Y + player1.GameDirection, player1.GameDirection);
                    richtung = checker.CurrentPosition.RightJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondNextYCoordinate, player1.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.LeftBackwardsJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X - 1, checker.CurrentPosition.Y - player1.GameDirection, player1.GameDirection);
                    richtung = checker.CurrentPosition.LeftBackwardsJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondYCoordinateBackwards, player1.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.RightBackwardsJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X + 1, checker.CurrentPosition.Y - player1.GameDirection, player1.GameDirection);
                    richtung = checker.CurrentPosition.RightBackwardsJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondYCoordinateBackwards, player1.GameDirection);
                }
            }

            checker.CurrentPosition = startPosition;
            return false;
        }

        private void FügeSprungZurListeHinzu(Position sprungPosition)
        {
            if (!jumpPositions.Contains(sprungPosition))
            {
                jumpPositions.Add(sprungPosition);
            }
        }

        private void ZeigeUmgewandelteKoordinatenAn(List<Checker> spielsteine)
        {
            // Umgewandelte x und y Koordinaten des ERSTEN gefundenen Spielstein, welcher über einen Sprung verfügt
            var xKoordinateAlsBuchstabe = Convert.ToChar(spielsteine[0].CurrentPosition.X + 97);
            var yKoordinateUmgewandelt = (spielsteine[0].CurrentPosition.Y - gameboard.ColumnCount) * -1;

            if (spielsteine.Count == 1)
            {
                _view.ShowPossibleJump(xKoordinateAlsBuchstabe, yKoordinateUmgewandelt);
            }
            else if (spielsteine.Count == 2)
            {
                // Umgewandelte x und y Koordinaten des ZWEITEN gefundenen Spielstein, welcher über einen Sprung verfügt
                var xKoordinate2AlsBuchstabe = Convert.ToChar(spielsteine[1].CurrentPosition.X + 97);
                var yKoordinate2Umgewandelt = (spielsteine[1].CurrentPosition.Y - gameboard.ColumnCount) * -1;
                _view.ShowTwoPossibleJumps(xKoordinateAlsBuchstabe, yKoordinateUmgewandelt, xKoordinate2AlsBuchstabe, yKoordinate2Umgewandelt);
            }
        }

        private bool CoordinatesAreValid(Checker checker, Position zielPosition)
        {
            Position geschlagenerSpielstein;
            // Sprung nach rechts unten
            var sprungMöglichkeit1 = jumpPositions.FirstOrDefault(p => p.X > checker.CurrentPosition.X && p.Y > checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit1))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit1.X - 1, sprungMöglichkeit1.Y - 1, player1.GameDirection);
                EntferneGegnerischenSpielstein(player2.Checkers, geschlagenerSpielstein);
                jumpPositions.Clear();
                return true;
            }
            // Sprung nach links unten
            var sprungMöglichkeit2 = jumpPositions.FirstOrDefault(p => p.X < checker.CurrentPosition.X && p.Y > checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit2))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit2.X + 1, sprungMöglichkeit2.Y - 1, player1.GameDirection);
                EntferneGegnerischenSpielstein(player2.Checkers, geschlagenerSpielstein);
                jumpPositions.Clear();
                return true;
            }
            // Sprung nach rechts oben
            var sprungMöglichkeit3 = jumpPositions.FirstOrDefault(p => p.X > checker.CurrentPosition.X && p.Y < checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit3))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit3.X - 1, sprungMöglichkeit3.Y + 1, player1.GameDirection);
                EntferneGegnerischenSpielstein(player2.Checkers, geschlagenerSpielstein);
                jumpPositions.Clear();
                return true;
            }
            // Sprung nach links oben
            var sprungMöglichkeit4 = jumpPositions.FirstOrDefault(p => p.X < checker.CurrentPosition.X && p.Y < checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit4))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit4.X + 1, sprungMöglichkeit4.Y + 1, player1.GameDirection);
                EntferneGegnerischenSpielstein(player2.Checkers, geschlagenerSpielstein);
                jumpPositions.Clear();
                return true;
            }

            return false;
        }

        private void EntferneGegnerischenSpielstein(List<Checker> gegnerischeSpielsteine, Position geschlagenerSpielstein)
        {
            for (var i = 0; i < gegnerischeSpielsteine.Count; i++)
            {
                if (gegnerischeSpielsteine[i].CurrentPosition.Equals(geschlagenerSpielstein))
                {
                    // Entfernt den geschlagenen Spielstein aus der Klasse View
                    _view.RemoveChecker(gegnerischeSpielsteine[i]);
                    // Entfernt den geschlagenen Spielstein aus der Liste der Spielfiguren
                    gegnerischeSpielsteine.Remove(gegnerischeSpielsteine[i]);
                    _view.BeatChecker();
                    break;
                }
            }
        }

        // ToDo: mit Implementierung eines Timers fortsetzen (Idee: Den Timer z.B. jede 15 Sekunden aktualisieren lassen)
        private void GeneriereZeitMesser()
        {
            stopwatch.Start();
            if (stopwatch.ElapsedMilliseconds >= 15000)
            {
                _view.ShowPassedTime(stopwatch);
                Thread.Sleep(1000);
            }
        }

        private void BeendeZeitMessung()
        {
            stopwatch.Stop();
        }

        private void StartGame()
        {
            _view.ShowSurrenderOption();
            var zähleSpielzüge = 1;
            var spieler2AmZug = 0;
            //GeneriereZeitMesser();
            // Das Spiel geht solange, bis einer der beiden Spieler keine Spielfiguren mehr hat
            while (player1.Checkers.Count != 0 || player2.Checkers.Count != 0)
            {
                if ((zähleSpielzüge + spieler2AmZug) % 2 != 0)
                {
                    _view.ShowPlayerTurn(player1.Name, player1.CheckersColor);
                    ChangePlayerTurn(player1);
                    zähleSpielzüge++;
                }
                else
                {
                    _view.ShowPlayerTurn(player2.Name, player2.CheckersColor);
                    ChangePlayerTurn(player2);
                    spieler2AmZug++;
                }
            }
            //BeendeZeitMessung();
        }

        private void SelectWinner()
        {
            if (player1.Checkers.Count == 0)
            {
                _view.ShowWinner(player2.Name);
            }
            else if (player2.Checkers.Count == 0)
            {
                _view.ShowWinner(player1.Name);
            }
        }

        private void ExitApplication()
        {
            _view.QuitGame();
            Environment.Exit(0);
        }
    }
}