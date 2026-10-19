using OpenX.Gfx;
using OpenX.Gfx.Vulkan;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
#pragma warning disable CS9113

namespace OpenX.Wpf.Control;

#region VulkanControl

/// <summary>
/// Same bindings as <see cref="OpenGLControl"/>. Presents a clear-color image on a child HWND.
/// </summary>
public abstract class VulkanControl(Func<object, object, object, string, object> shellState) : HwndHost {
    const int WS_CHILD = 0x40000000;
    const int WS_VISIBLE = 0x10000000;
    const int WS_CLIPSIBLINGS = 0x04000000;
    const uint WM_ERASEBKGND = 0x0014;
    static readonly HostWndProc HostProc = EraseNothing;
    static bool ClassReady;
    int Id = 0;
    bool Hooked;
    TimeSpan Last;
    VulkanView View;

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

    public virtual void Tick(float deltaTime) {
        Renderer?.Update(deltaTime);
        View?.Draw();
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent) {
        var width = Math.Max((int)ActualWidth, 1);
        var height = Math.Max((int)ActualHeight, 1);
        var instance = GetModuleHandle(null);
        EnsureClass(instance);
        var hwnd = CreateWindowEx(0, "GameXVulkanHost", "", WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, 0, 0, width, height, hwndParent.Handle, IntPtr.Zero, instance, IntPtr.Zero);
        try { View = VulkanView.Create(hwnd, instance); }
        catch (Exception e) { Console.Error.WriteLine($"Vulkan view: {e.Message}"); }
        return new HandleRef(this, hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd) {
        View?.Dispose();
        View = null;
        DestroyWindow(hwnd.Handle);
    }

    static void EnsureClass(IntPtr instance) {
        if (ClassReady) return;
        var wc = new WNDCLASS { lpfnWndProc = Marshal.GetFunctionPointerForDelegate(HostProc), hInstance = instance, lpszClassName = "GameXVulkanHost" };
        if (RegisterClass(ref wc) == 0 && Marshal.GetLastWin32Error() != 1410) throw new InvalidOperationException("RegisterClass failed.");
        ClassReady = true;
    }

    static IntPtr EraseNothing(IntPtr hwnd, uint msg, nuint wParam, nint lParam) => msg == WM_ERASEBKGND ? 1 : DefWindowProc(hwnd, msg, wParam, lParam);

    delegate IntPtr HostWndProc(IntPtr hwnd, uint msg, nuint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WNDCLASS {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern ushort RegisterClass(ref WNDCLASS wc);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr DefWindowProc(IntPtr hwnd, uint msg, nuint wParam, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

    [DllImport("user32.dll")]
    static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr GetModuleHandle(string name);

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
