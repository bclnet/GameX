using GameX.Bethesda.Formats.Records;
using GameX.Uncore.Formats;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Bethesda.Formats;

#region Binary_Ba2
// https://en.uesp.net/wiki/Bethesda5Mod:Archive_File_Format

public unsafe class Binary_Ba2 : ArcBinary<Binary_Ba2> {
    #region Headers : TES5

    // Default header data
    const uint F4_MAGIC = 0x58445442; // Magic for Fallout 4 BA2, the literal string "BTDX".
    const uint F4_VERSION1 = 0x01; // Version number of a Fallout 4 BA2
    const uint F4_VERSION2 = 0x02; // Version number of a Starfield BA2
    public enum HDR5Type : uint { GNRL = 0x4c524e47, DX10 = 0x30315844, GNMF = 0x464d4e47 }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HDR5 {
        public static (string, int) Struct = ("<3IQ", sizeof(HDR5));
        public uint Version;            // 04
        public HDR5Type Type;           // 08 GNRL=General, DX10=Textures, GNMF=?, ___=?
        public uint NumFiles;           // 0C
        public ulong NameTableOffset;   // 10 - relative to start of file
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FILE5 {
        public static (string, int) Struct = ("<4IQ3I", sizeof(FILE5));
        public uint NameHash;           // 00
        public uint Ext;                // 04 - extension
        public uint DirHash;            // 08
        public uint Flags;              // 0C - flags? 00100100
        public ulong Offset;            // 10 - relative to start of file
        public uint PackedSize;         // 18 - packed length (zlib)
        public uint FileSize;           // 1C - unpacked length
        public uint Align;              // 20 - BAADF00D
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TEX5 {
        public static (string, int) Struct = ("<3I2B3H4B", sizeof(TEX5));
        public uint NameHash;           // 00
        public uint Ext;                // 04 - extension
        public uint DirHash;            // 08
        public byte Unk0C;              // 0C
        public byte NumChunks;          // 0D
        public ushort ChunkHeaderSize;  // 0E - size of one chunk header
        public ushort Height;           // 10
        public ushort Width;            // 12
        public byte NumMips;            // 14
        public byte Format;             // 15 - DXGI_FORMAT
        public byte IsCubemap;          // 16
        public byte TileMode;           // 17
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TEXCHUNK5 {
        public static (string, int) Struct = ("<Q2I2HI", sizeof(TEXCHUNK5));
        public ulong Offset;            // 00
        public uint PackedSize;         // 08
        public uint FileSize;           // 0C
        public ushort StartMip;         // 10
        public ushort EndMip;           // 12
        public uint Align;              // 14 - BAADFOOD
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GNMF5 {
        public static (string, int) Struct = ("<3I2BH32sQ4I", sizeof(GNMF5));
        public uint NameHash;           // 00
        public uint Ext;                // 04 - extension
        public uint DirHash;            // 08
        public byte Unk0C;              // 0C
        public byte NumChunks;          // 0D
        public ushort Unk0E;            // 0E
        public fixed byte Header[32];   // 10
        public ulong Offset;            // 30
        public uint PackedSize;         // 38
        public uint FileSize;           // 3C
        public uint Unk40;              // 40
        public uint Align;              // 44
    }

    #endregion

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        FileSource[] files;

        // Fallout 4 - Starfield
        var magic = source.Magic = r.ReadUInt32();
        if (magic == F4_MAGIC) {
            var header = r.ReadS<HDR5>();
            if (header.Version > F4_VERSION2) throw new FormatException("BAD MAGIC");
            source.Version = header.Version;
            source.Files = files = new FileSource[header.NumFiles];
            // version2
            //if (header.Version == F4_VERSION2) r.Skip(8);

            switch (header.Type) {
                // General BA2 Format
                case HDR5Type.GNRL:
                    var headerFiles = r.ReadSArray<FILE5>((int)header.NumFiles);
                    for (var i = 0; i < headerFiles.Length; i++) {
                        ref FILE5 headerFile = ref headerFiles[i];
                        files[i] = new FileSource {
                            Compressed = headerFile.PackedSize != 0 ? 1 : 0,
                            PackedSize = headerFile.PackedSize,
                            FileSize = headerFile.FileSize,
                            Offset = (long)headerFile.Offset,
                        };
                    }
                    break;
                // Texture BA2 Format
                case HDR5Type.DX10:
                    for (var i = 0; i < header.NumFiles; i++) {
                        var headerTexture = r.ReadS<TEX5>();
                        var headerTextureChunks = r.ReadSArray<TEXCHUNK5>(headerTexture.NumChunks);
                        ref TEXCHUNK5 firstChunk = ref headerTextureChunks[0];
                        files[i] = new FileSource {
                            PackedSize = firstChunk.PackedSize,
                            FileSize = firstChunk.FileSize,
                            Offset = (long)firstChunk.Offset,
                            Tag = ((object)headerTexture, headerTextureChunks),
                        };
                    }
                    break;
                // GNMF BA2 Format
                case HDR5Type.GNMF:
                    for (var i = 0; i < header.NumFiles; i++) {
                        var headerGNMF = r.ReadS<GNMF5>();
                        var headerTextureChunks = r.ReadSArray<TEXCHUNK5>(headerGNMF.NumChunks);
                        files[i] = new FileSource {
                            PackedSize = headerGNMF.PackedSize,
                            FileSize = headerGNMF.FileSize,
                            Offset = (long)headerGNMF.Offset,
                            Tag = ((object)headerGNMF, headerTextureChunks),
                        };
                    }
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(header.Type), header.Type.ToString());
            }

            // assign full names to each file
            if (header.NameTableOffset > 0) {
                r.Seek((long)header.NameTableOffset);
                var path = r.ReadL16UString().Replace('\\', '/');
                foreach (var file in files) file.Path = path;
            }
        }
        else throw new InvalidOperationException("BAD MAGIC");
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        const int GNF_MAGIC = 0x20464E47;
        const int GNF_CONTENTSIZE = 248;

        // position
        r.Seek(file.Offset);

        // General BA2 Format
        if (file.Tag == null)
            return Task.FromResult<Stream>(file.Compressed != 0
                ? new MemoryStream(r.DecompressZlib2((int)file.PackedSize, (int)file.FileSize))
                : new MemoryStream(r.ReadBytes((int)file.FileSize)));

        var tag = ((object tex, TEXCHUNK5[] chunks))file.Tag;

        // Texture BA2 Format
        if (tag.tex is TEX5 tex) {
            var s = new MemoryStream();
            // write header
            var w = new BinaryWriter(s);
            var ddsHeader = new DDS_HEADER {
                dwSize = DDS_HEADER.SizeOf,
                dwFlags = DDSD.HEADER_FLAGS_TEXTURE | DDSD.HEADER_FLAGS_LINEARSIZE | DDSD.HEADER_FLAGS_MIPMAP,
                dwHeight = tex.Height,
                dwWidth = tex.Width,
                dwMipMapCount = tex.NumMips,
                dwCaps = DDSCAPS.SURFACE_FLAGS_TEXTURE | DDSCAPS.SURFACE_FLAGS_MIPMAP,
                dwCaps2 = tex.IsCubemap == 1 ? DDSCAPS2.CUBEMAP_ALLFACES : 0,
            };
            ddsHeader.ddspf.dwSize = DDS_PIXELFORMAT.SizeOf;
            switch ((DXGI_FORMAT)tex.Format) {
                case DXGI_FORMAT.BC1_UNORM:
                    ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                    ddsHeader.ddspf.dwFourCC = FourCC.DXT1;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height / 2U); // 4bpp
                    break;
                case DXGI_FORMAT.BC2_UNORM:
                    ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                    ddsHeader.ddspf.dwFourCC = FourCC.DXT3;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                    break;
                case DXGI_FORMAT.BC3_UNORM:
                    ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                    ddsHeader.ddspf.dwFourCC = FourCC.DXT5;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                    break;
                case DXGI_FORMAT.BC5_UNORM:
                    ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                    ddsHeader.ddspf.dwFourCC = FourCC.ATI2;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                    break;
                case DXGI_FORMAT.BC1_UNORM_SRGB:
                    ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                    ddsHeader.ddspf.dwFourCC = FourCC.DX10;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height / 2); // 4bpp
                    break;
                case DXGI_FORMAT.BC3_UNORM_SRGB:
                case DXGI_FORMAT.BC4_UNORM:
                case DXGI_FORMAT.BC5_SNORM:
                case DXGI_FORMAT.BC7_UNORM:
                case DXGI_FORMAT.BC7_UNORM_SRGB:
                    ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                    ddsHeader.ddspf.dwFourCC = FourCC.DX10;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                    break;
                case DXGI_FORMAT.R8G8B8A8_UNORM:
                case DXGI_FORMAT.R8G8B8A8_UNORM_SRGB:
                    ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                    ddsHeader.ddspf.dwRGBBitCount = 32;
                    ddsHeader.ddspf.dwRBitMask = 0x000000FF;
                    ddsHeader.ddspf.dwGBitMask = 0x0000FF00;
                    ddsHeader.ddspf.dwBBitMask = 0x00FF0000;
                    ddsHeader.ddspf.dwABitMask = 0xFF000000;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height * 4); // 32bpp
                    break;
                case DXGI_FORMAT.B8G8R8A8_UNORM:
                case DXGI_FORMAT.B8G8R8X8_UNORM:
                    ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                    ddsHeader.ddspf.dwRGBBitCount = 32;
                    ddsHeader.ddspf.dwRBitMask = 0x00FF0000;
                    ddsHeader.ddspf.dwGBitMask = 0x0000FF00;
                    ddsHeader.ddspf.dwBBitMask = 0x000000FF;
                    ddsHeader.ddspf.dwABitMask = 0xFF000000;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height * 4); // 32bpp
                    break;
                case DXGI_FORMAT.R8_UNORM:
                    ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                    ddsHeader.ddspf.dwRGBBitCount = 8;
                    ddsHeader.ddspf.dwRBitMask = 0xFF;
                    ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(tex.Format), $"Unsupported DDS r format. File: {file.Path}");
            }
            w.Write(DDS_HEADER.MAGIC);
            w.WriteS(ddsHeader);
            switch ((DXGI_FORMAT)tex.Format) {
                case DXGI_FORMAT.BC1_UNORM_SRGB:
                case DXGI_FORMAT.BC3_UNORM_SRGB:
                case DXGI_FORMAT.BC4_UNORM:
                case DXGI_FORMAT.BC5_SNORM:
                case DXGI_FORMAT.BC7_UNORM:
                case DXGI_FORMAT.BC7_UNORM_SRGB:
                    w.WriteS(new DDS_HEADER_DXT10 {
                        dxgiFormat = (DXGI_FORMAT)tex.Format,
                        resourceDimension = D3D10_RESOURCE_DIMENSION.TEXTURE2D,
                        miscFlag = 0,
                        arraySize = 1,
                        miscFlags2 = (uint)DDS_ALPHA_MODE.ALPHA_MODE_UNKNOWN,
                    });
                    break;
            }

            // write chunks
            var chunks = tag.chunks;
            for (var i = 0; i < tex.NumChunks; i++) {
                var chunk = chunks[i];
                r.Seek((long)chunk.Offset);
                if (chunk.PackedSize != 0) s.WriteBytes(r.DecompressZlib((int)file.PackedSize, (int)file.FileSize));
                else s.WriteBytes(r, (int)file.FileSize);
            }
            s.Position = 0;
            return Task.FromResult<Stream>(s);
        }
        // GNMF BA2 Format
        else if (tag.tex is GNMF5 gnmf) {
            var s = new MemoryStream();
            // write header
            var w = new BinaryWriter(s);
            w.Write(GNF_MAGIC); // 'GNF ' magic
            w.Write(GNF_CONTENTSIZE); // Content-size. Seems to be either 4 or 8 bytes
            w.Write((byte)0x2); // Version
            w.Write((byte)0x1); // Texture Count
            w.Write((byte)0x8); // Alignment
            w.Write((byte)0x0); // Unused
            var z = BitConverter.GetBytes(gnmf.FileSize + 256); z.Reverse();
            w.Write(z); // File size + header size
            w.Write(UnsafeX.FixedTArray(gnmf.Header, 32));
            for (var i = 0; i < 208; i++) w.Write((byte)0x0); // Padding

            // write chunks
            var chunks = tag.chunks;
            for (var i = 0; i < gnmf.NumChunks; i++) {
                var chunk = chunks[i];
                r.Seek((long)chunk.Offset);
                if (chunk.PackedSize != 0) s.WriteBytes(r.DecompressZlib2((int)file.PackedSize, (int)file.FileSize));
                else s.WriteBytes(r, (int)file.FileSize);
            }
            s.Position = 0;
            return Task.FromResult<Stream>(s);
        }
        else throw new ArgumentOutOfRangeException(nameof(tag.tex), tag.tex.ToString());
    }
}

#endregion

#region Binary_Bsa
// https://en.uesp.net/wiki/Bethesda4Mod:BSA_File_Format
// https://en.uesp.net/wiki/Bethesda3Mod:BSA_File_Format

public unsafe class Binary_Bsa : ArcBinary<Binary_Bsa> {
    #region Headers : TES4

    // Default header data
    const uint OB_MAGIC = 0x00415342;    // Magic for Oblivion BSA, the literal string "BSA\0".
    const uint OB_VERSION = 0x67;         // Version number of an Oblivion BSA
    const uint F3_VERSION = 0x68;         // Version number of a Fallout 3 BSA
    const uint SE_VERSION = 0x69;        // Version number of a Skyrim SE BSA

    // Archive flags
    const ushort FLAG4_PATHNAMES = 0x0001;  // Whether the BSA has names for paths
    const ushort FLAG4_FILENAMES = 0x0002;  // Whether the BSA has names for files
    const ushort FLAG4_COMPRESSFILES = 0x0004; // Whether the files are compressed
    const ushort FLAG4_PREFIXS = 0x0100; // Whether the name is prefixed to the data?

    // Bitmasks for the size field in the header
    const uint FILE4_SIZEMASK = 0x3fffffff; // Bit mask with OB_HeaderFile:SizeFlags to get the compression status
    const uint FILE4_SIZECOMPRESS = 0xC0000000; // Bit mask with OB_HeaderFile:SizeFlags to get the compression status

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HDR4 {
        public static (string, int) Struct = ("<8I", sizeof(HDR4));
        public uint Version;            // 04
        public uint FolderRecordOffset; // Offset of beginning of folder records
        public uint ArchiveFlags;       // Archive flags
        public uint FolderCount;        // Total number of folder records (OBBSAFolderInfo)
        public uint FileCount;          // Total number of file records (OBBSAFileInfo)
        public uint FolderNameLength;   // Total length of folder names
        public uint FileNameLength;     // Total length of file names
        public uint FileFlags;          // File flags
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DIR4 {
        public static (string, int) Struct = ("<Q2I", sizeof(DIR4));
        public ulong Hash;              // Hash of the folder name
        public uint FileCount;          // Number of files in folder
        public uint Offset;             // The offset
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DIR4SE {
        public static (string, int) Struct = ("<Q2IQ", sizeof(DIR4SE));
        public ulong Hash;              // Hash of the folder name
        public uint FileCount;          // Number of files in folder
        public uint Unk;                // Unknown
        public ulong Offset;            // The offset
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FILE4 {
        public static (string, int) Struct = ("<Q2I", sizeof(FILE4));
        public ulong Hash;              // Hash of the filename
        public uint Size;               // Size of the data, possibly with OB_BSAFILE_SIZECOMPRESS set
        public uint Offset;             // Offset to raw file data
    }

    #endregion

    #region Headers : TES3

    // Default header data
    const uint MW_MAGIC = 0x00000100; // Magic for Morrowind BSA

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HDR3 {
        public static (string, int) Struct = ("<2I", sizeof(HDR3));
        public uint HashOffset;         // Offset of hash table minus header size (12)
        public uint FileCount;          // Number of files in the archive
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FILE3 {
        public static (string, int) Struct = ("<2I", sizeof(FILE3));
        public uint FileSize;           // File size
        public uint FileOffset;         // File offset relative to data position
        public readonly uint Size => FileSize > 0 ? FileSize & 0x3FFFFFFF : 0; // The size of the file inside the BSA
    }

    #endregion

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        FileSource[] files;
        var magic = source.Magic = r.ReadUInt32();

        // Oblivion - Skyrim
        if (magic == OB_MAGIC) {
            var header = r.ReadS<HDR4>();
            if (header.Version != OB_VERSION && header.Version != F3_VERSION && header.Version != SE_VERSION) throw new FormatException("BAD MAGIC");
            if ((header.ArchiveFlags & FLAG4_PATHNAMES) == 0 || (header.ArchiveFlags & FLAG4_FILENAMES) == 0) throw new FormatException("HEADER FLAGS");
            source.Version = header.Version;

            // calculate some useful values
            var compressedToggle = (header.ArchiveFlags & FLAG4_COMPRESSFILES) > 0;
            if (header.Version == F3_VERSION || header.Version == SE_VERSION)
                source.Tag = (header.ArchiveFlags & FLAG4_PREFIXS) > 0;

            // read-all folders
            var foldersFiles = header.Version == SE_VERSION
                ? r.ReadSArray<DIR4SE>((int)header.FolderCount).Select(x => x.FileCount).ToArray()
                : r.ReadSArray<DIR4>((int)header.FolderCount).Select(x => x.FileCount).ToArray();

            // read-all folder files
            var j = 0U;
            source.Files = files = new FileSource[header.FileCount];
            for (var i = 0; i < header.FolderCount; i++) {
                var folderName = r.ReadFAString(r.ReadByte() - 1).Replace('\\', '/');
                r.Skip(1);
                var headerFiles = r.ReadSArray<FILE4>((int)foldersFiles[i]);
                foreach (var headerFile in headerFiles) {
                    var compressed = (headerFile.Size & FILE4_SIZECOMPRESS) != 0;
                    var packedSize = compressed ? headerFile.Size ^ FILE4_SIZECOMPRESS : headerFile.Size;
                    files[j++] = new FileSource {
                        Path = folderName,
                        Offset = headerFile.Offset,
                        Compressed = compressed ^ compressedToggle ? 1 : 0,
                        PackedSize = packedSize,
                        FileSize = source.Version == SE_VERSION ? packedSize & FILE4_SIZEMASK : packedSize,
                    };
                }
            }

            // read-all names
            foreach (var file in files) file.Path = $"{file.Path}/{r.ReadVAString()}";
        }
        // Morrowind
        else if (magic == MW_MAGIC) {
            var header = r.ReadS<HDR3>();
            var dataOffset = 12 + header.HashOffset + (header.FileCount << 3);

            // create filesources
            source.Files = files = new FileSource[header.FileCount];
            var headerFiles = r.ReadSArray<FILE3>((int)header.FileCount);
            for (var i = 0; i < headerFiles.Length; i++) {
                ref FILE3 headerFile = ref headerFiles[i];
                var size = headerFile.Size;
                files[i] = new FileSource {
                    Offset = dataOffset + headerFile.FileOffset,
                    FileSize = size,
                    PackedSize = size,
                };
            }

            // read filename offsets
            var filenameOffsets = r.ReadPArray<uint>("I", (int)header.FileCount); // relative offset in filenames section

            // read filenames
            var filenamesPosition = r.Tell();
            for (var i = 0; i < files.Length; i++) {
                r.Seek(filenamesPosition + filenameOffsets[i]);
                files[i].Path = r.ReadVAString(1000).Replace('\\', '/');
            }
        }
        else throw new InvalidOperationException("BAD MAGIC");
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        // position
        var fileSize = (int)file.FileSize;
        r.Seek(file.Offset);
        if (source.Tag is bool z && z) {
            var prefixLength = r.ReadByte() + 1;
            if (source.Version == SE_VERSION) fileSize -= prefixLength;
            r.Seek(file.Offset + prefixLength);
        }

        // not compressed
        if (fileSize <= 0 || file.Compressed == 0)
            return Task.FromResult<Stream>(new MemoryStream(r.ReadBytes(fileSize)));

        // compressed
        var newFileSize = (int)r.ReadUInt32(); fileSize -= 4;
        return Task.FromResult<Stream>(source.Version == SE_VERSION
            ? new MemoryStream(r.DecompressLz4(fileSize, newFileSize))
            : new MemoryStream(r.DecompressZlib(fileSize, newFileSize))); //was:Zlib2
    }
}

#endregion

#region Binary_Esm

/// <summary>
/// Binary_Esm
/// </summary>
/// <seealso cref="GameX.Formats._Packages.PakBinaryBethesdaEsm" />
public unsafe class Binary_Esm : ArcBinary<Binary_Esm>, IDatabase {
    public FormType Format;
    public Record Record;
    public Dictionary<FormType, RecordGroup> Groups;

    static FormType GetFormat(string game)
        => game switch {
            // tes
            "Morrowind" => FormType.TES3,
            "Oblivion" or "Oblivion:R" => FormType.TES4,
            "Skyrim" or "SkyrimSE" or "SkyrimVR" => FormType.TES5,
            // fallout
            "Fallout3" or "FalloutNV" => FormType.TES4,
            "Fallout4" or "Fallout4VR" => FormType.TES5,
            "Fallout76" => FormType.TES5,
            // starfield
            "Starfield" => FormType.TES6,
            _ => throw new ArgumentOutOfRangeException(nameof(game), game),
        };

    class SubEsm : BinaryArchive {
        Binary_Esm Arc;

        public SubEsm(BinaryArchive source, Binary_Esm arc, string path, object tag) : base(new BinaryState(source.Vfx, source.Game, source.Edition, path, tag), Current) {
            Arc = arc;
            //AssetFactoryFunc = source.AssetFactoryFunc;
            //UseReader = false;
            //Open();
        }

        public async override Task Read(object tag) {
            //var entry = (P4kEntry)Tag;
            //var stream = Arc.GetInputStream(entry.ZipFileIndex);
            //using var r2 = new BinaryReader(stream);
            //await ArcBinary.Read(this, r2, tag);
        }
    }

    /// <summary>
    /// Reads the asynchronous.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="b">The r.</param>
    /// <param name="stage">The stage.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">stage</exception>
    public override Task Read(BinaryArchive source, BinaryReader b, object tag) {
        Format = GetFormat(source.Game.Id);
        var r = new Reader(b, source.BinPath, Format, new[] { "Fallout3", "FalloutNV" }.Contains(source.Game.Id));
        var record = Record = Record.Factory(r, (FormType)r.ReadUInt32());
        record.ReadFields(r);
        var files = (List<FileSource>)(source.Files = [new FileSource { Path = $"{record.Type}", Tag = record }]);
        foreach (var s in RecordGroup.ReadAll(r))
            if (s.Preload) s.Read(r, files);
            else r.Seek(r.Tell() + s.DataSize);
        //if (r.Format == FormType.TES3) Groups = [.. Records.GroupBy(s => s.Type).Select(s => new RecordGroup(null) { Records = [.. s], Label = s.Key })];
        return Task.CompletedTask;
    }
    //var poolAction = (GenericPoolAction<BinaryReader>)source.GetReader().Action; //: Leak

    // TES3
    Dictionary<string, Record> MANYsById;
    Dictionary<long, LTEXRecord> LTEXsById;
    Dictionary<Int3, LANDRecord> LANDsById;
    Dictionary<Int3, CELLRecord> CELLsById;
    Dictionary<string, CELLRecord> CELLsByName;

    // TES4
    Dictionary<uint, Tuple<WRLDRecord, RecordGroup[]>> WRLDsById;
    Dictionary<string, LTEXRecord> LTEXsByEid;

    public override Task Process(BinaryArchive source) {
        if (Format == FormType.TES3) {
            //var statGroups = new List<Record>[] { Groups.ContainsKey(FormType.STAT) ? Groups[FormType.STAT].Load() : null };
            //MANYsById = statGroups.SelectMany(s => s).Where(s => s != null).ToDictionary(s => s.EDID.Value, s => (Record)s);
            //LTEXsById = Groups[FormType.LTEX].Load().Cast<LTEXRecord>().ToDictionary(s => s.INTV.Value);
            //var lands = Groups[FormType.LAND].Load().Cast<LANDRecord>().ToList();
            //foreach (var land in lands) land.GridId = new Int3(land.INTV.CellX, land.INTV.CellY, 0);
            //LANDsById = lands.ToDictionary(s => s.GridId);
            //var cells = Groups[FormType.CELL].Load().Cast<CELLRecord>().ToList();
            //foreach (var cell in cells) cell.GridId = new Int3(cell.XCLC.Value.GridX, cell.XCLC.Value.GridY, !cell.IsInterior ? 0 : -1);
            //CELLsById = cells.Where(x => !x.IsInterior).ToDictionary(s => s.GridId);
            //CELLsByName = cells.Where(x => x.IsInterior).ToDictionary(s => s.EDID.Value);
            return Task.CompletedTask;
        }
        //var wrldsByLabel = Groups[FormType.WRLD].GroupsByLabel;
        //WRLDsById = Groups[FormType.WRLD].Load().Cast<WRLDRecord>().ToDictionary(s => s.Id, s => { wrldsByLabel.TryGetValue(s.Id, out var wrlds); return new Tuple<WRLDRecord, RecordGroup[]>(s, wrlds); });
        //LTEXsByEid = Groups[FormType.LTEX].Load().Cast<LTEXRecord>().ToDictionary(s => s.EDID.Value);
        return Task.CompletedTask;
    }

    #region Query

    public static object FindTAGFactory(FormType type, RecordGroup group) => Activator.CreateInstance(typeof(FindTAG<>).MakeGenericType(Record.Factory(null, type).GetType()), group.Records);
    public class FindTAG<T>(List<Record> s) : List<T>(s.Cast<T>()), IHaveMetaInfo, IWriteToStream {
        public void WriteToStream(Stream stream) => this.Serialize(stream);
        public override string ToString() => this.Serialize();

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new (null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new ("NAME", items: [
            ])
        ];
    }
    public class FindLTEX(int index) {
        public object Tes3(Binary_Esm _) => _.LTEXsById.TryGetValue(index, out var z) ? z : default;
    }
    public class FindLAND(Int3 cell) {
        public object Tes3(Binary_Esm _) => _.LANDsById.TryGetValue(cell, out var z) ? z : default;
        public object Else(Binary_Esm _) {
            var world = _.WRLDsById[(uint)cell.Z];
            foreach (var wrld in world.Item2)
                foreach (var cellBlock in wrld.EnsureWrldAndCell(cell))
                    if (cellBlock.LANDsById.TryGetValue(cell, out var z)) return z;
            return null;
        }
    }
    public class FindCELL(Int3 cell) {
        public object Tes3(Binary_Esm _) => _.CELLsById.TryGetValue(cell, out var z) ? z : default;
        public object Else(Binary_Esm _) {
            var world = _.WRLDsById[(uint)cell.Z];
            foreach (var wrld in world.Item2)
                foreach (var cellBlock in wrld.EnsureWrldAndCell(cell))
                    if (cellBlock.CELLsById.TryGetValue(cell, out var z)) return z;
            return null;
        }
    }
    public class FindCELLByName(string name) {
        public object Tes3(Binary_Esm _) => _.CELLsByName.TryGetValue(name, out var z) ? z : default;
    }
    public object Query(object source) => source switch {
        FileSource s => s.Arc == null ? FindTAGFactory((FormType)s.Flags, (RecordGroup)s.Tag) : null,
        FindLTEX s => Format == FormType.TES3 ? s.Tes3(this) : throw new NotImplementedException(),
        FindLAND s => Format == FormType.TES3 ? s.Tes3(this) : s.Else(this),
        FindCELL s => Format == FormType.TES3 ? s.Tes3(this) : s.Else(this),
        FindCELLByName s => Format == FormType.TES3 ? s.Tes3(this) : throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException(),
    };

    #endregion
}

#endregion
