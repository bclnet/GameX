using GameX.Formats;
using System;

namespace GameX.Volition.Games.D;

public static class Database {
    public static Archive Archive;
    public static Binary_Pal Palette;

    internal static void Loaded(FamilyGame game) {
        Archive = game.Family.OpenArchive(new Uri("game:/descent.hog#Radius"));
        Palette = Archive.GetAsset<Binary_Pal>("palette.256").Result.ConvertVgaPalette();
    }
}
