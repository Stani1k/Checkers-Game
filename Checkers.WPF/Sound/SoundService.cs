using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using Checkers.Tools.Engine;
using Serilog;

namespace Checkers.WPF.Sound;

public interface ISoundService
{
    bool IsEnabled { get; set; }
    void Play(SoundEffect effect);
}

public sealed class SoundService : ISoundService
{
    private readonly Dictionary<SoundEffect, SoundPlayer> _players = new();

    public bool IsEnabled { get; set; } = true;

    public void Play(SoundEffect effect)
    {
        if (!IsEnabled) return;
        try
        {
            if (!_players.TryGetValue(effect, out var player))
            {
                player = new SoundPlayer(new MemoryStream(SoundSynth.Render(effect)));
                player.Load();
                _players[effect] = player;
            }

            player.Play();
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or TimeoutException)
        {
            Log.Warning(ex, "Could not play sound {Effect}", effect);
        }
    }
}
