using GameX.Capcom.Formats;
using GameX.Capcom.Transforms;
using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Unknown;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Capcom;

#region CapcomPakFile

/// <summary>
/// CapcomPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class CapcomPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CapcomPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CapcomPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = state.Game.Engine.n switch {
            "Unity" => Unity.UnityPakFile.ObjectFactory,
            _ => ObjectFactory,
        };
    }

    #region Factories

    static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new();

    static PakBinary GetPakBinary(FamilyGame game, string extension) => PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game, extension));

    static PakBinary PakBinaryFactory(FamilyGame game, string extension)
        => game.Engine.n switch {
            "Zip" => Binary_Zip.GetPakBinary(game),
            "Unity" => Unity.Formats.Binary_Unity.Current,
            _ => extension switch {
                ".pak" => Binary_Kpka.Current,
                ".arc" => Binary_Arc.Current,
                ".big" => Binary_Big.Current,
                ".bundle" => Binary_Bundle.Current,
                ".mbundle" => Binary_Plist.Current,
                _ => null, //throw new ArgumentOutOfRangeException(nameof(extension)),
            },
        };

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}

#endregion
