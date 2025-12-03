using GameX.ID.Formats;
using System;

namespace GameX.ID.Games.Q;

public static class Database {
    public static Archive Archive;

    internal static void Loaded(FamilyGame game) {
        Archive = game.Family.OpenArchive(new Uri("game:/#Q"));
        Archive.GetAsset<Binary_Lmp>("PAK0.PAK:gfx/palette.lmp");
        Archive.GetAsset<Binary_Lmp>("PAK0.PAK:gfx/colormap.lmp");
    }
}
