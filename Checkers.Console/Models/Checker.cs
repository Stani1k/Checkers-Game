using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Console.Models
{
    public class Checker
    {
        public ConsoleColor Farbe { get; set; }

        public Position Position { get; set; }

        public int AnzahlSprünge { get; set; }

        public bool IstEineDame { get; set; }

        public List<Position> ZugMöglichkeiten { get; set; }

        public Checker(ConsoleColor farbe, Position position)
        {
            Farbe = farbe;
            Position = position;
        }

        public Checker() {}

        public bool IstEinCheckerBereitsAufDemFeld(Player spieler, Position zielPosition)
        {
            return spieler.Spielsteine.Any(posSpielstein => zielPosition.Equals(posSpielstein.Position));
        }

        public void UmwandlungSpielfigurInDame()
        {
            IstEineDame = true;
        }

        public bool PrüfeBewegung(Position zielPosition)
        {
            if (Position.XKoordinateRechts == zielPosition.XKoordinate ||
                Position.XKoordinateLinks == zielPosition.XKoordinate)
            {
                if (Position.NächsteYKoordinate == zielPosition.YKoordinate)
                {
                    return true;
                }
            }

            return false;
        }

        public bool PrüfeBewegungAmerikanischerDame(Position zielPosition)
        {
            // Amerikanische Variante: Dame darf nur ein Feld vorwärts oder rückwärts ziehen
            if (Position.XKoordinateRechts == zielPosition.XKoordinate || Position.XKoordinateLinks == zielPosition.XKoordinate)
            {
                if (Position.NächsteYKoordinate == zielPosition.YKoordinate || Position.NächsteYKoordinateRückwärts == zielPosition.YKoordinate)
                {
                    return true;
                }
            }

            return false;
        }

        public bool PrüfeBewegungEuropäischerDame(Position zielPosition)
        {
            // Europäische Variante: Dame darf beliebig weit vorwärts oder rückwärts ziehen
            return ZugMöglichkeiten.Where(gültigePosition => gültigePosition.XKoordinate == zielPosition.XKoordinate).Any
                (gültigePosition => gültigePosition.YKoordinate == zielPosition.YKoordinate);
        }
    }
}