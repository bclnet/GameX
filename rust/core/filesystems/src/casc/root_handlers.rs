// PORT-SOURCE: Core/GameX.FileSystems/Casc/RootHandlers.cs
// PORT-SHA: 13d2a96c27cd2848
// PORT-STATUS: done
// PORT-GENERATED: gen_enums.py — do not hand-edit; regenerate instead.
//
// 6 enum(s), 279 members, generated from the C#
// rather than transcribed. At this size a one-digit typo would mis-identify a
// game asset in a way no test would obviously catch.
//
// C# allows duplicate discriminants within an enum; Rust does not. Where that
// happens the first member becomes the variant and the rest become associated
// consts pointing at it, so every C# name still resolves.
//
// Per enum:
//   SNOGroup                           i32   enum         68 members
//   D4FolderType                       i32   enum          5 members
//   SnoGroupD4                         i32   enum        152 members
//   CASCSearchPhase                    i32   enum          3 members
//   LocaleFlags                        u32   bitflags     25 members
//   ContentFlags                       u32   bitflags     26 members

// PARTIAL PORT. `RootHandlers.cs` is 3,817 live lines — 43% of
// `GameX.FileSystems` — and contains per-game root manifest parsing for WoW,
// Diablo III, Diablo IV, Overwatch, Heroes of the Storm and more. Generated
// here are its six enums (279 members, verified against the C#); the handlers
// themselves and `MD5Hash` are below and in `casc_key.rs`.
//
// ============ THE CORE KEY TYPE IS BURIED IN THIS FILE ==================
//
// `MD5Hash` — the 16-byte content key that every index, every encoding table
// and every root entry is keyed on — is declared at **line 4196 of this
// file**, between a flags enum and `RootEntry`. It is the most fundamental type
// in CASC and it lives two-thirds of the way down the largest file in the
// project.
//
// The port puts it in `casc_key.rs`, which is where anything else can
// reasonably depend on it. Worth doing on the C# side too: this file is large
// enough that nothing in it is findable, and the type placement is a symptom.
//
#![allow(non_camel_case_types, non_upper_case_globals)]

/// C# `enum SNOGroup : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum SNOGroup {
    Code = -0x2,
    None = -0x1,
    Actor = 0x1,
    Adventure = 0x2,
    AiBehavior = 0x3,
    AiState = 0x4,
    AmbientSound = 0x5,
    Anim = 0x6,
    Animation2D = 0x7,
    AnimSet = 0x8,
    Appearance = 0x9,
    Hero = 0xa,
    Cloth = 0xb,
    Conversation = 0xc,
    ConversationList = 0xd,
    EffectGroup = 0xe,
    Encounter = 0xf,
    Explosion = 0x11,
    FlagSet = 0x12,
    Font = 0x13,
    GameBalance = 0x14,
    Globals = 0x15,
    LevelArea = 0x16,
    Light = 0x17,
    MarkerSet = 0x18,
    Monster = 0x19,
    Observer = 0x1a,
    Particle = 0x1b,
    Physics = 0x1c,
    Power = 0x1d,
    Quest = 0x1f,
    Rope = 0x20,
    Scene = 0x21,
    SceneGroup = 0x22,
    Script = 0x23,
    ShaderMap = 0x24,
    Shaders = 0x25,
    Shakes = 0x26,
    SkillKit = 0x27,
    Sound = 0x28,
    SoundBank = 0x29,
    StringList = 0x2a,
    Surface = 0x2b,
    Textures = 0x2c,
    Trail = 0x2d,
    UI = 0x2e,
    Weather = 0x2f,
    Worlds = 0x30,
    Recipe = 0x31,
    Condition = 0x33,
    TreasureClass = 0x34,
    Account = 0x35,
    Conductor = 0x36,
    TimedEvent = 0x37,
    Act = 0x38,
    Material = 0x39,
    QuestRange = 0x3a,
    Lore = 0x3b,
    Reverb = 0x3c,
    PhysMesh = 0x3d,
    Music = 0x3e,
    Tutorial = 0x3f,
    BossEncounter = 0x40,
    ControlScheme = 0x41,
    Accolade = 0x42,
    AnimTree = 0x43,
    Vibration = 0x44,
    DungeonFinder = 0x45,
}

impl SNOGroup {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            -0x2 => Self::Code,
            -0x1 => Self::None,
            0x1 => Self::Actor,
            0x2 => Self::Adventure,
            0x3 => Self::AiBehavior,
            0x4 => Self::AiState,
            0x5 => Self::AmbientSound,
            0x6 => Self::Anim,
            0x7 => Self::Animation2D,
            0x8 => Self::AnimSet,
            0x9 => Self::Appearance,
            0xa => Self::Hero,
            0xb => Self::Cloth,
            0xc => Self::Conversation,
            0xd => Self::ConversationList,
            0xe => Self::EffectGroup,
            0xf => Self::Encounter,
            0x11 => Self::Explosion,
            0x12 => Self::FlagSet,
            0x13 => Self::Font,
            0x14 => Self::GameBalance,
            0x15 => Self::Globals,
            0x16 => Self::LevelArea,
            0x17 => Self::Light,
            0x18 => Self::MarkerSet,
            0x19 => Self::Monster,
            0x1a => Self::Observer,
            0x1b => Self::Particle,
            0x1c => Self::Physics,
            0x1d => Self::Power,
            0x1f => Self::Quest,
            0x20 => Self::Rope,
            0x21 => Self::Scene,
            0x22 => Self::SceneGroup,
            0x23 => Self::Script,
            0x24 => Self::ShaderMap,
            0x25 => Self::Shaders,
            0x26 => Self::Shakes,
            0x27 => Self::SkillKit,
            0x28 => Self::Sound,
            0x29 => Self::SoundBank,
            0x2a => Self::StringList,
            0x2b => Self::Surface,
            0x2c => Self::Textures,
            0x2d => Self::Trail,
            0x2e => Self::UI,
            0x2f => Self::Weather,
            0x30 => Self::Worlds,
            0x31 => Self::Recipe,
            0x33 => Self::Condition,
            0x34 => Self::TreasureClass,
            0x35 => Self::Account,
            0x36 => Self::Conductor,
            0x37 => Self::TimedEvent,
            0x38 => Self::Act,
            0x39 => Self::Material,
            0x3a => Self::QuestRange,
            0x3b => Self::Lore,
            0x3c => Self::Reverb,
            0x3d => Self::PhysMesh,
            0x3e => Self::Music,
            0x3f => Self::Tutorial,
            0x40 => Self::BossEncounter,
            0x41 => Self::ControlScheme,
            0x42 => Self::Accolade,
            0x43 => Self::AnimTree,
            0x44 => Self::Vibration,
            0x45 => Self::DungeonFinder,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum D4FolderType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum D4FolderType {
    Child = 0x0,
    Meta = 0x1,
    Payload = 0x2,
    PayLow = 0x3,
    PayMed = 0x4,
}

impl D4FolderType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Child,
            0x1 => Self::Meta,
            0x2 => Self::Payload,
            0x3 => Self::PayLow,
            0x4 => Self::PayMed,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum SnoGroupD4 : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum SnoGroupD4 {
    Unknown = -0x3,
    Code = -0x2,
    None = -0x1,
    Actor = 0x1,
    NPCComponentSet = 0x2,
    AIBehavior = 0x3,
    AIState = 0x4,
    AmbientSound = 0x5,
    Anim = 0x6,
    Anim2D = 0x7,
    AnimSet = 0x8,
    Appearance = 0x9,
    Hero = 0xa,
    Cloth = 0xb,
    Conversation = 0xc,
    ConversationList = 0xd,
    EffectGroup = 0xe,
    Encounter = 0xf,
    Explosion = 0x11,
    FlagSet = 0x12,
    Font = 0x13,
    GameBalance = 0x14,
    Global = 0x15,
    LevelArea = 0x16,
    Light = 0x17,
    MarkerSet = 0x18,
    Observer = 0x1a,
    Particle = 0x1b,
    Physics = 0x1c,
    Power = 0x1d,
    Quest = 0x1f,
    Rope = 0x20,
    Scene = 0x21,
    Script = 0x23,
    ShaderMap = 0x24,
    Shader = 0x25,
    Shake = 0x26,
    SkillKit = 0x27,
    Sound = 0x28,
    StringList = 0x2a,
    Surface = 0x2b,
    Texture = 0x2c,
    Trail = 0x2d,
    UI = 0x2e,
    Weather = 0x2f,
    World = 0x30,
    Recipe = 0x31,
    Condition = 0x33,
    TreasureClass = 0x34,
    Account = 0x35,
    Material = 0x39,
    Lore = 0x3b,
    Reverb = 0x3c,
    Music = 0x3e,
    Tutorial = 0x3f,
    AnimTree = 0x43,
    Vibration = 0x44,
    wWiseSoundBank = 0x47,
    Speaker = 0x48,
    Item = 0x49,
    PlayerClass = 0x4a,
    FogVolume = 0x4c,
    Biome = 0x4d,
    Wall = 0x4e,
    SoundTable = 0x4f,
    Subzone = 0x50,
    MaterialValue = 0x51,
    MonsterFamily = 0x52,
    TileSet = 0x53,
    Population = 0x54,
    MaterialValueSet = 0x55,
    WorldState = 0x56,
    Schedule = 0x57,
    VectorField = 0x58,
    Storyboard = 0x5a,
    Territory = 0x5c,
    AudioContext = 0x5d,
    VOProcess = 0x5e,
    DemonScroll = 0x5f,
    QuestChain = 0x60,
    LoudnessPreset = 0x61,
    ItemType = 0x62,
    Achievement = 0x63,
    Crafter = 0x64,
    HoudiniParticlesSim = 0x65,
    Movie = 0x66,
    TiledStyle = 0x67,
    Affix = 0x68,
    Reputation = 0x69,
    ParagonNode = 0x6a,
    MonsterAffix = 0x6b,
    ParagonBoard = 0x6c,
    SetItemBonus = 0x6d,
    StoreProduct = 0x6e,
    ParagonGlyph = 0x6f,
    ParagonGlyphAffix = 0x70,
    Challenge = 0x72,
    MarkingShape = 0x73,
    ItemRequirement = 0x74,
    Boost = 0x75,
    Emote = 0x76,
    Jewelry = 0x77,
    PlayerTitle = 0x78,
    Emblem = 0x79,
    Dye = 0x7a,
    FogOfWar = 0x7b,
    ParagonThreshold = 0x7c,
    AIAwareness = 0x7d,
    TrackedReward = 0x7e,
    CollisionSettings = 0x7f,
    Aspect = 0x80,
    ABTest = 0x81,
    Stagger = 0x82,
    EyeColor = 0x83,
    Makeup = 0x84,
    MarkingColor = 0x85,
    HairColor = 0x86,
    DungeonAffix = 0x87,
    Activity = 0x88,
    Season = 0x89,
    HairStyle = 0x8a,
    FacialHair = 0x8b,
    Face = 0x8c,
    MercenaryClass = 0x8d,
    PassivePowerContainer = 0x8e,
    MountProfile = 0x8f,
    AICoordinator = 0x90,
    CrafterTab = 0x91,
    TownPortalCosmetic = 0x92,
    AxeTest = 0x93,
    Wizard = 0x94,
    FootstepTable = 0x95,
    Modal = 0x96,
    CollectiblePower = 0x97,
    AppearanceSet = 0x98,
    Preset = 0x99,
    PreviewComposition = 0x9a,
    SpawnPool = 0x9b,
    Unknown_156 = 0x9c,
    BattlePassTier = 0x9d,
    Zone = 0x9e,
    Unknown_159 = 0x9f,
    Unknown_160 = 0xa0,
    Snippet = 0xa1,
    CommunityModifier = 0xa2,
    GenericNodeGraph = 0xa3,
    UserDefinedData = 0xa4,
    Unknown_165 = 0xa5,
    Unknown_166 = 0xa6,
    Unknown_167 = 0xa7,
    Unknown_168 = 0xa8,
    MAX_SNO_GROUPS = 0xa9,
}

impl SnoGroupD4 {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            -0x3 => Self::Unknown,
            -0x2 => Self::Code,
            -0x1 => Self::None,
            0x1 => Self::Actor,
            0x2 => Self::NPCComponentSet,
            0x3 => Self::AIBehavior,
            0x4 => Self::AIState,
            0x5 => Self::AmbientSound,
            0x6 => Self::Anim,
            0x7 => Self::Anim2D,
            0x8 => Self::AnimSet,
            0x9 => Self::Appearance,
            0xa => Self::Hero,
            0xb => Self::Cloth,
            0xc => Self::Conversation,
            0xd => Self::ConversationList,
            0xe => Self::EffectGroup,
            0xf => Self::Encounter,
            0x11 => Self::Explosion,
            0x12 => Self::FlagSet,
            0x13 => Self::Font,
            0x14 => Self::GameBalance,
            0x15 => Self::Global,
            0x16 => Self::LevelArea,
            0x17 => Self::Light,
            0x18 => Self::MarkerSet,
            0x1a => Self::Observer,
            0x1b => Self::Particle,
            0x1c => Self::Physics,
            0x1d => Self::Power,
            0x1f => Self::Quest,
            0x20 => Self::Rope,
            0x21 => Self::Scene,
            0x23 => Self::Script,
            0x24 => Self::ShaderMap,
            0x25 => Self::Shader,
            0x26 => Self::Shake,
            0x27 => Self::SkillKit,
            0x28 => Self::Sound,
            0x2a => Self::StringList,
            0x2b => Self::Surface,
            0x2c => Self::Texture,
            0x2d => Self::Trail,
            0x2e => Self::UI,
            0x2f => Self::Weather,
            0x30 => Self::World,
            0x31 => Self::Recipe,
            0x33 => Self::Condition,
            0x34 => Self::TreasureClass,
            0x35 => Self::Account,
            0x39 => Self::Material,
            0x3b => Self::Lore,
            0x3c => Self::Reverb,
            0x3e => Self::Music,
            0x3f => Self::Tutorial,
            0x43 => Self::AnimTree,
            0x44 => Self::Vibration,
            0x47 => Self::wWiseSoundBank,
            0x48 => Self::Speaker,
            0x49 => Self::Item,
            0x4a => Self::PlayerClass,
            0x4c => Self::FogVolume,
            0x4d => Self::Biome,
            0x4e => Self::Wall,
            0x4f => Self::SoundTable,
            0x50 => Self::Subzone,
            0x51 => Self::MaterialValue,
            0x52 => Self::MonsterFamily,
            0x53 => Self::TileSet,
            0x54 => Self::Population,
            0x55 => Self::MaterialValueSet,
            0x56 => Self::WorldState,
            0x57 => Self::Schedule,
            0x58 => Self::VectorField,
            0x5a => Self::Storyboard,
            0x5c => Self::Territory,
            0x5d => Self::AudioContext,
            0x5e => Self::VOProcess,
            0x5f => Self::DemonScroll,
            0x60 => Self::QuestChain,
            0x61 => Self::LoudnessPreset,
            0x62 => Self::ItemType,
            0x63 => Self::Achievement,
            0x64 => Self::Crafter,
            0x65 => Self::HoudiniParticlesSim,
            0x66 => Self::Movie,
            0x67 => Self::TiledStyle,
            0x68 => Self::Affix,
            0x69 => Self::Reputation,
            0x6a => Self::ParagonNode,
            0x6b => Self::MonsterAffix,
            0x6c => Self::ParagonBoard,
            0x6d => Self::SetItemBonus,
            0x6e => Self::StoreProduct,
            0x6f => Self::ParagonGlyph,
            0x70 => Self::ParagonGlyphAffix,
            0x72 => Self::Challenge,
            0x73 => Self::MarkingShape,
            0x74 => Self::ItemRequirement,
            0x75 => Self::Boost,
            0x76 => Self::Emote,
            0x77 => Self::Jewelry,
            0x78 => Self::PlayerTitle,
            0x79 => Self::Emblem,
            0x7a => Self::Dye,
            0x7b => Self::FogOfWar,
            0x7c => Self::ParagonThreshold,
            0x7d => Self::AIAwareness,
            0x7e => Self::TrackedReward,
            0x7f => Self::CollisionSettings,
            0x80 => Self::Aspect,
            0x81 => Self::ABTest,
            0x82 => Self::Stagger,
            0x83 => Self::EyeColor,
            0x84 => Self::Makeup,
            0x85 => Self::MarkingColor,
            0x86 => Self::HairColor,
            0x87 => Self::DungeonAffix,
            0x88 => Self::Activity,
            0x89 => Self::Season,
            0x8a => Self::HairStyle,
            0x8b => Self::FacialHair,
            0x8c => Self::Face,
            0x8d => Self::MercenaryClass,
            0x8e => Self::PassivePowerContainer,
            0x8f => Self::MountProfile,
            0x90 => Self::AICoordinator,
            0x91 => Self::CrafterTab,
            0x92 => Self::TownPortalCosmetic,
            0x93 => Self::AxeTest,
            0x94 => Self::Wizard,
            0x95 => Self::FootstepTable,
            0x96 => Self::Modal,
            0x97 => Self::CollectiblePower,
            0x98 => Self::AppearanceSet,
            0x99 => Self::Preset,
            0x9a => Self::PreviewComposition,
            0x9b => Self::SpawnPool,
            0x9c => Self::Unknown_156,
            0x9d => Self::BattlePassTier,
            0x9e => Self::Zone,
            0x9f => Self::Unknown_159,
            0xa0 => Self::Unknown_160,
            0xa1 => Self::Snippet,
            0xa2 => Self::CommunityModifier,
            0xa3 => Self::GenericNodeGraph,
            0xa4 => Self::UserDefinedData,
            0xa5 => Self::Unknown_165,
            0xa6 => Self::Unknown_166,
            0xa7 => Self::Unknown_167,
            0xa8 => Self::Unknown_168,
            0xa9 => Self::MAX_SNO_GROUPS,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum CASCSearchPhase : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CASCSearchPhase {
    Initializing = 0x0,
    Searching = 0x2,
    Finished = 0x4,
}

impl CASCSearchPhase {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Initializing,
            0x2 => Self::Searching,
            0x4 => Self::Finished,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum LocaleFlags : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct LocaleFlags: u32 {
        const All = 0xffffffff;
        const None = 0x0;
        const Unk1 = 0x1;
        const enUS = 0x2;
        const koKR = 0x4;
        const Unk8 = 0x8;
        const frFR = 0x10;
        const deDE = 0x20;
        const zhCN = 0x40;
        const esES = 0x80;
        const zhTW = 0x100;
        const enGB = 0x200;
        const enCN = 0x400;
        const enTW = 0x800;
        const esMX = 0x1000;
        const ruRU = 0x2000;
        const ptBR = 0x4000;
        const itIT = 0x8000;
        const ptPT = 0x10000;
        const enSG = 0x1000000;
        const plPL = 0x2000000;
        const jaJP = 0x4000000;
        const trTR = 0x8000000;
        const arSA = 0x10000000;
        const All_WoW = 0x1f3f6;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum ContentFlags : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct ContentFlags: u32 {
        const None = 0x0;
        const HighResTexture = 0x1;
        const F00000002 = 0x2;
        const F00000004 = 0x4;
        const Windows = 0x8;
        const MacOS = 0x10;
        const F00000020 = 0x20;
        const F00000040 = 0x40;
        const Alternate = 0x80;
        const F00000100 = 0x100;
        const F00000800 = 0x800;
        const F00008000 = 0x8000;
        const F00020000 = 0x20000;
        const F00040000 = 0x40000;
        const F00080000 = 0x80000;
        const F00100000 = 0x100000;
        const F00200000 = 0x200000;
        const F00400000 = 0x400000;
        const F00800000 = 0x800000;
        const F02000000 = 0x2000000;
        const F04000000 = 0x4000000;
        const Encrypted = 0x8000000;
        const NoNameHash = 0x10000000;
        const F20000000 = 0x20000000;
        const F40000000 = 0x40000000;
        const NotCompressed = 0x80000000; // sounds have this flag
    }
}
