// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ScrollBar. The track is three gumps. The up and down buttons swap graphic while held.
/// </summary>
public class ScrollBar : ScrollBarBase {
    ScrollRect _rectSlider, _emptySpace;

    const ushort BUTTON_UP_0 = 251;
    const ushort BUTTON_UP_1 = 250;
    const ushort BUTTON_DOWN_0 = 253;
    const ushort BUTTON_DOWN_1 = 252;
    const ushort BACKGROUND_0 = 257;
    const ushort BACKGROUND_1 = 256;
    const ushort BACKGROUND_2 = 255;
    const ushort SLIDER = 254;

    public ScrollBar(int x, int y, int height) {
        Height = height;
        X = x;
        Y = y;
        AcceptMouseInput = true;
        Place();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible || Height <= 0) return false;
        Size(BUTTON_UP_0, out _, out var up0H);
        Size(BUTTON_UP_1, out _, out var up1H);
        Size(BUTTON_DOWN_0, out _, out var down0H);
        Size(BACKGROUND_0, out var bg0W, out var bg0H);
        Size(BACKGROUND_2, out var bg2W, out var bg2H);
        Size(SLIDER, out var slW, out var slH);
        var middle = Height - up0H - down0H - bg0H - bg2H;
        if (middle > 0) {
            GumpArtDraw.Pic(BACKGROUND_0, x, y + up0H, bg0W, bg0H, false);
            GumpArtDraw.Pic(BACKGROUND_1, x, y + up1H + bg0H, bg0W, middle, true);
            GumpArtDraw.Pic(BACKGROUND_2, x, y + Height - down0H - bg2H, bg2W, bg2H, false);
        }
        else {
            middle = Height - up0H - down0H;
            if (middle > 0) GumpArtDraw.Pic(BACKGROUND_1, x, y + up0H, bg0W, middle, true);
        }
        GumpArtDraw.Pic(_btUpClicked ? BUTTON_UP_1 : BUTTON_UP_0, x, y, 0, 0, false);
        GumpArtDraw.Pic(_btDownClicked ? BUTTON_DOWN_1 : BUTTON_DOWN_0, x, y + Height - down0H, 0, 0, false);
        if (MaxValue > MinValue && middle > 0)
            GumpArtDraw.Pic(SLIDER, x + ((bg0W - slW) >> 1), y + up0H + _sliderPosition, slW, slH, false);
        return base.Draw(x, y);
    }

    public override void Update() {
        Place();
        base.Update();
    }

    public override void OnMouseDown(int x, int y, MouseButtonType button) {
        Place();
        base.OnMouseDown(x, y, button);
        if (_btnSliderClicked && _emptySpace.Contains(x, y)) CalculateByPosition(x, y);
    }

    public override bool Contains(int x, int y) => x >= 0 && x <= Width && y >= 0 && y <= Height;

    protected override int GetScrollableArea() {
        Size(BUTTON_UP_0, out _, out var upH);
        Size(BUTTON_DOWN_0, out _, out var downH);
        Size(SLIDER, out _, out var slH);
        return Height - upH - downH - slH;
    }

    protected override void CalculateByPosition(int x, int y) {
        if (y == _clickPosition.Y) return;
        y -= _emptySpace.Y + (_rectSlider.Height >> 1);
        if (y < 0) y = 0;
        var scrollableArea = GetScrollableArea();
        if (y > scrollableArea) y = scrollableArea;
        _sliderPosition = y;
        _clickPosition.X = x;
        _clickPosition.Y = y;
        Size(BUTTON_UP_0, out _, out var upH);
        Size(BUTTON_DOWN_0, out _, out var downH);
        Size(SLIDER, out _, out var slH);
        if (y == 0 && _clickPosition.Y < upH + (slH >> 1))
            _clickPosition.Y = upH + (slH >> 1);
        else if (y == scrollableArea && _clickPosition.Y > Height - downH - (slH >> 1))
            _clickPosition.Y = Height - downH - (slH >> 1);
        _value = (int)Math.Round(y / (float)scrollableArea * (MaxValue - MinValue) + MinValue);
    }

    void Place() {
        Size(BUTTON_UP_0, out var upW, out var upH);
        Size(BUTTON_DOWN_0, out var downW, out var downH);
        Size(BACKGROUND_0, out var bgW, out _);
        Size(SLIDER, out var slW, out var slH);
        if (bgW > 0) Width = bgW;
        _rectUpButton = new ScrollRect { X = 0, Y = 0, Width = upW, Height = upH };
        _rectDownButton = new ScrollRect { X = 0, Y = Height - downH, Width = downW, Height = downH };
        _rectSlider = new ScrollRect { X = (bgW - slW) >> 1, Y = upH + _sliderPosition, Width = slW, Height = slH };
        _emptySpace = new ScrollRect { X = 0, Y = upH, Width = slW, Height = Height - (downH + upH) };
    }

    static void Size(ushort graphic, out int w, out int h) {
        if (!GumpMetrics.TrySize(graphic, out w, out h)) w = h = 0;
    }
}
