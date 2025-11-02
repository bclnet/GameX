using GameX.Formats.Unknown;
using GameX.Gamebryo.Formats;
using GameX.Transforms;
using GameX.Unknown;
using OpenStack.Gfx;
using System;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Gamebryo;

/// <summary>
/// GamebryoPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class GamebryoPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="GamebryoPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public GamebryoPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
        PathFinders.Add(typeof(ITexture), FindTexture);
    }

    #region PathFinders

    /// <summary>
    /// Finds the actual path of a texture.
    /// </summary>
    public object FindTexture(object path) {
        if (path is not string p) return path;
        var textureName = Path.GetFileNameWithoutExtension(p);
        var textureNameInTexturesDir = $"textures/{textureName}";
        var texturePathWithoutExtension = $"{Path.GetDirectoryName(p)}/{textureName}";
        if (Contains(p = $"{textureNameInTexturesDir}.dds")) return p;
        else if (Contains(p = $"{texturePathWithoutExtension}.dds")) return p;
        else if (Contains(p = $"{textureNameInTexturesDir}.tga")) return p;
        else if (Contains(p = $"{texturePathWithoutExtension}.tga")) return p;
        else { Log($"Could not find file '{p}' in A PAK file."); return null; }
    }

    #endregion

    #region Factories

    static PakBinary GetPakBinary(FamilyGame game, string extension)
        => extension switch {
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".nif" => (FileOption.StreamObject, Binary_Nif.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
