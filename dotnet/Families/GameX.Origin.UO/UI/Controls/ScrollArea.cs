// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>ClassicUO ScrollbarBehaviour.</summary>
public enum ScrollbarBehaviour {
    ShowWhenDataExceedFromView,
    ShowAlways
}

/// <summary>
/// ClassicUO ScrollArea. The bar is child 0. Later children draw through Control.Draw,
/// shifted by the scroll value. This client has no clip rectangle.
/// </summary>
public class ScrollArea : Control {
    readonly ScrollBarBase _scrollBar;
    int _offsetX, _offsetY;

    public ScrollArea(int x, int y, int w, int h, bool normalScrollbar, int scrollMaxHeight = -1) {
        X = x;
        Y = y;
        Width = w;
        Height = h;
        if (normalScrollbar) _scrollBar = new ScrollBar(Width - 14, 0, Height);
        else {
            _scrollBar = new ScrollFlag { X = Width - 19, Height = h };
            Width += 15;
        }
        ScrollMaxHeight = scrollMaxHeight;
        _scrollBar.MinValue = 0;
        _scrollBar.MaxValue = scrollMaxHeight >= 0 ? scrollMaxHeight : Height;
        _scrollBar.Parent = this;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        CanMove = true;
        ScrollbarBehaviour = ScrollbarBehaviour.ShowWhenDataExceedFromView;
    }

    public int ScrollMaxHeight { get; set; } = -1;
    public ScrollbarBehaviour ScrollbarBehaviour { get; set; }
    public int ScrollValue => _scrollBar.Value;
    public int ScrollMinValue => _scrollBar.MinValue;
    public int ScrollMaxValue => _scrollBar.MaxValue;
    public ScrollRect ScissorRectangle;
    public Mouse.Point Offset => new() { X = _offsetX, Y = _offsetY };

    public void UpdateOffset(int x, int y) {
        if (_offsetX == x && _offsetY == y) return;
        _offsetX = x;
        _offsetY = y;
    }

    public override void Update() {
        base.Update();
        CalculateScrollBarMaxValue();
        if (ScrollbarBehaviour == ScrollbarBehaviour.ShowAlways) _scrollBar.IsVisible = true;
        else if (ScrollbarBehaviour == ScrollbarBehaviour.ShowWhenDataExceedFromView)
            _scrollBar.IsVisible = _scrollBar.MaxValue > _scrollBar.MinValue;
    }

    public void Scroll(bool isUp) {
        if (isUp) _scrollBar.Value -= _scrollBar.ScrollStep;
        else _scrollBar.Value += _scrollBar.ScrollStep;
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        _scrollBar.Draw(x + _scrollBar.X, y + _scrollBar.Y);
        var scrollY = -_scrollBar.Value + ScissorRectangle.Y;
        for (var i = 1; i < Children.Count; i++) {
            var child = Children[i];
            if (!child.IsVisible) continue;
            child.Draw(x + child.X, y + child.Y + scrollY);
        }
        return true;
    }

    public void OnMouseWheel(bool up) {
        if (up) _scrollBar.Value -= _scrollBar.ScrollStep;
        else _scrollBar.Value += _scrollBar.ScrollStep;
    }

    public void Clear() {
        for (var i = 1; i < Children.Count; i++) Children[i].Dispose();
    }

    void CalculateScrollBarMaxValue() {
        _scrollBar.Height = ScrollMaxHeight >= 0 ? ScrollMaxHeight : Height;
        var atMax = _scrollBar.Value == _scrollBar.MaxValue && _scrollBar.MaxValue != 0;
        int startY = 0, endY = 0;
        for (var i = 1; i < Children.Count; i++) {
            var c = Children[i];
            if (!c.IsVisible || c.IsDisposed) continue;
            if (c.Y < startY) startY = c.Y;
            if (c.Y + c.Height > endY) endY = c.Y + c.Height;
        }
        var height = Math.Abs(startY) + Math.Abs(endY) - _scrollBar.Height;
        height = Math.Max(0, height - (-ScissorRectangle.Y + ScissorRectangle.Height));
        if (height > 0) {
            _scrollBar.MaxValue = height;
            if (atMax) _scrollBar.Value = _scrollBar.MaxValue;
        }
        else _scrollBar.Value = _scrollBar.MaxValue = 0;
        _scrollBar.UpdateOffset(0, Offset.Y);
    }
}
