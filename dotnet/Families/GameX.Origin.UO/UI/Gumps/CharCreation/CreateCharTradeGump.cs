// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO custom-skill page. Skill names are the skills.mul order.
/// Stealth, Remove Trap, and Spellweaving stay out. Expansion skills follow 0xB9, or the client version when that is still zero.
/// </summary>
public class CreateCharTradeGump : Gump {
    readonly CharDraft _character;
    readonly CharSlider[] _attributes;
    readonly CharSlider[] _skillSliders;
    readonly CharCombo[] _skills;
    readonly int[] _skillIndex;
    CharComboList _open;

    public CreateCharTradeGump(World world, CharDraft character, ProfessionInfo profession) : base(world, 0, 0) {
        _character = character;
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        if (_character != null)
            for (var i = 0; i < _character.Skills.Length; i++) _character.Skills[i] = 0;

        Add(new ResizePic(2600) { X = 100, Y = 80, Width = 470, Height = 372 });
        AddPic(291, 42, 0x0589);
        AddPic(214, 58, 0x058B);
        AddPic(300, 51, 0x15A9);
        Add(new Label(CharCreationGump.Cliloc(3000326, "Choose a Trade for Your Character"), false, 0x0386, 2) { X = 148, Y = 132 });
        Add(new Label(CharCreationGump.Cliloc(3000111, "Strength"), false, 1, 1) { X = 158, Y = 170 });
        Add(new Label(CharCreationGump.Cliloc(3000112, "Dexterity"), false, 1, 1) { X = 158, Y = 250 });
        Add(new Label(CharCreationGump.Cliloc(3000113, "Intelligence"), false, 1, 1) { X = 158, Y = 330 });

        var (skills, stats) = ProfessionInfo.GetDefaults(CharCreationGump.Version);
        _attributes = new CharSlider[3];
        _attributes[0] = AddSlider(164, 196, 10, 60, stats[0]);
        _attributes[1] = AddSlider(164, 276, 10, 60, stats[1]);
        _attributes[2] = AddSlider(164, 356, 10, 60, stats[2]);
        Pair(_attributes);

        var names = new List<string>();
        var indexes = new List<int>();
        var race = _character?.Race ?? (byte)1;
        for (var i = 0; i < CharSkillNames.Names.Length; i++) {
            if (!Keep(i, race)) continue;
            names.Add(CharSkillNames.Names[i]);
            indexes.Add(i);
        }
        _skillIndex = indexes.ToArray();
        var count = CharCreationGump.SkillsCount;
        _skills = new CharCombo[count];
        _skillSliders = new CharSlider[count];
        var y = 172;
        for (var i = 0; i < count; i++) {
            _skills[i] = new CharCombo(this, 344, y, 182, names.ToArray(), "Click here");
            Add(_skills[i]);
            _skillSliders[i] = AddSlider(344, y + 32, 0, 50, skills[i, 1]);
            y += 70;
        }
        Pair(_skillSliders);

        var prev = new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2) { X = 586, Y = 445, ButtonAction = ButtonAction.Activate };
        var next = new Button((int)Buttons.Next, 0x15A4, 0x15A6, 0x15A5) { X = 610, Y = 445, ButtonAction = ButtonAction.Activate };
        if (!prev.IsDisposed) Add(prev);
        if (!next.IsDisposed) Add(next);
    }

    public override void OnButtonClick(int buttonID) {
        var shell = CharCreationGump.Shell(this);
        switch ((Buttons)buttonID) {
            case Buttons.Prev:
                shell?.StepBack();
                break;
            case Buttons.Next:
                if (!Valid(shell) || _character == null) break;
                for (var i = 0; i < _skills.Length; i++) {
                    var pick = _skills[i].SelectedIndex;
                    if (pick < 0 || pick >= _skillIndex.Length) continue;
                    _character.Skills[_skillIndex[pick]] = (ushort)_skillSliders[i].Value;
                }
                // ClassicUO stores slider 0, 1, 2 as strength, intelligence, dexterity.
                _character.Strength = (ushort)_attributes[0].Value;
                _character.Intelligence = (ushort)_attributes[1].Value;
                _character.Dexterity = (ushort)_attributes[2].Value;
                shell?.SetAttributes(true);
                break;
        }
    }

    public override void Dispose() {
        if (_open != null && !_open.IsDisposed) _open.Dispose();
        _open = null;
        base.Dispose();
    }

    void OpenCombo(CharCombo combo) {
        if (_open != null && !_open.IsDisposed) _open.Dispose();
        _open = new CharComboList(this, combo);
        UIManager.Add(_open);
    }

    bool Valid(CharCreationGump shell) {
        var missing = false;
        var dup = false;
        for (var i = 0; i < _skills.Length; i++) {
            var pick = _skills[i].SelectedIndex;
            if (pick < 0) { missing = true; break; }
            for (var j = 0; j < i; j++)
                if (_skills[j].SelectedIndex == pick) dup = true;
        }
        if (missing) {
            var text = CharCreationGump.Version <= ClientVersion.CV_5090
                ? "You must have three unique skills chosen!"
                : CharCreationGump.Cliloc(1080032, "You must have three unique skills chosen!");
            shell?.ShowMessage(text);
            return false;
        }
        if (dup) {
            shell?.ShowMessage(CharCreationGump.Cliloc(1080032, "You must have three unique skills chosen!"));
            return false;
        }
        return true;
    }

    static bool Keep(int index, byte race) {
        if (index == 47 || index == 48 || index == 54) return false;
        if (race != 3 && index == 57) return false;
        if (race == 3 && index == 31) return false;
        if (!CharCreationGump.Expansion(CharCreationGump.LockedAOS, ClientVersion.CV_308Z) && (index == 49 || index == 50 || index == 51))
            return false;
        if (!CharCreationGump.Expansion(CharCreationGump.LockedSE, ClientVersion.CV_405A) && (index == 52 || index == 53))
            return false;
        if (!CharCreationGump.Expansion(CharCreationGump.LockedSA, ClientVersion.CV_60144) && (index == 55 || index == 56))
            return false;
        return true;
    }

    CharSlider AddSlider(int x, int y, int min, int max, int value) {
        var slider = new CharSlider(x, y, 93, min, max, value);
        Add(slider);
        return slider;
    }

    static void Pair(CharSlider[] sliders) {
        for (var i = 0; i < sliders.Length; i++)
            for (var j = 0; j < sliders.Length; j++)
                if (i != j) sliders[i].Pair(sliders[j]);
    }

    void AddPic(int x, int y, ushort graphic) {
        var pic = new GumpPic(x, y, graphic, 0) { AcceptMouseInput = false, CanMove = false };
        if (!pic.IsDisposed) Add(pic);
    }

    enum Buttons { Prev, Next }

    sealed class CharCombo : Control {
        readonly CreateCharTradeGump _owner;
        readonly string[] _items;
        readonly string _empty;
        readonly Label _label;
        int _index = -1;

        public CharCombo(CreateCharTradeGump owner, int x, int y, int width, string[] items, string empty) {
            _owner = owner;
            _items = items ?? [];
            _empty = empty ?? "";
            X = x;
            Y = y;
            Width = width;
            Height = 25;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            Add(new ResizePic(0x0BB8) { Width = width, Height = 25, AcceptMouseInput = false, CanMove = false });
            _label = new Label(_empty, false, 0x0453, 9) { X = 2, Y = 5 };
            Add(_label);
            var arrow = new GumpPic(width - 18, 2, 0x00FC, 0) { AcceptMouseInput = false, CanMove = false };
            if (!arrow.IsDisposed) Add(arrow);
        }

        public int SelectedIndex => _index;
        public string[] Items => _items;

        public void Choose(int index) {
            if (index < 0 || index >= _items.Length) return;
            _index = index;
            _label.Text = _items[index];
        }

        public override void OnMouseClick(int x, int y) => _owner.OpenCombo(this);
    }

    sealed class CharComboList : Gump {
        readonly CharCombo _combo;

        public CharComboList(CreateCharTradeGump owner, CharCombo combo) : base(owner.World, 0, 0) {
            _combo = combo;
            CanMove = false;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;
            LayerOrder = UILayer.Over;
            WantUpdateSize = false;
            var items = combo.Items;
            const int rowH = 15;
            var h = items.Length * rowH;
            if (h > 200) h = 200;
            if (h < rowH) h = rowH;
            var w = combo.Width;
            X = At(combo, true);
            Y = At(combo, false);
            Add(new ResizePic(0x0BB8) { Width = w, Height = h, AcceptMouseInput = false, CanMove = false });
            Width = w;
            Height = h;
            for (var i = 0; i < items.Length; i++) {
                var row = new Row(this, i) { Y = i * rowH, Width = w, Height = rowH };
                row.Add(new Label(items[i], false, 0x0453, 9) { X = 2, Y = 0 });
                Add(row);
            }
        }

        void Choose(int index) {
            _combo.Choose(index);
            Dispose();
        }

        static int At(Control c, bool horizontal) {
            var n = 0;
            for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
            return n;
        }

        sealed class Row : Control {
            readonly CharComboList _list;
            readonly int _index;

            public Row(CharComboList list, int index) {
                _list = list;
                _index = index;
                AcceptMouseInput = true;
                CanMove = false;
                WantUpdateSize = false;
            }

            public override void OnMouseClick(int x, int y) => _list.Choose(_index);
        }
    }

    sealed class CharSlider : Control {
        readonly List<CharSlider> _paired = new();
        readonly Label _text;
        readonly int _bar;
        int _knob = 8;
        int _value = -1;
        int _x;

        public CharSlider(int x, int y, int width, int min, int max, int value) {
            X = x;
            Y = y;
            _bar = width;
            Min = min;
            Max = max;
            Width = width + 40;
            Height = 16;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            if (GumpMetrics.TrySize(216, out var kw, out var kh)) {
                _knob = kw;
                if (kh > Height) Height = kh;
            }
            _text = new Label("", false, 0x0386, 1) { X = _bar + 2, Y = 0 };
            Add(_text);
            Value = value;
        }

        public int Min { get; }
        public int Max { get; }

        public int Value {
            get => _value;
            set {
                var next = value;
                if (next < Min) next = Min;
                if (next > Max) next = Max;
                if (_value == next) return;
                var delta = _value < 0 ? 0 : next - _value;
                _value = next;
                _text.Text = _value.ToString();
                Place();
                if (delta != 0) MovePaired(delta);
            }
        }

        public void Pair(CharSlider other) => _paired.Add(other);

        public override void OnMouseClick(int x, int y) {
            var local = x - At(this, true);
            var len = _bar - _knob;
            if (len < 1) len = 1;
            var perc = local / (float)len * 100f;
            if (perc < 0) perc = 0;
            if (perc > 100) perc = 100;
            var span = Max - Min;
            Value = (int)(span * perc / 100f) + Min;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            DrawBar(x, y);
            GumpArtDraw.Pic(216, x + _x, y, _knob, Height, false);
            return base.Draw(x, y);
        }

        void DrawBar(int x, int y) {
            if (!GumpMetrics.TrySize(213, out var lw, out var lh)) return;
            GumpMetrics.TrySize(214, out _, out var mh);
            GumpMetrics.TrySize(215, out var rw, out var rh);
            var h = lh > 0 ? lh : Height;
            GumpArtDraw.Pic(213, x, y, lw, h, false);
            var mid = _bar - lw - rw;
            if (mid > 0) GumpArtDraw.Pic(214, x + lw, y, mid, mh > 0 ? mh : h, true);
            if (rw > 0) GumpArtDraw.Pic(215, x + _bar - rw, y, rw, rh > 0 ? rh : h, false);
        }

        void Place() {
            var span = Max - Min;
            var len = _bar - _knob;
            if (len < 0) len = 0;
            var pct = span > 0 ? (_value - Min) / (float)span * 100f : 0;
            _x = (int)(len * pct / 100f);
            if (_x < 0) _x = 0;
        }

        void SetSilent(int value) {
            if (value < Min) value = Min;
            if (value > Max) value = Max;
            _value = value;
            _text.Text = _value.ToString();
            Place();
        }

        void MovePaired(int delta) {
            if (_paired.Count == 0 || delta == 0) return;
            var step = delta > 0 ? -1 : 1;
            var points = Math.Abs(delta);
            var index = _value % _paired.Count;
            if (index < 0) index = 0;
            var moved = true;
            while (points > 0) {
                var peer = _paired[index];
                if (step > 0) {
                    if (peer.Value < peer.Max) { peer.SetSilent(peer.Value + step); points--; moved = true; }
                }
                else if (peer.Value > peer.Min) { peer.SetSilent(peer._value + step); points--; moved = true; }
                index++;
                if (index != _paired.Count) continue;
                if (!moved) return;
                moved = false;
                index = 0;
            }
        }

        static int At(Control c, bool horizontal) {
            var n = 0;
            for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
            return n;
        }
    }
}

/// <summary>skills.mul order. Keys are ClassicUO SkillEntry.HardCodedName, lowercased.</summary>
public static class CharSkillNames {
    public static readonly string[] Names = [
        "Alchemy", "Anatomy", "Animal Lore", "Item Identification", "Arms Lore", "Parrying", "Begging",
        "Blacksmithy", "Bowcraft", "Peacemaking", "Camping", "Carpentry", "Cartography", "Cooking",
        "Detecting Hidden", "Enticement", "Evaluating Intelligence", "Healing", "Fishing",
        "Forensic Evaluation", "Herding", "Hiding", "Provocation", "Inscription", "Lockpicking",
        "Magery", "Resisting Spells", "Tactics", "Snooping", "Musicianship", "Poisoning", "Archery",
        "Spirit Speak", "Stealing", "Tailoring", "Animal Taming", "Taste Identification", "Tinkering",
        "Tracking", "Veterinary", "Swordsmanship", "Mace Fighting", "Fencing", "Wrestling",
        "Lumberjacking", "Mining", "Meditation", "Stealth", "Remove Trap", "Necromancy", "Focus",
        "Chivalry", "Bushido", "Ninjitsu", "Spellweaving", "Mysticism", "Imbuing", "Throwing",
    ];

    static readonly string[] Keys = [
        "alchemy", "anatomy", "animallore", "itemid", "armslore", "parrying", "begging",
        "blacksmith", "bowcraft", "peacemaking", "camping", "carpentry", "cartography", "cooking",
        "detecthidden", "enticement", "evaluateintelligence", "healing", "fishing",
        "forensicevaluation", "herding", "hiding", "provocation", "inscription", "lockpicking",
        "magery", "resistingspells", "tactics", "snooping", "musicanship", "poisoning", "archery",
        "spiritspeak", "stealing", "tailoring", "animaltaming", "tasteidentification", "tinkering",
        "tracking", "veterinary", "swordsmanship", "macefighting", "fencing", "wrestling",
        "lumberjacking", "mining", "meditation", "stealth", "disarm", "necromancy", "focus",
        "chivalry", "bushido", "ninjitsu", "spellweaving", "mysticism", "imbuing", "throwing",
    ];

    public static int IndexOf(string token) {
        if (string.IsNullOrEmpty(token)) return -1;
        var compact = Compact(token);
        for (var i = 0; i < Keys.Length; i++) {
            if (Keys[i] == compact || Compact(Names[i]) == compact) return i;
            if (i == 47 && (compact == "removetrap" || compact == "removetraps")) return i;
        }
        return -1;
    }

    static string Compact(string text) {
        var n = 0;
        var raw = text.ToLowerInvariant();
        var buf = new char[raw.Length];
        for (var i = 0; i < raw.Length; i++) {
            var c = raw[i];
            if (c == ' ' || c == '\t' || c == '/') continue;
            buf[n++] = c;
        }
        return new string(buf, 0, n);
    }
}
