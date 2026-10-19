using OpenX.Gfx;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
#pragma warning disable CS9113

namespace OpenX.Wpf.Control;

#region VulkanControl

/// <summary>
/// Same bindings as <see cref="OpenGLControl"/>. No surface is created here (ADR 0009).
/// </summary>
public abstract class VulkanControl(Func<object, object, object, string, object> shellState) : FrameworkElement {
    int Id = 0;
    bool Hooked;
    TimeSpan Last;

    #region Binding

    protected Renderer Renderer;
    protected abstract Renderer CreateRenderer();

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ISource), typeof(VulkanControl), new PropertyMetadata((d, e) => (d as VulkanControl).OnSourceChanged()));
    public static readonly DependencyProperty Source2Property = DependencyProperty.Register(nameof(Source2), typeof(object), typeof(VulkanControl), new PropertyMetadata((d, e) => (d as VulkanControl).OnSourceChanged()));
    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(object), typeof(VulkanControl), new PropertyMetadata((d, e) => (d as VulkanControl).OnSourceChanged()));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(VulkanControl), new PropertyMetadata((d, e) => (d as VulkanControl).OnSourceChanged()));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(VulkanControl), new PropertyMetadata((d, e) => (d as VulkanControl).OnSourceChanged()));

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
        if (Value is ITextureSelect z2) z2.Select(Id);
    }

    #endregion

    #region Render

    protected override void OnInitialized(EventArgs e) {
        base.OnInitialized(e);
        Loaded += (_, _) => { if (Hooked) return; Hooked = true; CompositionTarget.Rendering += OnRendering; };
        Unloaded += (_, _) => { if (!Hooked) return; Hooked = false; CompositionTarget.Rendering -= OnRendering; };
    }

    void OnRendering(object sender, EventArgs e) {
        var time = ((RenderingEventArgs)e).RenderingTime;
        var delta = Last == default ? 0f : (float)(time - Last).TotalMilliseconds;
        Last = time;
        Tick(delta);
    }

    public virtual void Tick(float deltaTime) => Renderer?.Update(deltaTime);

    #endregion

    #region HandleInput

    static readonly Keys[] AllKeys = [Keys.Q, Keys.W, Keys.A, Keys.Z, Keys.Escape, Keys.Space, Keys.GraveAccent];
    readonly HashSet<Keys> KeyDowns = [];

    //protected override void HandleInput(MouseState mouseState, KeyboardState keyboardState) {
    //    if (Renderer == null) return;
    //    foreach (var key in Keys)
    //        if (!KeyDowns.Contains(key) && keyboardState.IsKeyDown(key)) KeyDowns.Add(key);
    //    foreach (var key in KeyDowns)
    //        if (keyboardState.IsKeyReleased(key)) {
    //            KeyDowns.Remove(key);
    //            switch (key) {
    //                case Key.W: Select(++Id); break;
    //                case Key.Q: Select(--Id); break;
    //            }
    //        }
    //}

    void Select(int id) {
        if (Source is ITextureSelect z2) z2.Select(id);
        OnSourceChanged();
    }

    #endregion
}

#endregion
