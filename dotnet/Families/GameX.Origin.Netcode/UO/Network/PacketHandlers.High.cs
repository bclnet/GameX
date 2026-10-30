// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GameX.Origin.Netcode.UO;

/// <summary>0x90 or 0xF5. Facet is read only for 0xF5.</summary>
public readonly struct UOMapState {
    public UOMapState(byte packet, uint serial, ushort gump, ushort startX, ushort startY, ushort endX, ushort endY, ushort width, ushort height, ushort facet) {
        Packet = packet;
        Serial = serial;
        Gump = gump;
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        Width = width;
        Height = height;
        Facet = facet;
    }

    public byte Packet { get; }
    public uint Serial { get; }
    public ushort Gump { get; }
    public ushort StartX { get; }
    public ushort StartY { get; }
    public ushort EndX { get; }
    public ushort EndY { get; }
    public ushort Width { get; }
    public ushort Height { get; }
    public ushort Facet { get; }
}

/// <summary>0x93 or 0xD4. Old packets use fixed UTF-8 title and author lengths.</summary>
public readonly struct UOBookState {
    public UOBookState(byte packet, uint serial, bool editable, ushort pages, string title, string author) {
        Packet = packet;
        Serial = serial;
        Editable = editable;
        Pages = pages;
        Title = title ?? "";
        Author = author ?? "";
    }

    public byte Packet { get; }
    public uint Serial { get; }
    public bool Editable { get; }
    public ushort Pages { get; }
    public string Title { get; }
    public string Author { get; }
}

/// <summary>0x99. Bytes 7 through 17 are skipped, matching Seek(18).</summary>
public readonly struct UOMultiTarget {
    public UOMultiTarget(bool allowGround, uint target, byte flags, ushort multi, ushort xOff, ushort yOff, ushort zOff, ushort hue) {
        AllowGround = allowGround;
        Target = target;
        Flags = flags;
        Multi = multi;
        XOff = xOff;
        YOff = yOff;
        ZOff = zOff;
        Hue = hue;
    }

    public bool AllowGround { get; }
    public uint Target { get; }
    public byte Flags { get; }
    public ushort Multi { get; }
    public ushort XOff { get; }
    public ushort YOff { get; }
    public ushort ZOff { get; }
    public ushort Hue { get; }
}

/// <summary>0xA5. The URL is a null-terminated string.</summary>
public readonly struct UOUrlState {
    public UOUrlState(string url) { Url = url ?? ""; }
    public string Url { get; }
}

/// <summary>0xA6. Flag 1 stops the read. Carriage returns are newlines.</summary>
public readonly struct UOTipState {
    public UOTipState(byte flag, uint tip, string text) {
        Flag = flag;
        Tip = tip;
        Text = text ?? "";
    }

    public byte Flag { get; }
    public uint Tip { get; }
    public string Text { get; }
}

/// <summary>0xAF. Three big-endian uints: mobile, corpse, running.</summary>
public readonly struct UODeathState {
    public UODeathState(uint serial, uint corpse, uint running) {
        Serial = serial;
        Corpse = corpse;
        Running = running;
    }

    public uint Serial { get; }
    public uint Corpse { get; }
    public uint Running { get; }
}

/// <summary>0xBF subcommand 0x2A. Female is the nonzero byte after the subcommand. Race is the next byte.</summary>
public readonly struct UORaceChange {
    public UORaceChange(bool female, byte race) {
        Female = female;
        Race = race;
    }

    public bool Female { get; }
    public byte Race { get; }
}

/// <summary>0xB2. Name is the channel or the user. Text is the unicode argument.</summary>
public readonly struct UOChatMessage {
    public UOChatMessage(ushort command, string name, string text, ushort userType, ushort msgType, bool hasPassword) {
        Command = command;
        Name = name ?? "";
        Text = text ?? "";
        UserType = userType;
        MsgType = msgType;
        HasPassword = hasPassword;
    }

    public ushort Command { get; }
    public string Name { get; }
    public string Text { get; }
    public ushort UserType { get; }
    public ushort MsgType { get; }
    public bool HasPassword { get; }
}

/// <summary>0xB8. Header is CP1252. Footer and body are UTF-16BE.</summary>
public readonly struct UOProfileState {
    public UOProfileState(uint serial, string header, string footer, string body) {
        Serial = serial;
        Header = header ?? "";
        Footer = footer ?? "";
        Body = body ?? "";
    }

    public uint Serial { get; }
    public string Header { get; }
    public string Footer { get; }
    public string Body { get; }
}

/// <summary>One worn item from 0x78 or 0xD3.</summary>
public readonly struct UOObjectEquip {
    public UOObjectEquip(uint serial, ushort graphic, byte layer, ushort hue) {
        Serial = serial;
        Graphic = graphic;
        Layer = layer;
        Hue = hue;
    }

    public uint Serial { get; }
    public ushort Graphic { get; }
    public byte Layer { get; }
    public ushort Hue { get; }
}

/// <summary>0x78 or 0xD3. 0xD3 skips six bytes after notoriety, then the same equipment list.</summary>
public readonly struct UOObjectState {
    public UOObjectState(byte packet, uint serial, ushort graphic, ushort x, ushort y, sbyte z, byte direction, ushort hue, byte flags, byte notoriety, UOObjectEquip[] equipment) {
        Packet = packet;
        Serial = serial;
        Graphic = graphic;
        X = x;
        Y = y;
        Z = z;
        Direction = direction;
        Hue = hue;
        Flags = flags;
        Notoriety = notoriety;
        Equipment = equipment ?? Array.Empty<UOObjectEquip>();
    }

    public byte Packet { get; }
    public uint Serial { get; }
    public ushort Graphic { get; }
    public ushort X { get; }
    public ushort Y { get; }
    public sbyte Z { get; }
    public byte Direction { get; }
    public ushort Hue { get; }
    public byte Flags { get; }
    public byte Notoriety { get; }
    public UOObjectEquip[] Equipment { get; }
}

/// <summary>One custom-house tile. Mode 2 x and y are not in the packet, so they stay 0.</summary>
public readonly struct UOHouseComponent {
    public UOHouseComponent(ushort id, sbyte x, sbyte y, sbyte z) {
        Id = id;
        X = x;
        Y = y;
        Z = z;
    }

    public ushort Id { get; }
    public sbyte X { get; }
    public sbyte Y { get; }
    public sbyte Z { get; }
}

/// <summary>One 0xD8 plane. Components are the tiles ClassicUO would add.</summary>
public readonly struct UOHousePlane {
    public UOHousePlane(int mode, int planeZ, int dlen, int clen, UOHouseComponent[] components) {
        Mode = mode;
        PlaneZ = planeZ;
        Dlen = dlen;
        Clen = clen;
        Components = components ?? Array.Empty<UOHouseComponent>();
    }

    public int Mode { get; }
    public int PlaneZ { get; }
    public int Dlen { get; }
    public int Clen { get; }
    public UOHouseComponent[] Components { get; }
}

/// <summary>0xD8. Planes follow a four-byte skip after the revision.</summary>
public readonly struct UOCustomHouse {
    public UOCustomHouse(bool compressed, bool enableResponse, uint serial, uint revision, UOHousePlane[] planes) {
        Compressed = compressed;
        EnableResponse = enableResponse;
        Serial = serial;
        Revision = revision;
        Planes = planes ?? Array.Empty<UOHousePlane>();
    }

    public bool Compressed { get; }
    public bool EnableResponse { get; }
    public uint Serial { get; }
    public uint Revision { get; }
    public UOHousePlane[] Planes { get; }
}

/// <summary>0xE5. Name is null-terminated UTF-16LE.</summary>
public readonly struct UOWaypoint {
    public UOWaypoint(uint serial, ushort x, ushort y, sbyte z, byte map, ushort type, bool ignoreObject, uint cliloc, string name) {
        Serial = serial;
        X = x;
        Y = y;
        Z = z;
        Map = map;
        Type = type;
        IgnoreObject = ignoreObject;
        Cliloc = cliloc;
        Name = name ?? "";
    }

    public uint Serial { get; }
    public ushort X { get; }
    public ushort Y { get; }
    public sbyte Z { get; }
    public byte Map { get; }
    public ushort Type { get; }
    public bool IgnoreObject { get; }
    public uint Cliloc { get; }
    public string Name { get; }
}

/// <summary>0xE6. Four-byte serial. Fixed from 6.0.1.3, otherwise after the length word.</summary>
public readonly struct UOWaypointRemove {
    public UOWaypointRemove(uint serial) { Serial = serial; }
    public uint Serial { get; }
}

/// <summary>One entity moved with a 0xF6 boat.</summary>
public readonly struct UOBoatEntity {
    public UOBoatEntity(uint serial, ushort x, ushort y, ushort z) {
        Serial = serial;
        X = x;
        Y = y;
        Z = z;
    }

    public uint Serial { get; }
    public ushort X { get; }
    public ushort Y { get; }
    public ushort Z { get; }
}

/// <summary>0xF6. Directions are masked with 0x7. Z is the ushort ClassicUO reads.</summary>
public readonly struct UOBoatMove {
    public UOBoatMove(uint serial, byte speed, byte moving, byte facing, ushort x, ushort y, ushort z, UOBoatEntity[] entities) {
        Serial = serial;
        Speed = speed;
        Moving = moving;
        Facing = facing;
        X = x;
        Y = y;
        Z = z;
        Entities = entities ?? Array.Empty<UOBoatEntity>();
    }

    public uint Serial { get; }
    public byte Speed { get; }
    public byte Moving { get; }
    public byte Facing { get; }
    public ushort X { get; }
    public ushort Y { get; }
    public ushort Z { get; }
    public UOBoatEntity[] Entities { get; }
}

/// <summary>0x53 or 0x85. One reason byte. 0x82 is not read here.</summary>
public readonly struct UOLoginRejection {
    public UOLoginRejection(byte packet, byte code) {
        Packet = packet;
        Code = code;
    }

    public byte Packet { get; }
    public byte Code { get; }
}

/// <summary>0xFD. Low and high are the ClassicUO pair (delay - 1) * 10 and delay * 10.</summary>
public readonly struct UOLoginDelay {
    public UOLoginDelay(byte delay) {
        Delay = delay;
        Low = (delay - 1) * 10;
        High = delay * 10;
    }

    public byte Delay { get; }
    public int Low { get; }
    public int High { get; }
}

/// <summary>An incoming packet whose ClassicUO handler reads no fields.</summary>
public readonly struct UOPacketNote {
    public UOPacketNote(byte id) { Id = id; }
    public byte Id { get; }
}

/// <summary>
/// ClassicUO handlers that were not on the login stream. Each keeps the latest snapshot. Take returns it once.
/// Version defaults to 7.0.104.0 so hue, 0xF3, 0xE6, and 0xFD match that table.
/// </summary>
public static partial class PacketHandlers {
    const int ChatMessages = 41;

    static ClientVersion _version = ClientVersion.CV_7010400;
    static PacketsTable _table = new PacketsTable(ClientVersion.CV_7010400);
    static readonly Queue<byte[]> _subs = new Queue<byte[]>();

    static UOMapState? _displayMap;
    static UOBookState? _openBook;
    static UOMultiTarget? _multi;
    static UOUrlState? _url;
    static UOTipState? _tip;
    static UODeathState? _death;
    static UORaceChange? _race;
    static UOChatMessage? _chat;
    static UOProfileState? _profile;
    static UOObjectState? _object;
    static UOCustomHouse? _house;
    static readonly Dictionary<uint, uint> _customRevision = new Dictionary<uint, uint>();
    static UOWaypoint? _waypoint;
    static UOWaypointRemove? _waypointRemove;
    static UOBoatMove? _boat;
    static UOLoginRejection? _reject;
    static UOLoginDelay? _delay;
    static UOPacketNote? _help, _messenger, _semivisible, _invalidMap, _ping, _queue, _config, _aos, _transfer, _kr;

    /// <summary>Sets the ClassicUO packet-length table and the 7.0.33.1 hue rule.</summary>
    public static void UseVersion(ClientVersion version) {
        _version = version;
        _table = new PacketsTable(version);
    }

    /// <summary>0x90 and 0xF5. Shared DisplayMap. Facet follows the size only on 0xF5.</summary>
    public static void DisplayMap(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var serial = r.U32();
        var gump = r.U16();
        var startX = r.U16();
        var startY = r.U16();
        var endX = r.U16();
        var endY = r.U16();
        var width = r.U16();
        var height = r.U16();
        ushort facet = 0;
        if (p[o] == 0xF5) facet = r.U16();
        _displayMap = new UOMapState(p[o], serial, gump, startX, startY, endX, endY, width, height, facet);
    }

    /// <summary>0x93 and 0xD4. Shared OpenBook. The second flag byte is editable only on 0xD4.</summary>
    public static void OpenBook(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var serial = r.U32();
        var oldPacket = p[o] == 0x93;
        var editable = r.U8() != 0;
        if (!oldPacket) editable = r.U8() != 0;
        else r.Skip(1);
        var pages = r.U16();
        string title, author;
        if (oldPacket) {
            title = r.Utf8(60, true);
            author = r.Utf8(30, true);
        }
        else {
            title = r.Utf8(r.U16(), true);
            author = r.Utf8(r.U16(), true);
        }
        _openBook = new UOBookState(p[o], serial, editable, pages, title, author);
    }

    /// <summary>0x99. allowGround, target, flags, then Seek(18), then multi and offsets.</summary>
    public static void MultiPlacement(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var allow = r.U8() != 0;
        var target = r.U32();
        var flags = r.U8();
        r.Seek(r.Origin + 18);
        _multi = new UOMultiTarget(allow, target, flags, r.U16(), r.U16(), r.U16(), r.U16(), r.U16());
    }

    /// <summary>0xA5. Null-terminated URL.</summary>
    public static void OpenUrl(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        _url = new UOUrlState(r.AsciiZ());
    }

    /// <summary>0xA6. Flag 1 returns before the tip id and the text.</summary>
    public static void TipWindow(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var flag = r.U8();
        if (flag == 1) {
            _tip = new UOTipState(flag, 0, "");
            return;
        }
        var tip = r.U32();
        var text = r.Ascii(r.U16()).Replace('\r', '\n');
        _tip = new UOTipState(flag, tip, text);
    }

    /// <summary>0xAF. Serial, corpse serial, and the running uint.</summary>
    public static void DisplayDeath(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        _death = new UODeathState(r.U32(), r.U32(), r.U32());
    }

    /// <summary>0xBF subcommand 0x2A. Female then race. Any other subcommand is not stored.</summary>
    public static void RaceChange(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        if (r.U16() != 0x2A) return;
        _race = new UORaceChange(r.U8() != 0, r.U8());
    }

    /// <summary>0xB2. Command switch in ClassicUO's order. Chat.enu sentences are not looked up.</summary>
    public static void ChatMessage(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var cmd = r.U16();
        switch (cmd) {
            case 0x03E8:
                r.Skip(4);
                _chat = new UOChatMessage(cmd, r.Utf16Z(Encoding.BigEndianUnicode), "", 0, 0, r.U16() == 0x31);
                return;
            case 0x03E9:
            case 0x03ED:
            case 0x03EF:
            case 0x03F1:
            case 0x03F4:
                r.Skip(4);
                _chat = new UOChatMessage(cmd, r.Utf16Z(Encoding.BigEndianUnicode), "", 0, 0, false);
                return;
            case 0x03EB:
            case 0x03EC:
            case 0x03F0:
                _chat = new UOChatMessage(cmd, "", "", 0, 0, false);
                return;
            case 0x03EE:
                r.Skip(4);
                var userType = r.U16();
                _chat = new UOChatMessage(cmd, r.Utf16Z(Encoding.BigEndianUnicode), "", userType, 0, false);
                return;
            case 0x0025:
            case 0x0026:
            case 0x0027:
                r.Skip(4);
                var msgType = r.U16();
                var name = r.Utf16Z(Encoding.BigEndianUnicode);
                _chat = new UOChatMessage(cmd, name, StripBrace(r.Utf16Z(Encoding.BigEndianUnicode)), 0, msgType, false);
                return;
            default:
                if (((cmd >= 0x0001 && cmd <= 0x0024) || (cmd >= 0x0028 && cmd <= 0x002C)) && cmd - 1 < ChatMessages) {
                    r.Skip(4);
                    _chat = new UOChatMessage(cmd, "", r.Utf16Z(Encoding.BigEndianUnicode), 0, 0, false);
                    return;
                }
                _chat = new UOChatMessage(cmd, "", "", 0, 0, false);
                return;
        }
    }

    /// <summary>0xB7. ClassicUO reads nothing.</summary>
    public static void Help(byte[] p, int o, int len) => Note(p, o, len, ref _help);

    /// <summary>0xB8. Serial, CP1252 header, UTF-16BE footer, UTF-16BE body.</summary>
    public static void CharacterProfile(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        _profile = new UOProfileState(r.U32(), r.AsciiZ(), r.Utf16Z(Encoding.BigEndianUnicode), r.Utf16Z(Encoding.BigEndianUnicode));
    }

    /// <summary>0xBB. ClassicUO reads nothing.</summary>
    public static void UltimaMessengerR(byte[] p, int o, int len) => Note(p, o, len, ref _messenger);

    /// <summary>0xC4. ClassicUO reads nothing.</summary>
    public static void Semivisible(byte[] p, int o, int len) => Note(p, o, len, ref _semivisible);

    /// <summary>0xC6. ClassicUO reads nothing.</summary>
    public static void InvalidMapEnable(byte[] p, int o, int len) => Note(p, o, len, ref _invalidMap);

    /// <summary>0xCA. ClassicUO reads nothing.</summary>
    public static void GetUserServerPingGodClientR(byte[] p, int o, int len) => Note(p, o, len, ref _ping);

    /// <summary>0xCB. ClassicUO reads nothing.</summary>
    public static void GlobalQueCount(byte[] p, int o, int len) => Note(p, o, len, ref _queue);

    /// <summary>0xD0. ClassicUO reads nothing.</summary>
    public static void ConfigurationFileR(byte[] p, int o, int len) => Note(p, o, len, ref _config);

    /// <summary>0x78 and 0xD3. Same fields. From 7.0.33.1 every item has a hue, otherwise only when graphic has 0x8000.</summary>
    public static void UpdateObject(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var serial = r.U32();
        var graphic = r.U16();
        var x = r.U16();
        var y = r.U16();
        var z = r.I8();
        var direction = r.U8();
        var hue = r.U16();
        var flags = r.U8();
        var notoriety = r.U8();
        if (p[o] != 0x78) r.Skip(6);
        var equip = new List<UOObjectEquip>();
        var itemSerial = r.U32();
        while (itemSerial != 0 && r.I < r.End) {
            var itemGraphic = r.U16();
            var layer = r.U8();
            ushort itemHue = 0;
            if (_version >= ClientVersion.CV_70331) itemHue = r.U16();
            else if ((itemGraphic & 0x8000) != 0) {
                itemGraphic = (ushort)(itemGraphic & 0x7FFF);
                itemHue = r.U16();
            }
            equip.Add(new UOObjectEquip(itemSerial, itemGraphic, layer, itemHue));
            itemSerial = r.U32();
        }
        _object = new UOObjectState(p[o], serial, graphic, x, y, z, direction, hue, flags, notoriety, equip.ToArray());
    }

    /// <summary>0xD7. ClassicUO reads nothing.</summary>
    public static void GenericAOSCommandsR(byte[] p, int o, int len) => Note(p, o, len, ref _aos);

    /// <summary>0xD8. Compression flag, response, serial, revision, skip 4, then zlib planes.</summary>
    public static void CustomHouse(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var compressed = r.U8() == 0x03;
        var enable = r.U8() != 0;
        var serial = r.U32();
        var revision = r.U32();
        r.Skip(4);
        var count = r.U8();
        var planes = new List<UOHousePlane>();
        for (var plane = 0; plane < count; plane++) {
            if (r.Remain < 4) break;
            var header = r.U32();
            var dlen = (int)(((header & 0xFF0000) >> 16) | ((header & 0xF0) << 4));
            var clen = (int)(((header & 0xFF00) >> 8) | ((header & 0x0F) << 8));
            var planeZ = (int)((header & 0x0F000000) >> 24);
            var planeMode = (int)((header & 0xF0000000) >> 28);
            if (clen <= 0) continue;
            if (clen > r.Remain) break;
            var raw = dlen > 0 ? Inflate(p, r.I, clen, dlen) : Array.Empty<byte>();
            r.Skip(clen);
            planes.Add(new UOHousePlane(planeMode, planeZ, dlen, clen, ReadHouse(raw, dlen, planeMode, planeZ)));
        }
        _house = new UOCustomHouse(compressed, enable, serial, revision, planes.ToArray());
        lock (_customRevision) _customRevision[serial] = revision;
    }

    /// <summary>0xDB. ClassicUO reads nothing.</summary>
    public static void CharacterTransferLog(byte[] p, int o, int len) => Note(p, o, len, ref _transfer);

    /// <summary>0xE3. ClassicUO reads nothing.</summary>
    public static void KREncryptionResponse(byte[] p, int o, int len) => Note(p, o, len, ref _kr);

    /// <summary>0xE5. Serial, position, map, type, ignore, cliloc, UTF-16LE name.</summary>
    public static void DisplayWaypoint(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        _waypoint = new UOWaypoint(r.U32(), r.U16(), r.U16(), r.I8(), r.U8(), r.U16(), r.U16() != 0, r.U32(), r.Utf16Z(Encoding.Unicode));
    }

    /// <summary>0xE6. The serial only.</summary>
    public static void RemoveWaypoint(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        _waypointRemove = new UOWaypointRemove(r.U32());
    }

    /// <summary>0xF6. Boat pose, then count, then each entity serial and position.</summary>
    public static void BoatMoving(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var serial = r.U32();
        var speed = r.U8();
        var moving = (byte)(r.U8() & 7);
        var facing = (byte)(r.U8() & 7);
        var x = r.U16();
        var y = r.U16();
        var z = r.U16();
        var count = r.U16();
        var rows = new List<UOBoatEntity>();
        for (var i = 0; i < count; i++) {
            if (r.Remain < 10) break;
            rows.Add(new UOBoatEntity(r.U32(), r.U16(), r.U16(), r.U16()));
        }
        _boat = new UOBoatMove(serial, speed, moving, facing, x, y, z, rows.ToArray());
    }

    /// <summary>0xF7. Count, then each 0xF3 subpacket's raw bytes. Any other id stops the list.</summary>
    public static void PacketList(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        var count = r.U16();
        var total = _table.GetPacketLength(0xF3);
        if (total < 2) return;
        var body = total - 1;
        for (var i = 0; i < count; i++) {
            if (r.Remain < 1) break;
            var id = r.U8();
            if (id != 0xF3) break;
            if (r.Remain < body) break;
            var raw = new byte[total];
            raw[0] = id;
            Buffer.BlockCopy(p, r.I, raw, 1, body);
            r.Skip(body);
            _subs.Enqueue(raw);
        }
    }

    /// <summary>0x53 and 0x85. One reason byte. Does not handle 0x82.</summary>
    public static void ReceiveLoginRejection(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        _reject = new UOLoginRejection(p[o], r.U8());
    }

    /// <summary>0xFD. One delay byte.</summary>
    public static void LoginDelay(byte[] p, int o, int len) {
        if (!Open(p, o, len, out var r)) return;
        _delay = new UOLoginDelay(r.U8());
    }

    public static bool TakeDisplayMap(out UOMapState value) => Take(ref _displayMap, out value);
    public static bool TakeOpenBook(out UOBookState value) => Take(ref _openBook, out value);
    public static bool TakeMultiPlacement(out UOMultiTarget value) => Take(ref _multi, out value);
    public static bool TakeOpenUrl(out UOUrlState value) => Take(ref _url, out value);
    public static bool TakeTipWindow(out UOTipState value) => Take(ref _tip, out value);
    public static bool TakeDisplayDeath(out UODeathState value) => Take(ref _death, out value);
    public static bool TakeRaceChange(out UORaceChange value) => Take(ref _race, out value);
    public static bool TakeChatMessage(out UOChatMessage value) => Take(ref _chat, out value);
    public static bool TakeHelp(out UOPacketNote value) => Take(ref _help, out value);
    public static bool TakeCharacterProfile(out UOProfileState value) => Take(ref _profile, out value);
    public static bool TakeUltimaMessengerR(out UOPacketNote value) => Take(ref _messenger, out value);
    public static bool TakeSemivisible(out UOPacketNote value) => Take(ref _semivisible, out value);
    public static bool TakeInvalidMapEnable(out UOPacketNote value) => Take(ref _invalidMap, out value);
    public static bool TakeGetUserServerPingGodClientR(out UOPacketNote value) => Take(ref _ping, out value);
    public static bool TakeGlobalQueCount(out UOPacketNote value) => Take(ref _queue, out value);
    public static bool TakeConfigurationFileR(out UOPacketNote value) => Take(ref _config, out value);
    public static bool TakeUpdateObject(out UOObjectState value) => Take(ref _object, out value);
    public static bool TakeGenericAOSCommandsR(out UOPacketNote value) => Take(ref _aos, out value);
    public static bool TakeCustomHouse(out UOCustomHouse value) => Take(ref _house, out value);

    /// <summary>Revision stored by 0xD8. False when that house has not arrived.</summary>
    public static bool TryCustomRevision(uint serial, out uint revision) {
        lock (_customRevision) return _customRevision.TryGetValue(serial, out revision);
    }

    /// <summary>Drops one 0xD8 revision. ClassicUO HouseManager.Remove when the item is gone.</summary>
    public static void DropCustomRevision(uint serial) {
        lock (_customRevision) _customRevision.Remove(serial);
    }

    public static void ClearCustomRevision() {
        lock (_customRevision) _customRevision.Clear();
    }
    public static bool TakeCharacterTransferLog(out UOPacketNote value) => Take(ref _transfer, out value);
    public static bool TakeKREncryptionResponse(out UOPacketNote value) => Take(ref _kr, out value);
    public static bool TakeDisplayWaypoint(out UOWaypoint value) => Take(ref _waypoint, out value);
    public static bool TakeRemoveWaypoint(out UOWaypointRemove value) => Take(ref _waypointRemove, out value);
    public static bool TakeBoatMoving(out UOBoatMove value) => Take(ref _boat, out value);

    /// <summary>The next 0xF7 subpacket, including its id byte. False when the queue is empty.</summary>
    public static bool TakeSubPacket(out byte[] packet) {
        if (_subs.Count == 0) {
            packet = null;
            return false;
        }
        packet = _subs.Dequeue();
        return true;
    }

    public static bool TakeReceiveLoginRejection(out UOLoginRejection value) => Take(ref _reject, out value);
    public static bool TakeLoginDelay(out UOLoginDelay value) => Take(ref _delay, out value);

    static bool Open(byte[] p, int o, int len, out R r) {
        r = default;
        if (p == null || len < 1 || o < 0 || o >= p.Length) return false;
        if (o + len > p.Length) len = p.Length - o;
        var start = _table.GetPacketLength(p[o]) < 0 ? 3 : 1;
        if (len < start) return false;
        r = new R(p, o, o + len);
        r.I = o + start;
        return true;
    }

    static void Note(byte[] p, int o, int len, ref UOPacketNote? slot) {
        if (p == null || len < 1 || o < 0 || o >= p.Length) return;
        slot = new UOPacketNote(p[o]);
    }

    static bool Take<T>(ref T? slot, out T value) where T : struct {
        if (slot.HasValue) {
            value = slot.Value;
            slot = null;
            return true;
        }
        value = default;
        return false;
    }

    static string StripBrace(string msg) {
        if (string.IsNullOrEmpty(msg)) return "";
        var idx = msg.IndexOf('{');
        var idxLast = msg.IndexOf('}') + 1;
        if (idxLast > idx && idx > -1) return msg.Remove(idx, idxLast - idx);
        return msg;
    }

    static UOHouseComponent[] ReadHouse(byte[] data, int dlen, int mode, int planeZ) {
        if (data == null || data.Length == 0 || dlen <= 0) return Array.Empty<UOHouseComponent>();
        var r = new R(data, 0, data.Length);
        var list = new List<UOHouseComponent>();
        int count;
        sbyte z;
        switch (mode) {
            case 0:
                count = dlen / 5;
                if (count > data.Length / 5) count = data.Length / 5;
                for (var i = 0; i < count; i++) {
                    var id = r.U16();
                    var x = r.I8();
                    var y = r.I8();
                    z = r.I8();
                    if (id != 0) list.Add(new UOHouseComponent(id, x, y, z));
                }
                break;
            case 1:
                z = PlaneTileZ(planeZ);
                count = dlen >> 2;
                if (count > data.Length / 4) count = data.Length / 4;
                for (var i = 0; i < count; i++) {
                    var id = r.U16();
                    var x = r.I8();
                    var y = r.I8();
                    if (id != 0) list.Add(new UOHouseComponent(id, x, y, z));
                }
                break;
            case 2:
                z = PlaneTileZ(planeZ);
                count = dlen >> 1;
                if (count > data.Length / 2) count = data.Length / 2;
                for (var i = 0; i < count; i++) {
                    var id = r.U16();
                    if (id != 0) list.Add(new UOHouseComponent(id, 0, 0, z));
                }
                break;
        }
        return list.Count == 0 ? Array.Empty<UOHouseComponent>() : list.ToArray();
    }

    static sbyte PlaneTileZ(int planeZ) {
        if (planeZ > 0) return (sbyte)((planeZ - 1) % 4 * 20 + 7);
        return 0;
    }

    static byte[] Inflate(byte[] src, int offset, int count, int dlen) {
        if (src == null || count <= 0 || dlen <= 0 || dlen > 4194304 || offset < 0 || offset + count > src.Length) return Array.Empty<byte>();
        if (count >= 2 && src[offset] == 0x78 && ((src[offset] << 8) + src[offset + 1]) % 31 == 0) {
            var wrapped = TryDeflate(src, offset + 2, count - 2, dlen);
            if (wrapped.Length != 0) return wrapped;
        }
        return TryDeflate(src, offset, count, dlen);
    }

    static byte[] TryDeflate(byte[] src, int offset, int count, int dlen) {
        if (count <= 0 || offset < 0 || offset + count > src.Length) return Array.Empty<byte>();
        try {
            var buf = new byte[dlen];
            using (var ms = new MemoryStream(src, offset, count, false))
            using (var z = new DeflateStream(ms, CompressionMode.Decompress)) {
                var got = 0;
                while (got < dlen) {
                    int n;
                    try { n = z.Read(buf, got, dlen - got); }
                    catch { break; }
                    if (n <= 0) break;
                    got += n;
                }
                if (got <= 0) return Array.Empty<byte>();
                if (got == dlen) return buf;
                var cut = new byte[got];
                Buffer.BlockCopy(buf, 0, cut, 0, got);
                return cut;
            }
        }
        catch { return Array.Empty<byte>(); }
    }

    static int Cp1252(byte c) {
        switch (c) {
            case 128: return 0x20AC;
            case 130: return 0x201A;
            case 131: return 0x0192;
            case 132: return 0x201E;
            case 133: return 0x2026;
            case 134: return 0x2020;
            case 135: return 0x2021;
            case 136: return 0x02C6;
            case 137: return 0x2030;
            case 138: return 0x0160;
            case 139: return 0x2039;
            case 140: return 0x0152;
            case 142: return 0x017D;
            case 145: return 0x2018;
            case 146: return 0x2019;
            case 147: return 0x201C;
            case 148: return 0x201D;
            case 149: return 0x2022;
            case 150: return 0x2013;
            case 151: return 0x2014;
            case 152: return 0x02DC;
            case 153: return 0x2122;
            case 154: return 0x0161;
            case 155: return 0x203A;
            case 156: return 0x0153;
            case 158: return 0x017E;
            case 159: return 0x0178;
            default: return c;
        }
    }

    static string SafeText(string s) {
        if (string.IsNullOrEmpty(s)) return "";
        var n = 0;
        for (var i = 0; i < s.Length; i++)
            if (s[i] >= 0x20 && s[i] < 0xFFFE) n++;
        if (n == s.Length) return s;
        var chars = new char[n];
        var w = 0;
        for (var i = 0; i < s.Length; i++)
            if (s[i] >= 0x20 && s[i] < 0xFFFE) chars[w++] = s[i];
        return new string(chars);
    }

    struct R {
        public readonly byte[] P;
        public readonly int Origin;
        public readonly int End;
        public int I;

        public R(byte[] p, int origin, int end) {
            P = p;
            Origin = origin;
            End = end;
            I = origin;
        }

        public int Remain => I >= End ? 0 : End - I;

        public byte U8() {
            if (P == null || I >= End) return 0;
            return P[I++];
        }

        public sbyte I8() => (sbyte)U8();

        public ushort U16() {
            if (P == null || I + 2 > End) {
                I = End;
                return 0;
            }
            var v = (ushort)((P[I] << 8) | P[I + 1]);
            I += 2;
            return v;
        }

        public uint U32() {
            if (P == null || I + 4 > End) {
                I = End;
                return 0;
            }
            var v = ((uint)P[I] << 24) | ((uint)P[I + 1] << 16) | ((uint)P[I + 2] << 8) | P[I + 3];
            I += 4;
            return v;
        }

        public void Skip(int count) {
            if (count > 0) I += count;
        }

        public void Seek(int index) { I = index; }

        public string AsciiZ() {
            if (P == null || I >= End) return "";
            var start = I;
            while (I < End && P[I] != 0) I++;
            var s = Cp1252Text(start, I - start);
            if (I < End) I++;
            return s;
        }

        public string Ascii(int length) {
            if (length <= 0 || P == null || I >= End) return "";
            var size = length < End - I ? length : End - I;
            var n = 0;
            while (n < size && P[I + n] != 0) n++;
            var s = Cp1252Text(I, n);
            I += size;
            return s;
        }

        public string Utf8(int length, bool safe) {
            if (length <= 0 || P == null || I >= End) return "";
            var size = length < End - I ? length : End - I;
            var n = 0;
            while (n < size && P[I + n] != 0) n++;
            var s = n == 0 ? "" : Encoding.UTF8.GetString(P, I, n);
            I += size;
            return safe ? SafeText(s) : s;
        }

        public string Utf16Z(Encoding enc) {
            if (P == null || enc == null || I + 2 > End) return "";
            var size = End - I;
            size -= size & 1;
            var n = 0;
            while (n + 1 < size && (P[I + n] != 0 || P[I + n + 1] != 0)) n += 2;
            var s = n == 0 ? "" : enc.GetString(P, I, n);
            I += n;
            if (n + 1 < size) I += 2;
            return s;
        }

        string Cp1252Text(int start, int n) {
            if (n <= 0) return "";
            var chars = new char[n];
            for (var i = 0; i < n; i++) chars[i] = (char)Cp1252(P[start + i]);
            return new string(chars);
        }
    }
}
