// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ResizableJournal. The tabs start as All, Chat, Guild|Party, and System and stay in memory.
/// No profile file. Rows are JournalManager.Entries. A name on IgnoreManager is skipped.
/// The frame is 0x0A8C and 0x0A8D. The 0.7 black fill is an opaque dark pixel.
/// Right-click on a tab opens the filter list. Right-click elsewhere closes. The grip resizes.
/// A journal line does not wrap. GmChat has no caption in ClassicUO, so it is not listed.
/// </summary>
public sealed class ResizableJournal : Gump {
    const int Border = 4;
    const int MinHeight = 100;
    const int BarW = 14;
    const int TabW = 80;
    const int TabH = 30;
    const int ScrollStep = 50;
    const ushort HBorder = 0x0A8C;
    const ushort VBorder = 0x0A8D;
    const int FillKey = 0x57200031;
    const int WhiteKey = 0x57200032;

    static readonly byte[] Dark = { 0x84, 0x90 };
    static readonly byte[] White = { 0xFF, 0xFF };
    static readonly List<TabInfo> Tabs = new List<TabInfo>();
    static int _lastX = 100;
    static int _lastY = 100;
    static int _lastW = 8 + TabW * 4 + 20;
    static int _lastH = 350;

    readonly List<TabButton> _tabs = new List<TabButton>();
    readonly List<Row> _rows = new List<Row>();
    readonly JournalLog _log;
    readonly JournalBar _bar;
    readonly AddTab _add;
    readonly Button _grip;
    int _stamp = int.MinValue;
    int _minW;
    int _savedW;
    int _savedH;
    int _selected = -1;
    string _filterName;
    bool _sizing;

    public static bool ReloadTabs { get; set; }

    static ResizableJournal() {
        AddDefault("All", MessageType.Alliance, MessageType.Command, MessageType.Emote, MessageType.Encoded, MessageType.Focus, MessageType.Guild, MessageType.Label, MessageType.Limit3Spell, MessageType.Party, MessageType.Regular, MessageType.Spell, MessageType.System, MessageType.Whisper, MessageType.Yell);
        AddDefault("Chat", MessageType.Regular, MessageType.Guild, MessageType.Alliance, MessageType.Emote, MessageType.Party, MessageType.Whisper, MessageType.Yell);
        AddDefault("Guild|Party", MessageType.Guild, MessageType.Alliance, MessageType.Party);
        AddDefault("System", MessageType.System);
    }

    public ResizableJournal(World world) : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        X = _lastX;
        Y = _lastY;
        Width = _lastW;
        Height = _lastH;
        _minW = MinWidth(Tabs.Count);
        _log = new JournalLog(this) { AcceptMouseInput = true, CanMove = true, WantUpdateSize = false };
        Add(_log);
        _bar = new JournalBar();
        Add(_bar);
        _add = new AddTab(this);
        Add(_add);
        var grip = new Button(0, 0x0837, 0x0838, 0x0838) { ButtonAction = ButtonAction.Activate, CanMove = false };
        if (grip.IsDisposed) _grip = null;
        else {
            if (grip.Width < 1) grip.Width = 16;
            if (grip.Height < 1) grip.Height = 16;
            _grip = grip;
            Add(_grip);
        }
        BuildTabs();
        Place();
        Fill();
        _stamp = Stamp();
    }

    public override void CloseWithRightClick() {
        if (UIManager.MouseOverControl is TabButton tab) {
            UIManager.GetGump<TabMenu>()?.Dispose();
            UIManager.Add(new TabMenu(this, tab.Caption));
            return;
        }
        base.CloseWithRightClick();
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID < 0 || buttonID >= Tabs.Count) return;
        _selected = buttonID;
        _filterName = Tabs[buttonID].Name;
        for (var i = 0; i < _tabs.Count; i++) _tabs[i].Selected = i == buttonID;
        _log.Recalc(true);
    }

    public override void Update() {
        if (IsDisposed) return;
        Resize();
        if (X != _lastX || Y != _lastY) {
            _lastX = X;
            _lastY = Y;
        }
        if (Width != _lastW || Height != _lastH) {
            _lastW = Width;
            _lastH = Height;
        }
        if (ReloadTabs) {
            ReloadTabs = false;
            BuildTabs();
        }
        Place();
        var stamp = Stamp();
        if (stamp != _stamp) {
            _stamp = stamp;
            Fill();
        }
        else _log.Recalc(false);
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var sink = GumpArt.Sink;
        var innerW = Width - Border * 2;
        var innerH = Height - Border * 2;
        if (sink != null && innerW > 0 && innerH > 0)
            sink.DrawRaw(FillKey, x + Border, y + Border, innerW, innerH, Dark, 1, 1);
        GumpArtDraw.Pic(HBorder, x, y, Width, Border, true);
        GumpArtDraw.Pic(HBorder, x, y + Height - Border, Width, Border, true);
        GumpArtDraw.Pic(VBorder, x, y, Border, Height, true);
        GumpMetrics.TrySize(VBorder, out var vw, out _);
        GumpArtDraw.Pic(VBorder, x + Width - Border, y + (vw >> 1), Border, Height - Border, true);
        return base.Draw(x, y);
    }

    public override void Dispose() {
        UIManager.GetGump<TabMenu>()?.Dispose();
        ClearRows();
        base.Dispose();
    }

    internal void AddNamed(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;
        name = name.Trim();
        for (var i = 0; i < Tabs.Count; i++)
            if (Tabs[i].Name == name) return;
        var tab = new TabInfo { Name = name };
        tab.Types.Add(MessageType.Regular);
        Tabs.Add(tab);
        ReloadTabs = true;
    }

    internal void DeleteTab(string name) {
        for (var i = 0; i < Tabs.Count; i++) {
            if (Tabs[i].Name != name) continue;
            if (_selected == i) {
                _selected = -1;
                _filterName = null;
            }
            else if (_selected > i) _selected--;
            Tabs.RemoveAt(i);
            break;
        }
        ReloadTabs = true;
    }

    internal bool Pass(byte textType, MessageType message) {
        List<MessageType> filter = null;
        if (_filterName != null) {
            for (var i = 0; i < Tabs.Count; i++)
                if (Tabs[i].Name == _filterName) {
                    filter = Tabs[i].Types;
                    break;
                }
        }
        if (filter == null) return true;
        for (var i = 0; i < filter.Count; i++) {
            var current = filter[i];
            if (textType == 1 && current == MessageType.System) return true;
            if (textType == 1 && current != MessageType.System) continue;
            if (current == message) return true;
        }
        return false;
    }

    void BuildTabs() {
        var keep = _selected;
        for (var i = 0; i < _tabs.Count; i++) {
            _tabs[i].Parent = null;
            _tabs[i].Dispose();
        }
        _tabs.Clear();
        for (var i = 0; i < Tabs.Count; i++) {
            var button = new TabButton(this, i, Tabs[i].Name);
            Add(button);
            _tabs.Add(button);
        }
        _add.X = Tabs.Count * TabW + 4;
        _minW = MinWidth(Tabs.Count);
        if (Width < _minW) Width = _minW;
        if (keep >= 0 && keep < _tabs.Count) {
            _selected = keep;
            _tabs[keep].Selected = true;
            _filterName = Tabs[keep].Name;
        }
        else {
            _selected = -1;
            _filterName = null;
        }
    }

    void Fill() {
        var atEnd = _bar.Value >= _bar.MaxValue;
        ClearRows();
        foreach (var entry in JournalManager.Entries) {
            if (entry == null) continue;
            if (!string.IsNullOrEmpty(entry.Name) && IgnoreManager.Instance.IgnoredCharsList.Contains(entry.Name))
                continue;
            var time = new Label($"{entry.Time:t}", entry.IsUnicode, entry.Hue, entry.Font);
            var text = new Label($"{entry.Name}: {entry.Text}", entry.IsUnicode, entry.Hue, entry.Font);
            _rows.Add(new Row(text, time, entry.TextType, entry.MessageType));
        }
        _log.Recalc(atEnd);
    }

    void ClearRows() {
        for (var i = 0; i < _rows.Count; i++) _rows[i].Destroy();
        _rows.Clear();
    }

    void Place() {
        var jh = Height - Border - TabH;
        if (jh < 1) jh = 1;
        var jw = Width - BarW - Border * 2;
        if (jw < 1) jw = 1;
        _log.X = Border;
        _log.Y = TabH;
        _log.Width = jw;
        _log.Height = jh;
        _bar.X = Width - BarW - Border;
        _bar.Y = _log.Y;
        _bar.Height = jh;
        _bar.IsVisible = _bar.MaxValue > _bar.MinValue;
        if (_grip != null && !_grip.IsDisposed) {
            _grip.X = Width - _grip.Width + 2;
            _grip.Y = Height - _grip.Height + 2;
        }
        for (var i = 0; i < _tabs.Count; i++) {
            _tabs[i].X = i * TabW + 4;
            _tabs[i].Y = 0;
        }
    }

    void Resize() {
        if (_grip == null || _grip.IsDisposed) return;
        if (Mouse.LButtonPressed && UIManager.LastControlMouseDown(1) == _grip) {
            if (!_sizing) {
                _sizing = true;
                _savedW = Width;
                _savedH = Height;
            }
            var offset = Mouse.LDragOffset;
            var w = _savedW + offset.X;
            var h = _savedH + offset.Y;
            if (w < _minW) w = _minW;
            if (h < MinHeight) h = MinHeight;
            Width = w;
            Height = h;
        }
        else _sizing = false;
    }

    static int Stamp() {
        var hash = JournalManager.Entries.Count;
        foreach (var entry in JournalManager.Entries) {
            if (entry == null) continue;
            hash = unchecked(hash * 31 + (entry.Text != null ? entry.Text.GetHashCode() : 0));
            hash = unchecked(hash * 31 + (entry.Name != null ? entry.Name.GetHashCode() : 0));
            hash = unchecked(hash * 31 + entry.Time.GetHashCode());
            hash = unchecked(hash * 31 + entry.Hue);
            hash = unchecked(hash * 31 + entry.TextType);
            hash = unchecked(hash * 31 + (int)entry.MessageType);
        }
        return hash;
    }

    static int MinWidth(int tabs) => Border * 2 + TabW * tabs + 20;

    static void AddDefault(string name, params MessageType[] types) {
        var tab = new TabInfo { Name = name };
        for (var i = 0; i < types.Length; i++) tab.Types.Add(types[i]);
        Tabs.Add(tab);
    }

    sealed class TabInfo {
        public string Name = "";
        public readonly List<MessageType> Types = new List<MessageType>();
    }

    sealed class Row {
        public Row(Label text, Label time, byte textType, MessageType message) {
            Text = text;
            Time = time;
            TextType = textType;
            Message = message;
        }

        public Label Text { get; }
        public Label Time { get; }
        public byte TextType { get; }
        public MessageType Message { get; }

        public void Destroy() {
            Text?.Dispose();
            Time?.Dispose();
        }
    }

    sealed class JournalLog : Control {
        readonly ResizableJournal _owner;
        int _wheel;
        bool _hot;

        public JournalLog(ResizableJournal owner) => _owner = owner;

        public void Recalc(bool stick) {
            var atEnd = stick || _owner._bar.Value >= _owner._bar.MaxValue;
            var height = 0;
            var rows = _owner._rows;
            for (var i = 0; i < rows.Count; i++) {
                var row = rows[i];
                if (row != null && _owner.Pass(row.TextType, row.Message))
                    height += row.Text.Height;
            }
            height -= Height;
            if (height > 0) {
                _owner._bar.MaxValue = height;
                if (atEnd) _owner._bar.Value = _owner._bar.MaxValue;
            }
            else {
                _owner._bar.MaxValue = 0;
                _owner._bar.Value = 0;
            }
        }

        public override void Update() {
            Wheel();
            base.Update();
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var rows = _owner._rows;
            var scroll = _owner._bar.Value;
            var my = 0;
            for (var i = 0; i < rows.Count; i++) {
                var row = rows[i];
                if (row == null || row.Text == null || string.IsNullOrEmpty(row.Text.Text)) continue;
                if (!_owner.Pass(row.TextType, row.Message)) continue;
                var h = row.Text.Height;
                if (my + h >= scroll && my <= scroll + Height) {
                    var dy = y + my - scroll;
                    row.Time.Draw(x, dy);
                    row.Text.Draw(x + row.Time.Width + 5, dy);
                }
                my += h;
            }
            return true;
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
            while (w >= _wheel + 120) {
                _wheel += 120;
                _owner._bar.Value -= ScrollStep;
            }
            while (w <= _wheel - 120) {
                _wheel -= 120;
                _owner._bar.Value += ScrollStep;
            }
        }

        bool Over() {
            for (var c = UIManager.MouseOverControl; c != null; c = c.Parent)
                if (c == this || c == _owner) return true;
            return false;
        }
    }

    sealed class JournalBar : Control {
        readonly Button _up;
        readonly Button _down;
        int _value;
        int _max;

        public JournalBar() {
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            Width = BarW;
            var up = new Button(1, 251, 250) { ButtonAction = ButtonAction.Activate };
            var down = new Button(2, 253, 252) { ButtonAction = ButtonAction.Activate };
            _up = up.IsDisposed ? null : up;
            _down = down.IsDisposed ? null : down;
            if (_up != null) {
                if (_up.Width < 1) _up.Width = BarW;
                if (_up.Height < 1) _up.Height = 16;
                Add(_up);
            }
            if (_down != null) {
                if (_down.Width < 1) _down.Width = BarW;
                if (_down.Height < 1) _down.Height = 16;
                Add(_down);
            }
            var track = new Track(this);
            Add(track);
        }

        public int MinValue => 0;

        public int Value {
            get => _value;
            set {
                if (value < 0) value = 0;
                if (value > _max) value = _max;
                _value = value;
            }
        }

        public int MaxValue {
            get => _max;
            set {
                _max = value < 0 ? 0 : value;
                if (_value > _max) _value = _max;
            }
        }

        public override void OnButtonClick(int buttonID) {
            if (buttonID == 1) Value -= ScrollStep;
            else if (buttonID == 2) Value += ScrollStep;
        }

        public override void Update() {
            var downH = _down != null ? _down.Height : 16;
            var upH = _up != null ? _up.Height : 16;
            if (_down != null) _down.Y = Height - downH;
            if (Children.Count > 0) {
                var track = Children[Children.Count - 1];
                track.Y = upH;
                track.Height = Height - upH - downH;
                if (track.Height < 1) track.Height = 1;
                track.Width = Width;
            }
            base.Update();
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var upH = _up != null ? _up.Height : 16;
            var downH = _down != null ? _down.Height : 16;
            GumpMetrics.TrySize(257, out _, out var h0);
            GumpMetrics.TrySize(255, out _, out var h2);
            if (h0 < 1) h0 = 8;
            if (h2 < 1) h2 = 8;
            var middle = Height - upH - downH - h0 - h2;
            if (middle > 0) {
                GumpArtDraw.Pic(257, x, y + upH, Width, h0, false);
                GumpArtDraw.Pic(256, x, y + upH + h0, Width, middle, true);
                GumpArtDraw.Pic(255, x, y + Height - downH - h2, Width, h2, false);
            }
            else {
                var span = Height - upH - downH;
                if (span < 1) span = 1;
                GumpArtDraw.Pic(256, x, y + upH, Width, span, true);
            }
            var drawn = base.Draw(x, y);
            if (_max > 0) {
                GumpMetrics.TrySize(254, out var sw, out var sh);
                if (sh < 1) sh = 16;
                if (sw < 1) sw = Width;
                var track = Height - upH - downH - sh;
                if (track < 0) track = 0;
                var slider = upH + (int)Math.Round(track * (_value / (float)_max));
                var sx = x + ((Width - sw) >> 1);
                GumpArtDraw.Pic(254, sx, y + slider, sw, sh, false);
            }
            return drawn;
        }

        internal void SetFromLocal(int localY) {
            var upH = _up != null ? _up.Height : 16;
            GumpMetrics.TrySize(254, out _, out var sh);
            if (sh < 1) sh = 16;
            var track = Height - upH - (_down != null ? _down.Height : 16) - sh;
            if (track < 1) track = 1;
            var t = localY - upH - (sh >> 1);
            if (t < 0) t = 0;
            if (t > track) t = track;
            Value = _max * t / track;
        }

        sealed class Track : Control {
            readonly JournalBar _bar;

            public Track(JournalBar bar) {
                _bar = bar;
                AcceptMouseInput = true;
                CanMove = false;
                WantUpdateSize = false;
            }

            public override void OnMouseClick(int x, int y) => _bar.SetFromLocal(y - Top(this));

            public override void Update() {
                if (Mouse.LButtonPressed && UIManager.LastControlMouseDown(1) == this)
                    _bar.SetFromLocal(Mouse.Position.Y - Top(this));
                base.Update();
            }

            static int Top(Control c) {
                var y = 0;
                for (var p = c; p != null; p = p.Parent) y += p.Y;
                return y;
            }
        }
    }

    sealed class TabButton : Control {
        readonly ResizableJournal _owner;

        public TabButton(ResizableJournal owner, int index, string caption) {
            _owner = owner;
            Index = index;
            Caption = caption ?? "";
            Width = TabW;
            Height = TabH;
            AcceptMouseInput = true;
            CanMove = false;
            CanCloseWithRightClick = false;
            WantUpdateSize = false;
        }

        public int Index { get; }
        public string Caption { get; }
        public bool Selected { get; set; }

        public override void OnMouseClick(int x, int y) => _owner.OnButtonClick(Index);

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (Selected && sink != null)
                sink.DrawRaw(WhiteKey, x, y, Width, Height, White, 1, 1);
            var tw = sink?.MeasureText(Caption, 1) ?? Caption.Length * 6;
            var tx = x + ((Width - tw) >> 1);
            if (tx < x) tx = x;
            var ty = y + ((Height - 16) >> 1);
            if (ty < y) ty = y;
            sink?.DrawText(tx, ty, Caption, 0, 1);
            return true;
        }
    }

    sealed class AddTab : Control {
        readonly ResizableJournal _owner;

        public AddTab(ResizableJournal owner) {
            _owner = owner;
            Width = 20;
            Height = TabH;
            AcceptMouseInput = true;
            CanMove = false;
            CanCloseWithRightClick = false;
            WantUpdateSize = false;
        }

        public override void OnMouseClick(int x, int y) {
            UIManager.Add(new NameBox(_owner.World, _owner.AddNamed));
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            const string text = "+";
            var tw = sink?.MeasureText(text, 1) ?? 6;
            sink?.DrawText(x + ((Width - tw) >> 1), y + ((Height - 16) >> 1), text, 0, 1);
            return true;
        }
    }

    sealed class NameBox : Gump {
        readonly TextBox _box;
        readonly Action<string> _action;
        bool _sent;

        public NameBox(World world, Action<string> action) : base(world, 0, 0) {
            _action = action;
            LayerOrder = UILayer.Over;
            CanMove = false;
            CanCloseWithRightClick = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            Width = 250;
            Height = 150;
            Add(new ResizePic(0x0A28) { Width = Width, Height = Height });
            var caption = new Label("Enter a tab name", false, 0x0386, 1) { X = 40, Y = 45 };
            Add(caption);
            _box = new TextBox {
                X = 42,
                Y = 45 + caption.Height + 7,
                Width = Width - 94,
                Height = 20,
                Hue = 0x0481,
                MaxLength = 32,
            };
            Add(_box);
            var ok = new Button(0, 0x0481, 0x0482, 0x0483) { ButtonAction = ButtonAction.Activate };
            if (!ok.IsDisposed) {
                ok.Y = Height - 45;
                ok.X = (Width - ok.Width) >> 1;
                Add(ok);
            }
            else {
                var word = new OkWord(this) { Y = Height - 40 };
                word.X = (Width - word.Width) >> 1;
                Add(word);
            }
            var sample = Mouse.Current;
            var bw = sample.BufW > 0 ? sample.BufW : 640;
            var bh = sample.BufH > 0 ? sample.BufH : 480;
            X = (bw - Width) >> 1;
            Y = (bh - Height) >> 1;
            _box.SetKeyboardFocus();
        }

        public override void OnButtonClick(int buttonID) => Submit();

        public override void OnKeyboardReturn(int textID, string text) => Submit();

        void Submit() {
            if (_sent || IsDisposed) return;
            _sent = true;
            _action?.Invoke(_box.Text);
            Dispose();
        }

        sealed class OkWord : Label {
            readonly NameBox _owner;

            public OkWord(NameBox owner) : base("OK", false, 0x0386, 1) {
                _owner = owner;
                AcceptMouseInput = true;
            }

            public override void OnMouseClick(int x, int y) => _owner.Submit();
        }
    }

    sealed class TabMenu : Gump {
        readonly ResizableJournal _owner;
        readonly string _name;

        public TabMenu(ResizableJournal owner, string name) : base(owner.World, 0, 0) {
            _owner = owner;
            _name = name ?? "";
            LayerOrder = UILayer.Over;
            CanMove = true;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            X = Mouse.Position.X;
            Y = Mouse.Position.Y;
            var y = 8;
            var values = (MessageType[])Enum.GetValues(typeof(MessageType));
            TabInfo tab = null;
            for (var i = 0; i < Tabs.Count; i++)
                if (Tabs[i].Name == _name) tab = Tabs[i];
            for (var i = 0; i < values.Length; i++) {
                var item = values[i];
                var caption = Caption(item);
                if (caption.Length == 0) continue;
                var box = new Checkbox(0x938, 0x939, caption, 1, 0x0386, true);
                if (box.IsDisposed) continue;
                box.X = 8;
                box.Y = y;
                var on = tab != null && tab.Types.Contains(item);
                if (on) box.IsChecked = true;
                var captured = item;
                var box2 = box;
                box.ValueChanged += (s, e) => Toggle(captured, box2.IsChecked);
                Add(box);
                y += box.Height + 2;
            }
            var delete = new DeleteRow(this) { X = 8, Y = y + 6 };
            Add(delete);
            Width = 220;
            Height = delete.Y + delete.Height + 8;
        }

        void Toggle(MessageType item, bool on) {
            if (_owner.IsDisposed) return;
            TabInfo tab = null;
            for (var i = 0; i < Tabs.Count; i++)
                if (Tabs[i].Name == _name) tab = Tabs[i];
            if (tab == null) return;
            var has = tab.Types.Contains(item);
            if (on && !has) tab.Types.Add(item);
            if (!on && has) tab.Types.Remove(item);
            _owner._log.Recalc(false);
        }

        void AskDelete() {
            var owner = _owner;
            var name = _name;
            Dispose();
            UIManager.Add(new QuestionGump(owner.World, "Delete [" + name + "] tab?", yes => {
                if (yes && !owner.IsDisposed) owner.DeleteTab(name);
            }));
        }

        static string Caption(MessageType item) {
            switch (item) {
                case MessageType.Regular: return "Regular";
                case MessageType.System: return "System";
                case MessageType.Emote: return "Emote";
                case MessageType.Limit3Spell: return "Limit3Spell(Sphere)";
                case MessageType.Label: return "Label";
                case MessageType.Focus: return "Focus";
                case MessageType.Whisper: return "Whisper";
                case MessageType.Yell: return "Yell";
                case MessageType.Spell: return "Spell";
                case MessageType.Guild: return "Guild";
                case MessageType.Alliance: return "Alliance";
                case MessageType.Command: return "Command";
                case MessageType.Encoded: return "Encoded";
                case MessageType.Party: return "Party";
                default: return "";
            }
        }

        sealed class DeleteRow : Label {
            readonly TabMenu _owner;

            public DeleteRow(TabMenu owner) : base("X Delete Tab", false, 0x0386, 1) {
                _owner = owner;
                AcceptMouseInput = true;
            }

            public override void OnMouseClick(int x, int y) => _owner.AskDelete();
        }
    }
}
