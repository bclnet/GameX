using GameX.Uncore.Formats;
using System;
using System.Collections.Concurrent;

namespace GameX.Bullfrog.Games.P2;

public static class Database {
    public static Archive Archive;
    static ConcurrentDictionary<string, Binary_Pal> Palettes = new();

    internal static void Loaded(FamilyGame game) {
        Archive = game.Family.GetArchive(new Uri("game:/#P2"));
    }

    public static Binary_Pal GetPalette(string path, string defaultValue)
        => Palettes.GetOrAdd(path ?? string.Empty, s => Archive.GetAsset<Binary_Pal>(s.Length > 0 && Archive.Contains($"{s}.PAL") ? $"{s}.PAL" : $"{defaultValue}.PAL").Result.ConvertVgaPalette());
}
