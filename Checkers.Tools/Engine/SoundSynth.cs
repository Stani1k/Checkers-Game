namespace Checkers.Tools.Engine;

public enum SoundEffect
{
    Move,
    Capture,
    King,
    Win,
    Draw
}

/// <summary>Synthesizes the sound effects as 16-bit mono PCM WAV data, so no audio assets are needed.</summary>
public static class SoundSynth
{
    public const int SampleRate = 22050;

    public static byte[] Render(SoundEffect effect) => effect switch
    {
        SoundEffect.Move => Wav(Tone(220, 0.07, 0.5, decay: 30)),
        SoundEffect.Capture => Wav(Concat(Tone(520, 0.05, 0.6, decay: 25), Tone(330, 0.10, 0.7, decay: 18))),
        SoundEffect.King => Wav(Concat(Tone(523, 0.09, 0.5), Tone(659, 0.09, 0.5), Tone(784, 0.16, 0.5, decay: 8))),
        SoundEffect.Win => Wav(Concat(Tone(523, 0.12, 0.5), Tone(659, 0.12, 0.5), Tone(784, 0.12, 0.5), Tone(1047, 0.3, 0.5, decay: 6))),
        SoundEffect.Draw => Wav(Concat(Tone(392, 0.15, 0.5), Tone(330, 0.25, 0.5, decay: 8))),
        _ => throw new ArgumentOutOfRangeException(nameof(effect))
    };

    private static short[] Tone(double frequency, double seconds, double volume, double decay = 12)
    {
        var count = (int)(SampleRate * seconds);
        var samples = new short[count];
        var attack = SampleRate * 0.004;
        for (var i = 0; i < count; i++)
        {
            var t = (double)i / SampleRate;
            var envelope = Math.Min(1.0, i / attack) * Math.Exp(-decay * t);
            var wave = Math.Sin(2 * Math.PI * frequency * t) + 0.3 * Math.Sin(4 * Math.PI * frequency * t);
            samples[i] = (short)(wave / 1.3 * envelope * volume * short.MaxValue);
        }

        return samples;
    }

    private static short[] Concat(params short[][] parts) => parts.SelectMany(p => p).ToArray();

    private static byte[] Wav(short[] samples)
    {
        using var stream = new MemoryStream();
        using var w = new BinaryWriter(stream);
        var dataLength = samples.Length * 2;
        w.Write("RIFF"u8.ToArray());
        w.Write(36 + dataLength);
        w.Write("WAVEfmt "u8.ToArray());
        w.Write(16);
        w.Write((short)1);
        w.Write((short)1);
        w.Write(SampleRate);
        w.Write(SampleRate * 2);
        w.Write((short)2);
        w.Write((short)16);
        w.Write("data"u8.ToArray());
        w.Write(dataLength);
        foreach (var s in samples)
        {
            w.Write(s);
        }

        return stream.ToArray();
    }
}
