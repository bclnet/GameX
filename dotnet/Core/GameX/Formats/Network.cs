using GameX.Eng;
using OpenStack;
using System;
using System.IO;
using System.Text;

namespace GameX.Formats.Network;

// PacketLogger
public class PacketLogger {
    public static PacketLogger Default { get; set; } = new PacketLogger();

    LogFile _logFile;

    public bool Enabled { get; set; }

    public LogFile CreateFile() {
        _logFile?.Dispose();
        return _logFile = new LogFile(Path.Join("Logs", "Network"), "packets.log");
    }

    public void Log(Span<byte> message, bool toServer) {
        if (!Enabled) return;
        Span<char> span = stackalloc char[256];
        var b = new ValueStringBuilder(span);
        {
            var off = sizeof(ulong) + 2;
            b.Append(' ', off);
            b.Append($"Ticks: 0 | {(toServer ? "Client -> Server" : "Server -> Client")} |  ID: {message[0]:X2}   Length: {message.Length}\n");
            if (message[0] == 0x80 || message[0] == 0x91) {
                b.Append(' ', off);
                b.Append("[ACCOUNT CREDENTIALS HIDDEN]\n");
            }
            else {
                b.Append(' ', off);
                b.Append("0  1  2  3  4  5  6  7   8  9  A  B  C  Radius  E  F\n");
                b.Append(' ', off);
                b.Append("-- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --\n");
                var address = 0UL;
                for (var i = 0; i < message.Length; i += 16, address += 16) {
                    b.Append($"{address:X8}");
                    for (var j = 0; j < 16; ++j) {
                        if ((j % 8) == 0) b.Append(" ");
                        b.Append(i + j < message.Length ? $" {message[i + j]:X2}" : "   ");
                    }
                    b.Append("  ");
                    for (var j = 0; j < 16 && i + j < message.Length; ++j) {
                        var c = message[i + j];
                        b.Append(c >= 0x20 && c < 0x80 ? (char)c : '.');
                    }
                    b.Append('\n');
                }
            }
            b.Append('\n');
            b.Append('\n');
            var s = b.ToString();
            if (_logFile != null) _logFile.Write(s);
            else Console.WriteLine(s);
            b.Dispose();
        }
    }
}