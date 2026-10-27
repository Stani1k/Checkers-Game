using System;
using Checkers.Tools.Engine;

namespace Checkers.UnitTests
{
    public class SoundSynthTests
    {
        [TestCaseSource(nameof(Effects))]
        public void Render_ProducesValidWav(SoundEffect effect)
        {
            var data = SoundSynth.Render(effect);

            Assert.That(System.Text.Encoding.ASCII.GetString(data, 0, 4), Is.EqualTo("RIFF"));
            Assert.That(System.Text.Encoding.ASCII.GetString(data, 8, 4), Is.EqualTo("WAVE"));
            Assert.That(BitConverter.ToInt32(data, 4), Is.EqualTo(data.Length - 8));
            Assert.That(BitConverter.ToInt32(data, 40), Is.EqualTo(data.Length - 44));
            Assert.That(data.Length, Is.GreaterThan(44));
        }

        [Test]
        public void Settings_SoundEnabledByDefault() => Assert.That(new AppSettings().IsSoundEnabled, Is.True);

        private static readonly SoundEffect[] Effects = Enum.GetValues<SoundEffect>();
    }
}
