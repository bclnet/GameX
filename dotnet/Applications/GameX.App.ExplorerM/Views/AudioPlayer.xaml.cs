using NAudio.Wave;
using NLayer.NAudioSupport;

namespace GameX.App.Explorer.Views;

public partial class AudioPlayer : ContentView {
    WaveOutEvent WaveOut = new();
    WaveStream WaveStream;

    public AudioPlayer() {
        InitializeComponent();
        WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
        Unloaded += AudioPlayer_Unloaded;
    }

    void AudioPlayer_Unloaded(object sender, EventArgs e) {
        WaveOut?.Dispose();
        WaveOut = null;
        WaveStream?.Dispose();
        WaveStream = null;
    }

    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(object), typeof(AudioPlayer), propertyChanged: (d, e, n) => (d as AudioPlayer).LoadSound());
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(Stream), typeof(AudioPlayer), propertyChanged: (d, e, n) => (d as AudioPlayer).LoadSound());
    public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(AudioPlayer), propertyChanged: (d, e, n) => (d as AudioPlayer).LoadSound());

    public object Source {
        get => GetValue(SourceProperty) as Stream;
        set => SetValue(SourceProperty, value);
    }

    public Stream Value {
        get => GetValue(ValueProperty) as Stream;
        set => SetValue(ValueProperty, value);
    }

    public string Format {
        get => GetValue(FormatProperty) as string;
        set => SetValue(FormatProperty, value);
    }

    void LoadSound() {
        if (Format != null && Value != null)
            try {
                WaveStream = Format.ToLowerInvariant() switch {
                    ".wav" => new WaveFileReader(Value),
                    ".mp3" => new Mp3FileReader(Value, wf => new Mp3FrameDecompressor(wf)),
                    ".aac" => new StreamMediaFoundationReader(Value),
                    _ => throw new ArgumentOutOfRangeException(nameof(Format), Format),
                };
                WaveOut.Init(WaveStream);
            }
            catch (Exception e) { Console.Error.WriteLine(e); }
    }

    void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e) => PlayButton.Text = "Play";

    void Play_Click(object sender, EventArgs e) {
        if (WaveOut.PlaybackState == PlaybackState.Stopped && WaveStream != null) WaveStream.Position = 0;
        if (WaveOut.PlaybackState == PlaybackState.Playing) { WaveOut.Pause(); PlayButton.Text = "Play"; }
        else { WaveOut.Play(); PlayButton.Text = "Pause"; }
    }
}
