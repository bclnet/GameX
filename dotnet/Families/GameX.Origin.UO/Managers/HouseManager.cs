// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

public sealed class HouseManager {
    readonly struct Piece {
        public Piece(ushort graphic, int x, int y, sbyte z) {
            Graphic = graphic;
            X = x;
            Y = y;
            Z = z;
        }

        public ushort Graphic { get; }
        public int X { get; }
        public int Y { get; }
        public sbyte Z { get; }
    }

    private sealed class House {
        public House(uint serial) { Serial = serial; }

        public uint Serial { get; }

        public void ClearComponents() {
            _pieces.Remove(Serial);
        }
    }

    static readonly Dictionary<uint, List<Piece>> _pieces = new Dictionary<uint, List<Piece>>();
    readonly Dictionary<uint, House> _houses = new Dictionary<uint, House>();
    readonly World _world;

    public HouseManager(World world) {
        _world = world;
    }

    public IReadOnlyCollection<uint> Houses => _houses.Keys;

    public void Add(uint serial) {
        _houses[serial] = new House(serial);
    }

    public bool TryGetHouse(uint serial, out uint house) {
        if (_houses.TryGetValue(serial, out House found)) {
            house = found.Serial;
            return true;
        }

        house = 0;
        return false;
    }

    public bool TryToRemove(uint serial, int distance) {
        if (!IsHouseInRange(serial, distance)) {
            if (_houses.TryGetValue(serial, out House house)) {
                house.ClearComponents();
                _houses.Remove(serial);
            }

            return true;
        }

        return false;
    }

    public bool IsHouseInRange(uint serial, int distance) {
        if (TryGetHouse(serial, out _)) {
            int currX = UOLoginNet.PlayerX;
            int currY = UOLoginNet.PlayerY;

            //if (World.Player.IsMoving)
            //{
            //    Mobile.Step step = World.Player.Steps.Back();
            //
            //    currX = step.X;
            //    currY = step.Y;
            //}
            //else
            //{
            //    currX = World.Player.X;
            //    currY = World.Player.Y;
            //}

            if (!_world.Items.TryGetValue(serial, out object raw) || raw is not Item found) {
                return true;
            }

            // no MultiDistanceBonus
            return Math.Abs(found.X - currX) <= distance && Math.Abs(found.Y - currY) <= distance;
        }

        return false;
    }

    public bool EntityIntoHouse(uint house, object obj) {
        if (obj != null && TryGetHouse(house, out _)) {
            if (!_world.Items.TryGetValue(house, out object raw) || raw is not Item) {
                return true;
            }

            // no MultiInfo
            return true;
        }

        return false;
    }

    public void Remove(uint serial) {
        if (TryGetHouse(serial, out _)) {
            if (_houses.TryGetValue(serial, out House house)) {
                house.ClearComponents();
                _houses.Remove(serial);
            }
        }
    }

    public void RemoveMultiTargetHouse() {
        if (_houses.TryGetValue(0, out House house)) {
            house.ClearComponents();
            _houses.Remove(0);
        }
    }

    public bool Exists(uint serial) {
        return _houses.ContainsKey(serial);
    }

    public void Clear() {
        foreach (KeyValuePair<uint, House> house in _houses) {
            house.Value.ClearComponents();
        }

        _houses.Clear();
    }

    /// <summary>Places one 0xD8 house. A missing foundation leaves the previous pieces. Mode 2 waits on the multi rectangle.</summary>
    public static void Load(UOCustomHouse house) {
        var mobiles = new List<UOMobileState>();
        var items = new List<UOItemState>();
        UOLoginNet.CopyActors(mobiles, items);
        UOItemState? foundation = null;
        for (var i = 0; i < items.Count; i++) {
            if (items[i].Serial != house.Serial) continue;
            foundation = items[i];
            break;
        }
        if (foundation == null) return;
        var fx = foundation.Value.X;
        var fy = foundation.Value.Y;
        var fz = foundation.Value.Z;
        var placed = new List<Piece>();
        var planes = house.Planes ?? Array.Empty<UOHousePlane>();
        var sawRect = false;
        var haveRect = false;
        short minX = 0, minY = 0, maxY = 0;
        for (var p = 0; p < planes.Length; p++) {
            var plane = planes[p];
            var rows = plane.Components;
            if (rows == null) continue;
            for (var c = 0; c < rows.Length; c++) {
                var tile = rows[c];
                if (tile.Id == 0) continue;
                int ox = tile.X;
                int oy = tile.Y;
                if (plane.Mode == 2) {
                    if (!sawRect) {
                        sawRect = true;
                        haveRect = TryMultiRect(foundation.Value.Graphic, out minX, out minY, out maxY);
                    }
                    if (!haveRect || !TryMode2(plane.PlaneZ, tile.Index, minX, minY, maxY, out ox, out oy)) continue;
                }
                placed.Add(new Piece(tile.Id, fx + ox, fy + oy, (sbyte)(fz + tile.Z)));
            }
        }
        _pieces[house.Serial] = placed;
    }

    /// <summary>Appends tiles on this cell. Graphic is the multi id. Does not clear dest.</summary>
    public static void CopyPieces(int wx, int wy, List<(ushort graphic, sbyte z)> dest) {
        foreach (var kv in _pieces) {
            var list = kv.Value;
            for (var i = 0; i < list.Count; i++) {
                var piece = list[i];
                if (piece.X != wx || piece.Y != wy) continue;
                dest.Add((piece.Graphic, piece.Z));
            }
        }
    }

    static bool TryMode2(int planeZ, int index, int minX, int minY, int maxY, out int x, out int y) {
        int offX, offY, multiHeight;
        if (planeZ <= 0) {
            offX = minX;
            offY = minY;
            multiHeight = maxY - minY + 2;
        }
        else if (planeZ <= 4) {
            offX = minX + 1;
            offY = minY + 1;
            multiHeight = maxY - minY;
        }
        else {
            offX = minX;
            offY = minY;
            multiHeight = maxY - minY + 1;
        }
        if (multiHeight == 0) {
            x = 0;
            y = 0;
            return false;
        }
        x = (sbyte)(index / multiHeight + offX);
        y = (sbyte)(index % multiHeight + offY);
        return true;
    }

    // ClassicUO MultiInfo starts at 0. A positive-only multi therefore keeps minX and minY at 0.
    static bool TryMultiRect(ushort graphic, out short minX, out short minY, out short maxY) {
        minX = 0;
        minY = 0;
        maxY = 0;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return false;
        var idxPath = Path.Combine(root, "multi.idx");
        var mulPath = Path.Combine(root, "multi.mul");
        if (!File.Exists(idxPath) || !File.Exists(mulPath)) return false;
        var index = graphic >= 0x4000 ? graphic & 0x3FFF : graphic;
        try {
            using var idx = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var at = (long)index * 12;
            if (at < 0 || at + 12 > idx.Length) return false;
            var hdr = new byte[12];
            idx.Position = at;
            if (idx.Read(hdr, 0, 12) < 12) return false;
            var offset = BitConverter.ToInt32(hdr, 0);
            var length = BitConverter.ToInt32(hdr, 4);
            if (offset < 0 || length <= 0) return false;
            using var mul = new FileStream(mulPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if ((long)offset + length > mul.Length) return false;
            var buf = new byte[length];
            mul.Position = offset;
            if (mul.Read(buf, 0, length) < length) return false;
            var ver = UltimaOnline.Current?.Version ?? default;
            var size = ver >= ClientVersion.CV_7090 ? 16 : 12;
            if (length < size) return false;
            var count = length / size;
            for (var i = 0; i < count; i++) {
                var o = i * size;
                var x = BitConverter.ToInt16(buf, o + 2);
                var y = BitConverter.ToInt16(buf, o + 4);
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
            return count > 0;
        }
        catch { return false; }
    }
}
