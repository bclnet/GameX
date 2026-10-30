// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CombatBookGump for client 7.0.113. Book 0x2B02, corners 0x08BB and 0x08BC.
/// Three index pages, then one page per ability. Ability count is 32 (MAX_ABILITIES_COUNT).
/// Versions before 7.0 use 13 or 29 abilities. This client does not.
/// A second Open does not add another book.
/// The two icons are UseAbilityButtonGump, primary then secondary, at 215,105 and 215,150.
/// They stay on the book: this host has no drag that pulls a child out.
/// A click sends Send_UseCombatAbility. The index is the 1-based ability, the value
/// UsePrimaryAbility passes to SendAbility on an Age of Shadows client, or 0 to turn it off.
/// Pre-AOS Stun and Disarm packets are not sent. There is no feature-flag read.
/// Book list slots are 0 through 31. Those are not the packet index. Packet index is slot + 1
/// (Armor Ignore is 1, Disrobe is 32). With no weapon the pair is AbilityData's default:
/// Paralyzing Blow 11 and Disarm 5. A mapped weapon replaces that pair.
/// Page-turn sound 0x0055 and the cliloc tooltips are not played. No tooltip host is wired here.
/// 0xBF subcommand 0x21 is TakeClearAbilities, once. That clears the 0x80 bit on both stored abilities.
/// </summary>
public sealed class CombatBookGump : Gump {
    const int AbilityCount = 32;
    const int IndexPages = 3;
    const int LastPage = IndexPages + AbilityCount;
    // AbilityData.DefaultItemAbilities: ParalyzingBlow, Disarm.
    const byte DefaultPrimary = 11;
    const byte DefaultSecondary = 5;

    readonly Corner _left, _right;
    readonly UseAbilityButtonGump _primaryIcon, _secondaryIcon;
    readonly List<UOEquipState> _equip = [];
    byte _primary, _secondary;
    int _rev = -1;

    public CombatBookGump(World world) : base(world, 0, 0) {
        X = 100;
        Y = 100;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        Add(new GumpPic(0, 0, 0x2B02, 0));
        _left = new Corner(this, 50, 8, 0x08BB, false);
        _right = new Corner(this, 321, 8, 0x08BC, true);
        if (!_left.IsDisposed) Add(_left);
        if (!_right.IsDisposed) Add(_right);
        BuildIndex();
        BuildPages();
        _primaryIcon = new UseAbilityButtonGump(this, true) { X = 215, Y = 105 };
        _secondaryIcon = new UseAbilityButtonGump(this, false) { X = 215, Y = 150 };
        Add(_primaryIcon);
        Add(_secondaryIcon);
        _rev = UOLoginNet.WorldRevision;
        ReadWeapon();
        ShowIcons();
        SetActivePage(1);
    }

    public static void Open(World world) {
        if (UIManager.GetGump<CombatBookGump>() != null) return;
        UIManager.Add(new CombatBookGump(world));
    }

    /// <summary>Same toggle as UsePrimaryAbility / UseSecondaryAbility. idx 0 clears.</summary>
    public void Use(bool primary) {
        var ability = primary ? _primary : _secondary;
        byte idx;
        if ((ability & 0x80) == 0) {
            _primary &= 0x7F;
            _secondary &= 0x7F;
            idx = (byte)(ability & 0x7F);
            if (primary) _primary |= 0x80;
            else _secondary |= 0x80;
        }
        else {
            idx = 0;
            if (primary) _primary &= 0x7F;
            else _secondary &= 0x7F;
        }
        UOLoginNet.Send(OutgoingPackets.Send_UseCombatAbility(UOLoginNet.PlayerSerial, idx));
        ShowIcons();
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            ReadWeapon();
        }
        if (UOLoginNet.TakeClearAbilities()) {
            _primary &= 0x7F;
            _secondary &= 0x7F;
        }
        ShowIcons();
        base.Update();
    }

    void ReadWeapon() {
        Pair(Weapon(), out var primary, out var secondary);
        if ((_primary & 0x7F) != primary) _primary = primary;
        if ((_secondary & 0x7F) != secondary) _secondary = secondary;
    }

    ushort Weapon() {
        UOLoginNet.CopyEquip(UOLoginNet.PlayerSerial, _equip);
        ushort two = 0;
        for (var i = 0; i < _equip.Count; i++) {
            var item = _equip[i];
            if (item.Graphic == 0) continue;
            if (item.Layer == 0x01) return item.Graphic;
            if (item.Layer == 0x02) two = item.Graphic;
        }
        return two;
    }

    static void Pair(ushort graphic, out byte primary, out byte secondary) {
        primary = DefaultPrimary;
        secondary = DefaultSecondary;
        if (graphic != 0 && GraphicAbilities.TryGetValue(graphic, out var pair)) {
            primary = (byte)pair.Primary;
            secondary = (byte)pair.Secondary;
            return;
        }
        var items = Tiles();
        if (items == null || graphic == 0 || graphic >= items.Length) return;
        var anim = items[graphic].AnimID;
        if (graphic > 0) {
            var prev = (ushort)(graphic - 1);
            if (items[prev].AnimID == anim && GraphicAbilities.TryGetValue(prev, out var prevPair)) {
                primary = (byte)prevPair.Primary;
                secondary = (byte)prevPair.Secondary;
                return;
            }
        }
        var next = graphic + 1;
        if (next < items.Length && items[next].AnimID == anim && GraphicAbilities.TryGetValue((ushort)next, out var nextPair)) {
            primary = (byte)nextPair.Primary;
            secondary = (byte)nextPair.Secondary;
        }
    }

    void ShowIcons() {
        _primaryIcon.Apply(_primary);
        _secondaryIcon.Apply(_secondary);
    }

    void SetActivePage(int page) {
        if (page < 1) page = 1;
        else if (page > LastPage) page = LastPage;
        ActivePage = page;
        if (_left != null && !_left.IsDisposed) _left.Page = page != 1 ? 0 : int.MaxValue;
        if (_right != null && !_right.IsDisposed) _right.Page = page != LastPage ? 0 : int.MaxValue;
        var index = page <= IndexPages;
        _primaryIcon.IsVisible = index;
        _secondaryIcon.IsVisible = index;
    }

    void BuildIndex() {
        var offs = 0;
        for (var page = 1; page <= IndexPages; page++) {
            for (var side = 0; side < 2; side++) {
                var right = side % 2 != 0;
                Add(new Label("INDEX", false, 0x0288, 6) { X = right ? 259 : 96, Y = 6 }, page);
                var count = right ? 4 : 9;
                var y = 0;
                for (var i = 0; i < count; i++) {
                    if (offs >= AbilityCount) break;
                    Add(new IndexName(this, Names[offs], IndexPages + 1 + offs) { X = right ? 215 : 52, Y = 42 + y }, page);
                    y += 15;
                    offs++;
                }
                if (!right) continue;
                Add(new Label("Primary Ability Icon", false, 0x0288, 6) { X = 265, Y = 105 }, page);
                Add(new Label("Secondary Ability Icon", false, 0x0288, 6) { X = 265, Y = 150 }, page);
            }
        }
    }

    void BuildPages() {
        var items = Tiles();
        for (var i = 0; i < AbilityCount; i++) {
            var page = IndexPages + 1 + i;
            var icon = new GumpPic(62, 40, (ushort)(0x5200 + i), 0);
            if (!icon.IsDisposed) Add(icon, page);
            Add(new Label(Names[i], false, 0x0288, 6) { X = 110, Y = 34 }, page);
            var rule = new GumpPicTiled(62, 88, 128, 0, 0x0835);
            if (!rule.IsDisposed) Add(rule, page);
            var list = UsedBy[i];
            var textX = 62;
            var textY = 98;
            for (var j = 0; j < list.Length; j++) {
                if (j == 6) {
                    textX = 215;
                    textY = 34;
                }
                var name = ItemName(items, list[j]);
                if (name.Length == 0) continue;
                Add(new Label(name, false, 0x0288, 9) { X = textX, Y = textY }, page);
                textY += 16;
            }
        }
    }

    static string ItemName(Binary_TileData.ItemData[] items, ushort id) {
        if (items == null || id >= items.Length) return "";
        var name = items[id].Name;
        if (string.IsNullOrWhiteSpace(name)) return "";
        return Capitalize(name.Trim());
    }

    static string Capitalize(string str) {
        var chars = str.ToCharArray();
        var cap = true;
        for (var i = 0; i < chars.Length; i++) {
            if (cap) chars[i] = char.ToUpper(chars[i]);
            if (!char.IsWhiteSpace(chars[i]))
                cap = i + 1 < chars.Length && char.IsWhiteSpace(chars[i + 1]);
        }
        return new string(chars);
    }

    static Binary_TileData.ItemData[] _tiles;
    static bool _tilesMiss;

    static Binary_TileData.ItemData[] Tiles() {
        if (_tiles != null || _tilesMiss) return _tiles;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return null;
        _tilesMiss = true;
        try {
            var path = Path.Combine(root, "tiledata.mul");
            if (!File.Exists(path)) return null;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var r = new BinaryReader(fs);
            _tiles = new Binary_TileData(r).ItemDatas;
        }
        catch { _tiles = null; }
        return _tiles;
    }

    sealed class Corner : GumpPic {
        readonly CombatBookGump _owner;
        readonly bool _next;

        public Corner(CombatBookGump owner, int x, int y, ushort graphic, bool next) : base(x, y, graphic, 0) {
            _owner = owner;
            _next = next;
        }

        public override void OnMouseClick(int x, int y) => _owner.SetActivePage(_next ? _owner.ActivePage + 1 : _owner.ActivePage - 1);

        public override bool OnMouseDoubleClick(int x, int y) {
            _owner.SetActivePage(_next ? LastPage : 1);
            return true;
        }
    }

    sealed class IndexName : Label {
        readonly CombatBookGump _owner;
        readonly int _to;

        public IndexName(CombatBookGump owner, string text, int to) : base(text, false, 0x0288, 9) {
            _owner = owner;
            _to = to;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.SetActivePage(_to);
    }

    static readonly string[] Names = {
        "Armor Ignore", "Bleed Attack", "Concussion Blow", "Crushing Blow", "Disarm", "Dismount",
        "Double Strike", "Infecting", "Mortal Strike", "Moving Shot", "Paralyzing Blow", "Shadow Strike",
        "Whirlwind Attack", "Riding Swipe", "Frenzied Whirlwind", "Block", "Defense Mastery", "Nerve Strike",
        "Talon Strike", "Feint", "Dual Wield", "Double Shot", "Armor Pierce", "Bladeweave",
        "Force Arrow", "Lightning Arrow", "Psychic Attack", "Serpent Arrow", "Force of Nature", "Infused Throw",
        "Mystic Arc", "Disrobe",
    };

    // GetItemsList. Slot 31 (Disrobe) has no weapons. ClassicUO has no case 31.
    static readonly ushort[][] UsedBy = {
        new ushort[] { 3908, 5048, 3935, 5119, 9927, 5181, 5040, 5121, 3939, 9932, 11554, 16497, 16502, 16494, 16491 },
        new ushort[] { 3779, 5115, 3912, 3910, 5185, 9924, 5127, 5040, 3720, 5125, 11552, 16499, 16498 },
        new ushort[] { 5048, 3912, 5183, 5179, 3933, 5113, 3722, 9930, 3920, 11556, 16487, 16500 },
        new ushort[] { 5050, 3914, 3935, 3714, 5092, 5179, 5127, 5177, 9926, 4021, 10146, 11556, 11560, 5109, 16500, 16495 },
        new ushort[] { 5111, 3718, 3781, 3908, 3573, 3714, 3933, 5125, 11558, 11560, 5109, 9934, 16493, 16494 },
        new ushort[] { 3918, 3914, 9927, 3573, 5044, 3720, 9930, 5117, 16501, 16495 },
        new ushort[] { 3718, 5187, 3916, 5046, 5119, 9931, 3722, 9929, 9933, 10148, 10153, 16488, 16493, 16496 },
        new ushort[] { 5111, 3779, 3922, 9928, 5121, 9929, 11553, 16490, 16488 },
        new ushort[] { 3910, 9925, 9931, 5181, 9926, 5123, 3920, 5042, 16499, 16502, 16496, 16491 },
        new ushort[] { 5117, 9932, 9933, 16492 },
        new ushort[] { 5050, 3918, 5046, 9924, 9925, 5113, 3569, 9928, 3939, 5042, 16497, 16498 },
        new ushort[] { 3781, 5187, 5185, 5092, 5044, 3922, 5123, 4021, 11553, 16490 },
        new ushort[] { 5115, 5183, 3916, 5177, 3569, 10157, 11559, 9934, 16501 },
        new ushort[] { 10146 },
        new ushort[] { 10148, 10150, 10151 },
        new ushort[] { 10147, 10158, 10159, 11557 },
        new ushort[] { 10151, 10157, 11561 },
        new ushort[] { 10152 },
        new ushort[] { 10155 },
        new ushort[] { 10152, 10153, 10158, 11554 },
        new ushort[] { 10155 },
        new ushort[] { 10149 },
        new ushort[] { 10149, 10159 },
        new ushort[] { 11555, 11558, 11559, 11561 },
        new ushort[] { 11550 },
        new ushort[] { 11551 },
        new ushort[] { 11551, 11552 },
        new ushort[] { 11550 },
        new ushort[] { 11557 },
        new ushort[] { 16492 },
        new ushort[] { 16487 },
        System.Array.Empty<ushort>(),
    };

    // AbilityData.GraphicToAbilitiesMap. The pair is (primary, secondary), Ability enum values.
    static readonly Dictionary<ushort, (int Primary, int Secondary)> GraphicAbilities = new Dictionary<ushort, (int Primary, int Secondary)> {
        { 0x0901, (10, 30) },
        { 0x0902, (8, 12) },
        { 0x0905, (7, 9) },
        { 0x0906, (4, 6) },
        { 0x090C, (2, 9) },
        { 0x0DF0, (13, 11) },
        { 0x0DF1, (13, 11) },
        { 0x0DF2, (6, 5) },
        { 0x0DF3, (6, 5) },
        { 0x0DF4, (6, 5) },
        { 0x0DF5, (6, 5) },
        { 0x0E81, (4, 5) },
        { 0x0E82, (4, 5) },
        { 0x0E85, (7, 5) },
        { 0x0E86, (7, 5) },
        { 0x0E87, (2, 6) },
        { 0x0E88, (2, 6) },
        { 0x0E89, (7, 3) },
        { 0x0E8A, (7, 3) },
        { 0x0EC2, (2, 8) },
        { 0x0EC3, (2, 8) },
        { 0x0EC4, (12, 2) },
        { 0x0EC5, (12, 2) },
        { 0x0F43, (1, 5) },
        { 0x0F44, (1, 5) },
        { 0x0F45, (2, 9) },
        { 0x0F46, (2, 9) },
        { 0x0F47, (2, 3) },
        { 0x0F48, (2, 3) },
        { 0x0F49, (4, 6) },
        { 0x0F4A, (4, 6) },
        { 0x0F4B, (7, 13) },
        { 0x0F4C, (7, 13) },
        { 0x0F4D, (11, 6) },
        { 0x0F4E, (11, 6) },
        { 0x0F4F, (3, 9) },
        { 0x0F50, (3, 9) },
        { 0x0F51, (8, 12) },
        { 0x0F52, (8, 12) },
        { 0x0F5C, (3, 5) },
        { 0x0F5D, (3, 5) },
        { 0x0F5E, (4, 1) },
        { 0x0F5F, (4, 1) },
        { 0x0F60, (1, 3) },
        { 0x0F61, (1, 3) },
        { 0x0F62, (1, 11) },
        { 0x0F63, (1, 11) },
        { 0x0FB5, (4, 12) },
        { 0x13AF, (1, 2) },
        { 0x13B0, (1, 2) },
        { 0x13B1, (11, 9) },
        { 0x13B2, (11, 9) },
        { 0x13B3, (12, 6) },
        { 0x13B4, (12, 6) },
        { 0x13B6, (7, 11) },
        { 0x13B7, (7, 11) },
        { 0x13B8, (7, 11) },
        { 0x13B9, (11, 4) },
        { 0x13BA, (11, 4) },
        { 0x13FD, (10, 6) },
        { 0x13E3, (4, 12) },
        { 0x13F6, (8, 5) },
        { 0x13F8, (3, 29) },
        { 0x13FB, (13, 2) },
        { 0x13FF, (7, 1) },
        { 0x1401, (1, 8) },
        { 0x1402, (12, 9) },
        { 0x1403, (12, 9) },
        { 0x1404, (2, 5) },
        { 0x1405, (2, 5) },
        { 0x1406, (4, 9) },
        { 0x1407, (4, 9) },
        { 0x1438, (13, 4) },
        { 0x1439, (13, 4) },
        { 0x143A, (7, 3) },
        { 0x143B, (7, 3) },
        { 0x143C, (1, 9) },
        { 0x143D, (1, 9) },
        { 0x143E, (13, 3) },
        { 0x143F, (13, 3) },
        { 0x1440, (2, 12) },
        { 0x1441, (2, 12) },
        { 0x1442, (7, 12) },
        { 0x1443, (7, 12) },
        { 0x26BA, (2, 11) },
        { 0x26BB, (11, 9) },
        { 0x26BC, (4, 9) },
        { 0x26BD, (1, 6) },
        { 0x26BE, (11, 8) },
        { 0x26BF, (7, 8) },
        { 0x26C0, (6, 3) },
        { 0x26C1, (7, 9) },
        { 0x26C2, (1, 10) },
        { 0x26C3, (7, 10) },
        { 0x26C4, (2, 11) },
        { 0x26C5, (11, 9) },
        { 0x26C6, (4, 9) },
        { 0x26C7, (1, 6) },
        { 0x26C8, (11, 8) },
        { 0x26C9, (7, 8) },
        { 0x26CA, (6, 3) },
        { 0x26CB, (7, 9) },
        { 0x26CC, (1, 10) },
        { 0x26CD, (7, 10) },
        { 0x26CE, (13, 5) },
        { 0x26CF, (13, 5) },
        { 0x27A2, (4, 14) },
        { 0x27A3, (20, 16) },
        { 0x27A4, (15, 7) },
        { 0x27A5, (23, 22) },
        { 0x27A6, (15, 4) },
        { 0x27A7, (17, 15) },
        { 0x27A8, (20, 18) },
        { 0x27A9, (20, 7) },
        { 0x27AA, (5, 11) },
        { 0x27AB, (21, 19) },
        { 0x27AD, (13, 17) },
        { 0x27AE, (16, 20) },
        { 0x27AF, (16, 23) },
        { 0x27ED, (4, 14) },
        { 0x27EE, (20, 16) },
        { 0x27EF, (15, 7) },
        { 0x27F0, (23, 22) },
        { 0x27F1, (15, 4) },
        { 0x27F2, (17, 15) },
        { 0x27F3, (20, 18) },
        { 0x27F4, (20, 7) },
        { 0x27F5, (5, 11) },
        { 0x27F6, (21, 19) },
        { 0x27F8, (13, 17) },
        { 0x27F9, (16, 20) },
        { 0x27FA, (16, 23) },
        { 0x2D1E, (25, 28) },
        { 0x2D1F, (26, 27) },
        { 0x2D20, (27, 2) },
        { 0x2D21, (8, 12) },
        { 0x2D22, (20, 1) },
        { 0x2D23, (5, 24) },
        { 0x2D24, (3, 4) },
        { 0x2D25, (16, 29) },
        { 0x2D26, (5, 24) },
        { 0x2D27, (13, 24) },
        { 0x2D28, (5, 4) },
        { 0x2D29, (17, 24) },
        { 0x2D2A, (25, 28) },
        { 0x2D2B, (26, 27) },
        { 0x2D2C, (27, 2) },
        { 0x2D2D, (8, 12) },
        { 0x2D2E, (20, 1) },
        { 0x2D2F, (5, 24) },
        { 0x2D30, (3, 4) },
        { 0x2D31, (16, 29) },
        { 0x2D32, (5, 24) },
        { 0x2D33, (13, 24) },
        { 0x2D34, (5, 4) },
        { 0x2D35, (17, 24) },
        { 0x4067, (31, 3) },
        { 0x08FD, (7, 8) },
        { 0x4068, (7, 8) },
        { 0x406B, (1, 9) },
        { 0x406C, (10, 30) },
        { 0x0904, (7, 5) },
        { 0x406D, (7, 5) },
        { 0x0903, (1, 5) },
        { 0x406E, (1, 5) },
        { 0x08FE, (2, 11) },
        { 0x4072, (2, 11) },
        { 0x090B, (4, 3) },
        { 0x4074, (4, 3) },
        { 0x0908, (13, 6) },
        { 0x4075, (13, 6) },
        { 0x4076, (1, 9) },
        { 0x48AE, (2, 8) },
        { 0x48B0, (2, 3) },
        { 0x48B3, (4, 6) },
        { 0x48B2, (4, 6) },
        { 0x48B5, (11, 6) },
        { 0x48B4, (11, 6) },
        { 0x48B7, (8, 5) },
        { 0x48B6, (8, 5) },
        { 0x48B9, (3, 11) },
        { 0x48B8, (3, 11) },
        { 0x48BB, (7, 1) },
        { 0x48BA, (7, 1) },
        { 0x48BD, (1, 8) },
        { 0x48BC, (1, 8) },
        { 0x48BF, (2, 5) },
        { 0x48BE, (2, 5) },
        { 0x48CB, (6, 3) },
        { 0x48CA, (6, 3) },
        { 0x0481, (13, 4) },
        { 0x48C0, (13, 4) },
        { 0x48C3, (7, 3) },
        { 0x48C2, (7, 3) },
        { 0x48C5, (2, 11) },
        { 0x48C4, (2, 11) },
        { 0x48C7, (11, 9) },
        { 0x48C6, (11, 9) },
        { 0x48C9, (11, 8) },
        { 0x48C8, (11, 8) },
        { 0x48CC, (20, 16) },
        { 0x48CD, (20, 16) },
        { 0x48CF, (21, 19) },
        { 0x48CE, (21, 19) },
        { 0x48D1, (20, 7) },
        { 0x48D0, (20, 7) },
        { 0xA289, (3, 13) },
        { 0xA291, (3, 13) },
        { 0xA28A, (23, 13) },
        { 0xA292, (23, 13) },
        { 0xA28B, (2, 13) },
        { 0xA293, (2, 13) },
        { 0x08FF, (31, 3) },
        { 0x4071, (1, 11) },
        { 0x0900, (1, 11) },
        { 0x090A, (1, 9) },
        { 0xAEA5, (7, 1) },
        { 0xAEB4, (7, 1) },
        { 0xAEC3, (7, 1) },
        { 0xAED2, (7, 1) },
        { 0xAEA4, (7, 13) },
        { 0xAEB3, (7, 13) },
        { 0xAEC2, (7, 13) },
        { 0xAED1, (7, 13) },
    };
}
