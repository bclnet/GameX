// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Opened for container gump 0xFFFF. Book art, page corners, and circle buttons follow ClassicUO.
/// A content row is a spell when Amount is the 1-based index, which is ClassicUO Item.Amount.
/// Not stored: 0xBF 0x1B spell bits and the graphic that packet writes, SpellDefinition reagents,
/// power words, mana, min skill, and tithing, World.Player.TithingPoints, World.OPL, and ActiveSpellIcons.
/// </summary>
public sealed class SpellbookGump : Gump {
    readonly BookPic _pic;
    readonly Corner _left, _right;
    readonly List<Control> _pages = [];
    readonly List<UOContainedState> _rows = [];
    readonly List<UOContainedState> _pack = [];
    readonly List<UOItemState> _worldItems = [];
    readonly List<UOMobileState> _mobiles = [];
    readonly List<UOEquipState> _equip = [];
    Button[] _circles;
    BookKind _kind;
    ushort _graphic, _bookGraphic, _minGraphic;
    ulong _bits;
    int _maxPage = 1;
    int _rev = -1;
    bool _ready, _minimized;

    public SpellbookGump(uint serial) : base(UIManager.GetGump<WorldGump>()?.World, serial, serial) {
        X = 64;
        Y = 64;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        _bookGraphic = 0x08AC;
        _minGraphic = 0x08BA;
        _pic = new BookPic(this, _bookGraphic);
        Add(_pic);
        Add(new MinBox(this));
        _left = new Corner(this, 50, 8, 0x08BB, false);
        _right = new Corner(this, 321, 8, 0x08BC, true);
        Add(_left);
        Add(_right);
        Fill();
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Fill();
        }
        base.Update();
    }

    public override void OnButtonClick(int buttonID) {
        if ((uint)buttonID < 4) SetActivePage(buttonID + 1);
    }

    void Fill() {
        var graphic = FindGraphic();
        var kind = KindOf(graphic);
        var info = Describe(kind);
        UOLoginNet.CopyContents(ServerSerial, _rows);
        ulong bits = 0;
        foreach (var it in _rows) {
            var n = it.Amount;
            if (n > 0 && n <= info.Max) bits |= 1UL << (n - 1);
        }
        if (_ready && graphic == _graphic && kind == _kind && bits == _bits) return;
        _ready = true;
        _graphic = graphic;
        _kind = kind;
        _bits = bits;
        _bookGraphic = info.Book;
        _minGraphic = info.Min;
        _pic.Graphic = _minimized ? _minGraphic : _bookGraphic;
        if (kind == BookKind.Magery) {
            if (_circles == null) AddCircles();
        }
        else if (_circles != null) {
            foreach (var b in _circles) {
                b.Parent = null;
                b.Dispose();
            }
            _circles = null;
        }
        foreach (var c in _pages) {
            c.Parent = null;
            c.Dispose();
        }
        _pages.Clear();
        Build(info, bits);
        SetActivePage(1);
        ApplyMin();
        WantUpdateSize = true;
    }

    // World.Items, player equipment, and the backpack. 0xBF 0x1B is the ClassicUO field that sets Item.Graphic and the spell bits. A book inside a nested bag is not in those three lists either, so the art stays the magery book 0x08AC.
    ushort FindGraphic() {
        if (World != null && World.Items.TryGetValue(LocalSerial, out var obj) && obj is Item item) return item.Graphic;
        UOLoginNet.CopyActors(_mobiles, _worldItems);
        foreach (var it in _worldItems)
            if (it.Serial == LocalSerial) return it.Graphic;
        UOLoginNet.CopyEquip(UOLoginNet.PlayerSerial, _equip);
        foreach (var e in _equip)
            if (e.Serial == LocalSerial) return e.Graphic;
        var pack = UOLoginNet.PlayerBackpack;
        if (pack != 0) {
            UOLoginNet.CopyContents(pack, _pack);
            foreach (var it in _pack)
                if (it.Serial == LocalSerial) return it.Graphic;
        }
        return 0x0EFA;
    }

    static BookKind KindOf(ushort graphic) => graphic switch {
        0x2253 => BookKind.Necromancy,
        0x2252 => BookKind.Chivalry,
        0x238C => (UOLoginNet.Features & 0x80) != 0 ? BookKind.Bushido : BookKind.Magery,
        0x23A0 => (UOLoginNet.Features & 0x80) != 0 ? BookKind.Ninjitsu : BookKind.Magery,
        0x2D50 => BookKind.Spellweaving,
        0x2D9D => BookKind.Mysticism,
        0x225A or 0x225B => BookKind.Mastery,
        _ => BookKind.Magery,
    };

    static BookInfo Describe(BookKind kind) {
        var info = new BookInfo { Kind = kind };
        switch (kind) {
            case BookKind.Necromancy: info.Max = 17; info.Book = 0x2B00; info.Min = 0x2B03; info.Icon = 0x5000; break;
            case BookKind.Chivalry: info.Max = 10; info.Book = 0x2B01; info.Min = 0x2B04; info.Icon = 0x5100; break;
            case BookKind.Bushido: info.Max = 6; info.Book = 0x2B07; info.Min = 0x2B09; info.Icon = 0x5400; break;
            case BookKind.Ninjitsu: info.Max = 8; info.Book = 0x2B06; info.Min = 0x2B08; info.Icon = 0x5300; break;
            case BookKind.Spellweaving: info.Max = 16; info.Book = 0x2B2F; info.Min = 0x2B2D; info.Icon = 0x59D8; break;
            case BookKind.Mysticism: info.Max = 16; info.Book = 0x2B32; info.Min = 0x2B30; info.Icon = 0x5DC0; break;
            case BookKind.Mastery: info.Max = 45; info.Book = 0x08AC; info.Min = 0x08BA; info.Icon = 0x0945; break;
            default: info.Max = 64; info.Book = 0x08AC; info.Min = 0x08BA; info.Icon = 0x08C0; break;
        }
        info.OnPage = info.Max >> 1;
        if (info.OnPage > 8) info.OnPage = 8;
        var dict = (info.Max + 7) / 8;
        if ((dict & 1) != 0) dict++;
        info.Pages = kind == BookKind.Mastery ? dict : dict >> 1;
        return info;
    }

    void Build(BookInfo info, ulong bits) {
        var total = 0;
        for (var i = 0; i < info.Max; i++)
            if (On(bits, i)) total++;
        _maxPage = info.Pages + ((total + 1) >> 1);
        if (_maxPage < 1) _maxPage = 1;
        var names = Names(info.Kind);
        var spell = 0;
        var done = 0;
        for (var page = 1; page <= info.Pages; page++) {
            for (var j = 0; j < 2; j++) {
                var right = j % 2 != 0;
                Text("Index", right ? 269 : 106, 10, 6, page);
                // Mastery abilities are World.OPL plus SpellsMastery.GumpIconID. Neither is stored.
                if (info.Kind == BookKind.Mastery && j >= 1) break;
                if (info.Kind == BookKind.Magery) {
                    var circle = (page - 1) * 2 + j;
                    if ((uint)circle < (uint)Circles.Length) Text(Circles[circle], right ? 225 : 62, 30, 6, page);
                }
                else if (info.Kind == BookKind.Mastery)
                    Text(page == info.Pages ? "Passive" : "Activated", right ? 225 : 62, 30, 6, page);
                var y = 0;
                var top = info.Pages + ((done + 1) >> 1);
                var dataX = right ? 225 : 62;
                if (info.Kind == BookKind.Mastery) {
                    var row = page - 1 < MasteryIndex.Length ? MasteryIndex[page - 1] : null;
                    if (row == null) continue;
                    foreach (var id in row) {
                        var index = id - 1;
                        if (!On(bits, index)) continue;
                        if ((done & 1) == 0) top++;
                        done++;
                        Link(NameOf(names, index), dataX, 52 + y, page, info.Pages + index / 2 + 1);
                        y += 15;
                    }
                }
                else {
                    for (var k = 0; k < info.OnPage; k++, spell++) {
                        if (!On(bits, spell)) continue;
                        if ((done & 1) == 0) top++;
                        done++;
                        Link(NameOf(names, spell), dataX, 52 + y, page, top);
                        y += 15;
                    }
                }
            }
        }
        var page1 = info.Pages + 1;
        var placed = 0;
        for (var i = 0; i < info.Max; i++) {
            if (!On(bits, i)) continue;
            var iconX = 62;
            var topX = 87;
            var nameX = 112;
            if (placed > 0) {
                if ((placed & 1) != 0) {
                    iconX = 225;
                    topX = 224;
                    nameX = 275;
                }
                else page1++;
            }
            placed++;
            var spellId = SpellId(info.Kind, i);
            var pic = new SpellIcon(iconX, 40, IconOf(info, i), spellId);
            if (!pic.IsDisposed) Place(pic, page1);
            if (info.Kind == BookKind.Magery) {
                var circle = i >> 3;
                if ((uint)circle < (uint)Circles.Length) Text(Circles[circle], topX, 10, 6, page1);
                CastText(NameOf(names, i), nameX, 34, 6, page1, spellId);
            }
            else if (info.Kind == BookKind.Mastery) {
                Text(MasteryGroup(i + 1), topX, 10, 6, page1);
                Text(NameOf(names, i), nameX, 34, 6, page1);
            }
            else CastText(NameOf(names, i), topX, 6, 6, page1, spellId);
        }
    }

    void AddCircles() {
        _circles = new Button[8];
        _circles[0] = Circle(0, 0x08B1, 58, 175);
        _circles[1] = Circle(0, 0x08B2, 93, 175);
        _circles[2] = Circle(1, 0x08B3, 130, 175);
        _circles[3] = Circle(1, 0x08B4, 164, 175);
        _circles[4] = Circle(2, 0x08B5, 227, 175);
        _circles[5] = Circle(2, 0x08B6, 260, 175);
        _circles[6] = Circle(3, 0x08B7, 297, 175);
        _circles[7] = Circle(3, 0x08B8, 332, 175);
    }

    Button Circle(int id, ushort graphic, int x, int y) {
        var b = new Button(id, graphic, graphic) { X = x, Y = y };
        if (!b.IsDisposed) Add(b);
        return b;
    }

    void Text(string text, int x, int y, byte font, int page) => Place(new Label(text, false, 0x0288, font) { X = x, Y = y }, page);

    void CastText(string text, int x, int y, byte font, int page, int spellId) => Place(new SpellName(text, font, spellId) { X = x, Y = y }, page);

    /// <summary>ClassicUO SpellDefinition.ID. Magery starts at 1, necromancy at 101, chivalry at 201, bushido at 401, ninjitsu at 501, spellweaving at 601, mysticism at 678.</summary>
    static int SpellId(BookKind kind, int index) => kind switch {
        BookKind.Necromancy => 101 + index,
        BookKind.Chivalry => 201 + index,
        BookKind.Bushido => 401 + index,
        BookKind.Ninjitsu => 501 + index,
        BookKind.Spellweaving => 601 + index,
        BookKind.Mysticism => 678 + index,
        BookKind.Magery => 1 + index,
        _ => 0,
    };

    void Link(string text, int x, int y, int page, int to) => Place(new IndexName(this, text, to) { X = x, Y = y }, page);

    void Place(Control c, int page) {
        Add(c, page);
        _pages.Add(c);
    }

    void SetActivePage(int page) {
        if (page < 1) page = 1;
        else if (page > _maxPage) page = _maxPage;
        ActivePage = page;
        _left.Page = page != 1 ? 0 : int.MaxValue;
        _right.Page = page != _maxPage ? 0 : int.MaxValue;
    }

    void ApplyMin() {
        foreach (var c in Children) c.IsVisible = !_minimized || c == _pic;
        _pic.IsVisible = true;
    }

    bool Minimized {
        get => _minimized;
        set {
            if (_minimized == value) return;
            _minimized = value;
            _pic.Graphic = value ? _minGraphic : _bookGraphic;
            ApplyMin();
            WantUpdateSize = true;
        }
    }

    static bool On(ulong bits, int index) => (uint)index < 64 && (bits & (1UL << index)) != 0;

    static ushort IconOf(BookInfo info, int index) {
        if (info.Kind != BookKind.Mastery) return (ushort)(info.Icon + index);
        if (index < 6) return (ushort)(0x0945 + index);
        return (ushort)(0x9B8B + (index - 6));
    }

    static string NameOf(string[] names, int index) => names != null && (uint)index < (uint)names.Length ? names[index] : "";

    static string[] Names(BookKind kind) => kind switch {
        BookKind.Necromancy => NecromancyNames,
        BookKind.Chivalry => ChivalryNames,
        BookKind.Bushido => BushidoNames,
        BookKind.Ninjitsu => NinjitsuNames,
        BookKind.Spellweaving => SpellweavingNames,
        BookKind.Mysticism => MysticismNames,
        BookKind.Mastery => MasteryNames,
        _ => MageryNames,
    };

    static string MasteryGroup(int id) => id switch {
        1 or 2 => "Provocation",
        3 or 4 => "Peacemaking",
        5 or 6 => "Discordance",
        7 or 8 => "Magery",
        9 or 10 => "Mysticism",
        11 or 12 => "Necromancy",
        13 or 14 => "Spellweaving",
        16 or 17 => "Bushido",
        19 or 20 => "Chivalry",
        21 or 22 => "Ninjitsu",
        23 or 24 => "Archery",
        25 or 26 => "Fencing",
        27 or 28 => "Mace Fighting",
        29 or 30 => "Swordmanship",
        31 or 32 => "Throwing",
        34 or 35 or 36 => "Parrying",
        37 or 38 or 39 => "Poisoning",
        40 or 41 or 42 => "Wrestling",
        43 or 44 or 45 => "Animal Taming",
        15 or 18 or 33 => "Passive",
        _ => "",
    };

    enum BookKind { Magery, Necromancy, Chivalry, Bushido, Ninjitsu, Spellweaving, Mysticism, Mastery }

    struct BookInfo {
        public BookKind Kind;
        public ushort Book, Min, Icon;
        public int Max, OnPage, Pages;
    }

    sealed class SpellIcon : GumpPic {
        readonly int _id;

        public SpellIcon(int x, int y, ushort graphic, int id) : base(x, y, graphic, 0) => _id = id;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (_id > 0) UOLoginNet.CastSpell(_id);
            return true;
        }
    }

    sealed class SpellName : Label {
        readonly int _id;

        public SpellName(string text, byte font, int id) : base(text, false, 0x0288, font) {
            _id = id;
            AcceptMouseInput = true;
        }

        public override bool OnMouseDoubleClick(int x, int y) {
            if (_id > 0) UOLoginNet.CastSpell(_id);
            return true;
        }
    }

    sealed class BookPic : GumpPic {
        readonly SpellbookGump _owner;

        public BookPic(SpellbookGump owner, ushort graphic) : base(0, 0, graphic, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (_owner.Minimized) _owner.Minimized = false;
            return true;
        }
    }

    sealed class MinBox : Control {
        readonly SpellbookGump _owner;

        public MinBox(SpellbookGump owner) {
            _owner = owner;
            X = 0;
            Y = 98;
            Width = 27;
            Height = 23;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) {
            if (!_owner.Minimized) _owner.Minimized = true;
        }
    }

    sealed class Corner : GumpPic {
        readonly SpellbookGump _owner;
        readonly bool _next;

        public Corner(SpellbookGump owner, int x, int y, ushort graphic, bool next) : base(x, y, graphic, 0) {
            _owner = owner;
            _next = next;
        }

        public override void OnMouseClick(int x, int y) => _owner.SetActivePage(_next ? _owner.ActivePage + 1 : _owner.ActivePage - 1);

        public override bool OnMouseDoubleClick(int x, int y) {
            _owner.SetActivePage(_next ? int.MaxValue : 1);
            return true;
        }
    }

    sealed class IndexName : Label {
        readonly SpellbookGump _owner;
        readonly int _to;

        public IndexName(SpellbookGump owner, string text, int to) : base(text, false, 0x0288, 9) {
            _owner = owner;
            _to = to;
            AcceptMouseInput = true;
            CanMove = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.SetActivePage(_to);
    }

    static readonly string[] Circles = {
        "First Circle", "Second Circle", "Third Circle", "Fourth Circle", "Fifth Circle", "Sixth Circle", "Seventh Circle", "Eighth Circle",
    };

    static readonly int[][] MasteryIndex = {
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 },
        new[] { 9, 10, 11, 12, 13, 14, 19, 20 },
        new[] { 17, 21, 22, 25, 26, 34, 35, 36 },
        new[] { 27, 28, 29, 32, 37, 38, 40, 41 },
        new[] { 23, 24, 30, 31, 43, 44 },
        new[] { 15, 16, 18, 33, 39, 42, 45 },
    };

    static readonly string[] MageryNames = {
        "Clumsy", "Create Food", "Feeblemind", "Heal", "Magic Arrow", "Night Sight", "Reactive Armor", "Weaken",
        "Agility", "Cunning", "Cure", "Harm", "Magic Trap", "Magic Untrap", "Protection", "Strength",
        "Bless", "Fireball", "Magic Lock", "Poison", "Telekinesis", "Teleport", "Unlock", "Wall of Stone",
        "Arch Cure", "Arch Protection", "Curse", "Fire Field", "Greater Heal", "Lightning", "Mana Drain", "Recall",
        "Blade Spirits", "Dispel Field", "Incognito", "Magic Reflection", "Mind Blast", "Paralyze", "Poison Field", "Summon Creature",
        "Dispel", "Energy Bolt", "Explosion", "Invisibility", "Mark", "Mass Curse", "Paralyze Field", "Reveal",
        "Chain Lightning", "Energy Field", "Flamestrike", "Gate Travel", "Mana Vampire", "Mass Dispel", "Meteor Swarm", "Polymorph",
        "Earthquake", "Energy Vortex", "Resurrection", "Air Elemental", "Summon Daemon", "Earth Elemental", "Fire Elemental", "Water Elemental",
    };

    static readonly string[] NecromancyNames = {
        "Animate Dead", "Blood Oath", "Corpse Skin", "Curse Weapon", "Evil Omen", "Horrific Beast", "Lich Form", "Mind Rot",
        "Pain Spike", "Poison Strike", "Strangle", "Summon Familiar", "Vampiric Embrace", "Vengeful Spirit", "Wither", "Wraith Form", "Exorcism",
    };

    static readonly string[] ChivalryNames = {
        "Cleanse by Fire", "Close Wounds", "Consecrate Weapon", "Dispel Evil", "Divine Fury", "Enemy of One", "Holy Light", "Noble Sacrifice", "Remove Curse", "Sacred Journey",
    };

    static readonly string[] BushidoNames = {
        "Honorable Execution", "Confidence", "Evasion", "Counter Attack", "Lightning Strike", "Momentum Strike",
    };

    static readonly string[] NinjitsuNames = {
        "Focus Attack", "Death Strike", "Animal Form", "Ki Attack", "Surprise Attack", "Backstab", "Shadowjump", "Mirror Image",
    };

    static readonly string[] SpellweavingNames = {
        "Arcane Circle", "Gift of Renewal", "Immolating Weapon", "Attunement", "Thunderstorm", "Nature's Fury", "Summon Fey", "Summon Fiend",
        "Reaper Form", "Wildfire", "Essence of Wind", "Dryad Allure", "Ethereal Voyage", "Word of Death", "Gift of Life", "Arcane Empowerment",
    };

    static readonly string[] MysticismNames = {
        "Nether Bolt", "Healing Stone", "Purge Magic", "Enchant", "Sleep", "Eagle Strike", "Animated Weapon", "Stone Form",
        "Spell Trigger", "Mass Sleep", "Cleansing Winds", "Bombard", "Spell Plague", "Hail Storm", "Nether Cyclone", "Rising Colossus",
    };

    static readonly string[] MasteryNames = {
        "Inspire", "Invigorate", "Resilience", "Perseverance", "Tribulation", "Despair", "Death Ray", "Ethereal Burst",
        "Nether Blast", "Mystic Weapon", "Command Undead", "Conduit", "Mana Shield", "Summon Reaper", "Enchanted Summoning", "Anticipate Hit",
        "Warcry", "Intuition", "Rejuvenate", "Holy Fist", "Shadow", "White Tiger Form", "Flaming Shot", "Playing The Odds",
        "Thrust", "Pierce", "Stagger", "Toughness", "Onslaught", "Focused Eye", "Elemental Fury", "Called Shot",
        "Warrior's Gifts", "Shield Bash", "Bodyguard", "Heighten Senses", "Tolerance", "Injected Strike", "Potency", "Rampage",
        "Fists of Fury", "Knockout", "Whispering", "Combat Training", "Boarding",
    };
}
