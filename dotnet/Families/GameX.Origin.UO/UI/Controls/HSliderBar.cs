// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>ClassicUO HSliderBarStyle.</summary>
public enum HSliderBarStyle {
    MetalWidgetRecessedBar,
    BlueWidgetNoBar
}

/// <summary>
/// ClassicUO HSliderBar. Value, min, max, and the drag math are the same.
/// Metal is gumps 213, 214, 215, and the 216 widget. Blue is gump 0x0845.
/// The value is drawn with DrawText. No profile file.
/// </summary>
public class HSliderBar : Control {
    readonly bool _hasText;
    readonly bool _drawUp;
    readonly List<HSliderBar> _pairedSliders = new List<HSliderBar>();
    readonly HSliderBarStyle _style;
    readonly byte _font;
    readonly ushort _color;
    bool _clicked;
    int _sliderX;
    int _value = -1;
    string _caption = "";
    int _wheel;
    bool _wheelReady;

    public HSliderBar(int x, int y, int w, int min, int max, int value, HSliderBarStyle style, bool hasText = false, byte font = 0, ushort color = 0, bool unicode = true, bool drawUp = false) {
        X = x;
        Y = y;
        _hasText = hasText;
        _drawUp = hasText && drawUp;
        _font = font;
        _color = color;
        MinValue = min;
        MaxValue = max;
        BarWidth = w;
        _style = style;
        AcceptMouseInput = true;
        Width = BarWidth;
        if (GumpMetrics.TrySize(Widget(), out _, out int h) && h > 0) Height = h;
        CalculateOffset();
        Value = value;
    }

    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public int BarWidth { get; set; }
    public float Percents { get; private set; }

    public int Value {
        get => _value;
        set {
            if (_value == value) return;
            int oldValue = _value;
            _value = value;
            if (_value < MinValue) _value = MinValue;
            else if (_value > MaxValue) _value = MaxValue;
            if (_hasText) _caption = Value.ToString();
            if (_value != oldValue) {
                ModifyPairedValues(_value - oldValue);
                CalculateOffset();
            }
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler ValueChanged;

    public void AddParisSlider(HSliderBar s) {
        _pairedSliders.Add(s);
    }

    public override void Update() {
        base.Update();
        if (Height <= 0 && GumpMetrics.TrySize(Widget(), out _, out int h) && h > 0) Height = h;
        _clicked = UIManager.LastControlMouseDown(1) == this && Mouse.LButtonPressed;
        if (_clicked) CalculateNew(Mouse.Position.X - At(this));
        Wheel();
    }

    public override void OnMouseClick(int x, int y) => CalculateNew(x - At(this));

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (_style == HSliderBarStyle.MetalWidgetRecessedBar) {
            Size(213, out int lw, out int lh);
            Size(214, out _, out int mh);
            Size(215, out int rw, out int rh);
            Size(216, out int sw, out int sh);
            GumpArtDraw.Pic(213, x, y, lw, lh, false);
            int mid = BarWidth - rw - lw;
            if (mid > 0) GumpArtDraw.Pic(214, x + lw, y, mid, mh, true);
            GumpArtDraw.Pic(215, x + BarWidth - rw, y, rw, rh, false);
            GumpArtDraw.Pic(216, x + _sliderX, y, sw, sh, false);
        }
        else {
            Size(0x0845, out int sw, out int sh);
            GumpArtDraw.Pic(0x0845, x + _sliderX, y, sw, sh, false);
        }
        if (_hasText && _caption.Length > 0) {
            if (_drawUp) GumpArt.Sink?.DrawText(x, y - 16, _caption, _color, _font);
            else GumpArt.Sink?.DrawText(x + BarWidth + 2, y + (Height >> 1) - 8, _caption, _color, _font);
        }
        return base.Draw(x, y);
    }

    public override void Dispose() {
        _caption = "";
        base.Dispose();
    }

    ushort Widget() => _style == HSliderBarStyle.MetalWidgetRecessedBar ? (ushort)216 : (ushort)0x0845;

    int WidgetWidth() {
        Size(Widget(), out int w, out _);
        return w;
    }

    static void Size(ushort graphic, out int w, out int h) {
        w = h = 0;
        GumpMetrics.TrySize(graphic, out w, out h);
    }

    static int At(Control c) {
        int n = 0;
        for (var p = c; p != null; p = p.Parent) n += p.X;
        return n;
    }

    void Wheel() {
        if (!Over() || !Mouse.HasSample) return;
        int w = Mouse.Current.Wheel;
        if (!_wheelReady) { _wheel = w; _wheelReady = true; return; }
        while (w >= _wheel + 120) { _wheel += 120; Value++; }
        while (w <= _wheel - 120) { _wheel -= 120; Value--; }
    }

    bool Over() {
        for (var p = UIManager.MouseOverControl; p != null; p = p.Parent)
            if (p == this) return true;
        return false;
    }

    void InternalSetValue(int value) {
        _value = value;
        CalculateOffset();
        if (_hasText) _caption = Value.ToString();
    }

    void CalculateNew(int x) {
        int len = BarWidth;
        int maxValue = MaxValue - MinValue;
        len -= WidgetWidth();
        if (len < 1) len = 1;
        float perc = x / (float)len * 100.0f;
        Value = (int)(maxValue * perc / 100.0f) + MinValue;
        CalculateOffset();
    }

    void CalculateOffset() {
        if (Value < MinValue) Value = MinValue;
        else if (Value > MaxValue) Value = MaxValue;
        int value = Value - MinValue;
        int maxValue = MaxValue - MinValue;
        int length = BarWidth - WidgetWidth();
        if (length < 0) length = 0;
        Percents = maxValue > 0 ? value / (float)maxValue * 100.0f : 0;
        _sliderX = (int)(length * Percents / 100.0f);
        if (_sliderX < 0) _sliderX = 0;
    }

    void ModifyPairedValues(int delta) {
        if (_pairedSliders.Count == 0) return;
        bool updateSinceLastCycle = true;
        int d = delta > 0 ? -1 : 1;
        int points = Math.Abs(delta);
        int sliderIndex = Value % _pairedSliders.Count;
        while (points > 0) {
            if (d > 0) {
                if (_pairedSliders[sliderIndex].Value < _pairedSliders[sliderIndex].MaxValue) {
                    updateSinceLastCycle = true;
                    _pairedSliders[sliderIndex].InternalSetValue(_pairedSliders[sliderIndex].Value + d);
                    points--;
                }
            }
            else if (_pairedSliders[sliderIndex].Value > _pairedSliders[sliderIndex].MinValue) {
                updateSinceLastCycle = true;
                _pairedSliders[sliderIndex].InternalSetValue(_pairedSliders[sliderIndex]._value + d);
                points--;
            }
            sliderIndex++;
            if (sliderIndex == _pairedSliders.Count) {
                if (!updateSinceLastCycle) return;
                updateSinceLastCycle = false;
                sliderIndex = 0;
            }
        }
    }
}
