using GameX.Formats.Unknown;
using GameX.Frontier.Formats;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Frontier;

/// <summary>
/// FrontierPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class FrontierPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="FrontierPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public FrontierPakFile(ArchiveState state) : base(state, Binary_Frontier.Current) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
