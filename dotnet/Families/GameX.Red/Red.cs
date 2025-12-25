using GameX.Bioware.Formats;
using GameX.Formats.IUnknown;
using GameX.Red.Formats;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Red;

/// <summary>
/// RedArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class RedArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="RedArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public RedArchive(ArchiveState state) : base(state, Binary_Red.Current) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            // witcher 1
            ".dlg" or ".qdb" or ".qst" => (0, Binary_Gff.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
