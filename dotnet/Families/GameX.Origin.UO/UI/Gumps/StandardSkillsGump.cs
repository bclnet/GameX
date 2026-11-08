// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO StandardSkillsGump. Groups are the in-memory SkillsGroupManager. No profile file.
/// A skill UOLoginNet does not have prints 0.0, which is an empty Skill. The number is fixed/10, format F1.
/// Show Real prints BaseValue, Show Caps prints Cap, and the total is every stored skill.
/// The total uses BaseValue only while Show Real is on. Lock art is 0x0984, 0x0986, and 0x082C.
/// A lock click cycles 0, 1, 2 and sends 0x3A. The action flag is not stored, so there is no use button.
/// Shift-click opens one SkillButtonGump. Double-click calls UseSkill. Delete is not forwarded, and a
/// skill is not dragged between groups. The scroll is 0x1F40 and grows from 274 to 800.
/// </summary>
public sealed class StandardSkillsGump : Gump {
    const int DiffY = 22;
    const int MinH = 274;
    const int MaxH = 800;
    const int SkillsCount = 60;
    const int FillBeige = 0x57200041;
    const int FillBisque = 0x57200042;
    const int FillWheat = 0x57200043;

    static readonly byte[] Beige = { 0xDB, 0xFB };
    static readonly byte[] Bisque = { 0x98, 0xFF };
    static readonly byte[] Wheat = { 0x76, 0xFB };

    static SkillsGroupManager _manager;

    readonly List<UOSkillState> _src = new List<UOSkillState>();
    readonly Dictionary<ushort, UOSkillState> _map = new Dictionary<ushort, UOSkillState>();
    readonly Dictionary<ushort, byte> _held = new Dictionary<ushort, byte>();
    readonly List<ushort> _drop = new List<ushort>();
    readonly List<GroupControl> _groups = new List<GroupControl>();
    readonly GumpPic _icon;
    readonly GumpPic _top;
    readonly GumpPicTiled _right;
    readonly GumpPicTiled _mid;
    readonly GumpPic _bottomPic;
    readonly GumpPic _title;
    readonly GumpPic _lineTop;
    readonly GumpPic _lineBot;
    readonly GumpPic _comment;
    readonly Button _expander;
    readonly Button _newGroup;
    readonly Checkbox _real;
    readonly Checkbox _caps;
    readonly Label _sum;
    readonly WordButton _reset;
    readonly GroupPane _pane;
    int _h = MinH;
    int _scroll;
    int _wheel;
    int _dragY;
    int _rev = -1;
    bool _hot;
    bool _dragging;
    bool _minimized;

    public StandardSkillsGump(World world) : base(world, 0, 0) {
        AcceptMouseInput = false;
        CanMove = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        var icon = new IconPic(this);
        _icon = icon.IsDisposed ? null : icon;
        if (_icon != null) Add(_icon);
        _top = Take(new GumpPic(0, DiffY, 0x1F40, 0));
        _right = TakeTile(0x1F41);
        _mid = TakeTile(0x1F42);
        _bottomPic = Take(new GumpPic(0, DiffY, 0x1F43, 0));
        _title = Take(new GumpPic(0, DiffY, 0x0834, 0));
        _lineTop = Take(new GumpPic(50, 35 + DiffY, 0x082B, 0));
        _lineBot = Take(new GumpPic(50, 0, 0x082B, 0));
        _comment = Take(new GumpPic(25, 0, 0x0836, 0));
        _sum = new Label("0.0", false, 600, 3);
        Add(_sum);
        var expander = new Button(0x7FBEEF, 0x082E, 0x082F) { ButtonAction = ButtonAction.Activate };
        _expander = expander.IsDisposed ? null : expander;
        if (_expander != null) Add(_expander);
        var newer = new Button(0, 0x083A, 0x083A, 0x083A) { X = 60, ButtonAction = ButtonAction.Activate };
        _newGroup = newer.IsDisposed ? null : newer;
        if (_newGroup != null) Add(_newGroup);
        var real = new Checkbox(0x938, 0x939, " - Show Real", 1, 0x0386, false);
        var caps = new Checkbox(0x938, 0x939, " - Show Caps", 1, 0x0386, false);
        _real = real.IsDisposed ? null : real;
        _caps = caps.IsDisposed ? null : caps;
        if (_real != null) {
            Add(_real);
            _real.ValueChanged += OnCheck;
        }
        if (_caps != null) {
            Add(_caps);
            _caps.ValueChanged += OnCheck;
        }
        _reset = new WordButton(this, "Reset groups", 1, 6, 0xFFFF);
        Add(_reset);
        _pane = new GroupPane { X = 22, AcceptMouseInput = true, CanMove = true, WantUpdateSize = false };
        Add(_pane);
        Add(new MinBox(this));
        Read();
        LoadSkills();
        Sum();
        _rev = UOLoginNet.WorldRevision;
        Layout();
        Arrange();
        ApplyMin();
    }

    public bool ShowReal => _real != null && !_real.IsDisposed && _real.IsChecked;
    public bool ShowCaps => _caps != null && !_caps.IsDisposed && _caps.IsChecked;

    public override void OnButtonClick(int buttonID) {
        if (buttonID == 0) AddGroup();
        else if (buttonID == 1) ConfirmReset();
    }

    public override void Update() {
        Drag();
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Read();
            for (var i = 0; i < _groups.Count; i++) _groups[i].ApplyAll();
            Sum();
        }
        Layout();
        Wheel();
        Arrange();
        ApplyMin();
        base.Update();
    }

    /// <summary>One skill changed. The row that owns the index updates, then the total.</summary>
    public void Update(int skillIndex) {
        for (var i = 0; i < _groups.Count; i++)
            if (_groups[i].UpdateSkill(skillIndex)) break;
        Sum();
    }

    static SkillsGroupManager Manager(World world) {
        if (_manager != null) return _manager;
        _manager = new SkillsGroupManager(world);
        _manager.Load();
        return _manager;
    }

    void AddGroup() {
        var group = new SkillsGroup { Name = "New Group" };
        Manager(World).Add(group);
        var control = new GroupControl(this, group);
        _pane.Add(control);
        _groups.Add(control);
        control.IsMinimized = !group.IsMaximized;
        Arrange();
    }

    void ConfirmReset() {
        UIManager.Add(new QuestionGump(World, "Skills will be placed in default groups.\nDo you want reset all groups?", yes => {
            if (!yes || IsDisposed) return;
            var manager = Manager(World);
            manager.Groups.Clear();
            manager.MakeDefault();
            for (var i = 0; i < _groups.Count; i++) {
                _groups[i].Parent = null;
                _groups[i].Dispose();
            }
            _groups.Clear();
            LoadSkills();
            Sum();
            Arrange();
        }));
    }

    void LoadSkills() {
        var groups = Manager(World).Groups;
        for (var g = 0; g < groups.Count; g++) {
            var group = groups[g];
            var control = new GroupControl(this, group);
            _pane.Add(control);
            _groups.Add(control);
            for (var i = 0; i < group.Count; i++) {
                var index = group.GetSkill(i);
                if (index == 0xFF || index >= SkillsCount) continue;
                control.AddSkill(index, 17 + i * 17);
            }
            control.IsMinimized = true;
        }
    }

    void OnCheck(object sender, EventArgs e) {
        if (ShowReal && ShowCaps) {
            if (sender == _real) _caps.IsChecked = false;
            else if (_real != null) _real.IsChecked = false;
        }
        for (var i = 0; i < _groups.Count; i++) _groups[i].ApplyAll();
        Sum();
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

    internal UOSkillState State(int index) {
        var id = (ushort)index;
        if (!_map.TryGetValue(id, out var skill))
            skill = new UOSkillState(id, 0, 0, 0, 0, null);
        if (_held.TryGetValue(id, out var next) && skill.Lock != next)
            skill = new UOSkillState(skill.Id, skill.Value, skill.BaseValue, next, skill.Cap, skill.Name);
        return skill;
    }

    internal void Hold(int index, byte next) {
        var id = (ushort)index;
        _held[id] = next;
        if (!_map.TryGetValue(id, out var skill))
            skill = new UOSkillState(id, 0, 0, 0, 0, null);
        _map[id] = new UOSkillState(skill.Id, skill.Value, skill.BaseValue, next, skill.Cap, skill.Name);
    }

    internal static string SkillName(UOSkillState skill) =>
        string.IsNullOrEmpty(skill.Name) ? "skill " + skill.Id : skill.Name;

    void Sum() {
        float sum = 0;
        var real = ShowReal;
        foreach (var skill in _map.Values)
            sum += (real ? skill.BaseValue : skill.Value) / 10f;
        _sum.Text = sum.ToString("F1");
    }

    void Drag() {
        if (_expander != null && !_expander.IsDisposed && Mouse.LButtonPressed && UIManager.LastControlMouseDown(1) == _expander) {
            if (!_dragging) {
                _dragging = true;
                _dragY = Mouse.Position.Y;
            }
            var y = Mouse.Position.Y;
            _h += y - _dragY;
            _dragY = y;
            if (_h < MinH) _h = MinH;
            if (_h > MaxH) _h = MaxH;
        }
        else _dragging = false;
    }

    void Layout() {
        Height = DiffY + _h;
        var topW = W(_top);
        var topH = H(_top);
        var rightW = W(_right);
        var midW = W(_mid);
        var botW = W(_bottomPic);
        var botH = H(_bottomPic);
        var expH = H(_expander);
        var width = topW;
        if (rightW > width) width = rightW;
        if (midW > width) width = midW;
        if (botW > width) width = botW;
        if (width < 220) width = 220;
        Width = width;
        var midH = _h - topH - botH - expH;
        if (midH < 1) midH = 1;
        if (_right != null) {
            _right.Y = DiffY + topH;
            _right.Height = midH;
            _right.X = rightW > 0 ? (width - rightW) / 2 : 0;
        }
        if (_mid != null) {
            _mid.Y = DiffY + topH;
            _mid.Height = midH;
            _mid.X = midW > 0 ? (width - midW) / 2 : 0;
        }
        if (_bottomPic != null) {
            var off = topW - botW;
            _bottomPic.X = off / 2 + off / 4;
            _bottomPic.Y = DiffY + _h - botH - expH;
        }
        if (_expander != null && !_expander.IsDisposed) {
            _expander.X = (width - _expander.Width) >> 1;
            _expander.Y = DiffY + _h - expH - 2;
        }
        if (_title != null && _top != null) {
            _title.X = _top.X + ((_top.Width - _title.Width) >> 1);
            _title.Y = _top.Y + ((_top.Height - _title.Height) >> 1);
        }
        if (_lineBot != null) _lineBot.Y = _h - 98;
        if (_comment != null) _comment.Y = _h - 85;
        if (_comment != null) {
            _sum.X = _comment.X + _comment.Width + 5;
            _sum.Y = _comment.Y + 2;
        }
        var groupY = _h - 52;
        if (_newGroup != null && !_newGroup.IsDisposed) _newGroup.Y = groupY;
        var checks = 60 + (_newGroup != null && !_newGroup.IsDisposed ? _newGroup.Width + 30 : 30);
        if (_real != null) {
            _real.X = checks;
            _real.Y = groupY - 6;
        }
        if (_caps != null) {
            _caps.X = checks;
            _caps.Y = groupY + 7;
        }
        _reset.X = 25;
        _reset.Y = DiffY + 7;
        var lineH = H(_lineTop);
        _pane.Y = 45 + DiffY + lineH - 10;
        var paneH = _h - (150 + DiffY);
        if (paneH < 1) paneH = 1;
        _pane.Height = paneH;
        _pane.Width = width - 14;
        if (_pane.Width < 1) _pane.Width = 1;
    }

    void Arrange() {
        var content = 0;
        for (var i = 0; i < _groups.Count; i++)
            if (!_groups[i].IsDisposed) content += _groups[i].Height;
        var max = content - _pane.Height;
        if (max < 0) max = 0;
        if (_scroll < 0) _scroll = 0;
        if (_scroll > max) _scroll = max;
        var y = -_scroll;
        for (var i = 0; i < _groups.Count; i++) {
            var group = _groups[i];
            if (group.IsDisposed) continue;
            group.Y = y;
            y += group.Height;
        }
    }

    void Wheel() {
        if (_minimized || !Over(_pane)) {
            _hot = false;
            return;
        }
        var w = Mouse.Current.Wheel;
        if (!_hot) {
            _hot = true;
            _wheel = w;
            return;
        }
        while (w >= _wheel + 120) {
            _wheel += 120;
            _scroll -= 20;
        }
        while (w <= _wheel - 120) {
            _wheel -= 120;
            _scroll += 20;
        }
    }

    static bool Over(Control target) {
        for (var c = UIManager.MouseOverControl; c != null; c = c.Parent)
            if (c == target) return true;
        return false;
    }

    void ApplyMin() {
        foreach (var c in Children)
            c.IsVisible = !_minimized || c == _icon;
        if (_icon == null || _icon.IsDisposed) return;
        _icon.IsVisible = true;
        _icon.Graphic = _minimized ? (ushort)0x839 : (ushort)0x82D;
        _icon.X = _minimized ? 0 : 160;
    }

    bool Minimized {
        get => _minimized;
        set {
            if (_minimized == value) return;
            _minimized = value;
            ApplyMin();
        }
    }

    GumpPic Take(GumpPic pic) {
        if (pic.IsDisposed) return null;
        Add(pic);
        return pic;
    }

    GumpPicTiled TakeTile(ushort graphic) {
        var pic = new GumpPicTiled(graphic);
        if (pic.IsDisposed) return null;
        Add(pic);
        return pic;
    }

    static int W(Control c) => c != null && !c.IsDisposed ? c.Width : 0;
    static int H(Control c) => c != null && !c.IsDisposed ? c.Height : 0;

    sealed class IconPic : GumpPic {
        readonly StandardSkillsGump _owner;

        public IconPic(StandardSkillsGump owner) : base(160, 0, 0x82D, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (!_owner.Minimized) return false;
            _owner.Minimized = false;
            return true;
        }
    }

    sealed class MinBox : Control {
        readonly StandardSkillsGump _owner;

        public MinBox(StandardSkillsGump owner) {
            _owner = owner;
            X = 160;
            Y = 0;
            Width = 23;
            Height = 24;
            AcceptMouseInput = true;
            WantUpdateSize = false;
        }

        public override void OnMouseClick(int x, int y) {
            if (!_owner.Minimized) _owner.Minimized = true;
        }
    }

    sealed class WordButton : Control {
        readonly StandardSkillsGump _owner;
        readonly string _text;
        readonly int _id;
        readonly byte _font;
        readonly ushort _hue;

        public WordButton(StandardSkillsGump owner, string text, int id, byte font, ushort hue) {
            _owner = owner;
            _text = text;
            _id = id;
            _font = font;
            _hue = hue;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            Width = GumpArt.Sink?.MeasureText(text, font) ?? text.Length * 6;
            if (Width < 1) Width = text.Length * 6;
            Height = 18;
        }

        public override void OnMouseClick(int x, int y) => _owner.OnButtonClick(_id);

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            GumpArt.Sink?.DrawText(x, y, _text, _hue, _font);
            return true;
        }
    }

    sealed class GroupPane : Control {
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

    sealed class GroupControl : Control {
        readonly StandardSkillsGump _owner;
        readonly SkillsGroup _group;
        readonly List<SkillItem> _skills = new List<SkillItem>();
        readonly Button _button;
        readonly GumpPicTiled _line;
        readonly TextBox _box;
        readonly EditHit _edit;
        readonly NameHit _name;
        byte _status;
        bool _minimized;

        public GroupControl(StandardSkillsGump owner, SkillsGroup group) {
            _owner = owner;
            _group = group;
            X = 3;
            Width = 255;
            Height = 20;
            CanMove = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            var button = new Button(1000, 0x0827, 0x0827, 0x0827) {
                ButtonAction = ButtonAction.Activate,
                IsVisible = false,
            };
            _button = button.IsDisposed ? null : button;
            if (_button != null) Add(_button);
            _name = new NameHit(this);
            Add(_name);
            var line = new GumpPicTiled(0x0835) { Y = 5, AcceptMouseInput = false };
            _line = line.IsDisposed ? null : line;
            if (_line != null) Add(_line);
            _box = new TextBox {
                X = 16,
                Y = -3,
                Width = 200,
                Height = 17,
                MaxLength = 64,
                IsVisible = false,
                AcceptKeyboardInput = false,
                AcceptMouseInput = false,
            };
            _box.Text = group.Name ?? "";
            Add(_box);
            // TextBox is sealed, so the click that leaves edit is this cover, not a subclass.
            _edit = new EditHit(this) {
                X = 16,
                Y = -3,
                Width = 200,
                Height = 17,
                IsVisible = false,
            };
            Add(_edit);
            LayoutLine();
        }

        public bool IsMinimized {
            get => _minimized;
            set {
                _minimized = value;
                if (_button != null && !_button.IsDisposed)
                    _button.ButtonGraphicNormal = (ushort)(value ? 0x0827 : 0x0826);
                for (var i = 0; i < _skills.Count; i++) _skills[i].IsVisible = !value;
                Measure();
            }
        }

        public void AddSkill(int index, int y) {
            var row = new SkillItem(_owner, index) { Y = y, IsVisible = !_minimized };
            _skills.Add(row);
            Add(row);
            if (_button != null && !_button.IsDisposed) _button.IsVisible = true;
            Measure();
        }

        public void ApplyAll() {
            for (var i = 0; i < _skills.Count; i++) _skills[i].Apply();
        }

        public bool UpdateSkill(int index) {
            for (var i = 0; i < _skills.Count; i++) {
                if (_skills[i].Index != index) continue;
                _skills[i].Apply();
                return true;
            }
            return false;
        }

        public override void OnButtonClick(int buttonID) {
            if (buttonID == 1000) IsMinimized = !IsMinimized;
        }

        public override void OnKeyboardReturn(int textID, string text) {
            if (_status != 2) return;
            _status = 0;
            Commit(text);
            ApplyStatus();
            if (UIManager.KeyboardFocusControl == _box) UIManager.KeyboardFocusControl = null;
        }

        public override void Update() {
            if (_status == 2 && UIManager.KeyboardFocusControl != _box) {
                _status = 0;
                Commit(_box.Text);
                ApplyStatus();
            }
            base.Update();
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (_status == 2 && sink != null)
                sink.DrawRaw(FillBeige, x, y, Width, 17, Beige, 1, 1);
            else if (_status == 1 && sink != null)
                sink.DrawRaw(FillBisque, x + 16, y, 200, 17, Bisque, 1, 1);
            return base.Draw(x, y);
        }

        internal bool Editing => _status == 2;

        internal void Cycle() {
            var was = _status;
            _status++;
            if (_status >= 3) _status = 0;
            if (was == 2) Commit(_box.Text);
            ApplyStatus();
        }

        internal string Caption => string.IsNullOrEmpty(_group.Name) ? "No Name" : _group.Name;

        void Commit(string text) {
            if (string.IsNullOrWhiteSpace(text)) text = "No Name";
            _group.Name = text;
            _box.Text = text;
            LayoutLine();
        }

        void ApplyStatus() {
            if (_name != null) _name.IsVisible = _status != 2;
            _box.IsVisible = _status == 2;
            _box.AcceptKeyboardInput = _status == 2;
            _edit.IsVisible = _status == 2;
            if (_status == 2) {
                _box.Text = _group.Name ?? "";
                _box.SetKeyboardFocus();
            }
            LayoutLine();
        }

        void LayoutLine() {
            if (_line == null || _line.IsDisposed) return;
            var text = Caption;
            var width = GumpArt.Sink?.MeasureText(text, 6) ?? text.Length * 6;
            var xx = width + 11 + 16;
            _line.IsVisible = _status != 2 && xx > 0 && xx < 215;
            _line.X = xx;
            _line.Y = 5;
            _line.Width = 215 - xx;
            if (_line.Width < 1) _line.IsVisible = false;
        }

        void Measure() {
            Height = _minimized || _skills.Count == 0 ? 20 : 17 + _skills.Count * 17;
        }

        sealed class EditHit : Control {
            readonly GroupControl _owner;

            public EditHit(GroupControl owner) {
                _owner = owner;
                AcceptMouseInput = true;
                CanMove = false;
                WantUpdateSize = false;
            }

            public override void OnMouseClick(int x, int y) {
                if (_owner.Editing) _owner.Cycle();
            }
        }

        sealed class NameHit : Control {
            readonly GroupControl _owner;

            public NameHit(GroupControl owner) {
                _owner = owner;
                X = 16;
                Y = -3;
                Width = 200;
                Height = 17;
                AcceptMouseInput = true;
                CanMove = false;
                WantUpdateSize = false;
            }

            public override void OnMouseClick(int x, int y) => _owner.Cycle();

            public override bool Draw(int x, int y) {
                if (IsDisposed || !IsVisible) return false;
                GumpArt.Sink?.DrawText(x, y, _owner.Caption, 0, 6);
                return true;
            }
        }
    }

    sealed class SkillItem : Control {
        readonly StandardSkillsGump _owner;
        readonly Button _lock;

        public SkillItem(StandardSkillsGump owner, int index) {
            _owner = owner;
            Index = index;
            Width = 255;
            Height = 17;
            CanMove = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            var skill = owner.State(index);
            Add(new Label(SkillName(skill), false, 0x0288, 9) { X = 22 });
            var value = new Label("", false, 0x0288, 9);
            Add(value);
            ValueLabel = value;
            var graphic = LockGraphic(skill.Lock);
            var button = new Button(1, graphic, graphic, graphic) {
                X = 251,
                ButtonAction = ButtonAction.Activate,
            };
            if (button.IsDisposed) _lock = null;
            else {
                _lock = button;
                Add(button);
            }
            Apply();
        }

        public int Index { get; }
        Label ValueLabel { get; }

        public void Apply() {
            var skill = _owner.State(Index);
            var n = _owner.ShowReal ? skill.BaseValue : _owner.ShowCaps ? skill.Cap : skill.Value;
            ValueLabel.Text = (n / 10f).ToString("F1");
            ValueLabel.X = 250 - ValueLabel.Width;
            if (_lock != null && !_lock.IsDisposed) _lock.ButtonGraphicNormal = LockGraphic(skill.Lock);
        }

        public override void OnButtonClick(int buttonID) {
            if (buttonID != 1) return;
            var skill = _owner.State(Index);
            var next = skill.Lock < 2 ? (byte)(skill.Lock + 1) : (byte)0;
            UOLoginNet.Send(OutgoingPackets.Send_SkillStatusChangeRequest((ushort)Index, next));
            _owner.Hold(Index, next);
            Apply();
        }

        public override void OnMouseClick(int x, int y) {
            if (!GameController.Shift) return;
            var open = SkillButtonGump.Find(Index);
            if (open != null) {
                open.Show();
                return;
            }
            var skill = _owner.State(Index);
            UIManager.Add(new SkillButtonGump(_owner.World, Index, SkillName(skill), Mouse.Position.X - 44, Mouse.Position.Y - 22));
        }

        public override bool OnMouseDoubleClick(int x, int y) {
            UOLoginNet.UseSkill(Index);
            return true;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            if (UIManager.LastControlMouseDown(1) == this)
                GumpArt.Sink?.DrawRaw(FillWheat, x, y, Width, Height, Wheat, 1, 1);
            return base.Draw(x, y);
        }

        static ushort LockGraphic(byte lockState) {
            switch (lockState) {
                case 1: return 0x0986;
                case 2: return 0x082C;
                default: return 0x0984;
            }
        }
    }
}
