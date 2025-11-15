using Compression;
using Compression.Doboz;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using K4os.Compression.LZ4;
using lzo.net;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ZstdNet;
using Decoder = SevenZip.Compression.LZMA.Decoder;

namespace GameX.Formats;

public static class Compression {
    const int BufferSize = 4096 * 10;
    public const int LZMAPropsSize = 5;

    #region Doboz
    public static byte[] DecompressDoboz(this BinaryReader r, int length, int newLength) {
        var data = r.ReadBytes(length);
        return DobozDecoder.Decode(data, 0, data.Length);
    }
    public static int DecompressDoboz(byte[] source, int sourceSize, byte[] target, int targetSize)
        => DobozDecoder.Decode(source, 0, sourceSize, target, 0, targetSize);
    #endregion

    #region Lz4
    public static byte[] DecompressLz4(this BinaryReader r, int length, int newLength) {
        var data = r.ReadBytes(length);
        var res = new byte[newLength];
        LZ4Codec.Decode(data, res);
        return res;
    }
    public static int DecompressLz4(byte[] source, byte[] target) => LZ4Codec.Decode(source, 0, source.Length, target, 0, target.Length);
    #endregion

    #region Lzo
    public static byte[] DecompressLzo(this BinaryReader r, int length, int newLength) {
        var fs = new LzoStream(r.BaseStream, CompressionMode.Decompress);
        return fs.ReadBytes(newLength);
    }
    public static int DecompressLzo(byte[] source, byte[] target) {
        using var fs = new LzoStream(new MemoryStream(source), CompressionMode.Decompress);
        var data = fs.ReadBytes(target.Length);
        Buffer.BlockCopy(data, 0, target, 0, data.Length);
        return data.Length;
    }
    #endregion

    #region Lzf
    public static int DecompressLzf(this BinaryReader r, int length, byte[] buffer) {
        var data = r.ReadBytes(length);
        return Lzf.Decompress(data, buffer);
    }
    public static int DecompressLzf(byte[] source, byte[] target) => Lzf.Decompress(source, target);
    //public static byte[] DecompressLzm(this BinaryReader r, int length, int newLength)
    //{
    //    var data = r.ReadBytes(length);
    //    var inflater = new Inflater();
    //    inflater.SetInput(data, 0, data.Length);
    //    int count;
    //    var buffer = new byte[BufferSize];
    //    using (var s = new MemoryStream())
    //    {
    //        while ((count = Inflater.Inflate(buffer)) > 0) s.Write(buffer, 0, count);
    //        s.Position = 0;
    //        return s.ToArray();
    //    }
    //}
    #endregion

    #region Oodle
    public static byte[] DecompressOodle(this BinaryReader r, int length, int newLength) {
        var oodleCompression = r.ReadBytes(4);
        if (!(oodleCompression.SequenceEqual(new byte[] { 0x4b, 0x41, 0x52, 0x4b }))) throw new NotImplementedException();
        var size = r.ReadUInt32();
        if (size != newLength) throw new FormatException();
        var data = r.ReadBytes(length - 8);
        var res = new byte[newLength];
        var unpackedSize = OodleLZ.Decompress(data, res);
        if (unpackedSize != newLength) throw new FormatException($"Unpacked size does not match real size. {unpackedSize} vs {newLength}");
        return res;
    }
    public static int DecompressOodle(byte[] source, byte[] target) => OodleLZ.Decompress(source, target);
    #endregion

    #region Snappy
    public static byte[] DecompressSnappy(this BinaryReader r, int length, int newLength) => throw new NotSupportedException();
    public static int DecompressSnappy(byte[] source, byte[] target) => throw new NotSupportedException();
    #endregion

    #region Zstd
    public static byte[] DecompressZstd(this BinaryReader r, int length, int newLength) {
        using var fs = new DecompressionStream(r.BaseStream);
        return fs.ReadBytes(newLength);
    }
    public static int DecompressZstd(byte[] source, byte[] target) {
        using var fs = new DecompressionStream(new MemoryStream(source));
        var data = fs.ReadBytes(target.Length);
        Buffer.BlockCopy(data, 0, target, 0, data.Length);
        return data.Length;
    }
    #endregion

    #region Zlib
    public static byte[] DecompressZlibStream(this BinaryReader r, bool noHeader = false) {
        byte[] b = new byte[1024], res = new byte[1024]; int outputUsed = 0, count;
        var z = new Inflater(noHeader);
        while (!z.IsFinished) {
            while (z.IsNeedingInput) { if ((count = r.Read(b, 0, 1024)) <= 0) throw new EndOfStreamException("Unexpected End Of File"); z.SetInput(b, 0, count); }
            if (outputUsed == res.Length) { var _ = new byte[res.Length * 2]; Array.Copy(res, _, res.Length); res = _; }
            try { outputUsed += z.Inflate(res, outputUsed, res.Length - outputUsed); }
            catch (FormatException e) { throw new IOException(e.ToString()); }
        }
        // Adjust reader to point to the next stream correctly.
        r.BaseStream.Seek(r.BaseStream.Position - z.RemainingInput, SeekOrigin.Begin);
        z.Reset();
        // rebuild output
        var realOutput = new byte[outputUsed];
        Array.Copy(res, realOutput, outputUsed);
        return realOutput;
    }
    public static byte[] DecompressZlib(this BinaryReader r, int length, int newLength, bool noHeader = false) {
        var data = r.ReadBytes(length);
        var z = new Inflater(noHeader);
        z.SetInput(data, 0, data.Length);
        int count;
        var b = new byte[BufferSize];
        using var s = new MemoryStream();
        while ((count = z.Inflate(b)) > 0) s.Write(b, 0, count);
        return s.ToArray();
    }
    public static int DecompressZlib(byte[] source, byte[] target) {
        var z = new Inflater(false);
        z.SetInput(source, 0, source.Length);
        return z.Inflate(target, 0, target.Length);
    }
    public static byte[] DecompressZlib2(this BinaryReader r, int length, int newLength) {
        var data = r.ReadBytes(length);
        using var s = new InflaterInputStream(new MemoryStream(data), new Inflater(false), 4096);
        var res = new byte[newLength];
        s.Read(res, 0, res.Length);
        return res;
    }
    public static byte[] CompressZlib(byte[] source, int length) {
        var z = new Deflater(Deflater.BEST_COMPRESSION);
        z.SetInput(source, 0, length);
        int count;
        var b = new byte[BufferSize];
        using var s = new MemoryStream();
        while ((count = z.Deflate(b)) > 0) s.Write(b, 0, count);
        return s.ToArray();
    }
    #endregion

    #region Zlib2
    //public static ulong DecompressZlib_2(this Stream input, int length, Stream output)
    //{
    //    var written = 0UL;
    //    var data = new byte[length];
    //    input.Read(data, 0, data.Length);
    //    using (var ms = new MemoryStream(data, false))
    //    using (var lz4Stream = LZ4Decoder.Create(ms, LZ4StreamMode.Read))
    //    {
    //        var buffer = new byte[BufferSize];
    //        int count;
    //        while ((count = lz4Stream.Read(buffer, 0, buffer.Length)) > 0) { output.Write(buffer, 0, count); written += (ulong)count; }
    //    }
    //}
    #endregion

    #region Lzma
    public static byte[] DecompressLzma(this BinaryReader r, int length, int newLength) {
        var z = new Decoder();
        z.SetDecoderProperties(r.ReadBytes(5));
        var data = r.ReadBytes(length);
        using var ds = new MemoryStream(data);
        using var rs = new MemoryStream(newLength);
        z.Code(ds, rs, data.Length, newLength, null);
        return rs.ToArray();
    }
    public static int DecompressLzma(byte[] source, byte[] target) => throw new NotImplementedException();
    #endregion

    #region Blast
    public static byte[] DecompressBlast(this BinaryReader r, int length, int newLength) {
        var z = new Blast();
        var data = r.ReadBytes(length);
        //var os = new byte[newLength];
        using var os = new MemoryStream(newLength);
        z.Decompress(data, os);
        return os.ToArray();
    }
    public static int DecompressBlast(byte[] source, byte[] target) => throw new NotImplementedException();
    #endregion

    #region Lzss
    public static byte[] DecompressLzss(this BinaryReader r, int length, int newLength) {
        using var s = new Lzss.BinaryReaderE(new MemoryStream(r.ReadBytes(length)));
        return new Lzss(s, newLength).Decompress();
    }
    public static int DecompressLzss(byte[] source, byte[] target) => throw new NotImplementedException();
    #endregion

    #region Xmem
    public static byte[] DecompressXmem(this BinaryReader r, int length, int newLength, XCompress.XMEMCODEC codec = XCompress.XMEMCODEC.LZX) {
        var res = new byte[newLength];
        using var ctx = new XCompress.DecompressionContext(codec);
        ctx.Decompress(r.ReadBytes(length), ref res);
        return res;
    }
    #endregion
}


// https://encode.su/threads/4089-unusual-zip-compression-methods