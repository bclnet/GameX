using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Ubisoft.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Ubisoft;

/// <summary>
/// UbisoftPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class UbisoftPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="UbisoftPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public UbisoftPakFile(ArchiveState state) : base(state, GetPakBinary(state.Game, state.Path)) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static ArcBinary GetPakBinary(FamilyGame game, string filePath)
        => filePath == null || Path.GetExtension(filePath).ToLowerInvariant() != ".zip"
            ? Binary_Ubi.Current
            : Binary_Zip.GetPakBinary(game);

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
       => Path.GetExtension(source.Path).ToLowerInvariant() switch {
           ".dds" => (0, Binary_Dds.Factory),
           _ => (0, null),
       };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
