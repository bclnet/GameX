// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.Text;

namespace GameX.Origin.Netcode.UO;

/// <summary>0x03. The byte after the length. ClassicUO reads it and does nothing.</summary>
public readonly struct UOTalk {
    public UOTalk(byte type) {
        Type = type;
    }

    public byte Type { get; }
}

/// <summary>0x15. The two serials ClassicUO reads and does not use.</summary>
public readonly struct UOFollow {
    public UOFollow(uint toFollow, uint isFollowing) {
        ToFollow = toFollow;
        IsFollowing = isFollowing;
    }

    public uint ToFollow { get; }
    public uint IsFollowing { get; }
}

/// <summary>0x56. Action 1 is a pin at X, Y. Action 7 is PlotState. Other actions are the serial only.</summary>
public readonly struct UOMapData {
    public UOMapData(uint serial, byte action, ushort x, ushort y, byte plotState) {
        Serial = serial;
        Action = action;
        X = x;
        Y = y;
        PlotState = plotState;
    }

    public uint Serial { get; }
    public byte Action { get; }
    public ushort X { get; }
    public ushort Y { get; }
    public byte PlotState { get; }
}

/// <summary>One 0x66 page. Page is the packet page minus one. Lines are the strings that were read.</summary>
public readonly struct UOBookPage {
    public UOBookPage(int page, string[] lines) {
        Page = page;
        Lines = lines ?? Array.Empty<string>();
    }

    public int Page { get; }
    public string[] Lines { get; }
}

/// <summary>0x66. Lines are null-terminated UTF-8, ClassicUO ReadUTF8 for a client newer than 2.0.0.</summary>
public readonly struct UOBookData {
    public UOBookData(uint serial, UOBookPage[] pages) {
        Serial = serial;
        Pages = pages ?? Array.Empty<UOBookPage>();
    }

    public uint Serial { get; }
    public UOBookPage[] Pages { get; }
}

/// <summary>0x71. Command 0 is the board name. Command 1 is a summary. Command 2 is the message lines.</summary>
public readonly struct UOBulletin {
    public UOBulletin(byte command, uint board, uint serial, uint parent, string name, string poster, string subject, string time, string[] lines) {
        Command = command;
        Board = board;
        Serial = serial;
        Parent = parent;
        Name = name ?? "";
        Poster = poster ?? "";
        Subject = subject ?? "";
        Time = time ?? "";
        Lines = lines ?? Array.Empty<string>();
    }

    public byte Command { get; }
    public uint Board { get; }
    public uint Serial { get; }
    public uint Parent { get; }
    public string Name { get; }
    public string Poster { get; }
    public string Subject { get; }
    public string Time { get; }
    public string[] Lines { get; }
}

/// <summary>0x86. One name per slot. Thirty password bytes after each name are skipped.</summary>
public readonly struct UOCharacterList {
    public UOCharacterList(string[] names) {
        Names = names ?? Array.Empty<string>();
    }

    public string[] Names { get; }
}

/// <summary>ClassicUO handlers for 0x03, 0x15, 0x32, 0x56, 0x66, 0x71, and 0x86.</summary>
public static partial class PacketHandlers {
    static readonly object Sync = new object();
    static UOTalk _talk;
    static UOFollow _follow;
    static UOMapData _map;
    static UOBookData _book;
    static UOBulletin _bulletin;
    static UOCharacterList _characters;
    static bool _haveTalk, _haveFollow, _haveMap, _haveBook, _haveBulletin, _haveCharacters;

    /// <summary>0x03. Variable length. Reads the byte after the length.</summary>
    public static void ClientTalk(byte[] p, int o, int len) {
        if (!Window(p, o, len, out var end)) return;
        var i = o + 3;
        if (i >= end) return;
        Keep(ref _talk, ref _haveTalk, new UOTalk(p[i]));
    }

    /// <summary>0x15. Nine bytes. Reads the two serials.</summary>
    public static void FollowR(byte[] p, int o, int len) {
        if (!Window(p, o, len, out var end)) return;
        var i = o + 1;
        if (i + 8 > end) return;
        var toFollow = ReadU32(p, ref i, end);
        var isFollowing = ReadU32(p, ref i, end);
        Keep(ref _follow, ref _haveFollow, new UOFollow(toFollow, isFollowing));
    }

    /// <summary>0x32. Two bytes. ClassicUO reads nothing.</summary>
    public static void Unknown_0x32(byte[] p, int o, int len) { }

    /// <summary>0x56. Eleven bytes. Action 1 skips the pin and reads X and Y. Action 7 reads the plot state.</summary>
    public static void MapData(byte[] p, int o, int len) {
        if (!Window(p, o, len, out var end)) return;
        var i = o + 1;
        if (i + 5 > end) return;
        var serial = ReadU32(p, ref i, end);
        var action = ReadU8(p, ref i, end);
        ushort x = 0, y = 0;
        byte plot = 0;
        if (action == 1) {
            Skip(ref i, end, 1);
            x = ReadU16(p, ref i, end);
            y = ReadU16(p, ref i, end);
        }
        else if (action == 7) plot = ReadU8(p, ref i, end);
        Keep(ref _map, ref _haveMap, new UOMapData(serial, action, x, y, plot));
    }

    /// <summary>0x66. Variable length. Serial, page count, then each page's null-terminated UTF-8 lines.</summary>
    public static void BookData(byte[] p, int o, int len) {
        if (!Window(p, o, len, out var end)) return;
        var i = o + 3;
        if (i + 6 > end) return;
        var serial = ReadU32(p, ref i, end);
        var pageCnt = ReadU16(p, ref i, end);
        var pages = new List<UOBookPage>();
        for (var n = 0; n < pageCnt; n++) {
            if (i + 2 > end) break;
            var pageNum = (int)ReadU16(p, ref i, end) - 1;
            if (pageNum < 0) {
                pages.Add(new UOBookPage(pageNum, Array.Empty<string>()));
                continue;
            }
            if (i + 2 > end) {
                pages.Add(new UOBookPage(pageNum, Array.Empty<string>()));
                break;
            }
            var lineCnt = ReadU16(p, ref i, end);
            var lines = new List<string>();
            for (var line = 0; line < lineCnt; line++) {
                if (i >= end) break;
                var before = i;
                lines.Add(ReadText(p, ref i, end, -1, true, true));
                if (i == before) break;
            }
            pages.Add(new UOBookPage(pageNum, lines.ToArray()));
        }
        Keep(ref _book, ref _haveBook, new UOBookData(serial, pages.ToArray()));
    }

    /// <summary>0x71. Variable length. Commands 0 and 1 are UTF-8. Command 2 poster and time are ASCII; subject and lines are UTF-8.</summary>
    public static void BulletinBoardData(byte[] p, int o, int len) {
        if (!Window(p, o, len, out var end)) return;
        var i = o + 3;
        if (i >= end) return;
        var command = ReadU8(p, ref i, end);
        uint board = 0, serial = 0, parent = 0;
        var name = "";
        var poster = "";
        var subject = "";
        var time = "";
        var lines = Array.Empty<string>();
        switch (command) {
            case 0:
                if (i + 4 > end) break;
                board = ReadU32(p, ref i, end);
                name = ReadText(p, ref i, end, 22, true, true);
                break;
            case 1:
                board = ReadU32(p, ref i, end);
                serial = ReadU32(p, ref i, end);
                parent = ReadU32(p, ref i, end);
                poster = ReadCounted(p, ref i, end, true, true);
                subject = ReadCounted(p, ref i, end, true, true);
                time = ReadCounted(p, ref i, end, true, true);
                break;
            case 2:
                board = ReadU32(p, ref i, end);
                serial = ReadU32(p, ref i, end);
                poster = ReadCounted(p, ref i, end, false, false);
                subject = ReadCounted(p, ref i, end, true, true);
                time = ReadCounted(p, ref i, end, false, false);
                Skip(ref i, end, 4);
                var unk = ReadU8(p, ref i, end);
                if (unk > 0) Skip(ref i, end, unk * 4);
                var count = ReadU8(p, ref i, end);
                var list = new List<string>();
                for (var n = 0; n < count; n++) {
                    var lineLen = ReadU8(p, ref i, end);
                    if (lineLen > 0) list.Add(ReadText(p, ref i, end, lineLen, true, true));
                }
                lines = list.ToArray();
                break;
        }
        Keep(ref _bulletin, ref _haveBulletin, new UOBulletin(command, board, serial, parent, name, poster, subject, time, lines));
    }

    /// <summary>0x86. Variable length. A count, then a 30-byte ASCII name and 30 skipped bytes per slot.</summary>
    public static void UpdateCharacterList(byte[] p, int o, int len) {
        if (!Window(p, o, len, out var end)) return;
        var i = o + 3;
        if (i >= end) return;
        var count = ReadU8(p, ref i, end);
        var names = new List<string>();
        for (var n = 0; n < count; n++) {
            if (end - i < 60) break;
            names.Add(ReadText(p, ref i, end, 30, false, false).TrimEnd('\0'));
            Skip(ref i, end, 30);
        }
        Keep(ref _characters, ref _haveCharacters, new UOCharacterList(names.ToArray()));
    }

    /// <summary>The latest 0x03, once.</summary>
    public static bool TakeTalk(out UOTalk value) => Take(ref _talk, ref _haveTalk, out value);

    /// <summary>The latest 0x15, once.</summary>
    public static bool TakeFollow(out UOFollow value) => Take(ref _follow, ref _haveFollow, out value);

    /// <summary>The latest 0x56, once.</summary>
    public static bool TakeMapData(out UOMapData value) => Take(ref _map, ref _haveMap, out value);

    /// <summary>The latest 0x66, once.</summary>
    public static bool TakeBookData(out UOBookData value) => Take(ref _book, ref _haveBook, out value);

    /// <summary>The latest 0x71, once.</summary>
    public static bool TakeBulletin(out UOBulletin value) => Take(ref _bulletin, ref _haveBulletin, out value);

    /// <summary>The latest 0x86, once.</summary>
    public static bool TakeCharacterList(out UOCharacterList value) => Take(ref _characters, ref _haveCharacters, out value);

    static void Keep<T>(ref T slot, ref bool have, T value) where T : struct {
        lock (Sync) {
            slot = value;
            have = true;
        }
    }

    static bool Take<T>(ref T slot, ref bool have, out T value) where T : struct {
        lock (Sync) {
            if (!have) {
                value = default;
                return false;
            }
            value = slot;
            slot = default;
            have = false;
            return true;
        }
    }

    static bool Window(byte[] p, int o, int len, out int end) {
        end = 0;
        if (p == null || o < 0 || len < 1 || o >= p.Length) return false;
        var last = (long)o + len;
        if (last <= o) return false;
        end = last > p.Length ? p.Length : (int)last;
        return true;
    }

    static uint ReadU32(byte[] p, ref int i, int end) {
        if (i < 0 || i + 4 > end) return 0;
        var v = ((uint)p[i] << 24) | ((uint)p[i + 1] << 16) | ((uint)p[i + 2] << 8) | p[i + 3];
        i += 4;
        return v;
    }

    static ushort ReadU16(byte[] p, ref int i, int end) {
        if (i < 0 || i + 2 > end) return 0;
        var v = (ushort)((p[i] << 8) | p[i + 1]);
        i += 2;
        return v;
    }

    static byte ReadU8(byte[] p, ref int i, int end) {
        if (i < 0 || i >= end) return 0;
        return p[i++];
    }

    static void Skip(ref int i, int end, int count) {
        if (count <= 0 || i >= end) return;
        var room = end - i;
        i += count < room ? count : room;
    }

    static string ReadCounted(byte[] p, ref int i, int end, bool utf8, bool safe) {
        var n = ReadU8(p, ref i, end);
        if (n == 0) return "";
        return ReadText(p, ref i, end, n, utf8, safe);
    }

    static string ReadText(byte[] p, ref int i, int end, int length, bool utf8, bool safe) {
        if (i < 0 || i >= end || length == 0) return "";
        var fixedLength = length > 0;
        var remaining = end - i;
        if (!fixedLength && remaining < 1) return "";
        var size = fixedLength ? length : remaining;
        if (size > remaining) size = remaining;
        var index = -1;
        for (var k = 0; k < size; k++) {
            if (p[i + k] == 0) {
                index = k;
                break;
            }
        }
        var take = index < 0 ? size : index;
        var result = take <= 0 ? "" : (utf8 ? Encoding.UTF8 : Encoding.ASCII).GetString(p, i, take);
        if (safe && result.Length != 0) result = Strip(result);
        var step = take;
        if (!fixedLength && index >= 0) step += 1;
        if (fixedLength && length > step) step = length;
        if (step > remaining) step = remaining;
        i += step;
        return result;
    }

    static string Strip(string s) {
        for (var k = 0; k < s.Length; k++) {
            var c = s[k];
            if (c < 0x20 || c >= '\uFFFE') {
                var sb = new StringBuilder(s.Length);
                for (var j = 0; j < s.Length; j++) {
                    var d = s[j];
                    if (d >= 0x20 && d < '\uFFFE') sb.Append(d);
                }
                return sb.ToString();
            }
        }
        return s;
    }
}
