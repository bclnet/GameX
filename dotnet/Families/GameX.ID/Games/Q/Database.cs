using GameX.ID.Formats;
using System;

namespace GameX.ID.Games.Q;

public static class Database {
    public static Archive PakFile;

    internal static void Loaded(FamilyGame game) {
        PakFile = game.Family.OpenArchive(new Uri("game:/#Q"));
        PakFile.GetAsset<Binary_Lmp>("PAK0.PAK:gfx/palette.lmp");
        PakFile.GetAsset<Binary_Lmp>("PAK0.PAK:gfx/colormap.lmp");
    }
}
