// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MobileView equipment on a world body. The mount is layer 0x19 and paints under the body.
/// Other layers follow PaperdollOrder.BuildInWorld. Backpack, bank, and shop layers are not sprites.
/// The graphic is tiledata AnimID, then Equipconv.def. Assets does not expose AnimID.
/// Worn layers keep the body action and direction. A mount uses GroupFor on its own id.
/// </summary>
public static class MobileArt {
    const int N = 0x19;

    // PaperdollOrder T1, T2, T3. Leading 0 is the sentinel. Later index paints on top.
    static readonly byte[] T1 = [
        0x00, 0x14, 0x05, 0x04, 0x03, 0x18, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x13, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];
    static readonly byte[] T2 = [
        0x00, 0x14, 0x05, 0x04, 0x03, 0x18, 0x13, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];
    static readonly byte[] T3 = [
        0x00, 0x14, 0x0D, 0x05, 0x04, 0x03, 0x18, 0x11, 0x08, 0x0E, 0x0F, 0x13, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x15, 0x09,
    ];

    // ClassicUO Mounts. Item graphic, animation body, rider Y offset.
    static readonly MountRow[] MountRows = [
        new(0x3E90, 0x0114, 0), new(0x3E91, 0x0115, 0), new(0x3E92, 0x011C, 0), new(0x3E94, 0x00F3, 0),
        new(0x3E95, 0x00A9, 0), new(0x3E97, 0x00C3, 0), new(0x3E98, 0x00C2, 0), new(0x3E9A, 0x00C1, 0),
        new(0x3E9B, 0x00C0, -9), new(0x3E9D, 0x00C0, -9), new(0x3E9C, 0x00BF, 0), new(0x3E9E, 0x00BE, 0),
        new(0x3EA0, 0x00E2, 0), new(0x3EA1, 0x00E4, 0), new(0x3EA2, 0x00CC, 0), new(0x3EA3, 0x00D2, 0),
        new(0x3EA4, 0x00DA, 0), new(0x3EA5, 0x00DB, 0), new(0x3EA6, 0x00DC, 0), new(0x3EA7, 0x0074, 0),
        new(0x3EA8, 0x0075, 0), new(0x3EA9, 0x0072, 0), new(0x3EAA, 0x0073, 0), new(0x3EAB, 0x00AA, 0),
        new(0x3EAC, 0x00AB, 0), new(0x3EAD, 0x0084, 0), new(0x3EAF, 0x0078, 0), new(0x3EB0, 0x0079, 0),
        new(0x3EB1, 0x0077, 0), new(0x3EB2, 0x0076, 0), new(0x3EB3, 0x0090, 0), new(0x3EB4, 0x007A, -9),
        new(0x3EB5, 0x00B1, 0), new(0x3EB6, 0x00B2, 0), new(0x3EB7, 0x00B3, 0), new(0x3EB8, 0x00BC, 0),
        new(0x3EBA, 0x00BB, 0), new(0x3EBB, 0x0319, -9), new(0x3EBC, 0x0317, 0), new(0x3EBD, 0x031A, 0),
        new(0x3EBE, 0x031F, 0), new(0x3EC3, 0x02D4, 0), new(0x3ECE, 0x059A, 0), new(0x3EC5, 0x00D5, 0),
        new(0x3F3A, 0x00D5, 0), new(0x3EC6, 0x01B0, 9), new(0x3EC7, 0x04E6, 18), new(0x3EC8, 0x04E7, 18),
        new(0x3EC9, 0x042D, 3), new(0x3ECA, 0x0579, 9), new(0x3ECC, 0x0582, 0), new(0x3ED1, 0x05E6, 0),
        new(0x3ECB, 0x057F, 0), new(0x3ED0, 0x05A1, 18), new(0x3ED2, 0x05F6, 9), new(0x3ECD, 0x0580, 0),
        new(0x3ECF, 0x05A0, 9), new(0x3ED3, 0x05F7, 18), new(0x3ED4, 0x060A, 0), new(0x3ED5, 0x060B, 0),
        new(0x3ED6, 0x060C, 0), new(0x3ED7, 0x060D, 0), new(0x3ED8, 0x060F, 0), new(0x3ED9, 0x0610, 0),
        new(0x3EDA, 0x0590, 9), new(0x3EDB, 0x0611, 12), new(0x3EDC, 0x0666, 12), new(0x3EDD, 0x0581, 0),
        new(0x3EDE, 0x0673, -10), new(0x3EDF, 0x0674, -10), new(0x3EE0, 0x0675, -10), new(0x3EE1, 0x0678, -10),
        new(0x3EE2, 0x0679, -10), new(0x3EE4, 0x06C1, -4),
    ];

    static Binary_TileData.ItemData[] _tiles;
    static bool _tilesMiss;
    static Dictionary<int, Dictionary<ushort, ushort>> _conv;
    static bool _convMiss;

    public struct Sprite {
        public int Key, X, Y, W, H, Ord, Seq;
        public byte[] Px;
    }

    readonly struct MountRow {
        public readonly ushort Item, Anim;
        public readonly sbyte Off;
        public MountRow(ushort item, ushort anim, sbyte off) {
            Item = item;
            Anim = anim;
            Off = off;
        }
    }

    /// <summary>
    /// Appends sprites for one mobile. ord is the body stamp. Mount is ord - 1 and seq 1, so it sits above a static and under the body.
    /// Worn pieces keep ord and take seq 1 and up, so they paint over the body.
    /// </summary>
    public static void Draw(Assets assets, List<UOEquipState> worn, int body, int action, int direction, int frame, bool moving, bool run, bool war, bool twoHanded, int anchorX, int anchorY, int ord, List<Sprite> dest) {
        if (assets == null || worn == null || dest == null || !Human(body)) return;
        var dir = direction & 7;
        var mount = Find(worn, 0x19);
        if (mount.Serial != 0 && mount.Graphic != 0x3E96) {
            var id = MountBody(mount.Graphic);
            var act = id > 0 && id <= 0xFFFF ? assets.GroupFor((ushort)id, moving, run, war, twoHanded, false, 0) : action;
            if (!Put(assets, id, act, dir, frame, anchorX, anchorY - MountOff(mount.Graphic), ord - 1, 1, dest) && act != action)
                Put(assets, id, action, dir, frame, anchorX, anchorY - MountOff(mount.Graphic), ord - 1, 1, dest);
        }
        var gfx = new ushort[N];
        for (var i = 0; i < worn.Count; i++) {
            var e = worn[i];
            if (e.Layer == 0 || e.Layer >= N || gfx[e.Layer] != 0) continue;
            var anim = AnimId(e.Graphic);
            if (anim != 0) gfx[e.Layer] = (ushort)anim;
        }
        var built = new byte[N];
        var layers = new byte[N];
        Build(gfx, Female(body) || Gargoyle(body), built);
        var count = Cloak(layers, Filter(built, layers), (byte)dir);
        var dead = Dead(body);
        var seq = 1;
        for (var i = 0; i < count; i++) {
            var layer = layers[i];
            if (dead && (layer == 0x0B || layer == 0x10)) continue;
            if (Covered(worn, layer, body)) continue;
            var item = Find(worn, layer);
            if (item.Serial == 0) continue;
            var anim = AnimId(item.Graphic);
            if (anim == 0) continue;
            var graphic = Gargoyle(body) ? GargoyleFix(anim) : anim;
            graphic = Conv(body, anim, graphic);
            if (Put(assets, graphic, action, dir, frame, anchorX, anchorY, ord, seq, dest)) seq++;
        }
    }

    static bool Put(Assets assets, int id, int action, int dir, int frame, int anchorX, int anchorY, int ord, int seq, List<Sprite> dest) {
        if (id <= 0 || id > 0xFFFF) return false;
        if (assets.TryFrameCount(id, action, dir, out var count) && count > 0 && frame >= count) frame = count - 1;
        if (!assets.TryBodyFrame(id, action, dir, frame, out var px, out var w, out var h, out var cx, out var cy)) return false;
        dest.Add(new Sprite {
            Key = 0x30000 + (id << 16) + ((action & 0x7F) << 9) + ((dir & 7) << 6) + (frame & 0x3F),
            X = anchorX - cx,
            Y = anchorY - cy - h,
            W = w,
            H = h,
            Px = px,
            Ord = ord,
            Seq = seq,
        });
        return true;
    }

    // Item.GetGraphicForAnimation for a mount. Ethereal ids return before the table. AnimID wins after it.
    static int MountBody(ushort graphic) {
        if (graphic == 0x3E9B || graphic == 0x3E9D) return 0x00C0;
        if (graphic == 0x3E9C) return 0x00BF;
        var body = (int)graphic;
        if (TryMount(graphic, out var row)) body = row.Anim;
        var anim = AnimId(graphic);
        if (anim != 0) body = anim;
        return body;
    }

    static int MountOff(ushort graphic) => TryMount(graphic, out var row) ? row.Off : 0;

    static bool TryMount(ushort graphic, out MountRow row) {
        for (var i = 0; i < MountRows.Length; i++)
            if (MountRows[i].Item == graphic) { row = MountRows[i]; return true; }
        row = default;
        return false;
    }

    static bool Human(int g) =>
        g >= 0x0190 && g <= 0x0193
        || g >= 0x00B7 && g <= 0x00BA
        || g >= 0x025D && g <= 0x0260
        || g == 0x029A || g == 0x029B || g == 0x02B6 || g == 0x02B7
        || g == 0x03DB || g == 0x03DF || g == 0x03E2
        || g == 0x02E8 || g == 0x02E9 || g == 0x04E5;

    static bool Dead(int g) => g == 0x0192 || g == 0x0193 || g >= 0x025F && g <= 0x0260 || g == 0x02B6 || g == 0x02B7;

    static bool Female(int g) => g == 0x0191 || g == 0x0193 || g == 0x025E || g == 0x029B;

    // MobileView's draw-path gargoyle, not the property. Client 7.0 or newer.
    static bool Gargoyle(int g) {
        var v = UltimaOnline.Current?.Version ?? 0;
        return v >= ClientVersion.CV_7000 && (g == 0x029A || g == 0x029B || g == 0x02B6 || g == 0x02B7);
    }

    static int GargoyleFix(int graphic) {
        switch (graphic) {
            case 0x01D5: return 0x0156;
            case 0x03CA: return 0x0223;
            case 0x03D8: return 329;
            case 0x0372: return 330;
            case 0x0374: return 328;
            case 0x036F: return 327;
            case 0x036E: return 328;
            case 0x0426: return 0x042B;
            case 1529: return 1531;
            case 1530: return 1532;
            default: return graphic;
        }
    }

    static void Build(ushort[] gfx, bool alt, byte[] order) {
        uint arms = gfx[0x13];
        var armsAlt = arms < 0x3D0 ? arms == 0x3CF || arms == 0x210 || arms == 0x3B3 : arms == 0x3DD;
        byte[] table;
        if (armsAlt) table = T1;
        else {
            uint torso = gfx[0x0D];
            if (torso == 0x21A) table = T1;
            else if (torso - 0x399 < 5 && alt) table = T3;
            else table = T2;
        }
        Array.Copy(table, order, N);
        if (gfx[0x05] != 0 && gfx[0x04] == 0x398) MoveTo(order, 0x04, 0x05);
        uint pants = gfx[0x04];
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
            if (gfx[0x03] != 0) MoveTo(order, 0x04, 0x03);
            skip = true;
        }
        if (!skip && gfx[0x03] != 0 && gfx[0x04] == 0x3E4) MoveTo(order, 0x04, 0x03);
        if (gfx[0x11] == 0x238) {
            MoveAfter(order, 0x11, 0x0C);
            if (gfx[0x16] != 0) {
                uint r = gfx[0x16];
                if (r == 0x4E8 || r == 0x4E9 || r == 0x4EA || r == 0x4EB || r == 0x5E2 || r == 0x5E3 || r == 0x5E4 || r == 0x5E5)
                    MoveTo(order, 0x16, 0x0A);
            }
        }
        uint cloak = gfx[0x14];
        if (cloak == 0x380 || cloak == 0x5F3) MoveAfter(order, 0x14, 0x16);
        uint helm = gfx[0x06];
        if (helm < 0x202) {
            uint neck = gfx[0x0A];
            if ((helm == 0x201 || helm == 0x1A9) && (neck == 0x1C8 || neck > 0x1D6 && neck < 0x1D9)) {
                MoveAfter(order, 0x0A, 0x06);
                return;
            }
        }
        else if (helm - 0x5E9u < 2 && gfx[0x16] != 0 && (uint)gfx[0x16] - 0x5E2u < 4)
            MoveTo(order, 0x16, 0x06);
    }

    static int Filter(byte[] order, byte[] dest) {
        var c = 0;
        for (var i = 0; i < N; i++) {
            var layer = order[i];
            if (layer == 0 || layer == 0x19 || layer == 0x15) continue;
            dest[c++] = layer;
        }
        return c;
    }

    // Facing, not the paperdoll cloak rule. North on top, southeast behind, otherwise just under the helmet.
    static int Cloak(byte[] layers, int count, byte dir) {
        var idx = -1;
        for (var i = 0; i < count; i++)
            if (layers[i] == 0x14) { idx = i; break; }
        if (idx < 0) return count;
        for (var i = idx; i < count - 1; i++) layers[i] = layers[i + 1];
        count--;
        int insert;
        if (dir == 0) insert = count;
        else if (dir == 3) insert = 0;
        else {
            insert = count;
            for (var i = 0; i < count; i++)
                if (layers[i] == 0x06) { insert = i; break; }
        }
        for (var i = count; i > insert; i--) layers[i] = layers[i - 1];
        layers[insert] = 0x14;
        return count + 1;
    }

    static int IndexOf(byte[] a, byte v) {
        for (var i = 0; i < N; i++)
            if (a[i] == v) return i;
        return -1;
    }

    static void MoveTo(byte[] a, byte A, byte B) {
        var i1 = IndexOf(a, A);
        if (i1 < 0) return;
        var i2 = IndexOf(a, B);
        if (i2 < 0 || i2 == i1) return;
        if (i2 < i1) { for (var k = i1; k > i2; k--) a[k] = a[k - 1]; a[i2] = A; }
        else { for (var k = i1; k < i2; k++) a[k] = a[k + 1]; a[i2] = A; }
    }

    static void MoveAfter(byte[] a, byte A, byte B) {
        var i1 = IndexOf(a, A);
        if (i1 < 0) return;
        var i2 = IndexOf(a, B);
        if (i2 < 0 || i2 == i1) return;
        if (i2 < i1) { for (var k = i1; k > i2 + 1; k--) a[k] = a[k - 1]; a[i2 + 1] = A; }
        else { for (var k = i1; k < i2; k++) a[k] = a[k + 1]; a[i2] = A; }
    }

    // MobileView.IsCovered. Robe graphics are the item id. Pants, skirt, and robe tests use AnimID.
    static bool Covered(List<UOEquipState> equip, byte layer, int body) {
        var robe = Find(equip, 0x16);
        var robeGfx = robe.Serial == 0 ? 0 : robe.Graphic;
        var robeAnim = robe.Serial == 0 ? 0 : AnimId(robe.Graphic);
        var open = robeGfx == 0x9985 || robeGfx == 0x9986 || robeGfx == 0xA2CA || robeGfx == 0xA2CB || robeGfx == 0xA412 || robeGfx == 0xB1DE;
        switch (layer) {
            case 0x03: return ShoesCovered(equip, robeAnim);
            case 0x04: return PantsCovered(equip, robe, robeAnim);
            case 0x11: {
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && AnimId(tunic.Graphic) == 0x0238) return robe.Serial != 0 && !open;
                break;
            }
            case 0x0D: {
                if (robeGfx != 0 && !open) return true;
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && tunic.Graphic != 0x1541 && tunic.Graphic != 0x1542) {
                    var torso = Find(equip, 0x0D);
                    if (torso.Serial != 0 && (torso.Graphic == 0x782A || torso.Graphic == 0x782B)) return true;
                }
                break;
            }
            case 0x13: return robeGfx != 0 && !open;
            case 0x0A: {
                if (robe.Serial == 0) return false;
                if (robeAnim == 0x5F2 || robeAnim == 0x5F5 || robeAnim >= 0x4E8 && robeAnim <= 0x4EB || robeAnim >= 0x5E2 && robeAnim <= 0x5E5)
                    return false;
                var neck = Find(equip, 0x0A);
                return neck.Serial != 0 && AnimId(neck.Graphic) == 0x5EC;
            }
            case 0x0E: {
                var bracelet = Find(equip, 0x0E);
                return bracelet.Serial != 0 && bracelet.Graphic == 0xB1C0 && Find(equip, 0x13).Serial != 0;
            }
            case 0x0B: {
                var helmet = Find(equip, 0x06);
                if (helmet.Serial != 0 && (uint)(helmet.Graphic - 0xA42B) < 2) return true;
                goto case 0x06;
            }
            case 0x06: return HeadCovered(robeGfx, body);
            case 0x17: {
                var skirt = Find(equip, 0x17);
                var skirtAnim = skirt.Serial == 0 ? 0 : AnimId(skirt.Graphic);
                if (skirtAnim != 0x1C7 && skirtAnim != 0x1E4) return false;
                var pantsAnim = AnimOf(Find(equip, 0x04));
                return pantsAnim == 0x1EB || pantsAnim == 0x1FA || pantsAnim == 0x200;
            }
        }
        return false;
    }

    static bool ShoesCovered(List<UOEquipState> equip, int robeAnim) {
        var pants = Find(equip, 0x04);
        var pantsAnim = AnimOf(pants);
        if (robeAnim != 0x504 && pantsAnim != 0x513 && pantsAnim != 0x514) {
            var pantsGfx = pants.Serial == 0 ? 0 : pants.Graphic;
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
        var pantsAnim = AnimOf(Find(equip, 0x04));
        if (pantsAnim != 0x1EB && pantsAnim != 0x1FA && pantsAnim != 0x200) return false;
        var skirt = Find(equip, 0x17);
        if (skirt.Serial != 0) {
            var skirtAnim = AnimId(skirt.Graphic);
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

    static int AnimOf(UOEquipState item) => item.Serial == 0 ? 0 : AnimId(item.Graphic);

    static UOEquipState Find(List<UOEquipState> equip, byte layer) {
        for (var i = 0; i < equip.Count; i++)
            if (equip[i].Layer == layer) return equip[i];
        return default;
    }

    static int AnimId(ushort graphic) {
        var items = Tiles();
        if (items == null || graphic >= items.Length) return 0;
        return (ushort)items[graphic].AnimID;
    }

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

    static int Conv(int body, int anim, int graphic) {
        var map = ConvMap(body);
        if (map != null && map.TryGetValue((ushort)anim, out var neu)) return neu;
        return graphic;
    }

    static Dictionary<ushort, ushort> ConvMap(int body) {
        LoadConv();
        if (_conv != null && _conv.TryGetValue(body, out var map)) return map;
        return null;
    }

    // Equipconv.def is not a static table. Columns are body, anim, new anim, gump, color.
    static void LoadConv() {
        if (_conv != null || _convMiss) return;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return;
        _conv = new Dictionary<int, Dictionary<ushort, ushort>>();
        _convMiss = true;
        var path = Path.Combine(root, "Equipconv.def");
        if (!File.Exists(path)) return;
        try {
            foreach (var raw in File.ReadAllLines(path)) {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#' || !char.IsDigit(line[0])) continue;
                var hash = line.IndexOf('#');
                if (hash >= 0) line = line.Substring(0, hash).Trim();
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5) continue;
                var who = Tok(parts[0]);
                var graphic = Tok(parts[1]);
                var neu = Tok(parts[2]);
                var gump = Tok(parts[3]);
                if (who < 0 || graphic < 0 || graphic > ushort.MaxValue || neu < 0 || neu > ushort.MaxValue) continue;
                if (gump > ushort.MaxValue) continue;
                if (!_conv.TryGetValue(who, out var map)) {
                    map = new Dictionary<ushort, ushort>();
                    _conv[who] = map;
                }
                map[(ushort)graphic] = (ushort)neu;
            }
        }
        catch { _conv.Clear(); }
    }

    static int Tok(string token) {
        if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.TryParse(token.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var h) ? h : -1;
        return int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : -1;
    }
}
