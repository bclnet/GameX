// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO settings.json and lastcharacter.json, stored under LocalAppData\GameX.
/// The password is ClassicUO's Crypter form (machine name). The file is not in the repo.
/// </summary>
public static class UOLoginSettings {
    static readonly List<LastChar> _last = [];
    static bool _loaded;

    public static string Username { get; set; } = "";
    public static string Password { get; set; } = "";
    public static bool AutoLogin { get; set; }
    public static bool SaveAccount { get; set; }
    public static string LastServerName { get; set; } = "";
    public static int LastServerNum { get; set; }

    static string Dir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameX");
    static string SettingsPath => Path.Combine(Dir, "uo-settings.json");
    static string LastPath => Path.Combine(Dir, "lastcharacter.json");

    public static void Load() {
        if (_loaded) return;
        _loaded = true;
        _last.Clear();
        if (!File.Exists(SettingsPath)) {
            AutoLogin = true;
            SaveAccount = true;
            Save();
        }
        else ReadSettings();
        ReadLast();
    }

    public static void Save() {
        try {
            Directory.CreateDirectory(Dir);
            var user = SaveAccount ? Username ?? "" : "";
            var pass = SaveAccount ? Encrypt(Password) : "";
            var json = new StringBuilder();
            json.Append("{\n");
            json.Append("  \"username\": \"").Append(Esc(user)).Append("\",\n");
            json.Append("  \"password\": \"").Append(Esc(pass)).Append("\",\n");
            json.Append("  \"autologin\": ").Append(AutoLogin ? "true" : "false").Append(",\n");
            json.Append("  \"saveaccount\": ").Append(SaveAccount ? "true" : "false").Append(",\n");
            json.Append("  \"last_server_name\": \"").Append(Esc(LastServerName)).Append("\",\n");
            json.Append("  \"lastservernum\": ").Append(LastServerNum).Append("\n");
            json.Append("}\n");
            File.WriteAllText(SettingsPath, json.ToString());
            WriteLast();
        }
        catch { }
    }

    public static void NoteServer(string name, int index) {
        LastServerName = name ?? "";
        LastServerNum = index;
        Save();
    }

    public static void NoteCharacter(string account, string server, string name) {
        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(name)) return;
        server ??= "";
        for (var i = 0; i < _last.Count; i++) {
            var row = _last[i];
            if (row.Account == account && row.Server == server) {
                _last[i] = new LastChar(account, server, name);
                Save();
                return;
            }
        }
        _last.Add(new LastChar(account, server, name));
        Save();
    }

    public static string LastCharacter(string account, string server) {
        if (string.IsNullOrEmpty(account)) return "";
        server ??= "";
        foreach (var row in _last)
            if (row.Account == account && row.Server == server) return row.Name ?? "";
        return "";
    }

    public static int ServerIndex(string[] servers) {
        if (servers == null || servers.Length == 0) return 0;
        if (!string.IsNullOrWhiteSpace(LastServerName))
            for (var i = 0; i < servers.Length; i++)
                if (string.Equals(servers[i], LastServerName, StringComparison.OrdinalIgnoreCase)) return i;
        var index = LastServerNum;
        if (index < 0 || index >= servers.Length) index = 0;
        return index;
    }

    public static int CharacterSlot(string[] characters, string account, string server) {
        if (characters == null) return -1;
        var last = LastCharacter(account, server);
        var first = -1;
        for (var i = 0; i < characters.Length; i++) {
            if (string.IsNullOrEmpty(characters[i])) continue;
            if (first < 0) first = i;
            if (last.Length > 0 && characters[i] == last) return i;
        }
        return first;
    }

    static void ReadSettings() {
        string json;
        try { json = File.ReadAllText(SettingsPath); }
        catch { return; }
        Username = Str(json, "username");
        Password = Decrypt(Str(json, "password"));
        AutoLogin = Bool(json, "autologin");
        SaveAccount = Bool(json, "saveaccount");
        LastServerName = Str(json, "last_server_name");
        LastServerNum = Num(json, "lastservernum");
    }

    static void ReadLast() {
        if (!File.Exists(LastPath)) return;
        string json;
        try { json = File.ReadAllText(LastPath); }
        catch { return; }
        var i = 0;
        while (i < json.Length) {
            var a = json.IndexOf("\"AccountName\"", i, StringComparison.Ordinal);
            if (a < 0) break;
            var account = StrAt(json, a);
            var s = json.IndexOf("\"ServerName\"", a, StringComparison.Ordinal);
            var n = json.IndexOf("\"LastCharacterName\"", a, StringComparison.Ordinal);
            if (s < 0 || n < 0) break;
            _last.Add(new LastChar(account, StrAt(json, s), StrAt(json, n)));
            i = n + 1;
        }
    }

    static void WriteLast() {
        var json = new StringBuilder();
        json.Append("[\n");
        for (var i = 0; i < _last.Count; i++) {
            var row = _last[i];
            if (i > 0) json.Append(",\n");
            json.Append("  {\"AccountName\": \"").Append(Esc(row.Account));
            json.Append("\", \"ServerName\": \"").Append(Esc(row.Server));
            json.Append("\", \"LastCharacterName\": \"").Append(Esc(row.Name)).Append("\"}");
        }
        json.Append("\n]\n");
        File.WriteAllText(LastPath, json.ToString());
    }

    static string Str(string json, string key) {
        var at = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
        return at < 0 ? "" : StrAt(json, at);
    }

    static string StrAt(string json, int keyAt) {
        var colon = json.IndexOf(':', keyAt);
        if (colon < 0) return "";
        var q = json.IndexOf('"', colon + 1);
        if (q < 0) return "";
        var sb = new StringBuilder();
        for (var i = q + 1; i < json.Length; i++) {
            var c = json[i];
            if (c == '\\' && i + 1 < json.Length) { sb.Append(json[++i]); continue; }
            if (c == '"') break;
            sb.Append(c);
        }
        return sb.ToString();
    }

    static bool Bool(string json, string key) {
        var at = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
        if (at < 0) return false;
        var colon = json.IndexOf(':', at);
        if (colon < 0) return false;
        var rest = json.Substring(colon + 1).TrimStart();
        return rest.StartsWith("true", StringComparison.Ordinal);
    }

    static int Num(string json, string key) {
        var at = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
        if (at < 0) return 0;
        var colon = json.IndexOf(':', at);
        if (colon < 0) return 0;
        var rest = json.Substring(colon + 1).TrimStart();
        var n = 0;
        var any = false;
        foreach (var c in rest) {
            if (c < '0' || c > '9') break;
            any = true;
            n = n * 10 + (c - '0');
        }
        return any ? n : 0;
    }

    static string Esc(string s) {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    // ClassicUO Crypter. The key is the machine name. A value that starts with "1-" is that form.
    static string Encrypt(string source) {
        if (string.IsNullOrEmpty(source)) return "";
        var key = Environment.MachineName;
        if (key.Length == 0) return "";
        var buff = Encoding.ASCII.GetBytes(source);
        var sb = new StringBuilder(source.Length * 2 + 2);
        sb.Append("1-");
        var kidx = 0;
        for (var i = 0; i < buff.Length; i++) {
            sb.AppendFormat("{0:X2}", (byte)(buff[i] ^ (byte)key[kidx++]));
            if (kidx >= key.Length) kidx = 0;
        }
        return sb.ToString();
    }

    static string Decrypt(string source) {
        if (string.IsNullOrEmpty(source)) return "";
        if (source.Length > 2 && source[0] == '1' && (source[1] == '-' || source[1] == '+')) {
            var key = Environment.MachineName;
            if (key.Length == 0) return "";
            var buff = new byte[(source.Length - 2) >> 1];
            var kidx = 0;
            for (var i = 2; i + 1 < source.Length; i += 2) {
                if (!byte.TryParse(source.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out var c)) continue;
                buff[(i - 2) >> 1] = (byte)(c ^ (byte)key[kidx++]);
                if (kidx >= key.Length) kidx = 0;
            }
            return Encoding.ASCII.GetString(buff);
        }
        var old = (byte)(source.Length >> 1);
        var raw = new byte[old];
        for (var i = 0; i + 1 < source.Length; i += 2) {
            if (!byte.TryParse(source.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out var c)) continue;
            raw[i >> 1] = (byte)(c ^ old++);
        }
        return Encoding.ASCII.GetString(raw);
    }

    readonly struct LastChar {
        public readonly string Account, Server, Name;
        public LastChar(string account, string server, string name) {
            Account = account ?? "";
            Server = server ?? "";
            Name = name ?? "";
        }
    }
}
