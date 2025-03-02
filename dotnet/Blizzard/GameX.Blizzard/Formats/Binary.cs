using GameX.Blizzard.Formats.Casc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Blizzard.Formats;

#region Binary_Blizzard

public unsafe class Binary_Blizzard : PakBinary<Binary_Blizzard>
{
    CascContext casc;

    public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
    {
        var files = source.Files = [];

        // load casc
        var editions = source.Game.Editions;
        var product = editions.First().Key;
        casc = new CascContext();
        casc.Read(source.PakPath, product, files);
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        => Task.FromResult(casc.ReadData(file));
}

#endregion

#region Binary_Pak

public class Binary_Pak : IHaveMetaInfo
{
    public Binary_Pak() { }
    public Binary_Pak(BinaryReader r) => Read(r);

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("Binary_Pak", items: [
            //new($"Type: {Type}"),
        ])
    ];

    public unsafe void Read(BinaryReader r)
        => throw new NotImplementedException();
}

#endregion
