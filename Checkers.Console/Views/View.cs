using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Checkers.Console.Models;
using Checkers.Console.Resources;

namespace Checkers.Console.Views
{
    public class View : IView
    {
        public int ColumnCount => Variant == Variant.American ? 10 : 8;
        public int RowCount => Variant == Variant.American ? 10 : 8;
        public int CursorXCoordinate => Variant == Variant.American ? 76 : 62;
        public int CursorYCoordinate => Variant == Variant.American ? 10: 8;
        public Variant Variant { get; set; }

        private const int Feldbreite = 7;
        private const int Feldhöhe = 3;
        // ASCII Nummer für 'a'
        private const int AsciiNummerA = 97;
        private const string LeeresFeld = "                                                                          ";

        public void SetUp()
        {
            // ToDo: Refactor this to not use absolute values
            // Console.BufferWidth = 150;
            // Console.SetWindowSize(Console.BufferWidth, 40);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
            System.Console.Clear();
        }

        public void GreetPlayer()
        {
            System.Console.SetCursorPosition(55, 1);
            System.Console.WriteLine(GlobalStrings.Willkommenstext);
            System.Console.SetCursorPosition(40, 2);
            System.Console.WriteLine(GlobalStrings.Trennzeichen);
        }

        public int ChooseLanguage()
        {
            System.Console.SetCursorPosition(56, 4);
            System.Console.WriteLine(GlobalStrings.VerfügbareSprachen);
            System.Console.SetCursorPosition(58, 5);
            System.Console.WriteLine(GlobalStrings.Englisch);
            System.Console.SetCursorPosition(58, 6);
            System.Console.WriteLine(GlobalStrings.Deutsch);
            System.Console.SetCursorPosition(58, 7);
            System.Console.WriteLine(GlobalStrings.Polnisch);
            System.Console.SetCursorPosition(58, 8);
            System.Console.Write(GlobalStrings.AuswahlDerSprache);
            int sprachauswahl = Convert.ToInt32(System.Console.ReadLine());
            switch (sprachauswahl)
            {
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 3;
                default:
                    System.Console.SetCursorPosition(37, 10);
                    System.Console.WriteLine(GlobalStrings.FehlerSprachauswahl);
                    break;
            }

            return 0;
        }

        public Variant SelectGameVariant()
        {
            // Entfernt die Ausgaben, für die Sprachauswahl
            ClearText(56, 4);
            ClearText(58, 5);
            ClearText(58, 6);
            ClearText(58, 7);
            ClearText(58, 8);
            ClearText(37, 10);
            System.Console.SetCursorPosition(37, 4);
            System.Console.WriteLine(GlobalStrings.SpielVariantenAuswahl);
            System.Console.SetCursorPosition(46, 5);
            System.Console.Write(GlobalStrings.VerfügbareVarianten);
            var spielversion = System.Console.ReadLine();
            while (string.IsNullOrWhiteSpace(spielversion) || spielversion != "1" && spielversion != "2")
            {
                System.Console.SetCursorPosition(46, 7);
                System.Console.Write(GlobalStrings.FehlerKeineGültigeZahl);
                spielversion = System.Console.ReadLine();
            }

            if (spielversion == "1")
            {
                Variant = Variant.American;
            }
            if (spielversion == "2")
            {
                Variant = Variant.European;
            }
            // Entfernt den "Willkommenstext"
            ClearText(55, 1);
            ClearText(40, 2);
            // Entfernt die Konsolenausgaben aus dieser Methode
            ClearText(37, 4);
            ClearText(46, 5);
            ClearText(46, 7);
            return Variant;
        }

        public string GetPlayerName(int spielerNummer)
        {
            var gültigeEingabe = false;
            var eingabeSpielerName = "";
            while (!gültigeEingabe)
            {
                // Entfernt den eingegebenen Namen des ersten Spielers
                ClearText(CursorXCoordinate, 4);
                System.Console.SetCursorPosition(CursorXCoordinate, 4);
                System.Console.Write(GlobalStrings.EingabeSpielername, spielerNummer);
                eingabeSpielerName = System.Console.ReadLine();
                gültigeEingabe = true;
                if (string.IsNullOrWhiteSpace(eingabeSpielerName))
                {
                    System.Console.SetCursorPosition(CursorXCoordinate, 10);
                    System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
                    gültigeEingabe = false;
                }
            }
            return eingabeSpielerName;
        }

        public string GetStartingSite(string spielername)
        {
            // Entfernt den Fehlertext "NutzerSelektiertZweiGleicheFarben"
            ClearText(CursorXCoordinate, 10);
            // Der Nutzer kann entscheiden auf welcher Seite er beginnen möchte (oben oder unten)
            System.Console.SetCursorPosition(CursorXCoordinate, 8);
            System.Console.WriteLine(GlobalStrings.AuswahlSpielseite, spielername);
            System.Console.SetCursorPosition(CursorXCoordinate, 9);
            System.Console.Write(GlobalStrings.ObenOderUnten);

            var eingabeSeitenwahl = System.Console.ReadLine();
            // Solange die Nutzereingabe nicht dem Wert 1 ODER 2 entspricht ODER leer ist, muss der Nutzer einen neuen Wert eingeben
            while (eingabeSeitenwahl != "1" && eingabeSeitenwahl != "2" || string.IsNullOrWhiteSpace(eingabeSeitenwahl))
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 11);
                System.Console.WriteLine(GlobalStrings.FehlerKeineGültigeZahl);
                ClearText(CursorXCoordinate, 9);
                System.Console.SetCursorPosition(CursorXCoordinate, 9);
                System.Console.Write(GlobalStrings.ObenOderUnten);
                eingabeSeitenwahl = System.Console.ReadLine();
            }
            // Entfernt die Texte aus der Methode "FarbeDerSpielsteineErmitteln"
            ClearText(CursorXCoordinate, 4);
            ClearText(CursorXCoordinate, 5);
            // Entfernt die Texte/Meldungen aus dieser Methode
            ClearText(CursorXCoordinate, 8);
            ClearText(CursorXCoordinate, 9);
            ClearText(CursorXCoordinate, 11);
            // Entfernt den Text der die ausgewählten Farben dem Nutzer anzeigt
            ClearText(CursorXCoordinate, 12);
            ClearText(CursorXCoordinate, 13);

            return eingabeSeitenwahl;
        }

        public ConsoleColor GetSelectedColor(string spielername, int spielerNummer)
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 4);
            System.Console.WriteLine(GlobalStrings.InfoText1FarbAuswahl);
            System.Console.SetCursorPosition(CursorXCoordinate, 5);
            System.Console.WriteLine(GlobalStrings.InfoText2FarbAuswahl);
            var valid = false;
            var spielsteinFarbe = ConsoleColor.White;
            while (!valid)
            {
                ClearText(CursorXCoordinate, 7);
                System.Console.SetCursorPosition(CursorXCoordinate, 7);
                System.Console.Write(GlobalStrings.AuswahlSpielsteinFarbe, spielerNummer, spielername);
                var spielerFarbenAuswahl = System.Console.ReadLine();

                if (string.IsNullOrWhiteSpace(spielerFarbenAuswahl))
                {
                    System.Console.SetCursorPosition(CursorXCoordinate, 9);
                    System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
                    continue;
                }

                if (spielerFarbenAuswahl.ToLower() == GlobalStrings.Rot)
                {
                    valid = true;
                    spielsteinFarbe = ConsoleColor.Red;
                }
                else if (spielerFarbenAuswahl.ToLower() == GlobalStrings.Blau)
                {
                    valid = true;
                    spielsteinFarbe = ConsoleColor.Blue;
                }
                else if (spielerFarbenAuswahl.ToLower() == GlobalStrings.Grün)
                {
                    valid = true;
                    spielsteinFarbe = ConsoleColor.Green;
                }
                else if (spielerFarbenAuswahl.ToLower() == GlobalStrings.Gelb)
                {
                    valid = true;
                    spielsteinFarbe = ConsoleColor.Yellow;
                }
                else
                {
                    System.Console.SetCursorPosition(CursorXCoordinate, 9);
                    System.Console.WriteLine(GlobalStrings.FehlerUngültigeFarbe);
                }
            }
            if (spielerNummer == 1)
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 12);
                System.Console.WriteLine(GlobalStrings.FarbenAuswahlÜbersicht, spielerNummer, spielername, spielsteinFarbe);
            }
            else if (spielerNummer == 2)
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 13);
                System.Console.WriteLine(GlobalStrings.FarbenAuswahlÜbersicht, spielerNummer, spielername, spielsteinFarbe);
            }
            ClearText(CursorXCoordinate, 7);
            ClearText(CursorXCoordinate, 9);
            return spielsteinFarbe;
        }

        public void EqualCheckerColorNotAllowed()
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 10);
            System.Console.WriteLine(GlobalStrings.SelektiereZweiGleicheFarben);
        }

        public void EqualNamesNotAllowed()
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 10);
            System.Console.WriteLine(GlobalStrings.SelektiereZweiGleicheNamen);
        }

        public void CreateGameboard()
        {
            for (var zeile = 0; zeile < RowCount; zeile++)
            {
                for (var höhe = 0; höhe < Feldhöhe; höhe++)
                {
                    if (höhe == Feldhöhe / 2)
                    {
                        CreateColumns(zeile);
                    }
                    else
                    {
                        ErstelleLeerzeichenAufYAchse(höhe, zeile);
                    }
                    CreateRows(zeile);
                    // Beginnt eine neue Zeile
                    System.Console.WriteLine();
                }
            }
            CreateXAxis();
        }

        private static void ErstelleLeerzeichenAufYAchse(int höhe, int zeile)
        {
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            if (höhe == 2)
            {
                if (zeile == 0)
                {
                    System.Console.SetCursorPosition(0, 3);
                }
                else
                {
                    System.Console.SetCursorPosition(0, 3 + zeile * 3);
                }
            }
            else
            {
                System.Console.SetCursorPosition(0, 1 + zeile * Feldhöhe);
            }
            System.Console.Write("   ");
        }

        public void CreateColumns(int columnNumber)
        {
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
            if (Variant == Variant.American && columnNumber == 0)
            {
                // der Wert 10 der y-Achse, wird hier mittig ausgegeben
                System.Console.Write((RowCount - columnNumber) + " ");
            }
            else
            {
                // die Werte 1-9 der y-Achse werden hier mittig ausgegeben
                System.Console.Write((RowCount - columnNumber) + "  ");
            }
        }

        public void CreateRows(int reihe)
        {
            // Erstellt einzelne Spalten in horizontaler Richtung von links nach rechts
            for (var spalte = 0; spalte < ColumnCount; spalte++)
            {
                for (var b = 0; b < Feldbreite; b++)
                {
                    if ((reihe + spalte) % 2 == 0)
                    {
                        System.Console.BackgroundColor = ConsoleColor.White;
                        System.Console.Write(" ");
                    }
                    else
                    {
                        System.Console.BackgroundColor = ConsoleColor.Black;
                        System.Console.Write(" ");
                    }
                }
            }
        }

        public void CreateXAxis()
        {
            for (var i = 0; i < ColumnCount; i++)
            {
                var asciiAlsBuchstabe = AsciiNummerA + i;
                // Berechnung der Buchstaben auf der x-Achse um die Buchstaben mittig auszugeben
                System.Console.SetCursorPosition(i * Feldbreite + Feldbreite / 2 + 3, RowCount * Feldhöhe + 1);
                System.Console.BackgroundColor = ConsoleColor.DarkYellow;
                System.Console.Write((char) asciiAlsBuchstabe);
            }
        }

        public void PrintChecker(Checker checker)
        {
            System.Console.SetCursorPosition(Feldbreite * checker.CurrentPosition.X + 6, Feldhöhe * checker.CurrentPosition.Y + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = checker.Color;
            System.Console.Write(GlobalStrings.Spielstein);
            // Nachdem der letzte Spielstein platziert wurde, werden die Farben der Konsole wieder auf "Default" gesetzt
            SetGameColors();
        }

        public void ShowSurrenderOption()
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 2);
            System.Console.Write(GlobalStrings.InformationZumAufgeben1);
            System.Console.SetCursorPosition(CursorXCoordinate, 3);
            System.Console.Write(GlobalStrings.InformationZumAufgeben2);
        }

        public Position GetPosition(bool istStartKoordinate, Player spieler)
        {
            SetGameColors();
            // Entfernt die eingegebenen Werte für die nächste Eingabe
            ClearText(CursorXCoordinate, 10);
            ClearText(CursorXCoordinate, 11);
            ClearText(CursorXCoordinate, 13);
            ClearText(CursorXCoordinate, 14);
            var xKoordinateAlsZahl = 0;
            var ersterBuchstabe = '\0';
            var valid = false;
            while (!valid)
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 10);
                System.Console.Write(GlobalStrings.EingabeXKoordinate);
                var buchstabe = System.Console.ReadLine();
                // Prüft ob die Eingabe des Nutzers leer ist
                if (string.IsNullOrWhiteSpace(buchstabe))
                {
                    System.Console.SetCursorPosition(CursorXCoordinate, 11);
                    System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
                    continue;
                }
                if (buchstabe == "!gg")
                {
                    return null;
                }
                ersterBuchstabe = buchstabe.ToLower().ToCharArray().First();
                var ascii = (int) ersterBuchstabe;
                if (ascii >= 97  && ascii < 97 + ColumnCount)
                {
                    xKoordinateAlsZahl = ascii - 97;
                    valid = true;
                }
                else
                {
                    System.Console.SetCursorPosition(CursorXCoordinate, 11);
                    System.Console.WriteLine(GlobalStrings.FehlerUngültigerKoordinatenwert);
                }
            }
            System.Console.SetCursorPosition(CursorXCoordinate, 13);
            System.Console.Write(GlobalStrings.EingabeYKoordinate);
            int yKoordinate;
            while (!int.TryParse(System.Console.ReadLine(), out yKoordinate))
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 14);
                System.Console.WriteLine(GlobalStrings.FehlerKoordinateIstKeineZahl);
                System.Console.SetCursorPosition(CursorXCoordinate, 13);
                System.Console.Write(GlobalStrings.EingabeYKoordinate);
            }
            var yKoordinateUmgewandelt = (yKoordinate - CursorYCoordinate) * -1;
            if (istStartKoordinate)
            {
                ShowCurrentPosition(ersterBuchstabe, yKoordinate);
            }

            if (!istStartKoordinate)
            {
                // Entfernt den Text der Methode, welche die aktuelle Position anzeigt
                ClearText(CursorXCoordinate, 22);
            }
            // Entfernt die Nutzerausgaben aus "BewegeChecker" Methode
            ClearText(CursorXCoordinate, 17);
            ClearText(CursorXCoordinate, 18);
            ClearText(CursorXCoordinate, 21);
            return new Position(xKoordinateAlsZahl, yKoordinateUmgewandelt, spieler.GameDirection);
        }

        public void ShowCurrentPosition(char xKoordinate, int yKoordinate)
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 22);
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Write(GlobalStrings.SelektiertePosition, xKoordinate, yKoordinate);
        }

        public void MoveChecker(Checker bewegenderSpielstein)
        {
            System.Console.SetCursorPosition(Feldbreite * bewegenderSpielstein.CurrentPosition.X + 6, Feldhöhe * bewegenderSpielstein.CurrentPosition.Y + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = bewegenderSpielstein.Color;
            if (bewegenderSpielstein.IsQueen)
            {
                DruckeDame();
            }
            else
            {
                DruckeSpielstein();
            }
        }

        public void RemoveChecker(Checker geschlagenerSpielstein)
        {
            System.Console.SetCursorPosition(Feldbreite * geschlagenerSpielstein.CurrentPosition.X + 6, Feldhöhe * geschlagenerSpielstein.CurrentPosition.Y + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = geschlagenerSpielstein.Color;
            System.Console.Write(" ");
        }

        private static void DruckeSpielstein()
        {
            System.Console.Write(GlobalStrings.Spielstein);
        }

        private static void DruckeDame()
        {
            System.Console.Write(GlobalStrings.DameSpielstein);
        }

        public void TransformCheckerToQueen(Position positionSpielstein)
        {
            SetGameColors();
            var xKoordinateUmgewandelt = Convert.ToChar(positionSpielstein.X + 97);
            var yKoordinateUmgewandelt = (positionSpielstein.Y - CursorYCoordinate) * - 1;
            System.Console.SetCursorPosition(CursorXCoordinate, 21);
            System.Console.WriteLine(GlobalStrings.UmwandlungSpielsteinInDame, xKoordinateUmgewandelt, yKoordinateUmgewandelt);
        }

        public void TryMovingEnemyChecker()
        {
            SetGameColors();
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.SelektiereGegnerischenSpielstein);
        }

        public void MoveCheckerOnWhiteField()
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.PlatziereSpielsteinAufWeißesFeld);
        }

        public void TryMovingCheckerToFarAway()
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.UngültigeBewegung);
            System.Console.SetCursorPosition(CursorXCoordinate, 18);
            System.Console.WriteLine(GlobalStrings.ÜngültigeBewegungHinweis);
        }

        public void SelectEmptyField()
        {
            SetGameColors();
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.SelektiereLeeresFeld);
        }

        public void MoveCheckerOnOccupiedField()
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.FeldBereitsBesetzt);
        }

        public void MovingNotPossible()
        {
            SetGameColors();
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.SpielsteinIstNichtBewegbar1);
            System.Console.SetCursorPosition(CursorXCoordinate, 18);
            System.Console.WriteLine(GlobalStrings.SpielsteinIstNichtBewegbar2);
        }

        public void ShowPlayerTurn(string spielername, ConsoleColor spielsteinFarbe)
        {   
            SetGameColors();
            ClearText(CursorXCoordinate, 6);
            System.Console.SetCursorPosition(CursorXCoordinate, 6);
            System.Console.Write(GlobalStrings.SpielerIstAmSpielzug, spielername, spielsteinFarbe);
            System.Console.SetCursorPosition(CursorXCoordinate, 7);
            System.Console.WriteLine(GlobalStrings.SpielzugTrennzeichen);
        }

        public void ShowPossibleJump(char xKoordinate, int yKoordinate)
        {
            SetGameColors();
            ClearText(CursorXCoordinate, 18);
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.SprungWurdeGefunden);
            System.Console.SetCursorPosition(CursorXCoordinate, 18);
            System.Console.Write(GlobalStrings.SpielsteinHatSprung, xKoordinate, yKoordinate);
        }

        public void ShowTwoPossibleJumps(char xKoordinateRechts, int yKoordinateRechts, char xKoordinateLinks, int yKoordinateLinks)
        {
            SetGameColors();
            System.Console.SetCursorPosition(CursorXCoordinate, 17);
            System.Console.WriteLine(GlobalStrings.Sprung2SpielsteineWurdeGefunden);
            System.Console.SetCursorPosition(CursorXCoordinate, 18);
            System.Console.Write(GlobalStrings.ZweiSpielsteineHabenSprung, xKoordinateRechts, yKoordinateRechts, xKoordinateLinks, yKoordinateLinks);
        }

        public void ShowPossibleJumpCount(List<Checker> spielsteine)
        {
            SetGameColors();
            // Umgewandelte x und y Koordinaten des ERSTEN gefundenen Spielsteins, welcher über einen Sprung verfügt
            var xKoordinateUmgewandelt = Convert.ToChar(spielsteine[0].CurrentPosition.X + 97);
            var yKoordinateUmgewandelt = (spielsteine[0].CurrentPosition.Y - CursorYCoordinate) * -1;

            System.Console.SetCursorPosition(CursorXCoordinate, 21);
            System.Console.WriteLine(GlobalStrings.AnzahlSprüngeSpielstein, xKoordinateUmgewandelt, yKoordinateUmgewandelt, spielsteine[0].PossibleJumpCount);
            // Wenn der zweite Spielstein nicht null ist, bedeutet es, dass zwei verschiedene Spielsteine gleichzeitig springen können
            if (spielsteine.Count == 2)
            {
                // Umgewandelte x und y Koordinaten des ZWEITEN gefundenen Spielsteins, welcher über einen Sprung verfügt
                var xKoordinate2Umgewandelt = Convert.ToChar(spielsteine[1].CurrentPosition.X + 97);
                var yKoordinate2Umgewandelt = (spielsteine[1].CurrentPosition.Y - CursorYCoordinate) * -1;
                System.Console.SetCursorPosition(CursorXCoordinate, 22);
                System.Console.WriteLine(GlobalStrings.AnzahlSprüngeSpielstein, xKoordinate2Umgewandelt, yKoordinate2Umgewandelt, spielsteine[1].PossibleJumpCount);
            }
        }

        public void CheckerHasLessJumps(Checker spielstein)
        {
            ClearText(CursorXCoordinate, 21);
            ClearText(CursorXCoordinate, 22);
            SetGameColors();
            var xKoordinateUmgewandelt = Convert.ToChar(spielstein.CurrentPosition.X + 97);
            var yKoordinateUmgewandelt = (spielstein.CurrentPosition.Y - CursorYCoordinate) * -1;
            System.Console.SetCursorPosition(CursorXCoordinate, 21);
            System.Console.WriteLine(GlobalStrings.SpielsteinHatWenigerSprünge);
            System.Console.SetCursorPosition(CursorXCoordinate, 22);
            System.Console.WriteLine(GlobalStrings.HinweisSelektiereAnderenSpielstein, xKoordinateUmgewandelt, yKoordinateUmgewandelt);
        }

        public void ForceJump(List<Position> sprungPositionen)
        {
            SetGameColors();
            var xKoordinate1 = Convert.ToChar(sprungPositionen.ElementAt(0).X + 97);
            var yKoordinate1 = (sprungPositionen.ElementAt(0).Y - CursorYCoordinate) * -1;

            if (sprungPositionen.Count == 1)
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 18);
                System.Console.WriteLine(GlobalStrings.EineSprungPositionVorhanden, xKoordinate1, yKoordinate1);
            }
            else if (sprungPositionen.Count == 2)
            {
                var xKoordinate2 = Convert.ToChar(sprungPositionen.ElementAt(1).X + 97);
                var yKoordinate2 = (sprungPositionen.ElementAt(1).Y - CursorYCoordinate) * -1;
                System.Console.SetCursorPosition(CursorXCoordinate, 18);
                System.Console.WriteLine(GlobalStrings.ZweiSprungPositionenVorhanden, xKoordinate1, yKoordinate1, xKoordinate2, yKoordinate2);
            }
            else if (sprungPositionen.Count == 3)
            {
                var xKoordinate2 = Convert.ToChar(sprungPositionen.ElementAt(1).X + 97);
                var yKoordinate2 = (sprungPositionen.ElementAt(1).Y - CursorYCoordinate) * -1;
                var xKoordinate3 = Convert.ToChar(sprungPositionen.ElementAt(2).X + 97);
                var yKoordinate3 = (sprungPositionen.ElementAt(2).Y - CursorYCoordinate) * -1;
                System.Console.SetCursorPosition(CursorXCoordinate, 18);
                System.Console.WriteLine(GlobalStrings.DreiSprungPositionenVorhanden, xKoordinate1, yKoordinate1, xKoordinate2, yKoordinate2, xKoordinate3, yKoordinate3);
            }
            else if (sprungPositionen.Count == 4)
            {
                var xKoordinate2 = Convert.ToChar(sprungPositionen.ElementAt(1).X + 97);
                var yKoordinate2 = (sprungPositionen.ElementAt(1).Y - CursorYCoordinate) * -1;
                var xKoordinate3 = Convert.ToChar(sprungPositionen.ElementAt(2).X + 97);
                var yKoordinate3 = (sprungPositionen.ElementAt(2).Y - CursorYCoordinate) * -1;
                var xKoordinate4 = Convert.ToChar(sprungPositionen.ElementAt(3).X + 97);
                var yKoordinate4 = (sprungPositionen.ElementAt(3).Y - CursorYCoordinate) * -1;
                System.Console.SetCursorPosition(CursorXCoordinate, 18);
                System.Console.WriteLine(GlobalStrings.VierSprungPositionenVorhanden, xKoordinate1, yKoordinate1, xKoordinate2, yKoordinate2, xKoordinate3, yKoordinate3, xKoordinate4, yKoordinate4);
            }
        }

        public void BeatChecker()
        {
            SetGameColors();
            System.Console.SetCursorPosition(CursorXCoordinate, 18);
            System.Console.WriteLine(GlobalStrings.SchlageSpielstein);
        }

        public void ShowCurrentCheckerAmount(string spieler1Name, int anzahlSpielsteineSpieler1, string spieler2Name, int anzahlSpielsteineSpieler2)
        {
            var xPosition1 = 0;
            var xPosition2 = 0;
            if (Variant == Variant.European)
            {
                xPosition1 = 27;
                xPosition2 = 28;
            }
            if (Variant == Variant.American)
            {
                xPosition1 = 33;
                xPosition2 = 34;
            }
            SetGameColors();
            System.Console.SetCursorPosition(3, xPosition1);
            System.Console.WriteLine(GlobalStrings.HinweisAnzahlSpielsteine);
            System.Console.SetCursorPosition(3, xPosition2);
            System.Console.Write(GlobalStrings.AnzahlSpielsteineProSpieler, spieler1Name, anzahlSpielsteineSpieler1, spieler2Name, anzahlSpielsteineSpieler2);
        }

        public void ShowPassedTime(Stopwatch zeitMessung)
        {
            System.Console.SetCursorPosition(3, 26);
            System.Console.Write(GlobalStrings.VerstricheneZeit, zeitMessung.Elapsed.Minutes, zeitMessung.Elapsed.Seconds);
        }

        public void ClearText(int xPosition, int yPosition)
        {
            System.Console.SetCursorPosition(xPosition, yPosition);
            System.Console.Write(LeeresFeld);
        }

        public void SetGameColors()
        {
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
        }

        public void PlayerHasSurrendered(Player verlierer, Player gewinner)
        {
            SetGameColors();
            System.Console.SetCursorPosition(CursorXCoordinate, 22);
            System.Console.WriteLine(GlobalStrings.VerliererDerPartie, verlierer.Name.ToUpper());
            System.Console.SetCursorPosition(CursorXCoordinate, 24);
            System.Console.WriteLine(GlobalStrings.GewinnerDerPartie, gewinner.Name.ToUpper());
            System.Console.WriteLine();
        }
        public void ShowWinner(string gewinnerName)
        {
            SetGameColors();
            System.Console.SetCursorPosition(CursorXCoordinate, 22);
            System.Console.WriteLine(GlobalStrings.GewinnerDerPartie, gewinnerName.ToUpper());
        }
        public void QuitGame()
        {
            System.Console.SetCursorPosition(60, 24);
            System.Console.WriteLine(GlobalStrings.SpielBeenden);
            System.Console.SetCursorPosition(60, 25);
            System.Console.WriteLine(GlobalStrings.VerabschiedeNutzer);
        }
    }
}
