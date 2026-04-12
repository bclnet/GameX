using NAudio.Wave;
using NLayer.NAudioSupport;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace GameX.App.Explorer.Views;

/// <summary>
/// Interaction logic for AudioPlayer.xaml
/// </summary>
public partial class AudioPlayer : UserControl, INotifyPropertyChanged {
    WaveOutEvent WaveOut = new();
    WaveStream WaveStream;

    public AudioPlayer() {
        InitializeComponent();
        WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
        Unloaded += AudioPlayer_Unloaded;
    }

    void AudioPlayer_Unloaded(object sender, RoutedEventArgs e) {
        WaveOut?.Dispose();
        WaveOut = null;
        WaveStream?.Dispose();
        WaveStream = null;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(Stream), typeof(AudioPlayer), new PropertyMetadata((d, e) => (d as AudioPlayer).Load()));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(AudioPlayer), new PropertyMetadata((d, e) => (d as AudioPlayer).Load()));
    public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(AudioPlayer), new PropertyMetadata((d, e) => (d as AudioPlayer).Load()));

    public object Source {
        get => GetValue(SourceProperty);
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

    void Load() {
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

    void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e) => PlayButton.Content = "Play";

    void Play_Click(object sender, RoutedEventArgs e) {
        if (WaveOut.PlaybackState == PlaybackState.Stopped && WaveStream != null) WaveStream.Position = 0;
        if (WaveOut.PlaybackState == PlaybackState.Playing) { WaveOut.Pause(); PlayButton.Content = "Play"; }
        else { WaveOut.Play(); PlayButton.Content = "Pause"; }
    }
}
