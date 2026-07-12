using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace GameX.Crytek;

public static class FarCry2 {
    static readonly Dictionary<string, ZipArchiveEntry> HashFiles;
    static FarCry2() {
        var assembly = typeof(FarCry2).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Crytek.FarCry2.zip");
        var arc = new ZipArchive(s, ZipArchiveMode.Read);
        HashFiles = arc.Entries.ToDictionary(s => s.FullName);
    }

    static readonly ConcurrentDictionary<string, Dictionary<ulong, string>> Hashes = new();
    public static Dictionary<ulong, string> GetHashes(string path) => Hashes.GetOrAdd(path, s => HashFiles.TryGetValue(s, out var z) ? FarCryX.HashFilelist32(z) : []);
}