// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>ClassicUO UILayer.</summary>
public enum UILayer { Over, Default, Under }

/// <summary>ClassicUO ClickPriority.</summary>
public enum ClickPriority { High, Default, Low }

/// <summary>
/// Gump index size. False only when the archive is loaded and the graphic has no size.
/// True with 0,0 when the archive is not loaded, so the control is kept.
/// </summary>
public static class GumpMetrics {
    public static bool TrySize(ushort graphic, out int width, out int height) {
        width = height = 0;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null) return true;
        return assets.TryGumpSize(graphic, out width, out height);
    }
}

/// <summary>
/// ClassicUO Control, without a batcher. Draw walks children and does not upload pixels.
/// </summary>
public abstract class Control {
    Control _parent;
    bool _acceptKeyboardInput;

    protected Control() {
        Children = [];
        IsEnabled = true;
        IsVisible = true;
        CanCloseWithRightClick = true;
        Alpha = 1;
    }

    public uint ServerSerial { get; set; }
    public uint LocalSerial { get; set; }
    public int Page { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsDisposed { get; private set; }
    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsFocused { get; set; }
    public bool CanMove { get; set; }
    public bool CanCloseWithRightClick { get; set; }
    public bool CanCloseWithEsc { get; set; }
    public float Alpha { get; set; }
    public bool WantUpdateSize { get; set; } = true;
    public UILayer LayerOrder { get; set; } = UILayer.Default;
    public virtual ClickPriority Priority { get; set; } = ClickPriority.Default;
    public List<Control> Children { get; }
    public bool HasKeyboardFocus => UIManager.KeyboardFocusControl == this;

    public virtual bool AcceptKeyboardInput {
        get => IsEnabled && !IsDisposed && IsVisible && _acceptKeyboardInput;
        set => _acceptKeyboardInput = value;
    }

    public virtual bool AcceptMouseInput { get; set; }

    public Control Parent {
        get => _parent;
        internal set {
            if (value == null) _parent?.Children.Remove(this);
            else {
                _parent?.Children.Remove(this);
                value.Children.Add(this);
            }
            _parent = value;
        }
    }

    public virtual T Add<T>(T c, int page = 0) where T : Control {
        c.Page = page;
        c.Parent = this;
        OnChildAdded();
        return c;
    }

    public virtual void Update() {
        if (IsDisposed || Children.Count == 0) return;
        int w = 0, h = 0;
        for (var i = 0; i < Children.Count; i++) {
            var c = Children[i];
            if (c.IsDisposed) {
                OnChildRemoved();
                Children.RemoveAt(i--);
                continue;
            }
            c.Update();
            if (WantUpdateSize && (c.Page == 0 || c.Page == ActivePage) && c.IsVisible) {
                if (w < c.X + c.Width) w = c.X + c.Width;
                if (h < c.Y + c.Height) h = c.Y + c.Height;
            }
        }
        if (WantUpdateSize && IsVisible) {
            if (w != Width) Width = w;
            if (h != Height) Height = h;
            WantUpdateSize = false;
        }
    }

    public int ActivePage { get; set; }

    /// <summary>Walks children. No pixels: this project does not reference MonoGame.</summary>
    public virtual bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        foreach (var c in Children)
            if ((c.Page == 0 || c.Page == ActivePage) && c.IsVisible)
                c.Draw(c.X + x, c.Y + y);
        return true;
    }

    public void SetKeyboardFocus() {
        if (AcceptKeyboardInput && !HasKeyboardFocus)
            UIManager.KeyboardFocusControl = this;
    }

    public virtual void OnFocusEnter() => IsFocused = true;
    public virtual void OnFocusLost() => IsFocused = false;
    public virtual void OnButtonClick(int buttonID) => Parent?.OnButtonClick(buttonID);
    public virtual void OnMouseClick(int x, int y) { }
    public virtual void OnKeyboardReturn(int textID, string text) => Parent?.OnKeyboardReturn(textID, text);
    protected virtual void OnChildAdded() { }
    protected virtual void OnChildRemoved() { }

    public virtual void Dispose() {
        if (IsDisposed) return;
        foreach (var c in Children) c.Dispose();
        Children.Clear();
        IsDisposed = true;
    }
}
