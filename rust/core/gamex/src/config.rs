// PORT-SOURCE: Core/GameX/_Config.cs
// PORT-SHA: be305f29e6d389d2
// PORT-STATUS: done
//
// The developer's startup selection: which family, game and platform the app
// opens with.
//
// The C# is a 36-branch `#if` / `#elif` chain over family names, with
// `#define Arkane` at the top of the file choosing one. Every branch assigns the
// same `static GlobalOption Option`.
//
// PORTED AS A RUNTIME TABLE, deliberately. Cargo features are additive and not
// mutually exclusive, so 36 `#[cfg(feature = ...)]` blocks assigning one
// static would either fail to compile with two features on or silently pick
// one. A lookup table with a default is the honest translation: the same data,
// selectable without editing source.
//
// The 36 entries below were extracted from the C# by script, not
// transcribed. First attempt got that wrong in a way worth recording: each
// branch carries several **commented-out** `Game = "..."` alternatives, and a
// regex over the raw block reads the last one, so `Arkane` came out as `RF`
// (from a trailing `//Missing: Game = "RF"`) instead of the active `AF`.
// Comments are stripped first now.
//
// Those commented alternatives turned out to be the most useful thing in the
// file — 421 game ids with descriptions, which is the
// closest thing to a game-id registry in the repository. They are preserved in
// [`ALTERNATIVES`] because they resolved an open question elsewhere; see the
// note on the `Height` rename in PORTING.md.
//
// ===================== TWO C#-SIDE OBSERVATIONS ==========================
//
//   1. **`ForceOpen = true` and `ForcePath = "sample:N"` are set in every
//      branch.** These are debug overrides — they make the app open a fixed
//      sample asset on startup — and they are committed enabled, so a release
//      build inherits them. Preserved as data; whether they should default on
//      is a call for you.
//
//   2. **Changing family requires editing source.** `#define Arkane` is the only
//      selector, so switching to another family means recompiling. The table
//      here makes it a runtime lookup, which is the practical reason for the
//      change of shape.

/// C# `FamilyManager.GlobalOption`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct GlobalOption {
    pub platform: Option<&'static str>,
    pub find_key: Option<&'static str>,
    pub family: Option<&'static str>,
    pub game: Option<&'static str>,
    pub edition: Option<&'static str>,
    pub force_path: Option<&'static str>,
    pub force_open: bool,
}

/// The branch `#define Arkane` selects in the committed C#.
pub const DEFAULT_FAMILY: &str = "Arkane";

/// Every `#if` branch, keyed by the family name the C# used as its symbol.
pub static OPTIONS: &[(&str, GlobalOption)] = &[
    ("Arkane", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Arkane"),
        game: Some("AF"),
        edition: None,
        force_path: Some("sample:4"),
        force_open: true,
    }),
    ("Beamdog", GlobalOption {
        platform: Some("UK"),
        find_key: None,
        family: Some("Beamdog"),
        game: Some("MDK2:HD"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Bethesda", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Bethesda"),
        game: Some("Morrowind"),
        edition: None,
        force_path: Some("sample:3"),
        force_open: true,
    }),
    ("Bioware", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Bioware"),
        game: Some("SWTOR"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Black", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Black"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Blizzard", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Blizzard"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Bohemia", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Bohemia"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Bullfrog", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Bullfrog"),
        game: Some("S"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Capcom", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Capcom"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: false,
    }),
    ("Cig", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Cig"),
        game: Some("StarCitizen"),
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Cryptic", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Cryptic"),
        game: Some("CO"),
        edition: None,
        force_path: Some("sample:1"),
        force_open: true,
    }),
    ("Crytek", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Crytek"),
        game: Some("FarCry2"),
        edition: None,
        force_path: Some("sample:1"),
        force_open: true,
    }),
    ("Cyanide", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Cyanide"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("EA", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("EA"),
        game: Some("xx"),
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Epic", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Epic"),
        game: Some("UE1"),
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Firaxis", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Firaxis"),
        game: Some("UE1"),
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Frictional", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Frictional"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Frontier", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Frontier"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Gamebryo", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Gamebryo"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("ID", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("ID"),
        game: Some("Q"),
        edition: None,
        force_path: Some("sample:1"),
        force_open: true,
    }),
    ("IW", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("IW"),
        game: Some("BO4"),
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Lucas", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Lucas"),
        game: Some("MM"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Monolith", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Monolith"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Mythic", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Mythic"),
        game: Some("UO"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Nintendo", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Nintendo"),
        game: Some("Z:TFH"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Origin", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Origin"),
        game: Some("UO"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Red", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Red"),
        game: Some("Witcher"),
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Rockstar", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Rockstar"),
        game: Some("GTA2"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Ubisoft", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Ubisoft"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Uncore", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Uncore"),
        game: Some("APP"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Unity", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Unity"),
        game: None,
        edition: None,
        force_path: Some("sample:*"),
        force_open: true,
    }),
    ("Valve", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Valve"),
        game: Some("HL:Src"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Volition", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Volition"),
        game: Some("D2"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("WB", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("WB"),
        game: Some("AC"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("X2K", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("X2K"),
        game: Some("XX"),
        edition: None,
        force_path: Some("sample:0"),
        force_open: true,
    }),
    ("Xbox", GlobalOption {
        platform: Some("GL"),
        find_key: None,
        family: Some("Xbox"),
        game: Some("StardewValley"),
        edition: None,
        force_path: Some("sample:5"),
        force_open: true,
    }),
];

/// The commented-out `Game = "..."` alternatives from each branch, with the
/// descriptions the C# carries beside them: `(family, id, description)`.
///
/// Not executable configuration — it is the only place in the repository that
/// maps a game id to a human name, so it is worth keeping.
pub static ALTERNATIVES: &[(&str, &str, &str)] = &[
    ("Arkane", "DOM", "Dark Messiah of Might and Magic [source]"),
    ("Arkane", "D", "Dishonored [unreal]"),
    ("Arkane", "D2", "Dishonored 2"),
    ("Arkane", "P", "Prey [cryengine]"),
    ("Arkane", "D:DOTO", "Dishonored: Death of the Outsider"),
    ("Arkane", "W:YB", "Wolfenstein: Youngblood [idTech:6]"),
    ("Arkane", "W:CP", "Wolfenstein: Cyberpilot [idTech:6]"),
    ("Arkane", "DL", "Deathloop"),
    ("Arkane", "RF", "Redfall (future)"),
    ("Beamdog", "P:T", "Baldur's Gate: Enhanced Edition"),
    ("Beamdog", "ID", "Baldur's Gate II: Enhanced Edition"),
    ("Beamdog", "ID:HoW", "Icewind Dale: Heart of Winter"),
    ("Beamdog", "ID2", "Icewind Dale II"),
    ("Beamdog", "BG:DA2", "Baldur's Gate: Dark Alliance II"),
    ("Bethesda", "Oblivion", "The Elder Scrolls IV: Oblivion"),
    ("Bethesda", "Fallout3", "Fallout 3"),
    ("Bethesda", "FalloutNV", "Fallout New Vegas"),
    ("Bethesda", "Skyrim", "The Elder Scrolls V: Skyrim"),
    ("Bethesda", "Fallout4", "Fallout 4"),
    ("Bethesda", "SkyrimSE", "The Elder Scrolls V: Skyrim – Special Edition"),
    ("Bethesda", "Fallout:S", "Fallout Shelter"),
    ("Bethesda", "Fallout4VR", "Fallout 4 VR"),
    ("Bethesda", "SkyrimVR", "The Elder Scrolls V: Skyrim VR"),
    ("Bethesda", "Fallout76", "Fallout 76"),
    ("Bethesda", "Starfield", "Starfield"),
    ("Bethesda", "Oblivion:R", "The Elder Scrolls IV: Oblivion Remastered"),
    ("Bioware", "SS", "Shattered Steel"),
    ("Bioware", "BG", "Baldur's Gate"),
    ("Bioware", "MDK2", "MDK2"),
    ("Bioware", "BG2", "Baldur's Gate II: Shadows of Amn"),
    ("Bioware", "NWN", "Neverwinter Nights"),
    ("Bioware", "KotOR", "Star Wars: Knights of the Old Republic"),
    ("Bioware", "JE", "Jade Empire"),
    ("Bioware", "ME", "Mass Effect"),
    ("Bioware", "NWN2", "Neverwinter Nights 2"),
    ("Bioware", "DA:O", "Dragon Age: Origins"),
    ("Bioware", "ME2", "Mass Effect 2"),
    ("Bioware", "DA2", "Dragon Age II"),
    ("Bioware", "ME3", "Mass Effect 3"),
    ("Bioware", "DA:I", "Dragon Age: Inquisition"),
    ("Bioware", "ME:A", "Mass Effect: Andromeda"),
    ("Bioware", "A", "Anthem"),
    ("Bioware", "ME:LE", "Mass Effect: Legendary Edition"),
    ("Black", "Fallout", "Fallout"),
    ("Black", "Fallout2", "Fallout 2"),
    ("Blizzard", "SC", "StarCraft"),
    ("Blizzard", "D2R", "Diablo II: Resurrected"),
    ("Blizzard", "W3", "Warcraft III: Reign of Chaos"),
    ("Blizzard", "WOW", "World of Warcraft"),
    ("Blizzard", "WOWC", "World of Warcraft: Classic"),
    ("Blizzard", "SC2", "StarCraft II: Wings of Liberty"),
    ("Blizzard", "D3", "Diablo III"),
    ("Blizzard", "HS", "Hearthstone"),
    ("Blizzard", "HOTS", "Heroes of the Storm"),
    ("Blizzard", "DI", "Diablo Immortal"),
    ("Blizzard", "OW2", "Overwatch 2"),
    ("Blizzard", "D4", "Diablo IV"),
    ("Bohemia", "FTaFFIaN", "Fairy Tale about Father Frost, Ivan and Nastya"),
    ("Bohemia", "OF", "Operation Flashpoint: Cold War Crisis"),
    ("Bohemia", "A", "Arma: Armed Assault"),
    ("Bohemia", "A2", "Arma 2"),
    ("Bohemia", "A2:OA", "Arma 2: Operation Arrowhead"),
    ("Bohemia", "TOH", "Take On Helicopters"),
    ("Bohemia", "MM2", "Memento Mori 2: Guardians of Immortality"),
    ("Bohemia", "CC:GM", "Carrier Command: Gaea Mission"),
    ("Bohemia", "AT", "Arma Tactics [unity]"),
    ("Bohemia", "A3", "Arma 3"),
    ("Bohemia", "TOM", "Take On Mars"),
    ("Bohemia", "DZ", "DayZ"),
    ("Bohemia", "V", "Vigor"),
    ("Bohemia", "YL", "Ylands [unity]"),
    ("Bohemia", "AR", "Arma Reforger"),
    ("Bohemia", "SYR", "Someday You'll Return [unreal]"),
    ("Bohemia", "SL", "Silica"),
    ("Bullfrog", "P", "Populous"),
    ("Bullfrog", "P2", "Populous II: Trials of the Olympian Gods"),
    ("Bullfrog", "MC", "Magic Carpet"),
    ("Bullfrog", "TP", "Theme Park"),
    ("Bullfrog", "MC2", "Magic Carpet 2"),
    ("Bullfrog", "S2", "Syndicate Wars"),
    ("Bullfrog", "TH", "Theme Hospital"),
    ("Bullfrog", "DK", "Dungeon Keeper"),
    ("Bullfrog", "P3", "Populous: The Beginning"),
    ("Bullfrog", "DK2", "Dungeon Keeper 2"),
    ("Capcom", "AAI:ME", "Ace Attorney Investigations: Miles Edgeworth"),
    ("Capcom", "AoB", "Age of Booty [console]"),
    ("Capcom", "AJ:AA", "Apollo Justice: Ace Attorney"),
    ("Capcom", "AYSTa5G", "Are You Smarter Than a 5th Grader? 2009 Edition [mobile]"),
    ("Capcom", "RE7:BH", "Resident Evil 7: Biohazard"),
    ("Capcom", "RE7:CV", "Resident Evil - Code: Veronica [console]"),
    ("Capcom", "BC", "Bionic Commando"),
    ("Capcom", "BC:R", "Bionic Commando Rearmed"),
    ("Capcom", "BC:R2", "Bionic Commando Rearmed 2"),
    ("Capcom", "BlackCommand", "Black Command [mobile]"),
    ("Capcom", "BoF4", "Breath of Fire IV"),
    ("Capcom", "CAS2", "Capcom Arcade 2nd Stadium"),
    ("Capcom", "CAS", "Capcom Arcade Stadium"),
    ("Capcom", "CAC", "Capcom Arcade Cabinet [console]"),
    ("Capcom", "BEUB", "Capcom Beat 'Em Up Bundle"),
    ("Capcom", "DV", "Dark Void"),
    ("Capcom", "DV:Z", "Dark Void Zero"),
    ("Capcom", "DR", "Dead Rising"),
    ("Capcom", "DR2", "Dead Rising 2"),
    ("Capcom", "DR2:OtR", "Dead Rising 2: Off the Record"),
    ("Capcom", "DR3", "Dead Rising 3"),
    ("Capcom", "DR4", "Dead Rising 4"),
    ("Capcom", "DR4:FBP", "Dead Rising 4: Frank's Big Package [console]"),
    ("Capcom", "DmC", "Devil May Cry [missing]"),
    ("Capcom", "DmC2", "Devil May Cry 2 [missing]"),
    ("Capcom", "DmC3:DA", "Devil May Cry 3: Dante's Awakening [missing]"),
    ("Capcom", "DmC3:S", "Devil May Cry 3: Special Edition"),
    ("Capcom", "DmC4:S", "Devil May Cry 4: Special Edition"),
    ("Capcom", "DmC5", "Devil May Cry 5"),
    ("Capcom", "DmC5:S", "Devil May Cry 5: Special Edition"),
    ("Capcom", "DmC:HD", "Devil May Cry: HD Collection"),
    ("Capcom", "DmC:X", "DmC: Devil May Cry"),
    ("Capcom", "DD", "Dragon's Dogma"),
    ("Capcom", "DD2", "Dragon's Dogma II"),
    ("Capcom", "DT:R", "DuckTales: Remastered"),
    ("Capcom", "DnD:CoM", "Dungeons & Dragons: Chronicles of Mystara"),
    ("Capcom", "Fighting:C", "[] Capcom Fighting Collection"),
    ("Capcom", "GNG:R", "Ghosts 'n Goblins Resurrection"),
    ("Capcom", "MM:LC", "Mega Man Legacy Collection"),
    ("Capcom", "MM:LC2", "Mega Man Legacy Collection 2"),
    ("Capcom", "MM:XD", "Mega Man X DiVE [Unity]"),
    ("Capcom", "MMZX:LC", "Mega Man Zero/ZX Legacy Collection"),
    ("Capcom", "MHR", "Monster Hunter Rise"),
    ("Capcom", "MH:S2", "Monster Hunter Stories 2: Wings of Ruin"),
    ("Capcom", "PWAA:T", "Phoenix Wright: Ace Attorney Trilogy"),
    ("Capcom", "RDR2", "Red Dead Redemption 2"),
    ("Capcom", "RER", "Resident Evil Resistance"),
    ("Capcom", "RE:RV", "Resident Evil Re:Verse"),
    ("Capcom", "Disney:AC", "The Disney Afternoon Collection"),
    ("Capcom", "TGAA:C", "The Great Ace Attorney Chronicles"),
    ("Capcom", "USF4", "Ultra Street Fighter IV"),
    ("Cryptic", "STO", "Star Trek Online [open, read]"),
    ("Cryptic", "NVW", "Neverwinter [open, read]"),
    ("Crytek", "FarCry", "Far Cry"),
    ("Crytek", "CrysisWarhead", "Crysis Warhead"),
    ("Crytek", "Warface", "Warface: Clutch"),
    ("Crytek", "FarCry3", "Far Cry 3"),
    ("Crytek", "FarCry3:BD", "Far Cry 3 - Blood Dragon"),
    ("Crytek", "Ryse", "Ryse: Son of Rome"),
    ("Crytek", "FarCry4", "Far Cry 4"),
    ("Crytek", "ArcheAge", "ArcheAge"),
    ("Crytek", "MWO", "MechWarrior Online"),
    ("Crytek", "FarCryP", "Far Cry Primal"),
    ("Crytek", "Robinson", "Robinson: The Journey"),
    ("Crytek", "FarCry5", "Far Cry 5"),
    ("Crytek", "Snow", "SNOW - The Ultimate Edition"),
    ("Crytek", "FarCryND", "Far Cry New Dawn"),
    ("Crytek", "Hunt", "Hunt: Showdown 1896"),
    ("Crytek", "Wolcen", "Wolcen: Lords of Mayhem"),
    ("Crytek", "Crysis", "Crysis Remastered"),
    ("Crytek", "Crysis2", "Crysis 2 Remastered"),
    ("Crytek", "Crysis3", "Crysis 3 Remastered"),
    ("Crytek", "FarCry6", "Far Cry 6"),
    ("Cyanide", "Council", "Council"),
    ("Cyanide", "Werewolf:TA", "Werewolf: The Apocalypse - Earthblood"),
    ("Epic", "BioShock", "BioShock"),
    ("Epic", "BioShockR", "BioShock Remastered"),
    ("Epic", "BioShock2", "BioShock 2"),
    ("Epic", "BioShock2R", "BioShock 2 Remastered"),
    ("Epic", "BioShock:Inf", "BioShock Infinite"),
    ("Firaxis", "BioShock", "BioShock"),
    ("Firaxis", "BioShockR", "BioShock Remastered"),
    ("Firaxis", "BioShock2", "BioShock 2"),
    ("Firaxis", "BioShock2R", "BioShock 2 Remastered"),
    ("Firaxis", "BioShock:Inf", "BioShock Infinite"),
    ("Frictional", "P:O", "Penumbra: Overture"),
    ("Frictional", "P:BP", "Penumbra: Black Plague"),
    ("Frictional", "P:R", "Penumbra: Requiem"),
    ("Frictional", "A:TDD", "Amnesia: The Dark Descent"),
    ("Frictional", "A:AMFP", "Amnesia: A Machine for Pigs"),
    ("Frictional", "SOMA", "SOMA"),
    ("Frictional", "A:R", "Amnesia: Rebirth"),
    ("ID", "Q2", "Quake II [25]"),
    ("ID", "Q3", "Quake III Arena [3]"),
    ("ID", "D3", "Doom 3 [4]"),
    ("ID", "Q:L", "Quake Live [3]"),
    ("ID", "R", "Rage [5]"),
    ("ID", "D", "Doom (2016) [6]"),
    ("ID", "D:VFR", "Doom VFR [6]"),
    ("ID", "R2", "Rage 2 [Apex]"),
    ("ID", "D:E", "Doom Eternal [7]"),
    ("ID", "Q:C", "Quake Champions [7]"),
    ("IW", "COD2", "Call of Duty 2 - IWD "),
    ("IW", "COD3", "Call of Duty 3 - XBOX only"),
    ("IW", "COD4", "Call of Duty 4: Modern Warfare - IWD, FF"),
    ("IW", "COD:WaW", "Call of Duty: World at War - IWD, FF"),
    ("IW", "MW2", "Call of Duty: Modern Warfare 2"),
    ("IW", "COD:BO", "Call of Duty: Black Ops - IWD, FF"),
    ("IW", "MW3", "Call of Duty: Call of Duty: Modern Warfare 3"),
    ("IW", "COD:BO2", "Call of Duty: Black Ops 2 - FF"),
    ("IW", "COD:AW", "Call of Duty: Advanced Warfare"),
    ("IW", "COD:BO3", "Call of Duty: Black Ops III - XPAC,FF"),
    ("IW", "MW3", "Call of Duty: Modern Warfare 3"),
    ("IW", "WWII", "Call of Duty: WWII"),
    ("IW", "BOCW", "Call of Duty Black Ops Cold War"),
    ("IW", "Vanguard", "Call of Duty Vanguard"),
    ("Lucas", "PP", "PHM Pegasus"),
    ("Lucas", "SF", "Strike Fleet"),
    ("Lucas", "B1942", "Battlehawks 1942"),
    ("Lucas", "ZMatAM", "Zak McKracken and the Alien Mindbenders - Scumm"),
    ("Lucas", "IJatLC:TAG", "Indiana Jones and the Last Crusade: The Action Game"),
    ("Lucas", "IJatLC", "Indiana Jones and the Last Crusade: The Graphic Adventure"),
    ("Lucas", "TFH", "Their Finest Hour"),
    ("Lucas", "TFM:V1", "Their Finest Missions: Volume One"),
    ("Lucas", "L", "Loom"),
    ("Lucas", "M", "Masterblazer"),
    ("Lucas", "NS", "Night Shift"),
    ("Lucas", "SWotL", "Secret Weapons of the Luftwaffe"),
    ("Lucas", "MI2:LR", "Monkey Island 2: LeChuck's Revenge"),
    ("Lucas", "IJatFoA", "Indiana Jones and the Fate of Atlantis"),
    ("Lucas", "SW:XW", "Star Wars: X-Wing"),
    ("Lucas", "DotT", "Day of the Tentacle - Missing"),
    ("Lucas", "ZAMN", "Zombies Ate My Neighbors"),
    ("Lucas", "SaMHtR", "Sam & Max Hit the Road"),
    ("Lucas", "SWC", "Star Wars Chess"),
    ("Lucas", "SW:TF", "Star Wars: TIE Fighter"),
    ("Lucas", "GP", "Ghoul Patrol"),
    ("Lucas", "SW:DF", "Star Wars: Dark Forces"),
    ("Lucas", "FT", "Full Throttle"),
    ("Lucas", "TD", "The Dig"),
    ("Lucas", "SW:RA2", "Star Wars: Rebel Assault II: The Hidden Empire"),
    ("Lucas", "IJaHDA", "Indiana Jones and His Desktop Adventures"),
    ("Lucas", "A", "Afterlife"),
    ("Lucas", "MatRotM", "Mortimer and the Riddles of the Medallion"),
    ("Lucas", "SW:SotE", "Star Wars: Shadows of the Empire"),
    ("Lucas", "SW:YS", "Star Wars: Yoda Stories"),
    ("Lucas", "O", "Outlaws"),
    ("Lucas", "SW:XvT", "Star Wars: X-Wing vs. TIE Fighter"),
    ("Lucas", "SWJK:DF2", "Star Wars Jedi Knight: Dark Forces II"),
    ("Lucas", "MSW", "Monopoly Star Wars"),
    ("Lucas", "TCoMI", "The Curse of Monkey Island"),
    ("Lucas", "SWJK:MotS", "Star Wars Jedi Knight: Mysteries of the Sith"),
    ("Lucas", "SW:R", "Star Wars: Rebellion"),
    ("Lucas", "SW:BtM", "Star Wars: Behind the Magic"),
    ("Lucas", "SW:DW", "Star Wars: DroidWorks"),
    ("Lucas", "GF", "Grim Fandango"),
    ("Lucas", "SW:RS", "Star Wars: Rogue Squadron"),
    ("Lucas", "SW:XA", "Star Wars: X-Wing Alliance"),
    ("Lucas", "SW1:TPM", "Star Wars Episode I: The Phantom Menace"),
    ("Lucas", "SW1:R", "Star Wars Episode I: Racer"),
    ("Lucas", "SW1:TGF", "Star Wars Episode I: The Gungan Frontier"),
    ("Lucas", "SW:YCAC", "Star Wars: Yoda's Challenge Activity Center"),
    ("Lucas", "SW:PD", "Star Wars: Pit Droids"),
    ("Lucas", "IJatIM", "Indiana Jones and the Infernal Machine"),
    ("Lucas", "SW:FC", "Star Wars: Force Commander"),
    ("Lucas", "EfMI", "Escape from Monkey Island"),
    ("Lucas", "SW:S", "Star Wars: Starfighter"),
    ("Lucas", "SWGB", "Star Wars: Galactic Battlegrounds"),
    ("Lucas", "SWJK2:JO", "Star Wars Jedi Knight II: Jedi Outcast"),
    ("Lucas", "IJatET", "Indiana Jones and the Emperor's Tomb"),
    ("Lucas", "SWG", "Star Wars Galaxies (closed)"),
    ("Lucas", "SW:KotOR", "Star Wars: Knights of the Old Republic"),
    ("Lucas", "SWJK:JA", "Star Wars Jedi Knight: Jedi Academy"),
    ("Lucas", "AaD", "Armed and Dangerous"),
    ("Lucas", "SW:B", "Star Wars: Battlefront"),
    ("Lucas", "SW:KotOR2", "Star Wars Knights of the Old Republic II: The Sith Lord"),
    ("Lucas", "SW:RC", "Star Wars: Republic Commando"),
    ("Lucas", "SW:B2", "Star Wars: Battlefront II"),
    ("Lucas", "SW:EaW", "Star Wars: Empire at War"),
    ("Lucas", "T:OtR", "Thrillville: Off the Rails"),
    ("Lucas", "LSW:TCS", "Lego Star Wars: The Complete Saga"),
    ("Lucas", "LIJ:TOA", "Lego Indiana Jones: The Original Adventures"),
    ("Lucas", "SW:TFU", "Star Wars: The Force Unleashed"),
    ("Lucas", "ToMI", "Tales of Monkey Island"),
    ("Lucas", "TSoMI:SE", "The Secret of Monkey Island: Special Edition"),
    ("Lucas", "SWTCW:RH", "Star Wars: The Clone Wars - Republic Heroes"),
    ("Lucas", "LU", "Lucidity"),
    ("Lucas", "LIJ2:TAC", "Lego Indiana Jones 2: The Adventure Continues"),
    ("Lucas", "MI2SE:LCR", "Monkey Island 2 Special Edition: LeChuck's Revenge"),
    ("Lucas", "SW:TFU2", "Star Wars: The Force Unleashed II"),
    ("Lucas", "LS3:TCW", "Lego Star Wars III: The Clone Wars"),
    ("Lucas", "SW:TOR", "Star Wars: The Old Republic"),
    ("Monolith", "FEAR", "F.E.A.R."),
    ("Monolith", "FEAR:EP", "F.E.A.R.: Extraction Point"),
    ("Monolith", "FEAR:PM", "F.E.A.R.: Perseus Mandate"),
    ("Monolith", "FEAR2", "F.E.A.R. 2: Project Origin"),
    ("Monolith", "FEAR3", "F.E.A.R. 3"),
    ("Mythic", "RM", "Rolemaster: Magestorm"),
    ("Mythic", "AO", "Aliens Online"),
    ("Mythic", "GO", "Godzilla Online"),
    ("Mythic", "DAoC", "Dark Age of Camelot"),
    ("Mythic", "WAR", "Warhammer Online: Age of Reckoning"),
    ("Mythic", "DA2", "Dragon Age II"),
    ("Nintendo", "AC:AF", "Animal Crossing: Amiibo Festival"),
    ("Nintendo", "SFZ", "Star Fox Zero"),
    ("Nintendo", "SFG", "Star Fox Guard"),
    ("Nintendo", "Z:BotW", "The Legend of Zelda: Breath of the Wild"),
    ("Nintendo", "MK8D", "Mario Kart 8 Deluxe"),
    ("Nintendo", "CaptainToad:TT", "Captain Toad: Treasure Tracker"),
    ("Nintendo", "NSMB:UD", "New Super Mario Bros. U Deluxe"),
    ("Nintendo", "Pikmin3D", "Pikmin 3 Deluxe"),
    ("Nintendo", "XX", "XX"),
    ("Nintendo", "XX", "XX"),
    ("Nintendo", "XX", "XX"),
    ("Nintendo", "XX", "XX"),
    ("Nintendo", "XX", "XX"),
    ("Nintendo", "XX", "XX"),
    ("Nintendo", "XX", "XX"),
    ("Nintendo", "XX", "XX"),
    ("Origin", "U8", "Ultima 8"),
    ("Origin", "U9", "Ultima IX"),
    ("Red", "Witcher2", "The Witcher 2"),
    ("Red", "Witcher3", "The Witcher 3: Wild Hunt"),
    ("Red", "CP77", "Cyberpunk 2077"),
    ("Red", "Witcher4", "The Witcher 4 Polaris (future)"),
    ("Rockstar", "GTA", "Grand Theft Auto"),
    ("Rockstar", "MTM64", "Monster Truck Madness 64"),
    ("Rockstar", "CP77", "Cyberpunk 2077"),
    ("Rockstar", "EWJ3D", "Earthworm Jim 3D"),
    ("Rockstar", "TSaD", "Thrasher: Skate and Destroy"),
    ("Rockstar", "EK", "Evel Knievel"),
    ("Rockstar", "AP:OB", "Austin Powers: Oh, Behave!"),
    ("Rockstar", "AP:WtMUL", "Austin Powers: Welcome to My Underground Lair!"),
    ("Rockstar", "MC:SR", "Midnight Club: Street Racing"),
    ("Rockstar", "SR", "Smuggler's Run"),
    ("Rockstar", "SH", "Surfing H3O"),
    ("Rockstar", "ON", "Oni"),
    ("Rockstar", "YDKJ", "You Don't Know Jack"),
    ("Rockstar", "GTA3", "Grand Theft Auto III"),
    ("Rockstar", "SR2", "Smuggler's Run 2: Hostile Territory"),
    ("Rockstar", "MP", "Max Payne"),
    ("Rockstar", "TIJ", "The Italian Job"),
    ("Rockstar", "SoE", "State of Emergency"),
    ("Rockstar", "SR:W", "Smuggler's Run: Warzones"),
    ("Rockstar", "GTA:VC", "Grand Theft Auto: Vice City"),
    ("Rockstar", "MC2", "Midnight Club II"),
    ("Rockstar", "MP2", "Max Payne 2: The Fall of Max Payne"),
    ("Rockstar", "MH", "Manhunt"),
    ("Rockstar", "RDV", "Red Dead Revolver"),
    ("Rockstar", "GTA:A", "Grand Theft Auto Advance"),
    ("Rockstar", "GTA:SA", "Grand Theft Auto: San Andreas"),
    ("Rockstar", "MC3:DE", "Midnight Club 3: DUB Edition"),
    ("Rockstar", "TW", "The Warriors"),
    ("Rockstar", "GTA:LCS", "Grand Theft Auto: Liberty City Stories"),
    ("Rockstar", "MC3:DER", "Midnight Club 3: DUB Edition Remix"),
    ("Rockstar", "RTT", "Rockstar Games Presents Table Tennis"),
    ("Rockstar", "B", "Bully"),
    ("Rockstar", "GTA:VCS", "Grand Theft Auto: Vice City Stories"),
    ("Rockstar", "MH2", "Manhunt 2"),
    ("Rockstar", "B:SE", "Bully: Scholarship Edition"),
    ("Rockstar", "GTA4", "Grand Theft Auto IV"),
    ("Rockstar", "MC:LA", "Midnight Club: Los Angeles"),
    ("Rockstar", "GTA:CW", "Grand Theft Auto: Chinatown Wars"),
    ("Rockstar", "BTR", "Beaterator"),
    ("Rockstar", "RDR", "Red Dead Redemption"),
    ("Rockstar", "LAN", "L.A. Noire"),
    ("Rockstar", "MP3", "Max Payne 3"),
    ("Rockstar", "GTA5", "Grand Theft Auto V"),
    ("Rockstar", "GTAO", "Grand Theft Auto Online"),
    ("Rockstar", "LAN:VR", "L.A. Noire: The VR Case Files"),
    ("Rockstar", "RDR2", "Red Dead Redemption 2"),
    ("Rockstar", "RDO", "Red Dead Online"),
    ("Ubisoft", "XX", "xx"),
    ("Unity", "AmongUs", "Among Us"),
    ("Unity", "Cities", "Cities: Skylines"),
    ("Unity", "Tabletop", "Tabletop Simulator"),
    ("Unity", "UBoat", "Destroyer: The U-Boat Hunter"),
    ("Unity", "7D2D", "7 Days to Die"),
    ("Valve", "HL", "Half-Life"),
    ("Valve", "TF", "Team Fortress Classic"),
    ("Valve", "HL:OF", "Half-Life: Opposing Force"),
    ("Valve", "Ricochet", "Ricochet"),
    ("Valve", "CS", "Counter-Strike"),
    ("Valve", "DM", "Deathmatch Classic"),
    ("Valve", "HL:BS", "Half-Life: Blue Shift"),
    ("Valve", "DOD", "Day of Defeat"),
    ("Valve", "CS:CZ", "Counter-Strike: Condition Zero"),
    ("Valve", "CS:Src", "Counter-Strike: Source"),
    ("Valve", "HL2", "Half-Life 2"),
    ("Valve", "HL2:DM", "Half-Life 2: Deathmatch"),
    ("Valve", "DOD:Src", "Day of Defeat: Source"),
    ("Valve", "HL2:LC", "Half-Life 2: Lost Coast"),
    ("Valve", "HL:DM:Src", "Half-Life Deathmatch: Source"),
    ("Valve", "HL2:E1", "Half-Life 2: Episode One"),
    ("Valve", "HL2:E2", "Half-Life 2: Episode Two"),
    ("Valve", "Portal", "Portal"),
    ("Valve", "TF2", "Team Fortress 2"),
    ("Valve", "L4D", "Left 4 Dead"),
    ("Valve", "L4D2", "Left 4 Dead 2"),
    ("Valve", "AlienSwarm", "Alien Swarm"),
    ("Valve", "Portal2", "Portal 2"),
    ("Valve", "CS2", "Counter-Strike: Global Offensive"),
    ("Valve", "D2", "Dota 2"),
    ("Valve", "CS:NZ", "Counter-Strike Nexon"),
    ("Valve", "TheLab:RR", "The Lab: Robot Repair"),
    ("Valve", "TheLab:SS", "The Lab: Secret Shop [!unity]"),
    ("Valve", "TheLab:TL", "The Lab: The Lab [!unity]"),
    ("Valve", "Artifact", "Artifact Classic"),
    ("Valve", "DU", "Dota Underlords"),
    ("Valve", "HL:A", "Half-Life: Alyx"),
    ("Valve", "ArtifactF", "Artifact Foundry"),
    ("Valve", "Aperture:DJ", "Aperture Desk Job"),
    ("Valve", "CodenameGordon", "Codename Gordon"),
    ("Valve", "GarrysMod", "Garry's Mod"),
    ("Valve", "Aperture:HL", "Aperture Hand Lab [!unity]"),
    ("Volition", "D", "Descent"),
    ("Volition", "FS", "Descent: FreeSpace - The Great War"),
    ("Volition", "FS2", "FreeSpace 2"),
    ("Volition", "S", "Summoner"),
    ("Volition", "RF", "Red Faction"),
    ("Volition", "S2", "Summoner 2 [missing]"),
    ("Volition", "RF2", "Red Faction II"),
    ("Volition", "TP", "The Punisher [missing]"),
    ("Volition", "SR06", "Saints Row [missing]"),
    ("Volition", "SR2", "Saints Row 2"),
    ("Volition", "RF:G", "Red Faction: Guerrilla"),
    ("Volition", "RF:A", "Red Faction: Armageddon"),
    ("Volition", "SR3", "Saints Row: The Third"),
    ("Volition", "SR4", "Saints Row IV"),
    ("Volition", "D3", "Saints Row 2"),
    ("Volition", "SR:G", "Descent 3"),
    ("Volition", "AoM", "Agents of Mayhem"),
    ("Volition", "RF:GR", "Red Faction: Guerrilla Re-Mars-tered"),
    ("Volition", "SR", "Saints Row"),
    ("Xbox", "AxiomVerge", "Axiom Verge"),
    ("Xbox", "Celeste", "Celeste"),
    ("Xbox", "AxiomVerge2", "Axiom Verge 2"),
];

/// C# `FamilyManager.Option` — the selected option.
pub fn option() -> GlobalOption {
    for (k, v) in OPTIONS {
        if *k == DEFAULT_FAMILY {
            return *v;
        }
    }
    GlobalOption::default()
}

/// Select a different branch by name, which the C# can only do by recompiling.
pub fn option_for(family: &str) -> Option<GlobalOption> {
    OPTIONS.iter().find(|(k, _)| *k == family).map(|(_, v)| *v)
}

/// Known game ids for a family, from the commented alternatives plus the
/// active one.
pub fn known_games(family: &str) -> Vec<(&'static str, &'static str)> {
    let mut v: Vec<(&str, &str)> = ALTERNATIVES
        .iter()
        .filter(|(f, _, _)| *f == family)
        .map(|(_, g, d)| (*g, *d))
        .collect();
    if let Some(o) = option_for(family) {
        if let Some(g) = o.game {
            v.insert(0, (g, "(active in the committed config)"));
        }
    }
    v
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn every_branch_was_extracted() {
        assert_eq!(OPTIONS.len(), 36);
    }

    #[test]
    fn the_active_branch_is_the_one_the_define_selects() {
        assert_eq!(DEFAULT_FAMILY, "Arkane");
        let o = option();
        assert_eq!(o.family, Some("Arkane"));
    }

    #[test]
    fn the_active_game_is_read_from_the_uncommented_line() {
        // A regex over the raw block reads the last `Game =`, which is a
        // commented-out `RF`. The active value is `AF`.
        assert_eq!(option().game, Some("AF"));
        assert_ne!(option().game, Some("RF"));
    }

    #[test]
    fn debug_overrides_are_present_as_the_c_sharp_has_them() {
        let o = option();
        assert!(o.force_open, "committed enabled");
        assert_eq!(o.force_path, Some("sample:4"));
    }

    #[test]
    fn every_branch_names_its_family() {
        for (k, v) in OPTIONS {
            assert_eq!(v.family, Some(*k), "branch {k} disagrees with its symbol");
        }
    }

    #[test]
    fn other_families_are_selectable_without_recompiling() {
        let b = option_for("Bethesda").expect("Bethesda branch");
        assert_eq!(b.family, Some("Bethesda"));
        assert!(option_for("NotAFamily").is_none());
    }

    #[test]
    fn the_alternatives_registry_survived_extraction() {
        assert_eq!(ALTERNATIVES.len(), 421);
        // The ids that resolved the rename question elsewhere.
        let arkane: Vec<&str> = ALTERNATIVES.iter()
            .filter(|(f, _, _)| *f == "Arkane").map(|(_, g, _)| *g).collect();
        assert!(arkane.contains(&"D"), "Dishonored");
        assert!(arkane.contains(&"D:DOTO"), "Death of the Outsider");
        assert!(arkane.contains(&"W:YB"), "Wolfenstein: Youngblood");
        assert!(arkane.contains(&"W:CP"), "Wolfenstein: Cyberpilot");
    }

    #[test]
    fn known_games_puts_the_active_id_first() {
        let g = known_games("Arkane");
        assert_eq!(g[0].0, "AF");
        assert!(g.len() > 1);
    }

    #[test]
    fn no_alternative_id_looks_like_a_renamed_symbol() {
        // `Radius` and `Height` appear as game ids in TestHelper.cs but are not
        // ids at all - this registry is the evidence.
        for (_, gid, _) in ALTERNATIVES {
            assert!(!gid.contains("Radius"), "{gid}");
            assert!(!gid.contains("Height"), "{gid}");
        }
    }
}
