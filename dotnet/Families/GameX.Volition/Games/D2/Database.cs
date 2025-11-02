using GameX.Formats;
using System;

namespace GameX.Volition.Games.D2;

public static class Database {
    public static PakFile PakFile;
    public static Binary_Pal Palette;

    internal static void Loaded(FamilyGame game) {
        PakFile = game.Family.OpenPakFile(new Uri("game:/#D2"));
        Palette = PakFile.LoadFileObject<Binary_Pal>("groupa.256").Result.ConvertVgaPalette();
    }
}
