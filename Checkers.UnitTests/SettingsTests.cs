using System.Globalization;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class SettingsTests
    {
        private Settings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = new Settings();
        }

        [Test]
        [TestCase(1, "en-US")]
        [TestCase(2, "de-DE")]
        [TestCase(3, "pl-PL")]
        public void SetGameLanguage_WhenCalled_ChangeCurrentUiCulture(int languageId, string expectedLanguage)
        {
            // Act
            Settings.SetGameLanguage(languageId);

            // Assert
            Assert.That(CultureInfo.CurrentUICulture, Is.EqualTo(CultureInfo.GetCultureInfo(expectedLanguage)));
        }
    }
}