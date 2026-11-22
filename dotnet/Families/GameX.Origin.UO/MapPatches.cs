// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO 0xBF subcommand 0x18. ExtendedCommand is entered after the packet id and length,
/// then reads the subcommand, so Apply skips those five bytes. Statics count is read before land count.
/// A patched block replaces that block. TryLand and CopyStatics return false when it was not,
/// and this does not change UOLoginNet.MapIndex.
/// </summary>
public static class MapPatches {
    const int MapsCount = 6;
    const int PacketHead = 5;
    const int LandBlock = 196;
    const int LandCells = 192;
    const int StaticSize = 7;
    const int StaidxSize = 12;

    // ClassicUO MapBlocksSize: MapsDefaultSize >> 3.
    static readonly int[] BlockWidth = [896, 896, 288, 320, 181, 160];
    static readonly int[] BlockHeight = [512, 512, 200, 256, 181, 512];

    static readonly Dictionary<int, byte[]>[] _land = new Dictionary<int, byte[]>[MapsCount];
    static readonly Dictionary<int, byte[]>[] _statics = new Dictionary<int, byte[]>[MapsCount];
    static readonly object _gate = new();

    static MapPatches() {
        for (var i = 0; i < MapsCount; i++) {
            _land[i] = new Dictionary<int, byte[]>();
            _statics[i] = new Dictionary<int, byte[]>();
        }
    }

    /// <summary>True when at least one block was patched.</summary>
    public static bool Apply(byte[] packet, int offset, int length) {
        if (packet == null || offset < 0 || length < PacketHead || offset > packet.Length - PacketHead) return false;
        var end = offset + length;
        if (end > packet.Length) end = packet.Length;
        var pos = offset + PacketHead;

        var clear = new bool[MapsCount];
        for (var i = 0; i < MapsCount; i++) clear[i] = CanReset(i);

        var patches = ReadI32BE(packet, ref pos, end);
        if (patches < 0) patches = 0;
        if (patches > MapsCount) patches = MapsCount;

        var result = false;
        var landAdd = new List<(int map, int block, byte[] cells)>();
        var staAdd = new List<(int map, int block, byte[] recs)>();

        for (var i = 0; i < patches; i++) {
            if (!MapLoaded(i)) {
                pos += 8;
                continue;
            }

            // Statics first, then land. The other order reads a server's counts swapped.
            var staticCount = ReadI32BE(packet, ref pos, end);
            var mapCount = ReadI32BE(packet, ref pos, end);
            var maxBlock = BlockWidth[i] * BlockHeight[i];

            if (mapCount != 0) {
                var difl = ReadMul($"mapdifl{i}.mul");
                var dif = ReadMul($"mapdif{i}.mul");
                if (difl == null || dif == null) continue;
                mapCount = Math.Min(mapCount, difl.Length >> 2);
                var difPos = 0L;
                for (var j = 0; j < mapCount; j++) {
                    var block = U32(difl, j * 4);
                    if (block < (uint)maxBlock) {
                        var cells = new byte[LandCells];
                        var at = difPos + 4;
                        if (at >= 0 && at + LandCells <= dif.Length) Buffer.BlockCopy(dif, (int)at, cells, 0, LandCells);
                        landAdd.Add((i, (int)block, cells));
                        result = true;
                    }
                    difPos += LandBlock;
                }
            }

            if (staticCount != 0) {
                var difl = ReadMul($"stadifl{i}.mul");
                var difi = ReadMul($"stadifi{i}.mul");
                var dif = ReadMul($"stadif{i}.mul");
                if (difl == null || difi == null || dif == null) continue;
                staticCount = Math.Min(staticCount, difl.Length >> 2);
                for (var j = 0; j < staticCount; j++) {
                    var lAt = (long)j * 4;
                    var iAt = (long)j * StaidxSize;
                    if (lAt >= difl.Length || iAt >= difi.Length) break;
                    if (lAt + 4 > difl.Length || iAt + StaidxSize > difi.Length) break;
                    var block = U32(difl, (int)lAt);
                    var position = U32(difi, (int)iAt);
                    var size = U32(difi, (int)iAt + 4);
                    if (block < (uint)maxBlock) {
                        var recs = StaticBytes(dif, position, size);
                        staAdd.Add((i, (int)block, recs));
                        result = true;
                    }
                }
            }
        }

        lock (_gate) {
            for (var i = 0; i < MapsCount; i++) {
                if (!clear[i]) continue;
                _land[i].Clear();
                _statics[i].Clear();
            }
            for (var n = 0; n < landAdd.Count; n++) _land[landAdd[n].map][landAdd[n].block] = landAdd[n].cells;
            for (var n = 0; n < staAdd.Count; n++) _statics[staAdd[n].map][staAdd[n].block] = staAdd[n].recs;
        }
        return result;
    }

    /// <summary>Patched land for this tile. False leaves the original map block. Tile is the low 14 bits.</summary>
    public static bool TryLand(int wx, int wy, out ushort tile, out sbyte z) {
        tile = 0;
        z = 0;
        if (wx < 0 || wy < 0) return false;
        var map = UOLoginNet.MapIndex;
        if ((uint)map >= MapsCount) return false;
        var cx = wx >> 3;
        var cy = wy >> 3;
        if (cx >= BlockWidth[map] || cy >= BlockHeight[map]) return false;
        var block = cx * BlockHeight[map] + cy;
        lock (_gate) {
            if (!_land[map].TryGetValue(block, out var cells) || cells == null || cells.Length < LandCells) return false;
            var o = ((wx & 7) * 8 + (wy & 7)) * 3;
            tile = (ushort)((cells[o] | (cells[o + 1] << 8)) & 0x3FFF);
            z = (sbyte)cells[o + 2];
            return true;
        }
    }

    /// <summary>
    /// Appends patched statics on this tile. False when the block is not patched, and dest is left as it was.
    /// Skips graphic 0x0C45 through 0x0DAF, the same range CopyStatics skips.
    /// </summary>
    public static bool CopyStatics(int wx, int wy, List<(ushort graphic, sbyte z)> dest) {
        if (wx < 0 || wy < 0) return false;
        var map = UOLoginNet.MapIndex;
        if ((uint)map >= MapsCount) return false;
        var cx = wx >> 3;
        var cy = wy >> 3;
        if (cx >= BlockWidth[map] || cy >= BlockHeight[map]) return false;
        var block = cx * BlockHeight[map] + cy;
        lock (_gate) {
            if (!_statics[map].TryGetValue(block, out var recs)) return false;
            if (dest != null && recs != null) {
                var lx = wx & 7;
                var ly = wy & 7;
                for (var i = 0; i + StaticSize <= recs.Length; i += StaticSize) {
                    if (recs[i + 2] != lx || recs[i + 3] != ly) continue;
                    var graphic = (ushort)(recs[i] | (recs[i + 1] << 8));
                    if (graphic >= 0x0C45 && graphic <= 0x0DAF) continue;
                    dest.Add((graphic, (sbyte)recs[i + 4]));
                }
            }
            return true;
        }
    }

    static byte[] StaticBytes(byte[] dif, uint position, uint size) {
        if (size == 0 || position == 0xFFFFFFFF) return [];
        var count = (int)Math.Min(1024u, size / 7u);
        var bytes = count * StaticSize;
        var start = (long)position;
        if (bytes <= 0 || start < 0 || start + bytes > dif.Length) return [];
        var recs = new byte[bytes];
        Buffer.BlockCopy(dif, (int)start, recs, 0, bytes);
        return recs;
    }

    // ClassicUO restores a map only when its mul, staidx, and statics are all non-empty.
    static bool CanReset(int i) => HasMul($"map{i}.mul") && HasMul($"staidx{i}.mul") && HasMul($"statics{i}.mul");

    static bool MapLoaded(int i) => HasMul($"map{i}.mul") || HasMul($"map{i}LegacyMUL.uop");

    static bool HasMul(string name) {
        var path = MulPath(name);
        if (path == null) return false;
        try {
            var info = new FileInfo(path);
            return info.Exists && info.Length != 0;
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    static byte[] ReadMul(string name) {
        var path = MulPath(name);
        if (path == null) return null;
        try {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (fs.Length == 0 || fs.Length > int.MaxValue) return null;
            var bytes = new byte[fs.Length];
            var n = 0;
            while (n < bytes.Length) {
                var r = fs.Read(bytes, n, bytes.Length - n);
                if (r <= 0) return null;
                n += r;
            }
            return bytes;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    static string MulPath(string name) {
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return null;
        return Path.Combine(root, name);
    }

    static int ReadI32BE(byte[] p, ref int pos, int end) {
        if (pos < 0 || pos + 4 > end) return 0;
        var v = (int)(((uint)p[pos] << 24) | ((uint)p[pos + 1] << 16) | ((uint)p[pos + 2] << 8) | p[pos + 3]);
        pos += 4;
        return v;
    }

    static uint U32(byte[] b, int o) =>
        (uint)b[o] | ((uint)b[o + 1] << 8) | ((uint)b[o + 2] << 16) | ((uint)b[o + 3] << 24);
}
