GameX
===

GameX is an open-source, cross-platform solution for delivering game assets as a service.

### GameX Benefits:
* Portable (windows, apple, linux, mobile, intel, arm)
* Loads textures, models, animations, sounds, and levels
* Avaliable with streaming assets (cached)
* References assets with a uniform resource location (url)
* Loaders for OpenGL, Unreal, Unity and Vulken
* Locates installed games
* Includes a desktop app to explore assets
* Includes a command line interface to export assets (list, unpack, shred)
* *future:* Usage tracking (think Spotify)

### Components:
1. Context - the interface for interacting with this service
2. Family - the grouping of games by a shared aspect
3. Platform - endpoints for using game assets like unity, unreal, etc
4. Application - a collection of application to interact with


## [Applications](docs/Applications/Readme.md)
Multiple applications are included in GameX to make it easier to work with the game assets.

The following are the current applications:

| ID                                               | Name
| --                                               | --  
| [Command Line Interface](docs/Applications/Command%20Line%20Interface/Readme.md)| A CLI tool.
| [Explorer](docs/Applications/Explorer/Readme.md)                   | An application explorer.
| [Unity Plugin](docs/Applications/Unity%20Plugin/Readme.md)         | A Unity plugin.
| [Unreal Plugin](docs/Applications/Unreal%20Plugin/Readme.md)       | A Unreal plugin.

## [Context](docs/Context/Readme.md)
Context provides the interface for interacting with this service

* Resource - a uri formated resource with a path and game component
* Family - represents a family of games by a shared aspect
* FamilyGame - represents a single game
* FamilyManager - a static interface for the service
* FamilyPlatform - represents the current platform
* PakFile - represents a games collection of assets


### Loading an asset:
1. service locates all installed games
2. (*optional*) initiate a game platform: `UnityPlatform.Startup()`
3. get a family reference: `var family = FamilyManager.GetFamily("ID")`
4. open a game specific archive file: `var pakFile = family.OpenPakFile("game:/Archive#ID")`
5. load a game specific asset: `var obj = await pakFile.LoadFileObjectAsync<object>("Path");`
6. service parses game objects for the specifed resource: textures, models, levels, etc
7. service adapts the game objects to the current platform: unity, unreal, etc
8. platform now contains the specified game asset
9. additionally the service provides a collection of applications


## [Families](docs/Families/Readme.md)
Families are the primary grouping mechanism for interacting with the asset services.

Usually file formats center around the game developer or game engine being used, and are modified, instead of replaced, as the studio releases new versions.

The following are the current familes:

| ID                                               | Name                      | Sample Game       | Status
| --                                               | --                        | --                | --
| [Arkane](docs/Families/Arkane/Readme.md)    | Arkane Studios            | Dishonored 2      | In Development
| [Bethesda](docs/Families/Bethesda/Readme.md)| The Elder Scrolls         | Skyrim            | In Development
| [Bioware](docs/Families/Bioware/Readme.md)  | Bioware                   | Neverwinter Nights| In Development
| [Black](docs/Families/Black/Readme.md)      | Black Isle Studios        | Fallout 2         | In Development
| [Blizzard](docs/Families/Blizzard/Readme.md)| Blizzard                  | StarCraft         | In Development
| [Capcom](docs/Families/Capcom/Readme.md)    | Capcom                    | Resident Evil     | In Development
| [Cig](docs/Families/Cig/Readme.md)          | Cloud Imperium Games      | Star Citizen      | In Development
| [Cryptic](docs/Families/Cryptic/Readme.md)  | Cryptic                   | Star Trek Online  | In Development
| [Crytek](docs/Families/Cry/Readme.md)       | Crytek                    | MechWarrior Online| In Development
| [Cyanide](docs/Families/Cyanide/Readme.md)  | Cyanide Formats           | The Council       | In Development
| [Epic](docs/Families/Epic/Readme.md)        | Epic                      | BioShock          | In Development
| [Frictional](docs/Families/Frictional/Readme.md)| Frictional Games      | SOMA              | In Development
| [Frontier](docs/Families/Frontier/Readme.md)| Frontier Developments     | Elite: Dangerous  | In Development
| [Id](docs/Families/Id/Readme.md)            | id Software               | Doom              | In Development
| [IW](docs/Families/IW/Readme.md)            | Infinity Ward             | Call of Duty      | In Development
| [Monolith](docs/Families/Monolith/Readme.md)| Monolith                  | F.E.A.R.          | In Development
| [Origin](docs/Families/Origin/Readme.md)    | Origin Systems            | Ultima Online     | In Development
| [Red](docs/Families/Red/Readme.md)          | REDengine                 | The Witcher 3: Wild Hunt | In Development
| [Unity](docs/Families/Unity/Readme.md)      | Unity                     | AmongUs           | In Development
| [Unknown](docs/Families/Unknown/Readme.md)  | Unknown                   | N/A               | In Development
| [Valve](docs/Families/Valve/Readme.md)      | Valve                     | Dota 2            | In Development
| [WbB](docs/Families/WbB/Readme.md)          | Asheron's Call            | Asheron's Call    | In Development

## [Platforms](docs/Platforms/Readme.md)
Platforms provide the interface to each gaming platform.

## Games
---
The following are the current games:

| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
| **Arkane** | **Arkane Studios**
| [AF](https://www.gog.com/en/game/arx_fatalis) | Arx Fatalis | open | read | gl -- -- | -- -- -- | -- -- --
| [DOM](https://store.steampowered.com/app/2100) | Dark Messiah of Might and Magic | open | read | -- -- -- | -- -- -- | -- -- --
| [KS]() | KarmaStar | - | - | -- -- -- | -- -- -- | -- -- --
| [D](https://www.gog.com/en/game/dishonored_definitive_edition) | Dishonored | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](https://www.gog.com/index.php/game/dishonored_2) | Dishonored 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [P](https://www.gog.com/en/game/prey) | Prey | open | read | -- -- -- | -- -- -- | -- -- --
| [D:DOTO](https://www.gog.com/en/game/dishonored_death_of_the_outsider) | Dishonored: Death of the Outsider | - | - | -- -- -- | -- -- -- | -- -- --
| [W:YB](https://store.steampowered.com/app/1056960) | Wolfenstein: Youngblood | - | - | -- -- -- | -- -- -- | -- -- --
| [W:CP](https://store.steampowered.com/app/1056970) | Wolfenstein: Cyberpilot | - | - | -- -- -- | -- -- -- | -- -- --
| [DL](https://store.steampowered.com/app/1252330) | Deathloop | - | - | -- -- -- | -- -- -- | -- -- --
| [RF](https://bethesda.net/en/game/redfall) | Redfall (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Beamdog** | **Beamdog**
| [MDK2:W](https://en.wikipedia.org/wiki/MDK2#Wii_port_and_MDK2_HD) | MDK2 (wii) | - | - | -- -- -- | -- -- -- | -- -- --
| [MDK2:HD](https://en.wikipedia.org/wiki/MDK2#Wii_port_and_MDK2_HD) | MDK2 HD | - | - | -- -- -- | -- -- -- | -- -- --
| [P:T](https://en.wikipedia.org/wiki/Planescape:_Torment) | Baldur's Gate: Enhanced Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ID](https://en.wikipedia.org/wiki/Icewind_Dale) | Baldur's Gate II: Enhanced Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ID:HoW](https://en.wikipedia.org/wiki/Icewind_Dale:_Heart_of_Winter) | Icewind Dale: Heart of Winter | - | - | -- -- -- | -- -- -- | -- -- --
| [ID2](https://en.wikipedia.org/wiki/Icewind_Dale_II) | Icewind Dale II | - | - | -- -- -- | -- -- -- | -- -- --
| [BG:DA2](https://en.wikipedia.org/wiki/Baldur%27s_Gate:_Dark_Alliance_II) | Baldur's Gate: Dark Alliance II | - | - | -- -- -- | -- -- -- | -- -- --
| **Bethesda** | **Bethesda Game Studios**
| [Morrowind](https://store.steampowered.com/app/22320/The_Elder_Scrolls_III_Morrowind_Game_of_the_Year_Edition/) | The Elder Scrolls III: Morrowind | - | - | -- -- -- | -- -- -- | -- -- --
| [IHRA](https://en.wikipedia.org/wiki/IHRA_Drag_Racing) | IHRA Professional Drag Racing 2005 | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion](https://store.steampowered.com/app/22330/The_Elder_Scrolls_IV_Oblivion_Game_of_the_Year_Edition/) | The Elder Scrolls IV: Oblivion | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout3](https://store.steampowered.com/app/22370/Fallout_3_Game_of_the_Year_Edition/) | Fallout 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [FalloutNV](https://store.steampowered.com/app/22380/Fallout_New_Vegas/) | Fallout New Vegas | - | - | -- -- -- | -- -- -- | -- -- --
| [Skyrim](https://store.steampowered.com/app/72850/The_Elder_Scrolls_V_Skyrim/) | The Elder Scrolls V: Skyrim | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4](https://store.steampowered.com/app/377160/Fallout_4/) | Fallout 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimSE](https://store.steampowered.com/app/489830/The_Elder_Scrolls_V_Skyrim_Special_Edition/) | The Elder Scrolls V: Skyrim - Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout:S](https://store.steampowered.com/app/588430/Fallout_Shelter/) | Fallout Shelter | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4VR](https://store.steampowered.com/app/611660/Fallout_4_VR/) | Fallout 4 VR | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimVR](https://store.steampowered.com/app/611670/The_Elder_Scrolls_V_Skyrim_VR/) | The Elder Scrolls V: Skyrim VR | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout76](https://store.steampowered.com/app/1151340/Fallout_76/) | Fallout 76 | - | - | -- -- -- | -- -- -- | -- -- --
| [TES:B](https://elderscrolls.bethesda.net/en/blades) | The Elder Scrolls: Blades | - | - | -- -- -- | -- -- -- | -- -- --
| [Starfield](https://store.steampowered.com/app/1716740/Starfield/) | Starfield | - | - | -- -- -- | -- -- -- | -- -- --
| [TES:C](https://en.wikipedia.org/wiki/The_Elder_Scrolls:_Castles) | The Elder Scrolls: Castles | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion:R](https://store.steampowered.com/app/2623190/The_Elder_Scrolls_IV_Oblivion_Remastered/) | The Elder Scrolls IV: Oblivion Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [Tes6](https://en.wikipedia.org/wiki/The_Elder_Scrolls) | The Elder Scrolls VI (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout5](https://en.wikipedia.org/wiki/Fallout_(series)) | Fallout 5 (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Bioware** | **BioWare**
| [SS](https://www.gog.com/en/game/shattered_steel) | Shattered Steel | - | - | -- -- -- | -- -- -- | -- -- --
| [BG](https://www.gog.com/en/game/baldurs_gate_enhanced_edition) | Baldur's Gate | - | - | -- -- -- | -- -- -- | -- -- --
| [MDK2](https://www.gog.com/en/game/mdk_2) | MDK2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BG2](https://www.gog.com/en/game/baldurs_gate_2_enhanced_edition) | Baldur's Gate II: Shadows of Amn | - | - | -- -- -- | -- -- -- | -- -- --
| [NWN](https://store.steampowered.com/app/704450) | Neverwinter Nights | - | - | -- -- -- | -- -- -- | -- -- --
| [KotOR](https://store.steampowered.com/app/32370) | Star Wars: Knights of the Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [JE](https://www.gog.com/en/game/jade_empire_special_edition) | Jade Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [ME](https://store.steampowered.com/app/17460) | Mass Effect | - | - | -- -- -- | -- -- -- | -- -- --
| [NWN2](https://www.gog.com/en/game/neverwinter_nights_2_complete) | Neverwinter Nights 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SC:TDB](https://en.wikipedia.org/wiki/Sonic_Chronicles:_The_Dark_Brotherhood) | Sonic Chronicles: The Dark Brotherhood | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:G](https://en.wikipedia.org/wiki/Mass_Effect_Galaxy) | Mass Effect Galaxy | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:O](https://store.steampowered.com/app/47810) | Dragon Age: Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [ME2](https://store.steampowered.com/app/24980) | Mass Effect 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DA2](https://en.wikipedia.org/wiki/Dragon_Age_II) | Dragon Age II | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:L](https://en.wikipedia.org/wiki/Dragon_Age_Legends) | Dragon Age Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTOR](https://store.steampowered.com/app/1286830) | Star Wars: The Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [ME3](https://store.steampowered.com/app/1238020) | Mass Effect 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [WO](https://en.wikipedia.org/wiki/Warhammer_Online:_Wrath_of_Heroes) | Warhammer Online: Wrath of Heroes (Cancelled) | - | - | -- -- -- | -- -- -- | -- -- --
| [CC](https://en.wikipedia.org/wiki/Warhammer_Online:_Wrath_of_Heroes) | Command & Conquer: Generals 2 (Cancelled) | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:I](https://store.steampowered.com/app/1222690) | Dragon Age: Inquisition | - | - | -- -- -- | -- -- -- | -- -- --
| [SR](https://en.wikipedia.org/wiki/Shadow_Realms) | Shadow Realms (Cancelled) | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:A](https://store.steampowered.com/app/1238000) | Mass Effect: Andromeda | - | - | -- -- -- | -- -- -- | -- -- --
| [A](https://www.ea.com/games/anthem/buy/pc) | Anthem | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:LE](https://store.steampowered.com/app/1328670) | Mass Effect: Legendary Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:D](https://www.ea.com/en-gb/games/dragon-age/dragon-age-dreadwolf) | Dragon Age: Dreadwolf (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [ME5](https://en.wikipedia.org/wiki/Mass_Effect) | Mass Effect 5 (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Black** | **Black Isle Studios**
| [Fallout](https://store.steampowered.com/app/38400) | Fallout | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout2](https://store.steampowered.com/app/38410) | Fallout 2 | - | - | -- -- -- | -- -- -- | -- -- --
| **Blizzard** | **Blizzard Entertainment**
| [TDAROS]() | The Death and Return of Superman | - | - | -- -- -- | -- -- -- | -- -- --
| [B]() | Blackthorne | - | - | -- -- -- | -- -- -- | -- -- --
| [W1]() | Warcraft: Orcs & Humans | - | - | -- -- -- | -- -- -- | -- -- --
| [JLTF]() | Justice League Task Force | - | - | -- -- -- | -- -- -- | -- -- --
| [W2]() | Warcraft II: Tides of Darkness | - | - | -- -- -- | -- -- -- | -- -- --
| [D1]() | Diablo | - | - | -- -- -- | -- -- -- | -- -- --
| [TLV2]() | The Lost Vikings 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SC](https://us.shop.battle.net/en-us/product/starcraft) | StarCraft | - | - | -- -- -- | -- -- -- | -- -- --
| [D2]() | Diablo II | - | - | -- -- -- | -- -- -- | -- -- --
| [D2R](https://us.shop.battle.net/en-us/product/diablo_ii_resurrected) | Diablo II: Resurrected | - | - | -- -- -- | -- -- -- | -- -- --
| [W3](https://us.shop.battle.net/en-us/product/warcraft-iii-reforged) | Warcraft III: Reign of Chaos | - | - | -- -- -- | -- -- -- | -- -- --
| [WOW](https://us.shop.battle.net/en-us/family/world-of-warcraft) | World of Warcraft | - | - | -- -- -- | -- -- -- | -- -- --
| [WOWC](https://us.shop.battle.net/en-us/family/world-of-warcraft-classic) | World of Warcraft: Classic | - | - | -- -- -- | -- -- -- | -- -- --
| [SC2](https://us.shop.battle.net/en-us/product/starcraft-ii) | StarCraft II: Wings of Liberty | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://us.shop.battle.net/en-us/product/diablo-iii) | Diablo III | - | - | -- -- -- | -- -- -- | -- -- --
| [HOTS](https://us.shop.battle.net/en-us/family/heroes-of-the-storm) | Heroes of the Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [HS](https://us.shop.battle.net/en-us/family/hearthstone) | Hearthstone | - | - | -- -- -- | -- -- -- | -- -- --
| [OW](https://us.shop.battle.net/en-us/family/overwatch) | Overwatch | - | - | -- -- -- | -- -- -- | -- -- --
| [CB](https://us.shop.battle.net/en-us/family/crash-bandicoot-4) | Crash Bandicoot™ 4: It's About Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DI](https://diabloimmortal.blizzard.com/en-us/) | Diablo Immortal | - | - | -- -- -- | -- -- -- | -- -- --
| [OW2](https://us.shop.battle.net/en-us/product/overwatch) | Overwatch 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D4](https://diablo4.blizzard.com/en-us/) | Diablo IV | - | - | -- -- -- | -- -- -- | -- -- --
| [Other]() | Other | - | - | -- -- -- | -- -- -- | -- -- --
| **Bohemia** | **Bohemia Interactive**
| [FTaFFIaN](https://en.wikipedia.org/wiki/Fairy_Tale_about_Father_Frost,_Ivan_and_Nastya) | Fairy Tale about Father Frost, Ivan and Nastya | - | - | -- -- -- | -- -- -- | -- -- --
| [MoLI](https://en.wikipedia.org/wiki/Missing_on_Lost_Island) | Missing on Lost Island | - | - | -- -- -- | -- -- -- | -- -- --
| [OF](https://en.wikipedia.org/wiki/Operation_Flashpoint:_Cold_War_Crisis) | Operation Flashpoint: Cold War Crisis | - | - | -- -- -- | -- -- -- | -- -- --
| [A](https://en.wikipedia.org/wiki/Arma:_Armed_Assault) | Arma: Armed Assault | - | - | -- -- -- | -- -- -- | -- -- --
| [MM](https://en.wikipedia.org/wiki/Memento_Mori_(video_game)) | Memento Mori | - | - | -- -- -- | -- -- -- | -- -- --
| [A2](https://en.wikipedia.org/wiki/Arma_2) | Arma 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PM](https://en.wikipedia.org/wiki/Pat_%26_Mat) | Pat & Mat | - | - | -- -- -- | -- -- -- | -- -- --
| [A2:OA](https://en.wikipedia.org/wiki/Arma_2:_Operation_Arrowhead) | Arma 2: Operation Arrowhead | - | - | -- -- -- | -- -- -- | -- -- --
| [Alternativa](https://en.wikipedia.org/wiki/Alternativa_(video_game)) | Alternativa | - | - | -- -- -- | -- -- -- | -- -- --
| [TOH](https://en.wikipedia.org/wiki/Take_On_Helicopters) | Take On Helicopters | - | - | -- -- -- | -- -- -- | -- -- --
| [MM2](https://store.steampowered.com/app/237970/Memento_Mori_2/) | Memento Mori 2: Guardians of Immortality | - | - | -- -- -- | -- -- -- | -- -- --
| [CC:GM](https://en.wikipedia.org/wiki/Carrier_Command:_Gaea_Mission) | Carrier Command: Gaea Mission | - | - | -- -- -- | -- -- -- | -- -- --
| [AT](https://en.wikipedia.org/wiki/Arma_Tactics) | Arma Tactics | - | - | -- -- -- | -- -- -- | -- -- --
| [A3](https://en.wikipedia.org/wiki/Arma_3) | Arma 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [AMO]() | Arma Mobile Ops | - | - | -- -- -- | -- -- -- | -- -- --
| [TOM](https://en.wikipedia.org/wiki/Take_On_Mars) | Take On Mars | - | - | -- -- -- | -- -- -- | -- -- --
| [DZ:M]() | Mini DayZ | - | - | -- -- -- | -- -- -- | -- -- --
| [AG](https://en.wikipedia.org/wiki/Argo_(video_game)) | Argo | - | - | -- -- -- | -- -- -- | -- -- --
| [DZ](https://en.wikipedia.org/wiki/DayZ_(video_game)) | DayZ | - | - | -- -- -- | -- -- -- | -- -- --
| [V](https://en.wikipedia.org/wiki/Vigor_(video_game)) | Vigor | - | - | -- -- -- | -- -- -- | -- -- --
| [YL](https://en.wikipedia.org/wiki/Ylands) | Ylands | - | - | -- -- -- | -- -- -- | -- -- --
| [DZ:M2]() | Mini DayZ 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [AR](https://en.wikipedia.org/wiki/Arma_(series)#Arma_Reforger) | Arma Reforger | - | - | -- -- -- | -- -- -- | -- -- --
| [SYR](https://en.wikipedia.org/wiki/Someday_You%27ll_Return) | Someday You'll Return | - | - | -- -- -- | -- -- -- | -- -- --
| [SL](https://en.wikipedia.org/wiki/Silica_(video_game)) | Silica | - | - | -- -- -- | -- -- -- | -- -- --
| [A4](https://en.wikipedia.org/wiki/Arma_(series)#Arma_4) | Arma 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [BH]() | Brute Horse | - | - | -- -- -- | -- -- -- | -- -- --
| [SV]() | Skyverse | - | - | -- -- -- | -- -- -- | -- -- --
| **Bullfrog** | **Bullfrog**
| [Fusion](https://en.wikipedia.org/wiki/Fusion_(video_game)) | Fusion | - | - | -- -- -- | -- -- -- | -- -- --
| [P](https://en.wikipedia.org/wiki/Populous_(video_game)) | Populous | - | - | -- -- -- | -- -- -- | -- -- --
| [Flood]() | Flood | - | - | -- -- -- | -- -- -- | -- -- --
| [PM](https://en.wikipedia.org/wiki/Powermonger) | Powermonger | - | - | -- -- -- | -- -- -- | -- -- --
| [BF](https://www.gog.com/index.php/game/dishonored_2) | Bullfrogger | - | - | -- -- -- | -- -- -- | -- -- --
| [P2](https://en.wikipedia.org/wiki/Populous_II:_Trials_of_the_Olympian_Gods) | Populous II: Trials of the Olympian Gods | - | - | -- -- -- | -- -- -- | -- -- --
| [PS](https://www.gog.com/en/game/dishonored_death_of_the_outsider) | Psycho Santa | - | - | -- -- -- | -- -- -- | -- -- --
| [S](https://en.wikipedia.org/wiki/Syndicate_(1993_video_game)) | Syndicate | - | - | -- -- -- | -- -- -- | -- -- --
| [MC](https://en.wikipedia.org/wiki/Magic_Carpet_(video_game)) | Magic Carpet | - | - | -- -- -- | -- -- -- | -- -- --
| [TP](https://en.wikipedia.org/wiki/Theme_Park_(video_game)) | Theme Park | - | - | -- -- -- | -- -- -- | -- -- --
| [Tube]() | Tube | - | - | -- -- -- | -- -- -- | -- -- --
| [HO](https://en.wikipedia.org/wiki/Hi-Octane) | Hi-Octane | - | - | -- -- -- | -- -- -- | -- -- --
| [MC2](https://en.wikipedia.org/wiki/Magic_Carpet_2) | Magic Carpet 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [GW](https://en.wikipedia.org/wiki/Genewars) | Genewars | - | - | -- -- -- | -- -- -- | -- -- --
| [S2](https://en.wikipedia.org/wiki/Syndicate_Wars) | Syndicate Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [TH](https://en.wikipedia.org/wiki/Theme_Hospital) | Theme Hospital | - | - | -- -- -- | -- -- -- | -- -- --
| [DK](https://en.wikipedia.org/wiki/Dungeon_Keeper) | Dungeon Keeper | - | - | -- -- -- | -- -- -- | -- -- --
| [P3](https://en.wikipedia.org/wiki/Populous:_The_Beginning) | Populous: The Beginning | - | - | -- -- -- | -- -- -- | -- -- --
| [TA](https://en.wikipedia.org/wiki/Theme_Aquarium) | Theme Aquarium | - | - | -- -- -- | -- -- -- | -- -- --
| [DK2](https://en.wikipedia.org/wiki/Dungeon_Keeper_2) | Dungeon Keeper 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TP:W](https://en.wikipedia.org/wiki/Theme_Park_World) | Theme Park World | - | - | -- -- -- | -- -- -- | -- -- --
| [TP:I](https://en.wikipedia.org/wiki/Theme_Park_Inc) | Theme Park Inc. | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom** | **Capcom**
| [CAS](https://store.steampowered.com/app/1515950) | Capcom Arcade Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [Fighting:C](https://store.steampowered.com/app/1685750) | Capcom Fighting Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [GnG:R](https://store.steampowered.com/app/1375400) | Ghosts 'n Goblins Resurrection | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:LC](https://store.steampowered.com/app/363440) | Mega Man Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:LC2](https://store.steampowered.com/app/495050) | Mega Man Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:XD](https://store.steampowered.com/app/1582620) | Mega Man X DiVE | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZX:LC](https://store.steampowered.com/app/999020) | Mega Man Zero/ZX Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MHR](https://store.steampowered.com/app/1446780) | Monster Hunter Rise | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:S2](https://store.steampowered.com/app/1277400) | Monster Hunter Stories 2: Wings of Ruin | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:T](https://store.steampowered.com/app/787480) | Phoenix Wright: Ace Attorney Trilogy | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR2](https://store.steampowered.com/app/1174180) | Red Dead Redemption 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RER](https://store.steampowered.com/app/952070) | Resident Evil Resistance | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:RV](https://store.steampowered.com/app/1236300) | Resident Evil Re:Verse | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:AC](https://store.steampowered.com/app/525040) | The Disney Afternoon Collection | open | read | gl -- -- | -- -- -- | -- -- --
| [TGAA:C](https://store.steampowered.com/app/1158850) | The Great Ace Attorney Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [USF4](https://store.steampowered.com/app/45760) | Ultra Street Fighter IV | - | - | -- -- -- | -- -- -- | -- -- --
| [1941:CA](https://en.wikipedia.org/wiki/1941:_Counter_Attack) | 1941: Counter Attack | - | - | -- -- -- | -- -- -- | -- -- --
| [1942](https://en.wikipedia.org/wiki/1942_(video_game)) | 1942 | - | - | -- -- -- | -- -- -- | -- -- --
| [1942:FS](https://en.wikipedia.org/wiki/1942_(video_game)) | 1942: First Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [1942:JS](https://en.wikipedia.org/wiki/1942:_Joint_Strike) | 1942: Joint Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [1943:TBoM](https://en.wikipedia.org/wiki/1943:_The_Battle_of_Midway) | 1943: The Battle of Midway | - | - | -- -- -- | -- -- -- | -- -- --
| [1944:TLM](https://en.wikipedia.org/wiki/1944:_The_Loop_Master) | 1944: The Loop Master | - | - | -- -- -- | -- -- -- | -- -- --
| [19XX:TWAD](https://en.wikipedia.org/wiki/19XX:_The_War_Against_Destiny) | 19XX: The War Against Destiny | - | - | -- -- -- | -- -- -- | -- -- --
| [AAI:ME](https://store.steampowered.com/app/2401970/Ace_Attorney_Investigations_Collection/) | Ace Attorney Investigations: Miles Edgeworth | - | - | -- -- -- | -- -- -- | -- -- --
| [AQ:CW]() | Adventure Quiz: Capcom World | - | - | -- -- -- | -- -- -- | -- -- --
| [AQ:CW2]() | Adventure Quiz: Capcom World 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [AQ2:H]() | Adventure Quiz 2: Hatena? no Daibouken | - | - | -- -- -- | -- -- -- | -- -- --
| [AITMK](https://en.wikipedia.org/wiki/Adventures_in_the_Magic_Kingdom) | Adventures in the Magic Kingdom | - | - | -- -- -- | -- -- -- | -- -- --
| [AoB](https://www.xbox.com/en-US/games/store/age-of-booty/BZ46NPQNX334/0001) | Age of Booty | - | - | -- -- -- | -- -- -- | -- -- --
| [Airborne]() | Airborne | - | - | -- -- -- | -- -- -- | -- -- --
| [AVP](https://en.wikipedia.org/wiki/Alien_vs._Predator_(arcade_game)) | Alien vs. Predator | - | - | -- -- -- | -- -- -- | -- -- --
| [AJ:AA](https://store.steampowered.com/app/2187220/Apollo_Justice_Ace_Attorney_Trilogy/) | Apollo Justice: Ace Attorney | - | - | -- -- -- | -- -- -- | -- -- --
| [AYSTa5G](https://play.google.com/store/apps/details?id=dh3games.areyousmarterthana5thgrader&hl=en_US) | Are You Smarter Than a 5th Grader? 2009 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ArmoredWarriors](https://en.wikipedia.org/wiki/Armored_Warriors) | Armored Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [AANM:IT](https://en.wikipedia.org/wiki/Arthur_to_Astaroth_no_Nazomakaimura:_Incredible_Toons) | Arthur & Astrot NazoMakaimura: Incredible Toons | - | - | -- -- -- | -- -- -- | -- -- --
| [ANJ2:TASR]() | Ashita no Joe 2: The Anime Super Remix | - | - | -- -- -- | -- -- -- | -- -- --
| [AsuraWrath](https://en.wikipedia.org/wiki/Asura%27s_Wrath) | Asura's Wrath | - | - | -- -- -- | -- -- -- | -- -- --
| [Ataxx](https://en.wikipedia.org/wiki/Ataxx) | Ataxx | - | - | -- -- -- | -- -- -- | -- -- --
| [AM](https://en.wikipedia.org/wiki/Auto_Modellista) | Auto Modellista | - | - | -- -- -- | -- -- -- | -- -- --
| [Avengers](https://en.wikipedia.org/wiki/Avengers_(1987_video_game)) | Avengers | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleCircuit](https://en.wikipedia.org/wiki/Battle_Circuit) | Battle Circuit | - | - | -- -- -- | -- -- -- | -- -- --
| [BD:FoV](https://en.wikipedia.org/wiki/Beat_Down:_Fists_of_Vengeance) | Beat Down: Fists of Vengeance | - | - | -- -- -- | -- -- -- | -- -- --
| [BigBangBar]() | Big Bang Bar | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7:BH](https://store.steampowered.com/app/418370/Resident_Evil_7_Biohazard/) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:CV](https://www.xbox.com/en-US/games/store/resident-evil-code-veronica-x/BQW8J8XM62JW) | Resident Evil - Code: Veronica | - | - | -- -- -- | -- -- -- | -- -- --
| [BC:1980](https://en.wikipedia.org/wiki/Bionic_Commando_(1987_video_game)) | Bionic Commando (1980) | - | - | -- -- -- | -- -- -- | -- -- --
| [BC](https://store.steampowered.com/app/21670/Bionic_Commando/) | Bionic Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [BC:R](https://store.steampowered.com/app/21680/Bionic_Commando_Rearmed/) | Bionic Commando Rearmed | - | - | -- -- -- | -- -- -- | -- -- --
| [BC:R2](https://www.xbox.com/en-US/games/store/bc-rearmed-2/C34MG6M35D3S) | Bionic Commando Rearmed 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BlackTiger](https://en.wikipedia.org/wiki/Black_Tiger_(video_game)) | Black Tiger | - | - | -- -- -- | -- -- -- | -- -- --
| [BlockBlock](https://en.wikipedia.org/wiki/Capcom_Classics_Collection) | Block Block | - | - | -- -- -- | -- -- -- | -- -- --
| [BlackCommand](https://black-command.en.uptodown.com/android) | Black Command | - | - | -- -- -- | -- -- -- | -- -- --
| [Bombastic](https://en.wikipedia.org/wiki/Bombastic_(video_game)) | Bombastic | - | - | -- -- -- | -- -- -- | -- -- --
| [BombLink]() | BombLink | - | - | -- -- -- | -- -- -- | -- -- --
| [Bonkers](https://en.wikipedia.org/wiki/Bonkers_(SNES_video_game)) | Bonkers | - | - | -- -- -- | -- -- -- | -- -- --
| [Bounty Hunter Sara]() | Bounty Hunter Sara: Holy Mountain no Teiou | - | - | -- -- -- | -- -- -- | -- -- --
| [BreakShot](https://en.wikipedia.org/wiki/Break_Shot) | BreakShot | - | - | -- -- -- | -- -- -- | -- -- --
| [BoF](https://en.wikipedia.org/wiki/Breath_of_Fire_(video_game)) | Breath of Fire | - | - | -- -- -- | -- -- -- | -- -- --
| [BoF2](https://en.wikipedia.org/wiki/Breath_of_Fire_II) | Breath of Fire II | - | - | -- -- -- | -- -- -- | -- -- --
| [BoF3](https://en.wikipedia.org/wiki/Breath_of_Fire_III) | Breath of Fire III | - | - | -- -- -- | -- -- -- | -- -- --
| [BoF4](https://www.myabandonware.com/game/breath-of-fire-iv-bgw) | Breath of Fire IV | - | - | -- -- -- | -- -- -- | -- -- --
| [BoF6](https://en.wikipedia.org/wiki/Breath_of_Fire_6) | Breath of Fire 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [BoF:DQ](https://en.wikipedia.org/wiki/Breath_of_Fire:_Dragon_Quarter) | Breath of Fire: Dragon Quarter | - | - | -- -- -- | -- -- -- | -- -- --
| [BusterBros](https://en.wikipedia.org/wiki/Buster_Bros.) | Buster Bros. | - | - | -- -- -- | -- -- -- | -- -- --
| [BusterBros:C](https://en.wikipedia.org/wiki/Buster_Bros._Collection) | Buster Bros. Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Cabal](https://en.wikipedia.org/wiki/Cabal_(video_game)) | Cabal | - | - | -- -- -- | -- -- -- | -- -- --
| [Cadillacs+Dinosaurs](https://en.wikipedia.org/wiki/Cadillacs_and_Dinosaurs_(video_game)) | Cadillacs and Dinosaurs | - | - | -- -- -- | -- -- -- | -- -- --
| [CannonSpike](https://en.wikipedia.org/wiki/Cannon_Spike) | Cannon Spike | - | - | -- -- -- | -- -- -- | -- -- --
| [CAS2](https://store.steampowered.com/app/1755910/Capcom_Arcade_2nd_Stadium/) | Capcom Arcade 2nd Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [CAC](https://www.xbox.com/en-US/games/store/capcom-arcade-cabinet/C26KM7D5BG4S/0001) | Capcom Arcade Cabinet | - | - | -- -- -- | -- -- -- | -- -- --
| [CAH2]() | Capcom Arcade Hits Volume 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Baseball:SGO]() | Capcom Baseball - Suketto Gaijin Oo-Abare | - | - | -- -- -- | -- -- -- | -- -- --
| [BEUB](https://store.steampowered.com/app/885150/Capcom_Beat_Em_Up_Bundle/) | Capcom Beat 'Em Up Bundle | - | - | -- -- -- | -- -- -- | -- -- --
| [Bowling](https://en.wikipedia.org/wiki/Capcom_Bowling) | Capcom Bowling | - | - | -- -- -- | -- -- -- | -- -- --
| [Classics](https://en.wikipedia.org/wiki/Capcom_Classics_Collection) | Capcom Classics Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Classics:RL](https://en.wikipedia.org/wiki/Capcom_Classics_Collection#Capcom_Classics_Collection_Reloaded) | Capcom Classics Collection Reloaded | - | - | -- -- -- | -- -- -- | -- -- --
| [Classics:RM](https://en.wikipedia.org/wiki/Capcom_Classics_Collection#Capcom_Classics_Collection_Remixed) | Capcom Classics Collection Remixed | - | - | -- -- -- | -- -- -- | -- -- --
| [Classics:V2](https://en.wikipedia.org/wiki/Capcom_Classics_Collection) | Capcom Classics Collection Vol. 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Classics:MM](https://en.wikipedia.org/wiki/Capcom_Classics_Collection#Capcom_Classics_Mini_Mix) | Capcom Classics Mini-Mix | - | - | -- -- -- | -- -- -- | -- -- --
| [FightingEvolution](https://en.wikipedia.org/wiki/Capcom_Fighting_Evolution) | Capcom Fighting Evolution | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen1](https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_1:_Wings_of_Destiny) | Capcom Generation 1 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen2](https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_2:_Chronicles_of_Arthur) | Capcom Generation 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen3](https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_3:_The_First_Generation) | Capcom Generation 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen4](https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_4:_Blazing_Guns) | Capcom Generation 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen5](https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_5:_Street_Fighter_Collection_2) | Capcom Generation 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [Golf]() | Capcom Golf | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:T]() | Capcom no Quiz: Tonosama no Yabou | - | - | -- -- -- | -- -- -- | -- -- --
| [Puzzle:World](https://en.wikipedia.org/wiki/Capcom_Puzzle_World) | Capcom Puzzle World | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:H]() | Capcom Quiz: Hatena? no Daibouken | - | - | -- -- -- | -- -- -- | -- -- --
| [SportsClub]() | Capcom Sports Club | - | - | -- -- -- | -- -- -- | -- -- --
| [TaisenFanDisk]() | Capcom Taisen Fan Disc | - | - | -- -- -- | -- -- -- | -- -- --
| [VS2](https://en.wikipedia.org/wiki/Capcom_vs._SNK_2) | Capcom vs. SNK 2: Mark of the Millennium | - | - | -- -- -- | -- -- -- | -- -- --
| [VS](https://en.wikipedia.org/wiki/Capcom_vs._SNK:_Millennium_Fight_2000#Versions) | Capcom vs. SNK: Millennium Fight 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [Football:MVP](https://en.wikipedia.org/wiki/Capcom%27s_MVP_Football) | Capcom's MVP Football | - | - | -- -- -- | -- -- -- | -- -- --
| [Soccer:Shootout]() | Capcom's Soccer Shootout | - | - | -- -- -- | -- -- -- | -- -- --
| [CaptainCommando](https://en.wikipedia.org/wiki/Captain_Commando) | Captain Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [CarrierAirWing](https://en.wikipedia.org/wiki/Carrier_Air_Wing_(video_game)) | Carrier Air Wing | - | - | -- -- -- | -- -- -- | -- -- --
| [CashCab](https://en.wikipedia.org/wiki/Cash_Cab_(British_game_show)) | Cash Cab | - | - | -- -- -- | -- -- -- | -- -- --
| [Catan](https://en.wikipedia.org/wiki/Catan) | Catan | - | - | -- -- -- | -- -- -- | -- -- --
| [ChaosLegion](https://en.wikipedia.org/wiki/Chaos_Legion) | Chaos Legion | - | - | -- -- -- | -- -- -- | -- -- --
| [CTHCC](https://en.wikipedia.org/wiki/Cherry_Tree_High_Comedy_Club) | Cherry Tree High Comedy Club | - | - | -- -- -- | -- -- -- | -- -- --
| [CnD:RR](https://en.wikipedia.org/wiki/Chip_%27n_Dale_Rescue_Rangers_(video_game)) | Chip 'n Dale Rescue Rangers | - | - | -- -- -- | -- -- -- | -- -- --
| [CnD:RR2](https://en.wikipedia.org/wiki/Chip_%27n_Dale_Rescue_Rangers_2) | Chip 'n Dale Rescue Rangers 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Choko]() | Choko | - | - | -- -- -- | -- -- -- | -- -- --
| [CT3](https://en.wikipedia.org/wiki/Clock_Tower_3) | Clock Tower 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [CN:V](https://en.wikipedia.org/wiki/Code_Name:_Viper) | Code Name: Viper | - | - | -- -- -- | -- -- -- | -- -- --
| [Commando](https://en.wikipedia.org/wiki/Commando_(video_game)) | Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [CrimsonTears](https://en.wikipedia.org/wiki/Crimson_Tears) | Crimson Tears | - | - | -- -- -- | -- -- -- | -- -- --
| [CriticalBullet]() | Critical Bullet: 7th Target | - | - | -- -- -- | -- -- -- | -- -- --
| [Cyberbots](https://en.wikipedia.org/wiki/Cyberbots:_Full_Metal_Madness) | Cyberbots: Full Metal Madness | - | - | -- -- -- | -- -- -- | -- -- --
| [DS:TNW](https://en.wikipedia.org/wiki/Darkstalkers:_The_Night_Warriors) | Darkstalkers: The Night Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [DS3](https://en.wikipedia.org/wiki/Darkstalkers_3) | Darkstalkers 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DS:TCT](https://en.wikipedia.org/wiki/Darkstalkers_Chronicle:_The_Chaos_Tower) | Darkstalkers Chronicle: The Chaos Tower | - | - | -- -- -- | -- -- -- | -- -- --
| [DS:R](https://en.wikipedia.org/wiki/Darkstalkers_Resurrection) | Darkstalkers Resurrection | - | - | -- -- -- | -- -- -- | -- -- --
| [DV](https://store.steampowered.com/app/45710/Dark_Void/) | Dark Void | - | - | -- -- -- | -- -- -- | -- -- --
| [DV:Z](https://store.steampowered.com/app/45730/Dark_Void_Zero/) | Dark Void Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [Darkwatch](https://en.wikipedia.org/wiki/Darkwatch) | Darkwatch | - | - | -- -- -- | -- -- -- | -- -- --
| [DarkwingDuck](https://en.wikipedia.org/wiki/Darkwing_Duck_(Capcom_video_game)) | Darkwing Duck | - | - | -- -- -- | -- -- -- | -- -- --
| [DeadPhoenix](https://en.wikipedia.org/wiki/Capcom_Five#Dead_Phoenix) | Dead Phoenix (Canceled) | - | - | -- -- -- | -- -- -- | -- -- --
| [DR](https://store.steampowered.com/app/427190/DEAD_RISING/) | Dead Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2](https://store.steampowered.com/app/45740/Dead_Rising_2/) | Dead Rising 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2:OtR](https://store.steampowered.com/app/45770/Dead_Rising_2_Off_the_Record/) | Dead Rising 2: Off the Record | - | - | -- -- -- | -- -- -- | -- -- --
| [DR3](https://store.steampowered.com/app/265550/Dead_Rising_3_Apocalypse_Edition/) | Dead Rising 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4](https://store.steampowered.com/app/543460/Dead_Rising_4/) | Dead Rising 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4:FBP](https://store.playstation.com/en-us/product/UP0102-CUSA08540_00-DEADRISING4BUNDL) | Dead Rising 4: Frank's Big Package | - | - | -- -- -- | -- -- -- | -- -- --
| [DR:CTYD](https://en.wikipedia.org/wiki/Dead_Rising:_Chop_Till_You_Drop) | Dead Rising: Chop Till You Drop | - | - | -- -- -- | -- -- -- | -- -- --
| [DeepDown](https://en.wikipedia.org/wiki/Deep_Down_(video_game)) | Deep Down | - | - | -- -- -- | -- -- -- | -- -- --
| [DemonsCrest](https://en.wikipedia.org/wiki/Demon%27s_Crest) | Demon's Crest | - | - | -- -- -- | -- -- -- | -- -- --
| [Desperado](https://en.wikipedia.org/wiki/Gun.Smoke) | Desperado | - | - | -- -- -- | -- -- -- | -- -- --
| [DOAE](https://en.wikipedia.org/wiki/Destiny_of_an_Emperor) | Destiny of an Emperor | - | - | -- -- -- | -- -- -- | -- -- --
| [DOAE2](https://en.wikipedia.org/wiki/Tenchi_wo_Kurau_II) | Destiny of an Emperor II | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC](https://en.wikipedia.org/wiki/Devil_May_Cry_(video_game)) | Devil May Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC2](https://en.wikipedia.org/wiki/Devil_May_Cry_2) | Devil May Cry 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC3:DA](https://en.wikipedia.org/wiki/Devil_May_Cry_3:_Dante%27s_Awakening) | Devil May Cry 3: Dante's Awakening | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC3:S](https://store.steampowered.com/app/6550/Devil_May_Cry_3_Special_Edition/) | Devil May Cry 3: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC4](https://en.wikipedia.org/wiki/Devil_May_Cry_4) | Devil May Cry 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC4:R](https://en.wikipedia.org/wiki/Devil_May_Cry_4) | Devil May Cry 4: Refrain | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC4:S](https://store.steampowered.com/app/329050/Devil_May_Cry_4_Special_Edition/) | Devil May Cry 4: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC5](https://store.steampowered.com/app/601150/Devil_May_Cry_5/) | Devil May Cry 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC5:S](https://www.xbox.com/en-US/games/store/devil-may-cry-5-special-edition/9MZ11KT5KLP6/0010) | Devil May Cry 5: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC:HD](https://store.steampowered.com/app/631510/Devil_May_Cry_HD_Collection/) | Devil May Cry: HD Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Dimahoo](https://en.wikipedia.org/wiki/Dimahoo) | Dimahoo | - | - | -- -- -- | -- -- -- | -- -- --
| [DC](https://en.wikipedia.org/wiki/Dino_Crisis_(video_game)) | Dino Crisis | - | - | -- -- -- | -- -- -- | -- -- --
| [DC2](https://en.wikipedia.org/wiki/Dino_Crisis_2) | Dino Crisis 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DC3](https://en.wikipedia.org/wiki/Dino_Crisis_3) | Dino Crisis 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DC:S](https://en.wikipedia.org/wiki/Dino_Stalker) | Dino Stalker | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:A](https://en.wikipedia.org/wiki/Disney%27s_Aladdin_(Capcom_video_game)) | Disney's Aladdin | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:HaS](https://en.wikipedia.org/wiki/Disney%27s_Hide_and_Sneak) | Disney's Hide and Sneak | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MM](https://en.wikipedia.org/wiki/Disney%27s_Magical_Mirror_Starring_Mickey_Mouse) | Disney's Magical Mirror Starring Mickey Mouse | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MQ](https://en.wikipedia.org/wiki/Disney%27s_Magical_Quest#The_Magical_Quest_Starring_Mickey_Mouse) | Disney's Magical Quest Starring Mickey Mouse | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MQ2](https://en.wikipedia.org/wiki/Disney%27s_Magical_Quest) | Disney's Magical Quest 2 Starring Mickey and Minnie | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MQ3](https://en.wikipedia.org/wiki/Disney%27s_Magical_Quest#Disney.27s_Magical_Quest_3_Starring_Mickey_.26_Donald) | Disney's Magical Quest 3 Starring Mickey & Donald | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC:X](https://store.steampowered.com/app/220440/DmC_Devil_May_Cry/) | DmC: Devil May Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [Dokaben](https://en.wikipedia.org/wiki/Dokaben) | Dokaben | - | - | -- -- -- | -- -- -- | -- -- --
| [Dokaben2](https://en.wikipedia.org/wiki/Dokaben) | Dokaben II | - | - | -- -- -- | -- -- -- | -- -- --
| [DD](https://store.steampowered.com/app/367500/Dragons_Dogma_Dark_Arisen/) | Dragon's Dogma | - | - | -- -- -- | -- -- -- | -- -- --
| [DD2](https://store.steampowered.com/app/2054970/Dragons_Dogma_2/) | Dragon's Dogma II | - | - | -- -- -- | -- -- -- | -- -- --
| [DT](https://en.wikipedia.org/wiki/DuckTales_(video_game)) | DuckTales | - | - | -- -- -- | -- -- -- | -- -- --
| [DT2](https://en.wikipedia.org/wiki/DuckTales_2) | DuckTales 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DT:R](https://store.steampowered.com/app/237630/DuckTales_Remastered/) | DuckTales: Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [DND:C](https://en.wikipedia.org/wiki/Dungeons_%26_Dragons:_Tower_of_Doom#Dungeons_&_Dragons_Collection) | Dungeons & Dragons Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [DnD:CoM](https://store.steampowered.com/app/229480/Dungeons__Dragons_Chronicles_of_Mystara/) | Dungeons & Dragons: Chronicles of Mystara | - | - | -- -- -- | -- -- -- | -- -- --
| [DnD:ToD](https://en.wikipedia.org/wiki/Dungeons_%26_Dragons:_Tower_of_Doom) | Dungeons & Dragons: Tower of Doom | - | - | -- -- -- | -- -- -- | -- -- --
| [Dustforce](https://en.wikipedia.org/wiki/Dustforce) | Dustforce | - | - | -- -- -- | -- -- -- | -- -- --
| [Dynasty Wars](https://en.wikipedia.org/wiki/Dynasty_Wars) | Dynasty Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [EXTroopers](https://en.wikipedia.org/wiki/E.X._Troopers) | E.X. Troopers | - | - | -- -- -- | -- -- -- | -- -- --
| [EcoFighters](https://en.wikipedia.org/wiki/Eco_Fighters) | Eco Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V1](https://en.wikipedia.org/wiki/El_Dorado_Gate) | El Dorado Gate Volume 1 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V2](https://en.wikipedia.org/wiki/El_Dorado_Gate) | El Dorado Gate Volume 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V3](https://en.wikipedia.org/wiki/El_Dorado_Gate) | El Dorado Gate Volume 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V4](https://en.wikipedia.org/wiki/El_Dorado_Gate) | El Dorado Gate Volume 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V5](https://en.wikipedia.org/wiki/El_Dorado_Gate) | El Dorado Gate Volume 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V6](https://en.wikipedia.org/wiki/El_Dorado_Gate) | El Dorado Gate Volume 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V7](https://en.wikipedia.org/wiki/El_Dorado_Gate) | El Dorado Gate Volume 7 | - | - | -- -- -- | -- -- -- | -- -- --
| [EtherVapor:R]() | Ether Vapor Remaster | - | - | -- -- -- | -- -- -- | -- -- --
| [Everblue](https://en.wikipedia.org/wiki/Everblue) | Everblue | - | - | -- -- -- | -- -- -- | -- -- --
| [Everblue2](https://en.wikipedia.org/wiki/Everblue_2) | Everblue 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [ExedExes](https://en.wikipedia.org/wiki/Exed_Exes) | Exed Exes | - | - | -- -- -- | -- -- -- | -- -- --
| [Exoprimal](https://store.steampowered.com/app/1286320/Exoprimal/) | Exoprimal | - | - | -- -- -- | -- -- -- | -- -- --
| [EotB](https://en.wikipedia.org/wiki/Eye_of_the_Beholder_(video_game)) | Eye of the Beholder | - | - | -- -- -- | -- -- -- | -- -- --
| [F1Dream](https://en.wikipedia.org/wiki/F-1_Dream) | F-1 Dream | - | - | -- -- -- | -- -- -- | -- -- --
| [FairyBloomFreesia]() | Fairy Bloom Freesia | - | - | -- -- -- | -- -- -- | -- -- --
| [GnG:FM]() | Famicom Mini: Ghosts 'n Goblins | - | - | -- -- -- | -- -- -- | -- -- --
| [Fate:TC](https://en.wikipedia.org/wiki/Fate/tiger_colosseum) | Fate/tiger colosseum | - | - | -- -- -- | -- -- -- | -- -- --
| [Fate:UC](https://en.wikipedia.org/wiki/Fate/unlimited_codes) | Fate/unlimited codes | - | - | -- -- -- | -- -- -- | -- -- --
| [FeverChance]() | Fever Chance | - | - | -- -- -- | -- -- -- | -- -- --
| [FightingStreet](https://en.wikipedia.org/wiki/Fighting_Street) | Fighting Street | - | - | -- -- -- | -- -- -- | -- -- --
| [FF](https://en.wikipedia.org/wiki/Final_Fight_(video_game)) | Final Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [FF2](https://en.wikipedia.org/wiki/Final_Fight_2) | Final Fight 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [FF3](https://en.wikipedia.org/wiki/Final_Fight_3) | Final Fight 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:G](https://en.wikipedia.org/wiki/Final_Fight_(video_game)#Super_NES_(Final_Fight_and_Final_Fight_Guy)) | Final Fight Guy | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:O](https://en.wikipedia.org/wiki/Final_Fight_(video_game)#Game_Boy_Advance_(Final_Fight_One)) | Final Fight One | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:R](https://en.wikipedia.org/wiki/Final_Fight_Revenge) | Final Fight Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:DI](https://en.wikipedia.org/wiki/Final_Fight:_Double_Impact) | Final Fight: Double Impact | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:S](https://en.wikipedia.org/wiki/Final_Fight:_Streetwise) | Final Fight: Streetwise | - | - | -- -- -- | -- -- -- | -- -- --
| [FinderLove](https://en.wikipedia.org/wiki/Finder_Love) | Finder Love | - | - | -- -- -- | -- -- -- | -- -- --
| [FlipperFootball]() | Flipper Football | - | - | -- -- -- | -- -- -- | -- -- --
| [Flock](https://store.steampowered.com/app/21640/Flock/) | Flock! | - | - | -- -- -- | -- -- -- | -- -- --
| [ForgottenWorlds](https://en.wikipedia.org/wiki/Forgotten_Worlds) | Forgotten Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [FoxHunt](https://en.wikipedia.org/wiki/Fox_Hunt_(video_game)) | Fox Hunt | - | - | -- -- -- | -- -- -- | -- -- --
| [FushigiDeka]() | Fushigi Deka | - | - | -- -- -- | -- -- -- | -- -- --
| [GIJ:TAF](https://en.wikipedia.org/wiki/G.I._Joe:_The_Atlantis_Factor) | G.I. Joe: The Atlantis Factor | - | - | -- -- -- | -- -- -- | -- -- --
| [Gaia:SD]() | Gaia Master Kessen!: Seikiou Densetsu | - | - | -- -- -- | -- -- -- | -- -- --
| [Gaia:KBG]() | Gaia Master: Kami no Board Game | - | - | -- -- -- | -- -- -- | -- -- --
| [GaistCrusher](https://en.wikipedia.org/wiki/Gaist_Crusher) | Gaist Crusher | - | - | -- -- -- | -- -- -- | -- -- --
| [Gakkou:HGK]() | Gakkou no Kowai Uwasa: Hanako-san ga Kita!! | - | - | -- -- -- | -- -- -- | -- -- --
| [GargoyleQuest](https://en.wikipedia.org/wiki/Gargoyle%27s_Quest) | Gargoyle's Quest | - | - | -- -- -- | -- -- -- | -- -- --
| [GargoyleQuest2](https://en.wikipedia.org/wiki/Gargoyle%27s_Quest_II) | Gargoyle's Quest II | - | - | -- -- -- | -- -- -- | -- -- --
| [GenmaOnimusha](https://en.wikipedia.org/wiki/Genma_Onimusha) | Genma Onimusha | - | - | -- -- -- | -- -- -- | -- -- --
| [GnG](https://en.wikipedia.org/wiki/Ghosts_%27n_Goblins_(video_game)) | Ghosts 'n Goblins | - | - | -- -- -- | -- -- -- | -- -- --
| [GnG:GK](https://en.wikipedia.org/wiki/Ghosts_%27n_Goblins:_Gold_Knights) | Ghosts 'n Goblins: Gold Knights | - | - | -- -- -- | -- -- -- | -- -- --
| [GnG:GK2](https://en.wikipedia.org/wiki/Ghosts_%27n_Goblins:_Gold_Knights) | Ghosts 'n Goblins: Gold Knights II | - | - | -- -- -- | -- -- -- | -- -- --
| [GhoulsGhosts](https://en.wikipedia.org/wiki/Ghouls_%27n_Ghosts) | Ghouls 'n Ghosts | - | - | -- -- -- | -- -- -- | -- -- --
| [GhostTrick](https://store.steampowered.com/app/1967430/Ghost_Trick_Phantom_Detective/) | Ghost Trick: Phantom Detective | - | - | -- -- -- | -- -- -- | -- -- --
| [GigaWing](https://en.wikipedia.org/wiki/Giga_Wing) | Giga Wing | - | - | -- -- -- | -- -- -- | -- -- --
| [GigaWing2](https://en.wikipedia.org/wiki/Giga_Wing_2) | Giga Wing 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [GioGio:BA](https://en.wikipedia.org/wiki/GioGio%27s_Bizarre_Adventure) | GioGio's Bizarre Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [GlassRose](https://en.wikipedia.org/wiki/Glass_Rose) | Glass Rose | - | - | -- -- -- | -- -- -- | -- -- --
| [GodHand](https://en.wikipedia.org/wiki/God_Hand) | God Hand | - | - | -- -- -- | -- -- -- | -- -- --
| [GoW](https://store.steampowered.com/app/1593500/God_of_War/) | God of War | - | - | -- -- -- | -- -- -- | -- -- --
| [GoW2](https://en.wikipedia.org/wiki/God_of_War_II) | God of War II | - | - | -- -- -- | -- -- -- | -- -- --
| [GoW:C](https://en.wikipedia.org/wiki/God_of_War_Collection) | God of War Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [GoldMedalChallenge](https://en.wikipedia.org/wiki/Gold_Medal_Challenge) | Gold Medal Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [GoofTroop](https://en.wikipedia.org/wiki/Goof_Troop_(video_game)) | Goof Troop | - | - | -- -- -- | -- -- -- | -- -- --
| [GotchaForce](https://en.wikipedia.org/wiki/Gotcha_Force) | Gotcha Force | - | - | -- -- -- | -- -- -- | -- -- --
| [GregoryHorrorShow](https://en.wikipedia.org/wiki/Gregory_Horror_Show_(video_game)) | Gregory Horror Show | - | - | -- -- -- | -- -- -- | -- -- --
| [Group S Challenge](https://en.wikipedia.org/wiki/Group_S_Challenge) | Group S Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [GunSmoke](https://en.wikipedia.org/wiki/Gun.Smoke) | Gun.Smoke | - | - | -- -- -- | -- -- -- | -- -- --
| [GyakutenKenji2](https://en.wikipedia.org/wiki/Gyakuten_Kenji_2) | Gyakuten Kenji 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [HarveyBirdman:AaL](https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_3:_The_First_Generation) | Harvey Birdman: Attorney at Law | - | - | -- -- -- | -- -- -- | -- -- --
| [HatTrick](https://en.wikipedia.org/wiki/Hat_Trick_(arcade_game)) | Hat Trick | - | - | -- -- -- | -- -- -- | -- -- --
| [Haunting Ground](https://en.wikipedia.org/wiki/Haunting_Ground) | Haunting Ground | - | - | -- -- -- | -- -- -- | -- -- --
| [HeavyMetal:G](https://en.wikipedia.org/wiki/Heavy_Metal:_Geomatrix) | Heavy Metal: Geomatrix | - | - | -- -- -- | -- -- -- | -- -- --
| [HigemaruMakaijima:NSD](https://en.wikipedia.org/wiki/Higemaru_Makaijima_-_Nanatsu_no_Shima_Daib%C5%8Dken) | Higemaru Makaijima - Nanatsu no Shima Daibōken | - | - | -- -- -- | -- -- -- | -- -- --
| [HSF2:TA](https://en.wikipedia.org/wiki/Hyper_Street_Fighter_II:_The_Anniversary_Edition) | Hyper Street Fighter II: The Anniversary Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong:IYJ]() | Ide no Yosuke no Jissen Mahjong | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong2:IYJ]() | Ide no Yosuke no Jissen Mahjong II | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong:IYMSJ]() | Ide Yousuke Meijin no Shinmi Jissen Mahjong | - | - | -- -- -- | -- -- -- | -- -- --
| [JoJo](https://en.wikipedia.org/wiki/JoJo%27s_Bizarre_Adventure_(video_game)) | JoJo's Venture | - | - | -- -- -- | -- -- -- | -- -- --
| [JoJo:HD](https://en.wikipedia.org/wiki/JoJo%27s_Bizarre_Adventure_(video_game)) | JoJo's Bizarre Adventure HD | - | - | -- -- -- | -- -- -- | -- -- --
| [JoJo:HftF](https://en.wikipedia.org/wiki/Capcom_vs._SNK_2) | JoJo's Bizarre Adventure: Heritage for the Future | - | - | -- -- -- | -- -- -- | -- -- --
| [KabuTraderShun](https://en.wikipedia.org/wiki/Kabu_Trader_Shun) | Kabu Trader Shun | - | - | -- -- -- | -- -- -- | -- -- --
| [KenKen:TYB]() | KenKen: Train Your Brain | - | - | -- -- -- | -- -- -- | -- -- --
| [Killer7](https://en.wikipedia.org/wiki/Killer7) | killer7 | - | - | -- -- -- | -- -- -- | -- -- --
| [Kingpin]() | Kingpin | - | - | -- -- -- | -- -- -- | -- -- --
| [Knights of the Round](https://en.wikipedia.org/wiki/Knights_of_the_Round_(video_game)) | Knights of the Round | - | - | -- -- -- | -- -- -- | -- -- --
| [Kunitsu:PotG]() | Kunitsu-Gami: Path of the Goddess | - | - | -- -- -- | -- -- -- | -- -- --
| [Kyojin:H](https://en.wikipedia.org/wiki/Kyojin_no_Hoshi) | Kyojin no Hoshi | - | - | -- -- -- | -- -- -- | -- -- --
| [LastDuel:IPW](https://en.wikipedia.org/wiki/Last_Duel_(video_game)) | Last Duel: Inter Planet War 2012 | - | - | -- -- -- | -- -- -- | -- -- --
| [LastRanker](https://en.wikipedia.org/wiki/Last_Ranker) | Last Ranker | - | - | -- -- -- | -- -- -- | -- -- --
| [LaytonVGyakuten](https://en.wikipedia.org/wiki/Layton-kyoju_VS_Gyakuten_Saiban) | Layton-kyoju VS Gyakuten Saiban | - | - | -- -- -- | -- -- -- | -- -- --
| [LEDStorm]() | LED Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [LegendKay](https://en.wikipedia.org/wiki/Legend_of_Kay) | Legend of Kay | - | - | -- -- -- | -- -- -- | -- -- --
| [LegendaryWings](https://en.wikipedia.org/wiki/Legendary_Wings) | Legendary Wings | - | - | -- -- -- | -- -- -- | -- -- --
| [LilPirates]() | Lil' Pirates | - | - | -- -- -- | -- -- -- | -- -- --
| [LittleLeague](https://en.wikipedia.org/wiki/Little_League) | Little League | - | - | -- -- -- | -- -- -- | -- -- --
| [LittleNemo:TDM](https://en.wikipedia.org/wiki/Little_Nemo:_The_Dream_Master) | Little Nemo: The Dream Master | - | - | -- -- -- | -- -- -- | -- -- --
| [LP:EC](https://store.steampowered.com/app/6510/Lost_Planet_Extreme_Condition/) | Lost Planet: Extreme Condition | - | - | -- -- -- | -- -- -- | -- -- --
| [LP:C](https://en.wikipedia.org/wiki/Lost_Planet:_Extreme_Condition#Collector.27s_and_Colonies_Edition) | Lost Planet: Colonies | - | - | -- -- -- | -- -- -- | -- -- --
| [LP2](https://en.wikipedia.org/wiki/Lost_Planet_2) | Lost Planet 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [LP3](https://store.steampowered.com/app/226720/LOST_PLANET_3/) | Lost Planet 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MagicSword](https://en.wikipedia.org/wiki/Magic_Sword_(video_game)) | Magic Sword | - | - | -- -- -- | -- -- -- | -- -- --
| [Tetris:MC](https://en.wikipedia.org/wiki/Magical_Tetris_Challenge) | Magical Tetris Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong:G]() | Mahjong Gakuen | - | - | -- -- -- | -- -- -- | -- -- --
| [Makaimura:FW](https://en.wikipedia.org/wiki/Makaimura_for_WonderSwan) | Makaimura for WonderSwan | - | - | -- -- -- | -- -- -- | -- -- --
| [MarsMatrix:HSS](https://en.wikipedia.org/wiki/Mars_Matrix:_Hyper_Solid_Shooting) | Mars Matrix: Hyper Solid Shooting | - | - | -- -- -- | -- -- -- | -- -- --
| [Marusa:O](https://en.wikipedia.org/wiki/Marusa_no_Onna) | Marusa no Onna | - | - | -- -- -- | -- -- -- | -- -- --
| [MSH](https://en.wikipedia.org/wiki/Marvel_Super_Heroes_(video_game)) | Marvel Super Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [MSHVSF](https://en.wikipedia.org/wiki/Marvel_Super_Heroes_vs._Street_Fighter) | Marvel Super Heroes vs. Street Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [MSHVSF:EX](https://en.wikipedia.org/wiki/Marvel_Super_Heroes_vs._Street_Fighter) | Marvel Super Heroes vs. Street Fighter EX Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [MSH:IWotG](https://en.wikipedia.org/wiki/Marvel_Super_Heroes_In_War_of_the_Gems) | Marvel Super Heroes In War of the Gems | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC](https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Clash_of_Super_Heroes) | Marvel vs. Capcom | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC2](https://en.wikipedia.org/wiki/Marvel_vs._Capcom_2) | Marvel vs. Capcom 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC2:NAoH](https://en.wikipedia.org/wiki/Marvel_vs._Capcom_2:_New_Age_of_Heroes) | Marvel vs. Capcom 2: New Age of Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC3:AFoTW](https://en.wikipedia.org/wiki/Marvel_vs._Capcom_3:_Fate_of_Two_Worlds) | Marvel vs. Capcom 3: Fate of Two Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:CoSH](https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Clash_of_Super_Heroes) | Marvel vs. Capcom: Clash of Super Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:CoSH:EX](https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Clash_of_Super_Heroes) | Marvel vs. Capcom Clash of Super Heroes EX Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:I](https://store.steampowered.com/app/493840) | Marvel vs. Capcom: Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:O](https://en.wikipedia.org/wiki/Marvel_vs._Capcom_Origins) | Marvel vs. Capcom Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [Maximo:AoZ](https://en.wikipedia.org/wiki/Maximo_vs._Army_of_Zin) | Maximo vs. Army of Zin | - | - | -- -- -- | -- -- -- | -- -- --
| [Maximo:GtG](https://en.wikipedia.org/wiki/Maximo:_Ghosts_to_Glory) | Maximo: Ghosts to Glory | - | - | -- -- -- | -- -- -- | -- -- --
| [MaXplosion](https://en.wikipedia.org/wiki/%27Splosion_Man#Controversy) | MaXplosion | - | - | -- -- -- | -- -- -- | -- -- --
| [MM](https://en.wikipedia.org/wiki/Mega_Man_(video_game)) | Mega Man | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:B](https://en.wikipedia.org/wiki/Mega_Man_%26_Bass) | Mega Man & Bass | - | - | -- -- -- | -- -- -- | -- -- --
| [MM2](https://en.wikipedia.org/wiki/Mega_Man_2) | Mega Man 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM2:TPF](https://en.wikipedia.org/wiki/Mega_Man_2:_The_Power_Fighters) | Mega Man 2: The Power Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [MM3](https://en.wikipedia.org/wiki/Mega_Man_3) | Mega Man 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM4](https://en.wikipedia.org/wiki/Mega_Man_4) | Mega Man 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM5](https://en.wikipedia.org/wiki/Mega_Man_5) | Mega Man 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM6](https://en.wikipedia.org/wiki/Mega_Man_6) | Mega Man 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:64](https://en.wikipedia.org/wiki/Mega_Man_64) | Mega Man 64 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM7](https://en.wikipedia.org/wiki/Mega_Man_7) | Mega Man 7 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM8](https://en.wikipedia.org/wiki/Mega_Man_8) | Mega Man 8 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM9](https://en.wikipedia.org/wiki/Mega_Man_9) | Mega Man 9 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM10](https://en.wikipedia.org/wiki/Mega_Man_10) | Mega Man 10 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM11](https://store.steampowered.com/app/742300) | Mega Man 11 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:AC](https://en.wikipedia.org/wiki/Mega_Man_Anniversary_Collection) | Mega Man Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:C](https://en.wikipedia.org/wiki/Mega_Man_Battle_%26_Chase) | Mega Man Battle & Chase | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:CC](https://en.wikipedia.org/wiki/Mega_Man_Battle_Chip_Challenge) | Mega Man Battle Chip Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_(video_game)) | Mega Man Battle Network | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N2](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_2) | Mega Man Battle Network 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N3](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_3) | Mega Man Battle Network 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N4](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_4) | Mega Man Battle Network 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N5](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_5) | Mega Man Battle Network 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N6](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_6) | Mega Man Battle Network 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:DWR](https://en.wikipedia.org/wiki/Mega_Man:_Dr._Wily%27s_Revenge) | Mega Man: Dr. Wily's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [MMII](https://en.wikipedia.org/wiki/Mega_Man_II_(Game_Boy)) | Mega Man II | - | - | -- -- -- | -- -- -- | -- -- --
| [MMIII](https://en.wikipedia.org/wiki/Mega_Man_III_(Game_Boy)) | Mega Man III | - | - | -- -- -- | -- -- -- | -- -- --
| [MMIV](https://en.wikipedia.org/wiki/Mega_Man_IV_(Game_Boy)) | Mega Man IV | - | - | -- -- -- | -- -- -- | -- -- --
| [MML](https://en.wikipedia.org/wiki/Mega_Man_Legends_(video_game)) | Mega Man Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [MML2](https://en.wikipedia.org/wiki/Mega_Man_Legends_2) | Mega Man Legends 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MML3](https://en.wikipedia.org/wiki/Mega_Man_Legends_3) | Mega Man Legends 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:M](https://en.wikipedia.org/wiki/Mega_Man_Mania) | Mega Man Mania | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:NT](https://en.wikipedia.org/wiki/Mega_Man_Network_Transmission) | Mega Man Network Transmission | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:PU](https://en.wikipedia.org/wiki/Mega_Man_Powered_Up) | Mega Man Powered Up | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:S](https://en.wikipedia.org/wiki/Mega_Man_Soccer) | Mega Man Soccer | - | - | -- -- -- | -- -- -- | -- -- --
| [MMSF](https://en.wikipedia.org/wiki/Mega_Man_Star_Force) | Mega Man Star Force | - | - | -- -- -- | -- -- -- | -- -- --
| [MMSF2](https://en.wikipedia.org/wiki/Mega_Man_Star_Force_2) | Mega Man Star Force 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMSF3](https://en.wikipedia.org/wiki/Mega_Man_Star_Force_3) | Mega Man Star Force 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:U](https://en.wikipedia.org/wiki/Mega_Man_Universe) | Mega Man Universe | - | - | -- -- -- | -- -- -- | -- -- --
| [MMV](https://en.wikipedia.org/wiki/Mega_Man_V_(Game_Boy)) | Mega Man V | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX](https://en.wikipedia.org/wiki/Mega_Man_X_(video_game)) | Mega Man X | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:C](https://en.wikipedia.org/wiki/Mega_Man_X_Collection) | Mega Man X Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:CM](https://en.wikipedia.org/wiki/Mega_Man_X:_Command_Mission) | Mega Man X: Command Mission | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC](https://store.steampowered.com/app/743890) | Mega Man X Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC2](https://store.steampowered.com/app/743900) | Mega Man X Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX2](https://en.wikipedia.org/wiki/Mega_Man_X2) | Mega Man X2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX3](https://en.wikipedia.org/wiki/Mega_Man_X3) | Mega Man X3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX4](https://en.wikipedia.org/wiki/Mega_Man_X4) | Mega Man X4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX5](https://en.wikipedia.org/wiki/Mega_Man_X5) | Mega Man X5 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX6](https://en.wikipedia.org/wiki/Mega_Man_X6) | Mega Man X6 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX7](https://en.wikipedia.org/wiki/Mega_Man_X7) | Mega Man X7 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX8](https://en.wikipedia.org/wiki/Mega_Man_X8) | Mega Man X8 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:X](https://en.wikipedia.org/wiki/Mega_Man_Xtreme) | Mega Man Xtreme | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:X2](https://en.wikipedia.org/wiki/Mega_Man_Xtreme_2) | Mega Man Xtreme 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ](https://en.wikipedia.org/wiki/Mega_Man_Zero_(video_game)) | Mega Man Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ2](https://en.wikipedia.org/wiki/Mega_Man_Zero_2) | Mega Man Zero 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ3](https://en.wikipedia.org/wiki/Mega_Man_Zero_3) | Mega Man Zero 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ4](https://en.wikipedia.org/wiki/Mega_Man_Zero_4) | Mega Man Zero 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ:C](https://en.wikipedia.org/wiki/Mega_Man_Zero_Collection) | Mega Man Zero Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZX](https://en.wikipedia.org/wiki/Mega_Man_ZX) | Mega Man ZX | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZX:A](https://en.wikipedia.org/wiki/Mega_Man_ZX_Advent) | Mega Man ZX Advent | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:MHX](https://en.wikipedia.org/wiki/Mega_Man:_Maverick_Hunter_X) | Mega Man: Maverick Hunter X | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:TPB](https://en.wikipedia.org/wiki/Mega_Man:_The_Power_Battle) | Mega Man: The Power Battle | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:TWW](https://en.wikipedia.org/wiki/Mega_Man:_The_Wily_Wars) | Mega Man: The Wily Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [MegaTwins](https://en.wikipedia.org/wiki/Mega_Twins) | Mega Twins | - | - | -- -- -- | -- -- -- | -- -- --
| [Mercs](https://en.wikipedia.org/wiki/Mercs) | Mercs | - | - | -- -- -- | -- -- -- | -- -- --
| [MetalWalker](https://en.wikipedia.org/wiki/Metal_Walker) | Metal Walker | - | - | -- -- -- | -- -- -- | -- -- --
| [Mickey:M](https://en.wikipedia.org/wiki/Mickey_Mousecapade) | Mickey Mousecapade | - | - | -- -- -- | -- -- -- | -- -- --
| [Mickey:DC](https://en.wikipedia.org/wiki/Mickey%27s_Dangerous_Chase) | Mickey's Dangerous Chase | - | - | -- -- -- | -- -- -- | -- -- --
| [MightyFinalFight](https://en.wikipedia.org/wiki/Mighty_Final_Fight) | Mighty Final Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [MTWI](https://en.wikipedia.org/wiki/Minute_to_Win_It) | Minute to Win It | - | - | -- -- -- | -- -- -- | -- -- --
| [Mizushima:D]() | Mizushima Shinji no Daikoushien | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:FVZA](https://en.wikipedia.org/wiki/Gundam_Seed:_Rengou_vs._Z.A.F.T.) | Mobile Suit Gundam SEED: Federation vs ZAFT | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:AVT]() | Mobile Suit Gundam: AEUG Vs Titans | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:FVZE](https://en.wikipedia.org/wiki/Mobile_Suit_Gundam:_Federation_vs._Zeon) | Mobile Suit Gundam: Federation vs. Zeon | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:FVZE:DX](https://en.wikipedia.org/wiki/List_of_Gundam_video_games#Dreamcast) | Mobile Suit Gundam: Federation vs. Zeon DX | - | - | -- -- -- | -- -- -- | -- -- --
| [MH](https://en.wikipedia.org/wiki/Monster_Hunter_(video_game)) | Monster Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [MH2](https://en.wikipedia.org/wiki/Monster_Hunter_2) | Monster Hunter 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH3](https://en.wikipedia.org/wiki/Monster_Hunter_Tri#Monster_Hunter_3_Ultimate) | Monster Hunter 3 Ultimate | - | - | -- -- -- | -- -- -- | -- -- --
| [MH4](https://en.wikipedia.org/wiki/Monster_Hunter_4) | Monster Hunter 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:F](https://en.wikipedia.org/wiki/Monster_Hunter_Freedom) | Monster Hunter Freedom | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:F2](https://en.wikipedia.org/wiki/Monster_Hunter_Freedom_2) | Monster Hunter Freedom 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:FU](https://en.wikipedia.org/wiki/Monster_Hunter_Freedom_Unite) | Monster Hunter Freedom Unite | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:FG](https://en.wikipedia.org/wiki/Monster_Hunter_Frontier_G) | Monster Hunter Frontier G | - | - | -- -- -- | -- -- -- | -- -- --
| [MHF:FO](https://en.wikipedia.org/wiki/Monster_Hunter_Frontier_Online) | Monster Hunter Frontier Online | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:G](https://en.wikipedia.org/wiki/Monster_Hunter_(video_game)#Monster_Hunter_G) | Monster Hunter G | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:GX](https://en.wikipedia.org/wiki/Monster_Hunter_Generations) | Monster Hunter Generations / Monster Hunter X | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:GU](https://en.wikipedia.org/wiki/Monster_Hunter_Generations) | Monster Hunter Generations Ultimate | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:P3](https://en.wikipedia.org/wiki/Monster_Hunter_Portable_3rd) | Monster Hunter Portable 3rd | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:P3HD](https://en.wikipedia.org/wiki/Monster_Hunter_Portable_3rd) | Monster Hunter Portable 3rd HD ver. | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:S](https://en.wikipedia.org/wiki/Monster_Hunter_Stories) | Monster Hunter Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:T](https://en.wikipedia.org/wiki/Monster_Hunter_Tri) | Monster Hunter Tri | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:W](https://store.steampowered.com/app/582010) | Monster Hunter: World | - | - | -- -- -- | -- -- -- | -- -- --
| [MHXX](https://en.wikipedia.org/wiki/Monster_Hunter_Generations) | Monster Hunter XX | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP](https://en.wikipedia.org/wiki/MotoGP_08) | MotoGP | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP07](https://en.wikipedia.org/wiki/MotoGP_%2707_(PS2)) | MotoGP '07 | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP08](https://en.wikipedia.org/wiki/MotoGP_%2708) | MotoGP '08 | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP09](https://en.wikipedia.org/wiki/MotoGP_09/10) | MotoGP 09/10 | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP10](https://en.wikipedia.org/wiki/MotoGP_10/11) | MotoGP 10/11 | - | - | -- -- -- | -- -- -- | -- -- --
| [MrBill](https://en.wikipedia.org/wiki/Mr._Bill) | Mr. Bill | - | - | -- -- -- | -- -- -- | -- -- --
| [MB](https://en.wikipedia.org/wiki/Muscle_Bomber) | Muscle Bomber | - | - | -- -- -- | -- -- -- | -- -- --
| [MB2](https://en.wikipedia.org/wiki/Muscle_Bomber) | Muscle Bomber 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MB:D](https://en.wikipedia.org/wiki/Muscle_Bomber_Duo) | Muscle Bomber Duo | - | - | -- -- -- | -- -- -- | -- -- --
| [MB:TBE](https://en.wikipedia.org/wiki/Muscle_Bomber:_The_Body_Explosion) | Muscle Bomber: The Body Explosion | - | - | -- -- -- | -- -- -- | -- -- --
| [NamcoxCapcom](https://en.wikipedia.org/wiki/Namco_%C3%97_Capcom) | Namco x Capcom | - | - | -- -- -- | -- -- -- | -- -- --
| [NazoWakuYakata]() | Nazo Waku Yakata | - | - | -- -- -- | -- -- -- | -- -- --
| [Nemo](https://en.wikipedia.org/wiki/Nemo_(arcade_game)) | Nemo | - | - | -- -- -- | -- -- -- | -- -- --
| [Tennis:N]() | Netto de Tennis | - | - | -- -- -- | -- -- -- | -- -- --
| [SMBW:CW](https://en.wikipedia.org/wiki/New_Super_Mario_Bros._Wii#New_Super_Mario_Bros._Wii_Coin_World) | New Super Mario Bros. Wii Coin World | - | - | -- -- -- | -- -- -- | -- -- --
| [NightWarrior:DR](https://en.wikipedia.org/wiki/Night_Warriors:_Darkstalkers%27_Revenge) | Night Warriors: Darkstalkers' Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [Okami](https://en.wikipedia.org/wiki/%C5%8Ckami) | Ōkami | - | - | -- -- -- | -- -- -- | -- -- --
| [Okami:HD](https://store.steampowered.com/app/587620) | Ōkami HD | - | - | -- -- -- | -- -- -- | -- -- --
| [Okamiden](https://en.wikipedia.org/wiki/%C5%8Ckamiden) | Ōkamiden | - | - | -- -- -- | -- -- -- | -- -- --
| [OnePieceMansion](https://en.wikipedia.org/wiki/One_Piece_Mansion) | One Piece Mansion | - | - | -- -- -- | -- -- -- | -- -- --
| [O2:SD](https://en.wikipedia.org/wiki/Onimusha_2:_Samurai%27s_Destiny) | Onimusha 2: Samurai's Destiny | - | - | -- -- -- | -- -- -- | -- -- --
| [O3:DS](https://en.wikipedia.org/wiki/Onimusha_3:_Demon_Siege) | Onimusha 3: Demon Siege | - | - | -- -- -- | -- -- -- | -- -- --
| [O:BW](https://en.wikipedia.org/wiki/Onimusha_Blade_Warriors) | Onimusha Blade Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [O:DoD](https://en.wikipedia.org/wiki/Onimusha:_Dawn_of_Dreams) | Onimusha: Dawn of Dreams | - | - | -- -- -- | -- -- -- | -- -- --
| [O:S](https://en.wikipedia.org/wiki/Onimusha_Soul) | Onimusha Soul | - | - | -- -- -- | -- -- -- | -- -- --
| [O:T](https://en.wikipedia.org/wiki/Onimusha_Tactics) | Onimusha Tactics | - | - | -- -- -- | -- -- -- | -- -- --
| [O:W](https://store.steampowered.com/app/761600) | Onimusha: Warlords | - | - | -- -- -- | -- -- -- | -- -- --
| [OshieteFighter]() | Oshiete Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [PN03](https://en.wikipedia.org/wiki/P.N.03) | P.N.03 | - | - | -- -- -- | -- -- -- | -- -- --
| [PanicShot:R]() | Panic Shot! Rockman | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA](https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney) | Phoenix Wright: Ace Attorney | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:DD](https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Dual_Destinies) | Phoenix Wright: Ace Attorney - Dual Destinies | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:JfA](https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Justice_for_All) | Phoenix Wright: Ace Attorney - Justice for All | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:SoJ](https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Spirit_of_Justice) | Phoenix Wright: Ace Attorney - Spirit of Justice | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:TaT](https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Trials_and_Tribulations) | Phoenix Wright: Ace Attorney - Trials and Tribulations | - | - | -- -- -- | -- -- -- | -- -- --
| [Pinball:M]() | Pinball Magic | - | - | -- -- -- | -- -- -- | -- -- --
| [PirateShipHigemaru](https://en.wikipedia.org/wiki/Pirate_Ship_Higemaru) | Pirate Ship Higemaru | - | - | -- -- -- | -- -- -- | -- -- --
| [PlanetWork]() | Planet Work | - | - | -- -- -- | -- -- -- | -- -- --
| [PlasmaSword:NoB](https://en.wikipedia.org/wiki/Plasma_Sword:_Nightmare_of_Bilstein) | Plasma Sword: Nightmare of Bilstein | - | - | -- -- -- | -- -- -- | -- -- --
| [PocketFighter](https://en.wikipedia.org/wiki/Pocket_Fighter) | Pocket Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [PocketRockets]() | Pocket Rockets | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerQuest](https://en.wikipedia.org/wiki/Power_Quest_(video_game)) | Power Quest | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerStone](https://en.wikipedia.org/wiki/Power_Stone_(video_game)) | Power Stone | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerStone2](https://en.wikipedia.org/wiki/Power_Stone_2) | Power Stone 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerStone:C](https://en.wikipedia.org/wiki/Power_Stone_(video_game)) | Power Stone Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Pragmata](https://en.wikipedia.org/wiki/Pragmata) | Pragmata | - | - | -- -- -- | -- -- -- | -- -- --
| [Fishing:PCS]() | Pro Cast Sports Fishing | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:VPL](https://en.wikipedia.org/wiki/Professor_Layton_vs._Phoenix_Wright:_Ace_Attorney) | Professor Layton vs. Phoenix Wright: Ace Attorney | - | - | -- -- -- | -- -- -- | -- -- --
| [ProYakyuu:SJ]() | Pro Yakyuu? Satsujin Jiken! | - | - | -- -- -- | -- -- -- | -- -- --
| [Progear](https://en.wikipedia.org/wiki/Progear) | Progear | - | - | -- -- -- | -- -- -- | -- -- --
| [ProjectJustice](https://en.wikipedia.org/wiki/Project_Justice) | Project Justice | - | - | -- -- -- | -- -- -- | -- -- --
| [ProjectXZone](https://en.wikipedia.org/wiki/Project_X_Zone) | Project X Zone | - | - | -- -- -- | -- -- -- | -- -- --
| [ProjectXZone2](https://en.wikipedia.org/wiki/Project_X_Zone_2) | Project X Zone 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzLoop](https://en.wikipedia.org/wiki/Puzz_Loop) | Puzz Loop | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzLoop2](https://en.wikipedia.org/wiki/Puzz_Loop_2) | Puzz Loop 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzleFighter](https://en.wikipedia.org/wiki/Puzzle_Fighter) | Puzzle Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:D](https://en.wikipedia.org/wiki/Quiz_%26_Dragons:_Capcom_Quiz_Game) | Quiz & Dragons: Capcom Quiz Game | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:ND](https://en.wikipedia.org/wiki/Quiz_Nanairo_Dreams) | Quiz Nanairo Dreams | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:NDNK](https://en.wikipedia.org/wiki/Quiz_Nanairo_Dreams) | Quiz Nanairo Dreams: Nijiiro-cho no Kiseki | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:SGS]() | Quiz San Goku Shi | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:TY2]() | Quiz Tonosama no Yabou 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR](https://en.wikipedia.org/wiki/Red_Dead_Revolver) | Red Dead Revolver | - | - | -- -- -- | -- -- -- | -- -- --
| [RedEarth](https://en.wikipedia.org/wiki/Red_Earth_(video_game)) | Red Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [RememberMe](https://store.steampowered.com/app/228300) | Remember Me | - | - | -- -- -- | -- -- -- | -- -- --
| [RE](https://store.steampowered.com/app/304240) | Resident Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2](https://store.steampowered.com/app/883710) | Resident Evil 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2+](https://en.wikipedia.org/wiki/Resident_Evil_2_(2019_video_game)) | Resident Evil 2 (remake) | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2:SDV](https://en.wikipedia.org/wiki/Resident_Evil_2:_Dual_Shock_Version) | Resident Evil 2: Dual Shock Version | - | - | -- -- -- | -- -- -- | -- -- --
| [RE3:N](https://en.wikipedia.org/wiki/Resident_Evil_3:_Nemesis) | Resident Evil 3: Nemesis | - | - | -- -- -- | -- -- -- | -- -- --
| [RE3](https://store.steampowered.com/app/952060) | Resident Evil 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4](https://store.steampowered.com/app/254700) | Resident Evil 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4+](https://en.wikipedia.org/wiki/Resident_Evil_4_(2023_video_game)) | Resident Evil 4 (remake) | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4:HD](https://en.wikipedia.org/wiki/Resident_Evil_4) | Resident Evil 4 HD | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4:UHD](https://en.wikipedia.org/wiki/Resident_Evil_4:_Ultimate_HD_Edition) | Resident Evil 4: Ultimate HD Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4:W](https://en.wikipedia.org/wiki/Resident_Evil_4) | Resident Evil 4: Wii Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RE5](https://store.steampowered.com/app/21690) | Resident Evil 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE6](https://store.steampowered.com/app/221040) | Resident Evil 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7](https://store.steampowered.com/app/418370) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [REA:RE](https://en.wikipedia.org/wiki/Resident_Evil_(2002_video_game)) | Resident Evil Archives: Resident Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [REA:REZ](https://en.wikipedia.org/wiki/Resident_Evil_Zero) | Resident Evil Archives: Resident Evil Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [REC:V](https://en.wikipedia.org/wiki/Resident_Evil_%E2%80%93_Code:_Veronica) | Resident Evil - Code: Veronica | - | - | -- -- -- | -- -- -- | -- -- --
| [REC:VX](https://en.wikipedia.org/wiki/Resident_Evil_%E2%80%93_Code:_Veronica_X) | Resident Evil - Code: Veronica X | - | - | -- -- -- | -- -- -- | -- -- --
| [REC:VXHD](https://en.wikipedia.org/wiki/Resident_Evil_%E2%80%93_Code:_Veronica) | Resident Evil - Code: Veronica X HD | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DA](https://en.wikipedia.org/wiki/Resident_Evil:_Dead_Aim) | Resident Evil: Dead Aim | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DS](https://en.wikipedia.org/wiki/Resident_Evil:_Deadly_Silence) | Resident Evil: Deadly Silence | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:D]() | Resident Evil: Degeneration | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DC](https://en.wikipedia.org/wiki/Resident_Evil:_Director%27s_Cut) | Resident Evil: Director's Cut | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DCDSV](https://en.wikipedia.org/wiki/Resident_Evil:_Director%27s_Cut) | Resident Evil: Director's Cut Dual Shock Version | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:G](https://en.wikipedia.org/wiki/Resident_Evil_Gaiden) | Resident Evil Gaiden | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:ORC](https://en.wikipedia.org/wiki/Resident_Evil:_Operation_Raccoon_City) | Resident Evil: Operation Raccoon City | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:O](https://en.wikipedia.org/wiki/Resident_Evil_Outbreak) | Resident Evil Outbreak | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:OF2](https://en.wikipedia.org/wiki/Resident_Evil_Outbreak:_File_2) | Resident Evil Outbreak File #2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:P](https://en.wikipedia.org/wiki/Resident_Evil_Portable) | Resident Evil Portable | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R](https://store.steampowered.com/app/222480) | Resident Evil: Revelations | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R2](https://store.steampowered.com/app/287290) | Resident Evil: Revelations 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:RU](https://en.wikipedia.org/wiki/Resident_Evil:_Revelations) | Resident Evil: Revelations Unveiled Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RES](https://en.wikipedia.org/wiki/Resident_Evil_Survivor) | Resident Evil Survivor | - | - | -- -- -- | -- -- -- | -- -- --
| [RES2C:V](https://en.wikipedia.org/wiki/Resident_Evil_Survivor_2_Code:_Veronica) | Resident Evil Survivor 2 Code: Veronica | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:TDC](https://en.wikipedia.org/wiki/Resident_Evil:_The_Darkside_Chronicles) | Resident Evil: The Darkside Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:TM3D](https://en.wikipedia.org/wiki/Resident_Evil:_The_Mercenaries_3D) | Resident Evil: The Mercenaries 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:TUC](https://en.wikipedia.org/wiki/Resident_Evil:_The_Umbrella_Chronicles) | Resident Evil: The Umbrella Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:V](https://store.steampowered.com/app/1196590) | Resident Evil Village | - | - | -- -- -- | -- -- -- | -- -- --
| [REZ](https://store.steampowered.com/app/339340) | Resident Evil Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [RivalSchools:UBF](https://en.wikipedia.org/wiki/Rival_Schools:_United_By_Fate) | Rival Schools: United By Fate | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocketmen: Axis of Evil](https://en.wikipedia.org/wiki/Rocketmen:_Axis_of_Evil) | Rocketmen: Axis of Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [Rockman:FMKC](https://en.wikipedia.org/wiki/Rockman_%26_Forte_Mirai_kara_no_Chosensha) | Rockman & Forte Mirai kara no Chosensha | - | - | -- -- -- | -- -- -- | -- -- --
| [Rockman:BF](https://en.wikipedia.org/wiki/Rockman_Battle_%26_Fighters) | Rockman Battle & Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R](https://en.wikipedia.org/wiki/Rockman_Complete_Works) | Rockman Complete Works: Rockman | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R2](https://en.wikipedia.org/wiki/Rockman_Complete_Works) | Rockman Complete Works: Rockman 2 Dr. Wily no Nazo!! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R3](https://en.wikipedia.org/wiki/Rockman_Complete_Works) | Rockman Complete Works: Rockman 3 Dr. Wily no Saigo!? | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R4](https://en.wikipedia.org/wiki/Rockman_Complete_Works) | Rockman Complete Works: Rockman 4 Aratanaru Yabou!! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R5](https://en.wikipedia.org/wiki/Rockman_Complete_Works) | Rockman Complete Works: Rockman 5 Blues no Wana! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R6](https://en.wikipedia.org/wiki/Rockman_Complete_Works) | Rockman Complete Works: Rockman 6 Shijou Saidai no Tatakai!! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanEX:N1B](https://en.wikipedia.org/wiki/Rockman_EXE_N1_Battle) | Rockman EXE N1 Battle | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanEX:OSS](https://en.wikipedia.org/wiki/Rockman_EXE_Operate_Shooting_Star) | Rockman EXE Operate Shooting Star | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanEX:WS](https://en.wikipedia.org/wiki/Rockman_EXE_WS) | Rockman EXE WS | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanIQ:C]() | Rockman IQ Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [SamuraiSword]() | Samurai Sword | - | - | -- -- -- | -- -- -- | -- -- --
| [SaturdayNightSlamMasters](https://en.wikipedia.org/wiki/Saturday_Night_Slam_Masters) | Saturday Night Slam Masters | - | - | -- -- -- | -- -- -- | -- -- --
| [SectionZ](https://en.wikipedia.org/wiki/Section_Z) | Section Z | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku](https://en.wikipedia.org/wiki/Devil_Kings) | Sengoku Basara | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku2](https://en.wikipedia.org/wiki/Sengoku_Basara_2) | Sengoku Basara 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:BH](https://en.wikipedia.org/wiki/Sengoku_Basara_Battle_Heroes) | Sengoku Basara Battle Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:CH](https://en.wikipedia.org/wiki/Sengoku_Basara_Chronicle_Heroes) | Sengoku Basara Chronicle Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:X](https://en.wikipedia.org/wiki/Sengoku_Basara_X) | Sengoku Basara X | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:SH](https://en.wikipedia.org/wiki/Sengoku_Basara:_Samurai_Heroes) | Sengoku Basara: Samurai Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku4](https://en.wikipedia.org/wiki/Sengoku_Basara_4) | Sengoku Basara 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:SYD](https://en.wikipedia.org/wiki/Sengoku_Basara) | Sengoku Basara: Sanada Yukimura-Den | - | - | -- -- -- | -- -- -- | -- -- --
| [Shadow of Rome](https://en.wikipedia.org/wiki/Shadow_of_Rome) | Shadow of Rome | - | - | -- -- -- | -- -- -- | -- -- --
| [Shantae](https://en.wikipedia.org/wiki/Shantae_(video_game)) | Shantae | - | - | -- -- -- | -- -- -- | -- -- --
| [ShichiseiToushin:G](https://en.wikipedia.org/wiki/Seven_Star_Fighting_God_Guyferd) | Shichisei Toushin: Guyferd | - | - | -- -- -- | -- -- -- | -- -- --
| [ShiritsuJusticeGakuen:NSN2](https://en.wikipedia.org/wiki/Rival_Schools:_United_By_Fate#Nekketsu_Seisyun_Nikki_2) | Shiritsu Justice Gakuen: Nekketsu Seisyun Nikki 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SideArms](https://en.wikipedia.org/wiki/Side_Arms_(video_game)) | Side Arms | - | - | -- -- -- | -- -- -- | -- -- --
| [SideArms:HD](https://en.wikipedia.org/wiki/Side_Arms_Hyper_Dyne) | Side Arms Hyper Dyne | - | - | -- -- -- | -- -- -- | -- -- --
| [SideArms:S]() | Side Arms Special | - | - | -- -- -- | -- -- -- | -- -- --
| [Slipstream]() | Slipstream | - | - | -- -- -- | -- -- -- | -- -- --
| [SmurfsVillage](https://en.wikipedia.org/wiki/The_Smurfs) | Smurfs' Village | - | - | -- -- -- | -- -- -- | -- -- --
| [SmurfsGrabber](https://en.wikipedia.org/wiki/The_Smurfs) | Smurfs' Grabber | - | - | -- -- -- | -- -- -- | -- -- --
| [SmurfLife](https://en.wikipedia.org/wiki/The_Smurfs) | Smurf Life | - | - | -- -- -- | -- -- -- | -- -- --
| [SnowBrothers](https://en.wikipedia.org/wiki/Snow_Brothers) | Snow Brothers | - | - | -- -- -- | -- -- -- | -- -- --
| [SonSon](https://en.wikipedia.org/wiki/SonSon) | SonSon | - | - | -- -- -- | -- -- -- | -- -- --
| [SonSon2]() | SonSon 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Spawn:ItDH](https://en.wikipedia.org/wiki/Spawn:_In_the_Demon%27s_Hand) | Spawn: In the Demon's Hand | - | - | -- -- -- | -- -- -- | -- -- --
| [Spyborgs](https://en.wikipedia.org/wiki/Spyborgs) | Spyborgs | - | - | -- -- -- | -- -- -- | -- -- --
| [StarGladiator](https://en.wikipedia.org/wiki/Star_Gladiator) | Star Gladiator | - | - | -- -- -- | -- -- -- | -- -- --
| [StartlingAdventures:K3]() | Startling Adventures Kuso 3 נDaiboken | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelBattalion](https://en.wikipedia.org/wiki/Steel_Battalion) | Steel Battalion | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelBattalion:HA](https://en.wikipedia.org/wiki/Steel_Battalion:_Heavy_Armor) | Steel Battalion: Heavy Armor | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelBattalion:LoC](https://en.wikipedia.org/wiki/Steel_Battalion:_Line_of_Contact) | Steel Battalion: Line of Contact | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelFang]() | Steel Fang | - | - | -- -- -- | -- -- -- | -- -- --
| [Stocker](https://en.wikipedia.org/wiki/Stocker_(video_game)) | Stocker | - | - | -- -- -- | -- -- -- | -- -- --
| [SF](https://en.wikipedia.org/wiki/Street_Fighter_(video_game)) | Street Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:30AC](https://store.steampowered.com/app/586200) | Street Fighter 30th Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2010:TFF](https://en.wikipedia.org/wiki/Street_Fighter_2010:_The_Final_Fight) | Street Fighter 2010: The Final Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA](https://en.wikipedia.org/wiki/Street_Fighter_Alpha) | Street Fighter Alpha | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA2](https://en.wikipedia.org/wiki/Street_Fighter_Alpha_2) | Street Fighter Alpha 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA3](https://en.wikipedia.org/wiki/Street_Fighter_Alpha_3) | Street Fighter Alpha 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA3:M](https://en.wikipedia.org/wiki/Street_Fighter_Alpha_3) | Street Fighter Alpha 3 Max | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA:A](https://en.wikipedia.org/wiki/Street_Fighter_Alpha_Anthology) | Street Fighter Alpha Anthology | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:AC](https://en.wikipedia.org/wiki/Street_Fighter_Anniversary_Collection) | Street Fighter Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:C](https://en.wikipedia.org/wiki/Street_Fighter_Collection) | Street Fighter Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX](https://en.wikipedia.org/wiki/Street_Fighter_EX) | Street Fighter EX | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX2](https://en.wikipedia.org/wiki/Street_Fighter_EX2) | Street Fighter EX2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX2+](https://en.wikipedia.org/wiki/Street_Fighter_EX2) | Street Fighter EX 2 Plus | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX3](https://en.wikipedia.org/wiki/Street_Fighter_EX3) | Street Fighter EX3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX:A](https://en.wikipedia.org/wiki/Street_Fighter_EX) | Street Fighter EX Alpha | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX+](https://en.wikipedia.org/wiki/Street_Fighter_EX_Plus) | Street Fighter EX Plus | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:TWW](https://en.wikipedia.org/wiki/Street_Fighter_II:_The_World_Warrior) | Street Fighter II: The World Warrior | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:C](https://en.wikipedia.org/wiki/Street_Fighter_II_Champion_Edition) | Street Fighter II Champion Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:M](https://en.wikipedia.org/wiki/List_of_Street_Fighter_games#Other_games) | Street Fighter II Movie | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:SC](https://en.wikipedia.org/wiki/Street_Fighter_II_Champion_Edition) | Street Fighter II: Special Champion Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:HF](https://en.wikipedia.org/wiki/Street_Fighter_II:_Hyper_Fighting) | Street Fighter II: Hyper Fighting | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:3S](https://en.wikipedia.org/wiki/Street_Fighter_III:_3rd_Strike) | Street Fighter III: 3rd Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:3SO](https://en.wikipedia.org/wiki/Street_Fighter_III:_3rd_Strike_Online_Edition) | Street Fighter III: 3rd Strike Online Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:DI](https://en.wikipedia.org/wiki/Street_Fighter_III:_Double_Impact) | Street Fighter III: Double Impact | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:NG](https://en.wikipedia.org/wiki/Street_Fighter_III) | Street Fighter III: New Generation | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:2I](https://en.wikipedia.org/wiki/Street_Fighter_III:_2nd_Impact) | Street Fighter III: 2nd Impact | - | - | -- -- -- | -- -- -- | -- -- --
| [SF4](https://en.wikipedia.org/wiki/Street_Fighter_IV) | Street Fighter IV | - | - | -- -- -- | -- -- -- | -- -- --
| [SF4:V]() | Street Fighter IV: Volt | - | - | -- -- -- | -- -- -- | -- -- --
| [SF5](https://store.steampowered.com/app/310950) | Street Fighter V | - | - | -- -- -- | -- -- -- | -- -- --
| [SF5:AV](https://en.wikipedia.org/wiki/Street_Fighter_V) | Street Fighter V: Arcade Version | - | - | -- -- -- | -- -- -- | -- -- --
| [SF6](https://en.wikipedia.org/wiki/Street_Fighter_6) | Street Fighter 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX:MM](https://en.wikipedia.org/wiki/Street_Fighter_X_Mega_Man) | Street Fighter X Mega Man | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX:T](https://en.wikipedia.org/wiki/Street_Fighter_X_Tekken) | Street Fighter X Tekken | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ](https://en.wikipedia.org/wiki/Street_Fighter_Zero) | Street Fighter Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ2:A](https://en.wikipedia.org/wiki/Street_Fighter_Zero_2_Alpha) | Street Fighter Zero 2 Alpha | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ2:D](https://en.wikipedia.org/wiki/Street_Fighter_Alpha_2) | Street Fighter Zero 2 Dash | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ3](https://en.wikipedia.org/wiki/Street_Fighter_Zero_3) | Street Fighter Zero 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ3:U](https://en.wikipedia.org/wiki/Street_Fighter_Zero_3_Upper) | Street Fighter Zero 3 Upper | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:TMa](https://en.wikipedia.org/wiki/Street_Fighter:_The_Movie_(arcade_game)) | Street Fighter: The Movie (arcade game) | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:TMc](https://en.wikipedia.org/wiki/Street_Fighter:_The_Movie_(console_video_game)) | Street Fighter: The Movie (console video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider:A]() | Strider | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider](https://store.steampowered.com/app/235210) | Strider (2014 video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider:NES](https://en.wikipedia.org/wiki/Strider_(1989_NES_video_game)) | Strider (1989 NES video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider:HD](https://en.wikipedia.org/wiki/Strider_(2014_video_game)) | Strider HD | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider2](https://en.wikipedia.org/wiki/Strider_2_(1999_video_game)) | Strider 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [StriderII](https://en.wikipedia.org/wiki/Strider_2_(1999_video_game)) | Strider II | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperAdventureRockman](https://en.wikipedia.org/wiki/Super_Adventure_Rockman) | Super Adventure Rockman | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperBusterBros](https://en.wikipedia.org/wiki/Super_Buster_Bros.) | Super Buster Bros. | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperGhoulsGhosts](https://en.wikipedia.org/wiki/Super_Ghouls_%27n_Ghosts) | Super Ghouls 'n Ghosts | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperPang](https://en.wikipedia.org/wiki/Super_Pang) | Super Pang | - | - | -- -- -- | -- -- -- | -- -- --
| [SPF2:T](https://en.wikipedia.org/wiki/Super_Puzzle_Fighter_II_Turbo) | Super Puzzle Fighter II Turbo | - | - | -- -- -- | -- -- -- | -- -- --
| [SPF2:THD](https://en.wikipedia.org/wiki/Super_Puzzle_Fighter_II_Turbo_HD_Remix) | Super Puzzle Fighter II Turbo HD Remix | - | - | -- -- -- | -- -- -- | -- -- --
| [SPF2:X](https://en.wikipedia.org/wiki/Super_Puzzle_Fighter_II_Turbo) | Super Puzzle Fighter II X for Matching Service | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2](https://en.wikipedia.org/wiki/Super_Street_Fighter_II) | Super Street Fighter II | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:T](https://en.wikipedia.org/wiki/Super_Street_Fighter_II_Turbo) | Super Street Fighter II Turbo | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:THD](https://en.wikipedia.org/wiki/Super_Street_Fighter_II_Turbo_HD_Remix) | Super Street Fighter II Turbo HD Remix | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:XGM]() | Super Street Fighter II X Grand Master Challenge for Matching Service | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:TR](https://en.wikipedia.org/wiki/Super_Street_Fighter_II_Turbo#Game_Boy_Advance) | Super Street Fighter II: Turbo Revival | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF4](https://en.wikipedia.org/wiki/Super_Street_Fighter_IV) | Super Street Fighter IV | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF4:3D](https://en.wikipedia.org/wiki/Super_Street_Fighter_IV:_3D_Edition) | Super Street Fighter IV: 3D Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF4:A](https://en.wikipedia.org/wiki/Super_Street_Fighter_IV:_Arcade_Edition) | Super Street Fighter IV: Arcade Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SuzuMonogatari]() | Suzu Monogatari | - | - | -- -- -- | -- -- -- | -- -- --
| [SweetHome](https://en.wikipedia.org/wiki/Sweet_Home_(video_game)) | Sweet Home | - | - | -- -- -- | -- -- -- | -- -- --
| [Sydney2000](https://en.wikipedia.org/wiki/Sydney_2000_(video_game)) | Sydney 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [TNGCP:AS]() | Taisen Net Gimmick Capcom & Psikyo All Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [TaleSpin](https://en.wikipedia.org/wiki/TaleSpin_(Capcom_video_game)) | TaleSpin | - | - | -- -- -- | -- -- -- | -- -- --
| [Talisman](https://en.wikipedia.org/wiki/Talisman_(video_game)) | Talisman | - | - | -- -- -- | -- -- -- | -- -- --
| [TVC:UAS](https://en.wikipedia.org/wiki/Tatsunoko_vs._Capcom:_Ultimate_All-Stars) | Tatsunoko vs. Capcom: Ultimate All-Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [TechRomancer](https://en.wikipedia.org/wiki/Tech_Romancer) | Tech Romancer | - | - | -- -- -- | -- -- -- | -- -- --
| [TGAA:A](https://en.wikipedia.org/wiki/The_Great_Ace_Attorney:_Adventures) | The Great Ace Attorney: Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [TGAA2:R](https://en.wikipedia.org/wiki/The_Great_Ace_Attorney_2:_Resolve) | The Great Ace Attorney 2: Resolve | - | - | -- -- -- | -- -- -- | -- -- --
| [TheKingofDragons](https://en.wikipedia.org/wiki/The_King_of_Dragons) | The King of Dragons | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:LTP](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_A_Link_to_the_Past_and_Four_Swords) | The Legend of Zelda: A Link to the Past and Four Swords | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:OoA](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Oracle_of_Seasons_and_Oracle_of_Ages) | The Legend of Zelda: Oracle of Ages | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:OoS](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Oracle_of_Seasons_and_Oracle_of_Ages) | The Legend of Zelda: Oracle of Seasons | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:TMC](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_The_Minish_Cap) | The Legend of Zelda: The Minish Cap | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLittleMermaid](https://en.wikipedia.org/wiki/The_Little_Mermaid_(video_game)) | The Little Mermaid | - | - | -- -- -- | -- -- -- | -- -- --
| [TheMagicalNinja:JK]() | The Magical Ninja: Jiraiya Kenzan! | - | - | -- -- -- | -- -- -- | -- -- --
| [TMOTB](https://en.wikipedia.org/wiki/The_Misadventures_of_Tron_Bonne) | The Misadventures of Tron Bonne | - | - | -- -- -- | -- -- -- | -- -- --
| [TheMaw](https://en.wikipedia.org/wiki/The_Maw_(video_game)) | The Maw | - | - | -- -- -- | -- -- -- | -- -- --
| [TNBC:OR](https://en.wikipedia.org/wiki/The_Nightmare_Before_Christmas:_Oogie%27s_Revenge) | The Nightmare Before Christmas: Oogie's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [ThePunisher](https://en.wikipedia.org/wiki/The_Punisher_(1993_video_game)) | The Punisher | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSpeedRumbler](https://en.wikipedia.org/wiki/The_Speed_Rumbler) | The Speed Rumbler | - | - | -- -- -- | -- -- -- | -- -- --
| [ThreeWonders](https://en.wikipedia.org/wiki/Three_Wonders) | Three Wonders | - | - | -- -- -- | -- -- -- | -- -- --
| [TigerRoad](https://en.wikipedia.org/wiki/Tiger_Road) | Tiger Road | - | - | -- -- -- | -- -- -- | -- -- --
| [TokiTori](https://en.wikipedia.org/wiki/Toki_Tori) | Toki Tori | - | - | -- -- -- | -- -- -- | -- -- --
| [TombRaider:TLR](https://en.wikipedia.org/wiki/Tomb_Raider:_The_Last_Revelation) | Tomb Raider: The Last Revelation | - | - | -- -- -- | -- -- -- | -- -- --
| [ToyStory](https://en.wikipedia.org/wiki/Toy_Story_(video_game)) | Toy Story | - | - | -- -- -- | -- -- -- | -- -- --
| [Snowboard:T](https://en.wikipedia.org/wiki/Trick%27N_Snowboarder) | Trick'N Snowboarder | - | - | -- -- -- | -- -- -- | -- -- --
| [Trojan](https://en.wikipedia.org/wiki/Trojan_(video_game)) | Trojan | - | - | -- -- -- | -- -- -- | -- -- --
| [Trouballs](https://en.wikipedia.org/wiki/Trouballs) | Trouballs | - | - | -- -- -- | -- -- -- | -- -- --
| [Turok](https://en.wikipedia.org/wiki/Turok_(video_game)) | Turok | - | - | -- -- -- | -- -- -- | -- -- --
| [UNSquadron](https://en.wikipedia.org/wiki/U.N._Squadron) | U.N. Squadron | - | - | -- -- -- | -- -- -- | -- -- --
| [UFC](https://en.wikipedia.org/wiki/Ultimate_Fighting_Championship_(video_game)) | Ultimate Fighting Championship | - | - | -- -- -- | -- -- -- | -- -- --
| [UGhostsGoblins](https://en.wikipedia.org/wiki/Ultimate_Ghosts_%27n_Goblins) | Ultimate Ghosts 'n Goblins | - | - | -- -- -- | -- -- -- | -- -- --
| [UMVC3](https://store.steampowered.com/app/357190) | Ultimate Marvel vs. Capcom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [UmbrellaCorps](https://store.steampowered.com/app/390340) | Umbrella Corps | - | - | -- -- -- | -- -- -- | -- -- --
| [UnderTheSkin](https://en.wikipedia.org/wiki/Under_the_Skin_(video_game)) | Under the Skin | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:C](https://en.wikipedia.org/wiki/Darkstalkers_Chronicle:_The_Chaos_Tower) | Vampire Chronicles for Matching Service | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:H2](https://en.wikipedia.org/wiki/Darkstalkers_3#Updates) | Vampire Hunter 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:S](https://en.wikipedia.org/wiki/Darkstalkers_3) | Vampire Savior | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:S2](https://en.wikipedia.org/wiki/Darkstalkers_3#Updates) | Vampire Savior 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:DC](https://en.wikipedia.org/wiki/Vampire:_Darkstalkers_Collection) | Vampire: Darkstalkers Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Varth:OT](https://en.wikipedia.org/wiki/Varth:_Operation_Thunderstorm) | Varth: Operation Thunderstorm | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe](https://en.wikipedia.org/wiki/Viewtiful_Joe_(video_game)) | Viewtiful Joe | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe2](https://en.wikipedia.org/wiki/Viewtiful_Joe_2) | Viewtiful Joe 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe:DT](https://en.wikipedia.org/wiki/Viewtiful_Joe:_Double_Trouble!) | Viewtiful Joe: Double Trouble! | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe:RHR](https://en.wikipedia.org/wiki/Viewtiful_Joe:_Red_Hot_Rumble) | Viewtiful Joe: Red Hot Rumble | - | - | -- -- -- | -- -- -- | -- -- --
| [Vulgus](https://en.wikipedia.org/wiki/Vulgus) | Vulgus | - | - | -- -- -- | -- -- -- | -- -- --
| [Wantame:DDS]() | Wantame Music Channel: Doko Demo Style | - | - | -- -- -- | -- -- -- | -- -- --
| [WOTF](https://en.wikipedia.org/wiki/War_of_the_Grail) | War of the Grail | - | - | -- -- -- | -- -- -- | -- -- --
| [Warauinu:SGL]() | Warauinu no Bouken GB: Silly Go Lucky! | - | - | -- -- -- | -- -- -- | -- -- --
| [WOF](https://en.wikipedia.org/wiki/Warriors_of_Fate) | Warriors of Fate | - | - | -- -- -- | -- -- -- | -- -- --
| [WOF2](https://en.wikipedia.org/wiki/Warriors_of_Fate) | Warriors of Fate II | - | - | -- -- -- | -- -- -- | -- -- --
| [WOTS2](https://en.wikipedia.org/wiki/Way_of_the_Samurai_2) | Way of the Samurai 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Golf:WL](https://en.wikipedia.org/wiki/We_Love_Golf!) | We Love Golf! | - | - | -- -- -- | -- -- -- | -- -- --
| [WFRR](https://en.wikipedia.org/wiki/Who_Framed_Roger_Rabbit_(1991_video_game)) | Who Framed Roger Rabbit | - | - | -- -- -- | -- -- -- | -- -- --
| [WWTBAM?]() | Who Wants to Be a Millionaire? | - | - | -- -- -- | -- -- -- | -- -- --
| [Willow](https://en.wikipedia.org/wiki/Willow_(video_game)) | Willow | - | - | -- -- -- | -- -- -- | -- -- --
| [WilyRight:NRTP](https://en.wikipedia.org/wiki/Wily_%26_Right_no_RockBoard:_That%27s_Paradise) | Wily & Right no RockBoard: That's Paradise | - | - | -- -- -- | -- -- -- | -- -- --
| [WithoutWarning](https://en.wikipedia.org/wiki/Without_Warning_(video_game)) | Without Warning | - | - | -- -- -- | -- -- -- | -- -- --
| [WizardryV](https://en.wikipedia.org/wiki/Wizardry) | Wizardry V | - | - | -- -- -- | -- -- -- | -- -- --
| [WOTB:C3](https://en.wikipedia.org/wiki/Wolf_of_the_Battlefield:_Commando_3) | Wolf of the Battlefield: Commando 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [WorldGoneSour](https://en.wikipedia.org/wiki/World_Gone_Sour) | World Gone Sour | - | - | -- -- -- | -- -- -- | -- -- --
| [X-Men:VSF](https://en.wikipedia.org/wiki/X-Men_vs._Street_Fighter) | X-Men vs. Street Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [X-Men:VSFEX](https://en.wikipedia.org/wiki/X-Men_vs._Street_Fighter) | X-Men vs. Street Fighter EX Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [XMen:CotA](https://en.wikipedia.org/wiki/X-Men:_Children_of_the_Atom_(video_game)) | X-Men: Children of the Atom | - | - | -- -- -- | -- -- -- | -- -- --
| [XMen:MA](https://en.wikipedia.org/wiki/X-Men:_Mutant_Apocalypse) | X-Men: Mutant Apocalypse | - | - | -- -- -- | -- -- -- | -- -- --
| [X2:NR](https://en.wikipedia.org/wiki/X2_(video_game)) | X2: No Relief | - | - | -- -- -- | -- -- -- | -- -- --
| [YoNoid](https://en.wikipedia.org/wiki/Yo!_Noid) | Yo! Noid | - | - | -- -- -- | -- -- -- | -- -- --
| [ZackWiki:QfBT](https://en.wikipedia.org/wiki/Zack_%26_Wiki:_Quest_for_Barbaros%27_Treasure) | Zack & Wiki: Quest for Barbaros' Treasure | - | - | -- -- -- | -- -- -- | -- -- --
| [ZombieCafe](https://en.wikipedia.org/wiki/Zombie_Cafe) | Zombie Cafe | - | - | -- -- -- | -- -- -- | -- -- --
| **Cig** | **Cloud Imperium Games**
| [StarCitizen](https://robertsspaceindustries.com/playstarcitizen) | Star Citizen | - | - | -- -- -- | -- -- -- | -- -- --
| **Cryptic** | **Cryptic**
| [COH](https://en.wikipedia.org/wiki/City_of_Heroes) | City of Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [CO](https://store.steampowered.com/app/9880) | Champions Online | open | read | -- -- -- | -- -- -- | -- -- --
| [STO](https://store.steampowered.com/app/9900) | Star Trek Online | open | read | -- -- -- | -- -- -- | -- -- --
| [NVW](https://store.steampowered.com/app/109600) | Neverwinter | open | read | -- -- -- | -- -- -- | -- -- --
| [MTG](https://en.wikipedia.org/wiki/Magic:_Legends) | Magic: The Gathering | - | - | -- -- -- | -- -- -- | -- -- --
| **Crytek** | **Crytek**
| [ArcheAge](https://store.steampowered.com/app/304030) | ArcheAge | - | - | -- -- -- | -- -- -- | -- -- --
| [Hunt](https://store.steampowered.com/app/594650) | Hunt: Showdown | - | - | -- -- -- | -- -- -- | -- -- --
| [MWO](https://store.steampowered.com/app/342200) | MechWarrior Online | - | - | -- -- -- | -- -- -- | -- -- --
| [Warface](https://store.steampowered.com/app/291480) | Warface | - | - | -- -- -- | -- -- -- | -- -- --
| [Wolcen](https://store.steampowered.com/app/424370) | Wolcen: Lords of Mayhem | - | - | -- -- -- | -- -- -- | -- -- --
| [Crysis](https://store.steampowered.com/app/1715130) | Crysis Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [Ryse](https://store.steampowered.com/app/302510) | Ryse: Son of Rome | - | - | -- -- -- | -- -- -- | -- -- --
| [Robinson](https://store.steampowered.com/app/579820) | Robinson: The Journey | - | - | -- -- -- | -- -- -- | -- -- --
| [Snow](https://store.steampowered.com/app/244930) | SNOW - The Ultimate Edition | - | - | -- -- -- | -- -- -- | -- -- --
| **Cyanide** | **Cyanide**
| [TC](https://store.steampowered.com/app/287630) | The Council | - | - | -- -- -- | -- -- -- | -- -- --
| [Werewolf:TA](https://store.steampowered.com/app/679110) | Werewolf: The Apocalypse - Earthblood | - | - | -- -- -- | -- -- -- | -- -- --
| **EA** | **Electronic Arts**
| **Epic** | **Epic**
| [UE1](https://oldgamesdownload.com/unreal) | Unreal | - | - | -- -- -- | -- -- -- | -- -- --
| [TWoT](https://www.gog.com/en/game/the_wheel_of_time) | The Wheel of Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx](https://www.gog.com/en/game/deus_ex) | Deus Ex™ GOTY Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx:MD](https://www.gog.com/en/game/deus_ex_mankind_divided) | Deus Ex: Mankind Divided | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx2:IW](https://www.gog.com/en/game/deus_ex_invisible_war) | Deus Ex 2: Invisible War | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx:HR](https://www.gog.com/en/game/deus_ex_human_revolution_directors_cut) | Deus Ex: Human Revolution - Director's Cut | - | - | -- -- -- | -- -- -- | -- -- --
| [Rune](https://www.gog.com/en/game/rune_classic) | Rune | - | - | -- -- -- | -- -- -- | -- -- --
| [Undying](https://www.gog.com/en/game/clive_barkers_undying) | Clive Barker's Undying | - | - | -- -- -- | -- -- -- | -- -- --
| [UT2K](https://oldgamesdownload.com/unreal-tournament-2003/) | Unreal Tournament 2003 | - | - | -- -- -- | -- -- -- | -- -- --
| [UE2](https://store.steampowered.com/app/13200/Unreal_2_The_Awakening/) | Unreal II: The Awakening | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock](https://store.steampowered.com/app/7670) | BioShock | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShockR](https://store.steampowered.com/app/409710) | BioShock Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2](https://store.steampowered.com/app/8850) | BioShock 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2R](https://store.steampowered.com/app/409720) | BioShock 2 Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock:Inf](https://store.steampowered.com/app/8870) | BioShock Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| **Frictional** | **HPL Engine**
| [P:O](https://store.steampowered.com/app/22180) | Penumbra: Overture | - | - | -- -- -- | -- -- -- | -- -- --
| [P:BP](https://store.steampowered.com/app/22120) | Penumbra: Black Plague | - | - | -- -- -- | -- -- -- | -- -- --
| [P:R](https://store.steampowered.com/app/22140) | Penumbra: Requiem | - | - | -- -- -- | -- -- -- | -- -- --
| [A:TDD](https://store.steampowered.com/app/57300) | Amnesia: The Dark Descent | - | - | -- -- -- | -- -- -- | -- -- --
| [A:J]() | Amnesia: Justine | - | - | -- -- -- | -- -- -- | -- -- --
| [A:AMFP](https://store.steampowered.com/app/239200) | Amnesia: A Machine for Pigs | - | - | -- -- -- | -- -- -- | -- -- --
| [SOMA](https://store.steampowered.com/app/282140) | SOMA | - | - | -- -- -- | -- -- -- | -- -- --
| [A:R](https://store.steampowered.com/app/999220) | Amnesia: Rebirth | - | - | -- -- -- | -- -- -- | -- -- --
| **Frontier** | **Frontier Developments**
| [FE](https://en.wikipedia.org/wiki/Frontier:_First_Encounters) | Frontier: First Encounters | - | - | -- -- -- | -- -- -- | -- -- --
| [DX](https://en.wikipedia.org/wiki/Darxide) | Darxide | - | - | -- -- -- | -- -- -- | -- -- --
| [V2K](https://en.wikipedia.org/wiki/Zarch#V2000) | V2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [IF]() | Infestation | - | - | -- -- -- | -- -- -- | -- -- --
| [DX:EMP](https://en.wikipedia.org/wiki/Darxide) | Darxide EMP | - | - | -- -- -- | -- -- -- | -- -- --
| [RTX](https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_(video_game)) | RollerCoaster Tycoon (Xbox port) | - | - | -- -- -- | -- -- -- | -- -- --
| [WG:PZ](https://en.wikipedia.org/wiki/Wallace_%26_Gromit_in_Project_Zoo) | Wallace & Gromit in Project Zoo | - | - | -- -- -- | -- -- -- | -- -- --
| [DL](https://en.wikipedia.org/wiki/Dog%27s_Life) | Dog's Life | - | - | -- -- -- | -- -- -- | -- -- --
| [RT2:WW](https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_2) | RollerCoaster Tycoon 2: Wacky Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [RT2:TT](https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_2) | RollerCoaster Tycoon 2: Time Twister | - | - | -- -- -- | -- -- -- | -- -- --
| [RT3](https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_3) | RollerCoaster Tycoon 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [RT3:S](https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_3) | RollerCoaster Tycoon 3: Soaked! | - | - | -- -- -- | -- -- -- | -- -- --
| [RT3:W](https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_3) | RollerCoaster Tycoon 3: Wild! | - | - | -- -- -- | -- -- -- | -- -- --
| [WG:TCotWR](https://en.wikipedia.org/wiki/Wallace_%26_Gromit:_The_Curse_of_the_Were-Rabbit_(video_game)) | Wallace & Gromit: The Curse of the Were-Rabbit | - | - | -- -- -- | -- -- -- | -- -- --
| [TV](https://en.wikipedia.org/wiki/Thrillville) | Thrillville | - | - | -- -- -- | -- -- -- | -- -- --
| [TV:OtR](https://en.wikipedia.org/wiki/Thrillville:_Off_the_Rails) | Thrillville: Off the Rails | - | - | -- -- -- | -- -- -- | -- -- --
| [LW](https://store.steampowered.com/app/447780) | LostWinds | - | - | -- -- -- | -- -- -- | -- -- --
| [LW2](https://store.steampowered.com/app/447800) | LostWinds 2: Winter of the Melodias | - | - | -- -- -- | -- -- -- | -- -- --
| [KT](https://en.wikipedia.org/wiki/Kinectimals) | Kinectimals | - | - | -- -- -- | -- -- -- | -- -- --
| [KT:NwB](https://en.wikipedia.org/wiki/Kinectimals) | Kinectimals: Now with Bears! | - | - | -- -- -- | -- -- -- | -- -- --
| [KT:DA](https://en.wikipedia.org/wiki/Kinect:_Disneyland_Adventures) | Kinect: Disneyland Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [CC]() | Coaster Crazy | - | - | -- -- -- | -- -- -- | -- -- --
| [ZT](https://en.wikipedia.org/wiki/Zoo_Tycoon_(2013_video_game)) | Zoo Tycoon | - | - | -- -- -- | -- -- -- | -- -- --
| [CCD]() | Coaster Crazy Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| [TFDS]() | Tales from Deep Space | - | - | -- -- -- | -- -- -- | -- -- --
| [ED](https://store.steampowered.com/app/359320) | Elite: Dangerous | - | - | -- -- -- | -- -- -- | -- -- --
| [SR](https://en.wikipedia.org/wiki/Screamride) | Screamride | - | - | -- -- -- | -- -- -- | -- -- --
| [ED:H](https://en.wikipedia.org/wiki/Elite_Dangerous#Horizons_season_of_expansions) | Elite Dangerous: Horizons | - | - | -- -- -- | -- -- -- | -- -- --
| [ED:A](https://en.wikipedia.org/wiki/Elite_Dangerous#Elite_Dangerous:_Arena) | Elite Dangerous: Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [PC](https://store.steampowered.com/app/493340) | Planet Coaster | - | - | -- -- -- | -- -- -- | -- -- --
| [JW](https://store.steampowered.com/app/648350) | Jurassic World Evolution | - | - | -- -- -- | -- -- -- | -- -- --
| [PZ](https://store.steampowered.com/app/703080) | Planet Zoo | - | - | -- -- -- | -- -- -- | -- -- --
| [JW2](https://store.steampowered.com/app/1244460) | Jurassic World Evolution 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:22](https://store.steampowered.com/app/1708520) | F1 Manager 2022 | - | - | -- -- -- | -- -- -- | -- -- --
| [W4K:CG](https://store.steampowered.com/app/1611910) | Warhammer 40,000: Chaos Gate - Daemonhunters | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:23](https://store.steampowered.com/app/2287220) | F1 Manager 2023 | - | - | -- -- -- | -- -- -- | -- -- --
| [W4K:AoS:RoR](https://en.wikipedia.org/wiki/Warhammer_Age_of_Sigmar) | Warhammer Age of Sigmar: Realms of Ruin | - | - | -- -- -- | -- -- -- | -- -- --
| **Gamebryo** | **Gamebryo**
| [PoP](https://en.wikipedia.org/wiki/Prince_of_Persia_3D) | Prince of Persia 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [WWW:TSA](https://en.wikipedia.org/wiki/Wild_Wild_West:_The_Steel_Assassin) | Wild Wild West: The Steel Assassin | - | - | -- -- -- | -- -- -- | -- -- --
| [DAoC](https://en.wikipedia.org/wiki/Dark_Age_of_Camelot) | Dark Age of Camelot | - | - | -- -- -- | -- -- -- | -- -- --
| [Oddworld:MO](https://en.wikipedia.org/wiki/Oddworld:_Munch%27s_Oddysee) | Oddworld: Munch's Oddysee | - | - | -- -- -- | -- -- -- | -- -- --
| [TetrisWorlds](https://en.wikipedia.org/wiki/Tetris_Worlds) | Tetris Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [Morrowind](https://en.wikipedia.org/wiki/The_Elder_Scrolls_III:_Morrowind) | The Elder Scrolls III: Morrowind | - | - | -- -- -- | -- -- -- | -- -- --
| [FreedomForce](https://en.wikipedia.org/wiki/Freedom_Force_(computer_game)) | Freedom Force | - | - | -- -- -- | -- -- -- | -- -- --
| [STS](https://en.wikipedia.org/wiki/Simon_the_Sorcerer_3D) | Simon The Sorcerer 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [ST:BC](https://en.wikipedia.org/wiki/Star_Trek:_Bridge_Commander) | Star Trek: Bridge Commander | - | - | -- -- -- | -- -- -- | -- -- --
| [Futurama](https://en.wikipedia.org/wiki/Futurama_(video_game)) | Futurama | - | - | -- -- -- | -- -- -- | -- -- --
| [ZooTycoon2](https://en.wikipedia.org/wiki/Zoo_Tycoon_2) | Zoo Tycoon 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [GrandChase](https://en.wikipedia.org/wiki/Grand_Chase) | Grand Chase | - | - | -- -- -- | -- -- -- | -- -- --
| [IHRA](https://en.wikipedia.org/wiki/IHRA_Professional_Drag_Racing_2005) | IHRA Professional Drag Racing 2005 | - | - | -- -- -- | -- -- -- | -- -- --
| [SidMeiersPirates](https://en.wikipedia.org/wiki/Sid_Meier%27s_Pirates!_(2004_video_game)) | Sid Meier's Pirates! | - | - | -- -- -- | -- -- -- | -- -- --
| [Civ4](https://en.wikipedia.org/wiki/Civilization_IV) | Civilization IV | - | - | -- -- -- | -- -- -- | -- -- --
| [IHRA:SE](https://en.wikipedia.org/wiki/IHRA_Drag_Racing:_Sportsman_Edition) | IHRA Drag Racing: Sportsman Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion](https://en.wikipedia.org/wiki/The_Elder_Scrolls_IV:_Oblivion) | The Elder Scrolls IV: Oblivion | - | - | -- -- -- | -- -- -- | -- -- --
| [SidMeiersRailroads](https://en.wikipedia.org/wiki/Sid_Meier%27s_Railroads!) | Sid Meier's Railroads! | - | - | -- -- -- | -- -- -- | -- -- --
| [DriftCity](https://en.wikipedia.org/wiki/Drift_City) | Drift City | - | - | -- -- -- | -- -- -- | -- -- --
| [ShinMegamiTensei:I](https://en.wikipedia.org/wiki/Shin_Megami_Tensei:_Imagine) | Shin Megami Tensei: Imagine | - | - | -- -- -- | -- -- -- | -- -- --
| [AtlanticaOnline](https://en.wikipedia.org/wiki/Atlantica_Online) | Atlantica Online | - | - | -- -- -- | -- -- -- | -- -- --
| [BlackShot](https://en.wikipedia.org/wiki/BlackShot) | BlackShot | - | - | -- -- -- | -- -- -- | -- -- --
| [Bully:SE](https://en.wikipedia.org/wiki/Bully:_Scholarship_Edition) | Bully: Scholarship Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [CivR](https://en.wikipedia.org/wiki/Civilization_Revolution) | Civilization Revolution | - | - | -- -- -- | -- -- -- | -- -- --
| [DefenseGrid:TA](https://en.wikipedia.org/wiki/Defense_Grid:_The_Awakening) | Defense Grid: The Awakening | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout3](https://en.wikipedia.org/wiki/Fallout_3) | Fallout 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SpeedRacer:TV](https://en.wikipedia.org/wiki/Speed_Racer:_The_Videogame) | Speed Racer: The Videogame | - | - | -- -- -- | -- -- -- | -- -- --
| [Tenchu:SA](https://en.wikipedia.org/wiki/Tenchu:_Shadow_Assassins) | Tenchu: Shadow Assassins | - | - | -- -- -- | -- -- -- | -- -- --
| [WO:AoR](https://en.wikipedia.org/wiki/Warhammer_Online:_Age_of_Reckoning) | Warhammer Online: Age of Reckoning | - | - | -- -- -- | -- -- -- | -- -- --
| [WotS3](https://en.wikipedia.org/wiki/Way_of_the_Samurai_3) | Way of the Samurai 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Wizard101](https://en.wikipedia.org/wiki/Wizard101) | Wizard101 | - | - | -- -- -- | -- -- -- | -- -- --
| [Dragonica](https://en.wikipedia.org/wiki/Dragonica) | Dragonica | - | - | -- -- -- | -- -- -- | -- -- --
| [Jeopardy](https://en.wikipedia.org/wiki/Jeopardy!_(video_game)) | Jeopardy! | - | - | -- -- -- | -- -- -- | -- -- --
| [WoF](https://en.wikipedia.org/wiki/Wheel_of_Fortune_video_games) | Wheel of Fortune | - | - | -- -- -- | -- -- -- | -- -- --
| [DoB](https://en.wikipedia.org/wiki/Dance_on_Broadway) | Dance on Broadway | - | - | -- -- -- | -- -- -- | -- -- --
| [Divinity2](https://en.wikipedia.org/wiki/Divinity_II) | Divinity II | - | - | -- -- -- | -- -- -- | -- -- --
| [EpicMickey](https://en.wikipedia.org/wiki/Epic_Mickey) | Epic Mickey | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout:NV](https://en.wikipedia.org/wiki/Fallout:_New_Vegas) | Fallout: New Vegas | - | - | -- -- -- | -- -- -- | -- -- --
| [GujianQitan](https://en.wikipedia.org/wiki/Gujian_Qitan) | Gujian Qitan | - | - | -- -- -- | -- -- -- | -- -- --
| [LEGOUniverse](https://en.wikipedia.org/wiki/LEGO_Universe) | LEGO Universe | - | - | -- -- -- | -- -- -- | -- -- --
| [MicroVolts](https://en.wikipedia.org/wiki/MicroVolts) | MicroVolts | - | - | -- -- -- | -- -- -- | -- -- --
| [Splatterhouse](https://en.wikipedia.org/wiki/Splatterhouse_(2010_video_game)) | Splatterhouse | - | - | -- -- -- | -- -- -- | -- -- --
| [Catherine](https://en.wikipedia.org/wiki/Catherine_(video_game)) | Catherine | - | - | -- -- -- | -- -- -- | -- -- --
| [Dawntide](https://en.wikipedia.org/wiki/Dawntide) | Dawntide (Cancelled) | - | - | -- -- -- | -- -- -- | -- -- --
| [EdenEternal](https://en.wikipedia.org/wiki/Eden_Eternal) | Eden Eternal | - | - | -- -- -- | -- -- -- | -- -- --
| [ElShaddai:AotM](https://en.wikipedia.org/wiki/El_Shaddai:_Ascension_of_the_Metatron) | El Shaddai: Ascension of the Metatron | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerUpHeroes](https://en.wikipedia.org/wiki/PowerUp_Heroes) | PowerUp Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Ragnarok2:LotS](https://en.wikipedia.org/wiki/Ragnarok_Online_2:_Legend_of_the_Second) | Ragnarok Online II: Legend of the Second | - | - | -- -- -- | -- -- -- | -- -- --
| [Rift](https://en.wikipedia.org/wiki/Rift_(video_game)) | Rift | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith](https://en.wikipedia.org/wiki/Rocksmith) | Rocksmith | - | - | -- -- -- | -- -- -- | -- -- --
| [YarsRevenge](https://en.wikipedia.org/wiki/Yar%27s_Revenge) | Yar's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [Epic Mickey 2: The Power of Two](XXX) | Epic Mickey 2: The Power of Two | - | - | -- -- -- | -- -- -- | -- -- --
| [Pirate101](https://en.wikipedia.org/wiki/Pirate101) | Pirate101 | - | - | -- -- -- | -- -- -- | -- -- --
| [WO:WoH](https://en.wikipedia.org/wiki/Warhammer_Online:_Wrath_of_Heroes) | Warhammer Online: Wrath of Heroes (Canceled) | - | - | -- -- -- | -- -- -- | -- -- --
| [Defiance](https://en.wikipedia.org/wiki/Defiance_(video_game)) | Defiance | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith2014](https://en.wikipedia.org/wiki/Rocksmith_2014) | Rocksmith 2014 | - | - | -- -- -- | -- -- -- | -- -- --
| [GitS:SAC](https://en.wikipedia.org/wiki/Ghost_in_the_Shell:_Stand_Alone_Complex_-_First_Assault_Online) | Ghost in the Shell: Stand Alone Complex - First Assault Online | - | - | -- -- -- | -- -- -- | -- -- --
| [MapleStory2](https://en.wikipedia.org/wiki/MapleStory_2) | MapleStory 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [OblivionR](https://en.wikipedia.org/wiki/The_Elder_Scrolls_IV:_Oblivion_Remastered) | The Elder Scrolls IV: Oblivion Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| **ID** | **ID**
| [CK:IOTV](https://en.wikipedia.org/wiki/Commander_Keen_in_Invasion_of_the_Vorticons) | Commander Keen in Invasion of the Vorticons | - | - | -- -- -- | -- -- -- | -- -- --
| [SK](https://en.wikipedia.org/wiki/Shadow_Knights) | Shadow Knights | - | - | -- -- -- | -- -- -- | -- -- --
| [HT3D](https://en.wikipedia.org/wiki/Hovertank_3D) | Hovertank 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [DD:THM](https://en.wikipedia.org/wiki/Dangerous_Dave_in_the_Haunted_Mansion) | Dangerous Dave in the Haunted Mansion | - | - | -- -- -- | -- -- -- | -- -- --
| [RR](https://en.wikipedia.org/wiki/Rescue_Rover) | Rescue Rover | - | - | -- -- -- | -- -- -- | -- -- --
| [CK:KD](https://en.wikipedia.org/wiki/Commander_Keen_in_Keen_Dreams) | Commander Keen in Keen Dreams | - | - | -- -- -- | -- -- -- | -- -- --
| [RR2]() | Rescue Rover 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [C3D](https://en.wikipedia.org/wiki/Catacomb_3-D) | Catacomb 3-D | - | - | -- -- -- | -- -- -- | -- -- --
| [CK:GG](https://en.wikipedia.org/wiki/Commander_Keen_in_Goodbye,_Galaxy) | Commander Keen in Goodbye, Galaxy | - | - | -- -- -- | -- -- -- | -- -- --
| [CK:AAMB](https://en.wikipedia.org/wiki/Commander_Keen_in_Aliens_Ate_My_Babysitter) | Commander Keen in Aliens Ate My Babysitter | - | - | -- -- -- | -- -- -- | -- -- --
| [W3D](https://en.wikipedia.org/wiki/Wolfenstein_3D) | Wolfenstein 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TOTD](https://en.wikipedia.org/wiki/Tiles_of_the_Dragon) | Tiles of the Dragon | - | - | -- -- -- | -- -- -- | -- -- --
| [D1](https://en.wikipedia.org/wiki/Doom_(1993_video_game)) | Doom | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](https://en.wikipedia.org/wiki/Doom_II) | Doom II | - | - | -- -- -- | -- -- -- | -- -- --
| [Q](https://store.steampowered.com/app/2310) | Quake | - | - | -- -- -- | -- -- -- | -- -- --
| [Q2](https://store.steampowered.com/app/2320) | Quake II | - | - | -- -- -- | -- -- -- | -- -- --
| [Q3](https://store.steampowered.com/app/0) | Quake III Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://store.steampowered.com/app/9050) | Doom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [D:RPG](https://en.wikipedia.org/wiki/Doom_RPG) | Doom RPG | - | - | -- -- -- | -- -- -- | -- -- --
| [OE](https://en.wikipedia.org/wiki/Orcs_%26_Elves) | Orcs & Elves | - | - | -- -- -- | -- -- -- | -- -- --
| [QE2](https://doomwiki.org/wiki/Orcs_%26_Elves_II) | Orcs & Elves II | - | - | -- -- -- | -- -- -- | -- -- --
| [W:RPG](https://en.wikipedia.org/wiki/Wolfenstein_RPG) | Wolfenstein RPG | - | - | -- -- -- | -- -- -- | -- -- --
| [D2:RPG](https://en.wikipedia.org/wiki/Doom_II_RPG) | Doom II RPG | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:L](https://store.steampowered.com/app/282440) | Quake Live | - | - | -- -- -- | -- -- -- | -- -- --
| [R:MBT](https://en.wikipedia.org/wiki/Rage:_Mutant_Bash_TV) | Rage: Mutant Bash TV | - | - | -- -- -- | -- -- -- | -- -- --
| [R](https://store.steampowered.com/app/9200) | Rage | - | - | -- -- -- | -- -- -- | -- -- --
| [D](https://store.steampowered.com/app/0) | Doom (2016) | - | - | -- -- -- | -- -- -- | -- -- --
| [D:VFR](https://store.steampowered.com/app/650000) | Doom VFR | - | - | -- -- -- | -- -- -- | -- -- --
| [R2](https://store.steampowered.com/app/548570) | Rage 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D:E](https://store.steampowered.com/app/0) | Doom Eternal | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:C](https://store.steampowered.com/app/611500) | Quake Champions | - | - | -- -- -- | -- -- -- | -- -- --
| **IW** | **Infinity Ward**
| [COD](https://store.steampowered.com/app/2620) | Call of Duty | - | - | -- -- -- | -- -- -- | -- -- --
| [COD2](https://store.steampowered.com/app/2630) | Call of Duty 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [COD3]() | Call of Duty 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [COD4](https://store.steampowered.com/app/7940) | Call of Duty 4: Modern Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [007:QoS]() | 007: Quantum of Solace | - | - | -- -- -- | -- -- -- | -- -- --
| [WaW](https://store.steampowered.com/app/10090) | Call of Duty: World at War | - | - | -- -- -- | -- -- -- | -- -- --
| [MW2](https://store.steampowered.com/app/10180) | Call of Duty: Modern Warfare 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BO](https://store.steampowered.com/app/42700) | Call of Duty: Black Ops | - | - | -- -- -- | -- -- -- | -- -- --
| [MW3](https://store.steampowered.com/app/42680) | Call of Duty: Modern Warfare 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [BO2](https://store.steampowered.com/app/202970) | Call of Duty: Black Ops II | - | - | -- -- -- | -- -- -- | -- -- --
| [Ghosts](https://store.steampowered.com/app/209160) | Call of Duty: Ghosts | - | - | -- -- -- | -- -- -- | -- -- --
| [AW](https://store.steampowered.com/app/209650) | Call of Duty: Advanced Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [BO3](https://store.steampowered.com/app/311210) | Call of Duty: Black Ops III | - | - | -- -- -- | -- -- -- | -- -- --
| [IW](https://store.steampowered.com/app/292730) | Call of Duty: Infinite Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [WWII](https://store.steampowered.com/app/476600) | Call of Duty: WWII | - | - | -- -- -- | -- -- -- | -- -- --
| [BO4](https://us.shop.battle.net/en-us/product/call-of-duty-black-ops-4) | Call of Duty: Black Ops 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MW](https://store.steampowered.com/app/393080) | Call of Duty: Modern Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [BOCW](https://us.shop.battle.net/en-us/product/call-of-duty-black-ops-cold-war) | Call of Duty: Black Ops Cold War | - | - | -- -- -- | -- -- -- | -- -- --
| [Vanguard](https://us.shop.battle.net/en-us/product/call-of-duty-vanguard) | Call of Duty: Vanguard | - | - | -- -- -- | -- -- -- | -- -- --
| [COD:MW2](https://store.steampowered.com/app/1938090) | Call of Duty: Modern Warfare II | - | - | -- -- -- | -- -- -- | -- -- --
| **Lucas** | **Lucas Arts**
| [B](https://en.wikipedia.org/wiki/Ballblazer) | Ballblazer | - | - | -- -- -- | -- -- -- | -- -- --
| [RoF](https://www.myabandonware.com/game/rescue-on-fractalus-528) | Rescue on Fractalus! | - | - | -- -- -- | -- -- -- | -- -- --
| [TE](https://www.myabandonware.com/game/the-eidolon-5ia) | The Eidolon | - | - | -- -- -- | -- -- -- | -- -- --
| [H](https://en.wikipedia.org/wiki/Habitat_(video_game)) | Habitat | - | - | -- -- -- | -- -- -- | -- -- --
| [KR](https://www.myabandonware.com/game/koronis-rift-5y0) | Koronis Rift | - | - | -- -- -- | -- -- -- | -- -- --
| [L:TCG](https://www.myabandonware.com/game/labyrinth-601) | Labyrinth: The Computer Game | - | - | -- -- -- | -- -- -- | -- -- --
| [PP](https://www.myabandonware.com/game/phm-pegasus-hr) | PHM Pegasus | - | - | -- -- -- | -- -- -- | -- -- --
| [MM](https://store.steampowered.com/app/529890) | Maniac Mansion | - | - | -- -- -- | -- -- -- | -- -- --
| [SF](https://www.myabandonware.com/game/strike-fleet-j2) | Strike Fleet | - | - | -- -- -- | -- -- -- | -- -- --
| [B1942](https://www.myabandonware.com/game/battlehawks-1942-eu) | Battlehawks 1942 | - | - | -- -- -- | -- -- -- | -- -- --
| [ZMatAM](https://store.steampowered.com/app/559070) | Zak McKracken and the Alien Mindbenders | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatLC:TAG](https://www.myabandonware.com/game/indiana-jones-and-the-last-crusade-the-action-game-2fd) | Indiana Jones and the Last Crusade: The Action Game | - | - | -- -- -- | -- -- -- | -- -- --
| [PD](https://en.wikipedia.org/wiki/Pipe_Mania) | Pipe Dream | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatLC](https://store.steampowered.com/app/32310) | Indiana Jones and the Last Crusade: The Graphic Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [TFH](https://www.myabandonware.com/game/their-finest-hour-the-battle-of-britain-sb) | Their Finest Hour | - | - | -- -- -- | -- -- -- | -- -- --
| [TFM:V1](https://www.myabandonware.com/game/their-finest-missions-volume-one-7i9) | Their Finest Missions: Volume One | - | - | -- -- -- | -- -- -- | -- -- --
| [L](https://store.steampowered.com/app/32340) | Loom | - | - | -- -- -- | -- -- -- | -- -- --
| [M](https://www.myabandonware.com/game/masterblazer-17d) | Masterblazer | - | - | -- -- -- | -- -- -- | -- -- --
| [NS](https://www.abandonwaredos.com/abandonware-game.php?abandonware=Night+Shift&gid=1553) | Night Shift | - | - | -- -- -- | -- -- -- | -- -- --
| [TSoMI](https://en.wikipedia.org/wiki/The_Secret_of_Monkey_Island) | The Secret of Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [SWotL](https://www.myabandonware.com/game/secret-weapons-of-the-luftwaffe-1xo) | Secret Weapons of the Luftwaffe | - | - | -- -- -- | -- -- -- | -- -- --
| [SW](https://en.wikipedia.org/wiki/Star_Wars_(1991_video_game)) | Star Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [MI2:LR](https://store.steampowered.com/app/32460) | Monkey Island 2: LeChuck's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatFoA](https://store.steampowered.com/app/6010) | Indiana Jones and the Fate of Atlantis | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatFoA:TAG](https://en.wikipedia.org/wiki/Indiana_Jones_and_the_Fate_of_Atlantis#Development) | Indiana Jones and the Fate of Atlantis: The Action Game | - | - | -- -- -- | -- -- -- | -- -- --
| [LCA](https://www.myabandonware.com/game/lucasarts-classic-adventures-2x4) | LucasArts Classic Adventures (bundle) | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TESB](https://en.wikipedia.org/wiki/Star_Wars:_The_Empire_Strikes_Back_(1992_video_game)) | Star Wars: The Empire Strikes Back | - | - | -- -- -- | -- -- -- | -- -- --
| [DoDC](https://en.wikipedia.org/wiki/Defenders_of_Dynatron_City) | Defenders of Dynatron City | - | - | -- -- -- | -- -- -- | -- -- --
| [SSW](https://en.wikipedia.org/wiki/Super_Star_Wars) | Super Star Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [TYIJC](https://en.wikipedia.org/wiki/The_Young_Indiana_Jones_Chronicles_(video_game)) | The Young Indiana Jones Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:XW](https://store.steampowered.com/app/354430) | Star Wars: X-Wing | - | - | -- -- -- | -- -- -- | -- -- --
| [SSW:TESB](https://en.wikipedia.org/wiki/Super_Star_Wars:_The_Empire_Strikes_Back) | Super Star Wars: The Empire Strikes Back | - | - | -- -- -- | -- -- -- | -- -- --
| [DotT](https://store.steampowered.com/app/388210) | Day of the Tentacle | - | - | -- -- -- | -- -- -- | -- -- --
| [SWA](https://en.wikipedia.org/wiki/Star_Wars_Arcade) | Star Wars Arcade | - | - | -- -- -- | -- -- -- | -- -- --
| [ZAMN](https://store.steampowered.com/app/1137970) | Zombies Ate My Neighbors | - | - | -- -- -- | -- -- -- | -- -- --
| [SaMHtR](https://store.steampowered.com/app/355170) | Sam & Max Hit the Road | - | - | -- -- -- | -- -- -- | -- -- --
| [SWC](https://www.myabandonware.com/game/the-software-toolworks-star-wars-chess-2dk) | Star Wars Chess | - | - | -- -- -- | -- -- -- | -- -- --
| [ACC](https://archive.org/details/air_combat_classics_kfx) | Air Combat Classics (bundle) | - | - | -- -- -- | -- -- -- | -- -- --
| [IoCsYIJ](https://en.wikipedia.org/wiki/Instruments_of_Chaos_starring_Young_Indiana_Jones) | Instruments of Chaos starring Young Indiana Jones | - | - | -- -- -- | -- -- -- | -- -- --
| [SSW:RotJ](https://en.wikipedia.org/wiki/Super_Star_Wars:_Return_of_the_Jedi) | Super Star Wars: Return of the Jedi | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TF](https://store.steampowered.com/app/355250) | Star Wars: TIE Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [IJGA](https://en.wikipedia.org/wiki/Indiana_Jones%27_Greatest_Adventures) | Indiana Jones' Greatest Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [GP](https://store.steampowered.com/app/1137970) | Ghoul Patrol | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:DF](https://store.steampowered.com/app/32400) | Star Wars: Dark Forces | - | - | -- -- -- | -- -- -- | -- -- --
| [MW](https://en.wikipedia.org/wiki/Metal_Warriors) | Metal Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [FT](https://store.steampowered.com/app/228360) | Full Throttle | - | - | -- -- -- | -- -- -- | -- -- --
| [BST](https://en.wikipedia.org/wiki/Big_Sky_Trooper) | Big Sky Trooper | - | - | -- -- -- | -- -- -- | -- -- --
| [TD](https://store.steampowered.com/app/6040) | The Dig | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RA2](https://store.steampowered.com/app/456540) | Star Wars: Rebel Assault II: The Hidden Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [IJaHDA](https://www.myabandonware.com/game/indiana-jones-and-his-desktop-adventures-3lf) | Indiana Jones and His Desktop Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [A](https://www.gog.com/en/game/afterlife) | Afterlife | - | - | -- -- -- | -- -- -- | -- -- --
| [MatRotM](https://www.myabandonware.com/game/mortimer-and-the-riddles-of-the-medallion-3na) | Mortimer and the Riddles of the Medallion | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:SotE](https://store.steampowered.com/app/560170) | Star Wars: Shadows of the Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:YS](https://www.myabandonware.com/game/star-wars-yoda-stories-bcn) | Star Wars: Yoda Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [O](https://store.steampowered.com/app/559620) | Outlaws | - | - | -- -- -- | -- -- -- | -- -- --
| [B:C](https://en.wikipedia.org/wiki/Ballblazer_Champions) | Ballblazer Champions | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:XvT](https://store.steampowered.com/app/361690) | Star Wars: X-Wing vs. TIE Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [H:A](https://en.wikipedia.org/wiki/Herc%27s_Adventures) | Herc's Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK:DF2](https://store.steampowered.com/app/32380) | Star Wars Jedi Knight: Dark Forces II | - | - | -- -- -- | -- -- -- | -- -- --
| [MSW](https://www.myabandonware.com/game/star-wars-monopoly-jal) | Monopoly Star Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:MoTK](https://en.wikipedia.org/wiki/Star_Wars:_Masters_of_Ter%C3%A4s_K%C3%A4si) | Star Wars: Masters of Teras Kasi | - | - | -- -- -- | -- -- -- | -- -- --
| [TCoMI](https://store.steampowered.com/app/730820) | The Curse of Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK:MotS](https://store.steampowered.com/app/32390) | Star Wars Jedi Knight: Mysteries of the Sith | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:R](https://store.steampowered.com/app/441550) | Star Wars: Rebellion | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:BtM](https://archive.org/details/btm-cd-1) | Star Wars: Behind the Magic | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:DW](https://www.myabandonware.com/game/star-wars-droidworks-3qo) | Star Wars: DroidWorks | - | - | -- -- -- | -- -- -- | -- -- --
| [GF](https://store.steampowered.com/app/316790) | Grim Fandango | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTA](https://en.wikipedia.org/wiki/Star_Wars_Trilogy_Arcade) | Star Wars Trilogy Arcade | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RS](https://store.steampowered.com/app/455910) | Star Wars: Rogue Squadron | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:XA](https://store.steampowered.com/app/361670) | Star Wars: X-Wing Alliance | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:TPM](https://www.myabandonware.com/game/star-wars-episode-i-the-phantom-menace-lv8) | Star Wars Episode I: The Phantom Menace | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:R](https://store.steampowered.com/app/808910) | Star Wars Episode I: Racer | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:TGF](https://www.myabandonware.com/game/star-wars-episode-i-the-gungan-frontier-3qq) | Star Wars Episode I: The Gungan Frontier | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:IG]() | Star Wars: Episode I Insider's Guide | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:YCAC](https://www.myabandonware.com/game/star-wars-yoda-s-challenge-activity-center-3qt) | Star Wars: Yoda's Challenge Activity Center | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:PD](https://www.myabandonware.com/game/star-wars-pit-droids-403) | Star Wars: Pit Droids | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatIM](https://store.steampowered.com/app/904540) | Indiana Jones and the Infernal Machine | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:FC](https://archive.org/details/star-wars-force-commander-windows-10-compatible) | Star Wars: Force Commander | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:AS]() | Star Wars: Anakin's Speedway | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:JPB](https://en.wikipedia.org/wiki/Star_Wars_Episode_I:_Jedi_Power_Battles) | Star Wars Episode I: Jedi Power Battles | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RA](https://en.wikipedia.org/wiki/Star_Wars:_Racer_Arcade) | Star Wars: Racer Arcade | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:ELAC]() | Star Wars: Early Learning Activity Center | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:MJGG]() | Star Wars: Math - Jabba's Game Galaxy | - | - | -- -- -- | -- -- -- | -- -- --
| [EfMI](https://store.steampowered.com/app/730830) | Escape from Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:D](https://en.wikipedia.org/wiki/Star_Wars:_Demolition) | Star Wars: Demolition | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:JJJ]() | Star Wars: Jar Jar's Journey | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:OA](https://en.wikipedia.org/wiki/Star_Wars_Episode_I:_Obi-Wan%27s_Adventures) | Star Wars Episode I: Obi-Wan's Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:BfN](https://en.wikipedia.org/wiki/Star_Wars:_Episode_I:_Battle_for_Naboo) | Star Wars: Episode I: Battle for Naboo | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:S](https://store.steampowered.com/app/32350) | Star Wars: Starfighter | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:SBR](https://en.wikipedia.org/wiki/Star_Wars:_Super_Bombad_Racing) | Star Wars: Super Bombad Racing | - | - | -- -- -- | -- -- -- | -- -- --
| [SWGB](https://store.steampowered.com/app/356500) | Star Wars: Galactic Battlegrounds | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RS2:RL](https://en.wikipedia.org/wiki/Star_Wars_Rogue_Squadron_II:_Rogue_Leader) | Star Wars Rogue Squadron II: Rogue Leader | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:O](https://en.wikipedia.org/wiki/Star_Wars:_Obi-Wan) | Star Wars: Obi-Wan | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RR](https://en.wikipedia.org/wiki/Star_Wars_Racer_Revenge) | Star Wars Racer Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:JS](https://en.wikipedia.org/wiki/Star_Wars:_Jedi_Starfighter) | Star Wars: Jedi Starfighter | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK2:JO](https://store.steampowered.com/app/6030) | Star Wars Jedi Knight II: Jedi Outcast | - | - | -- -- -- | -- -- -- | -- -- --
| [SW2:AotC](https://en.wikipedia.org/wiki/Star_Wars:_Episode_II_%E2%80%93_Attack_of_the_Clones_(video_game)) | Star Wars: Episode II - Attack of the Clones | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TCW](https://en.wikipedia.org/wiki/Star_Wars:_The_Clone_Wars_(2002_video_game)) | Star Wars: The Clone Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TNDA](https://en.wikipedia.org/wiki/Star_Wars:_The_New_Droid_Army) | Star Wars: The New Droid Army | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:BH](https://en.wikipedia.org/wiki/Star_Wars:_Bounty_Hunter) | Star Wars: Bounty Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatET](https://store.steampowered.com/app/560430) | Indiana Jones and the Emperor's Tomb | - | - | -- -- -- | -- -- -- | -- -- --
| [RRR](https://en.wikipedia.org/wiki/RTX_Red_Rock) | RTX Red Rock | - | - | -- -- -- | -- -- -- | -- -- --
| [SWG](https://en.wikipedia.org/wiki/Star_Wars_Galaxies) | Star Wars Galaxies (closed) | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:KotOR](https://store.steampowered.com/app/32370) | Star Wars: Knights of the Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK:JA](https://store.steampowered.com/app/6020) | Star Wars Jedi Knight: Jedi Academy | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RS3:RS](https://en.wikipedia.org/wiki/Star_Wars_Rogue_Squadron_III:_Rebel_Strike) | Star Wars Rogue Squadron III: Rebel Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [G](https://en.wikipedia.org/wiki/Gladius_(video_game)) | Gladius | - | - | -- -- -- | -- -- -- | -- -- --
| [SWON](https://en.wikipedia.org/wiki/Secret_Weapons_Over_Normandy) | Secret Weapons Over Normandy | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:FotF](https://en.wikipedia.org/wiki/Star_Wars:_Flight_of_the_Falcon) | Star Wars: Flight of the Falcon | - | - | -- -- -- | -- -- -- | -- -- --
| [AaD](https://store.steampowered.com/app/6090) | Armed and Dangerous | - | - | -- -- -- | -- -- -- | -- -- --
| [WU](https://en.wikipedia.org/wiki/Wrath_Unleashed) | Wrath Unleashed | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:B](https://store.steampowered.com/app/1237980) | Star Wars: Battlefront | - | - | -- -- -- | -- -- -- | -- -- --
| [SWT:AotF](https://en.wikipedia.org/wiki/Star_Wars_Trilogy:_Apprentice_of_the_Force) | Star Wars Trilogy: Apprentice of the Force | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:KotOR2](https://store.steampowered.com/app/208580) | Star Wars Knights of the Old Republic II: The Sith Lords | - | - | -- -- -- | -- -- -- | -- -- --
| [M:PoD](https://en.wikipedia.org/wiki/Mercenaries:_Playground_of_Destruction) | Mercenaries: Playground of Destruction | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RC](https://store.steampowered.com/app/6000) | Star Wars: Republic Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [LSW:TVG](https://en.wikipedia.org/wiki/Lego_Star_Wars:_The_Video_Game) | Lego Star Wars: The Video Game | - | - | -- -- -- | -- -- -- | -- -- --
| [SW3:RotS](https://en.wikipedia.org/wiki/Star_Wars:_Episode_III_%E2%80%93_Revenge_of_the_Sith_(video_game)) | Star Wars: Episode III: Revenge of the Sith | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:B2](https://store.steampowered.com/app/6060) | Star Wars: Battlefront II | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:EaW](https://store.steampowered.com/app/32470) | Star Wars: Empire at War | - | - | -- -- -- | -- -- -- | -- -- --
| [LSW2:TOT](https://en.wikipedia.org/wiki/Lego_Star_Wars_II:_The_Original_Trilogy) | Lego Star Wars II: The Original Trilogy | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TBoP](https://en.wikipedia.org/wiki/Star_Wars:_The_Best_of_PC) | Star Wars: The Best of PC (bundle) | - | - | -- -- -- | -- -- -- | -- -- --
| [T](https://en.wikipedia.org/wiki/Thrillville) | Thrillville | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:LA](https://en.wikipedia.org/wiki/Star_Wars:_Lethal_Alliance) | Star Wars: Lethal Alliance | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:IA]() | Star Wars: Imperial Ace | - | - | -- -- -- | -- -- -- | -- -- --
| [SWB:RS](https://en.wikipedia.org/wiki/Star_Wars_Battlefront:_Renegade_Squadron) | Star Wars Battlefront: Renegade Squadron | - | - | -- -- -- | -- -- -- | -- -- --
| [T:OtR](https://store.steampowered.com/app/6080) | Thrillville: Off the Rails | - | - | -- -- -- | -- -- -- | -- -- --
| [LSW:TCS](https://store.steampowered.com/app/32440) | Lego Star Wars: The Complete Saga | - | - | -- -- -- | -- -- -- | -- -- --
| [LIJ:TOA](https://store.steampowered.com/app/32330) | Lego Indiana Jones: The Original Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TFU](https://store.steampowered.com/app/32430) | Star Wars: The Force Unleashed | - | - | -- -- -- | -- -- -- | -- -- --
| [F](https://en.wikipedia.org/wiki/Fracture_(video_game)) | Fracture | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTCW:JA](https://en.wikipedia.org/wiki/Star_Wars:_The_Clone_Wars_%E2%80%93_Jedi_Alliance) | Star Wars: The Clone Wars - Jedi Alliance | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTCW:LD](https://en.wikipedia.org/wiki/Star_Wars:_The_Clone_Wars_%E2%80%93_Lightsaber_Duels) | Star Wars: The Clone Wars - Lightsaber Duels | - | - | -- -- -- | -- -- -- | -- -- --
| [SWB:MS](https://en.wikipedia.org/wiki/Star_Wars:_Battlefront_(series)#Star_Wars_Battlefront:_Mobile_Squadrons) | Star Wars Battlefront: Mobile Squadrons | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatSK](https://en.wikipedia.org/wiki/Indiana_Jones_and_the_Staff_of_Kings) | Indiana Jones and the Staff of Kings | - | - | -- -- -- | -- -- -- | -- -- --
| [ToMI](https://store.steampowered.com/app/31170) | Tales of Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [TSoMI:SE](https://store.steampowered.com/app/32360) | The Secret of Monkey Island: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTCW:RH](https://store.steampowered.com/app/32420) | Star Wars: The Clone Wars - Republic Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [LU](https://store.steampowered.com/app/32410) | Lucidity | - | - | -- -- -- | -- -- -- | -- -- --
| [SWB:ES](https://en.wikipedia.org/wiki/Star_Wars_Battlefront:_Elite_Squadron) | Star Wars Battlefront: Elite Squadron | - | - | -- -- -- | -- -- -- | -- -- --
| [LIJ2:TAC](https://store.steampowered.com/app/32450) | Lego Indiana Jones 2: The Adventure Continues | - | - | -- -- -- | -- -- -- | -- -- --
| [MI2SE:LCR](https://store.steampowered.com/app/32460) | Monkey Island 2 Special Edition: LeChuck's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [CWA](https://en.wikipedia.org/wiki/Clone_Wars_Adventures) | Clone Wars Adventures (closed) | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TFU2](https://store.steampowered.com/app/32500) | Star Wars: The Force Unleashed II | - | - | -- -- -- | -- -- -- | -- -- --
| [LS3:TCW](https://store.steampowered.com/app/32510) | Lego Star Wars III: The Clone Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TOR](https://store.steampowered.com/app/1286830) | Star Wars: The Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [KSW](https://en.wikipedia.org/wiki/Kinect_Star_Wars) | Kinect Star Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [AB:SW](https://en.wikipedia.org/wiki/Angry_Birds_Star_Wars) | Angry Birds Star Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [AB:SW2](https://en.wikipedia.org/wiki/Angry_Birds_Star_Wars_II) | Angry Birds Star Wars II | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TDS](https://en.wikipedia.org/wiki/Star_Wars:_Tiny_Death_Star) | Star Wars: Tiny Death Star | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:1313](https://en.wikipedia.org/wiki/Star_Wars_1313) | Star Wars 1313 (canceled) | - | - | -- -- -- | -- -- -- | -- -- --
| **Monolith** | **MonolithTech**
| [FEAR](https://store.steampowered.com/app/21090) | F.E.A.R. | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:EP](https://store.steampowered.com/app/21110) | F.E.A.R.: Extraction Point | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:PM](https://store.steampowered.com/app/21120) | F.E.A.R.: Perseus Mandate | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR2](https://store.steampowered.com/app/16450) | F.E.A.R. 2: Project Origin | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR3](https://store.steampowered.com/app/21100) | F.E.A.R. 3 | - | - | -- -- -- | -- -- -- | -- -- --
| **Mythic** | **Mythic Entertainment**
| [DG](https://en.wikipedia.org/wiki/Dragon%27s_Gate) | Dragon's Gate | - | - | -- -- -- | -- -- -- | -- -- --
| [RM](https://en.wikipedia.org/wiki/Magestorm) | Rolemaster: Magestorm | - | - | -- -- -- | -- -- -- | -- -- --
| [AO](https://en.wikipedia.org/wiki/Aliens_Online) | Aliens Online | - | - | -- -- -- | -- -- -- | -- -- --
| [GO](https://en.wikipedia.org/wiki/Godzilla_Online) | Godzilla Online | - | - | -- -- -- | -- -- -- | -- -- --
| [ST](https://www.gog.com/dreamlist/game/starship-troopers-battlespace-1998) | Starship Troopers: Battlespace | - | - | -- -- -- | -- -- -- | -- -- --
| [DF](https://muds.fandom.com/wiki/Darkness_Falls:_The_Crusade) | Darkness Falls: The Crusade | - | - | -- -- -- | -- -- -- | -- -- --
| [SB](https://en.wikipedia.org/wiki/Spellbinder:_The_Nexus_Conflict) | Spellbinder: The Nexus Conflict | - | - | -- -- -- | -- -- -- | -- -- --
| [ID4](https://en.wikipedia.org/wiki/ID4_Online) | ID4 Online | - | - | -- -- -- | -- -- -- | -- -- --
| [DAoC](https://en.wikipedia.org/wiki/Dark_Age_of_Camelot) | Dark Age of Camelot | - | - | -- -- -- | -- -- -- | -- -- --
| [WAR](https://en.wikipedia.org/wiki/Warhammer_Online:_Age_of_Reckoning) | Warhammer Online: Age of Reckoning | - | - | -- -- -- | -- -- -- | -- -- --
| [UO](https://en.wikipedia.org/wiki/Ultima_Online_expansions#Stygian_Abyss) | Ultima Online: Stygian Abyss | - | - | -- -- -- | -- -- -- | -- -- --
| [DA2](https://en.wikipedia.org/wiki/Dragon_Age_II) | Dragon Age II | - | - | -- -- -- | -- -- -- | -- -- --
| [UF:QftA](https://en.wikipedia.org/wiki/Ultima_Forever:_Quest_for_the_Avatar) | Ultima Forever: Quest for the Avatar | - | - | -- -- -- | -- -- -- | -- -- --
| [DK](https://en.wikipedia.org/wiki/Dungeon_Keeper_(2014_video_game)) | Dungeon Keeper | - | - | -- -- -- | -- -- -- | -- -- --
| **Nintendo** | **Nintendo**
| [Z:TFH](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Tri_Force_Heroes) | The Legend of Zelda: Tri Force Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:AF](https://en.wikipedia.org/wiki/Animal_Crossing:_Amiibo_Festival) | Animal Crossing: Amiibo Festival | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ](https://en.wikipedia.org/wiki/Star_Fox_Zero) | Star Fox Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [SFG](https://en.wikipedia.org/wiki/Star_Fox_Guard) | Star Fox Guard | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:NLWA](https://en.wikipedia.org/wiki/Animal_Crossing:_New_Leaf_-_Welcome_Amiibo) | Animal Crossing: New Leaf - Welcome Amiibo | - | - | -- -- -- | -- -- -- | -- -- --
| [Miitopia](https://en.wikipedia.org/wiki/Miitopia) | Miitopia | - | - | -- -- -- | -- -- -- | -- -- --
| [SMR](https://en.wikipedia.org/wiki/Super_Mario_Run) | Super Mario Run | - | - | -- -- -- | -- -- -- | -- -- --
| [Miitomo](https://en.wikipedia.org/wiki/Miitomo) | Miitomo | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:PC](https://en.wikipedia.org/wiki/Animal_Crossing:_Pocket_Camp) | Animal Crossing: Pocket Camp | - | - | -- -- -- | -- -- -- | -- -- --
| [M:SR](https://en.wikipedia.org/wiki/Metroid:_Samus_Returns) | Metroid: Samus Returns | - | - | -- -- -- | -- -- -- | -- -- --
| [Z:BotW](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Breath_of_the_Wild) | The Legend of Zelda: Breath of the Wild | - | - | -- -- -- | -- -- -- | -- -- --
| [MK8D](https://en.wikipedia.org/wiki/Mario_Kart_8#Mario_Kart_8_Deluxe) | Mario Kart 8 Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| [12Switch](https://en.wikipedia.org/wiki/1-2-Switch) | 1-2-Switch | - | - | -- -- -- | -- -- -- | -- -- --
| [Arms](https://en.wikipedia.org/wiki/Arms_(video_game)) | Arms | - | - | -- -- -- | -- -- -- | -- -- --
| [Splatoon2](https://en.wikipedia.org/wiki/Splatoon_2) | Splatoon 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SMO](https://en.wikipedia.org/wiki/Super_Mario_Odyssey) | Super Mario Odyssey | - | - | -- -- -- | -- -- -- | -- -- --
| [NintendoLabo](https://en.wikipedia.org/wiki/Nintendo_Labo) | Nintendo Labo | - | - | -- -- -- | -- -- -- | -- -- --
| [CaptainToad:TT](https://en.wikipedia.org/wiki/Captain_Toad:_Treasure_Tracker) | Captain Toad: Treasure Tracker | - | - | -- -- -- | -- -- -- | -- -- --
| [DrMarioWorld](https://en.wikipedia.org/wiki/Dr._Mario_World) | Dr. Mario World | - | - | -- -- -- | -- -- -- | -- -- --
| [MKT](https://en.wikipedia.org/wiki/Mario_Kart_Tour) | Mario Kart Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [NSMB:UD](https://en.wikipedia.org/wiki/New_Super_Mario_Bros._U_Deluxe) | New Super Mario Bros. U Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| [SMM2](https://en.wikipedia.org/wiki/Super_Mario_Maker_2) | Super Mario Maker 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RingFitAdventure](https://en.wikipedia.org/wiki/Ring_Fit_Adventure) | Ring Fit Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [DKBTfNS](https://en.wikipedia.org/wiki/Dr_Kawashima%27s_Brain_Training_for_Nintendo_Switch) | Dr Kawashima's Brain Training for Nintendo Switch | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:NH](https://en.wikipedia.org/wiki/Animal_Crossing:_New_Horizons) | Animal Crossing: New Horizons | - | - | -- -- -- | -- -- -- | -- -- --
| [SM3D:AS](https://en.wikipedia.org/wiki/Super_Mario_3D_All-Stars) | Super Mario 3D All-Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [Pikmin3D](https://en.wikipedia.org/wiki/Pikmin_3_Deluxe) | Pikmin 3 Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| [BowsersFury](https://en.wikipedia.org/wiki/Bowser%27s_Fury) | Bowser's Fury | - | - | -- -- -- | -- -- -- | -- -- --
| [GameBuilderGarage](https://en.wikipedia.org/wiki/Unreal_(1990_video_game)) | Game Builder Garage | - | - | -- -- -- | -- -- -- | -- -- --
| [M:D](https://en.wikipedia.org/wiki/Metroid_Dread) | Metroid Dread | - | - | -- -- -- | -- -- -- | -- -- --
| [BBA:BvB](https://en.wikipedia.org/wiki/Big_Brain_Academy:_Brain_vs._Brain) | Big Brain Academy: Brain vs. Brain | - | - | -- -- -- | -- -- -- | -- -- --
| [NintendoSwitchSports](https://en.wikipedia.org/wiki/Nintendo_Switch_Sports) | Nintendo Switch Sports | - | - | -- -- -- | -- -- -- | -- -- --
| [Splatoon3](https://en.wikipedia.org/wiki/Splatoon_3) | Splatoon 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Z:TotK](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Tears_of_the_Kingdom) | The Legend of Zelda: Tears of the Kingdom | - | - | -- -- -- | -- -- -- | -- -- --
| [12Switch2](https://en.wikipedia.org/wiki/Everybody_1-2-Switch!) | Everybody 1-2-Switch! | - | - | -- -- -- | -- -- -- | -- -- --
| [Pikmin4](https://en.wikipedia.org/wiki/Pikmin_4) | Pikmin 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [SMBW](https://en.wikipedia.org/wiki/Super_Mario_Bros._Wonder) | Super Mario Bros. Wonder | - | - | -- -- -- | -- -- -- | -- -- --
| [ETSM:FDC](https://en.wikipedia.org/wiki/Emio_%E2%80%93_The_Smiling_Man:_Famicom_Detective_Club) | Emio - The Smiling Man: Famicom Detective Club | - | - | -- -- -- | -- -- -- | -- -- --
| [MKW](https://en.wikipedia.org/wiki/Mario_Kart_World) | Mario Kart World | - | - | -- -- -- | -- -- -- | -- -- --
| [DKB](https://en.wikipedia.org/wiki/Donkey_Kong_Bananza) | Donkey Kong Bananza | - | - | -- -- -- | -- -- -- | -- -- --
| **Origin** | **Origin Systems**
| [CoC]() | Caverns of Callisto | - | - | -- -- -- | -- -- -- | -- -- --
| [U3](https://en.wikipedia.org/wiki/Ultima_III:_Exodus) | Ultima III: Exodus | - | - | -- -- -- | -- -- -- | -- -- --
| [Moebius](https://en.wikipedia.org/wiki/Moebius:_The_Orb_of_Celestial_Harmony) | Moebius: The Orb of Celestial Harmony | - | - | -- -- -- | -- -- -- | -- -- --
| [U4](https://en.wikipedia.org/wiki/Ultima_IV:_Quest_of_the_Avatar) | Ultima IV: Quest of the Avatar | - | - | -- -- -- | -- -- -- | -- -- --
| [AutoDuel](https://en.wikipedia.org/wiki/Autoduel) | AutoDuel | - | - | -- -- -- | -- -- -- | -- -- --
| [Ogre](https://en.wikipedia.org/wiki/Ogre_(video_game)) | Ogre | - | - | -- -- -- | -- -- -- | -- -- --
| [RingQuest]() | Ring Quest | - | - | -- -- -- | -- -- -- | -- -- --
| [U1](https://en.wikipedia.org/wiki/Ultima_I:_The_First_Age_of_Darkness) | Ultima I: The First Age of Darkness | - | - | -- -- -- | -- -- -- | -- -- --
| [2400AD](https://en.wikipedia.org/wiki/2400_A.D.) | 2400 A.D. | - | - | -- -- -- | -- -- -- | -- -- --
| [ToL](https://en.wikipedia.org/wiki/Times_of_Lore) | Times of Lore | - | - | -- -- -- | -- -- -- | -- -- --
| [U5](https://en.wikipedia.org/wiki/Ultima_V:_Warriors_of_Destiny) | Ultima V: Warriors of Destiny | - | - | -- -- -- | -- -- -- | -- -- --
| [KoL](https://en.wikipedia.org/wiki/Knights_of_Legend) | Knights of Legend | - | - | -- -- -- | -- -- -- | -- -- --
| [Omega](https://en.wikipedia.org/wiki/Omega_(video_game)) | Omega | - | - | -- -- -- | -- -- -- | -- -- --
| [TangledTales:TMoaWA](https://en.wikipedia.org/wiki/Tangled_Tales:_The_Misadventures_of_a_Wizard%27s_Apprentice) | Tangled Tales: The Misadventures of a Wizard's Apprentice | - | - | -- -- -- | -- -- -- | -- -- --
| [Windwalker](https://en.wikipedia.org/wiki/Windwalker_(video_game)) | Windwalker | - | - | -- -- -- | -- -- -- | -- -- --
| [BadBlood](https://en.wikipedia.org/wiki/Bad_Blood_(video_game)) | Bad Blood | - | - | -- -- -- | -- -- -- | -- -- --
| [U6](https://en.wikipedia.org/wiki/Ultima_VI:_The_False_Prophet) | Ultima VI: The False Prophet | - | - | -- -- -- | -- -- -- | -- -- --
| [WC](https://en.wikipedia.org/wiki/Wing_Commander_(video_game)) | Wing Commander | - | - | -- -- -- | -- -- -- | -- -- --
| [U:TSE](https://en.wikipedia.org/wiki/Omega_(video_game)) | Worlds of Ultima: The Savage Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [U:WoA2](https://en.wikipedia.org/wiki/Ultima:_Worlds_of_Adventure_2:_Martian_Dreams) | Ultima: Worlds of Adventure 2: Martian Dreams | - | - | -- -- -- | -- -- -- | -- -- --
| [WC2:VotK](https://en.wikipedia.org/wiki/Wing_Commander_II:_Vengeance_of_the_Kilrathi) | Wing Commander II: Vengeance of the Kilrathi | - | - | -- -- -- | -- -- -- | -- -- --
| [U:RoV](https://en.wikipedia.org/wiki/Ultima:_Runes_of_Virtue) | Ultima: Runes of Virtue | - | - | -- -- -- | -- -- -- | -- -- --
| [U7:FoV]() | Ultima VII: Forge of Virtue | - | - | -- -- -- | -- -- -- | -- -- --
| [U7](https://en.wikipedia.org/wiki/Ultima_VII:_The_Black_Gate) | Ultima VII: The Black Gate | - | - | -- -- -- | -- -- -- | -- -- --
| [UU:TSA](https://en.wikipedia.org/wiki/Ultima_Underworld:_The_Stygian_Abyss) | Ultima Underworld: The Stygian Abyss | - | - | -- -- -- | -- -- -- | -- -- --
| [ShadowCaster](https://en.wikipedia.org/wiki/ShadowCaster) | ShadowCaster | - | - | -- -- -- | -- -- -- | -- -- --
| [SC](https://en.wikipedia.org/wiki/Strike_Commander) | Strike Commander | - | - | -- -- -- | -- -- -- | -- -- --
| [U7:SI](https://en.wikipedia.org/wiki/Ultima_VII_Part_Two:_Serpent_Isle) | Ultima VII Part Two: Serpent Isle | - | - | -- -- -- | -- -- -- | -- -- --
| [UU:LoW](https://en.wikipedia.org/wiki/Ultima_Underworld_II:_Labyrinth_of_Worlds) | Ultima Underworld II: Labyrinth of Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [WCA](https://www.gog.com/en/game/wing_commander_academy) | Wing Commander Academy | - | - | -- -- -- | -- -- -- | -- -- --
| [WC:P](https://en.wikipedia.org/wiki/Wing_Commander:_Privateer) | Wing Commander: Privateer | - | - | -- -- -- | -- -- -- | -- -- --
| [U:RoV2](https://en.wikipedia.org/wiki/Ultima:_Runes_of_Virtue) | Ultima: Runes of Virtue II | - | - | -- -- -- | -- -- -- | -- -- --
| [MetalMorph](https://en.wikipedia.org/wiki/Metal_Morph) | Metal Morph | - | - | -- -- -- | -- -- -- | -- -- --
| [PacificStrike](https://en.wikipedia.org/wiki/Pacific_Strike) | Pacific Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [U8](https://en.wikipedia.org/wiki/Ultima_VIII:_Pagan) | Pagan: Ultima VIII | - | - | -- -- -- | -- -- -- | -- -- --
| [WC:S]() | Super Wing Commander | - | - | -- -- -- | -- -- -- | -- -- --
| [SS](https://en.wikipedia.org/wiki/System_Shock) | System Shock | - | - | -- -- -- | -- -- -- | -- -- --
| [WC:A](https://en.wikipedia.org/wiki/Wing_Commander:_Armada) | Wing Commander: Armada | - | - | -- -- -- | -- -- -- | -- -- --
| [WC3](https://en.wikipedia.org/wiki/Wing_Commander_III:_Heart_of_the_Tiger) | Wing Commander III: Heart of the Tiger | - | - | -- -- -- | -- -- -- | -- -- --
| [BioForge](https://en.wikipedia.org/wiki/BioForge) | BioForge | - | - | -- -- -- | -- -- -- | -- -- --
| [C:NR](https://en.wikipedia.org/wiki/Crusader:_No_Remorse) | Crusader: No Remorse | - | - | -- -- -- | -- -- -- | -- -- --
| [CyberMage](https://en.wikipedia.org/wiki/CyberMage:_Darklight_Awakening) | CyberMage: Darklight Awakening | - | - | -- -- -- | -- -- -- | -- -- --
| [WoG](https://en.wikipedia.org/wiki/Wings_of_Glory) | Wings of Glory | - | - | -- -- -- | -- -- -- | -- -- --
| [Abuse](https://en.wikipedia.org/wiki/Abuse_(video_game)) | Abuse | - | - | -- -- -- | -- -- -- | -- -- --
| [C:NR2](https://en.wikipedia.org/wiki/Crusader:_No_Regret) | Crusader: No Regret | - | - | -- -- -- | -- -- -- | -- -- --
| [J:L](https://en.wikipedia.org/wiki/Jane%27s_AH-64D_Longbow) | Jane's AH-64D Longbow | - | - | -- -- -- | -- -- -- | -- -- --
| [Transland]() | Transland | - | - | -- -- -- | -- -- -- | -- -- --
| [WC4](https://en.wikipedia.org/wiki/Wing_Commander_IV:_The_Price_of_Freedom) | Wing Commander IV: The Price of Freedom | - | - | -- -- -- | -- -- -- | -- -- --
| [J:L2]() | Jane's Combat Simulations: Longbow 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [UO](https://en.wikipedia.org/wiki/Ultima_Online) | Ultima Online | - | - | -- -- -- | -- -- -- | -- -- --
| [WC:P2](https://en.wikipedia.org/wiki/Wing_Commander:_Prophecy) | Wing Commander: Prophecy | - | - | -- -- -- | -- -- -- | -- -- --
| [U9](https://en.wikipedia.org/wiki/Ultima_IX:_Ascension) | Ultima IX: Ascension | - | - | -- -- -- | -- -- -- | -- -- --
| **Red** | **REDengine**
| [Witcher](https://www.gog.com/en/game/the_witcher) | The Witcher Enhanced Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher2](https://www.gog.com/en/game/the_witcher_2) | The Witcher 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher3](https://www.gog.com/en/game/the_witcher_3_wild_hunt) | The Witcher 3: Wild Hunt | - | - | -- -- -- | -- -- -- | -- -- --
| [CP77](https://store.steampowered.com/app/1091500) | Cyberpunk 2077 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher4](https://en.wikipedia.org/wiki/The_Witcher_(video_game_series)) | The Witcher 4 Polaris (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Rockstar** | **Rockstar Games**
| [GTA](https://www.rockstargames.com/games/gta) | Grand Theft Auto | - | - | -- -- -- | -- -- -- | -- -- --
| [WM](https://www.rockstargames.com/games/wildmetal) | Wild Metal | - | - | -- -- -- | -- -- -- | -- -- --
| [MTM64](https://en.wikipedia.org/wiki/Monster_Truck_Madness_2#Monster_Truck_Madness_64) | Monster Truck Madness 64 | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA2](https://www.rockstargames.com/games/gta2) | Grand Theft Auto 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [EWJ3D](https://store.steampowered.com/app/41600/Earthworm_Jim_3D/) | Earthworm Jim 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [T:SaD](https://www.rockstargames.com/games/skateanddestroy) | Thrasher: Skate and Destroy | - | - | -- -- -- | -- -- -- | -- -- --
| [EK](https://gamefaqs.gamespot.com/gbc/197242-evel-knievel) | Evel Knievel | - | - | -- -- -- | -- -- -- | -- -- --
| [AP:OB](https://en.wikipedia.org/wiki/Austin_Powers:_Oh,_Behave!) | Austin Powers: Oh, Behave! | - | - | -- -- -- | -- -- -- | -- -- --
| [AP:WtMUL](https://en.wikipedia.org/wiki/Austin_Powers:_Welcome_to_My_Underground_Lair!) | Austin Powers: Welcome to My Underground Lair! | - | - | -- -- -- | -- -- -- | -- -- --
| [MC](https://www.rockstargames.com/games/midnightclub) | Midnight Club: Street Racing | - | - | -- -- -- | -- -- -- | -- -- --
| [SR](https://www.rockstargames.com/games/smugglersrun) | Smuggler's Run | - | - | -- -- -- | -- -- -- | -- -- --
| [SH](https://en.wikipedia.org/wiki/Surfing_H3O) | Surfing H3O | - | - | -- -- -- | -- -- -- | -- -- --
| [ON](https://www.rockstargames.com/games/oni) | Oni | - | - | -- -- -- | -- -- -- | -- -- --
| [YDKJ](https://en.wikipedia.org/wiki/You_Don%27t_Know_Jack_(1999_video_game)) | You Don't Know Jack | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA3](https://store.steampowered.com/app/1546970/Grand_Theft_Auto_III__The_Definitive_Edition/) | Grand Theft Auto III | - | - | -- -- -- | -- -- -- | -- -- --
| [SR2](https://www.rockstargames.com/games/smugglersrun2) | Smuggler's Run 2: Hostile Territory | - | - | -- -- -- | -- -- -- | -- -- --
| [MP](https://store.steampowered.com/app/12140/Max_Payne/) | Max Payne | - | - | -- -- -- | -- -- -- | -- -- --
| [SoE](https://www.rockstargames.com/games/stateofemergency) | State of Emergency | - | - | -- -- -- | -- -- -- | -- -- --
| [TIJ](https://www.rockstargames.com/games/italianjob) | The Italian Job | - | - | -- -- -- | -- -- -- | -- -- --
| [SR:W](https://www.rockstargames.com/games/smugglersrunwarzones) | Smuggler's Run: Warzones | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:VC](https://store.steampowered.com/app/1546990/Grand_Theft_Auto_Vice_City__The_Definitive_Edition/) | Grand Theft Auto: Vice City | - | - | -- -- -- | -- -- -- | -- -- --
| [MC2](https://store.steampowered.com/app/12160/Midnight_Club_2/) | Midnight Club II | - | - | -- -- -- | -- -- -- | -- -- --
| [MP2](https://store.steampowered.com/app/12150/Max_Payne_2_The_Fall_of_Max_Payne/) | Max Payne 2: The Fall of Max Payne | - | - | -- -- -- | -- -- -- | -- -- --
| [MH](https://store.steampowered.com/app/12130/Manhunt/) | Manhunt | - | - | -- -- -- | -- -- -- | -- -- --
| [RDV](https://store.playstation.com/en-us/product/UP1004-CUSA03517_00-SLUS205000000001) | Red Dead Revolver | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:A](https://www.rockstargames.com/games/grandtheftauto-gba) | Grand Theft Auto Advance | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:SA](https://store.steampowered.com/app/1547000/Grand_Theft_Auto_San_Andreas__The_Definitive_Edition/) | Grand Theft Auto: San Andreas | - | - | -- -- -- | -- -- -- | -- -- --
| [MC3:DE](https://www.rockstargames.com/games/midnightclub3) | Midnight Club 3: DUB Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [TW](https://store.playstation.com/en-us/product/UP1004-CUSA03515_00-SLUS212150000001) | The Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:LCS](https://play.google.com/store/apps/details?id=com.rockstargames.gtalcs&hl=en_US) | Grand Theft Auto: Liberty City Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [MC3:DER](https://en.wikipedia.org/wiki/Midnight_Club_3:_Dub_Edition#Midnight_Club_3:_Dub_Edition_Remix) | Midnight Club 3: DUB Edition Remix | - | - | -- -- -- | -- -- -- | -- -- --
| [RTT](https://www.xbox.com/en-US/games/store/rockstar-table-tennis/BVZ4H08BMQ3H) | Rockstar Games Presents Table Tennis | - | - | -- -- -- | -- -- -- | -- -- --
| [B](https://www.rockstargames.com/bully) | Bully | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:VCS](https://archive.org/download/sony_playstation2_g/Grand%20Theft%20Auto%20-%20Vice%20City%20Stories%20%28USA%29.zip) | Grand Theft Auto: Vice City Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [MH2](https://www.rockstargames.com/games/manhunt2) | Manhunt 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [B:SE](https://store.steampowered.com/app/12200/Bully_Scholarship_Edition/) | Bully: Scholarship Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA4](https://store.steampowered.com/app/12210/Grand_Theft_Auto_IV_The_Complete_Edition/) | Grand Theft Auto IV | - | - | -- -- -- | -- -- -- | -- -- --
| [MC:LA](https://www.xbox.com/en-US/games/store/midnight-club-los-angeles-complete/BV6WLXGS887C) | Midnight Club: Los Angeles | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:CW](https://www.rockstargames.com/games/chinatownwars) | Grand Theft Auto: Chinatown Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [BTR](https://www.rockstargames.com/games/beaterator) | Beaterator | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR](https://store.steampowered.com/app/2668510/Red_Dead_Redemption/) | Red Dead Redemption | - | - | -- -- -- | -- -- -- | -- -- --
| [LAN](https://store.steampowered.com/app/110800/LA_Noire/) | L.A. Noire | - | - | -- -- -- | -- -- -- | -- -- --
| [MP3](https://store.steampowered.com/app/12120/Grand_Theft_Auto_San_Andreas/) | Max Payne 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA5](https://store.steampowered.com/app/271590/Grand_Theft_Auto_V/) | Grand Theft Auto V | - | - | -- -- -- | -- -- -- | -- -- --
| [GTAO](https://www.xbox.com/en-US/games/store/grand-theft-auto-online-xbox-series-xs/9NKC1Z4Z92VN) | Grand Theft Auto Online | - | - | -- -- -- | -- -- -- | -- -- --
| [LAN:VR](https://store.steampowered.com/app/722230/LA_Noire_The_VR_Case_Files/) | L.A. Noire: The VR Case Files | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR2](https://store.steampowered.com/app/1174180/Red_Dead_Redemption_2/) | Red Dead Redemption 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RDO](https://store.steampowered.com/app/1404210/Red_Dead_Online/) | Red Dead Online | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:T](https://www.xbox.com/en-US/games/store/grand-theft-auto-the-trilogy-the-definitive-edition/9MXMJFNZMVWD) | Grand Theft Auto: The Trilogy - The Definitive Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA6](https://www.rockstargames.com/VI) | Grand Theft Auto VI (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [MPX]() | Max Payne (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Ubisoft** | **Ubisoft**
| [Zombi86](https://en.wikipedia.org/wiki/Zombi_(1986_video_game)) | Zombi '86 | - | - | -- -- -- | -- -- -- | -- -- --
| [TrivialPursuit]() | Trivial Pursuit | - | - | -- -- -- | -- -- -- | -- -- --
| [Asphalt]() | Asphalt | - | - | -- -- -- | -- -- -- | -- -- --
| [DefenderoftheCrown](https://en.wikipedia.org/wiki/Defender_of_the_Crown) | Defender of the Crown | - | - | -- -- -- | -- -- -- | -- -- --
| [LeMaitredesAmes]() | Le Maître des Âmes | - | - | -- -- -- | -- -- -- | -- -- --
| [LeNecromancien]() | Le Nécromancien | - | - | -- -- -- | -- -- -- | -- -- --
| [MangeCailloux]() | Mange Cailloux | - | - | -- -- -- | -- -- -- | -- -- --
| [STKrak]() | ST Krak | - | - | -- -- -- | -- -- -- | -- -- --
| [NightHunter]() | Night Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [BAT](https://en.wikipedia.org/wiki/B.A.T._(video_game)) | B.A.T. | - | - | -- -- -- | -- -- -- | -- -- --
| [FinalCommand]() | Final Command | - | - | -- -- -- | -- -- -- | -- -- --
| [Fred]() | Fred | - | - | -- -- -- | -- -- -- | -- -- --
| [Ilyad]() | Ilyad | - | - | -- -- -- | -- -- -- | -- -- --
| [Intruder]() | Intruder | - | - | -- -- -- | -- -- -- | -- -- --
| [IronLord](https://en.wikipedia.org/wiki/Iron_Lord) | Iron Lord | - | - | -- -- -- | -- -- -- | -- -- --
| [ProTennisTour](https://en.wikipedia.org/wiki/Pro_Tennis_Tour) | Pro Tennis Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [OthelloKiller]() | Othello Killer | - | - | -- -- -- | -- -- -- | -- -- --
| [PuffysSaga](https://en.wikipedia.org/wiki/Puffy%27s_Saga) | Puffy's Saga | - | - | -- -- -- | -- -- -- | -- -- --
| [Skateball](https://en.wikipedia.org/wiki/Skateball) | Skateball | - | - | -- -- -- | -- -- -- | -- -- --
| [TwinWorld:LoV](https://en.wikipedia.org/wiki/Twinworld) | TwinWorld: Land of Vision | - | - | -- -- -- | -- -- -- | -- -- --
| [BrainBlasters]() | Brain Blasters | - | - | -- -- -- | -- -- -- | -- -- --
| [Hexsider]() | Hexsider | - | - | -- -- -- | -- -- -- | -- -- --
| [JupitersMasterdrive]() | Jupiter's Masterdrive | - | - | -- -- -- | -- -- -- | -- -- --
| [PicknPile]() | Pick 'n Pile | - | - | -- -- -- | -- -- -- | -- -- --
| [PoolofRadiance](https://en.wikipedia.org/wiki/Pool_of_Radiance) | Pool of Radiance | - | - | -- -- -- | -- -- -- | -- -- --
| [Ranx:TVG]() | Ranx: The Video Game | - | - | -- -- -- | -- -- -- | -- -- --
| [TomandtheGhost]() | Tom and the Ghost | - | - | -- -- -- | -- -- -- | -- -- --
| [Unreal](https://en.wikipedia.org/wiki/Unreal_(1990_video_game)) | Unreal | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleIsle](https://en.wikipedia.org/wiki/Battle_Isle_(video_game)) | Battle Isle | - | - | -- -- -- | -- -- -- | -- -- --
| [Bomberman91](https://en.wikipedia.org/wiki/Bomberman_(1990_video_game)) | Bomberman '91 | - | - | -- -- -- | -- -- -- | -- -- --
| [CelticLegends](https://en.wikipedia.org/wiki/Celtic_Legends) | Celtic Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [ProTennisTour2](https://en.wikipedia.org/wiki/Pro_Tennis_Tour_2) | Pro Tennis Tour 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MaupitiIsland](https://en.wikipedia.org/wiki/Maupiti_Island_(video_game)) | Maupiti Island | - | - | -- -- -- | -- -- -- | -- -- --
| [FirstSamurai](https://en.wikipedia.org/wiki/First_Samurai) | First Samurai | - | - | -- -- -- | -- -- -- | -- -- --
| [JimmyConnorsProTennisTour](https://en.wikipedia.org/wiki/Jimmy_Connors_Pro_Tennis_Tour) | Jimmy Connors Pro Tennis Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [MegaLoMania](https://en.wikipedia.org/wiki/Mega-Lo-Mania) | Mega-Lo-Mania | - | - | -- -- -- | -- -- -- | -- -- --
| [Starush]() | Starush | - | - | -- -- -- | -- -- -- | -- -- --
| [StarWars](https://en.wikipedia.org/wiki/Star_Wars_(1991_video_game)) | Star Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [StarWars:TESB](https://en.wikipedia.org/wiki/Star_Wars:_The_Empire_Strikes_Back_(1992_video_game)) | Star Wars: The Empire Strikes Back | - | - | -- -- -- | -- -- -- | -- -- --
| [TheKoshanConspiracy](https://en.wikipedia.org/wiki/B.A.T._II_%E2%80%93_The_Koshan_Conspiracy) | The Koshan Conspiracy | - | - | -- -- -- | -- -- -- | -- -- --
| [ThePerfectGeneral](https://en.wikipedia.org/wiki/The_Perfect_General) | The Perfect General | - | - | -- -- -- | -- -- -- | -- -- --
| [Vroom]() | Vroom | - | - | -- -- -- | -- -- -- | -- -- --
| [JimmyConnorsTennis](https://en.wikipedia.org/wiki/Jimmy_Connors_Tennis) | Jimmy Connors Tennis | - | - | -- -- -- | -- -- -- | -- -- --
| [F1PolePosition](https://en.wikipedia.org/wiki/F1_Pole_Position_(video_game)) | F1 Pole Position | - | - | -- -- -- | -- -- -- | -- -- --
| [IJLC:TAG](https://en.wikipedia.org/wiki/Indiana_Jones_and_the_Last_Crusade:_The_Action_Game) | Indiana Jones and the Last Crusade: The Action Game | - | - | -- -- -- | -- -- -- | -- -- --
| [ChampionshipManager](https://en.wikipedia.org/wiki/Championship_Manager_(video_game)) | Championship Manager | - | - | -- -- -- | -- -- -- | -- -- --
| [StreetRacer](https://en.wikipedia.org/wiki/Street_Racer_(1994_video_game)) | Street Racer | - | - | -- -- -- | -- -- -- | -- -- --
| [F1PolePosition2](https://en.wikipedia.org/wiki/F1_Pole_Position_2) | F1 Pole Position 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [HyperVBall]() | Hyper V-Ball | - | - | -- -- -- | -- -- -- | -- -- --
| [SoulBlazer](https://en.wikipedia.org/wiki/Soul_Blazer) | Soul Blazer | - | - | -- -- -- | -- -- -- | -- -- --
| [RY95](https://en.wikipedia.org/wiki/Rayman_(video_game)) | Rayman (95) | - | - | -- -- -- | -- -- -- | -- -- --
| [ActionEuroSoccer96]() | Action Euro Soccer 96 | - | - | -- -- -- | -- -- -- | -- -- --
| [ActionSoccer]() | Action Soccer | - | - | -- -- -- | -- -- -- | -- -- --
| [KatLN]() | Kiyeko and the Lost Night | - | - | -- -- -- | -- -- -- | -- -- --
| [PatIG]() | Payuta and the Ice God | - | - | -- -- -- | -- -- -- | -- -- --
| [AiMwtR]() | Adventures in Music with the Recorder | - | - | -- -- -- | -- -- -- | -- -- --
| [ALGwRY96]() | Amazing Learning Games with Rayman '96 | - | - | -- -- -- | -- -- -- | -- -- --
| [MaEwRY:V2]() | Maths and English with Rayman: Volume 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MaEwRY:V3]() | Maths and English with Rayman: Volume 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SecretsoftheLuxor](https://en.wikipedia.org/wiki/Secrets_of_the_Luxor) | Secrets of the Luxor | - | - | -- -- -- | -- -- -- | -- -- --
| [TAoVM](https://en.wikipedia.org/wiki/The_Adventures_of_Valdo_%26_Marie) | The Adventures of Valdo & Marie | - | - | -- -- -- | -- -- -- | -- -- --
| [F1PolePosition64](https://en.wikipedia.org/wiki/F1_Pole_Position_64) | F1 Pole Position 64 | - | - | -- -- -- | -- -- -- | -- -- --
| [SubCulture](https://en.wikipedia.org/wiki/Sub_Culture) | Sub Culture | - | - | -- -- -- | -- -- -- | -- -- --
| [Earth2140](https://en.wikipedia.org/wiki/Earth_2140) | Earth 2140 | - | - | -- -- -- | -- -- -- | -- -- --
| [F1RacingSimulation](https://en.wikipedia.org/wiki/F1_Racing_Simulation) | F1 Racing Simulation | - | - | -- -- -- | -- -- -- | -- -- --
| [POD](https://en.wikipedia.org/wiki/POD_(video_game)) | POD | - | - | -- -- -- | -- -- -- | -- -- --
| [POD:BtH]() | POD: Back to Hell | - | - | -- -- -- | -- -- -- | -- -- --
| [ProPinball:T](https://en.wikipedia.org/wiki/Pro_Pinball:_Timeshock!) | Pro Pinball: Timeshock! | - | - | -- -- -- | -- -- -- | -- -- --
| [RYDesigner](https://en.wikipedia.org/wiki/Rayman_Designer) | Rayman Designer | - | - | -- -- -- | -- -- -- | -- -- --
| [SDWCFootball]() | Sean Dundee's World Club Football | - | - | -- -- -- | -- -- -- | -- -- --
| [SCARS](https://en.wikipedia.org/wiki/S.C.A.R.S._(video_game)) | S.C.A.R.S. | - | - | -- -- -- | -- -- -- | -- -- --
| [ShadowGunner:TRW]() | Shadow Gunner: The Robot Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [SpeedBusters:AH](https://en.wikipedia.org/wiki/Speed_Devils) | Speed Busters: American Highways | - | - | -- -- -- | -- -- -- | -- -- --
| [TheFifthElement](https://en.wikipedia.org/wiki/The_Fifth_Element_(video_game)) | The Fifth Element | - | - | -- -- -- | -- -- -- | -- -- --
| [WorldFootball98](https://en.wikipedia.org/wiki/Kiko_World_Football) | World Football 98 | - | - | -- -- -- | -- -- -- | -- -- --
| [AllStarTennis99](https://en.wikipedia.org/wiki/All_Star_Tennis_%2799) | All Star Tennis '99 | - | - | -- -- -- | -- -- -- | -- -- --
| [StarWars:EIR](https://en.wikipedia.org/wiki/Star_Wars_Episode_I:_Racer) | Star Wars: Episode I - Racer | - | - | -- -- -- | -- -- -- | -- -- --
| [MGPRS2]() | Monaco Grand Prix Racing Simulation 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TonicTrouble](https://en.wikipedia.org/wiki/Tonic_Trouble) | Tonic Trouble | - | - | -- -- -- | -- -- -- | -- -- --
| [WallStreetTycoon]() | Wall Street Tycoon | - | - | -- -- -- | -- -- -- | -- -- --
| [RY2:TGE](https://en.wikipedia.org/wiki/Rayman_2:_The_Great_Escape) | Rayman 2: The Great Escape | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocket:RoW](https://en.wikipedia.org/wiki/Rocket:_Robot_on_Wheels) | Rocket: Robot on Wheels | - | - | -- -- -- | -- -- -- | -- -- --
| [M1RC](https://en.wikipedia.org/wiki/Mobil_1_Rally_Championship) | Mobil 1 Rally Championship | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLongestJourney](https://en.wikipedia.org/wiki/The_Longest_Journey) | The Longest Journey | - | - | -- -- -- | -- -- -- | -- -- --
| [Evolution:TWoSD](https://en.wikipedia.org/wiki/Evolution:_The_World_of_Sacred_Device) | Evolution: The World of Sacred Device | - | - | -- -- -- | -- -- -- | -- -- --
| [AlexBuildsHisFarm]() | Alex Builds His Farm | - | - | -- -- -- | -- -- -- | -- -- --
| [LaurasHappyAdventures](https://en.wikipedia.org/wiki/Laura%27s_Happy_Adventures) | Laura's Happy Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [Requiem:AA](https://en.wikipedia.org/wiki/Requiem:_Avenging_Angel) | Requiem: Avenging Angel | - | - | -- -- -- | -- -- -- | -- -- --
| [Skullcaps](https://en.wikipedia.org/wiki/Skull_Caps) | Skullcaps | - | - | -- -- -- | -- -- -- | -- -- --
| [SevenKingdomsII:TFW](https://en.wikipedia.org/wiki/Seven_Kingdoms_II:_The_Fryhtan_Wars) | Seven Kingdoms II: The Fryhtan Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [ShadowCompany:LfD](https://en.wikipedia.org/wiki/Shadow_Company:_Left_For_Dead) | Shadow Company: Left for Dead | - | - | -- -- -- | -- -- -- | -- -- --
| [Speed Devils](https://en.wikipedia.org/wiki/Speed_Devils) | Speed Devils | - | - | -- -- -- | -- -- -- | -- -- --
| [SAXR](https://en.wikipedia.org/wiki/Suzuki_Alstare_Extreme_Racing) | Suzuki Alstare Extreme Racing | - | - | -- -- -- | -- -- -- | -- -- --
| [Papyrus]() | Papyrus | - | - | -- -- -- | -- -- -- | -- -- --
| [Theocracy](https://en.wikipedia.org/wiki/Theocracy_(video_game)) | Theocracy | - | - | -- -- -- | -- -- -- | -- -- --
| [InspectorGadget:OM](https://en.wikipedia.org/wiki/Inspector_Gadget:_Operation_Madkactus) | Inspector Gadget: Operation Madkactus | - | - | -- -- -- | -- -- -- | -- -- --
| [BusinessTycoon]() | Business Tycoon | - | - | -- -- -- | -- -- -- | -- -- --
| [DisneysDinosaur](https://en.wikipedia.org/wiki/Disney%27s_Dinosaur_(video_game)) | Disney's Dinosaur | - | - | -- -- -- | -- -- -- | -- -- --
| [Evolution2:FoP](https://en.wikipedia.org/wiki/Evolution_2:_Far_Off_Promise) | Evolution 2: Far off Promise | - | - | -- -- -- | -- -- -- | -- -- --
| [AllStarTennis2000]() | All Star Tennis 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [Toonsylvania]() | Toonsylvania | - | - | -- -- -- | -- -- -- | -- -- --
| [SurfRiders](https://en.wikipedia.org/wiki/Surf_Riders) | Surf Riders | - | - | -- -- -- | -- -- -- | -- -- --
| [DeepFighter](https://en.wikipedia.org/wiki/Deep_Fighter) | Deep Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [Infestation]() | Infestation | - | - | -- -- -- | -- -- -- | -- -- --
| [OLearyManager2000](https://en.wikipedia.org/wiki/O%27Leary_Manager_2000) | O'Leary Manager 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [F1RacingChampionship](https://en.wikipedia.org/wiki/F1_Racing_Championship) | F1 Racing Championship | - | - | -- -- -- | -- -- -- | -- -- --
| [CarlLewisAthletics2000](https://en.wikipedia.org/wiki/Carl_Lewis_Athletics_2000) | Carl Lewis Athletics 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [InColdBlood](https://en.wikipedia.org/wiki/In_Cold_Blood_(video_game)) | In Cold Blood | - | - | -- -- -- | -- -- -- | -- -- --
| [VirtualSkipper]() | Virtual Skipper | - | - | -- -- -- | -- -- -- | -- -- --
| [BatmanBeyond:RotJ](https://en.wikipedia.org/wiki/Batman_Beyond:_Return_of_the_Joker_(video_game)) | Batman Beyond: Return of the Joker | - | - | -- -- -- | -- -- -- | -- -- --
| [DisneysDonaldDuck:GQ](https://en.wikipedia.org/wiki/Donald_Duck:_Goin%27_Quackers) | Disney's Donald Duck: Goin' Quackers | - | - | -- -- -- | -- -- -- | -- -- --
| [WaltDisneysTheJungleBook:RnG]() | Walt Disney's The Jungle Book: Rhythm n' Groove | - | - | -- -- -- | -- -- -- | -- -- --
| [GoldandGlory:TRtED]() | Gold and Glory: The Road to El Dorado | - | - | -- -- -- | -- -- -- | -- -- --
| [DisneysAladdin](https://en.wikipedia.org/wiki/Disney%27s_Aladdin_(Virgin_Games_video_game)) | Disney's Aladdin | - | - | -- -- -- | -- -- -- | -- -- --
| [WDTJB:MWA]() | Walt Disney's The Jungle Book: Mowgli's Wild Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS:RS](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six:_Rogue_Spear) | Tom Clancy's Rainbow Six: Rogue Spear | - | - | -- -- -- | -- -- -- | -- -- --
| [GrandiaII](https://en.wikipedia.org/wiki/Grandia_II) | Grandia II | - | - | -- -- -- | -- -- -- | -- -- --
| [PODSpeedZone](https://en.wikipedia.org/wiki/POD_2) | POD SpeedZone | - | - | -- -- -- | -- -- -- | -- -- --
| [RY2Forever]() | Rayman 2 Forever | - | - | -- -- -- | -- -- -- | -- -- --
| [TokyoXtremeRacer2](https://en.wikipedia.org/wiki/Tokyo_Xtreme_Racer_2) | Tokyo Xtreme Racer 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DTENG](https://en.wikipedia.org/wiki/The_Emperor%27s_New_Groove_(video_game)) | Disney's The Emperor's New Groove | - | - | -- -- -- | -- -- -- | -- -- --
| [EternalRing](https://en.wikipedia.org/wiki/Eternal_Ring) | Eternal Ring | - | - | -- -- -- | -- -- -- | -- -- --
| [LittleNicky](https://en.wikipedia.org/wiki/Little_Nicky_(video_game)) | Little Nicky | - | - | -- -- -- | -- -- -- | -- -- --
| [SCCR](https://en.wikipedia.org/wiki/Sno-Cross_Championship_Racing) | Sno-Cross Championship Racing | - | - | -- -- -- | -- -- -- | -- -- --
| [Arcatera:TDB]() | Arcatera: The Dark Brotherhood | - | - | -- -- -- | -- -- -- | -- -- --
| [ALGwRY]() | Amazing Learning Games with Rayman | - | - | -- -- -- | -- -- -- | -- -- --
| [AnneMcCaffreysFreedom:FR](https://en.wikipedia.org/wiki/Freedom:_First_Resistance) | Anne McCaffrey's Freedom: First Resistance | - | - | -- -- -- | -- -- -- | -- -- --
| [Animorphs](https://en.wikipedia.org/wiki/Animorphs_(video_game)) | Animorphs | - | - | -- -- -- | -- -- -- | -- -- --
| [DavidOLearysTotalSoccer2000]() | David O'Leary's Total Soccer 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [Grandia](https://en.wikipedia.org/wiki/Grandia_(video_game)) | Grandia | - | - | -- -- -- | -- -- -- | -- -- --
| [Papyrus:LSdlCP]() | Papyrus: Le Secret de la Cité Perdue | - | - | -- -- -- | -- -- -- | -- -- --
| [PrincesseSissietTempete]() | Princesse Sissi et Tempête | - | - | -- -- -- | -- -- -- | -- -- --
| [ProRally2001](https://en.wikipedia.org/wiki/Pro_Rally_2001) | Pro Rally 2001 | - | - | -- -- -- | -- -- -- | -- -- --
| [RYRevolution](https://en.wikipedia.org/wiki/Rayman_2:_The_Great_Escape) | Rayman Revolution | - | - | -- -- -- | -- -- -- | -- -- --
| [SeaDogs](https://en.wikipedia.org/wiki/Sea_Dogs_(video_game)) | Sea Dogs | - | - | -- -- -- | -- -- -- | -- -- --
| [Spirou:TRI](https://en.wikipedia.org/wiki/Spirou:_The_Robot_Invasion) | Spirou: The Robot Invasion | - | - | -- -- -- | -- -- -- | -- -- --
| [TheMaskofZorro](https://en.wikipedia.org/wiki/The_Mask_of_Zorro_(video_game)) | The Mask of Zorro | - | - | -- -- -- | -- -- -- | -- -- --
| [TomandJerryinFistsofFurry](https://en.wikipedia.org/wiki/Tom_and_Jerry_in_Fists_of_Furry) | Tom and Jerry in Fists of Furry | - | - | -- -- -- | -- -- -- | -- -- --
| [StupidInvaders](https://en.wikipedia.org/wiki/Stupid_Invaders) | Stupid Invaders | - | - | -- -- -- | -- -- -- | -- -- --
| [ArmoredCore2](https://en.wikipedia.org/wiki/Armored_Core_2) | Armored Core 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TSIV](https://en.wikipedia.org/wiki/The_Settlers_IV) | The Settlers IV | - | - | -- -- -- | -- -- -- | -- -- --
| [FlipperLopaka]() | Flipper & Lopaka | - | - | -- -- -- | -- -- -- | -- -- --
| [Batman:CiG](https://en.wikipedia.org/wiki/Batman:_Chaos_in_Gotham) | Batman: Chaos in Gotham | - | - | -- -- -- | -- -- -- | -- -- --
| [MystIII:E](https://en.wikipedia.org/wiki/Myst_III:_Exile) | Myst III: Exile | - | - | -- -- -- | -- -- -- | -- -- --
| [ConflictZone](https://en.wikipedia.org/wiki/Conflict_Zone) | Conflict Zone | - | - | -- -- -- | -- -- -- | -- -- --
| [EurofighterTyphoon](https://en.wikipedia.org/wiki/Eurofighter_Typhoon_(video_game)) | Eurofighter Typhoon | - | - | -- -- -- | -- -- -- | -- -- --
| [Hype:TTQ](https://en.wikipedia.org/wiki/Hype:_The_Time_Quest) | Hype: The Time Quest | - | - | -- -- -- | -- -- -- | -- -- --
| [RYAdvance]() | Rayman Advance | - | - | -- -- -- | -- -- -- | -- -- --
| [EuropaUniversalis](https://en.wikipedia.org/wiki/Europa_Universalis) | Europa Universalis | - | - | -- -- -- | -- -- -- | -- -- --
| [RoswellConspiracies:AML]() | Roswell Conspiracies: Aliens, Myths & Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [Moderngroove:MoSE]() | Moderngroove: Ministry of Sound Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [VIP]() | V.I.P. | - | - | -- -- -- | -- -- -- | -- -- --
| [DragonRiders:CoP](https://en.wikipedia.org/wiki/Dragonriders:_Chronicles_of_Pern) | Dragon Riders: Chronicles of Pern | - | - | -- -- -- | -- -- -- | -- -- --
| [Conquest:FW](https://en.wikipedia.org/wiki/Conquest:_Frontier_Wars) | Conquest: Frontier Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [PlanetoftheApes](https://en.wikipedia.org/wiki/Planet_of_the_Apes_(video_game)) | Planet of the Apes | - | - | -- -- -- | -- -- -- | -- -- --
| [FinalFight](https://en.wikipedia.org/wiki/Final_Fight_(video_game)) | Final Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [PoolofRadiance:RoMD](https://en.wikipedia.org/wiki/Pool_of_Radiance:_Ruins_of_Myth_Drannor) | Pool of Radiance: Ruins of Myth Drannor | - | - | -- -- -- | -- -- -- | -- -- --
| [EvilTwin:CC](https://en.wikipedia.org/wiki/Evil_Twin:_Cyprien%27s_Chronicles) | Evil Twin: Cyprien's Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [Batman:V](https://en.wikipedia.org/wiki/Batman:_Vengeance) | Batman: Vengeance | - | - | -- -- -- | -- -- -- | -- -- --
| [Kohan:IS](https://en.wikipedia.org/wiki/Kohan:_Immortal_Sovereigns) | Kohan: Immortal Sovereigns | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS:RSBT]() | Tom Clancy's Rainbow Six: Rogue Spear - Black Thorn | - | - | -- -- -- | -- -- -- | -- -- --
| [GadgetTycoon]() | Gadget Tycoon | - | - | -- -- -- | -- -- -- | -- -- --
| [SunnyGarciaSurfing](https://en.wikipedia.org/wiki/Sunny_Garcia_Surfing) | Sunny Garcia Surfing | - | - | -- -- -- | -- -- -- | -- -- --
| [RallyChampionship2002](https://en.wikipedia.org/wiki/Rally_Championship_Xtreme) | Rally Championship 2002 | - | - | -- -- -- | -- -- -- | -- -- --
| [SHII](https://en.wikipedia.org/wiki/Silent_Hunter_II) | Silent Hunter II | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleRealms](https://en.wikipedia.org/wiki/Battle_Realms) | Battle Realms | - | - | -- -- -- | -- -- -- | -- -- --
| [TheFinalCut](https://en.wikipedia.org/wiki/Alfred_Hitchcock_Presents:_The_Final_Cut) | The Final Cut | - | - | -- -- -- | -- -- -- | -- -- --
| [SSFII:TR]() | Super Street Fighter II: Turbo Revival | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR01](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_(2001_video_game)) | Tom Clancy's Ghost Recon '01 | - | - | -- -- -- | -- -- -- | -- -- --
| [DisneysTarzanUntamed](https://en.wikipedia.org/wiki/Tarzan:_Untamed) | Disney's Tarzan Untamed | - | - | -- -- -- | -- -- -- | -- -- --
| [IL2S](https://en.wikipedia.org/wiki/IL-2_Sturmovik_(video_game)) | IL-2 Sturmovik | - | - | -- -- -- | -- -- -- | -- -- --
| [WDSWatSD]() | Walt Disney's Snow White and the Seven Dwarfs | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperBustAMove]() | Super Bust-A-Move | - | - | -- -- -- | -- -- -- | -- -- --
| [MegaManBattleNetwork](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_(video_game)) | Mega Man Battle Network | - | - | -- -- -- | -- -- -- | -- -- --
| [RYArena]() | Rayman Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLegendofAlonDar](https://en.wikipedia.org/wiki/The_Legend_of_Alon_D%27ar) | The Legend of Alon D'ar | - | - | -- -- -- | -- -- -- | -- -- --
| [BreathofFire](https://en.wikipedia.org/wiki/Breath_of_Fire_(video_game)) | Breath of Fire | - | - | -- -- -- | -- -- -- | -- -- --
| [WormsWorldParty](https://en.wikipedia.org/wiki/Worms_World_Party) | Worms World Party | - | - | -- -- -- | -- -- -- | -- -- --
| [TrevorChansCapitalismII](https://en.wikipedia.org/wiki/Capitalism_II) | Trevor Chan's Capitalism II | - | - | -- -- -- | -- -- -- | -- -- --
| [JadeCocoon2](https://en.wikipedia.org/wiki/Jade_Cocoon_2) | Jade Cocoon 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DonaldDuckAdvance]() | Donald Duck Advance | - | - | -- -- -- | -- -- -- | -- -- --
| [AFPM2001]() | Alex Ferguson's Player Manager 2001 | - | - | -- -- -- | -- -- -- | -- -- --
| [Batman:GCR](https://en.wikipedia.org/wiki/Batman:_Gotham_City_Racer) | Batman: Gotham City Racer | - | - | -- -- -- | -- -- -- | -- -- --
| [EscapefromMonkeyIsland](https://en.wikipedia.org/wiki/Escape_from_Monkey_Island) | Escape from Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [Gunfighter:TLoJJ](https://en.wikipedia.org/wiki/Gunfighter:_The_Legend_of_Jesse_James) | Gunfighter: The Legend of Jesse James | - | - | -- -- -- | -- -- -- | -- -- --
| [InspectorGadget:GCM](https://en.wikipedia.org/wiki/Inspector_Gadget:_Gadget%27s_Crazy_Maze) | Inspector Gadget: Gadget's Crazy Maze | - | - | -- -- -- | -- -- -- | -- -- --
| [PearlHarbor:SaD]() | Pearl Harbor: Strike at Dawn | - | - | -- -- -- | -- -- -- | -- -- --
| [Scrabble01]() | Scrabble '01 | - | - | -- -- -- | -- -- -- | -- -- --
| [Taxi2]() | Taxi 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TokyoXtremeRacer:Z](https://en.wikipedia.org/wiki/Tokyo_Xtreme_Racer:_Zero) | Tokyo Xtreme Racer: Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [SaltLake2002](https://en.wikipedia.org/wiki/Salt_Lake_2002_(video_game)) | Salt Lake 2002 | - | - | -- -- -- | -- -- -- | -- -- --
| [Echelon](https://en.wikipedia.org/wiki/Echelon_(2001_video_game)) | Echelon | - | - | -- -- -- | -- -- -- | -- -- --
| [DestroyerCommand](https://en.wikipedia.org/wiki/Destroyer_Command) | Destroyer Command | - | - | -- -- -- | -- -- -- | -- -- --
| [MikeTysonBoxing](https://en.wikipedia.org/wiki/Mike_Tyson_Boxing) | Mike Tyson Boxing | - | - | -- -- -- | -- -- -- | -- -- --
| [EvolutionWorlds](https://en.wikipedia.org/wiki/Evolution_Worlds) | Evolution Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [RYRush]() | Rayman Rush | - | - | -- -- -- | -- -- -- | -- -- --
| [Bratz]() | Bratz | - | - | -- -- -- | -- -- -- | -- -- --
| [Warlords:BII](https://en.wikipedia.org/wiki/Warlords_Battlecry_II) | Warlords: Battlecry II | - | - | -- -- -- | -- -- -- | -- -- --
| [ETTheExtraTerrestrial:IM]() | E.T. The Extra-Terrestrial: Interplanetary Mission | - | - | -- -- -- | -- -- -- | -- -- --
| [DarkPlanet:BfN](https://en.wikipedia.org/wiki/Dark_Planet:_Battle_for_Natrolis) | Dark Planet: Battle for Natrolis | - | - | -- -- -- | -- -- -- | -- -- --
| [JHBitBBH](https://en.wikipedia.org/wiki/Jim_Henson%27s_Bear_in_the_Big_Blue_House) | Jim Henson's Bear in the Big Blue House | - | - | -- -- -- | -- -- -- | -- -- --
| [XBladez:IS]() | X-Bladez: Inline Skater | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR:DS](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon:_Desert_Siege) | Tom Clancy's Ghost Recon: Desert Siege | - | - | -- -- -- | -- -- -- | -- -- --
| [ET:PHA]() | E.T.: Phone Home Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [ProRally2002](https://en.wikipedia.org/wiki/Pro_Rally_2002) | Pro Rally 2002 | - | - | -- -- -- | -- -- -- | -- -- --
| [EuropaUniversalisII](https://en.wikipedia.org/wiki/Europa_Universalis_II) | Europa Universalis II | - | - | -- -- -- | -- -- -- | -- -- --
| [HootersRoadTrip](https://en.wikipedia.org/wiki/Hooters_Road_Trip) | Hooters Road Trip | - | - | -- -- -- | -- -- -- | -- -- --
| [SabrinatheTeenageWitch:PC]() | Sabrina, the Teenage Witch: Potion Commotion | - | - | -- -- -- | -- -- -- | -- -- --
| [UFC:T](https://en.wikipedia.org/wiki/Ultimate_Fighting_Championship:_Tapout) | Ultimate Fighting Championship: Tapout | - | - | -- -- -- | -- -- -- | -- -- --
| [MonsterJam:MD]() | Monster Jam: Maximum Destruction | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSumofAllFears](https://en.wikipedia.org/wiki/The_Sum_of_All_Fears_(video_game)) | The Sum of All Fears | - | - | -- -- -- | -- -- -- | -- -- --
| [UFC:T2](https://en.wikipedia.org/wiki/UFC:_Throwdown) | UFC: Throwdown | - | - | -- -- -- | -- -- -- | -- -- --
| [TESIII:M](https://en.wikipedia.org/wiki/The_Elder_Scrolls_III:_Morrowind) | The Elder Scrolls III: Morrowind | - | - | -- -- -- | -- -- -- | -- -- --
| [SGEWorldManager]() | Sven-Göran Eriksson's World Manager | - | - | -- -- -- | -- -- -- | -- -- --
| [BreathofFireII](https://en.wikipedia.org/wiki/Breath_of_Fire_II) | Breath of Fire II | - | - | -- -- -- | -- -- -- | -- -- --
| [MuppetPinballMayhem]() | Muppet Pinball Mayhem | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS:LW]() | Tom Clancy's Rainbow Six: Lone Wolf | - | - | -- -- -- | -- -- -- | -- -- --
| [Dokapon:MH](https://en.wikipedia.org/wiki/Dokapon:_Monster_Hunter) | Dokapon: Monster Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [TigerWoodsPGATourGolf]() | Tiger Woods PGA Tour Golf | - | - | -- -- -- | -- -- -- | -- -- --
| [WormsBlast](https://en.wikipedia.org/wiki/Worms_Blast) | Worms Blast | - | - | -- -- -- | -- -- -- | -- -- --
| [LargoWinch:EUT]() | Largo Winch: Empire Under Threat | - | - | -- -- -- | -- -- -- | -- -- --
| [Chessmaster9000](https://en.wikipedia.org/wiki/Chessmaster_9000) | Chessmaster 9000 | - | - | -- -- -- | -- -- -- | -- -- --
| [ColinMcRaeRally20](https://en.wikipedia.org/wiki/Colin_McRae_Rally_2.0) | Colin McRae Rally 2.0 | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperBustAMove2]() | Super Bust-A-Move 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR:IT](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon:_Island_Thunder) | Tom Clancy's Ghost Recon: Island Thunder | - | - | -- -- -- | -- -- -- | -- -- --
| [Deathrow](https://en.wikipedia.org/wiki/Deathrow_(video_game)) | Deathrow | - | - | -- -- -- | -- -- -- | -- -- --
| [MMBattleNetwork2](https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_2) | Mega Man Battle Network 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Wizardry:TotFL](https://en.wikipedia.org/wiki/Wizardry:_Tale_of_the_Forsaken_Land) | Wizardry: Tale of the Forsaken Land | - | - | -- -- -- | -- -- -- | -- -- --
| [SpeedChallenge:JVRV](https://en.wikipedia.org/wiki/Speed_Challenge:_Jacques_Villeneuve%27s_Racing_Vision) | Speed Challenge: Jacques Villeneuve's Racing Vision | - | - | -- -- -- | -- -- -- | -- -- --
| [DisneysPK:OotS](https://en.wikipedia.org/wiki/PK:_Out_of_the_Shadows) | Disney's PK: Out of the Shadows | - | - | -- -- -- | -- -- -- | -- -- --
| [RidingChampion:LoRH]() | Riding Champion: Legacy of Rosemond Hill | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocky](https://en.wikipedia.org/wiki/Rocky_(2002_video_game)) | Rocky | - | - | -- -- -- | -- -- -- | -- -- --
| [TombRaider:TP](https://en.wikipedia.org/wiki/Tomb_Raider:_The_Prophecy) | Tomb Raider: The Prophecy | - | - | -- -- -- | -- -- -- | -- -- --
| [Catz5]() | Catz 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [Dogz5]() | Dogz 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [DragonsLair3D:RttL](https://en.wikipedia.org/wiki/Dragon%27s_Lair_3D:_Return_to_the_Lair) | Dragon's Lair 3D: Return to the Lair | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Splinter_Cell_(video_game)) | Tom Clancy's Splinter Cell | - | - | -- -- -- | -- -- -- | -- -- --
| [TheMummy]() | The Mummy | - | - | -- -- -- | -- -- -- | -- -- --
| [SFAlpha3](https://en.wikipedia.org/wiki/Street_Fighter_Alpha_3) | Street Fighter Alpha 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Globetrotter2](https://en.wikipedia.org/wiki/Globetrotter_2) | Globetrotter 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RS3:RST](https://en.wikipedia.org/wiki/Racing_Simulation_3) | RS3: Racing Simulation Three | - | - | -- -- -- | -- -- -- | -- -- --
| [LunarLegend](https://en.wikipedia.org/wiki/Lunar_Legend) | Lunar Legend | - | - | -- -- -- | -- -- -- | -- -- --
| [DisneysTreasurePlanet]() | Disney's Treasure Planet | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoRacerAdvance](https://en.wikipedia.org/wiki/Moto_Racer_Advance) | Moto Racer Advance | - | - | -- -- -- | -- -- -- | -- -- --
| [ETTheExtraTerrestrial:AFH]() | E.T. The Extra-Terrestrial: Away From Home | - | - | -- -- -- | -- -- -- | -- -- --
| [JetIonGP]() | Jet Ion GP | - | - | -- -- -- | -- -- -- | -- -- --
| [LargoWinch:CS]() | Largo Winch .// Commando SAR | - | - | -- -- -- | -- -- -- | -- -- --
| [Moorhen3:CC]() | Moorhen 3: Chicken Chase | - | - | -- -- -- | -- -- -- | -- -- --
| [SGEWorldChallenge]() | Sven-Göran Eriksson's World Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [TrainsTrucksTycoon]() | Trains & Trucks Tycoon | - | - | -- -- -- | -- -- -- | -- -- --
| [Taxi3]() | Taxi 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SwordoftheSamurai](https://en.wikipedia.org/wiki/Sword_of_the_Samurai_(video_game)) | Sword of the Samurai | - | - | -- -- -- | -- -- -- | -- -- --
| [WildArms3](https://en.wikipedia.org/wiki/Wild_Arms_3) | Wild Arms 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [IL2S:FB](https://en.wikipedia.org/wiki/IL-2_Sturmovik:_Forgotten_Battles) | IL-2 Sturmovik: Forgotten Battles | - | - | -- -- -- | -- -- -- | -- -- --
| [Murakumo:RMP](https://en.wikipedia.org/wiki/Murakumo:_Renegade_Mech_Pursuit) | Murakumo: Renegade Mech Pursuit | - | - | -- -- -- | -- -- -- | -- -- --
| [RY3:HH](https://en.wikipedia.org/wiki/Rayman_3:_Hoodlum_Havoc) | Rayman 3: Hoodlum Havoc | - | - | -- -- -- | -- -- -- | -- -- --
| [GunfighterII:RoJJ](https://en.wikipedia.org/wiki/Gunfighter_II:_Revenge_of_Jesse_James) | Gunfighter II: Revenge of Jesse James | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS3:RS](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_3:_Raven_Shield) | Tom Clancy's Rainbow Six 3: Raven Shield | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:CSI]() | CSI: Crime Scene Investigation | - | - | -- -- -- | -- -- -- | -- -- --
| [Shadowbane](https://en.wikipedia.org/wiki/Shadowbane) | Shadowbane | - | - | -- -- -- | -- -- -- | -- -- --
| [Chessmaster](https://en.wikipedia.org/wiki/Chessmaster) | Chessmaster | - | - | -- -- -- | -- -- -- | -- -- --
| [WillRock](https://en.wikipedia.org/wiki/Will_Rock) | Will Rock | - | - | -- -- -- | -- -- -- | -- -- --
| [ApeEscape2](https://en.wikipedia.org/wiki/Ape_Escape_2) | Ape Escape 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [CharliesAngels](https://en.wikipedia.org/wiki/Charlie%27s_Angels_(video_game)) | Charlie's Angels | - | - | -- -- -- | -- -- -- | -- -- --
| [CTHD](https://en.wikipedia.org/wiki/Crouching_Tiger,_Hidden_Dragon_(video_game)) | Crouching Tiger, Hidden Dragon | - | - | -- -- -- | -- -- -- | -- -- --
| [XIII](https://en.wikipedia.org/wiki/XIII_(video_game)) | XIII | - | - | -- -- -- | -- -- -- | -- -- --
| [Batman:RoST](https://en.wikipedia.org/wiki/Batman:_Rise_of_Sin_Tzu) | Batman: Rise of Sin Tzu | - | - | -- -- -- | -- -- -- | -- -- --
| [InMemoriam](https://en.wikipedia.org/wiki/In_Memoriam_(video_game)) | In Memoriam | - | - | -- -- -- | -- -- -- | -- -- --
| [WarlordsIV:HoE](https://en.wikipedia.org/wiki/Warlords_IV:_Heroes_of_Etheria) | Warlords IV: Heroes of Etheria | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TSoT](https://en.wikipedia.org/wiki/Prince_of_Persia:_The_Sands_of_Time) | Prince of Persia: The Sands of Time | - | - | -- -- -- | -- -- -- | -- -- --
| [BeyondGoodEvil](https://en.wikipedia.org/wiki/Beyond_Good_%26_Evil_(video_game)) | Beyond Good & Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [Uru:ABM](https://en.wikipedia.org/wiki/Uru:_Ages_Beyond_Myst) | Uru: Ages Beyond Myst | - | - | -- -- -- | -- -- -- | -- -- --
| [SaddleUp:TtR]() | Saddle Up: Time to Ride | - | - | -- -- -- | -- -- -- | -- -- --
| [MuchaLucha:MotLC]() | Mucha Lucha! Mascaritas of the Lost Code | - | - | -- -- -- | -- -- -- | -- -- --
| [LockOn:MAC](https://en.wikipedia.org/wiki/Lock_On:_Modern_Air_Combat) | Lock On: Modern Air Combat | - | - | -- -- -- | -- -- -- | -- -- --
| [Monster4x4:MoM]() | Monster 4x4: Masters of Metal | - | - | -- -- -- | -- -- -- | -- -- --
| [Biathlon2004]() | Biathlon 2004 | - | - | -- -- -- | -- -- -- | -- -- --
| [DowntownRun](https://en.wikipedia.org/wiki/Downtown_Run) | Downtown Run | - | - | -- -- -- | -- -- -- | -- -- --
| [Scrabble:2003E]() | Scrabble: 2003 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [TOCA:WTC](https://en.wikipedia.org/wiki/TOCA_World_Touring_Cars) | TOCA: World Touring Cars | - | - | -- -- -- | -- -- -- | -- -- --
| [BaldursGate:DA](https://en.wikipedia.org/wiki/Baldur%27s_Gate:_Dark_Alliance) | Baldur's Gate: Dark Alliance | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS3:AS](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_3:_Raven_Shield) | Tom Clancy's Rainbow Six 3: Athena Sword | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR:JS](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon:_Jungle_Storm) | Tom Clancy's Ghost Recon: Jungle Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:DM](https://en.wikipedia.org/wiki/CSI:_Dark_Motives) | CSI: Dark Motives | - | - | -- -- -- | -- -- -- | -- -- --
| [FC](https://en.wikipedia.org/wiki/Far_Cry_(video_game)) | Far Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:PT](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Splinter_Cell:_Pandora_Tomorrow) | Tom Clancy's Splinter Cell: Pandora Tomorrow | - | - | -- -- -- | -- -- -- | -- -- --
| [HarvestMoon:AWL](https://en.wikipedia.org/wiki/Harvest_Moon:_A_Wonderful_Life) | Harvest Moon: A Wonderful Life | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR2](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_2) | Tom Clancy's Ghost Recon 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [FatalFrameII:CB](https://en.wikipedia.org/wiki/Fatal_Frame_II:_Crimson_Butterfly) | Fatal Frame II: Crimson Butterfly | - | - | -- -- -- | -- -- -- | -- -- --
| [ChampionsofNorrath](https://en.wikipedia.org/wiki/Champions_of_Norrath) | Champions of Norrath | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS3:BA](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_3:_Raven_Shield) | Tom Clancy's Rainbow Six 3: Black Arrow | - | - | -- -- -- | -- -- -- | -- -- --
| [ThePoliticalMachine](https://en.wikipedia.org/wiki/The_Political_Machine) | The Political Machine | - | - | -- -- -- | -- -- -- | -- -- --
| [Chessmaster10thEdition](https://en.wikipedia.org/wiki/Chessmaster_10th_Edition) | Chessmaster 10th Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [AdvanceGuardianHeroes](https://en.wikipedia.org/wiki/Advance_Guardian_Heroes) | Advance Guardian Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [StarWarsTrilogy:AotF](https://en.wikipedia.org/wiki/Star_Wars_Trilogy:_Apprentice_of_the_Force) | Star Wars Trilogy: Apprentice of the Force | - | - | -- -- -- | -- -- -- | -- -- --
| [RockyLegends](https://en.wikipedia.org/wiki/Rocky_Legends) | Rocky Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [SherlockHolmes:TCotSE](https://en.wikipedia.org/wiki/Sherlock_Holmes:_The_Case_of_the_Silver_Earring) | Sherlock Holmes: The Case of the Silver Earring | - | - | -- -- -- | -- -- -- | -- -- --
| [TheDukesofHazzard:RotGL](https://en.wikipedia.org/wiki/The_Dukes_of_Hazzard:_Return_of_the_General_Lee) | The Dukes of Hazzard: Return of the General Lee | - | - | -- -- -- | -- -- -- | -- -- --
| [MystIV:R](https://en.wikipedia.org/wiki/Myst_IV:_Revelation) | Myst IV: Revelation | - | - | -- -- -- | -- -- -- | -- -- --
| [ApeEscape:PP](https://en.wikipedia.org/wiki/Ape_Escape:_Pumped_%26_Primed) | Ape Escape: Pumped & Primed | - | - | -- -- -- | -- -- -- | -- -- --
| [PacificFighters](https://en.wikipedia.org/wiki/Pacific_Fighters) | Pacific Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:M]() | CSI: Miami | - | - | -- -- -- | -- -- -- | -- -- --
| [Asphalt:UG](https://en.wikipedia.org/wiki/Asphalt_Urban_GT) | Asphalt: Urban GT | - | - | -- -- -- | -- -- -- | -- -- --
| [Alexander](https://en.wikipedia.org/wiki/Alexander_(video_game)) | Alexander | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:WW](https://en.wikipedia.org/wiki/Prince_of_Persia:_Warrior_Within) | Prince of Persia: Warrior Within | - | - | -- -- -- | -- -- -- | -- -- --
| [Biathlon2005]() | Biathlon 2005 | - | - | -- -- -- | -- -- -- | -- -- --
| [Sprung](https://en.wikipedia.org/wiki/Sprung_(video_game)) | Sprung | - | - | -- -- -- | -- -- -- | -- -- --
| [Tork:PP](https://en.wikipedia.org/wiki/Tork:_Prehistoric_Punk) | Tork: Prehistoric Punk | - | - | -- -- -- | -- -- -- | -- -- --
| [DisneysWinniethePooh:RTA](https://en.wikipedia.org/wiki/Winnie_the_Pooh%27s_Rumbly_Tumbly_Adventure) | Disney's Winnie the Pooh's Rumbly Tumbly Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [HeritageofKings:TS](https://en.wikipedia.org/wiki/The_Settlers:_Heritage_of_Kings) | Heritage of Kings: The Settlers | - | - | -- -- -- | -- -- -- | -- -- --
| [BiA:RtH3](https://en.wikipedia.org/wiki/Brothers_in_Arms:_Road_to_Hill_30) | Brothers in Arms: Road to Hill 30 | - | - | -- -- -- | -- -- -- | -- -- --
| [Playboy:TM](https://en.wikipedia.org/wiki/Playboy:_The_Mansion) | Playboy: The Mansion | - | - | -- -- -- | -- -- -- | -- -- --
| [ColdFear](https://en.wikipedia.org/wiki/Cold_Fear) | Cold Fear | - | - | -- -- -- | -- -- -- | -- -- --
| [Champions:RtA](https://en.wikipedia.org/wiki/Champions:_Return_to_Arms) | Champions: Return to Arms | - | - | -- -- -- | -- -- -- | -- -- --
| [RY:HR](https://en.wikipedia.org/wiki/Rayman:_Hoodlums%27_Revenge) | Rayman: Hoodlum's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [SHIII](https://en.wikipedia.org/wiki/Silent_Hunter_III) | Silent Hunter III | - | - | -- -- -- | -- -- -- | -- -- --
| [RYDS]() | Rayman DS | - | - | -- -- -- | -- -- -- | -- -- --
| [TheBardsTale](https://en.wikipedia.org/wiki/The_Bard%27s_Tale_(2004_video_game)) | The Bard's Tale | - | - | -- -- -- | -- -- -- | -- -- --
| [Lumines:PF](https://en.wikipedia.org/wiki/Lumines:_Puzzle_Fusion) | Lumines: Puzzle Fusion | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:CT](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Splinter_Cell:_Chaos_Theory) | Tom Clancy's Splinter Cell: Chaos Theory | - | - | -- -- -- | -- -- -- | -- -- --
| [StarWars:EIII](https://en.wikipedia.org/wiki/Star_Wars:_Episode_III_-_Revenge_of_the_Sith_(video_game)) | Star Wars: Episode III - Revenge of the Sith | - | - | -- -- -- | -- -- -- | -- -- --
| [Bomberman](https://en.wikipedia.org/wiki/Bomberman_(Nintendo_DS)) | Bomberman | - | - | -- -- -- | -- -- -- | -- -- --
| [BombermanHardball](https://en.wikipedia.org/wiki/Bomberman_Hardball) | Bomberman Hardball | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR2:SS](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_2:_Summit_Strike) | Tom Clancy's Ghost Recon 2: Summit Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [Darkwatch](https://en.wikipedia.org/wiki/Darkwatch) | Darkwatch | - | - | -- -- -- | -- -- -- | -- -- --
| [187:RoD](https://en.wikipedia.org/wiki/187_Ride_or_Die) | 187: Ride or Die | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS:L](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six:_Lockdown) | Tom Clancy's Rainbow Six: Lockdown | - | - | -- -- -- | -- -- -- | -- -- --
| [MystV:EoA](https://en.wikipedia.org/wiki/Myst_V:_End_of_Ages) | Myst V: End of Ages | - | - | -- -- -- | -- -- -- | -- -- --
| [MarathonManager]() | Marathon Manager | - | - | -- -- -- | -- -- -- | -- -- --
| [FCInstincts](https://en.wikipedia.org/wiki/Far_Cry_Instincts) | Far Cry Instincts | - | - | -- -- -- | -- -- -- | -- -- --
| [Lunar:DS](https://en.wikipedia.org/wiki/Lunar:_Dragon_Song) | Lunar: Dragon Song | - | - | -- -- -- | -- -- -- | -- -- --
| [HeroesofthePacific](https://en.wikipedia.org/wiki/Heroes_of_the_Pacific) | Heroes of the Pacific | - | - | -- -- -- | -- -- -- | -- -- --
| [BiA:EiB](https://en.wikipedia.org/wiki/Brothers_in_Arms:_Earned_in_Blood) | Brothers in Arms: Earned in Blood | - | - | -- -- -- | -- -- -- | -- -- --
| [FLOW:UDU]() | FLOW: Urban Dance Uprising | - | - | -- -- -- | -- -- -- | -- -- --
| [AA:RoaS](https://en.wikipedia.org/wiki/America%27s_Army:_Rise_of_a_Soldier) | America's Army: Rise of a Soldier | - | - | -- -- -- | -- -- -- | -- -- --
| [AA:TS](https://en.wikipedia.org/wiki/America%27s_Army:_True_Soldiers) | America's Army: True Soldiers | - | - | -- -- -- | -- -- -- | -- -- --
| [GripShift](https://en.wikipedia.org/wiki/GripShift) | GripShift | - | - | -- -- -- | -- -- -- | -- -- --
| [KingKong:TOGotM](https://en.wikipedia.org/wiki/King_Kong_(2005_video_game)) | King Kong: The Official Game of the Movie | - | - | -- -- -- | -- -- -- | -- -- --
| [Kong:T8thWotWorld]() | Kong: The 8th Wonder of the World | - | - | -- -- -- | -- -- -- | -- -- --
| [Trollz:HA]() | Trollz: Hair Affair! | - | - | -- -- -- | -- -- -- | -- -- --
| [Frantix](https://en.wikipedia.org/wiki/Frantix) | Frantix | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TTT](https://en.wikipedia.org/wiki/Prince_of_Persia:_The_Two_Thrones) | Prince of Persia: The Two Thrones | - | - | -- -- -- | -- -- -- | -- -- --
| [BattlesofPoP](https://en.wikipedia.org/wiki/Battles_of_Prince_of_Persia) | Battles of Prince of Persia | - | - | -- -- -- | -- -- -- | -- -- --
| [Biathlon2006:GfG]() | Biathlon 2006: Go for Gold | - | - | -- -- -- | -- -- -- | -- -- --
| [PippaFunnell:TSFI]() | Pippa Funnell: The Stud Farm Inheritance | - | - | -- -- -- | -- -- -- | -- -- --
| [RugbyChallenge2006](https://en.wikipedia.org/wiki/Rugby_Challenge_2006) | Rugby Challenge 2006 | - | - | -- -- -- | -- -- -- | -- -- --
| [Drakengard2](https://en.wikipedia.org/wiki/Drakengard_2) | Drakengard 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Exit](https://en.wikipedia.org/wiki/Exit_(video_game)) | Exit | - | - | -- -- -- | -- -- -- | -- -- --
| [Curling2006]() | Curling 2006 | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGRAW](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_Advanced_Warfighter) | Tom Clancy's Ghost Recon Advanced Warfighter | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:3DoM](https://en.wikipedia.org/wiki/CSI:_3_Dimensions_of_Murder) | CSI: 3 Dimensions of Murder | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS:CH](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six:_Critical_Hour) | Tom Clancy's Rainbow Six: Critical Hour | - | - | -- -- -- | -- -- -- | -- -- --
| [CoC:DCotE](https://en.wikipedia.org/wiki/Call_of_Cthulhu:_Dark_Corners_of_the_Earth) | Call of Cthulhu: Dark Corners of the Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [BlazingAngels:SoWWII](https://en.wikipedia.org/wiki/Blazing_Angels:_Squadrons_of_WWII) | Blazing Angels: Squadrons of WWII | - | - | -- -- -- | -- -- -- | -- -- --
| [Monster4x4:WC](https://en.wikipedia.org/wiki/Monster_4x4:_World_Circuit) | Monster 4x4: World Circuit | - | - | -- -- -- | -- -- -- | -- -- --
| [StreetRiders](https://en.wikipedia.org/wiki/Street_Riders) | Street Riders | - | - | -- -- -- | -- -- -- | -- -- --
| [FC:I](https://en.wikipedia.org/wiki/Far_Cry_Instincts) | Far Cry: Instincts | - | - | -- -- -- | -- -- -- | -- -- --
| [PippaFunnell:TtR](https://en.wikipedia.org/wiki/Pippa_Funnell:_Take_the_Reins) | Pippa Funnell: Take the Reins | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:E](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Splinter_Cell:_Essentials) | Tom Clancy's Splinter Cell: Essentials | - | - | -- -- -- | -- -- -- | -- -- --
| [Catz]() | Catz | - | - | -- -- -- | -- -- -- | -- -- --
| [Dogz]() | Dogz | - | - | -- -- -- | -- -- -- | -- -- --
| [LostMagic](https://en.wikipedia.org/wiki/LostMagic) | LostMagic | - | - | -- -- -- | -- -- -- | -- -- --
| [Paradise](https://en.wikipedia.org/wiki/Paradise_(video_game)) | Paradise | - | - | -- -- -- | -- -- -- | -- -- --
| [HoMnMV](https://en.wikipedia.org/wiki/Heroes_of_Might_and_Magic_V) | Heroes of Might and Magic V | - | - | -- -- -- | -- -- -- | -- -- --
| [CoJ](https://en.wikipedia.org/wiki/Call_of_Juarez_(video_game)) | Call of Juarez | - | - | -- -- -- | -- -- -- | -- -- --
| [AND1Streetball](https://en.wikipedia.org/wiki/AND_1_Streetball) | AND 1 Streetball | - | - | -- -- -- | -- -- -- | -- -- --
| [AstonishiaStory](https://en.wikipedia.org/wiki/Astonishia_Story) | Astonishia Story | - | - | -- -- -- | -- -- -- | -- -- --
| [OverGFighters](https://en.wikipedia.org/wiki/Over_G_Fighters) | Over G Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [PiratesoftheCaribbean:TLoJS](https://en.wikipedia.org/wiki/Pirates_of_the_Caribbean:_The_Legend_of_Jack_Sparrow) | Pirates of the Caribbean: The Legend of Jack Sparrow | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC3:DASE]() | Devil May Cry 3: Dante's Awakening - Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [EnchantedArms](https://en.wikipedia.org/wiki/Enchanted_Arms) | Enchanted Arms | - | - | -- -- -- | -- -- -- | -- -- --
| [FacesofWar](https://en.wikipedia.org/wiki/Faces_of_War) | Faces of War | - | - | -- -- -- | -- -- -- | -- -- --
| [OpenSeason](https://en.wikipedia.org/wiki/Open_Season_(video_game)) | Open Season | - | - | -- -- -- | -- -- -- | -- -- --
| [ImportTunerChallenge](https://en.wikipedia.org/wiki/Import_Tuner_Challenge) | Import Tuner Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [TSII:10A](https://en.wikipedia.org/wiki/The_Settlers_II_(10th_Anniversary)) | The Settlers II: 10th Anniversary | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:DA](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Splinter_Cell:_Double_Agent) | Tom Clancy's Splinter Cell: Double Agent | - | - | -- -- -- | -- -- -- | -- -- --
| [DMoMnM](https://en.wikipedia.org/wiki/Dark_Messiah_of_Might_and_Magic) | Dark Messiah of Might and Magic | - | - | -- -- -- | -- -- -- | -- -- --
| [MindQuiz](https://en.wikipedia.org/wiki/Mind_Quiz) | Mind Quiz | - | - | -- -- -- | -- -- -- | -- -- --
| [Asphalt:UGT2](https://en.wikipedia.org/wiki/Asphalt:_Urban_GT_2) | Asphalt: Urban GT 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [GTProSeries](https://en.wikipedia.org/wiki/GT_Pro_Series) | GT Pro Series | - | - | -- -- -- | -- -- -- | -- -- --
| [RYRavingRabbids](https://en.wikipedia.org/wiki/Rayman_Raving_Rabbids) | Rayman Raving Rabbids | - | - | -- -- -- | -- -- -- | -- -- --
| [RedSteel](https://en.wikipedia.org/wiki/Red_Steel) | Red Steel | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS:V](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_Vegas) | Tom Clancy's Rainbow Six: Vegas | - | - | -- -- -- | -- -- -- | -- -- --
| [Horsez](https://en.wikipedia.org/wiki/Horsez) | Horsez | - | - | -- -- -- | -- -- -- | -- -- --
| [SafariPhotoAfrica:WE]() | Safari Photo Africa: Wild Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [BiA:DD](https://en.wikipedia.org/wiki/Brothers_in_Arms:_D-Day) | Brothers in Arms: D-Day | - | - | -- -- -- | -- -- -- | -- -- --
| [StarWars:LA](https://en.wikipedia.org/wiki/Star_Wars:_Lethal_Alliance) | Star Wars: Lethal Alliance | - | - | -- -- -- | -- -- -- | -- -- --
| [FCVengeance](https://en.wikipedia.org/wiki/Far_Cry_Vengeance) | Far Cry Vengeance | - | - | -- -- -- | -- -- -- | -- -- --
| [PippaFunnell:TGSC]() | Pippa Funnell: The Golden Stirrup Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [Scrabble2007Edition](https://en.wikipedia.org/wiki/Scrabble_2007_Edition) | Scrabble 2007 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RockyBalboa](https://en.wikipedia.org/wiki/Rocky_Balboa_(video_game)) | Rocky Balboa | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4](https://en.wikipedia.org/wiki/Resident_Evil_4) | Resident Evil 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGRAW2](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_Advanced_Warfighter_2) | Tom Clancy's Ghost Recon Advanced Warfighter 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TMNT](https://en.wikipedia.org/wiki/TMNT_(video_game)) | TMNT | - | - | -- -- -- | -- -- -- | -- -- --
| [Go:S](https://en.wikipedia.org/wiki/Go!_Sudoku) | Go! Sudoku | - | - | -- -- -- | -- -- -- | -- -- --
| [SH4:WotP](https://en.wikipedia.org/wiki/Silent_Hunter_4:_Wolves_of_the_Pacific) | Silent Hunter 4: Wolves of the Pacific | - | - | -- -- -- | -- -- -- | -- -- --
| [MindQuiz:YBC]() | Mind Quiz: Your Brain Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [BeyondDivinity](https://en.wikipedia.org/wiki/Beyond_Divinity) | Beyond Divinity | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:RS]() | Prince of Persia: Rival Swords | - | - | -- -- -- | -- -- -- | -- -- --
| [TESIV:O](https://en.wikipedia.org/wiki/The_Elder_Scrolls_IV:_Oblivion) | The Elder Scrolls IV: Oblivion | - | - | -- -- -- | -- -- -- | -- -- --
| [Driver76](https://en.wikipedia.org/wiki/Driver_76) | Driver 76 | - | - | -- -- -- | -- -- -- | -- -- --
| [PlatinumSudoku]() | Platinum Sudoku | - | - | -- -- -- | -- -- -- | -- -- --
| [WarTech:SnR](https://en.wikipedia.org/wiki/WarTech:_Senko_no_Ronde) | WarTech: Senko no Ronde | - | - | -- -- -- | -- -- -- | -- -- --
| [SurfsUp](https://en.wikipedia.org/wiki/Surf%27s_Up_(video_game)) | Surf's Up | - | - | -- -- -- | -- -- -- | -- -- --
| [PoPClassic](https://en.wikipedia.org/wiki/Prince_of_Persia_Classic) | Prince of Persia Classic | - | - | -- -- -- | -- -- -- | -- -- --
| [BiADS](https://en.wikipedia.org/wiki/Brothers_in_Arms_DS) | Brothers in Arms DS | - | - | -- -- -- | -- -- -- | -- -- --
| [Driver:PL](https://en.wikipedia.org/wiki/Driver:_Parallel_Lines) | Driver: Parallel Lines | - | - | -- -- -- | -- -- -- | -- -- --
| [TS]() | The Settlers | - | - | -- -- -- | -- -- -- | -- -- --
| [TopTrumpsAdventures]() | Top Trumps Adventures! | - | - | -- -- -- | -- -- -- | -- -- --
| [CosmicFamily](https://en.wikipedia.org/wiki/Cosmic_Family) | Cosmic Family | - | - | -- -- -- | -- -- -- | -- -- --
| [JamSessions](https://en.wikipedia.org/wiki/Jam_Sessions) | Jam Sessions | - | - | -- -- -- | -- -- -- | -- -- --
| [BlazingAngels2:SMoWWII](https://en.wikipedia.org/wiki/Blazing_Angels_2:_Secret_Missions_of_WWII) | Blazing Angels 2: Secret Missions of WWII | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:HE](https://en.wikipedia.org/wiki/CSI:_Hard_Evidence) | CSI: Hard Evidence | - | - | -- -- -- | -- -- -- | -- -- --
| [BrainSpa]() | Brain Spa | - | - | -- -- -- | -- -- -- | -- -- --
| [TS:RoaE](https://en.wikipedia.org/wiki/The_Settlers:_Rise_of_an_Empire) | The Settlers: Rise of an Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [RealFootball20083D]() | Real Football 2008 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TopTrumps:DD]() | Top Trumps: Dogs & Dinosaurs | - | - | -- -- -- | -- -- -- | -- -- --
| [TopTrumps:HP]() | Top Trumps: Horror & Predators | - | - | -- -- -- | -- -- -- | -- -- --
| [HoMnMV:TotE](https://en.wikipedia.org/wiki/The_Settlers:_Rise_of_an_Empire) | Heroes of Might and Magic V: Tribes of the East | - | - | -- -- -- | -- -- -- | -- -- --
| [TotallySpies3:SA]() | Totally Spies! 3: Secret Agents | - | - | -- -- -- | -- -- -- | -- -- --
| [Chessmaster:GE](https://en.wikipedia.org/wiki/Chessmaster) | Chessmaster: Grandmaster Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:AD](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Animal Doctor | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:B](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Babyz | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:FD](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Fashion Designer | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:MC](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Master Chef | - | - | -- -- -- | -- -- -- | -- -- --
| [Naruto:RoaN](https://en.wikipedia.org/wiki/Naruto:_Rise_of_a_Ninja) | Naruto: Rise of a Ninja | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzWildAnimals:D]() | Petz Wild Animals: Dolphinz | - | - | -- -- -- | -- -- -- | -- -- --
| [TellyAddicts]() | Telly Addicts | - | - | -- -- -- | -- -- -- | -- -- --
| [WhoWantstoBeaMillionaire:1E]() | Who Wants to Be a Millionaire: 1st Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:ID](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Interior Designer | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:WD](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Wedding Designer | - | - | -- -- -- | -- -- -- | -- -- --
| [MyFrenchCoach]() | My French Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [MySpanishCoach]() | My Spanish Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [MyWordCoach](https://en.wikipedia.org/wiki/My_Word_Coach) | My Word Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [Beowulf:TG](https://en.wikipedia.org/wiki/Beowulf:_The_Game) | Beowulf: The Game | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:C2]() | Petz: Catz 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:D2]() | Petz: Dogz 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:H2]() | Petz: Horsez 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RYRavingRabbids2](https://en.wikipedia.org/wiki/Rayman_Raving_Rabbids_2) | Rayman Raving Rabbids 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [AC](https://en.wikipedia.org/wiki/Assassin%27s_Creed_(video_game)) | Assassin's Creed | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:Ha2]() | Petz: Hamsterz 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [CraniumKabookii]() | Cranium Kabookii | - | - | -- -- -- | -- -- -- | -- -- --
| [MiamiNights:SitC](https://en.wikipedia.org/wiki/Miami_Nights:_Singles_in_the_City) | Miami Nights: Singles in the City | - | - | -- -- -- | -- -- -- | -- -- --
| [Nitrobike](https://en.wikipedia.org/wiki/Nitrobike) | Nitrobike | - | - | -- -- -- | -- -- -- | -- -- --
| [NoMoreHeroes](https://en.wikipedia.org/wiki/No_More_Heroes_(video_game)) | No More Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [ChessmasterLive](https://en.wikipedia.org/wiki/Chessmaster) | Chessmaster Live | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:AC](https://en.wikipedia.org/wiki/Assassin%27s_Creed:_Alta%C3%AFr%27s_Chronicles) | Assassin's Creed: Altaïr's Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [PuppyPalace]() | Puppy Palace | - | - | -- -- -- | -- -- -- | -- -- --
| [DM:MnME]() | Dark Messiah: Might and Magic - Elements | - | - | -- -- -- | -- -- -- | -- -- --
| [Chessmaster:TAoL](https://en.wikipedia.org/wiki/Chessmaster) | Chessmaster: The Art of Learning | - | - | -- -- -- | -- -- -- | -- -- --
| [Lost:VD](https://en.wikipedia.org/wiki/Lost:_Via_Domus) | Lost: Via Domus | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:WAT](https://en.wikipedia.org/wiki/Tigerz) | Petz: Wild Animals - Tigerz | - | - | -- -- -- | -- -- -- | -- -- --
| [Anno1701:DoD](https://en.wikipedia.org/wiki/Anno_1701:_Dawn_of_Discovery) | Anno 1701: Dawn of Discovery | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:FS](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Figure Skater | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:B]() | Petz: Bunnyz | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRS:V2](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_Vegas_2) | Tom Clancy's Rainbow Six: Vegas 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TheDogIsland](https://en.wikipedia.org/wiki/The_Dog_Island) | The Dog Island | - | - | -- -- -- | -- -- -- | -- -- --
| [Haze](https://en.wikipedia.org/wiki/Haze_(video_game)) | Haze | - | - | -- -- -- | -- -- -- | -- -- --
| [EmergencyHeroes](https://en.wikipedia.org/wiki/Emergency_Heroes) | Emergency Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Protothea](https://en.wikipedia.org/wiki/Prot%C3%B6thea) | Protöthea | - | - | -- -- -- | -- -- -- | -- -- --
| [Stratego:NE]() | Stratego: Next Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:RS](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Rock Star | - | - | -- -- -- | -- -- -- | -- -- --
| [MyWeightLossCoach](https://en.wikipedia.org/wiki/My_Weight_Loss_Coach) | My Weight Loss Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [GourmetChef]() | Gourmet Chef | - | - | -- -- -- | -- -- -- | -- -- --
| [AnimalGenius](https://en.wikipedia.org/wiki/Animal_Genius) | Animal Genius | - | - | -- -- -- | -- -- -- | -- -- --
| [QuickYogaTraining]() | Quick Yoga Training | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:T](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Teacher | - | - | -- -- -- | -- -- -- | -- -- --
| [MyChineseCoach](https://en.wikipedia.org/wiki/My_Chinese_Coach) | My Chinese Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [TS:RoC]() | The Settlers: Rise of Cultures | - | - | -- -- -- | -- -- -- | -- -- --
| [HellsKitchen:TG](https://en.wikipedia.org/wiki/Hell%27s_Kitchen:_The_Game) | Hell's Kitchen: The Game | - | - | -- -- -- | -- -- -- | -- -- --
| [ThePriceIsRight]() | The Price Is Right | - | - | -- -- -- | -- -- -- | -- -- --
| [ArmoredCore:FA](https://en.wikipedia.org/wiki/Armored_Core:_For_Answer) | Armored Core: For Answer | - | - | -- -- -- | -- -- -- | -- -- --
| [BiA:HH](https://en.wikipedia.org/wiki/Brothers_in_Arms:_Hell%27s_Highway) | Brothers in Arms: Hell's Highway | - | - | -- -- -- | -- -- -- | -- -- --
| [MySATCoach](https://en.wikipedia.org/wiki/My_SAT_Coach) | My SAT Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [BiA:DT](https://en.wikipedia.org/wiki/Brothers_in_Arms:_Double_Time) | Brothers in Arms: Double Time | - | - | -- -- -- | -- -- -- | -- -- --
| [MySecretWorldbyImagine]() | My Secret World by Imagine | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:B2](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Babysitters | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:FDNY](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Fashion Designer - New York | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:CR](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Champion Rider | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleofGiants:D](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Battle of Giants: Dinosaurs | - | - | -- -- -- | -- -- -- | -- -- --
| [CesarMillansDogWhisperer]() | Cesar Millan's Dog Whisperer | - | - | -- -- -- | -- -- -- | -- -- --
| [MyJapaneseCoach](https://en.wikipedia.org/wiki/My_Japanese_Coach) | My Japanese Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:NYTG]() | CSI: NY - The Game | - | - | -- -- -- | -- -- -- | -- -- --
| [CircusGames]() | Circus Games | - | - | -- -- -- | -- -- -- | -- -- --
| [Ener-G:DS]() | Ener-G: Dance Squad | - | - | -- -- -- | -- -- -- | -- -- --
| [FC2](https://en.wikipedia.org/wiki/Far_Cry_2) | Far Cry 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [GuitarRockTour](https://en.wikipedia.org/wiki/Guitar_Rock_Tour) | Guitar Rock Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:HC]() | Petz: Horse Club | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzRescue:OP]() | Petz Rescue: Ocean Patrol | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzRescue:WV]() | Petz Rescue: Wildlife Vet | - | - | -- -- -- | -- -- -- | -- -- --
| [RYRavingRabbids:TVP](https://en.wikipedia.org/wiki/Rayman_Raving_Rabbids:_TV_Party) | Rayman Raving Rabbids: TV Party | - | - | -- -- -- | -- -- -- | -- -- --
| [FamilyFestPresentsMovieGames]() | Family Fest Presents Movie Games | - | - | -- -- -- | -- -- -- | -- -- --
| [TCE](https://en.wikipedia.org/wiki/Tom_Clancy%27s_EndWar) | Tom Clancy's EndWar | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:PB]() | Imagine: Party Babyz | - | - | -- -- -- | -- -- -- | -- -- --
| [SWSnowboarding](https://en.wikipedia.org/wiki/Shaun_White_Snowboarding) | Shaun White Snowboarding | - | - | -- -- -- | -- -- -- | -- -- --
| [SWSnowboarding:RT]() | Shaun White Snowboarding: Road Trip | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:BS](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Ballet Star | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:MS](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Movie Star | - | - | -- -- -- | -- -- -- | -- -- --
| [Naruto:TBB](https://en.wikipedia.org/wiki/Naruto:_The_Broken_Bond) | Naruto: The Broken Bond | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:CM](https://en.wikipedia.org/wiki/Petz:_Crazy_Monkeyz) | Petz: Crazy Monkeyz | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:MH]() | Petz: Monkeyz House | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:CC]() | Petz: Catz Clan | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzRescue:EP]() | Petz Rescue: Endangered Paradise | - | - | -- -- -- | -- -- -- | -- -- --
| [WWtBaM:2E]() | Who Wants to Be a Millionaire: 2nd Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [MyStopSmokingCoach:ACE]() | My Stop Smoking Coach: Allen Carr's EasyWay | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzSports]() | Petz Sports | - | - | -- -- -- | -- -- -- | -- -- --
| [HappyCooking]() | Happy Cooking | - | - | -- -- -- | -- -- -- | -- -- --
| [MyFunFactsCoach]() | My Fun Facts Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP](https://en.wikipedia.org/wiki/Prince_of_Persia_(2008_video_game)) | Prince of Persia | - | - | -- -- -- | -- -- -- | -- -- --
| [BabyLife](https://en.wikipedia.org/wiki/Baby_Life) | Baby Life | - | - | -- -- -- | -- -- -- | -- -- --
| [MyFitnessCoach]() | My Fitness Coach | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:FP](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Fashion Party | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:PP](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Party Planner | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:C](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Cheerleader | - | - | -- -- -- | -- -- -- | -- -- --
| [JojosFashionShow:DiaD]() | Jojo's Fashion Show: Design in a Dash! | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:J](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Journalist | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:HR]() | Petz: Horseshoe Ranch | - | - | -- -- -- | -- -- -- | -- -- --
| [Tenchu:SA](https://en.wikipedia.org/wiki/Tenchu:_Shadow_Assassins) | Tenchu: Shadow Assassins | - | - | -- -- -- | -- -- -- | -- -- --
| [JakePower:F]() | Jake Power: Fireman | - | - | -- -- -- | -- -- -- | -- -- --
| [JakePower:P]() | Jake Power: Policeman | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:IC](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Ice Champions | - | - | -- -- -- | -- -- -- | -- -- --
| [SixFlagsFunPark]() | Six Flags Fun Park | - | - | -- -- -- | -- -- -- | -- -- --
| [TCHAWX](https://en.wikipedia.org/wiki/Tom_Clancy%27s_H.A.W.X) | Tom Clancy's H.A.W.X | - | - | -- -- -- | -- -- -- | -- -- --
| [GreysAnatomy:TVG](https://en.wikipedia.org/wiki/Grey%27s_Anatomy:_The_Video_Game) | Grey's Anatomy: The Video Game | - | - | -- -- -- | -- -- -- | -- -- --
| [WiC:SA](https://en.wikipedia.org/wiki/World_in_Conflict:_Soviet_Assault) | World in Conflict: Soviet Assault | - | - | -- -- -- | -- -- -- | -- -- --
| [VacationSports]() | Vacation Sports | - | - | -- -- -- | -- -- -- | -- -- --
| [BrokenSword:SotTTDC](https://en.wikipedia.org/wiki/Broken_Sword:_Shadow_of_the_Templars_%E2%80%93_The_Director%27s_Cut) | Broken Sword: Shadow of the Templars - The Director's Cut | - | - | -- -- -- | -- -- -- | -- -- --
| [Wheelman](https://en.wikipedia.org/wiki/Wheelman_(video_game)) | Wheelman | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:MR](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: My Restaurant | - | - | -- -- -- | -- -- -- | -- -- --
| [FSPC]() | Fashion Studio Paris Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:FD2](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Family Doctor | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:MF](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Music Fest | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:MA](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Makeup Artist | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:BO](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Boutique Owner | - | - | -- -- -- | -- -- -- | -- -- --
| [CellFactor:PW](https://en.wikipedia.org/wiki/CellFactor:_Psychokinetic_Wars) | CellFactor: Psychokinetic Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzFashion:DaC]() | Petz Fashion: Dogz and Catz | - | - | -- -- -- | -- -- -- | -- -- --
| [Anno1404](https://en.wikipedia.org/wiki/Anno_1404) | Anno 1404 | - | - | -- -- -- | -- -- -- | -- -- --
| [COJ:BiB](https://en.wikipedia.org/wiki/Call_of_Juarez:_Bound_in_Blood) | Call of Juarez: Bound in Blood | - | - | -- -- -- | -- -- -- | -- -- --
| [TMNT:TiTRS](https://en.wikipedia.org/wiki/Teenage_Mutant_Ninja_Turtles:_Turtles_in_Time_Re-Shelled) | Teenage Mutant Ninja Turtles: Turtles in Time Re-Shelled | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:SC](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Soccer Captain | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:TCT](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Teacher - Class Trip | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:D](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Detective | - | - | -- -- -- | -- -- -- | -- -- --
| [AcademyofChampions:S](https://en.wikipedia.org/wiki/Academy_of_Champions:_Soccer) | Academy of Champions: Soccer | - | - | -- -- -- | -- -- -- | -- -- --
| [CwaCoM](https://en.wikipedia.org/wiki/Cloudy_with_a_Chance_of_Meatballs_(video_game)) | Cloudy with a Chance of Meatballs | - | - | -- -- -- | -- -- -- | -- -- --
| [HeroesOverEurope](https://en.wikipedia.org/wiki/Heroes_Over_Europe) | Heroes Over Europe | - | - | -- -- -- | -- -- -- | -- -- --
| [NewU:FFPT]() | NewU: Fitness First Personal Trainer | - | - | -- -- -- | -- -- -- | -- -- --
| [TMNT:SU](https://en.wikipedia.org/wiki/Teenage_Mutant_Ninja_Turtles:_Smash-Up) | Teenage Mutant Ninja Turtles: Smash-Up | - | - | -- -- -- | -- -- -- | -- -- --
| [ThePriceisRight:2010E]() | The Price is Right: 2010 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [WheresWaldo:TFJ](https://en.wikipedia.org/wiki/Where%27s_Waldo%3F_The_Fantastic_Journey_(video_game)) | Where's Waldo? The Fantastic Journey | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleofGiants:D2]() | Battle of Giants: Dragons | - | - | -- -- -- | -- -- -- | -- -- --
| [FamilyFeud:2010E]() | Family Feud: 2010 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:SS](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Salon Stylist | - | - | -- -- -- | -- -- -- | -- -- --
| [MetropolisCrimes]() | Metropolis Crimes | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:Z](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Zookeeper | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:S16](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Sweet 16 | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:PBP]() | Petz: Pony Beauty Pageant | - | - | -- -- -- | -- -- -- | -- -- --
| [CoverGirl]() | Cover Girl | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:FDWT](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Fashion Designer - World Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzDolphinzEncounter]() | Petz Dolphinz Encounter | - | - | -- -- -- | -- -- -- | -- -- --
| [JamSessions2]() | Jam Sessions 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PanzerGeneral:AA](https://en.wikipedia.org/wiki/Panzer_General:_Allied_Assault) | Panzer General: Allied Assault | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:DI](https://en.wikipedia.org/wiki/CSI:_Deadly_Intent) | CSI: Deadly Intent | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:A](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Artist | - | - | -- -- -- | -- -- -- | -- -- --
| [Monster4x4StuntRacer]() | Monster 4x4 Stunt Racer | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:DF]() | Petz: Dogz Family | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsGoHome:ACA]() | Rabbids Go Home: A Comedy Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleofGiants:DBE]() | Battle of Giants: Dragons - Bronze Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [COP:TR](https://en.wikipedia.org/wiki/C.O.P._The_Recruit) | C.O.P.: The Recruit | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:BF](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Babyz Fashion | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsGoHome](https://en.wikipedia.org/wiki/Rabbids_Go_Home) | Rabbids Go Home | - | - | -- -- -- | -- -- -- | -- -- --
| [SWSnowboarding:WS](https://en.wikipedia.org/wiki/Shaun_White_Snowboarding:_World_Stage) | Shaun White Snowboarding: World Stage | - | - | -- -- -- | -- -- -- | -- -- --
| [KnockoutParty]() | Knockout Party | - | - | -- -- -- | -- -- -- | -- -- --
| [Fairyland:MM]() | Fairyland: Melody Magic | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzNursery]() | Petz Nursery | - | - | -- -- -- | -- -- -- | -- -- --
| [StyleLab:JD]() | Style Lab: Jewelry Design | - | - | -- -- -- | -- -- -- | -- -- --
| [StyleLab:M]() | Style Lab: Makeover | - | - | -- -- -- | -- -- -- | -- -- --
| [ACII](https://en.wikipedia.org/wiki/Assassin%27s_Creed_II) | Assassin's Creed II | - | - | -- -- -- | -- -- -- | -- -- --
| [ACII:D](https://en.wikipedia.org/wiki/Assassin%27s_Creed_II:_Discovery) | Assassin's Creed II: Discovery | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance](https://en.wikipedia.org/wiki/Just_Dance_(video_game)) | Just Dance | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:DTS]() | Petz: Dogz Talent Show | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:B](https://en.wikipedia.org/wiki/Assassin%27s_Creed:_Bloodlines) | Assassin's Creed: Bloodlines | - | - | -- -- -- | -- -- -- | -- -- --
| [CookWars]() | Cook Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:HS]() | Petz: Hamsterz Superstarz | - | - | -- -- -- | -- -- -- | -- -- --
| [YourShape](https://en.wikipedia.org/wiki/Your_Shape) | Your Shape | - | - | -- -- -- | -- -- -- | -- -- --
| [TMNT:AA]() | Teenage Mutant Ninja Turtles: Arcade Attack | - | - | -- -- -- | -- -- -- | -- -- --
| [JamesCameronsAvatar:TG](https://en.wikipedia.org/wiki/James_Cameron%27s_Avatar:_The_Game) | James Cameron's Avatar: The Game | - | - | -- -- -- | -- -- -- | -- -- --
| [MnM:CoH](https://en.wikipedia.org/wiki/Might_%26_Magic:_Clash_of_Heroes) | Might & Magic: Clash of Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [RY](https://en.wikipedia.org/wiki/Rayman_(video_game)) | Rayman | - | - | -- -- -- | -- -- -- | -- -- --
| [SleepoverParty]() | Sleepover Party | - | - | -- -- -- | -- -- -- | -- -- --
| [NoMoreHeroes2:DS](https://en.wikipedia.org/wiki/No_More_Heroes_2:_Desperate_Struggle) | No More Heroes 2: Desperate Struggle | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleofGiants:MI]() | Battle of Giants: Mutant Insects | - | - | -- -- -- | -- -- -- | -- -- --
| [SH5:BotA](https://en.wikipedia.org/wiki/Silent_Hunter_5:_Battle_of_the_Atlantic) | Silent Hunter 5: Battle of the Atlantic | - | - | -- -- -- | -- -- -- | -- -- --
| [RacquetSports](https://en.wikipedia.org/wiki/Racket_Sports_Party) | Racquet Sports | - | - | -- -- -- | -- -- -- | -- -- --
| [BootCampAcademy]() | Boot Camp Academy | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleofGiants:DFfS]() | Battle of Giants: Dinosaurs - Fight for Survival | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:G](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Gymnast | - | - | -- -- -- | -- -- -- | -- -- --
| [RedSteel2](https://en.wikipedia.org/wiki/Red_Steel_2) | Red Steel 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TS7:PtaK](https://en.wikipedia.org/wiki/The_Settlers_7:_Paths_to_a_Kingdom) | The Settlers 7: Paths to a Kingdom | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:C](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Splinter_Cell:_Conviction) | Tom Clancy's Splinter Cell: Conviction | - | - | -- -- -- | -- -- -- | -- -- --
| [CastleCo]() | Castle & Co | - | - | -- -- -- | -- -- -- | -- -- --
| [OK:PS]() | OK! Puzzle Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TFS](https://en.wikipedia.org/wiki/Prince_of_Persia:_The_Forgotten_Sands) | Prince of Persia: The Forgotten Sands | - | - | -- -- -- | -- -- -- | -- -- --
| [VoodooDice](https://en.wikipedia.org/wiki/Voodoo_Dice) | Voodoo Dice | - | - | -- -- -- | -- -- -- | -- -- --
| [DanceonBroadway](https://en.wikipedia.org/wiki/Dance_on_Broadway) | Dance on Broadway | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleofGiants:MIR]() | Battle of Giants: Mutant Insects - Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzHamsterzFamily]() | Petz Hamsterz Family | - | - | -- -- -- | -- -- -- | -- -- --
| [GalaxyRacers]() | Galaxy Racers | - | - | -- -- -- | -- -- -- | -- -- --
| [SCVTW:TG](https://en.wikipedia.org/wiki/Scott_Pilgrim_vs._the_World:_The_Game) | Scott Pilgrim vs. the World: The Game | - | - | -- -- -- | -- -- -- | -- -- --
| [GoldsGymDanceWorkout]() | Gold's Gym Dance Workout | - | - | -- -- -- | -- -- -- | -- -- --
| [TCHAWX2](https://en.wikipedia.org/wiki/Tom_Clancy%27s_H.A.W.X_2) | Tom Clancy's H.A.W.X 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RUSE](https://en.wikipedia.org/wiki/R.U.S.E.) | R.U.S.E. | - | - | -- -- -- | -- -- -- | -- -- --
| [AATROM]() | Arthur and the Revenge of Maltazard | - | - | -- -- -- | -- -- -- | -- -- --
| [SWSkateboarding](https://en.wikipedia.org/wiki/Shaun_White_Skateboarding) | Shaun White Skateboarding | - | - | -- -- -- | -- -- -- | -- -- --
| [TheHollywoodSquares]() | The Hollywood Squares | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2](https://en.wikipedia.org/wiki/Just_Dance_2) | Just Dance 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzlerWorld2011]() | Puzzler World 2011 | - | - | -- -- -- | -- -- -- | -- -- --
| [TSOnline](https://en.wikipedia.org/wiki/The_Settlers_Online) | The Settlers Online | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:FC](https://en.wikipedia.org/wiki/CSI:_Fatal_Conspiracy) | CSI: Fatal Conspiracy | - | - | -- -- -- | -- -- -- | -- -- --
| [BloodyGoodTime](https://en.wikipedia.org/wiki/Bloody_Good_Time) | Bloody Good Time | - | - | -- -- -- | -- -- -- | -- -- --
| [FightersUncaged](https://en.wikipedia.org/wiki/Fighters_Uncaged) | Fighters Uncaged | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDanceKids](https://en.wikipedia.org/wiki/Just_Dance_Kids_(2010_video_game)) | Just Dance Kids | - | - | -- -- -- | -- -- -- | -- -- --
| [FamilyFeud:D]() | Family Feud: Decades | - | - | -- -- -- | -- -- -- | -- -- --
| [MotionSports](https://en.wikipedia.org/wiki/MotionSports) | MotionSports | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:B2](https://en.wikipedia.org/wiki/Assassin%27s_Creed:_Brotherhood) | Assassin's Creed: Brotherhood | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_(2010_video_game)) | Tom Clancy's Ghost Recon | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGRPredator](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_Predator) | Tom Clancy's Ghost Recon Predator | - | - | -- -- -- | -- -- -- | -- -- --
| [RavingRabbids:TiT](https://en.wikipedia.org/wiki/Raving_Rabbids:_Travel_in_Time) | Raving Rabbids: Travel in Time | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:U](https://en.wikipedia.org/wiki/CSI:_Unsolved) | CSI: Unsolved! | - | - | -- -- -- | -- -- -- | -- -- --
| [MichaelJackson:TE](https://en.wikipedia.org/wiki/Michael_Jackson:_The_Experience) | Michael Jackson: The Experience | - | - | -- -- -- | -- -- -- | -- -- --
| [SportsCollection:15StM]() | Sports Collection: 15 Sports to Master | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TSoTHD]() | Prince of Persia: The Sands of Time HD | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TTTHD]() | Prince of Persia: The Two Thrones HD | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:WWHD]() | Prince of Persia: Warrior Within HD | - | - | -- -- -- | -- -- -- | -- -- --
| [Petz:CF]() | Petz: Catz Family | - | - | -- -- -- | -- -- -- | -- -- --
| [Zeit](https://en.wikipedia.org/wiki/Zeit%C2%B2) | Zeit | - | - | -- -- -- | -- -- -- | -- -- --
| [BeyondGoodEvilHD]() | Beyond Good & Evil HD | - | - | -- -- -- | -- -- -- | -- -- --
| [The1000000Pyramid]() | The $1,000,000 Pyramid | - | - | -- -- -- | -- -- -- | -- -- --
| [WeDare](https://en.wikipedia.org/wiki/We_Dare) | We Dare | - | - | -- -- -- | -- -- -- | -- -- --
| [FitinSix]() | Fit in Six | - | - | -- -- -- | -- -- -- | -- -- --
| [RY3D]() | Rayman 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [Asphalt6:Adrenaline](https://en.wikipedia.org/wiki/Asphalt_6:_Adrenaline) | Asphalt 6: Adrenaline | - | - | -- -- -- | -- -- -- | -- -- --
| [CombatofGiants:D3D](https://en.wikipedia.org/wiki/Combat_of_Giants:_Dinosaurs_3D) | Combat of Giants: Dinosaurs 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR:SW](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon:_Shadow_Wars) | Tom Clancy's Ghost Recon: Shadow Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [IL2S:CoD](https://en.wikipedia.org/wiki/IL-2_Sturmovik:_Cliffs_of_Dover) | IL-2 Sturmovik: Cliffs of Dover | - | - | -- -- -- | -- -- -- | -- -- --
| [Rabbids:TiT3D]() | Rabbids: Travel in Time 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC3D]() | Tom Clancy's Splinter Cell 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [ChildofEden](https://en.wikipedia.org/wiki/Child_of_Eden) | Child of Eden | - | - | -- -- -- | -- -- -- | -- -- --
| [CubicNinja](https://en.wikipedia.org/wiki/Cubic_Ninja) | Cubic Ninja | - | - | -- -- -- | -- -- -- | -- -- --
| [Outland](https://en.wikipedia.org/wiki/Outland_(video_game)) | Outland | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzFantasy3D]() | Petz Fantasy 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [CoJ:TC](https://en.wikipedia.org/wiki/Call_of_Juarez:_The_Cartel) | Call of Juarez: The Cartel | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance:SP](https://en.wikipedia.org/wiki/Just_Dance:_Summer_Party) | Just Dance: Summer Party | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSmurfs]() | The Smurfs | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSmurfsDanceParty](https://en.wikipedia.org/wiki/The_Smurfs_Dance_Party) | The Smurfs Dance Party | - | - | -- -- -- | -- -- -- | -- -- --
| [FromDust](https://en.wikipedia.org/wiki/From_Dust) | From Dust | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSmurfsCo]() | The Smurfs & Co | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSCClassicTrilogyHD]() | Tom Clancy's Splinter Cell Classic Trilogy HD | - | - | -- -- -- | -- -- -- | -- -- --
| [Driver:R](https://en.wikipedia.org/wiki/Driver:_Renegade) | Driver: Renegade | - | - | -- -- -- | -- -- -- | -- -- --
| [Driver:SF](https://en.wikipedia.org/wiki/Driver:_San_Francisco) | Driver: San Francisco | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzlerMindGym3D]() | Puzzler Mind Gym 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TrackMania2:Canyon]() | TrackMania 2: Canyon | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleofGiants:DS]() | Battle of Giants: Dinosaurs Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [MnMHeroesVI](https://en.wikipedia.org/wiki/Might_%26_Magic_Heroes_VI) | Might & Magic Heroes VI | - | - | -- -- -- | -- -- -- | -- -- --
| [FamilyFeud:2012Edition]() | Family Feud: 2012 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerUpHeroes]() | PowerUp Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [ThePriceisRight:D]() | The Price is Right: Decades | - | - | -- -- -- | -- -- -- | -- -- --
| [TheAdventuresofTintin:TSotU](https://en.wikipedia.org/wiki/The_Adventures_of_Tintin:_The_Secret_of_the_Unicorn_(video_game)) | The Adventures of Tintin: The Secret of the Unicorn | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDanceKids2](https://en.wikipedia.org/wiki/Just_Dance_Kids_2) | Just Dance Kids 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [ZooMania3D]() | Zoo Mania 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [JNHC](https://en.wikipedia.org/wiki/James_Noir%27s_Hollywood_Crimes) | James Noir's Hollywood Crimes | - | - | -- -- -- | -- -- -- | -- -- --
| [MotionSportsAdrenaline]() | MotionSports Adrenaline | - | - | -- -- -- | -- -- -- | -- -- --
| [NCIS](https://en.wikipedia.org/wiki/NCIS_(video_game)) | NCIS | - | - | -- -- -- | -- -- -- | -- -- --
| [RavingRabbids:AK](https://en.wikipedia.org/wiki/Raving_Rabbids:_Alive_%26_Kicking) | Raving Rabbids: Alive & Kicking | - | - | -- -- -- | -- -- -- | -- -- --
| [MichaelJackson:TE3D]() | Michael Jackson: The Experience 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [Puppies3D]() | Puppies 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TheBlackEyedPeasExperience](https://en.wikipedia.org/wiki/The_Black_Eyed_Peas_Experience) | The Black Eyed Peas Experience | - | - | -- -- -- | -- -- -- | -- -- --
| [SelfDefenseTrainingCamp](https://en.wikipedia.org/wiki/Self-Defense_Training_Camp) | Self-Defense Training Camp | - | - | -- -- -- | -- -- -- | -- -- --
| [YourShape:FE2012]() | Your Shape: Fitness Evolved 2012 | - | - | -- -- -- | -- -- -- | -- -- --
| [ABBA:YCD](https://en.wikipedia.org/wiki/ABBA:_You_Can_Dance) | ABBA: You Can Dance | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:R](https://en.wikipedia.org/wiki/Assassin%27s_Creed:_Revelations) | Assassin's Creed: Revelations | - | - | -- -- -- | -- -- -- | -- -- --
| [RYOrigins](https://en.wikipedia.org/wiki/Rayman_Origins) | Rayman Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [Anno2070](https://en.wikipedia.org/wiki/Anno_2070) | Anno 2070 | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance3](https://en.wikipedia.org/wiki/Just_Dance_3) | Just Dance 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzlerWorld2012]() | Puzzler World 2012 | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSCHD]() | Tom Clancy's Splinter Cell HD | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:CTHD]() | Tom Clancy's Splinter Cell: Chaos Theory HD | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:PTHD]() | Tom Clancy's Splinter Cell: Pandora Tomorrow HD | - | - | -- -- -- | -- -- -- | -- -- --
| [Asphalt:I](https://en.wikipedia.org/wiki/Asphalt:_Injection) | Asphalt: Injection | - | - | -- -- -- | -- -- -- | -- -- --
| [DungeonHunter:A](https://en.wikipedia.org/wiki/Dungeon_Hunter:_Alliance) | Dungeon Hunter: Alliance | - | - | -- -- -- | -- -- -- | -- -- --
| [Lumines:ES](https://en.wikipedia.org/wiki/Lumines:_Electronic_Symphony) | Lumines: Electronic Symphony | - | - | -- -- -- | -- -- -- | -- -- --
| [IAmAlive](https://en.wikipedia.org/wiki/I_Am_Alive) | I Am Alive | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:R2]() | Assassin's Creed: Recollection | - | - | -- -- -- | -- -- -- | -- -- --
| [ShootManyRobots](https://en.wikipedia.org/wiki/Shoot_Many_Robots) | Shoot Many Robots | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoHeroz](https://en.wikipedia.org/wiki/MotoHeroz) | MotoHeroz | - | - | -- -- -- | -- -- -- | -- -- --
| [RY3HD]() | Rayman 3 HD | - | - | -- -- -- | -- -- -- | -- -- --
| [FunkyBarn3D]() | Funky Barn 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [Horses3D]() | Horses 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance:BO](https://en.wikipedia.org/wiki/Just_Dance:_Best_Of) | Just Dance: Best Of | - | - | -- -- -- | -- -- -- | -- -- --
| [Trials Evolution](https://en.wikipedia.org/wiki/Trials_Evolution) | Trials Evolution | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGR:FS](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon:_Future_Soldier) | Tom Clancy's Ghost Recon: Future Soldier | - | - | -- -- -- | -- -- -- | -- -- --
| [MadRiders](https://en.wikipedia.org/wiki/Mad_Riders) | Mad Riders | - | - | -- -- -- | -- -- -- | -- -- --
| [BabelRising](https://en.wikipedia.org/wiki/Babel_Rising) | Babel Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [BabelRising3D](https://en.wikipedia.org/wiki/Babel_Rising) | Babel Rising 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance:GH](https://en.wikipedia.org/wiki/Just_Dance:_Best_Of) | Just Dance: Greatest Hits | - | - | -- -- -- | -- -- -- | -- -- --
| [TheExpendables2:V]() | The Expendables 2: Videogame | - | - | -- -- -- | -- -- -- | -- -- --
| [MnM:DoC]() | Might & Magic: Duel of Champions | - | - | -- -- -- | -- -- -- | -- -- --
| [NCIS3D]() | NCIS 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [RYJungleRun]() | Rayman Jungle Run | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance4](https://en.wikipedia.org/wiki/Just_Dance_4) | Just Dance 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [Monster4x43D]() | Monster 4x4 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith](https://en.wikipedia.org/wiki/Rocksmith) | Rocksmith | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance:DP](https://en.wikipedia.org/wiki/Just_Dance:_Disney_Party) | Just Dance: Disney Party | - | - | -- -- -- | -- -- -- | -- -- --
| [Imagine:FL](https://en.wikipedia.org/wiki/Imagine_(video_game_series)) | Imagine: Fashion Life | - | - | -- -- -- | -- -- -- | -- -- --
| [ACIII](https://en.wikipedia.org/wiki/Assassin%27s_Creed_III) | Assassin's Creed III | - | - | -- -- -- | -- -- -- | -- -- --
| [ACIII:L](https://en.wikipedia.org/wiki/Assassin%27s_Creed_III:_Liberation) | Assassin's Creed III: Liberation | - | - | -- -- -- | -- -- -- | -- -- --
| [TheAvengers:BfE](https://en.wikipedia.org/wiki/Marvel_Avengers:_Battle_for_Earth) | The Avengers: Battle for Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [NuttyFluffiesRollercoaster](https://en.wikipedia.org/wiki/Nutty_Fluffies_Rollercoaster) | Nutty Fluffies Rollercoaster | - | - | -- -- -- | -- -- -- | -- -- --
| [PoptropicaAdventures]() | Poptropica Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsRumble](https://en.wikipedia.org/wiki/Rabbids_Rumble) | Rabbids Rumble | - | - | -- -- -- | -- -- -- | -- -- --
| [TheHipHopDanceExperience](https://en.wikipedia.org/wiki/The_Hip_Hop_Dance_Experience) | The Hip Hop Dance Experience | - | - | -- -- -- | -- -- -- | -- -- --
| [ESPNSportsConnection]() | ESPN Sports Connection | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsLand](https://en.wikipedia.org/wiki/Rabbids_Land) | Rabbids Land | - | - | -- -- -- | -- -- -- | -- -- --
| [YourShape:FE2013]() | Your Shape: Fitness Evolved 2013 | - | - | -- -- -- | -- -- -- | -- -- --
| [ZombiU](https://en.wikipedia.org/wiki/ZombiU) | Zombi(U) | - | - | -- -- -- | -- -- -- | -- -- --
| [FC3](https://en.wikipedia.org/wiki/Far_Cry_3) | Far Cry 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [TrackMania2:S]() | TrackMania 2: Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [TrialsEvolution:GE]() | Trials Evolution: Gold Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ShootManiaStorm](https://en.wikipedia.org/wiki/ShootMania_Storm) | ShootMania Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [FC3:BD](https://en.wikipedia.org/wiki/Far_Cry_3:_Blood_Dragon) | Far Cry 3: Blood Dragon | - | - | -- -- -- | -- -- -- | -- -- --
| [CoJ:G](https://en.wikipedia.org/wiki/Call_of_Juarez:_Gunslinger) | Call of Juarez: Gunslinger | - | - | -- -- -- | -- -- -- | -- -- --
| [SC:BSB]() | Splinter Cell: Blacklist - Spider-Bot | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSmurfs2]() | The Smurfs 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Spartacus Legends](https://en.wikipedia.org/wiki/Spartacus_Legends) | Spartacus Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSmurfsCo:S]() | The Smurfs & Co: Spellbound | - | - | -- -- -- | -- -- -- | -- -- --
| [TrackMania2:V]() | TrackMania 2: Valley | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TSatF]() | Prince of Persia: The Shadow and the Flame | - | - | -- -- -- | -- -- -- | -- -- --
| [CloudberryKingdom](https://en.wikipedia.org/wiki/Cloudberry_Kingdom) | Cloudberry Kingdom | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:B](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Splinter_Cell:_Blacklist) | Tom Clancy's Splinter Cell: Blacklist | - | - | -- -- -- | -- -- -- | -- -- --
| [RYegends](https://en.wikipedia.org/wiki/Rayman_Legends) | Rayman Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [PanzerGeneralOnline]() | Panzer General Online | - | - | -- -- -- | -- -- -- | -- -- --
| [AnnoOnline](https://en.wikipedia.org/wiki/Anno_Online) | Anno Online | - | - | -- -- -- | -- -- -- | -- -- --
| [Flashback](https://en.wikipedia.org/wiki/Flashback_(2013_video_game)) | Flashback | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2014](https://en.wikipedia.org/wiki/Just_Dance_2014) | Just Dance 2014 | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsBigBang](https://en.wikipedia.org/wiki/Rabbids_Big_Bang) | Rabbids Big Bang | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDanceKids2014](https://en.wikipedia.org/wiki/Just_Dance_Kids_2014) | Just Dance Kids 2014 | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith:A2014E]() | Rocksmith: All-new 2014 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ACIV:BF](https://en.wikipedia.org/wiki/Assassin%27s_Creed_IV:_Black_Flag) | Assassin's Creed IV: Black Flag | - | - | -- -- -- | -- -- -- | -- -- --
| [RYFiestaRun](https://en.wikipedia.org/wiki/Rayman_Fiesta_Run) | Rayman Fiesta Run | - | - | -- -- -- | -- -- -- | -- -- --
| [FighterWithin](https://en.wikipedia.org/wiki/Fighter_Within) | Fighter Within | - | - | -- -- -- | -- -- -- | -- -- --
| [ACPirates]() | Assassin's Creed Pirates | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:LHD]() | Assassin's Creed: Liberation HD | - | - | -- -- -- | -- -- -- | -- -- --
| [MnMX:L](https://en.wikipedia.org/wiki/Might_%26_Magic_X:_Legacy) | Might & Magic X: Legacy | - | - | -- -- -- | -- -- -- | -- -- --
| [FCClassic]() | Far Cry Classic | - | - | -- -- -- | -- -- -- | -- -- --
| [SouthPark:TSoT](https://en.wikipedia.org/wiki/South_Park:_The_Stick_of_Truth) | South Park: The Stick of Truth | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGRPhantoms](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_Phantoms) | Tom Clancy's Ghost Recon Phantoms | - | - | -- -- -- | -- -- -- | -- -- --
| [TrialsFrontier](https://en.wikipedia.org/wiki/Trials_Frontier) | TrialsFrontier | - | - | -- -- -- | -- -- -- | -- -- --
| [TrialsFusion](https://en.wikipedia.org/wiki/Trials_Fusion) | TrialsFusion | - | - | -- -- -- | -- -- -- | -- -- --
| [ChildofLight](https://en.wikipedia.org/wiki/Child_of_Light) | Child of Light | - | - | -- -- -- | -- -- -- | -- -- --
| [CSI:HC]() | CSI: Hidden Crimes | - | - | -- -- -- | -- -- -- | -- -- --
| [WatchDogs](https://en.wikipedia.org/wiki/Watch_Dogs_(video_game)) | Watch Dogs | - | - | -- -- -- | -- -- -- | -- -- --
| [ValiantHearts:TGW](https://en.wikipedia.org/wiki/Valiant_Hearts:_The_Great_War) | Valiant Hearts: The Great War | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:M]() | Assassin's Creed: Memories | - | - | -- -- -- | -- -- -- | -- -- --
| [LittleRaiders:RR]() | Little Raiders: Robin's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDanceNow](https://en.wikipedia.org/wiki/Just_Dance_Now) | Just Dance Now | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzBeach]() | Petz Beach | - | - | -- -- -- | -- -- -- | -- -- --
| [PetzCountryside]() | Petz Countryside | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2015](https://en.wikipedia.org/wiki/Just_Dance_2015) | Just Dance 2015 | - | - | -- -- -- | -- -- -- | -- -- --
| [ShapeUp:BR]() | Shape Up: Battle Run | - | - | -- -- -- | -- -- -- | -- -- --
| [ACRogue](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Rogue) | Assassin's Creed Rogue | - | - | -- -- -- | -- -- -- | -- -- --
| [ACUnity](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Unity) | Assassin's Creed Unity | - | - | -- -- -- | -- -- -- | -- -- --
| [ShapeUp](https://en.wikipedia.org/wiki/Shape_Up_(video_game)) | Shape Up | - | - | -- -- -- | -- -- -- | -- -- --
| [TetrisUltimate](https://en.wikipedia.org/wiki/Tetris_Ultimate) | Tetris Ultimate | - | - | -- -- -- | -- -- -- | -- -- --
| [FC4](https://en.wikipedia.org/wiki/Far_Cry_4) | Far Cry 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [FC4:AP]() | Far Cry 4: Arcade Poker | - | - | -- -- -- | -- -- -- | -- -- --
| [FC4:AM]() | Far Cry 4: Arena Master | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsInvasion:TITVS]() | Rabbids Invasion: The Interactive TV Show | - | - | -- -- -- | -- -- -- | -- -- --
| [Anno:BaE]() | Anno: Build an Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [MonopolyDeal]() | Monopoly Deal | - | - | -- -- -- | -- -- -- | -- -- --
| [MonopolyPlus]() | Monopoly Plus | - | - | -- -- -- | -- -- -- | -- -- --
| [TheCrew](https://en.wikipedia.org/wiki/The_Crew_(video_game)) | The Crew | - | - | -- -- -- | -- -- -- | -- -- --
| [HorseHaven:WA]() | Horse Haven: World Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [Risk]() | Risk | - | - | -- -- -- | -- -- -- | -- -- --
| [GrowHome](https://en.wikipedia.org/wiki/Grow_Home) | Grow Home | - | - | -- -- -- | -- -- -- | -- -- --
| [TMQfEL](https://en.wikipedia.org/wiki/The_Mighty_Quest_for_Epic_Loot) | The Mighty Quest for Epic Loot | - | - | -- -- -- | -- -- -- | -- -- --
| [MonkeyKingEscape]() | Monkey King Escape | - | - | -- -- -- | -- -- -- | -- -- --
| [TrivialPursuitLive]() | Trivial Pursuit Live! | - | - | -- -- -- | -- -- -- | -- -- --
| [Driver:SP]() | Driver: Speedboat Paradise | - | - | -- -- -- | -- -- -- | -- -- --
| [ACChronicles:C]() | Assassin's Creed Chronicles: China | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsAppisodes]() | Rabbids Appisodes | - | - | -- -- -- | -- -- -- | -- -- --
| [Scrabble]() | Scrabble | - | - | -- -- -- | -- -- -- | -- -- --
| [Boggle]() | Boggle | - | - | -- -- -- | -- -- -- | -- -- --
| [ToySoldiers:WC](https://en.wikipedia.org/wiki/Toy_Soldiers:_War_Chest) | Toy Soldiers: War Chest | - | - | -- -- -- | -- -- -- | -- -- --
| [MnMHeroesVII](https://en.wikipedia.org/wiki/Might_%26_Magic_Heroes_VII) | Might & Magic Heroes VII | - | - | -- -- -- | -- -- -- | -- -- --
| [Care Bears: Belly Match]() | Care Bears: Belly Match | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSmurfs3D]() | The Smurfs 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [GravityFalls:LotGG](https://en.wikipedia.org/wiki/Gravity_Falls:_Legend_of_the_Gnome_Gemulets) | Gravity Falls: Legend of the Gnome Gemulets | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2016](https://en.wikipedia.org/wiki/Just_Dance_2016) | Just Dance 2016 | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance:DP2](https://en.wikipedia.org/wiki/Just_Dance:_Disney_Party_2) | Just Dance: Disney Party 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [ACSyndicate](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Syndicate) | Assassin's Creed Syndicate | - | - | -- -- -- | -- -- -- | -- -- --
| [Anno2205:AM]() | Anno 2205: Asteroid Miner | - | - | -- -- -- | -- -- -- | -- -- --
| [Anno2205](https://en.wikipedia.org/wiki/Anno_2205) | Anno 2205 | - | - | -- -- -- | -- -- -- | -- -- --
| [MnM:HO](https://en.wikipedia.org/wiki/Might_and_Magic:_Heroes_Online) | Might & Magic: Heroes Online | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRSSiege](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_Siege) | Tom Clancy's Rainbow Six Siege | - | - | -- -- -- | -- -- -- | -- -- --
| [TCEOnline]() | Tom Clancy's EndWar Online | - | - | -- -- -- | -- -- -- | -- -- --
| [RYAdventures]() | Rayman Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSmurfs:ER]() | The Smurfs: Epic Run | - | - | -- -- -- | -- -- -- | -- -- --
| [ACChronicles:I]() | Assassin's Creed Chronicles: India | - | - | -- -- -- | -- -- -- | -- -- --
| [Sandstorm: Pirate Wars](https://en.wikipedia.org/wiki/Sandstorm:_Pirate_Wars) | Sandstorm: Pirate Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [ACChronicles:R]() | Assassin's Creed Chronicles: Russia | - | - | -- -- -- | -- -- -- | -- -- --
| [FarCryPrimal](https://en.wikipedia.org/wiki/Far_Cry_Primal) | Far Cry Primal | - | - | -- -- -- | -- -- -- | -- -- --
| [ACIdentity](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Identity) | Assassin's Creed Identity | - | - | -- -- -- | -- -- -- | -- -- --
| [TCTD](https://en.wikipedia.org/wiki/Tom_Clancy%27s_The_Division) | Tom Clancy's The Division | - | - | -- -- -- | -- -- -- | -- -- --
| [TrackManiaTurbo](https://en.wikipedia.org/wiki/TrackMania_Turbo) | TrackMania Turbo | - | - | -- -- -- | -- -- -- | -- -- --
| [HungryShark:W]() | Hungry Shark: World | - | - | -- -- -- | -- -- -- | -- -- --
| [RockGods:TT]() | Rock Gods: Tap Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [TofBD](https://en.wikipedia.org/wiki/Trials_of_the_Blood_Dragon) | Trials of the Blood Dragon | - | - | -- -- -- | -- -- -- | -- -- --
| [NCIS:HC]() | NCIS: Hidden Crimes | - | - | -- -- -- | -- -- -- | -- -- --
| [Battleship]() | Battleship | - | - | -- -- -- | -- -- -- | -- -- --
| [Risk:UA]() | Risk: Urban Assault | - | - | -- -- -- | -- -- -- | -- -- --
| [Uno]() | Uno | - | - | -- -- -- | -- -- -- | -- -- --
| [ChampionsofAnteria](https://en.wikipedia.org/wiki/Champions_of_Anteria) | Champions of Anteria | - | - | -- -- -- | -- -- -- | -- -- --
| [JustSing](https://en.wikipedia.org/wiki/Just_Sing) | Just Sing | - | - | -- -- -- | -- -- -- | -- -- --
| [RabbidsCrazyRush]() | Rabbids Crazy Rush | - | - | -- -- -- | -- -- -- | -- -- --
| [FaceUp:TSG]() | Face Up: The Selfie Game | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith:A2014ER]() | Rocksmith: All-new 2014 Edition - Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [EagleFlight](https://en.wikipedia.org/wiki/Eagle_Flight) | Eagle Flight | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2017](https://en.wikipedia.org/wiki/Just_Dance_2017) | Just Dance 2017 | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:TEC]() | Assassin's Creed: The Ezio Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [WatchDogs2](https://en.wikipedia.org/wiki/Watch_Dogs_2) | Watch Dogs 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Steep](https://en.wikipedia.org/wiki/Steep_(video_game)) | Steep | - | - | -- -- -- | -- -- -- | -- -- --
| [WerewolvesWithin](https://en.wikipedia.org/wiki/Werewolves_Within) | Werewolves Within | - | - | -- -- -- | -- -- -- | -- -- --
| [CityofLove:P]() | City of Love: Paris | - | - | -- -- -- | -- -- -- | -- -- --
| [ForHonor](https://en.wikipedia.org/wiki/For_Honor) | For Honor | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGRWildlands](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_Wildlands) | Tom Clancy's Ghost Recon Wildlands | - | - | -- -- -- | -- -- -- | -- -- --
| [TrackMania2:L]() | TrackMania 2: Lagoon | - | - | -- -- -- | -- -- -- | -- -- --
| [StarTrek:BC](https://en.wikipedia.org/wiki/Star_Trek:_Bridge_Crew) | Star Trek: Bridge Crew | - | - | -- -- -- | -- -- -- | -- -- --
| [MarioRabbids:KB](https://en.wikipedia.org/wiki/Mario_%2B_Rabbids_Kingdom_Battle) | Mario + Rabbids Kingdom Battle | - | - | -- -- -- | -- -- -- | -- -- --
| [RYLegends:DE]() | Rayman Legends: Definitive Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Atomega](https://en.wikipedia.org/wiki/Atomega) | Atomega | - | - | -- -- -- | -- -- -- | -- -- --
| [SouthPark:TFBW](https://en.wikipedia.org/wiki/South_Park:_The_Fractured_but_Whole) | South Park: The Fractured But Whole | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2018](https://en.wikipedia.org/wiki/Just_Dance_2018) | Just Dance 2018 | - | - | -- -- -- | -- -- -- | -- -- --
| [ACOrigins](https://store.steampowered.com/app/582160) | Assassin's Creed Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [Monopoly]() | Monopoly | - | - | -- -- -- | -- -- -- | -- -- --
| [Jeopardy]() | Jeopardy! | - | - | -- -- -- | -- -- -- | -- -- --
| [WheelofFortune]() | Wheel of Fortune | - | - | -- -- -- | -- -- -- | -- -- --
| [SouthPark:PD](https://en.wikipedia.org/wiki/South_Park:_Phone_Destroyer) | South Park: Phone Destroyer | - | - | -- -- -- | -- -- -- | -- -- --
| [Ode]() | Ode | - | - | -- -- -- | -- -- -- | -- -- --
| [DiscoveryTour:ACAE]() | Discovery Tour: Assassin's Creed - Ancient Egypt | - | - | -- -- -- | -- -- -- | -- -- --
| [HungryDragon]() | Hungry Dragon | - | - | -- -- -- | -- -- -- | -- -- --
| [ACRogue:R]() | Assassin's Creed Rogue Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [FC5](https://en.wikipedia.org/wiki/Far_Cry_5) | Far Cry 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [FC3:CE]() | Far Cry 3: Classic Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [TheCrew2](https://en.wikipedia.org/wiki/The_Crew_2) | The Crew 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TS:HE]() | The Settlers: History Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [LegendaryFishing]() | Legendary Fishing | - | - | -- -- -- | -- -- -- | -- -- --
| [Transference](https://en.wikipedia.org/wiki/Transference_(video_game)) | Transference | - | - | -- -- -- | -- -- -- | -- -- --
| [ACOdyssey](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Odyssey) | Assassin's Creed Odyssey | - | - | -- -- -- | -- -- -- | -- -- --
| [ChildofLight:UE]() | Child of Light: Ultimate Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Starlink: Battle for Atlas](https://en.wikipedia.org/wiki/Starlink:_Battle_for_Atlas) | Starlink: Battle for Atlas | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2019](https://en.wikipedia.org/wiki/Just_Dance_2019) | Just Dance 2019 | - | - | -- -- -- | -- -- -- | -- -- --
| [SportsParty]() | Sports Party | - | - | -- -- -- | -- -- -- | -- -- --
| [Brawlhalla](https://en.wikipedia.org/wiki/Brawlhalla) | Brawlhalla | - | - | -- -- -- | -- -- -- | -- -- --
| [Valiant Hearts: The Great War](https://en.wikipedia.org/wiki/Valiant_Hearts:_The_Great_War) | Valiant Hearts: The Great War | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:R3]() | Assassin's Creed: Rebellion | - | - | -- -- -- | -- -- -- | -- -- --
| [FCNewDawn](https://en.wikipedia.org/wiki/Far_Cry_New_Dawn) | Far Cry New Dawn | - | - | -- -- -- | -- -- -- | -- -- --
| [Anno1800](https://en.wikipedia.org/wiki/Anno_1800) | Anno 1800 | - | - | -- -- -- | -- -- -- | -- -- --
| [TrialsRising](https://en.wikipedia.org/wiki/Trials_Rising) | Trials Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [TCTD2](https://en.wikipedia.org/wiki/Tom_Clancy%27s_The_Division_2) | Tom Clancy's The Division 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SpaceJunkies]() | Space Junkies | - | - | -- -- -- | -- -- -- | -- -- --
| [Growtopia](https://en.wikipedia.org/wiki/Growtopia) | Growtopia | - | - | -- -- -- | -- -- -- | -- -- --
| [ACIII:R]() | Assassin's Creed III Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [DiscoveryTour:ACAG]() | Discovery Tour: Assassin's Creed - Ancient Greece | - | - | -- -- -- | -- -- -- | -- -- --
| [RYMini](https://en.wikipedia.org/wiki/Rayman_Mini) | Rayman Mini | - | - | -- -- -- | -- -- -- | -- -- --
| [TCGRBreakpoint](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Ghost_Recon_Breakpoint) | Tom Clancy's Ghost Recon Breakpoint | - | - | -- -- -- | -- -- -- | -- -- --
| [Rabbids:C]() | Rabbids: Coding! | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2020](https://en.wikipedia.org/wiki/Just_Dance_2020) | Just Dance 2020 | - | - | -- -- -- | -- -- -- | -- -- --
| [MnMHeroes:EoC]() | Might & Magic Heroes: Era of Chaos | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:TRC]() | Assassin's Creed: The Rebel Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MnM:CR]() | Might & Magic: Chess Royale | - | - | -- -- -- | -- -- -- | -- -- --
| [Trackmania](https://en.wikipedia.org/wiki/TrackMania_(2020_video_game)) | Trackmania | - | - | -- -- -- | -- -- -- | -- -- --
| [HyperScape](https://en.wikipedia.org/wiki/Hyper_Scape) | Hyper Scape | - | - | -- -- -- | -- -- -- | -- -- --
| [TCES]() | Tom Clancy's Elite Squad | - | - | -- -- -- | -- -- -- | -- -- --
| [Rabbids:WR]() | Rabbids: Wild Race | - | - | -- -- -- | -- -- -- | -- -- --
| [AGOS:AGoS]() | AGOS: A Game of Space | - | - | -- -- -- | -- -- -- | -- -- --
| [WatchDogs:L](https://en.wikipedia.org/wiki/Watch_Dogs:_Legion) | Watch Dogs: Legion | - | - | -- -- -- | -- -- -- | -- -- --
| [ACValhalla](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Valhalla) | Assassin's Creed Valhalla | - | - | -- -- -- | -- -- -- | -- -- --
| [FamilyFeud]() | Family Feud | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2021](https://en.wikipedia.org/wiki/Just_Dance_2021) | Just Dance 2021 | - | - | -- -- -- | -- -- -- | -- -- --
| [IdleRestaurantTycoon]() | Idle Restaurant Tycoon | - | - | -- -- -- | -- -- -- | -- -- --
| [ImmortalsFenyxRising](https://en.wikipedia.org/wiki/Immortals_Fenyx_Rising) | Immortals Fenyx Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [SPVTW:TGCE](https://en.wikipedia.org/wiki/Scott_Pilgrim_vs._The_World:_The_Game_-_Complete_Edition) | Scott Pilgrim vs. The World: The Game - Complete Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [FC6](https://en.wikipedia.org/wiki/Far_Cry_6) | Far Cry 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [DiscoveryTour:VA]() | Discovery Tour: Viking Age | - | - | -- -- -- | -- -- -- | -- -- --
| [RidersRepublic](https://en.wikipedia.org/wiki/Riders_Republic) | Riders Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2022](https://en.wikipedia.org/wiki/Just_Dance_2022) | Just Dance 2022 | - | - | -- -- -- | -- -- -- | -- -- --
| [Monopoly:M]() | Monopoly: Madness | - | - | -- -- -- | -- -- -- | -- -- --
| [ClashofBeasts]() | Clash of Beasts | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRSExtraction](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_Extraction) | Tom Clancy's Rainbow Six Extraction | - | - | -- -- -- | -- -- -- | -- -- --
| [TrivialPursuitLive:2]() | Trivial Pursuit Live! 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RollerChampions](https://en.wikipedia.org/wiki/Roller_Champions) | Roller Champions | - | - | -- -- -- | -- -- -- | -- -- --
| [Rabbids:PoL]() | Rabbids: Party of Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [WildArenaSurvivors]() | Wild Arena Survivors | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith:P](https://en.wikipedia.org/wiki/Rocksmith%2B) | Rocksmith+ | - | - | -- -- -- | -- -- -- | -- -- --
| [MarioRabbidsSparksofHope](https://en.wikipedia.org/wiki/Mario_%2B_Rabbids_Sparks_of_Hope) | Mario + Rabbids Sparks of Hope | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2023Edition](https://en.wikipedia.org/wiki/Just_Dance_2023_Edition) | Just Dance 2023 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [OddBallers](https://en.wikipedia.org/wiki/OddBallers) | OddBallers | - | - | -- -- -- | -- -- -- | -- -- --
| [ValiantHearts:CH]() | Valiant Hearts: Coming Home | - | - | -- -- -- | -- -- -- | -- -- --
| [TS:NA](https://en.wikipedia.org/wiki/The_Settlers:_New_Allies) | The Settlers: New Allies | - | - | -- -- -- | -- -- -- | -- -- --
| [MightyQuestRoguePalace]() | Mighty Quest Rogue Palace | - | - | -- -- -- | -- -- -- | -- -- --
| [TheCrewMotorfest](https://en.wikipedia.org/wiki/The_Crew_Motorfest) | The Crew Motorfest | - | - | -- -- -- | -- -- -- | -- -- --
| [ACMirage](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Mirage) | Assassin's Creed Mirage | - | - | -- -- -- | -- -- -- | -- -- --
| [JustDance2024Edition](https://en.wikipedia.org/wiki/Just_Dance_2024_Edition) | Just Dance 2024 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ACNexusVR](https://en.wikipedia.org/wiki/Assassin%27s_Creed_Nexus_VR) | Assassin's Creed Nexus VR | - | - | -- -- -- | -- -- -- | -- -- --
| [Avatar:FoP](https://en.wikipedia.org/wiki/Avatar:_Frontiers_of_Pandora) | Avatar: Frontiers of Pandora | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TLC](https://en.wikipedia.org/wiki/Prince_of_Persia:_The_Lost_Crown) | Prince of Persia: The Lost Crown | - | - | -- -- -- | -- -- -- | -- -- --
| [SkullandBones](https://en.wikipedia.org/wiki/Skull_and_Bones_(video_game)) | Skull and Bones | - | - | -- -- -- | -- -- -- | -- -- --
| [Invincible:GtG]() | Invincible: Guarding the Globe | - | - | -- -- -- | -- -- -- | -- -- --
| [TCRSMobile](https://en.wikipedia.org/wiki/Tom_Clancy%27s_Rainbow_Six_Mobile) | Tom Clancy's Rainbow Six Mobile | - | - | -- -- -- | -- -- -- | -- -- --
| [TCTDResurgence](https://en.wikipedia.org/wiki/Tom_Clancy%27s_The_Division_Resurgence) | Tom Clancy's The Division Resurgence | - | - | -- -- -- | -- -- -- | -- -- --
| [StarWarsOutlaws](https://en.wikipedia.org/wiki/Star_Wars_Outlaws) | Star Wars Outlaws | - | - | -- -- -- | -- -- -- | -- -- --
| [XDefiant](https://en.wikipedia.org/wiki/XDefiant) | XDefiant | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:CH]() | Assassin's Creed: Codename Hexe | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:CI]() | Assassin's Creed: Codename Invictus | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:CR]() | Assassin's Creed: Codename Red | - | - | -- -- -- | -- -- -- | -- -- --
| [ACJade]() | Assassin's Creed Jade | - | - | -- -- -- | -- -- -- | -- -- --
| [BeyondGoodandEvil2](https://en.wikipedia.org/wiki/Beyond_Good_and_Evil_2) | Beyond Good and Evil 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PoP:TSoTR]() | Prince of Persia: The Sands of Time (remake) | - | - | -- -- -- | -- -- -- | -- -- --
| [ProjectU]() | Project U | - | - | -- -- -- | -- -- -- | -- -- --
| [TCTD3]() | Tom Clancy's The Division 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [TCTDHeartland](https://en.wikipedia.org/wiki/Tom_Clancy%27s_The_Division_Heartland) | Tom Clancy's The Division Heartland | - | - | -- -- -- | -- -- -- | -- -- --
| [TCSC:R]() | Tom Clancy's Splinter Cell (remake) | - | - | -- -- -- | -- -- -- | -- -- --
| [UntitledACmobilegame]() | Untitled Assassin's Creed mobile game | - | - | -- -- -- | -- -- -- | -- -- --
| **Unity** | **Unity**
| [AmongUs](https://store.steampowered.com/app/945360) | Among Us | - | - | -- -- -- | -- -- -- | -- -- --
| [Cities](https://store.steampowered.com/app/255710) | Cities: Skylines | - | - | -- -- -- | -- -- -- | -- -- --
| [Tabletop](https://store.steampowered.com/app/286160) | Tabletop Simulator | - | - | -- -- -- | -- -- -- | -- -- --
| [UBoat](https://store.steampowered.com/app/1272010) | Destroyer: The U-Boat Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [7D2D](https://store.steampowered.com/app/251570) | 7 Days to Die | - | - | -- -- -- | -- -- -- | -- -- --
| **Unknown** | **Unknown**
| [APP]() | Application | - | - | -- -- -- | -- -- -- | -- -- --
| [CAT]() | Catalog | - | - | -- -- -- | -- -- -- | -- -- --
| **Valve** | **Valve**
| [HL](https://store.steampowered.com/app/70) | Half-Life | open | read | gl -- -- | -- -- -- | -- -- --
| [TF](https://store.steampowered.com/app/20) | Team Fortress Classic | open | read | gl -- -- | -- -- -- | -- -- --
| [HL:OF](https://store.steampowered.com/app/50) | Half-Life: Opposing Force | open | read | -- -- -- | -- -- -- | -- -- --
| [Ricochet](https://store.steampowered.com/app/60) | Ricochet | open | read | -- -- -- | -- -- -- | -- -- --
| [CS](https://store.steampowered.com/app/10) | Counter-Strike | open | read | -- -- -- | -- -- -- | -- -- --
| [DM](https://store.steampowered.com/app/40) | Deathmatch Classic | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:BS](https://store.steampowered.com/app/130) | Half-Life: Blue Shift | open | read | -- -- -- | -- -- -- | -- -- --
| [DOD](https://store.steampowered.com/app/30) | Day of Defeat | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:CZ](https://store.steampowered.com/app/80) | Counter-Strike: Condition Zero | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:Src](https://store.steampowered.com/app/280) | Half-Life: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:Src](https://store.steampowered.com/app/240) | Counter-Strike: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2](https://store.steampowered.com/app/220) | Half-Life 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:DM](https://store.steampowered.com/app/320) | Half-Life 2: Deathmatch | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:DM:Src](https://store.steampowered.com/app/360) | Half-Life Deathmatch: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:E1](https://store.steampowered.com/app/380) | Half-Life 2: Episode One | open | read | -- -- -- | -- -- -- | -- -- --
| [Portal](https://store.steampowered.com/app/400) | Portal | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:E2](https://store.steampowered.com/app/420) | Half-Life 2: Episode Two | open | read | -- -- -- | -- -- -- | -- -- --
| [TF2](https://store.steampowered.com/app/440) | Team Fortress 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [L4D](https://store.steampowered.com/app/500) | Left 4 Dead | open | read | -- -- -- | -- -- -- | -- -- --
| [L4D2](https://store.steampowered.com/app/550) | Left 4 Dead 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [DOD:Src](https://store.steampowered.com/app/300) | Day of Defeat: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [Portal2](https://store.steampowered.com/app/620) | Portal 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:GO](https://store.steampowered.com/app/730) | Counter-Strike: Global Offensive | open | read | -- -- -- | -- -- -- | -- -- --
| [D2](https://store.steampowered.com/app/570) | Dota 2 | open | read | gl -- -- | -- -- -- | -- -- --
| [TheLab:RR](https://store.steampowered.com/app/450390) | The Lab: Robot Repair | open | read | gl -- -- | gl -- -- | -- -- --
| [TheLab:SS](https://store.steampowered.com/app/450390) | The Lab: Secret Shop | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLab:TL](https://store.steampowered.com/app/450390) | The Lab: The Lab | - | - | -- -- -- | -- -- -- | -- -- --
| [HL:Alyx](https://store.steampowered.com/app/546560) | Half-Life: Alyx | open | read | gl -- -- | gl -- -- | -- -- --
| **Volition** | **Volition**
| [D](https://en.wikipedia.org/wiki/Descent_(video_game)) | Descent | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](https://en.wikipedia.org/wiki/Descent_II) | Descent II | - | - | -- -- -- | -- -- -- | -- -- --
| [FS](https://en.wikipedia.org/wiki/Descent:_FreeSpace_%E2%80%93_The_Great_War) | Descent: FreeSpace - The Great War | - | - | -- -- -- | -- -- -- | -- -- --
| [FS2](https://en.wikipedia.org/wiki/FreeSpace_2) | FreeSpace 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [S](https://en.wikipedia.org/wiki/Summoner_(video_game)) | Summoner | - | - | -- -- -- | -- -- -- | -- -- --
| [RF](https://en.wikipedia.org/wiki/Red_Faction_(video_game)) | Red Faction | - | - | -- -- -- | -- -- -- | -- -- --
| [S2](https://en.wikipedia.org/wiki/Summoner_2) | Summoner 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RF2](https://en.wikipedia.org/wiki/Red_Faction_II) | Red Faction II | - | - | -- -- -- | -- -- -- | -- -- --
| [TP](https://en.wikipedia.org/wiki/The_Punisher_(2004_video_game)) | The Punisher | - | - | -- -- -- | -- -- -- | -- -- --
| [SR06](https://en.wikipedia.org/wiki/Saints_Row_(2006_video_game)) | Saints Row | - | - | -- -- -- | -- -- -- | -- -- --
| [SR2](https://en.wikipedia.org/wiki/Saints_Row_2) | Saints Row 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RF:G](https://en.wikipedia.org/wiki/Red_Faction:_Guerrilla) | Red Faction: Guerrilla | - | - | -- -- -- | -- -- -- | -- -- --
| [RF:A](https://en.wikipedia.org/wiki/Red_Faction:_Armageddon) | Red Faction: Armageddon | - | - | -- -- -- | -- -- -- | -- -- --
| [SR3](https://en.wikipedia.org/wiki/Saints_Row:_The_Third) | Saints Row: The Third | - | - | -- -- -- | -- -- -- | -- -- --
| [SR4](https://en.wikipedia.org/wiki/Saints_Row_IV) | Saints Row IV | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://store.steampowered.com/app/273590) | Descent 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SR:G](https://en.wikipedia.org/wiki/Saints_Row:_Gat_out_of_Hell) | Saints Row: Gat out of Hell | - | - | -- -- -- | -- -- -- | -- -- --
| [AoM](https://en.wikipedia.org/wiki/Agents_of_Mayhem) | Agents of Mayhem | - | - | -- -- -- | -- -- -- | -- -- --
| [RF:GR](https://en.wikipedia.org/wiki/Red_Faction:_Guerrilla) | Red Faction: Guerrilla Re-Mars-tered | - | - | -- -- -- | -- -- -- | -- -- --
| [SR](https://en.wikipedia.org/wiki/Saints_Row_(2022_video_game)) | Saints Row | - | - | -- -- -- | -- -- -- | -- -- --
| **WB** | **WB Games Boston**
| [AC](https://en.wikipedia.org/wiki/Asheron%27s_Call) | Asheron's Call | open | read | gl -- -- | -- -- -- | -- -- --
| [AC2](https://en.wikipedia.org/wiki/Asheron%27s_Call_2:_Fallen_Kings) | Asheron's Call 2: Fallen Kings | - | - | -- -- -- | -- -- -- | -- -- --
| [DDO](https://en.wikipedia.org/wiki/Dungeons_%26_Dragons_Online) | Dungeons & Dragons Online | - | - | -- -- -- | -- -- -- | -- -- --
| [LotRO](https://en.wikipedia.org/wiki/The_Lord_of_the_Rings_Online) | The Lord of the Rings Online | - | - | -- -- -- | -- -- -- | -- -- --
| [IC](https://en.wikipedia.org/wiki/Infinite_Crisis_(video_game)) | Infinite Crisis | - | - | -- -- -- | -- -- -- | -- -- --
| [B:AU](https://en.wikipedia.org/wiki/Asheron%27s_Call_2:_Fallen_Kings) | Batman: Arkham Underworld | - | - | -- -- -- | -- -- -- | -- -- --
| [GoT:C](https://warnerbrosgames.com/game/game-of-thrones-conquest) | Game of Thrones: Conquest | - | - | -- -- -- | -- -- -- | -- -- --
| **X2K** | **Rockstar Games**
| [M](https://store.steampowered.com/app/40990/Mafia/) | Mafia | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K5](https://en.wikipedia.org/wiki/Major_League_Baseball_2K5) | Major League Baseball 2K5 | - | - | -- -- -- | -- -- -- | -- -- --
| [FordRacing3](https://en.wikipedia.org/wiki/Ford_Racing_3) | Ford Racing 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [CloseCombat:FtF](https://en.wikipedia.org/wiki/Close_Combat:_First_to_Fight) | Close Combat: First to Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [FordMustang:TLL](https://en.wikipedia.org/wiki/Ford_Mustang:_The_Legend_Lives) | Ford Mustang: The Legend Lives | - | - | -- -- -- | -- -- -- | -- -- --
| [Stronghold2](https://store.steampowered.com/app/40960/Stronghold_2_Steam_Edition/) | Stronghold 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MotocrossMania3](https://cdromance.org/ps2-iso/motocross-mania-3-usa/) | Motocross Mania 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:P](https://en.wikipedia.org/wiki/Sid_Meier%27s_Pirates!_(2004_video_game)) | Sid Meier's Pirates! | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K6](https://en.wikipedia.org/wiki/NHL_2K6) | NHL 2K6 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K6](https://en.wikipedia.org/wiki/NBA_2K6) | NBA 2K6 | - | - | -- -- -- | -- -- -- | -- -- --
| [TopSpin](https://en.wikipedia.org/wiki/Top_Spin_(video_game)) | Top Spin | - | - | -- -- -- | -- -- -- | -- -- --
| [Conflict:GT](https://www.myabandonware.com/game/conflict-global-terror-e39) | Conflict: Global Terror | - | - | -- -- -- | -- -- -- | -- -- --
| [SeriousSam2](https://store.steampowered.com/app/204340/Serious_Sam_2/) | Serious Sam 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K5:WSE](https://en.wikipedia.org/wiki/Major_League_Baseball_2K5#World_Series_Edition) | Major League Baseball 2K5: World Series Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ShatteredUnion](https://store.steampowered.com/app/3960/Shattered_Union/) | Shattered Union | - | - | -- -- -- | -- -- -- | -- -- --
| [WorldPokerTour](https://en.wikipedia.org/wiki/World_Poker_Tour_(video_game)) | World Poker Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [CoC:DCotE](https://en.wikipedia.org/wiki/Call_of_Cthulhu:_Dark_Corners_of_the_Earth) | Call of Cthulhu: Dark Corners of the Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [Vietcong2](https://www.myabandonware.com/game/vietcong-2-eop) | Vietcong 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:C4](https://store.steampowered.com/app/3900/Sid_Meiers_Civilization_IV/) | Sid Meier's Civilization IV | - | - | -- -- -- | -- -- -- | -- -- --
| [Zathura](https://en.wikipedia.org/wiki/Zathura_(video_game)) | Zathura | - | - | -- -- -- | -- -- -- | -- -- --
| [FvC](https://en.wikipedia.org/wiki/Ford_vs._Chevy) | Ford vs Chevy | - | - | -- -- -- | -- -- -- | -- -- --
| [Amped3](https://en.wikipedia.org/wiki/Amped_3) | Amped 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Hoops2K6](https://en.wikipedia.org/wiki/College_Hoops_2K6) | College Hoops 2K6 | - | - | -- -- -- | -- -- -- | -- -- --
| [Torino2006](https://en.wikipedia.org/wiki/Torino_2006_(video_game)) | Torino 2006 | - | - | -- -- -- | -- -- -- | -- -- --
| [X24:TG](https://en.wikipedia.org/wiki/24:_The_Game) | 24: The Game | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion](https://store.steampowered.com/app/22330/The_Elder_Scrolls_IV_Oblivion_Game_of_the_Year_Edition/) | The Elder Scrolls IV: Oblivion | - | - | -- -- -- | -- -- -- | -- -- --
| [TopSpin2](https://en.wikipedia.org/wiki/Top_Spin_2) | Top Spin 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K6](https://en.wikipedia.org/wiki/Major_League_Baseball_2K6) | Major League Baseball 2K6 | - | - | -- -- -- | -- -- -- | -- -- --
| [TDVC](https://www.myabandonware.com/game/the-da-vinci-code-fjg) | The Da Vinci Code | - | - | -- -- -- | -- -- -- | -- -- --
| [Prey](https://www.myabandonware.com/game/prey-dd1) | Prey | - | - | -- -- -- | -- -- -- | -- -- --
| [CivCity:R](https://store.steampowered.com/app/3980/CivCity_Rome/) | CivCity: Rome | - | - | -- -- -- | -- -- -- | -- -- --
| [DS2](https://store.steampowered.com/app/39200/Dungeon_Siege_II/) | Dungeon Siege II: Broken World | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K7](https://en.wikipedia.org/wiki/NHL_2K7) | NHL 2K7 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K7](https://en.wikipedia.org/wiki/NBA_2K7) | NBA 2K7 | - | - | -- -- -- | -- -- -- | -- -- --
| [FGVG](https://en.wikipedia.org/wiki/Family_Guy_Video_Game!) | Family Guy Video Game! | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:R](https://store.steampowered.com/app/7600/Sid_Meiers_Railroads/) | Sid Meier's Railroads! | - | - | -- -- -- | -- -- -- | -- -- --
| [StrongholdLegends](https://store.steampowered.com/app/40980/Stronghold_Legends_Steam_Edition/) | Stronghold Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [DS:ToA](https://cdromance.org/psp/dungeon-siege-throne-of-agony/) | Dungeon Siege: Throne of Agony | - | - | -- -- -- | -- -- -- | -- -- --
| [Hoops2K7](https://en.wikipedia.org/wiki/College_Hoops_2K7) | College Hoops 2K7 | - | - | -- -- -- | -- -- -- | -- -- --
| [GhostRider](https://en.wikipedia.org/wiki/Ghost_Rider_(video_game)) | Ghost Rider | - | - | -- -- -- | -- -- -- | -- -- --
| [JE:SE](https://store.steampowered.com/app/7110/Jade_Empire_Special_Edition/) | Jade Empire: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K7](https://en.wikipedia.org/wiki/Major_League_Baseball_2K7) | Major League Baseball 2K7 | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:RotSS](https://en.wikipedia.org/wiki/Fantastic_Four:_Rise_of_the_Silver_Surfer_(video_game)) | Fantastic Four: Rise of the Silver Surfer | - | - | -- -- -- | -- -- -- | -- -- --
| [TheBigs](https://en.wikipedia.org/wiki/The_Bigs) | The Bigs | - | - | -- -- -- | -- -- -- | -- -- --
| [TheDarkness](https://www.xbox.com/en-US/games/store/the-darkness/C035L0NS3SQN) | The Darkness | - | - | -- -- -- | -- -- -- | -- -- --
| [Football2K8](https://en.wikipedia.org/wiki/All-Pro_Football_2K8) | All-Pro Football 2K8 | - | - | -- -- -- | -- -- -- | -- -- --
| [BS](https://store.steampowered.com/app/7670/BioShock/) | BioShock | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K8](https://en.wikipedia.org/wiki/NHL_2K8) | NHL 2K8 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K8](https://en.wikipedia.org/wiki/NBA_2K8) | NBA 2K8 | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB::PP](https://en.wikipedia.org/wiki/MLB_Power_Pros) | MLB Power Pros | - | - | -- -- -- | -- -- -- | -- -- --
| [DtE:DStM](https://en.wikipedia.org/wiki/Dora_the_Explorer_video_games#Dora_Saves_the_Mermaids) | Dora the Explorer: Dora Saves the Mermaids | - | - | -- -- -- | -- -- -- | -- -- --
| [GDG:SR](https://en.wikipedia.org/wiki/Go,_Diego,_Go!_Safari_Rescue) | Go, Diego, Go! Safari Rescue | - | - | -- -- -- | -- -- -- | -- -- --
| [Hoops2K8](https://en.wikipedia.org/wiki/College_Hoops_2K8) | College Hoops 2K8 | - | - | -- -- -- | -- -- -- | -- -- --
| [DoND:SVG](https://www.myabandonware.com/game/deal-or-no-deal-secret-vault-games-imw) | Deal or No Deal: Secret Vault Games | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K8](https://en.wikipedia.org/wiki/Major_League_Baseball_2K8) | Major League Baseball 2K8 | - | - | -- -- -- | -- -- -- | -- -- --
| [DKP:P](https://en.wikipedia.org/wiki/Don_King_Presents:_Prizefighter) | Don King Presents: Prizefighter | - | - | -- -- -- | -- -- -- | -- -- --
| [TopSpin3](https://en.wikipedia.org/wiki/Top_Spin_3) | Top Spin 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [CG](https://store.steampowered.com/app/1249740/Carnival_Games/) | Carnival Games | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:CR](https://www.xbox.com/en-US/games/store/sid-meiers-civilization-revolution/BQ0WGG6B6X2H) | Sid Meier's Civilization Revolution | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K8:PP](https://en.wikipedia.org/wiki/MLB_Power_Pros_2008) | MLB Power Pros 2008 | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K9](https://en.wikipedia.org/wiki/NHL_2K9) | NHL 2K9 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K9](https://en.wikipedia.org/wiki/NBA_2K9) | NBA 2K9 | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB:SB](https://en.wikipedia.org/wiki/MLB_Stickball) | MLB Stickball | - | - | -- -- -- | -- -- -- | -- -- --
| [CG:MG](https://en.wikipedia.org/wiki/Carnival_Games:_Mini-Golf) | Carnival Games: Mini-Golf | - | - | -- -- -- | -- -- -- | -- -- --
| [DtE:DStSP](https://en.wikipedia.org/wiki/Dora_the_Explorer_video_games#Dora_Saves_the_Snow_Princess) | Dora the Explorer: Dora Saves the Snow Princess | - | - | -- -- -- | -- -- -- | -- -- --
| [GDG:GDR](https://en.wikipedia.org/wiki/Go,_Diego,_Go!#Video_games) | Go, Diego, Go! Great Dinosaur Rescue | - | - | -- -- -- | -- -- -- | -- -- --
| [WP:StA](https://en.wikipedia.org/wiki/Wonder_Pets!#Game) | Wonder Pets! Save the Animals! | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB:SS](https://en.wikipedia.org/wiki/MLB_Superstars) | MLB Superstars | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB:FOM](https://en.wikipedia.org/wiki/MLB_Front_Office_Manager) | MLB Front Office Manager | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K9](https://en.wikipedia.org/wiki/Major_League_Baseball_2K9) | Major League Baseball 2K9 | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K9:FAS](https://en.wikipedia.org/wiki/Major_League_Baseball_2K9_Fantasy_All-Stars) | Major League Baseball 2K9: Fantasy All-Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [BS2:SitS](https://en.wikipedia.org/wiki/BioShock_2#Release) | There's Something in the Sea | - | - | -- -- -- | -- -- -- | -- -- --
| [DKB](XXX) | Don King Boxing | - | - | -- -- -- | -- -- -- | -- -- --
| [TheBigs2](https://en.wikipedia.org/wiki/The_Bigs_2) | The Bigs 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BPB](https://en.wikipedia.org/wiki/Birthday_Party_Bash) | Birthday Party Bash | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K10:DC](https://en.wikipedia.org/wiki/NBA_2K10_Draft_Combine) | NBA 2K10: Draft Combine | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K10](https://en.wikipedia.org/wiki/NHL_2K10) | NHL 2K10 | - | - | -- -- -- | -- -- -- | -- -- --
| [BaseballBlast](https://nintendo.fandom.com/wiki/Baseball_Blast!) | Baseball Blast! | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K10](https://en.wikipedia.org/wiki/NBA_2K10) | NBA 2K10 | - | - | -- -- -- | -- -- -- | -- -- --
| [AxelPixel](https://store.steampowered.com/app/8970/Axel__Pixel/) | Axel & Pixel | - | - | -- -- -- | -- -- -- | -- -- --
| [BL](https://store.steampowered.com/app/729040/Borderlands_Game_of_the_Year_Enhanced/) | Borderlands | - | - | -- -- -- | -- -- -- | -- -- --
| [DoraPuppy](https://en.wikipedia.org/wiki/Dora_the_Explorer_video_games#Dora_Puppy) | Dora Puppy | - | - | -- -- -- | -- -- -- | -- -- --
| [DtE:DStCK](https://en.wikipedia.org/wiki/Dora_the_Explorer_video_games#Dora_Saves_the_Crystal_Kingdom) | Dora the Explorer: Dora Saves the Crystal Kingdom | - | - | -- -- -- | -- -- -- | -- -- --
| [NHKL:NYC]() | Ni Hao, Kai-Lan: New Year Celebration | - | - | -- -- -- | -- -- -- | -- -- --
| [NHKL:SGDay]() | Ni Hao, Kai-Lan: Super Game Day | - | - | -- -- -- | -- -- -- | -- -- --
| [TheBackyardigans]() | The Backyardigans | - | - | -- -- -- | -- -- -- | -- -- --
| [RBaBB:CFAE]() | Ringling Bros. and Barnum & Bailey: Circus Friends - Asian Elephants | - | - | -- -- -- | -- -- -- | -- -- --
| [RBaBB:TGSoEarth]() | Ringling Bros. and Barnum & Bailey: The Greatest Show on Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [BS2](https://store.steampowered.com/app/8850/BioShock_2/) | BioShock 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TMoPBW](https://store.steampowered.com/app/40930/The_Misadventures_of_PB_Winterbottom/) | The Misadventures of P.B. Winterbottom | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K10](https://en.wikipedia.org/wiki/Major_League_Baseball_2K10) | Major League Baseball 2K10 | - | - | -- -- -- | -- -- -- | -- -- --
| [DtE:DBBA]() | Dora the Explorer: Dora's Big Birthday Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [M2](https://store.steampowered.com/app/50130/Mafia_II_Classic/) | Mafia II | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K11](https://en.wikipedia.org/wiki/NHL_2K11) | NHL 2K11 | - | - | -- -- -- | -- -- -- | -- -- --
| [CG:N](https://en.wikipedia.org/wiki/New_Carnival_Games) | New Carnival Games | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:C5](https://store.steampowered.com/app/8930/Sid_Meiers_Civilization_V/) | Sid Meier's Civilization V | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K11](https://en.wikipedia.org/wiki/NBA_2K11) | NBA 2K11 | - | - | -- -- -- | -- -- -- | -- -- --
| [DCC](https://en.wikipedia.org/wiki/Dora_the_Explorer_video_games#Dora's_Cooking_Club) | Dora's Cooking Club | - | - | -- -- -- | -- -- -- | -- -- --
| [MB:DBaR]() | Mega Bloks: Diego's Build and Rescue | - | - | -- -- -- | -- -- -- | -- -- --
| [NickFit]() | Nickelodeon Fit | - | - | -- -- -- | -- -- -- | -- -- --
| [CG:V2]() | Carnival Games Volume II | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K11](https://en.wikipedia.org/wiki/Major_League_Baseball_2K11) | Major League Baseball 2K11 | - | - | -- -- -- | -- -- -- | -- -- --
| [TopSpin4](https://en.wikipedia.org/wiki/Top_Spin_4) | Top Spin 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DukeNudem]() | Duke Nudem | - | - | -- -- -- | -- -- -- | -- -- --
| [DukeNukem](https://store.steampowered.com/app/57900/Duke_Nukem_Forever/) | Duke Nukem Forever | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:CW](https://en.wikipedia.org/wiki/Civilization_World) | Sid Meier's Civilization World | - | - | -- -- -- | -- -- -- | -- -- --
| [NicktoonsMLB](https://en.wikipedia.org/wiki/Nicktoons_MLB) | Nicktoons MLB | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K12](https://en.wikipedia.org/wiki/NBA_2K12) | NBA 2K12 | - | - | -- -- -- | -- -- -- | -- -- --
| [DKL:PS]() | Dora & Kai-Lan's Pet Shelter | - | - | -- -- -- | -- -- -- | -- -- --
| [TeamUmizoomi]() | Team Umizoomi | - | - | -- -- -- | -- -- -- | -- -- --
| [NickelodeonDance]() | Nickelodeon Dance | - | - | -- -- -- | -- -- -- | -- -- --
| [LetsCheer]() | Let's Cheer! | - | - | -- -- -- | -- -- -- | -- -- --
| [CG:WW3](https://en.wikipedia.org/wiki/Carnival_Games:_Wild_West_3D) | Carnival Games: Wild West 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TheDarkness2](https://store.steampowered.com/app/67370/The_Darkness_II/) | The Darkness II | - | - | -- -- -- | -- -- -- | -- -- --
| [CG:MSMD]() | Carnival Games: Monkey See, Monkey Do | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K12](https://en.wikipedia.org/wiki/Major_League_Baseball_2K12) | Major League Baseball 2K12 | - | - | -- -- -- | -- -- -- | -- -- --
| [NicktoonsMLB3D](https://en.wikipedia.org/wiki/Nicktoons_MLB_3D) | Nicktoons MLB 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [SpecOps:TL](https://store.steampowered.com/app/50300/Spec_Ops_The_Line/) | Spec Ops: The Line | - | - | -- -- -- | -- -- -- | -- -- --
| [CCIG]() | Comedy Central's Indecision Game | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K:AS]() | NBA 2K All Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [HPfFtC]() | House Pest featuring Fiasco the Cat | - | - | -- -- -- | -- -- -- | -- -- --
| [BL2](https://store.steampowered.com/app/49520/Borderlands_2/) | Borderlands 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [GridBlock]() | GridBlock | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K13](https://en.wikipedia.org/wiki/NBA_2K13) | NBA 2K13 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K:ML]() | NBA 2K MyLIFE | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K:M]() | MyNBA 2K | - | - | -- -- -- | -- -- -- | -- -- --
| [XCOM:EU](https://store.steampowered.com/app/200510/XCOM_Enemy_Unknown/) | XCOM: Enemy Unknown | - | - | -- -- -- | -- -- -- | -- -- --
| [BLL](https://en.wikipedia.org/wiki/Borderlands_Legends) | Borderlands Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [BubbleGuppies]() | Bubble Guppies | - | - | -- -- -- | -- -- -- | -- -- --
| [NickDance2]() | Nickelodeon Dance 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TUD:FF]() | Team Umizoomi & Dora's Fantastic Flight | - | - | -- -- -- | -- -- -- | -- -- --
| [Herd3]() | Herd, Herd, Herd | - | - | -- -- -- | -- -- -- | -- -- --
| [BSI:IR]() | BioShock Infinite: Industrial Revolution | - | - | -- -- -- | -- -- -- | -- -- --
| [MLB2K13](https://en.wikipedia.org/wiki/Major_League_Baseball_2K13) | Major League Baseball 2K13 | - | - | -- -- -- | -- -- -- | -- -- --
| [BSI](https://store.steampowered.com/app/8870/BioShock_Infinite/) | BioShock Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| [HauntedHollow](https://en.wikipedia.org/wiki/Haunted_Hollow) | Haunted Hollow | - | - | -- -- -- | -- -- -- | -- -- --
| [Baseball2K:P]() | Pro Baseball 2K | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:AP](https://store.steampowered.com/app/244070/Sid_Meiers_Ace_Patrol/) | Sid Meier's Ace Patrol | - | - | -- -- -- | -- -- -- | -- -- --
| [Beejumbled]() | Beejumbled | - | - | -- -- -- | -- -- -- | -- -- --
| [TurdBirds](XXX) | Turd Birds | - | - | -- -- -- | -- -- -- | -- -- --
| [XCOM:TBD](https://store.steampowered.com/app/65930/The_Bureau_XCOM_Declassified/) | The Bureau: XCOM Declassified | - | - | -- -- -- | -- -- -- | -- -- --
| [2KDrive](https://en.wikipedia.org/wiki/2K_Drive) | 2K Drive | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K14](https://en.wikipedia.org/wiki/NBA_2K14) | NBA 2K14 | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K14](https://en.wikipedia.org/wiki/WWE_2K14) | WWE 2K14 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K14:M]() | MyNBA 2K14 | - | - | -- -- -- | -- -- -- | -- -- --
| [SenseiWars]() | Sensei Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:CR2](https://en.wikipedia.org/wiki/Civilization_Revolution_2) | Sid Meier's Civilization Revolution 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE:SC](https://en.wikipedia.org/wiki/WWE_SuperCard) | WWE SuperCard | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K15](https://en.wikipedia.org/wiki/NBA_2K15) | NBA 2K15 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K15:M]() | MyNBA 2K15 | - | - | -- -- -- | -- -- -- | -- -- --
| [BL:TPS](https://store.steampowered.com/app/261640/Borderlands_The_PreSequel/) | Borderlands: The Pre-Sequel! | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K](https://en.wikipedia.org/wiki/NHL_2K_(2014_video_game)) | NHL 2K | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:CBE](https://store.steampowered.com/app/65980/Sid_Meiers_Civilization_Beyond_Earth/) | Sid Meier's Civilization: Beyond Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K15](https://en.wikipedia.org/wiki/WWE_2K15) | WWE 2K15 | - | - | -- -- -- | -- -- -- | -- -- --
| [Evolve:HQ]() | Evolve: Hunter's Quest | - | - | -- -- -- | -- -- -- | -- -- --
| [Evolve](https://www.xbox.com/en-US/games/store/evolve/C4HHJM5JSSJ1/0001) | Evolve | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:S](https://store.steampowered.com/app/282210/Sid_Meiers_Starships/) | Sid Meier's Starships | - | - | -- -- -- | -- -- -- | -- -- --
| [BL:THC](https://store.steampowered.com/bundle/8133/Borderlands_The_Handsome_Collection/) | Borderlands: The Handsome Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K](https://en.wikipedia.org/wiki/WWE_2K_(video_game)) | WWE 2K | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K16](https://en.wikipedia.org/wiki/NBA_2K16) | NBA 2K16 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K16:M]() | MyNBA 2K16 | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL:SC]() | NHL SuperCard | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K16](https://en.wikipedia.org/wiki/WWE_2K16) | WWE 2K16 | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:CO]() | Sid Meier's Civilization Online | - | - | -- -- -- | -- -- -- | -- -- --
| [XCOM2](https://store.steampowered.com/app/268500/XCOM_2/) | XCOM 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [XCOM:EUP]() | XCOM: Enemy Unknown Plus | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:CR2P]() | Sid Meier's Civilization Revolution 2 Plus | - | - | -- -- -- | -- -- -- | -- -- --
| [BattlebornTap]() | Battleborn Tap | - | - | -- -- -- | -- -- -- | -- -- --
| [Battleborn](https://store.steampowered.com/app/394230/Battleborn/) | Battleborn | - | - | -- -- -- | -- -- -- | -- -- --
| [EvolveStage2](https://store.steampowered.com/app/273350/Evolve_Stage_2/) | Evolve Stage 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K17:M]() | MyNBA 2K17 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K17:TP](https://en.wikipedia.org/wiki/NBA_2K17:_The_Prelude) | NBA 2K17: The Prelude | - | - | -- -- -- | -- -- -- | -- -- --
| [BS:TC](https://store.steampowered.com/sub/127633/) | BioShock: The Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [BS:R](https://store.steampowered.com/app/409710/BioShock_Remastered/) | BioShock Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BS2:R](https://store.steampowered.com/app/409720/BioShock_2_Remastered/) | BioShock 2 Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K17](https://en.wikipedia.org/wiki/NBA_2K17) | NBA 2K17 | - | - | -- -- -- | -- -- -- | -- -- --
| [M3](https://www.playstation.com/en-us/games/mafia-iii/) | Mafia III | - | - | -- -- -- | -- -- -- | -- -- --
| [M3:R]() | Mafia III: Rivals | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K17](https://en.wikipedia.org/wiki/WWE_2K17) | WWE 2K17 | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K17:SC]() | NHL SuperCard 2K17 | - | - | -- -- -- | -- -- -- | -- -- --
| [SM:C6](https://store.steampowered.com/app/289070/Sid_Meiers_Civilization_VI/) | Sid Meier's Civilization VI | - | - | -- -- -- | -- -- -- | -- -- --
| [CG:VR](https://store.steampowered.com/app/458920/Carnival_Games_VR/) | Carnival Games VR | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K:VR](https://store.steampowered.com/app/519490/NBA_2KVR_Experience/) | NBA 2KVR Experience | - | - | -- -- -- | -- -- -- | -- -- --
| [Battleborn:FT]() | Battleborn: Free Trial | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K18:M]() | MyNBA 2K18 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K18:TP](https://en.wikipedia.org/wiki/NBA_2K18:_The_Prelude) | NBA 2K18: The Prelude | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K18](https://en.wikipedia.org/wiki/NBA_2K18) | NBA 2K18 | - | - | -- -- -- | -- -- -- | -- -- --
| [NHL2K18:SC]() | NHL SuperCard 2K18 | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K18](https://en.wikipedia.org/wiki/WWE_2K18) | WWE 2K18 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K:O2]() | NBA 2K Online 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [TGC2019:fPT](https://en.wikipedia.org/wiki/The_Golf_Club_2019_featuring_PGA_Tour) | The Golf Club 2019 featuring PGA Tour | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K19:M]() | MyNBA 2K19 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K19:TP](https://en.wikipedia.org/wiki/NBA_2K19:_The_Prelude) | NBA 2K19: The Prelude | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K19](https://en.wikipedia.org/wiki/NBA_2K19) | NBA 2K19 | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K19](https://en.wikipedia.org/wiki/WWE_2K19) | WWE 2K19 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K:P2](https://store.steampowered.com/app/726590/NBA_2K_Playgrounds_2/) | NBA 2K Playgrounds 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K:MB](https://en.wikipedia.org/wiki/NBA_2K_Mobile) | NBA 2K Mobile | - | - | -- -- -- | -- -- -- | -- -- --
| [BL2:VR](https://store.steampowered.com/app/991260/Borderlands_2_VR/) | Borderlands 2 VR | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K20:M]() | MyNBA 2K20 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K20](https://en.wikipedia.org/wiki/NBA_2K20) | NBA 2K20 | - | - | -- -- -- | -- -- -- | -- -- --
| [BL3](https://store.steampowered.com/app/397540/Borderlands_3/) | Borderlands 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K20](https://en.wikipedia.org/wiki/WWE_2K20) | WWE 2K20 | - | - | -- -- -- | -- -- -- | -- -- --
| [XCOM:CS](https://store.steampowered.com/app/882100/XCOM_Chimera_Squad/) | XCOM: Chimera Squad | - | - | -- -- -- | -- -- -- | -- -- --
| [PGATour2K21](https://en.wikipedia.org/wiki/PGA_Tour_2K21) | PGA Tour 2K21 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K21:M]() | MyNBA 2K21 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K21](https://en.wikipedia.org/wiki/NBA_2K21) | NBA 2K21 | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K:B](https://store.steampowered.com/app/1142100/WWE_2K_BATTLEGROUNDS/) | WWE 2K Battlegrounds | - | - | -- -- -- | -- -- -- | -- -- --
| [M:DE](https://store.steampowered.com/app/1030840/Mafia_Definitive_Edition/) | Mafia: Definitive Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [M2:DE](https://store.steampowered.com/app/1030830/Mafia_II_Definitive_Edition/) | Mafia II: Definitive Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA:SC]() | NBA SuperCard | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K22](https://en.wikipedia.org/wiki/NBA_2K22) | NBA 2K22 | - | - | -- -- -- | -- -- -- | -- -- --
| [M3:DE](https://store.steampowered.com/app/360430/Mafia_III_Definitive_Edition/) | Mafia III: Definitive Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K22](https://en.wikipedia.org/wiki/WWE_2K22) | WWE 2K22 | - | - | -- -- -- | -- -- -- | -- -- --
| [TTW](https://store.steampowered.com/app/1286680/Tiny_Tinas_Wonderlands/) | Tiny Tina's Wonderlands | - | - | -- -- -- | -- -- -- | -- -- --
| [TheQuarry](https://store.steampowered.com/app/1577120/The_Quarry/) | The Quarry | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K23](https://store.steampowered.com/app/1919590/NBA_2K23/) | NBA 2K23 | - | - | -- -- -- | -- -- -- | -- -- --
| [PGATour2K23](https://store.steampowered.com/app/1588010/PGA_TOUR_2K23/) | PGA Tour 2K23 | - | - | -- -- -- | -- -- -- | -- -- --
| [BL:NT](https://store.steampowered.com/app/1454970/New_Tales_from_the_Borderlands/) | New Tales from the Borderlands | - | - | -- -- -- | -- -- -- | -- -- --
| [Marvel:MS](https://store.steampowered.com/app/368260/Marvels_Midnight_Suns/) | Marvel's Midnight Suns | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K23](https://store.steampowered.com/app/1942660/WWE_2K23/) | WWE 2K23 | - | - | -- -- -- | -- -- -- | -- -- --
| [Lego2KDrive](https://store.steampowered.com/app/1451810/LEGO_2K_Drive/) | Lego 2K Drive | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K24](https://store.steampowered.com/app/2338770/NBA_2K24/) | NBA 2K24 | - | - | -- -- -- | -- -- -- | -- -- --
| [WWE2K24](https://store.steampowered.com/app/2315690/WWE_2K24/) | WWE 2K24 | - | - | -- -- -- | -- -- -- | -- -- --
| [TopSpin2K25](https://store.steampowered.com/app/1785650/TopSpin_2K25/) | TopSpin 2K25 | - | - | -- -- -- | -- -- -- | -- -- --
| [NBA2K25](https://store.steampowered.com/app/2878980/NBA_2K25/) | NBA 2K25 | - | - | -- -- -- | -- -- -- | -- -- --
| [M:TOC](https://store.steampowered.com/app/1941540/Mafia_The_Old_Country/) | Mafia: The Old Country | - | - | -- -- -- | -- -- -- | -- -- --
| [UK:3U]() | Untitled (31st Union) | - | - | -- -- -- | -- -- -- | -- -- --
| [UK:BS]() | Untitled (BioShock) | - | - | -- -- -- | -- -- -- | -- -- --
| [UK:NFL2K]() | Untitled NFL 2K | - | - | -- -- -- | -- -- -- | -- -- --
| **Xbox** | **Xbox Game Studios**
| [AxiomVerge](https://store.steampowered.com/app/332200/Axiom_Verge/) | Axiom Verge | - | - | -- -- -- | -- -- -- | -- -- --
| [StardewValley](https://store.steampowered.com/app/413150/Stardew_Valley/) | Stardew Valley | - | - | -- -- -- | -- -- -- | -- -- --
| [Celeste](https://store.steampowered.com/app/504230/Celeste/) | Celeste | - | - | -- -- -- | -- -- -- | -- -- --
| [AxiomVerge2](https://store.steampowered.com/app/946030/Axiom_Verge_2/) | Axiom Verge 2 | - | - | -- -- -- | -- -- -- | -- -- --
