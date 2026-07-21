using GameX.Crytek.Formats;
using GameX.Crytek.Formats.Dunia;
using GameX.Crytek.Transforms;
using GameX.Formats.IUnknown;
using GameX.Uncore;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("GameX.Cig")]

namespace GameX.Crytek;

/// <summary>
/// CrytekArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class CrytekArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CrytekArchive" /> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="state">The state.</param>
    public CrytekArchive(Archive parent, BinaryState state) : base(parent, state, GetArcBinary(state.Game)) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> ArcBinarys = new();

    static ArcBinary GetArcBinary(FamilyGame game)
        => ArcBinarys.GetOrAdd(game.Id, _ => game.Engine.n switch {
            "ArcheAge" => new Binary_ArcheAge((byte[])game.Key),
            "Dunia" => Binary_Dunia.Current,
            _ => new Binary_Cry3((byte[])game.Key),
        });

    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => game.Engine.n switch {
            "Dunia" => Path.GetExtension(source.Path).ToLowerInvariant() switch {
                ".aiw" => (0, Binary_AIWorkspace.Factory), // ??
                //".lua" => (0, Binary_Lua.Factory), // Lua scripts
                //".spk" or ".sbao" => (0, Binary_Spk.Factory), // Sound files
                ".xbt" => (0, Binary_Xbt.Factory), // Textures
                //".xbg" => (0, Binary_Xbg.Factory), // 3D Models
                ".fcb" => (0, Binary_Resource.Factory), // Binary
                ".xml" or ".rml" => (0, Binary_Xml.Factory), // Xml
                // ".mab" => Animation
                // ".hkx" => Havok physics
                // ".vcs" => Vistas
                // ".rnv" => Rnv Maps
                _ => UncoreArchive.AssetFactory(source, game),
            },
            _ => Path.GetExtension(source.Path).ToLowerInvariant() switch {
                ".xml" => (0, Binary_CryXml.Factory),
                ".cgf" or ".cga" or ".chr" or ".skin" or ".anim" => (0, Binary_CryFile.Factory),
                _ => UncoreArchive.AssetFactory(source, game),
            }
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(object source, Archive transformTo) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(object source, Archive transformTo) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
