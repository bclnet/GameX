// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Label without a font texture. Width is MeasureText when a sink is attached.
/// </summary>
public class Label : Control {
    string _text;

    public Label(string text, bool isunicode, ushort hue, byte font = 0) {
        IsUnicode = isunicode;
        Hue = hue;
        Font = font;
        AcceptMouseInput = false;
        Text = text;
    }

    public bool IsUnicode { get; }
    public ushort Hue { get; set; }
    public byte Font { get; }

    public string Text {
        get => _text;
        set {
            _text = value ?? "";
            Width = GumpArt.Sink?.MeasureText(_text, Font) ?? _text.Length * 6;
            Height = 16;
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArt.Sink?.DrawText(x, y, Text, Hue, Font);
        return base.Draw(x, y);
    }
}
