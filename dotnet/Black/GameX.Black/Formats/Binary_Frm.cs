using OpenStack.Gfx.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Black.Formats
{
    public class Binary_Frm : IHaveMetaInfo, ITextureFramesSelect
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Frm(r, f, s));

        // Header
        #region Headers
        // https://falloutmods.fandom.com/wiki/FRM_File_Format

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct FrmHeader
        {
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
        public unsafe struct FrmFrame
        {
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

        public unsafe Binary_Frm(BinaryReader r, FileSource f, PakFile s)
        {
            var pallet = GetPalletObjAsync(f.Path, (BinaryPakFile)s).Result ?? throw new Exception("No pallet found");
            var rgba32 = pallet.Rgba32;

            // parse header
            var header = r.ReadS<FrmHeader>();
            var frames = new List<(FrmFrame f, byte[] b)>();
            var stream = r.BaseStream;
            for (var i = 0; i < 6 * header.FramesPerDirection && stream.Position < stream.Length; i++)
            {
                var frameOffset = Header.FrameOffset[i];
                var frame = r.ReadS<FrmFrame>();
                var data = r.ReadBytes((int)frame.Size);
                var image = new byte[frame.Width * frame.Height * 4];
                fixed (byte* image_ = image)
                {
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

        async Task<Binary_Pal2> GetPalletObjAsync(string path, BinaryPakFile s)
        {
            var palletPath = $"{path[..^4]}.PAL";
            if (s.Contains(palletPath))
                return await s.LoadFileObject<Binary_Pal2>(palletPath);
            if (DefaultPallet == null && s.Contains("COLOR.PAL"))
            {
                DefaultPallet ??= await s.LoadFileObject<Binary_Pal2>("COLOR.PAL");
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

        public (byte[] bytes, object format, Range[] spans) Begin(string platform) => (Bytes, Format, null);
        public void End() { }

        // ITextureFrames
        public int Fps => Header.Fps;
        public int FrameMax => Frames.Length == 1 ? 1 : Header.FramesPerDirection;
        public void FrameSelect(int id)
        {
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
}
