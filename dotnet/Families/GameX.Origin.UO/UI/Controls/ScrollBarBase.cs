// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>Inclusive origin, exclusive far edge. The XNA rectangle test the scroll buttons use.</summary>
public struct ScrollRect {
    public int X, Y, Width, Height;
    public bool Contains(int x, int y) => x >= X && y >= Y && x < X + Width && y < Y + Height;
}

/// <summary>
/// ClassicUO ScrollBarBase. Value clamps and raises ValueChanged.
/// A held button repeats. The drag writes the value from the pointer.
/// UIManager records the press and does not call OnMouseDown, so Update starts the drag.
/// </summary>
public abstract class ScrollBarBase : Control {
    const uint TIME_BETWEEN_CLICKS = 2;

    uint _timeUntilNextClick;
    bool _leftDown;
    int _offsetX, _offsetY;

    // ClassicUO keeps these on Control. They only accelerate the held buttons.
    static int _stepsDone = 1;
    static int _stepChanger = 1;

    protected bool _btUpClicked, _btDownClicked, _btnSliderClicked;
    protected Mouse.Point _clickPosition;
    protected ScrollRect _rectUpButton, _rectDownButton;
    protected int _sliderPosition;
    protected int _value, _minValue, _maxValue;

    public int Value {
        get => _value;
        set {
            if (_value == value) return;
            _value = value;
            if (_value < MinValue) _value = MinValue;
            else if (_value > MaxValue) _value = MaxValue;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int MinValue {
        get => _minValue;
        set {
            if (_minValue == value) return;
            _minValue = value;
            if (_value < _minValue) _value = _minValue;
        }
    }

    public int MaxValue {
        get => _maxValue;
        set {
            if (_maxValue == value) return;
            _maxValue = value < 0 ? 0 : value;
            if (_value > _maxValue) _value = _maxValue;
        }
    }

    public int ScrollStep { get; set; } = 50;

    public Mouse.Point Offset => new() { X = _offsetX, Y = _offsetY };

    public event EventHandler ValueChanged;

    public void UpdateOffset(int x, int y) {
        if (_offsetX == x && _offsetY == y) return;
        _offsetX = x;
        _offsetY = y;
    }

    public override void Update() {
        if (IsDisposed) return;
        ReadMouse();
        base.Update();

        if (_btnSliderClicked) CalculateByPosition(LocalX(), LocalY());

        if (MaxValue <= MinValue) Value = MaxValue = MinValue;

        _sliderPosition = GetSliderYPosition();

        if (_btUpClicked || _btDownClicked) {
            if (_timeUntilNextClick < Time.Ticks) {
                _timeUntilNextClick = Time.Ticks + TIME_BETWEEN_CLICKS;
                if (_btUpClicked) Value -= 1 + _stepChanger;
                else if (_btDownClicked) Value += 1 + _stepChanger;
                _stepsDone++;
                if (_stepsDone % 8 == 0) _stepChanger++;
            }
        }
    }

    public virtual void OnMouseWheel(bool up) {
        Value += up ? -ScrollStep : ScrollStep;
    }

    public virtual void OnMouseDown(int x, int y, MouseButtonType button) {
        if (button != MouseButtonType.Left) return;
        _timeUntilNextClick = 0;
        _btnSliderClicked = false;
        if (_rectDownButton.Contains(x, y)) _btDownClicked = true;
        else if (_rectUpButton.Contains(x, y)) _btUpClicked = true;
        else if (Contains(x, y)) {
            _btnSliderClicked = true;
            CalculateByPosition(x, y);
        }
    }

    public virtual void OnMouseUp(int x, int y, MouseButtonType button) {
        if (button != MouseButtonType.Left) return;
        _btDownClicked = false;
        _btUpClicked = false;
        _btnSliderClicked = false;
        _stepChanger = _stepsDone = 1;
    }

    public virtual bool Contains(int x, int y) => x >= 0 && x <= Width && y >= 0 && y <= Height;

    protected int GetSliderYPosition() {
        if (MaxValue == MinValue) return 0;
        return (int)Math.Round(GetScrollableArea() * ((Value - MinValue) / (float)(MaxValue - MinValue)));
    }

    protected abstract int GetScrollableArea();

    protected abstract void CalculateByPosition(int x, int y);

    void ReadMouse() {
        var down = AcceptMouseInput && IsVisible && IsEnabled && Mouse.LButtonPressed
            && UIManager.LastControlMouseDown((int)MouseButtonType.Left) == this;
        if (down) {
            if (_leftDown) return;
            _leftDown = true;
            OnMouseDown(LocalX(), LocalY(), MouseButtonType.Left);
            return;
        }
        if (!_leftDown) return;
        _leftDown = false;
        if (!Mouse.LButtonPressed) OnMouseUp(LocalX(), LocalY(), MouseButtonType.Left);
    }

    int LocalX() => Mouse.Position.X - X - ChainX();
    int LocalY() => Mouse.Position.Y - Y - ChainY();

    int ChainX() {
        var n = 0;
        for (var p = Parent; p != null; p = p.Parent) n += p.X;
        return n;
    }

    int ChainY() {
        var n = 0;
        for (var p = Parent; p != null; p = p.Parent) n += p.Y;
        return n;
    }
}
