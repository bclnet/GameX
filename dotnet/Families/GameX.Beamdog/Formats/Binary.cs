using GameX.Uncore.Formats;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Beamdog.Formats;

#region Binary_Abc

public class Binary_Abc : IHaveMetaInfo {
    public Binary_Abc(BinaryReader r) {
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("BinaryPak", items: [
            //new($"Type: {Type}"),
        ])
    ];
}

#endregion

// https://gibberlings3.github.io/iesdp/file_formats/ie_formats/key_v1.htm
// https://gibberlings3.github.io/iesdp/file_formats/ie_formats/bif_v1.htm
// BIFC V1.0 is version, uncompressed size, then (declen, complen, zlib) blocks.

#region Binary_Key

/// <summary>
/// Infinity Engine chitin.key, BIFF V1, and block-compressed BIFC V1.0.
/// </summary>
public class Binary_Key : ArcBinary<Binary_Key> {
    const uint KEY_MAGIC = 0x2059454B; // "KEY "
    const uint KEY_VERSION = 0x20203156; // "V1  "
    const uint BIFF_MAGIC = 0x46464942; // "BIFF"
    const uint BIFF_VERSION = 0x20203156; // "V1  "
    const uint BIFC_MAGIC = 0x43464942; // "BIFC"
    const uint BIFC_VERSION = 0x302E3156; // "V1.0"

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        var files = new List<FileSource>();
        var magic = source.Magic = r.ReadUInt32();
        if (magic == KEY_MAGIC) ReadKey(source, r, files);
        else if (magic == BIFF_MAGIC) ReadBiff(source, r, files, null, null, null, true);
        else if (magic == BIFC_MAGIC) ReadBifc(source, r, files, null, null, null, true);
        else throw new FormatException($"Unknown File Type {magic}");
        source.Files = files;
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        if (file.Tag is string path) return Task.FromResult(ReadPath(path, file.Offset, file.FileSize));
        if (source.Tag is byte[] inflated) return Task.FromResult<Stream>(new MemoryStream(Slice(inflated, file.Offset, file.FileSize)));
        if (source.Magic == BIFC_MAGIC) {
            r.Seek(0);
            var magic = r.ReadUInt32();
            if (magic != BIFC_MAGIC) throw new FormatException($"Unknown File Type {magic}");
            return Task.FromResult<Stream>(new MemoryStream(Slice(InflateBifc(r), file.Offset, file.FileSize)));
        }
        r.Seek(file.Offset);
        return Task.FromResult<Stream>(new MemoryStream(r.ReadBytes((int)file.FileSize)));
    }

    static void ReadKey(BinaryArchive source, BinaryReader r, List<FileSource> files) {
        var version = r.ReadUInt32();
        if (version != KEY_VERSION) throw new FormatException("BAD MAGIC");
        source.Version = version;
        var bifCount = r.ReadUInt32();
        var resourceCount = r.ReadUInt32();
        var bifOffset = r.ReadUInt32();
        var resourceOffset = r.ReadUInt32();

        var raw = new List<(uint nameOfs, ushort nameLen)>();
        r.Seek(bifOffset);
        for (var i = 0u; i < bifCount; i++) {
            _ = r.ReadUInt32();
            var nameOfs = r.ReadUInt32();
            var nameLen = r.ReadUInt16();
            _ = r.ReadUInt16();
            raw.Add((nameOfs, nameLen));
        }
        var bifs = new List<string>(raw.Count);
        foreach (var s in raw) bifs.Add(ReadName(r, s.nameOfs, s.nameLen));

        var byBif = new Dictionary<int, Dictionary<int, (string resref, ushort type)>>();
        r.Seek(resourceOffset);
        for (var i = 0u; i < resourceCount; i++) {
            var resref = ResRef(r.ReadBytes(8));
            var resType = r.ReadUInt16();
            var locator = r.ReadUInt32();
            if (((locator >> 14) & 0x3F) != 0) continue;
            var bif = (int)((locator >> 20) & 0xFFF);
            var index = (int)(locator & 0x3FFF);
            if (!byBif.TryGetValue(bif, out var map)) byBif[bif] = map = new Dictionary<int, (string, ushort)>();
            if (!map.ContainsKey(index)) map[index] = (resref, resType);
        }

        var dir = Path.GetDirectoryName(source.BinPath) ?? "";
        for (var i = 0; i < bifs.Count; i++) {
            var rel = bifs[i];
            var full = Path.GetFullPath(Path.Combine(dir, rel));
            if (!File.Exists(full)) continue;
            if (!byBif.TryGetValue(i, out var map)) map = new Dictionary<int, (string, ushort)>();
            OpenBif(source, files, full, rel, map);
        }
    }

    static void OpenBif(BinaryArchive source, List<FileSource> files, string full, string rel, Dictionary<int, (string resref, ushort type)> map) {
        using var br = new BinaryReader(File.OpenRead(full));
        var magic = br.ReadUInt32();
        if (magic == BIFF_MAGIC) ReadBiff(source, br, files, rel, full, map, false);
        else if (magic == BIFC_MAGIC) ReadBifc(source, br, files, rel, full, map, false);
        else throw new FormatException($"Unknown File Type {magic}");
    }

    static void ReadBifc(BinaryArchive source, BinaryReader r, List<FileSource> files, string rel, string tagPath, Dictionary<int, (string resref, ushort type)> map, bool setVersion) {
        var inflated = InflateBifc(r);
        if (setVersion && source.Tag is not System.Collections.IDictionary) source.Tag = inflated;
        using var inner = new BinaryReader(new MemoryStream(inflated));
        var magic = inner.ReadUInt32();
        if (magic != BIFF_MAGIC) throw new FormatException($"Unknown File Type {magic}");
        ReadBiff(source, inner, files, rel, tagPath, map, setVersion);
    }

    static void ReadBiff(BinaryArchive source, BinaryReader r, List<FileSource> files, string rel, string tagPath, Dictionary<int, (string resref, ushort type)> map, bool setVersion) {
        var version = r.ReadUInt32();
        if (version != BIFF_VERSION) throw new FormatException("BAD MAGIC");
        if (setVersion) source.Version = version;
        var fileCount = r.ReadUInt32();
        _ = r.ReadUInt32();
        var fileTableOffset = r.ReadUInt32();
        r.Seek(fileTableOffset);
        for (var i = 0u; i < fileCount; i++) {
            var locator = r.ReadUInt32();
            var offset = r.ReadUInt32();
            var size = r.ReadUInt32();
            var resType = r.ReadUInt16();
            _ = r.ReadUInt16();
            if (map != null) {
                var index = (int)(locator & 0x3FFF);
                if (!map.TryGetValue(index, out var res)) continue;
                files.Add(new FileSource {
                    Path = $"{rel}/{res.resref}{Ext(res.type)}",
                    Offset = offset,
                    FileSize = size,
                    Tag = tagPath,
                });
            }
            else files.Add(new FileSource {
                Path = $"{i:D4}{Ext(resType)}",
                Offset = offset,
                FileSize = size,
            });
        }
    }

    // IESDP BIFC V1.0. Cap the copy at the stated uncompressed size.
    static byte[] InflateBifc(BinaryReader r) {
        var version = r.ReadUInt32();
        if (version != BIFC_VERSION) throw new FormatException("BAD MAGIC");
        var uncompressed = r.ReadUInt32();
        if (uncompressed > int.MaxValue) throw new FormatException("zlib");
        var output = new byte[uncompressed];
        var filled = 0;
        while (filled < uncompressed) {
            var declen = r.ReadUInt32();
            var complen = r.ReadUInt32();
            if (declen == 0 || complen == 0 || declen > int.MaxValue || complen > int.MaxValue) throw new FormatException("zlib");
            var chunk = r.DecompressZlib((int)complen, (int)declen);
            if (chunk.Length < declen) throw new FormatException("zlib");
            var take = (int)declen;
            if (filled + take > uncompressed) take = (int)uncompressed - filled;
            Buffer.BlockCopy(chunk, 0, output, filled, take);
            filled += take;
        }
        return output;
    }

    static Stream ReadPath(string path, long offset, long size) {
        using var br = new BinaryReader(File.OpenRead(path));
        var magic = br.ReadUInt32();
        if (magic == BIFC_MAGIC) return new MemoryStream(Slice(InflateBifc(br), offset, size));
        br.Seek(offset);
        return new MemoryStream(br.ReadBytes((int)size));
    }

    static byte[] Slice(byte[] data, long offset, long size) {
        var o = checked((int)offset);
        var n = checked((int)size);
        var s = new byte[n];
        Buffer.BlockCopy(data, o, s, 0, n);
        return s;
    }

    static string ReadName(BinaryReader r, uint offset, ushort length) {
        if (length == 0) return "";
        r.Seek(offset);
        return BifName(r.ReadBytes(length));
    }

    static string BifName(byte[] b) {
        var n = 0;
        while (n < b.Length && b[n] != 0) n++;
        return Encoding.ASCII.GetString(b, 0, n).Replace('\\', '/');
    }

    static string ResRef(byte[] b) {
        var n = 0;
        while (n < b.Length && b[n] != 0) n++;
        return Encoding.ASCII.GetString(b, 0, n).Trim();
    }

    static string Ext(int type) => type switch {
        0x0001 => ".bmp",
        0x0002 => ".mve",
        0x0004 => ".wav",
        0x0005 => ".wfx",
        0x0006 => ".plt",
        0x03e8 => ".bam",
        0x03e9 => ".wed",
        0x03ea => ".chu",
        0x03eb => ".tis",
        0x03ec => ".mos",
        0x03ed => ".itm",
        0x03ee => ".spl",
        0x03ef => ".bcs",
        0x03f0 => ".ids",
        0x03f1 => ".cre",
        0x03f2 => ".are",
        0x03f3 => ".dlg",
        0x03f4 => ".2da",
        0x03f5 => ".gam",
        0x03f6 => ".sto",
        0x03f7 => ".wmp",
        0x03f8 => ".eff",
        0x03f9 => ".bs",
        0x03fa => ".chr",
        0x03fb => ".vvc",
        0x03fc => ".vef",
        0x03fd => ".pro",
        0x03fe => ".bio",
        0x03ff => ".wbm",
        0x0400 => ".fnt",
        0x0402 => ".gui",
        0x0403 => ".sql",
        0x0404 => ".pvrz",
        0x0405 => ".glsl",
        0x0406 => ".tot",
        0x0407 => ".toh",
        0x0408 => ".menu",
        0x0409 => ".lua",
        0x040a => ".ttf",
        0x040b => ".png",
        0x044c => ".bah",
        0x0802 => ".ini",
        0x0803 => ".src",
        0x0804 => ".maze",
        _ => "",
    };
}

#endregion

// No MDK2 HD .dat on disk. Steam app 213350 is not installed (install dir "MDK 2 HD"); checked libraryfolders on C, E, and G, AbandonLibrary, GOG, and %LOCALAPPDATA%\MDK2HD. GOG's MDK 2 is the 2000 zip game, not this archive. PCGamingWiki has no file list. Magic was not observed. 0x04034B50 rewinds into Binary_Zip; any other magic throws. MDK2:W has no second magic.

#region Binary_Dat

/// <summary>
/// Binary_Dat
/// </summary>
public class Binary_Dat : ArcBinary<Binary_Dat> {
    const uint ZipMagic = 0x04034B50;

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        if (r.BaseStream.Length < 4) throw new FormatException("Binary_Dat: no sample");
        var magic = r.ReadUInt32();
        if (magic == ZipMagic) {
            r.BaseStream.Position = 0;
            return Binary_Zip.GetArcBinary(source.Game).Read(source, r, tag);
        }
        throw new FormatException($"Binary_Dat: unknown magic {magic:X8}");
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        return Binary_Zip.GetArcBinary(source.Game).ReadData(source, r, file, option);
    }
}

#endregion

// GOB and LMP from jbgda GobExtractor / Lmp. Dark Alliance stores the name in the 0x40 LMP row.
// The other Snowblind games store a name offset in a 0x0C row. DA2 is not its own jbgda game, so the inline row is tried first. No 2022 PC sample was on disk.

#region Binary_Da2

/// <summary>
/// Binary_Da2
/// </summary>
public class Binary_Da2 : ArcBinary<Binary_Da2> {
    const int GobStride = 0x28;
    const int LmpDaStride = 0x40;
    const int LmpRefStride = 0x0C;
    const int MaxFiles = 100000;

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        if (r.BaseStream.Length < 4) throw new FormatException("Binary_Da2: no sample");
        var ext = Path.GetExtension(source.BinPath).ToLowerInvariant();
        var files = new List<FileSource>();
        if (ext == ".gob") ReadGob(r, files);
        else if (ext == ".lmp") ReadLmp(r, files);
        else throw new FormatException($"Binary_Da2: unknown magic {r.ReadUInt32():X8}");
        source.Files = files;
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        r.BaseStream.Position = file.Offset;
        return Task.FromResult<Stream>(new MemoryStream(r.ReadBytes((int)file.FileSize)));
    }

    static void ReadGob(BinaryReader r, List<FileSource> files) {
        var length = r.BaseStream.Length;
        var pos = 0L;
        var rows = new List<(string name, long offset, long size)>();
        while (pos + GobStride <= length) {
            r.BaseStream.Position = pos;
            var rec = r.ReadBytes(GobStride);
            var name = CString(rec, 0, 0x20);
            if (name.Length == 0) break;
            if (rec[0] < 0x20 || rec[0] > 0x7E) throw new FormatException($"Binary_Da2: unknown magic {BitConverter.ToUInt32(rec, 0):X8}");
            var offset = BitConverter.ToUInt32(rec, 0x20);
            var sizeField = BitConverter.ToUInt32(rec, 0x24);
            rows.Add((name.Replace('\\', '/'), offset, sizeField));
            pos += GobStride;
        }
        for (var i = 0; i < rows.Count; i++) {
            var (name, offset, sizeField) = rows[i];
            long size = sizeField > 0 && offset + sizeField <= length ? sizeField : (i + 1 < rows.Count ? rows[i + 1].offset : length) - offset;
            if (size < 0 || offset + size > length) throw new FormatException("Binary_Da2: bad gob entry");
            files.Add(new FileSource { Path = name, Offset = offset, FileSize = size });
        }
    }

    static void ReadLmp(BinaryReader r, List<FileSource> files) {
        var length = r.BaseStream.Length;
        r.BaseStream.Position = 0;
        var count = r.ReadUInt32();
        if (count == 0) return;
        if (count > MaxFiles) throw new FormatException($"Binary_Da2: unknown magic {count:X8}");
        if (TryDa(r, length, count, files)) return;
        files.Clear();
        if (TryRef(r, length, count, files)) return;
        throw new FormatException($"Binary_Da2: unknown magic {count:X8}");
    }

    static bool TryDa(BinaryReader r, long length, uint count, List<FileSource> files) {
        var header = 4L;
        if (header + count * LmpDaStride > length) return false;
        for (var i = 0; i < count; i++) {
            r.BaseStream.Position = header;
            var rec = r.ReadBytes(LmpDaStride);
            if (!HasCString(rec, 0, 0x38)) return false;
            var name = CString(rec, 0, 0x38).Replace('\\', '/');
            var offset = BitConverter.ToUInt32(rec, 0x38);
            var size = BitConverter.ToUInt32(rec, 0x3C);
            if (name.Length == 0 || offset + (long)size > length) return false;
            files.Add(new FileSource { Path = name, Offset = offset, FileSize = size });
            header += LmpDaStride;
        }
        return true;
    }

    static bool TryRef(BinaryReader r, long length, uint count, List<FileSource> files) {
        var header = 4L;
        if (header + count * LmpRefStride > length) return false;
        var rows = new List<(uint str, uint offset, uint size)>();
        for (var i = 0; i < count; i++) {
            r.BaseStream.Position = header;
            var str = r.ReadUInt32();
            var offset = r.ReadUInt32();
            var size = r.ReadUInt32();
            if (str >= length || offset + (long)size > length) return false;
            rows.Add((str, offset, size));
            header += LmpRefStride;
        }
        foreach (var (str, offset, size) in rows) {
            r.BaseStream.Position = str;
            var name = ReadCString(r, (int)(length - str)).Replace('\\', '/');
            if (name.Length == 0) return false;
            files.Add(new FileSource { Path = name, Offset = offset, FileSize = size });
        }
        return true;
    }

    static bool HasCString(byte[] rec, int start, int max) {
        for (var i = 0; i < max; i++) {
            var b = rec[start + i];
            if (b == 0) return true;
            if (b < 0x20 || b > 0x7E) return false;
        }
        return false;
    }

    static string CString(byte[] rec, int start, int max) {
        var n = 0;
        while (n < max && rec[start + n] != 0) n++;
        return Encoding.ASCII.GetString(rec, start, n);
    }

    static string ReadCString(BinaryReader r, int max) {
        var bytes = new List<byte>();
        for (var i = 0; i < max; i++) {
            var b = r.ReadByte();
            if (b == 0) break;
            bytes.Add(b);
        }
        return Encoding.ASCII.GetString(bytes.ToArray());
    }
}

#endregion

// PST irongolm.bif GLOW3 frame 0 is 10 00 10 00 08 00 08 00 2A 04 00 00: 16x16, data offset 0x42A, bit 31 clear, 98 payload bytes. Header byte 0x0B is 0.
// pillar.bif BONE has the same 0 and a clear bit 31, and its payload length equals width*height with no 0 byte, so that byte is the RLE color, not "0 means uncompressed".
// 668 of 669 PST V1 BAMs store 0 there; one stores 28. Uncompressed is bit 31 set: irongolm.bif entry 1 frame 0 is 96 00 96 00 01 00 01 00 E0 05 00 80 (0x800005E0), 32 frames, 720000 pixels, file 721504.
// Bit 31 clear is RLE and is not decoded. Cycles follow the frames as uint16 count plus uint16 lookup start. GLOW3 at file+36 is 01 00 00 00 and the lookup at 1064 is 00 00. BONE's lookup is 32 bytes for 16 uint16 indexes.

#region Binary_Bam

public class Binary_Bam : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult<object>(new Binary_Bam(r));

    public readonly int FrameCount;
    readonly GfxSpriteSheet _sheet;
    public GfxSpriteSheet ToSheet() => _sheet;

    public Binary_Bam(BinaryReader r) {
        var sigBytes = r.ReadBytes(8);
        if (sigBytes.Length != 8) throw new FormatException("Binary_Bam: truncated");
        var sig = Encoding.ASCII.GetString(sigBytes);
        if (sig == "BAMCV1  ") throw new FormatException("Binary_Bam: compressed BAMC");
        if (sig != "BAM V1  ") throw new FormatException("Binary_Bam: not BAM V1");
        FrameCount = r.ReadUInt16();
        var cycleCount = r.ReadByte();
        var rleColor = r.ReadByte();
        var frameOfs = r.ReadUInt32();
        var paletteOfs = r.ReadUInt32();
        var lookupOfs = r.ReadUInt32();
        var len = r.BaseStream.Length;
        void Need(long at, long n) { if (at < 0 || n < 0 || at + n > len) throw new FormatException("Binary_Bam: truncated"); }

        Need(frameOfs, 12L * FrameCount + 4L * cycleCount);
        r.Seek(frameOfs);
        var frames = new (int w, int h, uint data)[FrameCount];
        for (var i = 0; i < FrameCount; i++) {
            var w = r.ReadUInt16();
            var h = r.ReadUInt16();
            _ = r.ReadInt16();
            _ = r.ReadInt16();
            frames[i] = (w, h, r.ReadUInt32());
        }
        var heads = new (int count, int first)[cycleCount];
        for (var i = 0; i < cycleCount; i++) heads[i] = (r.ReadUInt16(), r.ReadUInt16());

        Need(paletteOfs, 1024);
        r.Seek(paletteOfs);
        var pal = r.ReadBytes(1024);
        if (pal.Length != 1024) throw new FormatException("Binary_Bam: truncated");

        var cells = new List<(byte[] rgba, int width, int height, float duration)>(FrameCount);
        foreach (var (w, h, data) in frames) {
            if ((data & 0x80000000) == 0) throw new FormatException($"Binary_Bam: compressed RLE (color {rleColor})");
            var n = (long)w * h;
            if (n > int.MaxValue) throw new FormatException("Binary_Bam: truncated");
            var at = (long)(data & 0x7FFFFFFFu);
            Need(at, n);
            r.Seek(at);
            var idx = r.ReadBytes((int)n);
            if (idx.Length != n) throw new FormatException("Binary_Bam: truncated");
            var rgba = new byte[idx.Length * 4];
            for (var i = 0; i < idx.Length; i++) {
                var p = idx[i];
                var o = p * 4;
                rgba[i * 4] = pal[o + 2];
                rgba[i * 4 + 1] = pal[o + 1];
                rgba[i * 4 + 2] = pal[o];
                rgba[i * 4 + 3] = p == 0 ? (byte)0 : pal[o + 3] == 0 ? (byte)255 : pal[o + 3];
            }
            cells.Add((rgba, w, h, 0.1f));
        }

        var anims = new List<(string name, int[] frames)>();
        if (lookupOfs != 0) {
            foreach (var (count, first) in heads) {
                Need(lookupOfs + 2L * first, 2L * count);
                r.Seek(lookupOfs + 2L * first);
                var idx = new int[count];
                for (var k = 0; k < count; k++) idx[k] = r.ReadUInt16();
                anims.Add(($"{anims.Count}", idx));
            }
        }
        _sheet = GfxSpriteSheet.Pack(cells, anims);
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("Binary_Bam", items: [
            new($"Frames: {FrameCount}"),
        ])
    ];
}

#endregion

#region Binary_XXX

public unsafe class Binary_XXX : ArcBinary<Binary_XXX> {
    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        var files = source.Files = [];
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        throw new NotImplementedException();
    }
}

#endregion
