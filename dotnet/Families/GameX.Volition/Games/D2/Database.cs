using GameX.Uncore.Formats;
using System;

namespace GameX.Volition.Games.D2;

public static class Database {
    public static Archive Archive;
    public static Binary_Pal Palette;

    internal static void Loaded(FamilyGame game) {
        Archive = game.Family.GetArchive(new Uri("game:/#D2"));
        Palette = Archive.GetAsset<Binary_Pal>("groupa.256").Result.ConvertVgaPalette();
    }
}
