﻿using GameX.Formats;
using OpenStack.Gfx.Render;
using OpenStack.Gfx.Texture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Bullfrog.Formats;

#region Binary_Bullfrog

public class Binary_Bullfrog : PakBinary<Binary_Bullfrog>
{
    #region Factories

    public static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
     => game.Id switch
     {
         _ => default
     };
    //=> source.Path.ToLowerInvariant() switch
    //{
    //    _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
    //    {
    //        ".pal" => (0, Binary_Pal.Factory_3),
    //        ".wav" => (0, Binary_Snd.Factory),
    //        ".tex" or ".raw" => (0, Binary_Raw.FactoryMethod(Binary_RawFunc, (id, value) => id switch
    //        {
    //            "DK" => Games.DK.Database.GetPalette(value, "DATA/MAIN"),
    //            "DK2" => Games.DK2.Database.GetPalette(value, "DATA/MAIN"),
    //            _ => throw new ArgumentOutOfRangeException(nameof(game.Id), game.Id),
    //        })),
    //        _ => (0, null),
    //    }
    //};

    static void Binary_RawFunc(Binary_Raw s, BinaryReader r, FileSource f)
    {
        s.Body = r.Peek(x => x.ReadUInt32()) == Rnc.RNC_MAGIC ? Rnc.Read(r) : r.ReadToEnd();
        if (f.Tag == null)
        {
            s.Palette = f.Path.EndsWith(".bmp") ? "" : f.Path[..^4];
            switch (s.Body.Length)
            {
                case 1024: s.Width = 32; s.Height = 32; break;
                case 64000: s.Width = 320; s.Height = 200; break;
                case 307200: s.Width = 640; s.Height = 480; break;
                case 1228800: s.Width = 640; s.Height = 1920; break;
                default: throw new ArgumentOutOfRangeException(nameof(s.Body), $"{s.Body.Length}");
            }
            ;
        }
    }

    #endregion

    #region Headers

    const int TEXTURE_BLOCKSA = 544; // Static textures in tmapa
    const int TEXTURE_BLOCKSB = 544; // Static textures in tmapb

    // Dungeon Keeper
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct SBK_Header
    {
        public static (string, int) Struct = ("<14sI", sizeof(SBK_Header));
        public fixed byte Unknown1[14];
        public uint Unknown2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct SBK_Entry
    {
        public static (string, int) Struct = ("<4I", sizeof(SBK_Entry));
        public uint EntryOffset;
        public uint BankOffset;
        public uint Size;
        public uint Unknown;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct SBK_Sample
    {
        public static (string, int) Struct = ("<18QI2x", sizeof(SBK_Sample));
        public fixed byte Path[18];
        public ulong Offset;
        public uint DataSize;
        public byte Sfxid;
        public byte Unknown;
    }

    // Dungeon Keeper
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct CRE_Sprite
    {
        public static (string, int) Struct = ("<I8x2h", sizeof(CRE_Sprite));
        public uint DataOffset;
        public byte SWidth;
        public byte SHeight;
        public byte FrameWidth;
        public byte FrameHeight;
        public byte Rotable;
        public byte FramesCount;
        public byte FrameOffsW;
        public byte FrameOffsH;
        public short OffsetX;
        public short OffsetY;
    };

    #endregion

    byte[] Data;

    public override async Task Read(BinaryPakFile source, BinaryReader r, object tag)
    {
        //Data = Rnc.Read(r);
        List<FileSource> files;
        source.Files = files = new List<FileSource>();
        //var tabPath = $"{source.PakPath[..^4]}.TAB";
        //var fileName = Path.GetFileName(source.PakPath);
        //if (fileName.StartsWith("CREATURE")) await source.Reader(s => ParseCreature(s, files), tabPath);
        //else if (fileName.StartsWith("GUI")) ParseGui(r, files);
        //else if (fileName.StartsWith("SOUND")) ParseSound(r, files, 2);
        //else if (fileName.StartsWith("SPEECH")) ParseSound(r, files, 2);
        //else if (fileName.StartsWith("TMAPA")) ParseTexure(files, TEXTURE_BLOCKSA);
        //else if (fileName.StartsWith("TMAPB")) ParseTexure(files, TEXTURE_BLOCKSB);
        //return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default)
    {
        var bytes = Data.AsSpan((int)file.Offset, (int)file.FileSize);
        return Task.FromResult((Stream)new MemoryStream(bytes.ToArray()));
    }

    unsafe static Task ParseCreature(BinaryReader r, List<FileSource> files)
    {
        var numSprites = (int)(r.BaseStream.Length / sizeof(CRE_Sprite));
        var sprites = r.ReadSArray<CRE_Sprite>(numSprites);
        var lastI = numSprites - 1;
        for (var i = 0; i < numSprites; i++)
        {
            ref CRE_Sprite s = ref sprites[i];
            files.Add(new FileSource
            {
                Path = $"{i:000}.tex",
                Offset = s.DataOffset,
                FileSize = (i != lastI ? sprites[i + 1].DataOffset : r.BaseStream.Length) - s.DataOffset,
                Tag = new Binary_Raw.Tag { Width = s.SWidth, Height = s.SHeight },
            });
        }
        return Task.CompletedTask;
    }

    static void ParseGui(BinaryReader r, List<FileSource> files)
    {
        const int TextureBlockSize = 100 * 100;
        for (int i = 1, o = 0; i < 1; i++, o += TextureBlockSize)
            files.Add(new FileSource
            {
                Path = $"gui/{i:000}.tex",
                Offset = o,
                FileSize = TextureBlockSize,
                Tag = new Binary_Raw.Tag { Width = 32, Height = 32 },
            });
    }

    static void ParseTexure(List<FileSource> files, int count)
    {
        const int TextureBlockSize = 32 * 32;
        for (int i = 1, o = 0; i < count; i++, o += TextureBlockSize)
            files.Add(new FileSource
            {
                Path = $"texs/{i:000}.tex",
                Offset = o,
                FileSize = TextureBlockSize,
                Tag = new Binary_Raw.Tag { Width = 32, Height = 32 },
            });
    }

    unsafe static void ParseSound(BinaryReader r, List<FileSource> files, int bankId)
    {
        r.BaseStream.Seek(-4, SeekOrigin.End);
        var offset = r.ReadUInt32();
        r.Seek(offset);
        var header = r.ReadS<SBK_Header>();
        var banks = r.ReadSArray<SBK_Entry>(9);
        // read bank
        var bank = banks[bankId];
        if (bank.EntryOffset == 0 || bank.Size == 0) throw new FormatException("BAD BANK");
        var bankOffset = bank.BankOffset;
        r.Seek(bank.EntryOffset);
        var numSamples = (int)bank.Size / sizeof(SBK_Sample);
        files.AddRange(r.ReadSArray<SBK_Sample>(numSamples).Select(s =>
            new FileSource
            {
                Path = UnsafeX.FixedAString(s.Path, 18) ?? "DEFAULT.WAV",
                Offset = (long)(bankOffset + s.Offset),
                FileSize = s.DataSize,
            }
        ));
    }
}

#endregion

#region Binary_Fli

public unsafe class Binary_Fli : IDisposable, ITextureFrames, IHaveMetaInfo
{
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Fli(r, f));

    // logging
    //static StreamWriter F;
    //static void FO(string x) { F = File.CreateText("C:\\T_\\FROG\\Fli2.txt"); }
    //static void FW(string x) { F.Write(x); F.Flush(); }
    //FO("C:\\T_\\FROG\\Fli2.txt");

    #region Headers

    public const int MAGIC = 0xAF12;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_Header
    {
        public static (string, int) Struct = ("<I4H", sizeof(X_Header));
        public uint Size;
        public ushort Type;
        public ushort Frames;
        public ushort Width;
        public ushort Height;
    }

    public enum ChunkType : ushort
    {
        COLOR_256 = 0x4,    // COLOR_256
        DELTA_FLC = 0x7,    // DELTA_FLC (FLI_SS2)
        BYTE_RUN = 0xF,     // BYTE_RUN
        FRAME = 0xF1FA,     // FRAME_TYPE
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_ChunkHeader
    {
        public static (string, int) Struct = ("<IH", sizeof(X_ChunkHeader));
        public uint Size;
        public ChunkType Type;
        public bool IsValid => Type == ChunkType.COLOR_256 || Type == ChunkType.DELTA_FLC || Type == ChunkType.BYTE_RUN || Type == ChunkType.FRAME;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_FrameHeader
    {
        public static (string, int) Struct = ("<5H", sizeof(X_FrameHeader));
        public ushort NumChunks;
        public ushort Delay;
        public ushort Reserved;
        public ushort WidthOverride;
        public ushort HeightOverride;
    }

    public enum OpCode : ushort
    {
        PACKETCOUNT = 0,    // PACKETCOUNT
        UNDEFINED = 1,      // UNDEFINED
        LASTPIXEL = 2,      // LASTPIXEL
        LINESKIPCOUNT = 3,  // LINESKIPCOUNT
    }

    #endregion

    public BinaryReader R;
    public byte[] Pixels;
    public byte[] Palette;
    //public byte[][] Palette = new byte[256][];
    public S.Event[] Events;
    public int Frames;
    public int NumFrames;
    public byte[] Bytes;

    public Binary_Fli(BinaryReader r, FileSource f)
    {
        // read events
        Events = S.GetEvents($"{Path.GetFileNameWithoutExtension(f.Path).ToLowerInvariant()}.evt");

        // read header
        var header = r.ReadS<X_Header>();
        if (header.Type != MAGIC) throw new FormatException("BAD MAGIC");
        Width = header.Width;
        Height = header.Height;
        Frames = NumFrames = header.Frames;

        // set values
        R = r;
        Fps = Path.GetFileName(f.Path).StartsWith("mscren", StringComparison.OrdinalIgnoreCase) ? 20 : 15;
        Pixels = new byte[Width * Height];
        Palette = new byte[256 * 3];
        Bytes = new byte[Width * Height * 3];
    }

    public void Dispose() => R.Close();

    #region ITexture
    static readonly object Format = (TextureFormat.RGB24, TexturePixel.Unknown);
    //(TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte),
    //(TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte),
    //TextureUnityFormat.RGB24,
    //TextureUnrealFormat.Unknown);
    public int Width { get; }
    public int Height { get; }
    public int Depth => 0;
    public int MipMaps => 0;
    public TextureFlags TexFlags => 0;
    public int Fps { get; }
    public (byte[] bytes, object format, Range[] spans) Begin(string platform) => (Bytes, Format, null);
    public void End() { }
    #endregion

    public bool HasFrames => NumFrames > 0;

    public bool DecodeFrame()
    {
        var r = R;
        X_FrameHeader frameHeader;
        var header = r.ReadS<X_ChunkHeader>();
        do
        {
            var nextPosition = r.BaseStream.Position + (header.Size - 6);
            switch (header.Type)
            {
                case ChunkType.COLOR_256: SetPalette(r); break;
                case ChunkType.DELTA_FLC: DecodeDeltaFLC(r); break;
                case ChunkType.BYTE_RUN: DecodeByteRun(r); break;
                case ChunkType.FRAME:
                    frameHeader = r.ReadS<X_FrameHeader>();
                    NumFrames--;
                    //Log($"Frames Remaining: {NumFrames}, Chunks: {frameHeader.NumChunks}");
                    break;
                default:
                    Log($"Unknown Type: {header.Type}");
                    r.Skip(header.Size);
                    break;
            }
            if (header.Type != ChunkType.FRAME && r.BaseStream.Position != nextPosition) r.Seek(nextPosition);
            header = r.ReadS<X_ChunkHeader>();
        }
        while (header.IsValid && header.Type != ChunkType.FRAME);
        Rasterize.CopyPixelsByPalette(Bytes, 3, Pixels, Palette, 3);
        if (header.Type == ChunkType.FRAME) r.Skip(-sizeof(X_ChunkHeader));
        return header.IsValid;
    }

    void SetPalette(BinaryReader r)
    {
        var numPackets = r.ReadUInt16();
        if (r.ReadUInt16() == 0) // special case
        {
            var data = r.ReadBytes(256 * 3);
            for (int i = 0; i < data.Length; i += 3)
            {
                Palette[i + 0] = (byte)((data[i + 0] << 2) | (data[i + 0] & 3));
                Palette[i + 1] = (byte)((data[i + 1] << 2) | (data[i + 1] & 3));
                Palette[i + 2] = (byte)((data[i + 2] << 2) | (data[i + 2] & 3));
            }
            return;
        }
        r.Skip(-2);
        var palPos = 0;
        while (numPackets-- != 0)
        {
            palPos += r.ReadByte() * 3;
            var change = r.ReadByte();
            var data = r.ReadBytes(change * 3);
            for (int i = 0; i < data.Length; i += 3)
            {
                Palette[palPos + i + 0] = (byte)((data[i + 0] << 2) | (data[i + 0] & 3));
                Palette[palPos + i + 1] = (byte)((data[i + 1] << 2) | (data[i + 1] & 3));
                Palette[palPos + i + 2] = (byte)((data[i + 2] << 2) | (data[i + 2] & 3));
            }
            palPos += change;
        }
    }

    void DecodeDeltaFLC(BinaryReader r)
    {
        var linesInChunk = r.ReadUInt16();
        int curLine = 0, numPackets = 0, value;
        while (linesInChunk-- > 0)
        {
            // first process all the opcodes.
            OpCode opcode;
            do
            {
                value = r.ReadUInt16();
                opcode = (OpCode)((value >> 14) & 3);
                switch (opcode)
                {
                    case OpCode.PACKETCOUNT: numPackets = value; break;
                    case OpCode.UNDEFINED: break;
                    case OpCode.LASTPIXEL: Pixels[(curLine * Width) + (Width - 1)] = (byte)(value & 0xFF); break;
                    case OpCode.LINESKIPCOUNT: curLine += -(short)value; break;
                }
            } while (opcode != OpCode.PACKETCOUNT);

            // now interpret the RLE data
            value = 0;
            while (numPackets-- > 0)
            {
                value += r.ReadByte();
                var pixels = Pixels.AsSpan((curLine * Width) + value);
                fixed (byte* _ = pixels)
                {
                    var count = (int)r.ReadSByte();
                    if (count > 0)
                    {
                        var size = count << 1;
                        Unsafe.CopyBlock(ref *_, ref r.ReadBytes(size)[0], (uint)size);
                        value += size;
                    }
                    else if (count < 0)
                    {
                        var __ = (ushort*)_;
                        count = -count;
                        var size = count << 1;
                        var data = r.ReadUInt16();
                        while (count-- != 0) *__++ = data;
                        value += size;
                    }
                    else return;
                }
            }
            curLine++;
        }
    }

    void DecodeByteRun(BinaryReader r)
    {
        fixed (byte* _ = Pixels)
        {
            byte* ptr = _, endPtr = _ + (Width * Height);
            while (ptr < endPtr)
            {
                var numChunks = r.ReadByte();
                while (numChunks-- != 0)
                {
                    var count = (int)r.ReadSByte();
                    if (count > 0) { Unsafe.InitBlock(ref *ptr, r.ReadByte(), (uint)count); ptr += count; }
                    else { count = -count; Unsafe.CopyBlock(ref *ptr, ref r.ReadBytes(count)[0], (uint)count); ptr += count; }
                }
            }
        }
    }

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "VideoTexture", Name = Path.GetFileName(file.Path), Value = this }),
        new("Video", items: [
            new($"Width: {Width}"),
            new($"Height: {Height}"),
            new($"Frames: {Frames}")
        ])
    ];
}

#endregion

#region Binary_Populus

public class Binary_Populus : PakBinary<Binary_Populus>
{
    #region Factories

    public static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => source.Path.ToLowerInvariant() switch
        {
            _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".pal" => (0, Binary_Pal.Factory_3),
                ".wav" => (0, Binary_Snd.Factory),
                ".raw" => (0, Binary_Raw.FactoryMethod(Binary_RawFunc, (id, value) => id switch
                {
                    "P2" => Games.P2.Database.GetPalette(value, "DATA/MQAZ"),
                    _ => throw new ArgumentOutOfRangeException(nameof(game.Id), game.Id),
                })),
                _ => (0, null),
            }
        };

    static void Binary_RawFunc(Binary_Raw s, BinaryReader r, FileSource f)
    {
        if (f.Tag == null)
        {
            s.Body = r.ReadToEnd();
            s.Palette = f.Path[..^4];
            switch (s.Body.Length)
            {
                case 64000: s.Width = 320; s.Height = 200; break;
                //case 307200: s.Width = 640; s.Height = 480; break;
                default: throw new ArgumentOutOfRangeException(nameof(s.Body), $"{s.Body.Length}");
            }
            ;
        }
    }

    #endregion

    #region Headers

    const uint MAGIC_SPR = 0x42465350;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct SPR_Record
    {
        public static (string, int) Struct = ("<2HI", sizeof(SPR_Record));
        public ushort Width;
        public ushort Height;
        public uint Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct DAT_Sprite
    {
        public static (string, int) Struct = ("<2bI", sizeof(DAT_Sprite));
        public byte Width;
        public byte Height;
        public ushort Unknown;
        public ushort Offset;
    }

    #endregion

    public override async Task Read(BinaryPakFile source, BinaryReader r, object tag)
    {
        List<FileSource> files;
        source.Files = files = [];
        var tabPath = $"{source.PakPath[..^4]}.TAB";
        var fileName = Path.GetFileName(source.PakPath);
        switch (source.Game.Id)
        {
            case "P2":
                if (fileName.StartsWith("EMSCBLK")) await source.ReaderT(s => ParseSprite(s, files), tabPath);
                else if (fileName.StartsWith("EMSFACES") || fileName.StartsWith("MEMSFACE")) await source.ReaderT(s => ParseSprite(s, files), tabPath);
                else if (fileName.StartsWith("EMSICON") || fileName.StartsWith("MEMICON")) await source.ReaderT(s => ParseSprite(s, files), tabPath);
                else if (fileName.StartsWith("EMSSPR") || fileName.StartsWith("MEMSPR")) await source.ReaderT(s => ParseSprite(s, files), tabPath);
                else if (fileName.StartsWith("HPOINTER") || fileName.StartsWith("MPOINTER")) await source.ReaderT(s => ParseSprite(s, files), tabPath);
                else if (fileName.StartsWith("MBLOCK")) await source.ReaderT(s => ParseSprite(s, files), tabPath);
                else if (fileName.StartsWith("LAND")) ParseOther(files, 10);
                return;
            case "P3":
                var magic = r.ReadUInt32();
                switch (magic)
                {
                    case MAGIC_SPR:
                        {
                            var count = r.ReadUInt32();
                            int i = 0;
                            FileSource n, l = null;
                            var lastOffset = r.Tell();
                            foreach (var s in r.ReadSArray<SPR_Record>((int)count))
                            {
                                files.Add(n = new FileSource
                                {
                                    Path = $"sprs/spr{i++}.spr",
                                    Offset = s.Offset,
                                    Tag = (s.Width, s.Height),
                                });
                                if (l != null) { l.FileSize = n.Offset - (lastOffset = l.Offset); }
                                l = n;
                            }
                            l.FileSize = r.BaseStream.Length - lastOffset;
                            return;
                        }
                    default: throw new FormatException("BAD MAGIC");
                }
            default: return;
        }
    }

    unsafe static Task ParseSprite(BinaryReader r, List<FileSource> files)
    {
        r.Skip(4);
        var numSprites = (int)(r.BaseStream.Length - 4 / sizeof(DAT_Sprite));
        var sprites = r.ReadSArray<DAT_Sprite>(numSprites);
        var lastI = numSprites - 1;
        for (var i = 0; i < numSprites; i++)
        {
            ref DAT_Sprite s = ref sprites[i];
            files.Add(new FileSource
            {
                Path = $"{i:000}.raw",
                Offset = s.Offset,
                FileSize = (i != lastI ? sprites[i + 1].Offset : r.BaseStream.Length) - s.Offset,
                Tag = new Binary_Raw.Tag { Width = s.Width, Height = s.Height },
            });
        }
        return Task.CompletedTask;
    }

    static void ParseOther(List<FileSource> files, int count)
    {
        const int TextureBlockSize = 32 * 32;
        for (int i = 1, o = 0; i < count; i++, o += TextureBlockSize)
            files.Add(new FileSource
            {
                Path = $"sprs/{i:000}.raw",
                Offset = o,
                FileSize = TextureBlockSize,
                Tag = new Binary_Raw.Tag { Width = 32, Height = 32 },
            });
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default)
    {
        r.Seek(file.Offset);
        var bytes = r.ReadBytes((int)file.FileSize);
        return Task.FromResult((Stream)new MemoryStream(bytes));
    }
}

#endregion

#region Binary_Syndicate

public class Binary_Syndicate : PakBinary<Binary_Syndicate>
{
    #region Factories

    static readonly string[] S_FLIFILES = ["INTRO.DAT", "MBRIEF.DAT", "MBRIEOUT.DAT", "MCONFOUT.DAT", "MCONFUP.DAT", "MDEBRIEF.DAT", "MDEOUT.DAT", "MENDLOSE.DAT", "MENDWIN.DAT", "MGAMEWIN.DAT", "MLOSA.DAT", "MLOSAOUT.DAT", "MLOSEGAM.DAT", "MMAP.DAT", "MMAPOUT.DAT", "MOPTION.DAT", "MOPTOUT.DAT", "MRESOUT.DAT", "MRESRCH.DAT", "MSCRENUP.DAT", "MSELECT.DAT", "MSELOUT.DAT", "MTITLE.DAT", "MMULTI.DAT", "MMULTOUT.DAT"];
    public static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
     => Path.GetFileName(source.Path).ToUpperInvariant() switch
     {
         var x when S_FLIFILES.Contains(x) => (0, Binary_Fli.Factory),
         //"MCONSCR.DAT" => (0, Binary_Raw.FactoryMethod()),
         //"MLOGOS.DAT" => (0, Binary_Raw.FactoryMethod()),
         //"MMAPBLK.DAT" => (0, Binary_Raw.FactoryMethod()),
         //"MMINLOGO.DAT" => (0, Binary_Raw.FactoryMethod()),
         "HFNT01.DAT" => (0, Binary_SyndicateX.Factory_Font),
         var x when x.StartsWith("GAME") && x.EndsWith(".DAT") => (0, Binary_SyndicateX.Factory_Game),
         "COL01.DAT" => (0, Binary_SyndicateX.Factory_MapColumn),
         var x when x.StartsWith("MAP") && x.EndsWith(".DAT") => (0, Binary_SyndicateX.Factory_MapData),
         "HBLK01.DAT" => (0, Binary_SyndicateX.Factory_MapTile),
         var x when x.StartsWith("MISS") && x.EndsWith(".DAT") => (0, Binary_SyndicateX.Factory_Mission),
         "INTRO.XMI" or "SYNGAME.XMI" => (0, Binary_Iif.Factory),
         var x when x.StartsWith("HPAL") && x.EndsWith(".DAT") || x == "HPALETTE.DAT" || x == "MSELECT.PAL" => (0, Binary_SyndicateX.Factory_Palette),
         "MLOGOS.DAT" or "MMAPBLK.DAT" or "MMINLOGO.PAL" => (0, Binary_SyndicateX.Factory_Raw),
         "HREQ.DAT" => (0, Binary_SyndicateX.Factory_Req),
         var x when x.StartsWith("ISNDS-") && x.EndsWith(".DAT") || x.StartsWith("SOUND-") && x.EndsWith(".DAT") => (0, Binary_SyndicateX.Factory_SoundData),
         var x when x.StartsWith("ISNDS-") && x.EndsWith(".TAB") || x.StartsWith("SOUND-") && x.EndsWith(".TAB") => (0, Binary_SyndicateX.Factory_SoundTab),
         "HSTA-0.ANI" => (0, Binary_SyndicateX.Factory_SpriteAnim),
         "HFRA-0.ANI" => (0, Binary_SyndicateX.Factory_SpriteFrame),
         "HELE-0.ANI" => (0, Binary_SyndicateX.Factory_SpriteElement),
         "HPOINTER.TAB" or "HSPR-0.TAB" or "MFNT-0.TAB" or "MSPR-0.TAB" => (0, Binary_SyndicateX.Factory_SpriteTab),
         "HPOINTER.DAT" or "HSPR-0.DAT" or "MFNT-0.DAT" or "MSPR-0.DAT" => (0, Binary_SyndicateX.Factory_SpriteData),
         _ => throw new ArgumentOutOfRangeException(),
     };

    #endregion
}

#endregion

#region Binary_SyndicateX

public unsafe class Binary_SyndicateX : IHaveMetaInfo
{
    public enum Kind { Font, Game, MapColumn, MapData, MapTile, Mission, Palette, Raw, Req, SoundData, SoundTab, SpriteAnim, SpriteFrame, SpriteElement, SpriteTab, SpriteData };

    public static Task<object> Factory_Font(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.Font));
    public static Task<object> Factory_Game(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.Game));
    public static Task<object> Factory_MapColumn(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.MapColumn));
    public static Task<object> Factory_MapData(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.MapData));
    public static Task<object> Factory_MapTile(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.MapTile));
    public static Task<object> Factory_Mission(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.Mission));
    public static Task<object> Factory_Palette(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.Palette));
    public static Task<object> Factory_Raw(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.Raw));
    public static Task<object> Factory_Req(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.Req));
    public static Task<object> Factory_SoundData(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.SoundData));
    public static Task<object> Factory_SoundTab(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.SoundTab));
    public static Task<object> Factory_SpriteAnim(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.SpriteAnim));
    public static Task<object> Factory_SpriteFrame(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.SpriteFrame));
    public static Task<object> Factory_SpriteElement(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.SpriteElement));
    public static Task<object> Factory_SpriteTab(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.SpriteTab));
    public static Task<object> Factory_SpriteData(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SyndicateX(r, f, s, Kind.SpriteData));

    #region Headers

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_Palette
    {
        public static (string, int) Struct = ("<3x", sizeof(X_Palette));
        public byte R;
        public byte G;
        public byte B;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_Font
    {
        public static (string, int) Struct = ("<H3x", sizeof(X_Font));
        public ushort Offset;
        public byte Width;
        public byte Height;
        public byte LineOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_MapData
    {
        public static (string, int) Struct = ("<3I", sizeof(X_MapData));
        public uint MaxX;
        public uint MaxY;
        public uint MaxZ;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_SoundTab
    {
        public static (string, int) Struct = ("<I28x", sizeof(X_SoundTab));
        public uint Size;
        public fixed byte Unknown[28];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Patch(int i, byte[] sample)
        {
            // patch sample rates
            if (i == 13) sample[0x1e] = 0x9c;
            else if (i == 24) sample[0x1e] = 0x9c;
            else if (i == 25) sample[0x1e] = 0x38;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_MapTile
    {
        public static (string, int) Struct = ("<6I", sizeof(X_MapTile));
        public uint Offsets0;
        public uint Offsets1;
        public uint Offsets2;
        public uint Offsets3;
        public uint Offsets4;
        public uint Offsets5;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_SpriteTab
    {
        public static (string, int) Struct = ("<I2x", sizeof(X_SpriteTab));
        public uint Offset;
        public byte Width;
        public byte Height;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Stride() => Width <= 0 ? 0 : (Width % 8) != 0 ? ((Width / 8) + 1) * 8 : Width;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_SpriteElement
    {
        public static (string, int) Struct = ("<5H", sizeof(X_SpriteElement));
        public ushort Sprite;
        public ushort OffsetX;
        public ushort OffsetY;
        public ushort Flipped;
        public ushort NextElement;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_SpriteFrame
    {
        public static (string, int) Struct = ("<H2x2H", sizeof(X_SpriteFrame));
        public ushort FirstElement;
        public byte Width;
        public byte Height;
        public ushort Flags;
        public ushort NextElement;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameHeader
    {
        public static (string, int) Struct = ("<8x16384HH", sizeof(X_GameHeader));
        public fixed byte Header[6];
        public fixed ushort Offsets[16384]; //: 128 * 128
        public ushort OffsetRef;
    }

    ///*! Constant for field Scenario::type :  Use vehicle to go somewhere.*/
    //static const int kScenarioTypeUseVehicle = 0x02;
    ///*! Constant for field Scenario::type :  Target has escape the map.*/
    //static const int kScenarioTypeEscape = 0x07;
    ///*! Constant for field Scenario::type : this is a trigger. 
    // * Agents will trigger it when they enter the circle defined by the center and a fixed radius.*/
    //static const int kScenarioTypeTrigger = 0x08;
    ///*! Constant for field Scenario::type : Reset all scripted action.*/
    //static const int kScenarioTypeReset = 0x09;


    public enum ObjectLocation : byte
    {
        OnMap = 0x04,               // Obj is on the map (Visible)
        NotOnMap = 0x05,            // Obj is not on map (Hidden)
        OnMap2 = 0x06,              // Static: On map, but why not 0x04
        OnMap3 = 0x07,              // Static: On map, objects visibility is dependent on orientation 0x40, 0x80 are drawn
        AboveWalkSurf = 0x0C,       // Located level above possible walking surface
        NotVisible = 0x0D,          // They are not visible/present on original map (on water located)
        // 0x0D and 0x0C are excluded from being loaded
    }

    public enum ObjectState : byte
    {
        Ped_Standing = 0x0,         // Ped is standing
        Ped_Walking = 0x10,         // Ped is walking
        Ped_Dead = 0x11,            // Ped is dead
    }

    public enum ObjectType : byte
    {
        Ped = 0x01,                 // Ped
        Vehicle = 0x02,             // Vehicle
        Weapon = 0x04,              // Weapon
        Object = 0x05,              // Object; allow to display a target, a pickup, and for minimap
    }

    // Orientation
    //from 0xF0 to 0x10 : south = 0
    //from 0x10 to 0x30 : south-east = 1
    //from 0x30 to 0x50 : east = 2
    //from 0x50 to 0x70 : east-north = 3
    //from 0x70 to 0x90 : north = 4
    //from 0x90 to 0xB0 : north-west = 5
    //from 0xB0 to 0xD0 : west = 6
    //from 0xD0 to 0xF0 : west-south = 7

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameObject // 28 bytes
    {
        public static (string, int) Struct = ("<5H2x6H3x", sizeof(X_GameObject));
        public ushort OffsetNext;           // 'offset + 32774' gives the offset in this file of the next object
        public ushort OffsetPrev;           // 'offset + 32774' gives the offset in this file of the previous object (sometimes weapon, or the next target for example ???)
        public ushort TileX;
        public ushort TileY;
        public ushort TileZ;                // tile = (uint16)/128, offz =(uint16)%128 or offz = mapposz[0] & 0x1F
        public ObjectLocation Location;
        public ObjectState State;
        public ushort Unkn3;                // nothing changes when this changes
        public ushort IndexBaseAnim;        // index in (HSTA-0.ANI)
        public ushort IndexCurrentFrame;    // index in (HFRA-0.ANI)
        public ushort IndexCurrentAnim;     // index in (HSTA-0.ANI)
        public short Health;
        public ushort OffsetLastEnemy;
        public ObjectType Type;
        public byte Status_SubType;         // this can be sub type(?)
        public byte Orientation;            // surface is mapped not to 360 degrees/surface, but 256 degrees/surface
        public byte Unkn4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GamePerson // 92 bytes
    {
        public X_GameObject Base;
        public static (string, int) Struct = ("<{5H2x6H3x}2x3x10H6x2H6xH41x", sizeof(X_GamePerson));
        public byte TypePed; // when 01 pedestrian, 02 agent, 04 police, 08 guard, 16 criminal
        public fixed byte Unkn5[3];
        public ushort OffsetOfPersuader;
        public ushort Unkn6;
        public ushort OffsetOfVehicle;
        public ushort OffsetScenarioCurr; // currently executed scenario
        public ushort OffsetScenarioStart; // starting point for current scenario
        public ushort Unkn7;
        public ushort OffsetOfVehicle2; // ??
        public ushort GotoMapPosX;
        public ushort GotoMapPosY;
        public ushort GotoMapPosZ;
        public fixed byte Unkn8[6];
        public ushort OffsetEquipment;
        public ushort ModsInfo; // bitmask, 0b - gender, 1-2b - leg, 3-4b - arm, 5-6b - chest, 7-8b - heart, 9-10b - eye, 11-12b - brain, 13-15b - unknown
        public fixed byte Unkn9[6];
        public ushort OffsetCurWeapon;
        // IPA levels: white bar level,set level,exhaused level and forced level
        public byte Unkn10;
        public byte Adrena_amount;
        public byte Adrena_dependency;
        public byte Adrena_effect;
        public byte Unkn11;
        public byte InteliAmount;
        public byte InteliDependency;
        public byte InteliEffect;
        public byte Unkn12;
        public byte PercepAmount;
        public byte PercepDependency;
        public byte PercepEffect;
        public byte Unkn13;
        public fixed byte Unkn14[9];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameVehicle // 42 bytes
    {
        public static (string, int) Struct = ("<{5H2x6H3x}15x", sizeof(X_GameVehicle));
        public X_GameObject Base;
        public byte OffsetOfDriver; // driver
        public fixed byte Unkn5[11];
        public byte Speed;
        public byte Unkn6;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameStatic // 30 bytes
    {
        public static (string, int) Struct = ("<{5H2x6H3x}H", sizeof(X_GameStatic));
        public X_GameObject Base;
        public ushort Unkn27;
    }

    public enum GameWeaponSubType : byte
    {
        Persuadertron = 0x01,           // Persuadertron
        Pistol = 0x02,                  // Pistol
        GaussGun = 0x03,                // Gauss Gun
        Shotgun = 0x04,                 // Shotgun
        Uzi = 0x05,                     // Uzi
        Minigun = 0x06,                 // Minigun
        Laser = 0x07,                   // Laser
        Flamer = 0x08,                  // Flamer
        LongRange = 0x09,               // Long Range
        Scanner = 0x0A,                 // Scanner
        Medikit = 0x0B,                 // Medikit
        TimeBomb = 0x05,                // Time Bomb
        AccessCard = 0x05,              // Access Card
        Invalid1 = 0x0E,                // Invalid
        Invalid2 = 0x0F,                // Invalid
        Invalid3 = 0x10,                // Invalid
        EnergyShield = 0x11,            // Energy Shield
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameWeapon // 36 bytes
    {
        public static (string, int) Struct = ("<{5H2x6H3x}4H", sizeof(X_GameWeapon));
        public X_GameObject Base;
        public ushort OffsetNextInventory;
        public ushort OffsetPrevInventory;
        public ushort OffsetOwner;
        public ushort Unkn7;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameSfx // 30 bytes
    {
        public static (string, int) Struct = ("<{5H2x6H3x}H}", sizeof(X_GameSfx));
        public X_GameObject Base;
        public ushort OffsetOwner;
    }

    public enum GameScenarioType : byte
    {
        Scn0 = 0x00,                // unset scenario type, is found at start of array and end;
        Scn1 = 0x01,                // walking/driving to pos, x,y defined, no object offset
        Scn2 = 0x02,                // vehicle to use and goto
        Scn3 = 0x03,                // ?(south africa)
        Scn5 = 0x05,                // ?(kenya)
        Scn6 = 0x06,                // (kenya) - ped offset when in vehicle, and?
        Scn7 = 0x07,                // assasinate target escaped, mission failed
        Scn8 = 0x08,                // walking to pos, triggers on our agents in range, x,y defined
        Scn9 = 0x09,                // repeat from start, actually this might be end of script
        ScnA = 0x0A,                // train stops and waits
        ScnB = 0x0B,                // protected target reached destination(kenya)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameScenario // 8 bytes
    {
        public static (string, int) Struct = ("<2H4x", sizeof(X_GameScenario));
        public ushort Next;
        public ushort OffsetObject;
        public byte TileX;
        public byte TileY;
        public byte TileZ;
        public GameScenarioType Type;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameMapinfo // 14 bytes
    {
        public static (string, int) Struct = ("<5H4x", sizeof(X_GameMapinfo));
        public ushort Map;
        public ushort MinX;
        public ushort MinY;
        public ushort MaxX;
        public ushort MaxY;
        public byte Status; // status flag is set to 1 if the mission has been successfully completed
        public fixed byte Unkn1[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct X_GameObjective // 14 bytes
    {
        public static (string, int) Struct = ("<5H4x", sizeof(X_GameObjective));
        /* only max 5 objectives are non-zero, we will read 6
         * 0x00 action for non-agent(?) ;0x01 persuade; 0x02 assassinate;
         * 0x03 protect; 0x05 equipment aquisition; 0x0a combat sweep (police);
         * 0x0b combat sweep; 0x0e destroy vehicle; 0x0f use vehicle;
         * 0x10 evacuate
         * more info in mission.cpp : loadLevel()
        */
        public ushort Type;
        public ushort Offset;
        public ushort TileX;
        public ushort TileY;
        public ushort TileZ;
        /* If "protect", the next objective are the goals and their type is zero.
         * The list finish with zero and the offset of the protected item ?
         * The status flag is set to 1 if the objective has to be completed
        */
        public byte Status;
        public fixed byte Unkn1[3];
    }

    #endregion

    #region Objects

    public class Palette
    {
        public X_Palette[] Records;
    }

    public class Font
    {
        public X_Font[] Records;
        public byte[] Data;
    }

    public class MapData
    {
        public int MapId;
        public int MaxX;
        public int MaxY;
        public int MaxZ;
        public byte[] Tiles;
        public int Width;
        public int Height;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PatchTile(int x, int y, int z, byte tile) => Tiles[(y * MaxX + x) * MaxZ + z] = tile;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MapData Patch()
        {
            // patch for "YUKON" map
            if (MapId == 0x27)
            {
                PatchTile(60, 63, 1, 0x27);
                PatchTile(61, 63, 1, 0x27);
                PatchTile(62, 63, 1, 0x27);
                PatchTile(60, 64, 1, 0x27);
                PatchTile(61, 64, 1, 0x27);
                PatchTile(62, 64, 1, 0x27);
                PatchTile(60, 65, 1, 0x27);
                PatchTile(61, 65, 1, 0x27);
                PatchTile(62, 65, 1, 0x27);
                PatchTile(60, 66, 1, 0x27);
                PatchTile(61, 66, 1, 0x27);
                PatchTile(62, 66, 1, 0x27);
                PatchTile(60, 67, 1, 0x27);
                PatchTile(61, 67, 1, 0x27);
                PatchTile(62, 67, 1, 0x27);
            }
            // patch for "INDONESIA" map
            else if (MapId == 0x5B)
            {
                PatchTile(49, 27, 2, 0);
                PatchTile(49, 28, 2, 0);
                PatchTile(49, 29, 2, 0);
            }
            return this;
        }
    }

    public enum ColType : byte
    {
        None,
        SlopeSN,
        SlopeNS,
        SlopeEW,
        SlopeWE,
        Ground,
        RoadSideEW,
        RoadSideWE,
        RoadSideSN,
        RoadSideNS,
        Wall,
        RoadCurve,
        HandrailLight,
        Roof,
        RoadPedCross,
        RoadMark,
        Unknown
    }

    public class MapTile
    {
        public byte[][] Tiles;
        public ColType[] Types;
    }

    public class Sprite
    {
        public int Width;
        public int Height;
        public int Stride;
        public byte[] Pixels;
    }

    public class SpriteElement
    {
        public int Sprite;
        public int OffsetX;
        public int OffsetY;
        public bool Flipped;
        public int NextElement;
    }

    public class SpriteFrame
    {
        public int FirstElement;
        public int Width;
        public int Height;
        public uint Flags;
        public int NextElement;
    }

    public class Mission
    {
        public int[] InfoCosts;
        public int[] EnhtsCosts;
        public string[] Briefings;
    }

    public class Game
    {
        public X_GameHeader Header;
        public X_GamePerson[] People;
        public X_GameVehicle[] Vehicles;
        public X_GameStatic[] Statics;
        public X_GameWeapon[] Weapons;
        public X_GameSfx[] Sfxs;
        public X_GameScenario[] Scenarios;
        public byte[] Unknown9;
        public X_GameMapinfo Mapinfo;
        public X_GameObjective[] Objectives;
        public byte[] Unknown11;
    }

    #endregion

    public object Obj;

    public Binary_SyndicateX(BinaryReader r, FileSource f, PakFile s, Kind kind)
    {
        const int NUMOFTILES = 256;
        const int PIXELS_PER_BLOCK = 8;
        const int COLOR_BYTES_PER_BLOCK = PIXELS_PER_BLOCK / 2, ALPHA_BYTES_PER_BLOCK = PIXELS_PER_BLOCK / 8;
        const int BLOCK_LENGTH = COLOR_BYTES_PER_BLOCK + ALPHA_BYTES_PER_BLOCK;
        const int TILE_WIDTH = 64, TILE_HEIGHT = 48, SUBTILE_WIDTH = 32, SUBTILE_HEIGHT = 16;
        const int SUBTILE_ROW_LENGTH = BLOCK_LENGTH * SUBTILE_WIDTH / PIXELS_PER_BLOCK;

        using var r2 = new BinaryReader(new MemoryStream(Rnc.Read(r)));
        var streamSize = r2.BaseStream.Length;
        switch (kind)
        {
            case Kind.SoundData: throw new FormatException("Please load the .TAB");
            case Kind.SoundTab: // Skips Kind.SoundData
                {
                    var data = ((MemoryStream)s.LoadFileData(Path.ChangeExtension(f.Path, ".DAT")).Result).ToArray();
                    const int OFFSET = 58;
                    var sounds = new List<byte[]>();
                    r2.Seek(OFFSET);
                    var offset = 0;
                    Obj = r2.ReadSArray<X_SoundTab>((int)((streamSize - OFFSET) / sizeof(X_SoundTab)) + 1).Select((t, i) =>
                    {
                        if (t.Size <= 144) return null;
                        var sample = data[offset..(offset + (int)t.Size)];
                        offset += (int)t.Size;
                        X_SoundTab.Patch(i, sample);
                        return sample;
                    }).Where(t => t != null).ToArray();
                    break;
                }
            case Kind.Palette:
                Obj = new Palette
                {
                    Records = r2.ReadSArray<X_Palette>(256)
                };
                break;
            case Kind.Font:
                Obj = new Font
                {
                    Records = r2.ReadSArray<X_Font>(128),
                    Data = r2.ReadToEnd(),
                };
                break;
            case Kind.Req:
                //var fonts = r2.ReadSArray<X_Font>(128);
                //var data = r2.ReadToEnd();
                Obj = null;
                break;
            case Kind.MapData:
                {
                    var mapId = int.Parse(Path.GetFileNameWithoutExtension(f.Path)[3..]);
                    var t = r2.ReadS<X_MapData>();
                    int maxX = (int)t.MaxX, maxY = (int)t.MaxY, maxZ = (int)t.MaxZ;
                    var width = (maxX + maxY) * (TILE_WIDTH / 2);
                    var height = (maxX + maxY + maxZ) * TILE_HEIGHT / 3;
                    var data = r2.ReadToEnd(); r2.Seek(12);
                    var lookups = r2.ReadPArray<uint>("I", maxX * maxY);
                    // get tiles
                    // note: increased map height by 1 to enable range check on higher tiles
                    var tiles = new byte[maxX * maxY * (maxZ + 1)];
                    for (int h = 0, zr = maxZ + 1; h < maxY; h++)
                        for (int w = 0; w < maxX; w++)
                            for (int z = 0, idx = h * maxX + w; z < maxZ; z++)
                            {
                                var tileNum = data[lookups[idx] + z];
                                tiles[idx * zr + z] = tileNum;
                            }
                    // add buffer
                    maxZ++;
                    for (int h = 0, z = maxZ - 1; h < maxY; h++)
                        for (int w = 0; w < maxX; w++)
                            tiles[(h * maxX + w) * maxZ + z] = 0;
                    // set obj
                    Obj = new MapData
                    {
                        MapId = mapId,
                        Width = width,
                        Height = height,
                        MaxX = maxX,
                        MaxY = maxY,
                        MaxZ = maxZ,
                        Tiles = tiles,
                    }.Patch();
                    break;
                }
            case Kind.MapColumn: Obj = r2.ReadToEnd().Cast<ColType>().ToArray(); break;
            case Kind.MapTile: // Loads Kind.MapColumn
                {
                    var types = (ColType[])s.LoadFileObject<Binary_SyndicateX>(f.Path.Replace("HBLK", "COL")).Result.Obj;

                    static void UnpackBlock(byte[] data, Span<byte> pixels)
                    {
                        for (var j = 0; j < 4; ++j)
                            for (var i = 0; i < 8; ++i)
                                pixels[j * 8 + i] = BitValue(data[j], 7 - i) == 0
                                    ? (byte)(
                                        (byte)((BitValue(data[4 + j], 7 - i) << 0) & 0xff) |
                                        (byte)((BitValue(data[8 + j], 7 - i) << 1) & 0xff) |
                                        (byte)((BitValue(data[12 + j], 7 - i) << 2) & 0xff) |
                                        (byte)((BitValue(data[16 + j], 7 - i) << 3) & 0xff))
                                    : (byte)0xff; // transparent
                    }

                    static void LoadSubTile(BinaryReader r2, uint offset, int index, int stride, byte[] pixels)
                    {
                        r2.Seek(offset);
                        for (var i = 0; i < SUBTILE_HEIGHT; i++)
                            UnpackBlock(r2.ReadBytes(SUBTILE_ROW_LENGTH), pixels.AsSpan(index + (SUBTILE_HEIGHT - 1 - i) * stride));
                    }

                    var tiles = r2.ReadSArray<X_MapTile>(NUMOFTILES).Select(t =>
                    {
                        var b = new byte[TILE_WIDTH * TILE_HEIGHT];
                        LoadSubTile(r2, t.Offsets0, 2 * SUBTILE_HEIGHT * TILE_WIDTH + 0 * SUBTILE_WIDTH, TILE_WIDTH, b);
                        LoadSubTile(r2, t.Offsets1, 1 * SUBTILE_HEIGHT * TILE_WIDTH + 0 * SUBTILE_WIDTH, TILE_WIDTH, b);
                        LoadSubTile(r2, t.Offsets2, 0 * SUBTILE_HEIGHT * TILE_WIDTH + 0 * SUBTILE_WIDTH, TILE_WIDTH, b);
                        LoadSubTile(r2, t.Offsets3, 2 * SUBTILE_HEIGHT * TILE_WIDTH + 1 * SUBTILE_WIDTH, TILE_WIDTH, b);
                        LoadSubTile(r2, t.Offsets4, 1 * SUBTILE_HEIGHT * TILE_WIDTH + 1 * SUBTILE_WIDTH, TILE_WIDTH, b);
                        LoadSubTile(r2, t.Offsets5, 0 * SUBTILE_HEIGHT * TILE_WIDTH + 1 * SUBTILE_WIDTH, TILE_WIDTH, b);
                        return b;
                    }).ToArray();
                    Obj = new MapTile
                    {
                        Tiles = tiles,
                        Types = types,
                    };
                    break;
                }
            case Kind.SpriteData: Obj = new MemoryStream(r2.ReadToEnd()); break;
            case Kind.SpriteTab: // Loads Kind.SpriteData
                {
                    using var r3 = new BinaryReader((MemoryStream)s.LoadFileObject<Binary_SyndicateX>(Path.ChangeExtension(f.Path, ".DAT")).Result.Obj);

                    static void UnpackBlock(byte[] data, Span<byte> pixels)
                    {
                        for (var i = 0; i < 8; ++i)
                            pixels[i] = BitValue(data[0], 7 - i) == 0
                                    ? (byte)(
                                        (byte)((BitValue(data[1], 7 - i) << 0) & 0xff) |
                                        (byte)((BitValue(data[2], 7 - i) << 1) & 0xff) |
                                        (byte)((BitValue(data[3], 7 - i) << 2) & 0xff) |
                                        (byte)((BitValue(data[4], 7 - i) << 3) & 0xff))
                                    : (byte)0xff; // transparent
                    }

                    static Sprite LoadSprite(BinaryReader r, ref X_SpriteTab t, bool rle)
                    {
                        int width = t.Width, height = t.Height, stride = t.Stride();
                        if (width == 0 || height == 0) return new Sprite();
                        var pixels = new byte[stride * height];
                        r.Seek(t.Offset);
                        if (rle)
                            for (var i = 0; i < height; i++)
                            {
                                var spriteWidth = width;
                                var currentPixel = i * stride;

                                // first
                                var b = r.ReadByte();
                                var runLength = b < 128 ? b : -(256 - b);
                                while (runLength != 0)
                                {
                                    spriteWidth -= runLength;

                                    // pixel run
                                    if (runLength > 0)
                                    {
                                        if (currentPixel < 0) currentPixel = 0;
                                        if (currentPixel + runLength > height * stride) runLength = height * stride - currentPixel;
                                        for (var j = 0; j < runLength; j++) pixels[currentPixel++] = r.ReadByte();
                                    }
                                    // transparent run
                                    else if (runLength < 0)
                                    {
                                        runLength *= -1;
                                        if (currentPixel < 0) currentPixel = 0;
                                        if (currentPixel + runLength > height * stride) runLength = height * stride - currentPixel;
                                        for (var j = 0; j < runLength; j++) pixels[currentPixel++] = 0xff;
                                        new Span<byte>(pixels, currentPixel, runLength).Fill(0xff);
                                        currentPixel += runLength;
                                    }
                                    // end of the row
                                    else if (runLength == 0)
                                        spriteWidth = 0;

                                    // next
                                    b = r.ReadByte();
                                    runLength = b < 128 ? b : -(256 - b);
                                }
                            }
                        else
                            for (var j = 0; j < height; ++j)
                            {
                                var currentPixel = j * stride;
                                for (var i = 0; i < width; i += PIXELS_PER_BLOCK)
                                {
                                    UnpackBlock(r.ReadBytes(BLOCK_LENGTH), pixels.AsSpan(currentPixel));
                                    currentPixel += PIXELS_PER_BLOCK;
                                }
                            }
                        return new Sprite
                        {
                            Width = width,
                            Height = height,
                            Stride = stride,
                            Pixels = pixels,
                        };
                    }

                    Obj = r2.ReadSArray<X_SpriteTab>((int)streamSize / sizeof(X_SpriteTab)).Select(t =>
                        LoadSprite(r3, ref t, false)
                    ).ToArray();
                    break;
                }
            case Kind.SpriteElement:
                {
                    Obj = r2.ReadSArray<X_SpriteElement>((int)streamSize / sizeof(X_SpriteElement)).Select(t => new SpriteElement
                    {
                        Sprite = t.Sprite / 6,
                        OffsetX = (t.OffsetX & (1 << 15)) != 0 ? -(65536 - t.OffsetX) : t.OffsetX,
                        OffsetY = (t.OffsetY & (1 << 15)) != 0 ? -(65536 - t.OffsetY) : t.OffsetY,
                        Flipped = t.Flipped != 0,
                        NextElement = t.NextElement,
                    }).ToArray();
                    break;
                }
            case Kind.SpriteFrame:
                {
                    Obj = r2.ReadSArray<X_SpriteFrame>((int)streamSize / sizeof(X_SpriteFrame)).Select(t => new SpriteFrame
                    {
                        FirstElement = t.FirstElement,
                        Width = t.Width,
                        Height = t.Height,
                        Flags = t.Flags,
                        NextElement = t.NextElement,
                    }).ToArray();
                    break;
                }
            case Kind.SpriteAnim:
                {
                    Obj = r2.ReadPArray<ushort>("H", (int)streamSize / sizeof(ushort));
                    break;
                }
            case Kind.Mission:
                {
                    var p = Encoding.ASCII.GetString(r2.ReadToEnd()).Split("\n|\n");
                    Obj = new Mission
                    {
                        InfoCosts = p[0].Split('\n').Select(UnsafeX.Atoi).ToArray(),
                        EnhtsCosts = p[1].Split('\n').Select(UnsafeX.Atoi).ToArray(),
                        Briefings = p[2..],
                    };
                    break;
                }
            case Kind.Game:
                {
                    Obj = new Game
                    {
                        Header = r2.ReadS<X_GameHeader>(),
                        People = r2.ReadSArray<X_GamePerson>(256),
                        Vehicles = r2.ReadSArray<X_GameVehicle>(64),
                        Statics = r2.ReadSArray<X_GameStatic>(400),
                        Weapons = r2.ReadSArray<X_GameWeapon>(512),
                        Sfxs = r2.ReadSArray<X_GameSfx>(256),
                        Scenarios = r2.ReadSArray<X_GameScenario>(2048),
                        Unknown9 = r2.ReadBytes(448),
                        Mapinfo = r2.ReadS<X_GameMapinfo>(),
                        Objectives = r2.ReadSArray<X_GameObjective>(10),
                        Unknown11 = r2.ReadBytes(1896),
                    };
                    break;
                }
        }
    }

    //using var F2 = File.CreateText("C:\\T_\\FROG\\Sprite2.txt");
    //F2.Write($"{currentPixel:000}\n");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint BitValue(uint value, int index) => (value >> index) & 1;

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        => [
            new(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Bullfrog" }),
            new("Bullfrog", items: [
                //new($"Records: {Records.Length}"),
            ])
        ];
}

#endregion
