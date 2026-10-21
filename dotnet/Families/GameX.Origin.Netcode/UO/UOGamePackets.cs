// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.Netcode.UO;

public readonly struct UOMobileState {
    public UOMobileState(uint serial, ushort body, int x, int y, int z, byte direction) {
        Serial = serial;
        Body = body;
        X = x;
        Y = y;
        Z = z;
        Direction = direction;
    }

    public uint Serial { get; }
    public ushort Body { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public byte Direction { get; }
}

public readonly struct UOItemState {
    public UOItemState(uint serial, ushort graphic, int x, int y, int z) {
        Serial = serial;
        Graphic = graphic;
        X = x;
        Y = y;
        Z = z;
    }

    public uint Serial { get; }
    public ushort Graphic { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
}

/// <summary>
/// Game-stream framing. Lengths match Origin's PacketsTable. 0xFF is variable; that row is missing there.
/// </summary>
public static class UOGamePackets {
    public static readonly short[] Lengths = {
        0x0068, 0x0005, 0x0007, -1, 0x0002, 0x0005, 0x0005, 0x0007,
        0x000E, 0x0005, 0x000B, 0x010A, -1, 0x0003, -1, 0x003D,
        0x00D7, -1, -1, 0x000A, 0x0006, 0x0009, 0x0001, -1,
        -1, -1, -1, 0x0025, -1, 0x0005, 0x0004, 0x0008,
        0x0013, 0x0008, 0x0003, 0x001A, 0x0007, 0x0014, 0x0005, 0x0002,
        0x0005, 0x0001, 0x0005, 0x0002, 0x0002, 0x0011, 0x000F, 0x000A,
        0x0005, 0x0001, 0x0002, 0x0002, 0x000A, 0x028D, -1, 0x0008,
        0x0007, 0x0009, -1, -1, -1, 0x0002, 0x0025, -1,
        0x00C9, -1, -1, 0x0229, 0x02C9, 0x0005, -1, 0x000B,
        0x0049, 0x005D, 0x0005, 0x0009, -1, -1, 0x0006, 0x0002,
        -1, -1, -1, 0x0002, 0x000C, 0x0001, 0x000B, 0x006E,
        0x006A, -1, -1, 0x0004, 0x0002, 0x0049, -1, 0x0031,
        0x0005, 0x0009, 0x000F, 0x000D, 0x0001, 0x0004, -1, 0x0015,
        -1, -1, 0x0003, 0x0009, 0x0013, 0x0003, 0x000E, -1,
        0x001C, -1, 0x0005, 0x0002, -1, 0x0023, 0x0010, 0x0011,
        -1, 0x0009, -1, 0x0002, -1, 0x000D, 0x0002, -1,
        0x003E, -1, 0x0002, 0x0027, 0x0045, 0x0002, -1, -1,
        0x0042, -1, -1, -1, 0x000B, -1, -1, -1,
        0x0013, 0x0041, -1, 0x0063, -1, 0x0009, -1, 0x0002,
        -1, 0x001A, -1, 0x0102, 0x0135, 0x0033, -1, -1,
        0x0003, 0x0009, 0x0009, 0x0009, 0x0095, -1, -1, 0x0004,
        -1, -1, 0x0005, -1, -1, -1, -1, 0x000D,
        -1, -1, -1, -1, -1, 0x0040, 0x0009, -1,
        -1, 0x0003, 0x0006, 0x0009, 0x0003, -1, -1, -1,
        0x0024, -1, -1, -1, 0x0006, 0x00CB, 0x0001, 0x0031,
        0x0002, 0x0006, 0x0006, 0x0007, -1, 0x0001, -1, 0x004E,
        -1, 0x0002, 0x0019, -1, -1, -1, -1, -1,
        -1, 0x010C, -1, -1, 0x09, -1, -1, -1,
        -1, -1, 0x0A, -1, -1, -1, 0x05, 0x0C,
        0x0D, 0x4B, 0x03, -1, -1, -1, 0x0A, 0x0015,
        -1, 0x09, 0x19, 0x1A, -1, 0x15, -1, -1,
        0x6A, -1, -1, -1, -1, -1, -1, -1,
    };

    static ClientVersion _version;

    /// <summary>
    /// Same length patches as Origin's PacketsTable constructor. The static table is the unpatched list.
    /// </summary>
    public static void UseVersion(ClientVersion version) => _version = version;

    static short Adjusted(byte id) {
        var n = Lengths[id];
        var version = _version;
        if (version >= ClientVersion.CV_500A) {
            if (id == 0x0B) n = 0x07;
            else if (id == 0x16 || id == 0x31) n = -1;
        }
        if (version >= ClientVersion.CV_6017) {
            if (id == 0x08) n = 0x0F;
            else if (id == 0x25) n = 0x15;
        }
        if (version >= ClientVersion.CV_60142 && id == 0xB9) n = 0x05;
        if (version >= ClientVersion.CV_7090) {
            if (id == 0x24) n = 0x09;
            else if (id == 0x99) n = 0x1E;
            else if (id == 0xBA) n = 0x0A;
            else if (id == 0xF1) n = 0x09;
            else if (id == 0xF2) n = 0x19;
            else if (id == 0xF3) n = 0x1A;
        }
        if (version >= ClientVersion.CV_70180 && id == 0x00) n = 0x6A;
        if (version >= ClientVersion.CV_706400) {
            if (id == 0xFA) n = 0x01;
            else if (id == 0xFB) n = 0x02;
        }
        if (version >= ClientVersion.CV_7010400) {
            if (id == 0xD5) n = 0x09;
            else if (id == 0xFD) n = 2;
        }
        return n;
    }

    public static int FrameLength(ReadOnlySpan<byte> s) {
        if (s.Length < 1) return 0;
        var n = Adjusted(s[0]);
        if (n < 0) {
            if (s.Length < 3) return 0;
            var size = (s[1] << 8) | s[2];
            if (size < 3) return -1;
            if (size > s.Length) return 0;
            return size;
        }
        if (n == 0) return -1;
        if (s.Length < n) return 0;
        return n;
    }

    /// <summary>73-byte 0x5D. Big-endian fields. No password.</summary>
    public static byte[] PlayCharacter(string name, int slot, uint ip) {
        var p = new byte[73];
        p[0] = 0x5D;
        p[1] = p[2] = p[3] = p[4] = 0xED;
        WriteAscii(p, 5, name);
        WriteU32(p, 37, 0x1F);
        WriteU32(p, 65, (uint)slot);
        WriteU32(p, 69, ip);
        return p;
    }

    public static bool LoginConfirm(byte[] p, int o, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        if (o + 17 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        body = ReadU16(p, o + 9);
        x = ReadU16(p, o + 11);
        y = ReadU16(p, o + 13);
        z = (sbyte)p[o + 15];
        dir = p[o + 16];
        return true;
    }

    public static bool UpdatePlayer(byte[] p, int o, int len, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        if (len < 19) return false;
        serial = ReadU32(p, o + 1);
        body = ReadU16(p, o + 5);
        x = ReadU16(p, o + 11);
        y = ReadU16(p, o + 13);
        dir = p[o + 17];
        z = (sbyte)p[o + 18];
        return true;
    }

    public static bool MoveReject(byte[] p, int o, out int x, out int y, out byte dir, out int z) {
        x = y = z = 0;
        dir = 0;
        if (o + 8 > p.Length) return false;
        x = ReadU16(p, o + 2);
        y = ReadU16(p, o + 4);
        dir = p[o + 6];
        z = (sbyte)p[o + 7];
        return true;
    }

    public static bool Moving(byte[] p, int o, int len, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        if (len < 13) return false;
        serial = ReadU32(p, o + 1);
        body = ReadU16(p, o + 5);
        x = ReadU16(p, o + 7);
        y = ReadU16(p, o + 9);
        z = (sbyte)p[o + 11];
        dir = p[o + 12];
        return true;
    }

    public static bool Incoming(byte[] p, int o, int len, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        if (len < 15) return false;
        serial = ReadU32(p, o + 3);
        body = ReadU16(p, o + 7);
        x = ReadU16(p, o + 9);
        y = ReadU16(p, o + 11);
        z = (sbyte)p[o + 13];
        dir = p[o + 14];
        return true;
    }

    public static bool WorldItem(byte[] p, int o, int len, out uint serial, out ushort graphic, out int x, out int y, out int z) {
        serial = 0;
        graphic = 0;
        x = y = z = 0;
        if (len < 9) return false;
        var i = o + 3;
        var end = o + len;
        serial = ReadU32(p, i);
        i += 4;
        if (i + 2 > end) return false;
        graphic = ReadU16(p, i);
        i += 2;
        if ((serial & 0x80000000) != 0) {
            if (i + 2 > end) return false;
            i += 2;
        }
        serial &= 0x7FFFFFFF;
        graphic = (ushort)(graphic & 0x7FFF);
        if (i + 4 > end) return false;
        var xu = ReadU16(p, i);
        i += 2;
        var yu = ReadU16(p, i);
        i += 2;
        if ((xu & 0x8000) != 0) {
            if (i >= end) return false;
            i++;
        }
        x = xu & 0x7FFF;
        y = yu & 0x3FFF;
        if (i >= end) return false;
        z = (sbyte)p[i];
        return true;
    }

    /// <summary>0xBF subcommand 0x0002: big-endian keys.</summary>
    public static void FastWalk(byte[] p, int o, int len, Queue<uint> keys) {
        if (len < 5) return;
        var sub = (p[o + 3] << 8) | p[o + 4];
        if (sub != 0x0002) return;
        var i = o + 5;
        var end = o + len;
        while (i + 4 <= end) {
            keys.Enqueue(ReadU32(p, i));
            i += 4;
        }
    }

    static void WriteAscii(byte[] dest, int offset, string value) {
        if (string.IsNullOrEmpty(value)) return;
        var bytes = System.Text.Encoding.ASCII.GetBytes(value);
        var n = bytes.Length < 30 ? bytes.Length : 30;
        Buffer.BlockCopy(bytes, 0, dest, offset, n);
    }

    static void WriteU32(byte[] dest, int offset, uint value) {
        dest[offset] = (byte)(value >> 24);
        dest[offset + 1] = (byte)(value >> 16);
        dest[offset + 2] = (byte)(value >> 8);
        dest[offset + 3] = (byte)value;
    }

    static uint ReadU32(byte[] src, int offset)
        => ((uint)src[offset] << 24) | ((uint)src[offset + 1] << 16) | ((uint)src[offset + 2] << 8) | src[offset + 3];

    static ushort ReadU16(byte[] src, int offset)
        => (ushort)((src[offset] << 8) | src[offset + 1]);
}
