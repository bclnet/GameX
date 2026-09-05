// PORT-SOURCE: Families/GameX.Xbox/Formats/StardewValley/GameData.cs
// PORT-SHA: ffbb7e4129ef367f
// PORT-STATUS: done
// PORT-GENERATED: gen_enums.py — do not hand-edit; regenerate instead.
//
// 29 enum(s), 108 members, generated from the C#
// rather than transcribed. At this size a one-digit typo would mis-identify a
// game asset in a way no test would obviously catch.
//
// C# allows duplicate discriminants within an enum; Rust does not. Where that
// happens the first member becomes the variant and the rest become associated
// consts pointing at it, so every C# name still resolves.
//
// Per enum:
//   Gender                             i32   enum          3 members
//   Season                             i32   enum          4 members
//   MusicContext                       i32   enum          7 members
//   PlantableResult                    i32   enum          3 members
//   PlantableRuleContext               i32   enum          3 members
//   QuantityModifierModificationType   i32   enum          5 members
//   QuantityModifierQuantityModifierMode i32   enum          3 members
//   BuildingsBuildingChestType         i32   enum          3 members
//   CharactersCalendarBehavior         i32   enum          3 members
//   CharactersEndSlideShowBehavior     i32   enum          3 members
//   CharactersNpcAge                   i32   enum          3 members
//   CharactersNpcLanguage              i32   enum          2 members
//   CharactersNpcManner                i32   enum          3 members
//   CharactersNpcOptimism              i32   enum          3 members
//   CharactersNpcSocialAnxiety         i32   enum          3 members
//   CharactersSocialTabBehavior        i32   enum          4 members
//   CropsHarvestMethod                 i32   enum          2 members
//   FarmAnimalsFarmAnimalGender        i32   enum          3 members
//   FarmAnimalsFarmAnimalHarvestType   i32   enum          3 members
//   FloorsAndPathsFloorPathConnectType i32   enum          4 members
//   FloorsAndPathsFloorPathShadowType  i32   enum          3 members
//   MachinesMachineOutputTrigger       i32   enum          5 members
//   MachinesMachineTimeBlockers        i32   enum          9 members
//   PetsPetAnimationLoopMode           i32   enum          3 members
//   ShopsLimitedStockMode              i32   enum          3 members
//   ShopsShopOwnerType                 i32   enum          4 members
//   ShopsStackSizeVisibility           i32   enum          3 members
//   SpecialOrdersQuestDuration         i32   enum          6 members
//   WildTreesWildTreeGrowthStage       i32   enum          5 members

#![allow(non_camel_case_types, non_upper_case_globals)]

/// A character's gender identity.
/// C# `enum Gender : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Gender {
    Male = 0x0,
    Female = 0x1,
    Undefined = 0x2,
}

impl Gender {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Male,
            0x1 => Self::Female,
            0x2 => Self::Undefined,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// A season of the year.
/// C# `enum Season : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Season {
    Spring = 0x0,
    Summer = 0x1,
    Fall = 0x2,
    Winter = 0x3,
}

impl Season {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Spring,
            0x1 => Self::Summer,
            0x2 => Self::Fall,
            0x3 => Self::Winter,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum MusicContext : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum MusicContext {
    Default = 0x0,
    SubLocation = 0x1,
    MusicPlayer = 0x2,
    Event = 0x3,
    MiniGame = 0x4,
    ImportantSplitScreenMusic = 0x5,
    MAX = 0x6,
}

impl MusicContext {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Default,
            0x1 => Self::SubLocation,
            0x2 => Self::MusicPlayer,
            0x3 => Self::Event,
            0x4 => Self::MiniGame,
            0x5 => Self::ImportantSplitScreenMusic,
            0x6 => Self::MAX,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// Indicates when a seed/sapling can be planted in a location.
/// C# `enum PlantableResult : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PlantableResult {
    Default = 0x0,
    Allow = 0x1,
    Deny = 0x2,
}

impl PlantableResult {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Default,
            0x1 => Self::Allow,
            0x2 => Self::Deny,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// As part of <see cref="T:StardewValley.GameData.PlantableRule" />, indicates which cases the rule applies to.
/// C# `enum PlantableRuleContext : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PlantableRuleContext {
    Ground = 0x1,
    GardenPot = 0x2,
    Any = 0x3,
}

impl PlantableRuleContext {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x1 => Self::Ground,
            0x2 => Self::GardenPot,
            0x3 => Self::Any,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// The type of change to apply for a <see cref="T:StardewValley.GameData.QuantityModifier" />.
/// C# `enum QuantityModifierModificationType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum QuantityModifierModificationType {
    Add = 0x0,
    Subtract = 0x1,
    Multiply = 0x2,
    Divide = 0x3,
    Set = 0x4,
}

impl QuantityModifierModificationType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Add,
            0x1 => Self::Subtract,
            0x2 => Self::Multiply,
            0x3 => Self::Divide,
            0x4 => Self::Set,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// Indicates how multiple quantity modifiers are combined.
/// C# `enum QuantityModifierQuantityModifierMode : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum QuantityModifierQuantityModifierMode {
    Stack = 0x0,
    Minimum = 0x1,
    Maximum = 0x2,
}

impl QuantityModifierQuantityModifierMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Stack,
            0x1 => Self::Minimum,
            0x2 => Self::Maximum,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// The inventory type for a building chest.
/// C# `enum BuildingsBuildingChestType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BuildingsBuildingChestType {
    Chest = 0x0,
    Collect = 0x1,
    Load = 0x2,
}

impl BuildingsBuildingChestType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Chest,
            0x1 => Self::Collect,
            0x2 => Self::Load,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// How an NPC's birthday is shown on the calendar.
/// C# `enum CharactersCalendarBehavior : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersCalendarBehavior {
    AlwaysShown = 0x0,
    HiddenUntilMet = 0x1,
    HiddenAlways = 0x2,
}

impl CharactersCalendarBehavior {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::AlwaysShown,
            0x1 => Self::HiddenUntilMet,
            0x2 => Self::HiddenAlways,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// How an NPC appears in the end-game perfection slide show.
/// C# `enum CharactersEndSlideShowBehavior : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersEndSlideShowBehavior {
    Hidden = 0x0,
    MainGroup = 0x1,
    TrailingGroup = 0x2,
}

impl CharactersEndSlideShowBehavior {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Hidden,
            0x1 => Self::MainGroup,
            0x2 => Self::TrailingGroup,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// The general age of an NPC.
/// C# `enum CharactersNpcAge : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersNpcAge {
    Adult = 0x0,
    Teen = 0x1,
    Child = 0x2,
}

impl CharactersNpcAge {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Adult,
            0x1 => Self::Teen,
            0x2 => Self::Child,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// The language spoken by an NPC.
/// C# `enum CharactersNpcLanguage : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersNpcLanguage {
    Default = 0x0,
    Dwarvish = 0x1,
}

impl CharactersNpcLanguage {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Default,
            0x1 => Self::Dwarvish,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// A measure of a character's general politeness.
/// C# `enum CharactersNpcManner : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersNpcManner {
    Neutral = 0x0,
    Polite = 0x1,
    Rude = 0x2,
}

impl CharactersNpcManner {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Neutral,
            0x1 => Self::Polite,
            0x2 => Self::Rude,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// A measure of a character's overall optimism.
/// C# `enum CharactersNpcOptimism : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersNpcOptimism {
    Positive = 0x0,
    Negative = 0x1,
    Neutral = 0x2,
}

impl CharactersNpcOptimism {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Positive,
            0x1 => Self::Negative,
            0x2 => Self::Neutral,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// A measure of a character's comfort with social situations.
/// C# `enum CharactersNpcSocialAnxiety : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersNpcSocialAnxiety {
    Outgoing = 0x0,
    Shy = 0x1,
    Neutral = 0x2,
}

impl CharactersNpcSocialAnxiety {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Outgoing,
            0x1 => Self::Shy,
            0x2 => Self::Neutral,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// How an NPC is shown on the social tab when unlocked.
/// C# `enum CharactersSocialTabBehavior : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CharactersSocialTabBehavior {
    UnknownUntilMet = 0x0,
    AlwaysShown = 0x1,
    HiddenUntilMet = 0x2,
    HiddenAlways = 0x3,
}

impl CharactersSocialTabBehavior {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::UnknownUntilMet,
            0x1 => Self::AlwaysShown,
            0x2 => Self::HiddenUntilMet,
            0x3 => Self::HiddenAlways,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// Indicates how a crop can be harvested.
/// C# `enum CropsHarvestMethod : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CropsHarvestMethod {
    Grab = 0x0,
    Scythe = 0x1,
}

impl CropsHarvestMethod {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Grab,
            0x1 => Self::Scythe,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// The default gender for a farm animal type.
/// C# `enum FarmAnimalsFarmAnimalGender : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FarmAnimalsFarmAnimalGender {
    Female = 0x0,
    Male = 0x1,
    MaleOrFemale = 0x2,
}

impl FarmAnimalsFarmAnimalGender {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Female,
            0x1 => Self::Male,
            0x2 => Self::MaleOrFemale,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// How produced items are collected from an animal.
/// C# `enum FarmAnimalsFarmAnimalHarvestType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FarmAnimalsFarmAnimalHarvestType {
    DropOvernight = 0x0,
    HarvestWithTool = 0x1,
    DigUp = 0x2,
}

impl FarmAnimalsFarmAnimalHarvestType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::DropOvernight,
            0x1 => Self::HarvestWithTool,
            0x2 => Self::DigUp,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// When drawing adjacent flooring items across multiple tiles, how the flooring sprite for each tile is selected.
/// C# `enum FloorsAndPathsFloorPathConnectType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FloorsAndPathsFloorPathConnectType {
    Default = 0x0,
    Path = 0x1,
    CornerDecorated = 0x2,
    Random = 0x3,
}

impl FloorsAndPathsFloorPathConnectType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Default,
            0x1 => Self::Path,
            0x2 => Self::CornerDecorated,
            0x3 => Self::Random,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// How the shadow under a floor or path tile sprite should be drawn.
/// C# `enum FloorsAndPathsFloorPathShadowType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FloorsAndPathsFloorPathShadowType {
    None = 0x0,
    Square = 0x1,
    Contoured = 0x2,
}

impl FloorsAndPathsFloorPathShadowType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::None,
            0x1 => Self::Square,
            0x2 => Self::Contoured,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, indicates when a machine should start producing output.
/// C# `enum MachinesMachineOutputTrigger : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum MachinesMachineOutputTrigger {
    None = 0x0,
    ItemPlacedInMachine = 0x1,
    OutputCollected = 0x2,
    MachinePutDown = 0x4,
    DayUpdate = 0x8,
}

impl MachinesMachineOutputTrigger {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::None,
            0x1 => Self::ItemPlacedInMachine,
            0x2 => Self::OutputCollected,
            0x4 => Self::MachinePutDown,
            0x8 => Self::DayUpdate,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// As part of a <see cref="T:StardewValley.GameData.Machines.MachineTimeBlockers" />, indicates when the machine should be paused.
/// C# `enum MachinesMachineTimeBlockers : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum MachinesMachineTimeBlockers {
    Outside = 0x0,
    Inside = 0x1,
    Spring = 0x2,
    Summer = 0x3,
    Fall = 0x4,
    Winter = 0x5,
    Sun = 0x6,
    Rain = 0x7,
    Always = 0x8,
}

impl MachinesMachineTimeBlockers {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Outside,
            0x1 => Self::Inside,
            0x2 => Self::Spring,
            0x3 => Self::Summer,
            0x4 => Self::Fall,
            0x5 => Self::Winter,
            0x6 => Self::Sun,
            0x7 => Self::Rain,
            0x8 => Self::Always,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// As part of <see cref="T:StardewValley.GameData.Pets.PetBehavior" />, what to do when the last animation frame is reached while the behavior is still active.
/// C# `enum PetsPetAnimationLoopMode : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PetsPetAnimationLoopMode {
    None = 0x0,
    Loop = 0x1,
    Hold = 0x2,
}

impl PetsPetAnimationLoopMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::None,
            0x1 => Self::Loop,
            0x2 => Self::Hold,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// How a shop stock limit is applied in multiplayer.
/// C# `enum ShopsLimitedStockMode : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ShopsLimitedStockMode {
    Global = 0x0,
    Player = 0x1,
    None = 0x2,
}

impl ShopsLimitedStockMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Global,
            0x1 => Self::Player,
            0x2 => Self::None,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// Specifies how a shop owner entry matches NPCs.
/// C# `enum ShopsShopOwnerType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ShopsShopOwnerType {
    NamedNpc = 0x0,
    Any = 0x1,
    AnyOrNone = 0x2,
    None = 0x3,
}

impl ShopsShopOwnerType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::NamedNpc,
            0x1 => Self::Any,
            0x2 => Self::AnyOrNone,
            0x3 => Self::None,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// How to draw stack size numbers in the shop list.
/// C# `enum ShopsStackSizeVisibility : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ShopsStackSizeVisibility {
    Hide = 0x0,
    Show = 0x1,
    ShowIfMultiple = 0x2,
}

impl ShopsStackSizeVisibility {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Hide,
            0x1 => Self::Show,
            0x2 => Self::ShowIfMultiple,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// The period for which a special order is valid.
/// C# `enum SpecialOrdersQuestDuration : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum SpecialOrdersQuestDuration {
    Week = 0x0,
    Month = 0x1,
    TwoWeeks = 0x2,
    TwoDays = 0x3,
    ThreeDays = 0x4,
    OneDay = 0x5,
}

impl SpecialOrdersQuestDuration {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Week,
            0x1 => Self::Month,
            0x2 => Self::TwoWeeks,
            0x3 => Self::TwoDays,
            0x4 => Self::ThreeDays,
            0x5 => Self::OneDay,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// The growth state for a tree.
/// <remarks>These mainly exist to make content edits more readable. Most code should use the constants like <c>Tree.seedStage</c>, which have the same values.</remarks>
/// C# `enum WildTreesWildTreeGrowthStage : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum WildTreesWildTreeGrowthStage {
    Seed = 0x0,
    Sprout = 0x1,
    Sapling = 0x2,
    Bush = 0x3,
    Tree = 0x5,
}

impl WildTreesWildTreeGrowthStage {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Seed,
            0x1 => Self::Sprout,
            0x2 => Self::Sapling,
            0x3 => Self::Bush,
            0x5 => Self::Tree,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}
