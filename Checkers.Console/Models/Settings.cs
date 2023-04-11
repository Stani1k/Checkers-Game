using System.Globalization;
using System.Threading;

namespace Checkers.Console.Models
{
    public class Settings
    {
        public Variant Variant { get; private set; }

        public static void SetGameLanguage(int languageId)
        {
            switch (languageId)
            {
                case 1:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                    break;
                case 2:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
                    break;
                case 3:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pl-PL");
                    break;
            }
        }

        public void SetGameVariant(Variant variant)
        {
            Variant = variant;
        }
    }
}
