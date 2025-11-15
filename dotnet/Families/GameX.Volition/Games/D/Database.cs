using GameX.Formats;
using System;

namespace GameX.Volition.Games.D;

public static class Database {
    public static PakFile PakFile;
    public static Binary_Pal Palette;

    internal static void Loaded(FamilyGame game) {
        PakFile = game.Family.OpenPakFile(new Uri("game:/descent.hog#Radius"));
        Palette = PakFile.LoadFileObject<Binary_Pal>("palette.256").Result.ConvertVgaPalette();
    }
}
