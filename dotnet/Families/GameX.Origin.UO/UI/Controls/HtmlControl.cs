// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO HtmlControl. The tag parser and the text runs are kept.
/// DrawText has no RGB channel, so a run whose color is the gump hue uses that hue.
/// Any other parsed color is drawn unhued. A link click does not open a browser.
/// </summary>
public class HtmlControl : Control {
    const int LineHtml = 18;
    const int LinePlain = 16;
    const int SpaceHtml = 8;

    static readonly byte[] HueTable = {
        0x00, 0x08, 0x10, 0x18, 0x20, 0x29, 0x31, 0x39, 0x41, 0x4A, 0x52, 0x5A, 0x62, 0x6A, 0x73, 0x7B,
        0x83, 0x8B, 0x94, 0x9C, 0xA4, 0xAC, 0xB4, 0xBD, 0xC5, 0xCD, 0xD5, 0xDE, 0xE6, 0xEE, 0xF6, 0xFF
    };
    static readonly HashSet<string> Visited = new HashSet<string>();

    readonly List<HtmlLine> _lines = new List<HtmlLine>();
    readonly List<HtmlLink> _links = new List<HtmlLink>();
    readonly List<string> _urls = new List<string>();
    string _text = "";
    bool _isHtml;
    byte _font = 1;
    bool _isUnicode = true;
    ushort _style;
    TEXT_ALIGN_TYPE _align = TEXT_ALIGN_TYPE.TS_LEFT;
    int _maxWidth;
    int _textHeight;
    int _marginLeft, _marginTop, _marginRight, _marginBottom;
    uint _baseColor;
    uint _linkColor = 0xFF0000FF;
    uint _visitedColor = 0x0000FFFF;
    uint _bgColor;
    bool _bgColored;
    byte[] _bgPx = { 0x00, 0x80 };
    uint _bgPxColor;
    ushort _drawHue;
    HtmlScroll _scroll;
    bool _chrome;
    int _wheel;
    bool _wheelReady;

    public HtmlControl(List<string> parts, string[] lines) : this() {
        X = int.Parse(parts[1]);
        Y = int.Parse(parts[2]);
        Width = int.Parse(parts[3]);
        Height = int.Parse(parts[4]);
        int textIndex = int.Parse(parts[5]);
        HasBackground = parts[6] == "1";
        HasScrollbar = parts[7] != "0";
        UseFlagScrollbar = HasScrollbar && parts[7] == "2";
        _isHtml = true;
        IsFromServer = true;
        string text = "";
        if (textIndex >= 0 && textIndex < lines.Length) text = lines[textIndex];
        InternalBuild(text, 0);
    }

    public HtmlControl(int x, int y, int w, int h, bool hasbackground, bool hasscrollbar, bool useflagscrollbar = false, string text = "", int hue = 0, bool ishtml = false, byte font = 1, bool isunicode = true, FontStyle style = FontStyle.None, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT) : this() {
        X = x;
        Y = y;
        Width = w;
        Height = h;
        HasBackground = hasbackground;
        HasScrollbar = hasscrollbar;
        UseFlagScrollbar = useflagscrollbar;
        if (!string.IsNullOrEmpty(text)) {
            _isHtml = ishtml;
            _style = (ushort)style;
            _align = align;
            _font = font;
            _isUnicode = isunicode;
        }
        InternalBuild(text, hue);
    }

    public HtmlControl() {
        CanMove = true;
        AcceptMouseInput = true;
    }

    public bool HasScrollbar { get; }
    public bool HasBackground { get; }
    public bool UseFlagScrollbar { get; }
    public bool IsFromServer { get; set; }
    public bool IsUnicode => _isUnicode;
    public int ScrollX { get; set; }
    public int ScrollY { get; set; }

    public string Text {
        get => _text;
        set {
            _text = value ?? "";
            Layout();
            FitScroll();
        }
    }

    public override void Update() {
        if (_scroll != null && Over()) Wheel();
        if (WantUpdateSize) {
            FitScroll();
            WantUpdateSize = false;
        }
        if (_scroll != null) ScrollY = _scroll.Value;
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        DrawBackground(x, y);
        if (!base.Draw(x, y)) return false;
        int offset = HasBackground ? 4 : 0;
        int view = ScrollY;
        for (int i = 0; i < _lines.Count; i++) {
            HtmlLine line = _lines[i];
            int contentY = offset + _marginTop + line.Y;
            if (contentY + line.H <= view) continue;
            if (contentY >= view + Height) break;
            int dx = x + offset + _marginLeft + line.Shift;
            int dy = y + contentY - view;
            var runs = line.Runs;
            for (int r = 0; r < runs.Count; r++) {
                HtmlRun run = runs[r];
                if (run.Text.Length == 0) continue;
                GumpArt.Sink?.DrawText(dx + run.X, dy, run.Text, HueOf(run.Color), run.Font);
            }
        }
        return true;
    }

    void DrawBackground(int x, int y) {
        if (!_bgColored || _bgColor == 0 || Width < 1 || Height < 1) return;
        if (_bgPxColor != _bgColor || _bgPx == null || _bgPx.Length != 2) {
            _bgPxColor = _bgColor;
            _bgPx = BgPixel(_bgColor);
        }
        int pad = HasBackground ? 4 : 0;
        int w = Width - pad * 2;
        int h = Height - pad * 2;
        if (w < 1 || h < 1) return;
        GumpArt.Sink?.DrawRaw(unchecked((int)(0x57210000 + (_bgColor & 0xFFFF))), x + pad, y + pad, w, h, _bgPx, 1, 1);
    }

    static byte[] BgPixel(uint color) {
        int b = (int)((color >> 24) & 0xFF);
        int g = (int)((color >> 16) & 0xFF);
        int r = (int)((color >> 8) & 0xFF);
        int r5 = r * 31 / 255;
        int g5 = g * 31 / 255;
        int b5 = b * 31 / 255;
        int c = 0x8000 | (r5 << 10) | (g5 << 5) | b5;
        return new byte[] { (byte)c, (byte)(c >> 8) };
    }

    public override void OnMouseClick(int x, int y) {
        if (_links.Count == 0) return;
        int lx = x - At(this, true);
        int ly = y - At(this, false) + ScrollY;
        for (int i = 0; i < _links.Count; i++) {
            HtmlLink link = _links[i];
            if (string.IsNullOrEmpty(link.Url)) continue;
            if (lx < link.X || lx >= link.X + link.W || ly < link.Y || ly >= link.Y + link.H) continue;
            if (Visited.Count < 1024) Visited.Add(link.Url);
            Layout();
            FitScroll();
            break;
        }
    }

    void InternalBuild(string text, int hue) {
        _text = text ?? "";
        _maxWidth = Width - (HasScrollbar ? 16 : 0) - (HasBackground ? 8 : 0);
        if (!string.IsNullOrEmpty(_text)) ApplyColor(hue);
        Layout();
        if (_chrome) {
            FitScroll();
            return;
        }
        _chrome = true;
        if (HasBackground) {
            Add(new ResizePic(0x2486) {
                Width = Width - (HasScrollbar ? 16 : 0),
                Height = Height,
                AcceptMouseInput = false
            });
        }
        _scroll = new HtmlScroll(UseFlagScrollbar) {
            X = Width - 14,
            Y = 0,
            Height = Height,
            IsVisible = HasScrollbar
        };
        Add(_scroll);
        FitScroll();
    }

    void ApplyColor(int hue) {
        if (!_isHtml) {
            _drawHue = unchecked((ushort)hue);
            _baseColor = 0;
            return;
        }
        uint html = 0xFFFFFFFF;
        ushort hue16 = 0;
        if (hue > 0) {
            if (hue == 0x00FFFFFF || hue == 0xFFFF || hue == 0xFF) html = 0xFFFFFFFE;
            else html = (Color16To32(unchecked((ushort)hue)) << 8) | 0xFF;
        }
        else if (!HasBackground) {
            hue16 = 0xFFFF;
            if (!HasScrollbar) html = 0x010101FF;
        }
        else {
            _maxWidth -= 9;
            html = 0x010101FF;
        }
        _baseColor = html;
        if (hue > 0 && hue != 0x00FFFFFF && hue != 0xFFFF && hue != 0xFF) _drawHue = unchecked((ushort)hue);
        else _drawHue = hue16;
    }

    void Layout() {
        _lines.Clear();
        _links.Clear();
        _urls.Clear();
        _textHeight = 0;
        _marginLeft = _marginTop = _marginRight = _marginBottom = 0;
        _bgColor = 0;
        _linkColor = 0xFF0000FF;
        _visitedColor = 0x0000FFFF;
        _bgColored = !HasBackground;
        if (string.IsNullOrEmpty(_text)) return;
        var chars = new List<HtmlChar>(_text.Length);
        if (_isHtml) Parse(_text, chars);
        else {
            for (int i = 0; i < _text.Length; i++) {
                chars.Add(new HtmlChar {
                    Char = _text[i],
                    Font = _font,
                    Align = _align,
                    Flags = _style,
                    Color = 0,
                    Link = 0
                });
            }
        }
        Wrap(chars);
    }

    void FitScroll() {
        if (_scroll == null) return;
        _scroll.Height = Height;
        _scroll.MinValue = 0;
        int max = _textHeight - Height + (HasBackground ? 8 : 0);
        _scroll.MaxValue = max > 0 ? max : 0;
        ScrollY = _scroll.Value;
    }

    void Wheel() {
        if (_scroll == null || !Mouse.HasSample) return;
        int w = Mouse.Current.Wheel;
        if (!_wheelReady) { _wheel = w; _wheelReady = true; return; }
        while (w >= _wheel + 120) { _wheel += 120; _scroll.Value -= _scroll.ScrollStep; }
        while (w <= _wheel - 120) { _wheel -= 120; _scroll.Value += _scroll.ScrollStep; }
    }

    bool Over() {
        for (var p = UIManager.MouseOverControl; p != null; p = p.Parent)
            if (p == this) return true;
        return false;
    }

    ushort HueOf(uint color) {
        if (!_isHtml) return _drawHue;
        if (color == 0 || color == _baseColor || color == 0xFFFFFFFF) return _drawHue;
        return 0;
    }

    void Wrap(List<HtmlChar> chars) {
        int max = _maxWidth;
        if (_isHtml && max > 0 && (_marginLeft != 0 || _marginRight != 0)) {
            max -= _marginLeft + _marginRight;
            if (max < 10) max = 10;
        }
        int lineH = _isHtml ? LineHtml : LinePlain;
        bool crop = (_style & (ushort)FontStyle.Cropped) != 0;
        bool wrap = max > 0 && !crop && (_style & (ushort)FontStyle.Fixed) == 0;
        var line = new List<HtmlChar>();
        int y = 0;
        int i = 0;
        while (i < chars.Count) {
            HtmlChar ch = chars[i];
            if (ch.Char == '\r' || ch.Char == '\0') { i++; continue; }
            if (ch.Char == '\n') {
                Emit(line, y, lineH, max);
                line.Clear();
                y += lineH;
                i++;
                continue;
            }
            int width = LineWidth(line);
            int cw = CharW(ch);
            if (wrap && line.Count > 0 && width + cw > max) {
                int br = LastSpace(line);
                if (br >= 0) {
                    int carry = line.Count - (br + 1);
                    List<HtmlChar> rest = carry > 0 ? line.GetRange(br + 1, carry) : null;
                    if (carry > 0) line.RemoveRange(br + 1, carry);
                    Emit(line, y, lineH, max);
                    line.Clear();
                    y += lineH;
                    if (rest != null) line.AddRange(rest);
                    continue;
                }
                Emit(line, y, lineH, max);
                line.Clear();
                y += lineH;
            }
            else if (crop && max > 0 && line.Count > 0 && width + cw > max) break;
            line.Add(ch);
            i++;
        }
        if (line.Count > 0) {
            Emit(line, y, lineH, max);
            y += lineH;
        }
        _textHeight = y + _marginTop + _marginBottom + (_isHtml ? 4 : 0);
    }

    void Emit(List<HtmlChar> src, int y, int lineH, int max) {
        var runs = new List<HtmlRun>();
        byte align = src.Count > 0 ? (byte)src[0].Align : (byte)_align;
        int indent = src.Count > 0 ? IndentOf(src[0]) : 0;
        int x = indent;
        int i = 0;
        while (i < src.Count) {
            HtmlChar a = src[i];
            var sb = new StringBuilder();
            int w = 0;
            while (i < src.Count && Same(src[i], a)) {
                sb.Append(src[i].Char);
                w += CharW(src[i]);
                i++;
            }
            runs.Add(new HtmlRun {
                Text = sb.ToString(),
                Font = a.Font,
                Flags = a.Flags,
                Color = a.Color,
                Link = a.Link,
                X = x,
                W = w
            });
            x += w;
        }
        int shift = Shift((TEXT_ALIGN_TYPE)align, x, max);
        int ox = (HasBackground ? 4 : 0) + _marginLeft + shift;
        int oy = (HasBackground ? 4 : 0) + _marginTop + y;
        for (int r = 0; r < runs.Count; r++) {
            HtmlRun run = runs[r];
            if (run.Link == 0 || run.Link > _urls.Count) continue;
            _links.Add(new HtmlLink {
                Url = _urls[run.Link - 1],
                X = ox + run.X,
                Y = oy,
                W = run.W < 1 ? 1 : run.W,
                H = lineH < 14 ? 14 : lineH
            });
        }
        _lines.Add(new HtmlLine { Runs = runs, Y = y, H = lineH, Align = align, Width = x, Shift = shift });
    }

    int LineWidth(List<HtmlChar> line) {
        if (line.Count == 0) return 0;
        int w = IndentOf(line[0]);
        for (int i = 0; i < line.Count; i++) w += CharW(line[i]);
        return w;
    }

    int CharW(HtmlChar ch) {
        int w = ch.Char == ' ' && _isHtml ? SpaceHtml : Measure(ch.Char.ToString(), ch.Font);
        if ((ch.Flags & (ushort)FontStyle.Solid) != 0) w++;
        return w;
    }

    static int IndentOf(HtmlChar ch) =>
        ch.Align == TEXT_ALIGN_TYPE.TS_LEFT && (ch.Flags & (ushort)FontStyle.Indention) != 0 ? 14 : 0;

    static int Shift(TEXT_ALIGN_TYPE align, int lineWidth, int max) {
        if (max <= 0) return 0;
        if (align == TEXT_ALIGN_TYPE.TS_CENTER) {
            int w = (max - 8) / 2 - lineWidth / 2;
            return w < 0 ? 0 : w;
        }
        if (align == TEXT_ALIGN_TYPE.TS_RIGHT) {
            int w = max - 10 - lineWidth;
            return w < 0 ? 0 : w;
        }
        return 0;
    }

    static int LastSpace(List<HtmlChar> line) {
        for (int i = line.Count - 1; i >= 0; i--)
            if (line[i].Char == ' ') return i;
        return -1;
    }

    static bool Same(HtmlChar a, HtmlChar b) =>
        a.Font == b.Font && a.Flags == b.Flags && a.Color == b.Color && a.Link == b.Link;

    static int Measure(string text, byte font) =>
        GumpArt.Sink?.MeasureText(text, font) ?? (string.IsNullOrEmpty(text) ? 0 : text.Length * 6);

    static int At(Control c, bool horizontal) {
        int n = 0;
        for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
        return n;
    }

    void Parse(string text, List<HtmlChar> chars) {
        var root = new HtmlInfo {
            Tag = HtmlTag.None,
            Align = _align,
            Flags = _style,
            Font = _font,
            Color = _baseColor,
            Link = 0
        };
        var stack = new List<HtmlInfo> { root };
        HtmlInfo current = root;
        for (int i = 0; i < text.Length; i++) {
            char si = text[i];
            if (si == '<') {
                bool endTag = false;
                var opened = new HtmlInfo {
                    Tag = HtmlTag.None,
                    Align = TEXT_ALIGN_TYPE.TS_LEFT,
                    Flags = 0,
                    Font = 0xFF,
                    Color = 0,
                    Link = 0
                };
                HtmlTag tag = ParseTag(text, text.Length, ref i, ref endTag, ref opened);
                if (tag == HtmlTag.None) continue;
                if (!endTag) {
                    if (opened.Font == 0xFF) opened.Font = stack[stack.Count - 1].Font;
                    if (tag != HtmlTag.Body) stack.Add(opened);
                    else {
                        chars.Clear();
                        if (opened.Color != 0) root.Color = opened.Color;
                        stack.Clear();
                        stack.Add(root);
                    }
                }
                else if (stack.Count > 1) {
                    for (int j = stack.Count - 1; j >= 1; j--) {
                        if (stack[j].Tag == tag) {
                            stack.RemoveAt(j);
                            break;
                        }
                    }
                }
                current = CurrentInfo(stack);
                switch (tag) {
                    case HtmlTag.Left:
                    case HtmlTag.Center:
                    case HtmlTag.Right:
                        if (chars.Count != 0) endTag = true;
                        goto case HtmlTag.P;
                    case HtmlTag.P:
                        si = endTag ? '\n' : '\0';
                        break;
                    case HtmlTag.BodyBg:
                    case HtmlTag.Br:
                    case HtmlTag.Bq:
                        si = '\n';
                        break;
                    default:
                        si = '\0';
                        break;
                }
            }
            if (si != 0) {
                chars.Add(new HtmlChar {
                    Char = si,
                    Font = current.Font == 0xFF ? _font : current.Font,
                    Align = current.Align,
                    Flags = current.Flags,
                    Color = current.Color,
                    Link = current.Link
                });
            }
        }
    }

    HtmlInfo CurrentInfo(List<HtmlInfo> list) {
        var info = new HtmlInfo {
            Tag = HtmlTag.None,
            Align = TEXT_ALIGN_TYPE.TS_LEFT,
            Flags = 0,
            Font = 0xFF,
            Color = 0,
            Link = 0
        };
        for (int i = 0; i < list.Count; i++) {
            HtmlInfo cur = list[i];
            switch (cur.Tag) {
                case HtmlTag.None:
                    info = cur;
                    break;
                case HtmlTag.B:
                case HtmlTag.I:
                case HtmlTag.U:
                case HtmlTag.P:
                    info.Flags |= cur.Flags;
                    info.Align = cur.Align;
                    break;
                case HtmlTag.A:
                    info.Flags |= cur.Flags;
                    info.Color = cur.Color;
                    info.Link = cur.Link;
                    break;
                case HtmlTag.Big:
                case HtmlTag.Small:
                    if (cur.Font != 0xFF) info.Font = cur.Font;
                    break;
                case HtmlTag.BaseFont:
                    if (cur.Font != 0xFF) info.Font = cur.Font;
                    if (cur.Color != 0) info.Color = cur.Color;
                    break;
                case HtmlTag.H1:
                case HtmlTag.H2:
                case HtmlTag.H4:
                case HtmlTag.H5:
                    info.Flags |= cur.Flags;
                    if (cur.Font != 0xFF) info.Font = cur.Font;
                    break;
                case HtmlTag.H3:
                case HtmlTag.H6:
                    if (cur.Font != 0xFF) info.Font = cur.Font;
                    break;
                case HtmlTag.Bq:
                    info.Color = cur.Color;
                    info.Flags |= cur.Flags;
                    break;
                case HtmlTag.Left:
                case HtmlTag.Center:
                case HtmlTag.Right:
                case HtmlTag.Div:
                    info.Align = cur.Align;
                    break;
            }
        }
        return info;
    }

    HtmlTag ParseTag(string str, int len, ref int i, ref bool endTag, ref HtmlInfo info) {
        HtmlTag tag = HtmlTag.None;
        i++;
        if (i < len && str[i] == '/') { endTag = true; i++; }
        while (i < len && str[i] == ' ') i++;
        int j = i;
        for (; i < len; i++) {
            if (str[i] == '/') { endTag = true; break; }
            if (str[i] == ' ' || str[i] == '>') break;
        }
        if (j != i && i < len) {
            int cmdLen = i - j;
            int start = j;
            j = i;
            while (i < len && str[i] != '>') i++;
            var span = str.AsSpan(start, cmdLen);
            if (SameText(span, "b")) tag = HtmlTag.B;
            else if (SameText(span, "i")) tag = HtmlTag.I;
            else if (SameText(span, "a")) tag = HtmlTag.A;
            else if (SameText(span, "u")) tag = HtmlTag.U;
            else if (SameText(span, "p")) tag = HtmlTag.P;
            else if (SameText(span, "big")) tag = HtmlTag.Big;
            else if (SameText(span, "small")) tag = HtmlTag.Small;
            else if (SameText(span, "body")) tag = HtmlTag.Body;
            else if (SameText(span, "basefont")) tag = HtmlTag.BaseFont;
            else if (SameText(span, "h1")) tag = HtmlTag.H1;
            else if (SameText(span, "h2")) tag = HtmlTag.H2;
            else if (SameText(span, "h3")) tag = HtmlTag.H3;
            else if (SameText(span, "h4")) tag = HtmlTag.H4;
            else if (SameText(span, "h5")) tag = HtmlTag.H5;
            else if (SameText(span, "h6")) tag = HtmlTag.H6;
            else if (SameText(span, "br")) tag = HtmlTag.Br;
            else if (SameText(span, "bq")) tag = HtmlTag.Bq;
            else if (SameText(span, "left")) tag = HtmlTag.Left;
            else if (SameText(span, "center")) tag = HtmlTag.Center;
            else if (SameText(span, "right")) tag = HtmlTag.Right;
            else if (SameText(span, "div")) tag = HtmlTag.Div;
            else {
                int at = IndexOf(str, "bodybgcolor");
                if (at >= 0) { tag = HtmlTag.BodyBg; j = IndexOf(str, "bgcolor"); endTag = false; }
                else if ((at = IndexOf(str, "basefont")) >= 0) { tag = HtmlTag.BaseFont; j = IndexOf(str, "color"); endTag = false; }
                else if ((at = IndexOf(str, "bodytext")) >= 0) { tag = HtmlTag.Body; j = IndexOf(str, "text"); endTag = false; }
            }
            if (!endTag) {
                info = InfoFromTag(tag);
                if (i < len && j != i) {
                    switch (tag) {
                        case HtmlTag.BodyBg:
                        case HtmlTag.Body:
                        case HtmlTag.BaseFont:
                        case HtmlTag.A:
                        case HtmlTag.Div:
                        case HtmlTag.P:
                            cmdLen = i - j;
                            if (cmdLen >= 0 && j >= 0 && j < str.Length && j + cmdLen <= str.Length)
                                InfoFromContent(ref info, str.AsSpan(j, cmdLen));
                            break;
                    }
                }
            }
        }
        return tag;
    }

    void InfoFromContent(ref HtmlInfo info, ReadOnlySpan<char> content) {
        while (!content.IsEmpty) {
            while (!content.IsEmpty && char.IsWhiteSpace(content[0])) content = content.Slice(1);
            if (content.IsEmpty) break;
            int i = 0;
            ReadOnlySpan<char> command = default;
            for (; i < content.Length; i++) {
                char c = content[i];
                if (char.IsWhiteSpace(c) || c == '=' || c == '\\') {
                    command = content.Slice(0, i);
                    content = content.Slice(i);
                    break;
                }
            }
            if (command.IsEmpty) break;
            while (!content.IsEmpty && char.IsWhiteSpace(content[0])) content = content.Slice(1);
            ReadOnlySpan<char> value = default;
            if (!content.IsEmpty && content[0] == '=') {
                content = content.Slice(1);
                while (!content.IsEmpty && char.IsWhiteSpace(content[0])) content = content.Slice(1);
                if (!content.IsEmpty && content[0] == '"') {
                    int end = content.Slice(1).IndexOf('"');
                    if (end >= 0) {
                        value = content.Slice(1, end);
                        content = content.Slice(end + 2);
                    }
                    else {
                        value = content.Length > 1 ? content.Slice(1) : default;
                        content = default;
                    }
                }
                else {
                    i = 0;
                    for (; i < content.Length; i++) {
                        char c = content[i];
                        if (char.IsWhiteSpace(c) || c == '\\' || c == '<' || c == '>' || c == '=') break;
                    }
                    value = content.Slice(0, i);
                    content = content.Slice(i);
                }
            }
            switch (info.Tag) {
                case HtmlTag.Body:
                case HtmlTag.BodyBg:
                    if (SameText(command, "text")) ReadColor(value, ref info.Color);
                    else if (SameText(command, "bgcolor")) { if (_bgColored) ReadColor(value, ref _bgColor); }
                    else if (SameText(command, "link")) ReadColor(value, ref _linkColor);
                    else if (SameText(command, "vlink")) ReadColor(value, ref _visitedColor);
                    else if (SameText(command, "leftmargin")) _marginLeft = int.Parse(value.ToString());
                    else if (SameText(command, "topmargin")) _marginTop = int.Parse(value.ToString());
                    else if (SameText(command, "rightmargin")) _marginRight = int.Parse(value.ToString());
                    else if (SameText(command, "bottommargin")) _marginBottom = int.Parse(value.ToString());
                    break;
                case HtmlTag.BaseFont:
                    if (SameText(command, "color")) ReadColor(value, ref info.Color);
                    else if (SameText(command, "size")) {
                        if (!byte.TryParse(value.ToString(), out byte font)) {
                            if (SameText(value, "big")) info.Font = 4;
                            else if (SameText(value, "small")) info.Font = 0;
                            else info.Font = 1;
                        }
                        else switch (font) {
                            case 0:
                            case 4: info.Font = 1; break;
                            case 1:
                            case 2:
                            case 3: info.Font = 2; break;
                            default: info.Font = 0; break;
                        }
                    }
                    break;
                case HtmlTag.A:
                    if (SameText(command, "href")) {
                        info.Flags = (ushort)FontStyle.Underline;
                        info.Color = _linkColor;
                        info.Link = RegisterUrl(value.ToString(), ref info.Color);
                    }
                    break;
                case HtmlTag.P:
                case HtmlTag.Div:
                    if (SameText(command, "align")) {
                        if (SameText(value, "left")) info.Align = TEXT_ALIGN_TYPE.TS_LEFT;
                        else if (SameText(value, "center")) info.Align = TEXT_ALIGN_TYPE.TS_CENTER;
                        else if (SameText(value, "right")) info.Align = TEXT_ALIGN_TYPE.TS_RIGHT;
                    }
                    break;
            }
            while (!content.IsEmpty && char.IsWhiteSpace(content[0])) content = content.Slice(1);
        }
    }

    ushort RegisterUrl(string url, ref uint color) {
        if (string.IsNullOrEmpty(url)) return 0;
        if (Visited.Contains(url)) color = _visitedColor;
        if (_urls.Count >= ushort.MaxValue) return 0;
        _urls.Add(url);
        return (ushort)_urls.Count;
    }

    static HtmlInfo InfoFromTag(HtmlTag tag) {
        var info = new HtmlInfo {
            Tag = tag,
            Align = TEXT_ALIGN_TYPE.TS_LEFT,
            Flags = 0,
            Font = 0xFF,
            Color = 0,
            Link = 0
        };
        switch (tag) {
            case HtmlTag.B: info.Flags = (ushort)FontStyle.Solid; break;
            case HtmlTag.I: info.Flags = (ushort)FontStyle.Italic; break;
            case HtmlTag.U: info.Flags = (ushort)FontStyle.Underline; break;
            case HtmlTag.P: info.Flags = (ushort)FontStyle.Indention; break;
            case HtmlTag.Big: info.Font = 0; break;
            case HtmlTag.Small: info.Font = 2; break;
            case HtmlTag.H1:
                info.Flags = (ushort)(FontStyle.Solid | FontStyle.Underline);
                info.Font = 0;
                break;
            case HtmlTag.H2:
                info.Flags = (ushort)FontStyle.Solid;
                info.Font = 0;
                break;
            case HtmlTag.H3: info.Font = 0; break;
            case HtmlTag.H4:
                info.Flags = (ushort)FontStyle.Solid;
                info.Font = 2;
                break;
            case HtmlTag.H5:
                info.Flags = (ushort)FontStyle.Italic;
                info.Font = 2;
                break;
            case HtmlTag.H6: info.Font = 2; break;
            case HtmlTag.Bq:
                info.Flags = (ushort)FontStyle.BQ;
                info.Color = 0x008000FF;
                break;
            case HtmlTag.Left: info.Align = TEXT_ALIGN_TYPE.TS_LEFT; break;
            case HtmlTag.Center: info.Align = TEXT_ALIGN_TYPE.TS_CENTER; break;
            case HtmlTag.Right: info.Align = TEXT_ALIGN_TYPE.TS_RIGHT; break;
        }
        return info;
    }

    static void ReadColor(ReadOnlySpan<char> buffer, ref uint color) {
        color = 0;
        if (buffer.IsEmpty) return;
        if (buffer[0] == '#') {
            if (buffer.Length <= 1) return;
            int start = buffer.Length > 2 && buffer[1] == '0' && buffer[2] == 'x' ? 3 : 1;
            if (start >= buffer.Length) return;
            if (!uint.TryParse(buffer.Slice(start).ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint cc)) return;
            byte b0 = (byte)cc;
            byte b1 = (byte)(cc >> 8);
            byte b2 = (byte)(cc >> 16);
            color = (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | 0xFF);
            return;
        }
        if (char.IsNumber(buffer[0])) {
            uint.TryParse(buffer.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color);
            return;
        }
        if (SameText(buffer, "red")) color = 0x0000FFFF;
        else if (SameText(buffer, "cyan")) color = 0xFFFF00FF;
        else if (SameText(buffer, "blue")) color = 0xFF0000FF;
        else if (SameText(buffer, "darkblue")) color = 0xA00000FF;
        else if (SameText(buffer, "lightblue")) color = 0xE6D8ADFF;
        else if (SameText(buffer, "purple")) color = 0x800080FF;
        else if (SameText(buffer, "yellow")) color = 0x00FFFFFF;
        else if (SameText(buffer, "lime")) color = 0x00FF00FF;
        else if (SameText(buffer, "magenta")) color = 0xFF00FFFF;
        else if (SameText(buffer, "white")) color = 0xFFFEFEFF;
        else if (SameText(buffer, "silver")) color = 0xC0C0C0FF;
        else if (SameText(buffer, "grey") || SameText(buffer, "gray")) color = 0x808080FF;
        else if (SameText(buffer, "black")) color = 0x010101FF;
        else if (SameText(buffer, "orange")) color = 0x00A5FFFF;
        else if (SameText(buffer, "brown")) color = 0x2A2AA5FF;
        else if (SameText(buffer, "maroon")) color = 0x000080FF;
        else if (SameText(buffer, "green")) color = 0x008000FF;
        else if (SameText(buffer, "olive")) color = 0x008080FF;
    }

    static bool SameText(ReadOnlySpan<char> span, string name) =>
        span.Equals(name.AsSpan(), StringComparison.InvariantCultureIgnoreCase);

    static int IndexOf(string str, string name) =>
        str.IndexOf(name, StringComparison.InvariantCultureIgnoreCase);

    static uint Color16To32(ushort c) =>
        (uint)(HueTable[(c >> 10) & 0x1F] | (HueTable[(c >> 5) & 0x1F] << 8) | (HueTable[c & 0x1F] << 16));

    enum HtmlTag : byte {
        None = 0,
        B, I, A, U, P, Big, Small, Body, BaseFont,
        H1, H2, H3, H4, H5, H6, Br, Bq, Left, Center, Right, Div, BodyBg
    }

    struct HtmlInfo {
        public HtmlTag Tag;
        public TEXT_ALIGN_TYPE Align;
        public ushort Flags;
        public byte Font;
        public uint Color;
        public ushort Link;
    }

    struct HtmlChar {
        public char Char;
        public byte Font;
        public TEXT_ALIGN_TYPE Align;
        public ushort Flags;
        public uint Color;
        public ushort Link;
    }

    struct HtmlRun {
        public string Text;
        public byte Font;
        public ushort Flags;
        public uint Color;
        public ushort Link;
        public int X, W;
    }

    struct HtmlLink {
        public string Url;
        public int X, Y, W, H;
    }

    sealed class HtmlLine {
        public List<HtmlRun> Runs = new List<HtmlRun>();
        public int Y, H, Width, Shift;
        public byte Align;
    }

    /// <summary>Scroll gumps 250-257, or the 0x0828 flag when the gump asks for one.</summary>
    sealed class HtmlScroll : Control {
        readonly bool _flag;
        bool _drag, _heldBefore, _upHeld, _downHeld;
        int _value, _min, _max;

        public HtmlScroll(bool flag) {
            _flag = flag;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            ushort graphic = flag ? (ushort)0x0828 : (ushort)257;
            Width = 16;
            if (GumpMetrics.TrySize(graphic, out int w, out _) && w > 0) Width = w;
            ScrollStep = 50;
        }

        public int ScrollStep { get; set; }

        public int MinValue {
            get => _min;
            set {
                _min = value;
                if (_value < _min) _value = _min;
            }
        }

        public int MaxValue {
            get => _max;
            set {
                _max = value < 0 ? 0 : value;
                if (_value > _max) _value = _max;
                if (_value < _min) _value = _min;
            }
        }

        public int Value {
            get => _value;
            set {
                if (value < _min) value = _min;
                else if (value > _max) value = _max;
                _value = value;
            }
        }

        public override void Update() {
            base.Update();
            bool held = UIManager.LastControlMouseDown(1) == this && Mouse.LButtonPressed;
            int local = Mouse.Position.Y - At(this, false);
            if (held && !_heldBefore) _drag = _flag || (local >= UpH() && local < Height - DownH());
            if (!held) _drag = false;
            _heldBefore = held;
            if (held && _drag) Apply(local);
            _upHeld = held && !_flag && local < UpH();
            _downHeld = held && !_flag && local >= Height - DownH();
        }

        public override void OnMouseClick(int x, int y) {
            int local = y - At(this, false);
            if (_flag) { Apply(local); return; }
            if (local < UpH()) { Value -= 1; return; }
            if (local >= Height - DownH()) { Value += 1; return; }
            Apply(local);
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible || Height <= 0) return false;
            if (_flag) {
                GumpArtDraw.Pic(0x0828, x, y + SliderY(), 0, 0, false);
                return true;
            }
            Sz up = Size(251);
            Sz down = Size(253);
            Sz top = Size(257);
            Sz mid = Size(256);
            Sz bot = Size(255);
            Sz sl = Size(254);
            int middle = Height - up.H - down.H - top.H - bot.H;
            if (middle > 0) {
                GumpArtDraw.Pic(257, x, y + up.H, top.W, top.H, false);
                GumpArtDraw.Pic(256, x, y + up.H + top.H, top.W > 0 ? top.W : mid.W, middle, true);
                GumpArtDraw.Pic(255, x, y + Height - down.H - bot.H, bot.W, bot.H, false);
            }
            else {
                int mh = Height - up.H - down.H;
                if (mh > 0) GumpArtDraw.Pic(256, x, y + up.H, mid.W > 0 ? mid.W : Width, mh, true);
            }
            GumpArtDraw.Pic(_upHeld ? (ushort)250 : (ushort)251, x, y, 0, 0, false);
            GumpArtDraw.Pic(_downHeld ? (ushort)252 : (ushort)253, x, y + Height - down.H, 0, 0, false);
            if (MaxValue > MinValue) {
                int sx = top.W > sl.W ? (top.W - sl.W) >> 1 : 0;
                GumpArtDraw.Pic(254, x + sx, y + up.H + SliderY(), 0, 0, false);
            }
            return true;
        }

        void Apply(int localY) {
            int up = UpH();
            int down = DownH();
            int slider = SliderH();
            int area = Height - up - down - slider;
            if (area < 1) area = 1;
            int y = localY - up - (slider >> 1);
            if (y < 0) y = 0;
            if (y > area) y = area;
            int span = MaxValue - MinValue;
            Value = span <= 0 ? MinValue : (int)Math.Round(y / (float)area * span) + MinValue;
        }

        int SliderY() {
            int span = MaxValue - MinValue;
            if (span <= 0) return 0;
            int area = Height - UpH() - DownH() - SliderH();
            if (area < 1) return 0;
            return (int)Math.Round(area * ((Value - MinValue) / (float)span));
        }

        int UpH() => _flag ? 0 : Size(251).H;
        int DownH() => _flag ? 0 : Size(253).H;
        int SliderH() => _flag ? Size(0x0828).H : Size(254).H;

        static Sz Size(ushort graphic) {
            GumpMetrics.TrySize(graphic, out int w, out int h);
            return new Sz { W = w, H = h };
        }

        struct Sz { public int W, H; }
    }
}
