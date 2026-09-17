using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace GameX.Crytek;

public static class FarCry2 {
    static readonly Dictionary<string, ZipArchiveEntry> Files;
    static FarCry2() {
        var assembly = typeof(FarCry2).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Crytek.FarCry2.zip");
        var arc = new ZipArchive(s, ZipArchiveMode.Read);
        Files = arc.Entries.ToDictionary(s => s.FullName);
    }

    static readonly ConcurrentDictionary<string, Dictionary<ulong, string>> FileHashes = new();
    static readonly ConcurrentDictionary<string, Definition> ObjDefs = new();
    public static Dictionary<ulong, string> GetFileHashes(string path) => FileHashes.GetOrAdd(path, s => Files.TryGetValue(s, out var z) ? FarCryX.HashFilelist32(z) : []);
    public static Definition GetObjDef(string path) => ObjDefs.GetOrAdd(path, s => Files.TryGetValue(s, out var z) ? FarCryX.HashObj32(z) : null);
}