using OpenX;
using OpenX.Client;
using System;
using static GameX.FamilyManager;

namespace GameX.App.Client;

record RunArgs {
    public string[] Args;
    public string Platform;
    public string Family;
    public string Game;
    public string Edition;
}

// test
partial class Program {
    [STAThread]
    public static int Main(string[] args2) {
        // debug: _Config family defines are off. Same shape as GameX.Cli's args override.
        if (args2.Length == 0 && string.IsNullOrEmpty(Option.Family))
            args2 = ["-e", "MG", "-f", "Origin", "-g", "UO"];
        var platform = Option.Engine;
        var family = Option.Family;
        var game = Option.Game;
        var edition = Option.Edition;
        var rest = new string[args2.Length];
        var n = 0;
        for (var i = 0; i < args2.Length; i++) {
            var a = args2[i];
            if (i + 1 < args2.Length && a is "-e" or "--engine") platform = args2[++i];
            else if (i + 1 < args2.Length && a is "-f" or "--family") family = args2[++i];
            else if (i + 1 < args2.Length && a is "-g" or "--game") game = args2[++i];
            else if (i + 1 < args2.Length && a is "--edition") edition = args2[++i];
            else rest[n++] = a;
        }
        if (n != rest.Length) Array.Resize(ref rest, n);
        RunArgs args = new() { Args = rest, Platform = platform, Family = family, Game = game, Edition = edition };
        return Run(args);
    }

    public static IClientHost ClientHost;

    // factory
    public static Func<ClientBase> CreateClient(string family, Uri uri, string[] args, object tag) {
        var archive = GetFamily(family).GetArchive(uri);
        return () => archive.Game.GetClient(new ClientState(archive, args, tag));
    }

    public static IClientHost CreateClientHost(string platform, Func<ClientBase> client) => platform switch {
        "GL" => new MgClientHost(client),
        "EX" => new ExClientHost(client),
        "MG" => new MgClientHost(client),
        _ => throw new ArgumentOutOfRangeException(nameof(platform))
    };

    static int Run(RunArgs args) {
        Log.Trace("Running game...");
        using (ClientHost = CreateClientHost(args.Platform, CreateClient(args.Family, FamilyGame.ToUri(args.Game, args.Edition), args.Args, null))) ClientHost.Run();
        Log.Trace("Exiting game...");
        return 0;
    }
}
