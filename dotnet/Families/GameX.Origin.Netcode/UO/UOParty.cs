// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.Text;

namespace GameX.Origin.Netcode.UO;

/// <summary>
/// 0xBF subcommand 6, the party block ClassicUO parses after the subcommand.
/// Code 1 is the member list (add). Code 2 is remove. Codes 3 and 4 are a message.
/// Code 7 is the invite. The list packets carry serials only, at most 10 of them.
/// Parse does not open a gump and does not send.
/// </summary>
public static class UOParty {
    const int PartySize = 10;

    static readonly object Gate = new();
    static readonly uint[] _serials = new uint[PartySize];
    static readonly string[] _names = new string[PartySize];
    static readonly Queue<(uint serial, string text)> _messages = new();
    static uint _leader;
    static uint _inviter;
    static bool _invite;
    static bool _canLoot = false;
    static int _revision;

    /// <summary>Whole 0xBF packet. <paramref name="o"/> is the id byte. Subcommand matches TryFacet.</summary>
    public static void Parse(byte[] p, int o, int len) {
        if (p == null || o < 0 || len < 6 || p.Length < 6 || o > p.Length - 6) return;
        if (((p[o + 3] << 8) | p[o + 4]) != 6) return;
        var end = len > p.Length - o ? p.Length : o + len;
        switch (p[o + 5]) {
            case 1: // add
            case 2: // remove
                ReadList(p, o + 6, end, p[o + 5] == 1);
                break;
            case 3: // private message
            case 4: // party message
                ReadMessage(p, o + 6, end);
                break;
            case 7: // invite
                ReadInvite(p, o + 6, end);
                break;
        }
    }

    public static void CopyMembers(List<(uint serial, string name)> dest) {
        lock (Gate) {
            dest.Clear();
            for (var i = 0; i < PartySize; i++)
                dest.Add((_serials[i], _names[i] ?? ""));
        }
    }

    public static uint Leader { get { lock (Gate) return _leader; } }

    public static bool CanLoot { get { lock (Gate) return _canLoot; } }

    public static int Revision { get { lock (Gate) return _revision; } }

    /// <summary>One pending invite, then cleared.</summary>
    public static bool TakeInvite(out uint inviter) {
        lock (Gate) {
            if (!_invite) {
                inviter = 0;
                return false;
            }
            inviter = _inviter;
            _inviter = 0;
            _invite = false;
            return true;
        }
    }

    /// <summary>One pending party message, then cleared.</summary>
    public static bool TakeMessage(out uint serial, out string text) {
        lock (Gate) {
            if (_messages.Count == 0) {
                serial = 0;
                text = "";
                return false;
            }
            (serial, text) = _messages.Dequeue();
            return true;
        }
    }

    static void ReadList(byte[] p, int i, int end, bool add) {
        if (i >= end) return;
        var count = p[i++];
        if (count <= 1) {
            Commit(new uint[PartySize], 0);
            return;
        }
        uint toRemove = 0xFFFFFFFF;
        if (!add) {
            if (end - i < 4) return;
            toRemove = ReadU32(p, i);
            i += 4;
        }
        if (count > (end - i) / 4) return;
        var removeAll = !add && toRemove == UOLoginNet.PlayerSerial;
        var next = new uint[PartySize];
        var done = 0;
        uint leader = 0;
        var haveLeader = false;
        for (var n = 0; n < count; n++) {
            var serial = ReadU32(p, i);
            i += 4;
            var remove = !add && serial == toRemove;
            if (remove && n == 0) removeAll = true;
            if (remove || removeAll) continue;
            done++;
            if (serial != 0 && n < PartySize && !Contains(next, serial)) next[n] = serial;
            if (n == 0) {
                leader = serial;
                haveLeader = true;
            }
        }
        if (!add && done <= 1) {
            Array.Clear(next, 0, next.Length);
            haveLeader = false;
        }
        Commit(next, haveLeader ? leader : 0);
    }

    static void ReadMessage(byte[] p, int i, int end) {
        if (end - i < 4) return;
        var serial = ReadU32(p, i);
        var text = ReadUtf16(p, i + 4, end);
        lock (Gate) _messages.Enqueue((serial, text));
    }

    static void ReadInvite(byte[] p, int i, int end) {
        if (end - i < 4) return;
        var serial = ReadU32(p, i);
        lock (Gate) {
            _inviter = serial;
            _invite = true;
        }
    }

    static void Commit(uint[] next, uint leader) {
        lock (Gate) {
            var changed = _leader != leader;
            for (var i = 0; i < PartySize; i++) {
                if (_serials[i] != next[i]) changed = true;
                _serials[i] = next[i];
                _names[i] = "";
            }
            _leader = leader;
            _inviter = 0;
            _invite = false;
            if (changed) _revision++;
        }
    }

    static bool Contains(uint[] serials, uint serial) {
        for (var i = 0; i < serials.Length; i++)
            if (serials[i] == serial) return true;
        return false;
    }

    static uint ReadU32(byte[] src, int offset)
        => ((uint)src[offset] << 24) | ((uint)src[offset + 1] << 16) | ((uint)src[offset + 2] << 8) | src[offset + 3];

    static string ReadUtf16(byte[] src, int offset, int end) {
        if (offset >= end) return "";
        var n = offset;
        while (n + 1 < end && (src[n] != 0 || src[n + 1] != 0)) n += 2;
        return n == offset ? "" : Encoding.BigEndianUnicode.GetString(src, offset, n - offset);
    }
}
