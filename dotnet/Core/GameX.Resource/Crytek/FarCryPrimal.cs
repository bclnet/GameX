using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace GameX.Crytek;

public static class FarCryPrimal {
    static readonly IDictionary<string, ZipArchiveEntry> Files;
    static FarCryPrimal() {
        var assembly = typeof(FarCryPrimal).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Crytek.FarCryPrimal.zip");
        var arc = new ZipArchive(s, ZipArchiveMode.Read);
        Files = arc.Entries.ToDictionary(s => s.FullName);
    }

    static readonly ConcurrentDictionary<string, Dictionary<ulong, string>> FileHashes = new();
    public static Dictionary<ulong, string> GetFileHashes(string path) => FileHashes.GetOrAdd(path, s => Files.TryGetValue(s, out var z) ? FarCryX.HashFilelist64(z) : []);
}