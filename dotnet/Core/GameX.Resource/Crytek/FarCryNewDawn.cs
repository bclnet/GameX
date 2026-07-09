using GameX.Algorithms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GameX.Crytek;

public static class FarCryNewDawn {
    static readonly IDictionary<string, ZipArchiveEntry> HashEntries;
    static FarCryNewDawn() {
        var assembly = typeof(FarCry2).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Crytek.FarCryNewDawn.zip");
        var arc = new ZipArchive(s, ZipArchiveMode.Read);
        HashEntries = arc.Entries.ToDictionary(x => x.Name, x => x);
    }

    static readonly ConcurrentDictionary<string, IDictionary<ulong, string>> HashLookups = new();
    public static IDictionary<ulong, string> GetHashLookup(string path) => HashLookups.GetOrAdd(path, x => {
        var value = new Dictionary<ulong, string>();
        string line;
        using var r = new StreamReader(HashEntries[path].Open());
        while ((line = r.ReadLine()) != null) {
            var hashLower = MurmurHash3.Hash(line.ToLowerInvariant());
            var hashUpper = MurmurHash3.Hash(line.ToUpperInvariant());
            var hash = (ulong)hashUpper << 32 | hashLower;
            if (value.TryGetValue(hash, out var collision))
                Console.WriteLine("[COLLISION]: " + collision + " <-> " + line);
            value.Add(hash, line);
        }
        return value;
    });
}