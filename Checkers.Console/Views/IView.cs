using System;
using System.Collections.Generic;
using System.Diagnostics;
using Checkers.Console.Models;

namespace Checkers.Console.Views
{
    public interface IView
    {
        int ColumnCount { get; }
        int RowCount { get; }
        int CursorXCoordinate { get; }
        int CursorYCoordinate { get; }
        Variant Variant { get; set; }
        /// <summary>
        /// SetUp everything related to View like culture, window size etc.
        /// </summary>
        void SetUp();
        void GreetPlayer();
        int ChooseLanguage();
        Variant SelectGameVariant();
        string EnterPlayerName(int playerID);
        string SelectStartingSite(string playerName);
        ConsoleColor SelectCheckersColor(string spielername, int spielerNummer);
        void PlayerSelectSameCheckerColor();
        void PlayerSelectEqualNames();
        void CreateGameboard();
        void CreateColumns(int columnNumber);
        void CreateRows(int rowNumber);
        void ErstelleXAchse();
        void PrintChecker(Checker checker);
        void ShowSurrenderOption();
        Position GetPosition(bool isStartingCoordinate, Player player);
        void ShowCurrentPosition(char x, int y);
        void MoveChecker(Checker bewegenderSpielstein);
        void EntferneSpielstein(Checker geschlagenerSpielstein);
        void SpielsteinWurdeInDameUmgewandelt(Position positionSpielstein);
        void BewegeGegnerischenSpielstein();
        void BewegeSpielsteinAufWeißesFeld();
        void BewegeSpielsteinWeiterAlsErlaubt();
        void SelektiereLeeresFeld();
        void BewegeCheckerAufAnderenChecker();
        void BewegeSpielsteinNichtMöglich();
        void ZeigeSpielzugAn(string spielername, ConsoleColor spielsteinFarbe);
        void ZeigeEinenSprungAn(char xKoordinate, int yKoordinate);
        void ZeigeZweiMöglicheSprüngeAn(char xKoordinateRechts, int yKoordinateRechts, char xKoordinateLinks, int yKoordinateLinks);
        void ZeigeAnzahlGefundenerSprünge(List<Checker> spielsteine);
        void SpielsteinHatWenigerSprünge(Checker spielstein);
        void ErzwingeSprung(List<Position> sprungPositionen);
        void SchlageGegnerischenSpielstein();
        void ZeigeAktuelleAnzahlDerSpielsteine(string spieler1Name, int anzahlSpielsteineSpieler1, string spieler2Name, int anzahlSpielsteineSpieler2);
        void ZeigeVerstricheneZeitAn(Stopwatch zeitMessung);
        void EntferneText(int xPosition, int yPosition);
        void SetzeKonsolenFarben();
        void SpielerHatAufgegeben(Player verlierer, Player gewinner);
        void ZeigeGewinnerAn(string gewinnerName);
        void BeendeSpiel();
    }
}