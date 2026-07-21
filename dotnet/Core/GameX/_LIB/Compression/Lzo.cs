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
    uint _inputPointer = 0;
    uint _outputPointer = 0;
    uint _mOutputPointer;
    readonly byte[] _input = input;
    readonly byte[] _output = output;
    byte InputByte => _input[_inputPointer++];
    byte InputBytePeek => _input[_inputPointer];
    //byte OutputByte { set { _output[_outputPointer++] = value; } }

    public int Decompress() {
        uint t;
        bool gotoFirst = false, gotoMatchdone = false;
        if (InputBytePeek > 17) { t = (uint)(InputByte - 17); if (t >= 4) gotoFirst = true; CopyBytes(Math.Max(1, t)); }
        while (true) {
            if (gotoFirst) { gotoFirst = false; goto _first; }
            t = InputByte;
            if (t >= 16) goto _match;
            if (t == 0) t += 15 + ReadLength();
            CopyBytes(4 + t - 1);
        _first:
            t = InputByte;
            if (t >= 16) goto _match;
            _mOutputPointer = _outputPointer - (1 + 0x0800) - (t >> 2) - ((uint)InputByte << 2);
            CopyBytes(3, copyFromOutputBuffer: true);
            gotoMatchdone = true;
        _match:
            while (true) {
                if (gotoMatchdone) { gotoMatchdone = false; goto _match_done; }
                if (t >= 64) {
                    _mOutputPointer = _outputPointer - 1 - ((t >> 2) & 7) - ((uint)(InputByte << 3));
                    t = (t >> 5) - 1;
                    CopyBytes(Math.Max(3, t + 2), copyFromOutputBuffer: true);
                    goto _match_done;
                }
                else if (t >= 32) {
                    t &= 31;
                    if (t == 0) t += 31 + ReadLength();
                    _mOutputPointer = _outputPointer - 1 - (((uint)ReadUshortFromInput()) >> 2);
                    _inputPointer += 2;
                }
                else if (t >= 16) {
                    _mOutputPointer = _outputPointer - ((t & 8) << 11);
                    t &= 7;
                    if (t == 0) t += 7 + ReadLength();
                    _mOutputPointer -= (uint)ReadUshortFromInput() >> 2;
                    _inputPointer += 2;
                    if (_mOutputPointer == _outputPointer) goto _eof;
                    _mOutputPointer -= 0x4000;
                }
                else {
                    _mOutputPointer = _outputPointer - 1 - (t >> 2) - ((uint)InputByte << 2);
                    CopyBytes(2, copyFromOutputBuffer: true);
                    goto _match_done;
                }
                if (t >= 2 * 4 - (3 - 1) && (_outputPointer - _mOutputPointer) >= 4) { t += 4 - (3 - 1); CopyBytes(t, copyFromOutputBuffer: true); }
                else CopyBytes(Math.Max(3, t + 2), copyFromOutputBuffer: true);
            _match_done:
                t = (uint)(_input[_inputPointer - 2] & 3);
                if (t == 0) break;
                CopyBytes(t);
                t = InputByte;
            }
        }
    _eof:
        return _inputPointer == _input.Length ? 0 : _inputPointer < _input.Length ? -8 : -4;
    }

    uint ReadLength() {
        uint length = 0U;
        while (true) {
            var inputByte = InputByte;
            if (inputByte == 0) length += 255;
            else { length += inputByte; break; }
        }
        return length;
    }

    void CopyBytes(uint numberOfBytes, bool copyFromOutputBuffer = false) {
        if (copyFromOutputBuffer) {
            for (var i = 0; i < numberOfBytes; i++) _output[_outputPointer + i] = _output[_mOutputPointer + i];
            _outputPointer += numberOfBytes;
            _mOutputPointer += numberOfBytes;
        }
        else {
            for (var i = 0; i < numberOfBytes; i++) _output[_outputPointer + i] = _input[_inputPointer + i];
            _outputPointer += numberOfBytes;
            _inputPointer += numberOfBytes;
        }
    }

    ushort ReadUshortFromInput() => (ushort)(_input[_inputPointer] + (_input[_inputPointer + 1] << 8));
}