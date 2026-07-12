using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace GameX.Crytek;

public static class FarCryPrimal {
    static readonly IDictionary<string, ZipArchiveEntry> HashFiles;
    static FarCryPrimal() {
        var assembly = typeof(FarCryPrimal).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Crytek.FarCryPrimal.zip");
        var arc = new ZipArchive(s, ZipArchiveMode.Read);
        HashFiles = arc.Entries.ToDictionary(s => s.FullName);
    }

    static readonly ConcurrentDictionary<string, Dictionary<ulong, string>> Hashes = new();
    public static Dictionary<ulong, string> GetHashes(string path) => Hashes.GetOrAdd(path, s => HashFiles.TryGetValue(s, out var z) ? FarCryX.HashFilelist64(z) : []);
}