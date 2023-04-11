using System;
using System.Collections.Generic;
using Checkers.Console.Interfaces;

namespace Checkers.Console.Models
{
    public class Player
    {
        private readonly IView _view;
        public string Name { get; set; }
        public int Nummer { get; set; }
        public ConsoleColor Farbe { get; set; }
        public int SpielRichtung { get; set; }
        public List<Checker> Spielsteine { get; set; }

        public Player(IView view = null)
        {
            _view = view ?? new View();
        }

        public void WähleSeite(string seitenwahl)
        {
            switch (seitenwahl)
            {
                case "1":
                    SpielRichtung = 1;
                    break;

                case "2":
                    SpielRichtung = -1;
                    break;
            }
        }

        public bool MöchteAufgeben(Position eingabeStartKoordinaten)
        {
            if (eingabeStartKoordinaten == null) return true;
            return false;
        }


        public Checker SelektiereSpielstein(Player spieler, List<Checker> spielsteineMaxSprünge)
        {
            Checker selektierterSpielstein;
            do
            {
                var startPosition = _view.EingabeKoordinaten(true, spieler);
                selektierterSpielstein = PrüfeSelektiertenSpielstein(spielsteineMaxSprünge, startPosition);
            } while (selektierterSpielstein == null);

            return selektierterSpielstein;
        }

        private Checker PrüfeSelektiertenSpielstein(List<Checker> spielsteine, Position startPosition)
        {
            foreach (var spielstein in spielsteine)
            {
                if (startPosition.Equals(spielstein.Position))
                {
                    return spielstein;
                }
            }

            return null;
        }

        public Checker SucheSpielsteinFürStartKoordinaten(Position startPosition)
        {
            foreach (var spielstein in Spielsteine)
            {
                if (spielstein.Position.Equals(startPosition))
                {
                    return spielstein;
                }
            }
            return null;
        }
    }
}