using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Frictional.Formats;

#region Binary_Abc

public class Binary_Abc : IHaveMetaInfo
{
    public Binary_Abc() { }
    public Binary_Abc(BinaryReader r) => Read(r);

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new MetaInfo("BinaryPak", items: [
            //new($"Type: {Type}"),
        ])
    ];

    public unsafe void Read(BinaryReader r)
        => throw new NotImplementedException();
}

#endregion

#region Binary_Hpl

public unsafe class Binary_Hpl : PakBinary<Binary_Hpl>
{
    public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
    {
        var files = source.Files = [];

        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
    {
        throw new NotImplementedException();
    }
}

#endregion