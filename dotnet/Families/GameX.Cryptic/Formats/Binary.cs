using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Cryptic.Formats;

#region Binary_Bin

public unsafe class Binary_Bin : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Bin(r));

    #region Headers

    const ulong MAGIC = 0x5363697470797243;
    const string PARSE_N = "ParseN";
    const string PARSE_M = "ParseM";

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header_M {
        public ulong Magic;                     // CrypticS
        public ushort ParseHash;                // 
        public ushort Flags;                    // 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header_N {
        public ulong Magic;                     // CrypticS
        public uint ParseHash;                  // 
        public uint Flags;                      // 
    }

    #endregion

    public Binary_Bin(BinaryReader r) {
        var header = r.ReadS<Header_M>();
        if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");
        var type = r.ReadL16WString(maxLength: 4096); r.Align();
        if (type != PARSE_M) throw new FormatException("BAD TYPE");

        // file section
        var filesTag = r.ReadL16WString(20); r.Align();
        if (filesTag != "Files1") throw new FormatException("BAD Tag");
        var fileSectionEnd = r.ReadUInt32() + r.Tell();
        var files = r.ReadL32FArray(x => {
            var name = x.ReadL16WString(maxLength: 260); x.Align();
            var timestamp = x.ReadUInt32();
            return (name, timestamp);
        });
        if (r.Tell() != fileSectionEnd) throw new FormatException("did not read blob file entry correctly");

        // extra section
        var extraTag = r.ReadL16WString(20); r.Align();
        if (extraTag != "Files1") throw new FormatException("BAD Tag");
        var extraSectionEnd = r.ReadUInt32() + r.Tell();
        var extras = r.ReadL32FArray(x => {
            return (string)null;
        });
        if (r.Tell() != extraSectionEnd) throw new FormatException("did not read blob file entry correctly");

        // dependency section
        var dependencyTag = r.ReadL16WString(20); r.Align();
        if (dependencyTag != "Depen1") throw new FormatException("BAD Tag");
        var dependencySectionEnd = r.ReadUInt32() + r.Tell();
        var dependencys = r.ReadL32FArray(x => {
            var type = x.ReadUInt32();
            var name = x.ReadL16WString(maxLength: 260); x.Align();
            var hash = x.ReadUInt32();
            return (type, name, hash);
        });
        if (r.Tell() != dependencySectionEnd) throw new FormatException("did not read blob file entry correctly");
    }

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) {
        var nodes = new List<MetaInfo> {
            new MetaInfo("BinaryBin", items: new List<MetaInfo> {
                //new MetaInfo($"Type: {Type}"),
            })
        };
        return nodes;
    }
}

#endregion

#region Binary_Hogg
// https://github.com/nohbdy/libhogg

public unsafe class Binary_Hogg : PakBinary<Binary_Hogg> {
    // Headers
    #region Headers

    const uint MAGIC = 0xDEADF00D; // DEADF00D

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header {
        public uint Magic;                      // DEADF00D
        public ushort Version;                  // Version == 0x0400
        public ushort OperationJournalSection;  // Size of the operation journal section == 0x000A
        public uint FileEntrySection;           // Size of the file entry section
        public uint AttributeEntrySection;      // Size of the attribute entry section
        public uint DataFileNumber;             // 
        public uint FileJournalSection;         // Size of the file journal section == 0x000A
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FileJournalHeader {
        public uint Unknown1;                   // Unknown
        public uint Size;                      // Size
        public uint Size2;                      // Size
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FileEntry {
        public long Offset;                     // offset to the file's data
        public int FileSize;                    // size of the filedata within the archive
        public uint Timestamp;                  // 32-bit timestamp (seconds since 1970)
        public uint Checksum;                   // checksum
        public uint Unknown4;                   // Unknown
        public ushort Unknown5;                 // Unknown
        public ushort Unknown6;                 // Unknown
        public int Id;                          // Id
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AttributeEntry {
        public int PathId;                      // Data ID of the file's path
        public int ExcerptId;                   // Data ID of a data excerpt, or -1 if there is none
        public uint UncompressedSize;           // Size of the data after decompression, or 0 if the file is not compressed
        public uint Flags;                      // Flags
    }

    #endregion

    public override Task Read(BinaryPakFile source, BinaryReader r, object tag) {
        // read header
        var header = r.ReadS<Header>();
        if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");
        if (header.Version < 10 || header.Version > 11) throw new FormatException("BAD Version");
        if (header.OperationJournalSection > 1024) throw new FormatException("BAD Journal");
        if (header.FileEntrySection != header.AttributeEntrySection << 1) throw new FormatException("data entry / compression info section size mismatch");
        var numFiles = (int)(header.AttributeEntrySection >> 4);

        // skip journals
        r.Skip(header.OperationJournalSection);
        var fileJournalPosition = r.BaseStream.Position;
        r.Skip(header.FileJournalSection);

        // read files
        var fileEntries = r.ReadSArray<FileEntry>(numFiles);
        var attributeEntries = r.ReadSArray<AttributeEntry>(numFiles);
        var files = new FileSource[numFiles];
        for (var i = 0; i < files.Length; i++) {
            ref FileEntry s = ref fileEntries[i];
            ref AttributeEntry a = ref attributeEntries[i];
            files[i] = new FileSource {
                Id = s.Id,
                Offset = s.Offset,
                FileSize = s.FileSize,
                PackedSize = a.UncompressedSize,
                Compressed = a.UncompressedSize > 0 ? 1 : 0,
            };
        }

        // read "Datalist" file
        var dataListFile = files[0];
        if (dataListFile.Id != 0 || dataListFile.FileSize == -1) throw new FormatException("BAD DataList");
        var fileAttribs = new Dictionary<int, byte[]>();
        using (var r2 = new BinaryReader(ReadData(source, r, dataListFile).Result)) {
            if (r2.ReadUInt32() != 0) throw new FormatException("BAD DataList");
            var count = r2.ReadInt32();
            for (var i = 0; i < count; i++) fileAttribs.Add(i, r2.ReadBytes((int)r2.ReadUInt32()));
        }

        // read file journal
        r.Seek(fileJournalPosition);
        var fileJournalHeader = r.ReadS<FileJournalHeader>();
        var endPosition = r.BaseStream.Position + fileJournalHeader.Size;
        while (r.BaseStream.Position < endPosition) {
            var action = r.ReadByte();
            var targetId = r.ReadInt32();
            switch (action) {
                case 1: fileAttribs[targetId] = r.ReadBytes((int)r.ReadUInt32()); break;
                case 2: fileAttribs.Remove(targetId); break;
            }
        }

        // assign file path
        for (var i = 0; i < files.Length; i++) {
            var file = files[i];
            file.Path = Encoding.ASCII.GetString(fileAttribs[attributeEntries[i].PathId][..^1]);
            if (file.Path.EndsWith(".hogg", StringComparison.OrdinalIgnoreCase)) file.Pak = new SubPakFile(source, file, file.Path);
        }

        // remove filesize of -1 and file 0
        source.Files = files.Where(x => x.FileSize != -1 && x.Id != 0).ToList();
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default) {
        r.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(file.Compressed != 0
            ? r.DecompressZlib((int)file.PackedSize, (int)file.FileSize)
            : r.ReadBytes((int)file.FileSize)));
    }
}

#endregion

#region Binary_MSet

public unsafe class Binary_MSet : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MSet(r));

    #region MSet

    public class MSetFile {
        public MSetFileHeader Header;
        public MSetSourceFileSet SourceFileSet;
        public MSetModel[] Models;
    }

    public class MSetFileHeader {
        public int HeaderSize;
        public int FileVersion;
        public int DataCrc;
        public MSetModelDefinition[] ModelDefinitions;
    }

    public class MSetModel {
        public int DataSize; // The size of this structure plus all persistent packed data
        public int VertexCount;
        public int FaceCount;
        public int TextureCount; // number of tex_idxs (sum of all tex_idx->counts == tri_count)
        public float AverageTexelDensity;
        public float StdDevTexelDensity;
        public int ProcessTimeFlags;
        public int Unknown1C;
        public MSetModelDataOffset[] ModelDataOffsets;
        public string _ModelName; // ignore
    }

    public class MSetModelDataOffset {
        public int CompressedSize;
        public int DecompressedSize;
        public int Offset;
        public bool IsEncoded;
        public byte[] Data;

        internal MSetModelDataOffset ReadData(BinaryReader r, int masterOffset) {
            if (Offset <= 0) return this;
            var len = CompressedSize;
            if (len == 0 && DecompressedSize > 0) len = DecompressedSize;
            r.Seek(Offset + masterOffset);
            Data = r.ReadBytes(len);
            return this;
        }
    }

    public class MSetModelDefinition {
        public string ModelName;
        public MSetModelOffset[] ModelOffsets;
    }

    public struct MSetModelOffset {
        public static (string, int) Struct = (">2i", sizeof(MSetModelOffset));
        public int Offset;
        public int Length;
    }

    public class MSetSourceFilePath {
        public string Path;
        public int Timestamp;
    }

    public class MSetSourceFileSet {
        public string FileSetName;
        public int SetLength;
        public MSetSourceFilePath[] Files;
    }

    #endregion

    #region Headers

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header {
        public static (string, int) Struct = (">4x2i", sizeof(Header));
        public int HeaderSize;
        public int FileVersion;
        public int DataCrc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ModelOffset {
        public static (string, int) Struct = (">4i2f2i", sizeof(ModelOffset));
        public int DataSize;
        public int VertexCount;
        public int FaceCount;
        public int TextureCount;
        public float AverageTexelDensity;
        public float StdDevTexelDensity;
        public int ProcessTimeFlags;
        public int Unknown1C;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ModelDataOffset {
        public static (string, int) Struct = (">3i", sizeof(ModelDataOffset));
        public int CompressedSize;
        public int DecompressedSize;
        public int Offset;
    }

    #endregion

    MSetFile Mset;

    public Binary_MSet(BinaryReader r) {
        string p;
        var header = r.ReadS<Header>();
        var mh = new MSetFileHeader {
            HeaderSize = header.HeaderSize,
            FileVersion = header.FileVersion,
            DataCrc = header.DataCrc,
            ModelDefinitions = r.ReadL16FArray(r1 => new MSetModelDefinition {
                ModelName = r1.ReadL16AString(endian: true),
                ModelOffsets = r1.ReadL16SArray<MSetModelOffset>(endian: true),
            }, endian: true)
        };
        r.Skip(3 * sizeof(int));
        static int PaddingSize(int len) => (4 - ((len + 6) & 3)) & 3;
        var msf = new MSetSourceFileSet {
            FileSetName = r.ReadL16AString(),
            SetLength = r.ReadInt32(),
            Files = r.ReadL32FArray(r1 => new MSetSourceFilePath {
                Path = p = r1.ReadL16AString(),
                Timestamp = r1.Skip(PaddingSize(p.Length)).ReadInt32(),
            }),
        };
        var mdls = mh.ModelDefinitions.SelectMany(x => x.ModelOffsets, (model, offset) => {
            if (offset.Offset <= 0) return null;
            r.Seek(offset.Offset);
            var m = r.ReadS<ModelOffset>();
            return new MSetModel {
                DataSize = m.DataSize,
                VertexCount = m.VertexCount,
                FaceCount = m.FaceCount,
                TextureCount = m.TextureCount,
                AverageTexelDensity = m.AverageTexelDensity,
                StdDevTexelDensity = m.StdDevTexelDensity,
                ProcessTimeFlags = m.ProcessTimeFlags,
                Unknown1C = m.Unknown1C,
                ModelDataOffsets = r.ReadSArray<ModelDataOffset>(12).Select(x => new MSetModelDataOffset {
                    CompressedSize = x.CompressedSize,
                    DecompressedSize = Math.Abs(x.DecompressedSize),
                    Offset = x.Offset,
                    IsEncoded = x.DecompressedSize > 0
                }.ReadData(r, offset.Offset)).ToArray(),
                _ModelName = model.ModelName,
            };
        }).Where(x => x != null).ToArray();
        Mset = new MSetFile {
            Header = mh,
            SourceFileSet = msf,
            Models = mdls,
        };
    }

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("UnknownFileModel", items: [
            //new($"Type: {Type}"),
        ])
    ];
}

#endregion

#region Binary_Tex

public class Binary_Tex : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Tex(r, (int)f.FileSize));

    public string Data;

    public Binary_Tex(BinaryReader r, int fileSize) {
        Data = r.ReadFUString(fileSize);
    }

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("BinaryBin", items: [
            //new($"Type: {Type}"),
        ])
    ];
}

#endregion