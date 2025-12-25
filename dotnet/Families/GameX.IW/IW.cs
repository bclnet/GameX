using GameX.Formats.IUnknown;
using GameX.IW.Formats;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.IW;

/// <summary>
/// IWArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class IWArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="IWArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public IWArchive(ArchiveState state) : base(state, Binary_IW.Current) {
        AssetFactoryFunc = AssetFactory;
        UseReader = false;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            //".roq" => (0, VIDEO.Factory),
            //".wav" => (0, BinaryWav.Factory),
            //".d3dbsp" => (0, BinaryD3dbsp.Factory),
            ".iwi" => (0, Binary_Iwi.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
