// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO FontStyle. The sink does not draw a border, a crop, or an alignment.</summary>
public enum FontStyle : ushort {
    None = 0x0000,
    Solid = 0x0001,
    Italic = 0x0002,
    Indention = 0x0004,
    BlackBorder = 0x0008,
    Underline = 0x0010,
    Fixed = 0x0020,
    Cropped = 0x0040,
    BQ = 0x0080,
    ExtraHeight = 0x0100,
    CropTexture = 0x0200,
}

/// <summary>ClassicUO TEXT_ALIGN_TYPE.</summary>
public enum TEXT_ALIGN_TYPE {
    TS_LEFT = 0,
    TS_CENTER,
    TS_RIGHT,
}

/// <summary>
/// ClassicUO HoveredLabel. Hue follows selected, then hover, then normal.
/// The text keeps the leading space. maxwidth only caps the hit width.
/// </summary>
public class HoveredLabel : Label {
    readonly ushort _overHue, _normalHue, _selectedHue;

    public HoveredLabel(
        string text,
        bool isunicode,
        ushort hue,
        ushort overHue,
        ushort selectedHue,
        int maxwidth = 0,
        byte font = 255,
        FontStyle style = FontStyle.None,
        TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT
    ) : base($" {text}", isunicode, hue, font) {
        _overHue = overHue;
        _normalHue = hue;
        _selectedHue = selectedHue;
        AcceptMouseInput = true;
        if (maxwidth > 0 && Width > maxwidth) Width = maxwidth;
    }

    public bool DrawBackgroundCurrentIndex;
    public bool IsSelected, ForceHover;

    public override void Update() {
        if (IsSelected) {
            if (Hue != _selectedHue) Hue = _selectedHue;
        }
        else if (UIManager.MouseOverControl == this || ForceHover) {
            if (Hue != _overHue) Hue = _overHue;
        }
        else if (Hue != _normalHue) Hue = _normalHue;
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (DrawBackgroundCurrentIndex && UIManager.MouseOverControl == this && !string.IsNullOrWhiteSpace(Text)) {
            var w = Width - 4;
            var h = Height - 4;
            if (w > 0 && h > 0)
                GumpArt.Sink?.DrawRaw(0x57200011, x, y + 2, w, h, Gray, 1, 1);
        }
        return base.Draw(x, y);
    }

    // Color.Gray, opaque. 128 maps to channel 16.
    static readonly byte[] Gray = { 0x10, 0xC2 };
}
