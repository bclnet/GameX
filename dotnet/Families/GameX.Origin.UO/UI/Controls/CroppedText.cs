// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System.Collections.Generic;
using System.Globalization;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CroppedText. Font is 1 from client 3.0.5d, else 0. DrawText has no black border.
/// </summary>
public class CroppedText : Control {
    readonly string _text;
    readonly ushort _hue;
    readonly byte _font;
    readonly int _maxWidth;

    public CroppedText(string text, ushort hue, int maxWidth = 0) {
        _text = text ?? "";
        _hue = hue;
        _maxWidth = maxWidth;
        var ver = UltimaOnline.Current?.Version ?? default;
        _font = ver >= ClientVersion.CV_305D ? (byte)1 : (byte)0;
        AcceptMouseInput = false;
        var shown = Fit();
        Width = GumpArt.Sink?.MeasureText(shown, _font) ?? shown.Length * 6;
        Height = 16;
    }

    public CroppedText(List<string> parts, string[] lines) : this(Line(parts, lines), (ushort)(ParseUShort(parts[5]) + 1), int.Parse(parts[3])) {
        X = int.Parse(parts[1]);
        Y = int.Parse(parts[2]);
        Width = int.Parse(parts[3]);
        Height = int.Parse(parts[4]);
        IsFromServer = true;
    }

    public bool IsUnicode => true;
    public bool IsFromServer { get; set; }
    public string Text => _text;
    public ushort Hue => _hue;
    public byte Font => _font;
    public int MaxWidth => _maxWidth;

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var shown = Fit();
        if (shown.Length > 0) GumpArt.Sink?.DrawText(x, y, shown, _hue, _font);
        return base.Draw(x, y);
    }

    public override void Dispose() => base.Dispose();

    string Fit() {
        var text = _text;
        if (_maxWidth <= 0 || text.Length == 0) return text;
        var sink = GumpArt.Sink;
        int WidthOf(string s) => sink?.MeasureText(s, _font) ?? s.Length * 6;
        if (WidthOf(text) <= _maxWidth) return text;
        var n = text.Length;
        while (n > 0 && WidthOf(text.Substring(0, n)) > _maxWidth) n--;
        return n == text.Length ? text : text.Substring(0, n);
    }

    static string Line(List<string> parts, string[] lines) {
        if (!int.TryParse(parts[6], out var lineIndex) || lineIndex < 0 || lines == null || lineIndex >= lines.Length) return "";
        return lines[lineIndex] ?? "";
    }

    static ushort ParseUShort(string str) {
        if (string.IsNullOrEmpty(str)) return 0;
        if (str.StartsWith("0x")) {
            uint.TryParse(str.Substring(2), NumberStyles.HexNumber, null, out var hex);
            return (ushort)hex;
        }
        if (str.Length > 1 && str[0] == '-' && short.TryParse(str, out var neg)) return (ushort)neg;
        uint.TryParse(str, out var v);
        return (ushort)v;
    }
}
