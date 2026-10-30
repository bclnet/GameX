// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ProfileGump. Header, the 0x005C rule, body, the 0x005F rule, then the footer.
/// Scroll 0x0820 starts at y 22. Height is 300 plus that 22. The minimize pic is 0x82D at 143, 0, or 0x9D4 at 0, 0.
/// The hit is 23 by 24 on that pic. A left click minimizes. A double-click restores.
/// The player serial would be editable. Send_ProfileUpdate is not queued, so the body stays labels.
/// </summary>
public sealed class ProfileGump : Gump {
    const ushort Scroll = 0x0820;
    const int DiffY = 22;
    const int Special = 300;

    readonly MinPic _pic;
    readonly GumpPic _top, _bottom;
    readonly GumpPicTiled _right, _mid;
    readonly Button _expander;
    readonly GumpPic _ruleLeft, _ruleRight, _footLeft, _footRight;
    readonly GumpPicTiled _ruleMid, _footMid;
    readonly Label _header, _footer;
    readonly MinHit _hit;
    readonly List<Label> _body = [];
    bool _minimized;

    public ProfileGump(World world, UOProfileState state) : base(world, state.Serial, state.Serial) {
        Height = Special + DiffY;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptKeyboardInput = true;
        AcceptMouseInput = true;
        _pic = new MinPic(this);
        Add(_pic);
        _top = new GumpPic(0, DiffY, Scroll, 0);
        _right = new GumpPicTiled((ushort)(Scroll + 1));
        _mid = new GumpPicTiled((ushort)(Scroll + 2));
        _bottom = new GumpPic(0, DiffY, (ushort)(Scroll + 3), 0);
        _expander = new Button(0x7FBEEF, 0x082E, 0x082F) { ButtonAction = ButtonAction.Activate };
        Keep(_top);
        Keep(_right);
        Keep(_mid);
        Keep(_bottom);
        Keep(_expander);
        _ruleLeft = new GumpPic(0, 0, 0x005C, 0);
        _ruleMid = new GumpPicTiled(0x005D);
        _ruleRight = new GumpPic(0, 0, 0x005E, 0);
        _footLeft = new GumpPic(0, 0, 0x005F, 0);
        _footMid = new GumpPicTiled(0x0060);
        _footRight = new GumpPic(0, 0, 0x0061, 0);
        Keep(_ruleLeft);
        Keep(_ruleMid);
        Keep(_ruleRight);
        Keep(_footLeft);
        Keep(_footMid);
        Keep(_footRight);
        _header = new Label("", true, 0, 1);
        _footer = new Label("", true, 0, 1);
        Add(_header);
        Add(_footer);
        _hit = new MinHit(this);
        Add(_hit);
        Show(state);
    }

    public static void Open(World world, UOProfileState state) {
        var open = UIManager.GetGump<ProfileGump>(state.Serial);
        if (open == null) UIManager.Add(new ProfileGump(world, state));
        else open.Show(state);
    }

    public bool Minimized {
        get => _minimized;
        set {
            if (_minimized == value) return;
            _minimized = value;
            if (!_pic.IsDisposed) {
                _pic.Graphic = value ? (ushort)0x9D4 : (ushort)0x82D;
                _pic.X = value ? 0 : 143;
            }
            foreach (var c in Children) c.IsVisible = !value;
            if (!_pic.IsDisposed) _pic.IsVisible = true;
            WantUpdateSize = true;
        }
    }

    public override void Update() {
        Layout();
        base.Update();
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == 0x7FBEEF) return;
    }

    void Show(UOProfileState state) {
        _header.Text = state.Header ?? "";
        _footer.Text = state.Footer ?? "";
        WantUpdateSize = true;
        foreach (var label in _body) {
            label.Parent = null;
            label.Dispose();
        }
        _body.Clear();
        var text = (state.Body ?? "").Replace("\r", "");
        if (text.Length > 0) {
            foreach (var part in text.Split('\n')) {
                var label = new Label(part, true, 0, 1);
                Add(label);
                _body.Add(label);
            }
        }
        Layout();
        if (!_minimized) return;
        foreach (var c in Children)
            if (c != _pic) c.IsVisible = false;
        if (!_pic.IsDisposed) _pic.IsVisible = true;
    }

    void Keep(Control c) {
        if (c != null && !c.IsDisposed) Add(c);
    }

    void Layout() {
        PlaceScroll();
        if (_minimized) return;
        const int areaX = 22;
        var areaY = 32 + DiffY;
        if (!_header.IsDisposed) {
            _header.X = areaX + 53;
            _header.Y = areaY + 6;
            _header.IsVisible = true;
        }
        var offset = _header.Height - 15;
        Place(_ruleLeft, areaX + 4, areaY + offset);
        if (_ruleMid != null && !_ruleMid.IsDisposed) {
            _ruleMid.X = areaX + 56;
            _ruleMid.Y = areaY + offset;
            _ruleMid.Width = 138;
            _ruleMid.IsVisible = true;
        }
        Place(_ruleRight, areaX + 194, areaY + offset);
        offset += 44;
        var bodyY = areaY + offset;
        for (var i = 0; i < _body.Count; i++) {
            var line = _body[i];
            if (line.IsDisposed) continue;
            line.X = areaX + 4;
            line.Y = bodyY + i * 16;
            line.IsVisible = true;
        }
        var bodyH = _body.Count * 16;
        if (bodyH < 20) bodyH = 20;
        var boxY = bodyY + bodyH + 3;
        Place(_footLeft, areaX + 4, boxY);
        if (_footMid != null && !_footMid.IsDisposed) {
            _footMid.X = areaX + 13;
            _footMid.Y = boxY + 9;
            _footMid.Width = 197;
            _footMid.IsVisible = true;
        }
        Place(_footRight, areaX + 210, boxY);
        if (!_footer.IsDisposed) {
            _footer.X = areaX + 2;
            _footer.Y = boxY + 26;
            _footer.IsVisible = true;
        }
    }

    static void Place(GumpPic pic, int x, int y) {
        if (pic == null || pic.IsDisposed) return;
        pic.X = x;
        pic.Y = y;
        pic.IsVisible = true;
    }

    void PlaceScroll() {
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
        var midY = DiffY + h0;
        if (_top != null && !_top.IsDisposed) { _top.X = 0; _top.Y = DiffY; }
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
            _bottom.Y = DiffY + Special - h3 - eh;
        }
        if (_expander != null && !_expander.IsDisposed) {
            var scrollW = w2 > 0 ? w2 : width;
            _expander.X = (scrollW - ew) >> 1;
            _expander.Y = DiffY + Special - eh - 2;
        }
    }

    sealed class MinPic : GumpPic {
        readonly ProfileGump _owner;

        public MinPic(ProfileGump owner) : base(143, 0, 0x82D, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (!_owner.Minimized) return false;
            _owner.Minimized = false;
            return true;
        }
    }

    sealed class MinHit : Control {
        readonly ProfileGump _owner;

        public MinHit(ProfileGump owner) {
            _owner = owner;
            X = 143;
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
