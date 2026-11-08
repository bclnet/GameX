// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using OpenX.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO PaperDollInteractable. Worn rows come from UOLoginNet.CopyEquip.
/// The body and each layer are gump graphics. Tileart and ConvertBodyIfNeeded are not on Assets.
/// Hue and layer are stored on the equipment picture. Hair, beard, and the backpack are not lifted.
/// No profile, so the player's backpack skin is 0xC4F6 when that gump exists.
/// </summary>
public class PaperDollInteractable : Control {
    const int N = 25;
    const int MaleGump = 50000;
    const int FemaleGump = 60000;

    // PaperdollOrder T1, T2, T3. Index 0 is the sentinel. Later index paints on top.
    static readonly byte[] T1 = [
        0x00, 0x14, 0x05, 0x04, 0x03, 0x18, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x13, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];
    static readonly byte[] T2 = [
        0x00, 0x14, 0x05, 0x04, 0x03, 0x18, 0x13, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];
    static readonly byte[] T3 = [
        0x00, 0x14, 0x0D, 0x05, 0x04, 0x03, 0x18, 0x11, 0x08, 0x0E, 0x0F, 0x13, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];

    static Binary_TileData.ItemData[] _items;
    static Dictionary<ushort, Dictionary<ushort, ushort>> _conv;
    static bool _tables;

    readonly PaperdollGump _doll;
    readonly List<UOEquipState> _equip = [];
    readonly List<UOEquipState> _scan = [];
    readonly List<UOContainedState> _rows = [];
    bool _updateUI = true;
    int _rev = -1;

    public PaperDollInteractable(int x, int y, uint serial, PaperdollGump paperDollGump) {
        X = x;
        Y = y;
        _doll = paperDollGump;
        AcceptMouseInput = false;
        LocalSerial = serial;
        CanMove = false;
    }

    public bool HasFakeItem { get; private set; }

    public override void Update() {
        if (IsDisposed) return;
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            _updateUI = true;
        }
        if (_updateUI) {
            UpdateUI();
            _updateUI = false;
        }
        if (IsDisposed) return;
        base.Update();
    }

    public override bool Draw(int x, int y) => base.Draw(x, y);

    public void SetFakeItem(bool value) {
        _updateUI = HasFakeItem && !value || !HasFakeItem && value;
        HasFakeItem = value;
    }

    public void RequestUpdate() => _updateUI = true;

    void UpdateUI() {
        if (IsDisposed) return;
        var body = MobileBody();
        if (body == 0 && LocalSerial != UOLoginNet.PlayerSerial) {
            Dispose();
            return;
        }
        Clear();
        WantUpdateSize = true;
        var hue = (ushort)0;
        ushort extra = 0;
        var bodyId = DollGump(body, ref hue, ref extra);
        var bodyPic = new GumpPic(0, 0, bodyId, hue) { IsPartialHue = true, AcceptMouseInput = false, CanMove = false };
        if (!bodyPic.IsDisposed) Add(bodyPic);
        if (extra != 0) {
            // Mobile hue is not stored. The 0x03DB overlay stays unhued. The body uses 0x03EA.
            var over = new GumpPic(0, 0, extra, 0) { IsPartialHue = true, AcceptMouseInput = true, CanMove = false };
            if (!over.IsDisposed) Add(over);
        }
        UOLoginNet.CopyEquip(LocalSerial, _equip);
        var gfx = LayerGfx(_equip);
        Held held = default;
        var fake = HasFakeItem && TryHeld(out held);
        if (fake && held.Layer > 0 && held.Layer < gfx.Length && gfx[held.Layer] == 0 && held.Anim != 0)
            gfx[held.Layer] = held.Anim;
        var female = Female(body);
        var order = new byte[N];
        var layers = new byte[N];
        Build(gfx, female || Gargoyle(body), order);
        var count = Filter(order, layers);
        for (var i = 0; i < count; i++) {
            var layer = layers[i];
            var item = Find(_equip, layer);
            if (item.Serial != 0) {
                if (Covered(_equip, layer, body)) continue;
                if (!TryWear(body, female, item.Graphic, 0, out var id)) continue;
                var worn = new GumpPicEquipment(_doll, item.Serial, 0, 0, id, (ushort)(item.Hue & 0x3FFF), layer) {
                    IsPartialHue = Partial(item.Graphic),
                    CanLift = AllowsLift(layer),
                };
                if (!worn.IsDisposed) Add(worn);
            }
            else if (fake && held.Layer == layer && held.Anim != 0 && TryWear(body, female, held.Graphic, held.Anim, out var ghostId)) {
                // GumpArtDraw has no alpha. The field is stored and the preview stays opaque.
                var ghost = new GumpPicEquipment(_doll, 0, 0, 0, ghostId, (ushort)(held.Hue & 0x3FFF), held.Layer) {
                    IsPartialHue = held.Partial,
                    Alpha = 0.5f,
                };
                if (!ghost.IsDisposed) Add(ghost);
            }
        }
        var pack = Find(_equip, 0x15);
        if (pack.Serial != 0 && AnimOf(pack.Graphic) != 0 && TryPack(LocalSerial == UOLoginNet.PlayerSerial, pack.Graphic, out var packGump)) {
            var bx = (UOLoginNet.Features & 0x20) != 0 ? -6 : 0;
            var worn = new GumpPicEquipment(_doll, pack.Serial, bx, 0, packGump, (ushort)(pack.Hue & 0x3FFF), 0x15);
            if (!worn.IsDisposed) Add(worn);
        }
    }

    void Clear() {
        for (var i = Children.Count - 1; i >= 0; i--) {
            var c = Children[i];
            c.Parent = null;
            c.Dispose();
        }
    }

    ushort MobileBody() {
        if (LocalSerial == UOLoginNet.PlayerSerial) return UOLoginNet.PlayerBody;
        var world = _doll?.World;
        if (world != null && world.Mobiles.TryGetValue(LocalSerial, out var obj) && obj is Mobile m) return m.Body;
        return 0;
    }

    // The player is dead, or this is hair or beard, or the doll is not ours and the lift bit is off.
    bool AllowsLift(byte layer) {
        if (layer == 0x0B || layer == 0x10) return false;
        if (UOLoginNet.IsDead(UOLoginNet.PlayerSerial)) return false;
        return _doll != null && (_doll.CanLift || LocalSerial == UOLoginNet.PlayerSerial);
    }

    bool TryHeld(out Held held) {
        held = default;
        if (!HasFakeItem || !UOLoginNet.Holding) return false;
        var serial = UOLoginNet.HeldSerial;
        if (serial == 0 || !Lookup(serial, out var graphic, out var hue, out var layer)) return false;
        var anim = AnimOf(graphic);
        if (anim == 0 || layer == 0) return false;
        held = new Held(graphic, hue, layer, anim, Partial(graphic));
        return true;
    }

    bool Lookup(uint serial, out ushort graphic, out ushort hue, out byte layer) {
        graphic = hue = 0;
        layer = 0;
        if (InEquip(LocalSerial, serial, out graphic, out hue, out layer)) return true;
        var player = UOLoginNet.PlayerSerial;
        if (player != 0 && player != LocalSerial && InEquip(player, serial, out graphic, out hue, out layer)) return true;
        var world = _doll?.World;
        if (world != null)
            foreach (var kv in world.Mobiles)
                if (kv.Key != LocalSerial && kv.Key != player && InEquip(kv.Key, serial, out graphic, out hue, out layer))
                    return true;
        if (InBag(UOLoginNet.PlayerBackpack, serial, out graphic, out hue)) {
            layer = TileLayer(graphic);
            return layer != 0;
        }
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (!n.Value.IsDisposed && n.Value is ContainerGump bag && InBag(bag.ServerSerial, serial, out graphic, out hue)) {
                layer = TileLayer(graphic);
                return layer != 0;
            }
        if (world != null && world.Items.TryGetValue(serial, out var obj) && obj is Item item) {
            graphic = (ushort)(item.Graphic & 0x7FFF);
            hue = 0;
            layer = TileLayer(graphic);
            return graphic != 0 && layer != 0;
        }
        return false;
    }

    bool InEquip(uint parent, uint serial, out ushort graphic, out ushort hue, out byte layer) {
        graphic = hue = 0;
        layer = 0;
        if (parent == 0) return false;
        UOLoginNet.CopyEquip(parent, _scan);
        for (var i = 0; i < _scan.Count; i++)
            if (_scan[i].Serial == serial) {
                graphic = (ushort)(_scan[i].Graphic & 0x7FFF);
                hue = _scan[i].Hue;
                layer = _scan[i].Layer;
                return true;
            }
        return false;
    }

    bool InBag(uint container, uint serial, out ushort graphic, out ushort hue) {
        graphic = hue = 0;
        if (container == 0) return false;
        UOLoginNet.CopyContents(container, _rows);
        for (var i = 0; i < _rows.Count; i++)
            if (_rows[i].Serial == serial) {
                graphic = (ushort)(_rows[i].Graphic & 0x7FFF);
                hue = _rows[i].Hue;
                return true;
            }
        return false;
    }

    // PaperDollInteractable body gump. No IsFemale flag, so an unlisted body is the male gump 0x000C.
    static ushort DollGump(ushort mobile, ref ushort hue, ref ushort extra) {
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

    static bool Female(ushort body) => body == 0x0191 || body == 0x0193 || body == 0x025E || body == 0x029B;

    static bool Gargoyle(ushort body) => body == 0x029A || body == 0x029B || body == 0x02B6 || body == 0x02B7;

    static ushort[] LayerGfx(List<UOEquipState> equip) {
        var gfx = new ushort[N];
        for (var i = 0; i < equip.Count; i++) {
            var layer = equip[i].Layer;
            if (layer == 0 || layer >= gfx.Length || gfx[layer] != 0) continue;
            gfx[layer] = AnimOf(equip[i].Graphic);
        }
        return gfx;
    }

    static void Build(ushort[] graphic, bool alt, byte[] order) {
        uint arms = graphic[0x13];
        var armsAlt = arms < 0x3D0 ? arms == 0x3CF || arms == 0x210 || arms == 0x3B3 : arms == 0x3DD;
        byte[] table;
        if (armsAlt) table = T1;
        else {
            uint torso = graphic[0x0D];
            if (torso == 0x21A) table = T1;
            else if (torso - 0x399 < 5 && alt) table = T3;
            else table = T2;
        }
        Array.Copy(table, order, N);
        if (graphic[0x05] != 0 && graphic[0x04] == 0x398) MoveTo(order, 0x04, 0x05);
        uint pants = graphic[0x04];
        var skip = false;
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
        else if (pants - 0x513u < 2) {
            if (graphic[0x03] != 0) MoveTo(order, 0x04, 0x03);
            skip = true;
        }
        if (!skip && graphic[0x03] != 0 && graphic[0x04] == 0x3E4) MoveTo(order, 0x04, 0x03);
        if (graphic[0x11] == 0x238) {
            MoveAfter(order, 0x11, 0x0C);
            if (graphic[0x16] != 0) {
                uint robe = graphic[0x16];
                if (robe == 0x4E8 || robe == 0x4E9 || robe == 0x4EA || robe == 0x4EB || robe == 0x5E2 || robe == 0x5E3 || robe == 0x5E4 || robe == 0x5E5)
                    MoveTo(order, 0x16, 0x0A);
            }
        }
        uint cloak = graphic[0x14];
        if (cloak == 0x380 || cloak == 0x5F3) MoveAfter(order, 0x14, 0x16);
        uint helm = graphic[0x06];
        if (helm < 0x202) {
            uint neck = graphic[0x0A];
            if ((helm == 0x201 || helm == 0x1A9) && (neck == 0x1C8 || neck > 0x1D6 && neck < 0x1D9)) {
                MoveAfter(order, 0x0A, 0x06);
                return;
            }
        }
        else if (helm - 0x5E9u < 2 && graphic[0x16] != 0 && (uint)graphic[0x16] - 0x5E2u < 4)
            MoveTo(order, 0x16, 0x06);
    }

    static int Filter(byte[] order, byte[] dest) {
        var c = 0;
        for (var i = 0; i < N; i++) {
            var layer = order[i];
            if (layer == 0 || layer == 0x15 || layer == 0x19) continue;
            dest[c++] = layer;
        }
        return c;
    }

    static int IndexOf(byte[] a, byte v) {
        for (var i = 0; i < N; i++)
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

    // GetAnimID. A missing gump is skipped. The item graphic is not used as a gump id.
    static bool TryWear(ushort body, bool female, ushort graphic, ushort anim, out ushort gump) {
        gump = 0;
        if (anim == 0) anim = AnimOf(graphic);
        if (anim == 0) return false;
        var ver = UltimaOnline.Current?.Version ?? default;
        if (ver >= ClientVersion.CV_7000 && anim == 0x03CA && (body == 0x02B7 || body == 0x02B6)) anim = 0x0223;
        var conv = Conv();
        if (conv != null && conv.TryGetValue(body, out var dict) && dict.TryGetValue(anim, out var mapped)) {
            if (mapped > MaleGump) anim = (ushort)(mapped >= FemaleGump ? mapped - FemaleGump : mapped - MaleGump);
            else anim = mapped;
        }
        var offset = female ? FemaleGump : MaleGump;
        if (!GumpAt(anim + offset)) offset = female ? MaleGump : FemaleGump;
        if (!GumpAt(anim + offset)) return false;
        gump = (ushort)(anim + offset);
        return true;
    }

    static bool TryPack(bool player, ushort graphic, out ushort gump) {
        gump = 0;
        if (AnimOf(graphic) == 0) return false;
        if (player && GumpAt(0xC4F6)) {
            gump = 0xC4F6;
            return true;
        }
        var id = AnimOf(graphic) + MaleGump;
        if (!GumpAt(id)) return false;
        gump = (ushort)id;
        return true;
    }

    static bool GumpAt(int id) {
        if (id <= 0 || id > 0xFFFF) return false;
        var assets = UltimaOnline.Current?.Assets;
        return assets != null && assets.TryGumpPixels(id, out _, out var w, out var h) && w > 0 && h > 0;
    }

    static ushort AnimOf(ushort graphic) {
        var items = Items();
        graphic = (ushort)(graphic & 0x7FFF);
        if (items == null || graphic >= items.Length) return 0;
        var anim = items[graphic].AnimID;
        return anim <= 0 ? (ushort)0 : (ushort)anim;
    }

    static bool Partial(ushort graphic) {
        var items = Items();
        graphic = (ushort)(graphic & 0x7FFF);
        return items != null && graphic < items.Length && items[graphic].IsPartialHue;
    }

    // The byte after weight is ClassicUO's layer. Binary_TileData stores it in Quality.
    static byte TileLayer(ushort graphic) {
        var items = Items();
        graphic = (ushort)(graphic & 0x7FFF);
        if (items == null || graphic >= items.Length) return 0;
        var layer = items[graphic].Quality;
        return layer < 0 || layer > byte.MaxValue ? (byte)0 : (byte)layer;
    }

    static Binary_TileData.ItemData[] Items() {
        EnsureTables();
        return _items;
    }

    static Dictionary<ushort, Dictionary<ushort, ushort>> Conv() {
        EnsureTables();
        return _conv;
    }

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
            return int.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out var h) ? h : -1;
        var end = s[0] == '-' || s[0] == '+' ? 1 : 0;
        while (end < s.Length && s[end] >= '0' && s[end] <= '9') end++;
        return int.TryParse(s.Substring(0, end), out var n) ? n : -1;
    }

    static UOEquipState Find(List<UOEquipState> equip, byte layer) {
        for (var i = 0; i < equip.Count; i++)
            if (equip[i].Layer == layer) return equip[i];
        return default;
    }

    // MobileView.IsCovered. Robe graphics are the item id. Pants, skirt, and robe tests use AnimID.
    static bool Covered(List<UOEquipState> equip, byte layer, int body) {
        var robe = Find(equip, 0x16);
        var robeGfx = robe.Serial == 0 ? 0 : robe.Graphic & 0x7FFF;
        var robeAnim = robe.Serial == 0 ? 0 : AnimOf(robe.Graphic);
        var open = robeGfx == 0x9985 || robeGfx == 0x9986 || robeGfx == 0xA2CA || robeGfx == 0xA2CB || robeGfx == 0xA412 || robeGfx == 0xB1DE;
        switch (layer) {
            case 0x03: return ShoesCovered(equip, robeAnim);
            case 0x04: return PantsCovered(equip, robe, robeAnim);
            case 0x11: {
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && AnimOf(tunic.Graphic) == 0x0238) return robe.Serial != 0 && !open;
                break;
            }
            case 0x0D: {
                if (robeGfx != 0 && !open) return true;
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && (tunic.Graphic & 0x7FFF) != 0x1541 && (tunic.Graphic & 0x7FFF) != 0x1542) {
                    var torso = Find(equip, 0x0D);
                    if (torso.Serial != 0 && ((torso.Graphic & 0x7FFF) == 0x782A || (torso.Graphic & 0x7FFF) == 0x782B)) return true;
                }
                break;
            }
            case 0x13: return robeGfx != 0 && !open;
            case 0x0A: {
                if (robe.Serial == 0) return false;
                if (robeAnim == 0x5F2 || robeAnim == 0x5F5 || robeAnim >= 0x4E8 && robeAnim <= 0x4EB || robeAnim >= 0x5E2 && robeAnim <= 0x5E5)
                    return false;
                var neck = Find(equip, 0x0A);
                return neck.Serial != 0 && AnimOf(neck.Graphic) == 0x5EC;
            }
            case 0x0E: {
                var bracelet = Find(equip, 0x0E);
                return bracelet.Serial != 0 && (bracelet.Graphic & 0x7FFF) == 0xB1C0 && Find(equip, 0x13).Serial != 0;
            }
            case 0x0B: {
                var helmet = Find(equip, 0x06);
                if (helmet.Serial != 0 && (uint)((helmet.Graphic & 0x7FFF) - 0xA42B) < 2) return true;
                goto case 0x06;
            }
            case 0x06: return HeadCovered(robeGfx, body);
            case 0x17: {
                var skirt = Find(equip, 0x17);
                var skirtAnim = skirt.Serial == 0 ? 0 : AnimOf(skirt.Graphic);
                if (skirtAnim != 0x1C7 && skirtAnim != 0x1E4) return false;
                var pants = Find(equip, 0x04);
                var pantsAnim = pants.Serial == 0 ? 0 : AnimOf(pants.Graphic);
                return pantsAnim == 0x1EB || pantsAnim == 0x1FA || pantsAnim == 0x200;
            }
        }
        return false;
    }

    static bool ShoesCovered(List<UOEquipState> equip, int robeAnim) {
        var pants = Find(equip, 0x04);
        var pantsAnim = pants.Serial == 0 ? 0 : AnimOf(pants.Graphic);
        if (robeAnim != 0x504 && pantsAnim != 0x513 && pantsAnim != 0x514) {
            var pantsGfx = pants.Serial == 0 ? 0 : pants.Graphic & 0x7FFF;
            if (pantsGfx < 0xAEB2) {
                if (pantsGfx == 0xAEB1 || pantsGfx == 0x1411) return true;
                if (pantsGfx != 0xAEA2) return Find(equip, 0x18).Serial != 0;
            }
            else {
                if (pantsGfx == 0xAEC0) return true;
                if (pantsGfx != 0xAECF) return Find(equip, 0x18).Serial != 0;
            }
        }
        return true;
    }

    static bool PantsCovered(List<UOEquipState> equip, UOEquipState robe, int robeAnim) {
        if (Find(equip, 0x18).Serial != 0 || robeAnim == 0x504) return true;
        var pants = Find(equip, 0x04);
        var pantsAnim = pants.Serial == 0 ? 0 : AnimOf(pants.Graphic);
        if (pantsAnim != 0x1EB && pantsAnim != 0x1FA && pantsAnim != 0x200) return false;
        var skirt = Find(equip, 0x17);
        if (skirt.Serial != 0) {
            var skirtAnim = AnimOf(skirt.Graphic);
            if (skirtAnim != 0x1C7 && skirtAnim != 0x1E4) return true;
        }
        if (robe.Serial == 0) return false;
        if (robeAnim < 0x4EC) {
            if (robeAnim > 0x4E7) return false;
            return robeAnim != 0x229;
        }
        return (uint)(robeAnim - 0x5E2) > 3;
    }

    static bool HeadCovered(int robeGfx, int body) {
        if (robeGfx < 0x4B9E) {
            if (robeGfx != 0x4B9D) {
                if (robeGfx < 0x2FBA) {
                    if (robeGfx != 0x2FB9) {
                        if (robeGfx > 0x2687) return false;
                        if (robeGfx < 0x2683 && (robeGfx < 0x204E || robeGfx > 0x204F)) return false;
                    }
                }
                else if (robeGfx != 0x3173) return false;
                return true;
            }
        }
        else if (robeGfx < 0xA0B0) {
            if (robeGfx < 0xA0AB && robeGfx != 0x7816) return false;
        }
        else if (robeGfx != 0xB2B7) return false;
        var garg = body == 0x029A || body == 0x029B || body == 0x02B6 || body == 0x02B7;
        return !garg;
    }

    readonly struct Held {
        public readonly ushort Graphic, Hue, Anim;
        public readonly byte Layer;
        public readonly bool Partial;
        public Held(ushort graphic, ushort hue, byte layer, ushort anim, bool partial) {
            Graphic = graphic;
            Hue = hue;
            Layer = layer;
            Anim = anim;
            Partial = partial;
        }
    }

    /// <summary>One worn gump. Hue and layer stay here. A drag calls Lift. A double-click calls Use.</summary>
    public sealed class GumpPicEquipment : GumpPic {
        readonly PaperdollGump _owner;

        public GumpPicEquipment(PaperdollGump gump, uint serial, int x, int y, ushort graphic, ushort hue, byte layer)
            : base(x, y, graphic, hue) {
            _owner = gump;
            LocalSerial = serial;
            CanMove = false;
            Layer = layer;
            AcceptMouseInput = true;
            WantUpdateSize = false;
        }

        public byte Layer { get; }
        public bool CanLift { get; set; }

        public override void Update() {
            if (IsDisposed) return;
            // UpdateAbilities is not on UOLoginNet. A weapon lift is still 0x07.
            if (CanLift && LocalSerial != 0 && _owner != null && !_owner.IsDisposed && !UOLoginNet.Holding && !UOLoginNet.HasTarget
                && !UOLoginNet.IsDead(UOLoginNet.PlayerSerial) && Mouse.LButtonPressed
                && UIManager.LastControlMouseDown((int)MouseButtonType.Left) == this
                && (Waited() || Mouse.LDragOffset.X != 0 || Mouse.LDragOffset.Y != 0))
                UOLoginNet.Lift(LocalSerial);
            base.Update();
        }

        public override void OnMouseClick(int x, int y) {
            if (LocalSerial != 0 && UOLoginNet.HasTarget) UOLoginNet.TargetSerial(LocalSerial);
        }

        public override bool OnMouseDoubleClick(int x, int y) {
            if (LocalSerial == 0) return true;
            if (UOLoginNet.HasTarget || UOLoginNet.TargetedJustNow) {
                if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(LocalSerial);
                return true;
            }
            UOLoginNet.Use(LocalSerial);
            return true;
        }

        static bool Waited() {
            var last = Mouse.LastLeftButtonClickTime;
            return last != 0 && last != 0xFFFF_FFFF && last + Mouse.MOUSE_DELAY_DOUBLE_CLICK < GlobalTime.Ticks;
        }
    }
}
