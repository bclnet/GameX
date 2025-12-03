using System;

namespace GameX.Origin.Clients.UO;

public static class Database {
    public static Archive Archive = FamilyManager.GetFamily("Origin").OpenArchive(new Uri("game:/#UO"));

    //Games.UO.Database.Archive?.GetAsset<Binary_StringTable>("Cliloc.enu").Result;
    //public static int ItemIDMask => ClientVersion.InstallationIsUopFormat ? 0xffff : 0x3fff;

    //internal static FamilyGame Ensure(FamilyGame game)
    //{
    //    if (Archive != null) return game;
    //    try
    //    {
    //        Archive = game.Family.OpenArchive(new Uri("game:/#UO"));
    //        //Archive.GetAsset<object>("Cliloc.enu").Wait();
    //        //Cliloc = Binary_StringTable.Records;
    //        Log($"Successfully opened {Archive} file");
    //    }
    //    catch (Exception e)
    //    {
    //        Log($"An exception occured while attempting to open {Archive} file. This needs to be corrected in order for Landblocks to load.");
    //        Log($"Exception: {e.Message}");
    //    }
    //    return game;
    //}
}
