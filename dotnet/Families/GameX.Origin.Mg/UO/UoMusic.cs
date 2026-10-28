using GameX;
using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using GameX.Origin.UO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.Origin.Mg.UO;

/// <summary>
/// Plays a login song from the UO install Music folder through Song and MediaPlayer, then a changed
/// sound or music index. Lives here because GameX.Origin and GameX.Origin.UO are netstandard2.1 and
/// do not reference MonoGame. Not an OpenX type.
/// </summary>
public static class UoMusic {
    static bool _done;
    static int _tries;
    static Song _song; // held so SetEnabled does not call FromUri again
    static bool _indexed;
    static int _soundSeen;
    static int _musicSeen;
    static bool _soundOnce;
    static Archive _pack;
    static bool _packTried;
    static Dictionary<int, (string name, bool loop)> _catalog;
    static string _musicDir;
    static bool _catalogTried;
    static SoundEffectInstance _musicFx;
    static SoundEffect _musicEffect;
    static readonly List<SoundEffect> _held = new();

    /// <summary>
    /// Login song, then any SoundIndex or MusicIndex that changed since the last call.
    /// </summary>
    public static void Attach() {
        Login();
        Poll();
        UoMp3.Pump();
    }

    /// <summary>
    /// Plays once. An empty client path can be retried. A missing folder, a successful play,
    /// or a container rejection is final so login is not spammed.
    /// </summary>
    static void Login() {
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
            if (Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                UoMp3.Start(path, true, 0.4f, false);
            else if (Path.GetExtension(path).Equals(".wav", StringComparison.OrdinalIgnoreCase))
                PlayWav(path, true);
            else {
                var song = Song.FromUri(Path.GetFileNameWithoutExtension(path), new Uri(path));
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.4f;
                MediaPlayer.Play(song);
                _song = song;
            }
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
    /// 0 is nothing new. Any other value plays once, including when it returns after a 0.
    /// </summary>
    static void Poll() {
        var clientPath = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(clientPath)) return;
        var sound = UOLoginNet.SoundIndex;
        if (sound != _soundSeen) {
            _soundSeen = sound;
            if (sound != 0)
                try { PlaySound(sound); }
                catch (Exception ex) { LogSoundOnce($"UO sound: {ex.GetType().Name}: {ex.Message}"); }
        }
        var music = UOLoginNet.MusicIndex;
        if (music != _musicSeen) {
            _musicSeen = music;
            if (music == 0) { StopIndexed(); return; }
            try { PlayMusic(clientPath, music); }
            catch (Exception ex) {
                Console.WriteLine($"UO music: {ex.GetType().Name}: {ex.Message}");
                Console.Out.Flush();
            }
        }
    }

    /// <summary>
    /// Pauses or resumes the song from a successful Attach. Does nothing when play never succeeded, and does not log.
    /// </summary>
    public static void SetEnabled(bool on) {
        if (_song != null) {
            if (on) MediaPlayer.Resume();
            else MediaPlayer.Pause();
        }
        if (_musicFx != null) {
            if (on) _musicFx.Resume();
            else _musicFx.Pause();
        }
        UoMp3.SetEnabled(on);
    }

    /// <summary>
    /// The same open Assets uses for a pack. soundLegacyMUL.uop, else soundidx.mul.
    /// </summary>
    static Archive OpenPack(Archive game, string name) {
        var (arc, file) = game.GetSource(name, throwOnError: false);
        if (file?.Arc != null) return file.Arc.Open();
        if (file == null && arc != null && !ReferenceEquals(arc, game)) return arc;
        return null;
    }

    static Archive SoundPack() {
        if (_packTried) return _pack;
        _packTried = true;
        try {
            var game = Database.Archive;
            if (game == null) { LogSoundOnce("UO sound: missing"); return null; }
            _pack = OpenPack(game, "soundLegacyMUL.uop") ?? OpenPack(game, "soundidx.mul");
        }
        catch (Exception ex) {
            LogSoundOnce($"UO sound: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
        if (_pack == null) LogSoundOnce("UO sound: missing");
        return _pack;
    }

    static byte[] ReadSound(Archive pack, int index) {
        var task = pack.GetData($"file{index:x5}.wav", default, false);
        if (task == null) return null;
        using var data = task.GetAwaiter().GetResult();
        if (data == null) return null;
        if (data.CanSeek) data.Position = 0;
        using var ms = new MemoryStream();
        data.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// A RIFF entry is already a wav. Otherwise the entry is a 40-byte name and 16-bit mono 22050 PCM,
    /// which SoundEffect plays with no decoder. Anything else is logged once.
    /// </summary>
    static void PlaySound(int index) {
        var pack = SoundPack();
        if (pack == null) return;
        var data = ReadSound(pack, index);
        if (data == null) { LogSoundOnce("UO sound: missing"); return; }
        SoundEffect fx;
        string label;
        if (IsWav(data)) {
            using var ms = new MemoryStream(data, false);
            fx = SoundEffect.FromStream(ms);
            label = index.ToString();
        }
        else if (data.Length >= 42 && ((data.Length - 40) & 1) == 0) {
            fx = new SoundEffect(data, 40, data.Length - 40, 22050, AudioChannels.Mono, 0, 0);
            label = SoundName(data) ?? index.ToString();
        }
        else { LogSoundOnce("UO sound: not a wav"); return; }
        if (!fx.Play(0.4f, 0f, 0f)) { fx.Dispose(); return; }
        Hold(fx);
        Console.WriteLine($"UO sound: {label}");
        Console.Out.Flush();
    }

    static bool IsWav(byte[] data) => data.Length > 12
        && data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F'
        && data[8] == (byte)'W' && data[9] == (byte)'A' && data[10] == (byte)'V' && data[11] == (byte)'E';

    static string SoundName(byte[] data) {
        var n = 0;
        var limit = Math.Min(16, data.Length);
        while (n < limit && data[n] != 0) n++;
        return n == 0 ? null : Encoding.ASCII.GetString(data, 0, n);
    }

    static void LogSoundOnce(string line) {
        if (_soundOnce) return;
        _soundOnce = true;
        Console.WriteLine(line);
        Console.Out.Flush();
    }

    static void Hold(SoundEffect fx) {
        _held.Add(fx);
        while (_held.Count > 8) {
            _held[0].Dispose();
            _held.RemoveAt(0);
        }
    }

    static bool MusicCatalog(string clientPath) {
        if (_catalogTried) return _catalog != null;
        _catalogTried = true;
        var digital = Path.Combine(clientPath, "Music", "Digital");
        var dir = Directory.Exists(digital) ? digital : Path.Combine(clientPath, "Music");
        var config = Path.Combine(dir, "Config.txt");
        if (!File.Exists(config)) return false;
        var map = new Dictionary<int, (string name, bool loop)>();
        foreach (var line in File.ReadLines(config)) {
            var splits = line.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length < 2 || splits.Length > 3) continue;
            if (!int.TryParse(splits[0], out var index)) continue;
            map[index] = (splits[1], splits.Length == 3 && splits[2] == "loop");
        }
        _catalog = map;
        _musicDir = dir;
        return true;
    }

    static string FindTrack(string configName) {
        var stem = Path.GetFileNameWithoutExtension(configName);
        string wav = null, ogg = null, mp3 = null;
        foreach (var file in Directory.GetFiles(_musicDir, "*", SearchOption.AllDirectories)) {
            if (!Path.GetFileNameWithoutExtension(file).Equals(stem, StringComparison.OrdinalIgnoreCase)) continue;
            var ext = Path.GetExtension(file);
            if (ext.Equals(".wav", StringComparison.OrdinalIgnoreCase)) wav = file;
            else if (ext.Equals(".ogg", StringComparison.OrdinalIgnoreCase)) ogg = file;
            else if (ext.Equals(".mp3", StringComparison.OrdinalIgnoreCase)) mp3 = file;
        }
        return wav ?? ogg ?? mp3;
    }

    /// <summary>
    /// A wav track plays through SoundEffect. An mp3 is decoded by UoMp3. ogg stays on Song.FromUri.
    /// </summary>
    static void PlayMusic(string clientPath, int index) {
        StopIndexed();
        if (!MusicCatalog(clientPath) || !_catalog.TryGetValue(index, out var row)) {
            Console.WriteLine($"UO music: index {index} missing");
            Console.Out.Flush();
            return;
        }
        var path = FindTrack(row.name);
        if (path == null) {
            Console.WriteLine($"UO music: {row.name} missing");
            Console.Out.Flush();
            return;
        }
        if (Path.GetExtension(path).Equals(".wav", StringComparison.OrdinalIgnoreCase))
            PlayWav(path, row.loop);
        else if (Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase)) {
            StopSong();
            UoMp3.Start(path, row.loop, 0.4f, true);
            _indexed = true;
        }
        else {
            var song = Song.FromUri(Path.GetFileNameWithoutExtension(path), new Uri(path));
            MediaPlayer.IsRepeating = row.loop;
            MediaPlayer.Volume = 0.4f;
            MediaPlayer.Play(song);
            _song = song;
            _indexed = true;
        }
        Console.WriteLine($"UO music: {Path.GetFileName(path)}");
        Console.Out.Flush();
    }

    static void PlayWav(string path, bool loop) {
        using var fs = File.OpenRead(path);
        var fx = SoundEffect.FromStream(fs);
        var inst = fx.CreateInstance();
        inst.IsLooped = loop;
        inst.Volume = 0.4f;
        _musicFx?.Dispose();
        _musicEffect?.Dispose();
        _musicFx = inst;
        _musicEffect = fx;
        inst.Play();
    }

    static void StopIndexed() {
        if (_musicFx != null) {
            _musicFx.Stop();
            _musicFx.Dispose();
            _musicFx = null;
        }
        _musicEffect?.Dispose();
        _musicEffect = null;
        if (!_indexed) return;
        _indexed = false;
        if (UoMp3.Indexed) UoMp3.Stop();
        StopSong();
    }

    static void StopSong() {
        if (_song == null) return;
        MediaPlayer.Stop();
        _song = null;
    }
}