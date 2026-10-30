// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// Opened by 0x88. ClassicUO 7.0.113 paperdoll: background 0x07D0 or 0x07D1, the six side slots, and worn art.
/// Client 7.0 uses the quests button, not journal. Flag 0x02 is the lift bit and is shown in the title line.
/// Double-click sends 0x06. Right-click closes.
/// Feature 0x20 draws the combat book 0x2B34 and the racial book 0x2B28. Double-click opens one of each.
/// A drag is a UseLabel, the same hit a container uses, so mouse up calls Lift. Hair, beard, and the backpack are not one.
/// </summary>
public sealed class PaperdollGump : Gump {
    // PaperdollOrder base tables. Index 0 is the sentinel. T2 is the default. Later index paints on top.
    const int MaleGump = 50000;
    const int FemaleGump = 60000;
    static readonly byte[] OrderT1 = [
        0x00, 0x14, 0x05, 0x04, 0x03, 0x18, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x13, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];
    static readonly byte[] OrderT2 = [
        0x00, 0x14, 0x05, 0x04, 0x03, 0x18, 0x13, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];
    static readonly byte[] OrderT3 = [
        0x00, 0x14, 0x0D, 0x05, 0x04, 0x03, 0x18, 0x11, 0x08, 0x0E, 0x0F, 0x13, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];
    static Binary_TileData.ItemData[] _items;
    static Dictionary<ushort, Dictionary<ushort, ushort>> _conv;
    static bool _tables;
    // EquipmentSlot order: helmet, earrings, necklace, ring, bracelet, tunic. Y is 75 + 21 * index.
    static readonly byte[] SlotLayers = [0x06, 0x12, 0x0A, 0x08, 0x0E, 0x11];
    static readonly ushort[] PeaceMode = [0x07E5, 0x07E6, 0x07E7];
    static readonly ushort[] WarMode = [0x07E8, 0x07E9, 0x07EA];
    static readonly (ushort id, string text)[] EmptyEntries = [];

    readonly bool _player;
    readonly ushort _baseGraphic;
    readonly Label _title;
    readonly DollHost _doll;
    readonly EquipSlot[] _slots;
    readonly List<UOEquipState> _equip = [];
    GumpPic _pic;
    Button _warBtn;
    string _titleText;
    byte _flags;
    bool _minimized, _war;
    int _rev = -1;

    enum Buttons {
        Help,
        Options,
        LogOut,
        Journal,
        Quests,
        Skills,
        Guild,
        PeaceWarToggle,
        Status,
    }

    public PaperdollGump(World world, uint serial, string title, byte flags) : base(world, serial, serial) {
        CanCloseWithRightClick = true;
        CanMove = true;
        AcceptMouseInput = true;
        X = 80;
        Y = 40;
        _player = serial == UOLoginNet.PlayerSerial;
        _baseGraphic = _player ? (ushort)0x07D0 : (ushort)0x07D1;
        _titleText = title;
        _flags = flags;
        // Chrome is added after the doll so the button rectangles stay above the body gump.
        _pic = _player ? new BasePic(this, _baseGraphic) : new GumpPic(0, 0, _baseGraphic, 0);
        Add(_pic);
        _doll = new DollHost();
        Add(_doll);
        _slots = new EquipSlot[SlotLayers.Length];
        for (var i = 0; i < SlotLayers.Length; i++) {
            _slots[i] = new EquipSlot(this, SlotLayers[i], 75 + 21 * i);
            Add(_slots[i]);
        }
        var books = _player && (UOLoginNet.Features & 0x20) != 0;
        if (_player) {
            AddButton(Buttons.Help, 0x07EF, 0x07F0, 0x07F1, 0);
            AddButton(Buttons.Options, 0x07D6, 0x07D7, 0x07D8, 1);
            AddButton(Buttons.LogOut, 0x07D9, 0x07DA, 0x07DB, 2);
            // 7.0.113 is past CV_500A, so this slot is quests, not journal 0x07DC.
            AddButton(Buttons.Quests, 0x57B5, 0x57B7, 0x57B6, 3);
            AddButton(Buttons.Skills, 0x07DF, 0x07E0, 0x07E1, 4);
            AddButton(Buttons.Guild, 0x57B2, 0x57B4, 0x57B3, 5);
            _war = UOLoginNet.InWarMode;
            var war = _war ? WarMode : PeaceMode;
            _warBtn = AddButton(Buttons.PeaceWarToggle, war[0], war[1], war[2], 6);
            var profileX = 25;
            if (books) profileX += 14;
            Add(new ProfilePic(profileX, serial));
            profileX += 14;
            Add(new PartyPic(this, profileX));
            Add(new MinBox(this));
        }
        else Add(new ProfilePic(25, serial));
        AddButton(Buttons.Status, 0x07EB, 0x07EC, 0x07ED, 7);
        Add(new VirtuePic(serial));
        if (books) {
            Add(new CombatBookPic(this));
            Add(new RacialBookPic(this));
        }
        _title = new Label(Line(_titleText, _flags), false, 0x0386, 1) { X = 39, Y = 262 };
        Add(_title);
        Refresh();
        _rev = UOLoginNet.WorldRevision;
    }

    public string Title {
        get => _titleText;
        set {
            _titleText = value;
            if (_title != null) _title.Text = Line(value, _flags);
        }
    }

    public byte Flags {
        get => _flags;
        set {
            _flags = value;
            if (_title != null) _title.Text = Line(_titleText, value);
        }
    }

    public bool CanLift => (_flags & 0x02) != 0;

    // ClassicUO does not pick up hair, beard, or the backpack. The backpack still opens on 0x06.
    bool AllowsDrag(byte layer) {
        if (layer == 0x0B || layer == 0x10 || layer == 0x15) return false;
        return _player || CanLift;
    }

    public bool IsMinimized {
        get => _minimized;
        set {
            if (_minimized == value || _pic == null || _pic.IsDisposed) return;
            _minimized = value;
            var graphic = value ? (ushort)0x07EE : _baseGraphic;
            if (_pic.Graphic != graphic) _pic.Graphic = graphic;
            if (_pic.IsDisposed) return;
            foreach (var c in Children) c.IsVisible = !value;
            _pic.IsVisible = true;
            WantUpdateSize = true;
        }
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            _title.Text = Line(_titleText, _flags);
            Refresh();
        }
        if (_player && _warBtn != null && !_warBtn.IsDisposed && _war != UOLoginNet.InWarMode) {
            _war = UOLoginNet.InWarMode;
            _warBtn.ButtonGraphicNormal = (_war ? WarMode : PeaceMode)[0];
        }
        base.Update();
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.Help:
                UOLoginNet.Send(OutgoingPackets.Send_HelpRequest());
                break;
            case Buttons.Options:
                // OpenSettings is not ported.
                break;
            case Buttons.LogOut:
                if (UIManager.GetGump<QuestionGump>() == null)
                    UIManager.Add(new QuestionGump(World, "Quit\nUltima Online?", yes => {
                        if (!yes) return;
                        UOLoginNet.RequestQuit();
                    }));
                break;
            case Buttons.Quests:
                UOLoginNet.Send(OutgoingPackets.Send_QuestMenuRequest(UOLoginNet.PlayerSerial));
                break;
            case Buttons.Skills:
                if (UIManager.GetGump<SkillsGump>() == null) UIManager.Add(new SkillsGump(World));
                break;
            case Buttons.Guild:
                UOLoginNet.Send(OutgoingPackets.Send_GuildMenuRequest(UOLoginNet.PlayerSerial));
                break;
            case Buttons.PeaceWarToggle:
                UOLoginNet.ToggleWar();
                break;
            case Buttons.Status:
                if (UIManager.GetGump<StatusGump>() == null) UIManager.Add(new StatusGump(World));
                break;
        }
    }

    Button AddButton(Buttons id, ushort normal, ushort pressed, ushort over, int index) {
        var b = new Button((int)id, normal, pressed, over) {
            X = 185,
            Y = 44 + 27 * index,
            ButtonAction = ButtonAction.Activate,
        };
        return Add(b);
    }

    void Refresh() {
        UOLoginNet.CopyEquip(ServerSerial, _equip);
        for (var i = 0; i < _slots.Length; i++) _slots[i].Show(Find(_equip, _slots[i].Layer));
        var kids = _doll.Children;
        for (var i = kids.Count - 1; i >= 0; i--) {
            var c = kids[i];
            c.Parent = null;
            c.Dispose();
        }
        var mobile = MobileBody();
        var body = DollGump(mobile, out var hue, out var extra);
        var pic = new GumpPic(0, 0, body, hue) { IsPartialHue = true, AcceptMouseInput = false, CanMove = false };
        _doll.Add(pic);
        if (extra != 0) {
            // Mobile hue is not stored. The 0x03DB overlay stays unhued; the body uses 0x03EA.
            _doll.Add(new GumpPic(0, 0, extra, 0) { IsPartialHue = true, AcceptMouseInput = true, CanMove = false });
        }
        // Action 4 stand, under the worn gumps. Each gump is drawn at (0, 0); the art is already placed on the body.
        _doll.Add(new BodyArt(mobile, body));
        var female = Female(mobile);
        var gfx = LayerGfx(_equip);
        var order = new byte[25];
        var count = PaintOrder(gfx, female || Gargoyle(mobile), order);
        for (var i = 0; i < count; i++) {
            var layer = order[i];
            var item = Find(_equip, layer);
            if (item.Serial == 0 || IsCovered(_equip, layer)) continue;
            if (!TryWearGump(mobile, female, item.Graphic, out var gump)) continue;
            var worn = new Worn(item.Serial, 0, 0, gump, (ushort)(item.Hue & 0x3FFF), AllowsDrag(layer));
            if (!worn.IsDisposed) _doll.Add(worn);
        }
        var pack = Find(_equip, 0x15);
        if (pack.Serial != 0 && TryPackGump(_player, pack.Graphic, out var pg)) {
            // PaperdollBooks (feature 0x20) shifts the backpack six pixels left.
            var bx = (UOLoginNet.Features & 0x20) != 0 ? -6 : 0;
            var worn = new Worn(pack.Serial, bx, 0, pg, (ushort)(pack.Hue & 0x3FFF), false);
            if (!worn.IsDisposed) _doll.Add(worn);
        }
    }

    ushort MobileBody() {
        if (ServerSerial == UOLoginNet.PlayerSerial) return UOLoginNet.PlayerBody;
        if (World != null && World.Mobiles.TryGetValue(ServerSerial, out var obj) && obj is Mobile m) return m.Body;
        return 0;
    }

    // PaperDollInteractable body gump. No IsFemale flag, so an unlisted body is the male gump 0x000C.
    static ushort DollGump(ushort mobile, out ushort hue, out ushort extra) {
        hue = 0;
        extra = 0;
        if (mobile == 0x0191 || mobile == 0x0193) return 0x000D;
        if (mobile == 0x025D) return 0x000E;
        if (mobile == 0x025E) return 0x000F;
        if (mobile == 0x029A || mobile == 0x02B6) return 0x029A;
        if (mobile == 0x029B || mobile == 0x02B7) return 0x0299;
        if (mobile == 0x04E5) return 0xC835;
        if (mobile == 0x03DB) {
            hue = 0x03EA;
            extra = 0xC72B;
            return 0x000C;
        }
        return 0x000C;
    }

    // CheckGraphicChange. An unlisted body, including a dead gargoyle, is not female.
    static bool Female(ushort body) => body == 0x0191 || body == 0x0193 || body == 0x025E || body == 0x029B;

    static bool Gargoyle(ushort body) => body == 0x029A || body == 0x029B || body == 0x02B6 || body == 0x02B7;

    // PaperdollOrder.Build. graphic[] is AnimID per layer, not the world tile.
    static int PaintOrder(ushort[] graphic, bool altTorso, byte[] dest) {
        var table = OrderT2;
        var arms = graphic[0x13];
        var armsAlt = arms < 0x3D0 ? arms == 0x3CF || arms == 0x210 || arms == 0x3B3 : arms == 0x3DD;
        if (armsAlt) table = OrderT1;
        else {
            var torso = graphic[0x0D];
            if (torso == 0x21A) table = OrderT1;
            else if (torso >= 0x399 && torso - 0x399 < 5 && altTorso) table = OrderT3;
        }
        var order = (byte[])table.Clone();
        if (graphic[0x05] != 0 && graphic[0x04] == 0x398) MoveTo(order, 0x04, 0x05);
        var pants = graphic[0x04];
        var skipFinal = false;
        if (pants < 0x201) {
            if (pants == 0x200 || pants == 0x1EB || pants == 0x1FA) {
                var iShoes = IndexOf(order, 0x03);
                var iPants = IndexOf(order, 0x04);
                if (iShoes >= 0 && iPants >= 0 && iPants < iShoes) {
                    order[iShoes] = 0x04;
                    order[iPants] = 0x03;
                }
            }
        }
        else if ((uint)(pants - 0x513) < 2) {
            if (graphic[0x03] != 0) MoveTo(order, 0x04, 0x03);
            skipFinal = true;
        }
        if (!skipFinal && graphic[0x03] != 0 && pants == 0x3E4) MoveTo(order, 0x04, 0x03);
        if (graphic[0x11] == 0x238) {
            MoveAfter(order, 0x11, 0x0C);
            var robe = graphic[0x16];
            if (robe == 0x4E8 || robe == 0x4E9 || robe == 0x4EA || robe == 0x4EB || robe == 0x5E2 || robe == 0x5E3 || robe == 0x5E4 || robe == 0x5E5)
                MoveTo(order, 0x16, 0x0A);
        }
        if (graphic[0x14] == 0x380 || graphic[0x14] == 0x5F3) MoveAfter(order, 0x14, 0x16);
        var helm = graphic[0x06];
        if (helm < 0x202) {
            var neck = graphic[0x0A];
            if ((helm == 0x201 || helm == 0x1A9) && (neck == 0x1C8 || (neck > 0x1D6 && neck < 0x1D9))) {
                MoveAfter(order, 0x0A, 0x06);
                return Filter(order, dest);
            }
        }
        else if ((uint)(helm - 0x5E9) < 2 && graphic[0x16] != 0 && (uint)(graphic[0x16] - 0x5E2) < 4)
            MoveTo(order, 0x16, 0x06);
        return Filter(order, dest);
    }

    static int Filter(byte[] order, byte[] dest) {
        var c = 0;
        for (var i = 0; i < order.Length; i++) {
            var layer = order[i];
            if (layer == 0x00 || layer == 0x15 || layer == 0x19) continue;
            dest[c++] = layer;
        }
        return c;
    }

    static int IndexOf(byte[] a, byte v) {
        for (var i = 0; i < a.Length; i++)
            if (a[i] == v) return i;
        return -1;
    }

    static void MoveTo(byte[] a, byte from, byte onto) {
        var i1 = IndexOf(a, from);
        if (i1 < 0) return;
        var i2 = IndexOf(a, onto);
        if (i2 < 0 || i2 == i1) return;
        if (i2 < i1) { for (var k = i1; k > i2; k--) a[k] = a[k - 1]; a[i2] = from; }
        else { for (var k = i1; k < i2; k++) a[k] = a[k + 1]; a[i2] = from; }
    }

    static void MoveAfter(byte[] a, byte from, byte after) {
        var i1 = IndexOf(a, from);
        if (i1 < 0) return;
        var i2 = IndexOf(a, after);
        if (i2 < 0 || i2 == i1) return;
        if (i2 < i1) { for (var k = i1; k > i2 + 1; k--) a[k] = a[k - 1]; a[i2 + 1] = from; }
        else { for (var k = i1; k < i2; k++) a[k] = a[k + 1]; a[i2] = from; }
    }

    static ushort[] LayerGfx(List<UOEquipState> equip) {
        var gfx = new ushort[25];
        for (var i = 0; i < equip.Count; i++) {
            var layer = equip[i].Layer;
            if (layer == 0 || layer >= gfx.Length) continue;
            gfx[layer] = AnimOf(equip[i].Graphic);
        }
        return gfx;
    }

    // GetAnimID. Tileart and ConvertBodyIfNeeded are not on Assets, so those steps are skipped.
    // A gump that is not in the file is skipped. The item graphic is not used as a gump id.
    static bool TryWearGump(ushort body, bool female, ushort graphic, out ushort gump) {
        gump = 0;
        var anim = AnimOf(graphic);
        if (anim == 0) return false;
        var ver = UltimaOnline.Current?.Version ?? default;
        if (ver >= ClientVersion.CV_7000 && anim == 0x03CA && (body == 0x02B7 || body == 0x02B6)) anim = 0x0223;
        var conv = Conv();
        if (conv != null && conv.TryGetValue(body, out var dict) && dict.TryGetValue(anim, out var mapped)) {
            if (mapped > MaleGump) anim = (ushort)(mapped >= FemaleGump ? mapped - FemaleGump : mapped - MaleGump);
            else anim = mapped;
        }
        var offset = female ? FemaleGump : MaleGump;
        if (!GumpAt(anim, offset)) offset = female ? MaleGump : FemaleGump;
        if (!GumpAt(anim, offset)) return false;
        gump = (ushort)(anim + offset);
        return true;
    }

    // Backpack is always the male offset. The player's default skin is 0xC4F6 when that gump exists.
    static bool TryPackGump(bool player, ushort graphic, out ushort gump) {
        gump = 0;
        var anim = AnimOf(graphic);
        if (anim == 0) return false;
        var assets = UltimaOnline.Current?.Assets;
        if (player && assets != null && assets.TryGumpPixels(0xC4F6, out _, out var sw, out var sh) && sw > 0 && sh > 0) {
            gump = 0xC4F6;
            return true;
        }
        if (!GumpAt(anim, MaleGump)) return false;
        gump = (ushort)(anim + MaleGump);
        return true;
    }

    static bool GumpAt(int anim, int offset) {
        var id = anim + offset;
        if (id <= 0 || id > 0xFFFF) return false;
        var assets = UltimaOnline.Current?.Assets;
        return assets != null && assets.TryGumpPixels(id, out _, out var w, out var h) && w > 0 && h > 0;
    }

    static ushort AnimOf(ushort graphic) {
        var items = Items();
        if (items == null || graphic >= items.Length) return 0;
        var anim = items[graphic].AnimID;
        return anim == 0 ? (ushort)0 : (ushort)anim;
    }

    static Binary_TileData.ItemData[] Items() {
        EnsureTables();
        return _items;
    }

    static Dictionary<ushort, Dictionary<ushort, ushort>> Conv() {
        EnsureTables();
        return _conv;
    }

    // ItemData.AnimID is not a method on Assets. tiledata.mul is the public parser. Equipconv.def is the gump override.
    static void EnsureTables() {
        if (_tables) return;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return;
        _tables = true;
        var tile = Path.Combine(root, "tiledata.mul");
        if (File.Exists(tile)) {
            using var fs = File.OpenRead(tile);
            using var br = new BinaryReader(fs);
            _items = new Binary_TileData(br).ItemDatas;
        }
        var ver = UltimaOnline.Current?.Version ?? default;
        if (ver < ClientVersion.CV_300) return;
        var def = Path.Combine(root, "Equipconv.def");
        if (!File.Exists(def)) return;
        _conv = [];
        foreach (var raw in File.ReadLines(def)) {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#' || !char.IsDigit(line[0])) continue;
            var hash = line.IndexOf('#');
            if (hash >= 0) line = line.Substring(0, hash);
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) continue;
            var body = Tok(parts[0]);
            var graphic = Tok(parts[1]);
            var gump = Tok(parts[3]);
            if (gump > ushort.MaxValue) continue;
            if (gump == 0) gump = graphic;
            else if (gump == 0xFFFF || gump == -1) gump = Tok(parts[2]);
            if (body < 0 || graphic < 0 || gump < 0 || gump > ushort.MaxValue) continue;
            var key = (ushort)body;
            if (!_conv.TryGetValue(key, out var dict)) _conv[key] = dict = [];
            dict[(ushort)graphic] = (ushort)gump;
        }
    }

    static int Tok(string s) {
        if (string.IsNullOrEmpty(s)) return -1;
        if (s.Length > 2 && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
            return int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out var h) ? h : -1;
        var end = s[0] == '-' || s[0] == '+' ? 1 : 0;
        while (end < s.Length && s[end] >= '0' && s[end] <= '9') end++;
        return int.TryParse(s.Substring(0, end), out var n) ? n : -1;
    }

    static UOEquipState Find(List<UOEquipState> equip, byte layer) {
        for (var i = 0; i < equip.Count; i++)
            if (equip[i].Layer == layer) return equip[i];
        return default;
    }

    static bool TryArt(ushort graphic, out int key, out byte[] px, out int w, out int h) {
        key = w = h = 0;
        px = null;
        var id = (graphic & 0x7FFF) + 0x4000;
        if (id > 0xFFFF) return false;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !assets.TryArtPixels(id, out px, out w, out h)) return false;
        key = unchecked((int)(0x51000000 + graphic));
        return true;
    }

    static void ClickItem(uint serial) {
        if (serial != 0 && UOLoginNet.HasTarget) UOLoginNet.TargetSerial(serial);
    }

    static bool DoubleItem(uint serial) {
        if (serial == 0) return true;
        if (UOLoginNet.HasTarget || UOLoginNet.TargetedJustNow) {
            if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(serial);
            return true;
        }
        UOLoginNet.Use(serial);
        return true;
    }

    static string Line(string title, byte flags) {
        var text = string.IsNullOrEmpty(title) ? "Paperdoll" : title;
        return (flags & 0x02) != 0 ? text + "  lift" : text;
    }

    // MobileView.IsCovered. A missing layer is serial 0.
    static bool IsCovered(List<UOEquipState> equip, byte layer) {
        switch (layer) {
            case 0x03: {
                var pants = Find(equip, 0x04);
                if (Find(equip, 0x18).Serial != 0 || pants.Serial != 0 && pants.Graphic == 0x1411) return true;
                var robe = Find(equip, 0x16);
                if (pants.Serial != 0 && (pants.Graphic == 0x0513 || pants.Graphic == 0x0514) || robe.Serial != 0 && robe.Graphic == 0x0504)
                    return true;
                break;
            }
            case 0x04: {
                var robe = Find(equip, 0x16);
                var pants = Find(equip, 0x04);
                if (Find(equip, 0x18).Serial != 0 || robe.Serial != 0 && robe.Graphic == 0x0504) return true;
                if (pants.Serial != 0 && (pants.Graphic == 0x01EB || pants.Graphic == 0x03E5 || pants.Graphic == 0x03EB)) {
                    var skirt = Find(equip, 0x17);
                    if (skirt.Serial != 0 && skirt.Graphic != 0x01C7 && skirt.Graphic != 0x01E4) return true;
                    if (robe.Serial != 0 && robe.Graphic != 0x0229 && (robe.Graphic <= 0x04E7 || robe.Graphic > 0x04EB)) return true;
                }
                break;
            }
            case 0x11: {
                var robe = Find(equip, 0x16);
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && tunic.Graphic == 0x0238)
                    return robe.Serial != 0 && robe.Graphic != 0x9985 && robe.Graphic != 0x9986 && robe.Graphic != 0xA412;
                break;
            }
            case 0x0D: {
                var robe = Find(equip, 0x16);
                if (robe.Serial != 0 && robe.Graphic != 0 && robe.Graphic != 0x9985 && robe.Graphic != 0x9986 && robe.Graphic != 0xA412 && robe.Graphic != 0xA2CA)
                    return true;
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && tunic.Graphic != 0x1541 && tunic.Graphic != 0x1542) {
                    var torso = Find(equip, 0x0D);
                    if (torso.Serial != 0 && (torso.Graphic == 0x782A || torso.Graphic == 0x782B)) return true;
                }
                break;
            }
            case 0x13: {
                var robe = Find(equip, 0x16);
                return robe.Serial != 0 && robe.Graphic != 0 && robe.Graphic != 0x9985 && robe.Graphic != 0x9986 && robe.Graphic != 0xA412;
            }
            case 0x06:
            case 0x0B: {
                var robe = Find(equip, 0x16);
                if (robe.Serial != 0 && RobeCoversHead(robe.Graphic)) return true;
                break;
            }
        }
        return false;
    }

    static bool RobeCoversHead(ushort graphic) {
        if (graphic > 0x3173) return graphic == 0x4B9D || graphic == 0x7816;
        if (graphic <= 0x2687) {
            if (graphic < 0x2683) return graphic >= 0x204E && graphic <= 0x204F;
            return true;
        }
        return graphic == 0x2FB9 || graphic == 0x3173;
    }

    /// <summary>
    /// Standing body on the doll. ClassicUO's character picture is PaperDollInteractable (8, 19) plus the body gump at (0, 0).
    /// This host is that point. The frame anchor is the bottom center of that picture, then WorldGump's X = anchor - centerX, Y = anchor - centerY - height.
    /// </summary>
    sealed class BodyArt : Control {
        readonly ushort _body, _gump;

        public BodyArt(ushort body, ushort gump) {
            _body = body;
            _gump = gump;
            AcceptMouseInput = false;
            CanMove = false;
            WantUpdateSize = false;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var assets = UltimaOnline.Current?.Assets;
            var sink = GumpArt.Sink;
            if (assets == null || sink == null || _body == 0) return false;
            var dir = UOLoginNet.PlayerDirection & 7;
            if (!assets.TryBodyFrame(_body, 4, dir, 0, out var px, out var w, out var h, out var cx, out var cy)) return false;
            int ax, ay;
            if (assets.TryGumpSize(_gump, out var gw, out var gh) && gw > 0 && gh > 0) {
                ax = gw >> 1;
                ay = gh;
            }
            else {
                // Slots at x=2 (width 19) and x=162, name at y=262. Host is (8, 19).
                ax = ((2 + 19 + 162) >> 1) - 8;
                ay = 262 - 19;
            }
            var key = unchecked((int)(0x53000000u | ((uint)_body << 8) | ((uint)dir << 4)));
            sink.DrawRaw(key, x + ax - cx, y + ay - cy - h, w, h, px, w, h);
            return true;
        }
    }

    sealed class DollHost : Control {
        public DollHost() {
            X = 8;
            Y = 19;
            AcceptMouseInput = false;
            CanMove = false;
            WantUpdateSize = false;
        }
    }

    sealed class BasePic : GumpPic {
        readonly PaperdollGump _owner;

        public BasePic(PaperdollGump owner, ushort graphic) : base(0, 0, graphic, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (_owner.IsMinimized) _owner.IsMinimized = false;
            return true;
        }
    }

    sealed class VirtuePic : GumpPic {
        readonly uint _serial;

        public VirtuePic(uint serial) : base(80, 4, 0x0071, 0) => _serial = serial;

        public override bool OnMouseDoubleClick(int x, int y) {
            UOLoginNet.ReplyGump(UOLoginNet.PlayerSerial, 0x000001CD, 0x00000001, new[] { _serial }, EmptyEntries);
            return true;
        }
    }

    sealed class ProfilePic : GumpPic {
        readonly uint _serial;

        public ProfilePic(int x, uint serial) : base(x, 196, 0x07D2, 0) => _serial = serial;

        public override bool OnMouseDoubleClick(int x, int y) {
            UOLoginNet.Send(OutgoingPackets.Send_ProfileRequest(_serial));
            return true;
        }
    }

    sealed class PartyPic : GumpPic {
        readonly PaperdollGump _owner;

        public PartyPic(PaperdollGump owner, int x) : base(x, 196, 0x07D2, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (UIManager.GetGump<PartyGump>() == null) UIManager.Add(new PartyGump(_owner.World));
            return true;
        }
    }

    sealed class MinBox : Control {
        readonly PaperdollGump _owner;

        public MinBox(PaperdollGump owner) {
            _owner = owner;
            X = 228;
            Y = 260;
            Width = 16;
            Height = 16;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
        }

        public override void OnMouseClick(int x, int y) {
            if (!_owner.IsMinimized) _owner.IsMinimized = true;
        }
    }

    sealed class EquipSlot : Control {
        readonly PaperdollGump _owner;
        readonly UseLabel _lift;
        int _key, _w, _h;
        byte[] _px;

        public EquipSlot(PaperdollGump owner, byte layer, int y) {
            _owner = owner;
            Layer = layer;
            X = 2;
            Y = y;
            Width = 19;
            Height = 20;
            WantUpdateSize = false;
            CanMove = false;
            AcceptMouseInput = true;
            Add(new GumpPicTiled(0, 0, 19, 20, 0x243A) { AcceptMouseInput = false, CanMove = false });
            Add(new GumpPic(0, 0, 0x2344, 0) { AcceptMouseInput = false, CanMove = false });
            _lift = new UseLabel("", 0) { Width = 19, Height = 20, IsVisible = false };
            Add(_lift);
        }

        public byte Layer { get; }

        public void Show(UOEquipState item) {
            LocalSerial = item.Serial;
            var drag = item.Serial != 0 && _owner.AllowsDrag(Layer);
            _lift.Serial = drag ? item.Serial : 0;
            _lift.IsVisible = drag;
            _px = null;
            if (item.Serial != 0 && TryArt(item.Graphic, out _key, out _px, out _w, out _h)) return;
            _px = null;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var ok = base.Draw(x, y);
            if (_px != null && GumpArt.Sink != null) {
                var dw = _w < 18 ? _w : 18;
                var dh = _h < 18 ? _h : 18;
                GumpArt.Sink.DrawRaw(_key, x + (18 >> 1) - (dw >> 1), y + (18 >> 1) - (dh >> 1), dw, dh, _px, _w, _h);
            }
            return ok;
        }

        public override void OnMouseClick(int x, int y) => ClickItem(LocalSerial);

        public override bool OnMouseDoubleClick(int x, int y) => DoubleItem(LocalSerial);
    }

    sealed class Worn : Control {
        readonly ushort _graphic, _hue;

        public Worn(uint serial, int x, int y, ushort graphic, ushort hue, bool drag) {
            LocalSerial = serial;
            X = x;
            Y = y;
            _graphic = graphic;
            _hue = hue;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            if (!GumpMetrics.TrySize(graphic, out var w, out var h) || w <= 0 || h <= 0) {
                Dispose();
                return;
            }
            Width = w;
            Height = h;
            if (drag) Add(new UseLabel("", 0) { Serial = serial, Width = w, Height = h });
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            GumpArtDraw.Pic(_graphic, x, y, Width, Height, false, _hue);
            return base.Draw(x, y);
        }

        public override void OnMouseClick(int x, int y) => ClickItem(LocalSerial);

        public override bool OnMouseDoubleClick(int x, int y) => DoubleItem(LocalSerial);
    }

    sealed class CombatBookPic : GumpPic {
        readonly PaperdollGump _owner;

        public CombatBookPic(PaperdollGump owner) : base(156, 200, 0x2B34, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            CombatBookGump.Open(_owner.World);
            return true;
        }
    }

    sealed class RacialBookPic : GumpPic {
        readonly PaperdollGump _owner;

        public RacialBookPic(PaperdollGump owner) : base(23, 200, 0x2B28, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (UIManager.GetGump<RacialAbilitiesBookGump>() == null)
                UIManager.Add(new RacialAbilitiesBookGump(_owner.World, 100, 100));
            return true;
        }
    }
}
