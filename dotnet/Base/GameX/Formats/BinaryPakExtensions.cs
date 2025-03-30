using GameX.Formats.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Formats;

public static class BinaryPakExtensions
{
    const int MaxDegreeOfParallelism = 8; //1;

    #region Export

    public static async Task ExportAsync(this BinaryPakFile source, string filePath, int from = 0, object option = default, Action<FileSource, int> advance = null, Action<FileSource, string> exception = null)
    {
        var fo = option as FileOption?;

        // write pak
        if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

        // write files
        Parallel.For(from, source.Files.Count, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async index =>
        {
            var file = source.Files[index];
            var newPath = filePath != null ? Path.Combine(filePath, file.Path) : null;

            // create directory
            var directory = newPath != null ? Path.GetDirectoryName(newPath) : null;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // recursive extract pak, and exit
            if (file.Pak != null) { await file.Pak.ExportAsync(newPath); return; }

            // ensure cached object factory
            if ((fo & (FileOption.Stream | FileOption.Model)) != 0) source.EnsureCachedObjectFactory(file);

            // extract file
            try
            {
                await ExportFileAsync(file, source, newPath, option);
                if (file.Parts != null && (fo & FileOption.Raw) != 0)
                    foreach (var part in file.Parts) await ExportFileAsync(part, source, Path.Combine(filePath, part.Path), option);
                advance?.Invoke(file, index);
            }
            catch (Exception e) { exception?.Invoke(file, $"Exception: {e.Message}"); }
        });

        // write pak-raw
        if ((fo & FileOption.Marker) != 0) await new StreamPakFile(source, new PakState(source.FileSystem, source.Game, source.Edition, filePath)).Write(null);
    }

    static async Task ExportFileAsync(FileSource file, BinaryPakFile source, string newPath, object option = default)
    {
        var fo = option as FileOption?;

        if (file.FileSize == 0 && file.PackedSize == 0) return;
        var fileOption = file.CachedObjectOption as FileOption?;
        if ((fo & fileOption) != 0)
        {
            if ((fo & FileOption.Model) != 0)
            {
                var model = await source.LoadFileObject<IUnknownFileModel>(file, FamilyManager.UnknownPakFile);
                UnknownFileWriter.Factory("default", model).Write(newPath, false);
                return;
            }
            else if ((fo & FileOption.Stream) != 0)
            {
                if (!(await source.LoadFileObject<object>(file) is IHaveStream haveStream))
                {
                    PakBinary.HandleException(null, option, $"ExportFileAsync: {file.Path} @ {file.FileSize}");
                    throw new InvalidOperationException();
                }
                using var b2 = haveStream.GetStream();
                using var s2 = newPath != null
                    ? new FileStream(newPath, FileMode.Create, FileAccess.Write)
                    : (Stream)new MemoryStream();
                b2.CopyTo(s2);
                return;
            }
        }
        using var b = await source.LoadFileData(file, option);
        using var s = newPath != null
            ? new FileStream(newPath, FileMode.Create, FileAccess.Write)
            : (Stream)new MemoryStream();
        b.CopyTo(s);
        if (file.Parts != null && (fo & FileOption.Raw) == 0)
            foreach (var part in file.Parts)
            {
                using var b2 = await source.LoadFileData(part, option);
                b2.CopyTo(s);
            }
    }

    #endregion

    #region Import

    public static async Task ImportAsync(this BinaryPakFile source, BinaryWriter w, string filePath, int from = 0, object option = default, Action<FileSource, int> advance = null, Action<FileSource, string> exception = null)
    {
        // read pak
        if (string.IsNullOrEmpty(filePath) || !Directory.Exists(filePath)) { exception?.Invoke(null, $"Directory Missing: {filePath}"); return; }
        var setPath = Path.Combine(filePath, ".set");
        using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(source, r, "Set");
        var metaPath = Path.Combine(filePath, ".meta");
        using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(source, r, "Meta");
        var rawPath = Path.Combine(filePath, ".raw");
        if (File.Exists(rawPath)) using (var r = new BinaryReader(File.Open(rawPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(source, r, "Raw");

        // write header
        if (from == 0) await source.PakBinary.Write(source, w, "Header");

        // write files
        Parallel.For(0, source.Files.Count, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async index =>
        {
            var file = source.Files[index];
            var newPath = Path.Combine(filePath, file.Path);

            // check directory
            var directory = Path.GetDirectoryName(newPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) { exception?.Invoke(file, $"Directory Missing: {directory}"); return; }

            // insert file
            try
            {
                await source.PakBinary.Write(source, w);
                using (var s = File.Open(newPath, FileMode.Open, FileAccess.Read, FileShare.Read)) await source.WriteData(file, s, option);
                advance?.Invoke(file, index);
            }
            catch (Exception e) { PakBinary.HandleException(file, option, $"Exception: {e.Message}"); }
        });
    }

    #endregion
}