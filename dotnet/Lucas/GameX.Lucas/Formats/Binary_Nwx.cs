using GameX.Formats;
using OpenStack.Gfx.Textures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Lucas.Formats
{
    public class Binary_Nwx : IHaveMetaInfo, ITextureSelect
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Nwx(r, f, s));

        #region Headers

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct X_Header
        {
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
        unsafe struct X_CellHeader
        {
            public static (string, int) Struct = ("<3I", sizeof(X_CellHeader));
            public uint Magic; // 'CELT'
            public uint Count;
            public uint TableSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct X_Cell
        {
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

        static (byte r, byte g, byte b)[] PaletteBuilder(string path, PakFile pak)
        {
            var paths = path.Split(':');
            var pcx = pak.OpenPakFile(paths[0]).LoadFileObject<Binary_Pcx>(paths[1], throwOnError: false).Result;
            if (pcx == null) return null;
            var pal = pcx.GetPalette();
            var b = new List<(byte r, byte g, byte b)>();
            for (var i = 0; i < 256; i++)
                b.Add((pal[(i * 3) + 0], pal[(i * 3) + 1], pal[(i * 3) + 2]));
            return b.ToArray();
        }

        #endregion

        public Binary_Nwx(BinaryReader r, FileSource f, PakFile s)
        {
            const uint WAXF_MAGIC = 0x46584157;
            const uint CELT_MAGIC = 0x544c4543;

            Palette = Palettes.GetOrAdd(s.Game.Id switch
            {
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
            for (var i = 0; i < cellHeader.Count; i++)
            {
                var cell = r.ReadS<X_Cell>();
                if (cell.Size == 1 && cell.Width == 0) { Log($"Empty Cell: {i}"); r.ReadByte(); continue; } // 0xCD Terminator

                // Bit 0 specifies what dimension to use for column table retrieval and decompression. Just swap dimensions.
                var flip = (cell.Flags & 0x00000001) == 0;
                if (flip) { var temp = cell.Width; cell.Width = cell.Height; cell.Height = temp; }

                var cellOffsetTablePosition = r.Tell();
                var cellOffsets = r.ReadPArray<uint>("I", (int)cell.Width);
                var data = new List<byte>();
                foreach (var offset in cellOffsets)
                {
                    r.Seek(cellOffsetTablePosition + offset);
                    var pixels = new List<byte>();
                    var pixelCount = 0;
                    while (pixelCount < cell.Height)
                    {
                        var control = r.ReadByte();
                        if (control < 0x02)
                        {
                            pixels.Add(r.ReadByte());
                            pixelCount += 1;
                        }
                        else
                        {
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

        public void Select(int id) => (Width, Height, Flip, CellData) = Cells[id % Cells.Count];
        public (byte[] bytes, object format, Range[] spans) Begin(string platform)
        {
            int width = Width, height = Height;
            var data = CellData;

            var bytes = new byte[width * height * 4];
            if (Flip)
            {
                var i = 0;
                for (var row = 0; row < height; row++)
                    for (var col = 0; col < width; col++, i += 4)
                    {
                        var pixel = data[col * height + row];
                        bytes[i + 0] = Palette[pixel].r;
                        bytes[i + 1] = Palette[pixel].g;
                        bytes[i + 2] = Palette[pixel].b;
                        bytes[i + 3] = pixel == 0 ? (byte)0 : (byte)255;
                    }
            }
            else
            {
                var i = bytes.Length - 4;
                for (var row = 0; row < height; row++)
                    for (var col = 0; col < width; col++, i -= 4)
                    {
                        var pixel = data[col * height + row];
                        bytes[i + 0] = Palette[pixel].r;
                        bytes[i + 1] = Palette[pixel].g;
                        bytes[i + 2] = Palette[pixel].b;
                        bytes[i + 3] = pixel == 0 ? (byte)0 : (byte)255;
                    }
            }

            return (bytes, Format, null);
        }
        public void End() { }
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
}
