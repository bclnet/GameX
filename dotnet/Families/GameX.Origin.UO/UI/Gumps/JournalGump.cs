// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO JournalGump, the gump OpenJournal adds. ResizableJournal is the other journal and is not used here.
/// Scroll art is 0x1F40 through 0x1F43, title 0x82A, minimize 0x82D or 0x830, expander 0x082E / 0x082F, flag 0x0828.
/// A speech row is UOSpeechState. An empty name is the text. Any other name is "Name: text".
/// Dark mode and the System, Objects, Client, and Guild boxes write a profile this client does not store, so they are not drawn.
/// The flag sits at the bottom when a line does not fit. This host has no drag, so the flag does not move and the expander does not resize.
/// </summary>
public sealed class JournalGump : Gump {
    const int DiffY = 22;
    const int ViewH = 300;
    const int Special = ViewH - DiffY;
    const int TextX = 25;
    const int TextY = DiffY + 36;
    const int TextH = ViewH - (98 + DiffY);
    // Filter row is Special - box - DiffY + 10. Twelve labels of 16 end on that line. The speech list holds twelve.
    const int Rows = 12;
    const ushort ScrollGraphic = 0x1F40;
    const ushort TitleGraphic = 0x82A;
    const ushort OpenGraphic = 0x82D;
    const ushort ClosedGraphic = 0x830;
    const ushort FlagGraphic = 0x0828;
    const int ExpanderId = 0x7FBEEF;

    readonly MinPic _minPic;
    readonly MinHit _minHit;
    readonly GumpPic _top;
    readonly GumpPicTiled _right;
    readonly GumpPicTiled _middle;
    readonly GumpPic _bottom;
    readonly GumpPic _title;
    readonly GumpPic _flag;
    readonly Button _expander;
    readonly Label[] _rows;
    readonly List<UOSpeechState> _lines = [];
    int _rev = -1;
    bool _minimized;

    public JournalGump(World world) : base(world, 0, 0) {
        Height = ViewH;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        X = 64;
        Y = 64;
        _minPic = new MinPic(this);
        Keep(_minPic);
        _top = new GumpPic(0, DiffY, ScrollGraphic, 0);
        Keep(_top);
        _right = new GumpPicTiled(0, 0, 0, 0, (ushort)(ScrollGraphic + 1));
        Keep(_right);
        _middle = new GumpPicTiled(0, 0, 0, 0, (ushort)(ScrollGraphic + 2));
        Keep(_middle);
        _bottom = new GumpPic(0, 0, (ushort)(ScrollGraphic + 3), 0);
        Keep(_bottom);
        _expander = new Button(ExpanderId, 0x082E, 0x082F) { ButtonAction = ButtonAction.Activate };
        Keep(_expander);
        _title = new GumpPic(0, DiffY, TitleGraphic, 0);
        Keep(_title);
        _flag = new GumpPic(-25, TextY, FlagGraphic, 0);
        Keep(_flag);
        _rows = new Label[Rows];
        for (var i = 0; i < Rows; i++) {
            _rows[i] = new Label("", false, 0, 3) { X = TextX, Y = TextY + i * 16, IsVisible = false };
            Add(_rows[i]);
        }
        _minHit = new MinHit(this);
        Add(_minHit);
        Place();
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Fill();
        }
        Place();
        WantUpdateSize = true;
        base.Update();
    }

    void Keep(Control c) {
        if (!c.IsDisposed) Add(c);
    }

    void Fill() {
        UOLoginNet.CopySpeech(_lines);
        var start = _lines.Count > Rows ? _lines.Count - Rows : 0;
        var n = 0;
        for (var i = start; i < _lines.Count && n < Rows; i++, n++) {
            var line = _lines[i];
            var row = _rows[n];
            row.Text = string.IsNullOrEmpty(line.Name) ? line.Text : $"{line.Name}: {line.Text}";
            row.Hue = line.Hue;
            row.IsVisible = !_minimized;
        }
        for (; n < Rows; n++) _rows[n].IsVisible = false;
        ShowFlag();
    }

    void Place() {
        GumpMetrics.TrySize(ScrollGraphic, out var w0, out var h0);
        GumpMetrics.TrySize((ushort)(ScrollGraphic + 1), out var w1, out _);
        GumpMetrics.TrySize((ushort)(ScrollGraphic + 2), out var w2, out _);
        GumpMetrics.TrySize((ushort)(ScrollGraphic + 3), out var w3, out var h3);
        GumpMetrics.TrySize(0x082E, out var ew, out var eh);
        GumpMetrics.TrySize(TitleGraphic, out var tw, out var th);
        GumpMetrics.TrySize(FlagGraphic, out var fw, out var fh);
        var width = w0;
        if (w1 > width) width = w1;
        if (w2 > width) width = w2;
        if (w3 > width) width = w3;
        var midH = Special - h0 - h3 - eh;
        if (midH < 0) midH = 0;
        var midY = DiffY + h0;
        var scrollW = w2 > 0 ? w2 : width;
        // Both tiles use graphic + 1's width. Bottom x is (w0 - w3) / 2 + that / 4.
        if (!_top.IsDisposed) {
            _top.X = 0;
            _top.Y = DiffY;
        }
        if (!_right.IsDisposed) {
            _right.X = (width - w1) / 2;
            _right.Y = midY;
            if (w1 > 0) _right.Width = w1;
            _right.Height = midH;
        }
        if (!_middle.IsDisposed) {
            _middle.X = (width - w1) / 2;
            _middle.Y = midY;
            if (w2 > 0) _middle.Width = w2;
            _middle.Height = midH;
        }
        if (!_bottom.IsDisposed) {
            var off = w0 - w3;
            _bottom.X = off / 2 + off / 4;
            _bottom.Y = DiffY + Special - h3 - eh;
        }
        if (!_expander.IsDisposed) {
            _expander.X = (scrollW - ew) >> 1;
            _expander.Y = DiffY + Special - eh - 2;
        }
        if (!_title.IsDisposed) {
            _title.X = (w0 - tw) >> 1;
            _title.Y = DiffY + ((h0 - th) >> 1);
        }
        if (!_flag.IsDisposed) {
            var listW = scrollW - (fw >> 1) - 5;
            _flag.X = TextX + listW - (fw >> 1) + 5;
            _flag.Y = TextY + TextH - fh;
            if (_flag.Y < TextY) _flag.Y = TextY;
        }
        if (_minimized) HideForMin();
        else ShowChrome();
    }

    void ShowChrome() {
        Show(_top);
        Show(_right);
        Show(_middle);
        Show(_bottom);
        Show(_title);
        Show(_expander);
        Show(_minPic);
        Show(_minHit);
        ShowFlag();
    }

    void HideForMin() {
        foreach (var c in Children)
            if (c != _minPic) c.IsVisible = false;
        if (!_minPic.IsDisposed) _minPic.IsVisible = true;
    }

    void Show(Control c) {
        if (!c.IsDisposed) c.IsVisible = true;
    }

    void ShowFlag() {
        if (_flag.IsDisposed) return;
        _flag.IsVisible = !_minimized && _lines.Count > Rows;
    }

    bool Minimized {
        get => _minimized;
        set {
            if (_minimized == value) return;
            _minimized = value;
            if (!_minPic.IsDisposed) {
                _minPic.Graphic = value ? ClosedGraphic : OpenGraphic;
                _minPic.X = value ? 0 : 160;
            }
            if (value) HideForMin();
            else {
                ShowChrome();
                Fill();
            }
            WantUpdateSize = true;
        }
    }

    sealed class MinPic : GumpPic {
        readonly JournalGump _owner;

        public MinPic(JournalGump owner) : base(160, 0, OpenGraphic, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (!_owner.Minimized) return false;
            _owner.Minimized = false;
            return true;
        }
    }

    sealed class MinHit : Control {
        readonly JournalGump _owner;

        public MinHit(JournalGump owner) {
            _owner = owner;
            X = 160;
            Y = 0;
            Width = 23;
            Height = 24;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) {
            if (!_owner.Minimized) _owner.Minimized = true;
        }
    }
}
