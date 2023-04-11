namespace Checkers.Console.Models
{
    public class Field
    {
        private readonly Settings _settings;

        public Field(Settings settings)
        {
            _settings = settings;
        }

        // Anzahl der Spalten/Reihen ist je nach Spielvariante unterschiedlich
        public int AnzahlSpalten => _settings.Variante == Variant.Amerikanisch ? 10 : 8;
        public int AnzahlReihen => _settings.Variante == Variant.Amerikanisch ? 10 : 8;
        // die Nummer der Spielfeldreihe ab welcher die Spielfiguren auf das Spielfeld platziert werden
        public int ZeileOben => _settings.Variante == Variant.Amerikanisch ? 4 : 3;
        public int ZeileUnten => _settings.Variante == Variant.Amerikanisch ? 5 : 4;
        // Allgemeine Grenzen des Spielfelds als properties
        public int EndeDerSpielfeldGrenze => _settings.Variante == Variant.Amerikanisch ? 9 : 7;
        public int StartDerSpielfeldGrenze => 0;

        public bool IstDiagonalesFeldFrei(int xKoordinate, int yKoordinate, Player spieler)
        {
            // Durchsucht die Liste mit den vorhandenen Checker und schaut ob es einen Checker mit den parametrierten x und y Koordinaten findet
            foreach (var spielFigur in spieler.Spielsteine)
            {
                // Falls die zu überprüfbaren x-Koordinaten außerhalb der Spielfeldgrenzen liegen, wird das diagonale Feld als besetzt gewertet
                if (xKoordinate > EndeDerSpielfeldGrenze || xKoordinate < StartDerSpielfeldGrenze)
                {
                    return false;
                }

                // Falls die zu überprüfbaren y-Koordinaten außerhalb der Spielfeldgrenzen liegen, wird das diagonale Feld als besetzt gewertet
                if (yKoordinate > EndeDerSpielfeldGrenze || yKoordinate < StartDerSpielfeldGrenze)
                {
                    return false;
                }

                // Prüft ob die übergebenen x und y-Koordinaten, der Position eines Checker entsprechen
                if (spielFigur.Position.Equals(new Position(xKoordinate, yKoordinate, spieler.SpielRichtung)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}