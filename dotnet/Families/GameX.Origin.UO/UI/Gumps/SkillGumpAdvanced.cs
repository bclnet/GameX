// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO SkillGumpAdvanced. Name sorts the label, Real sorts Base, Base sorts Value, Cap sorts Cap.
/// A row prints the fixed-point ushort divided by 10 with the default float format. Totals use F1.
/// An empty UOLoginNet list prints 0.0 and no rows. A missing name prints "skill {id}".
/// Lock art is 0x983 up, 0x985 down, 0x82C locked. A click cycles up, down, locked and sends 0x3A.
/// The action flag is not stored, so there is no use button. Shift-click opens one SkillButtonGump.
/// This host has no drag. The gray frame is opaque. The 0.95 black fill is an opaque dark pixel.
/// </summary>
public sealed class SkillGumpAdvanced : Gump {
    const int ViewW = 500;
    const int ViewH = 360;
    const int ListX = 20;
    const int ListY = 60;
    const int ListW = 460;
    const int ListH = 250;
    const int RowH = 20;
    const ushort Hue = 1153;
    const int FillKey = 0x57200021;
    const int LineKey = 0x57200022;
    const int PickKey = 0x57200023;

    // r = g = b = 1, opaque. Stand-in for AlphaBlendControl(0.95).
    static readonly byte[] Dark = { 0x21, 0x84 };
    static readonly byte[] Gray = { 0x10, 0xC2 };
    static readonly byte[] White = { 0xFF, 0xFF };

    readonly List<UOSkillState> _src = new List<UOSkillState>();
    readonly Dictionary<ushort, UOSkillState> _map = new Dictionary<ushort, UOSkillState>();
    readonly Dictionary<ushort, byte> _held = new Dictionary<ushort, byte>();
    readonly List<ushort> _drop = new List<ushort>();
    readonly List<SkillRow> _rows = new List<SkillRow>();
    readonly SortHead[] _heads;
    readonly Label _totalReal;
    readonly Label _totalValue;
    readonly GumpPic _arrow;
    readonly SkillPane _pane;
    int _rev = -1;
    int _scroll;
    int _wheel;
    bool _hot;
    bool _update = true;
    bool _sortAsc;
    string _sortField;

    public SkillGumpAdvanced(World world) : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        Width = ViewW;
        Height = ViewH;
        _heads = new[] {
            new SortHead(this, (int)Buttons.SortName, "Name:", 40, 25, 180, 25),
            new SortHead(this, (int)Buttons.SortReal, "Real", 220, 25, 80, 25),
            new SortHead(this, (int)Buttons.SortBase, "Base", 300, 25, 80, 25),
            new SortHead(this, (int)Buttons.SortCap, "Cap", 380, 25, 80, 25),
        };
        for (var i = 0; i < _heads.Length; i++) Add(_heads[i]);
        _pane = new SkillPane { X = ListX, Y = ListY, Width = ListW, Height = ListH };
        Add(_pane);
        Add(new Label("Total: ", true, Hue, 1) { X = 40, Y = 320 });
        _totalReal = new Label("0.0", true, Hue, 1) { X = 220, Y = 320 };
        _totalValue = new Label("0.0", true, Hue, 1) { X = 300, Y = 320 };
        Add(_totalReal);
        Add(_totalValue);
        var arrow = new GumpPic(0, 0, 0x0983, 0);
        if (arrow.IsDisposed) _arrow = null;
        else {
            _arrow = arrow;
            Add(_arrow);
        }
        _sortField = "";
        OnButtonClick((int)Buttons.SortName);
    }

    public void ForceUpdate() => _update = true;

    public override void OnButtonClick(int buttonID) {
        var field = Field(buttonID);
        if (field == null) return;
        if (_sortField == field) _sortAsc = !_sortAsc;
        _sortField = field;
        SortHead head = null;
        for (var i = 0; i < _heads.Length; i++) {
            var on = _heads[i].Id == buttonID;
            _heads[i].Selected = on;
            if (on) head = _heads[i];
        }
        if (_arrow != null && !_arrow.IsDisposed && head != null) {
            _arrow.Graphic = (ushort)(_sortAsc ? 0x0985 : 0x0983);
            _arrow.X = head.X + head.Width - 15;
            _arrow.Y = head.Y + 5;
        }
        _update = true;
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (_update || rev != _rev) {
            _rev = rev;
            _update = false;
            Build();
        }
        Wheel();
        Place();
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var sink = GumpArt.Sink;
        if (sink != null && Width > 2 && Height > 2)
            sink.DrawRaw(FillKey, x + 1, y + 1, Width - 2, Height - 2, Dark, 1, 1);
        if (sink != null) {
            sink.DrawRaw(LineKey, x, y, Width, 1, Gray, 1, 1);
            sink.DrawRaw(LineKey, x, y + Height - 1, Width, 1, Gray, 1, 1);
            sink.DrawRaw(LineKey, x, y, 1, Height, Gray, 1, 1);
            sink.DrawRaw(LineKey, x + Width - 1, y, 1, Height, Gray, 1, 1);
        }
        var drawn = base.Draw(x, y);
        if (sink != null) {
            sink.DrawRaw(LineKey, x + 20, y + 60, 435, 1, White, 1, 1);
            sink.DrawRaw(LineKey, x + 20, y + 310, 435, 1, White, 1, 1);
        }
        return drawn;
    }

    void Build() {
        Read();
        var list = new List<UOSkillState>(_map.Values);
        list.Sort(Compare);
        for (var i = 0; i < _rows.Count; i++) {
            _rows[i].Parent = null;
            _rows[i].Dispose();
        }
        _rows.Clear();
        double real = 0;
        double value = 0;
        for (var i = 0; i < list.Count; i++) {
            var skill = list[i];
            real += skill.BaseValue / 10.0;
            value += skill.Value / 10.0;
            var row = new SkillRow(this, skill);
            _pane.Add(row);
            _rows.Add(row);
        }
        _totalReal.Text = real.ToString("F1");
        _totalValue.Text = value.ToString("F1");
        Place();
    }

    void Read() {
        UOLoginNet.CopySkills(_src);
        _map.Clear();
        for (var i = 0; i < _src.Count; i++) {
            var row = _src[i];
            if (!_map.TryGetValue(row.Id, out var prev)) {
                _map[row.Id] = row;
                continue;
            }
            var name = string.IsNullOrEmpty(row.Name) ? prev.Name : row.Name;
            var body = row.Name == null ? row : prev.Name == null ? prev : row;
            _map[row.Id] = body.Name == name ? body : new UOSkillState(body.Id, body.Value, body.BaseValue, body.Lock, body.Cap, name);
        }
        if (_held.Count == 0) return;
        _drop.Clear();
        foreach (var pair in _held) {
            if (!_map.TryGetValue(pair.Key, out var skill)) continue;
            if (skill.Lock == pair.Value) {
                _drop.Add(pair.Key);
                continue;
            }
            _map[pair.Key] = new UOSkillState(skill.Id, skill.Value, skill.BaseValue, pair.Value, skill.Cap, skill.Name);
        }
        for (var i = 0; i < _drop.Count; i++) _held.Remove(_drop[i]);
    }

    internal void Hold(ushort id, byte next) {
        _held[id] = next;
        if (!_map.TryGetValue(id, out var skill)) return;
        _map[id] = new UOSkillState(skill.Id, skill.Value, skill.BaseValue, next, skill.Cap, skill.Name);
    }

    void Place() {
        var max = _rows.Count * RowH - ListH;
        if (max < 0) max = 0;
        if (_scroll < 0) _scroll = 0;
        if (_scroll > max) _scroll = max;
        for (var i = 0; i < _rows.Count; i++) {
            var y = i * RowH - _scroll;
            var row = _rows[i];
            row.Y = y;
            row.IsVisible = y + RowH > 0 && y < ListH;
        }
    }

    void Wheel() {
        if (!Over()) {
            _hot = false;
            return;
        }
        var w = Mouse.Current.Wheel;
        if (!_hot) {
            _hot = true;
            _wheel = w;
            return;
        }
        var max = _rows.Count * RowH - ListH;
        if (max < 0) max = 0;
        while (w >= _wheel + 120) {
            _wheel += 120;
            _scroll -= RowH;
        }
        while (w <= _wheel - 120) {
            _wheel -= 120;
            _scroll += RowH;
        }
        if (_scroll < 0) _scroll = 0;
        if (_scroll > max) _scroll = max;
    }

    bool Over() {
        for (var c = UIManager.MouseOverControl; c != null; c = c.Parent)
            if (c == this) return true;
        return false;
    }

    int Compare(UOSkillState a, UOSkillState b) {
        int c;
        switch (_sortField) {
            case "Base": c = a.BaseValue.CompareTo(b.BaseValue); break;
            case "Value": c = a.Value.CompareTo(b.Value); break;
            case "Cap": c = a.Cap.CompareTo(b.Cap); break;
            default: c = string.Compare(SkillName(a), SkillName(b), StringComparison.CurrentCulture); break;
        }
        if (c == 0) return a.Id.CompareTo(b.Id);
        return _sortAsc ? -c : c;
    }

    static string Field(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.SortName: return "Name";
            case Buttons.SortReal: return "Base";
            case Buttons.SortBase: return "Value";
            case Buttons.SortCap: return "Cap";
            default: return null;
        }
    }

    internal static string SkillName(UOSkillState skill) =>
        string.IsNullOrEmpty(skill.Name) ? "skill " + skill.Id : skill.Name;

    internal static string Num(ushort fixedPoint) => (fixedPoint / 10f).ToString();

    internal static ushort LockGraphic(byte lockState) {
        switch (lockState) {
            case 1: return 0x0985;
            case 2: return 0x082C;
            default: return 0x0983;
        }
    }

    enum Buttons { SortName = 1, SortReal = 2, SortBase = 3, SortCap = 4 }

    sealed class SortHead : Control {
        readonly SkillGumpAdvanced _owner;
        readonly string _text;

        public SortHead(SkillGumpAdvanced owner, int id, string text, int x, int y, int w, int h) {
            _owner = owner;
            _text = text;
            Id = id;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
        }

        public int Id { get; }
        public bool Selected { get; set; }

        public override void OnMouseClick(int x, int y) => _owner.OnButtonClick(Id);

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (Selected && sink != null)
                sink.DrawRaw(PickKey, x, y, Width, Height, White, 1, 1);
            var tw = sink?.MeasureText(_text, 1) ?? _text.Length * 6;
            var tx = x + ((Width - tw) >> 1);
            if (tx < x) tx = x;
            var ty = y + ((Height - 16) >> 1);
            if (ty < y) ty = y;
            sink?.DrawText(tx, ty, _text, 0, 1);
            return true;
        }
    }

    sealed class SkillPane : Control {
        public SkillPane() {
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            for (var i = 0; i < Children.Count; i++) {
                var c = Children[i];
                if (!c.IsVisible || c.IsDisposed) continue;
                if (c.Y + c.Height <= 0 || c.Y >= Height) continue;
                c.Draw(x + c.X, y + c.Y);
            }
            return true;
        }
    }

    sealed class SkillRow : Control {
        readonly SkillGumpAdvanced _owner;
        readonly Button _lock;
        UOSkillState _skill;

        public SkillRow(SkillGumpAdvanced owner, UOSkillState skill) {
            _owner = owner;
            _skill = skill;
            Width = 450;
            Height = RowH;
            CanMove = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            Add(new Label(SkillName(skill), true, Hue, 3) { X = 20 });
            Add(new Label(Num(skill.BaseValue), true, Hue, 3) { X = 200 });
            Add(new Label(Num(skill.Value), true, Hue, 3) { X = 280 });
            Add(new Label(Num(skill.Cap), true, Hue, 3) { X = 360 });
            var graphic = LockGraphic(skill.Lock);
            var button = new Button(1, graphic, graphic, graphic) {
                X = 425,
                Y = 4,
                ButtonAction = ButtonAction.Activate,
            };
            if (button.IsDisposed) _lock = null;
            else {
                _lock = button;
                Add(button);
            }
        }

        public override void OnButtonClick(int buttonID) {
            if (buttonID != 1) return;
            var next = _skill.Lock == 0 ? (byte)1 : _skill.Lock == 1 ? (byte)2 : (byte)0;
            UOLoginNet.Send(OutgoingPackets.Send_SkillStatusChangeRequest(_skill.Id, next));
            _skill = new UOSkillState(_skill.Id, _skill.Value, _skill.BaseValue, next, _skill.Cap, _skill.Name);
            _owner.Hold(_skill.Id, next);
            if (_lock != null && !_lock.IsDisposed) _lock.ButtonGraphicNormal = LockGraphic(next);
        }

        public override void OnMouseClick(int x, int y) {
            if (!GameController.Shift) return;
            var open = SkillButtonGump.Find(_skill.Id);
            if (open != null) {
                open.Show();
                return;
            }
            UIManager.Add(new SkillButtonGump(_owner.World, _skill.Id, SkillName(_skill), Mouse.Position.X - 44, Mouse.Position.Y - 22));
        }
    }
}
