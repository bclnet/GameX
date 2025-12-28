using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Unity.Formats;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Unity;

/// <summary>
/// UnityArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class UnityArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="UnityArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public UnityArchive(BinaryState state) : base(state, Binary_Unity.Current) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
