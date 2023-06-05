using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Checkers.Console.Models;
using Checkers.Console.Views;

namespace Checkers.Console
{
    // Controller kennt die View

    public class Controller
    {
        private Player _spieler1;
        private Player _spieler2;

        private readonly IView _view;
        private readonly Settings _settings;
        private readonly Gameboard _spielfeld;
        private readonly Stopwatch _zeitMessung = new Stopwatch();

        public Controller(IView view = null)
        {
            _view = view ?? new View();
            _settings = new Settings();
            _spielfeld = new Gameboard(_settings);
        }

        // Wert für die Umwandlung der yKoordinate
        private int UmwandlungsWertYKoordinate => _settings.Variante == Variant.Amerikanisch ? 10 : 8;
        // Liste um alle möglichen Sprung Positionen, pro Sprung, zu speichern (max 4. Möglichkeiten)
        private readonly List<Position> _sprungPositionen = new List<Position>();
        private readonly List<Position> _übersprungeneSpielsteine = new List<Position>();
        private List<int> _möglichkeitenEuropäischerDame= new List<int>();

        public void StarteAnwendung()
        {
            _view.KonfiguriereKonsolenfenster();
            _view.GrüßeNutzer();
            _settings.WähleSprache(_view.Sprachauswahl());
            _settings.WähleSpielvariante(_view.EingabeSpielvariante());
            _view.ErstelleSpielbrett();
            WähleSpielernamen();
            ErmittleSpielsteinFarben();
            SeitenZuweisen();
            ErzeugeSpielsteine(_spieler1);
            ErzeugeSpielsteine(_spieler2);
            PlatziereSpielsteineVorStart(_spieler1);
            PlatziereSpielsteineVorStart(_spieler2);
            WiederholeSpielzüge();
            GewinnerErmitteln();
            BeendeAnwendung();
        }

        private void WähleSpielernamen()
        {
            _spieler1 = new Player(_view) {Nummer = 1};
            _spieler2 = new Player(_view){Nummer = 2};
            _spieler1.Name = _view.EingabeSpielername(_spieler1.Nummer);
            _spieler2.Name = _view.EingabeSpielername(_spieler2.Nummer);
            while (_spieler1.Name.Equals(_spieler2.Name))
            {
                _view.NutzerWählenZweiGleicheNamen();
                _spieler1.Name = _view.EingabeSpielername(_spieler1.Nummer);
                _spieler2.Name = _view.EingabeSpielername(_spieler2.Nummer);
            }
        }

        private void ErmittleSpielsteinFarben()
        {
            _spieler1.CheckersColor = _view.FarbeDerSpielsteineErmitteln(_spieler1.Name, _spieler1.Nummer);
            _spieler2.CheckersColor = _view.FarbeDerSpielsteineErmitteln(_spieler2.Name, _spieler2.Nummer);
            while (_spieler1.CheckersColor.Equals(_spieler2.CheckersColor))
            {
                _view.NutzerSelektierenZweiGleicheFarben();
                _spieler1.CheckersColor = _view.FarbeDerSpielsteineErmitteln(_spieler1.Name, _spieler1.Nummer);
                _spieler2.CheckersColor = _view.FarbeDerSpielsteineErmitteln(_spieler2.Name, _spieler2.Nummer);
            }
        }

        private void SeitenZuweisen()
        {
            _spieler1.SetGameDirection(_view.ErmittleSeiten(_spieler1.Name));
            if (_spieler1.GameDirection == 1)
            {
                _spieler2.GameDirection = -1;
            }
            else if (_spieler1.GameDirection == -1)
            {
                _spieler2.GameDirection = 1;
            }
        }

        private void ErzeugeSpielsteine(Player spieler)
        {
            spieler.Checkers = new List<Checker>();
            // y = y-Koordinate ; x = x-Koordinate
            for (var y = 0; y < _spielfeld.RowCount; y++)
            {
                for (var x = 0; x < _spielfeld.ColumnCount; x++)
                {
                    if (spieler.GameDirection == 1 && y < _spielfeld.StartingUpperRow || spieler.GameDirection == -1 && y > _spielfeld.StartingLowerRow)
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
                _view.DruckeSpielstein(druckeChecker);
            }
        }

        private Checker HoleStartKoordinatenVonView(Player spieler, Player gegner)
        {
            Checker bewegenderChecker;
            while (true)
            {
                var startPosition = _view.EingabeKoordinaten(true, spieler);
                if (spieler.IsSurrendering(startPosition))
                {
                    _view.SpielerHatAufgegeben(spieler, gegner);
                    Environment.Exit(0);
                }
                bewegenderChecker = spieler.GetCheckerWithMatchingCoordinates(startPosition);
                if (IstDasEinGegnerischerSpielstein(gegner, startPosition))
                {
                    _view.BewegeGegnerischenSpielstein();
                    continue;
                }
                if (IstDasEinLeeresFeld(bewegenderChecker))
                {
                    _view.SelektiereLeeresFeld();
                    continue;
                }
                if (!IstDieserSpielsteinBewegbar(bewegenderChecker, spieler, gegner))
                {
                    _view.BewegeSpielsteinNichtMöglich();
                    continue;
                }
                if (IstDieserSpielsteinBewegbar(bewegenderChecker, spieler, gegner))
                {
                    if (bewegenderChecker.IsQueen && _settings.Variante == Variant.Europäisch)
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
                var zielPosition = _view.EingabeKoordinaten(false, spieler);
                if (zielPosition.IstDasEinWeißesFeld(zielPosition))
                {
                    _view.BewegeSpielsteinAufWeißesFeld();
                    continue;
                }
                if (checker.IsFieldAlreadyOccupied(spieler, zielPosition) || checker.IsFieldAlreadyOccupied(gegner, zielPosition))
                {
                    _view.BewegeCheckerAufAnderenChecker();
                    continue;
                }
                if (istSprungVerfügbar)
                {
                    Springe(spieler, gegner, checker, zielPosition);
                    break;
                }
                if (!IstDasEineGültigeDiagonaleBewegung(checker, zielPosition))
                {
                    _view.BewegeSpielsteinWeiterAlsErlaubt();
                    continue;
                }
                checker.CurrentPosition = zielPosition;
                if (IstUmwandlungInDameMöglich(spieler, checker, zielPosition))
                {
                    checker.TransformCheckerInQueen();
                    _view.SpielsteinWurdeInDameUmgewandelt(checker.CurrentPosition);
                    _view.PlatziereSpielstein(checker);
                }
                else
                {
                    _view.PlatziereSpielstein(checker);
                }
                break;
            }
        }

        private void Springe(Player spieler, Player gegner, Checker checker, Position zielPosition)
        {
            while (!SindDasGültigeSprungKoordinaten(spieler, gegner, checker, zielPosition))
            {
                _view.ErzwingeSprung(_sprungPositionen);
                zielPosition = _view.EingabeKoordinaten(false, spieler);
            }
            _view.EntferneSpielstein(checker);
            checker.CurrentPosition = zielPosition;
            if (IstUmwandlungInDameMöglich(spieler, checker, zielPosition))
            {
                checker.TransformCheckerInQueen();
                _view.SpielsteinWurdeInDameUmgewandelt(checker.CurrentPosition);
                _view.PlatziereSpielstein(checker);
            }
            else
            {
                _view.PlatziereSpielstein(checker);
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
                    _view.EntferneSpielstein(selektierterChecker);
                    // Je nach Anzahl der Sprünge, werden für jeden einzelnen Sprung folgende Anweisungen wiederholt
                    for (var i = 0; i < selektierterChecker.PossibleJumpCount; i++)
                    {
                        if (i > 0)
                        {
                            if (SprungWurdeGefunden(selektierterChecker, spieler, gegner)) { }
                        }
                        BewegeSpielstein(selektierterChecker, spieler, gegner, true);
                        _view.ZeigeAktuelleAnzahlDerSpielsteine(_spieler1.Name, _spieler1.Checkers.Count, _spieler2.Name, _spieler2.Checkers.Count);
                    }
                    break;
                }
                var bewegenderChecker = HoleStartKoordinatenVonView(spieler, gegner);
                // Falls einer der Checker des Spielers keinen Sprung zur Verfügung hat, wird geprüft ob der ausgewählte Checker sich diagonal bewegen kann
                if (IstDieserSpielsteinBewegbar(bewegenderChecker, spieler, gegner))
                {
                    _view.EntferneSpielstein(bewegenderChecker);
                    BewegeSpielstein(bewegenderChecker, spieler, gegner, false);
                    _view.ZeigeAktuelleAnzahlDerSpielsteine(_spieler1.Name, _spieler1.Checkers.Count, _spieler2.Name, _spieler2.Checkers.Count);
                    break;
                }
            }
        }

        private bool IstUmwandlungInDameMöglich(Player spieler, Checker checker, Position zielPosition)
        {
            if (!checker.IsQueen)
            {
                // Prüft ob der Checker auf der gegnerischen Grundreihe angekommen ist
                if (zielPosition.Equals(new Position(zielPosition.XKoordinate, _spielfeld.FirstPlayableRowNumber, spieler.GameDirection)) ^
                    zielPosition.Equals(new Position(zielPosition.XKoordinate, _spielfeld.LastPlayableRowNumber, spieler.GameDirection)))
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
            if (checker.IsQueen && _settings.Variante == Variant.Amerikanisch)
            {
                return BerechneSprüngeAmerikanischerDame(spieler, gegner, checker, positionSpielstein);
            }
            if (checker.IsQueen && _settings.Variante == Variant.Europäisch)
            {
                return BerechneSprüngeEuropäischerDame(spieler, gegner, checker, positionSpielstein);
            }
            return 0;
        }

        private int BerechneSprüngeFürSpielstein(Player spieler, Player gegner, Checker checker, Position position)
        {
            var zielPos1 = new Position(position.ÜbernächsteXKoordinateRechts,
                position.ÜbernächsteYKoordinate, spieler.GameDirection);
            var zielPos2 = new Position(position.ÜbernächsteXKoordinateLinks,
                position.ÜbernächsteYKoordinate, spieler.GameDirection);
            var istSprungMöglichRechts = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(position.XKoordinateRechts, position.NächsteYKoordinate, zielPos1.XKoordinate, zielPos1.YKoordinate));
            var istSprungMöglichLinks = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(position.XKoordinateLinks, position.NächsteYKoordinate, zielPos2.XKoordinate, zielPos2.YKoordinate));

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
            var zielPos1 = new Position(position.ÜbernächsteXKoordinateRechts,
                position.ÜbernächsteYKoordinate, spieler.GameDirection);
            var zielPos2 = new Position(position.ÜbernächsteXKoordinateLinks,
                position.ÜbernächsteYKoordinate, spieler.GameDirection);
            var istSprungMöglichRechts = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(position.XKoordinateRechts, position.NächsteYKoordinate, zielPos1.XKoordinate, zielPos1.YKoordinate));
            var istSprungMöglichLinks = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(position.XKoordinateLinks, position.NächsteYKoordinate, zielPos2.XKoordinate, zielPos2.YKoordinate));
            var zielPos1Rückwärts = new Position(position.ÜbernächsteXKoordinateRechts,
                position.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);
            var zielPos2Rückwärts = new Position(position.ÜbernächsteXKoordinateLinks,
                position.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);
            // Möglichkeiten um rückwärts zu springen, falls der Spielstein eine Dame ist
            var istSprungMöglichRechtsRückwärts = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(position.XKoordinateRechts, position.NächsteYKoordinateRückwärts, zielPos1Rückwärts.XKoordinate, zielPos1Rückwärts.YKoordinate));
            var istSprungMöglichLinksRückwärts = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(position.XKoordinateLinks, position.NächsteYKoordinateRückwärts, zielPos2Rückwärts.XKoordinate, zielPos2Rückwärts.YKoordinate));

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
            _möglichkeitenEuropäischerDame.Clear();
            var dameSprungRechts = SucheSprungNachRechts(spieler, gegner, position);
            Position sprungMöglichkeit = null;
            if (dameSprungRechts != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungRechts, new Position(dameSprungRechts.XKoordinate - 1, dameSprungRechts.YKoordinate - 1, spieler.GameDirection));
            }

            var dameSprungLinks = SucheSprungNachLinks(spieler, gegner, position);
            if (dameSprungLinks != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungLinks, new Position(dameSprungLinks.XKoordinate + 1, dameSprungLinks.YKoordinate - 1, spieler.GameDirection));
            }

            var dameSprungRechtsRückwärts = SucheSprungNachRechtsRückwärts(spieler, gegner, position);
            if (dameSprungRechtsRückwärts != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungRechtsRückwärts, new Position(dameSprungRechtsRückwärts.XKoordinate - 1, dameSprungRechtsRückwärts.YKoordinate + 1, spieler.GameDirection));
            }

            var dameSprungLinksRückwärts = SucheSprungNachLinksRückwärts(spieler, gegner, position);
            if (dameSprungLinksRückwärts != null)
            {
                sprungMöglichkeit = FügeSprungZurListeHinzu(spieler, gegner, checker, dameSprungLinksRückwärts, new Position(dameSprungLinksRückwärts.XKoordinate + 1, dameSprungLinksRückwärts.YKoordinate + 1, spieler.GameDirection));
            }

            // Falls der Spielstein hier noch null ist, hat er keine Sprünge und wir geben den Wert 0 zurück
            if (sprungMöglichkeit == null)
            {
                return 0;
            }

            _übersprungeneSpielsteine.Clear();
            return _möglichkeitenEuropäischerDame.Max();
        }

        private Position FügeSprungZurListeHinzu(Player spieler, Player gegner, Checker checker, Position dameSprung, Position geschlagenerSpielstein)
        {
            if (dameSprung != null)
            {
                if (!_übersprungeneSpielsteine.Contains(geschlagenerSpielstein))
                {
                    _übersprungeneSpielsteine.Add(geschlagenerSpielstein);
                    _möglichkeitenEuropäischerDame.Add(1 + BerechneSprüngeEuropäischerDame(spieler, gegner, checker, dameSprung));
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
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.XKoordinate, neuPosSpielstein.YKoordinate))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateRechts, neuPosSpielstein.NächsteYKoordinate, spieler))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateRechts, neuPosSpielstein.NächsteYKoordinate, gegner) && 
                    !_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.ÜbernächsteXKoordinateRechts, neuPosSpielstein.ÜbernächsteYKoordinate, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(neuPosSpielstein.XKoordinateRechts, neuPosSpielstein.NächsteYKoordinate, neuPosSpielstein.ÜbernächsteXKoordinateRechts, neuPosSpielstein.ÜbernächsteYKoordinate)))
                {
                    return new Position(neuPosSpielstein.ÜbernächsteXKoordinateRechts, neuPosSpielstein.ÜbernächsteYKoordinate, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.XKoordinate + 1, neuPosSpielstein.YKoordinate + 1, spieler.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachLinks(Player spieler, Player gegner, Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.XKoordinate, neuPosSpielstein.YKoordinate))
            {
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateLinks, neuPosSpielstein.NächsteYKoordinate, spieler))
                {
                    return null;
                }
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateLinks, neuPosSpielstein.NächsteYKoordinate, gegner) && 
                    !_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.ÜbernächsteXKoordinateLinks, neuPosSpielstein.ÜbernächsteYKoordinate, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(neuPosSpielstein.XKoordinateLinks, neuPosSpielstein.NächsteYKoordinate, neuPosSpielstein.ÜbernächsteXKoordinateLinks, neuPosSpielstein.ÜbernächsteYKoordinate)))
                {
                    return new Position(neuPosSpielstein.ÜbernächsteXKoordinateLinks, neuPosSpielstein.ÜbernächsteYKoordinate, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.XKoordinate - 1, neuPosSpielstein.YKoordinate + 1, spieler.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachRechtsRückwärts(Player spieler, Player gegner, Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.XKoordinate, neuPosSpielstein.YKoordinate))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateRechts, neuPosSpielstein.NächsteYKoordinateRückwärts, spieler))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateRechts, neuPosSpielstein.NächsteYKoordinateRückwärts, gegner) && 
                    !_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.ÜbernächsteXKoordinateRechts, neuPosSpielstein.ÜbernächsteYKoordinateRückwärts, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(neuPosSpielstein.XKoordinateRechts, neuPosSpielstein.NächsteYKoordinateRückwärts, neuPosSpielstein.ÜbernächsteXKoordinateRechts, neuPosSpielstein.ÜbernächsteYKoordinateRückwärts)))
                {
                    return new Position(neuPosSpielstein.ÜbernächsteXKoordinateRechts, neuPosSpielstein.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.XKoordinate + 1, neuPosSpielstein.YKoordinate - 1, spieler.GameDirection);
            }

            return null;
        }

        private Position SucheSprungNachLinksRückwärts(Player spieler, Player gegner, Position neuPosSpielstein)
        {
            while (!SindKoordinatenAußerhalbSpielfeld(neuPosSpielstein.XKoordinate, neuPosSpielstein.YKoordinate))
            {
                // Falls von der neu berechneten Position, sich diagonal rechts ein eigener Spielstein befindet, wird die "Suche" beendet
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateLinks, neuPosSpielstein.NächsteYKoordinateRückwärts, spieler))
                {
                    return null;
                }
                // Falls diagonal rechts UND auf dem übernächsten diagonalen Feld rechts sich ein gegnerischer Spielstein befindet, wird die "Suche" ebenfalls beendet
                if (!_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.XKoordinateLinks, neuPosSpielstein.NächsteYKoordinateRückwärts, gegner) && 
                    !_spielfeld.IsDiagonalFieldEmpty(neuPosSpielstein.ÜbernächsteXKoordinateLinks, neuPosSpielstein.ÜbernächsteYKoordinateRückwärts, gegner))
                {
                    return null;
                }
                if (IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(neuPosSpielstein.XKoordinateLinks, neuPosSpielstein.NächsteYKoordinateRückwärts, neuPosSpielstein.ÜbernächsteXKoordinateLinks, neuPosSpielstein.ÜbernächsteYKoordinateRückwärts)))
                {
                    return new Position(neuPosSpielstein.ÜbernächsteXKoordinateLinks, neuPosSpielstein.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);
                }
                neuPosSpielstein = new Position(neuPosSpielstein.XKoordinate - 1, neuPosSpielstein.YKoordinate - 1, spieler.GameDirection);
            }

            return null;
        }

        private int BerechneAnzahlRückwärtsSprünge(Player spieler, Player gegner, Checker checker, Position zielPositionChecker)
        {
            int anzahlSprüngeRechtsRückwärts;
            int anzahlSprüngeLinksRückwärts;

            var zielPosition1Rückwärts = new Position(zielPositionChecker.ÜbernächsteXKoordinateRechts, zielPositionChecker.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);
            var zielPosition2Rückwärts = new Position(zielPositionChecker.ÜbernächsteXKoordinateLinks, zielPositionChecker.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);

            // Möglichkeiten um rückwärts zu springen
            var istSprungMöglichRechtsRückwärts = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(zielPositionChecker.XKoordinateRechts, zielPositionChecker.NächsteYKoordinateRückwärts, zielPosition1Rückwärts.XKoordinate, zielPosition1Rückwärts.YKoordinate));
            var istSprungMöglichLinksRückwärts = IstEinSprungVerfügbar(spieler, gegner, new SprungRichtung(zielPositionChecker.XKoordinateLinks, zielPositionChecker.NächsteYKoordinateRückwärts, zielPosition2Rückwärts.XKoordinate, zielPosition2Rückwärts.YKoordinate));

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
            if (checker.IsQueen && _settings.Variante == Variant.Amerikanisch)
            {
                if (checker.IsAmericanQueenMoveValid(zielPosition)) return true;
            }
            if (checker.IsQueen && _settings.Variante == Variant.Europäisch)
            {
                if (checker.IsEuropeanQueenMoveValid(zielPosition)) return true;
            }

            return false;
        }

        private void BestimmeZugMöglichkeitenEuropäischerDame(Player spieler, Player gegner, Checker dame)
        {
            // Deklariert eine neue Liste für alle Zugmöglichkeiten einer europäischen Dame
            dame.Moves = new List<Position>();
            var zielPosition = new Position(dame.CurrentPosition.XKoordinateRechts, dame.CurrentPosition.NächsteYKoordinate, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS VORNE
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.XKoordinate, zielPosition.YKoordinate, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.XKoordinate + 1, zielPosition.YKoordinate + 1, spieler.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XKoordinateLinks, dame.CurrentPosition.NächsteYKoordinate, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS VORNE
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.XKoordinate, zielPosition.YKoordinate, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.XKoordinate - 1, zielPosition.YKoordinate + 1, spieler.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XKoordinateRechts, dame.CurrentPosition.NächsteYKoordinateRückwärts, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach RECHTS HINTEN
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.XKoordinate, zielPosition.YKoordinate, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.XKoordinate + 1, zielPosition.YKoordinate - 1, spieler.GameDirection);
            }
            zielPosition = new Position(dame.CurrentPosition.XKoordinateLinks, dame.CurrentPosition.NächsteYKoordinateRückwärts, spieler.GameDirection);
            // Bestimmt die möglichen Züge einer Dame nach LINKS HINTEN
            while (IstEineDameBewegungMöglich(spieler, gegner, new Position(zielPosition.XKoordinate, zielPosition.YKoordinate, spieler.GameDirection)))
            {
                dame.Moves.Add(zielPosition);
                zielPosition = new Position(zielPosition.XKoordinate - 1, zielPosition.YKoordinate - 1, spieler.GameDirection);
            }
        }

        private bool IstDieserSpielsteinBewegbar(Checker checker, Player spieler, Player gegner)
        {
            var spielfigur = new Position(checker.CurrentPosition.XKoordinate, checker.CurrentPosition.YKoordinate, spieler.GameDirection);
            if (!checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateRechts, spielfigur.NächsteYKoordinate, spieler) || !_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateRechts, spielfigur.NächsteYKoordinate, gegner)) 
                    && (!_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateLinks, spielfigur.NächsteYKoordinate, spieler) || !_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateLinks, spielfigur.NächsteYKoordinate, gegner)))
                {
                    return false;
                }
            }

            if (checker.IsQueen)
            {
                // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach OBEN mit eigenen oder gegnerischen Spielsteinen besetzt sind
                if ((!_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateRechts, spielfigur.NächsteYKoordinate, spieler) ||
                     !_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateRechts, spielfigur.NächsteYKoordinate, gegner))
                    && (!_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateLinks, spielfigur.NächsteYKoordinate, spieler) ||
                        !_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateLinks, spielfigur.NächsteYKoordinate, gegner)))
                {
                    // Bedingung trifft zu, wenn BEIDE diagonalen Felder nach UNTEN ebenfalls mit eigenen oder gegnerischen Spielsteinen besetzt sind
                    if ((!_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateRechts, spielfigur.NächsteYKoordinateRückwärts, spieler) ||
                         !_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateRechts, spielfigur.NächsteYKoordinateRückwärts, gegner))
                        && (!_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateLinks, spielfigur.NächsteYKoordinateRückwärts, spieler) ||
                            !_spielfeld.IsDiagonalFieldEmpty(spielfigur.XKoordinateLinks, spielfigur.NächsteYKoordinateRückwärts, gegner)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool SindKoordinatenAußerhalbSpielfeld(int xKoordinate, int yKoordinate = 0)
        {
            if (xKoordinate > _spielfeld.LastPlayableRowNumber || xKoordinate < _spielfeld.FirstPlayableRowNumber)
            {
                return true;
            }

            if (yKoordinate > _spielfeld.LastPlayableRowNumber || yKoordinate < _spielfeld.FirstPlayableRowNumber)
            {
                return true;
            }

            return false;
        }

        private bool IstEineDameBewegungMöglich(Player spieler, Player gegner, Position zielPosition)
        {
            // Prüft ob das diagonale Feld, mit den als Parameter übergebenen Koordinaten, frei ist
            if (_spielfeld.IsDiagonalFieldEmpty(zielPosition.XKoordinate, zielPosition.YKoordinate, spieler) && _spielfeld.IsDiagonalFieldEmpty(zielPosition.XKoordinate, zielPosition.YKoordinate, gegner))
            {
                return true;
            }

            return false;
        }
        private bool IstEinSprungVerfügbar(Player spieler, Player gegner, SprungRichtung sprungRichtung)
        {
            // Prüft ob das nächste diagonale Feld besetzt ist
            if (!_spielfeld.IsDiagonalFieldEmpty(sprungRichtung.NächsteXKoordinate, sprungRichtung.NächsteYKoordinate, gegner))
            {
                // Prüft ob das übernächste diagonale Feld frei ist, d.h. es befindet sich auf diesem Feld weder ein gegnerischer, noch ein eigener Checker
                if (_spielfeld.IsDiagonalFieldEmpty(sprungRichtung.ÜbernächsteXKoordinate, sprungRichtung.ÜbernächsteYKoordinate, gegner) && _spielfeld.IsDiagonalFieldEmpty(sprungRichtung.ÜbernächsteXKoordinate, sprungRichtung.ÜbernächsteYKoordinate, spieler))
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
            _view.ZeigeAnzahlGefundenerSprünge(spielsteineGleichVieleSprünge);

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

            var sprungMöglichkeit1 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.SprungLinksVorne, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateLinks, checker.CurrentPosition.ÜbernächsteYKoordinate,
                spieler.GameDirection));
            var sprungMöglichkeit2 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.SprungRechtsVorne, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateRechts,
                checker.CurrentPosition.ÜbernächsteYKoordinate, spieler.GameDirection));

            if (checker.IsQueen && _settings.Variante == Variant.Amerikanisch)
            {
                sprungMöglichkeit3 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.SprungLinksHinten, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateLinks,
                    checker.CurrentPosition.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection));
                sprungMöglichkeit4 = SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, checker.CurrentPosition.SprungRechtsHinten, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateRechts, 
                    checker.CurrentPosition.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection));
            }
            if (checker.IsQueen && _settings.Variante == Variant.Europäisch)
            {
                sprungMöglichkeit5 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.SprungLinksVorne, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateLinks, checker.CurrentPosition.ÜbernächsteYKoordinate, spieler.GameDirection), checker);
                sprungMöglichkeit6 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.SprungRechtsVorne, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateRechts, checker.CurrentPosition.ÜbernächsteYKoordinate, spieler.GameDirection), checker);
                sprungMöglichkeit7 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.SprungLinksHinten, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateLinks, checker.CurrentPosition.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection), checker);
                sprungMöglichkeit8 = SucheSprungAufMehrerenDiagonalenFeldern(spieler, gegner, checker.CurrentPosition.SprungRechtsHinten, new Position(checker.CurrentPosition.ÜbernächsteXKoordinateRechts, checker.CurrentPosition.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection), checker);
            }

            if (sprungMöglichkeit1 || sprungMöglichkeit2 || sprungMöglichkeit3 || sprungMöglichkeit4 || sprungMöglichkeit5 ||
                sprungMöglichkeit6 || sprungMöglichkeit7 || sprungMöglichkeit8)
            {
                return true;
            }

            return false;
        }

        private bool SucheSprungAufNächstemDiagonalenFeld(Player spieler, Player gegner, SprungRichtung richtung, Position sprungPosition)
        {
            if (IstEinSprungVerfügbar(spieler, gegner, richtung))
            {
                FügeSprungZurListeHinzu(sprungPosition);
                return true;
            }

            return false;
        }

        private bool SucheSprungAufMehrerenDiagonalenFeldern(Player spieler, Player gegner, SprungRichtung richtung, Position sprungPosition, Checker checker)
        {
            var startPosition = checker.CurrentPosition;
            while (!SindKoordinatenAußerhalbSpielfeld(checker.CurrentPosition.XKoordinate))
            {
                // Durchsucht alle diagonalen Felder, in eine bestimmte Richtung, nach einem Sprung
                if (SucheSprungAufNächstemDiagonalenFeld(spieler, gegner, richtung, sprungPosition))
                {
                    checker.CurrentPosition = startPosition;
                    return true;
                }
                if (richtung.Equals(checker.CurrentPosition.SprungLinksVorne))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.XKoordinate - 1, checker.CurrentPosition.YKoordinate + spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.SprungLinksVorne;
                    sprungPosition = new Position(checker.CurrentPosition.ÜbernächsteXKoordinateLinks, checker.CurrentPosition.ÜbernächsteYKoordinate, spieler.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.SprungRechtsVorne))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.XKoordinate + 1, checker.CurrentPosition.YKoordinate + spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.SprungRechtsVorne;
                    sprungPosition = new Position(checker.CurrentPosition.ÜbernächsteXKoordinateRechts, checker.CurrentPosition.ÜbernächsteYKoordinate, spieler.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.SprungLinksHinten))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.XKoordinate - 1, checker.CurrentPosition.YKoordinate - spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.SprungLinksHinten;
                    sprungPosition = new Position(checker.CurrentPosition.ÜbernächsteXKoordinateLinks, checker.CurrentPosition.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);
                }
                else if (richtung.Equals(checker.CurrentPosition.SprungRechtsHinten))
                {
                    checker.CurrentPosition = new Position(checker.CurrentPosition.XKoordinate + 1, checker.CurrentPosition.YKoordinate - spieler.GameDirection, spieler.GameDirection);
                    richtung = checker.CurrentPosition.SprungRechtsHinten;
                    sprungPosition = new Position(checker.CurrentPosition.ÜbernächsteXKoordinateRechts, checker.CurrentPosition.ÜbernächsteYKoordinateRückwärts, spieler.GameDirection);
                }
            }

            checker.CurrentPosition = startPosition;
            return false;
        }

        private void FügeSprungZurListeHinzu(Position sprungPosition)
        {
            if (!_sprungPositionen.Contains(sprungPosition))
            {
                _sprungPositionen.Add(sprungPosition);
            }
        }

        private void ZeigeUmgewandelteKoordinatenAn(List<Checker> spielsteine)
        {
            // Umgewandelte x und y Koordinaten des ERSTEN gefundenen Spielstein, welcher über einen Sprung verfügt
            var xKoordinateAlsBuchstabe = Convert.ToChar(spielsteine[0].CurrentPosition.XKoordinate + 97);
            var yKoordinateUmgewandelt = (spielsteine[0].CurrentPosition.YKoordinate - UmwandlungsWertYKoordinate) * -1;

            if (spielsteine.Count == 1)
            {
                _view.ZeigeEinenSprungAn(xKoordinateAlsBuchstabe, yKoordinateUmgewandelt);
            }
            else if (spielsteine.Count == 2)
            {
                // Umgewandelte x und y Koordinaten des ZWEITEN gefundenen Spielstein, welcher über einen Sprung verfügt
                var xKoordinate2AlsBuchstabe = Convert.ToChar(spielsteine[1].CurrentPosition.XKoordinate + 97);
                var yKoordinate2Umgewandelt = (spielsteine[1].CurrentPosition.YKoordinate - UmwandlungsWertYKoordinate) * -1;
                _view.ZeigeZweiMöglicheSprüngeAn(xKoordinateAlsBuchstabe, yKoordinateUmgewandelt, xKoordinate2AlsBuchstabe, yKoordinate2Umgewandelt);
            }
        }

        private bool SindDasGültigeSprungKoordinaten(Player spieler, Player gegner, Checker checker, Position zielPosition)
        {
            Position geschlagenerSpielstein;
            // Sprung nach rechts unten
            var sprungMöglichkeit1 = _sprungPositionen.FirstOrDefault(p => p.XKoordinate > checker.CurrentPosition.XKoordinate && p.YKoordinate > checker.CurrentPosition.YKoordinate);
            if (zielPosition.Equals(sprungMöglichkeit1))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit1.XKoordinate - 1, sprungMöglichkeit1.YKoordinate - 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
                _sprungPositionen.Clear();
                return true;
            }
            // Sprung nach links unten
            var sprungMöglichkeit2 = _sprungPositionen.FirstOrDefault(p => p.XKoordinate < checker.CurrentPosition.XKoordinate && p.YKoordinate > checker.CurrentPosition.YKoordinate);
            if (zielPosition.Equals(sprungMöglichkeit2))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit2.XKoordinate + 1, sprungMöglichkeit2.YKoordinate - 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
                _sprungPositionen.Clear();
                return true;
            }
            // Sprung nach rechts oben
            var sprungMöglichkeit3 = _sprungPositionen.FirstOrDefault(p => p.XKoordinate > checker.CurrentPosition.XKoordinate && p.YKoordinate < checker.CurrentPosition.YKoordinate);
            if (zielPosition.Equals(sprungMöglichkeit3))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit3.XKoordinate - 1, sprungMöglichkeit3.YKoordinate + 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
                _sprungPositionen.Clear();
                return true;
            }
            // Sprung nach links oben
            var sprungMöglichkeit4 = _sprungPositionen.FirstOrDefault(p => p.XKoordinate < checker.CurrentPosition.XKoordinate && p.YKoordinate < checker.CurrentPosition.YKoordinate);
            if (zielPosition.Equals(sprungMöglichkeit4))
            {
                geschlagenerSpielstein = new Position(sprungMöglichkeit4.XKoordinate + 1, sprungMöglichkeit4.YKoordinate + 1, spieler.GameDirection);
                EntferneGegnerischenSpielstein(gegner.Checkers, geschlagenerSpielstein);
                _sprungPositionen.Clear();
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
                    _view.EntferneSpielstein(gegnerischeSpielsteine[i]);
                    // Entfernt den geschlagenen Spielstein aus der Liste der Spielfiguren
                    gegnerischeSpielsteine.Remove(gegnerischeSpielsteine[i]);
                    _view.SchlageGegnerischenSpielstein();
                    break;
                }
            }
        }

        // ToDo: mit Implementierung eines Timers fortsetzen (Idee: Den Timer z.B. jede 15 Sekunden aktualisieren lassen)
        private void GeneriereZeitMesser()
        {
            _zeitMessung.Start();
            if (_zeitMessung.ElapsedMilliseconds >= 15000)
            {
                _view.ZeigeVerstricheneZeitAn(_zeitMessung);
                Thread.Sleep(1000);
            }
        }

        private void BeendeZeitMessung()
        {
            _zeitMessung.Stop();
        }

        private void WiederholeSpielzüge()
        {
            _view.SpielinformationenZumAufgeben();
            var zähleSpielzüge = 1;
            var spieler2AmZug = 0;
            //GeneriereZeitMesser();
            // Das Spiel geht solange, bis einer der beiden Spieler keine Spielfiguren mehr hat
            while (_spieler1.Checkers.Count != 0 || _spieler2.Checkers.Count != 0)
            {
                if ((zähleSpielzüge + spieler2AmZug) % 2 != 0)
                {
                    _view.ZeigeSpielzugAn(_spieler1.Name, _spieler1.CheckersColor);
                    SpielzugDurchführen(_spieler1, _spieler2);
                    zähleSpielzüge++;
                }
                else
                {
                    _view.ZeigeSpielzugAn(_spieler2.Name, _spieler2.CheckersColor);
                    SpielzugDurchführen(_spieler2, _spieler1);
                    spieler2AmZug++;
                }
            }
            //BeendeZeitMessung();
        }

        private void GewinnerErmitteln()
        {
            if (_spieler1.Checkers.Count == 0)
            {
                _view.ZeigeGewinnerAn(_spieler2.Name);
            }
            else if (_spieler2.Checkers.Count == 0)
            {
                _view.ZeigeGewinnerAn(_spieler1.Name);
            }
        }

        private void BeendeAnwendung()
        {
            _view.BeendeSpiel();
            Environment.Exit(0);
        }
    }
}