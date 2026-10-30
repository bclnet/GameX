// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;

namespace GameX.Origin.Netcode.UO;

/// <summary>ClassicUO Network/PacketsTable. A length of -1 is a big-endian size at bytes 1-2. Packet 0xFF is -1.</summary>
public sealed class PacketsTable {
    readonly short[] _packetsTable = {
        0x0068, 0x0005, 0x0007, -1, 0x0002, 0x0005, 0x0005, 0x0007, // 0x00
        0x000E, 0x0005, 0x000B, 0x010A, -1, 0x0003, -1, 0x003D, // 0x08
        0x00D7, -1, -1, 0x000A, 0x0006, 0x0009, 0x0001, -1, // 0x10
        -1, -1, -1, 0x0025, -1, 0x0005, 0x0004, 0x0008, // 0x18
        0x0013, 0x0008, 0x0003, 0x001A, 0x0007, 0x0014, 0x0005, 0x0002, // 0x20
        0x0005, 0x0001, 0x0005, 0x0002, 0x0002, 0x0011, 0x000F, 0x000A, // 0x28
        0x0005, 0x0001, 0x0002, 0x0002, 0x000A, 0x028D, -1, 0x0008, // 0x30
        0x0007, 0x0009, -1, -1, -1, 0x0002, 0x0025, -1, // 0x38
        0x00C9, -1, -1, 0x0229, 0x02C9, 0x0005, -1, 0x000B, // 0x40
        0x0049, 0x005D, 0x0005, 0x0009, -1, -1, 0x0006, 0x0002, // 0x48
        -1, -1, -1, 0x0002, 0x000C, 0x0001, 0x000B, 0x006E, // 0x50
        0x006A, -1, -1, 0x0004, 0x0002, 0x0049, -1, 0x0031, // 0x58
        0x0005, 0x0009, 0x000F, 0x000D, 0x0001, 0x0004, -1, 0x0015, // 0x60
        -1, -1, 0x0003, 0x0009, 0x0013, 0x0003, 0x000E, -1, // 0x68
        0x001C, -1, 0x0005, 0x0002, -1, 0x0023, 0x0010, 0x0011, // 0x70
        -1, 0x0009, -1, 0x0002, -1, 0x000D, 0x0002, -1, // 0x78
        0x003E, -1, 0x0002, 0x0027, 0x0045, 0x0002, -1, -1, // 0x80
        0x0042, -1, -1, -1, 0x000B, -1, -1, -1, // 0x88
        0x0013, 0x0041, -1, 0x0063, -1, 0x0009, -1, 0x0002, // 0x90
        -1, 0x001A, -1, 0x0102, 0x0135, 0x0033, -1, -1, // 0x98
        0x0003, 0x0009, 0x0009, 0x0009, 0x0095, -1, -1, 0x0004, // 0xA0
        -1, -1, 0x0005, -1, -1, -1, -1, 0x000D, // 0xA8
        -1, -1, -1, -1, -1, 0x0040, 0x0009, -1, // 0xB0
        -1, 0x0003, 0x0006, 0x0009, 0x0003, -1, -1, -1, // 0xB8
        0x0024, -1, -1, -1, 0x0006, 0x00CB, 0x0001, 0x0031, // 0xC0
        0x0002, 0x0006, 0x0006, 0x0007, -1, 0x0001, -1, 0x004E, // 0xC8
        -1, 0x0002, 0x0019, -1, -1, -1, -1, -1, // 0xD0
        -1, 0x010C, -1, -1, 0x0009, -1, -1, -1, // 0xD8
        -1, -1, 0x000A, -1, -1, -1, 0x0005, 0x000C, // 0xE0
        0x000D, 0x004B, 0x0003, -1, -1, -1, 0x000A, 0x0015, // 0xE8
        -1, 0x0009, 0x0019, 0x001A, -1, 0x0015, -1, -1, // 0xF0
        0x006A, -1, -1, -1, -1, -1, -1, // 0xF8
    };

    public PacketsTable(ClientVersion version) {
        if (version >= ClientVersion.CV_500A) {
            _packetsTable[0x0B] = 0x07;
            _packetsTable[0x16] = -1;
            _packetsTable[0x31] = -1;
        }
        else {
            _packetsTable[0x0B] = 0x10A;
            _packetsTable[0x16] = 0x01;
            _packetsTable[0x31] = 0x01;
        }
        if (version >= ClientVersion.CV_5090) _packetsTable[0xE1] = -1;
        else _packetsTable[0xE1] = 0x09;
        if (version >= ClientVersion.CV_6013) {
            _packetsTable[0xE3] = -1;
            _packetsTable[0xE6] = 0x05;
            _packetsTable[0xE7] = 0x0C;
            _packetsTable[0xE8] = 0x0D;
            _packetsTable[0xE9] = 0x4B;
            _packetsTable[0xEA] = 0x03;
        }
        else {
            _packetsTable[0xE3] = 0x4D;
            _packetsTable[0xE6] = -1;
            _packetsTable[0xE7] = -1;
            _packetsTable[0xE8] = -1;
            _packetsTable[0xE9] = -1;
            _packetsTable[0xEA] = -1;
        }
        if (version >= ClientVersion.CV_6017) {
            _packetsTable[0x08] = 0x0F;
            _packetsTable[0x25] = 0x15;
        }
        else {
            _packetsTable[0x08] = 0x0E;
            _packetsTable[0x25] = 0x14;
        }
        if (version >= ClientVersion.CV_6060) {
            _packetsTable[0xEE] = 0x2000;
            _packetsTable[0xEF] = 0x2000;
            _packetsTable[0xF1] = 0x09;
        }
        else {
            _packetsTable[0xEE] = -1;
            _packetsTable[0xEF] = 0x15;
            _packetsTable[0xF1] = -1;
        }
        if (version >= ClientVersion.CV_60142) _packetsTable[0xB9] = 0x05;
        else _packetsTable[0xB9] = 0x03;
        if (version >= ClientVersion.CV_7000) {
            _packetsTable[0xEE] = 0x0A;
            _packetsTable[0xEF] = 0x15;
        }
        else {
            _packetsTable[0xEE] = -1;
            _packetsTable[0xEF] = 0x15;
        }
        if (version >= ClientVersion.CV_7090) {
            _packetsTable[0x24] = 0x09;
            _packetsTable[0x99] = 0x1E;
            _packetsTable[0xBA] = 0x0A;
            _packetsTable[0xF3] = 0x1A;
            _packetsTable[0xF1] = 0x09;
            _packetsTable[0xF2] = 0x19;
        }
        else {
            _packetsTable[0x24] = 0x07;
            _packetsTable[0x99] = 0x1A;
            _packetsTable[0xBA] = 0x06;
            _packetsTable[0xF3] = 0x18;
            _packetsTable[0xF1] = -1;
            _packetsTable[0xF2] = -1;
        }
        if (version >= ClientVersion.CV_70180) _packetsTable[0x00] = 0x6A;
        else _packetsTable[0x00] = 0x68;
        if (version >= ClientVersion.CV_706400) {
            _packetsTable[0xFA] = 0x01;
            _packetsTable[0xFB] = 0x02;
        }
        if (version >= ClientVersion.CV_7010400) {
            _packetsTable[0xD5] = 0x09;
            _packetsTable[0xFD] = 2;
        }
    }

    public short GetPacketLength(int id) => (short)(id >= 0xFF ? -1 : _packetsTable[id]);
}
