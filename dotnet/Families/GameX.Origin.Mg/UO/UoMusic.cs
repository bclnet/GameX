using GameX.Origin.UO;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.Mg.UO;

/// <summary>
/// Plays a login song from the UO install Music folder through Song and MediaPlayer. Lives here because
/// GameX.Origin and GameX.Origin.UO are netstandard2.1 and do not reference MonoGame. Not an OpenX type.
/// </summary>
public static class UoMusic {
    static bool _done;
    static int _tries;
    static Song _song; // held so SetEnabled does not call FromUri again

    /// <summary>
    /// Plays once. An empty client path can be retried. A missing folder, a successful play,
    /// or a container rejection is final so login is not spammed.
    /// </summary>
    public static void Attach() {
        if (_done || _tries >= 6) return;
        try {
            var clientPath = UltimaOnline.Current?.ClientPath;
            if (string.IsNullOrEmpty(clientPath)) return;
            var musicDir = Path.Combine(clientPath, "Music");
            if (!Directory.Exists(musicDir)) { _done = true; return; }
            var files = new List<string>();
            files.AddRange(Directory.GetFiles(musicDir, "*.mp3", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(musicDir, "*.ogg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(musicDir, "*.wav", SearchOption.AllDirectories));
            if (files.Count == 0) { _done = true; return; }
            files.Sort(StringComparer.OrdinalIgnoreCase);
            string path = null, stones = null;
            foreach (var file in files) {
                var name = Path.GetFileName(file);
                if (name.IndexOf("login", StringComparison.OrdinalIgnoreCase) >= 0) { path = file; break; }
                if (stones == null && name.IndexOf("stones", StringComparison.OrdinalIgnoreCase) >= 0) stones = file;
            }
            path ??= stones ?? files[0];
            _tries++;
            var song = Song.FromUri(Path.GetFileNameWithoutExtension(path), new Uri(path));
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.4f;
            MediaPlayer.Play(song);
            _song = song;
            _done = true;
            Console.WriteLine($"UO music: {Path.GetFileName(path)}");
            Console.Out.Flush();
        }
        catch (Exception ex) {
            _done = true;
            Console.WriteLine($"UO music: {ex.GetType().Name}: {ex.Message}");
            Console.Out.Flush();
        }
    }

    /// <summary>
    /// Pauses or resumes the song from a successful Attach. Does nothing when play never succeeded, and does not log.
    /// </summary>
    public static void SetEnabled(bool on) {
        if (_song == null) return;
        if (on) MediaPlayer.Resume();
        else MediaPlayer.Pause();
    }
}
