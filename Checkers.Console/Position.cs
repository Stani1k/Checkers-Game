namespace Checkers.Console
{
    // Klasse Position enthält alle möglichen Koordinaten, welche für eine normale Bewegung oder einen Sprung benötigt werden
    public class Position
    {
        public int XKoordinate { get; set; }
        public int YKoordinate { get; set; }
        // sogenannte calculated properties
        public int XKoordinateRechts => XKoordinate + 1;
        public int XKoordinateLinks => XKoordinate - 1;
        public int ÜbernächsteXKoordinateRechts => XKoordinate + 2;
        public int ÜbernächsteXKoordinateLinks => XKoordinate - 2;
        public int NächsteYKoordinate { get; set; }
        public int ÜbernächsteYKoordinate { get; set; }
        public int NächsteYKoordinateRückwärts { get; set; }
        public int ÜbernächsteYKoordinateRückwärts { get; set; }
        public SprungRichtung SprungRechtsHinten { get; set; }
        public SprungRichtung SprungLinksHinten { get; set; }
        public SprungRichtung SprungRechtsVorne { get; set; }
        public SprungRichtung SprungLinksVorne { get; set; }

        public Position(int xKoordinate, int yKoordinate, int spielRichtung)
        {
            XKoordinate = xKoordinate;
            YKoordinate = yKoordinate;
            NächsteYKoordinate = yKoordinate + spielRichtung;
            ÜbernächsteYKoordinate = yKoordinate + 2 * spielRichtung;
            NächsteYKoordinateRückwärts = yKoordinate - spielRichtung;
            ÜbernächsteYKoordinateRückwärts = yKoordinate - 2 * spielRichtung;

            SprungRechtsHinten = new SprungRichtung(XKoordinateRechts, NächsteYKoordinateRückwärts, ÜbernächsteXKoordinateRechts, ÜbernächsteYKoordinateRückwärts);
            SprungLinksHinten = new SprungRichtung(XKoordinateLinks, NächsteYKoordinateRückwärts, ÜbernächsteXKoordinateLinks, ÜbernächsteYKoordinateRückwärts);
            SprungRechtsVorne = new SprungRichtung(XKoordinateRechts, NächsteYKoordinate, ÜbernächsteXKoordinateRechts, ÜbernächsteYKoordinate);
            SprungLinksVorne = new SprungRichtung(XKoordinateLinks, NächsteYKoordinate, ÜbernächsteXKoordinateLinks, ÜbernächsteYKoordinate);
        }

        protected bool Equals(Position other)
        {
            return XKoordinate == other.XKoordinate && YKoordinate == other.YKoordinate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Position) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (XKoordinate * 397) ^ YKoordinate;
            }
        }

        public bool IstDasEinWeißesFeld(Position zielPosition)
        {
            // Prüft ob die eingegebenen Koordinaten einem weißen Feld entsprechen
            var istEinWeißesFeld = (zielPosition.XKoordinate + zielPosition.YKoordinate) % 2 == 0;

            return istEinWeißesFeld;
        }
    }
}
