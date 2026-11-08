// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ScrollFlag. The flag graphic is the thumb. Button graphics stay off.
/// A hit is an opaque pixel of the flag (BGRA1555 bit 15).
/// </summary>
public class ScrollFlag : ScrollBarBase {
    readonly bool _showButtons;

    const ushort BUTTON_UP = 0x0824;
    const ushort BUTTON_DOWN = 0x0825;
    const ushort BUTTON_FLAG = 0x0828;

    public ScrollFlag(int x, int y, int height, bool showbuttons) : this() {
        X = x;
        Y = y;
        Height = height;
        // ClassicUO forces the arrow graphics off.
        _showButtons = false;
        _ = showbuttons;
    }

    public ScrollFlag() {
        AcceptMouseInput = true;
        if (!GumpMetrics.TrySize(BUTTON_FLAG, out var flagW, out var flagH)) {
            Dispose();
            return;
        }
        Size(BUTTON_UP, out var upW, out var upH);
        Size(BUTTON_DOWN, out var downW, out var downH);
        Width = flagW;
        Height = flagH;
        _rectUpButton = new ScrollRect { X = 0, Y = 0, Width = upW, Height = upH };
        _rectDownButton = new ScrollRect { X = 0, Y = Height, Width = downW, Height = downH };
        WantUpdateSize = false;
    }

    public override ClickPriority Priority { get; set; } = ClickPriority.High;

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (MaxValue != MinValue) GumpArtDraw.Pic(BUTTON_FLAG, x, y + _sliderPosition, 0, 0, false);
        if (_showButtons) {
            GumpArtDraw.Pic(BUTTON_UP, x, y, 0, 0, false);
            GumpArtDraw.Pic(BUTTON_DOWN, x, y + Height, 0, 0, false);
        }
        return base.Draw(x, y);
    }

    protected override int GetScrollableArea() {
        Size(BUTTON_FLAG, out _, out var h);
        return Height - h;
    }

    protected override void CalculateByPosition(int x, int y) {
        if (y == _clickPosition.Y) return;
        Size(BUTTON_FLAG, out _, out var height);
        y -= height >> 1;
        if (y < 0) y = 0;
        var scrollableArea = GetScrollableArea();
        if (y > scrollableArea) y = scrollableArea;
        _sliderPosition = y;
        _clickPosition.X = x;
        _clickPosition.Y = y;
        if (y == 0 && _clickPosition.Y < height >> 1)
            _clickPosition.Y = height >> 1;
        else if (y == scrollableArea && _clickPosition.Y > Height - (height >> 1))
            _clickPosition.Y = Height - (height >> 1);
        _value = (int)Math.Round(y / (float)scrollableArea * (MaxValue - MinValue) + MinValue);
    }

    public override bool Contains(int x, int y) {
        Size(BUTTON_FLAG, out var w, out var h);
        if (w <= 0 || h <= 0) return false;
        y -= _sliderPosition;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !assets.TryGumpPixels(BUTTON_FLAG, out var px, out w, out h)) return false;
        if ((uint)x >= (uint)w || (uint)y >= (uint)h) return false;
        var i = (y * w + x) * 2;
        if (i < 0 || i + 1 >= px.Length) return false;
        return (px[i + 1] & 0x80) != 0;
    }

    static void Size(ushort graphic, out int w, out int h) {
        if (!GumpMetrics.TrySize(graphic, out w, out h)) w = h = 0;
    }
}
