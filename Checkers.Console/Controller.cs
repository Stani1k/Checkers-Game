using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Checkers.Console.Models;
using Checkers.Console.Views;

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
            SelectPlayerNames();
            ErmittleSpielsteinFarben();
            SeitenZuweisen();
            ErzeugeSpielsteine(player1);
            ErzeugeSpielsteine(player2);
            PlatziereSpielsteineVorStart(player1);
            PlatziereSpielsteineVorStart(player2);
            WiederholeSpielzüge();
            GewinnerErmitteln();
            BeendeAnwendung();
        }

        private void SelectPlayerNames()
        {
            player1 = new Player(_view) {Nummer = 1};
            player2 = new Player(_view){Nummer = 2};
            player1.Name = _view.EnterPlayerName(player1.Nummer);
            player2.Name = _view.EnterPlayerName(player2.Nummer);
            while (player1.Name.Equals(player2.Name))
            {
                _view.PlayerSelectEqualNames();
                player1.Name = _view.EnterPlayerName(player1.Nummer);
                player2.Name = _view.EnterPlayerName(player2.Nummer);
            }
        }

        private void ErmittleSpielsteinFarben()
        {
            player1.CheckersColor = _view.SelectCheckersColor(player1.Name, player1.Nummer);
            player2.CheckersColor = _view.SelectCheckersColor(player2.Name, player2.Nummer);
            while (player1.CheckersColor.Equals(player2.CheckersColor))
            {
                _view.PlayerSelectSameCheckerColor();
                player1.CheckersColor = _view.SelectCheckersColor(player1.Name, player1.Nummer);
                player2.CheckersColor = _view.SelectCheckersColor(player2.Name, player2.Nummer);
            }
        }

        private void SeitenZuweisen()
        {
            player1.SetGameDirection(_view.SelectStartingSite(player1.Name));
            if (player1.GameDirection == 1)
            {
                player2.GameDirection = -1;
            }
            else if (player1.GameDirection == -1)
            {
                player2.GameDirection = 1;
            }
        }

        private void ErzeugeSpielsteine(Player spieler)
        {
            spieler.Checkers = new List<Checker>();
            // y = y-Koordinate ; x = x-Koordinate
            for (var y = 0; y < gameboard.RowCount; y++)
            {
                for (var x = 0; x < gameboard.ColumnCount; x++)
                {
                    if (spieler.GameDirection == 1 && y < gameboard.StartingUpperRow || spieler.GameDirection == -1 && y > gameboard.StartingLowerRow)
                    {
                        // != Ungleich Null bedeutet, dass die Spielsteine auf einem schwarzen Feld positioniert werden
                        if ((x + y) % 2 != 0)
                        {
                            spieler.Checkers.Add(new Checker(spieler.CheckersColor, new Position(x, y, spieler.GameDirection)));
                        }
                    }
                }
            }
        }

        private void PlatziereSpielsteineVorStart(Player spieler)
        {
            foreach (var druckeChecker in spieler.Checkers)
            {
                _view.PrintChecker(druckeChecker);
            }
        }

        private Checker HoleStartKoordinatenVonView(Player spieler, Player gegner)
        {
            Checker bewegenderChecker;
            while (true)
            {
                var startPosition = _view.GetPosition(true, spieler);
                if (spieler.IsSurrendering(startPosition))
                {
                    _view.PlayerHasSurrendered(spieler, gegner);
                    Environment.Exit(0);
                }
                bewegenderChecker = spieler.GetCheckerWithMatchingCoordinates(startPosition);
                if (IstDasEinGegnerischerSpielstein(gegner, startPosition))
                {
                    _view.TryMovingOpponentChecker();
                    continue;
                }
                if (IstDasEinLeeresFeld(bewegenderChecker))
                {
                    _view.SelectEmptyField();
                    continue;
                }
                if (!IstDieserSpielsteinBewegbar(bewegenderChecker, spieler, gegner))
                {
                    _view.MovingNotPossible();
                    continue;
                }
                if (IstDieserSpielsteinBewegbar(bewegenderChecker, spieler, gegner))
                {
                    if (bewegenderChecker.IsQueen && _settings.Variant == Variant.European)
                    {
                        BestimmeZugMöglichkeitenEuropäischerDame(spieler, gegner, bewegenderChecker);
                    }
                    break;
                }
            }
            return bewegenderChecker;
        }

        private void BewegeSpielstein(Checker checker, Player spieler, Player gegner, bool istSprungVerfügbar)
        {
            while (true)
            {
                var zielPosition = _view.GetPosition(false, spieler);
                if (zielPosition.IsWhiteField(zielPosition))
                {
                    _view.MoveCheckerOnWhiteField();
                    continue;
                }
                if (checker.IsFieldAlreadyOccupied(spieler, zielPosition) || checker.IsFieldAlreadyOccupied(gegner, zielPosition))
                {
                    _view.MoveCheckerOnOccupiedField();
                    continue;
                }
                if (istSprungVerfügbar)
                {
                    Springe(spieler, gegner, checker, zielPosition);
                    break;
                }
                if (!IstDasEineGültigeDiagonaleBewegung(checker, zielPosition))
                {
                    _view.TryMovingCheckerToFarAway();
                    continue;
                }
                checker.CurrentPosition = zielPosition;
                if (IstUmwandlungInDameMöglich(spieler, checker, zielPosition))
                {
                    checker.TransformCheckerInQueen();
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

        private void Springe(Player spieler, Player gegner, Checker checker, Position zielPosition)
        {
            while (!SindDasGültigeSprungKoordinaten(spieler, gegner, checker, zielPosition))
            {
                _view.ForceJump(jumpPositions);
                zielPosition = _view.GetPosition(false, spieler);
            }
            _view.RemoveChecker(checker);
            checker.CurrentPosition = zielPosition;
            if (IstUmwandlungInDameMöglich(spieler, checker, zielPosition))
            {
                checker.TransformCheckerInQueen();
                _view.TransformCheckerToQueen(checker.CurrentPosition);
                _view.MoveChecker(checker);
            }
            else
            {
                _view.MoveChecker(checker);
            }
        }

        private void SpielzugDurchführen(Player spieler, Player gegner)
        {
            while (true)
            {
                // Zu Beginn eines Spielzugs wird geprüft, ob der Spieler der gerade am Zug ist, einen oder mehrere, Sprünge vorhanden hat
                var spielsteineHabenSprung = SucheNachSprung(spieler, gegner);
                if (spielsteineHabenSprung.Any())
                {
                    var spielsteine = BestimmeSpielsteineMitMaxSprüngen(spielsteineHabenSprung);
                    var selektierterChecker = spieler.ChooseChecker(spieler, spielsteine);
                    _view.RemoveChecker(selektierterChecker);
                    // Je nach Anzahl der Sprünge, werden für jeden einzelnen Sprung folgende Anweisungen wiederholt
                    for (var i = 0; i < selektierterChecker.PossibleJumpCount; i++)
                    {
                        if (i > 0)
                        {
                            if (SprungWurdeGefunden(selektierterChecker, spieler, gegner)) { }
                        }
                        BewegeSpielstein(selektierterChecker, spieler, gegner, true);
                        _view.ShowCurrentCheckerAmount(player1.Name, player1.Checkers.Count, player2.Name, player2.Checkers.Count);
                    }
                    break;
                }
                var bewegenderChecker = HoleStartKoordinatenVonView(spieler, gegner);
                // Falls einer der Checker des Spielers keinen Sprung zur Verfügung hat, wird geprüft ob der ausgewählte Checker sich diagonal bewegen kann
                if (IstDieserSpielsteinBewegbar(bewegenderChecker, spieler, gegner))
                {
                    _view.RemoveChecker(bewegenderChecker);
                    BewegeSpielstein(bewegenderChecker, spieler, gegner, false);
                    _view.ShowCurrentCheckerAmount(player1.Name, player1.Checkers.Count, player2.Name, player2.Checkers.Count);
                    break;
                }
            }
        }

        private bool IstUmwandlungInDameMöglich(Player spieler, Checker checker, Position zielPosition)
        {
            if (!checker.IsQueen)
            {
                // Prüft ob der Checker auf der gegnerischen Grundreihe angekommen ist
                if (zielPosition.Equals(new Position(zielPosition.X, gameboard.StartingLowerRow, spieler.GameDirection)) ^
                    zielPosition.Equals(new Position(zielPosition.X, gameboard.StartingUpperRow, spieler.GameDirection)))
                {
                    return true;
                }
            }

            return false;
        }

        // ToDo - FRAGE: Warum erhalte ich eine "StackOverflowException" wenn ich den Parameter "positionSpielstein" entferne und dafür checker.Position verwende??
        private int ZähleAnzahlSprünge(Player spieler, Player gegner, Checker checker, Position positionSpielstein)
        {
            if (!checker.IsQueen)
            {
                return BerechneSprüngeFürSpielstein(spieler, gegner, checker, positionSpielstein);
            }
            if (checker.IsQueen && _settings.Variant == Variant.American)
            {
                return BerechneSprüngeAmerikanischerDame(spieler, gegner, checker, positionSpielstein);
            }
            if (checker.IsQueen && _settings.Variant == Variant.European)
            {
                return BerechneSprüngeEuropäischerDame(spieler, gegner, checker, positionSpielstein);
            }
            return 0;
        }

        private int BerechneSprüngeFürSpielstein(Player spieler, Player gegner, Checker checker, Position position)
        {
            var zielPos1 = new Position(position.SecondXRight,
                position.SecondNextYCoordinate, spieler.GameDirection);
            var zielPos2 = new Position(position.SecondXLeft,
                position.SecondNextYCoordinate, spieler.GameDirection);
            var istSprungMöglichRechts = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(position.XRight, position.NextYCoordinate, zielPos1.X, zielPos1.Y));
            var istSprungMöglichLinks = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(position.XLeft, position.NextYCoordinate, zielPos2.X, zielPos2.Y));

            int anzahlSprüngeRechts;
            if (istSprungMöglichRechts)
            {
                anzahlSprüngeRechts = 1 + BerechneSprüngeFürSpielstein(spieler, gegner, checker, zielPos1);
            }
            else
            {
                anzahlSprüngeRechts = 0;
            }
            int anzahlSprüngeLinks;
            if (istSprungMöglichLinks)
            {
                anzahlSprüngeLinks = 1 + BerechneSprüngeFürSpielstein(spieler, gegner, checker, zielPos2);
            }
            else
            {
                anzahlSprüngeLinks = 0;
            }

            return Math.Max(anzahlSprüngeRechts, anzahlSprüngeLinks);
        }

        private int BerechneSprüngeAmerikanischerDame(Player spieler, Player gegner, Checker checker, Position position)
        {
            // ToDo - FRAGE: Kann man, und wenn ja wie am besten, eine rekursive Methode wie diese hier, verkürzen? (Clean Code)
            var zielPos1 = new Position(position.SecondXRight,
                position.SecondNextYCoordinate, spieler.GameDirection);
            var zielPos2 = new Position(position.SecondXLeft,
                position.SecondNextYCoordinate, spieler.GameDirection);
            var istSprungMöglichRechts = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(position.XRight, position.NextYCoordinate, zielPos1.X, zielPos1.Y));
            var istSprungMöglichLinks = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(position.XLeft, position.NextYCoordinate, zielPos2.X, zielPos2.Y));
            var zielPos1Rückwärts = new Position(position.SecondXRight,
                position.SecondYCoordinateBackwards, spieler.GameDirection);
            var zielPos2Rückwärts = new Position(position.SecondXLeft,
                position.SecondYCoordinateBackwards, spieler.GameDirection);
            // Möglichkeiten um rückwärts zu springen, falls der Spielstein eine Dame ist
            var istSprungMöglichRechtsRückwärts = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(position.XRight, position.NextYCoordinateBackwards, zielPos1Rückwärts.X, zielPos1Rückwärts.Y));
            var istSprungMöglichLinksRückwärts = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(position.XLeft, position.NextYCoordinateBackwards, zielPos2Rückwärts.X, zielPos2Rückwärts.Y));

            int anzahlSprüngeRechts;
            if (istSprungMöglichRechts)
            {
                anzahlSprüngeRechts = 1 + BerechneSprüngeAmerikanischerDame(spieler, gegner, checker, zielPos1);
            }
            else if (istSprungMöglichRechtsRückwärts)
            {
                anzahlSprüngeRechts = 1 + BerechneAnzahlRückwärtsSprünge(spieler, gegner, checker, zielPos1Rückwärts);
            }
            else
            {
                anzahlSprüngeRechts = 0;
            }

            int anzahlSprüngeLinks;
            if (istSprungMöglichLinks)
            {
                anzahlSprüngeLinks = 1 + BerechneSprüngeAmerikanischerDame(spieler, gegner, checker, zielPos2);
            }
            else if (istSprungMöglichLinksRückwärts)
            {
                anzahlSprüngeLinks = 1 + BerechneAnzahlRückwärtsSprünge(spieler, gegner, checker, zielPos2Rückwärts);
            }
            else
            {
                anzahlSprüngeLinks = 0;
            }

            return Math.Max(anzahlSprüngeRechts, anzahlSprüngeLinks);
        }

        private int BerechneSprüngeEuropäischerDame(Player spieler, Player gegner, Checker checker, Position position)
        {
            queenPossiblePositionCount.Clear();
            var dameSprungRechts = SucheSprungNachRechts(spieler, gegner, position);
            Position sprungMöglichkeit = null;
            if (dameSprungRechts != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungRechts, new Position(dameSprungRechts.X - 1, dameSprungRechts.Y - 1, spieler.GameDirection));
            }

            var dameSprungLinks = SucheSprungNachLinks(spieler, gegner, position);
            if (dameSprungLinks != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungLinks, new Position(dameSprungLinks.X + 1, dameSprungLinks.Y - 1, spieler.GameDirection));
            }

            var dameSprungRechtsRückwärts = SucheSprungNachRechtsRückwärts(spieler, gegner, position);
            if (dameSprungRechtsRückwärts != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungRechtsRückwärts, new Position(dameSprungRechtsRückwärts.X - 1, dameSprungRechtsRückwärts.Y + 1, spieler.GameDirection));
            }

            var dameSprungLinksRückwärts = SucheSprungNachLinksRückwärts(spieler, gegner, position);
            if (dameSprungLinksRückwärts != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungLinksRückwärts, new Position(dameSprungLinksRückwärts.X + 1, dameSprungLinksRückwärts.Y + 1, spieler.GameDirection));
            }

            // Falls der Spielstein hier noch null ist, hat er keine Sprünge und wir geben den Wert 0 zurück
            if (sprungMöglichkeit == null)
            {
                return 0;
            }

            skippedCheckers.Clear();
            return queenPossiblePositionCount.Max();
        }

        private Position FügeSprungZurListeHinzu(Player spieler, Player gegner, Checker checker, Position dameSprung, Position geschlagenerSpielstein)
        {
            if (dameSprung != null)
            {
                if (!skippedCheckers.Contains(geschlagenerSpielstein))
                {
                    skippedCheckers.Add(geschlagenerSpielstein);
                    queenPossiblePositionCount.Add(1 + BerechneSprüngeEuropäischerDame(spieler, gegner, checker, dameSprung));
                }
                else
                {
                    dameSprung = null;
                }
            }

            return dameSprung;
        }

        private Position SucheSprungNachRechts(Player spieler, Player gegner, Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinate, spieler))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinate, gegner) && 
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondNextYCoordinate, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinate, neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondNextYCoordinate)))
                {
                    return new Position(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondNextYCoordinate, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X + 1, neuPosSpielstein.Y + 1, spieler.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachLinks(Player spieler, Player gegner, Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinate, spieler))
                {
                    return null;
                }
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinate, gegner) && 
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondNextYCoordinate, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinate, neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondNextYCoordinate)))
                {
                    return new Position(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondNextYCoordinate, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X - 1, neuPosSpielstein.Y + 1, spieler.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachRechtsRückwärts(Player spieler, Player gegner, Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinateBackwards, spieler))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinateBackwards, gegner) && 
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondYCoordinateBackwards, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(neuPosSpielstein.XRight, neuPosSpielstein.NextYCoordinateBackwards, neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondYCoordinateBackwards)))
                {
                    return new Position(neuPosSpielstein.SecondXRight, neuPosSpielstein.SecondYCoordinateBackwards, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X + 1, neuPosSpielstein.Y - 1, spieler.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachLinksRückwärts(Player spieler, Player gegner, Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.X, neuPosSpielstein.Y))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinateBackwards, spieler))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinateBackwards, gegner) && 
                    !gameboard.IsDiagonalFieldEmpty(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondYCoordinateBackwards, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(neuPosSpielstein.XLeft, neuPosSpielstein.NextYCoordinateBackwards, neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondYCoordinateBackwards)))
                {
                    return new Position(neuPosSpielstein.SecondXLeft, neuPosSpielstein.SecondYCoordinateBackwards, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.X - 1, neuPosSpielstein.Y - 1, spieler.GameDirection);
            }

            return null;
        }

        private int BerechneAnzahlRückwärtsSprünge(Player spieler, Player gegner, Checker checker, Position zielPositionChecker)
        {
            int anzahlSprüngeRechtsRückwärts;
            int anzahlSprüngeLinksRückwärts;

            var zielPosition1Rückwärts = new Position(zielPositionChecker.SecondXRight, zielPositionChecker.SecondYCoordinateBackwards, spieler.GameDirection);
            var zielPosition2Rückwärts = new Position(zielPositionChecker.SecondXLeft, zielPositionChecker.SecondYCoordinateBackwards, spieler.GameDirection);

            // Möglichkeiten um rückwärts zu springen
            var istSprungMöglichRechtsRückwärts = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(zielPositionChecker.XRight, zielPositionChecker.NextYCoordinateBackwards, zielPosition1Rückwärts.X, zielPosition1Rückwärts.Y));
            var istSprungMöglichLinksRückwärts = IstEinSprungVerfügbar(spieler, gegner, new JumpDestination(zielPositionChecker.XLeft, zielPositionChecker.NextYCoordinateBackwards, zielPosition2Rückwärts.X, zielPosition2Rückwärts.Y));

            if (istSprungMöglichRechtsRückwärts)
            {
                anzahlSprüngeRechtsRückwärts = 1 + BerechneAnzahlRückwärtsSprünge(spieler, gegner, checker, zielPosition1Rückwärts);
            }
            else 
                anzahlSprüngeRechtsRückwärts = 0;

            if (istSprungMöglichLinksRückwärts)
            {
                anzahlSprüngeLinksRückwärts = 1 + BerechneAnzahlRückwärtsSprünge(spieler, gegner, checker, zielPosition2Rückwärts);
            }
            else
                anzahlSprüngeLinksRückwärts = 0;

            return Math.Max(anzahlSprüngeRechtsRückwärts, anzahlSprüngeLinksRückwärts);
        }

        private static bool IstDasEinLeeresFeld(Checker selektierterSpielstein)
        {
            return selektierterSpielstein == null;
        }

        private static bool IstDasEinGegnerischerSpielstein(Player gegner, Position startPosition)
        {
            return gegner.Checkers.Any(spielstein => startPosition.Equals(spielstein.CurrentPosition));
        }

        private bool IstDasEineGültigeDiagonaleBewegung(Checker checker, Position zielPosition)
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

        private void BestimmeZugMöglichkeitenEuropäischerDame(Player spieler, Player gegner, Checker dame)
        {
            // Deklariert eine neue Liste für alle Zugmöglichkeiten einer europäischen Dame
            dame.Moves = new List<Position>();
            var zielPosition = new Position(dame.CurrentPosition.XRight, dame.CurrentPosition.NextYCoordinate, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS VORNE
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.X, zielPosition.Y, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X + 1, zielPosition.Y + 1, spieler.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XLeft, dame.CurrentPosition.NextYCoordinate, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS VORNE
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.X, zielPosition.Y, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X - 1, zielPosition.Y + 1, spieler.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XRight, dame.CurrentPosition.NextYCoordinateBackwards, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS HINTEN
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.X, zielPosition.Y, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X + 1, zielPosition.Y - 1, spieler.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XLeft, dame.CurrentPosition.NextYCoordinateBackwards, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS HINTEN
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.X, zielPosition.Y, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.X - 1, zielPosition.Y - 1, spieler.GameDirection);
            }
        }

        private bool IstDieserSpielsteinBewegbar(Checker checker, Player spieler, Player gegner)
        {
            var spielfigur = new Position(checker.CurrentPosition.X, checker.CurrentPosition.Y, spieler.GameDirection);
            if (!checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, spieler) || !gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, gegner)) 
                    && (!gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, spieler) || !gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, gegner)))
                {
                    return false;
                }
            }

            if (checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach OBEN mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, spieler) ||
                     !gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinate, gegner))
                    && (!gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, spieler) ||
                        !gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinate, gegner)))
                {
                    // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach UNTEN ebenfalls mit eigenen oder gegnerischen Spielsteinen besetzt sind
                    if ((!gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinateBackwards, spieler) ||
                         !gameboard.IsDiagonalFieldEmpty(spielfigur.XRight, spielfigur.NextYCoordinateBackwards, gegner))
                        && (!gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinateBackwards, spieler) ||
                            !gameboard.IsDiagonalFieldEmpty(spielfigur.XLeft, spielfigur.NextYCoordinateBackwards, gegner)))
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

        private bool IstEineDameBewegungMöglich(Player spieler, Player gegner, Position zielPosition)
        {
            // Prüft ob das diagonale Feld, mit den als Parameter übergebenen Koordinaten, frei ist
            if (gameboard.IsDiagonalFieldEmpty(zielPosition.X, zielPosition.Y, spieler) && gameboard.IsDiagonalFieldEmpty(zielPosition.X, zielPosition.Y, gegner))
            {
                return true;
            }

            return false;
        }
        private bool IstEinSprungVerfügbar(Player spieler, Player gegner, JumpDestination sprungRichtung)
        {
            // Prüft ob das nächste diagonale Feld besetzt ist
            if (!gameboard.IsDiagonalFieldEmpty(sprungRichtung.X, sprungRichtung.Y, gegner))
            {
                // Prüft ob das übernächste diagonale Feld frei ist, d.h. es befindet sich auf diesem Feld weder ein gegnerischer, noch ein eigener Checker
                if (gameboard.IsDiagonalFieldEmpty(sprungRichtung.XNext, sprungRichtung.YNext, gegner) && gameboard.IsDiagonalFieldEmpty(sprungRichtung.XNext, sprungRichtung.YNext, spieler))
                {
                    return true;
                }
            }

            return false;
        }

        private List<Checker> SucheNachSprung(Player spieler, Player gegner)
        {
            List<Checker> spielsteine = new List<Checker>();
            foreach (var spielstein in spieler.Checkers)
            {
                // Prüft ob der Spielstein aus dem aktuellen Durchlauf, über einen Sprung verfügt
                if (SprungWurdeGefunden(spielstein, spieler, gegner))
                {
                    spielstein.PossibleJumpCount = ZähleAnzahlSprünge(spieler, gegner, spielstein, spielstein.CurrentPosition);
                    spielsteine.Add(spielstein);
                }
            }

            return spielsteine;
        }

        private List<Checker> BestimmeSpielsteineMitMaxSprüngen(List<Checker> spielsteine)
        {
            var spielsteineGleichVieleSprünge = spielsteine.Where(c => c.PossibleJumpCount == spielsteine.Max(s => s.PossibleJumpCount)).ToList();
            ZeigeUmgewandelteKoordinatenAn(spielsteineGleichVieleSprünge);
            _view.ShowPossibleJumpCount(spielsteineGleichVieleSprünge);

            return spielsteineGleichVieleSprünge;
        }

        private bool SprungWurdeGefunden(Checker checker, Player spieler, Player gegner)
        {
            var sprungMöglichkeit3 = false;
            var sprungMöglichkeit4 = false;
            var sprungMöglichkeit5 = false;
            var sprungMöglichkeit6 = false;
            var sprungMöglichkeit7 = false;
            var sprungMöglichkeit8 = false;

            var sprungMöglichkeit1 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.LeftJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate,
                spieler.GameDirection));
            var sprungMöglichkeit2 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight,
                checker.CurrentPosition.SecondNextYCoordinate, spieler.GameDirection));

            if (checker.IsQueen && _settings.Variant == Variant.American)
            {
                sprungMöglichkeit3 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft,
                    checker.CurrentPosition.SecondYCoordinateBackwards, spieler.GameDirection));
                sprungMöglichkeit4 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight, 
                    checker.CurrentPosition.SecondYCoordinateBackwards, spieler.GameDirection));
            }
            if (checker.IsQueen && _settings.Variant == Variant.European)
            {
                sprungMöglichkeit5 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.LeftJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, spieler.GameDirection), checker);
                sprungMöglichkeit6 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.RightJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondNextYCoordinate, spieler.GameDirection), checker);
                sprungMöglichkeit7 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.LeftBackwardsJump, new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondYCoordinateBackwards, spieler.GameDirection), checker);
                sprungMöglichkeit8 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.RightBackwardsJump, new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondYCoordinateBackwards, spieler.GameDirection), checker);
            }

            if (sprungMöglichkeit1 || sprungMöglichkeit2 || sprungMöglichkeit3 || sprungMöglichkeit4 || sprungMöglichkeit5 ||
                sprungMöglichkeit6 || sprungMöglichkeit7 || sprungMöglichkeit8)
            {
                return true;
            }

            return false;
        }

        private bool SucheSprungAufNächstemDiagonalenFeld(Player spieler, Player gegner, JumpDestination richtung, Position sprungPosition)
        {
            if (IstEinSprungVerfügbar(spieler, gegner, richtung))
            {
                FügeSprungZurListeHinzu(sprungPosition);
                return true;
            }

            return false;
        }

        private bool SucheSprungAufMehrerenDiagonalenFeldern(Player spieler, Player gegner, JumpDestination richtung, Position sprungPosition, Checker checker)
        {
            var startPosition = checker.CurrentPosition;
            while (!SindKoordinatenAußerhalbSpielfeld(checker.CurrentPosition.X))
            {
                // Durchsucht alle diagonalen Felder, in eine bestimmte Richtung, nach einem Sprung
                if (SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, richtung, sprungPosition))
                {
                    checker.CurrentPosition = startPosition;
                    return true;
                }
                if (richtung.Equals(checker.CurrentPosition.LeftJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X - 1, checker.CurrentPosition.Y + spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.LeftJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondNextYCoordinate, spieler.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.RightJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X + 1, checker.CurrentPosition.Y + spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.RightJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondNextYCoordinate, spieler.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.LeftBackwardsJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X - 1, checker.CurrentPosition.Y - spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.LeftBackwardsJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXLeft, checker.CurrentPosition.SecondYCoordinateBackwards, spieler.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.RightBackwardsJump))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.X + 1, checker.CurrentPosition.Y - spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.RightBackwardsJump;
                    sprungPosition = new Position(checker.CurrentPosition.SecondXRight, checker.CurrentPosition.SecondYCoordinateBackwards, spieler.GameDirection);
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

        private bool SindDasGültigeSprungKoordinaten(Player spieler, Player gegner, Checker checker, Position zielPosition)
        {
            Position geschlagenerSpielstein;
            // Sprung nach rechts unten
            var sprungMöglichkeit1 = jumpPositions.FirstOrDefault(p => p.X > checker.CurrentPosition.X && p.Y > checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit1))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit1.X - 1, sprungMöglichkeit1.Y - 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
                jumpPositions.Clear();
                return true;
            }
            // Sprung nach links unten
            var sprungMöglichkeit2 = jumpPositions.FirstOrDefault(p => p.X < checker.CurrentPosition.X && p.Y > checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit2))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit2.X + 1, sprungMöglichkeit2.Y - 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
                jumpPositions.Clear();
                return true;
            }
            // Sprung nach rechts oben
            var sprungMöglichkeit3 = jumpPositions.FirstOrDefault(p => p.X > checker.CurrentPosition.X && p.Y < checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit3))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit3.X - 1, sprungMöglichkeit3.Y + 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
                jumpPositions.Clear();
                return true;
            }
            // Sprung nach links oben
            var sprungMöglichkeit4 = jumpPositions.FirstOrDefault(p => p.X < checker.CurrentPosition.X && p.Y < checker.CurrentPosition.Y);
            if (zielPosition.Equals(sprungMöglichkeit4))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit4.X + 1, sprungMöglichkeit4.Y + 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
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

        private void WiederholeSpielzüge()
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
                    SpielzugDurchführen(player1, player2);
                    zähleSpielzüge++;
                }
                else
                {
                    _view.ShowPlayerTurn(player2.Name, player2.CheckersColor);
                    SpielzugDurchführen(player2, player1);
                    spieler2AmZug++;
                }
            }
            //BeendeZeitMessung();
        }

        private void GewinnerErmitteln()
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

        private void BeendeAnwendung()
        {
            _view.QuitGame();
            Environment.Exit(0);
        }
    }
}