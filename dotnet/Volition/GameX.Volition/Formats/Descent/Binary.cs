using OpenStack.Gfx.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static GameX.Volition.Formats.Binary_Descent;

namespace GameX.Volition.Formats.Descent;

#region Binary_Bmp

public class Binary_Bmp : IHaveMetaInfo, ITexture
{
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Bmp(r, s.Game, f.Tag));

    public Binary_Bmp(BinaryReader r, FamilyGame game, object tag)
    {
        // get body
        game.Ensure();
        Body = r.ReadToEnd();

        // parse tag
        if (tag is ValueTuple<PIG_Flags, short, short> b)
        {
            PigFlags = b.Item1;
            Width = b.Item2;
            Height = b.Item3;
        }
        else throw new ArgumentOutOfRangeException(nameof(tag), tag.ToString());

        // get palette
        Palette = game.Id switch
        {
            "D" => Games.D.Database.Palette.Records,
            "D2" => Games.D2.Database.Palette.Records,
            _ => throw new ArgumentOutOfRangeException(nameof(game.Id), game.Id),
        };
    }

    PIG_Flags PigFlags;
    byte[] Body;
    byte[][] Palette;

    #region ITexture
    static readonly object Format = (TextureFormat.RGBA32, TexturePixel.Unknown);
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //(TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
    //TextureUnityFormat.RGBA32,
    //TextureUnrealFormat.Unknown);
    public int Width { get; }
    public int Height { get; }
    public int Depth { get; } = 0;
    public int MipMaps { get; } = 1;
    public TextureFlags TexFlags { get; } = 0;

    /// <summary>
    /// Set a color using palette index
    /// </summary>
    /// <param name="palette"></param>
    /// <param name="pixels"></param>
    /// <param name="pixel"></param>
    /// <param name="color"></param>
    static void SetPixel(byte[][] palette, byte[] pixels, ref int pixel, int color)
    {
        var record = palette[color];
        pixels[pixel + 0] = record[0];
        pixels[pixel + 1] = record[1];
        pixels[pixel + 2] = record[2];
        pixels[pixel + 3] = 255; // alpha channel
        pixel += 4;
    }

    public (byte[] bytes, object format, Range[] spans) Begin(string platform)
    {
        byte[] DecodeRLE()
        {
            var palette = Palette;
            var pixels = new byte[Width * Height * 4];
            var pixel = 0;
            var ofs = 0;
            var size = BitConverter.ToUInt32(Body);
            var ofsEnd = ofs + size;
            ofs += 4;
            ofs += (PigFlags & PIG_Flags.RLEBIG) != 0 ? Height * 2 : Height;
            while (ofs < ofsEnd)
            {
                var b = Body[ofs++];
                if ((b & 0xe0) == 0xe0)
                {
                    var c = b & 0x1f;
                    if (c == 0) continue;
                    b = Body[ofs++];
                    for (var i = 0; i < c; i++) SetPixel(palette, pixels, ref pixel, b);
                }
                else SetPixel(palette, pixels, ref pixel, b);
            }
            return pixels;
        }

        byte[] DecodeRaw() => Body.SelectMany(s => Palette[s]).ToArray();

        return ((PigFlags & (PIG_Flags.RLE | PIG_Flags.RLEBIG)) != 0
            ? DecodeRLE()
            : DecodeRaw(), Format, null);
    }
    public void End() { }

    #endregion

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
        new($"{nameof(Binary_Bmp)}", items: [
            new($"PigFlags: {PigFlags}"),
            new($"Width: {Width}"),
            new($"Height: {Height}"),
        ])
    ];
}

/*
if (tag is PIG_Bitmap b)
{
var width = b.Width + ((b.DFlags & PIG_DFlag.LARGE) != 0 ? 256U : 0U);
var height = b.Height;
var dataSize = width * height * 4;
var s = new MemoryStream();
// write header
var w = new BinaryWriter(s);
w.WriteT(new BmpHeader
{
    Type = 0x4d42,
    Size = (uint)sizeof(BmpHeader) + dataSize,
    OffBits = (uint)sizeof(BmpHeader),
    Info = new BmpInfoHeader
    {
        Size = (uint)sizeof(BmpInfoHeader),
        Width = width,
        Height = height,
        Planes = 1,
        BitCount = 3,
        Compression = 1,
        SizeImage = 0,
        XPixelsPerM = 0,
        YPixelsPerM = 0,
        ColorsUsed = 256,
        ColorsImportant = 0,
    }
});
w.Write(r.ReadBytes((int)dataSize));
s.Position = 0;
File.WriteAllBytes(@"C:\T_\test.bmp", s.ToArray());
return Task.FromResult<Stream>(s);
}
*/

#endregion

#region Binary_Rdl

public unsafe class Binary_Rdl : IHaveMetaInfo
{
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Rdl(r));

    #region Headers

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct X_Header
    {
        public static (string, int) Struct = ("<3I", sizeof(X_Header));
        public uint Magic;
        public uint Version;
        public uint GeoOffset;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct X_Geo
    {
        public static (string, int) Struct = ("<2H", sizeof(X_Geo));
        public ushort NumVerts;
        public ushort NumSegments;
    }

    public class Side
    {
        public int Bitmap;
        public int Bitmap2;
        public Vector2[] Uvs = new Vector2[4];
        public double[] Lights = new double[4];

        public Side(BinaryReader r)
        {
            Bitmap = r.ReadUInt16();
            if ((Bitmap & 0x8000) != 0)
            {
                Bitmap &= 0x7fff;
                Bitmap2 = r.ReadUInt16();
            }
            else Bitmap2 = -1;
            for (var j = 0; j < 4; j++)
            {
                Uvs[j] = r.ReadVector2();
                Lights[j] = ReadInt16Fixed(r);
            }
        }
        public override string ToString()
            => $"[t: {Bitmap} t2: {Bitmap2} uv: {string.Join(",", Array.ConvertAll(Uvs, x => x.ToString()))} l: {string.Join(",", Array.ConvertAll(Lights, x => x.ToString()))}]";
    }

    public class Segment
    {
        public int[] ChildIdxs = new int[6]; // left, top, right, bottom, back, front
        public int[] VertIdxs = new int[8];
        public byte[] WallIds = new byte[6];
        public bool IsSpecial;
        public byte Special;
        public byte EcNum;
        public int Value;
        public double StaticLight;
        public Side[] Sides = new Side[6];

        public Segment(BinaryReader r)
        {
            var mask = r.ReadByte();
            for (var i = 0; i < 6; i++) ChildIdxs[i] = (mask & (1 << i)) != 0 ? r.ReadInt16() : -1;
            for (var i = 0; i < 8; i++) VertIdxs[i] = r.ReadInt16();
            IsSpecial = (mask & 64) != 0;
            if (IsSpecial)
            {
                Special = r.ReadByte();
                EcNum = r.ReadByte();
                Value = r.ReadInt16();
            }
            StaticLight = ReadInt16Fixed(r);
            var wallMask = r.ReadByte();
            for (var i = 0; i < 6; i++) WallIds[i] = (wallMask & (1 << i)) != 0 ? r.ReadByte() : (byte)255;
            for (var i = 0; i < 6; i++)
                if (ChildIdxs[i] == -1 || WallIds[i] != 255)
                    Sides[i] = new Side(r);
        }
        public override string ToString()
            => $"v: {string.Join(",", Array.ConvertAll(VertIdxs, x => x.ToString()))} w: {string.Join(",", Array.ConvertAll(WallIds, x => x.ToString()))} l: {StaticLight} {string.Join(",", Array.ConvertAll(Sides, x => x == null ? "-" : x.ToString()))}";
    }

    #endregion

    public Vector3[] Vectors;
    public Segment[] Segments;

    public Binary_Rdl(BinaryReader r)
    {
        const uint MAGIC = 0x0;

        var header = r.ReadS<X_Header>();
        if (header.Magic == MAGIC) throw new FormatException("BAD MAGIC");
        r.Seek(header.GeoOffset);
        var geo = r.ReadS<X_Geo>();
        Vectors = r.ReadSArray<Vector3>(geo.NumVerts);
        Segments = r.ReadFArray<Segment>(r => new Segment(r), geo.NumSegments);
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
        new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
        new MetaInfo($"{nameof(Binary_Rdl)}", items: new List<MetaInfo> {
            new MetaInfo($"Vectors: {Vectors.Length}"),
            new MetaInfo($"Segments: {Segments.Length}"),
        })
    };

    public override string ToString() => "OK";

    static double ReadInt16Fixed(BinaryReader r) => r.ReadInt16() / 4096.0;
}

#endregion
