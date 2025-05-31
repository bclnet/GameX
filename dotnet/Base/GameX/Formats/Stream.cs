using OpenStack.Vfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Formats;

#region Stream

public class PakBinaryCanStream : PakBinary {
    public override Task Read(BinaryPakFile source, BinaryReader r, object tag) {
        switch ((string)tag) {
            case null: return Task.CompletedTask;
            case "Set": {
                    var files = source.Files = new List<FileSource>();
                    var data = r.ReadToEnd();
                    // dir /s/b/a-d > .set
                    var lines = Encoding.ASCII.GetString(data)?.Split('\n');
                    if (lines?.Length == 0) return Task.CompletedTask;
                    string path;
                    var startIndex = Path.GetDirectoryName(lines[0].TrimEnd().Replace('\\', '/')).Length + 1;
                    foreach (var line in lines)
                        if (line.Length >= startIndex && (path = line[startIndex..].TrimEnd().Replace('\\', '/')) != ".set")
                            files.Add(new FileSource { Path = path });
                    return Task.CompletedTask;
                }
            case "Meta": {
                    _ = source.Process();

                    var data = r.ReadToEnd();
                    var lines = Encoding.ASCII.GetString(data)?.Split('\n');
                    if (lines?.Length == 0) return Task.CompletedTask;
                    var state = -1;
                    var paramsx = source.Params;
                    var filesByPath = source.FilesByPath;
                    foreach (var line in lines) {
                        var path = line.TrimEnd().Replace('\\', '/');
                        if (state == -1) {
                            if (path == "Params:") state = 0;
                            else if (path == "AllCompressed") foreach (var file in source.Files) file.Compressed = 1;
                            else if (path == "Compressed:") state = 1;
                            else if (path == "Crypted:") state = 2;
                        }
                        else {
                            if (string.IsNullOrEmpty(line)) { state = -1; continue; }
                            var files = filesByPath[line];
                            switch (state) {
                                case 0: var args = line.Split([':'], 2); paramsx[args[0]] = args[1]; continue;
                                case 1: if (files != null) files.First().Compressed = 1; continue;
                                case 2: if (files != null) files.First().Flags = 1; continue;
                            }
                        }
                    }
                    return Task.CompletedTask;
                }
            case "Raw": {
                    var filesRawSet = source.FilesRawSet = [];
                    var data = r.ReadToEnd();
                    var lines = Encoding.ASCII.GetString(data)?.Split('\n');
                    if (lines?.Length == 0) return Task.CompletedTask;
                    foreach (var line in lines) filesRawSet.Add(line.TrimEnd().Replace('\\', '/'));
                    return Task.CompletedTask;
                }
            default: throw new ArgumentOutOfRangeException(nameof(tag), tag?.ToString());
        }
    }

    public override Task Write(BinaryPakFile source, BinaryWriter w, object tag) {
        switch ((string)tag) {
            case null: return Task.CompletedTask;
            case "Set": {
                    var pathAsBytes = Encoding.ASCII.GetBytes($@"C:/{source.Name}/");
                    w.Write(pathAsBytes);
                    w.Write(Encoding.ASCII.GetBytes(".set"));
                    w.Write((byte)'\n');
                    w.Flush();
                    // files
                    var files = source.Files;
                    foreach (var file in files) //.OrderBy(x => x.Path))
                    {
                        w.Write(pathAsBytes);
                        w.Write(Encoding.ASCII.GetBytes(file.Path));
                        w.Write((byte)'\n');
                        w.Flush();
                    }
                    return Task.CompletedTask;
                }
            case "Meta": {
                    // meta
                    var @params = source.Params;
                    if (@params.Count > 0) {
                        w.Write(Encoding.ASCII.GetBytes("Params:\n"));
                        foreach (var param in @params) {
                            w.Write(Encoding.ASCII.GetBytes($"{param.Key}:{param.Value}"));
                            w.Write((byte)'\n');
                            w.Flush();
                        }
                        w.Write((byte)'\n');
                        w.Flush();
                    }
                    // compressed
                    var files = source.Files;
                    var numCompressed = files.Count(x => x.Compressed != 0);
                    if (files.Count == numCompressed) w.Write(Encoding.ASCII.GetBytes("AllCompressed\n"));
                    else if (numCompressed > 0) {
                        w.Write(Encoding.ASCII.GetBytes("Compressed:\n"));
                        foreach (var file in files.Where(x => x.Compressed != 0)) {
                            w.Write(Encoding.ASCII.GetBytes(file.Path));
                            w.Write((byte)'\n');
                            w.Flush();
                        }
                        w.Write((byte)'\n');
                        w.Flush();
                    }
                    // crypted
                    if (files.Any(x => x.Flags != 0)) {
                        w.Write(Encoding.ASCII.GetBytes("Crypted:\n"));
                        foreach (var file in files.Where(x => x.Flags != 0)) {
                            w.Write(Encoding.ASCII.GetBytes(file.Path));
                            w.Write((byte)'\n');
                            w.Flush();
                        }
                        w.Write((byte)'\n');
                        w.Flush();
                    }
                    return Task.CompletedTask;
                }
            case "Raw": {
                    if (source.FilesRawSet == null) throw new ArgumentNullException(nameof(source.FilesRawSet));
                    foreach (var file in source.FilesRawSet) {
                        w.Write(Encoding.ASCII.GetBytes(file));
                        w.Write((byte)'\n');
                        w.Flush();
                    }
                    return Task.CompletedTask;
                }
            default: throw new ArgumentOutOfRangeException(nameof(tag), tag?.ToString());
        }
    }
}

/// <summary>
/// StreamPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class StreamPakFile : BinaryPakFile {
    readonly NetworkHost Host;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamPakFile" /> class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <param name="state">The state.</param>
    /// <param name="address">The host.</param>
    public StreamPakFile(Func<Uri, string, NetworkHost> factory, PakState state, Uri address = null) : base(state, new PakBinaryCanStream()) {
        UseReader = false;
        if (address != null) Host = factory(address, state.Path);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamPakFile" /> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="state">The state.</param>
    public StreamPakFile(BinaryPakFile parent, PakState state) : base(state, new PakBinaryCanStream()) {
        UseReader = false;
        Files = parent.Files;
    }

    /// <summary>
    /// Reads the asynchronous.
    /// </summary>
    /// <param name="tag">The tag.</param>
    public override async Task Read(object tag) {
        // http pak
        if (Host != null) {
            var files = Files = [];
            var set = await Host.GetSetAsync() ?? throw new NotSupportedException(".set not found");
            foreach (var item in set) files.Add(new FileSource { Path = item });
            return;
        }

        // read pak
        var path = PakPath;
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
        var setPath = Path.Combine(path, ".set");
        if (File.Exists(setPath)) using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(this, r, "Set");
        var metaPath = Path.Combine(path, ".meta");
        if (File.Exists(metaPath)) using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(this, r, "Meta");
        var rawPath = Path.Combine(path, ".raw");
        if (File.Exists(rawPath)) using (var r = new BinaryReader(File.Open(rawPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(this, r, "Raw");
    }

    /// <summary>
    /// Writes the asynchronous.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <exception cref="NotSupportedException"></exception>
    public override async Task Write(object tag) {
        // http pak
        if (Host != null) throw new NotSupportedException();

        // write pak
        var path = PakPath;
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path)) Directory.CreateDirectory(path);
        var setPath = Path.Combine(path, ".set");
        using (var w = new BinaryWriter(new FileStream(setPath, FileMode.Create, FileAccess.Write))) await PakBinary.Stream.Write(this, w, "Set");
        var metaPath = Path.Combine(path, ".meta");
        using (var w = new BinaryWriter(new FileStream(metaPath, FileMode.Create, FileAccess.Write))) await PakBinary.Stream.Write(this, w, "Meta");
        var rawPath = Path.Combine(path, ".raw");
        if (FilesRawSet != null && FilesRawSet.Count > 0) using (var w = new BinaryWriter(new FileStream(rawPath, FileMode.Create, FileAccess.Write))) await PakBinary.Stream.Write(this, w, "Raw");
        else if (File.Exists(rawPath)) File.Delete(rawPath);
    }

    /// <summary>
    /// Reads the file data asynchronous.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="option">The option.</param>
    /// <param name="exception">The exception.</param>
    /// <returns></returns>
    public override async Task<Stream> ReadData(FileSource file, object option = default) {
        var path = file.Path;
        // http pak
        if (Host != null) return await Host.GetFileAsync(path);

        // read pak
        path = System.IO.Path.Combine(PakPath, path);
        return File.Exists(path) ? File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
    }

    /// <summary>
    /// Writes the file data asynchronous.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="data">The data.</param>
    /// <param name="option">The option.</param>
    /// <param name="exception">The exception.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public override Task WriteData(FileSource file, Stream data, object option = default) => throw new NotSupportedException();
}

#endregion