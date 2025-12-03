using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.WB.Formats;
using GameX.WB.Formats.AC.FileTypes;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Environment = GameX.WB.Formats.AC.FileTypes.Environment;

namespace GameX.WB;

/// <summary>
/// ACGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class ACGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
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
/// WBArchive
/// </summary>
/// <seealso cref="GameEstate.Formats.BinaryArchive" />
public class WBArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    static WBArchive() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    /// <summary>
    /// Initializes a new instance of the <see cref="WBArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public WBArchive(ArchiveState state) : base(state, Binary_AC.Current) {
        AssetFactoryFunc = AssetFactory;
        UseFileId = true;
    }

    #region Factories

    internal static string GetPath(FileSource source, BinaryReader r, PakType pakType, out ArchiveType? fileType) {
        if ((uint)source.Id == Iteration.FILE_ID) { fileType = null; return "Iteration"; }
        var (type, ext) = GetFileType(source, pakType);
        if (type == 0) { fileType = null; return $"{source.Id:X8}"; }
        fileType = type;
        return ext switch {
            null => $"{fileType}/{source.Id:X8}",
            string extension => $"{fileType}/{source.Id:X8}.{extension}",
            Func<FileSource, BinaryReader, string> func => $"{fileType}/{source.Id:X8}.{func(source, r)}",
            _ => throw new ArgumentOutOfRangeException(nameof(ext), ext.ToString()),
        };
    }

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game) {
        var (pakType, type) = ((PakType, ArchiveType?))source.Tag2;
        if ((uint)source.Id == Iteration.FILE_ID) return (0, (r, m, s) => Task.FromResult((object)new Iteration(r)));
        else if (type == null) return (0, null);
        else return type.Value switch {
            ArchiveType.LandBlock => (0, (r, m, s) => Task.FromResult((object)new Landblock(r))),
            ArchiveType.LandBlockInfo => (0, (r, m, s) => Task.FromResult((object)new LandblockInfo(r))),
            ArchiveType.EnvCell => (0, (r, m, s) => Task.FromResult((object)new EnvCell(r))),
            //ArchiveType.LandBlockObjects => (0, null),
            //ArchiveType.Instantiation => (0, null),
            ArchiveType.GfxObject => (0, (r, m, s) => Task.FromResult((object)new GfxObj(r))),
            ArchiveType.Setup => (0, (r, m, s) => Task.FromResult((object)new SetupModel(r))),
            ArchiveType.Animation => (0, (r, m, s) => Task.FromResult((object)new Animation(r))),
            //ArchiveType.AnimationHook => (0, null),
            ArchiveType.Palette => (0, (r, m, s) => Task.FromResult((object)new Palette(r))),
            ArchiveType.SurfaceTexture => (0, (r, m, s) => Task.FromResult((object)new SurfaceTexture(r))),
            ArchiveType.Texture => (0, (r, m, s) => Task.FromResult((object)new Texture(r, game))),
            ArchiveType.Surface => (0, (r, m, s) => Task.FromResult((object)new Surface(r))),
            ArchiveType.MotionTable => (0, (r, m, s) => Task.FromResult((object)new MotionTable(r))),
            ArchiveType.Wave => (0, (r, m, s) => Task.FromResult((object)new Wave(r))),
            ArchiveType.Environment => (0, (r, m, s) => Task.FromResult((object)new Environment(r))),
            ArchiveType.ChatPoseTable => (0, (r, m, s) => Task.FromResult((object)new ChatPoseTable(r))),
            ArchiveType.ObjectHierarchy => (0, (r, m, s) => Task.FromResult((object)new GeneratorTable(r))), //: Name wayoff
            ArchiveType.BadData => (0, (r, m, s) => Task.FromResult((object)new BadData(r))),
            ArchiveType.TabooTable => (0, (r, m, s) => Task.FromResult((object)new TabooTable(r))),
            ArchiveType.FileToId => (0, null),
            ArchiveType.NameFilterTable => (0, (r, m, s) => Task.FromResult((object)new NameFilterTable(r))),
            ArchiveType.MonitoredProperties => (0, null),
            ArchiveType.PaletteSet => (0, (r, m, s) => Task.FromResult((object)new PaletteSet(r))),
            ArchiveType.Clothing => (0, (r, m, s) => Task.FromResult((object)new ClothingTable(r))),
            ArchiveType.DegradeInfo => (0, (r, m, s) => Task.FromResult((object)new GfxObjDegradeInfo(r))),
            ArchiveType.Scene => (0, (r, m, s) => Task.FromResult((object)new Scene(r))),
            ArchiveType.Region => (0, (r, m, s) => Task.FromResult((object)new RegionDesc(r))),
            ArchiveType.KeyMap => (0, null),
            ArchiveType.RenderTexture => (0, (r, m, s) => Task.FromResult((object)new RenderTexture(r))),
            ArchiveType.RenderMaterial => (0, null),
            ArchiveType.MaterialModifier => (0, null),
            ArchiveType.MaterialInstance => (0, null),
            ArchiveType.SoundTable => (0, (r, m, s) => Task.FromResult((object)new SoundTable(r))),
            ArchiveType.UILayout => (0, null),
            ArchiveType.EnumMapper => (0, (r, m, s) => Task.FromResult((object)new EnumMapper(r))),
            ArchiveType.StringTable => (0, (r, m, s) => Task.FromResult((object)new StringTable(r))),
            ArchiveType.DidMapper => (0, (r, m, s) => Task.FromResult((object)new DidMapper(r))),
            ArchiveType.ActionMap => (0, null),
            ArchiveType.DualDidMapper => (0, (r, m, s) => Task.FromResult((object)new DualDidMapper(r))),
            ArchiveType.String => (0, (r, m, s) => Task.FromResult((object)new LanguageString(r))), //: Name wayoff
            ArchiveType.ParticleEmitter => (0, (r, m, s) => Task.FromResult((object)new ParticleEmitterInfo(r))),
            ArchiveType.PhysicsScript => (0, (r, m, s) => Task.FromResult((object)new PhysicsScript(r))),
            ArchiveType.PhysicsScriptTable => (0, (r, m, s) => Task.FromResult((object)new PhysicsScriptTable(r))),
            ArchiveType.MasterProperty => (0, null),
            ArchiveType.Font => (0, (r, m, s) => Task.FromResult((object)new Font(r))),
            ArchiveType.FontLocal => (0, null),
            ArchiveType.StringState => (0, (r, m, s) => Task.FromResult((object)new LanguageInfo(r))), //: Name wayoff
            ArchiveType.DbProperties => (0, null),
            ArchiveType.RenderMesh => (0, null),
            ArchiveType.WeenieDefaults => (0, null),
            ArchiveType.CharacterGenerator => (0, (r, m, s) => Task.FromResult((object)new CharGen(r))),
            ArchiveType.SecondaryAttributeTable => (0, (r, m, s) => Task.FromResult((object)new SecondaryAttributeTable(r))),
            ArchiveType.SkillTable => (0, (r, m, s) => Task.FromResult((object)new SkillTable(r))),
            ArchiveType.SpellTable => (0, (r, m, s) => Task.FromResult((object)new SpellTable(r))),
            ArchiveType.SpellComponentTable => (0, (r, m, s) => Task.FromResult((object)new SpellComponentTable(r))),
            ArchiveType.TreasureTable => (0, null),
            ArchiveType.CraftTable => (0, null),
            ArchiveType.XpTable => (0, (r, m, s) => Task.FromResult((object)new XpTable(r))),
            ArchiveType.Quests => (0, null),
            ArchiveType.GameEventTable => (0, null),
            ArchiveType.QualityFilter => (0, (r, m, s) => Task.FromResult((object)new QualityFilter(r))),
            ArchiveType.CombatTable => (0, (r, m, s) => Task.FromResult((object)new CombatManeuverTable(r))),
            ArchiveType.ItemMutation => (0, null),
            ArchiveType.ContractTable => (0, (r, m, s) => Task.FromResult((object)new ContractTable(r))),
            _ => (0, null),
        };
    }

    public static (ArchiveType fileType, object ext) GetFileType(FileSource source, PakType pakType) {
        var objectId = (uint)source.Id;
        if (pakType == PakType.Cell) {
            if ((objectId & 0xFFFF) == 0xFFFF) return (ArchiveType.LandBlock, "land");
            else if ((objectId & 0xFFFF) == 0xFFFE) return (ArchiveType.LandBlockInfo, "lbi");
            else return (ArchiveType.EnvCell, "cell");
        }
        else if (pakType == PakType.Portal) {
            switch (objectId >> 24) {
                case 0x01: return (ArchiveType.GfxObject, "obj");
                case 0x02: return (ArchiveType.Setup, "set");
                case 0x03: return (ArchiveType.Animation, "anm");
                case 0x04: return (ArchiveType.Palette, "pal");
                case 0x05: return (ArchiveType.SurfaceTexture, "texture");
                case 0x06: return (ArchiveType.Texture, "tex"); // new ArchiveExtensionAttribute(typeof(FormatExtensions), "TextureExtensionLookup").Value);
                case 0x08: return (ArchiveType.Surface, "surface");
                case 0x09: return (ArchiveType.MotionTable, "dsc");
                case 0x0A: return (ArchiveType.Wave, "wav");
                case 0x0D: return (ArchiveType.Environment, "env");
                case 0x0F: return (ArchiveType.PaletteSet, "pst");
                case 0x10: return (ArchiveType.Clothing, "clo");
                case 0x11: return (ArchiveType.DegradeInfo, "deg");
                case 0x12: return (ArchiveType.Scene, "scn");
                case 0x13: return (ArchiveType.Region, "rgn");
                case 0x14: return (ArchiveType.KeyMap, "keymap");
                case 0x15: return (ArchiveType.RenderTexture, "rtexture");
                case 0x16: return (ArchiveType.RenderMaterial, "mat");
                case 0x17: return (ArchiveType.MaterialModifier, "mm");
                case 0x18: return (ArchiveType.MaterialInstance, "mi");
                case 0x20: return (ArchiveType.SoundTable, "stb");
                case 0x22: return (ArchiveType.EnumMapper, "emp");
                case 0x25: return (ArchiveType.DidMapper, "imp");
                case 0x26: return (ArchiveType.ActionMap, "actionmap");
                case 0x27: return (ArchiveType.DualDidMapper, "dimp");
                case 0x30: return (ArchiveType.CombatTable, null);
                case 0x31: return (ArchiveType.String, "str");
                case 0x32: return (ArchiveType.ParticleEmitter, "emt");
                case 0x33: return (ArchiveType.PhysicsScript, "pes");
                case 0x34: return (ArchiveType.PhysicsScriptTable, "pet");
                case 0x39: return (ArchiveType.MasterProperty, "mpr");
                case 0x40: return (ArchiveType.Font, "font");
                case 0x78: return (ArchiveType.DbProperties, new ArchiveExtensionAttribute(typeof(WBArchive), "DbPropertyExtensionLookup").Value);
            }
            switch (objectId >> 16) {
                case 0x0E01: return (ArchiveType.QualityFilter, null);
                case 0x0E02: return (ArchiveType.MonitoredProperties, "monprop");
            }
            if (objectId == 0x0E000002) return (ArchiveType.CharacterGenerator, null);
            else if (objectId == 0x0E000003) return (ArchiveType.SecondaryAttributeTable, null);
            else if (objectId == 0x0E000004) return (ArchiveType.SkillTable, null);
            else if (objectId == 0x0E000007) return (ArchiveType.ChatPoseTable, "cps");
            else if (objectId == 0x0E00000D) return (ArchiveType.ObjectHierarchy, "hrc");
            else if (objectId == 0x0E00000E) return (ArchiveType.SpellTable, "cps");
            else if (objectId == 0x0E00000F) return (ArchiveType.SpellComponentTable, "cps");
            else if (objectId == 0x0E000018) return (ArchiveType.XpTable, "cps");
            else if (objectId == 0xE00001A) return (ArchiveType.BadData, "bad");
            else if (objectId == 0x0E00001D) return (ArchiveType.ContractTable, null);
            else if (objectId == 0x0E00001E) return (ArchiveType.TabooTable, "taboo");
            else if (objectId == 0x0E00001F) return (ArchiveType.FileToId, null);
            else if (objectId == 0x0E000020) return (ArchiveType.NameFilterTable, "nft");
        }
        if (pakType == PakType.Language)
            switch (objectId >> 24) {
                case 0x21: return (ArchiveType.UILayout, null);
                case 0x23: return (ArchiveType.StringTable, null);
                case 0x41: return (ArchiveType.StringState, null);
            }
        Console.WriteLine($"Unknown file type: {objectId:X8}");
        return (0, null);
    }

    static string DbPropertyExtensionLookup(FileSource source, BinaryReader r)
        => 0 switch {
            0 => "dbpc",
            1 => "pmat",
            _ => throw new ArgumentOutOfRangeException(),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
