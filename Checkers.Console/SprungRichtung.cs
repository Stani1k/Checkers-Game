namespace Checkers.Console
{
    public class SprungRichtung
    {
        public SprungRichtung(int nächsteXKoordinate, int nächsteYKoordinate, int übernächsteXKoordinate, int übernächsteYKoordinate)
        {
            NächsteXKoordinate = nächsteXKoordinate;
            NächsteYKoordinate = nächsteYKoordinate;
            ÜbernächsteXKoordinate = übernächsteXKoordinate;
            ÜbernächsteYKoordinate = übernächsteYKoordinate;
        }

        public int NächsteXKoordinate { get; private set; }
        public int NächsteYKoordinate { get; private set; }
        public int ÜbernächsteXKoordinate { get; private set; }
        public int ÜbernächsteYKoordinate { get; private set; }
    }
}