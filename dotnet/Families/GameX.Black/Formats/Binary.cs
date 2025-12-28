using GameX.Uncore.Formats;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Black.Formats;

#region Binary_Dat

// Fallout 2
public unsafe class Binary_Dat : ArcBinary<Binary_Dat> {
    // Header : F1
    #region Headers : F1
    // https://falloutmods.fandom.com/wiki/DAT_file_format

    const uint F1_HEADER_FILEID = 0x000000001;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct F1_Header {
        //public static string Map = "B4B4B4B4";
        public static (string, int) Struct = (">4I", sizeof(F1_Header));
        public uint DirectoryCount; // DirectoryCount
        public uint Unknown1; // Usually 0x0A (0x5E for master.dat). Must not be less than 1 or Fallout will crash instantly with a memory read error. Possibly some kind of memory buffer size.
        public uint Unknown2; // Always 0.
        public uint Unknown3; // Could be some kind of checksum, but Fallout seems to work fine with any value.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct F1_Directory {
        //public static string Map = "B4B4B4B4";
        public static (string, int) Struct = (">4I", sizeof(F1_Directory));
        public uint FileCount; // Number of files in the directory.
        public uint Unknown1; // Similar to (Unknown1), the default value seems to be 0x0A and Fallout works with most positive non-zero values.
        public uint Unknown2; // Seems to always be 0x10.
        public uint Unknown3; // See (Unknown3).
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct F1_File {
        //public static string Map = "B4B4B4B4";
        public static (string, int) Struct = (">4I", sizeof(F1_File));
        public uint Attributes; // 0x20 means plain-text, 0x40 - compressed with LZSS.
        public uint Offset; // Position in the file (from the beginning of the DAT file), where the file contets start.
        public uint Size; // Original (uncompressed) file size.
        public uint PackedSize; // Size of the compressed file in dat. If file is not compressed, PackedSize is 0.
    }

    #endregion

    // Header : F2
    #region Headers : F2
    // https://falloutmods.fandom.com/wiki/DAT_file_format

    const uint F2_HEADER_FILEID = 0x000000011;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct F2_Header {
        public static (string, int) Struct = ("<2I", sizeof(F2_Header));
        public uint TreeSize;               // Size of DirTree in bytes
        public uint DataSize;               // Full size of the archive in bytes
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct F2_File {
        public static (string, int) Struct = ("<B3I", sizeof(F2_File));
        public byte Type;               // 1 = Compressed 0 = Decompressed
        public uint RealSize;           // Size of the file without compression.
        public uint PackedSize;         // Size of the compressed file.
        public uint Offset;             // Address/Location of the file.
    }

    #endregion

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        var gameId = source.Game.Id;

        // Fallout
        if (gameId == "Fallout") {
            source.Magic = F1_HEADER_FILEID;
            var header = r.ReadS<F1_Header>();
            var directoryPaths = new string[header.DirectoryCount];
            for (var i = 0; i < header.DirectoryCount; i++)
                directoryPaths[i] = r.ReadL8UString().Replace('\\', '/');
            // Create file metadatas
            var files = new List<FileSource>(); source.Files = files;
            for (var i = 0; i < header.DirectoryCount; i++) {
                var directory = r.ReadS<F1_Directory>();
                var directoryPath = directoryPaths[i] != "." ? directoryPaths[i] + "/" : string.Empty;
                for (var j = 0; j < directory.FileCount; j++) {
                    var path = directoryPath + r.ReadL8UString().Replace('\\', '/');
                    var file = r.ReadS<F1_File>();
                    files.Add(new FileSource {
                        Path = path,
                        Compressed = (int)file.Attributes & 0x40,
                        Offset = file.Offset,
                        FileSize = file.Size,
                        PackedSize = file.PackedSize,
                    });
                }
            }
        }

        // Fallout2
        else if (gameId == "Fallout2") {
            source.Magic = F2_HEADER_FILEID;
            r.Seek(r.BaseStream.Length - sizeof(F2_Header));
            var header = r.ReadS<F2_Header>();
            if (header.DataSize != r.BaseStream.Length) throw new InvalidOperationException("File is not a valid bsa archive.");
            r.Seek(header.DataSize - header.TreeSize - sizeof(F2_Header));

            // Create file metadatas
            var files = new FileSource[r.ReadInt32()]; source.Files = files;
            for (var i = 0; i < files.Length; i++) {
                var path = r.ReadL32UString().Replace('\\', '/');
                var file = r.ReadS<F2_File>();
                files[i] = new FileSource {
                    Path = path,
                    Compressed = file.Type,
                    FileSize = file.RealSize,
                    PackedSize = file.PackedSize,
                    Offset = file.Offset,
                };
            }
        }
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        var magic = source.Magic;
        // F1
        if (magic == F1_HEADER_FILEID) {
            r.Seek(file.Offset);
            Stream fileData = new MemoryStream(file.Compressed == 0
                ? r.ReadBytes((int)file.PackedSize)
                : r.DecompressLzss((int)file.PackedSize, (int)file.FileSize));
            return Task.FromResult(fileData);
        }
        // F2
        else if (magic == F2_HEADER_FILEID) {
            r.Seek(file.Offset);
            Stream fileData = new MemoryStream(r.Peek(z => z.ReadUInt16()) == 0xda78
                ? r.DecompressZlib((int)file.PackedSize, -1)
                : r.ReadBytes((int)file.PackedSize));
            return Task.FromResult(fileData);
        }
        else throw new InvalidOperationException("BAD MAGIC");
    }
}

#endregion

#region Binary_Frm

public class Binary_Frm : IHaveMetaInfo, ITextureFramesSelect {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Frm(r, f, s));

    #region Headers
    // https://falloutmods.fandom.com/wiki/FRM_File_Format

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FrmHeader {
        //internal static string Endian = "B4B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B4B4B4B4B4B4B4";
        public static (string, int) Struct = (">I3H6h6h6II", sizeof(FrmHeader));
        public uint Version;                            // Version
        public ushort Fps;                              // FPS
        public ushort ActionFrame;                      // Action frame
        public ushort FramesPerDirection;               // Number of frames per direction
        public fixed short PixelShiftX[6];              // Pixel shift in the x direction, of frames with orientation N
        public fixed short PixelShiftY[6];              // Pixel shift in the y direction, of frames with orientation N
        public fixed uint FrameOffset[6];               // Offset of first frame in orientation N from beginning of frame area
        public uint SizeOfFrame;                        // Size of frame data
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FrmFrame {
        //internal static string Endian = "B2B2B4B2B2";
        public static (string, int) Struct = (">2HI2h", sizeof(FrmFrame));
        public ushort Width;                            // FRAME-0-WIDTH: Width of frame 0 
        public ushort Height;                           // FRAME-0-HEIGHT: Height of frame 0
        public uint Size;                               // FRAME-0-SIZE: Number of pixels for frame 0
        public short PixelShiftX;                       // Pixel shift in x direction of frame 0
        public short PixelShiftY;                       // Pixel shift in y direction of frame 0
    }

    #endregion

    static Binary_Pal2 DefaultPallet;

    public FrmHeader Header;
    public (FrmFrame f, byte[] b)[] Frames;
    byte[] Bytes;

    public unsafe Binary_Frm(BinaryReader r, FileSource f, Archive s) {
        var pallet = GetPalletObjAsync(f.Path, (BinaryArchive)s).Result ?? throw new Exception("No pallet found");
        var rgba32 = pallet.Rgba32;

        // parse header
        var header = r.ReadS<FrmHeader>();
        var frames = new List<(FrmFrame f, byte[] b)>();
        var stream = r.BaseStream;
        for (var i = 0; i < 6 * header.FramesPerDirection && stream.Position < stream.Length; i++) {
            var frameOffset = Header.FrameOffset[i];
            var frame = r.ReadS<FrmFrame>();
            var data = r.ReadBytes((int)frame.Size);
            var image = new byte[frame.Width * frame.Height * 4];
            fixed (byte* image_ = image) {
                var _ = image_;
                for (var j = 0; j < data.Length; j++, _ += 4) *(uint*)_ = rgba32[data[j]];
            }
            frames.Add((f: frame, b: image));
        }
        Header = header;
        Frames = [.. frames];

        // select a frame
        FrameSelect(0);
    }

    async Task<Binary_Pal2> GetPalletObjAsync(string path, BinaryArchive s) {
        var palletPath = $"{path[..^4]}.PAL";
        if (s.Contains(palletPath))
            return await s.GetAsset<Binary_Pal2>(palletPath);
        if (DefaultPallet == null && s.Contains("COLOR.PAL")) {
            DefaultPallet ??= await s.GetAsset<Binary_Pal2>("COLOR.PAL");
            DefaultPallet.SetColors();
        }
        return DefaultPallet;
    }

    // ITexture
    static readonly object Format = (TextureFormat.RGBA32, TexturePixel.Unknown);
    //((TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGBA32, TextureUnrealFormat.R8G8B8A8);
    public int Width { get; internal set; }
    public int Height { get; internal set; }
    public int Depth => 0;
    public int MipMaps => 1;
    public TextureFlags TexFlags => 0;
    public T Create<T>(string platform, Func<object, T> func) => func(new Texture_Bytes(Bytes, Format, null));

    // ITextureFrames
    public int Fps => Header.Fps;
    public int FrameMax => Frames.Length == 1 ? 1 : Header.FramesPerDirection;
    public void FrameSelect(int id) {
        Bytes = Frames[id].b;
        Width = Frames[id].f.Width;
        Height = Frames[id].f.Height;
    }
    public bool HasFrames => false;
    public bool DecodeFrame() => false;

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new($"{nameof(Binary_Frm)}", items: [
                new($"Frames: {Frames.Length}"),
                new($"Width: {Width}"),
                new($"Height: {Height}"),
            ])
    ];
}

#endregion

#region Binary_Pal2

public unsafe class Binary_Pal2 : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Pal2(r, f));

    public uint[] Rgba32 = new uint[256];

    public Binary_Pal2(BinaryReader r, FileSource f) {
        var rgb = r.ReadBytes(256 * 3);
        fixed (byte* s = rgb)
        fixed (uint* d = Rgba32) {
            var _ = s;
            for (var i = 0; i < 256; i++, _ += 3)
                d[i] = (uint)(0x00 << 24 | _[2] << (16 + 2) | _[1] << (8 + 2) | _[0]);
            //d[0] = uint.MaxValue;
        }
    }

    public void SetColors() {
        for (var i = 229; i <= 232; i++) Rgba32[i] = 0x00ff0000; // animated green (for radioactive waste)
        for (var i = 233; i <= 237; i++) Rgba32[i] = 0x0000ff00; // bright blue (computer screens)
        for (var i = 238; i <= 247; i++) Rgba32[i] = 0xff000000; // orange, red and yellow (for fires)
        for (var i = 248; i <= 254; i++) Rgba32[i] = 0x0000ff00; // bright blue (computer screens)
    }

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Null", Name = Path.GetFileName(file.Path), Value = this }),
        new($"{nameof(Binary_Pal2)}", items: [])
    ];
}

#endregion

#region Binary_Rix

public unsafe class Binary_Rix : IHaveMetaInfo, ITexture {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Rix(r, f));

    #region Headers
    // https://falloutmods.fandom.com/wiki/RIX_File_Format

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header {
        public uint Magic;                      // RIX3 - the file signature 
        public ushort Width;                    // 640 - width of the image
        public ushort Height;                   // 480 - height of the image
        public byte PaletteType;                // VGA - Palette type
        public byte StorageType;                // linear - Storage type
    }

    #endregion

    byte[] Bytes;

    public Binary_Rix(BinaryReader r, FileSource f) {
        var header = r.ReadS<Header>();
        var rgb = r.ReadBytes(256 * 3);
        var rgba32 = stackalloc uint[256];
        fixed (byte* s = rgb) {
            var d = rgba32;
            var _ = s;
            for (var i = 0; i < 256; i++, _ += 3)
                d[i] = (uint)(0x00 << 24 | _[2] << (16 + 2) | _[1] << (8 + 2) | _[0]);
        }
        var data = r.ReadBytes(header.Width * header.Height);
        var image = new byte[header.Width * header.Height * 4];
        fixed (byte* image_ = image) {
            var _ = image_;
            for (var j = 0; j < data.Length; j++, _ += 4) *(uint*)_ = rgba32[data[j]];
        }
        Bytes = image;
        Width = header.Width;
        Height = header.Height;
    }

    // ITexture
    static readonly object Format = (TextureFormat.RGBA32, TexturePixel.Unknown);
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //TextureUnityFormat.RGBA32,
    //TextureUnrealFormat.R8G8B8A8);
    public int Width { get; }
    public int Height { get; }
    public int Depth => 0;
    public int MipMaps => 1;
    public TextureFlags TexFlags => 0;
    public T Create<T>(string platform, Func<object, T> func) => func(new Texture_Bytes(Bytes, Format, null));

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new($"{nameof(Binary_Rix)}", items: [
                new($"Width: {Width}"),
                new($"Height: {Height}"),
            ])
    ];
}

#endregion