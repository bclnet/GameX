using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.App.Cli;

partial class Program {
    [Verb("list", HelpText = "List files contents")]
    class ListOptions {
        [Option('f', "family", HelpText = "Family")] public string Family { get; set; }
        [Option('u', "uri", HelpText = "Uri to list")] public Uri Uri { get; set; }
    }

    static Task<int> RunListAsync(ListOptions args) {
        // list families
        if (string.IsNullOrEmpty(args.Family)) {
            Console.WriteLine("Families installed:\n");
            foreach (var _ in FamilyManager.Families) Console.WriteLine($"{_.Key} - {_.Value.Name}");
            return Task.FromResult(0);
        }

        // get family
        var family = FamilyManager.GetFamily(args.Family, false);
        if (family == null) { Console.WriteLine($"No family found named \"{args.Family}\"."); return Task.FromResult(0); }

        // list found paths in family
        if (args.Uri == null) {
            Console.WriteLine($"{family.Id}");
            Console.WriteLine($"  name: {family.Name}");
            Console.WriteLine($"  description: {family.Description}");
            Console.WriteLine($"  studio: {family.Studio}");
            Console.WriteLine($"\nGames:");
            foreach (var game in family.Games.Values) {
                Console.WriteLine($"{game.Name}");
                if (game.Found == null) continue;
                Console.WriteLine($"  urls: {string.Join(',', (IEnumerable<Uri>)game.Arcs)}");
                Console.WriteLine($"  root: {game.Found.Root}");
            }
            return Task.FromResult(0);
        }

        // list files in pack for family
        else {
            Console.WriteLine($"{family.Name} - {args.Uri}\n");
            using var s = family.OpenArchive(args.Uri) as BinaryAsset ?? throw new InvalidOperationException("s not BinaryAsset");
            if (s.Count == 0) { Console.WriteLine("Nothing found."); return Task.FromResult(0); }
            Console.WriteLine("files:");
            foreach (var p in s.Files.OrderBy(x => x.Path)) {
                Console.WriteLine($"{p.Path}");
                var pak = p.Arc;
                if (pak == null) continue;
                pak.Open();
                foreach (var x in pak.Files.Select(x => Path.GetExtension(x.Path)).GroupBy(x => x)) Console.WriteLine($"  {x.Key}: {x.Count()}");
            }
        }
        return Task.FromResult(0);
    }
}
