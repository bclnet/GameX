using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GameX.Formats.Compression;

namespace GameX.Mythic.Formats;

#region Binary_Crf

public unsafe class Binary_Crf : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Crf(r));

    #region Headers

    //const uint MW_BSAHEADER_FILEID = 0x00000100; // Magic for Morrowind BSA

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RealmInfo {
        public static (string, int) Struct = ("<5I", sizeof(RealmInfo));
        public uint RecipeCount;
        public uint CategoryCount;
        public uint RecipeListOffset;
        public uint CategoryListOffset;
        public uint ProfessionListOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header {
        public static (string, int) Struct = ("<4I15I", sizeof(Header));
        public uint Version;
        public uint StringsBlockSize;
        public uint StringsCount;
        public uint StringsOffset;
        public RealmInfo Realm0;
        public RealmInfo Realm1;
        public RealmInfo Realm2;
    }

    #endregion

    public Binary_Crf(BinaryReader r) {
        var magic = r.ReadUInt32();
        if (magic != 0x66 && magic != 0x67) throw new FormatException("BAD MAGIC");
        var header = r.ReadS<Header>();
        if (header.Version != 0x66 && header.Version != 0x67) throw new FormatException("BAD MAGIC");
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("BinaryPak", items: [
                //new($"Type: {Type}"),
            ])
    ];
}

#endregion

#region Binary_Mpk

public unsafe class Binary_Mpk : PakBinary<Binary_Mpk> {
    #region Headers

    const uint MAGIC = 0x4b41504d;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MPK_File {
        public static (string, int) Struct = ("<256sQ5I", 284);
        public fixed byte FileName[256];
        public ulong CreatedDate;
        public uint Unknown1;
        public uint FileSize;
        public uint Offset;
        public uint PackedSize;
        public uint Unknown2;
    }

    #endregion

    public override Task Read(BinaryPakFile source, BinaryReader r, object tag) {
        var magic = r.ReadUInt32();
        if (magic != MAGIC) throw new FormatException("BAD MAGIC");
        r.Seek(21);
        source.Tag = Encoding.ASCII.GetString(r.DecompressZlibStream()); // tag: ArchiveName
        var files = r.DecompressZlibStream(); var baseOffset = r.Tell();
        var s = new BinaryReader(new MemoryStream(files));
        source.Files = [.. s.ReadSArray<MPK_File>(files.Length / 284).Select(f => new FileSource {
            Path = UnsafeX.FixedAStringScan(f.FileName, 256),
            Offset = baseOffset + f.Offset,
            PackedSize = f.PackedSize,
            FileSize = f.FileSize,
            Date = new DateTime((long)(f.CreatedDate * 1000000L))
        })];
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default) {
        r.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(r.DecompressZlib((int)file.PackedSize, (int)file.FileSize)));
    }
}

#endregion
