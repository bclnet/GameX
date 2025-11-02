using CommandLine;
using System;
using static OpenStack.Debug;

namespace GameX.App.Client;

partial class Program {
    [Verb("game", isDefault: true, HelpText = "Get files contents")]
    class GameOptions {
        [Option('f', "family", Required = true, HelpText = "Family")] public string Family { get; set; }
        [Option('u', "uri", Required = true, HelpText = "Uri to extract")] public Uri Uri { get; set; }
    }

    public static GameController Game;

    static int RunGame(GameOptions args) {
        IPluginHost pluginHost = null;

        // get family
        var family = FamilyManager.GetFamily(args.Family);
        if (family == null) { Console.WriteLine($"No family found named '{args.Family}'."); return 0; }

        // get game
        var game = family.OpenPakFile(args.Uri);
        if (game == null) { Console.WriteLine($"No game found named '{args.Uri}'."); return 0; }

        Trace("Running game...");
        using (Game = new GameController(pluginHost)) {
            Game.Run();
        }
        Trace("Exiting game...");
        return 0;
    }
}
