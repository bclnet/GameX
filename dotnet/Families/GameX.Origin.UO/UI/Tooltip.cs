// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;
using System.Text;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Tooltip. Properties come from UOLoginNet.TryProperties. A miss asks RequestProperties.
/// There is no HTML font, so the name and the data are drawn as text. An item name uses hue 0x0035.
/// A mobile name uses Notoriety.GetHue. BGRA1555 has no partial alpha, so the black fill is opaque.
/// Profile path is not ported. Font is 1, text hue is 0xFFFF, delay is 250, zoom is 100, opacity is 70.
/// The delay reads Time.Ticks.
/// </summary>
public class Tooltip {
    const int FillKey = 0x57400007;
    const int FrameKey = 0x57400008;
    const int MaxWidthDefault = 600;

    // Profile path is not ported. Defaults match Profile.cs.
    static readonly bool UseTooltip = true;
    static readonly byte TooltipFont = 1;
    static readonly ushort TooltipTextHue = 0xFFFF;
    static readonly int TooltipDelay = 250;
    static readonly int TooltipZoom = 100;

    static readonly byte[] BlackPx = { 0x00, 0x80 };
    static readonly byte[] GrayPx = { 0x10, 0xC2 };

    uint _hash;
    uint _lastHoverTime;
    int _maxWidth;
    string _textHTML;
    ushort _hue = 0xFFFF;
    readonly World _world;
    readonly List<string> _lines = [];
    string _lineKey;

    public Tooltip(World world) {
        _world = world;
        _hash = 0;
        _lastHoverTime = 0;
        _maxWidth = 0;
        _textHTML = null;
        Text = null;
        Serial = 0;
    }

    public string Text { get; protected set; }

    public bool IsEmpty => Text == null;

    public uint Serial { get; private set; }

    public bool Draw(int x, int y) {
        if (_world == null) return false;
        if (SerialHelper.IsValid(Serial)) {
            var revision = Revision(Serial);
            if (_hash != revision) {
                _hash = revision;
                Text = ReadProperties(Serial, out _textHTML);
            }
        }
        var shown = string.IsNullOrEmpty(Text) ? _textHTML : Text;
        if (string.IsNullOrEmpty(shown)) return false;
        if (_lastHoverTime > Time.Ticks) return false;
        // Profile path is not ported. Opacity 70 is opaque.
        EnsureLines(shown);
        if (_lines.Count == 0) return false;
        var textW = 0;
        for (var i = 0; i < _lines.Count; i++) {
            var w = Measure(_lines[i], TooltipFont);
            if (w > textW) textW = w;
        }
        var textH = _lines.Count * 16;
        var zWidth = textW + 8;
        var zHeight = textH + 8;
        var boxW = zWidth * TooltipZoom / 100;
        var boxH = zHeight * TooltipZoom / 100;
        if (boxW < 1) boxW = 1;
        if (boxH < 1) boxH = 1;
        Screen(out var sw, out var sh);
        if (x < 0) x = 0;
        else if (x > sw - boxW) x = sw - boxW;
        if (y < 0) y = 0;
        else if (y > sh - boxH) y = sh - boxH;
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        var sink = GumpArt.Sink;
        if (sink != null) {
            sink.DrawRaw(FillKey, x - 4, y - 2, boxW, boxH, BlackPx, 1, 1);
            Frame(sink, x - 4, y - 2, boxW, boxH);
            for (var i = 0; i < _lines.Count; i++)
                sink.DrawText(x + 3, y + 3 + i * 16, _lines[i], _hue, TooltipFont);
        }
        return true;
    }

    public void Clear() {
        Serial = 0;
        _hash = 0;
        _textHTML = Text = null;
        _maxWidth = 0;
        _hue = TooltipTextHue;
        _lines.Clear();
        _lineKey = null;
    }

    public void SetGameObject(uint serial) {
        if (_world == null) return;
        if (Serial == 0 || serial != Serial) {
            var revision2 = Revision(serial);
            var revision = Serial == 0 ? 0 : Revision(Serial);
            if (Serial == 0 || Serial != serial || revision != revision2) {
                _maxWidth = 0;
                Serial = serial;
                _hash = revision2;
                Text = ReadProperties(serial, out _textHTML);
                _lastHoverTime = Time.Ticks + (uint)TooltipDelay;
            }
        }
    }

    public void SetText(string text, int maxWidth = 0) {
        // Profile path is not ported. UseTooltip defaults to true.
        if (!UseTooltip) return;
        _maxWidth = maxWidth;
        Serial = 0;
        _hue = TooltipTextHue;
        Text = _textHTML = text;
        _lastHoverTime = Time.Ticks + (uint)TooltipDelay;
    }

    string ReadProperties(uint serial, out string htmltext) {
        htmltext = "";
        _hue = TooltipTextHue;
        if (_world == null || !SerialHelper.IsValid(serial)) return null;
        if (!UOLoginNet.TryProperties(serial, out var name, out var data)) {
            UOLoginNet.RequestProperties(serial);
            return null;
        }
        var plain = new StringBuilder();
        var html = new StringBuilder();
        var hasColor = false;
        if (!string.IsNullOrEmpty(name)) {
            if (SerialHelper.IsItem(serial)) {
                html.Append("<basefont color=\"yellow\">");
                hasColor = true;
                _hue = 0x0035;
            }
            else {
                var flag = (NotorietyFlag)UOLoginNet.MobileNotoriety(serial);
                var tag = Notoriety.GetHTMLHue(flag);
                if (!string.IsNullOrEmpty(tag)) {
                    html.Append(tag);
                    hasColor = true;
                }
                _hue = Notoriety.GetHue(flag);
            }
            plain.Append(name);
            html.Append(name);
            if (hasColor) html.Append("<basefont color=\"#FFFFFFFF\">");
        }
        if (!string.IsNullOrEmpty(data)) {
            if (plain.Length > 0) {
                plain.Append('\n');
                html.Append('\n');
            }
            plain.Append(data);
            html.Append(data);
        }
        htmltext = html.ToString();
        var result = plain.ToString();
        return result.Length == 0 ? null : result;
    }

    void EnsureLines(string shown) {
        var cap = _maxWidth > 0 ? _maxWidth : MaxWidthDefault;
        var key = shown + "\n" + cap;
        if (key == _lineKey) return;
        _lineKey = key;
        _lines.Clear();
        var parts = shown.Replace('\r', '\n').Split('\n');
        for (var p = 0; p < parts.Length; p++) Wrap(parts[p], cap);
        if (_lines.Count == 0) _lines.Add("");
    }

    void Wrap(string para, int cap) {
        if (string.IsNullOrEmpty(para)) {
            _lines.Add("");
            return;
        }
        if (Measure(para, TooltipFont) <= cap) {
            _lines.Add(para);
            return;
        }
        var cur = "";
        var words = para.Split(' ');
        for (var i = 0; i < words.Length; i++) {
            var word = words[i];
            var next = cur.Length == 0 ? word : cur + " " + word;
            if (cur.Length > 0 && Measure(next, TooltipFont) > cap) {
                _lines.Add(cur);
                cur = word;
            }
            else cur = next;
        }
        if (cur.Length > 0) _lines.Add(cur);
    }

    static uint Revision(uint serial) {
        if (!UOLoginNet.TryProperties(serial, out var name, out var data)) return 0;
        unchecked {
            uint h = 2166136261;
            h = Mix(h, name);
            h = Mix(h, data);
            return h == 0 ? 1 : h;
        }
    }

    static uint Mix(uint h, string s) {
        if (string.IsNullOrEmpty(s)) return h;
        for (var i = 0; i < s.Length; i++) h = (h ^ s[i]) * 16777619;
        return h;
    }

    static int Measure(string text, byte font) {
        if (string.IsNullOrEmpty(text)) return 0;
        return GumpArt.Sink?.MeasureText(text, font) ?? text.Length * 6;
    }

    static void Screen(out int w, out int h) {
        var s = Mouse.Current;
        w = s.BufW > 0 ? s.BufW : 640;
        h = s.BufH > 0 ? s.BufH : 480;
    }

    static void Frame(IGumpArt sink, int x, int y, int w, int h) {
        if (w < 1 || h < 1) return;
        sink.DrawRaw(FrameKey, x, y, w, 1, GrayPx, 1, 1);
        if (h > 1) sink.DrawRaw(FrameKey, x, y + h - 1, w, 1, GrayPx, 1, 1);
        sink.DrawRaw(FrameKey, x, y, 1, h, GrayPx, 1, 1);
        if (w > 1) sink.DrawRaw(FrameKey, x + w - 1, y, 1, h, GrayPx, 1, 1);
    }
}
