using CommandLine;
using OpenStack;
using OpenStack.Vfx;
using System;
using System.Threading.Tasks;

namespace GameX.App.Cli {
    partial class Program {
        [Verb("set", HelpText = "Insert files contents to pak")]
        class SetOptions {
            [Option('f', "family", Required = true, HelpText = "Family")] public string Family { get; set; }
            [Option('u', "uri", Required = true, HelpText = "Uri to create")] public Uri Uri { get; set; }
            [Option('m', "match", HelpText = "Match")] public string Match { get; set; }
            [Option('o', "option", Default = FileOption.Default, HelpText = "Option")] public FileOption Option { get; set; }
            [Option('p', "path", Default = @".\out", HelpText = "Insert folder")] public string Path { get; set; }
        }

        static async Task<int> RunSetAsync(SetOptions args) {
            var from = ProgramState.Load(Convert.ToInt32, 0);

            // get family
            var family = FamilyManager.GetFamily(args.Family);
            if (family == null) { Console.WriteLine($"No family found named \"{args.Family}\"."); return 0; }

            // get resource
            var res = family.ParseResource(args.Uri);
            var path = PlatformX.DecodePath(args.Path);
            var match = args.Match != null ? FileSystem.CreateMatcher(args.Match) : null;

            // import
            await ImportManager.ImportAsync(family, res, path, match, from, args.Option);

            ProgramState.Clear();
            return 0;
        }
    }
}