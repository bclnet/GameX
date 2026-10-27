// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO's software cursor. The OS cursor stays hidden.
/// Art is the graphic plus 0x4000. Green on the first row or column is the hotspot:
/// BGRA1555 mask 0x03E0, the same split UoGumpArt.Upload uses for a 16-bit BMP.
/// A 1px border is cropped. Draw sits at the mouse minus that hotspot.
/// A held item is drawn after that pointer and is not cropped. It is centered on the mouse.
/// Its sink key starts at 0x54000000 so it is not the pointer (0x40000000) or a paperdoll (0x51000000).
/// </summary>
public static class GameCursor {
    // Peace 0x206A..0x2079, war 0x2053..0x2062. Index 9 is the pointer.
    static readonly ushort[] Peace = [
        0x206A, 0x206B, 0x206C, 0x206D, 0x206E, 0x206F, 0x2070, 0x2071,
        0x2072, 0x2073, 0x2074, 0x2075, 0x2076, 0x2077, 0x2078, 0x2079,
    ];
    static readonly ushort[] War = [
        0x2053, 0x2054, 0x2055, 0x2056, 0x2057, 0x2058, 0x2059, 0x205A,
        0x205B, 0x205C, 0x205D, 0x205E, 0x205F, 0x2060, 0x2061, 0x2062,
    ];

    // Player anchor in WorldGump. Direction points away from it.
    const int AnchorX = 320;
    const int AnchorY = 220;
    const int RawKey = 0x40000000;
    const int HoldKey = 0x54000000;

    static readonly Dictionary<int, Art> Cache = [];
    static readonly List<UOContainedState> HoldRows = [];
    static readonly List<UOEquipState> HoldWorn = [];
    static readonly List<uint> HoldBags = [];
    static readonly HashSet<uint> HoldSeen = [];
    static uint HeldSeen;
    static int HeldRev = -1;
    static bool HeldOk;
    static ushort HeldGraphic, HeldHue, HeldAmount;

    readonly struct Art {
        public readonly byte[] Px;
        public readonly int W, H, X, Y;
        public Art(byte[] px, int w, int h, int x, int y) { Px = px; W = w; H = h; X = x; Y = y; }
    }

    public static void Draw(bool inWorld) {
        var sink = GumpArt.Sink;
        if (sink == null) return;
        var graphic = Pick(inWorld);
        var hue = (ushort)(inWorld && UOLoginNet.MapIndex != 0 && !UOLoginNet.InWarMode ? 0x0033 : 0);
        if (TryArt(graphic, hue, out var used, out var px, out var w, out var h, out var hotX, out var hotY))
            sink.DrawRaw(RawKey | (used << 16) | graphic, UIManager.MouseX - hotX, UIManager.MouseY - hotY, w, h, px, w, h);
        if (UOLoginNet.Holding) DrawHeld(sink);
    }

    /// <summary>
    /// ClassicUO ItemHold: art at graphic + 0x4000, centered with width/2 and height/2.
    /// No grab offset is stored, so that mouse offset is 0. A stack is the same art at +5, +5.
    /// Coins with a count use the pile graphic instead, and then the extra stamp is skipped.
    /// </summary>
    static void DrawHeld(IGumpArt sink) {
        if (!TryHeld(UOLoginNet.HeldSerial, out var graphic, out var hue, out var amount)) return;
        var shown = Shown(graphic, amount);
        if (!TryHeldArt(shown, hue, out var px, out var w, out var h, out var hotX, out var hotY, out var key)) return;
        var x = UIManager.MouseX - hotX;
        var y = UIManager.MouseY - hotY;
        sink.DrawRaw(key, x, y, w, h, px, w, h);
        if (amount > 1 && shown == graphic)
            sink.DrawRaw(key, x + 5, y + 5, w, h, px, w, h);
    }

    static ushort Shown(ushort graphic, ushort amount) {
        if (amount <= 1 || (graphic != 0x0EEA && graphic != 0x0EED && graphic != 0x0EF0)) return graphic;
        return (ushort)(graphic + (amount > 5 ? 2 : 1));
    }

    static bool TryHeld(uint serial, out ushort graphic, out ushort hue, out ushort amount) {
        var rev = UOLoginNet.WorldRevision;
        if (serial == HeldSeen && rev == HeldRev) {
            graphic = HeldGraphic;
            hue = HeldHue;
            amount = HeldAmount;
            return HeldOk;
        }
        HeldSeen = serial;
        HeldRev = rev;
        HeldOk = Lookup(serial, out HeldGraphic, out HeldHue, out HeldAmount);
        graphic = HeldGraphic;
        hue = HeldHue;
        amount = HeldAmount;
        return HeldOk;
    }

    static bool Lookup(uint serial, out ushort graphic, out ushort hue, out ushort amount) {
        graphic = hue = amount = 0;
        if (serial == 0) return false;
        var world = WorldOf();
        if (world != null && world.Items.TryGetValue(serial, out var obj) && obj is Item item) {
            graphic = (ushort)(item.Graphic & 0x7FFF);
            return true;
        }
        if (InContents(serial, world, out graphic, out hue, out amount)) return true;
        if (InEquip(serial, world, out graphic, out hue)) return true;
        return false;
    }

    static World WorldOf() {
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (!n.Value.IsDisposed) return n.Value.World;
        return null;
    }

    static bool InContents(uint serial, World world, out ushort graphic, out ushort hue, out ushort amount) {
        graphic = hue = amount = 0;
        HoldBags.Clear();
        HoldSeen.Clear();
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (!n.Value.IsDisposed && n.Value is ContainerGump bag) Seed(bag.ServerSerial);
        Seed(UOLoginNet.PlayerBackpack);
        SeedWorn(UOLoginNet.PlayerSerial);
        if (world != null)
            foreach (var kv in world.Mobiles) SeedWorn(kv.Key);
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (!n.Value.IsDisposed && n.Value is PaperdollGump doll) SeedWorn(doll.ServerSerial);
        if (world != null)
            foreach (var kv in world.Items) Seed(kv.Key);
        for (var i = 0; i < HoldBags.Count; i++) {
            UOLoginNet.CopyContents(HoldBags[i], HoldRows);
            foreach (var it in HoldRows) {
                if (it.Serial == serial) {
                    graphic = (ushort)(it.Graphic & 0x7FFF);
                    hue = it.Hue;
                    amount = it.Amount;
                    return true;
                }
                Seed(it.Serial);
            }
        }
        return false;
    }

    static bool InEquip(uint serial, World world, out ushort graphic, out ushort hue) {
        graphic = hue = 0;
        if (MatchEquip(UOLoginNet.PlayerSerial, serial, out graphic, out hue)) return true;
        if (world != null)
            foreach (var kv in world.Mobiles)
                if (kv.Key != UOLoginNet.PlayerSerial && MatchEquip(kv.Key, serial, out graphic, out hue)) return true;
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (!n.Value.IsDisposed && n.Value is PaperdollGump doll && MatchEquip(doll.ServerSerial, serial, out graphic, out hue))
                return true;
        return false;
    }

    static bool MatchEquip(uint parent, uint serial, out ushort graphic, out ushort hue) {
        graphic = hue = 0;
        if (parent == 0) return false;
        UOLoginNet.CopyEquip(parent, HoldWorn);
        foreach (var e in HoldWorn)
            if (e.Serial == serial) {
                graphic = (ushort)(e.Graphic & 0x7FFF);
                hue = e.Hue;
                return true;
            }
        return false;
    }

    static void SeedWorn(uint parent) {
        if (parent == 0) return;
        UOLoginNet.CopyEquip(parent, HoldWorn);
        for (var i = 0; i < HoldWorn.Count; i++) Seed(HoldWorn[i].Serial);
    }

    static void Seed(uint id) {
        if (id == 0 || !HoldSeen.Add(id)) return;
        HoldBags.Add(id);
    }

    static bool TryHeldArt(ushort graphic, ushort hue, out byte[] pixels, out int width, out int height, out int hotX, out int hotY, out int key) {
        pixels = null;
        width = height = hotX = hotY = key = 0;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null) return false;
        ushort used = 0;
        if ((hue & 0x3FFF) != 0 && assets.TryHueRamp(hue & 0x3FFF, out _)) used = hue;
        key = HeldKey(graphic, used);
        if (Cache.TryGetValue(key, out var hit)) {
            pixels = hit.Px;
            width = hit.W;
            height = hit.H;
            hotX = hit.X;
            hotY = hit.Y;
            return pixels != null;
        }
        if (!assets.TryArtPixels(graphic + 0x4000, out var src, out var sw, out var sh) || sw <= 0 || sh <= 0) return false;
        if (src == null || src.Length < sw * sh * 2) return false;
        if (used != 0 && assets.TryHueRamp(used & 0x3FFF, out var ramp))
            pixels = Hue(src, sw, sh, ramp, (used & 0x8000) != 0);
        else pixels = src;
        width = sw;
        height = sh;
        hotX = width >> 1;
        hotY = height >> 1;
        Cache[key] = new Art(pixels, width, height, hotX, hotY);
        return true;
    }

    static int HeldKey(ushort graphic, ushort used) {
        var key = HoldKey | graphic;
        var index = used & 0x3FFF;
        if (index != 0) key += index << 16;
        if ((used & 0x8000) != 0) key |= unchecked((int)0x80000000);
        return key;
    }

    static ushort Pick(bool inWorld) {
        var war = inWorld && UOLoginNet.InWarMode;
        if (UOLoginNet.HasTarget) return At(war, 12);
        if (UOLoginNet.Holding) return At(war, 8);
        var over = UIManager.MouseOverControl;
        if (over is TextBox && over.AcceptKeyboardInput) return At(war, 14);
        if (!inWorld || over != null) return At(war, 9);
        return At(war, GetMouseDirection(AnchorX, AnchorY, UIManager.MouseX, UIManager.MouseY, 1));
    }

    static ushort At(bool war, int index) => (war ? War : Peace)[index];

    static bool TryArt(ushort graphic, ushort hue, out ushort used, out byte[] pixels, out int width, out int height, out int hotX, out int hotY) {
        used = 0;
        pixels = null;
        width = height = hotX = hotY = 0;
        var assets = UltimaOnline.Current?.Assets;
        if (hue != 0 && assets != null && assets.TryHueRamp(hue & 0x3FFF, out _)) used = hue;
        var key = (used << 16) | graphic;
        if (Cache.TryGetValue(key, out var hit)) {
            pixels = hit.Px;
            width = hit.W;
            height = hit.H;
            hotX = hit.X;
            hotY = hit.Y;
            return pixels != null;
        }
        if (assets == null || !assets.TryArtPixels(graphic + 0x4000, out var src, out var sw, out var sh) || sw <= 0 || sh <= 0) return false;
        var count = sw * sh;
        if (src == null || src.Length < count * 2) return false;
        var full = new byte[count * 2];
        Buffer.BlockCopy(src, 0, full, 0, count * 2);
        hotX = hotY = 0;
        for (var y = 0; y < sh; y++)
            for (var x = 0; x < sw; x++) {
                var i = (y * sw + x) * 2;
                var c = (ushort)(full[i] | (full[i + 1] << 8));
                if (c == 0 || c == 0x8000) continue;
                var edge = x == 0 || y == 0 || x >= sw - 1 || y >= sh - 1;
                if (!edge) continue;
                if ((x == 0 || y == 0) && x < sw - 1 && y < sh - 1 && (c & 0x7FFF) == 0x03E0) {
                    if (x == 0) hotY = y;
                    if (y == 0) hotX = x;
                }
                full[i] = 0;
                full[i + 1] = 0;
            }
        if (sw >= 3 && sh >= 3) {
            width = sw - 2;
            height = sh - 2;
            pixels = new byte[width * height * 2];
            for (var y = 0; y < height; y++)
                Buffer.BlockCopy(full, ((y + 1) * sw + 1) * 2, pixels, y * width * 2, width * 2);
        }
        else {
            pixels = full;
            width = sw;
            height = sh;
        }
        if (used != 0 && assets.TryHueRamp(used & 0x3FFF, out var ramp))
            pixels = Hue(pixels, width, height, ramp);
        Cache[key] = new Art(pixels, width, height, hotX, hotY);
        return true;
    }

    /// <summary>Full hues.mul remap. 0x0033 has no partial bit, matching the software cursor hue. Partial recolors gray pixels only.</summary>
    static byte[] Hue(byte[] src, int width, int height, ushort[] ramp, bool partial = false) {
        var count = width * height;
        var dest = new byte[count * 2];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(src[i * 2] | (src[i * 2 + 1] << 8));
            if (c != 0) {
                var r = (c >> 10) & 31;
                var g = (c >> 5) & 31;
                var b = c & 31;
                if (!partial || (r == g && g == b)) {
                    c = ramp[Math.Max(r, Math.Max(g, b))];
                    if (c != 0) c |= 0x8000;
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }

    public static int GetMouseDirection(int x1, int y1, int toX, int toY, int currentFacing) {
        var shiftX = toX - x1;
        var shiftY = toY - y1;
        var hashf = 100 * (Sgn(shiftX) + 2) + 10 * (Sgn(shiftY) + 2);
        if (shiftX != 0 && shiftY != 0) {
            shiftX = Math.Abs(shiftX);
            shiftY = Math.Abs(shiftY);
            if (shiftY * 5 <= shiftX * 2) hashf += 1;
            else if (shiftY * 2 >= shiftX * 5) hashf += 3;
            else hashf += 2;
        }
        else if (shiftX == 0 && shiftY == 0) return currentFacing;
        return hashf switch {
            111 => 6, // West
            112 => 7, // Up
            113 => 0, // North
            120 => 6,
            131 => 6,
            132 => 5, // Left
            133 => 4, // South
            210 => 0,
            230 => 4,
            311 => 2, // East
            312 => 1, // Right
            313 => 0,
            320 => 2,
            331 => 2,
            332 => 3, // Down
            333 => 4,
            _ => currentFacing,
        };
    }

    static int Sgn(int val) {
        var a = 0 < val ? 1 : 0;
        var b = val < 0 ? 1 : 0;
        return a - b;
    }
}
