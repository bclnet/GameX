using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Crytek.Formats;

#region Binary_ArcheAge

public class Binary_ArcheAge : ArcBinary {
    readonly byte[] Key;

    public Binary_ArcheAge(byte[] key) => Key = key;

    #region Headers

    const uint AA_MAGIC = 0x4f424957; // Magic for Archeage, the literal string "WIBO".

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AA_Header {
        public uint Magic;
        public uint Dummy1;
        public uint FileCount;
        public uint ExtraFiles;
        public uint Dummy2;
        public uint Dummy3;
        public uint Dummy4;
        public uint Dummy5;
    }

    #endregion

    public unsafe override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        FileSource[] files;

        var stream = r.BaseStream;
        using (var aes = Aes.Create()) {
            aes.Key = Key;
            aes.IV = new byte[16];
            aes.Mode = CipherMode.CBC;
            r = new BinaryReader(new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read));
            stream.Seek(stream.Length - 0x200, SeekOrigin.Begin);

            var header = r.ReadS<AA_Header>();
            if (header.Magic != AA_MAGIC) throw new FormatException("BAD MAGIC");
            source.Magic = header.Magic;

            var totalSize = (header.FileCount + header.ExtraFiles) * 0x150;
            var infoOffset = stream.Length - 0x200;
            infoOffset -= totalSize;
            while (infoOffset >= 0) {
                if ((infoOffset % 0x200) != 0) infoOffset -= 0x10;
                else break;
            }

            // read-all files
            var fileIdx = 0U;
            source.Files = files = new FileSource[header.FileCount];
            for (var i = 0; i < header.FileCount; i++) {
                stream.Seek(infoOffset, SeekOrigin.Begin);
                r = new BinaryReader(new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read));
                var nameAsSpan = r.ReadBytes(0x108).AsSpan();
                files[fileIdx++] = new FileSource {
                    //.Replace('\\', '/')
                    Path = Encoding.ASCII.GetString(nameAsSpan[..nameAsSpan.IndexOf(byte.MinValue)]), //: name
                    Offset = r.ReadInt64(),   //: offset
                    FileSize = r.ReadInt64(),   //: size
                    PackedSize = r.ReadInt64(), //: xsize
                    Compressed = r.ReadInt32(), //: ysize
                };
                infoOffset += 0x150;
            }
        }
        return Task.CompletedTask;
    }

    public unsafe override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        // position
        r.Seek(file.Offset);
        Stream fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
        return Task.FromResult(fileData);
    }
}

#endregion

#region Binary_Cry3

/// <summary>
/// Binary_Cry3
/// </summary>
/// <seealso cref="GameX.Formats.PakBinary" />
public unsafe class Binary_Cry3 : ArcBinary<Binary_Cry3> {
    readonly byte[] Key;

    public Binary_Cry3() { }
    public Binary_Cry3(byte[] key = null) => Key = key;

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        var files = source.Files = new List<FileSource>();
        source.UseReader = false;

        var arc = (Cry3File)(source.Tag = new Cry3File(r.BaseStream, Key));
        var parentByPath = new Dictionary<string, FileSource>();
        var partByPath = new Dictionary<string, SortedList<string, FileSource>>();
        foreach (ZipEntry entry in arc) {
            var metadata = new FileSource {
                Path = entry.Name.Replace('\\', '/'),
                Flags = entry.Flags,
                PackedSize = entry.CompressedSize,
                FileSize = entry.Size,
                Tag = entry,
            };
            var metadataPath = metadata.Path;
            if (metadataPath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase)) parentByPath.Add(metadataPath, metadata);
            else if (metadataPath[^8..].Contains(".dds.", StringComparison.OrdinalIgnoreCase)) {
                var parentPath = metadataPath[..(metadataPath.IndexOf(".dds", StringComparison.OrdinalIgnoreCase) + 4)];
                var parts = partByPath.TryGetValue(parentPath, out var z) ? z : null;
                if (parts == null) partByPath.Add(parentPath, parts = []);
                parts.Add(metadataPath, metadata);
                continue;
            }
            files.Add(metadata);
        }

        // process links
        if (partByPath.Count != 0)
            foreach (var kv in partByPath) if (parentByPath.TryGetValue(kv.Key, out var parent)) parent.Parts = kv.Value.Values;
        return Task.CompletedTask;
    }

    public override Task Write(BinaryArchive source, BinaryWriter w, object tag) {
        source.UseReader = false;
        var files = source.Files;
        var arc = (Cry3File)(source.Tag = new Cry3File(w.BaseStream, Key));
        arc.BeginUpdate();
        foreach (var file in files) {
            var entry = (ZipEntry)(file.Tag = new ZipEntry(Path.GetFileName(file.Path)));
            arc.Add(entry);
            source.ArcBinary.WriteData(source, w, file, null);
        }
        arc.CommitUpdate();
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        var arc = (Cry3File)source.Tag;
        var entry = (ZipEntry)file.Tag;
        try {
            using var input = arc.GetInputStream(entry);
            if (!input.CanRead) { HandleException(file, option, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
            var s = new MemoryStream();
            input.CopyTo(s);
            s.Position = 0;
            return Task.FromResult((Stream)s);
        }
        catch (Exception e) { HandleException(file, option, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
    }

    public override Task WriteData(BinaryArchive source, BinaryWriter w, FileSource file, Stream data, object option = default) {
        var arc = (Cry3File)source.Tag;
        var entry = (ZipEntry)file.Tag;
        try {
            using var s = arc.GetInputStream(entry);
            data.CopyTo(s);
        }
        catch (Exception e) { HandleException(file, option, $"Exception: {e.Message}"); }
        return Task.CompletedTask;
    }
}

#endregion
