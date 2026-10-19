// SPDX-License-Identifier: BSD-2-Clause

using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// Reads the install's Login.cfg. First LoginServer=host,port that parses.
/// </summary>
public static class UOLoginServers {
    public static bool TryFirst(string clientPath, out string host, out int port) {
        host = null;
        port = 0;
        if (string.IsNullOrEmpty(clientPath)) return false;
        var path = Path.Combine(clientPath, "Login.cfg");
        if (!File.Exists(path)) return false;
        string[] lines;
        try { lines = File.ReadAllLines(path); }
        catch { return false; }
        foreach (var raw in lines) {
            var line = raw?.Trim();
            if (string.IsNullOrEmpty(line) || line[0] == ';') continue;
            const string prefix = "LoginServer=";
            if (!line.StartsWith(prefix)) continue;
            var value = line.Substring(prefix.Length);
            var comma = value.IndexOf(',');
            if (comma <= 0) continue;
            var h = value.Substring(0, comma).Trim();
            if (h.Length == 0) continue;
            if (!int.TryParse(value.Substring(comma + 1).Trim(), out var p)) continue;
            host = h;
            port = p;
            return true;
        }
        return false;
    }
}
