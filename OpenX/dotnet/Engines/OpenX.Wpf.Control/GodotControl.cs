using OpenX.Gfx;
using System;
using System.Windows;

namespace OpenX.Wpf.Control;

public abstract class GodotControl(Func<object, object, object, string, object> shellState) : ShellControl {
    protected override string GetShellFile() => FindRepoFile(@"engines\Godot\Shell\_build\Shell.exe");
    protected override string LogArgument(string log) => $"--log-file \"{log}\"";
    protected override bool KeepProcess => true;
    protected override string PipeName => "GameX.Godot.Shell";

    #region Binding

    protected Renderer Renderer;
    protected abstract Renderer CreateRenderer();
    protected override object GetShellState() => shellState(Source, Path, Value, Type);

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ISource), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));
    public static readonly DependencyProperty Source2Property = DependencyProperty.Register(nameof(Source2), typeof(object), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));
    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(object), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));

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
        DeliverState(EncodeState(GetShellState()));
    }

    #endregion
}
