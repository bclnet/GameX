// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Drawing;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ResizableGump. The border is 0x0A8C and 0x0A8D. The grip is button 0x0837.
/// Button has no MouseDown event. The grip is the control from LastControlMouseDown,
/// and the size follows Mouse.LDragOffset until the button is up.
/// BoderSize keeps ClassicUO's spelling.
/// </summary>
public abstract class ResizableGump : Gump {
    readonly Border _border;
    readonly Button _button;
    bool _clicked;
    Point _lastSize, _savedSize, _beforeResizeSize;
    int _minW, _minH;

    public sealed class ResizeCompletedEventArgs : EventArgs {
        public ResizeCompletedEventArgs(Point beforeResize) { BeforeResize = beforeResize; }

        public Point BeforeResize { get; }
    }

    public delegate void ResizeCompletedHandler(object sender, ResizeCompletedEventArgs e);

    public event ResizeCompletedHandler ResizeCompleted;

    protected ResizableGump(World world, int width, int height, int minW, int minH, uint local, uint server, ushort borderHue = 0)
        : base(world, local, server) {
        _border = new Border(4, borderHue);
        Add(_border);
        var grip = new Grip();
        if (grip.IsDisposed) _button = null;
        else {
            _button = grip;
            Add(_button);
        }
        WantUpdateSize = false;
        Width = _lastSize.X = width;
        Height = _lastSize.Y = height;
        _savedSize = _lastSize;
        _beforeResizeSize = _savedSize;
        _minW = minW;
        _minH = minH;
        _clicked = false;
        OnResize();
    }

    public bool ShowBorder {
        get => _border != null && _border.IsVisible;
        set {
            if (_border != null) _border.IsVisible = value;
            if (_button != null) _button.IsVisible = value;
        }
    }

    public int BoderSize => _border != null ? _border.BorderSize : 0;

    protected int MinH {
        get => _minH;
        set { _minH = value; Update(); }
    }

    protected int MinW {
        get => _minW;
        set { _minW = value; Update(); }
    }

    public Point ResizeWindow(Point newSize) {
        if (newSize.X < _minW) newSize.X = _minW;
        if (newSize.Y < _minH) newSize.Y = _minH;
        _savedSize = newSize;
        return newSize;
    }

    public override void Update() {
        if (IsDisposed) return;
        var down = _button != null && !_button.IsDisposed && Mouse.LButtonPressed && UIManager.LastControlMouseDown(1) == _button;
        if (down && !_clicked) {
            _clicked = true;
            _beforeResizeSize = _savedSize;
        }
        var size = _savedSize;
        if (_clicked && down) {
            var off = Mouse.LDragOffset;
            var w = _savedSize.X + off.X;
            var h = _savedSize.Y + off.Y;
            if (w < _minW) w = _minW;
            if (h < _minH) h = _minH;
            size = new Point(w, h);
            _lastSize = size;
        }
        if (!down && _clicked) {
            size = ResizeWindow(_lastSize);
            _clicked = false;
            ResizeCompleted?.Invoke(this, new ResizeCompletedEventArgs(_beforeResizeSize));
        }
        if (Width != size.X || Height != size.Y) {
            Width = size.X;
            Height = size.Y;
            OnResize();
        }
        base.Update();
    }

    public virtual void OnResize() {
        if (_border != null) {
            _border.Width = Width;
            _border.Height = Height;
        }
        if (_button != null && !_button.IsDisposed) {
            _button.X = Width - _button.Width + 2;
            _button.Y = Height - _button.Height + 2;
        }
    }

    sealed class Border : Control {
        public const ushort Horizontal = 0x0A8C;
        public const ushort Vertical = 0x0A8D;

        public Border(int size, ushort hue) {
            BorderSize = size;
            Hue = hue;
            CanMove = true;
            AcceptMouseInput = true;
            WantUpdateSize = false;
        }

        public int BorderSize { get; }
        public ushort Hue { get; set; }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible || Width < 1 || Height < 1 || BorderSize < 1) return false;
            var s = BorderSize;
            GumpArtDraw.Pic(Horizontal, x, y, Width, s, true, Hue);
            if (Height > s) GumpArtDraw.Pic(Horizontal, x, y + Height - s, Width, s, true, Hue);
            GumpArtDraw.Pic(Vertical, x, y, s, Height, true, Hue);
            if (Width > s) GumpArtDraw.Pic(Vertical, x + Width - s, y, s, Height, true, Hue);
            return true;
        }
    }

    /// <summary>The grip does not activate a parent button. Resize lives on the gump Update.</summary>
    sealed class Grip : Button {
        public Grip() : base(0, 0x0837, 0x0838, 0x0838) { }

        public override void OnButtonClick(int buttonID) { }
    }
}
