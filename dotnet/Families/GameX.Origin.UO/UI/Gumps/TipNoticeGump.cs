// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO TipNoticeGump. Flag 1 is the stop and opens nothing.
/// Scroll 0x0820. A tip (flag 0) titles 0x9CA and uses prev 0x9CC and next 0x9CD. A notice titles 0x9D2.
/// Flag 0 sits at 200, 100. Any other flag sits at 20, 20. Height is 300.
/// Prev and next do not send: UOLoginNet does not queue Send_TipRequest. The expander does not resize.
/// </summary>
public sealed class TipNoticeGump : Gump {
    const ushort Scroll = 0x0820;
    const int Special = 300;

    readonly GumpPic _top, _bottom, _title;
    readonly GumpPicTiled _right, _mid;
    readonly Button _expander, _prev, _next;
    readonly List<Label> _lines = [];
    byte _flag;

    public TipNoticeGump(World world, UOTipState tip) : base(world, tip.Tip, tip.Tip) {
        Height = Special;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        X = tip.Flag == 0 ? 200 : 20;
        Y = tip.Flag == 0 ? 100 : 20;
        _top = new GumpPic(0, 0, Scroll, 0);
        _right = new GumpPicTiled((ushort)(Scroll + 1));
        _mid = new GumpPicTiled((ushort)(Scroll + 2));
        _bottom = new GumpPic(0, 0, (ushort)(Scroll + 3), 0);
        _expander = new Button(0x7FBEEF, 0x082E, 0x082F) { ButtonAction = ButtonAction.Activate };
        _title = new GumpPic(0, 0, 0x09CA, 0);
        _prev = new Button(1, 0x09CC, 0x09CC) { X = 35, Y = 0, ButtonAction = ButtonAction.Activate };
        _next = new Button(2, 0x09CD, 0x09CD) { X = 240, Y = 0, ButtonAction = ButtonAction.Activate };
        Keep(_top);
        Keep(_right);
        Keep(_mid);
        Keep(_bottom);
        Keep(_expander);
        Keep(_title);
        Keep(_prev);
        Keep(_next);
        Show(tip);
    }

    public static void Open(World world, UOTipState tip) {
        if (tip.Flag == 1) return;
        var open = UIManager.GetGump<TipNoticeGump>(tip.Tip);
        if (open == null) UIManager.Add(new TipNoticeGump(world, tip));
        else open.Show(tip);
    }

    public override void Update() {
        Place();
        base.Update();
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID != 1 && buttonID != 2) return;
    }

    void Show(UOTipState tip) {
        _flag = tip.Flag;
        WantUpdateSize = true;
        if (_title != null && !_title.IsDisposed) _title.Graphic = _flag == 0 ? (ushort)0x09CA : (ushort)0x09D2;
        var on = _flag == 0;
        if (_prev != null && !_prev.IsDisposed) _prev.IsVisible = on;
        if (_next != null && !_next.IsDisposed) _next.IsVisible = on;
        foreach (var line in _lines) {
            line.Parent = null;
            line.Dispose();
        }
        _lines.Clear();
        var text = (tip.Text ?? "").Replace("\r", "");
        var parts = text.Split('\n');
        var y = 32;
        foreach (var part in parts) {
            var label = new Label(part, false, 0, 6) { X = 35, Y = y };
            Add(label);
            _lines.Add(label);
            y += 16;
        }
        Place();
    }

    void Keep(Control c) {
        if (c != null && !c.IsDisposed) Add(c);
    }

    void Place() {
        GumpMetrics.TrySize(Scroll, out var w0, out var h0);
        GumpMetrics.TrySize((ushort)(Scroll + 1), out var w1, out _);
        GumpMetrics.TrySize((ushort)(Scroll + 2), out var w2, out _);
        GumpMetrics.TrySize((ushort)(Scroll + 3), out var w3, out var h3);
        GumpMetrics.TrySize(0x082E, out var ew, out var eh);
        var width = w0;
        if (w1 > width) width = w1;
        if (w2 > width) width = w2;
        if (w3 > width) width = w3;
        var midH = Special - h0 - h3 - eh;
        if (midH < 0) midH = 0;
        var midY = h0;
        if (_top != null && !_top.IsDisposed) { _top.X = 0; _top.Y = 0; }
        if (_right != null && !_right.IsDisposed) {
            _right.X = (width - w1) / 2;
            _right.Y = midY;
            if (w1 > 0) _right.Width = w1;
            _right.Height = midH;
        }
        if (_mid != null && !_mid.IsDisposed) {
            _mid.X = (width - w1) / 2;
            _mid.Y = midY;
            if (w2 > 0) _mid.Width = w2;
            _mid.Height = midH;
        }
        if (_bottom != null && !_bottom.IsDisposed) {
            var off = w0 - w3;
            _bottom.X = off / 2 + off / 4;
            _bottom.Y = Special - h3 - eh;
        }
        if (_expander != null && !_expander.IsDisposed) {
            var scrollW = w2 > 0 ? w2 : width;
            _expander.X = (scrollW - ew) >> 1;
            _expander.Y = Special - eh - 2;
        }
        if (_title != null && !_title.IsDisposed) { _title.X = 0; _title.Y = 0; }
    }
}
