using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Bioware.Formats;

#region Binary_Aurora

public unsafe class Binary_Aurora : PakBinary<Binary_Aurora> {
    // https://nwn2.fandom.com/wiki/File_formats

    #region Headers : KEY/BIF

    const uint KEY_MAGIC = 0x2059454b;
    const uint KEY_VERSION = 0x20203156;

    const uint BIFF_MAGIC = 0x46464942;
    const uint BIFF_VERSION = 0x20203156;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KEY_Header {
        public static (string, int) Struct = ("<7I32s", 60);
        public uint Version;            // Version ("V1  ")
        public uint NumFiles;           // Number of entries in FILETABLE
        public uint NumKeys;            // Number of entries in KEYTABLE.
        public uint FilesOffset;        // Offset to FILETABLE (0x440000).
        public uint KeysOffset;         // Offset to KEYTABLE.
        public uint BuildYear;          // Build year (less 1900).
        public uint BuildDay;           // Build day
        public fixed byte NotUsed02[32]; // Not used
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KEY_HeaderFile {
        public static (string, int) Struct = ("<2I2H", 12);
        public uint FileSize;           // BIF Filesize
        public uint FileNameOffset;     // Offset To BIF name
        public ushort FileNameSize;     // Size of BIF name
        public ushort Drives;           // A number that represents which drives the BIF file is located in
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KEY_HeaderFileName {
        public static (string, int) Struct = ("<16s", 16);
        public fixed byte Name[0x10];   // Null-padded string Resource Name (sans extension).
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KEY_HeaderKey {
        public static (string, int) Struct = ("<16sHI", 22);
        public fixed byte Name[0x10];   // Null-padded string Resource Name (sans extension).
        public ushort ResourceType;     // Resource Type
        public uint Id;                 // Resource ID
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BIFF_Header {
        public static (string, int) Struct = ("<4I", 16);
        public uint Version;            // Version ("V1  ")
        public uint NumFiles;           // File Count
        public uint NotUsed01;          // Not used
        public uint FilesOffset;        // Offset to FILETABLE
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BIFF_HeaderFile {
        public static (string, int) Struct = ("<4I", 16);
        public uint FileId;             // File ID
        public uint Offset;             // Offset to File Data.
        public uint FileSize;           // Size of File Data.
        public uint FileType;           // File Type
        public uint Id => (FileId & 0xFFF00000) >> 20; // BIF index
    }

    static readonly Dictionary<int, string> BIFF_FileTypes = new Dictionary<int, string> {
        {0x0000, "res"}, // Misc. GFF resources
        {0x0001, "bmp"}, // Microsoft Windows Bitmap
        {0x0002, "mve"},
        {0x0003, "tga"}, // Targa Graphics Format
        {0x0004, "wav"}, // Wave

        {0x0006, "plt"}, // Bioware Packed Layer Texture
        {0x0007, "ini"}, // Windows INI
        {0x0008, "mp3"}, // MP3
        {0x0009, "mpg"}, // MPEG
        {0x000A, "txt"}, // Text file
        {0x000B, "xml"},

        {0x07D0, "plh"},
        {0x07D1, "tex"},
        {0x07D2, "mdl"}, // Model
        {0x07D3, "thg"},

        {0x07D5, "fnt"}, // Font

        {0x07D7, "lua"}, // Lua script source code
        {0x07D8, "slt"},
        {0x07D9, "nss"}, // NWScript source code
        {0x07DA, "ncs"}, // NWScript bytecode
        {0x07DB, "mod"}, // Module
        {0x07DC, "are"}, // Area (GFF)
        {0x07DD, "set"}, // Tileset (unused in KOTOR?)
        {0x07DE, "ifo"}, // Module information
        {0x07DF, "bic"}, // Character sheet (unused)
        {0x07E0, "wok"}, // Walk-mesh
        {0x07E1, "2da"}, // 2-dimensional array
        {0x07E2, "tlk"}, // conversation file

        {0x07E6, "txi"}, // Texture information
        {0x07E7, "git"}, // Dynamic area information, game instance file, all area and objects that are scriptable
        {0x07E8, "bti"},
        {0x07E9, "uti"}, // item blueprint
        {0x07EA, "btc"},
        {0x07EB, "utc"}, // Creature blueprint

        {0x07ED, "dlg"}, // Dialogue
        {0x07EE, "itp"}, // tile blueprint pallet file
        {0x07EF, "btt"},
        {0x07F0, "utt"}, // trigger blueprint
        {0x07F1, "dds"}, // compressed texture file
        {0x07F2, "bts"},
        {0x07F3, "uts"}, // sound blueprint
        {0x07F4, "ltr"}, // letter combo probability info
        {0x07F5, "gff"}, // Generic File Format
        {0x07F6, "fac"}, // faction file
        {0x07F7, "bte"},
        {0x07F8, "ute"}, // encounter blueprint
        {0x07F9, "btd"},
        {0x07FA, "utd"}, // door blueprint
        {0x07FB, "btp"},
        {0x07FC, "utp"}, // placeable object blueprint
        {0x07FD, "dft"}, // default values file (text-ini)
        {0x07FE, "gic"}, // game instance comments
        {0x07FF, "gui"}, // GUI definition (GFF)
        {0x0800, "css"},
        {0x0801, "ccs"},
        {0x0802, "btm"},
        {0x0803, "utm"}, // store merchant blueprint
        {0x0804, "dwk"}, // door walkmesh
        {0x0805, "pwk"}, // placeable object walkmesh
        {0x0806, "btg"},

        {0x0808, "jrl"}, // Journal
        {0x0809, "sav"}, // Saved game (ERF)
        {0x080A, "utw"}, // waypoint blueprint
        {0x080B, "4pc"},
        {0x080C, "ssf"}, // sound set file

        {0x080F, "bik"}, // movie file (bik format)
        {0x0810, "ndb"}, // script debugger file
        {0x0811, "ptm"}, // plot manager/plot instance
        {0x0812, "ptt"}, // plot wizard blueprint
        {0x0813, "ncm"},
        {0x0814, "mfx"},
        {0x0815, "mat"},
        {0x0816, "mdb"}, // not the standard MDB, multiple file formats present despite same type
        {0x0817, "say"},
        {0x0818, "ttf"}, // standard .ttf font files
        {0x0819, "ttc"},
        {0x081A, "cut"}, // cutscene? (GFF)
        {0x081B, "ka"},  // karma file (XML)
        {0x081C, "jpg"}, // jpg image
        {0x081D, "ico"}, // standard windows .ico files
        {0x081E, "ogg"}, // ogg vorbis sound file
        {0x081F, "spt"},
        {0x0820, "spw"},
        {0x0821, "wfx"}, // woot effect class (XML)
        {0x0822, "ugm"}, // 2082 ?? [textures00.bif]
        {0x0823, "qdb"}, // quest database (GFF v3.38)
        {0x0824, "qst"}, // quest (GFF)
        {0x0825, "npc"}, // spawn point? (GFF)
        {0x0826, "spn"},
        {0x0827, "utx"},
        {0x0828, "mmd"},
        {0x0829, "smm"},
        {0x082A, "uta"}, // uta (GFF)
        {0x082B, "mde"},
        {0x082C, "mdv"},
        {0x082D, "mda"},
        {0x082E, "mba"},
        {0x082F, "oct"},
        {0x0830, "bfx"},
        {0x0831, "pdb"},
        {0x0832, "TheWitcherSave"},
        {0x0833, "pvs"},
        {0x0834, "cfx"},
        {0x0835, "luc"}, // compiled lua script

        {0x0837, "prb"},
        {0x0838, "cam"},
        {0x0839, "vds"},
        {0x083A, "bin"},
        {0x083B, "wob"},
        {0x083C, "api"},
        {0x083D, "properties"},
        {0x083E, "png"},

        {0x270B, "big"},

        {0x270D, "erf"}, // Encapsulated Resource Format
        {0x270E, "bif"},
        {0x270F, "key"},
    };

    #endregion

    public override Task Read(BinaryPakFile source, BinaryReader r, object tag) {
        FileSource[] files; List<FileSource> files2;

        // KEY
        var magic = source.Magic = r.ReadUInt32();
        if (magic == KEY_MAGIC) // Signature("KEY ")
        {
            var header = r.ReadS<KEY_Header>();
            if (header.Version != KEY_VERSION) throw new FormatException("BAD MAGIC");
            source.Version = header.Version;
            source.Files = files = new FileSource[header.NumFiles];

            // parts
            r.Seek(header.FilesOffset);
            var headerFiles = r.ReadSArray<KEY_HeaderFile>((int)header.NumFiles).Select(x => {
                r.Seek(x.FileNameOffset);
                return (file: x, path: r.ReadFAString(x.FileNameSize - 1));
            }).ToArray();
            r.Seek(header.KeysOffset);
            var headerKeys = r.ReadSArray<KEY_HeaderKey>((int)header.NumKeys).ToDictionary(x => x.Id, x => UnsafeX.FixedAString(x.Name, 0x10));

            // combine
            var subPathFormat = Path.Combine(Path.GetDirectoryName(source.PakPath), "{0}");
            for (var i = 0; i < header.NumFiles; i++) {
                var (file, path) = headerFiles[i];
                var subPath = string.Format(subPathFormat, path);
                if (!File.Exists(subPath)) continue;
                files[i] = new FileSource {
                    Path = path,
                    FileSize = file.FileSize,
                    Pak = new SubPakFile(source, null, subPath, (headerKeys, (uint)i)),
                };
            }
        }
        // BIFF
        else if (magic == BIFF_MAGIC) // Signature("BIFF")
        {
            if (source.Tag == null) throw new FormatException("BIFF files can only be processed through KEY files");
            var (keys, bifId) = ((Dictionary<uint, string> keys, uint bifId))source.Tag;
            var header = r.ReadS<BIFF_Header>();
            if (header.Version != BIFF_VERSION) throw new FormatException("BAD MAGIC");
            source.Version = header.Version;
            source.Files = files2 = [];

            // files
            r.Seek(header.FilesOffset);
            var headerFiles = r.ReadSArray<BIFF_HeaderFile>((int)header.NumFiles);
            for (var i = 0; i < headerFiles.Length; i++) {
                var headerFile = headerFiles[i];
                if (headerFile.Id > i) continue;
                var path = $"{(keys.TryGetValue(headerFile.Id, out var key) ? key : $"{i}")}{(BIFF_FileTypes.TryGetValue((int)headerFile.FileType, out var z) ? $".{z}" : string.Empty)}".Replace('\\', '/');
                files2.Add(new FileSource {
                    Id = (int)headerFile.Id,
                    Path = path,
                    FileSize = headerFile.FileSize,
                    Offset = headerFile.Offset,
                });
            }
        }
        else throw new FormatException($"Unknown File Type {magic}");
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default) {
        Stream fileData;
        r.Seek(file.Offset);
        if (source.Version == BIFF_VERSION) fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
        else throw new ArgumentOutOfRangeException(nameof(source.Version), $"{source.Version}");
        return Task.FromResult(fileData);
    }
}

#endregion

#region Binary_Myp

public unsafe class Binary_Myp : PakBinary<Binary_Myp> {
    #region Headers

    const uint MYP_MAGIC = 0x0050594d;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MYP_Header {
        public static (string, int) Struct = ("<3IQ4I", 36);
        public uint Magic;              // "MYP\0"
        public uint Version;            // Version
        public uint Bom;                // Byte order marker
        public ulong TableOffset;       // Number of entries in FILETABLE
        public uint TableCapacity;      // Number of files
        public uint TotalFiles;         // Number of entries in FILETABLE
        public uint Unk1;               //
        public uint Unk2;               //

        public void Verify() {
            if (Magic != MYP_MAGIC) throw new FormatException("Not a .tor file (Wrong file header)");
            if (Version != 5 && Version != 6) throw new FormatException($"Only versions 5 and 6 are supported, file has {Version}");
            if (Bom != 0xfd23ec43) throw new FormatException("Unexpected byte order");
            if (TableOffset == 0) throw new FormatException("File is empty");
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MYP_HeaderFile {
        public static (string, int) Struct = ("<Q3IQIH", 34);
        public ulong Offset;            //
        public uint HeaderSize;         //
        public uint PackedSize;         //
        public uint FileSize;           //
        public ulong Digest;            //
        public uint Crc;                //
        public ushort Compressed;       //
    }

    #endregion

    public override Task Read(BinaryPakFile source, BinaryReader r, object tag) {
        var files = source.Files = [];
        var hashLookup = source.Game.Id switch {
            "SWTOR" => TOR.HashLookup,
            "WAR" => WAR.HashLookup,
            _ => []
        };

        var header = r.ReadS<MYP_Header>();
        header.Verify();
        source.Version = header.Version;

        var tableOffset = (long)header.TableOffset;
        while (tableOffset != 0) {
            r.Seek(tableOffset);

            var numFiles = r.ReadInt32();
            if (numFiles == 0) break;
            tableOffset = r.ReadInt64();

            var headerFiles = r.ReadSArray<MYP_HeaderFile>(numFiles);
            for (var i = 0; i < headerFiles.Length; i++) {
                var headerFile = headerFiles[i];
                if (headerFile.Offset == 0) continue;
                var hash = headerFile.Digest;
                var path = hashLookup.TryGetValue(hash, out var z) ? z.Replace('\\', '/') : $"{hash:X2}.bin";
                files.Add(new FileSource {
                    Id = i,
                    Path = path.StartsWith('/') ? path[1..] : path,
                    FileSize = headerFile.FileSize,
                    PackedSize = headerFile.PackedSize,
                    Offset = (long)(headerFile.Offset + headerFile.HeaderSize),
                    Hash = hash,
                    Compressed = headerFile.Compressed
                });
            }
        }
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default) {
        if (file.FileSize == 0) return Task.FromResult(System.IO.Stream.Null);
        r.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(file.Compressed == 0
            ? r.ReadBytes((int)file.PackedSize)
            : source.Version switch {
                6 => r.DecompressZstd((int)file.PackedSize, (int)file.FileSize),
                _ => r.DecompressZlib((int)file.PackedSize, (int)file.FileSize),
            }));
    }
}

#endregion

#region Binary_Gff

public unsafe class Binary_Gff : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Gff(r));

    #region Headers

    const uint GFF_VERSION3_2 = 0x322e3356; // literal string "V3.2".
    const uint GFF_VERSION3_3 = 0x332e3356; // literal string "V3.3".

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct GFF_Header {
        public uint Version;            // Version ("V3.3")
        public uint StructOffset;       // Offset of Struct array as bytes from the beginning of the file
        public uint StructCount;        // Number of elements in Struct array
        public uint FieldOffset;        // Offset of Field array as bytes from the beginning of the file
        public uint FieldCount;         // Number of elements in Field array
        public uint LabelOffset;        // Offset of Label array as bytes from the beginning of the file
        public uint LabelCount;         // Number of elements in Label array
        public uint FieldDataOffset;    // Offset of Field Data as bytes from the beginning of the file
        public uint FieldDataSize;      // Number of bytes in Field Data block
        public uint FieldIndicesOffset; // Offset of Field Indices array as bytes from the beginning of the file
        public uint FieldIndicesSize;   // Number of bytes in Field Indices array
        public uint ListIndicesOffset;  // Offset of List Indices array as bytes from the beginning of the file
        public uint ListIndicesSize;    // Number of bytes in List Indices array
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct GFF_Struct {
        public uint Id;                 // Programmer-defined integer ID.
        public uint DataOrDataOffset;   // If FieldCount = 1, this is an index into the Field Array.
                                        // If FieldCount > 1, this is a byte offset into the Field Indices array.
        public uint FieldCount;         // Number of fields in this Struct.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct GFF_Field {
        public uint Type;               // Data type
        public uint LabelIndex;         // Index into the Label Array
        public uint DataOrDataOffset;   // If Type is a simple data type, then this is the value actual of the field.
                                        // If Type is a complex data type, then this is a byte offset into the Field Data block.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct GFF_Label {
        public fixed byte Name[0x10];     // Label
    }

    #endregion

    public enum DataType : uint {
        DLG = 0x20474c44,
        QDB = 0x20424451,
        QST = 0x20545351,
    }

    public DataType Type { get; private set; }
    public IDictionary<string, object> Root { get; private set; }
    public IDictionary<uint, object> Index { get; private set; }

    public class ResourceRef {
        public string Name;
    }

    public class LocalizedRef {
        public uint DialogID;
        public (uint id, string value)[] Values;
    }

    public Binary_Gff(BinaryReader r) {
        Type = (DataType)r.ReadUInt32();
        var header = r.ReadS<GFF_Header>();
        if (header.Version != GFF_VERSION3_2 && header.Version != GFF_VERSION3_3) throw new FormatException("BAD MAGIC");
        r.Seek(header.StructOffset);
        var headerStructs = r.ReadSArray<GFF_Struct>((int)header.StructCount);
        var index = new Dictionary<uint, object>();
        var structs = new IDictionary<string, object>[header.StructCount];
        for (var i = 0; i < structs.Length; i++) {
            var id = headerStructs[i].Id;
            var s = structs[i] = new Dictionary<string, object>();
            if (id == 0) continue;
            s.Add("_", id);
            index.Add(id, s);
        }
        r.Seek(header.FieldOffset);
        var headerFields = r.ReadSArray<GFF_Field>((int)header.FieldCount).Select<GFF_Field, (uint label, object value)>(x => {
            switch (x.Type) {
                case 0: return (x.LabelIndex, (byte)x.DataOrDataOffset);    //: Byte
                case 1: return (x.LabelIndex, (char)x.DataOrDataOffset);    //: Char
                case 2: return (x.LabelIndex, (ushort)x.DataOrDataOffset);  //: Word
                case 3: return (x.LabelIndex, (short)x.DataOrDataOffset);   //: Short
                case 4: return (x.LabelIndex, x.DataOrDataOffset);          //: DWord
                case 5: return (x.LabelIndex, (int)x.DataOrDataOffset);     //: Int
                case 8: return (x.LabelIndex, BitConverter.ToSingle(BitConverter.GetBytes(x.DataOrDataOffset), 0)); //: Float
                case 14: return (x.LabelIndex, structs[x.DataOrDataOffset]); //: Struct
                case 15: //: List
                    r.Seek(header.ListIndicesOffset + x.DataOrDataOffset);
                    var list = new IDictionary<string, object>[(int)r.ReadUInt32()];
                    for (var i = 0; i < list.Length; i++) {
                        var idx = r.ReadUInt32();
                        if (idx >= structs.Length) throw new Exception();
                        list[i] = structs[idx];
                    }
                    return (x.LabelIndex, list);
            }
            r.Seek(header.FieldDataOffset + x.DataOrDataOffset);
            switch (x.Type) {
                case 6: return (x.LabelIndex, r.ReadUInt64());              //: DWord64
                case 7: return (x.LabelIndex, r.ReadInt64());               //: Int64
                case 9: return (x.LabelIndex, r.ReadDouble());              //: Double
                case 10: return (x.LabelIndex, r.ReadL32UString());           //: CExoString
                case 11: return (x.LabelIndex, new ResourceRef { Name = r.ReadL8UString() }); //: ResRef
                case 12: //: CExoLocString
                    r.Skip(4);
                    var dialogID = r.ReadUInt32();
                    var values = new (uint id, string value)[r.ReadUInt32()];
                    for (var i = 0; i < values.Length; i++) values[i] = (r.ReadUInt32(), r.ReadL32UString());
                    return (x.LabelIndex, new LocalizedRef { DialogID = dialogID, Values = values });
                case 13: return (x.LabelIndex, r.ReadBytes((int)r.ReadUInt32()));
            }
            throw new ArgumentOutOfRangeException(nameof(x.Type), x.Type.ToString());
        }).ToArray();
        r.Seek(header.LabelOffset);
        var headerLabels = r.ReadSArray<GFF_Label>((int)header.LabelCount).Select(x => UnsafeX.FixedAString(x.Name, 0x10)).ToArray();
        // combine
        for (var i = 0; i < structs.Length; i++) {
            var fieldCount = headerStructs[i].FieldCount;
            var dataOrDataOffset = headerStructs[i].DataOrDataOffset;
            if (fieldCount == 1) {
                var (label, value) = headerFields[dataOrDataOffset];
                structs[i].Add(headerLabels[label], value);
                continue;
            }
            var fields = structs[i];
            r.Seek(header.FieldIndicesOffset + dataOrDataOffset);
            foreach (var idx in r.ReadPArray<uint>("I", (int)fieldCount)) {
                var (label, value) = headerFields[idx];
                fields.Add(headerLabels[label], value);
            }
        }
        Root = structs[0];
        Index = index;
    }

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("BinaryGFF", items: [
            new($"Type: {Type}"),
        ])
    ];
}

#endregion