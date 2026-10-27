// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ColorPickerGump. Background 0x0906, OK 0x0907/0x0908/0x0909, eyedropper 0x5669/0x566B/0x566A.
/// The grid is 10 by 20 cells of 8 at 34,34. The slider is 0x0845, range 0..4, and starts at 1.
/// The selected hue is (graduation + 2) + 5 * index. The dye tub art is 0x0FAB.
/// </summary>
public class ColorPickerGump : Gump {
    const int Rows = 10;
    const int Columns = 20;
    const int Cell = 8;
    readonly ColorPickerBox _box;
    readonly HueSlider _slider;
    readonly Dictionary<ushort, TubeArt> _tubes = new();
    ushort _preview;
    bool _sent;

    public ColorPickerGump(World world, uint serial, ushort graphic, int x, int y) : base(world, serial, 0) {
        Graphic = graphic;
        X = x;
        Y = y;
        CanMove = true;
        CanCloseWithRightClick = serial == 0;
        AcceptMouseInput = false;
        Add(new GumpPic(0, 0, 0x0906, 0));
        Add(new Button(1, 0x5669, 0x566B, 0x566A) { X = 212, Y = 33, ButtonAction = ButtonAction.Activate });
        Add(new Button(0, 0x0907, 0x0908, 0x0909) { X = 208, Y = 138, ButtonAction = ButtonAction.Activate });
        _slider = new HueSlider(39, 142, 145, 0, 4, 1);
        Add(_slider);
        _box = new ColorPickerBox(this, 34, 34);
        Add(_box);
        _slider.ValueChanged = value => _box.Graduation = value;
        _preview = _box.SelectedHue;
    }

    public ushort Graphic { get; }

    /// <summary>The hue OK sends. Not the cell index.</summary>
    public ushort SelectedHue => _box.SelectedHue;

    public override void OnButtonClick(int buttonID) {
        // 1 is the 0x5669 eyedropper. No local target callback, so it does not send.
        if (buttonID != 0 || _sent || IsDisposed) return;
        _sent = true;
        OnOk(_box.SelectedHue);
        Dispose();
    }

    public override bool Draw(int x, int y) {
        if (!base.Draw(x, y)) return false;
        DrawTube(x + 200, y + 78);
        return true;
    }

    /// <summary>OK only. Right-click uses Gump.CloseWithRightClick and does not come here.</summary>
    protected virtual void OnOk(ushort hue) { }

    void Preview(ushort hue) => _preview = hue;

    void DrawTube(int x, int y) {
        var sink = GumpArt.Sink;
        if (sink == null || !TryTube(_preview, out var art)) return;
        sink.DrawRaw(unchecked((int)(0x5E100000 + _preview)), x, y, art.W, art.H, art.Px, art.W, art.H);
    }

    bool TryTube(ushort hue, out TubeArt art) {
        if (_tubes.TryGetValue(hue, out art)) return art.Px != null;
        art = default;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null) return false;
        if (!assets.TryArtPixels(0x0FAB + 0x4000, out var src, out var w, out var h) || src == null || w < 1 || h < 1) {
            _tubes[hue] = default;
            return false;
        }
        if (!TryRamp(hue, out var ramp)) return false;
        art = new TubeArt { Px = Remap(src, w, h, ramp, false), W = w, H = h };
        _tubes[hue] = art;
        return true;
    }

    /// <summary>ClassicUO's hue shader, and MovingEffect, index hues.mul at hue - 1.</summary>
    static bool TryRamp(ushort hue, out ushort[] ramp) {
        ramp = null;
        var index = hue & 0x7FFF;
        if (index != 0) index--;
        if (index == 0) return false;
        var uo = UltimaOnline.Current;
        return uo != null && uo.TryHueRamp(index, out ramp) && ramp != null && ramp.Length > 31;
    }

    static byte[] Remap(byte[] src, int w, int h, ushort[] ramp, bool partial) {
        var count = w * h;
        var dest = new byte[count * 2];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(src[i * 2] | (src[i * 2 + 1] << 8));
            if (c != 0) {
                var r = (c >> 10) & 31;
                var g = (c >> 5) & 31;
                var b = c & 31;
                if (!partial || (r == g && g == b)) {
                    c = ramp[Math.Max(r, Math.Max(g, b))];
                    if (c != 0) c |= 0x8000;
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }

    static int At(Control c, bool horizontal) {
        var n = 0;
        for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
        return n;
    }

    /// <summary>ClassicUO ColorPickerBox with no custom palette. A click stores the cell hue, not the index.</summary>
    sealed class ColorPickerBox : Control {
        static readonly byte[] Marker = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        readonly ColorPickerGump _owner;
        readonly Dictionary<ushort, byte[]> _swatch = new();
        readonly ushort[] _hues;
        int _graduation, _selectedIndex;

        public ColorPickerBox(ColorPickerGump owner, int x, int y) {
            _owner = owner;
            X = x;
            Y = y;
            Width = Columns * Cell;
            Height = Rows * Cell;
            _hues = new ushort[Rows * Columns];
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            Graduation = 1;
            SelectedIndex = 0;
        }

        public int Graduation {
            get => _graduation;
            set {
                if (_graduation == value) return;
                _graduation = value;
                Fill();
                _owner.Preview(SelectedHue);
            }
        }

        public int SelectedIndex {
            get => _selectedIndex;
            set {
                if (value < 0 || value >= _hues.Length || _selectedIndex == value) return;
                _selectedIndex = value;
                _owner.Preview(SelectedHue);
            }
        }

        public ushort SelectedHue => _selectedIndex < 0 || _selectedIndex >= _hues.Length ? (ushort)0 : _hues[_selectedIndex];

        public override void OnMouseClick(int x, int y) {
            if (Width < Columns || Height < Rows) return;
            var row = (x - At(this, true)) / (Width / Columns);
            var column = (y - At(this, false)) / (Height / Rows);
            SelectedIndex = row + column * Columns;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (sink != null) {
                for (var i = 0; i < Rows; i++)
                    for (var j = 0; j < Columns; j++) {
                        var hue = _hues[i * Columns + j];
                        var px = Swatch(hue);
                        if (px == null) continue;
                        sink.DrawRaw(unchecked((int)(0x5E000000 + hue)), x + j * Cell, y + i * Cell, Cell, Cell, px, Cell, Cell);
                    }
                if (_hues.Length > 1) {
                    var mx = (int)(x + Width / Columns * (SelectedIndex % Columns + .5f) - 1);
                    var my = (int)(y + Height / Rows * (SelectedIndex / Columns + .5f) - 1);
                    sink.DrawRaw(0x5E00FFFE, mx, my, 2, 2, Marker, 2, 2);
                }
            }
            return base.Draw(x, y);
        }

        // CreateTexture. startColor begins at graduation + 1. Each cell stores that plus one, then start steps by 5.
        void Fill() {
            ushort startColor = (ushort)(_graduation + 1);
            for (var y = 0; y < Rows; y++)
                for (var x = 0; x < Columns; x++) {
                    _hues[y * Columns + x] = (ushort)(startColor + 1);
                    startColor += 5;
                }
        }

        byte[] Swatch(ushort hue) {
            if (_swatch.TryGetValue(hue, out var px)) return px;
            if (!TryRamp(hue, out var ramp)) return null;
            var c = ramp[31];
            if (c == 0) return null;
            c |= 0x8000;
            var pixel = c;
            var lo = (byte)pixel;
            var hi = (byte)(pixel >> 8);
            px = new byte[Cell * Cell * 2];
            for (var i = 0; i < Cell * Cell; i++) {
                px[i * 2] = lo;
                px[i * 2 + 1] = hi;
            }
            _swatch[hue] = px;
            return px;
        }
    }

    /// <summary>ClassicUO HSliderBar, BlueWidgetNoBar. The widget is gump 0x0845. A click sets the value.</summary>
    sealed class HueSlider : Control {
        readonly GumpPic _widget;
        readonly int _min, _max;
        int _value = -1;

        public HueSlider(int x, int y, int w, int min, int max, int value) {
            X = x;
            Y = y;
            Width = w;
            _min = min;
            _max = max;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            _widget = new GumpPic(0, 0, 0x0845, 0) { AcceptMouseInput = false };
            Add(_widget);
            if (!_widget.IsDisposed && _widget.Height > 0) Height = _widget.Height;
            Value = value;
        }

        public Action<int> ValueChanged { get; set; }

        public int Value {
            get => _value;
            set {
                if (value < _min) value = _min;
                else if (value > _max) value = _max;
                if (_value == value) return;
                _value = value;
                Place();
                ValueChanged?.Invoke(_value);
            }
        }

        public override void OnMouseClick(int x, int y) {
            var len = Width;
            var widget = _widget.IsDisposed ? 0 : _widget.Width;
            if (widget > 0) len -= widget;
            if (len < 1) return;
            var local = x - At(this, true);
            var perc = local / (float)len * 100f;
            Value = (int)((_max - _min) * perc / 100f) + _min;
        }

        void Place() {
            var value = _value - _min;
            var maxValue = _max - _min;
            var length = Width;
            var widget = _widget.IsDisposed ? 0 : _widget.Width;
            if (widget > 0) length -= widget;
            var perc = maxValue > 0 ? value / (float)maxValue * 100f : 0f;
            var sx = length > 0 ? (int)(length * perc / 100f) : 0;
            if (sx < 0) sx = 0;
            _widget.X = sx;
        }
    }

    struct TubeArt {
        public byte[] Px;
        public int W, H;
    }
}
