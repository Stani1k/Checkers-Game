using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Checkers.Console.Interfaces;
using Checkers.Console.Resources;

namespace Checkers.Console
{
    public class View : IView
    {
        public int AnzahlSpalten => Variant == Variant.Amerikanisch ? 10 : 8;
        public int AnzahlZeilen => Variant == Variant.Amerikanisch ? 10 : 8;
        public int CursorXPosition => Variant == Variant.Amerikanisch ? 76 : 62;
        public int UmwandlungswertYKoordinate => Variant == Variant.Amerikanisch ? 10: 8;
        public Variant Variant { get; set; }

        private const int Feldbreite = 7;
        private const int Feldhöhe = 3;
        // ASCII Nummer für 'a'
        private const int AsciiNummerA = 97;
        private const string LeeresFeld = "                                                                          ";

        public void KonfiguriereKonsolenfenster()
        {
            // ToDo: Refactor this to not use absolute values
            // Console.BufferWidth = 150;
            // Console.SetWindowSize(Console.BufferWidth, 40);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
            System.Console.Clear();
        }

        public void GrüßeNutzer()
        {
            System.Console.SetCursorPosition(55, 1);
            System.Console.WriteLine(GlobalStrings.Willkommenstext);
            System.Console.SetCursorPosition(40, 2);
            System.Console.WriteLine(GlobalStrings.Trennzeichen);
        }

        public int Sprachauswahl()
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

        public Variant EingabeSpielvariante()
        {
            // Entfernt die Ausgaben, für die Sprachauswahl
            EntferneText(56, 4);
            EntferneText(58, 5);
            EntferneText(58, 6);
            EntferneText(58, 7);
            EntferneText(58, 8);
            EntferneText(37, 10);
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
                Variant = Variant.Amerikanisch;
            }
            if (spielversion == "2")
            {
                Variant = Variant.Europäisch;
            }
            // Entfernt den "Willkommenstext"
            EntferneText(55, 1);
            EntferneText(40, 2);
            // Entfernt die Konsolenausgaben aus dieser Methode
            EntferneText(37, 4);
            EntferneText(46, 5);
            EntferneText(46, 7);
            return Variant;
        }

        public string EingabeSpielername(int spielerNummer)
        {
            var gültigeEingabe = false;
            var eingabeSpielerName = "";
            while (!gültigeEingabe)
            {
                // Entfernt den eingegebenen Namen des ersten Spielers
                EntferneText(CursorXPosition, 4);
                System.Console.SetCursorPosition(CursorXPosition, 4);
                System.Console.Write(GlobalStrings.EingabeSpielername, spielerNummer);
                eingabeSpielerName = System.Console.ReadLine();
                gültigeEingabe = true;
                if (string.IsNullOrWhiteSpace(eingabeSpielerName))
                {
                    System.Console.SetCursorPosition(CursorXPosition, 10);
                    System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
                    gültigeEingabe = false;
                }
            }
            return eingabeSpielerName;
        }

        public string ErmittleSeiten(string spielername)
        {
            // Entfernt den Fehlertext "NutzerSelektiertZweiGleicheFarben"
            EntferneText(CursorXPosition, 10);
            // Der Nutzer kann entscheiden auf welcher Seite er beginnen möchte (oben oder unten)
            System.Console.SetCursorPosition(CursorXPosition, 8);
            System.Console.WriteLine(GlobalStrings.AuswahlSpielseite, spielername);
            System.Console.SetCursorPosition(CursorXPosition, 9);
            System.Console.Write(GlobalStrings.ObenOderUnten);

            var eingabeSeitenwahl = System.Console.ReadLine();
            // Solange die Nutzereingabe nicht dem Wert 1 ODER 2 entspricht ODER leer ist, muss der Nutzer einen neuen Wert eingeben
            while (eingabeSeitenwahl != "1" && eingabeSeitenwahl != "2" || string.IsNullOrWhiteSpace(eingabeSeitenwahl))
            {
                System.Console.SetCursorPosition(CursorXPosition, 11);
                System.Console.WriteLine(GlobalStrings.FehlerKeineGültigeZahl);
                EntferneText(CursorXPosition, 9);
                System.Console.SetCursorPosition(CursorXPosition, 9);
                System.Console.Write(GlobalStrings.ObenOderUnten);
                eingabeSeitenwahl = System.Console.ReadLine();
            }
            // Entfernt die Texte aus der Methode "FarbeDerSpielsteineErmitteln"
            EntferneText(CursorXPosition, 4);
            EntferneText(CursorXPosition, 5);
            // Entfernt die Texte/Meldungen aus dieser Methode
            EntferneText(CursorXPosition, 8);
            EntferneText(CursorXPosition, 9);
            EntferneText(CursorXPosition, 11);
            // Entfernt den Text der die ausgewählten Farben dem Nutzer anzeigt
            EntferneText(CursorXPosition, 12);
            EntferneText(CursorXPosition, 13);

            return eingabeSeitenwahl;
        }

        public ConsoleColor FarbeDerSpielsteineErmitteln(string spielername, int spielerNummer)
        {
            System.Console.SetCursorPosition(CursorXPosition, 4);
            System.Console.WriteLine(GlobalStrings.InfoText1FarbAuswahl);
            System.Console.SetCursorPosition(CursorXPosition, 5);
            System.Console.WriteLine(GlobalStrings.InfoText2FarbAuswahl);
            var valid = false;
            var spielsteinFarbe = ConsoleColor.White;
            while (!valid)
            {
                EntferneText(CursorXPosition, 7);
                System.Console.SetCursorPosition(CursorXPosition, 7);
                System.Console.Write(GlobalStrings.AuswahlSpielsteinFarbe, spielerNummer, spielername);
                var spielerFarbenAuswahl = System.Console.ReadLine();

                if (string.IsNullOrWhiteSpace(spielerFarbenAuswahl))
                {
                    System.Console.SetCursorPosition(CursorXPosition, 9);
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
                    System.Console.SetCursorPosition(CursorXPosition, 9);
                    System.Console.WriteLine(GlobalStrings.FehlerUngültigeFarbe);
                }
            }
            if (spielerNummer == 1)
            {
                System.Console.SetCursorPosition(CursorXPosition, 12);
                System.Console.WriteLine(GlobalStrings.FarbenAuswahlÜbersicht, spielerNummer, spielername, spielsteinFarbe);
            }
            else if (spielerNummer == 2)
            {
                System.Console.SetCursorPosition(CursorXPosition, 13);
                System.Console.WriteLine(GlobalStrings.FarbenAuswahlÜbersicht, spielerNummer, spielername, spielsteinFarbe);
            }
            EntferneText(CursorXPosition, 7);
            EntferneText(CursorXPosition, 9);
            return spielsteinFarbe;
        }

        public void NutzerSelektierenZweiGleicheFarben()
        {
            System.Console.SetCursorPosition(CursorXPosition, 10);
            System.Console.WriteLine(GlobalStrings.SelektiereZweiGleicheFarben);
        }

        public void NutzerWählenZweiGleicheNamen()
        {
            System.Console.SetCursorPosition(CursorXPosition, 10);
            System.Console.WriteLine(GlobalStrings.SelektiereZweiGleicheNamen);
        }

        public void ErstelleSpielbrett()
        {
            for (var zeile = 0; zeile < AnzahlZeilen; zeile++)
            {
                for (var höhe = 0; höhe < Feldhöhe; höhe++)
                {
                    if (höhe == Feldhöhe / 2)
                    {
                        ErstelleYAchse(zeile);
                    }
                    else
                    {
                        ErstelleLeerzeichenAufYAchse(höhe, zeile);
                    }
                    ErstelleSpalten(zeile);
                    // Beginnt eine neue Zeile
                    System.Console.WriteLine();
                }
            }
            ErstelleXAchse();
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

        public void ErstelleYAchse(int zeile)
        {
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
            if (Variant == Variant.Amerikanisch && zeile == 0)
            {
                // der Wert 10 der y-Achse, wird hier mittig ausgegeben
                System.Console.Write((AnzahlZeilen - zeile) + " ");
            }
            else
            {
                // die Werte 1-9 der y-Achse werden hier mittig ausgegeben
                System.Console.Write((AnzahlZeilen - zeile) + "  ");
            }
        }

        public void ErstelleSpalten(int reihe)
        {
            // Erstellt einzelne Spalten in horizontaler Richtung von links nach rechts
            for (var spalte = 0; spalte < AnzahlSpalten; spalte++)
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

        public void ErstelleXAchse()
        {
            for (var i = 0; i < AnzahlSpalten; i++)
            {
                var asciiAlsBuchstabe = AsciiNummerA + i;
                // Berechnung der Buchstaben auf der x-Achse um die Buchstaben mittig auszugeben
                System.Console.SetCursorPosition(i * Feldbreite + Feldbreite / 2 + 3, AnzahlZeilen * Feldhöhe + 1);
                System.Console.BackgroundColor = ConsoleColor.DarkYellow;
                System.Console.Write((char) asciiAlsBuchstabe);
            }
        }

        public void DruckeSpielstein(Checker checker)
        {
            System.Console.SetCursorPosition(Feldbreite * checker.Position.XKoordinate + 6, Feldhöhe * checker.Position.YKoordinate + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = checker.Farbe;
            System.Console.Write(GlobalStrings.Spielstein);
            // Nachdem der letzte Spielstein platziert wurde, werden die Farben der Konsole wieder auf "Default" gesetzt
            SetzeKonsolenFarben();
        }

        public void SpielinformationenZumAufgeben()
        {
            System.Console.SetCursorPosition(CursorXPosition, 2);
            System.Console.Write(GlobalStrings.InformationZumAufgeben1);
            System.Console.SetCursorPosition(CursorXPosition, 3);
            System.Console.Write(GlobalStrings.InformationZumAufgeben2);
        }

        public Position EingabeKoordinaten(bool istStartKoordinate, Player spieler)
        {
            SetzeKonsolenFarben();
            // Entfernt die eingegebenen Werte für die nächste Eingabe
            EntferneText(CursorXPosition, 10);
            EntferneText(CursorXPosition, 11);
            EntferneText(CursorXPosition, 13);
            EntferneText(CursorXPosition, 14);
            var xKoordinateAlsZahl = 0;
            var ersterBuchstabe = '\0';
            var valid = false;
            while (!valid)
            {
                System.Console.SetCursorPosition(CursorXPosition, 10);
                System.Console.Write(GlobalStrings.EingabeXKoordinate);
                var buchstabe = System.Console.ReadLine();
                // Prüft ob die Eingabe des Nutzers leer ist
                if (string.IsNullOrWhiteSpace(buchstabe))
                {
                    System.Console.SetCursorPosition(CursorXPosition, 11);
                    System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
                    continue;
                }
                if (buchstabe == "!gg")
                {
                    return null;
                }
                ersterBuchstabe = buchstabe.ToLower().ToCharArray().First();
                var ascii = (int) ersterBuchstabe;
                if (ascii >= 97  && ascii < 97 + AnzahlSpalten)
                {
                    xKoordinateAlsZahl = ascii - 97;
                    valid = true;
                }
                else
                {
                    System.Console.SetCursorPosition(CursorXPosition, 11);
                    System.Console.WriteLine(GlobalStrings.FehlerUngültigerKoordinatenwert);
                }
            }
            System.Console.SetCursorPosition(CursorXPosition, 13);
            System.Console.Write(GlobalStrings.EingabeYKoordinate);
            int yKoordinate;
            while (!int.TryParse(System.Console.ReadLine(), out yKoordinate))
            {
                System.Console.SetCursorPosition(CursorXPosition, 14);
                System.Console.WriteLine(GlobalStrings.FehlerKoordinateIstKeineZahl);
                System.Console.SetCursorPosition(CursorXPosition, 13);
                System.Console.Write(GlobalStrings.EingabeYKoordinate);
            }
            var yKoordinateUmgewandelt = (yKoordinate - UmwandlungswertYKoordinate) * -1;
            if (istStartKoordinate)
            {
                AktuellePositionAnzeigen(ersterBuchstabe, yKoordinate);
            }

            if (!istStartKoordinate)
            {
                // Entfernt den Text der Methode, welche die aktuelle Position anzeigt
                EntferneText(CursorXPosition, 22);
            }
            // Entfernt die Nutzerausgaben aus "BewegeChecker" Methode
            EntferneText(CursorXPosition, 17);
            EntferneText(CursorXPosition, 18);
            EntferneText(CursorXPosition, 21);
            return new Position(xKoordinateAlsZahl, yKoordinateUmgewandelt, spieler.SpielRichtung);
        }

        public void AktuellePositionAnzeigen(char xKoordinate, int yKoordinate)
        {
            System.Console.SetCursorPosition(CursorXPosition, 22);
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Write(GlobalStrings.SelektiertePosition, xKoordinate, yKoordinate);
        }

        public void PlatziereSpielstein(Checker bewegenderSpielstein)
        {
            System.Console.SetCursorPosition(Feldbreite * bewegenderSpielstein.Position.XKoordinate + 6, Feldhöhe * bewegenderSpielstein.Position.YKoordinate + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = bewegenderSpielstein.Farbe;
            if (bewegenderSpielstein.IstEineDame)
            {
                DruckeDame();
            }
            else
            {
                DruckeSpielstein();
            }
        }

        public void EntferneSpielstein(Checker geschlagenerSpielstein)
        {
            System.Console.SetCursorPosition(Feldbreite * geschlagenerSpielstein.Position.XKoordinate + 6, Feldhöhe * geschlagenerSpielstein.Position.YKoordinate + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = geschlagenerSpielstein.Farbe;
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

        public void SpielsteinWurdeInDameUmgewandelt(Position positionSpielstein)
        {
            SetzeKonsolenFarben();
            var xKoordinateUmgewandelt = Convert.ToChar(positionSpielstein.XKoordinate + 97);
            var yKoordinateUmgewandelt = (positionSpielstein.YKoordinate - UmwandlungswertYKoordinate) * - 1;
            System.Console.SetCursorPosition(CursorXPosition, 21);
            System.Console.WriteLine(GlobalStrings.UmwandlungSpielsteinInDame, xKoordinateUmgewandelt, yKoordinateUmgewandelt);
        }

        public void BewegeGegnerischenSpielstein()
        {
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.SelektiereGegnerischenSpielstein);
        }

        public void BewegeSpielsteinAufWeißesFeld()
        {
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.PlatziereSpielsteinAufWeißesFeld);
        }

        public void BewegeSpielsteinWeiterAlsErlaubt()
        {
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.UngültigeBewegung);
            System.Console.SetCursorPosition(CursorXPosition, 18);
            System.Console.WriteLine(GlobalStrings.ÜngültigeBewegungHinweis);
        }

        public void SelektiereLeeresFeld()
        {
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.SelektiereLeeresFeld);
        }

        public void BewegeCheckerAufAnderenChecker()
        {
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.FeldBereitsBesetzt);
        }

        public void BewegeSpielsteinNichtMöglich()
        {
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.SpielsteinIstNichtBewegbar1);
            System.Console.SetCursorPosition(CursorXPosition, 18);
            System.Console.WriteLine(GlobalStrings.SpielsteinIstNichtBewegbar2);
        }

        public void ZeigeSpielzugAn(string spielername, ConsoleColor spielsteinFarbe)
        {   
            SetzeKonsolenFarben();
            EntferneText(CursorXPosition, 6);
            System.Console.SetCursorPosition(CursorXPosition, 6);
            System.Console.Write(GlobalStrings.SpielerIstAmSpielzug, spielername, spielsteinFarbe);
            System.Console.SetCursorPosition(CursorXPosition, 7);
            System.Console.WriteLine(GlobalStrings.SpielzugTrennzeichen);
        }

        public void ZeigeEinenSprungAn(char xKoordinate, int yKoordinate)
        {
            SetzeKonsolenFarben();
            EntferneText(CursorXPosition, 18);
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.SprungWurdeGefunden);
            System.Console.SetCursorPosition(CursorXPosition, 18);
            System.Console.Write(GlobalStrings.SpielsteinHatSprung, xKoordinate, yKoordinate);
        }

        public void ZeigeZweiMöglicheSprüngeAn(char xKoordinateRechts, int yKoordinateRechts, char xKoordinateLinks, int yKoordinateLinks)
        {
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(CursorXPosition, 17);
            System.Console.WriteLine(GlobalStrings.Sprung2SpielsteineWurdeGefunden);
            System.Console.SetCursorPosition(CursorXPosition, 18);
            System.Console.Write(GlobalStrings.ZweiSpielsteineHabenSprung, xKoordinateRechts, yKoordinateRechts, xKoordinateLinks, yKoordinateLinks);
        }

        public void ZeigeAnzahlGefundenerSprünge(List<Checker> spielsteine)
        {
            SetzeKonsolenFarben();
            // Umgewandelte x und y Koordinaten des ERSTEN gefundenen Spielsteins, welcher über einen Sprung verfügt
            var xKoordinateUmgewandelt = Convert.ToChar(spielsteine[0].Position.XKoordinate + 97);
            var yKoordinateUmgewandelt = (spielsteine[0].Position.YKoordinate - UmwandlungswertYKoordinate) * -1;

            System.Console.SetCursorPosition(CursorXPosition, 21);
            System.Console.WriteLine(GlobalStrings.AnzahlSprüngeSpielstein, xKoordinateUmgewandelt, yKoordinateUmgewandelt, spielsteine[0].AnzahlSprünge);
            // Wenn der zweite Spielstein nicht null ist, bedeutet es, dass zwei verschiedene Spielsteine gleichzeitig springen können
            if (spielsteine.Count == 2)
            {
                // Umgewandelte x und y Koordinaten des ZWEITEN gefundenen Spielsteins, welcher über einen Sprung verfügt
                var xKoordinate2Umgewandelt = Convert.ToChar(spielsteine[1].Position.XKoordinate + 97);
                var yKoordinate2Umgewandelt = (spielsteine[1].Position.YKoordinate - UmwandlungswertYKoordinate) * -1;
                System.Console.SetCursorPosition(CursorXPosition, 22);
                System.Console.WriteLine(GlobalStrings.AnzahlSprüngeSpielstein, xKoordinate2Umgewandelt, yKoordinate2Umgewandelt, spielsteine[1].AnzahlSprünge);
            }
        }

        public void SpielsteinHatWenigerSprünge(Checker spielstein)
        {
            EntferneText(CursorXPosition, 21);
            EntferneText(CursorXPosition, 22);
            SetzeKonsolenFarben();
            var xKoordinateUmgewandelt = Convert.ToChar(spielstein.Position.XKoordinate + 97);
            var yKoordinateUmgewandelt = (spielstein.Position.YKoordinate - UmwandlungswertYKoordinate) * -1;
            System.Console.SetCursorPosition(CursorXPosition, 21);
            System.Console.WriteLine(GlobalStrings.SpielsteinHatWenigerSprünge);
            System.Console.SetCursorPosition(CursorXPosition, 22);
            System.Console.WriteLine(GlobalStrings.HinweisSelektiereAnderenSpielstein, xKoordinateUmgewandelt, yKoordinateUmgewandelt);
        }

        public void ErzwingeSprung(List<Position> sprungPositionen)
        {
            SetzeKonsolenFarben();
            var xKoordinate1 = Convert.ToChar(sprungPositionen.ElementAt(0).XKoordinate + 97);
            var yKoordinate1 = (sprungPositionen.ElementAt(0).YKoordinate - UmwandlungswertYKoordinate) * -1;

            if (sprungPositionen.Count == 1)
            {
                System.Console.SetCursorPosition(CursorXPosition, 18);
                System.Console.WriteLine(GlobalStrings.EineSprungPositionVorhanden, xKoordinate1, yKoordinate1);
            }
            else if (sprungPositionen.Count == 2)
            {
                var xKoordinate2 = Convert.ToChar(sprungPositionen.ElementAt(1).XKoordinate + 97);
                var yKoordinate2 = (sprungPositionen.ElementAt(1).YKoordinate - UmwandlungswertYKoordinate) * -1;
                System.Console.SetCursorPosition(CursorXPosition, 18);
                System.Console.WriteLine(GlobalStrings.ZweiSprungPositionenVorhanden, xKoordinate1, yKoordinate1, xKoordinate2, yKoordinate2);
            }
            else if (sprungPositionen.Count == 3)
            {
                var xKoordinate2 = Convert.ToChar(sprungPositionen.ElementAt(1).XKoordinate + 97);
                var yKoordinate2 = (sprungPositionen.ElementAt(1).YKoordinate - UmwandlungswertYKoordinate) * -1;
                var xKoordinate3 = Convert.ToChar(sprungPositionen.ElementAt(2).XKoordinate + 97);
                var yKoordinate3 = (sprungPositionen.ElementAt(2).YKoordinate - UmwandlungswertYKoordinate) * -1;
                System.Console.SetCursorPosition(CursorXPosition, 18);
                System.Console.WriteLine(GlobalStrings.DreiSprungPositionenVorhanden, xKoordinate1, yKoordinate1, xKoordinate2, yKoordinate2, xKoordinate3, yKoordinate3);
            }
            else if (sprungPositionen.Count == 4)
            {
                var xKoordinate2 = Convert.ToChar(sprungPositionen.ElementAt(1).XKoordinate + 97);
                var yKoordinate2 = (sprungPositionen.ElementAt(1).YKoordinate - UmwandlungswertYKoordinate) * -1;
                var xKoordinate3 = Convert.ToChar(sprungPositionen.ElementAt(2).XKoordinate + 97);
                var yKoordinate3 = (sprungPositionen.ElementAt(2).YKoordinate - UmwandlungswertYKoordinate) * -1;
                var xKoordinate4 = Convert.ToChar(sprungPositionen.ElementAt(3).XKoordinate + 97);
                var yKoordinate4 = (sprungPositionen.ElementAt(3).YKoordinate - UmwandlungswertYKoordinate) * -1;
                System.Console.SetCursorPosition(CursorXPosition, 18);
                System.Console.WriteLine(GlobalStrings.VierSprungPositionenVorhanden, xKoordinate1, yKoordinate1, xKoordinate2, yKoordinate2, xKoordinate3, yKoordinate3, xKoordinate4, yKoordinate4);
            }
        }

        public void SchlageGegnerischenSpielstein()
        {
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(CursorXPosition, 18);
            System.Console.WriteLine(GlobalStrings.SchlageSpielstein);
        }

        public void ZeigeAktuelleAnzahlDerSpielsteine(string spieler1Name, int anzahlSpielsteineSpieler1, string spieler2Name, int anzahlSpielsteineSpieler2)
        {
            var xPosition1 = 0;
            var xPosition2 = 0;
            if (Variant == Variant.Europäisch)
            {
                xPosition1 = 27;
                xPosition2 = 28;
            }
            if (Variant == Variant.Amerikanisch)
            {
                xPosition1 = 33;
                xPosition2 = 34;
            }
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(3, xPosition1);
            System.Console.WriteLine(GlobalStrings.HinweisAnzahlSpielsteine);
            System.Console.SetCursorPosition(3, xPosition2);
            System.Console.Write(GlobalStrings.AnzahlSpielsteineProSpieler, spieler1Name, anzahlSpielsteineSpieler1, spieler2Name, anzahlSpielsteineSpieler2);
        }

        public void ZeigeVerstricheneZeitAn(Stopwatch zeitMessung)
        {
            System.Console.SetCursorPosition(3, 26);
            System.Console.Write(GlobalStrings.VerstricheneZeit, zeitMessung.Elapsed.Minutes, zeitMessung.Elapsed.Seconds);
        }

        public void EntferneText(int xPosition, int yPosition)
        {
            System.Console.SetCursorPosition(xPosition, yPosition);
            System.Console.Write(LeeresFeld);
        }

        public void SetzeKonsolenFarben()
        {
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
        }

        public void SpielerHatAufgegeben(Player verlierer, Player gewinner)
        {
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(CursorXPosition, 22);
            System.Console.WriteLine(GlobalStrings.VerliererDerPartie, verlierer.Name.ToUpper());
            System.Console.SetCursorPosition(CursorXPosition, 24);
            System.Console.WriteLine(GlobalStrings.GewinnerDerPartie, gewinner.Name.ToUpper());
            System.Console.WriteLine();
        }
        public void ZeigeGewinnerAn(string gewinnerName)
        {
            SetzeKonsolenFarben();
            System.Console.SetCursorPosition(CursorXPosition, 22);
            System.Console.WriteLine(GlobalStrings.GewinnerDerPartie, gewinnerName.ToUpper());
        }
        public void BeendeSpiel()
        {
            System.Console.SetCursorPosition(60, 24);
            System.Console.WriteLine(GlobalStrings.SpielBeenden);
            System.Console.SetCursorPosition(60, 25);
            System.Console.WriteLine(GlobalStrings.VerabschiedeNutzer);
        }
    }
}
