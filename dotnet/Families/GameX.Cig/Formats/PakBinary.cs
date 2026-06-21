using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Cig.Formats;

/// <summary>
/// PakBinary_P4k
/// </summary>
/// <seealso cref="GameX.Formats.PakBinary" />
public class PakBinary_P4k : ArcBinary<PakBinary_P4k> {
    readonly byte[] Key = [0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47];

    protected class SubArchiveP4k : BinaryArchive {
        ZipArchiveX Arc;

        public SubArchiveP4k(BinaryArchive parent, ZipArchiveX arc, string path, object tag) : base(parent, new BinaryState(parent.Vfx, parent.Game, parent.Edition, path, tag), Current) {
            AssetFactoryFunc = parent.AssetFactoryFunc;
            Arc = arc;
            UseReader = false;
            //Open();
        }

        public async override Task Read(object tag) {
            var entry = (ZipArchiveEntry)Tag;
            var stream = entry.OpenX();
            using var r2 = new BinaryReader(stream);
            await ArcBinary.Read(this, r2, tag);
        }
    }

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        source.UseReader = false;
        var files = source.Files = [];

        var arc = (ZipArchiveX)(source.Tag = new ZipArchiveX(r.BaseStream, path: source.BinPath, key: Key, kind: ZipKind.P4k));
        var parentByPath = new Dictionary<string, FileSource>();
        var partsByPath = new Dictionary<string, SortedList<string, FileSource>>();
        foreach (var entry in arc.Entries) {
            var metadata = new FileSource {
                Path = entry.FullName.Replace('\\', '/'),
                //Flags = entry.Flags,
                PackedSize = entry.CompressedLength,
                FileSize = entry.Length,
                Tag = entry,
            };
            var metadataPath = metadata.Path;
            if (metadataPath.EndsWith(".arc", StringComparison.OrdinalIgnoreCase) || metadataPath.EndsWith(".socpak", StringComparison.OrdinalIgnoreCase)) metadata.Arc = new SubArchiveP4k(source, arc, metadataPath, metadata.Tag);
            else if (metadataPath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) || metadataPath.EndsWith(".dds.a", StringComparison.OrdinalIgnoreCase)) parentByPath.Add(metadataPath, metadata);
            else if (metadataPath.Length > 8 && metadataPath[^8..].Contains(".dds.", StringComparison.OrdinalIgnoreCase)) {
                var parentPath = metadataPath[..(metadataPath.IndexOf(".dds", StringComparison.OrdinalIgnoreCase) + 4)];
                if (metadataPath.EndsWith("a")) parentPath += ".a";
                var parts = partsByPath.TryGetValue(parentPath, out var z) ? z : null;
                if (parts == null) partsByPath.Add(parentPath, parts = []);
                parts.Add(metadataPath, metadata);
                continue;
            }
            files.Add(metadata);
        }

        // process links
        if (partsByPath.Count > 0)
            foreach (var kv in partsByPath) if (parentByPath.TryGetValue(kv.Key, out var parent)) parent.Parts = kv.Value.Values;
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        var arc = (ZipArchiveX)source.Tag;
        var entry = (ZipArchiveEntry)file.Tag;
        try {
            using var input = entry.OpenX();
            if (!input.CanRead) { HandleException(file, option, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
            var s = new MemoryStream();
            input.CopyTo(s);
            if (file.Parts != null)
                foreach (var part in file.Parts.Reverse()) {
                    var entry2 = (ZipArchiveEntry)part.Tag;
                    using var input2 = entry2.OpenX();
                    if (!input2.CanRead) { HandleException(file, option, $"Unable to read stream for file: {part.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                    input2.CopyTo(s);
                }
            s.Position = 0;
            return Task.FromResult((Stream)s);
        }
        catch (Exception e) { HandleException(file, option, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
    }

    #region Write

    //public override Task Write(BinaryArchive source, BinaryWriter w, object tag) {
    //    source.UseReader = false;
    //    var files = source.Files;

    //    var arc = (P4kArchive)(source.Tag = new P4kArchive(w.BaseStream, source.BinPath, Key));
    //    arc.BeginUpdate();
    //    foreach (var file in files) {
    //        var entry = (ZipArchiveEntry)(file.Tag = new ZipArchiveEntry(Path.GetFileName(file.Path)));
    //        arc.Add(entry);
    //        source.ArcBinary.WriteData(source, w, file, null);
    //    }
    //    arc.CommitUpdate();
    //    return Task.CompletedTask;
    //}

    //public override Task WriteData(BinaryArchive source, BinaryWriter w, FileSource file, Stream data, object option = default) {
    //    var arc = (P4kArchive)source.Tag;
    //    var entry = (ZipArchiveEntry)file.Tag;
    //    try {
    //        using var s = entry.Open2();
    //        data.CopyTo(s);
    //        if (file.Parts != null)
    //            foreach (var part in file.Parts.Reverse()) {
    //                var entry2 = (ZipArchiveEntry)part.Tag;
    //                using var s2 = entry2.Open2();
    //                data.CopyTo(s2);
    //            }
    //    }
    //    catch (Exception e) { HandleException(file, option, $"Exception: {e.Message}"); }
    //    return Task.CompletedTask;
    //}

    #endregion
}