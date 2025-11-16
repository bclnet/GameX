using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Unknown;
using GameX.Xbox.Formats;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Xbox;

/// <summary>
/// StardewGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class StardewGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Binary_Xnb.ContentReader.Add(new Binary_Xnb.TypeReader<string>("BmFont.XmlSourceReader", "System.String", r => r.ReadLV7UString()));
        Binary_Xnb.ContentReader.Add(new Binary_Xnb.TypeReader<string>("BmFont.XmlSourceReader", "System.String", r => r.ReadLV7UString()));
    }
}

/// <summary>
/// XboxPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class XboxPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="XboxPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public XboxPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static PakBinary GetPakBinary(FamilyGame game, string extension)
        => extension switch {
            "" => null,
            ".xxx" => Binary_XXX.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".xnb" => (0, Binary_Xnb.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
