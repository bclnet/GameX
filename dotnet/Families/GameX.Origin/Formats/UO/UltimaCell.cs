using GameX;
using GameX.Origin;
using GameX.Origin.Formats;
using OpenX;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static OpenX.CellManager;

namespace GameX.Origin.Formats.UO;

/// <summary>
/// Ultima map cells. A cell is an 8x8 block of 8x8 land chunks (64x64 tiles).
/// Land rules are <see cref="UltimaCells"/>. The caller supplies the map stream, mul or UOP.
/// </summary>
public sealed class UltimaLtex(int index) : ILtex {
    public long Intv => index;
    public string Path => $"tex{index}";
}

public sealed class UltimaModel(string name) : ICellXrefModel {
    public string ModelPath => name;
}

public sealed class UltimaXref(string name, Vector3 position) : ICellXref {
    public string Name => name;
    public float? Scale => null;
    public Vector3 Position => position;
    public Vector3 EulerAngles => default;
}

public sealed class UltimaLand(Int3 gridId, uint[] vtex, sbyte[] heights) : ILand {
    public Int3 GridId => gridId;
    public uint[] Vtex => vtex;
    public float HeightOffset => 0;
    public sbyte[] Heights => heights;
}

public sealed class UltimaCell(uint id, Int3 gridId, string name, List<ICellXref> xrefs) : ICell {
    public uint Id => id;
    public bool IsInterior => false;
    public Int3 GridId => gridId;
    public string Name => name;
    public System.Drawing.Color? AmbientLight => null;
    public List<ICellXref> Xrefs => xrefs;
}

/// <summary>
/// One Ultima facet. <paramref name="textureIds"/> is land tile id to texture id, indexed by <c>tileId &amp; 0x3FFF</c>.
/// </summary>
public sealed class UltimaCellQuery : IQuery {
    public const float MeterInUnitsValue = 10f;
    public const float CellLengthValue = 64f;
    public const int CellPack = 8;
    const int LandChunkSize = 196;
    const int LandChunkData = 192;
    static readonly int[] MapHeights = [512, 512, 200, 256, 181];
    static readonly int[] RadiusValue = [2, 2];

    readonly Stream Map, Statics, Staidx;
    readonly short[] TextureIds;
    readonly Dictionary<Int3, UltimaCell> Cells = [];
    readonly Dictionary<Int3, UltimaLand> Lands = [];
    readonly uint ChunkHeight, ChunkWidth;

    public UltimaCellQuery(BinaryArchive map, Stream statics, Stream staidx, short[] textureIds, int mapIndex = 0)
        : this(Binary_UO.OpenMap(map), statics, staidx, textureIds, mapIndex) { }

    public UltimaCellQuery(Stream map, Stream statics, Stream staidx, short[] textureIds, int mapIndex = 0) {
        Map = map; Statics = statics; Staidx = staidx;
        TextureIds = textureIds ?? [];
        World = mapIndex;
        var height = MapHeights[mapIndex < 0 ? 0 : mapIndex >= MapHeights.Length ? MapHeights.Length - 1 : mapIndex];
        ChunkHeight = (uint)height;
        var length = map?.Length ?? 0;
        ChunkWidth = height == 0 ? 0 : (uint)(length / (height * LandChunkSize));
    }

    public float MeterInUnits => MeterInUnitsValue;
    public float CellLengthInMeters => CellLengthValue;
    public int[] Radius => RadiusValue;
    public int World { get; private set; }
    public void SetWorld(int world) { }
    public Int3 GetCellId(Vector3 point) => new((int)Math.Floor(point.X / CellLengthInMeters), (int)Math.Floor(point.Z / CellLengthInMeters), World);
    public ICell FindCellByName(string name) => null;
    public ILtex FindLtex(int index) => new UltimaLtex(index);
    public object FindAnyByName(string name) {
        if (name == null || name.Length < 6) return null;
        if ((name.StartsWith("sta") || name.StartsWith("lnd")) && short.TryParse(name.Substring(3), out _)) return new UltimaModel(name);
        return null;
    }
    public ILand FindLand(Int3 cell) { Ensure(cell); return Lands.TryGetValue(cell, out var land) ? land : null; }
    public ICell FindCell(Int3 cell) { Ensure(cell); return Cells.TryGetValue(cell, out var found) ? found : null; }
    public CellBuilder CreateCellBuilder<O, M, T, S>(ISource source, IOpenGfx[] gfx) => new UltimaCellBuilder<O, M, T, S>(source, this, gfx);

    void Ensure(Int3 cell) {
        lock (this) {
            if (Cells.ContainsKey(cell) || ChunkWidth == 0) return;
            var heights = new sbyte[UltimaCells.Side * UltimaCells.Side];
            var vtex = new uint[UltimaCells.Side * UltimaCells.Side];
            var xrefs = new List<ICellXref>();
            for (uint cy = 0; cy < CellPack; cy++)
                for (uint cx = 0; cx < CellPack; cx++)
                    ReadChunk(heights, vtex, xrefs, (uint)cell.X * CellPack + cx, (uint)cell.Y * CellPack + cy);
            Lands[cell] = new UltimaLand(cell, vtex, heights);
            Cells[cell] = new UltimaCell((uint)cell.X | ((uint)cell.Y << 16), cell, $"{cell.X}x{cell.Y}", xrefs);
        }
    }

    void ReadChunk(sbyte[] heights, uint[] vtex, List<ICellXref> xrefs, uint chunkX, uint chunkY) {
        var ground = ReadLand(chunkX, chunkY);
        var i = 0;
        for (uint x = 0; x < 8; x++)
            for (uint y = 0; y < 8; y++) {
                var tileId = (short)(ground[i++] | (ground[i++] << 8));
                var z = (sbyte)ground[i++];
                var idx = (((chunkY & 7) * 8 + y) * UltimaCells.Side) + ((chunkX & 7) * 8) + x;
                heights[idx] = z;
                var tid = tileId & 0x3FFF;
                vtex[idx] = (uint)(tid < TextureIds.Length ? TextureIds[tid] : 0);
            }
        var statics = ReadStatic(chunkX, chunkY, out var length);
        var count = length / 7;
        var s = 0;
        for (var n = 0; n < count; n++) {
            var tileId = (short)(statics[s++] | (statics[s++] << 8));
            var x = (sbyte)statics[s++];
            var y = (sbyte)statics[s++];
            var z = (sbyte)statics[s++];
            s += 2;
            if (tileId >= 0x0C45 && tileId <= 0x0DAF) continue;
            xrefs.Add(new UltimaXref($"sta{tileId:000}", new Vector3(chunkX * 8 + x, chunkY * 8 + y, z / MeterInUnitsValue)));
        }
    }

    byte[] ReadLand(uint chunkX, uint chunkY) {
        var data = new byte[LandChunkData];
        if (Map == null || ChunkWidth == 0) return data;
        chunkX %= ChunkWidth; chunkY %= ChunkHeight;
        var ptr = ((chunkX * ChunkHeight) + chunkY) * LandChunkSize + 4;
        lock (Map) {
            if (ptr < 0 || ptr + LandChunkData > Map.Length) return data;
            Map.Position = ptr;
            Map.Read(data, 0, LandChunkData);
        }
        return data;
    }

    byte[] ReadStatic(uint chunkX, uint chunkY, out int length) {
        length = 0;
        if (Statics == null || Staidx == null || ChunkWidth == 0) return [];
        chunkX %= ChunkWidth; chunkY %= ChunkHeight;
        var pos = ((chunkX * ChunkHeight) + chunkY) * 12;
        var idx = new byte[8];
        lock (Staidx) {
            if (pos < 0 || pos + 8 > Staidx.Length) return [];
            Staidx.Position = pos;
            if (Staidx.Read(idx, 0, 8) < 8) return [];
        }
        var lookup = BitConverter.ToInt32(idx, 0);
        length = BitConverter.ToInt32(idx, 4);
        if (lookup < 0 || length <= 0) { length = 0; return []; }
        var buf = new byte[length];
        lock (Statics) {
            if (lookup + length > Statics.Length) { length = 0; return []; }
            Statics.Position = lookup;
            length = Statics.Read(buf, 0, length);
        }
        return buf;
    }
}
