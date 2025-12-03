using GameX.Formats.Unknown;
using GameX.IW.Formats;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.IW;

/// <summary>
/// IWPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class IWPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="IWPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public IWPakFile(ArchiveState state) : base(state, Binary_IW.Current) {
        ObjectFactoryFunc = ObjectFactory;
        UseReader = false;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            //".roq" => (0, VIDEO.Factory),
            //".wav" => (0, BinaryWav.Factory),
            //".d3dbsp" => (0, BinaryD3dbsp.Factory),
            ".iwi" => (0, Binary_Iwi.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
