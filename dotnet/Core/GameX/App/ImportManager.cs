using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.App;

public static class ImportManager {
    const int MaxDegreeOfParallelism = 8; //1;

    public static async Task ImportAsync(Family family, Resource resource, string filePath, Func<string, bool> match, int from, object option) {
        //foreach (var path in resource.Paths)
        //{
        //    using var arc = family.OpenArchive(resource.Game, new[] { path }) as BinaryArchive;
        //    if (arc == null) throw new InvalidOperationException("Arc not a BinaryArchive");

        //    // import arc
        //    var w = await ImportPakAsync(filePath, from, path, option, arc);
        //}
    }

    static async Task<BinaryWriter> ImportPakAsync(string filePath, int from, string path, object option, BinaryAsset arc) {
        // import arc
        var w = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
        await arc.ImportAsync(w, filePath, from, option, (file, index) => {
            //if ((index % 50) == 0)
            //    Console.WriteLine($"{file.Path}");
        }, (file, message) => {
            Console.WriteLine($"{message}: {file?.Path}");
        });
        return w;
    }

    static async Task ImportAsync(this BinaryAsset source, BinaryWriter w, string filePath, int from = 0, object option = default, Action<FileSource, int> advance = null, Action<FileSource, string> exception = null) {
        // read arc
        if (string.IsNullOrEmpty(filePath) || !Directory.Exists(filePath)) { exception?.Invoke(null, $"Directory Missing: {filePath}"); return; }
        var setPath = Path.Combine(filePath, ".set");
        using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await ArcBinary.Stream.Read(source, r, "Set");
        var metaPath = Path.Combine(filePath, ".meta");
        using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await ArcBinary.Stream.Read(source, r, "Meta");
        var rawPath = Path.Combine(filePath, ".raw");
        if (File.Exists(rawPath)) using (var r = new BinaryReader(File.Open(rawPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await ArcBinary.Stream.Read(source, r, "Raw");

        // write header
        if (from == 0) await source.ArcBinary.Write(source, w, "Header");

        // write files
        Parallel.For(0, source.Files.Count, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async index => {
            var file = source.Files[index].Fix();
            var newPath = Path.Combine(filePath, file.Path);

            // check directory
            var directory = Path.GetDirectoryName(newPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) { exception?.Invoke(file, $"Directory Missing: {directory}"); return; }

            // insert file
            try {
                await source.ArcBinary.Write(source, w);
                using (var s = File.Open(newPath, FileMode.Open, FileAccess.Read, FileShare.Read)) await source.WriteData(file, s, option);
                advance?.Invoke(file, index);
            }
            catch (Exception e) { ArcBinary.HandleException(file, option, $"Exception: {e.Message}"); }
        });
    }
}