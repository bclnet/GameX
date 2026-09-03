using OpenStack;
using OpenStack.Client;
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
        RunArgs args = new() { Args = args2, Platform = Option.Platform, Family = Option.Family, Game = Option.Game, Edition = Option.Edition };
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
