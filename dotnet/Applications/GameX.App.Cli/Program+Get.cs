using CommandLine;
using OpenStack;
using OpenStack.Vfx;
using System;
using System.Threading.Tasks;

namespace GameX.App.Cli;

partial class Program {
    [Verb("get", HelpText = "GetByName files contents")]
    class GetOptions {
        [Option('f', "family", Required = true, HelpText = "Family")] public string Family { get; set; }
        [Option('u', "uri", Required = true, HelpText = "Uri to extract")] public Uri Uri { get; set; }
        [Option('m', "match", HelpText = "Match")] public string Match { get; set; }
        [Option('o', "option", Default = FileOption.Default, HelpText = "Option")] public FileOption Option { get; set; }
        [Option('p', "path", Default = @".\out", HelpText = "Output folder")] public string Path { get; set; }
    }

    static async Task<int> RunGetAsync(GetOptions args) {
        var from = ProgramState.Load(Convert.ToInt32, 0);

        // get family
        var family = FamilyManager.GetFamily(args.Family);
        if (family == null) { Console.WriteLine($"No family found named \"{args.Family}\"."); return 0; }

        // get resource
        var res = family.ParseResource(args.Uri);
        var path = PlatformX.DecodePath(args.Path);
        var match = args.Match != null ? FileSystem.CreateMatcher(args.Match) : null;

        // export
        await ExportManager.ExportAsync(family, res, path, match, from, args.Option);

        ProgramState.Clear();
        return 0;
    }
}
