using GameX.Uncore.Formats;
using System;

namespace GameX.Origin.Games.U9;

public static class Database {
    public static Archive Archive;
    public static Binary_Pal Palette;

    internal static FamilyGame Ensure(FamilyGame game) {
        Archive = game.Family.GetArchive(new Uri("game:/#U9"));
        Palette = Archive.GetAsset<Binary_Pal>("static/ankh.pal").Result;
        return game;
    }
}
