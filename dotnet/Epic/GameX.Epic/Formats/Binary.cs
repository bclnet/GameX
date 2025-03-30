﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Epic.Formats;

#region Binary_Pck

/// <summary>
/// PakBinaryPck
/// </summary>
/// <seealso cref="GameX.Formats.PakBinary" />
public unsafe class Binary_Pck : PakBinary<Binary_Pck>
{
    public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
    {
        List<FileSource> files;
        source.Files = files = new List<FileSource>();
        var header = new Core.UPackage(r, source.PakPath);
        if (header.Exports == null) return Task.CompletedTask;
        var R = header.R;
        foreach (var item in header.Exports)
            files.Add(new FileSource
            {
                Path = $"{header.GetClassNameFor(item)}/{item.ObjectName}",
                Offset = item.SerialOffset,
                FileSize = item.SerialSize,
                Tag = R,
            });
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default)
    {
        var R = (BinaryReader)file.Tag;
        R.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(R.ReadBytes((int)file.FileSize)));
    }

    public override Task WriteData(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, object option = default)
        => throw new NotImplementedException();
}

#endregion