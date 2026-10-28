using Microsoft.Xna.Framework.Audio;
using NLayer;
using System;

namespace GameX.Origin.Mg.UO;

/// <summary>
/// ClassicUO plays an mp3 through MP3Sharp into a DynamicSoundEffectInstance.
/// Song.FromUri rejects this install's mp3, so the same stream goes through NLayer.
/// </summary>
static class UoMp3 {
    const int Chunk = 0x8000;
    static Play _play;

    public static bool Active => _play != null;
    public static bool Indexed { get; private set; }

    public static void Start(string path, bool loop, float volume, bool indexed) {
        Stop();
        var play = new Play(path, loop, volume);
        _play = play;
        Indexed = indexed;
        play.Start();
    }

    public static void Pump() => _play?.Pump();

    public static void SetEnabled(bool on) => _play?.SetEnabled(on);

    public static void Stop() {
        var play = _play;
        _play = null;
        Indexed = false;
        play?.Dispose();
    }

    sealed class Play : IDisposable {
        readonly MpegFile _mpeg;
        readonly DynamicSoundEffectInstance _inst;
        readonly bool _loop;
        readonly object _gate = new();
        readonly float[] _samples;
        readonly byte[] _pcm = new byte[Chunk];
        bool _playing = true;
        bool _dead;

        public Play(string path, bool loop, float volume) {
            _mpeg = new MpegFile(path);
            _loop = loop;
            var channels = _mpeg.Channels <= 1 ? AudioChannels.Mono : AudioChannels.Stereo;
            _inst = new DynamicSoundEffectInstance(_mpeg.SampleRate, channels) { Volume = volume };
            _samples = new float[Chunk / 2];
            _inst.BufferNeeded += OnNeeded;
        }

        public void Start() {
            Pump();
            _inst.Play();
        }

        public void Pump() {
            if (_dead) return;
            Fill();
        }

        public void SetEnabled(bool on) {
            if (_dead || _inst.IsDisposed) return;
            if (on) _inst.Resume();
            else _inst.Pause();
        }

        void OnNeeded(object sender, EventArgs e) => Fill();

        void Fill() {
            lock (_gate) {
                if (_dead || !_playing || _inst.IsDisposed) return;
                while (_inst.PendingBufferCount < 3) {
                    var n = Read();
                    if (n <= 0) {
                        _playing = _loop;
                        break;
                    }
                    var copy = new byte[n];
                    Buffer.BlockCopy(_pcm, 0, copy, 0, n);
                    _inst.SubmitBuffer(copy);
                }
            }
        }

        int Read() {
            var got = _mpeg.ReadSamples(_samples, 0, _samples.Length);
            if (got == 0 && _loop) {
                _mpeg.Time = TimeSpan.Zero;
                got = _mpeg.ReadSamples(_samples, 0, _samples.Length);
            }
            if (got <= 0) return 0;
            var channels = _mpeg.Channels <= 1 ? 1 : 2;
            got -= got % channels;
            for (var i = 0; i < got; i++) {
                var s = _samples[i];
                if (s > 1f) s = 1f;
                else if (s < -1f) s = -1f;
                var v = (short)(s * 32767f);
                _pcm[i * 2] = (byte)v;
                _pcm[i * 2 + 1] = (byte)(v >> 8);
            }
            return got * 2;
        }

        public void Dispose() {
            lock (_gate) {
                _dead = true;
                _playing = false;
                _inst.BufferNeeded -= OnNeeded;
                if (!_inst.IsDisposed) {
                    _inst.Stop();
                    _inst.Dispose();
                }
                _mpeg.Dispose();
            }
        }
    }
}
