using Compression;
using GameX.Crytek.Formats.Core;
using GameX.Crytek.Formats.Core.Chunks;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using K4os.Compression.LZ4;
using OpenStack;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GameX.Crytek.Formats;

#region Binary_ArcheAge

public class Binary_ArcheAge(byte[] key) : ArcBinary {
    readonly byte[] Key = key;

    #region Headers

    const uint MAGIC = 0x4f424957; // Magic for Archeage, the literal string "WIBO".

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HDR {
        public static (string, int) Struct = ("<8I", 32);
        public uint Magic, Dummy1;
        public uint FileCount;
        public uint ExtraFiles, Dummy2, Dummy3, Dummy4, Dummy5;
    }

    #endregion

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        var fs = r.BaseStream; var fsLength = fs.Length;
        using var aes = new AesManaged { Key = Key, IV = new byte[16], Mode = CipherMode.CBC };
        r = new BinaryReader(new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read));

        // read hdr & skip
        fs.Seek(fsLength - 0x200, SeekOrigin.Begin);
        var hdr = r.ReadS<HDR>();
        if (hdr.Magic != MAGIC) throw new FormatException("BAD MAGIC");
        var totalSize = (hdr.FileCount + hdr.ExtraFiles) * 0x150;
        var infoOffset = fsLength - 0x200 - totalSize;
        while (infoOffset >= 0)
            if ((infoOffset % 0x200) != 0) infoOffset -= 0x10;
            else break;

        // read-all files
        FileSource[] files;
        source.Files = files = new FileSource[hdr.FileCount];
        for (var i = 0; i < hdr.FileCount; i++) {
            r = new BinaryReader(new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read));
            fs.Seek(infoOffset, SeekOrigin.Begin);
            files[i] = new FileSource {
                Path = r.ReadFAString(0x108), //: name //.Replace('\\', '/')
                Offset = r.ReadInt64(),       //: offset
                FileSize = r.ReadInt64(),     //: size
                PackedSize = r.ReadInt64(),   //: xsize
                Compressed = r.ReadInt32(),   //: ysize
            };
            infoOffset += 0x150;
        }
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        r.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
    }
}

#endregion

#region Binary_Dunia

/// <summary>
/// Binary_Dunia
/// </summary>
public class Binary_Dunia : ArcBinary<Binary_Dunia> {
    const uint MAGIC = 0x46415432; // 'FAT2'

    enum Platform : byte {
        Any = 0,
        Windows, // data_win32 or data_win64 or data_final
        Xenon, // no platform directory (Xbox 360)
        PS3, // data_ps3
        WiiU, // no platform directory (Wii U)
        Durango, // data_durango (Xbox One)
        Orbis, // data_orbis (PS4)
        Scarlett, // data_scarlett (Xbox Series X)
        Prospero, // data_prospero (PS5)
        Yeti, // data_yeti (Stadia)
    }

    enum Compression : byte {
        None = 0,
        LZO1x,
        Zlib,
        XMemCompress, // Xbox 360
        LZMA,
        LZ4,
        LZ4LW,
        Oodle,
    }

    static readonly ReadOnlyCollection<Version> KnownVersions = new([
        // 32
        (5, Platform.Any, 0),       // Far Cry 2
        (5, Platform.Windows, 3),   // Far Cry 2
        (5, Platform.PS3, 4),       // Far Cry 2
        (9, Platform.Any, 3),       // Far Cry 3
        (9, Platform.Windows, 3),   // Far Cry 3
        // 64
        (9, Platform.Any, 0),       // Far Cry 3, Far Cry 3 Blood Dragon, Far Cry 4, Far Cry Primal
        (9, Platform.Windows, 3),   // Far Cry 3, Far Cry 3 Blood Dragon, Far Cry 4
        (9, Platform.Windows, 4),   // Far Cry Primal
        (10, Platform.Windows, 0),  // Far Cry 5, Far Cry New Dawn
        (11, Platform.Windows, 0),  // Far Cry 6
    ]);

    readonly struct Version(int fileVersion, Platform platform, byte compressionVersion) : IEquatable<Version> {
        public readonly int FileVersion = fileVersion;
        public readonly Platform Platform = platform;
        public readonly byte CompressionVersion = compressionVersion;
        public readonly void Deconstruct(out int fileVersion, out Platform platform, out byte compressionVersion) { fileVersion = FileVersion; platform = Platform; compressionVersion = CompressionVersion; }
        public static implicit operator (int fileVersion, Platform platform, byte compressionVersion)(Version value) => (value.FileVersion, value.Platform, value.CompressionVersion);
        public static implicit operator Version((int fileVersion, Platform platform, byte compressionVersion) value) => new(value.fileVersion, value.platform, value.compressionVersion);
        public bool Equals(Version other) => FileVersion == other.FileVersion && Platform == other.Platform && CompressionVersion == other.CompressionVersion;
        public override bool Equals(object obj) => obj is Version version && Equals(version) == true;
        public override readonly int GetHashCode() => HashCode.Combine(FileVersion, Platform, CompressionVersion);
        public static bool operator ==(Version left, Version right) => left.Equals(right);
        public static bool operator !=(Version left, Version right) => !left.Equals(right);
    }

    static readonly byte[] TEAXors = [0x76, 0x41, 0x74, 0x1E, 0x4E, 0x16, 0x1E, 0x02, 0x6A, 0x5B, 0x72, 0x0B, 0x60, 0x4F, 0x72, 0x25];
    static uint[] MakeXTEAKey(uint value) {
        var key = new byte[16];
        for (var i = 0; i < 16; i++) {
            byte b = (byte)((value >> i) + 0x39), x = TEAXors[i];
            key[i] = b != x ? (byte)(b ^ x) : (byte)0xFF;
        }
        return [BitConverter.ToUInt32(key, 0), BitConverter.ToUInt32(key, 4), BitConverter.ToUInt32(key, 8), BitConverter.ToUInt32(key, 12)];
    }

    static BinaryReader DecryptIndex(byte[] data) {
        var size = data.Length & ~7;
        var key = MakeXTEAKey((uint)size);
        XTEA.Decrypt(data, 0, size, key);
        return new BinaryReader(new MemoryStream(data, false));
    }

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        if (!source.BinPath.EndsWith(".fat")) throw new FormatException("must be a .fat file");
        var magic = r.ReadUInt32();
        if (magic != MAGIC) throw new FormatException("BAD MAGIC");
        var fileVersionAndEncryptionFlag = r.ReadUInt32();
        var fileVersion = (int)(fileVersionAndEncryptionFlag & ~0x80000000u);
        var indexIsEncrypted = (fileVersionAndEncryptionFlag & 0x80000000u) != 0;
        if (indexIsEncrypted && fileVersion < 11) throw new FormatException("encryption flag set when unsupported");
        if (fileVersion > 11) throw new FormatException("unsupported version");
        var flags = fileVersion >= 3 ? r.ReadUInt32() : 0u;
        var unknown0C = fileVersion >= 9 ? r.ReadUInt32() : 0u;
        var unknown10 = fileVersion >= 9 ? r.ReadUInt32() : 0u;
        var platform = (Platform)(byte)(flags >> 0);
        var compressionVersion = (byte)(flags >> 8);
        if ((flags & 0xFFFF0000u) != 0) throw new FormatException("unknown flags");
        var version = new Version(fileVersion, platform, compressionVersion);
        if (!KnownVersions.Contains(version)) throw new FormatException("unknown version/platform/CV combination");
        if (unknown0C != 0 || unknown10 != 0) throw new NotImplementedException();

        // get hashes
        var filelist = Path.ChangeExtension(source.BinPath, ".filelist").Replace('\\', '/');
        var hashes = source.Game.Id switch {
            "FarCry2" => FarCry2.GetHashes(filelist),
            "FarCry3" or "FarCry3:BD" or "FarCry4" => FarCry3.GetHashes(filelist),
            "FarCry5" => FarCry5.GetHashes(filelist),
            "FarCry6" => FarCry6.GetHashes(filelist),
            "FarCryND" => FarCryNewDawn.GetHashes(filelist),
            "FarCryP" => FarCryPrimal.GetHashes(filelist),
            _ => throw new NotImplementedException($"{source.Game.Id}")
        };

        // read files
        source.BinPath = Path.ChangeExtension(source.BinPath, ".dat");
        var entryCount = r.ReadUInt32();
        FileSource[] files; source.Files = files = new FileSource[entryCount]; ulong hash;
        switch (fileVersion) {
            case 1: // NOTUSED
                if (indexIsEncrypted) r = DecryptIndex(r.ReadBytes(entryCount * 24));
                for (var i = 0; i < files.Length; i++) {
                    uint a = r.ReadUInt32(), _ = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(), e = r.ReadUInt32(), _2 = r.ReadUInt32();
                    files[i] = new() {
                        Hash = hash = a,
                        Path = hashes.TryGetValue(hash, out var z) ? z : hash.ToString(),
                        FileSize = (int)((e >> 2) & 0x3FFFFFFFu),
                        Compressed = (byte)((e >> 0) & 0x3u),
                        Offset = (long)(((ulong)d << 2) | ((c >> 30) & 0x3u)),
                        PackedSize = (int)((c >> 0) & 0x3FFFFFFFu),
                    };
                }
                break;
            case 6 or 8: // NOTUSED
                if (indexIsEncrypted) r = DecryptIndex(r.ReadBytes(entryCount * 20));
                for (var i = 0; i < files.Length; i++) {
                    ulong a = r.ReadUInt64(); uint c = r.ReadUInt32(), d = r.ReadUInt32(), e = r.ReadUInt32();
                    files[i] = new() {
                        Hash = hash = a,
                        Path = hashes.TryGetValue(hash, out var z) ? z : hash.ToString(),
                        FileSize = (int)((c >> 2) & 0x3FFFFFFFu),
                        Compressed = (byte)((c >> 0) & 0x3u),
                        Offset = (long)((ulong)(d << 2) | ((e >> 30) & 0x3u)),
                        PackedSize = (int)((e >> 0) & 0x3FFFFFFFul),
                    };
                }
                break;
            case 7: // NOTUSED
                if (indexIsEncrypted) r = DecryptIndex(r.ReadBytes(entryCount * 24));
                for (var i = 0; i < files.Length; i++) {
                    uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), _ = r.ReadUInt32(), e = r.ReadUInt32(), f = r.ReadUInt32();
                    files[i] = new() {
                        Hash = hash = ((ulong)a << 32) | b,
                        Path = hashes.TryGetValue(hash, out var z) ? z : hash.ToString(),
                        FileSize = (int)((c >> 2) & 0x3FFFFFFFu),
                        Compressed = (byte)((c >> 0) & 0x3u),
                        Offset = (long)((ulong)(f << 2) | ((e >> 30) & 0x3u)),
                        PackedSize = (int)((e >> 0) & 0x3FFFFFFFu),
                    };
                }
                break;
            case 5:
                if (indexIsEncrypted) r = DecryptIndex(r.ReadBytes(entryCount * 16));
                for (var i = 0; i < files.Length; i++) {
                    uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32();
                    files[i] = new() {
                        Hash = hash = a,
                        Path = hashes.TryGetValue(hash, out var z) ? z : hash.ToString(),
                        FileSize = (int)((b >> 2) & 0x3FFFFFFFu),
                        Compressed = (byte)((b >> 0) & 0x3u),
                        PackedSize = (int)((c >> 0) & 0x3FFFFFFFu),
                        Offset = (long)(((c >> 30) & 0x3u) | (ulong)d << 2),
                    };
                }
                break;
            case 9:
                if (indexIsEncrypted) r = DecryptIndex(r.ReadBytes(entryCount * 20));
                for (var i = 0; i < files.Length; i++) {
                    uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(), e = r.ReadUInt32();
                    files[i] = new() {
                        Hash = hash = ((ulong)a << 32) | b,
                        Path = hashes.TryGetValue(hash, out var z) ? z : hash.ToString(),
                        FileSize = (int)((c >> 2) & 0x3FFFFFFFu),
                        Compressed = (byte)((c >> 0) & 0x3u),
                        Offset = (long)((ulong)(d << 2) | ((e >> 30) & 0x3u)),
                        PackedSize = (int)((e >> 0) & 0x3FFFFFFFul),
                    };
                }
                break;
            case 10:
                if (indexIsEncrypted) r = DecryptIndex(r.ReadBytes(entryCount * 20));
                for (var i = 0; i < files.Length; i++) {
                    uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(), e = r.ReadUInt32();
                    files[i] = new() {
                        Hash = hash = ((ulong)a << 32) | b,
                        Path = hashes.TryGetValue(hash, out var z) ? z : hash.ToString(),
                        FileSize = (int)((c >> 2) & 0x3FFFFFFFu),
                        Flags = ((c >> 0) & 0x1u) != 0 ? 1 : 0,
                        Compressed = (byte)((c >> 1) & 0x1u),
                        Offset = (long)(((ulong)d << 3) | (e >> 29)),
                        PackedSize = (int)((e >> 0) & 0x1FFFFFFFu),
                    };
                }
                break;
            case 11:
                if (indexIsEncrypted) r = DecryptIndex(r.ReadBytes(entryCount * 20));
                for (var i = 0; i < files.Length; i++) {
                    uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(), e = r.ReadUInt32();
                    files[i] = new() {
                        Hash = hash = ((ulong)a << 32) | b,
                        Path = hashes.TryGetValue(hash, out var z) ? z : hash.ToString(),
                        FileSize = (int)((c >> 2) & 0x3FFFFFFFu),
                        Flags = ((c >> 0) & 0x1u) != 0 ? 1 : 0,
                        Compressed = (byte)((c >> 1) & 0x1u),
                        Offset = (long)(((ulong)d << 7) | ((e >> 25) & 0x70)),
                        PackedSize = (int)((e >> 0) & 0x1FFFFFFFu),
                    };
                }
                break;
            default: throw new ArgumentOutOfRangeException("fileVersion", fileVersion, null);
        }

        // read localization
        var localization = r.ReadL32FArray(_ => (r.ReadL32AString(32), r.ReadUInt64()));
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        r.Seek(file.Offset);
        if (file.Flags != 0) {
            var entrySize = (int)(file.Compressed != 0 ? file.PackedSize : file.FileSize);
            var encryptedSize = Math.Min(0x100000, entrySize);
            encryptedSize &= ~7;
            if (encryptedSize > 0) {
                var data = r.ReadBytes(entrySize);
                var key = MakeXTEAKey((uint)encryptedSize);
                XTEA.Decrypt(data, 0, encryptedSize, key);
                r = new BinaryReader(new MemoryStream(data, false));
            }
        }
        Action<BinaryReader, FileSource, Stream> decompress = (Compression)file.Compressed switch {
            Compression.None => DecompressNone,
            Compression.LZO1x => DecompressLZO1x,
            Compression.Zlib => DecompressZlib,
            Compression.LZ4 => DecompressLZ4,
            _ => throw new NotSupportedException("unsupported compression scheme"),
        };
        var s = (Stream)new MemoryStream();
        decompress(r, file, s);
        s.Position = 0;
        return Task.FromResult(s);
    }

    static void DecompressNone(BinaryReader r, FileSource f, Stream o) {
        r.BaseStream.CopyTo(o, f.FileSize);
    }

    static void DecompressLZO1x(BinaryReader r, FileSource f, Stream o) {
        var compressedBytes = new byte[f.PackedSize];
        if (r.Read(compressedBytes, 0, compressedBytes.Length) != compressedBytes.Length) throw new EndOfStreamException("could not read all compressed bytes");
        var uncompressedBytes = new byte[f.FileSize];
        var result = new Lzo1xDecompressor(compressedBytes, uncompressedBytes).Decompress();
        if (result != 0) throw new InvalidOperationException($"LZO decompression failure ({result})");
        o.Write(uncompressedBytes, 0, uncompressedBytes.Length);
    }

    static void DecompressZlib(BinaryReader r, FileSource f, Stream o) {
        if (f.PackedSize < 16) throw new EndOfStreamException("not enough data for zlib compressed data");
        var sizes = new ushort[8];
        for (var i = 0; i < 8; i++) sizes[i] = r.ReadUInt16();
        var blockCount = sizes[0];
        var maximumUncompressedBlockSize = 16 * (sizes[1] + 1);
        var left = f.FileSize;
        for (int i = 0, c = 2; i < blockCount; i++, c++) {
            if (c == 8) { for (var j = 0; j < 8; j++) sizes[j] = r.ReadUInt16(); c = 0; }
            var compressedBlockSize = sizes[c];
            if (compressedBlockSize != 0) {
                var uncompressedBlockSize = i + 1 < blockCount ? Math.Min(maximumUncompressedBlockSize, left) : left;
                using (var t = new MemoryStream(r.ReadBytes(compressedBlockSize))) {
                    var s = new InflaterInputStream(t, new Inflater(true));
                    s.CopyTo(o, uncompressedBlockSize);
                    left -= uncompressedBlockSize;
                }
                var padding = (16 - (compressedBlockSize % 16)) % 16;
                if (padding > 0) r.Skip(padding);
            }
            else throw new NotImplementedException();
        }
        if (left > 0) throw new InvalidOperationException("did not decompress enough data");
    }

    static void DecompressLZ4(BinaryReader r, FileSource f, Stream o) {
        // K4os.Compression.LZ4 does not currently support partial decoding.
        // https://github.com/MiloszKrajewski/K4os.Compression.LZ4/issues/61
#if K4OS_WAS_FIXED
            var uncompressedSize = f.FileSize;
            var compressedBytes = input.ReadBytes((int)f.PackedSize);
            var uncompressedBytes = new byte[uncompressedSize];
            var decoded = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
            if (decoded != uncompressedSize) throw new InvalidOperationException();
            o.WriteBytes(uncompressedBytes);
#else
        var uncompressedSize = f.FileSize;
        var compressedBytes = r.ReadBytes((int)f.PackedSize);
        var uncompressedBytes = new byte[f.FileSize];
        var decoded = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
        if (decoded != f.FileSize) throw new InvalidOperationException();
        o.Write(uncompressedBytes, 0, (int)uncompressedSize);
#endif
    }
}

#endregion

#region Binary_Cry3

/// <summary>
/// Binary_Cry3
/// </summary>
/// <seealso cref="GameX.Formats.PakBinary" />
public class Binary_Cry3 : ArcBinary<Binary_Cry3> {
    readonly byte[] Key;

    public Binary_Cry3() { }
    public Binary_Cry3(byte[] key = null) => Key = key;

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        source.UseReader = false;
        var files = source.Files = [];

        var arc = (ZipArchiveX)(source.Tag = new ZipArchiveX(r.BaseStream, path: source.BinPath, key: Key, kind: ZipKind.Cry3));
        var parentByPath = new Dictionary<string, FileSource>();
        var partByPath = new Dictionary<string, SortedList<string, FileSource>>();
        foreach (var s in arc.Entries) {
            if (s.FullName.EndsWith("/")) continue;
            var metadata = new FileSource {
                Path = s.FullName.Replace('\\', '/'),
                //Flags = entry.Flags,
                PackedSize = s.CompressedLength,
                FileSize = s.Length,
                Tag = s
            };
            var metadataPath = metadata.Path;
            if (metadataPath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase)) parentByPath.Add(metadataPath, metadata);
            else if (metadataPath.Length > 8 && metadataPath[^8..].Contains(".dds.", StringComparison.OrdinalIgnoreCase)) {
                var parentPath = metadataPath[..(metadataPath.IndexOf(".dds", StringComparison.OrdinalIgnoreCase) + 4)];
                var parts = partByPath.TryGetValue(parentPath, out var z) ? z : null;
                if (parts == null) partByPath.Add(parentPath, parts = []);
                parts.Add(metadataPath, metadata);
                continue;
            }
            files.Add(metadata);
        }

        // process links
        if (partByPath.Count != 0)
            foreach (var kv in partByPath) if (parentByPath.TryGetValue(kv.Key, out var parent)) parent.Parts = kv.Value.Values;
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        var entry = (ZipArchiveEntry)file.Tag;
        try {
            using var input = entry.OpenX();
            //if (!input.CanRead) { HandleException(file, option, $"Unable to read fs for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
            var s = new MemoryStream();
            input.CopyTo(s);
            s.Position = 0;
            return Task.FromResult((Stream)s);
        }
        catch (Exception e) { HandleException(file, option, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
    }

    //public override Task Write(BinaryArchive source, BinaryWriter w, object tag) {
    //    source.UseReader = false;
    //    var files = source.Files;
    //    var arc = (Cry3Archive)(source.Tag = new Cry3Archive(w.BaseStream, source.BinPath, Key));
    //    //arc.BeginUpdate();
    //    foreach (var file in files) {
    //        //var entry = (ZipEntry)(file.Tag = new ZipEntry(Path.GetFileName(file.Path)));
    //        //arc.Add(entry);
    //        source.ArcBinary.WriteData(source, w, file, null);
    //    }
    //    //arc.CommitUpdate();
    //    return Task.CompletedTask;
    //}
    //public override Task WriteData(BinaryArchive source, BinaryWriter w, FileSource file, Stream data, object option = default) {
    //    var arc = (Cry3Archive)source.Tag;
    //    var entry = (ZipEntry)file.Tag;
    //    try {
    //        using var s = arc.GetInputStream(entry);
    //        data.CopyTo(s);
    //    }
    //    catch (Exception e) { HandleException(file, option, $"Exception: {e.Message}"); }
    //    return Task.CompletedTask;
    //}
}

#endregion

#region Binary_CryXml

public class Binary_CryXml : XmlDocument, IHaveMetaInfo, IStream {
    class Node {
        public int NodeId { get; set; }
        public int NodeNameOffset { get; set; }
        public int ItemType { get; set; }
        public short AttributeCount { get; set; }
        public short ChildCount { get; set; }
        public int ParentNodeId { get; set; }
        public int FirstAttributeIndex { get; set; }
        public int FirstChildIndex { get; set; }
        public int Reserved { get; set; }
    }

    public static Task<object> Factory(BinaryReader r, FileSource m, Archive s) => Task.FromResult((object)new Binary_CryXml(r, false));

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
    ];

    public Binary_CryXml(string inFile, bool writeLog = false) : this(new BinaryReader(File.OpenRead(inFile)), writeLog) { }
    public Binary_CryXml(byte[] bytes, bool writeLog = false) : this(new BinaryReader(new MemoryStream(bytes)), writeLog) { }
    public Binary_CryXml(BinaryReader r, bool writeLog = false) {
        var startOffset = (int)r.BaseStream.Position;
        var peek = r.PeekChar();
        if (peek == '<') { Load(r.BaseStream); return; } // File is already XML, so return the XML.
        else if (peek != 'C') throw new Exception("Unknown File Format"); // Unknown file format

        var header = r.ReadFWString(7);
        if (header == "CryXml" || header == "CryXmlB") r.ReadVWString();
        else if (header == "CRY3SDK") r.ReadBytes(2);
        else throw new FormatException("Unknown File Format");

        var headerLength = r.BaseStream.Position;
        var fileLength = r.ReadInt32();
        if (fileLength != r.BaseStream.Length) throw new FormatException("Invalid byte order");

        var nodeTableOffset = r.ReadInt32();
        var nodeTableCount = r.ReadInt32();
        var nodeTableSize = 28;

        var attributeTableOffset = r.ReadInt32();
        var attributeTableCount = r.ReadInt32();
        var attributeTableSize = 8;

        var childTableOffset = r.ReadInt32();
        var childTableCount = r.ReadInt32();
        var childTableSize = 4;

        var stringTableOffset = r.ReadInt32();
        var stringTableCount = r.ReadInt32();

        // NODE TABLE
        if (writeLog) {
            Console.WriteLine("Header");
            Console.WriteLine($"0x{0x00:X6}: {header}");
            Console.WriteLine($"0x{headerLength + 0x00:X6}: {{0:X8}} (Dec: {{0:D8}})", fileLength);
            Console.WriteLine($"0x{headerLength + 0x04:X6}: {{0:X8}} (Dec: {{0:D8}})", nodeTableOffset);
            Console.WriteLine($"0x{headerLength + 0x08:X6}: {{0:X8}} (Dec: {{0:D8}})", nodeTableCount);
            Console.WriteLine($"0x{headerLength + 0x12:X6}: {{0:X8}} (Dec: {{0:D8}})", attributeTableOffset);
            Console.WriteLine($"0x{headerLength + 0x16:X6}: {{0:X8}} (Dec: {{0:D8}})", attributeTableCount);
            Console.WriteLine($"0x{headerLength + 0x20:X6}: {{0:X8}} (Dec: {{0:D8}})", childTableOffset);
            Console.WriteLine($"0x{headerLength + 0x24:X6}: {{0:X8}} (Dec: {{0:D8}})", childTableCount);
            Console.WriteLine($"0x{headerLength + 0x28:X6}: {{0:X8}} (Dec: {{0:D8}})", stringTableOffset);
            Console.WriteLine($"0x{headerLength + 0x32:X6}: {{0:X8}} (Dec: {{0:D8}})", stringTableCount);
            Console.WriteLine("\nNode Table");
        }
        var nodeTable = new List<Node> { };
        r.BaseStream.Seek(nodeTableOffset, SeekOrigin.Begin);
        var nodeId = 0;
        while (r.BaseStream.Position < nodeTableOffset + nodeTableCount * nodeTableSize) {
            var position = r.BaseStream.Position;
            var value = new Node {
                NodeId = nodeId++,
                NodeNameOffset = r.ReadInt32(),
                ItemType = r.ReadInt32(),
                AttributeCount = r.ReadInt16(),
                ChildCount = r.ReadInt16(),
                ParentNodeId = r.ReadInt32(),
                FirstAttributeIndex = r.ReadInt32(),
                FirstChildIndex = r.ReadInt32(),
                Reserved = r.ReadInt32(),
            };
            nodeTable.Add(value);
            if (writeLog) Console.WriteLine($"0x{position:X6}: {value.NodeNameOffset:X8} {value.ItemType:X8} {value.AttributeCount:X4} {value.ChildCount:X4} {value.ParentNodeId:X8} {value.FirstAttributeIndex:X8} {value.FirstChildIndex:X8} {value.Reserved:X8}");
        }

        // ATTRIBUTE TABLE
        if (writeLog) Console.WriteLine("\nAttribute Table");
        var attributeTable = new List<(int NameOffset, int ValueOffset)> { };
        r.BaseStream.Seek(attributeTableOffset, SeekOrigin.Begin);
        while (r.BaseStream.Position < attributeTableOffset + attributeTableCount * attributeTableSize) {
            var position = r.BaseStream.Position;
            var value = (NameOffset: r.ReadInt32(), ValueOffset: r.ReadInt32());
            attributeTable.Add(value);
            if (writeLog) Console.WriteLine($"0x{position:X6}: {value.NameOffset:X8} {value.ValueOffset:X8}");
        }

        // PARENT TABLE
        if (writeLog) Console.WriteLine("\nParent Table");
        var parentTable = new List<int> { };
        r.BaseStream.Seek(childTableOffset, SeekOrigin.Begin);
        while (r.BaseStream.Position < childTableOffset + childTableCount * childTableSize) {
            var position = r.BaseStream.Position;
            var value = r.ReadInt32();
            parentTable.Add(value);
            if (writeLog) Console.WriteLine($"0x{position:X6}: {value:X8}");
        }

        // STRING DICTIONARY
        if (writeLog) Console.WriteLine("\nString Dictionary");

        var dataTable = new List<(int Offset, string Value)> { };
        r.BaseStream.Seek(stringTableOffset, SeekOrigin.Begin);
        while (r.BaseStream.Position < r.BaseStream.Length) {
            var position = r.BaseStream.Position;
            var value = (Offset: (int)position - stringTableOffset, Value: r.ReadVWString());
            dataTable.Add(value);
            if (writeLog) Console.WriteLine($"0x{position:X6}: {value.Offset:X8} {value.Value}");
        }

        var dataMap = dataTable.ToDictionary(k => k.Offset, v => v.Value);
        var attributeIndex = 0;

        // DOCUMENT
        var xmlMap = new Dictionary<int, XmlElement> { };
        foreach (var node in nodeTable) {
            var element = CreateElement(dataMap[node.NodeNameOffset]);
            for (int i = 0, j = node.AttributeCount; i < j; i++) {
                if (dataMap.ContainsKey(attributeTable[attributeIndex].ValueOffset)) element.SetAttribute(dataMap[attributeTable[attributeIndex].NameOffset], dataMap[attributeTable[attributeIndex].ValueOffset]);
                else { element.SetAttribute(dataMap[attributeTable[attributeIndex].NameOffset], "BUGGED"); }
                attributeIndex++;
            }
            xmlMap[node.NodeId] = element;
            if (xmlMap.ContainsKey(node.ParentNodeId)) xmlMap[node.ParentNodeId].AppendChild(element);
            else AppendChild(element);
        }
    }

    public override string ToString() {
        using var s = new MemoryStream();
        using var w = new XmlTextWriter(s, Encoding.Unicode) { Formatting = Formatting.Indented };
        WriteContentTo(w);
        w.Flush();
        s.Flush();
        s.Position = 0;
        return new StreamReader(s).ReadToEnd();
    }

    public static TObject Deserialize<TObject>(Stream inStream) where TObject : class {
        using var s = new MemoryStream();
        var xs = new XmlSerializer(typeof(TObject));
        var xmlDoc = new Binary_CryXml(new BinaryReader(inStream));
        xmlDoc.Save(s);
        s.Seek(0, SeekOrigin.Begin);
        return xs.Deserialize(s) as TObject;
    }

    public static TObject Deserialize<TObject>(string inFile) where TObject : class {
        using var s = new MemoryStream();
        var xmlDoc = new Binary_CryXml(inFile);
        xmlDoc.Save(s);
        s.Seek(0, SeekOrigin.Begin);
        var xs = new XmlSerializer(typeof(TObject));
        return xs.Deserialize(s) as TObject;
    }

    public Stream GetStream() {
        var s = new MemoryStream();
        Save(s);
        s.Seek(0, SeekOrigin.Begin);
        return s;
    }
}

#endregion

#region Binary_CryFile

public partial class Binary_CryFile {
    public static Task<object> Factory(BinaryReader r, FileSource m, Archive s) {
        var file = new Binary_CryFile(m.Path);
        file.LoadFromPak(r.BaseStream, m, s);
        return Task.FromResult((object)file);
    }

    /// <summary>
    /// File extensions processed by CryEngine
    /// </summary>
    static HashSet<string> _validExtensions = new HashSet<string>
    {
        ".soc",
        ".cgf",
        ".cga",
        ".chr",
        ".skin",
        ".anim"
    };

    public Binary_CryFile(string fileName) {
        // Validate file extension - handles .cgam / skinm
        if (!_validExtensions.Contains(Path.GetExtension(fileName))) throw new FileLoadException("Warning: Unsupported file extension - please use a cga, cgf, chr, skin or anim file", fileName);
        InputFile = fileName;
    }

    public void LoadFromFile() {
        var files = new List<(string, Stream)> { (InputFile, File.Open(InputFile, FileMode.Open)) };
        var mFilePath = Path.ChangeExtension(InputFile, $"{Path.GetExtension(InputFile)}m");
        if (File.Exists(mFilePath)) {
            Log.Info($"Found geometry file {Path.GetFileName(mFilePath)}");
            files.Add((mFilePath, File.Open(mFilePath, FileMode.Open))); // Add to list of files to process
        }
        LoadAsync(null, files, FindMaterialFromFile, path => Task.FromResult<(string, Stream)>((path, File.Open(path, FileMode.Open)))).Wait();
    }

    public void LoadFromPak(Stream stream, FileSource metadata, Archive arc) {
        var files = new List<(string, Stream)> { (InputFile, stream) };
        var mFilePath = Path.ChangeExtension(InputFile, $"{Path.GetExtension(InputFile)}m");
        if (arc.Contains(mFilePath)) {
            Log.Info($"Found geometry file {Path.GetFileName(mFilePath)}");
            files.Add((mFilePath, arc.GetData(mFilePath).Result)); // Add to list of files to process
        }
        LoadAsync(arc, files, FindMaterialFromPak, path => Task.FromResult<(string, Stream)>((path, arc.GetData(path).Result))).Wait();
    }

    static string FindMaterialFromFile(Archive arc, string materialPath, string fileName, string cleanName) {
        // First try relative to file being processed
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // Then try just the last part of the chunk, relative to the file being processed
        if (!File.Exists(materialPath)) materialPath = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(cleanName));
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // Then try relative to the ObjectDir
        if (!File.Exists(materialPath)) materialPath = Path.Combine("Sbi", cleanName);
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // Then try just the fileName.mtl
        if (!File.Exists(materialPath)) materialPath = fileName;
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // TODO: Try more paths
        return File.Exists(materialPath) ? materialPath : null;
    }

    static string FindMaterialFromPak(Archive arc, string materialPath, string fileName, string cleanName) {
        // First try relative to file being processed
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // Then try just the last part of the chunk, relative to the file being processed
        if (!arc.Contains(materialPath)) materialPath = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(cleanName));
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // Then try relative to the ObjectDir
        if (!arc.Contains(materialPath)) materialPath = Path.Combine("Sbi", cleanName);
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // Then try just the fileName.mtl
        if (!arc.Contains(materialPath)) materialPath = fileName;
        if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
        // TODO: Try more paths
        return arc.Contains(materialPath) ? materialPath : null;
    }

    public async Task LoadAsync(Archive arc, IEnumerable<(string, Stream)> files, Func<Archive, string, string, string, string> getMaterialPath, Func<string, Task<(string, Stream)>> getFileAsync) {
        try {
            Models = new List<Model> { };
            foreach (var file in files) {
                // Each file (.cga and .cgam if applicable) will have its own RootNode.  This can cause problems.  .cga files with a .cgam files won't have geometry for the one root node.
                var model = new Model(file);
                if (RootNode == null) RootNode = model.RootNode; // This makes the assumption that we read the .cga file before the .cgam file.
                Bones ??= model.Bones;
                Models.Add(model);
            }
            SkinningInfo = ConsolidateSkinningInfo();
            // For eanch node with geometry info, populate that node's Mesh Chunk GeometryInfo with the geometry data.
            ConsolidateGeometryInfo();
            // Get the material file name
            var fileName = files.First().Item1;
            foreach (ChunkMtlName mtlChunk in Models.SelectMany(a => a.ChunkMap.Values).Where(c => c.ChunkType == ChunkType.MtlName)) {
                // Don't process child or collision materials for now
                if (mtlChunk.MatType == MtlNameType.Child || mtlChunk.MatType == MtlNameType.Unknown1) continue;
                // The Replace part is for SC files that point to a _core material file that doesn't exist.
                var cleanName = mtlChunk.Name.Replace("_core", string.Empty);
                //
                string materialFilePath;
                if (mtlChunk.Name.Contains("default_body")) {
                    // New MWO models for some crazy reason don't put the actual mtl file name in the mtlchunk.  They just have /objects/mechs/default_body
                    // have to assume that it's /objects/mechs/<mechname>/body/<mechname>_body.mtl.  There is also a <mechname>.mtl that contains mtl 
                    // info for hitboxes, but not needed.
                    // TODO:  This isn't right.  Fix it.
                    var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                    if (charsToClean.Length > 0) foreach (char character in charsToClean) cleanName = cleanName.Replace(character.ToString(), string.Empty);
                    materialFilePath = Path.Combine(Path.GetDirectoryName(fileName), cleanName);
                }
                else if (mtlChunk.Name.Contains("/") || mtlChunk.Name.Contains("\\")) {
                    // The mtlname has a path.  Most likely starts at the Objects directory.
                    var stringSeparators = new[] { "/", "\\" };
                    // if objectdir is provided, check objectdir + mtlchunk.name
                    materialFilePath = Path.Combine("Sbi", mtlChunk.Name);
                    //else // object dir not provided, but we have a path.  Just grab the last part of the name and check the dir of the cga file
                    //{
                    //    var r = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                    //    materialFilePath = r[r.Length - 1];
                    //}
                }
                else {
                    var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                    if (charsToClean.Length > 0) foreach (var character in charsToClean) cleanName = cleanName.Replace(character.ToString(), string.Empty);
                    materialFilePath = Path.Combine(Path.GetDirectoryName(fileName), cleanName);
                }
                // Populate CryEngine_Core.Material
                var materialPath = getMaterialPath(arc, materialFilePath, fileName, cleanName);
                var material = materialPath != null ? Material.FromFile(await getFileAsync(materialPath)) : null;
                if (material != null) {
                    Log.Info($"Located material file {Path.GetFileName(materialPath)}");
                    Materials = FlattenMaterials(material).Where(m => m.Textures != null).ToArray();
                    // only one material, so it's a material file with no submaterials.  Check and set the name
                    if (Materials.Length == 1) Materials[0].Name = RootNode.Name;
                    return; // Early return - we have the material map
                }
                else Log.Info($"Unable to locate material file {mtlChunk.Name}.mtl");
            }
            Log.Info("Unable to locate any material file");
            Materials = new Material[0];
        }
        catch (Exception e) {
            throw e;
        }
    }

    void ConsolidateGeometryInfo() {
        //foreach (Model model in Models)
        //{
        //    var nodes = model.ChunkNodes;
        //}
    }

    SkinningInfo ConsolidateSkinningInfo() {
        var skin = new SkinningInfo();
        foreach (var model in Models) {
            skin.HasSkinningInfo = Models.Any(a => a.SkinningInfo.HasSkinningInfo);
            skin.HasBoneMapDatastream = Models.Any(a => a.SkinningInfo.HasBoneMapDatastream);
            if (model.SkinningInfo.IntFaces != null) skin.IntFaces = model.SkinningInfo.IntFaces;
            if (model.SkinningInfo.IntVertices != null) skin.IntVertices = model.SkinningInfo.IntVertices;
            if (model.SkinningInfo.LookDirectionBlends != null) skin.LookDirectionBlends = model.SkinningInfo.LookDirectionBlends;
            if (model.SkinningInfo.MorphTargets != null) skin.MorphTargets = model.SkinningInfo.MorphTargets;
            if (model.SkinningInfo.PhysicalBoneMeshes != null) skin.PhysicalBoneMeshes = model.SkinningInfo.PhysicalBoneMeshes;
            if (model.SkinningInfo.BoneEntities != null) skin.BoneEntities = model.SkinningInfo.BoneEntities;
            if (model.SkinningInfo.BoneMapping != null) skin.BoneMapping = model.SkinningInfo.BoneMapping;
            if (model.SkinningInfo.Collisions != null) skin.Collisions = model.SkinningInfo.Collisions;
            if (model.SkinningInfo.CompiledBones != null) skin.CompiledBones = model.SkinningInfo.CompiledBones;
            if (model.SkinningInfo.Ext2IntMap != null) skin.Ext2IntMap = model.SkinningInfo.Ext2IntMap;
        }
        return skin;
    }

    /// <summary>
    /// There will be one Model for each model in this object.  
    /// </summary>
    public List<Model> Models { get; internal set; }
    public Material[] Materials { get; internal set; }
    public ChunkNode RootNode { get; internal set; }
    public ChunkCompiledBones Bones { get; internal set; }
    public SkinningInfo SkinningInfo { get; set; }
    public string InputFile { get; internal set; }
    public string Name => Path.GetFileName(InputFile);

    Chunk[] _chunks;
    public Chunk[] Chunks {
        get {
            if (_chunks == null) _chunks = Models.SelectMany(m => m.ChunkMap.Values).ToArray();
            return _chunks;
        }
    }

    // Cannot use the Node name for the key.  Across a couple files, you may have multiple nodes with same name.
    public Dictionary<string, ChunkNode> _nodeMap;
    public Dictionary<string, ChunkNode> NodeMap {
        get {
            if (_nodeMap == null) {
                _nodeMap = new Dictionary<string, ChunkNode>(StringComparer.InvariantCultureIgnoreCase) { };
                ChunkNode rootNode = null;
                //Log.Info("Mapping Nodes");
                foreach (var model in Models) {
                    model.RootNode = rootNode ??= model.RootNode; // Each model will have it's own rootnode.
                    foreach (var node in model.ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).Select(c => c as ChunkNode)) {
                        // Preserve existing parents
                        if (_nodeMap.ContainsKey(node.Name)) {
                            var parentNode = _nodeMap[node.Name].ParentNode;
                            if (parentNode != null) parentNode = _nodeMap[parentNode.Name];
                            node.ParentNode = parentNode;
                        }
                        _nodeMap[node.Name] = node; // TODO:  fix this.  The node name can conflict.
                    }
                }
            }
            return _nodeMap;
        }
    }

    /// <summary>
    /// Flatten all child materials into a one dimensional list
    /// </summary>
    /// <param name="material"></param>
    /// <returns></returns>
    public static IEnumerable<Material> FlattenMaterials(Material material) {
        if (material != null) {
            yield return material;
            if (material.SubMaterials != null)
                foreach (var subMaterial in material.SubMaterials.SelectMany(m => FlattenMaterials(m)))
                    yield return subMaterial;
        }
    }

    public IEnumerable<string> GetTexturePaths() {
        foreach (var texture in Materials.SelectMany(x => x.Textures))
            if (!string.IsNullOrEmpty(texture.File))
                yield return $@"Sbi\{texture.File}";
    }
}

#endregion
