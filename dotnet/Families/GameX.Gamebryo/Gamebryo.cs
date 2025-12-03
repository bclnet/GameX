using GameX.Formats.Unknown;
using GameX.Gamebryo.Formats;
using GameX.Transforms;
using GameX.Unknown;
using OpenStack;
using OpenStack.Gfx;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Gamebryo;

/// <summary>
/// GamebryoArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class GamebryoArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="GamebryoArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public GamebryoArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
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
        else { Log.Info($"Could not find file '{p}' in an arc file."); return null; }
    }

    #endregion

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => extension switch {
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".nif" => (FileOption.StreamObject, Binary_Nif.Factory),
            _ => UnknownArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
