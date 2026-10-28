// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// Screen pick for the world anchor at 320, 220. The foot is the same diamond WorldGump uses.
/// A hit is within 36 pixels of a point 30 pixels above that foot, so the body is the target.
/// Mobiles replace an item at the same distance.
/// </summary>
public static class WorldPick {
    public static bool TryActor(World world, int sx, int sy, out uint serial) {
        serial = 0;
        if (world == null) return false;
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        var best = 36 * 36;
        var found = 0u;
        void Consider(int x, int y, int z, uint ser) {
            var dx = x - px;
            var dy = y - py;
            var ax = 320 + (dx - dy) * 22;
            var ay = 220 + (dx + dy) * 22 - (z - pz) * 4 - 30;
            var ddx = sx - ax;
            var ddy = sy - ay;
            var d = ddx * ddx + ddy * ddy;
            if (d > best) return;
            best = d;
            found = ser;
        }
        foreach (var entry in world.Items)
            if (entry.Value is Item it) Consider(it.X, it.Y, it.Z, it.Serial);
        Consider(px, py, pz, UOLoginNet.PlayerSerial);
        foreach (var entry in world.Mobiles)
            if (entry.Value is Mobile m) Consider(m.X, m.Y, m.Z, m.Serial);
        serial = found;
        return found != 0;
    }
}
