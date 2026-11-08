// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO AudioManager. The sound and music maps keep the current index, the volume, and Play/Stop.
/// Nothing here opens a device or decodes mp3.
/// </summary>
public sealed class AudioManager {
    const float SOUND_DELTA = 250;
    const int MAX_MUSIC_DATA_INDEX_COUNT = 150;

    bool _canReproduceAudio = true;
    float _masterVolume = 1f;
    readonly Dictionary<int, Track> _sounds = new Dictionary<int, Track>();
    readonly LinkedList<Track> _currentSounds = new LinkedList<Track>();
    readonly Dictionary<int, Track> _music = new Dictionary<int, Track>();
    readonly Track[] _currentMusic = { null, null };
    readonly int[] _currentMusicIndices = { 0, 0 };

    public int LoginMusicIndex { get; private set; }
    public int DeathMusicIndex { get; } = 42;
    public int CurrentSoundIndex { get; private set; }
    public int CurrentMusicIndex { get; private set; }
    public float MasterVolume => _masterVolume;
    public bool IsActive { get; private set; } = true;

    // Profile path is not ported.
    public bool EnableSound { get; set; } = true;
    public int SoundVolume { get; set; } = 100;
    public bool EnableMusic { get; set; } = true;
    public int MusicVolume { get; set; } = 100;
    public bool EnableCombatMusic { get; set; } = true;
    public bool ReproduceSoundsInBackground { get; set; }
    public bool LoginMusic { get; set; } = true;
    public int LoginMusicVolume { get; set; } = 70;

    public void Initialize() {
        // device playback stays on UoMp3
        _canReproduceAudio = true;
        var version = UltimaOnline.Current != null ? UltimaOnline.Current.Version : default;
        LoginMusicIndex = version >= ClientVersion.CV_7000 ? 78 : version > ClientVersion.CV_308Z ? 0 : 8;
    }

    public void OnWindowDeactivated() {
        IsActive = false;
        if (!_canReproduceAudio || ReproduceSoundsInBackground) return;
        // device playback stays on UoMp3
        _masterVolume = 0;
    }

    public void OnWindowActivated() {
        IsActive = true;
        if (!_canReproduceAudio || ReproduceSoundsInBackground) return;
        // device playback stays on UoMp3
        _masterVolume = 1;
    }

    public void PlaySound(int index) {
        if (!_canReproduceAudio) return;

        float volume = SoundVolume / SOUND_DELTA;
        if (IsActive) {
            if (!ReproduceSoundsInBackground) volume = SoundVolume / SOUND_DELTA;
        }
        else if (!ReproduceSoundsInBackground) volume = 0;

        if (volume < -1 || volume > 1f) return;
        if (!EnableSound || !IsActive && !ReproduceSoundsInBackground) volume = 0;

        var sound = GetSound(index);
        if (sound != null && sound.Play(Time.Ticks, volume)) {
            sound.X = -1;
            sound.Y = -1;
            sound.CalculateByDistance = false;
            _currentSounds.AddLast(sound);
            CurrentSoundIndex = index;
        }
    }

    public void PlaySoundWithDistance(World world, int index, int x, int y) {
        // World.Player is not on this World. The tile is UOLoginNet.
        if (!_canReproduceAudio || world == null || UOLoginNet.PlayerSerial == 0) return;

        int distX = Math.Abs(x - UOLoginNet.PlayerX);
        int distY = Math.Abs(y - UOLoginNet.PlayerY);
        int distance = Math.Max(distX, distY);
        float volume = SoundVolume / SOUND_DELTA;
        float distanceFactor = 0.0f;
        int view = UOLoginNet.ViewRange;

        if (distance >= 1) {
            float volumeByDist = volume / (view + 1);
            distanceFactor = volumeByDist * distance;
        }

        if (distance > view) volume = 0;
        if (volume < -1 || volume > 1f) return;
        if (!EnableSound || !IsActive && !ReproduceSoundsInBackground) volume = 0;

        var sound = GetSound(index);
        if (sound != null && sound.Play(Time.Ticks, volume, distanceFactor)) {
            sound.X = x;
            sound.Y = y;
            sound.CalculateByDistance = true;
            _currentSounds.AddLast(sound);
            CurrentSoundIndex = index;
        }
    }

    public void PlayMusic(int music, bool iswarmode = false, bool is_login = false) {
        if (!_canReproduceAudio) return;
        if (music >= MAX_MUSIC_DATA_INDEX_COUNT) return;

        float volume;
        if (is_login) volume = LoginMusic ? LoginMusicVolume / SOUND_DELTA : 0;
        else {
            if (!EnableMusic) volume = 0;
            else volume = MusicVolume / SOUND_DELTA;
            if (!EnableCombatMusic && iswarmode) return;
        }

        if (volume < -1 || volume > 1f) return;

        var m = GetMusic(music);
        if (m == null && _currentMusic[0] != null) StopMusic();
        else if (m != null && (m != _currentMusic[0] || iswarmode)) {
            StopMusic();
            int idx = iswarmode ? 1 : 0;
            _currentMusicIndices[idx] = music;
            CurrentMusicIndex = music;
            _currentMusic[idx] = m;
            _currentMusic[idx].Play(Time.Ticks, volume);
        }
    }

    public void UpdateCurrentMusicVolume(bool isLogin = false) {
        if (!_canReproduceAudio) return;

        for (int i = 0; i < 2; i++) {
            if (_currentMusic[i] != null) {
                float volume;
                if (isLogin) volume = LoginMusic ? LoginMusicVolume / SOUND_DELTA : 0;
                else volume = !EnableMusic ? 0 : MusicVolume / SOUND_DELTA;

                if (volume < -1 || volume > 1f) return;
                _currentMusic[i].Volume = i == 0 && _currentMusic[1] != null ? 0 : volume;
            }
        }
    }

    public void UpdateCurrentSoundsVolume() {
        if (!_canReproduceAudio) return;

        float volume = !EnableSound ? 0 : SoundVolume / SOUND_DELTA;
        if (volume < -1 || volume > 1f) return;

        for (var soundNode = _currentSounds.First; soundNode != null; soundNode = soundNode.Next)
            soundNode.Value.Volume = volume;
    }

    public void StopMusic() {
        for (int i = 0; i < 2; i++) {
            if (_currentMusic[i] != null) {
                _currentMusic[i].Stop();
                _currentMusic[i].Dispose();
                _currentMusic[i] = null;
            }
        }
    }

    public void StopWarMusic() => PlayMusic(_currentMusicIndices[0]);

    public void StopSounds() {
        var first = _currentSounds.First;
        while (first != null) {
            var next = first.Next;
            first.Value.Stop();
            _currentSounds.Remove(first);
            first = next;
        }
    }

    public void Update() {
        if (!_canReproduceAudio) return;

        bool runninWarMusic = _currentMusic[1] != null;
        for (int i = 0; i < 2; i++) {
            if (_currentMusic[i] != null) {
                if (IsActive) {
                    if (!ReproduceSoundsInBackground)
                        _currentMusic[i].Volume = i == 0 && runninWarMusic || !EnableMusic ? 0 : MusicVolume / SOUND_DELTA;
                }
                else if (!ReproduceSoundsInBackground && _currentMusic[i].Volume != 0.0f)
                    _currentMusic[i].Volume = 0;
            }

            _currentMusic[i]?.Update();
        }

        var first = _currentSounds.First;
        while (first != null) {
            var next = first.Next;
            if (!first.Value.IsPlaying(Time.Ticks)) {
                first.Value.Stop();
                _currentSounds.Remove(first);
            }
            first = next;
        }
    }

    /// <summary>Playing music index, or -1 when neither slot is playing.</summary>
    public int GetCurrentMusic() {
        for (int i = 0; i < 2; i++)
            if (_currentMusic[i] != null && _currentMusic[i].IsPlaying(Time.Ticks))
                return _currentMusic[i].Index;
        return -1;
    }

    Track GetSound(int index) {
        if (!_sounds.TryGetValue(index, out var sound)) {
            sound = new Track(index, 250); // Sound.Delay. The wav length is not known.
            _sounds[index] = sound;
        }
        return sound;
    }

    Track GetMusic(int index) {
        if (!_music.TryGetValue(index, out var music)) {
            music = new Track(index, 0); // UOMusic.Delay. The mp3 is not opened.
            _music[index] = music;
        }
        return music;
    }

    sealed class Track {
        readonly uint Delay;
        uint _lastPlayed;
        float _volume = 1f;
        float _volumeFactor;
        float _instanceVolume = 1f;

        public Track(int index, uint delay) {
            Index = index;
            Delay = delay;
        }

        public int Index { get; }
        public int X, Y;
        public bool CalculateByDistance;
        public bool Playing { get; private set; }
        public float InstanceVolume => _instanceVolume;

        public float Volume {
            get => _volume;
            set {
                if (value < 0.0f) value = 0f;
                else if (value > 1f) value = 1f;
                _volume = value;
                _instanceVolume = Math.Max(value - _volumeFactor, 0f);
                // device playback stays on UoMp3
            }
        }

        public float VolumeFactor {
            get => _volumeFactor;
            set {
                _volumeFactor = value;
                Volume = _volume;
            }
        }

        public bool Play(uint curTime, float volume = 1.0f, float volumeFactor = 0.0f) {
            if (_lastPlayed > curTime) return false;
            // device playback stays on UoMp3
            _lastPlayed = curTime + Delay;
            VolumeFactor = volumeFactor;
            Volume = volume;
            Playing = true;
            return true;
        }

        public void Stop() {
            // device playback stays on UoMp3
            Playing = false;
        }

        public bool IsPlaying(uint curTime) {
            // device playback stays on UoMp3
            return Playing;
        }

        public void Update() {
            // device playback stays on UoMp3
        }

        public void Dispose() => Stop();
    }
}
