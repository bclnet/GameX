// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameX.Origin.Netcode.UO;

/// <summary>
/// Login-socket result for LoginScene. Network thread posts; UI thread takes.
/// </summary>
public enum UOLoginPhase {
    ServersReady,
    CharactersReady,
    Failed,
    RelayInProgress,
    WorldEntered
}

/// <summary>
/// ClassicUO's first login socket. Clear 4-byte seed, optional encrypted 0xEF, encrypted 0x80, then login replies.
/// Login packets are not Huffman. 0x5D switches this socket onto the Huffman game stream.
/// </summary>
public static class UOLoginNet {
    const int AsciiField = 30;
    const uint SeedWhenNotIpv4 = 0x7F000001;
    const int ReadWaitMs = 50;

    static readonly object Gate = new();
    static TcpClient _client;
    static int _runId;
    static string _password;
    static int? _select;
    static bool _have;
    static string[] _takeServers, _takeCharacters;
    static string _takeMessage;
    static UOLoginPhase _takePhase;
    static int? _playIndex;
    static string _playName;
    static readonly Queue<byte> _walks = new();
    static readonly Queue<uint> _fastWalk = new();
    static readonly Dictionary<uint, UOMobileState> _mobiles = new();
    static readonly Dictionary<uint, UOItemState> _items = new();
    static int _revision;
    static uint _player;
    static ushort _pbody;
    static int _px, _py, _pz;
    static byte _pdir;
    static uint _auth;
    static byte _seq;
    static bool _inWorld;
    static bool _gameStream;
    static bool _sawCharacters;
    static bool _postedWorld;
    static volatile bool _gameReady;
    static UOHuffman _huff = new();

    /// <summary>Connect or protocol failure.</summary>
    public static string LastError { get; private set; }

    /// <summary>True after the seed and the encrypted 0x80 have been written.</summary>
    public static bool Started => _started;
    static volatile bool _started;

    /// <summary>
    /// Opens the login server on a background thread and writes the seed and encrypted account packet.
    /// A null or empty host returns without throwing.
    /// </summary>
    public static void Begin(string host, int port, string account, string password, ClientVersion version) {
        if (string.IsNullOrEmpty(host)) return;
        if (version == 0) {
            _started = false;
            LastError = "client version is missing";
            Post(UOLoginPhase.Failed, message: LastError);
            return;
        }
        UOGamePackets.UseVersion(version);
        _started = false;
        LastError = null;
        var id = Interlocked.Increment(ref _runId);
        TcpClient previous;
        lock (Gate) {
            _select = null;
            _playIndex = null;
            _playName = null;
            _walks.Clear();
            _mobiles.Clear();
            _items.Clear();
            _player = 0;
            _pbody = 0;
            _px = _py = _pz = 0;
            _pdir = 0;
            _revision++;
            previous = _client;
            _client = null;
        }
        previous?.Close();
        Task.Run(() => Connect(host, port, account, password, version, id));
    }

    /// <summary>
    /// True once per new network result. Call from the UI thread. The network thread does not touch the scene.
    /// </summary>
    public static bool Take(out string[] servers, out string[] characters, out string message, out UOLoginPhase phase) {
        lock (Gate) {
            if (!_have) {
                servers = null;
                characters = null;
                message = null;
                phase = default;
                return false;
            }
            _have = false;
            servers = _takeServers;
            characters = _takeCharacters;
            message = _takeMessage;
            phase = _takePhase;
            _takeServers = null;
            _takeCharacters = null;
            _takeMessage = null;
            return true;
        }
    }

    /// <summary>
    /// Queues a 3-byte 0xA0 for the network thread.
    /// </summary>
    public static void SelectServer(int index) {
        lock (Gate) _select = index;
    }

    /// <summary>Queues 0x5D. The network thread sends it. Does not open a socket.</summary>
    public static void SelectCharacter(int index, string name) {
        lock (Gate) {
            _playIndex = index;
            _playName = name ?? "";
        }
    }

    /// <summary>Queues 0x02 once the game stream is up. Ignored before 0x5D.</summary>
    public static void Walk(byte direction) {
        if (!_gameReady) return;
        lock (Gate) {
            if (_walks.Count >= 4) return;
            _walks.Enqueue(direction);
        }
    }

    public static uint PlayerSerial { get { lock (Gate) return _player; } }
    public static ushort PlayerBody { get { lock (Gate) return _pbody; } }
    public static int PlayerX { get { lock (Gate) return _px; } }
    public static int PlayerY { get { lock (Gate) return _py; } }
    public static int PlayerZ { get { lock (Gate) return _pz; } }
    public static byte PlayerDirection { get { lock (Gate) return _pdir; } }
    public static int WorldRevision { get { lock (Gate) return _revision; } }

    public static void CopyActors(List<UOMobileState> mobiles, List<UOItemState> items) {
        lock (Gate) {
            mobiles.Clear();
            items.Clear();
            foreach (var m in _mobiles.Values) mobiles.Add(m);
            foreach (var it in _items.Values) items.Add(it);
        }
    }

    static void Connect(string host, int port, string account, string password, ClientVersion version, int id) {
        TcpClient client = null;
        _password = password;
        _inWorld = false;
        _gameReady = false;
        _gameStream = false;
        _sawCharacters = false;
        _postedWorld = false;
        _seq = 0;
        _huff = new UOHuffman();
        lock (Gate) _fastWalk.Clear();
        try {
            client = new TcpClient { NoDelay = true };
            client.Connect(host, port);
            if (id != Volatile.Read(ref _runId)) return;
            SetClient(client, id);
            var stream = client.GetStream();
            stream.ReadTimeout = ReadWaitMs;
            var seed = Seed(client);
            var seedUint = ReadU32(seed, 0);
            var crypt = new UOLoginCrypt(version);
            crypt.Initialize(seedUint);
            if (((uint)version >> 24) >= 6) {
                var ef = LoginSeed(seedUint, version);
                stream.Write(ef, 0, ef.Length);
            }
            else stream.Write(seed, 0, seed.Length);
            var login = AccountLogin(account, password);
            crypt.Encrypt(login);
            stream.Write(login, 0, login.Length);
            stream.Flush();
            _started = true;
            Pump(ref client, ref stream, crypt, account, id);
        }
        catch (Exception ex) {
            if (id != Volatile.Read(ref _runId)) return;
            Fail(ex.Message, id);
        }
        finally {
            if (id == Volatile.Read(ref _runId)) _password = null;
            try { client?.Close(); } catch { }
            lock (Gate) {
                if (_client == client) _client = null;
            }
        }
    }

    static void Pump(ref TcpClient client, ref NetworkStream stream, UOLoginCrypt crypt, string account, int id) {
        var acc = new byte[8192];
        var tmp = new byte[4096];
        var accLen = 0;
        while (id == Volatile.Read(ref _runId)) {
            WriteSelect(stream, crypt);
            WritePlay(stream, crypt, client);
            WriteWalk(stream, crypt);
            int n;
            try { n = stream.Read(tmp, 0, tmp.Length); }
            catch (Exception ex) when (TimedOut(ex)) { continue; }
            if (n <= 0) {
                if (!_sawCharacters && !_inWorld) Fail("disconnected", id);
                return;
            }
            if (_gameStream) {
                if (!IngestGame(tmp, n, crypt, ref acc, ref accLen, id)) return;
                continue;
            }
            if (accLen + n > acc.Length) {
                var bigger = new byte[Math.Max(acc.Length * 2, accLen + n)];
                Buffer.BlockCopy(acc, 0, bigger, 0, accLen);
                acc = bigger;
            }
            Buffer.BlockCopy(tmp, 0, acc, accLen, n);
            accLen += n;
            var used = 0;
            while (used < accLen) {
                var need = PacketLength(acc.AsSpan(used, accLen - used));
                if (need == 0) break;
                if (need < 0) {
                    Fail($"unknown packet 0x{acc[used]:X2}", id);
                    return;
                }
                var kind = HandlePacket(acc, used, need, account, id, ref client, ref stream, ref crypt);
                if (kind == HandleResult.Stop) return;
                if (kind == HandleResult.Relay) {
                    accLen = 0;
                    used = 0;
                    break;
                }
                if (acc[used] == 0xA9) _sawCharacters = true;
                used += need;
            }
            if (used > 0 && accLen > used) {
                Buffer.BlockCopy(acc, used, acc, 0, accLen - used);
                accLen -= used;
            }
            else if (used > 0) accLen = 0;
        }
    }

    enum HandleResult { Continue, Stop, Relay }

    static HandleResult HandlePacket(byte[] p, int o, int len, string account, int id, ref TcpClient client, ref NetworkStream stream, ref UOLoginCrypt crypt) {
        switch (p[o]) {
            case 0xA8:
                Post(UOLoginPhase.ServersReady, servers: ReadServerNames(p, o, len));
                return HandleResult.Continue;
            case 0x82:
                Fail(RejectReason(len > 1 ? p[o + 1] : (byte)0xFF), id);
                return HandleResult.Stop;
            case 0x8C:
                return Relay(p, o, account, id, ref client, ref stream, ref crypt) ? HandleResult.Relay : HandleResult.Stop;
            case 0xA9: {
                var names = ReadCharacterNames(p, o, len);
                if (names == null) {
                    Fail("bad character list", id);
                    return HandleResult.Stop;
                }
                Post(UOLoginPhase.CharactersReady, characters: names);
                return HandleResult.Continue;
            }
            default:
                return HandleResult.Continue;
        }
    }

    static bool Relay(byte[] p, int o, string account, int id, ref TcpClient client, ref NetworkStream stream, ref UOLoginCrypt crypt) {
        var ip = new IPAddress([p[o + 1], p[o + 2], p[o + 3], p[o + 4]]);
        var port = (p[o + 5] << 8) | p[o + 6];
        var auth = ReadU32(p, o + 7);
        _auth = auth;
        Post(UOLoginPhase.RelayInProgress);
        client.Close();
        var next = new TcpClient { NoDelay = true };
        next.Connect(ip, port);
        if (id != Volatile.Read(ref _runId)) {
            next.Close();
            return false;
        }
        SetClient(next, id);
        client = next;
        stream = next.GetStream();
        stream.ReadTimeout = ReadWaitMs;
        var seed = new[] { (byte)(auth >> 24), (byte)(auth >> 16), (byte)(auth >> 8), (byte)auth };
        stream.Write(seed, 0, seed.Length);
        crypt.BeginGame(auth);
        _huff.Reset();
        _gameStream = true;
        var login = GameLogin(auth, account, _password);
        crypt.EncryptGame(login);
        stream.Write(login, 0, login.Length);
        stream.Flush();
        return true;
    }

    static int PacketLength(Span<byte> s) => UOGamePackets.FrameLength(s);

    static string[] ReadServerNames(byte[] p, int o, int len) {
        if (len < 6) return [];
        var count = (p[o + 4] << 8) | p[o + 5];
        var names = new List<string>();
        var pos = o + 6;
        var end = o + len;
        for (var i = 0; i < count; i++) {
            if (pos + 40 > end) break;
            names.Add(ReadAscii(p, pos + 2, 32));
            pos += 40;
        }
        return names.ToArray();
    }

    static string[] ReadCharacterNames(byte[] p, int o, int len) {
        if (len < 4) return null;
        var count = p[o + 3];
        if (count > 16 || 4 + count * 60 > len) return null;
        var names = new string[count];
        for (var i = 0; i < count; i++)
            names[i] = ReadAscii(p, o + 4 + i * 60, AsciiField);
        return names;
    }

    static string RejectReason(byte reason) => reason switch {
        0 => "invalid",
        1 => "already in use",
        2 => "blocked",
        3 => "bad password",
        _ => "login rejected"
    };

    static void WriteSelect(NetworkStream stream, UOLoginCrypt crypt) {
        int index;
        lock (Gate) {
            if (_select is not int i) return;
            index = i;
            _select = null;
        }
        var pkt = new byte[] { 0xA0, (byte)(index >> 8), (byte)index };
        crypt.Encrypt(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
    }

    static void Post(UOLoginPhase phase, string[] servers = null, string[] characters = null, string message = null) {
        lock (Gate) {
            _have = true;
            _takePhase = phase;
            _takeServers = servers;
            _takeCharacters = characters;
            _takeMessage = message;
        }
    }

    static void Fail(string message, int id) {
        if (id != Volatile.Read(ref _runId)) return;
        LastError = message;
        Post(UOLoginPhase.Failed, message: message);
    }

    static void SetClient(TcpClient client, int id) {
        TcpClient previous;
        lock (Gate) {
            if (id != _runId) {
                client?.Close();
                return;
            }
            previous = _client;
            _client = client;
        }
        if (previous != null && previous != client) previous.Close();
    }

    static bool TimedOut(Exception ex) {
        for (; ex != null; ex = ex.InnerException)
            if (ex is SocketException se && se.SocketErrorCode == SocketError.TimedOut)
                return true;
        return false;
    }

    /// <summary>Four address bytes of the local endpoint, or 0x7F000001 when that address is not IPv4.</summary>
    static byte[] Seed(TcpClient client) {
        uint seed = SeedWhenNotIpv4;
        if (client.Client?.LocalEndPoint is IPEndPoint ep && ep.Address.AddressFamily == AddressFamily.InterNetwork) {
            var b = ep.Address.GetAddressBytes();
            if (b.Length == 4)
                seed = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }
        return [(byte)(seed >> 24), (byte)(seed >> 16), (byte)(seed >> 8), (byte)seed];
    }

    static bool WritePlay(NetworkStream stream, UOLoginCrypt crypt, TcpClient client) {
        int index;
        string name;
        lock (Gate) {
            if (_playIndex is not int i) return false;
            index = i;
            name = _playName ?? "";
            _playIndex = null;
            _playName = null;
        }
        var pkt = UOGamePackets.PlayCharacter(name, index, ClientIp(client));
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
        _inWorld = true;
        _gameReady = true;
        return true;
    }

    static uint ClientIp(TcpClient client) {
        if (client?.Client?.LocalEndPoint is IPEndPoint ep && ep.Address.AddressFamily == AddressFamily.InterNetwork) {
            var b = ep.Address.GetAddressBytes();
            if (b.Length == 4)
                return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }
        return SeedWhenNotIpv4;
    }

    static void WriteWalk(NetworkStream stream, UOLoginCrypt crypt) {
        if (!_inWorld) return;
        byte dir;
        uint key;
        lock (Gate) {
            if (_walks.Count == 0 || _fastWalk.Count == 0) return;
            dir = _walks.Dequeue();
            key = _fastWalk.Dequeue();
        }
        var pkt = new byte[7];
        pkt[0] = 0x02;
        pkt[1] = dir;
        pkt[2] = _seq++;
        WriteU32(pkt, 3, key);
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
        Predict(dir);
    }

    static void Predict(byte dir) {
        int dx = 0, dy = 0;
        switch (dir & 7) {
            case 0: dy = -1; break;
            case 1: dx = 1; dy = -1; break;
            case 2: dx = 1; break;
            case 3: dx = 1; dy = 1; break;
            case 4: dy = 1; break;
            case 5: dx = -1; dy = 1; break;
            case 6: dx = -1; break;
            default: dx = -1; dy = -1; break;
        }
        lock (Gate) {
            _px += dx;
            _py += dy;
            _pdir = (byte)(dir & 7);
            if (_player != 0 && _mobiles.TryGetValue(_player, out var m))
                _mobiles[_player] = new UOMobileState(_player, m.Body, _px, _py, _pz, _pdir);
            _revision++;
        }
    }

    static bool IngestGame(byte[] src, int n, UOLoginCrypt crypt, ref byte[] acc, ref int accLen, int id) {
        var raw = src.AsSpan(0, n);
        crypt.DecryptGame(raw);
        var decoded = new byte[n * 8 + 64];
        int produced;
        try { produced = _huff.Decompress(raw, decoded); }
        catch (Exception ex) {
            Fail(ex.Message, id);
            return false;
        }
        if (produced <= 0) return true;
        if (accLen + produced > acc.Length) {
            var bigger = new byte[Math.Max(acc.Length * 2, accLen + produced)];
            Buffer.BlockCopy(acc, 0, bigger, 0, accLen);
            acc = bigger;
        }
        Buffer.BlockCopy(decoded, 0, acc, accLen, produced);
        accLen += produced;
        var used = 0;
        while (used < accLen) {
            var need = PacketLength(acc.AsSpan(used, accLen - used));
            if (need == 0) break;
            if (need < 0) {
                Fail($"unknown packet 0x{acc[used]:X2}", id);
                return false;
            }
            if (HandleGame(acc, used, need, id) == HandleResult.Stop) return false;
            used += need;
        }
        if (used > 0 && accLen > used) {
            Buffer.BlockCopy(acc, used, acc, 0, accLen - used);
            accLen -= used;
        }
        else if (used > 0) accLen = 0;
        return true;
    }

    static HandleResult HandleGame(byte[] p, int o, int len, int id) {
        switch (p[o]) {
            case 0x1B:
                if (UOGamePackets.LoginConfirm(p, o, out var serial, out var body, out var x, out var y, out var z, out var dir))
                    SetPlayer(serial, body, x, y, z, dir, post: true);
                return HandleResult.Continue;
            case 0x55:
                PostWorld();
                return HandleResult.Continue;
            case 0x20:
                if (UOGamePackets.UpdatePlayer(p, o, len, out serial, out body, out x, out y, out z, out dir))
                    SetPlayer(serial, body, x, y, z, dir, post: true);
                return HandleResult.Continue;
            case 0x21:
                if (UOGamePackets.MoveReject(p, o, out x, out y, out dir, out z)) {
                    _seq = p[o + 1];
                    lock (Gate) _walks.Clear();
                    SetPlayer(_player, _pbody, x, y, z, dir, post: false);
                }
                return HandleResult.Continue;
            case 0x22:
                return HandleResult.Continue;
            case 0x77:
                if (UOGamePackets.Moving(p, o, len, out serial, out body, out x, out y, out z, out dir))
                    SetMobile(serial, body, x, y, z, dir);
                return HandleResult.Continue;
            case 0x78:
                if (UOGamePackets.Incoming(p, o, len, out serial, out body, out x, out y, out z, out dir))
                    SetMobile(serial, body, x, y, z, dir);
                return HandleResult.Continue;
            case 0x1A:
                if (UOGamePackets.WorldItem(p, o, len, out var iserial, out var graphic, out x, out y, out z))
                    SetItem(iserial, graphic, x, y, z);
                return HandleResult.Continue;
            case 0xBF:
                // Subcommand 0x0002 is the six fast-walk keys.
                UOGamePackets.FastWalk(p, o, len, _fastWalk);
                return HandleResult.Continue;
            case 0xA9: {
                var names = ReadCharacterNames(p, o, len);
                if (names == null) {
                    Fail("bad character list", id);
                    return HandleResult.Stop;
                }
                _sawCharacters = true;
                Post(UOLoginPhase.CharactersReady, characters: names);
                return HandleResult.Continue;
            }
            case 0x82:
                Fail(RejectReason(len > 1 ? p[o + 1] : (byte)0xFF), id);
                return HandleResult.Stop;
            default:
                return HandleResult.Continue;
        }
    }

    static void SetPlayer(uint serial, ushort body, int x, int y, int z, byte dir, bool post) {
        lock (Gate) {
            _player = serial;
            _pbody = body;
            _px = x;
            _py = y;
            _pz = z;
            _pdir = (byte)(dir & 7);
            _mobiles[serial] = new UOMobileState(serial, body, x, y, z, _pdir);
            _revision++;
        }
        if (post) PostWorld();
    }

    static void SetMobile(uint serial, ushort body, int x, int y, int z, byte dir) {
        if (serial == _player) {
            SetPlayer(serial, body, x, y, z, dir, post: false);
            return;
        }
        lock (Gate) {
            _mobiles[serial] = new UOMobileState(serial, body, x, y, z, (byte)(dir & 7));
            _revision++;
        }
    }

    static void SetItem(uint serial, ushort graphic, int x, int y, int z) {
        lock (Gate) {
            _items[serial] = new UOItemState(serial, graphic, x, y, z);
            _revision++;
        }
    }

    static void PostWorld() {
        if (_postedWorld) return;
        _postedWorld = true;
        Post(UOLoginPhase.WorldEntered);
    }

    static byte[] LoginSeed(uint seed, ClientVersion version) {
        var packet = new byte[21];
        var v = (uint)version;
        packet[0] = 0xEF;
        WriteU32(packet, 1, seed);
        WriteU32(packet, 5, v >> 24);
        WriteU32(packet, 9, (v >> 16) & 0xFF);
        WriteU32(packet, 13, (v >> 8) & 0xFF);
        WriteU32(packet, 17, v & 0xFF);
        return packet;
    }

    /// <summary>Fixed 0x80 packet: id, 30-byte account, 30-byte password, 0xFF. Packet length 0x3E, no length word.</summary>
    static byte[] AccountLogin(string account, string password) {
        var packet = new byte[1 + AsciiField + AsciiField + 1];
        packet[0] = 0x80;
        WriteAscii(packet, 1, account);
        WriteAscii(packet, 1 + AsciiField, password);
        packet[packet.Length - 1] = 0xFF;
        return packet;
    }

    static byte[] GameLogin(uint auth, string account, string password) {
        var packet = new byte[1 + 4 + AsciiField + AsciiField];
        packet[0] = 0x91;
        WriteU32(packet, 1, auth);
        WriteAscii(packet, 5, account);
        WriteAscii(packet, 5 + AsciiField, password);
        return packet;
    }

    static void WriteAscii(byte[] dest, int offset, string value) {
        if (string.IsNullOrEmpty(value)) return;
        var bytes = Encoding.ASCII.GetBytes(value);
        var n = bytes.Length < AsciiField ? bytes.Length : AsciiField;
        Buffer.BlockCopy(bytes, 0, dest, offset, n);
    }

    static string ReadAscii(byte[] src, int offset, int max) {
        var n = 0;
        while (n < max && src[offset + n] != 0) n++;
        return n == 0 ? "" : Encoding.ASCII.GetString(src, offset, n);
    }

    static uint ReadU32(byte[] src, int offset)
        => ((uint)src[offset] << 24) | ((uint)src[offset + 1] << 16) | ((uint)src[offset + 2] << 8) | src[offset + 3];

    static void WriteU32(byte[] dest, int offset, uint value) {
        dest[offset] = (byte)(value >> 24);
        dest[offset + 1] = (byte)(value >> 16);
        dest[offset + 2] = (byte)(value >> 8);
        dest[offset + 3] = (byte)value;
    }
}
