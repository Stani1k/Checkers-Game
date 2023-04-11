using System.Globalization;
using System.Threading;

namespace Checkers.Console
{
    public class Settings
    {
        public Variant Variante { get; set; }

        public void WähleSprache(int sprachauswahl)
        {
            if (sprachauswahl == 1)
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            }
            else if (sprachauswahl == 2)
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
            }
            else if (sprachauswahl == 3)
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pl-PL");
            }
        }

        public void WähleSpielvariante(Variant variante)
        {
            Variante = variante;
        }
    }
}
