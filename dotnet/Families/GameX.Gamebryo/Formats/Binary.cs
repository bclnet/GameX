using GameX.Gamebryo.Formats.Nif;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Gamebryo.Formats;

#region Binary_Nif

public class Binary_Nif(BinaryReader r, FileSource f) : NiReader(r), IHaveMetaInfo, IModel, IWriteToStream {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Nif(r, f));

    public string Name = Path.GetFileNameWithoutExtension(f.Path);

    #region IModel

    public T Create<T>(string platform, Func<object, T> func) {
        //Activator.CreateInstance("");
        func(null);
        return default;
    }

    #endregion

    public bool IsSkinnedMesh() => Blocks.Any(s => s is NiSkinInstance);

    public IEnumerable<string> GetTexturePaths() => Blocks.Select(s => s is NiSourceTexture z && !string.IsNullOrEmpty(z.FileName) ? z.FileName : null).Where(s => s != null);

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