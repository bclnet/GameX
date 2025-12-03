using GameX.Formats;
using System;

namespace GameX.Origin.Games.U9;

public static class Database {
    public static Archive PakFile;
    public static Binary_Pal Palette;

    internal static FamilyGame Ensure(FamilyGame game) {
        PakFile = game.Family.OpenArchive(new Uri("game:/#U9"));
        Palette = PakFile.GetAsset<Binary_Pal>("static/ankh.pal").Result;
        return game;
    }
}
