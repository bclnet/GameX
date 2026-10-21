// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameX.Origin.UO;

/// <summary>
/// Login-socket result for LoginScene. Network thread posts; UI thread takes.
/// </summary>
public enum UOLoginPhase {
    ServersReady,
    CharactersReady,
    Failed,
    RelayInProgress
}

/// <summary>
/// ClassicUO's first login socket. Clear 4-byte seed, optional encrypted 0xEF, encrypted 0x80, then login replies.
/// Login packets are not Huffman. No play-character packet.
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

    /// <summary>Connect or protocol failure.</summary>
    public static string LastError { get; private set; }

    /// <summary>True after the seed and the encrypted 0x80 have been written.</summary>
    public static bool Started => _started;
    static volatile bool _started;

    /// <summary>
    /// Opens the login server on a background thread and writes the seed and encrypted account packet.
    /// A null or empty host returns without throwing.
    /// </summary>
    public static void Begin(string host, int port, string account, string password) {
        if (string.IsNullOrEmpty(host)) return;
        var current = UltimaOnline.Current;
        if (current == null || current.Version == 0) {
            _started = false;
            LastError = "client version is missing";
            Post(UOLoginPhase.Failed, message: LastError);
            return;
        }
        _started = false;
        LastError = null;
        var id = Interlocked.Increment(ref _runId);
        TcpClient previous;
        lock (Gate) {
            _select = null;
            previous = _client;
            _client = null;
        }
        previous?.Close();
        var version = current.Version;
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

    static void Connect(string host, int port, string account, string password, ClientVersion version, int id) {
        TcpClient client = null;
        _password = password;
        try {
            client = new TcpClient { NoDelay = true };
            client.Connect(host, port);
            if (id != Volatile.Read(ref _runId)) return;
            SetClient(client, id);
            var stream = client.GetStream();
            stream.ReadTimeout = ReadWaitMs;
            var seed = Seed(client);
            var seedUint = ReadU32(seed, 0);
            stream.Write(seed, 0, seed.Length);
            var crypt = new UOLoginCrypt(version);
            crypt.Initialize(seedUint);
            if (((uint)version >> 24) >= 6) {
                var ef = LoginSeed(seedUint, version);
                crypt.Encrypt(ef);
                stream.Write(ef, 0, ef.Length);
            }
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
        var gotCharacters = false;
        while (id == Volatile.Read(ref _runId)) {
            WriteSelect(stream, crypt);
            int n;
            try { n = stream.Read(tmp, 0, tmp.Length); }
            catch (Exception ex) when (TimedOut(ex)) { continue; }
            if (n <= 0) {
                if (!gotCharacters) Fail("disconnected", id);
                return;
            }
            crypt.Encrypt(tmp.AsSpan(0, n));
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
                if (acc[used] == 0xA9) gotCharacters = true;
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
        crypt.Initialize(auth);
        var login = GameLogin(auth, account, _password);
        crypt.Encrypt(login);
        stream.Write(login, 0, login.Length);
        stream.Flush();
        return true;
    }

    static int PacketLength(Span<byte> s) {
        if (s.Length < 1) return 0;
        var id = s[0];
        if (id == 0x82) return s.Length >= 2 ? 2 : 0;
        if (id == 0x8C) return s.Length >= 11 ? 11 : 0;
        if (s.Length < 3) return 0;
        var size = (s[1] << 8) | s[2];
        if (size < 3) return -1;
        if (size > s.Length) return 0;
        return size;
    }

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

    /// <summary>Four address bytes of the remote endpoint, or 0x7F000001 when that address is not IPv4.</summary>
    static byte[] Seed(TcpClient client) {
        uint seed = SeedWhenNotIpv4;
        if (client.Client?.RemoteEndPoint is IPEndPoint ep && ep.Address.AddressFamily == AddressFamily.InterNetwork) {
            var b = ep.Address.GetAddressBytes();
            if (b.Length == 4)
                seed = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }
        return [(byte)(seed >> 24), (byte)(seed >> 16), (byte)(seed >> 8), (byte)seed];
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
