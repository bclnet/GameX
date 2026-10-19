// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO's first login socket. Old 4-byte seed, then the clear 0x80 account packet.
/// No world read, character list, encryption provider, Huffman, or plugin.
/// </summary>
public static class UOLoginNet {
    const int AsciiField = 30;
    const uint SeedWhenNotIpv4 = 0x7F000001;

    static TcpClient _client;

    /// <summary>Connect failure, or the clear-login note once the socket is open.</summary>
    public static string LastError { get; private set; }

    /// <summary>True after the seed and the account packet have been written.</summary>
    public static bool Started { get; private set; }

    /// <summary>
    /// Opens the login server on a background thread and writes the seed and account packet.
    /// A null or empty host returns without throwing.
    /// </summary>
    public static void Begin(string host, int port, string account, string password) {
        if (string.IsNullOrEmpty(host)) return;
        Started = false;
        Task.Run(() => Connect(host, port, account, password));
    }

    static void Connect(string host, int port, string account, string password) {
        TcpClient client = null;
        try {
            client = new TcpClient { NoDelay = true };
            client.Connect(host, port);
            var stream = client.GetStream();
            var seed = Seed(client);
            stream.Write(seed, 0, seed.Length);
            var login = AccountLogin(account, password);
            stream.Write(login, 0, login.Length);
            stream.Flush();
            var previous = _client;
            _client = client;
            client = null;
            previous?.Close();
            Started = true;
            // Send_FirstLogin does not skip encryption. This slice has no provider, so the 0x80 stays clear.
            LastError = "login encryption is not in this slice";
        }
        catch (Exception ex) {
            Started = false;
            LastError = ex.Message;
            client?.Close();
        }
    }

    /// <summary>Four address bytes of the remote endpoint, or 0x7F000001 when that address is not IPv4.</summary>
    static byte[] Seed(TcpClient client) {
        uint seed = SeedWhenNotIpv4;
        if (client.Client?.RemoteEndPoint is IPEndPoint ep && ep.Address.AddressFamily == AddressFamily.InterNetwork) {
            var b = ep.Address.GetAddressBytes();
            if (b.Length == 4)
                seed = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }
        return new[] { (byte)(seed >> 24), (byte)(seed >> 16), (byte)(seed >> 8), (byte)seed };
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

    static void WriteAscii(byte[] dest, int offset, string value) {
        if (string.IsNullOrEmpty(value)) return;
        var bytes = Encoding.ASCII.GetBytes(value);
        var n = bytes.Length < AsciiField ? bytes.Length : AsciiField;
        Buffer.BlockCopy(bytes, 0, dest, offset, n);
    }
}
