using GameX.Formats;
using GameX.Formats.Unknown;
using OpenStack.Vfx;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.App;

public static class ExportManager {
    const int MaxDegreeOfParallelism = 1; //4;

    public static async Task ExportAsync(Family family, Resource res, string filePath, Func<string, bool> match, int from, object option) {
        var fo = option as FileOption? ?? FileOption.Default;
        using var pak = family.OpenPakFile(res);
        // single
        if (pak is not MultiPakFile multi) { await ExportPakAsync(filePath, match, from, option, pak); return; }
        // write paks
        if ((fo & FileOption.Marker) != 0) {
            if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            var setPath = Path.Combine(filePath, ".set");
            using var w = new BinaryWriter(new FileStream(setPath, FileMode.Create, FileAccess.Write));
            await PakBinary.Stream.Write(new StreamPakFile(NetworkHost.Factory, new PakState(null, null, null, "Root")) {
                Files = [.. multi.PakFiles.Select(x => new FileSource { Path = x.Name })]
            }, w, "Set");
        }
        // multi
        foreach (var _ in multi.PakFiles) await ExportPakAsync(filePath, match, from, option, _);
    }

    static async Task ExportPakAsync(string filePath, Func<string, bool> match, int from, object option, PakFile _) {
        if (_ is not BinaryPakFile pak) throw new InvalidOperationException("s not a BinaryPakFile");
        var newPath = filePath != null ? Path.Combine(filePath, Path.GetFileName(pak.PakPath)) : null;
        // write pak
        await ExportPak2Async(pak, newPath, match, from, option,
            (file, idx) => { if ((idx % 50) == 0) Console.WriteLine($"{idx,6}> {file.Path}"); },
            (file, msg) => Console.WriteLine($"ERROR: {msg} - {file?.Path}"));
    }

    static async Task ExportPak2Async(BinaryPakFile source, string filePath, Func<string, bool> match, int from, object option, Action<FileSource, int> next, Action<FileSource, string> error) {
        var fo = option as FileOption? ?? FileOption.Default;
        source.Open();
        // create directory
        if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
        // write files
        Parallel.For(from, source.Files.Count, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async index => {
            var file = source.Files[index];
            if (match != null && !match(file.Path)) return;
            var newPath = filePath != null ? Path.Combine(filePath, file.Path) : null;

            // create directory
            var directory = newPath != null ? Path.GetDirectoryName(newPath) : null;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // recursive extract pak, and exit
            if (file.Pak != null) { await ExportPak2Async(file.Pak, newPath, match, 0, option, next, error); return; }

            // ensure cached object factory
            if ((fo & FileOption.Object) != 0) source.EnsureCachedObjectFactory(file);

            // extract file
            try {
                await ExportFileAsync(file, source, newPath, option);
                if (file.Parts != null && (fo & FileOption.Raw) != 0)
                    foreach (var part in file.Parts) await ExportFileAsync(part, source, Path.Combine(filePath, part.Path), option);
                next?.Invoke(file, index);
            }
            catch (Exception e) { error?.Invoke(file, $"Exception: {e.Message}"); }
        });
        // write pak-raw
        if ((fo & FileOption.Marker) != 0) await new StreamPakFile(source, new PakState(source.Vfx, source.Game, source.Edition, filePath)).Write(null);
    }

    static async Task ExportFileAsync(FileSource file, BinaryPakFile source, string newPath, object option) {
        var fo = option as FileOption? ?? FileOption.Default;
        if (file.FileSize == 0 && file.PackedSize == 0) return;
        var oo = (file.CachedObjectOption as FileOption?) ?? FileOption.Default;
        if (file.CachedObjectOption != null && (fo & oo) != 0) {
            if (oo.HasFlag(FileOption.UnknownFileModel)) {
                var model = await source.LoadFileObject<IUnknownFileModel>(file, FamilyManager.UnknownPakFile);
                UnknownFileWriter.Factory("default", model).Write(newPath, false);
                return;
            }
            else if (oo.HasFlag(FileOption.BinaryObject)) {
                var obj = await source.LoadFileObject<object>(file);
                if (obj is IStream src) {
                    using var b2 = src.GetStream();
                    using var s2 = newPath != null
                        ? new FileStream(newPath, FileMode.Create, FileAccess.Write)
                        : (Stream)new MemoryStream();
                    b2.CopyTo(s2);
                    return;
                }
                PakBinary.HandleException(null, option, $"BinaryObject: {file.Path} @ {file.FileSize}");
                throw new InvalidOperationException();
            }
            else if (oo.HasFlag(FileOption.StreamObject)) {
                var obj = await source.LoadFileObject<object>(file);
                if (obj is IWriteToStream src) {
                    using var s2 = newPath != null
                        ? new FileStream(newPath, FileMode.Create, FileAccess.Write)
                        : (Stream)new MemoryStream();
                    src.WriteToStream(s2);
                    return;
                }
                PakBinary.HandleException(null, option, $"StreamObject: {file.Path} @ {file.FileSize}");
                throw new InvalidOperationException();
            }
        }
        using var b = await source.LoadFileData(file, option);
        using var s = newPath != null
            ? new FileStream(newPath, FileMode.Create, FileAccess.Write)
            : (Stream)new MemoryStream();
        b.CopyTo(s);
        if (file.Parts != null && (fo & FileOption.Raw) == 0)
            foreach (var part in file.Parts) {
                using var b2 = await source.LoadFileData(part, option);
                b2.CopyTo(s);
            }
    }
}