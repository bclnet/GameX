using OpenStack;
using OpenStack.Client;
using OpenStack.Vfx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static GameX.FamilyManager;
using static GameX.Util;

namespace GameX;

#region Factory

/// <summary>
/// FamilyManager
/// </summary>
public partial class FamilyManager {
    /// <summary>
    /// Search by.
    /// </summary>
    public enum SearchBy {
        Default,
        Arc,
        TopDir,
        TwoDir,
        DirDown,
        AllDir,
    }

    /// <summary>
    /// The system path.
    /// </summary>
    public class SystemPath {
        public string Root;
        public string Type;
        public string[] Paths;
    }

    /// <summary>
    /// Gets the host factory.
    /// </summary>
    /// <value>
    /// The host factory.
    /// </value>
    public virtual Func<Uri, string, NetworkHost> HostFactory { get; } = NetworkHost.Factory;

    /// <summary>
    /// Parse Key.
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static object ParseKey(JsonElement elem) {
        var str = elem.ToString();
        if (string.IsNullOrEmpty(str)) { return null; }
        else if (str.StartsWith("b64:", StringComparison.Ordinal)) return Convert.FromBase64String(str[4..]);
        else if (str.StartsWith("hex:", StringComparison.Ordinal)) return (str = str[4..]).StartsWith("/")
            ? Enumerable.Range(0, str.Length >> 2).Select(x => byte.Parse(str.Substring((x << 2) + 2, 2), NumberStyles.HexNumber)).ToArray()
            : Enumerable.Range(0, str.Length >> 1).Select(x => byte.Parse(str.Substring(x << 1, 2), NumberStyles.HexNumber)).ToArray();
        else if (str.StartsWith("asc:", StringComparison.Ordinal)) return Encoding.ASCII.GetBytes(str[4..]);
        else return str;
    }

    /// <summary>
    /// Parse Engine.
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static (string n, string v) ParseEngine(JsonElement elem) {
        var str = elem.ToString();
        if (string.IsNullOrEmpty(str)) { return default; }
        var p = str.Split([':'], 2);
        return (p[0], p.Length < 2 ? null : p[1]);
    }

    /// <summary>
    /// Create family sample.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="loader"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static FamilySample CreateFamilySample(string path, Func<string, string> loader) {
        var json = loader(path);
        if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        var elem = doc.RootElement;
        return new FamilySample(elem);
    }

    /// <summary>
    /// Create family.
    /// </summary>
    /// <param name="any"></param>
    /// <param name="loader"></param>
    /// <param name="loadSamples"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static Family CreateFamily(string any, Func<string, string> loader = null, bool loadSamples = false) {
        var json = loader != null ? loader(any) : any;
        if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        var elem = doc.RootElement;
        var familyType = _valueF(elem, "familyType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("familyType", $"Unknown type: {z}"));
        var family = familyType != null ? (Family)Activator.CreateInstance(familyType, elem) : new Family(elem);
        if (family.SpecSamples != null && loadSamples)
            foreach (var sample in family.SpecSamples)
                family.MergeSample(CreateFamilySample(sample, loader));
        if (family.Specs != null)
            foreach (var spec in family.Specs)
                family.Merge(CreateFamily(spec, loader, loadSamples));
        return family;
    }

    /// <summary>
    /// Create family engine.
    /// </summary>
    /// <param name="family"></param>
    /// <param name="id"></param>
    /// <param name="elem"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static FamilyEngine CreateFamilyEngine(Family family, string id, JsonElement elem) {
        var engineType = _valueF(elem, "engineType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("engineType", $"Unknown type: {z}"));
        return engineType != null ? (FamilyEngine)Activator.CreateInstance(engineType, family, id, elem) : new FamilyEngine(family, id, elem);
    }

    /// <summary>
    /// Create family game.
    /// </summary>
    /// <param name="family"></param>
    /// <param name="id"></param>
    /// <param name="elem"></param>
    /// <param name="dgame"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static FamilyGame CreateFamilyGame(Family family, string id, JsonElement elem, ref FamilyGame dgame) {
        var gameType = _valueF(elem, "gameType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("gameType", $"Unknown type: {z}"), dgame.GameType);
        var game = gameType != null ? (FamilyGame)Activator.CreateInstance(gameType, family, id, elem, dgame) : new FamilyGame(family, id, elem, dgame);
        game.GameType = gameType;
        if (id.StartsWith("*")) { dgame = game; return null; }
        return game;
    }

    /// <summary>
    /// Create family app.
    /// </summary>
    /// <param name="family"></param>
    /// <param name="id"></param>
    /// <param name="elem"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static FamilyApp CreateFamilyApp(Family family, string id, JsonElement elem) {
        var appType = _valueF(elem, "appType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("appType", $"Unknown type: {z}"));
        return appType != null ? (FamilyApp)Activator.CreateInstance(appType, family, id, elem) : new FamilyApp(family, id, elem);
    }

    /// <summary>
    /// Creates the file system.
    /// </summary>
    /// <param name="vfxType">The vfxType.</param>
    /// <param name="path">The path.</param>
    /// <param name="subPath">The subPath.</param>
    /// <param name="virtuals">The virtuals.</param>
    /// <param name="host">The host.</param>
    /// <returns></returns>
    internal static FileSystem CreateFileSystem(Type vfxType, SystemPath path, string subPath, Uri host = null) {
        var vfx = host != null ? new NetworkFileSystem(host)
            : vfxType != null ? (FileSystem)Activator.CreateInstance(vfxType, path)
            : null;
        if (vfx != null) return vfx.Next();
        var baseRoot = string.IsNullOrEmpty(subPath) ? path.Root : Path.Combine(path.Root, subPath);
        if (baseRoot.EndsWith("/") || baseRoot.EndsWith("\\")) baseRoot = baseRoot[..^1];
        var basePath = path?.Paths?.FirstOrDefault();
        vfx = new DirectoryFileSystem(baseRoot, basePath);
        return vfx.Next();
    }
}

#endregion

#region Detector

/// <summary>
/// Detector
/// </summary>
public class Detector {
    protected ConcurrentDictionary<string, object> Cache = new();
    protected Dictionary<string, Dictionary<string, object>> Hashs;

    /// <summary>
    /// Gets the identifier
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// Gets the Game.
    /// </summary>
    public FamilyGame Game { get; }
    /// <summary>
    /// Gets the Data
    /// </summary>
    public Dictionary<string, object> Data { get; }
    /// <summary>
    /// Gets the game name.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Detector
    /// </summary>
    /// <param name="id"></param>
    /// <param name="game"></param>
    /// <param name="elem"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Detector(FamilyGame game, string id, JsonElement elem) {
        Id = Name = id;
        Game = game;
        Data = elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
            "name" => Name = x.Value.GetString(),
            "type" => x.Value.GetString(),
            "key" => _valueF(elem, "key", ParseKey),
            "hashs" => Hashs = _related(elem, "hashs", k => k.GetProperty("hash").GetString(), v => ParseHash(game, v)),
            _ => _valueV(x.Value)
        });
    }

    public virtual Dictionary<string, object> ParseHash(FamilyGame game, JsonElement elem)
        => elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
            "edition" => game.Editions != null && game.Editions.TryGetValue(x.Value.GetString(), out var a) ? a : x.Value.GetString(),
            "locale" => game.Locales != null && game.Locales.TryGetValue(x.Value.GetString(), out var a) ? a : x.Value.GetString(),
            _ => _valueV(x.Value)
        });

    public unsafe virtual string GetHash(BinaryReader r) {
        var type = Data.TryGetValue("type", out var z) ? z : "md5";
        switch (type) {
            case "crc": {
                    // create table
                    var seed = 0xEDB88320U;
                    var table = stackalloc uint[256];
                    uint j, n;
                    for (var i = 0U; i < 256; i++) {
                        n = i;
                        for (j = 0; j < 8; j++) n = (n & 1) != 0 ? (n >> 1) ^ seed : n >> 1;
                        table[i] = n;
                    }
                    // generate crc
                    var crc = 0xFFFFFFFFU;
                    var len = r.BaseStream.Length;
                    for (var i = 0U; i < len; i++)
                        crc = (crc >> 8) ^ table[(crc ^ r.ReadByte()) & 0xFF];
                    crc ^= 0xFFFFFFFF;
                    return $"{crc:s}";
                }
            case "md5": {
                    using var md5 = System.Security.Cryptography.MD5.Create();
                    var data = r.ReadBytes(1024 * 1024);
                    var h = md5.ComputeHash(data, 0, data.Length);
                    return $"{h[0]:x2}{h[1]:x2}{h[2]:x2}{h[3]:x2}{h[4]:x2}{h[5]:x2}{h[6]:x2}{h[7]:x2}{h[8]:x2}{h[9]:x2}{h[10]:x2}{h[11]:x2}{h[12]:x2}{h[13]:x2}{h[14]:x2}{h[15]:x2}";
                }
            default: throw new ArgumentOutOfRangeException(nameof(Type), $"Unknown Type {type}");
        }
    }

    public T Get<T>(string key, object value, Func<Detector, T, T> func) where T : class => Cache.GetOrAdd(key, (k, v) => {
        var s = Detect<T>(k, v);
        return s == null || func == null ? s : func(this, s);
    }, value) as T;

    public virtual T Detect<T>(string key, object value) where T : class {
        if (Hashs == null) throw new NullReferenceException(nameof(Hashs));
        switch (value) {
            case null: throw new ArgumentNullException(nameof(value));
            case BinaryReader r: {
                    r.BaseStream.Position = 0;
                    var hash = GetHash(r);
                    r.BaseStream.Position = 0;
                    return hash != null && Hashs.TryGetValue(hash, out var z) ? z as T : default;
                }
            default: throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}

#endregion

#region Resource

/// <summary>
/// Resource
/// </summary>
public struct Resource {
    /// <summary>
    /// The filesystem.
    /// </summary>
    public FileSystem Vfx;
    /// <summary>
    /// The game.
    /// </summary>
    public FamilyGame Game;
    /// <summary>
    /// The game edition.
    /// </summary>
    public FamilyGame.Edition Edition;
    /// <summary>
    /// The search pattern.
    /// </summary>
    public string SearchPattern;
}

#endregion

#region Family

/// <summary>
/// Family
/// </summary>
public class Family {
    /// <summary>
    /// An empty family.
    /// </summary>
    public static readonly Family Empty = new() {
        Id = string.Empty,
        Name = "Empty",
        Games = []
    };

    /// <summary>
    /// Gets the file filters.
    /// </summary>
    /// <value>
    /// The file filters.
    /// </value>
    //public Dictionary<string, Dictionary<string, string>> Filters => Game.Filters;

    /// <summary>
    /// Gets or sets the family identifier.
    /// </summary>
    /// <value>
    /// The family identifier.
    /// </value>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the family name.
    /// </summary>
    /// <value>
    /// The family name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the family studio.
    /// </summary>
    /// <value>
    /// The family studio.
    /// </value>
    public string Studio { get; set; }

    /// <summary>
    /// Gets or sets the family description.
    /// </summary>
    /// <value>
    /// The family description.
    /// </value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the game urls.
    /// </summary>
    public Uri[] Urls { get; set; }

    /// <summary>
    /// Gets the families spec samples.
    /// </summary>
    /// <returns></returns>
    public string[] SpecSamples { get; set; }

    /// <summary>
    /// Gets the families specs.
    /// </summary>
    /// <returns></returns>
    public string[] Specs { get; set; }


    /// <summary>
    /// Gets the family apps.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, FamilyApp> Apps { get; set; }

    /// <summary>
    /// Gets the family engines.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, FamilyEngine> Engines { get; set; }

    /// <summary>
    /// Gets the family games.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, FamilyGame> Games { get; set; }

    /// <summary>
    /// Gets the family samples.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, List<FamilySample.File>> Samples { get; set; }

    /// <summary>
    /// Family
    /// </summary>
    internal Family() { }
    /// <summary>
    /// Family
    /// </summary>
    /// <param name="elem"></param>
    public Family(JsonElement elem) {
        try {
            Id = _value(elem, "id") ?? throw new ArgumentNullException("id");
            Name = _value(elem, "name");
            Studio = _value(elem, "studio");
            Description = _value(elem, "description");
            Urls = _list(elem, "url", x => new Uri(x));
            SpecSamples = _list(elem, "samples");
            Specs = _list(elem, "specs");
            // related
            var dgame = new FamilyGame { SearchBy = SearchBy.Default, Arcs = [new Uri("archive:/")] };
            Apps = _related(elem, "apps", (k, v) => CreateFamilyApp(this, k, v));
            Engines = _related(elem, "engines", (k, v) => CreateFamilyEngine(this, k, v));
            Games = _dictTrim(_related(elem, "games", (k, v) => CreateFamilyGame(this, k, v, ref dgame)));
            Samples = [];
        }
        catch (Exception e) {
            Log.Error($"{Id}: {e.Message}\n{e.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Touches this instance.
    /// </summary>
    public static void Touch() { }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString() => Name;

    /// <summary>
    /// Merges the family.
    /// </summary>
    /// <param name="source">The source.</param>
    public void Merge(Family source) {
        if (source == null) return;
        foreach (var s in source.Engines) Engines.Add(s.Key, s.Value);
        foreach (var s in source.Games) Games.Add(s.Key, s.Value);
        foreach (var s in source.Apps) Apps.Add(s.Key, s.Value);
    }

    /// <summary>
    /// Merges the family sample.
    /// </summary>
    /// <param name="source">The source.</param>
    public void MergeSample(FamilySample source) {
        if (source == null) return;
        foreach (var s in source.Samples) Samples.Add(s.Key, s.Value);
    }

    /// <summary>
    /// Gets the specified family game.
    /// </summary>
    /// <param name="id">The game id.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">game</exception>
    public FamilyGame GetGame(string id, out FamilyGame.Edition edition, bool throwOnError = true) {
        var ids = id.Split('.', 2);
        string gid = ids[0], eid = ids.Length > 1 ? ids[1] : string.Empty;
        var game = Games.TryGetValue(gid, out var z1) ? z1
            : (throwOnError ? throw new ArgumentOutOfRangeException(nameof(id), id) : default);
        edition = game.Editions.TryGetValue(eid, out var z2) ? z2 : default;
        return game;
    }

    /// <summary>
    /// Generates the specified family game string.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="edition">The edition.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">game</exception>
    public string ToGame(FamilyGame game, FamilyGame.Edition edition) => edition != null ? $"{game.Id}.{edition.Id}" : game.Id;

    /// <summary>
    /// Parses the family resource uri.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    public Resource ParseResource(Uri uri, bool throwOnError = true) {
        if (uri == null || string.IsNullOrEmpty(uri.Fragment)) return new Resource { Game = new FamilyGame() };
        var game = GetGame(uri.Fragment[1..], out var edition);
        var searchPattern = uri.IsFile ? null : uri.LocalPath[1..];
        var virtuals = game.Virtuals;
        var found = game.Found;
        var subPath = edition?.Path;
        var vfxType = game.VfxType;
        var vfx =
            string.Equals(uri.Scheme, "archive", StringComparison.OrdinalIgnoreCase) ? found != null ? CreateFileSystem(vfxType, found, subPath) : default
            : uri.IsFile ? !string.IsNullOrEmpty(uri.LocalPath) ? CreateFileSystem(vfxType, new SystemPath { Root = uri.LocalPath }, subPath) : default
            : uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? !string.IsNullOrEmpty(uri.Host) ? CreateFileSystem(vfxType, found, subPath, uri) : default
            : default;
        if (vfx == null)
            if (throwOnError) throw new ArgumentOutOfRangeException(nameof(uri), $"{game.Id}: unable to find archive");
            else return default;
        if (virtuals != null) vfx = new AggregateFileSystem([vfx, new VirtualFileSystem(virtuals)]);
        return new Resource {
            Vfx = vfx,
            Game = game,
            Edition = edition,
            SearchPattern = searchPattern
        };
    }

    /// <summary>
    /// Generates the family resource uri.
    /// </summary>
    /// <param name="res">The res.</param>
    /// <returns></returns>
    public Uri ToResource(Resource res) {
        return default;
    }

    /// <summary>
    /// Opens the family archive.
    /// </summary>
    /// <param name="res">The res.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    public Archive GetArchive(object res, bool throwOnError = true) {
        var r = res switch {
            Resource s => s,
            string s => ParseResource(new Uri(s)),
            Uri u => ParseResource(u),
            _ => throw new ArgumentOutOfRangeException(nameof(res)),
        };
        if (!r.Game.HasLoaded) { r.Game.HasLoaded = true; r.Game.Loaded(); }
        return r.Game != null
            ? r.Game.GetArchive(r.Vfx, r.Edition, r.SearchPattern, throwOnError)?.Open()
            : throw new ArgumentNullException(nameof(r.Game));
    }
}

#endregion

#region FamilyApp

/// <summary>
/// FamilyApp
/// </summary>
public class FamilyApp {
    /// <summary>
    /// Gets the family.
    /// </summary>
    public Family Family { get; }
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// Gets the data.
    /// </summary>
    public Dictionary<string, object> Data { get; set; }
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; protected set; }
    /// <summary>
    /// Gets the explorer type.
    /// </summary>
    public Type ExplorerType { get; protected set; }
    /// <summary>
    /// Gets the explorer2 type.
    /// </summary>
    public Type Explorer2Type { get; protected set; }

    /// <summary>
    /// FamilyApp
    /// </summary>
    /// <param name="family"></param>
    /// <param name="id"></param>
    /// <param name="elem"></param>
    public FamilyApp(Family family, string id, JsonElement elem) {
        Family = family;
        Id = Name = id;
        Data = elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
            "name" => Name = x.Value.GetString(),
            "explorerAppType" => ExplorerType = Type.GetType(x.Value.GetString(), false),
            "explorerApp2Type" => Explorer2Type = Type.GetType(x.Value.GetString(), false),
            "key" => _valueF(elem, "key", ParseKey),
            _ => _valueV(x.Value)
        });
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString() => Name;

    /// <summary>
    /// Gets or sets the game name.
    /// </summary>
    public virtual Task OpenAsync(Type explorerType, MetaManager manager) {
        var explorer = Activator.CreateInstance(explorerType);
        var startupMethod = explorerType.GetMethod("Application_Startup", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new ArgumentOutOfRangeException(nameof(explorerType), "No Application_Startup found");
        startupMethod.Invoke(explorer, [this, null]);
        return Task.CompletedTask;
    }
}

#endregion

#region FamilyEngine

/// <summary>
/// FamilyEngine
/// </summary>
public class FamilyEngine {
    /// <summary>
    /// Gets the family.
    /// </summary>
    public Family Family { get; }
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// Gets the data.
    /// </summary>
    public Dictionary<string, object> Data { get; }
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// FamilyEngine
    /// </summary>
    /// <param name="family"></param>
    /// <param name="id"></param>
    /// <param name="elem"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FamilyEngine(Family family, string id, JsonElement elem) {
        Family = family;
        Id = Name = id;
        Data = elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
            "name" => Name = x.Value.GetString(),
            "key" => _valueF(elem, "key", ParseKey),
            _ => _valueV(x.Value)
        });
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString() => Name;
}

#endregion

#region FamilySample

/// <summary>
/// FamilySample
/// </summary>
public class FamilySample {
    public Dictionary<string, List<File>> Samples { get; } = [];

    /// <summary>
    /// FamilySample
    /// </summary>
    /// <param name="elem"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FamilySample(JsonElement elem) {
        foreach (var s in elem.EnumerateObject())
            Samples.Add(s.Name, [.. s.Value.GetProperty("files").EnumerateArray().Select(x => new File(x))]);
    }

    /// <summary>
    /// The sample file.
    /// </summary>
    public class File {
        /// <summary>
        /// The path
        /// </summary>
        public string[] Paths { get; protected set; }
        /// <summary>
        /// The Data
        /// </summary>
        public Dictionary<string, object> Data { get; }

        /// <summary>
        /// File
        /// </summary>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public File(JsonElement elem)
            => Data = elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
                "path" => Paths = x.Value.ValueKind == JsonValueKind.String ? [x.Value.GetString()] : [.. x.Value.EnumerateArray().Select(s => s.GetString())],
                "size" => x.Value.GetInt64(),
                _ => _valueV(x.Value)
            });

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        //public override string ToString() => Path;
    }
}

#endregion

#region FamilyGame

/// <summary>
/// ClientState
/// </summary>
/// <param name="archive">The archive.</param>
/// <param name="args">The pargsath.</param>
/// <param name="tag">The tag.</param>
public class ClientState(Archive archive, string[] args = null, object tag = null) {
    /// <summary>
    /// Gets the arc family game.
    /// </summary>
    public readonly Archive Archive = archive;

    /// <summary>
    /// Gets the args.
    /// </summary>
    public readonly string[] Args = args ?? [];

    /// <summary>
    /// Gets the tag.
    /// </summary>
    public object Tag = tag;
}

/// <summary>
/// FamilyGame
/// </summary>
public class FamilyGame {
    /// <summary>
    /// An empty family game.
    /// </summary>
    public static readonly FamilyGame Empty = new() {
        Family = Family.Empty,
        Id = "Empty",
        Name = "Empty",
    };

    /// <summary>
    /// The game edition.
    /// </summary>
    public class Edition {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the data.
        /// </summary>
        public Dictionary<string, object> Data { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Gets the paths.
        /// </summary>
        public string Path { get; protected set; }
        /// <summary>
        /// Gets the ignores.
        /// </summary>
        public string[] Ignores { get; protected set; }

        /// <summary>
        /// Edition
        /// </summary>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Edition(string id, JsonElement elem) {
            Id = Name = id;
            Data = elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
                "name" => Name = x.Value.GetString(),
                "path" => Path = x.Value.GetString(),
                "ignore" => Ignores = _list(elem, "ignore"),
                "key" => _valueF(elem, "key", ParseKey),
                _ => _valueV(x.Value)
            });
        }
    }

    /// <summary>
    /// The game DLC.
    /// </summary>
    public class DownloadableContent {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the data.
        /// </summary>
        public Dictionary<string, object> Data { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Gets the path.
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// DownloadableContent
        /// </summary>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DownloadableContent(string id, JsonElement elem) {
            Id = Name = id;
            Data = elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
                "name" => Name = x.Value.GetString(),
                "path" => Path = x.Value.GetString(),
                "key" => _valueF(elem, "key", ParseKey),
                _ => _valueV(x.Value)
            });
        }
    }

    /// <summary>
    /// The game locale.
    /// </summary>
    public class Locale {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the data.
        /// </summary>
        public Dictionary<string, object> Data { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Locale
        /// </summary>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Locale(string id, JsonElement elem) {
            Id = Name = id;
            Data = elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch {
                "name" => Name = x.Value.GetString(),
                "key" => _valueF(elem, "key", ParseKey),
                _ => _valueV(x.Value)
            });
        }
    }

    /// <summary>
    /// The game files.
    /// </summary>
    public class FileSet(JsonElement elem) {
        public string[] Keys = _list(elem, "key");
        public string[] Paths = _list(elem, "path", []);
    }

    /// <summary>
    /// Has loaded
    /// </summary>
    public bool HasLoaded;
    /// <summary>
    /// Gets or sets the game type.
    /// </summary>
    public Type GameType { get; set; }
    /// <summary>
    /// Gets or sets the family.
    /// </summary>
    public Family Family { get; set; }
    /// <summary>
    /// Gets or sets the game identifier.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Gets or sets the game name.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the game engine.
    /// </summary>
    public (string n, string v) Engine { get; set; }
    /// <summary>
    /// Gets or sets the game resource.
    /// </summary>
    public string Resource { get; set; }
    /// <summary>
    /// Gets or sets the game urls.
    /// </summary>
    public Uri[] Urls { get; set; }
    /// <summary>
    /// Gets or sets the game date.
    /// </summary>
    public DateTime Date { get; set; }
    /// <summary>
    /// Gets or sets the search by.
    /// </summary>
    public SearchBy SearchBy { get; set; }
    /// <summary>
    /// Gets or sets the archive type.
    /// </summary>
    public Type ArchiveType { get; set; }
    /// <summary>
    /// Gets or sets the arc exts.
    /// </summary>
    public string[] ArcExts { get; set; }
    /// <summary>
    /// Gets or sets the paks.
    /// </summary>
    public Uri[] Arcs { get; set; }
    /// <summary>
    /// Gets or sets the Paths.
    /// </summary>
    public string[] Paths { get; set; }
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    public object Key { get; set; }
    /// <summary>
    /// Gets or sets the Status.
    /// </summary>
    public string Status { get; set; }
    /// <summary>
    /// Gets or sets the Tags.
    /// </summary>
    public string[] Tags { get; set; }
    /// <summary>
    /// Gets or sets the type of the file system.
    /// </summary>
    public Type VfxType { get; set; }
    /// <summary>
    /// Gets or sets the type of the client.
    /// </summary>
    public Type ClientType { get; set; }
    /// <summary>
    /// Gets or sets the game editions.
    /// </summary>
    public Dictionary<string, Edition> Editions { get; set; }
    /// <summary>
    /// Gets or sets the game dlcs.
    /// </summary>
    public Dictionary<string, DownloadableContent> Dlcs { get; set; }
    /// <summary>
    /// Gets or sets the game locales.
    /// </summary>
    public Dictionary<string, Locale> Locales { get; set; }
    /// <summary>
    /// Gets or sets the detectorss.
    /// </summary>
    public Dictionary<string, Detector> Detectors { get; set; }
    /// <summary>
    /// Gets the displayed game name.
    /// </summary>
    /// <value>
    /// The name of the displayed.
    /// </value>
    public string DisplayedName => $"{Name}{(Found != null ? " - found" : null)}";

    /// <summary>
    /// Determines if the game has been found.
    /// </summary>
    public SystemPath Found;
    /// <summary>
    /// Gets or sets the files.
    /// </summary>
    public FileSet Files;
    /// <summary>
    /// Gets or sets the ignores.
    /// </summary>
    public HashSet<string> Ignores = [];
    /// <summary>
    /// Gets or sets the virtuals.
    /// </summary>
    public Dictionary<string, byte[]> Virtuals = [];
    /// <summary>
    /// Gets or sets the filters.
    /// </summary>
    public Dictionary<string, string> Filters = [];
    // Options
    public YamlDict Options;

    /// <summary>
    /// Create Detector.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="id"></param>
    /// <param name="elem"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal Detector CreateDetector(string id, JsonElement elem) {
        var detectorType = _valueF(elem, "detectorType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("detectorType", $"Unknown type: {z}"));
        return detectorType != null ? (Detector)Activator.CreateInstance(detectorType, this, id, elem) : new Detector(this, id, elem);
    }

    /// <summary>
    /// Create archive
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="state">The state.</param>
    /// <returns></returns>
    internal Archive CreateArchive(BinaryState state) => (Archive)Activator.CreateInstance(ArchiveType ?? throw new InvalidOperationException($"{Id} missing ArchiveType"), state);

    /// <summary>
    /// Create client
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="state">The state.</param>
    /// <returns></returns>
    internal ClientBase CreateClient(ClientState state) => (ClientBase)Activator.CreateInstance(ClientType ?? throw new InvalidOperationException($"{Id} missing ArchiveType"), state);

    /// <summary>
    /// FamilyGame
    /// </summary>
    internal FamilyGame() { }
    /// <summary>
    /// FamilyGame
    /// </summary>
    /// <param name="family"></param>
    /// <param name="id"></param>
    /// <param name="elem"></param>
    /// <param name="dgame"></param>
    public FamilyGame(Family family, string id, JsonElement elem, FamilyGame dgame) {
        try {
            HasLoaded = false;
            Family = family;
            Id = id;
            Name = _value(elem, "name"); //System.Diagnostics.Debugger.Log(0, null, $"Game: {Name}\n");
            Engine = _valueF(elem, "engine", ParseEngine, dgame.Engine);
            Resource = _value(elem, "resource", dgame.Resource);
            Urls = _list(elem, "url", x => new Uri(x));
            Date = _valueF(elem, "date", z => DateTime.Parse(z.GetString()));
            //Option = _value(elem, "option", z => Enum.TryParse<GameOption>(z.GetString(), true, out var zT) ? zT : throw new ArgumentOutOfRangeException("option", $"Unknown option: {z}"), dgame.Option);
            Arcs = _list(elem, "arc", x => new Uri(x), dgame.Arcs);
            Paths = _list(elem, "path", dgame.Paths);
            Key = _valueF(elem, "key", ParseKey, dgame.Key);
            Status = _value(elem, "status");
            Tags = _value(elem, "tags", string.Empty).Split(' ');
            // interface
            ClientType = _valueF(elem, "clientType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("clientType", $"Unknown type: {z}"), dgame.ClientType);
            VfxType = _valueF(elem, "vfxType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("vfxType", $"Unknown type: {z}"), dgame.VfxType);
            SearchBy = _valueF(elem, "searchBy", z => Enum.TryParse<SearchBy>(z.GetString(), true, out var zS) ? zS : throw new ArgumentOutOfRangeException("searchBy", $"Unknown option: {z}"), dgame.SearchBy);
            ArchiveType = _valueF(elem, "archiveType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("archiveType", $"Unknown type: {z}"), dgame.ArchiveType);
            ArcExts = _list(elem, "arcExt", dgame.ArcExts);
            // related
            Editions = _related(elem, "editions", (k, v) => new Edition(k, v));
            Dlcs = _related(elem, "dlcs", (k, v) => new DownloadableContent(k, v));
            Locales = _related(elem, "locals", (k, v) => new Locale(k, v));
            Detectors = _related(elem, "detectors", (k, v) => CreateDetector(k, v));
            // files
            Files = _valueF(elem, "files", x => new FileSet(x));
            Ignores = [.. _list(elem, "ignores", [])];
            Virtuals = _related(elem, "virtuals", (k, v) => (byte[])ParseKey(v));
            Filters = _related(elem, "filters", (k, v) => _valueV(v).ToString());
            // find
            Found = GetSystemPath(Option.FindKey, family.Id, elem);
            Options = null;
        }
        catch (Exception e) {
            Log.Error($"{Family.Id}.{Id}: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString() => Name;

    /// <summary>
    /// Detect
    /// </summary>
    public T Detect<T>(string id, string key, object value, Func<Detector, T, T> func = null) where T : class => Detectors.TryGetValue(id, out var z) ? z.Get(key, value, func) : default;

    /// <summary>
    /// Ensures this instance.
    /// </summary>
    public virtual void Loaded() { Options = new YamlDict($"~/.gamex.{Family.Id}_{Id}.yaml"); }

    /// <summary>
    /// Converts the game to uris.
    /// <param name="edition"></param>
    /// </summary>
    public IList<Uri> ToUris(string edition) => [.. Arcs.Select(s => ToUri(Id, edition, s))];

    /// <summary>
    /// Converts the game to a uri
    /// <param name="id"></param>
    /// <param name="edition"></param>
    /// <param name="prefix"></param>
    /// </summary>
    public static Uri ToUri(string id, string edition = null, Uri prefix = null) => new($"{prefix ?? new("archive:/")}#{id}{(string.IsNullOrEmpty(edition) ? "" : $".{edition}")}");

    /// <summary>
    /// Converts the game to an id
    /// <param name="id"></param>
    /// <param name="edition"></param>
    /// </summary>
    public static string ToId(string id, string edition = null) => $"{id}{(string.IsNullOrEmpty(edition) ? "" : $".{edition}")}";

    /// <summary>
    /// Gets a client
    /// </summary>
    /// <param name="state"></param>
    public ClientBase GetClient(ClientState state) => CreateClient(state);

    /// <summary>
    /// Gets a game sample
    /// </summary>
    public FamilySample.File GetSample(string id) {
        if (!Family.Samples.TryGetValue(Id, out var samples) || samples.Count == 0) return default;
        var idx = id == "*" ? new Random((int)DateTime.Now.Ticks).Next(samples.Count) : int.Parse(id);
        return samples.Count > idx ? samples[idx] : default;
    }

    /// <summary>
    /// Gets a game system path
    /// </summary>
    SystemPath GetSystemPath(string startsWith, string family, JsonElement elem) {
        if (Files == null || Files.Keys == null) return default;
        foreach (var key in startsWith != null ? Files.Keys.Where(startsWith.StartsWith) : Files.Keys) {
            var p = key.Split('#', 2);
            string k = p[0], v = p.Length > 1 ? p[1] : default;
            var path = Store.GetPathByKey(k, family, elem);
            if (string.IsNullOrEmpty(path)) continue;
            path = Path.GetFullPath(PlatformX.DecodePath(path));
            if (!Directory.Exists(path) && !File.Exists(path)) continue;
            return new SystemPath { Root = path, Type = v, Paths = Files.Paths };
        }
        return default;
    }

    #region Arc

    /// <summary>
    /// Adds the platform.
    /// </summary>
    /// <param name="archive">The arc file.</param>
    /// <returns></returns>
    //public static Archive SetPlatform(Archive archive, Platform platform)
    //{
    //    if (archive == null) { return null; }
    //    archive.Gfx = platform.GfxFactory?.Invoke(archive);
    //    archive.Sfx = platform.SfxFactory?.Invoke(archive);
    //    return archive;
    //}

    /// <summary>
    /// Creates the search patterns.
    /// </summary>
    /// <param name="searchPattern">The search pattern.</param>
    /// <returns></returns>
    string CreateSearchPatterns(string searchPattern) {
        if (!string.IsNullOrEmpty(searchPattern)) return searchPattern;
        return SearchBy switch {
            SearchBy.Default => "",
            SearchBy.Arc => ArcExts == null || ArcExts.Length == 0 ? ""
                : ArcExts.Length == 1 ? $"*{ArcExts[0]}" : $"(*{string.Join(":*", ArcExts)})",
            SearchBy.TopDir => "*",
            SearchBy.TwoDir => "*/*",
            SearchBy.DirDown => "**/*",
            SearchBy.AllDir => "**/*",
            _ => throw new ArgumentOutOfRangeException(nameof(SearchBy), $"{SearchBy}"),
        };
    }

    /// <summary>
    /// Is arc file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    internal bool IsArcPath(string path) => ArcExts != null && ArcExts.Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Create the archive.
    /// </summary>
    /// <param name="vfx">The vfx.</param>
    /// <param name="edition">The edition.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    internal Archive GetArchive(FileSystem vfx, Edition edition, string searchPattern, bool throwOnError) {
        if (vfx is NetworkFileSystem k) throw new NotImplementedException($"{k}"); //return new StreamArchive(family.FileManager.HostFactory, game, path, vfx),
        searchPattern = CreateSearchPatterns(searchPattern);
        var archives = new List<Archive>();
        var dlcKeys = Dlcs.Where(x => !string.IsNullOrEmpty(x.Value.Path)).Select(x => x.Key).ToArray();
        var slash = '\\';
        foreach (var key in (new string[] { null }).Concat(dlcKeys))
            foreach (var p in FindPaths(vfx, edition, key != null ? Dlcs[key] : null, searchPattern))
                switch (SearchBy) {
                    case SearchBy.Arc:
                        foreach (var path in p.paths)
                            if (IsArcPath(path)) archives.Add(GetArchiveObj(vfx, edition, path));
                        break;
                    default:
                        archives.Add(GetArchiveObj(vfx, edition,
                            SearchBy == SearchBy.DirDown ? (p.root, p.paths.Where(x => x.Contains(slash)).ToArray())
                            : p));
                        break;
                }
        return (archives.Count == 1 ? archives[0] : GetArchiveObj(vfx, edition, archives))?.SetPlatform(PlatformX.Current);
    }

    /// <summary>
    /// Create the archive.
    /// </summary>
    /// <param name="vfx">The vfx.</param>
    /// <param name="edition">The edition.</param>
    /// <param name="value">The value.</param>
    /// <param name="tag">The tag.</param>
    /// <returns></returns>
    Archive GetArchiveObj(FileSystem vfx, Edition edition, object value, object tag = null) {
        var state = new BinaryState(vfx, this, edition, value as string, tag);
        return value switch {
            string s => IsArcPath(s) ? CreateArchive(state) : throw new InvalidOperationException($"{Id} missing {s}"),
            ValueTuple<string, string[]> s => s.Item2.Length == 1 && IsArcPath(s.Item2[0])
                ? GetArchiveObj(vfx, edition, s.Item2[0], tag)
                : new ManyArchive(
                    CreateArchive(state), state,
                    s.Item1.Length > 0 ? s.Item1 : "Many", s.Item2,
                    pathSkip: s.Item1.Length > 0 ? s.Item1.Length + 1 : 0),
            IList<Archive> s => s.Count == 1 ? s[0] : new MultiArchive(state, "Multi", s),
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(value), $"{value}"),
        };
    }

    /// <summary>
    /// Find the games paths.
    /// </summary>
    /// <param name="vfx">The vfx.</param>
    /// <param name="edition">The edition.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <returns></returns>
    IEnumerable<(string root, string[] paths)> FindPaths(FileSystem vfx, Edition edition, DownloadableContent dlc, string searchPattern) {
        var ignores = Ignores;
        foreach (var path in Paths ?? [""]) {
            var searchPath = dlc != null && dlc.Path != null ? Path.Join(path, dlc.Path) : path;
            var fileSearch = vfx.FindPaths(searchPath, searchPattern);
            if (ignores != null) fileSearch = fileSearch.Where(s => !ignores.Contains(Path.GetFileName(s)));
            yield return (path, fileSearch.ToArray());
        }
    }

    #endregion
}

#endregion

#region Loader

public partial class FamilyManager {
    static readonly Func<string, Stream> GetManifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream;
    static string FamilyJsonLoader(string path) { using var r = new StreamReader(GetManifestResourceStream($"GameX.Specs.{path}") ?? throw new Exception($"Unable to spec: GameX.Specs.{path}")); return r.ReadToEnd(); }

    /// <summary>
    /// Load samples.
    /// </summary>
    public static bool LoadSamples = true;
    /// <summary>
    /// The families.
    /// </summary>
    public static readonly Dictionary<string, Family> Families = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// The Unknown family.
    /// </summary>
    public static readonly Family Uncore;
    /// <summary>
    /// The Unknown arc file.
    /// </summary>
    public static readonly Archive UncoreArchive;

    static FamilyManager() {
        Family.Touch();
        Uncore = GetFamily("Uncore");
        UncoreArchive = Uncore.GetArchive(new Uri("archive:/#APP"), throwOnError: false);
    }

    /// <summary>
    /// Gets the specified family.
    /// </summary>
    /// <param name="id">Name of the family.</param>
    /// <param name="throwOnError">Throw on error.</param>
    /// <returns>Family</returns>
    /// <exception cref="ArgumentOutOfRangeException">id</exception>
    public static Family GetFamily(string id, bool throwOnError = true) {
        if (Families.TryGetValue(id, out var family)) return family;
        try {
            family = CreateFamily($"{id}Family.json", FamilyJsonLoader, LoadSamples);
            Families.Add(family.Id, family);
            return family;
        }
        catch (Exception e) {
            Log.Error(e.ToString());
            Console.WriteLine(e.ToString());
        }
        return throwOnError ? throw new ArgumentOutOfRangeException(nameof(id), id) : default;
    }

    /// <summary>
    /// Loads all families
    /// </summary>
    /// <param name="throwOnError">Throw on error.</param>
    public static void LoadFamilies(string[] ids = null, bool throwOnError = true) {
        foreach (var s in ids ?? FamilyIds) GetFamily(s, throwOnError);
    }
}

#endregion
