using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenX;

public static class Util {
    public static string DecodePath(string applicationPath, string path, string rootPath = null) =>
        path.StartsWith("~", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{path[1..]}"
        : path.StartsWith("%Path%", StringComparison.OrdinalIgnoreCase) ? $"{rootPath}{path[6..]}"
        : path.StartsWith("%AppPath%", StringComparison.OrdinalIgnoreCase) ? $"{applicationPath}{path[9..]}"
        : path.StartsWith("%AppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{path[9..]}"
        : path.StartsWith("%LocalAppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{path[14..]}"
        : path;

    /// <summary>
    /// C# <c>Uri.UnescapeDataString</c>. <c>+</c> stays <c>+</c>. A bad % sequence stays escaped.
    /// <c>Uri.LocalPath</c> is already decoded; call this on a still-encoded path.
    /// </summary>
    public static string DecodeUri(string path) {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (path.IndexOf('%') < 0) return path;
        var out_ = new System.Text.StringBuilder(path.Length);
        var i = 0; var n = path.Length;
        while (i < n) {
            if (path[i] == '%' && i + 2 < n) {
                var b = DecodeHex(path[i + 1], path[i + 2]);
                if (b < 0) { out_.Append('%'); i += 1; continue; }
                if (b < 128) { out_.Append((char)b); i += 3; continue; }
                var need = (b & 0xE0) == 0xC0 ? 2 : (b & 0xF0) == 0xE0 ? 3 : (b & 0xF8) == 0xF0 ? 4 : 0;
                if (need == 0) { out_.Append(path, i, 3); i += 3; continue; }
                var raw = new byte[need]; raw[0] = (byte)b; var j = i + 3; var got = 1; var ok = true;
                while (got < need) {
                    if (j + 2 >= n || path[j] != '%') { ok = false; break; }
                    var nb = DecodeHex(path[j + 1], path[j + 2]);
                    if (nb < 128) { ok = false; break; }
                    raw[got++] = (byte)nb; j += 3;
                }
                if (ok) {
                    try { out_.Append(Utf8Throw.GetString(raw)); i = j; continue; }
                    catch (ArgumentException) { }
                }
                out_.Append(path, i, 3); i += 3; continue;
            }
            out_.Append(path[i]); i += 1;
        }
        return out_.ToString();
    }

    static readonly System.Text.Encoding Utf8Throw = new System.Text.UTF8Encoding(false, true);

    static int DecodeHex(char a, char b) {
        var h = Hex(a); var l = Hex(b);
        return h is int hi && l is int lo ? (hi << 4) | lo : -1;
    }

    static int? Hex(char c) {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        return null;
    }
}

// YamlDict
public class YamlDict : Dictionary<string, object> {
    static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
    static readonly ISerializer Serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
    public string Path;
    public bool Dirty;

    public YamlDict(string file) {
        Path = Util.DecodePath(null, file);
        if (!File.Exists(Path)) return;
        var items = (Dictionary<object, object>)Deserializer.Deserialize(File.ReadAllText(Path));
        foreach (var s in items) Add((string)s.Key, s.Value);
    }

    public void Flush() {
        if (!Dirty) return;
        File.WriteAllText(Path, Serializer.Serialize(this));
        Dirty = true;
    }
}