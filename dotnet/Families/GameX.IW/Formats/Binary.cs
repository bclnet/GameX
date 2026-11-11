using GameX.Blizzard.Formats.Casc;
using ICSharpCode.SharpZipLib.Zip;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Compression;
using static GameX.IW.Formats.Binary_Iwi.FORMAT;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace GameX.IW.Formats;

#region Binary_Abc

public class Binary_Abc : IHaveMetaInfo {
    public Binary_Abc() { }
    public Binary_Abc(BinaryReader r) => Read(r);

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("BinaryPak", items: [
            //new($"Type: {Type}"),
        ])
    ];

    public unsafe void Read(BinaryReader r)
        => throw new NotImplementedException();
}

#endregion

#region Binary_D3dBsp
// https://github.com/SE2Dev/D3DBSP_Converter

public class Binary_D3dBsp : IHaveMetaInfo {
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "UnknownFileModel", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Gsc
// https://github.com/SE2Dev/gsc_parser

public class Binary_Gsc : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Gsc());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "GSC", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_IW

public unsafe class Binary_IW : ArcBinary<Binary_IW> {
    CascContext casc;

    //class XSUB_Archive : BinaryArchive
    //{
    //    public XSUB_Archive(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, Instance, tag) { Open(); }
    //}

    enum Magic {
        CASC,
        IWD,
        FF,
        PAK,
        IPAK,
        XPAK,
        XSUB,
    }

    // Headers : FF
    #region Headers : FF

    internal enum FF_MAGIC : uint {
        IWff = 0x66665749, // IWff
        S1ff = 0x66663153, // S1ff
        TAff = 0x66664154, // TAff
    }

    internal enum FF_FORMAT : uint {
        U100 = 0x30303175, // u100
        A100 = 0x30303161, // a100
        _100 = 0x30303130, // 0100
        _000 = 0x30303030, // 0000
    }

    internal enum FF_VERSION : uint {
        // IW X.0 : 2017 - Call of Duty: WWII
        CO4_WWII = 0x0005,  // IW 3.0 : 2007 - Call of Duty 4: Modern Warfare
        WaW = 0x0183,       // IW 3.0+: 2008 - Call of Duty: World at War
        MW2 = 0x0114,       // IW 4.0 : 2009 - Call of Duty: Modern Warfare 2
        BO = 0x01d9,        // IW 3.0 : 2010 - Call of Duty: Black Ops
        MW3 = 0x0001,       // IW 5.0 : 2011 - Call of Duty: Modern Warfare 3
        BO2 = 0x0093,       // IW 3.0m: 2012 - Call of Duty: Black Ops II
        Ghosts = 0x0235,    // IW X.0 : 2013 - Call of Duty: Ghosts
        AW = 0x072e,        // IW X.0 : 2014 - Call of Duty: Advanced Warfare
        BO3 = 0x0251,       // IW 3.0m: 2015 - Call of Duty: Black Ops III
        IW = 0x0653,        // IW X.0 : 2016 - Call of Duty: Infinite Warfare
        BO4 = 0x0,          // IW X.0 : 2018 - Call of Duty: Black Ops 4
        MW = 0x0042,        // IW X.0 : 2019 - Call of Duty: Modern Warfare
        BOCW = 0x0,         // IW X.0 : 2020 - Call of Duty: Black Ops Cold War
        Vanguard = 0x0,     // IW X.0 : 2021 - Call of Duty: Vanguard
        COD_MW2 = 0x0017,   // IW X.0: Call of Duty: Modern Warfare II
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FF_Header {
        public static (string, int) Struct = ("<?", sizeof(FF_Header));
        [MarshalAs(UnmanagedType.U4)] public FF_MAGIC Magic;
        [MarshalAs(UnmanagedType.U4)] public FF_FORMAT Format;
        [MarshalAs(UnmanagedType.U4)] public FF_VERSION Version;
    }

    #endregion

    // Headers : IPAK (Black Ops 2)
    #region Headers : IPAK

    const uint IPAK_MAGIC = 0x12345678;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IPAK_Header {
        public static (string, int) Struct = ("<?", sizeof(IPAK_Header));
        public uint Magic;
        public uint Version;
        public uint Size;
        public uint SegmentCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IPAK_Segment {
        public static (string, int) Struct = ("<?", sizeof(IPAK_Segment));
        public uint Type;
        public uint Offset;
        public uint Size;
        public uint EntryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IPAK_DataHeader {
        public static (string, int) Struct = ("<?", sizeof(IPAK_DataHeader));
        public uint OffsetCount; // Count and offset are packed into a single integer
        public fixed uint Commands[31]; // The commands tell what each block of data does
        public uint Offset => OffsetCount << 8;
        public byte Count => (byte)(OffsetCount & 0xFF);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IPAK_Entry {
        public static (string, int) Struct = ("<?", sizeof(IPAK_Entry));
        public ulong Key;
        public uint Offset;
        public uint Size;
    }

    #endregion

    // Headers : XPAK (Black Ops 3)
    #region Headers : XPAK

    const uint XPAK_MAGIC = 0x4950414b;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct XPAK_Header //: BO3XPakHeader, VGXPAKHeader
    {
        public static (string, int) Struct = ("<?", sizeof(XPAK_Body));
        public uint Magic; // KAPI / IPAK
        public ushort Zero;
        public ushort Version;
        public ulong Unknown2;
        //public ulong Type;
        ///*16*/ public ulong Size;
        //public fixed byte UnknownHashes[1896];
        //public ulong FileCount;     /*24*/
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct XPAK_Body //: BO3XPakHeader, VGXPAKHeader
    {
        public static (string, int) Struct = ("<?", sizeof(XPAK_Body));
        public ulong DataOffset;    /*00*/
        public ulong DataSize;      /*08*/
        public ulong HashCount;     /*16*/
        public ulong HashOffset;    /*24*/
        public ulong HashSize;      /*32*/
        public ulong Unknown3;      /*40*/
        public ulong UnknownOffset; /*48*/
        public ulong Unknown4;      /*56*/
        public ulong IndexCount;    /*64*/
        public ulong IndexOffset;   /*72*/
        public ulong IndexSize;     /*80*/
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct XPAK_HeaderVG //: VGXPAKHeader
    {
        public static (string, int) Struct = ("<?", sizeof(XPAK_HeaderVG));
        public uint Magic; // KAPI / IPAK
        public ushort Zero;
        public ushort Version;
        public ulong Unknown2;
        public ulong Type;
        public ulong Size;
        public fixed byte UnknownHashes[1896];

        public ulong FileCount;
        public ulong DataOffset;
        public ulong DataSize;
        public ulong HashCount;
        public ulong HashOffset;
        public ulong HashSize;
        public ulong Unknown3;
        public ulong UnknownOffset;
        public ulong Unknown4;
        public ulong IndexCount;
        public ulong IndexOffset;
        public ulong IndexSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct XPAK_HashEntry //: BO3XPakHashEntry
    {
        public static (string, int) Struct = ("<?", sizeof(XPAK_HashEntry));
        public ulong Key;
        public ulong Offset;
        public ulong Size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct XPAK_HashEntryVG //: VGXPAKHashEntry
    {
        public static (string, int) Struct = ("<?", sizeof(XPAK_HashEntryVG));
        public ulong Key;
        public ulong PackedInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct XPAK_DataHeader //: BO3XPakDataHeader
    {
        public static (string, int) Struct = ("<?", sizeof(XPAK_DataHeader));
        public uint Offset;
        public uint Count;
        public fixed uint Commands[31]; // The commands tell what each block of data does
    }

    #endregion

    // Headers : WWII (WWII)
    #region Headers : WWII

    const uint WWII_MAGIC = 0x12345678;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WWII_Header {
        public static (string, int) Struct = ("<?", sizeof(WWII_Header));
        public ulong Magic;
        public uint Version;
        public uint EntriesCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WWII_Segment {
        public static (string, int) Struct = ("<?", sizeof(WWII_Segment));
        public fixed byte Hash[16];
        public ulong Offset;
        public uint Size;
        public ushort PackageIndex;
    }

    #endregion

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        var files = source.Files = [];
        var extension = Path.GetExtension(source.BinPath);

        switch (source.Game.Id) {
            case "BO4":
            case "BOCW":
            case "Vanguard":
                source.Magic = (int)Magic.CASC;
                var editions = source.Game.Editions;
                var product = editions.First().Key;
                casc = new CascContext();
                casc.Read(source.BinPath, product, files);
                return Task.CompletedTask;
        }

        switch (extension) {
            // PK3, IWD
            case ".pk3" or ".iwd": {
                    source.UseReader = false;
                    source.Magic = (int)Magic.IWD;
                    var arc = (ZipFile)(source.Tag = new ZipFile(r.BaseStream));
                    foreach (ZipEntry entry in arc)
                        if (entry.Size != 0)
                            files.Add(new FileSource {
                                Path = entry.Name.Replace('\\', '/'),
                                Flags = entry.Flags,
                                PackedSize = entry.CompressedSize,
                                FileSize = entry.Size,
                                Tag = entry,
                            });
                    return Task.CompletedTask;
                }
            // FF
            case ".ff": {
                    source.Magic = (int)Magic.FF;
                    var header = r.ReadS<FF_Header>();
                    if (header.Magic != FF_MAGIC.IWff && header.Magic != FF_MAGIC.S1ff && header.Magic != FF_MAGIC.TAff) throw new FormatException($"Bad magic {header.Magic}");
                    if (header.Format != FF_FORMAT.U100 && header.Format != FF_FORMAT._100 && header.Format != FF_FORMAT.A100 && header.Format != FF_FORMAT._000) throw new FormatException($"Bad format {header.Format}");

                    var listed = ListFastFile(r, header);
                    if (listed is { } z) {
                        source.Tag = z.zone;
                        foreach (var asset in z.files)
                            files.Add(new FileSource {
                                Id = asset.Id,
                                Path = asset.Path,
                                Offset = asset.Position,
                                FileSize = asset.FileSize,
                                Data = asset.Data,
                            });
                    }
                    return Task.CompletedTask;
                }
            // PAK
            case ".pak": {
                    source.Magic = (int)Magic.PAK;
                    return Task.CompletedTask;
                }
            // IPAK
            case ".ipak": {
                    source.Magic = (int)Magic.IPAK;
                    return Task.CompletedTask;
                }
            // XPAK
            case ".xpak": {
                    source.Magic = (int)Magic.XPAK;
                    var header = r.ReadS<XPAK_Header>();
                    // Verify the magic and offset
                    if (header.Magic != XPAK_MAGIC) throw new FormatException("Bad magic");
                    var type = header.Version == 0x1 ? r.ReadUInt64() : 0;
                    var size = r.ReadUInt64();
                    var unknownHashes = header.Version == 0x1 ? r.ReadBytes(1896) : null;
                    var fileCount = r.ReadUInt64();
                    if (header.Version == 0xD) r.Skip(288); // If MW4 we need to skip the new bytes
                    var body = r.ReadS<XPAK_Body>();
                    // Verify the magic and offset
                    if (body.HashOffset >= (ulong)r.BaseStream.Length) throw new FormatException("Bad magic");

                    // Jump to hash offset
                    r.Seek((long)body.IndexOffset);
                    //var indexHeader = r.ReadBytes(16); //<XPAK_DataHeader>(sizeof(XPAK_HashEntry), (int)header.HashCount);
                    //var abc = r.ReadCString(); //<XPAK_DataHeader>(sizeof(XPAK_HashEntry), (int)header.HashCount);

                    // Read hash entries
                    r.Seek((long)body.HashOffset);
                    var entries = r.ReadSArray<XPAK_HashEntry>((int)body.HashCount);
                    for (var i = 0; i < (int)body.HashCount; i++) {
                        // Read it
                        ref XPAK_HashEntry entry = ref entries[i];
                        files.Add(new FileSource {
                            Id = (int)entry.Key,
                            Path = entry.Key.ToString(),
                            Offset = (long)(body.DataOffset + entry.Offset), //: Offset
                            PackedSize = (long)(entry.Size & 0xFFFFFFFFFFFFFF), //: CompressedSize, 0x80 in last 8 bits in some entries in new XPAKs
                            FileSize = 0, //: UncompressedSize
                            Tag = entry,
                        });
                    }
                    return Task.CompletedTask;
                }
            default: return Task.CompletedTask;
        }
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        switch ((Magic)source.Magic) {
            case Magic.CASC:
                return Task.FromResult(casc.ReadData(file));
            case Magic.IWD: {
                    var arc = (ZipFile)source.Tag;
                    var entry = (ZipEntry)file.Tag;
                    using var input = arc.GetInputStream(entry);
                    if (!input.CanRead) { HandleException(file, option, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                    var s = new MemoryStream();
                    input.CopyTo(s);
                    s.Position = 0;
                    return Task.FromResult((Stream)s);
                }
            case Magic.FF: {
                    if (file.Data != null) return Task.FromResult((Stream)new MemoryStream(file.Data, false));
                    if (source.Tag is byte[] zone && file.FileSize > 0)
                        return Task.FromResult((Stream)new MemoryStream(zone, (int)file.Offset, (int)file.FileSize, false));
                    return Task.FromResult((Stream)new MemoryStream());
                }
            default: throw new NotImplementedException();
        }
    }

    #region FastFile
    internal class FileHeader {
        public int Id;
        public string Path;
        public long Position;
        public long FileSize;
        public byte[] Data;
    }

    // Titles that are not one of these stay empty. GetZoneFile is in FastFileUnused.cs and stays commented: it writes a sibling .ff~.
    internal static (byte[] zone, List<FileHeader> files)? ListFastFile(BinaryReader r, FF_Header header) {
        if (header.Magic == FF_MAGIC.IWff && header.Format == FF_FORMAT.U100 && (header.Version == FF_VERSION.CO4_WWII || header.Version == FF_VERSION.WaW || header.Version == FF_VERSION.BO)) return List32(r, header);
        if (header.Magic == FF_MAGIC.IWff && (header.Version == FF_VERSION.MW2 || header.Version == FF_VERSION.MW3 || (header.Format == FF_FORMAT.U100 && header.Version == FF_VERSION.Ghosts))) return List64(r, header);
        if (header.Magic == FF_MAGIC.TAff && header.Format == FF_FORMAT._100 && header.Version == FF_VERSION.BO2) return ListBo(r, false);
        if (header.Magic == FF_MAGIC.TAff && header.Format == FF_FORMAT._000 && header.Version == FF_VERSION.BO3) return ListBo(r, true);
        return null;
    }

    static string Iw3Name(int type) => type >= 0 && type < (int)FastFile.IW3XAssetType.max ? ((FastFile.IW3XAssetType)type).ToString() : $"type{type}";
    static string Iw4Name(int type) => type >= 0 && type < (int)FastFile.IW4XAssetType.max ? ((FastFile.IW4XAssetType)type).ToString() : $"type{type}";
    static string Iw5Name(int type) => type >= 0 && type < (int)FastFile.IW5XAssetType.max ? ((FastFile.IW5XAssetType)type).ToString() : $"type{type}";
    static string OthName(int type) => type >= 0 && type < (int)FastFile.Oth_AssetType.Max && Enum.IsDefined(typeof(FastFile.Oth_AssetType), type) ? ((FastFile.Oth_AssetType)type).ToString() : $"type{type}";

    // Nameless index rows are type/index.ext, the same shape as Valve WAD lumps and Origin UO.
    // The extension is the asset-factory key, three letters when the type name is longer.
    // A name that already has one (rawfile, BO harvest) is left alone.
    // image is .iwi and scriptfile is .gsc, the extensions Binary_Iwi and Binary_Gsc read.
    // Oth names without underscores share the IW code (PhysColMap and phys_collmap are both .pcm).
    static string AssetExt(string typeName) {
        var key = typeName.ToLowerInvariant().Replace("_", "");
        return key switch {
            "image" => ".iwi",
            "scriptfile" => ".gsc",
            "xmodelpieces" => ".xmp",
            "physpreset" => ".phy",
            "xanim" => ".xan",
            "xmodel" => ".xmd",
            "material" => ".mat",
            "techset" => ".tec",
            "sound" => ".snd",
            "sndcurve" => ".crv",
            "loadedsound" => ".lds",
            "colmapsp" => ".csp",
            "colmapmp" => ".cmp",
            "commap" => ".cmm",
            "gamemapsp" => ".gsp",
            "gamemapmp" => ".gmp",
            "mapents" => ".ent",
            "gfxmap" => ".gfx",
            "lightdef" => ".lgt",
            "uimap" => ".uim",
            "font" => ".fnt",
            "menufile" => ".mnf",
            "menu" => ".mnu",
            "localize" => ".loc",
            "weapon" => ".wep",
            "snddriverglobals" => ".sdg",
            "fx" => ".fx",
            "impactfx" => ".ifx",
            "aitype" => ".ait",
            "mptype" => ".mpt",
            "character" => ".chr",
            "xmodelalias" => ".xma",
            "rawfile" => ".raw",
            "stringtable" => ".tbl",
            "physcolmap" => ".pcm",
            "physcollmap" => ".pcm",
            "xmodelsurfs" => ".xms",
            "pixelshader" => ".psh",
            "vertexshader" => ".vsh",
            "vertexdecl" => ".vdl",
            "fxmap" => ".fxm",
            "leaderboarddef" => ".lbd",
            "structureddatadef" => ".sdd",
            "tracer" => ".trc",
            "vehicle" => ".veh",
            "vehical" => ".veh",
            "addonmapents" => ".ame",
            "glassmap" => ".gls",
            "aipaths" => ".aip",
            "vehicletracks" => ".vtr",
            "attachment" => ".att",
            "surfacefx" => ".sfx",
            _ => key.Length <= 3 ? "." + key : "." + key.Substring(0, 3),
        };
    }
        static string IndexPath(string typeName, int i) => $"{typeName}/{i:D4}{AssetExt(typeName)}";

    static byte[] InflateZlib(Stream stream) {
        using (var deflate = new DeflateStream(stream, CompressionMode.Decompress, true))
        using (var ms = new MemoryStream()) { deflate.CopyTo(ms); return ms.ToArray(); }
    }

    static byte[] InflateAt(BinaryReader r, int offset) {
        r.BaseStream.Position = offset;
        var cmf = r.ReadByte();
        var flg = r.ReadByte();
        if (cmf != 0x78 || (flg != 0x01 && flg != 0x5E && flg != 0x9C && flg != 0xDA)) throw new FormatException($"Fastfile is not zlib ({cmf:X2}{flg:X2})");
        return InflateZlib(r.BaseStream);
    }

    // Signed IW4/IW5 PC: IWffs100 at 0x15, then the zlib stream at 0x4015.
    // Data is 8KB chunks, 256 of them, then one hash chunk that is not part of the stream.
    static byte[] InflateAuthed(BinaryReader r) {
        r.BaseStream.Position = 0;
        var file = r.ReadBytes((int)r.BaseStream.Length);
        if (file.Length < 0x4017 || file[0x4015] != 0x78) throw new FormatException("Signed fastfile has no zlib stream");
        using (var packed = new MemoryStream()) {
            var pos = 0x4015;
            var dataChunks = 0;
            while (pos < file.Length) {
                if (dataChunks == 256) { pos += 0x2000; dataChunks = 0; continue; }
                var n = Math.Min(0x2000, file.Length - pos);
                packed.Write(file, pos, n);
                pos += n;
                dataChunks++;
            }
            packed.Position = 2;
            return InflateZlib(packed);
        }
    }

    // COD4, WaW, and Black Ops. Zlib at byte 12. COD4 is script strings, then (type, pointer).
    // WaW and Black Ops are (pointer, type) at zone 0x30. A type outside the enum lists nothing.
    static (byte[] zone, List<FileHeader> files) List32(BinaryReader r, FF_Header header) {
        if (header.Version == FF_VERSION.CO4_WWII) {
            var cmf = r.ReadByte();
            var flg = r.ReadByte();
            if (cmf != 0x78 || (flg != 0x01 && flg != 0x5E && flg != 0x9C && flg != 0xDA)) throw new FormatException($"COD4 fastfile is not zlib ({cmf:X2}{flg:X2})");
            byte[] zone;
            using (var deflate = new DeflateStream(r.BaseStream, CompressionMode.Decompress, true))
            using (var ms = new MemoryStream()) { deflate.CopyTo(ms); zone = ms.ToArray(); }
            if (zone.Length < 0x3C) throw new FormatException($"COD4 zone is {zone.Length} bytes");
            var zr = new BinaryReader(new MemoryStream(zone));
            zr.Seek(0x2C);
            var stringCount = zr.ReadInt32();
            var stringPtr = zr.ReadInt32();
            var assetCount = zr.ReadInt32();
            var assetPtr = zr.ReadInt32();
            if (stringCount < 0 || assetCount < 0 || stringCount > 1_000_000 || assetCount > 1_000_000) throw new FormatException($"COD4 index counts {stringCount}/{assetCount}");
            var files = new List<FileHeader>(assetCount + 1);
            if (stringCount > 0) {
                if (stringPtr != -1) throw new FormatException($"COD4 script strings are not inline ({stringPtr:X8})");
                var ptrs = new int[stringCount];
                for (var i = 0; i < stringCount; i++) ptrs[i] = zr.ReadInt32();
                var text = new System.Text.StringBuilder();
                for (var i = 0; i < stringCount; i++) {
                    var ptr = ptrs[i];
                    if (ptr == -1) {
                        var name = zr.ReadVAString();
                        if (name.Length > 0) text.Append(i).Append('\t').Append(name).Append('\n');
                    }
                    else if (ptr != 0) throw new FormatException($"COD4 script string pointer {ptr:X8}");
                }
                if (text.Length > 0) {
                    var data = System.Text.Encoding.ASCII.GetBytes(text.ToString());
                    files.Add(new FileHeader { Id = -1, Path = "scriptstrings.txt", FileSize = data.Length, Data = data });
                }
            }
            if (assetCount > 0) {
                if (assetPtr != -1) throw new FormatException($"COD4 asset list is not inline ({assetPtr:X8})");
                for (var i = 0; i < assetCount; i++) {
                    var pos = zr.BaseStream.Position;
                    var type = zr.ReadInt32();
                    zr.ReadInt32();
                    files.Add(new FileHeader { Id = i, Path = IndexPath(Iw3Name(type), i), Position = pos, FileSize = 8 });
                }
            }
            return (zone, files);
        }
        Func<int, string> nameOf;
        int maxType;
        if (header.Version == FF_VERSION.WaW) { nameOf = Iw3Name; maxType = (int)FastFile.IW3XAssetType.max; }
        else { nameOf = OthName; maxType = (int)FastFile.Oth_AssetType.Max; }
        var zone2 = InflateAt(r, 12);
        if (zone2.Length < 0x30) return (zone2, new List<FileHeader>());
        var count = BitConverter.ToInt32(zone2, 0x2C);
        if (count <= 0 || count > 1_000_000 || 0x30 + (long)count * 8 > zone2.Length) return (zone2, new List<FileHeader>());
        var rows = new List<FileHeader>(count);
        for (var i = 0; i < count; i++) {
            var pos = 0x30 + i * 8;
            var type = BitConverter.ToInt32(zone2, pos + 4);
            // code_post stores asset bodies in this array, so the "type" is not an enum value.
            if ((uint)type >= (uint)maxType) return (zone2, new List<FileHeader>());
            rows.Add(new FileHeader { Id = i, Path = IndexPath(nameOf(type), i), Position = pos, FileSize = 8 });
        }
        return (zone2, rows);
    }

    // Ghosts nil pointer. The rawfile record is the same 24 bytes as MW2, with this sentinel instead of -1.
    static readonly long GhostsNil = unchecked((long)0xFDFDFDFFFFFFFFFFUL);

    // MW2 and MW3 share the 16-byte index. When every type is rawfile, the 24-byte record follows.
    // Ghosts uses that record with GhostsNil and scans for runs instead of trusting a count.
    static (byte[] zone, List<FileHeader> files) List64(BinaryReader r, FF_Header header) {
        if (header.Version == FF_VERSION.Ghosts) {
            var ghosts = InflateAt(r, 0x2C);
            return (ghosts, ScanRaw(ghosts, GhostsNil));
        }
        int countAt, recAt, rawType, maxType;
        Func<int, string> nameOf;
        if (header.Version == FF_VERSION.MW2) {
            countAt = 0x38; recAt = 0x40; rawType = (int)FastFile.IW4XAssetType.rawfile; maxType = (int)FastFile.IW4XAssetType.max; nameOf = Iw4Name;
        }
        else {
            countAt = 0x3C; recAt = 0x44; rawType = (int)FastFile.IW5XAssetType.rawfile; maxType = (int)FastFile.IW5XAssetType.max; nameOf = Iw5Name;
        }
        byte[] zone;
        if (header.Format == FF_FORMAT.U100) zone = InflateAt(r, 0x15);
        else if (header.Format == FF_FORMAT._100) zone = InflateAuthed(r);
        else return (null, new List<FileHeader>());
        if (zone.Length < countAt + 4) return (zone, new List<FileHeader>());
        var count = BitConverter.ToInt32(zone, countAt);
        if (count <= 0 || count > 1_000_000 || recAt + (long)count * 16 > zone.Length) return (zone, new List<FileHeader>());
        var files = new List<FileHeader>(count);
        var allRaw = true;
        for (var i = 0; i < count; i++) {
            var pos = recAt + i * 16;
            var type = BitConverter.ToInt64(zone, pos + 8);
            if (type < 0 || type >= maxType) return (zone, new List<FileHeader>());
            if (type != rawType) allRaw = false;
            files.Add(new FileHeader { Id = i, Path = IndexPath(nameOf((int)type), i), Position = pos, FileSize = 16 });
        }
        if (allRaw) {
            var body = recAt + count * 16;
            if (body + 8 <= zone.Length && BitConverter.ToInt64(zone, body) == -1) body += 8;
            if (TryWalkAll(zone, body, count, -1, out var named)) return (zone, named);
        }
        return (zone, files);
    }

    static bool TryRaw(byte[] z, int pos, long nil, out string name, out byte[] data, out int next) {
        name = null; data = null; next = pos;
        if (pos < 0 || pos + 24 > z.Length) return false;
        var p1 = BitConverter.ToInt64(z, pos);
        var cLen = BitConverter.ToInt32(z, pos + 8);
        var len = BitConverter.ToInt32(z, pos + 12);
        var p2 = BitConverter.ToInt64(z, pos + 16);
        if (p1 != nil || p2 != nil || cLen < 0 || len < 0 || cLen > z.Length - pos) return false;
        var s = pos + 24;
        var e = s;
        while (e < z.Length && z[e] != 0) {
            if (z[e] < 32 || z[e] > 126) return false;
            e++;
            if (e - s > 120) return false;
        }
        if (e >= z.Length || e == s) return false;
        name = System.Text.Encoding.ASCII.GetString(z, s, e - s);
        var p = e + 1;
        if ((p & 1) != 0 && p < z.Length && z[p] == 0) p++;
        else if (cLen == 0 && p + 1 < z.Length && z[p] == 0 && z[p + 1] == 0xFF) p++;
        if (p > z.Length || cLen > z.Length - p) return false;
        if (cLen > 0 && z[p] != 0x78) return false;
        if (cLen == 0) data = Array.Empty<byte>();
        else {
            try {
                using (var ms = new MemoryStream(z, p + 2, cLen - 2, false)) data = InflateZlib(ms);
            }
            catch (InvalidDataException) { return false; }
        }
        next = p + cLen;
        return data != null;
    }

    static bool TryWalkAll(byte[] z, int pos, int count, long nil, out List<FileHeader> files) {
        files = new List<FileHeader>(count);
        for (var i = 0; i < count; i++) {
            if (!TryRaw(z, pos, nil, out var name, out var data, out var next) || next < pos) return false;
            files.Add(new FileHeader { Id = i, Path = name, FileSize = data.Length, Data = data });
            pos = next;
        }
        return true;
    }

    static List<FileHeader> ScanRaw(byte[] z, long nil) {
        var files = new List<FileHeader>();
        for (var pos = 0x40; pos + 24 < z.Length; pos++) {
            if (BitConverter.ToInt64(z, pos) != nil) continue;
            if (!TryRaw(z, pos, nil, out var name, out var data, out var next)) continue;
            var run = new List<FileHeader>();
            var p = pos;
            var guard = 0;
            while (guard++ < 100_000 && TryRaw(z, p, nil, out name, out data, out next) && next >= p) {
                run.Add(new FileHeader { Id = files.Count + run.Count, Path = name, FileSize = data.Length, Data = data });
                if (next == p) break;
                p = next;
            }
            if (run.Count >= 2 || (run.Count == 1 && p >= z.Length - 16)) {
                files.AddRange(run);
                if (p > pos) pos = p - 1;
            }
        }
        return files;
    }

    static readonly byte[] Bo2Key = {
        0x64, 0x1D, 0x8A, 0x2F, 0xE3, 0x1D, 0x3A, 0xA6, 0x36, 0x22, 0xBB, 0xC9, 0xCE, 0x85, 0x87, 0x22,
        0x9D, 0x42, 0xB0, 0xF8, 0xED, 0x9B, 0x92, 0x41, 0x30, 0xBF, 0x88, 0xB6, 0x5E, 0xDC, 0x50, 0xBE,
    };

    static byte[] CreateIvTable(byte[] source) {
        var ivTable = new byte[16000];
        var addDiv = 0;
        var nameKeyLength = Array.FindIndex(source, b => b == 0);
        if (nameKeyLength <= 0) return ivTable;
        for (var i = 0; i < ivTable.Length; i += nameKeyLength * 4)
            for (var x = 0; x < nameKeyLength * 4; x += 4) {
                if (i + addDiv >= ivTable.Length || i + x >= ivTable.Length) return ivTable;
                addDiv = x > 0 ? x / 4 : 0;
                for (var y = 0; y < 4; y++) ivTable[i + x + y] = source[addDiv];
            }
        return ivTable;
    }

    static void UpdateIvTable(int index, byte[] hash, byte[] ivTable, int[] ivCounter) {
        for (var i = 0; i < 20; i += 5) {
            var value = (index + 4 * ivCounter[index]) % 800 * 5;
            for (var x = 0; x < 5; x++) ivTable[4 * value + x + i] ^= hash[i + x];
        }
        ivCounter[index]++;
    }

    static byte[] IvAt(int index, byte[] ivTable, int[] ivCounter) {
        var iv = new byte[8];
        var arrayIndex = (index + 4 * (ivCounter[index] - 1)) % 800 * 20;
        Array.Copy(ivTable, arrayIndex, iv, 0, 8);
        return iv;
    }

    // BO2 is Salsa20 sections, then raw deflate. BO3 is the blocked zlib loop.
    // The shared step is the name harvest. A BO3 flag mismatch lists nothing.
    static (byte[] zone, List<FileHeader> files) ListBo(BinaryReader r, bool bo3) {
        byte[] zone;
        if (!bo3) {
            r.BaseStream.Position = 0x18;
            var ivCount = new[] { 1, 1, 1, 1 };
            var ivTable = CreateIvTable(r.ReadBytes(0x20));
            r.BaseStream.Position += 0x100;
            var salsa = new Salsa20 { Key = Bo2Key };
            using (var zoneMs = new MemoryStream()) {
                var section = 0;
                while (r.BaseStream.Position + 4 <= r.BaseStream.Length && section < 10_000) {
                    var size = r.ReadInt32();
                    if (size == 0) break;
                    if (size < 0 || r.BaseStream.Position + size > r.BaseStream.Length) throw new FormatException($"BO2 section {size}");
                    salsa.IV = IvAt(section % 4, ivTable, ivCount);
                    var decrypted = salsa.CreateDecryptor().TransformFinalBlock(r.ReadBytes(size), 0, size);
                    using (var sha1 = SHA1.Create()) UpdateIvTable(section % 4, sha1.ComputeHash(decrypted), ivTable, ivCount);
                    using (var s = new MemoryStream(decrypted))
                    using (var deflate = new DeflateStream(s, CompressionMode.Decompress))
                        deflate.CopyTo(zoneMs);
                    section++;
                }
                zone = zoneMs.ToArray();
            }
        }
        else {
            // Byte 12 is unknown. zlib, pc, and encrypted follow it. Installed files are 00 01 00 00.
            r.BaseStream.Position = 12;
            var unknown = r.ReadByte();
            var flagsZ = r.ReadByte();
            var flagsPc = r.ReadByte();
            var flagsEncrypted = r.ReadByte();
            if (unknown != 0 || flagsZ != 1 || flagsPc != 0 || flagsEncrypted != 0) return (null, new List<FileHeader>());
            r.BaseStream.Position = 0x90;
            var size = r.ReadInt64();
            if (size <= 0 || size > 512L * 1024 * 1024) throw new FormatException($"BO3 zone size {size}");
            r.BaseStream.Position = 0x248;
            using (var zoneMs = new MemoryStream()) {
                long consumed = 0;
                while (consumed < size) {
                    var pos = r.BaseStream.Position;
                    if (pos + 16 > r.BaseStream.Length) break;
                    var compressedSize = r.ReadInt32();
                    var decompressedSize = r.ReadInt32();
                    var blockSize = r.ReadInt32();
                    var position = r.ReadInt32();
                    if (position != pos) throw new FormatException($"BO3 block position {position:X}");
                    if (decompressedSize == 0) {
                        r.BaseStream.Position = (r.BaseStream.Position + 0x7FFFFF) & ~0x7FFFFFL;
                        continue;
                    }
                    if (compressedSize < 2) throw new FormatException($"BO3 block compressed size {compressedSize}");
                    r.BaseStream.Position += 2;
                    var compressed = r.ReadBytes(compressedSize - 2);
                    using (var s = new MemoryStream(compressed))
                    using (var deflate = new DeflateStream(s, CompressionMode.Decompress))
                        deflate.CopyTo(zoneMs);
                    consumed += decompressedSize;
                    r.BaseStream.Position = position + 16 + blockSize;
                }
                zone = zoneMs.ToArray();
            }
        }
        return (zone, HarvestNames(zone, bo3));
    }

    static bool IsAssetName(string name) {
        if (name.Length < 3 || name.Length > 180) return false;
        for (var i = 0; i < name.Length; i++) if (name[i] < 32 || name[i] > 126) return false;
        if (name.IndexOf('/') >= 0 || name.IndexOf('\\') >= 0 || name[0] == '$') return true;
        return name.EndsWith(".cfg") || name.EndsWith(".csv") || name.EndsWith(".gsc") || name.EndsWith(".csc")
            || name.EndsWith(".ddl") || name.EndsWith(".txt") || name.EndsWith(".vision") || name.EndsWith(".menu");
    }

    // commaPrefix: BO3 asset names are stored with a leading comma. BO2 names are plain paths.
    static List<FileHeader> HarvestNames(byte[] zone, bool commaPrefix) {
        var files = new List<FileHeader>();
        var seen = new HashSet<string>();
        for (var i = 0; i < zone.Length; i++) {
            var start = i;
            if (commaPrefix) {
                if (zone[i] != (byte)',' || (i > 0 && zone[i - 1] >= 32)) continue;
                start = i + 1;
            }
            else if (i > 0 && zone[i - 1] >= 32 && zone[i - 1] < 127) continue;
            var e = start;
            while (e < zone.Length && zone[e] >= 32 && zone[e] < 127) e++;
            if (e >= zone.Length || zone[e] != 0 || e == start) continue;
            var name = System.Text.Encoding.ASCII.GetString(zone, start, e - start);
            if (!IsAssetName(name) || !seen.Add(name)) continue;
            files.Add(new FileHeader { Id = files.Count, Path = name, Position = start, FileSize = e - start });
        }
        return files;
    }
    #endregion
}

#endregion

#region Binary_Iwi
// https://github.com/XLabsProject/img-format-helper - IWI
// https://github.com/DentonW/DevIL/blob/master/DevIL/src-IL/src/il_iwi.cpp - IWI
// https://github.com/XLabsProject/img-format-helper - IWI

public class Binary_Iwi : ITexture, IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Iwi(r));

    #region Headers

    public enum VERSION : byte {
        /// <summary>
        /// COD2
        /// </summary>
        COD2 = 0x05,
        /// <summary>
        /// COD4
        /// </summary>
        COD4 = 0x06,
        /// <summary>
        /// COD5
        /// </summary>
        COD5 = 0x06,
        /// <summary>
        /// CODMW2
        /// </summary>
        CODMW2 = 0x08,
        /// <summary>
        /// CODMW3
        /// </summary>
        CODMW3 = 0x08,
        /// <summary>
        /// CODBO1
        /// </summary>
        CODBO1 = 0x0D,
        /// <summary>
        /// CODBO2
        /// </summary>
        CODBO2 = 0x1B,
    }

    public enum FORMAT : byte {
        /// <summary>
        /// ARGB32 - DDS_Standard_A8R8G8B8
        /// </summary>
        ARGB32 = 0x01,
        /// <summary>
        /// RGB24 - DDS_Standard_R8G8B8
        /// </summary>
        RGB24 = 0x02,
        /// <summary>
        /// GA16 - DDS_Standard_D16_UNORM
        /// </summary>
        GA16 = 0x03,
        /// <summary>
        /// A8 - DDS_Standard_A8_UNORM
        /// </summary>
        A8 = 0x04,
        /// <summary>
        /// A8b - DDS_Standard_A8_UNORM
        /// </summary>
        A8b = 0x05,
        /// <summary>
        /// JPG
        /// </summary>
        JPG = 0x07,
        /// <summary>
        /// DXT1 - DDS_BC1_UNORM;
        /// </summary>
        DXT1 = 0x0B,
        /// <summary>
        /// DXT3 - DDS_BC2_UNORM
        /// </summary>
        DXT2 = 0x0C,
        /// <summary>
        /// DXT5 - DDS_BC3_UNORM
        /// </summary>
        DXT3 = 0x0D,
        /// <summary>
        /// DXT5 - DDS_BC5_UNORM
        /// </summary>
        DXT5 = 0x0E,
    }

    [Flags]
    public enum FLAGS : byte {
        NOPICMIP = 1 << 0,
        /// <summary>
        /// NOMIPMAPS
        /// </summary>
        NOMIPMAPS = 1 << 1,
        /// <summary>
        /// CUBEMAP
        /// </summary>
        CUBEMAP = 1 << 2,
        /// <summary>
        /// VOLMAP
        /// </summary>
        VOLMAP = 1 << 3,
        /// <summary>
        /// STREAMING
        /// </summary>
        STREAMING = 1 << 4,
        /// <summary>
        /// LEGACY_NORMALS
        /// </summary>
        LEGACY_NORMALS = 1 << 5,
        /// <summary>
        /// CLAMP_U
        /// </summary>
        CLAMP_U = 1 << 6,
        /// <summary>
        /// CLAMP_V
        /// </summary>
        CLAMP_V = 1 << 7,
    }

    [Flags]
    public enum FLAGS_EXT : int {
        /// <summary>
        /// DYNAMIC
        /// </summary>
        DYNAMIC = 1 << 16,
        /// <summary>
        /// RENDER_TARGET
        /// </summary>
        RENDER_TARGET = 1 << 17,
        /// <summary>
        /// SYSTEMMEM
        /// </summary>
        SYSTEMMEM = 1 << 18
    }

    /// <summary>
    /// Describes a IWI file header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct HEADER {
        /// <summary>
        /// MAGIC (IWi)
        /// </summary>
        public const uint MAGIC = 0x69574900;
        /// <summary>
        /// Struct
        /// </summary>
        public static (string, int) Struct = ("<2c3H", sizeof(HEADER));
        /// <summary>
        /// Format
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public FORMAT Format;
        /// <summary>
        /// Usage
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public FLAGS Flags;
        /// <summary>
        /// Width
        /// </summary>
        public ushort Width;
        /// <summary>
        /// Height
        /// </summary>
        public ushort Height;
        /// <summary>
        /// Depth
        /// </summary>
        public ushort Depth;

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        public void Verify() {
            if (Width == 0 || Height == 0) throw new FormatException($"Invalid DDS file header");
            if (Format >= DXT1 && Format <= DXT5 && Width != MathX.NextPower(Width) && Height != MathX.NextPower(Height)) throw new FormatException($"DXT images must have power-of-2 dimensions..");
            if (Format > DXT5) throw new FormatException($"Unknown Format: {Format}");
        }

        public static byte[] Read(BinaryReader r, out HEADER header, out Range[] ranges, out (FORMAT type, object value) format) {
            var magic = r.ReadUInt32();
            var version = (VERSION)(magic >> 24);
            magic <<= 8;
            if (magic != MAGIC) throw new FormatException($"Invalid IWI file magic: {magic}.");
            if (version == VERSION.CODMW2) r.Seek(8);
            header = r.ReadS<HEADER>();
            header.Verify();

            // read mips offsets
            r.Seek(version switch {
                VERSION.COD2 => 0xC,
                VERSION.COD4 => 0xC,
                VERSION.CODMW2 => 0x10,
                VERSION.CODBO1 => 0x10,
                VERSION.CODBO2 => 0x20,
                _ => throw new FormatException($"Invalid IWI Version: {version}."),
            });

            var mips = r.ReadPArray<int>("i", version < VERSION.CODBO1 ? 4 : 8);
            var mipsLength = mips[0] == mips[1] || mips[0] == mips[^1] ? 1 : mips.Length - 1;
            var mipsBase = mipsLength == 1 ? (int)r.Tell() : mips[^1];
            var size = (int)(r.BaseStream.Length - mipsBase);
            ranges = mipsLength > 1
                ? Enumerable.Range(0, mipsLength).Select(i => new Range(mips[i + 1] - mipsBase, mips[i] - mipsBase)).ToArray()
                : [new Range(0, size)];
            r.Seek(mipsBase);
            format = header.Format switch {
                ARGB32 => (ARGB32, (TextureFormat.ARGB32, TexturePixel.Unknown)),
                RGB24 => (RGB24, (TextureFormat.RGB24, TexturePixel.Unknown)),
                DXT1 => (DXT1, (TextureFormat.DXT1, TexturePixel.Unknown)),
                DXT2 => (DXT2, (TextureFormat.DXT3, TexturePixel.Unknown)),
                DXT3 => (DXT3, (TextureFormat.DXT3, TexturePixel.Unknown)),
                DXT5 => (DXT5, (TextureFormat.DXT5, TexturePixel.Unknown)),
                _ => throw new ArgumentOutOfRangeException(nameof(Header.Format), $"{header.Format}"),
            };
            return r.ReadBytes(size);
        }
    }

    #endregion

    public Binary_Iwi(BinaryReader r) {
        Bytes = HEADER.Read(r, out Header, out Mips, out Format);
    }

    HEADER Header;
    Range[] Mips;
    byte[] Bytes;

    #region ITexture
    readonly (FORMAT type, object value) Format;
    public int Width => Header.Width;
    public int Height => Header.Height;
    public int Depth => 0;
    public int MipMaps => Mips.Length;
    public TextureFlags TexFlags => (Header.Flags & FLAGS.CUBEMAP) != 0 ? TextureFlags.CUBE_TEXTURE : 0;
    public T Create<T>(string platform, Func<object, T> func) => func(new TextureAsBytes(Bytes, Format.value, Mips));
    #endregion

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture", items: [
                new($"Format: {Format.type}"),
                new($"Width: {Header.Width}"),
                new($"Height: {Header.Height}"),
                new($"Mipmaps: {Mips.Length}"),
            ]),
        ];
}

#endregion

// Nameless fastfile rows are the index record. These factories do not read it.

#region Binary_Xmp

public class Binary_Xmp : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xmp());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "XMP", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Phy

public class Binary_Phy : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Phy());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "PHY", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Xan

public class Binary_Xan : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xan());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "XAN", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Xmd

public class Binary_Xmd : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xmd());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "XMD", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Mat

public class Binary_Mat : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Mat());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "MAT", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Tec

public class Binary_Tec : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Tec());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "TEC", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Snd

public class Binary_Snd : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Snd());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "SND", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Crv

public class Binary_Crv : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Crv());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "CRV", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Lds

public class Binary_Lds : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Lds());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "LDS", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Csp

public class Binary_Csp : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Csp());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "CSP", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Cmp

public class Binary_Cmp : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Cmp());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "CMP", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Cmm

public class Binary_Cmm : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Cmm());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "CMM", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Gsp

public class Binary_Gsp : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Gsp());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "GSP", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Gmp

public class Binary_Gmp : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Gmp());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "GMP", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Ent

public class Binary_Ent : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Ent());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "ENT", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Gfx

public class Binary_Gfx : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Gfx());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "GFX", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Lgt

public class Binary_Lgt : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Lgt());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "LGT", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Uim

public class Binary_Uim : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Uim());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "UIM", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Fnt

public class Binary_Fnt : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Fnt());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "FNT", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Mnf

public class Binary_Mnf : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Mnf());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "MNF", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Mnu

public class Binary_Mnu : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Mnu());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "MNU", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Loc

public class Binary_Loc : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Loc());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "LOC", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Wep

public class Binary_Wep : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Wep());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "WEP", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Sdg

public class Binary_Sdg : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Sdg());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "SDG", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Fx

public class Binary_Fx : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Fx());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "FX", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Ifx

public class Binary_Ifx : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Ifx());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "IFX", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Ait

public class Binary_Ait : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Ait());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "AIT", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Mpt

public class Binary_Mpt : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Mpt());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "MPT", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Chr

public class Binary_Chr : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Chr());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "CHR", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Xma

public class Binary_Xma : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xma());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "XMA", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Raw

public class Binary_Raw : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Raw());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "RAW", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Tbl

public class Binary_Tbl : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Tbl());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "TBL", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Pcm

public class Binary_Pcm : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Pcm());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "PCM", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Xms

public class Binary_Xms : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xms());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "XMS", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Psh

public class Binary_Psh : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Psh());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "PSH", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Vsh

public class Binary_Vsh : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Vsh());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "VSH", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Vdl

public class Binary_Vdl : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Vdl());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "VDL", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Fxm

public class Binary_Fxm : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Fxm());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "FXM", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Lbd

public class Binary_Lbd : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Lbd());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "LBD", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Sdd

public class Binary_Sdd : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Sdd());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "SDD", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Trc

public class Binary_Trc : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Trc());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "TRC", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Veh

public class Binary_Veh : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Veh());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "VEH", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Ame

public class Binary_Ame : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Ame());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "AME", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Gls

public class Binary_Gls : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Gls());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "GLS", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Aip

public class Binary_Aip : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Aip());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "AIP", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Vtr

public class Binary_Vtr : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Vtr());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "VTR", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Att

public class Binary_Att : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Att());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "ATT", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion

#region Binary_Sfx

public class Binary_Sfx : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Sfx());

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "SFX", Name = Path.GetFileName(file.Path), Value = this })
    ];
}

#endregion
