// SPDX-License-Identifier: BSD-2-Clause

using OpenX;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>Reads and writes FamilyGame.Options, the yaml on the UO game. Login keys stay put.</summary>
public static class UOSettings {
    static readonly HashSet<string> Locked = new HashSet<string> { "username", "password", "ip", "port", "encryption", "clientVersion", "version" };

    public static YamlDict File => UltimaOnline.Current?.Settings;

    public static string Get(string key, string fallback) {
        var file = File;
        if (file == null || key == null || !file.TryGetValue(key, out var v) || v == null) return fallback;
        var s = v as string ?? v.ToString();
        return string.IsNullOrEmpty(s) ? fallback : s;
    }

    public static int GetInt(string key, int fallback) {
        var file = File;
        if (file == null || key == null || !file.TryGetValue(key, out var v) || v == null) return fallback;
        if (v is int i) return i;
        if (v is long n) return (int)n;
        return int.TryParse(v.ToString(), out var p) ? p : fallback;
    }

    public static bool GetBool(string key, bool fallback) {
        var file = File;
        if (file == null || key == null || !file.TryGetValue(key, out var v) || v == null) return fallback;
        if (v is bool b) return b;
        var s = v.ToString();
        if (s == "1" || s == "true" || s == "True") return true;
        if (s == "0" || s == "false" || s == "False") return false;
        return fallback;
    }

    public static void Set(string key, object value) {
        var file = File;
        if (file == null || key == null || Locked.Contains(key)) return;
        file[key] = value;
        file.Dirty = true;
        file.Flush();
    }

    public static List<string> GetList(string key) {
        var list = new List<string>();
        var file = File;
        if (file == null || key == null || !file.TryGetValue(key, out var v) || v == null) return list;
        if (v is IEnumerable<object> items) {
            foreach (var item in items) if (item != null) list.Add(item.ToString());
            return list;
        }
        if (v is IEnumerable<string> lines) { list.AddRange(lines); return list; }
        return list;
    }
}
