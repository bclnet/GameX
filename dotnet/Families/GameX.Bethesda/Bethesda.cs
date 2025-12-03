using GameX.Bethesda.Formats;
using GameX.Formats.Unknown;
using GameX.Gamebryo.Formats;
using GameX.Transforms;
using GameX.Unknown;
using OpenStack;
using OpenStack.Gfx;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Bethesda;

/// <summary>
/// BethesdaFamily
/// </summary>
/// <seealso cref="GameX.Family" />
public class BethesdaFamily(JsonElement elem) : Family(elem) { }

/// <summary>
/// MorrowindGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class MorrowindGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        DatabaseManager.Loaded(this);
    }
}

/// <summary>
/// BethesdaArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class BethesdaArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BethesdaArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BethesdaArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
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
            "" => null,
            ".bsa" => Binary_Bsa.Current,
            ".ba2" => Binary_Ba2.Current,
            ".esm" => Binary_Esm.Current,
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
