using GameX.Formats;
using System;

namespace GameX.Origin.Games.U9;

public static class Database {
    public static PakFile PakFile;
    public static Binary_Pal Palette;

    internal static FamilyGame Ensure(FamilyGame game) {
        PakFile = game.Family.OpenPakFile(new Uri("game:/#U9"));
        Palette = PakFile.LoadFileObject<Binary_Pal>("static/ankh.pal").Result;
        return game;
    }
}
