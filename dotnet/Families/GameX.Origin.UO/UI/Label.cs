// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Label without a font texture. Width is text length times 6, not the real glyph width.
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
            Width = _text.Length * 6;
            Height = 16;
        }
    }
}
