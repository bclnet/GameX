using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace GameX.Crytek;

public static class FarCry3 {
    static readonly Dictionary<string, ZipArchiveEntry> Files;
    static FarCry3() {
        var assembly = typeof(FarCry2).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Crytek.FarCry3.zip");
        var arc = new ZipArchive(s, ZipArchiveMode.Read);
        Files = arc.Entries.ToDictionary(s => s.FullName);
    }

    static readonly ConcurrentDictionary<string, Dictionary<ulong, string>> FileHashes = new();
    public static Dictionary<ulong, string> GetFileHashes(string path) => FileHashes.GetOrAdd(path, s => Files.TryGetValue(s, out var z) ? FarCryX.HashFilelist64(z) : []);
}