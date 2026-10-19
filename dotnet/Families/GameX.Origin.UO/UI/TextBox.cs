// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// Account and password field. Not ClassicUO's StbTextBox: no caret, no glyph texture.
/// Draw asks GumpArt.Sink.DrawText. A password stores the real characters and draws stars.
/// </summary>
public sealed class TextBox : Control {
    string _text = "";

    public TextBox() {
        AcceptKeyboardInput = true;
        AcceptMouseInput = true;
    }

    public bool IsPassword { get; set; }
    public ushort Hue { get; set; }
    public int MaxLength { get; set; } = 16;

    public string Text {
        get => _text;
        set {
            var s = value ?? "";
            if (MaxLength >= 0 && s.Length > MaxLength) s = s[..MaxLength];
            _text = s;
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArt.Sink?.DrawText(x, y, IsPassword ? new string('*', Text.Length) : Text, Hue);
        return base.Draw(x, y);
    }
}
