using System;
using System.Runtime.InteropServices;

namespace Compression;

public class XCompress {
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static int XMemCreateCompressionContext(XMEMCODEC type, ref PARAMETERS_LZX param, int flags, out IntPtr context);
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static int XMemResetCompressionContext(IntPtr ctx);
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static void XMemDestroyCompressionContext(IntPtr ctx);
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static int XMemCompress(IntPtr ctx, byte[] dest, ref int destSize, byte[] src, int srcSize);
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static int XMemCreateDecompressionContext(XMEMCODEC type, ref PARAMETERS_LZX param, int flags, out IntPtr ctx);
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static int XMemResetDecompressionContext(IntPtr ctx);
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static void XMemDestroyDecompressionContext(IntPtr ctx);
    [DllImport("x64/xcompress64.dll", CallingConvention = CallingConvention.StdCall)] public extern static int XMemDecompress(IntPtr ctx, byte[] dest, ref int destSize, byte[] src, int srcSize);

    public enum XMEMCODEC : int {
        DEFAULT = 0,
        LZX = 1
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PARAMETERS_LZX {
        [FieldOffset(0)] public uint Flags;
        [FieldOffset(4)] public uint WindowSize;
        [FieldOffset(8)] public uint ChunkSize;
    }

    public class CompressionContext : IDisposable {
        IntPtr ctx;

        public CompressionContext(XMEMCODEC type = XMEMCODEC.LZX) {
            var param = new PARAMETERS_LZX {
                Flags = 0,
                WindowSize = 64 * 1024, // 128 * 1024; // 128kB
                ChunkSize = 256 * 1024, // 512 * 1024; // 512kB
            };
            int hr;
            if ((hr = XMemCreateCompressionContext(type, ref param, 0, out ctx)) != 0)
                throw new Exception($"XMemCreateCompressionContext returned non-zero value {hr}.");
        }

        public void Dispose() => XMemDestroyCompressionContext(ctx);

        public void Reset() {
            int hr;
            if ((hr = XMemResetCompressionContext(ctx)) != 0)
                throw new Exception($"XMemResetCompressionContext returned non-zero value {hr}.");
        }

        /// <summary>
        /// Compresses decompressed data.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <param name="output">Where the compressed data will be put. This array will be resized to fit the data.</param>
        public void Compress(byte[] data, ref byte[] output) {
            var outputLen = output.Length;
            var outputLenRef = outputLen;
            int hr;
            if ((hr = XMemCompress(ctx, output, ref outputLenRef, data, data.Length)) != 0)
                throw new Exception($"XMemCompress returned non-zero value {hr}.");
            if (outputLen != outputLenRef) Array.Resize(ref output, outputLenRef);
        }

        /// <summary>
        /// Compresses decompressed data.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <param name="len">Length of the uncompressed data.</param>
        /// <returns>The compressed data.</returns>
        //public byte[] Compress(byte[] data, int len) {
        //    var output = new byte[len];
        //    Compress(data, ref output);
        //    return output;
        //}
    }

    public class DecompressionContext : IDisposable {
        IntPtr ctx;

        public DecompressionContext(XMEMCODEC type = XMEMCODEC.LZX) {
            int hr;
            var param = new PARAMETERS_LZX {
                Flags = 0,
                WindowSize = 64 * 1024, // 128 * 1024; // 128kB
                ChunkSize = 256 * 1024, // 512 * 1024; // 512kB
            };
            if ((hr = XMemCreateDecompressionContext(type, ref param, 0, out ctx)) != 0)
                throw new Exception($"XMemCreateDecompressionContext returned non-zero value {hr}.");
        }

        public void Dispose() => XMemDestroyDecompressionContext(ctx);

        public void Reset() {
            int hr;
            if ((hr = XMemResetDecompressionContext(ctx)) != 0)
                throw new Exception($"XMemResetDecompressionContext returned non-zero value {hr}.");
        }

        /// <summary>
        /// Decompresses compressed data.
        /// </summary>
        /// <param name="data">The data to decompress.</param>
        /// <param name="output">Where the decompressed data will put.</param>
        /// <returns>The total size of the compressed data.</returns>
        public void Decompress(byte[] data, ref byte[] output) {
            var outputLen = output.Length;
            var outputLenRef = outputLen;
            int hr;
            if ((hr = XMemDecompress(ctx, output, ref outputLenRef, data, data.Length)) != 0)
                throw new Exception($"XMemDecompress returned non-zero value {hr}.");
            if (outputLen != outputLenRef) Array.Resize(ref output, outputLenRef);
        }

        /// <summary>
        /// Decompresses compressed data.
        /// </summary>
        /// <param name="data">The data to decompress.</param>
        /// <param name="len">Length of the compressed data.</param>
        /// <returns>The decompressed data.</returns>
        //public byte[] Decompress(byte[] data, int len) {
        //    var output = new byte[len];
        //    Decompress(data, ref output);
        //    return output;
        //}
    }
}

