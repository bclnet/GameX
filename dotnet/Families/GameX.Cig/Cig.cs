using GameX.Cig.Formats;
using GameX.Cig.Transforms;
using GameX.Crytek.Formats;
using GameX.Formats.IUnknown;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cig;

/// <summary>
/// CigArchive
/// </summary>
/// <seealso cref="GameEstate.Formats.BinaryArchive" />
public class CigArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CigArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CigArchive(ArchiveState state) : base(state, PakBinary_P4k.Current) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    internal static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            //".cfg" => (0, BinaryDcb.Factory),
            ".mtl" or ".xml" => (0, CryXmlFile.Factory),
            ".a" => (0, Binary_DdsA.Factory),
            ".dcb" => (0, Binary_Dcb.Factory),
            ".soc" or ".cgf" or ".cga" or ".chr" or ".skin" or ".anim" => (0, CryFile.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
