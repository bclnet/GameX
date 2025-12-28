using GameX.Formats.IUnknown;
using GameX.Firaxis.Formats;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Firaxis;

/// <summary>
/// FiraxisArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class FiraxisArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="FiraxisArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public FiraxisArchive(BinaryState state) : base(state, Binary_Hpl.Current) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
