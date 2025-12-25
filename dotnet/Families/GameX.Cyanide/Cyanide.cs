using GameX.Cyanide.Formats;
using GameX.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cyanide;

/// <summary>
/// CyanideArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class CyanideArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CyanideArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CyanideArchive(ArchiveState state) : base(state, Binary_Cpk.Current) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".dds" => (0, Binary_Dds.Factory),
            _ => (0, null),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
