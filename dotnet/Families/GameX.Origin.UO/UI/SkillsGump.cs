// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO StandardSkillsGump. The printed number is the stored ushort divided by 10, format F1:
/// that is Skill.ValueFixed / 10.0f. Show Real prints BaseValue, Show Caps prints Cap, and the total
/// uses BaseValue only while Show Real is on. An empty Name prints the id, the same stand-in SetSkills
/// uses when the 0xFE block did not supply one. Lock art follows the stored byte. The click does not
/// send 0x3A: UOGamePackets has no skill-lock writer. The 0xFE action flag is not stored and there is
/// no 0x12 UseSkill writer, so the use button is not shown. skillgrp groups are not in this client.
/// Right-click closes.
/// </summary>
public sealed class SkillsGump : Gump {
    const int DiffY = 22;
    const int Row = 17;
    const int MinScroll = 274;

    readonly List<UOSkillState> _src = [];
    readonly List<UOSkillState> _skills = [];
    readonly List<SkillRow> _rows = [];
    readonly GumpPic _icon, _top, _bottomPic, _title, _lineTop, _lineBot, _comment;
    readonly GumpPicTiled _mid, _right;
    readonly Button _expander, _newGroup;
    readonly Checkbox _real, _caps;
    readonly Label _sum;
    int _rev = -1;
    bool _minimized;

    public SkillsGump(World world) : base(world, 0, 0) {
        AcceptMouseInput = false;
        CanMove = true;
        CanCloseWithRightClick = true;
        X = 100;
        Y = 100;
        _icon = new IconPic(this);
        Add(_icon);
        _top = new GumpPic(0, DiffY, 0x1F40, 0);
        Add(_top);
        _right = new GumpPicTiled(0x1F41);
        Add(_right);
        _mid = new GumpPicTiled(0x1F42);
        Add(_mid);
        _bottomPic = new GumpPic(0, DiffY, 0x1F43, 0);
        Add(_bottomPic);
        _title = new GumpPic(0, DiffY, 0x0834, 0);
        Add(_title);
        _lineTop = new GumpPic(50, 35 + DiffY, 0x082B, 0);
        Add(_lineTop);
        _lineBot = new GumpPic(50, 0, 0x082B, 0);
        Add(_lineBot);
        _comment = new GumpPic(25, 0, 0x0836, 0);
        Add(_comment);
        _sum = new Label("0.0", false, 600, 3);
        Add(_sum);
        _expander = new Button(0x7FBEEF, 0x082E, 0x082F) { ButtonAction = ButtonAction.Activate };
        if (!_expander.IsDisposed) Add(_expander);
        _newGroup = new Button(0, 0x083A, 0x083A, 0x083A) { X = 60, ButtonAction = ButtonAction.Activate };
        if (!_newGroup.IsDisposed) Add(_newGroup);
        _real = new Checkbox(0x938, 0x939, " - Show Real", 1, 0x0386, false);
        _caps = new Checkbox(0x938, 0x939, " - Show Caps", 1, 0x0386, false);
        if (!_real.IsDisposed) {
            Add(_real);
            _real.ValueChanged += OnCheck;
        }
        if (!_caps.IsDisposed) {
            Add(_caps);
            _caps.ValueChanged += OnCheck;
        }
        Add(new MinBox(this));
        Fill();
        _rev = UOLoginNet.WorldRevision;
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Fill();
        }
        base.Update();
    }

    /// <summary>Lock, new-group, and the expander do not send. There is no 0x3A skill-lock writer.</summary>
    public override void OnButtonClick(int buttonID) { }

    void OnCheck(object sender, EventArgs e) {
        if (_real != null && _caps != null && !_real.IsDisposed && !_caps.IsDisposed && _real.IsChecked && _caps.IsChecked) {
            if (sender == _real) _caps.IsChecked = false;
            else _real.IsChecked = false;
        }
        PaintValues();
        PaintSum();
    }

    void Fill() {
        UOLoginNet.CopySkills(_src);
        Merge();
        foreach (var c in _rows) {
            c.Parent = null;
            c.Dispose();
        }
        _rows.Clear();
        var y = 45 + DiffY + _lineTop.Height - 10;
        foreach (var s in _skills) {
            var row = new SkillRow(this, s) { Y = y };
            Add(row);
            _rows.Add(row);
            y += Row;
        }
        Layout();
        PaintSum();
        ApplyMin();
        WantUpdateSize = true;
    }

    void Merge() {
        var map = new Dictionary<ushort, UOSkillState>();
        foreach (var row in _src) {
            if (!map.TryGetValue(row.Id, out var prev)) {
                map[row.Id] = row;
                continue;
            }
            var name = string.IsNullOrEmpty(row.Name) ? prev.Name : row.Name;
            var body = row.Name == null ? row : prev.Name == null ? prev : row;
            map[row.Id] = body.Name == name ? body : new UOSkillState(body.Id, body.Value, body.BaseValue, body.Lock, body.Cap, name);
        }
        _skills.Clear();
        foreach (var row in map.Values) _skills.Add(row);
        _skills.Sort((a, b) => a.Id.CompareTo(b.Id));
    }

    void Layout() {
        var areaTop = 45 + DiffY + _lineTop.Height - 10;
        var scroll = areaTop + _skills.Count * Row + 110;
        if (scroll < MinScroll) scroll = MinScroll;
        var expH = _expander != null && !_expander.IsDisposed ? _expander.Height : 0;
        var width = _top.Width;
        if (_right.Width > width) width = _right.Width;
        if (_mid.Width > width) width = _mid.Width;
        if (_bottomPic.Width > width) width = _bottomPic.Width;
        _mid.Y = _right.Y = DiffY + _top.Height;
        var midH = scroll - _top.Height - _bottomPic.Height - expH;
        if (midH < 1) midH = 1;
        _mid.Height = _right.Height = midH;
        if (width > 0) {
            _mid.X = (width - _mid.Width) / 2;
            _right.X = (width - _right.Width) / 2;
            var off = _top.Width - _bottomPic.Width;
            _bottomPic.X = (off / 2) + (off / 4);
        }
        _bottomPic.Y = DiffY + scroll - _bottomPic.Height - expH;
        if (_expander != null && !_expander.IsDisposed) {
            _expander.X = (width - _expander.Width) >> 1;
            _expander.Y = DiffY + scroll - expH - 2;
        }
        _title.X = _top.X + ((_top.Width - _title.Width) >> 1);
        _title.Y = _top.Y + ((_top.Height - _title.Height) >> 1);
        _lineBot.Y = scroll - 98;
        _comment.Y = scroll - 85;
        _sum.X = _comment.X + _comment.Width + 5;
        _sum.Y = _comment.Y + 2;
        var groupY = scroll - 52;
        if (_newGroup != null && !_newGroup.IsDisposed) _newGroup.Y = groupY;
        var checks = 60 + (_newGroup != null && !_newGroup.IsDisposed ? _newGroup.Width + 30 : 0);
        if (_real != null && !_real.IsDisposed) {
            _real.X = checks;
            _real.Y = groupY - 6;
        }
        if (_caps != null && !_caps.IsDisposed) {
            _caps.X = checks;
            _caps.Y = groupY + 7;
        }
    }

    void PaintValues() {
        foreach (var row in _rows) row.Apply();
    }

    void PaintSum() {
        float sum = 0;
        var real = _real != null && !_real.IsDisposed && _real.IsChecked;
        foreach (var s in _skills) sum += (real ? s.BaseValue : s.Value) / 10.0f;
        _sum.Text = sum.ToString("F1");
    }

    string Shown(UOSkillState s) {
        var real = _real != null && !_real.IsDisposed && _real.IsChecked;
        var cap = _caps != null && !_caps.IsDisposed && _caps.IsChecked;
        var n = real ? s.BaseValue : cap ? s.Cap : s.Value;
        return (n / 10.0f).ToString("F1");
    }

    static string SkillName(UOSkillState s) => string.IsNullOrEmpty(s.Name) ? "skill " + s.Id : s.Name;

    static ushort LockGraphic(byte lockState) => lockState switch {
        1 => 0x0986,
        2 => 0x082C,
        _ => 0x0984,
    };

    void ApplyMin() {
        foreach (var c in Children) c.IsVisible = !_minimized || c == _icon;
        _icon.IsVisible = true;
        if (_minimized) {
            _icon.Graphic = 0x839;
            _icon.X = 0;
        }
        else {
            _icon.Graphic = 0x82D;
            _icon.X = 160;
        }
    }

    bool Minimized {
        get => _minimized;
        set {
            if (_minimized == value) return;
            _minimized = value;
            ApplyMin();
            WantUpdateSize = true;
        }
    }

    sealed class SkillRow : Control {
        readonly SkillsGump _owner;
        readonly UOSkillState _skill;
        readonly Label _value;
        readonly Button _lock;

        public SkillRow(SkillsGump owner, UOSkillState skill) {
            _owner = owner;
            _skill = skill;
            X = 25;
            Width = 255;
            Height = Row;
            CanMove = false;
            AcceptMouseInput = true;
            Add(new Label(SkillName(skill), false, 0x0288, 9) { X = 22 });
            _value = new Label("", false, 0x0288, 9);
            Add(_value);
            var g = LockGraphic(skill.Lock);
            var b = new Button(skill.Id, g, g, g) { X = 251, ButtonAction = ButtonAction.Activate };
            if (b.IsDisposed) _lock = null;
            else {
                _lock = b;
                Add(b);
            }
            Apply();
        }

        public override bool OnMouseDoubleClick(int x, int y) {
            UOLoginNet.UseSkill(_skill.Id);
            return true;
        }

        public void Apply() {
            _value.Text = _owner.Shown(_skill);
            _value.X = 250 - _value.Width;
            if (_lock != null && !_lock.IsDisposed) _lock.ButtonGraphicNormal = LockGraphic(_skill.Lock);
        }
    }

    sealed class IconPic : GumpPic {
        readonly SkillsGump _owner;

        public IconPic(SkillsGump owner) : base(160, 0, 0x82D, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (!_owner.Minimized) return false;
            _owner.Minimized = false;
            return true;
        }
    }

    sealed class MinBox : Control {
        readonly SkillsGump _owner;

        public MinBox(SkillsGump owner) {
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
