using GameX.Cyanide.Formats;
using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cyanide;

/// <summary>
/// CyanidePakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class CyanidePakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CyanidePakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CyanidePakFile(ArchiveState state) : base(state, Binary_Cpk.Current) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".dds" => (0, Binary_Dds.Factory),
            _ => (0, null),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
