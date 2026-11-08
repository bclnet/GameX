// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Text;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO PartyManager. Members, leader, inviter, loot, and the heal target stay here.
/// ParsePacket starts at the party code byte, after 0xBF subcommand 6.
/// </summary>
public sealed class PartyManager {
    const int PARTY_SIZE = 10;

    readonly World _world;

    public PartyManager(World world) { _world = world; }

    public uint Leader { get; set; }
    public uint Inviter { get; set; }
    public bool CanLoot { get; set; }

    public PartyMember[] Members { get; } = new PartyMember[PARTY_SIZE];

    public long PartyHealTimer { get; set; }
    public uint PartyHealTarget { get; set; }

    /// <summary><paramref name="offset"/> is the code byte. <paramref name="end"/> is exclusive.</summary>
    public void ParsePacket(byte[] p, int offset, int end) {
        var r = new Cursor(p, offset, end);
        var code = r.ReadUInt8();
        var add = false;
        switch (code) {
            case 1:
                add = true;
                goto case 2;

            case 2: {
                var count = r.ReadUInt8();
                if (count <= 1) {
                    Leader = 0;
                    Inviter = 0;
                    for (var i = 0; i < PARTY_SIZE; i++) {
                        if (Members[i] == null || Members[i].Serial == 0) break;
                        var serial = Members[i].Serial;
                        if (code == 2) Members[i].Serial = 0;
                        RequestHealthBar(serial);
                    }
                    Clear();
                    RequestPartyGump();
                    break;
                }

                Clear();
                var toRemove = 0xFFFF_FFFF;
                if (!add) {
                    toRemove = r.ReadUInt32BE();
                    RequestHealthBar(toRemove);
                }

                var removeAll = !add && toRemove == UOLoginNet.PlayerSerial;
                var done = 0;
                for (var i = 0; i < count; i++) {
                    var serial = r.ReadUInt32BE();
                    var remove = !add && serial == toRemove;
                    if (remove && serial == toRemove && i == 0) removeAll = true;
                    if (!remove && !removeAll) {
                        if (i < PARTY_SIZE && !Contains(serial))
                            Members[i] = new PartyMember(_world, serial);
                        done++;
                    }
                    if (i == 0 && !remove && !removeAll) Leader = serial;
                    RequestHealthBar(serial);
                }

                if (done <= 1 && !add) {
                    for (var i = 0; i < PARTY_SIZE; i++) {
                        if (Members[i] != null && SerialHelper.IsValid(Members[i].Serial)) {
                            var serial = Members[i].Serial;
                            Members[i] = null;
                            RequestHealthBar(serial);
                        }
                    }
                    Clear();
                }

                RequestPartyGump();
                break;
            }

            case 3:
            case 4: {
                var ser = r.ReadUInt32BE();
                var text = r.ReadUnicodeBE();
                for (var i = 0; i < PARTY_SIZE; i++) {
                    if (Members[i] != null && Members[i].Serial == ser) {
                        NotePartyMessage(text, Members[i].Name);
                        break;
                    }
                }
                break;
            }

            case 7:
                Inviter = r.ReadUInt32BE();
                // No profile flag in this tree. The invite gump is the one PartyGump.Poll opens.
                UIManager.Add(new PartyInviteGump(_world, Inviter));
                break;
        }
    }

    public bool Contains(uint serial) {
        for (var i = 0; i < PARTY_SIZE; i++) {
            var mem = Members[i];
            if (mem != null && mem.Serial == serial) return true;
        }
        return false;
    }

    public void Clear() {
        Leader = 0;
        Inviter = 0;
        for (var i = 0; i < PARTY_SIZE; i++) Members[i] = null;
    }

    static void RequestHealthBar(uint serial) {
        // BaseHealthBarGump is not in this tree.
        _ = serial;
    }

    static void RequestPartyGump() {
        var gump = UIManager.GetGump<PartyGump>();
        if (gump != null) gump.InvalidateContents = true;
    }

    static void NotePartyMessage(string text, string name) {
        // no MessageManager in this tree
        if (string.IsNullOrEmpty(text)) return;
        _ = name;
    }

    struct Cursor {
        readonly byte[] _data;
        readonly int _end;
        int _i;

        public Cursor(byte[] data, int offset, int end) {
            _data = data;
            _i = offset < 0 ? 0 : offset;
            if (data == null) _end = _i;
            else _end = end > data.Length ? data.Length : end;
        }

        public byte ReadUInt8() {
            if (_data == null || _i + 1 > _end) return 0;
            return _data[_i++];
        }

        public uint ReadUInt32BE() {
            if (_data == null || _i + 4 > _end) return 0;
            var i = _i;
            _i += 4;
            return ((uint)_data[i] << 24) | ((uint)_data[i + 1] << 16) | ((uint)_data[i + 2] << 8) | _data[i + 3];
        }

        public string ReadUnicodeBE() {
            if (_data == null || _i + 2 > _end) return "";
            var n = _i;
            while (n + 1 < _end && (_data[n] != 0 || _data[n + 1] != 0)) n += 2;
            var size = n - _i;
            var text = size == 0 ? "" : Encoding.BigEndianUnicode.GetString(_data, _i, size);
            _i = n + 1 < _end ? n + 2 : n;
            return text;
        }
    }
}

/// <summary>One party slot. The name is NameOf while that mobile is in the world.</summary>
public class PartyMember : IEquatable<PartyMember> {
    readonly World _world;
    string _name;

    public PartyMember(World world, uint serial) {
        _world = world;
        Serial = serial;
        _name = Name;
    }

    public string Name {
        get {
            if (_world.Mobiles.TryGetValue(Serial, out var obj) && obj is Mobile) {
                _name = UOLoginNet.NameOf(Serial);
                if (string.IsNullOrEmpty(_name)) _name = "<not seeing>";
            }
            return _name;
        }
    }

    public bool Equals(PartyMember other) {
        if (other == null) return false;
        return other.Serial == Serial;
    }

    public uint Serial;
}
