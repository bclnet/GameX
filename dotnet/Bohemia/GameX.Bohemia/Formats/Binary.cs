using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Bohemia.Formats;

#region Binary_Abc

public class Binary_Abc : IHaveMetaInfo {
    public Binary_Abc(BinaryReader r) {
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("BinaryPak", items: [
            //new($"Type: {Type}"),
        ])
    ];
}

#endregion

#region Binary_XXX

public unsafe class Binary_XXX : PakBinary<Binary_XXX> {
    public override Task Read(BinaryPakFile source, BinaryReader r, object tag) {
        var files = source.Files = [];

        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default) {
        throw new NotImplementedException();
    }
}

#endregion