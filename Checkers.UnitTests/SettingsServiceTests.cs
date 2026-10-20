using System.IO;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;

namespace Checkers.UnitTests
{
    [TestFixture]
    public class SettingsServiceTests
    {
        private string _directory = string.Empty;
        private SettingsService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "checkers-tests-" + Guid.NewGuid().ToString("N"));
            _service = new SettingsService(Path.Combine(_directory, "settings.json"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Test]
        public void Load_WithoutFile_ReturnsDefaults()
        {
            var settings = _service.Load();

            Assert.That(settings.Language, Is.EqualTo(Language.English));
            Assert.That(settings.IsDarkTheme, Is.True);
            Assert.That(settings.IsMaximized, Is.True);
        }

        [Test]
        public void SaveThenLoad_RoundTripsAllSettings()
        {
            Assert.That(_service.TrySave(new AppSettings { Language = Language.Polish, IsDarkTheme = false, IsMaximized = false }), Is.True);

            var loaded = new SettingsService(_service.FilePath).Load();

            Assert.That(loaded.Language, Is.EqualTo(Language.Polish));
            Assert.That(loaded.IsDarkTheme, Is.False);
            Assert.That(loaded.IsMaximized, Is.False);
        }

        [TestCase("not json")]
        [TestCase("")]
        [TestCase("{ \"Language\": \"Klingon\" }")]
        [TestCase("{ \"Language\": 42 }")]
        public void Load_WithInvalidContent_ReturnsDefaults(string content)
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_service.FilePath, content);

            var settings = _service.Load();

            Assert.That(settings.Language, Is.EqualTo(Language.English));
            Assert.That(settings.IsDarkTheme, Is.True);
        }
    }
}
