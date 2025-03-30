﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Cyanide.Formats;

#region Binary_Cpk

public unsafe class Binary_Cpk : PakBinary<Binary_Cpk>
{
    #region Headers

    const uint CPK_MAGIC = 0x01439855;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CPK_Header
    {
        public uint NumFiles;               // Number of files
        public fixed byte Root[512];        // Root name
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CPK_HeaderFile
    {
        public uint FileSize;               // File size
        public ulong Offset;                // File position
        public fixed byte FileName[512];    // File name
    }

    #endregion

    public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
    {
        var magic = source.Magic = r.ReadUInt32();
        if (magic != CPK_MAGIC) throw new FormatException("BAD MAGIC");
        var header = r.ReadS<CPK_Header>();
        var headerFiles = r.ReadSArray<CPK_HeaderFile>((int)header.NumFiles);
        var files = source.Files = new FileSource[header.NumFiles];
        UnsafeX.FixedAString(header.Root, 512);
        for (var i = 0; i < files.Count; i++)
        {
            var headerFile = headerFiles[i];
            files[i] = new FileSource
            {
                Path = UnsafeX.FixedAString(headerFile.FileName, 512).Replace('\\', '/'),
                FileSize = headerFile.FileSize,
                Offset = (long)headerFile.Offset,
            };
        }
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default)
    {
        r.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
    }
}

#endregion