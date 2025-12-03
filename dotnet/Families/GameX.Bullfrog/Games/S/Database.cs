using GameX.Formats;
using System;
using System.Collections.Concurrent;

namespace GameX.Bullfrog.Games.S;

public static class Database {
    public static Archive PakFile;
    static ConcurrentDictionary<string, Binary_Pal> Palettes = new();

    internal static void Loaded(FamilyGame game) {
        PakFile = game.Family.OpenArchive(new Uri("game:/#S"));
    }

    public static Binary_Pal GetPalette(string path, string defaultValue)
        => Palettes.GetOrAdd(path ?? string.Empty, s => PakFile.GetAsset<Binary_Pal>(s.Length > 0 && PakFile.Contains($"{s}.PAL") ? $"{s}.PAL" : $"{defaultValue}.PAL").Result.ConvertVgaPalette());
}
