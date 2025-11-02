using CommandLine;

namespace GameX.App.Client;

// test
partial class Program {
    static void Main(string[] args) {
        // TEST
        args = ["game", "-f", "Origin", "-u", @"game:/#UO"];

        Parser.Default.ParseArguments<GameOptions, TestOptions>(args)
        .MapResult(
            (GameOptions opts) => RunGame(opts),
            (TestOptions opts) => RunTestAsync(opts).GetAwaiter().GetResult(),
            errs => 1);
    }
}
