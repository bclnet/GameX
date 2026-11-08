// SPDX-License-Identifier: BSD-2-Clause

using OpenX.Client;
using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ArrowNumbersTextBox. The field is TextBox, which has no font, so Draw skips it and draws with Font.
/// A press steps once, then every 250 ms after 500 ms. Time.Ticks is not advanced, so the wait uses GlobalTime.
/// Typed characters must be digits. Leaving the field clamps to min and max.
/// </summary>
public class ArrowNumbersTextBox : Control {
    const int TIME_BETWEEN_CLICKS = 250;
    readonly int _Min, _Max;
    readonly byte _font;
    readonly TextBox _textBox;
    readonly Button _up, _down;
    uint _timeUntilNextClick;
    bool _upHeld, _downHeld, _wasFocus;
    bool _fromCode;
    string _shown = "";

    public ArrowNumbersTextBox(
        int x,
        int y,
        int width,
        int raiseamount,
        int minvalue,
        int maxvalue,
        byte font = 0,
        int maxcharlength = -1,
        bool isunicode = true,
        FontStyle style = FontStyle.None,
        ushort hue = 0
    ) {
        int height = 20;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        _Min = minvalue;
        _Max = maxvalue;
        _font = font;
        _ = isunicode;
        _ = style;
        WantUpdateSize = false;
        AcceptMouseInput = true;
        CanMove = false;

        Add(new ResizePic(0x0BB8) {
            Width = width,
            Height = height + 4,
            AcceptMouseInput = false,
            CanMove = false,
        });

        _up = new Button(raiseamount, 0x983, 0x984) {
            X = width - 12,
            ButtonAction = ButtonAction.Activate,
        };
        Add(_up);

        _down = new Button(-raiseamount, 0x985, 0x986) {
            X = width - 12,
            Y = height - 7,
            ButtonAction = ButtonAction.Activate,
        };
        Add(_down);

        _textBox = new TextBox {
            X = 2,
            Y = 2,
            Height = height,
            Width = width - 17,
            Hue = hue,
            MaxLength = maxcharlength,
        };
        Add(_textBox);
    }

    public byte Font => _font;

    /// <summary>ClassicUO Control.Tag. ValidateValue stores the clamped int here.</summary>
    public object Tag { get; set; }

    public string Text {
        get => _textBox == null || _textBox.IsDisposed ? "" : _textBox.Text;
        set => SetBox(value);
    }

    public override void Update() {
        if (IsDisposed) return;
        bool focus = _textBox != null && !_textBox.IsDisposed && _textBox.HasKeyboardFocus;
        if (_wasFocus && !focus) Commit();
        _wasFocus = focus;

        var held = Mouse.LButtonPressed ? UIManager.LastControlMouseDown((int)MouseButtonType.Left) : null;
        bool up = held == _up;
        bool down = held == _down;
        if ((up && !_upHeld) || (down && !_downHeld)) {
            _upHeld = up;
            _downHeld = down;
            UpdateValue();
            // ClassicUO stored 500 here and compared it to the clock, so the next update stepped again.
            _timeUntilNextClick = GlobalTime.Ticks + TIME_BETWEEN_CLICKS * 2;
        }
        else {
            _upHeld = up;
            _downHeld = down;
            if ((_upHeld || _downHeld) && GlobalTime.Ticks > _timeUntilNextClick) {
                _timeUntilNextClick = GlobalTime.Ticks + TIME_BETWEEN_CLICKS;
                UpdateValue();
            }
        }
        KeepNumbers();
        base.Update();
    }

    public override void OnFocusLost() {
        if (IsDisposed) return;
        Commit();
        base.OnFocusLost();
    }

    public override void OnButtonClick(int buttonID) {
        // The step already happened on the press. Do not step again, and do not tell the parent.
        if (_up != null && buttonID == _up.ButtonID) return;
        if (_down != null && buttonID == _down.ButtonID) return;
        base.OnButtonClick(buttonID);
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        foreach (var c in Children) {
            if (c == _textBox || c.IsDisposed || !c.IsVisible) continue;
            if (c.Page != 0 && c.Page != ActivePage) continue;
            c.Draw(c.X + x, c.Y + y);
        }
        if (_textBox != null && !_textBox.IsDisposed && _textBox.IsVisible)
            GumpArt.Sink?.DrawText(x + _textBox.X, y + _textBox.Y, _textBox.Text, _textBox.Hue, _font);
        return true;
    }

    void UpdateValue() {
        if (_textBox == null || _textBox.IsDisposed) return;
        KeepNumbers();
        int.TryParse(_textBox.Text, out int i);
        if (_upHeld) i += _up.ButtonID;
        else i += _down.ButtonID;
        ValidateValue(i);
    }

    void Commit() {
        if (IsDisposed || _textBox == null || _textBox.IsDisposed) return;
        KeepNumbers();
        int.TryParse(_textBox.Text, out int i);
        ValidateValue(i);
    }

    void ValidateValue(int val) {
        Tag = val = Math.Max(_Min, Math.Min(_Max, val));
        SetBox(val.ToString());
    }

    void SetBox(string value) {
        if (_textBox == null || _textBox.IsDisposed) return;
        _fromCode = true;
        _textBox.Text = value ?? "";
        _shown = _textBox.Text;
        _fromCode = false;
    }

    void KeepNumbers() {
        if (_fromCode || _textBox == null || _textBox.IsDisposed) return;
        var s = _textBox.Text;
        if (s == _shown) return;
        foreach (var ch in s)
            if (!char.IsNumber(ch)) {
                _textBox.Text = _shown;
                return;
            }
        _shown = s;
    }
}
