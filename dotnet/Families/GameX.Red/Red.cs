using GameX.Bioware.Formats;
using GameX.Formats.Unknown;
using GameX.Red.Formats;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Red;

/// <summary>
/// RedPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class RedPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="RedPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public RedPakFile(ArchiveState state) : base(state, Binary_Red.Current) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            // witcher 1
            ".dlg" or ".qdb" or ".qst" => (0, Binary_Gff.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
