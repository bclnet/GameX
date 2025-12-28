using GameX.Black.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Black;

/// <summary>
/// BlackArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class BlackArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BlackArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BlackArchive(BinaryState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => Binary_Dat.Current;

    //string.IsNullOrEmpty(extension)
    //? PakBinary_Dat.Instance
    //: extension switch
    //{
    //    ".dat" => PakBinary_Dat.Instance,
    //    _ => throw new ArgumentOutOfRangeException(nameof(extension)),
    //};

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            var x when x.StartsWith(".fr") => (0, Binary_Frm.Factory),
            ".pal" => (0, Binary_Pal2.Factory),
            ".rix" => (0, Binary_Rix.Factory),
            _ => (0, null),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
