using System.Linq;
using Checkers.Tools.Localization;
using Checkers.Tools.Models;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class TranslationsTests
    {
        [Test]
        public void EveryLanguage_HasExactlyTheEnglishKeys()
        {
            var english = Translations.For(Language.English).Keys.ToHashSet();

            foreach (var language in Enum.GetValues<Language>())
            {
                var keys = Translations.For(language).Keys.ToHashSet();
                Assert.That(keys, Is.EquivalentTo(english), $"Keys differ for {language}");
            }
        }

        [Test]
        public void EveryText_IsNotEmpty_AndKeepsPlaceholders()
        {
            var english = Translations.For(Language.English);

            foreach (var language in Enum.GetValues<Language>())
            {
                foreach (var (key, text) in Translations.For(language))
                {
                    Assert.That(text, Is.Not.Empty, $"{language}:{key}");
                    for (var i = 0; i < 4; i++)
                    {
                        Assert.That(text.Contains($"{{{i}}}"), Is.EqualTo(english[key].Contains($"{{{i}}}")), $"{language}:{key} placeholder {i}");
                    }
                }
            }
        }
    }
}
