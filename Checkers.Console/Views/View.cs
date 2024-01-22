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
        private readonly GameBoard board;
        public int CursorXCoordinate => board.Variant == Variant.American ? 76 : 62;
        public int CursorYCoordinate => board.Variant == Variant.American ? 10: 8;

        private const int AsciiNumberLetterA = 97;
        private const string EmptyField = "                                                                          ";

        public View(GameBoard gameBoard)
        {
            board = gameBoard;
        }

        public void SetUp()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
            System.Console.Clear();
        }

        public void ShowMainMenu()
        {
            System.Console.SetCursorPosition(55, 1);
            System.Console.WriteLine(GlobalStrings.Willkommenstext);
            System.Console.SetCursorPosition(40, 2);
            System.Console.WriteLine(GlobalStrings.Trennzeichen);

            System.Console.WriteLine("[1] Start Game");
            System.Console.WriteLine("[2] Settings");
            System.Console.WriteLine("[3] Help");
            System.Console.WriteLine("[4] About");
            System.Console.WriteLine("[5] Quit");
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
            var languageId = Convert.ToInt32(System.Console.ReadLine());
            switch (languageId)
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

        public Variant GetSelectedGameVariant()
        {
            // Removes the language selection outputs
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
            SelectGameVariant();
            // Remove "Welcome Text"
            ClearText(55, 1);
            ClearText(40, 2);
            // Clear Console output from this method
            ClearText(37, 4);
            ClearText(46, 5);
            ClearText(46, 7);
            return board.Variant;
        }

        private void SelectGameVariant()
        {
            var gameVariant = System.Console.ReadLine();
            while (string.IsNullOrWhiteSpace(gameVariant) || gameVariant != "1" && gameVariant != "2")
            {
                System.Console.SetCursorPosition(46, 7);
                System.Console.Write(GlobalStrings.FehlerKeineGültigeZahl);
                gameVariant = System.Console.ReadLine();
            }

            if (gameVariant == "1")
            {
                board.SetGameVariant(Variant.American);
            }
            if (gameVariant == "2")
            {
                board.SetGameVariant(Variant.European);
            }
        }

        public string GetPlayerName(int playerNumber)
        {
            var validInput = false;
            var playerName = string.Empty;
            while (validInput == false)
            {
                // Remove console output for previous player name
                ClearText(CursorXCoordinate, 4);
                System.Console.SetCursorPosition(CursorXCoordinate, 4);
                System.Console.Write(GlobalStrings.EingabeSpielername, playerNumber);
                playerName = System.Console.ReadLine();
                validInput = true;
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    System.Console.SetCursorPosition(CursorXCoordinate, 10);
                    System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
                    validInput = false;
                }
            }

            return playerName;
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

        public ConsoleColor GetSelectedColor(string playerName, int playerNumber)
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 4);
            System.Console.WriteLine(GlobalStrings.InfoText1FarbAuswahl);
            System.Console.SetCursorPosition(CursorXCoordinate, 5);
            System.Console.WriteLine(GlobalStrings.InfoText2FarbAuswahl);
            var checkerColor = ConsoleColor.White;
            ClearText(CursorXCoordinate, 7);
            System.Console.SetCursorPosition(CursorXCoordinate, 7);
            System.Console.Write(GlobalStrings.AuswahlSpielsteinFarbe, playerNumber, playerName);
            var selectedColorText = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(selectedColorText))
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 9);
                System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
            }

            if (selectedColorText.ToLower() == GlobalStrings.Rot)
            {
                checkerColor = ConsoleColor.Red;
            }
            else if (selectedColorText.ToLower() == GlobalStrings.Blau)
            {
                checkerColor = ConsoleColor.Blue;
            }
            else if (selectedColorText.ToLower() == GlobalStrings.Grün)
            {
                checkerColor = ConsoleColor.Green;
            }
            else if (selectedColorText.ToLower() == GlobalStrings.Gelb)
            {
                checkerColor = ConsoleColor.Yellow;
            }
            else
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 9);
                System.Console.WriteLine(GlobalStrings.FehlerUngültigeFarbe);
            }
            switch (playerNumber)
            {
                case 1:
                    System.Console.SetCursorPosition(CursorXCoordinate, 12);
                    System.Console.WriteLine(GlobalStrings.FarbenAuswahlÜbersicht, playerNumber, playerName, checkerColor);
                    break;
                case 2:
                    System.Console.SetCursorPosition(CursorXCoordinate, 13);
                    System.Console.WriteLine(GlobalStrings.FarbenAuswahlÜbersicht, playerNumber, playerName, checkerColor);
                    break;
            }
            ClearText(CursorXCoordinate, 7);
            ClearText(CursorXCoordinate, 9);
            return checkerColor;
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
            for (var row = 0; row < board.RowCount; row++)
            {
                for (var height = 0; height < GameBoard.FieldHeight; height++)
                {
                    if (height == GameBoard.FieldHeight / 2)
                    {
                        PrintRowValueNumbers(row);
                    }
                    else
                    {
                        PrintWhiteSpacesBetweenRowValues(height, row);
                    }
                    CreateRows(row);
                    System.Console.WriteLine();
                }
            }
            CreateColumns();
        }

        private static void PrintWhiteSpacesBetweenRowValues(int height, int row)
        {
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            if (height == 2)
            {
                if (row == 0)
                {
                    System.Console.SetCursorPosition(0, 3);
                }
                else
                {
                    System.Console.SetCursorPosition(0, 3 + row * 3);
                }
            }
            else
            {
                System.Console.SetCursorPosition(0, 1 + row * GameBoard.FieldHeight);
            }
            System.Console.Write("   ");
        }

        public void PrintRowValueNumbers(int columnNumber)
        {
            System.Console.BackgroundColor = ConsoleColor.DarkYellow;
            System.Console.ForegroundColor = ConsoleColor.Black;
            if (board.Variant == Variant.American && columnNumber == 0)
            {
                // Value 10 is shown in the center
                System.Console.Write(board.RowCount + " ");
            }
            else
            {
                // Values 1-9 are shown in the center
                System.Console.Write((board.RowCount - columnNumber) + "  ");
            }
        }

        public void CreateRows(int reihe)
        {
            for (var spalte = 0; spalte < board.ColumnCount; spalte++)
            {
                for (var b = 0; b < GameBoard.FieldWidth; b++)
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

        public void CreateColumns()
        {
            for (var i = 0; i < board.ColumnCount; i++)
            {
                var aAscii = AsciiNumberLetterA + i;
                System.Console.SetCursorPosition(i * GameBoard.FieldWidth + GameBoard.FieldWidth / 2 + 3, board.RowCount * GameBoard.FieldHeight + 1);
                System.Console.BackgroundColor = ConsoleColor.DarkYellow;
                System.Console.Write((char) aAscii);
            }
        }

        public void PrintChecker(Checker checker)
        {
            System.Console.SetCursorPosition(GameBoard.FieldWidth * checker.CurrentPosition.X + 6, GameBoard.FieldHeight * checker.CurrentPosition.Y + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = checker.Color;
            System.Console.Write(GlobalStrings.Spielstein);
            SetGameColors();
        }

        public void ShowSurrenderOption()
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 2);
            System.Console.Write(GlobalStrings.InformationZumAufgeben1);
            System.Console.SetCursorPosition(CursorXCoordinate, 3);
            System.Console.Write(GlobalStrings.InformationZumAufgeben2);
        }

        public Position? GetPosition(bool isStartCoordinate, Player player)
        {
            SetGameColors();
            ClearText(CursorXCoordinate, 10);
            ClearText(CursorXCoordinate, 11);
            ClearText(CursorXCoordinate, 13);
            ClearText(CursorXCoordinate, 14);
            var enteredCoordinates = GetEnteredCoordinates(isStartCoordinate);
            var yCoordinate = (enteredCoordinates.x - CursorYCoordinate) * -1;
            if (!isStartCoordinate)
            {
                ClearText(CursorXCoordinate, 22);
            }
            ClearText(CursorXCoordinate, 17);
            ClearText(CursorXCoordinate, 18);
            ClearText(CursorXCoordinate, 21);
            return new Position(enteredCoordinates.y, yCoordinate, player.GameDirection);
        }

        private (int x, int y) GetEnteredCoordinates(bool isStartCoordinate)
        {
            int xCoordinate = 0, yCoordinate = 0;
            var valid = false;
            char firstLetter = '\0';
            while (valid == false)
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 10);
                System.Console.Write(GlobalStrings.EingabeXKoordinate);
                var enteredLetter = System.Console.ReadLine();
                if (string.IsNullOrWhiteSpace(enteredLetter))
                {
                    System.Console.SetCursorPosition(CursorXCoordinate, 11);
                    System.Console.WriteLine(GlobalStrings.FehlerLeereEingabe);
                    continue;
                }

                firstLetter = enteredLetter.ToLower().ToCharArray().First();
                var ascii = (int) firstLetter;
                if (ascii >= 97  && ascii < 97 + board.ColumnCount)
                {
                    xCoordinate = ascii - 97;
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
            while (!int.TryParse(System.Console.ReadLine(), out yCoordinate))
            {
                System.Console.SetCursorPosition(CursorXCoordinate, 14);
                System.Console.WriteLine(GlobalStrings.FehlerKoordinateIstKeineZahl);
                System.Console.SetCursorPosition(CursorXCoordinate, 13);
                System.Console.Write(GlobalStrings.EingabeYKoordinate);
            }
            
            if (isStartCoordinate)
            {
                ShowCurrentPosition(firstLetter, yCoordinate);
            }
            return (xCoordinate, yCoordinate);
        }

        public void ShowCurrentPosition(char x, int y)
        {
            System.Console.SetCursorPosition(CursorXCoordinate, 22);
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Write(GlobalStrings.SelektiertePosition, x, y);
        }

        public void MoveChecker(Checker checkerToMove)
        {
            System.Console.SetCursorPosition(GameBoard.FieldWidth * checkerToMove.CurrentPosition.X + 6, GameBoard.FieldHeight * checkerToMove.CurrentPosition.Y + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = checkerToMove.Color;
            if (checkerToMove.IsQueen)
            {
                PrintQueen();
            }
            else
            {
                PrintChecker();
            }
        }

        public void RemoveChecker(Checker geschlagenerSpielstein)
        {
            System.Console.SetCursorPosition(GameBoard.FieldWidth * geschlagenerSpielstein.CurrentPosition.X + 6, GameBoard.FieldHeight * geschlagenerSpielstein.CurrentPosition.Y + 2);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = geschlagenerSpielstein.Color;
            System.Console.Write(" ");
        }

        private static void PrintChecker()
        {
            System.Console.Write(GlobalStrings.Spielstein);
        }

        private static void PrintQueen()
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
            System.Console.WriteLine(GlobalStrings.AnzahlSprüngeSpielstein, xKoordinateUmgewandelt, yKoordinateUmgewandelt, spielsteine[0].JumpCount);
            // Wenn der zweite Spielstein nicht null ist, bedeutet es, dass zwei verschiedene Spielsteine gleichzeitig springen können
            if (spielsteine.Count == 2)
            {
                // Umgewandelte x und y Koordinaten des ZWEITEN gefundenen Spielsteins, welcher über einen Sprung verfügt
                var xKoordinate2Umgewandelt = Convert.ToChar(spielsteine[1].CurrentPosition.X + 97);
                var yKoordinate2Umgewandelt = (spielsteine[1].CurrentPosition.Y - CursorYCoordinate) * -1;
                System.Console.SetCursorPosition(CursorXCoordinate, 22);
                System.Console.WriteLine(GlobalStrings.AnzahlSprüngeSpielstein, xKoordinate2Umgewandelt, yKoordinate2Umgewandelt, spielsteine[1].JumpCount);
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
            if (board.Variant == Variant.European)
            {
                xPosition1 = 27;
                xPosition2 = 28;
            }
            if (board.Variant == Variant.American)
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
            System.Console.Write(EmptyField);
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
