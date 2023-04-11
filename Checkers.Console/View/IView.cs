using System;
using System.Collections.Generic;
using System.Diagnostics;
using Checkers.Console.Models;

namespace Checkers.Console.View
{
    public interface IView
    {
        int AnzahlSpalten { get; }
        int AnzahlZeilen { get; }
        int CursorXPosition { get; }
        int UmwandlungswertYKoordinate { get; }
        Variant Variant { get; set; }
        void KonfiguriereKonsolenfenster();
        void GrüßeNutzer();
        int Sprachauswahl();
        Variant EingabeSpielvariante();
        string EingabeSpielername(int spielerNummer);
        string ErmittleSeiten(string spielername);
        ConsoleColor FarbeDerSpielsteineErmitteln(string spielername, int spielerNummer);
        void NutzerSelektierenZweiGleicheFarben();
        void NutzerWählenZweiGleicheNamen();
        void ErstelleSpielbrett();
        void ErstelleYAchse(int zeile);
        void ErstelleSpalten(int reihe);
        void ErstelleXAchse();
        void DruckeSpielstein(Checker checker);
        void SpielinformationenZumAufgeben();
        Position EingabeKoordinaten(bool istStartKoordinate, Player spieler);
        void AktuellePositionAnzeigen(char xKoordinate, int yKoordinate);
        void PlatziereSpielstein(Checker bewegenderSpielstein);
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