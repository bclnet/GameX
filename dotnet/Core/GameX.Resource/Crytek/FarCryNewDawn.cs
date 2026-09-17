using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace GameX.Crytek;

public static class FarCryNewDawn {
    static readonly IDictionary<string, ZipArchiveEntry> Files;
    static FarCryNewDawn() {
        var assembly = typeof(FarCryNewDawn).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Crytek.FarCryNewDawn.zip");
        var arc = new ZipArchive(s, ZipArchiveMode.Read);
        Files = arc.Entries.ToDictionary(s => s.FullName);
    }

    static readonly ConcurrentDictionary<string, Dictionary<ulong, string>> FileHashes = new();
    public static Dictionary<ulong, string> GetFileHashes(string path) => FileHashes.GetOrAdd(path, s => Files.TryGetValue(s, out var z) ? FarCryX.HashFilelist64(z) : []);
}