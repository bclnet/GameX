using OpenX.Gfx;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows;

namespace OpenX.Wpf.Control;

public abstract class UnityControl(Func<object, object, object, string, object> shellState) : ShellControl {
    protected override string GetShellFile() => FindRepoFile(@"engines\Unity\Shell\Build\Shell.exe");
    protected override string LogArgument(string log) => $"-logFile \"{log}\"";
    protected override string ExtraArguments(IntPtr parent) {
        var w = (int)ActualWidth;
        var h = (int)ActualHeight;
        var size = w > 1 && h > 1 ? $" -screen-width {w} -screen-height {h}" : "";
        return $"-parentHWND {parent.ToInt64()} -screen-fullscreen 0{size}";
    }
    protected override bool AttachParent => false;
    protected override bool KeepProcess => true;

    static NamedPipeServerStream Pipe;
    static readonly object PipeLock = new();

    protected override void DeliverState(string token) {
        if (string.IsNullOrEmpty(token)) return;
        try {
            var pipe = EnsurePipe();
            if (pipe == null || !pipe.IsConnected) return;
            var bytes = Encoding.UTF8.GetBytes(token + "\n");
            ThreadPool.QueueUserWorkItem(_ => {
                try {
                    lock (PipeLock) {
                        if (pipe == null || !pipe.IsConnected) return;
                        pipe.Write(bytes, 0, bytes.Length);
                        pipe.Flush();
                    }
                }
                catch { }
            });
        }
        catch { }
    }

    static NamedPipeServerStream EnsurePipe() {
        if (Pipe != null) return Pipe;
        lock (PipeLock) {
            if (Pipe != null) return Pipe;
            var pipe = new NamedPipeServerStream("GameX.Unity.Shell", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            pipe.BeginWaitForConnection(ar => { try { pipe.EndWaitForConnection(ar); } catch { } }, null);
            return Pipe = pipe;
        }
    }

    #region Binding

    protected Renderer Renderer;
    protected abstract Renderer CreateRenderer();
    protected override object GetShellState() => shellState(Source, Path, Value, Type);

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ISource), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));
    public static readonly DependencyProperty Source2Property = DependencyProperty.Register(nameof(Source2), typeof(object), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));
    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(object), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));

    public ISource Source {
        get => GetValue(SourceProperty) as ISource;
        set => SetValue(SourceProperty, value);
    }

    public object Source2 {
        get => GetValue(Source2Property);
        set => SetValue(Source2Property, value);
    }

    public object Path {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public object Value {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Type {
        get => GetValue(TypeProperty) as string;
        set => SetValue(TypeProperty, value);
    }

    void OnSourceChanged() {
        if (Source == null || Path == null || Value == null || Type == null) return;
        Renderer = CreateRenderer();
        Renderer?.Start();
        try { DeliverState(EncodeState(GetShellState())); }
        catch { }
    }

    #endregion
}
