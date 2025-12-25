using GameX.Cryptic.Formats;
using GameX.Uncore.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cryptic;

/// <summary>
/// CrypticArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class CrypticArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CrypticArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CrypticArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => Binary_Hogg.Current;

    //ref https://github.com/PlumberTaskForce/Datamining-Guide/blob/master/README.md
    internal static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".bin" => (0, Binary_Bin.Factory),
            ".htex" or ".wtex" => (0, Binary_Tex.Factory), // Textures
            ".mset" => (0, Binary_MSet.Factory), // 3D Models
            ".fsb" => (0, Binary_Fsb.Factory), // FMod Soundbanks
            ".bik" => (0, Binary_Bik.Factory), // Bink Video
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
