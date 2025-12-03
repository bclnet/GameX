using GameX.Origin.Clients.UO;
using OpenStack;
using System;

namespace GameX.App.Client;

record RunArgs {
    public string Family;
    public Uri Uri;
}

// test
partial class Program {
    static int Main(string[] args2) {
        RunArgs args = new() { Family = "Origin", Uri = new Uri(@"game:/#UO") };
        return Run(args);
    }

    public static GameController Game;

    static int Run(RunArgs args) {
        IPluginHost pluginHost = null;

        // get family
        var family = FamilyManager.GetFamily(args.Family);
        if (family == null) { Console.WriteLine($"No family found named '{args.Family}'."); return 0; }

        // get game
        var game = family.OpenArchive(args.Uri);
        if (game == null) { Console.WriteLine($"No game found named '{args.Uri}'."); return 0; }

        Log.Trace("Running game...");
        using (Game = new UOGameController<object>(game, pluginHost)) {
            Game.Run();
        }
        Log.Trace("Exiting game...");
        return 0;
    }
}
