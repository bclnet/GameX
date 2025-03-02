using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.MODEL.Formats;

    #region Binary_Abc

    public class Binary_Abc : IHaveMetaInfo
    {
        public Binary_Abc(BinaryReader r)
        {
        }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new("BinaryPak", items: [
                //new($"Type: {Type}"),
            ])
        ];
    }

    #endregion

    #region Binary_XXX

    public unsafe class Binary_XXX : PakBinary<PakBinary_XXX>
    {
        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
