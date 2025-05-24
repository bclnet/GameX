using GameX.BlizzardProto;
using Google.Protobuf;
using Microsoft.Win32;
using OpenStack;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using static SQLitePCL.raw;

namespace GameX;

/// <summary>
/// Store
/// </summary>
public static class Store {
    public static string GetPathByKey(string key, string family, JsonElement elem) {
        //Console.WriteLine($"Abandon:{Store_Abandon.Paths}");
        //Console.WriteLine($"Archive:{Store_Archive.Paths}");
        //Console.WriteLine($"Blizzard:{Store_Blizzard.Paths}");
        //Console.WriteLine($"Direct:{Store_Direct.GetPathByKey("%AppPath%")}");
        //Console.WriteLine($"Epic:{Store_Epic.Paths}");
        //Console.WriteLine($"Gog:{Store_Gog.Paths}");
        //Console.WriteLine($"Local:{Store_Local.Paths}");
        //Console.WriteLine($"Steam:{Store_Steam.Paths}");
        //Console.WriteLine($"Ubisoft:{Store_Ubisoft.Paths}");
        //Console.WriteLine($"WinReg:{Store_WinReg.GetPathByKey("GOG.com/Games/1207658680", default)}");
        var p = key.Split(':', 2);
        string k = p[0], v = p.Length > 1 ? p[1] : default;
        return k switch {
            "Steam" => Store_Steam.Paths.TryGetValue(v, out var z) ? z : null,
            "Gog" => Store_Gog.Paths.TryGetValue(v, out var z) ? z : null,
            "Blizzard" => Store_Blizzard.Paths.TryGetValue(v, out var z) ? z : null,
            "Epic" => Store_Epic.Paths.TryGetValue(v, out var z) ? z : null,
            "Ubisoft" => Store_Ubisoft.Paths.TryGetValue(v, out var z) ? z : null,
            "Abandon" => Store_Abandon.Paths.TryGetValue($"{family}/{v}", out var z) ? z : null,
            "Archive" => Store_Archive.Paths.TryGetValue($"{family}/{v}", out var z) ? z : null,
            "WinReg" => Store_WinReg.GetPathByKey(v, elem),
            "Local" => Store_Local.Paths.TryGetValue(v, out var z) ? z : null,
            "Direct" => Store_Direct.GetPathByKey(v),
            "Droid" => null,
            "XBox" => null,
            "Sony" => null,
            "Unknown" => null,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key),
        };
    }
}

#region Store_Abandon

/// <summary>
/// Store_Abandon
/// </summary>
static class Store_Abandon {
    public static readonly Dictionary<string, string> Paths = [];
    static string GetPath() => @"E:\AbandonLibrary";
    static Store_Abandon() {
        var paths = Paths;
        var root = GetPath();
        if (root == null || !Directory.Exists(root)) return;
        // # query games
        foreach (var s in Directory.EnumerateDirectories(root))
            foreach (var t in Directory.EnumerateFiles(s))
                paths.Add($"{Path.GetFileName(s)}/{Path.GetFileName(t)}", t);
    }
}

#endregion

#region Store_Archive

/// <summary>
/// Store_Archive
/// </summary>
static class Store_Archive {
    public static readonly Dictionary<string, string> Paths = [];
    static string GetPath() => @"E:\ArchiveLibrary";
    static Store_Archive() {
        var paths = Paths;
        var root = GetPath();
        if (root == null || !Directory.Exists(root)) return;
        // # query games
        foreach (var s in Directory.EnumerateDirectories(root)) {
            foreach (var t in Directory.EnumerateFiles(s))
                paths.Add($"{Path.GetFileName(s)}/{Path.GetFileName(t)}", t);
            foreach (var t in Directory.EnumerateDirectories(s))
                if (!Path.GetFileName(t).StartsWith(".")) paths.Add($"{Path.GetFileName(s)}/{Path.GetFileName(t)}", t);
        }
    }
}

#endregion

#region Store_Blizzard

/// <summary>
/// Store_Blizzard
/// </summary>
static class Store_Blizzard {
    public static readonly Dictionary<string, string> Paths = [];
    static string GetPath() {
        IEnumerable<string> paths;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // windows paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            paths = [Path.Combine(home, "Battle.net", "Agent")];
        }
        else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            // linux paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] search = [".steam", ".steam/steam", ".steam/root", ".local/share/Steam"];
            paths = search.Select(path => Path.Join(home, path, "appcache"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            // mac paths
            var home = "/Users/Shared";
            string[] search = ["Battle.net/Agent"];
            paths = search.Select(path => Path.Join(home, path, "data"));
        }
        else throw new PlatformNotSupportedException();
        return paths.FirstOrDefault(Directory.Exists);
    }
    static Store_Blizzard() {
        string dbPath;
        var paths = Paths;
        var root = GetPath();
        if (root == null || !File.Exists(dbPath = Path.Combine(root, "product.db"))) return;

        // query games
        Database productDb;
        using var s = File.OpenRead(dbPath);
        try {
            productDb = Database.Parser.ParseFrom(s);
        }
        catch (InvalidProtocolBufferException) {
            productDb = new Database { ProductInstall = { ProductInstall.Parser.ParseFrom(s) } };
        }
        foreach (var app in productDb.ProductInstall) {
            // add appPath if exists
            var appPath = app.Settings.InstallPath;
            if (Directory.Exists(appPath)) paths.Add(app.Uid, appPath);
        }
    }
}

#endregion

#region Store_Direct

/// <summary>
/// Store_Direct
/// </summary>
static class Store_Direct {
    public static string GetPathByKey(string key) => key;
}

#endregion

#region Store_Epic

/// <summary>
/// Store_Epic
/// </summary>
static class Store_Epic {
    public static readonly Dictionary<string, string> Paths = [];
    static string GetPath() {
        IEnumerable<string> paths;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // windows paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string[] search = [@"Epic\EpicGamesLauncher"];
            paths = search.Select(path => Path.Join(home, path, "Sbi"));
        }
        else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            // linux paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] search = ["Epic/EpicGamesLauncher"];
            paths = search.Select(path => Path.Join(home, path, "Sbi"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            // mac paths
            var home = "/Users/Shared";
            string[] search = ["Epic/EpicGamesLauncher"];
            paths = search.Select(path => Path.Join(home, path, "Sbi"));
        }
        else throw new PlatformNotSupportedException();
        return paths.FirstOrDefault(Directory.Exists);
    }
    static Store_Epic() {
        string dbPath;
        var path = Paths;
        var root = GetPath();
        if (root == null || !File.Exists(dbPath = Path.Combine(root, "Manifests"))) return;
        // # query games
        foreach (var s in Directory.EnumerateFiles(dbPath).Where(s => s.EndsWith(".item"))) {
            // add appPath if exists
            var appPath = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(s)).GetProperty("InstallLocation").GetString();
            if (Directory.Exists(appPath)) path.Add(Path.GetFileNameWithoutExtension(s), appPath);
        }
    }
}

#endregion

#region Store_Gog

/// <summary>
/// Store_Gog
/// </summary>
static class Store_Gog {
    public static readonly Dictionary<string, string> Paths = [];
    static string GetPath() {
        IEnumerable<string> paths;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // windows paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string[] search = [@"GOG.com\Galaxy"];
            paths = search.Select(path => Path.Join(home, path, "storage"));
        }
        else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            // linux paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] search = ["??"];
            paths = search.Select(path => Path.Join(home, path, "Storage"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            // mac paths
            var home = "/Users/Shared";
            string[] search = ["GOG.com/Galaxy"];
            paths = search.Select(path => Path.Join(home, path, "Storage"));

        }
        else throw new PlatformNotSupportedException();
        return paths.FirstOrDefault(Directory.Exists);
    }
    static Store_Gog() {
        SetProvider(new SQLite3Provider_e_sqlite3());
        string dbPath;
        var paths = Paths;
        var root = GetPath();
        if (root == null || !File.Exists(dbPath = Path.Combine(root, "galaxy-2.0.db"))) return;
        // query games
        if (sqlite3_open(dbPath, out var conn) != SQLITE_OK ||
            sqlite3_prepare_v2(conn, "SELECT productId, installationPath FROM InstalledBaseProducts", out var stmt) != SQLITE_OK) return;
        var read = true;
        while (read)
            switch (sqlite3_step(stmt)) {
                case SQLITE_ROW:
                    // add appPath if exists
                    var appId = sqlite3_column_int(stmt, 0).ToString();
                    var appPath = sqlite3_column_text(stmt, 1).utf8_to_string();
                    if (Directory.Exists(appPath)) paths.Add(appId, appPath);
                    break;
                case SQLITE_DONE: read = false; break;
            }
        sqlite3_finalize(stmt);
    }
}

#endregion

#region Store_Local

/// <summary>
/// Store_Local
/// </summary>
static class Store_Local {
    public static readonly Dictionary<string, string> Paths;
    const string GAMESPATH = "Games";
    static Store_Local() {
        // get locale games
        var gameRoots = DriveInfo.GetDrives().Select(x => Path.Combine(x.Name, GAMESPATH)).ToList();
        if (PlatformX.PlatformOS == PlatformX.OS.Android) gameRoots.Add(Path.Combine("/sdcard", GAMESPATH));
        Paths = gameRoots.Where(Directory.Exists).SelectMany(Directory.GetDirectories).ToDictionary(Path.GetFileName, x => x);
    }
}

#endregion

#region Store_Steam

/// <summary>
/// Store_Steam
/// </summary>
static class Store_Steam {
    public class AcfStruct {
        public Dictionary<string, AcfStruct> Get = [];
        public Dictionary<string, string> Value = [];
        public static AcfStruct Read(string path) => File.Exists(path) ? new AcfStruct(File.ReadAllText(path)) : null;
        public AcfStruct(string region) {
            int lengthOfRegion = region.Length, index = 0;
            while (lengthOfRegion > index) {
                var firstStart = region.IndexOf('"', index);
                if (firstStart == -1) break;
                var firstEnd = region.IndexOf('"', firstStart + 1);
                index = firstEnd + 1;
                var first = region.Substring(firstStart + 1, firstEnd - firstStart - 1);
                int secondStart = region.IndexOf('"', index), secondOpen = region.IndexOf('{', index);
                if (secondStart == -1)
                    Get.Add(first, null);
                else if (secondOpen == -1 || secondStart < secondOpen) {
                    var secondEnd = region.IndexOf('"', secondStart + 1);
                    index = secondEnd + 1;
                    var second = region.Substring(secondStart + 1, secondEnd - secondStart - 1);
                    Value.Add(first, second.Replace(@"\\", @"\"));
                }
                else {
                    var secondClose = NextEndOf(region, '{', '}', secondOpen + 1);
                    var acfs = new AcfStruct(region.Substring(secondOpen + 1, secondClose - secondOpen - 1));
                    index = secondClose + 1;
                    Get.Add(first, acfs);
                }
            }
        }
        static int NextEndOf(string str, char open, char close, int startIndex) {
            if (open == close) throw new Exception("\"Open\" and \"Close\" char are equivalent!");
            int openItem = 0, closeItem = 0;
            for (var i = startIndex; i < str.Length; i++) {
                if (str[i] == open) openItem++;
                if (str[i] == close) {
                    closeItem++;
                    if (closeItem > openItem) return i;
                }
            }
            throw new Exception("Not enough closing characters!");
        }
        public override string ToString() => ToString(0);
        public string ToString(int depth) {
            var b = new StringBuilder();
            foreach (var item in Value) {
                b.Append('\t', depth);
                b.AppendFormat("\"{0}\"\t\t\"{1}\"\r\n", item.Key, item.Value);
            }
            foreach (var item in Get) {
                b.Append('\t', depth);
                b.AppendFormat("\"{0}\"\n", item.Key);
                b.Append('\t', depth);
                b.AppendLine("{");
                b.Append(item.Value.ToString(depth + 1));
                b.Append('\t', depth);
                b.AppendLine("}");
            }
            return b.ToString();
        }
    }
    public static readonly Dictionary<string, string> Paths = [];
    static string GetPath() {
        IEnumerable<string> paths;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // windows paths
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam")
                ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (key == null) return null;
            return (string)key.GetValue("SteamPath");
        }
        else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            // linux paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] search = [".steam", ".steam/steam", ".steam/root", ".local/share/Steam"];
            paths = search.Select(path => Path.Join(home, path, "appcache"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            // mac paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] search = ["Library/Application Support/Steam"];
            paths = search.Select(path => Path.Join(home, path, "appcache"));
        }
        else throw new PlatformNotSupportedException();
        return paths.FirstOrDefault(Directory.Exists);
    }
    static Store_Steam() {
        var paths = Paths;
        var root = GetPath();
        if (root == null) return;

        // query games
        var libraryFolders = AcfStruct.Read(Path.Join(root, "steamapps", "libraryfolders.vdf"));
        foreach (var folder in libraryFolders.Get["libraryfolders"].Get.Values) {
            var path = folder.Value["path"];
            if (!Directory.Exists(path)) continue;
            foreach (var appId in folder.Get["apps"].Value.Keys) {
                var appManifest = AcfStruct.Read(Path.Join(path, "steamapps", $"appmanifest_{appId}.acf"));
                if (appManifest == null) continue;
                // add appPath if exists
                var appPath = Path.Join(path, "steamapps", Path.Join("common", appManifest.Get["AppState"].Value["installdir"]));
                if (Directory.Exists(appPath)) paths.Add(appId, appPath);
            }
        }
    }
}

#endregion

#region Store_Ubisoft

/// <summary>
/// Store_Ubisoft
/// </summary>
static class Store_Ubisoft {
    public static readonly Dictionary<string, string> Paths = [];
    static string GetPath() {
        IEnumerable<string> paths;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // windows paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] search = ["Ubisoft Game Launcher"];
            paths = search.Select(path => Path.Join(home, path));
        }
        else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            // linux paths
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] search = ["??"];
            paths = search.Select(path => Path.Join(home, path));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            // mac paths
            var home = "/Users/Shared";
            string[] search = ["??"];
            paths = search.Select(path => Path.Join(home, path));

        }
        else throw new PlatformNotSupportedException();
        return paths.FirstOrDefault(Directory.Exists);
    }
    static Store_Ubisoft() {
        string dbPath;
        var paths = Paths;
        var root = GetPath();
        if (root == null) return;
        if (root == null || !File.Exists(dbPath = Path.Combine(root, "settings.yaml"))) return;
        // query games
        var body = File.ReadAllText(dbPath);
        var gamePath = body[(body.IndexOf("game_installation_path:") + 23)..body.IndexOf("installer_cache_path")].Trim();
        foreach (var s in Directory.EnumerateDirectories(gamePath))
            paths.Add(Path.GetFileName(s), s);
    }
}

#endregion

#region Store_WinReg

/// <summary>
/// Store_WinReg
/// </summary>
static class Store_WinReg {
    public static string GetPathByKey(string key, JsonElement elem)
        => PlatformX.PlatformOS == PlatformX.OS.Windows ? default
        : GetPathByRegistryKey(key, elem.TryGetProperty(key, out var y) ? y : null);

    /// <summary>
    /// Gets the executable path.
    /// </summary>
    /// <param name="name">Name of the sub.</param>
    /// <returns></returns>
    static string FindRegistryPath(string[] paths) {
        var localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        var currentUser64 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        foreach (var p in paths)
            try {
                var keyPath = p.Replace('/', '\\');
                var key = new Func<RegistryKey>[] {
                    () => localMachine64.OpenSubKey($"SOFTWARE\\{keyPath}"),
                    () => currentUser64.OpenSubKey($"SOFTWARE\\{keyPath}"),
                    () => Registry.ClassesRoot.OpenSubKey($"VirtualStore\\MACHINE\\SOFTWARE\\{keyPath}") }
                    .Select(x => x()).FirstOrDefault(x => x != null);
                if (key == null) continue;
                // search directories
                var path = new[] { "Path", "Install Dir", "InstallDir", "InstallLocation" }
                    .Select(x => key.GetValue(x) as string)
                    .FirstOrDefault(x => !string.IsNullOrEmpty(x) && Directory.Exists(x));
                if (path == null) {
                    // search files
                    path = new[] { "Installed Path", "ExePath", "Exe" }
                        .Select(x => key.GetValue(x) as string)
                        .FirstOrDefault(x => !string.IsNullOrEmpty(x) && File.Exists(x));
                    if (path != null) path = Path.GetDirectoryName(path);
                }
                if (path != null && Directory.Exists(path)) return path;
            }
            catch { return null; }
        return null;
    }

    static string GetPathByRegistryKey(string key, JsonElement? elem) {
        var path = FindRegistryPath([$@"Wow6432Node\{key}", key]);
        return elem == null ? path
        : elem.Value.TryGetProperty("path", out var z) ? Path.GetFullPath(PlatformX.DecodePath(z.GetString(), path))
        : elem.Value.TryGetProperty("xml", out z) && elem.Value.TryGetProperty("xmlPath", out var y) ? GetSingleFileValue(PlatformX.DecodePath(z.GetString(), path), "xml", y.GetString())
        : null;
    }

    static string GetSingleFileValue(string path, string ext, string select) {
        if (!File.Exists(path)) return null;
        var content = File.ReadAllText(path);
        var value = ext switch {
            "xml" => XDocument.Parse(content).XPathSelectElement(select)?.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(ext)),
        };
        return value != null ? Path.GetDirectoryName(value) : null;
    }
}

#endregion
