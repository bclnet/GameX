using GameX.Formats;
using OpenStack;
using OpenStack.Gfx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Lucas.Formats;

#region Binary_Abc

public class Binary_Abc : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Abc(r, (int)f.FileSize));

    public Binary_Abc(BinaryReader r, int fileSize) => Data = r.ReadBytes(fileSize);

    public byte[] Data;

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Sbi", Name = Path.GetFileName(file.Path), Value = new MemoryStream(Data), Tag = Path.GetExtension(file.Path) }),
    ];
}

#endregion

#region Binary_Jedi

public unsafe class Binary_Jedi : ArcBinary<Binary_Jedi> {
    #region Headers

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GOB_Header {
        public static (string, int) Struct = ("<2I", sizeof(GOB_Header));
        public uint Magic;              // Always 'GOB '
        public uint EntryOffset;        // Offset to GOB_Entry
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GOB_Entry {
        public static (string, int) Struct = ("<2I13s", sizeof(GOB_Entry));
        public uint Offset;             // Offset in the archive file
        public uint FileSize;           // Size in bytes of this entry
        public fixed byte Path[13];     // File name
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LFD_Entry {
        public static (string, int) Struct = ("<I8sI", sizeof(LFD_Entry));
        public uint Type;
        public fixed byte Name[8];
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LAB_Header {
        public static (string, int) Struct = ("<4I", sizeof(LAB_Header));
        public uint Magic;              // Always 'LABN'
        public uint Version;            // Apparently always 0x10000 for Outlaws
        public uint FileCount;          // File entry count
        public uint NameTableLength;    // Length including null bytes of the filename list/string
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LAB_Entry {
        public static (string, int) Struct = ("<4I", sizeof(LAB_Entry));
        public uint NameOffset;         // Offset in the name string
        public uint Offset;             // Offset in the archive file
        public uint FileSize;           // Size in bytes of this entry
        public uint FourCC;             // All zeros or a 4CC related to the filename extension
    }

    #endregion

    public override Task Read(BinaryAsset source, BinaryReader r, object tag) {
        const uint GOB_MAGIC = 0x0a424f47;
        const uint LFD_MAGIC = 0x50414d52;
        const uint LAB_MAGIC = 0x4e42414c;

        switch (Path.GetExtension(source.Name).ToLowerInvariant()) {
            case ".gob": {
                    var header = r.ReadS<GOB_Header>();
                    if (header.Magic != GOB_MAGIC) throw new FormatException("BAD MAGIC");

                    r.Seek(header.EntryOffset);
                    var entries = r.ReadL32SArray<GOB_Entry>();
                    source.Files = entries.Select(s => new FileSource {
                        Path = UnsafeX.FixedAString(s.Path, 13),
                        Offset = s.Offset,
                        FileSize = s.FileSize,
                    }).ToArray();
                    return Task.CompletedTask;
                }
            case ".lfd": {
                    var header = r.ReadS<LFD_Entry>();
                    if (header.Type != LFD_MAGIC) throw new FormatException("BAD MAGIC");
                    else if (UnsafeX.FixedAString(header.Name, 8) != "resource") throw new FormatException("BAD NAME");
                    else if (header.Size % 16 != 0) throw new FormatException("BAD SIZE");
                    var entries = r.ReadSArray<LFD_Entry>((int)header.Size / 16);
                    var offset = header.Size + 16;
                    source.Files = entries.Select(s => new FileSource {
                        Path = UnsafeX.FixedAString(s.Name, 8),
                        Offset = (offset += s.Size + 16) - s.Size,
                        FileSize = s.Size,
                    }).ToArray();
                    return Task.CompletedTask;
                }
            case ".lab": {
                    var header = r.ReadS<LAB_Header>();
                    if (header.Magic != LAB_MAGIC) throw new FormatException("BAD MAGIC");
                    else if (header.Version != 0x10000) throw new FormatException("BAD VERSION");

                    var entries = r.ReadSArray<LAB_Entry>((int)header.FileCount);
                    var paths = r.ReadCStringArray((int)header.FileCount);
                    source.Files = entries.Select((s, i) => new FileSource {
                        Path = paths[i],
                        Offset = s.Offset,
                        FileSize = s.FileSize,
                    }).ToArray();
                    return Task.CompletedTask;
                }
            default: throw new FormatException();
        }
    }

    public override Task<Stream> ReadData(BinaryAsset source, BinaryReader r, FileSource file, object option = default) {
        r.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
    }
}

#endregion

#region Binary_Nwx

public class Binary_Nwx : IHaveMetaInfo, ITextureSelect {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Nwx(r, f, s));

    #region Headers

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct X_Header {
        public static (string, int) Struct = ("<3I2F3I", sizeof(X_Header));
        public uint Magic; // 'WAXF'
        public uint MajorVersion;
        public uint MinorVersion;
        public float ScaleX;
        public float ScaleY;
        public uint CellTableOffset;
        public uint FrameTableOffset;
        public uint ChoreographyTableOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct X_CellHeader {
        public static (string, int) Struct = ("<3I", sizeof(X_CellHeader));
        public uint Magic; // 'CELT'
        public uint Count;
        public uint TableSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct X_Cell {
        public static (string, int) Struct = ("<5I", sizeof(X_Cell));
        public uint Id;
        public uint Size;
        public uint Width;
        public uint Height;
        public uint Flags;
    }

    #endregion

    #region Palette

    static readonly ConcurrentDictionary<string, (byte r, byte g, byte b)[]> Palettes = new ConcurrentDictionary<string, (byte r, byte g, byte b)[]>();

    static (byte r, byte g, byte b)[] PaletteBuilder(string path, Archive arc) {
        var paths = path.Split(':');
        var pcx = arc.GetArchive(paths[0]).GetAsset<Binary_Pcx>(paths[1], throwOnError: false).Result;
        if (pcx == null) return null;
        var pal = pcx.GetPalette();
        var b = new List<(byte r, byte g, byte b)>();
        for (var i = 0; i < 256; i++)
            b.Add((pal[(i * 3) + 0], pal[(i * 3) + 1], pal[(i * 3) + 2]));
        return b.ToArray();
    }

    #endregion

    public Binary_Nwx(BinaryReader r, FileSource f, Archive s) {
        const uint WAXF_MAGIC = 0x46584157;
        const uint CELT_MAGIC = 0x544c4543;

        Palette = Palettes.GetOrAdd(s.Game.Id switch {
            "O" => "outlaws.lab:simms.pcx",
            "SW:DF" => "DARK.GOB:simms.pcx",
            _ => throw new ArgumentOutOfRangeException(),
        }, PaletteBuilder, s);

        // read header
        var header = r.ReadS<X_Header>();
        if (header.Magic != WAXF_MAGIC) throw new FormatException("BAD MAGIC");
        else if (header.MajorVersion != 2) throw new FormatException("BAD VERSION");
        else if (header.MinorVersion != 1) throw new FormatException("BAD VERSION");

        // read cell table
        r.Seek(header.CellTableOffset);
        var cellHeader = r.ReadS<X_CellHeader>();
        if (cellHeader.Magic != CELT_MAGIC) throw new FormatException("BAD MAGIC");

        // read each cell
        Cells = [];
        for (var i = 0; i < cellHeader.Count; i++) {
            var cell = r.ReadS<X_Cell>();
            if (cell.Size == 1 && cell.Width == 0) { Log.Info($"Empty Cell: {i}"); r.ReadByte(); continue; } // 0xCD Terminator

            // Bit 0 specifies what dimension to use for column table retrieval and decompression. Just swap dimensions.
            var flip = (cell.Flags & 0x00000001) == 0;
            if (flip) { var temp = cell.Width; cell.Width = cell.Height; cell.Height = temp; }

            var cellOffsetTablePosition = r.Tell();
            var cellOffsets = r.ReadPArray<uint>("I", (int)cell.Width);
            var data = new List<byte>();
            foreach (var offset in cellOffsets) {
                r.Seek(cellOffsetTablePosition + offset);
                var pixels = new List<byte>();
                var pixelCount = 0;
                while (pixelCount < cell.Height) {
                    var control = r.ReadByte();
                    if (control < 0x02) {
                        pixels.Add(r.ReadByte());
                        pixelCount += 1;
                    }
                    else {
                        var length = (control / 2) + 1;
                        pixels.AddRange(control % 2 == 0 ? r.ReadBytes(length) : Enumerable.Repeat(r.ReadByte(), length));
                        pixelCount += length;
                    }
                }
                data.AddRange(pixels);
            }
            Cells.Add(((int)cell.Width, (int)cell.Height, flip, data.ToArray()));
            r.ReadByte(); // 0xCD Terminator
        }
        Select(0);
    }

    readonly (byte r, byte g, byte b)[] Palette;
    readonly List<(int width, int height, bool flip, byte[] data)> Cells;
    bool Flip;
    byte[] CellData;

    #region ITexture
    static readonly object Format = (TextureFormat.RGBA32, TexturePixel.Unknown);
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //TextureUnityFormat.RGBA32,
    //TextureUnrealFormat.Unknown);
    public int Width { get; set; }
    public int Height { get; set; }
    public int Depth { get; } = 0;
    public int MipMaps { get; } = 1;
    public TextureFlags TexFlags { get; } = 0;
    public T Create<T>(string platform, Func<object, T> func) {
        int width = Width, height = Height;
        var data = CellData;

        var bytes = new byte[width * height * 4];
        if (Flip) {
            var i = 0;
            for (var row = 0; row < height; row++)
                for (var col = 0; col < width; col++, i += 4) {
                    var pixel = data[col * height + row];
                    bytes[i + 0] = Palette[pixel].r;
                    bytes[i + 1] = Palette[pixel].g;
                    bytes[i + 2] = Palette[pixel].b;
                    bytes[i + 3] = pixel == 0 ? (byte)0 : (byte)255;
                }
        }
        else {
            var i = bytes.Length - 4;
            for (var row = 0; row < height; row++)
                for (var col = 0; col < width; col++, i -= 4) {
                    var pixel = data[col * height + row];
                    bytes[i + 0] = Palette[pixel].r;
                    bytes[i + 1] = Palette[pixel].g;
                    bytes[i + 2] = Palette[pixel].b;
                    bytes[i + 3] = pixel == 0 ? (byte)0 : (byte)255;
                }
        }
        return func(new Texture_Bytes(bytes, Format, null));
    }
    public void Select(int id) => (Width, Height, Flip, CellData) = Cells[id % Cells.Count];
    #endregion

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
        new($"{nameof(Binary_Nwx)}", items: [
            new($"Cells: {Cells.Count}"),
            //new($"Width: {Width}"),
            //new($"Height: {Height}"),
        ])
    ];
}

#endregion

#region Binary_San

public class Binary_San : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_San(r, (int)f.FileSize));

    #region Headers

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct X_Header {
        public static (string, int) Struct = (">2I", sizeof(X_Header)); //: BE
        public uint Magic; // 'ANIM'
        public uint ChunkSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct X_AHeader {
        public static (string, int) Struct = (">2I3H", sizeof(X_AHeader));
        public uint Magic; // 'AHDR'
        public uint Size;
        public ushort Version;
        public ushort NumFrames;
        public ushort Unknown;
        public fixed byte Palette[0x300];
    }

    Range _palDirty = 0..255;
    void SetDirtyColors(int min, int max) {
        //if (_palDirty.Start.Value > min)
        //    _palDirty.Start = min;
        //if (_palDirtyMax < max)
        //    _palDirtyMax = max;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct X_Chunk {
        public static (string, int) Struct = (">2I", sizeof(X_Chunk));
        public uint Magic; // 'FRME'
        public uint ChunkSize;
        public readonly uint Size => ChunkSize + ((ChunkSize & 1) != 0 ? 1U : 0U);
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    //unsafe struct X_Chunk_FOBJ
    //{
    //    public static (string, int) Struct = (">2I", sizeof(X_Chunk_FOBJ));
    //    public uint Magic; // 'FOBJ'
    //    public uint ChunkSize;
    //}

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    //unsafe struct X_Chunk_IACT
    //{
    //    public static (string, int) Struct = (">2I", sizeof(X_Chunk_IACT));
    //    public uint Magic; // 'IACT'
    //    public uint ChunkSize;
    //}

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    //unsafe struct X_Chunk_PSAD
    //{
    //    public static (string, int) Struct = (">2I", sizeof(X_Chunk_PSAD));
    //    public uint Magic; // 'PSAD'
    //    public uint ChunkSize;
    //}

    #endregion

    public Binary_San(BinaryReader r, int fileSize) {
        const uint ANIM_MAGIC = 0x414e494d;
        const uint AHDR_MAGIC = 0x41484452;
        const uint FRME_MAGIC = 0x46524d45;

        //const uint NPAL_MAGIC = 0x4e50414c;
        //const uint ZFOB_MAGIC = 0x5a464f42;

        // read header
        var header = r.ReadS<X_Header>();
        if (header.Magic != ANIM_MAGIC) throw new FormatException("BAD MAGIC");

        // read aheader
        var aheader = r.ReadS<X_AHeader>();
        if (aheader.Magic != AHDR_MAGIC) throw new FormatException("BAD MAGIC");
        var aheaderBody = r.ReadBytes((int)aheader.Size - 6);

        // read frames
        for (var f = 0; f < aheader.NumFrames; f++) {
            var chunk = r.ReadS<X_Chunk>();
            if (chunk.Magic != FRME_MAGIC) throw new FormatException("BAD MAGIC");
            var chunkEnd = r.BaseStream.Position + chunk.ChunkSize;
            while (r.BaseStream.Position < chunkEnd) {
                chunk = r.ReadS<X_Chunk>();
                switch (chunk.Magic) {
                    //case NPAL_MAGIC:
                    //case ZFOB_MAGIC:
                    default:
                        var z = BitConverter.GetBytes(chunk.Magic); z.Reverse();
                        Log.Info($"{Encoding.ASCII.GetString(z)}");
                        r.Skip(chunk.Size);
                        break;
                }
            }
        }
    }

    void HandleFrame(BinaryReader r, int frameSize) {
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Sbi", Name = Path.GetFileName(file.Path), Value = null, Tag = Path.GetExtension(file.Path) }),
    ];
}

#endregion

#region Binary_Scumm

public unsafe class Binary_Scumm : ArcBinary<Binary_Scumm> {
    [Flags]
    public enum Features {
        None,
        SixteenColors = 0x01,
        Old256 = 0x02,
        FewLocals = 0x04,
        Demo = 0x08,
        Is16BitColor = 0x10,
        AudioTracks = 0x20,
    }

    public enum Platform {
        None,
        Apple2GS,
        C64,
        Amiga,
        AtariST,
        SegaCD,
        Macintosh,
        FMTowns,
    }

    public class ArrayDefinition {
        public uint Index;
        public int Type;
        public int Dim1;
        public int Dim2;
    }

    public class ResourceIndex {
        public const ushort CLASSIC_MAGIC = 0x0A31;
        public const ushort ENHANCE_MAGIC = 0x0100;

        // objects
        public Dictionary<string, int> ObjectIDMap; // Version8
        public byte[] ObjectOwnerTable;
        public byte[] ObjectStateTable;
        public uint[] ClassData;

        // resources
        public Dictionary<byte, string> RoomNames;
        public (byte, long)[] RoomResources;
        public (byte, long)[] ScriptResources;
        public (byte, long)[] SoundResources;
        public (byte, long)[] CostumeResources;
        public (byte, long)[] CharsetResources;
        public (byte, long)[] RoomScriptResources; // Version8

        // max sizes
        public List<ArrayDefinition> ArrayDefinitions = new List<ArrayDefinition>();
        public int NumVerbs = 100;
        public int NumInventory = 80;
        public int NumVariables = 800;
        public int NumBitVariables = 4096;
        public int NumLocalObjects = 200;
        public int NumArray = 50;
        public int NumGlobalScripts = 200;
        public byte[] ObjectRoomTable;
        public string[] AudioNames = new string[0];

        public ResourceIndex(BinaryReader r, FamilyGame game, Dictionary<string, object> detect) {
            var varient = (Dictionary<string, object>)detect["variant"];
            var features = ((Features)detect["features"]);
            var version = (int)varient["version"];
            var oldBundle = version <= 3 && features.HasFlag(Features.SixteenColors);
            switch (version) {
                case 0: Load0(game, r, detect); break;
                case 1: if ((Platform)detect["platform"] == Platform.C64) Load0(game, r, detect); else Load2(game, r, detect); break;
                case 2: Load2(game, r, detect); break;
                case 3: if (oldBundle) Load3_16(game, r, detect); else Load3(game, r, detect, features.HasFlag(Features.Old256) ? (byte)0 : (byte)0xff); break;
                case 4: Load4(game, r, detect); break;
                case 5: Load5(game, r, detect); break;
                case 6: Load6(game, r, detect); break;
                case 7: Load7(game, r, detect); break;
                case 8: Load8(game, r, detect); break;
                default: throw new NotSupportedException($"Version {0} is not supported.");
            }
            ;
        }

        #region Reads

        [Flags]
        enum ObjectFlags {
            CountX = 0b00,
            CountS = 0b01,
            CountI = 0b10,
            CountMask = 0b11,
            Loop1 = 0b00100,
            Loop2 = 0b01000,
            Loop3 = 0b01100,
            Loop4 = 0b10000,
            Loop5 = 0b10100,
            LoopMask = 0b11100,
            // combined
            CXL1 = CountX | Loop1,
            CSL1 = CountS | Loop1,
            CSL2 = CountS | Loop2,
            CSL3 = CountS | Loop3,
            CSL4 = CountS | Loop4,
            CIL5 = CountS | Loop5,
        }

        [Flags]
        enum ResourceFlags {
            CountX = 0b00,
            CountB = 0b01,
            CountS = 0b10,
            CountMask = 0b11,
            Loop1 = 0b00100,
            Loop2 = 0b01000,
            Loop3 = 0b01100,
            Loop4 = 0b10000,
            LoopMask = 0b11100,
            // combined
            CXL1 = CountX | Loop1,
            CXL2 = CountX | Loop2,
            CBL1 = CountB | Loop1,
            CBL2 = CountB | Loop2,
            CSL3 = CountB | Loop3,
            CSL4 = CountB | Loop4,
        }

        static uint ToOffset(ushort offset) => offset == 0xFFFF ? 0xFFFFFFFF : offset;

        void ReadObjects(BinaryReader r, ObjectFlags flags, int count = 0) {
            count = (flags & ObjectFlags.CountMask) switch {
                ObjectFlags.CountX => count,
                ObjectFlags.CountS => r.ReadUInt16(),
                ObjectFlags.CountI => r.ReadInt32(),
                _ => throw new NotSupportedException(),
            };
            switch (flags & ObjectFlags.LoopMask) {
                case ObjectFlags.Loop1:
                    ObjectOwnerTable = new byte[count];
                    ObjectStateTable = new byte[count];
                    ClassData = new uint[count];
                    for (var i = 0; i < count; i++) {
                        var tmp = r.ReadByte();
                        ObjectStateTable[i] = (byte)(tmp >> 4);
                        ObjectOwnerTable[i] = (byte)(tmp & 0x0F);
                    }
                    break;
                case ObjectFlags.Loop2:
                    ObjectOwnerTable = new byte[count];
                    ObjectStateTable = new byte[count];
                    ClassData = new uint[count];
                    for (var i = 0; i < count; i++) {
                        ClassData[i] = r.ReadByte() | (uint)(r.ReadByte() << 8) | (uint)(r.ReadByte() << 16);
                        var tmp = r.ReadByte();
                        ObjectStateTable[i] = (byte)(tmp >> 4);
                        ObjectOwnerTable[i] = (byte)(tmp & 0x0F);
                    }
                    break;
                case ObjectFlags.Loop3:
                    ObjectOwnerTable = new byte[count];
                    ObjectStateTable = new byte[count];
                    for (var i = 0; i < count; i++) {
                        var tmp = r.ReadByte();
                        ObjectStateTable[i] = (byte)(tmp >> 4);
                        ObjectOwnerTable[i] = (byte)(tmp & 0x0F);
                    }
                    ClassData = r.ReadPArray<uint>("I", count);
                    break;
                case ObjectFlags.Loop4:
                    ObjectStateTable = r.ReadBytes(count);
                    ObjectRoomTable = r.ReadBytes(count);
                    ObjectOwnerTable = new byte[count];
                    for (var i = 0; i < count; i++)
                        ObjectOwnerTable[i] = 0xFF;
                    ClassData = r.ReadPArray<uint>("I", count);
                    break;
                case ObjectFlags.Loop5:
                    ObjectIDMap = new Dictionary<string, int>();
                    ObjectStateTable = new byte[count];
                    ObjectRoomTable = new byte[count];
                    ObjectOwnerTable = new byte[count];
                    ClassData = new uint[count];
                    for (var i = 0; i < count; i++) {
                        var name = r.ReadFWString(40);
                        ObjectIDMap[name] = i;
                        ObjectStateTable[i] = r.ReadByte();
                        ObjectRoomTable[i] = r.ReadByte();
                        ClassData[i] = r.ReadUInt32();
                        ObjectOwnerTable[i] = 0xFF;
                    }
                    break;
            }
        }

        static (byte, long)[] ReadResources(BinaryReader r, ResourceFlags flags, int count = 0) {
            count = (flags & ResourceFlags.CountMask) switch {
                ResourceFlags.CountX => count,
                ResourceFlags.CountB => r.ReadByte(),
                ResourceFlags.CountS => r.ReadUInt16(),
                _ => throw new NotSupportedException(),
            };
            var res = new (byte, long)[count];
            var rooms = r.ReadBytes(count);
            switch (flags & ResourceFlags.LoopMask) {
                case ResourceFlags.Loop1: for (var i = 0; i < count; i++) res[i] = ((byte)i, ToOffset(r.ReadUInt16())); break;
                case ResourceFlags.Loop2: for (var i = 0; i < count; i++) res[i] = (rooms[i], ToOffset(r.ReadUInt16())); break;
                case ResourceFlags.Loop3: for (var i = 0; i < count; i++) res[i] = (r.ReadByte(), r.ReadUInt32()); break;
                case ResourceFlags.Loop4: for (var i = 0; i < count; i++) res[i] = (rooms[i], r.ReadUInt32()); break;
            }
            return res;
        }

        static string[] ReadNames(BinaryReader r) {
            var values = new string[r.ReadUInt16()];
            for (var i = 0; i < values.Length; i++)
                values[i] = r.ReadFWString(9);
            return values;
        }

        static Dictionary<byte, string> ReadRoomNames(BinaryReader r) {
            var values = new Dictionary<byte, string>();
            for (byte room; (room = r.ReadByte()) != 0;) {
                var name = r.ReadBytes(9);
                var b = new StringBuilder();
                for (var i = 0; i < 9; i++) {
                    var c = name[i] ^ 0xFF;
                    if (c == 0) continue;
                    b.Append((char)c);
                }
                values[room] = b.ToString();
            }
            return values;
        }

        void ReadMaxSizes(BinaryReader r, FamilyGame game, Features features, int ver) {
            switch (ver) {
                case 5: {
                        NumVariables = r.ReadUInt16();      // 800
                        r.ReadUInt16();                     // 16
                        NumBitVariables = r.ReadUInt16();   // 2048
                        NumLocalObjects = r.ReadUInt16();   // 200
                        r.ReadUInt16();                     // 50
                        var numCharsets = r.ReadUInt16();   // 9
                        r.ReadUInt16();                     // 100
                        r.ReadUInt16();                     // 50
                        NumInventory = r.ReadUInt16();      // 80
                        break;
                    }
                case 6: {
                        NumVariables = r.ReadUInt16();      // 800
                        r.ReadUInt16();                     // 16
                        NumBitVariables = r.ReadUInt16();   // 2048
                        NumLocalObjects = r.ReadUInt16();   // 200
                        NumArray = r.ReadUInt16();          // 50
                        r.ReadUInt16();
                        NumVerbs = r.ReadUInt16();          // 100
                        var numFlObject = r.ReadUInt16();   // 50
                        NumInventory = r.ReadUInt16();      // 80
                        var numRooms = r.ReadUInt16();
                        var numScripts = r.ReadUInt16();
                        var numSounds = r.ReadUInt16();
                        var numCharsets = r.ReadUInt16();
                        var numCostumes = r.ReadUInt16();
                        var numGlobalObjects = r.ReadUInt16();
                        break;
                    }
                case 7: {
                        r.Skip(50); // Skip over SCUMM engine version
                        r.Skip(50); // Skip over data file version
                        NumVariables = r.ReadUInt16();
                        NumBitVariables = r.ReadUInt16();
                        r.ReadUInt16();
                        var numGlobalObjects = r.ReadUInt16();
                        NumLocalObjects = r.ReadUInt16();
                        var numNewNames = r.ReadUInt16();
                        NumVerbs = r.ReadUInt16();
                        var numFlObject = r.ReadUInt16();
                        NumInventory = r.ReadUInt16();
                        NumArray = r.ReadUInt16();
                        var numRooms = r.ReadUInt16();
                        var numScripts = r.ReadUInt16();
                        var numSounds = r.ReadUInt16();
                        var numCharsets = r.ReadUInt16();
                        var numCostumes = r.ReadUInt16();
                        NumGlobalScripts = game.Id == "FT" && features.HasFlag(Features.Demo) ? 300 : 2000;
                        break;
                    }
                case 8: {
                        r.Skip(50); // Skip over SCUMM engine version
                        r.Skip(50); // Skip over data file version
                        NumVariables = r.ReadInt32();
                        NumBitVariables = r.ReadInt32();
                        r.ReadInt32();
                        var numScripts = r.ReadInt32();
                        var numSounds = r.ReadInt32();
                        var numCharsets = r.ReadInt32();
                        var numCostumes = r.ReadInt32();
                        var numRooms = r.ReadInt32();
                        r.ReadInt32();
                        var numGlobalObjects = r.ReadInt32();
                        r.ReadInt32();
                        NumLocalObjects = r.ReadInt32();
                        var numNewNames = r.ReadInt32();
                        var numFlObject = r.ReadInt32();
                        NumInventory = r.ReadInt32();
                        NumArray = r.ReadInt32();
                        NumVerbs = r.ReadInt32();
                        NumGlobalScripts = 2000;
                        break;
                    }
            }
        }

        static List<ArrayDefinition> ReadIndexFile(BinaryReader r) {
            var values = new List<ArrayDefinition>();
            uint num;
            while ((num = r.ReadUInt16()) != 0) {
                var a = r.ReadUInt16();
                var b = r.ReadUInt16();
                var c = r.ReadUInt16();
                values.Add(new ArrayDefinition { Index = num, Type = c, Dim2 = a, Dim1 = b });
            }
            return values;
        }

        //$"DIRN: 0x{BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DIRN")):X}".Dump();

        #endregion

        #region Loads

        byte[] V0_roomDisks;
        byte[] V0_roomTracks;
        byte[] V0_roomSectors;

        void Load0(FamilyGame game, BinaryReader r, Dictionary<string, object> detect) {
            V0_roomDisks = new byte[59];
            V0_roomTracks = new byte[59];
            V0_roomSectors = new byte[59];
            // determine counts
            int numGlobalObjects, numRooms, numCostumes, numScripts, numSounds;
            if (game.Id == "MM") // Maniac Mansion
            {
                numGlobalObjects = 256; numRooms = 55; numCostumes = 25;
                if (((Features)detect["features"]).HasFlag(Features.Demo)) { numScripts = 55; numSounds = 40; }
                else { numScripts = 160; numSounds = 70; }
            }
            else { numGlobalObjects = 775; numRooms = 59; numCostumes = 38; numScripts = 155; numSounds = 127; }

            // skip
            if ((Platform)detect["platform"] == Platform.Apple2GS) r.Seek(142080);

            // read magic
            var magic = r.ReadUInt16();
            if (magic != CLASSIC_MAGIC) throw new FormatException("BAD MAGIC");

            // object flags
            ReadObjects(r, ObjectFlags.CXL1, numGlobalObjects);

            // room offsets
            for (var i = 0; i < numRooms; i++) V0_roomDisks[i] = (byte)(r.ReadByte() - '0');
            for (var i = 0; i < numRooms; i++) {
                V0_roomSectors[i] = r.ReadByte();
                V0_roomTracks[i] = r.ReadByte();
            }
            CostumeResources = ReadResources(r, ResourceFlags.CXL2, numCostumes);
            ScriptResources = ReadResources(r, ResourceFlags.CXL2, numScripts);
            SoundResources = ReadResources(r, ResourceFlags.CXL2, numSounds);
        }

        void Load2(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect) {
            var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0xff));
            var magic = r.ReadUInt16();
            switch (magic) {
                case CLASSIC_MAGIC: {
                        int numGlobalObjects, numRooms, numCostumes, numScripts, numSounds;
                        if (game.Id == "MM") { numGlobalObjects = 800; numRooms = 55; numCostumes = 35; numScripts = 200; numSounds = 100; }
                        else if (game.Id == "ZMatAM") { numGlobalObjects = 775; numRooms = 61; numCostumes = 37; numScripts = 155; numSounds = 120; }
                        else throw new NotSupportedException($"Version2 for {game.Id} is not supported.");
                        ReadObjects(r, ObjectFlags.CXL1, numGlobalObjects);
                        RoomResources = ReadResources(r, ResourceFlags.CXL1, numRooms);
                        CostumeResources = ReadResources(r, ResourceFlags.CXL2, numCostumes);
                        ScriptResources = ReadResources(r, ResourceFlags.CXL2, numScripts);
                        SoundResources = ReadResources(r, ResourceFlags.CXL2, numSounds);
                        return;
                    }
                case ENHANCE_MAGIC: {
                        ReadObjects(r, ObjectFlags.CSL1);
                        RoomResources = ReadResources(r, ResourceFlags.CBL1);
                        CostumeResources = ReadResources(r, ResourceFlags.CBL2);
                        ScriptResources = ReadResources(r, ResourceFlags.CBL2);
                        SoundResources = ReadResources(r, ResourceFlags.CBL2);
                        return;
                    }
                default: throw new FormatException("BAD MAGIC");
            }
        }

        void Load3_16(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect) {
            var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0xff));
            var magic = r.ReadUInt16();
            if (magic != ENHANCE_MAGIC) throw new FormatException("BAD MAGIC");
            ReadObjects(r, ObjectFlags.CSL2);
            RoomResources = ReadResources(r, ResourceFlags.CBL1);
            CostumeResources = ReadResources(r, ResourceFlags.CBL2);
            ScriptResources = ReadResources(r, ResourceFlags.CBL2);
            SoundResources = ReadResources(r, ResourceFlags.CBL2);
        }

        void Load3(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect, byte xorByte) {
            var indy3FmTowns = game.Id == "IJatLC" && (Platform)detect["platform"] == Platform.FMTowns;
            var r = xorByte != 0 ? new BinaryReader(new ByteXorStream(r_.BaseStream, xorByte)) : r_;
            while (r.BaseStream.Position < r.BaseStream.Length) {
                r.ReadUInt32();
                var block = r.ReadInt16();
                switch (block) {
                    case 0x4E52: RoomNames = ReadRoomNames(r); break;
                    case 0x5230: RoomResources = ReadResources(r, ResourceFlags.CSL3); break; // 'R0'
                    case 0x5330: ScriptResources = ReadResources(r, ResourceFlags.CSL3); break; // 'S0'
                    case 0x4E30: SoundResources = ReadResources(r, ResourceFlags.CSL3); break; // 'N0'
                    case 0x4330: CostumeResources = ReadResources(r, ResourceFlags.CSL3); break; // 'C0'
                    case 0x4F30: ReadObjects(r, ObjectFlags.CSL2); if (indy3FmTowns) r.Skip(32); break;// 'O0' - Indy3 FM-TOWNS has 32 extra bytes
                    default: Log.Info($"Unknown block {block:X2}"); break;
                }
            }
        }

        void Load4(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect) => Load3(game, r_, detect, 0);

        void Load5(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect) {
            var features = (Features)detect["features"];
            var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0x69));
            while (r.BaseStream.Position < r.BaseStream.Length) {
                var block = r.ReadUInt32();
                r.ReadUInt32E(); // size
                switch (block) {
                    case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                    case 0x5358414D: ReadMaxSizes(r, game, features, 5); break; // 'MAXS'
                    case 0x4F4F5244: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'
                    case 0x52435344: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'
                    case 0x554F5344: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'
                    case 0x534F4344: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'
                    case 0x52484344: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'
                    case 0x4A424F44: ReadObjects(r, ObjectFlags.CSL3); break; // 'DOBJ'
                    default: Log.Info($"Unknown block {block:X2}"); break;
                }
            }
        }

        void Load6(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect) {
            var features = (Features)detect["features"];
            var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0x69));
            while (r.BaseStream.Position < r.BaseStream.Length) {
                var block = r.ReadUInt32();
                r.ReadUInt32E(); // size
                switch (block) {
                    case 0x52484344: case 0x46524944: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'/'DIRF'
                    case 0x4A424F44: ReadObjects(r, ObjectFlags.CSL3); break; // 'DOBJ'
                    case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                    case 0x4F4F5244: case 0x52524944: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'/'DIRR'
                    case 0x52435344: case 0x53524944: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'/'DIRS'
                    case 0x534F4344: case 0x43524944: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'/'DIRC'
                    case 0x5358414D: ReadMaxSizes(r, game, features, 6); break; // 'MAXS'
                    case 0x554F5344: case 0x4E524944: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'/'DIRN'
                    case 0x59524141: ArrayDefinitions = ReadIndexFile(r); break; // 'AARY'
                    default: Log.Info($"Unknown block {block:X2}"); break;
                }
            }
        }

        void Load7(FamilyGame game, BinaryReader r, Dictionary<string, object> detect) {
            var features = (Features)detect["features"];
            while (r.BaseStream.Position < r.BaseStream.Length) {
                var block = r.ReadUInt32();
                r.ReadUInt32E(); // size
                switch (block) {
                    case 0x52484344: case 0x46524944: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'/'DIRF'
                    case 0x4A424F44: ReadObjects(r, ObjectFlags.CSL4); break; // 'DOBJ'
                    case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                    case 0x4F4F5244: case 0x52524944: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'/'DIRR'
                    case 0x52435344: case 0x53524944: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'/'DIRS'
                    case 0x534F4344: case 0x43524944: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'/'DIRC'
                    case 0x5358414D: ReadMaxSizes(r, game, features, 7); break; // 'MAXS'
                    case 0x554F5344: case 0x4E524944: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'/'DIRN'
                    case 0x59524141: ArrayDefinitions = ReadIndexFile(r); break; // 'AARY'
                    case 0x4D414E41: AudioNames = ReadNames(r); break; // 'ANAM' - Used by: The Dig, FT
                    default: Log.Info($"Unknown block {block:X2}"); break;
                }
            }
        }

        void Load8(FamilyGame game, BinaryReader r, Dictionary<string, object> detect) {
            var features = (Features)detect["features"];
            while (r.BaseStream.Position < r.BaseStream.Length) {
                var block = r.ReadUInt32();
                r.ReadUInt32E(); // size
                switch (block) {
                    case 0x52484344: case 0x46524944: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'/'DIRF'
                    case 0x4A424F44: ReadObjects(r, ObjectFlags.CIL5); break; // 'DOBJ'
                    case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                    case 0x4F4F5244: case 0x52524944: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'/'DIRR'
                    case 0x52435344: case 0x53524944: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'/'DIRS'
                    case 0x43535244: RoomScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DRSC''
                    case 0x534F4344: case 0x43524944: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'/'DIRC'
                    case 0x5358414D: ReadMaxSizes(r, game, features, 8); break; // 'MAXS'
                    case 0x554F5344: case 0x4E524944: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'/'DIRN'
                    case 0x59524141: ArrayDefinitions = ReadIndexFile(r); break; // 'AARY'
                    case 0x4D414E41: AudioNames = ReadNames(r); break; // 'ANAM' - Used by: The Dig, FT
                    default: Log.Info($"Unknown block {block:X2}"); break;
                }
            }
        }

        #endregion
    }

    public class ResourceFile {
    }

    public override Task Read(BinaryAsset source, BinaryReader r, object tag) {
        var game = source.Game;
        var detect = source.Game.Detect<Dictionary<string, object>>("scumm", source.BlobPath, r, (p, s) => {
            s["variant"] = ((Dictionary<string, object>)p.Data["variants"])[(string)s["variant"]];
            s["features"] = ((string)s["features"]).Split(' ').Aggregate(Features.None, (a, f) => a |= (Features)Enum.Parse(typeof(Features), f, true));
            s["platform"] = (Platform)Enum.Parse(typeof(Platform), s.TryGetValue("platform", out var z) ? (string)z : "None", true);
            return s;
        }) ?? throw new FormatException("No Detect");
        // get index
        ResourceIndex index = new ResourceIndex(r, game, detect);

        // add files
        var files = new List<FileSource>(); source.Files = files;

        if (index.RoomResources != null) files.AddRange(index.RoomResources
            .Select((s, i) => new FileSource { Path = $"rooms/room{i:00}.dat", FileSize = s.Item1, Offset = s.Item2 }));
        if (index.ScriptResources != null) files.AddRange(index.ScriptResources
            .Select((s, i) => new FileSource { Path = $"scripts/script{i:000}.dat", FileSize = s.Item1, Offset = s.Item2 }));
        if (index.SoundResources != null) files.AddRange(index.SoundResources
            .Select((s, i) => new FileSource { Path = $"sounds/sound{i:000}.dat", FileSize = s.Item1, Offset = s.Item2 }));
        if (index.CostumeResources != null) files.AddRange(index.CostumeResources
            .Select((s, i) => new FileSource { Path = $"costumes/costume{i:00}.dat", FileSize = s.Item1, Offset = s.Item2 }));
        if (index.CharsetResources != null) files.AddRange(index.CharsetResources
            .Select((s, i) => new FileSource { Path = $"charsets/charset{i:002}.dat", FileSize = s.Item1, Offset = s.Item2 }));
        if (index.RoomScriptResources != null) files.AddRange(index.RoomScriptResources
            .Select((s, i) => new FileSource { Path = $"scripts/roomScript{i:00}.dat", FileSize = s.Item1, Offset = s.Item2 }));
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryAsset source, BinaryReader r, FileSource file, object option = default) {
        throw new NotImplementedException();
    }
}

#endregion

#region Binary_XX

public unsafe class Binary_XX : ArcBinary<Binary_XX> {
    public override Task Read(BinaryAsset source, BinaryReader r, object tag) {
        var files = source.Files = [];

        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryAsset source, BinaryReader r, FileSource file, object option = default) {
        throw new NotImplementedException();
    }
}

#endregion

#region Binary_Xga

// https://en.wikipedia.org/wiki/Color_Graphics_Adapter
// https://www.quora.com/What-is-the-difference-between-an-EGA-and-VGA-card-What-are-the-benefits-of-using-an-EGA-or-VGA-card-over-a-standard-VGA-card#:~:text=EGA%20(Enhanced%20Graphics%20Adapter)%20was,graphics%20and%20more%20detailed%20images.

public unsafe class Binary_Xga : IHaveMetaInfo, ITexture {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xga(r, f));

    #region Headers

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct X_Header {
        public static (string, int) Struct = ("<4B6H48c2B4H54c", sizeof(X_Header));
        public byte Magic;              // Fixed header field valued at a hexadecimal
        public byte Version;            // Version number referring to the Paintbrush software release
        public byte Encoding;           // Method used for encoding the image data
        public byte Bpp;                // Number of bits constituting one plane
        public ushort XMin;             // Minimum x co-ordinate of the image position
        public ushort YMin;             // Minimum y co-ordinate of the image position
        public ushort XMax;             // Maximum x co-ordinate of the image position
        public ushort YMax;             // Maximum y co-ordinate of the image position
        public ushort HDpi;             // Horizontal image resolution in DPI
        public ushort VDpi;             // Vertical image resolution in DPI
        public fixed byte Palette[48];  // EGA palette for 16-color images
        public byte Reserved1;          // First reserved field
        public byte BitPlanes;          // Number of color planes constituting the pixel data
        public ushort Bpr;              // Number of bytes of one color plane representing a single scan line
        public ushort Mode;             // Mode in which to construe the palette
        public ushort HRes;             // horizontal resolution of the source system's screen
        public ushort VRes;             // vertical resolution of the source system's screen
        public fixed byte Reserved2[54]; // Second reserved field, intended for future extension
    }

    #endregion

    public Binary_Xga(BinaryReader r, FileSource f) {
        Header = r.ReadS<X_Header>();
        if (Header.Magic != 0x0a) throw new FormatException("BAD MAGIC");
        Body = r.ReadToEnd();
    }

    X_Header Header;
    byte[] Body;

    #region ITexture
    static readonly object Format = (TextureFormat.RGBA32, TexturePixel.Unknown);
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //TextureUnityFormat.RGBA32,
    //TextureUnrealFormat.Unknown);
    public int Width { get; } = 320;
    public int Height { get; } = 200;
    public int Depth { get; } = 0;
    public int MipMaps { get; } = 1;
    public TextureFlags TexFlags { get; } = 0;
    public T Create<T>(string platform, Func<object, T> func) {
        //var bytes = Header.Bpp switch
        //{
        //    //8 => Decode8bpp(),
        //    //1 => Decode4bpp(),
        //    _ => throw new FormatException($"Unsupported bpp: {Header.Bpp}"),
        //};
        return func(new Texture_Bytes(null, Format, null)); // bytes;
    }
    #endregion

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
        new($"{nameof(Binary_Xga)}", items: [
            new($"Width: {Width}"),
            new($"Height: {Height}"),
        ])
    ];
}
#endregion