using System.Globalization;
using Checkers.Console;
using Checkers.Console.Models;
using Checkers.Console.Views;
using Moq;
using NUnit.Framework;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class SettingsTests
    {
        private Mock<IView> _view;
        private Settings _settings;

        [SetUp]
        public void SetUp()
        {
            _view = new Mock<IView>();
            _settings = new Settings();
        }

        [Test]
        [TestCase(1, "en-US")]
        [TestCase(2, "de-DE")]
        [TestCase(3, "pl-PL")]
        public void WähleSprache_BeimAufruf_ÄndereSpracheDerAnwendung(int number, string erwarteteSprache)
        {
            // Act
            Settings.SetGameLanguage(number);

            // Assert
            Assert.That(CultureInfo.CurrentUICulture, Is.EqualTo(CultureInfo.GetCultureInfo(erwarteteSprache)));
        }

        [Test]
        [TestCase(Variant.American, Variant.American)]
        [TestCase(Variant.European, Variant.European)]
        public void WähleSpielvariante_NutzerWähltVariante_SetzePropertyWertAufVariante(Variant selektierteVariante, Variant erwartung)
        {
            // Act
            _view.Setup(v => v.EingabeSpielvariante()).Returns(selektierteVariante);

            // Assert
            Assert.That(_settings.Variant, Is.EqualTo(erwartung));
        }
    }
}