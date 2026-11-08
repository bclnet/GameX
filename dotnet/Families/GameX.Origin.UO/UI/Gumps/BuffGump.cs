// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO BuffGump. Icons are UOLoginNet.CopyBuffs. One gump, not one per icon.
/// Background starts at 0x7580. The button is 0x7585 / 0x7589 and cycles 0x757F through 0x7582.
/// Opened at 100, 100, which is where ClassicUO adds it from the status button.
/// There is no tooltip here. The buff text is a line under the bar. Timer text stays off, matching BuffBarTime.
/// </summary>
public sealed class BuffGump : Gump {
    static readonly List<UOBuffState> Copy = [];

    readonly GumpPic _back;
    readonly Button _button;
    readonly List<Control> _icons = [];
    readonly List<UOBuffState> _shown = [];
    Dir _direction;
    ushort _graphic;

    public BuffGump(World world) : base(world, 0, 0) {
        X = 100;
        Y = 100;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        _direction = Dir.LeftHorizontal;
        _graphic = 0x7580;
        _back = new GumpPic(0, 0, _graphic, 0) { LocalSerial = 1 };
        if (!_back.IsDisposed) Add(_back);
        _button = new Button(0, 0x7585, 0x7589, 0x7589) { ButtonAction = ButtonAction.Activate };
        if (!_button.IsDisposed) Add(_button);
        PlaceChrome();
    }

    static string _sig = "";

    public static void Refresh(World world) {
        UOLoginNet.CopyBuffs(Copy);
        var open = UIManager.GetGump<BuffGump>();
        var sig = Sig(Copy);
        // A closed bar stays closed until the icon list changes. CopyBuffs is every frame, not a Take.
        if (open == null) {
            if (Copy.Count == 0 || sig == _sig) return;
            UIManager.Add(open = new BuffGump(world));
        }
        open.Apply(Copy);
        _sig = sig;
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID != 0) return;
        _graphic++;
        if (_graphic > 0x7582) _graphic = 0x757F;
        _direction = _graphic switch {
            0x7580 => Dir.LeftHorizontal,
            0x7581 => Dir.RightVertical,
            0x7582 => Dir.RightHorizontal,
            _ => Dir.LeftVertical,
        };
        PlaceChrome();
        Rebuild();
    }

    void Apply(List<UOBuffState> rows) {
        if (Same(rows)) return;
        _shown.Clear();
        foreach (var row in rows) _shown.Add(row);
        Rebuild();
    }

    bool Same(List<UOBuffState> rows) {
        if (rows.Count != _shown.Count) return false;
        for (var i = 0; i < rows.Count; i++) {
            var a = rows[i];
            var b = _shown[i];
            if (a.Type != b.Type || a.Graphic != b.Graphic || a.Timer != b.Timer || a.Text != b.Text) return false;
        }
        return true;
    }

    void PlaceChrome() {
        if (_back != null && !_back.IsDisposed) {
            _back.Graphic = _graphic;
            _back.X = 0;
            _back.Y = 0;
        }
        if (_button == null || _button.IsDisposed) return;
        switch (_direction) {
            case Dir.LeftHorizontal: _button.X = -2; _button.Y = 36; break;
            case Dir.RightVertical: _button.X = 34; _button.Y = 78; break;
            case Dir.RightHorizontal: _button.X = 76; _button.Y = 36; break;
            default: _button.X = 0; _button.Y = 0; break;
        }
    }

    void Rebuild() {
        foreach (var icon in _icons) {
            icon.Parent = null;
            icon.Dispose();
        }
        _icons.Clear();
        WantUpdateSize = true;
        var textY = (_back == null || _back.IsDisposed ? 48 : _back.Height) + 8;
        for (var i = 0; i < _shown.Count; i++) {
            var buff = _shown[i];
            var pic = new GumpPic(0, 0, buff.Graphic, 0) { CanMove = true };
            PlaceIcon(pic, i);
            if (!pic.IsDisposed) {
                Add(pic);
                _icons.Add(pic);
            }
            var plain = Plain(buff.Text);
            if (plain.Length == 0) plain = buff.Type.ToString();
            var parts = plain.Replace("\r", "").Split('\n');
            foreach (var part in parts) {
                if (part.Length == 0) continue;
                var label = new Label(part, true, 0x0386, 1) { X = 8, Y = textY };
                Add(label);
                _icons.Add(label);
                textY += 16;
            }
        }
    }

    void PlaceIcon(GumpPic pic, int index) {
        var offset = index * 31;
        var bw = _back == null || _back.IsDisposed ? 0 : _back.Width;
        var bh = _back == null || _back.IsDisposed ? 0 : _back.Height;
        switch (_direction) {
            case Dir.LeftVertical: pic.X = 25; pic.Y = 26 + offset; break;
            case Dir.LeftHorizontal: pic.X = 26 + offset; pic.Y = 5; break;
            case Dir.RightVertical: pic.X = 5; pic.Y = bh - 48 - offset; break;
            default: pic.X = bw - 48 - offset; pic.Y = 5; break;
        }
    }

    static string Sig(List<UOBuffState> rows) {
        var parts = new string[rows.Count];
        for (var i = 0; i < rows.Count; i++) {
            var row = rows[i];
            parts[i] = row.Type + ":" + row.Graphic + ":" + row.Timer + ":" + row.Text;
        }
        System.Array.Sort(parts, System.StringComparer.Ordinal);
        return string.Join("\n", parts);
    }

    static string Plain(string text) {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace("<left>", "").Replace("</left>", "");
    }

    enum Dir { LeftVertical, LeftHorizontal, RightVertical, RightHorizontal }
}
