// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Overhead view after 0x1B. The player is a body frame plus the center label. Arrows are handled by LoginScene.
/// Felucca land and static art are drawn through GumpArt.Sink when the map is open.
/// </summary>
public class WorldGump : Gump {
    const int Slots = 48;
    const int Radius = 10;
    readonly Label _coords;
    readonly Label[] _rows;
    readonly List<Stamp> _stamps = [];
    readonly List<(ushort graphic, sbyte z)> _statics = [];
    int _rev = -1;

    struct Stamp {
        public int Key, X, Y, W, H, Ord;
        public byte[] Px;
    }

    public WorldGump(World world, LoginScene scene) : base(world, 0, 0) {
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        AcceptKeyboardInput = false;
        _coords = new Label("", false, 0x0386, 2) { X = 16, Y = 16 };
        Add(_coords);
        _rows = new Label[Slots];
        for (var i = 0; i < Slots; i++) {
            _rows[i] = new Label("", false, 0x0481, 5) { X = 0, Y = 0, IsVisible = false };
            Add(_rows[i]);
        }
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Paint();
        }
        base.Update();
    }

    public override bool Draw(int x, int y) {
        PaintGround(x, y);
        return base.Draw(x, y);
    }

    void PaintGround(int ox, int oy) {
        var sink = GumpArt.Sink;
        var assets = UltimaOnline.Current?.Assets;
        if (sink == null || assets == null) return;
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        _stamps.Clear();
        for (var dy = -Radius; dy <= Radius; dy++)
            for (var dx = -Radius; dx <= Radius; dx++) {
                var wx = px + dx;
                var wy = py + dy;
                if (!assets.TryLandTile(wx, wy, out var tile, out var tz)) continue;
                if (!assets.TryArtPixels(tile, out var pix, out var w, out var h)) continue;
                var sx = 320 + (dx - dy) * 22;
                var sy = 220 + (dx + dy) * 22 - (tz - pz) * 4;
                _stamps.Add(new Stamp { Key = 0x10000 | tile, X = ox + sx - 22, Y = oy + sy - 22, W = w, H = h, Px = pix, Ord = (dx + dy + 32) * 1000 + tz });
                _statics.Clear();
                assets.CopyStatics(wx, wy, _statics);
                foreach (var (graphic, z) in _statics) {
                    var id = graphic + 0x4000;
                    if (id > 0xFFFF || !assets.TryArtPixels(id, out var ap, out var aw, out var ah)) continue;
                    var ssy = 220 + (dx + dy) * 22 - (z - pz) * 4;
                    _stamps.Add(new Stamp { Key = 0x20000 | id, X = ox + sx - aw / 2, Y = oy + ssy - ah + 22, W = aw, H = ah, Px = ap, Ord = (dx + dy + 32) * 1000 + z + 1 });
                }
            }
        AddBody(assets, ox, oy, UOLoginNet.PlayerBody, UOLoginNet.PlayerDirection, 0, 0, pz);
        foreach (var entry in World.Mobiles) {
            if (entry.Value is not Mobile m || m.Serial == UOLoginNet.PlayerSerial) continue;
            var dx = m.X - px;
            var dy = m.Y - py;
            if (dx > Radius || dx < -Radius || dy > Radius || dy < -Radius) continue;
            AddBody(assets, ox, oy, m.Body, m.Direction, dx, dy, m.Z);
        }
        foreach (var entry in World.Items) {
            if (entry.Value is not Item it) continue;
            var dx = it.X - px;
            var dy = it.Y - py;
            if (dx > Radius || dx < -Radius || dy > Radius || dy < -Radius) continue;
            var id = (it.Graphic & 0x7FFF) + 0x4000;
            if (id > 0xFFFF || !assets.TryArtPixels(id, out var ap, out var aw, out var ah)) continue;
            var sx = 320 + (dx - dy) * 22;
            var sy = 220 + (dx + dy) * 22 - (it.Z - pz) * 4;
            _stamps.Add(new Stamp { Key = 0x20000 | id, X = ox + sx - aw / 2, Y = oy + sy - ah + 22, W = aw, H = ah, Px = ap, Ord = (dx + dy + 32) * 1000 + it.Z + 1 });
        }
        _stamps.Sort((a, b) => a.Ord - b.Ord);
        foreach (var s in _stamps)
            sink.DrawRaw(s.Key, s.X, s.Y, s.W, s.H, s.Px, s.W, s.H);
    }

    void AddBody(Assets assets, int ox, int oy, int body, int direction, int dx, int dy, int z) {
        if (body <= 0 || assets == null) return;
        var dir = direction & 7;
        var action = body >= 400 ? 4 : 0;
        if (!assets.TryBodyFrame(body, action, dir, out var pix, out var w, out var h, out var cx, out var cy)) {
            if (action == 0 || !assets.TryBodyFrame(body, 0, dir, out pix, out w, out h, out cx, out cy)) return;
            action = 0;
        }
        var sx = 320 + (dx - dy) * 22;
        var sy = 220 + (dx + dy) * 22 - (z - UOLoginNet.PlayerZ) * 4;
        var key = unchecked((int)(0x40000000 + body * 32 + dir * 4 + (action & 3)));
        _stamps.Add(new Stamp { Key = key, X = ox + sx - cx, Y = oy + sy - cy - h, W = w, H = h, Px = pix, Ord = (dx + dy + 32) * 1000 + z + 2 });
    }

    void Paint() {
        var mobiles = new List<UOMobileState>();
        var items = new List<UOItemState>();
        UOLoginNet.CopyActors(mobiles, items);
        World.Mobiles.Clear();
        World.Items.Clear();
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        var dir = UOLoginNet.PlayerDirection & 7;
        _coords.Text = $"X {px}  Y {py}  Z {pz}  body {UOLoginNet.PlayerBody}  {DirName((byte)dir)}   arrows move, shift runs";
        var n = 0;
        Place(ref n, "You", 0x0035, 0, 0, 0);
        foreach (var m in mobiles) {
            World.Mobiles[m.Serial] = new Mobile(m.Serial, m.Body, m.X, m.Y, m.Z, m.Direction);
            if (m.Serial == UOLoginNet.PlayerSerial) continue;
            Place(ref n, $"M {m.Body:X4}", 0x0481, m.X - px, m.Y - py, m.Z - pz);
        }
        foreach (var it in items) {
            World.Items[it.Serial] = new Item(it.Serial, it.Graphic, it.X, it.Y, it.Z);
            Place(ref n, $"I {it.Graphic:X4}", 0x034F, it.X - px, it.Y - py, it.Z - pz);
        }
        for (; n < Slots; n++) _rows[n].IsVisible = false;
    }

    void Place(ref int n, string text, ushort hue, int dx, int dy, int dz) {
        if (n >= Slots) return;
        if (dx > 12 || dx < -12 || dy > 12 || dy < -12) return;
        var row = _rows[n++];
        row.Hue = hue;
        row.Text = text;
        row.X = 320 + (dx - dy) * 22 - 20;
        row.Y = 220 + (dx + dy) * 22 - dz * 4;
        row.IsVisible = true;
    }

    static string DirName(byte d) => d switch {
        0 => "N",
        1 => "NE",
        2 => "E",
        3 => "SE",
        4 => "S",
        5 => "SW",
        6 => "W",
        _ => "NW",
    };
}
