// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameX.Origin.UO;

/// <summary>ClassicUO A* from the player tile. Each step's Z is UOLoginNet.StepHeight.</summary>
public sealed class Pathfinder {
    const int MaxNodes = 10000;
    const int Body = 16;
    const int RunDistance = 14;
    static readonly int[] DX = { 0, 1, 1, 1, 0, -1, -1, -1 };
    static readonly int[] DY = { -1, -1, 0, 1, 1, 1, 0, -1 };
    static readonly FieldInfo TileField = typeof(Assets).GetField("_tile", BindingFlags.Instance | BindingFlags.NonPublic);

    readonly World _world;
    readonly List<Node> _nodes = new List<Node>();
    readonly List<int> _heap = new List<int>();
    readonly List<int> _chain = new List<int>();
    readonly Dictionary<(int x, int y, int z), int> _best = new Dictionary<(int, int, int), int>();
    readonly HashSet<(int x, int y, int z)> _closed = new HashSet<(int, int, int)>();
    readonly List<Step> _steps = new List<Step>();
    readonly List<(ushort graphic, sbyte z)> _statics = new List<(ushort, sbyte)>(8);
    Binary_TileData.ItemData[] _items;
    Binary_TileData.LandData[] _land;
    bool _flagsMiss;
    int _fromX, _fromY, _at, _armX, _armY, _armFacing;
    bool _run, _armed;

    public Pathfinder(World world) => _world = world;

    public bool Active { get; private set; }

    /// <summary>Drops the path.</summary>
    public void Stop() {
        _steps.Clear();
        _at = 0;
        _run = false;
        _armed = false;
        Active = false;
    }

    /// <summary>
    /// Path from the player tile to x,y. A blocked goal stops on a neighbor.
    /// False when the player is already close enough, or no route exists.
    /// </summary>
    public bool Build(int x, int y, int z) {
        Stop();
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        var distance = CanStand(x, y, z, out _) ? 0 : 1;
        if (Cheb(px, py, x, y) <= distance) return false;
        _run = Cheb(px, py, x, y) > RunDistance;
        if (!Search(px, py, pz, x, y, distance)) return false;
        _fromX = px;
        _fromY = py;
        _at = 0;
        Active = _steps.Count > 0;
        return Active;
    }

    /// <summary>
    /// Next direction, with the run bit when the original goal is more than 14 tiles away.
    /// False while the last send has not turned or stepped, so the walk queue cannot pass a corner.
    /// </summary>
    public bool Issue(out byte direction) {
        direction = 0;
        if (!Active) return false;
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var facing = UOLoginNet.PlayerDirection & 7;
        while (_at < _steps.Count && _steps[_at].X == px && _steps[_at].Y == py) {
            _fromX = px;
            _fromY = py;
            _at++;
            _armed = false;
        }
        if (_at >= _steps.Count) {
            Stop();
            return false;
        }
        if (px != _fromX || py != _fromY) {
            Stop();
            return false;
        }
        if (_armed && _armX == px && _armY == py && _armFacing == facing) return false;
        direction = _steps[_at].Dir;
        if (_run) direction |= 0x80;
        _armed = true;
        _armX = px;
        _armY = py;
        _armFacing = facing;
        return true;
    }

    bool Search(int px, int py, int pz, int gx, int gy, int distance) {
        _nodes.Clear();
        _heap.Clear();
        _best.Clear();
        _closed.Clear();
        _steps.Clear();
        _nodes.Add(new Node { X = px, Y = py, Z = pz, F = GoalCost(px, py, gx, gy, distance), Parent = -1 });
        _best[(px, py, pz)] = 0;
        HeapPush(0);
        while (_heap.Count > 0) {
            var i = HeapPop();
            var node = _nodes[i];
            var key = (node.X, node.Y, node.Z);
            if (!_best.TryGetValue(key, out var known) || known != node.G) continue;
            if (!_closed.Add(key)) continue;
            if (i > 0 && Cheb(node.X, node.Y, gx, gy) <= distance) {
                Reconstruct(i);
                return _steps.Count > 0;
            }
            for (var dir = 0; dir < 8; dir++) {
                if (!CanStep(node.X, node.Y, node.Z, dir, out var nx, out var ny, out var nz)) continue;
                var nkey = (nx, ny, nz);
                if (_closed.Contains(nkey)) continue;
                var ng = node.G + ((dir & 1) == 0 ? 1 : 2);
                if (_best.TryGetValue(nkey, out var old) && old <= ng) continue;
                if (_nodes.Count >= MaxNodes) return false;
                _best[nkey] = ng;
                var ni = _nodes.Count;
                _nodes.Add(new Node {
                    X = nx,
                    Y = ny,
                    Z = nz,
                    G = ng,
                    F = ng + GoalCost(nx, ny, gx, gy, distance),
                    Parent = i,
                    Dir = (byte)dir,
                });
                HeapPush(ni);
            }
        }
        return false;
    }

    void Reconstruct(int goal) {
        _chain.Clear();
        for (var cursor = goal; cursor > 0 && _chain.Count < MaxNodes; cursor = _nodes[cursor].Parent)
            _chain.Add(cursor);
        _steps.Clear();
        for (var i = _chain.Count - 1; i >= 0; i--) {
            var n = _nodes[_chain[i]];
            _steps.Add(new Step { X = n.X, Y = n.Y, Dir = n.Dir });
        }
    }

    bool CanStep(int x, int y, int z, int dir, out int nx, out int ny, out int nz) {
        nx = x + DX[dir];
        ny = y + DY[dir];
        if (!CanStand(nx, ny, z, out nz)) return false;
        if ((dir & 1) == 0) return true;
        var a = (dir + 7) & 7;
        var b = (dir + 1) & 7;
        return CanStand(x + DX[a], y + DY[a], z, out _) && CanStand(x + DX[b], y + DY[b], z, out _);
    }

    bool CanStand(int x, int y, int fromZ, out int z) {
        z = fromZ;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !assets.TryLandTile(x, y, out var tile, out _)) return false;
        var step = UOLoginNet.StepHeight;
        if (step != null) z = step(x, y, fromZ);
        LoadFlags(assets);
        var floor = false;
        _statics.Clear();
        assets.CopyStatics(x, y, _statics);
        for (var i = 0; i < _statics.Count; i++) {
            var (graphic, sz) = _statics[i];
            if (!TryItem(graphic, out var data)) continue;
            if (OnFloor(data, sz, z)) floor = true;
            if (Blocks(data, sz, z)) return false;
        }
        if (_world != null) {
            foreach (var entry in _world.Items) {
                if (entry.Value is not Item it || it.X != x || it.Y != y) continue;
                if (!TryItem((ushort)(it.Graphic & 0x7FFF), out var data)) continue;
                if (OnFloor(data, it.Z, z)) floor = true;
                if (Blocks(data, it.Z, z)) return false;
            }
            if (LandImpassable(tile) && !floor) return false;
            var self = UOLoginNet.PlayerSerial;
            foreach (var entry in _world.Mobiles) {
                if (entry.Value is not Mobile m || m.Serial == self || m.X != x || m.Y != y) continue;
                if (Hits(m.Z, Body, z)) return false;
            }
        }
        return true;
    }

    static bool OnFloor(Binary_TileData.ItemData data, int objZ, int feet) =>
        (data.IsSurface || data.IsBridge) && objZ + data.Height == feet;

    static bool Blocks(Binary_TileData.ItemData data, int objZ, int feet) {
        if (!data.IsImpassable && !data.IsSurface) return false;
        if (OnFloor(data, objZ, feet)) return false;
        return Hits(objZ, data.Height, feet);
    }

    static bool Hits(int objZ, int height, int feet) {
        if (height <= 0) return objZ >= feet && objZ < feet + Body;
        return objZ < feet + Body && objZ + height > feet;
    }

    // StepZ reads this private field. There is no public flag API.
    void LoadFlags(Assets assets) {
        if (_items != null || assets == null || _flagsMiss) return;
        if (TileField?.GetValue(assets) is not Binary_TileData tile || tile.ItemDatas == null) {
            _flagsMiss = true;
            return;
        }
        _items = tile.ItemDatas;
        _land = tile.LandDatas;
    }

    bool TryItem(ushort graphic, out Binary_TileData.ItemData data) {
        if (_items != null && graphic < _items.Length) {
            data = _items[graphic];
            return true;
        }
        data = default;
        return false;
    }

    bool LandImpassable(ushort tile) => _land != null && tile < _land.Length && _land[tile].IsImpassible;

    void HeapPush(int index) {
        _heap.Add(index);
        var i = _heap.Count - 1;
        while (i > 0) {
            var parent = (i - 1) >> 1;
            if (_nodes[_heap[parent]].F <= _nodes[_heap[i]].F) break;
            var swap = _heap[parent];
            _heap[parent] = _heap[i];
            _heap[i] = swap;
            i = parent;
        }
    }

    int HeapPop() {
        var top = _heap[0];
        var last = _heap[_heap.Count - 1];
        _heap.RemoveAt(_heap.Count - 1);
        if (_heap.Count == 0) return top;
        _heap[0] = last;
        var i = 0;
        while (true) {
            var left = (i << 1) + 1;
            var right = left + 1;
            var small = i;
            if (left < _heap.Count && _nodes[_heap[left]].F < _nodes[_heap[small]].F) small = left;
            if (right < _heap.Count && _nodes[_heap[right]].F < _nodes[_heap[small]].F) small = right;
            if (small == i) break;
            var swap = _heap[small];
            _heap[small] = _heap[i];
            _heap[i] = swap;
            i = small;
        }
        return top;
    }

    static int Cheb(int x, int y, int gx, int gy) {
        var dx = x - gx;
        var dy = y - gy;
        if (dx < 0) dx = -dx;
        if (dy < 0) dy = -dy;
        return dx > dy ? dx : dy;
    }

    static int GoalCost(int x, int y, int gx, int gy, int distance) {
        var left = Cheb(x, y, gx, gy) - distance;
        return left > 0 ? left : 0;
    }

    struct Node {
        public int X, Y, Z, G, F, Parent;
        public byte Dir;
    }

    struct Step {
        public int X, Y;
        public byte Dir;
    }
}
