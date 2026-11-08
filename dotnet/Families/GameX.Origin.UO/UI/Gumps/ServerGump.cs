// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameX.Origin.UO;

/// <summary>
/// Generic server gump from 0xB0 or 0xDD. Commands use the ClassicUO split: braces, then space and comma.
/// HTML is a label. Tile pictures, checker translucency, and tooltips are skipped.
/// A button sends 0xB1. A page button only changes the page. Right-click sends button 0 when close is allowed.
/// </summary>
public class ServerGump : Gump {
    bool _replied;
    TextBox _firstText;

    public ServerGump(World world, UOServerGump src) : base(world, src.Sender, src.Id) {
        X = src.X;
        Y = src.Y;
        CanMove = true;
        CanCloseWithRightClick = true;
        CanCloseWithEsc = true;
        IsFromServer = true;
        AcceptMouseInput = true;
        Build(src.Layout, src.Lines ?? Array.Empty<string>());
        _firstText?.SetKeyboardFocus();
    }

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        if (ServerSerial != 0) OnButtonClick(0);
        if (!IsDisposed) Dispose();
    }

    public override void OnButtonClick(int buttonID) {
        if (IsDisposed || _replied || LocalSerial == 0) return;
        _replied = true;
        var switches = new List<uint>();
        var entries = new List<(ushort id, string text)>();
        foreach (var c in Children) {
            if (c is Checkbox box && box.IsChecked) switches.Add(box.LocalSerial);
            else if (c is TextBox tb) entries.Add(((ushort)tb.LocalSerial, tb.Text ?? ""));
        }
        UOLoginNet.ReplyGump(LocalSerial, ServerSerial, buttonID, switches.ToArray(), entries.ToArray());
        Dispose();
    }

    /// <summary>0xBF subcommand 4. Button 0 disposes. Any other button replies, then disposes.</summary>
    public void ServerClose(int button) {
        if (IsDisposed) return;
        if (button != 0) OnButtonClick(button);
        if (!IsDisposed) Dispose();
    }

    void Build(string layout, string[] lines) {
        var commands = Tokens(layout, new[] { ' ' }, '{', '}');
        var page = 0;
        var group = 0;
        foreach (var command in commands) {
            var parts = Tokens(command, new[] { ' ', ',' }, '@', '@');
            if (parts.Count == 0) continue;
            var name = parts[0];
            if (Eq(name, "page")) {
                if (TryInt(parts, 1, out var next)) page = next;
            }
            else if (Eq(name, "resizepic")) AddResize(parts, page);
            else if (Eq(name, "gumppic") || Eq(name, "tilepicasgumppic")) AddPic(parts, page);
            else if (Eq(name, "gumppictiled")) AddTiled(parts, page);
            else if (Eq(name, "button") || Eq(name, "buttontileart")) AddButton(parts, page);
            else if (Eq(name, "text")) AddText(parts, lines, page);
            else if (Eq(name, "croppedtext")) AddCropped(parts, lines, page);
            else if (Eq(name, "htmlgump")) AddHtml(parts, lines, page, 0);
            else if (Eq(name, "xmfhtmlgump")) AddXmf(parts, page, colorAt: -1, clilocAt: 5, argAt: -1);
            else if (Eq(name, "xmfhtmlgumpcolor")) AddXmf(parts, page, colorAt: 8, clilocAt: 5, argAt: -1);
            else if (Eq(name, "xmfhtmltok")) AddXmf(parts, page, colorAt: 7, clilocAt: 8, argAt: 9);
            else if (Eq(name, "checkbox")) AddCheck(parts, page, group, radio: false);
            else if (Eq(name, "radio")) AddCheck(parts, page, group, radio: true);
            else if (Eq(name, "textentry") || Eq(name, "textentrylimited")) AddEntry(parts, lines, page, Eq(name, "textentrylimited"));
            else if (Eq(name, "noclose")) CanCloseWithRightClick = false;
            else if (Eq(name, "nodispose")) CanCloseWithEsc = false;
            else if (Eq(name, "nomove")) CanMove = false;
            else if (Eq(name, "group") || Eq(name, "endgroup")) group++;
        }
    }

    void AddResize(List<string> parts, int page) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y) || !TryU16(parts, 3, out var graphic)) return;
        if (!TryInt(parts, 4, out var w) || !TryInt(parts, 5, out var h)) return;
        var pic = new ResizePic(graphic) { X = x, Y = y, Width = w, Height = h };
        Add(pic, page);
    }

    void AddPic(List<string> parts, int page) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y) || !TryU16(parts, 3, out var graphic)) return;
        var hue = parts.Count > 4 ? HuePlus(parts[4]) : (ushort)0;
        if (hue <= 2) hue = 0;
        var pic = new GumpPic(x, y, graphic, hue);
        if (!pic.IsDisposed) Add(pic, page);
    }

    void AddTiled(List<string> parts, int page) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y)) return;
        if (!TryInt(parts, 3, out var w) || !TryInt(parts, 4, out var h) || !TryU16(parts, 5, out var graphic)) return;
        var pic = new GumpPicTiled(x, y, w, h, graphic);
        if (!pic.IsDisposed) Add(pic, page);
    }

    void AddButton(List<string> parts, int page) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y)) return;
        if (!TryU16(parts, 3, out var normal) || !TryU16(parts, 4, out var pressed)) return;
        var id = TryInt(parts, 7, out var buttonId) ? buttonId : 0;
        var button = new Button(id, normal, pressed) { X = x, Y = y };
        if (button.IsDisposed) return;
        var activate = TryInt(parts, 5, out var action) && action != 0;
        button.IsPageButton = !activate;
        if (TryInt(parts, 6, out var to)) button.ToPage = to;
        Add(button, page);
    }

    void AddText(List<string> parts, string[] lines, int page) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y) || parts.Count < 5) return;
        Add(new Label(LineAt(lines, parts[4]), true, HuePlus(parts[3])) { X = x, Y = y }, page);
    }

    void AddCropped(List<string> parts, string[] lines, int page) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y) || parts.Count < 7) return;
        Add(new Label(LineAt(lines, parts[6]), true, HuePlus(parts[5])) { X = x, Y = y }, page);
    }

    void AddHtml(List<string> parts, string[] lines, int page, int hue) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y) || parts.Count < 6) return;
        var text = Plain(LineAt(lines, parts[5]));
        if (text.Length == 0) return;
        Add(new Label(text, true, (ushort)hue) { X = x, Y = y }, page);
    }

    void AddXmf(List<string> parts, int page, int colorAt, int clilocAt, int argAt) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y) || parts.Count <= clilocAt) return;
        var idText = parts[clilocAt];
        if (idText.Length > 0 && idText[0] == '#') idText = idText.Substring(1);
        if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)) return;
        string args = null;
        if (argAt >= 0 && parts.Count > argAt) {
            var sb = new StringBuilder();
            for (var i = argAt; i < parts.Count; i++) {
                sb.Append('\t');
                sb.Append(parts[i]);
            }
            args = sb.ToString().Trim('@').Replace('@', '\t');
        }
        var hue = 0;
        if (colorAt >= 0 && colorAt < parts.Count && int.TryParse(parts[colorAt], NumberStyles.Integer, CultureInfo.InvariantCulture, out var color) && color != 0x7FFF && color >= 0 && color <= 0xFFFF)
            hue = color;
        var text = Plain(UOGamePackets.ClilocText(id, args));
        if (text.Length == 0) return;
        Add(new Label(text, true, (ushort)hue) { X = x, Y = y }, page);
    }

    void AddCheck(List<string> parts, int page, int group, bool radio) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y)) return;
        if (!TryU16(parts, 3, out var inactive) || !TryU16(parts, 4, out var active) || parts.Count < 7) return;
        var box = new ServerCheck(inactive, active) { X = x, Y = y, Group = group, Radio = radio };
        if (box.IsDisposed) return;
        box.IsChecked = parts[5] == "1";
        box.LocalSerial = ParseSerial(parts[6]);
        if (radio) box.ValueChanged += OnRadio;
        Add(box, page);
    }

    void AddEntry(List<string> parts, string[] lines, int page, bool limited) {
        if (!TryInt(parts, 1, out var x) || !TryInt(parts, 2, out var y)) return;
        if (!TryInt(parts, 3, out var w) || !TryInt(parts, 4, out var h) || parts.Count < 8) return;
        var max = 255;
        if (limited && TryInt(parts, 8, out var limit) && limit >= 0) max = limit;
        var box = new TextBox {
            X = x,
            Y = y,
            Width = w,
            Height = h,
            Hue = HuePlus(parts[5]),
            MaxLength = max,
            LocalSerial = ParseSerial(parts[6]),
        };
        box.Text = LineAt(lines, parts[7]);
        if (_firstText == null) _firstText = box;
        Add(box, page);
    }

    void OnRadio(object sender, EventArgs e) {
        if (sender is not ServerCheck box || !box.Radio || !box.IsChecked) return;
        foreach (var c in Children)
            if (c is ServerCheck other && other != box && other.Radio && other.Group == box.Group && other.IsChecked)
                other.IsChecked = false;
    }

    static string LineAt(string[] lines, string index) {
        if (lines == null || !int.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return "";
        if (n < 0 || n >= lines.Length || lines[n] == null) return "";
        return lines[n];
    }

    static string Plain(string text) {
        if (string.IsNullOrEmpty(text)) return "";
        var sb = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++) {
            var c = text[i];
            if (c == '<') {
                var end = text.IndexOf('>', i + 1);
                if (end < 0) break;
                i = end;
                continue;
            }
            if (c == '&') {
                var end = text.IndexOf(';', i + 1);
                if (end > i && end - i < 10) {
                    var ent = text.Substring(i + 1, end - i - 1);
                    if (ent == "nbsp") sb.Append(' ');
                    else if (ent == "lt") sb.Append('<');
                    else if (ent == "gt") sb.Append('>');
                    else if (ent == "amp") sb.Append('&');
                    else sb.Append('&').Append(ent).Append(';');
                    i = end;
                    continue;
                }
            }
            sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    static ushort HuePlus(string s) {
        if (string.IsNullOrEmpty(s)) return 0;
        var eq = s.IndexOf('=');
        if (eq >= 0) s = s.Substring(eq + 1);
        if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
            && !(s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out n)))
            return 0;
        if (n < 0 || n > 0xFFFF) return 0;
        return (ushort)(n + 1);
    }

    static uint ParseSerial(string s) {
        if (string.IsNullOrEmpty(s)) return 0;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return uint.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex) ? hex : 0;
        return uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    static bool TryInt(List<string> parts, int index, out int value) {
        value = 0;
        if (index >= parts.Count) return false;
        var s = parts[index];
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return true;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        return false;
    }

    static bool TryU16(List<string> parts, int index, out ushort value) {
        value = 0;
        if (!TryInt(parts, index, out var n) || n < 0 || n > 0xFFFF) return false;
        value = (ushort)n;
        return true;
    }

    static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    static List<string> Tokens(string str, char[] delims, char q0, char q1) {
        var result = new List<string>();
        if (string.IsNullOrEmpty(str)) return result;
        var pos = 0;
        while (pos < str.Length) {
            var eol = str.IndexOf('\n', pos);
            if (eol < 0) eol = str.Length;
            while (pos < eol && IsDelim(str[pos], delims)) pos++;
            while (pos < eol) {
                var sb = new StringBuilder();
                if (str[pos] == q0) {
                    var same = q0 == q1;
                    pos++;
                    while (pos < eol) {
                        if (str[pos] == q1) {
                            if (!same) pos++;
                            break;
                        }
                        sb.Append(str[pos]);
                        pos++;
                    }
                }
                else {
                    while (pos < eol && !IsDelim(str[pos], delims) && str[pos] != q0)
                        sb.Append(str[pos++]);
                }
                if (sb.Length > 0) {
                    var token = sb.ToString().Trim();
                    if (token.Length > 0) result.Add(token);
                }
                while (pos < eol && IsDelim(str[pos], delims)) pos++;
            }
            pos = eol + 1;
        }
        return result;
    }

    static bool IsDelim(char c, char[] delims) {
        foreach (var d in delims)
            if (c == d) return true;
        return false;
    }

    sealed class ServerCheck : Checkbox {
        public ServerCheck(ushort inactive, ushort active) : base(inactive, active) { }
        public int Group { get; set; }
        public bool Radio { get; set; }
    }
}
