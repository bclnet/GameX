using GameX.Gamebryo.Formats.Nif;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Gamebryo.Formats;

#region Binary_Nif

public class Binary_Nif(BinaryReader r, FileSource f) : NifReader(r), IHaveMetaInfo, IModel, IWriteToStream {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Nif(r, f));

    public string Name = Path.GetFileNameWithoutExtension(f.Path);

    #region IModel

    public T Create<T>(string platform, Func<object, T> func) {
        //Activator.CreateInstance("");
        func(null);
        return default;
    }

    #endregion

    public bool IsSkinnedMesh() => Blocks.Any(b => b is NiSkinInstance);

    public IEnumerable<string> GetTexturePaths() {
        foreach (var niObject in Blocks)
            if (niObject is NiSourceTexture niSourceTexture && !string.IsNullOrEmpty(niSourceTexture.FileName))
                yield return niSourceTexture.FileName;
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Object", Name = Name, Value = this }),
        new("NIF", items: [
            new($"NumBlocks: {NumBlocks}"),
        ]),
    ];

    public void WriteToStream(Stream stream) => this.Serialize(stream);

    public override string ToString() => this.Serialize();
}

#endregion