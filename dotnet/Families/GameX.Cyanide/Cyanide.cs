using GameX.Cyanide.Formats;
using GameX.Uncore.Formats;
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
public class CyanideArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CyanideArchive" /> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="state">The state.</param>
    public CyanideArchive(Archive parent, BinaryState state) : base(parent, state, Binary_Cpk.Current) {
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

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(object source, Archive transformTo) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(object source, Archive transformTo) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
