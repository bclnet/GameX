using GameX.Epic.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Epic;

/// <summary>
/// EpicPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class EpicPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="EpicPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public EpicPakFile(ArchiveState state) : base(state, Binary_Pck.Current) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    // object factory
    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
