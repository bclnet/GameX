namespace Compression;

using System;

public class Lzo1xDecompressor(byte[] input, byte[] output) {
    //public static byte[] Decompress(byte[] compressed, int uncompressedLength) {
    //    byte[] decompressed = new byte[uncompressedLength];
    //    var lzoDecompressor = new Lzo1xDecompressor(compressed, decompressed);
    //    var result = lzoDecompressor.Decompress();
    //    if (result != 0) throw new Exception("Decompression failed.");
    //    return decompressed;
    //}
    readonly byte[] Input = input;
    readonly byte[] Output = output;
    uint InputPointer = 0;
    uint OutputPointer = 0;
    uint OutputPointer2;

    byte Read() => Input[InputPointer++];
    byte Peek() => Input[InputPointer];
    ushort ReadUInt16() => (ushort)(Input[InputPointer++] + (Input[InputPointer++] << 8));

    public int Decompress() {
        bool gotoFirst = false, gotoMatchdone = false; uint t;
        if (Peek() > 17) {
            t = (uint)(Read() - 17);
            if (t >= 4) gotoFirst = true;
            CopyBytes(Math.Max(1, t));
        }
        while (true) {
            if (gotoFirst) { gotoFirst = false; goto _first; }
            t = Read();
            if (t >= 16) goto _match;
            if (t == 0) t += 15 + ReadLength();
            CopyBytes(4 + t - 1);
        _first:
            t = Read();
            if (t >= 16) goto _match;
            OutputPointer2 = OutputPointer - (1 + 0x0800) - (t >> 2) - ((uint)Read() << 2);
            CopyBytes(3, fromOutput: true);
            gotoMatchdone = true;
        _match:
            while (true) {
                if (gotoMatchdone) { gotoMatchdone = false; goto _match_done; }
                if (t >= 64) {
                    OutputPointer2 = OutputPointer - 1 - ((t >> 2) & 7) - ((uint)Read() << 3);
                    t = (t >> 5) - 1;
                    CopyBytes(Math.Max(3, t + 2), fromOutput: true);
                    goto _match_done;
                }
                else if (t >= 32) {
                    t &= 31;
                    if (t == 0) t += 31 + ReadLength();
                    OutputPointer2 = OutputPointer - 1 - ((uint)ReadUInt16() >> 2);
                }
                else if (t >= 16) {
                    OutputPointer2 = OutputPointer - ((t & 8) << 11);
                    t &= 7;
                    if (t == 0) t += 7 + ReadLength();
                    OutputPointer2 -= (uint)ReadUInt16() >> 2;
                    if (OutputPointer2 == OutputPointer) goto _eof;
                    OutputPointer2 -= 0x4000;
                }
                else {
                    OutputPointer2 = OutputPointer - 1 - (t >> 2) - ((uint)Read() << 2);
                    CopyBytes(2, fromOutput: true);
                    goto _match_done;
                }
                if (t >= 2 * 4 - (3 - 1) && (OutputPointer - OutputPointer2) >= 4) { t += 4 - (3 - 1); CopyBytes(t, fromOutput: true); }
                else CopyBytes(Math.Max(3, t + 2), fromOutput: true);
            _match_done:
                t = (uint)(Input[InputPointer - 2] & 3);
                if (t == 0) break;
                CopyBytes(t);
                t = Read();
            }
        }
    _eof:
        return InputPointer == Input.Length ? 0 : InputPointer < Input.Length ? -8 : -4;
    }

    uint ReadLength() {
        uint length = 0U;
        while (true) {
            var b = Read();
            if (b == 0) length += 255;
            else { length += b; break; }
        }
        return length;
    }

    void CopyBytes(uint size, bool fromOutput = false) {
        if (fromOutput) {
            for (var i = 0; i < size; i++) Output[OutputPointer + i] = Output[OutputPointer2 + i];
            OutputPointer += size;
            OutputPointer2 += size;
        }
        else {
            for (var i = 0; i < size; i++) Output[OutputPointer + i] = Input[InputPointer + i];
            OutputPointer += size;
            InputPointer += size;
        }
    }

}