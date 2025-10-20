using GameX.Formats;
using System;
using System.Collections.Concurrent;

namespace GameX.Bullfrog.Games.DK;

public static class Database {
    public static PakFile PakFile;
    static ConcurrentDictionary<string, Binary_Pal> Palettes = new();

    internal static FamilyGame Ensure(FamilyGame game) {
        PakFile = game.Family.OpenPakFile(new Uri("game:/#DK"));
        return game;
    }

    public static Binary_Pal GetPalette(string path, string defaultValue)
        => Palettes.GetOrAdd(path ?? string.Empty, s => PakFile.LoadFileObject<Binary_Pal>(s.Length > 0 && PakFile.Contains($"{s}.PAL") ? $"{s}.PAL" : $"{defaultValue}.PAL").Result.ConvertVgaPalette());
}
