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
| [WB](docs/Families/WB/Readme.md)            | Asheron's Call            | Asheron's Call    | In Development

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
| [D](https://www.gog.com/en/game/dishonored_definitive_edition) | Dishonored | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](https://www.gog.com/index.php/game/dishonored_2) | Dishonored 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [P](https://www.gog.com/en/game/prey) | Prey | open | read | -- -- -- | -- -- -- | -- -- --
| [D:DOTO](https://www.gog.com/en/game/dishonored_death_of_the_outsider) | Dishonored: Death of the Outsider | - | - | -- -- -- | -- -- -- | -- -- --
| [W:YB](https://store.steampowered.com/app/1056960) | Wolfenstein: Youngblood | - | - | -- -- -- | -- -- -- | -- -- --
| [W:CP](https://store.steampowered.com/app/1056970) | Wolfenstein: Cyberpilot | - | - | -- -- -- | -- -- -- | -- -- --
| [DL](https://store.steampowered.com/app/1252330) | Deathloop | - | - | -- -- -- | -- -- -- | -- -- --
| **Beamdog** | **Beamdog**
| **Bethesda** | **Bethesda Game Studios**
| [Morrowind](https://store.steampowered.com/app/22320/The_Elder_Scrolls_III_Morrowind_Game_of_the_Year_Edition/) | The Elder Scrolls III: Morrowind | - | - | -- -- -- | -- -- -- | -- -- --
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
| [Starfield](https://store.steampowered.com/app/1716740/Starfield/) | Starfield | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion:R](https://store.steampowered.com/app/2623190/The_Elder_Scrolls_IV_Oblivion_Remastered/) | The Elder Scrolls IV: Oblivion Remastered | - | - | -- -- -- | -- -- -- | -- -- --
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
| [DA:O](https://store.steampowered.com/app/47810) | Dragon Age: Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [ME2](https://store.steampowered.com/app/24980) | Mass Effect 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DA2](https://en.wikipedia.org/wiki/Dragon_Age_II) | Dragon Age II | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTOR](https://store.steampowered.com/app/1286830) | Star Wars: The Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [ME3](https://store.steampowered.com/app/1238020) | Mass Effect 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:I](https://store.steampowered.com/app/1222690) | Dragon Age: Inquisition | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:A](https://store.steampowered.com/app/1238000) | Mass Effect: Andromeda | - | - | -- -- -- | -- -- -- | -- -- --
| [A](https://www.ea.com/games/anthem/buy/pc) | Anthem | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:LE](https://store.steampowered.com/app/1328670) | Mass Effect: Legendary Edition | - | - | -- -- -- | -- -- -- | -- -- --
| **Black** | **Black Isle Studios**
| [Fallout](https://store.steampowered.com/app/38400) | Fallout | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout2](https://store.steampowered.com/app/38410) | Fallout 2 | - | - | -- -- -- | -- -- -- | -- -- --
| **Blizzard** | **Blizzard Entertainment**
| [SC](https://us.shop.battle.net/en-us/product/starcraft) | StarCraft | - | - | -- -- -- | -- -- -- | -- -- --
| [D2R](https://us.shop.battle.net/en-us/product/diablo_ii_resurrected) | Diablo II: Resurrected | - | - | -- -- -- | -- -- -- | -- -- --
| [W3](https://us.shop.battle.net/en-us/product/warcraft-iii-reforged) | Warcraft III: Reign of Chaos | - | - | -- -- -- | -- -- -- | -- -- --
| [WOW](https://us.shop.battle.net/en-us/family/world-of-warcraft) | World of Warcraft | - | - | -- -- -- | -- -- -- | -- -- --
| [WOWC](https://us.shop.battle.net/en-us/family/world-of-warcraft-classic) | World of Warcraft: Classic | - | - | -- -- -- | -- -- -- | -- -- --
| [SC2](https://us.shop.battle.net/en-us/product/starcraft-ii) | StarCraft II: Wings of Liberty | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://us.shop.battle.net/en-us/product/diablo-iii) | Diablo III | - | - | -- -- -- | -- -- -- | -- -- --
| [HOTS](https://us.shop.battle.net/en-us/family/heroes-of-the-storm) | Heroes of the Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [HS](https://us.shop.battle.net/en-us/family/hearthstone) | Hearthstone | - | - | -- -- -- | -- -- -- | -- -- --
| [CB](https://us.shop.battle.net/en-us/family/crash-bandicoot-4) | Crash Bandicoot™ 4: It's About Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DI](https://diabloimmortal.blizzard.com/en-us/) | Diablo Immortal | - | - | -- -- -- | -- -- -- | -- -- --
| [OW2](https://us.shop.battle.net/en-us/product/overwatch) | Overwatch 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D4](https://diablo4.blizzard.com/en-us/) | Diablo IV | - | - | -- -- -- | -- -- -- | -- -- --
| **Bohemia** | **Bohemia Interactive**
| [FTaFFIaN](https://en.wikipedia.org/wiki/Fairy_Tale_about_Father_Frost,_Ivan_and_Nastya) | Fairy Tale about Father Frost, Ivan and Nastya | - | - | -- -- -- | -- -- -- | -- -- --
| **Bullfrog** | **Bullfrog Productions**
| [P](https://en.wikipedia.org/wiki/Populous_(video_game)) | Populous | - | - | -- -- -- | -- -- -- | -- -- --
| [P2](https://en.wikipedia.org/wiki/Populous_II:_Trials_of_the_Olympian_Gods) | Populous II: Trials of the Olympian Gods | - | - | -- -- -- | -- -- -- | -- -- --
| [S](https://en.wikipedia.org/wiki/Syndicate_(1993_video_game)) | Syndicate | - | - | -- -- -- | -- -- -- | -- -- --
| [MC](https://en.wikipedia.org/wiki/Magic_Carpet_(video_game)) | Magic Carpet | - | - | -- -- -- | -- -- -- | -- -- --
| [TP](https://en.wikipedia.org/wiki/Theme_Park_(video_game)) | Theme Park | - | - | -- -- -- | -- -- -- | -- -- --
| [MC2](https://en.wikipedia.org/wiki/Magic_Carpet_2) | Magic Carpet 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [S2](https://en.wikipedia.org/wiki/Syndicate_Wars) | Syndicate Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [TH](https://en.wikipedia.org/wiki/Theme_Hospital) | Theme Hospital | - | - | -- -- -- | -- -- -- | -- -- --
| [DK](https://en.wikipedia.org/wiki/Dungeon_Keeper) | Dungeon Keeper | - | - | -- -- -- | -- -- -- | -- -- --
| [P3](https://en.wikipedia.org/wiki/Populous:_The_Beginning) | Populous: The Beginning | - | - | -- -- -- | -- -- -- | -- -- --
| [DK2](https://en.wikipedia.org/wiki/Dungeon_Keeper_2) | Dungeon Keeper 2 | - | - | -- -- -- | -- -- -- | -- -- --
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
| [AAI:ME](https://store.steampowered.com/app/2401970/Ace_Attorney_Investigations_Collection/) | Ace Attorney Investigations: Miles Edgeworth | - | - | -- -- -- | -- -- -- | -- -- --
| [AoB](https://www.xbox.com/en-US/games/store/age-of-booty/BZ46NPQNX334/0001) | Age of Booty | - | - | -- -- -- | -- -- -- | -- -- --
| [AJ:AA](https://store.steampowered.com/app/2187220/Apollo_Justice_Ace_Attorney_Trilogy/) | Apollo Justice: Ace Attorney | - | - | -- -- -- | -- -- -- | -- -- --
| [AYSTa5G](https://play.google.com/store/apps/details?id=dh3games.areyousmarterthana5thgrader&hl=en_US) | Are You Smarter Than a 5th Grader? 2009 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7:BH](https://store.steampowered.com/app/418370/Resident_Evil_7_Biohazard/) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:CV](https://www.xbox.com/en-US/games/store/resident-evil-code-veronica-x/BQW8J8XM62JW) | Resident Evil - Code: Veronica | - | - | -- -- -- | -- -- -- | -- -- --
| [BC](https://store.steampowered.com/app/21670/Bionic_Commando/) | Bionic Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [BC:R](https://store.steampowered.com/app/21680/Bionic_Commando_Rearmed/) | Bionic Commando Rearmed | - | - | -- -- -- | -- -- -- | -- -- --
| [BC:R2](https://www.xbox.com/en-US/games/store/bc-rearmed-2/C34MG6M35D3S) | Bionic Commando Rearmed 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BlackCommand](https://black-command.en.uptodown.com/android) | Black Command | - | - | -- -- -- | -- -- -- | -- -- --
| [BoF4](https://www.myabandonware.com/game/breath-of-fire-iv-bgw) | Breath of Fire IV | - | - | -- -- -- | -- -- -- | -- -- --
| [CAS2](https://store.steampowered.com/app/1755910/Capcom_Arcade_2nd_Stadium/) | Capcom Arcade 2nd Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [CAC](https://www.xbox.com/en-US/games/store/capcom-arcade-cabinet/C26KM7D5BG4S/0001) | Capcom Arcade Cabinet | - | - | -- -- -- | -- -- -- | -- -- --
| [BEUB](https://store.steampowered.com/app/885150/Capcom_Beat_Em_Up_Bundle/) | Capcom Beat 'Em Up Bundle | - | - | -- -- -- | -- -- -- | -- -- --
| [DV:Z](https://store.steampowered.com/app/45730/Dark_Void_Zero/) | Dark Void Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [DR](https://store.steampowered.com/app/427190/DEAD_RISING/) | Dead Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2](https://store.steampowered.com/app/45740/Dead_Rising_2/) | Dead Rising 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2:OtR](https://store.steampowered.com/app/45770/Dead_Rising_2_Off_the_Record/) | Dead Rising 2: Off the Record | - | - | -- -- -- | -- -- -- | -- -- --
| [DR3](https://store.steampowered.com/app/265550/Dead_Rising_3_Apocalypse_Edition/) | Dead Rising 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4](https://store.steampowered.com/app/543460/Dead_Rising_4/) | Dead Rising 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4:FBP](https://store.playstation.com/en-us/product/UP0102-CUSA08540_00-DEADRISING4BUNDL) | Dead Rising 4: Frank's Big Package | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC3:S](https://store.steampowered.com/app/6550/Devil_May_Cry_3_Special_Edition/) | Devil May Cry 3: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC4:S](https://store.steampowered.com/app/329050/Devil_May_Cry_4_Special_Edition/) | Devil May Cry 4: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC5](https://store.steampowered.com/app/601150/Devil_May_Cry_5/) | Devil May Cry 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC5:S](https://www.xbox.com/en-US/games/store/devil-may-cry-5-special-edition/9MZ11KT5KLP6/0010) | Devil May Cry 5: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC:HD](https://store.steampowered.com/app/631510/Devil_May_Cry_HD_Collection/) | Devil May Cry: HD Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [DmC:X](https://store.steampowered.com/app/220440/DmC_Devil_May_Cry/) | DmC: Devil May Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [DD](https://store.steampowered.com/app/367500/Dragons_Dogma_Dark_Arisen/) | Dragon's Dogma | - | - | -- -- -- | -- -- -- | -- -- --
| [DD2](https://store.steampowered.com/app/2054970/Dragons_Dogma_2/) | Dragon's Dogma II | - | - | -- -- -- | -- -- -- | -- -- --
| [DT:R](https://store.steampowered.com/app/237630/DuckTales_Remastered/) | DuckTales: Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [DnD:CoM](https://store.steampowered.com/app/229480/Dungeons__Dragons_Chronicles_of_Mystara/) | Dungeons & Dragons: Chronicles of Mystara | - | - | -- -- -- | -- -- -- | -- -- --
| [Flock](https://store.steampowered.com/app/21640/Flock/) | Flock! | - | - | -- -- -- | -- -- -- | -- -- --
| [LP:EC](https://store.steampowered.com/app/6510/Lost_Planet_Extreme_Condition/) | Lost Planet: Extreme Condition | - | - | -- -- -- | -- -- -- | -- -- --
| [LP3](https://store.steampowered.com/app/226720/LOST_PLANET_3/) | Lost Planet 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:I](https://store.steampowered.com/app/493840) | Marvel vs. Capcom: Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| [MM11](https://store.steampowered.com/app/742300) | Mega Man 11 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC](https://store.steampowered.com/app/743890) | Mega Man X Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC2](https://store.steampowered.com/app/743900) | Mega Man X Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:W](https://store.steampowered.com/app/582010) | Monster Hunter: World | - | - | -- -- -- | -- -- -- | -- -- --
| [Okami:HD](https://store.steampowered.com/app/587620) | Ōkami HD | - | - | -- -- -- | -- -- -- | -- -- --
| [O:W](https://store.steampowered.com/app/761600) | Onimusha: Warlords | - | - | -- -- -- | -- -- -- | -- -- --
| [RememberMe](https://store.steampowered.com/app/228300) | Remember Me | - | - | -- -- -- | -- -- -- | -- -- --
| [RE](https://store.steampowered.com/app/304240) | Resident Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2](https://store.steampowered.com/app/883710) | Resident Evil 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE3](https://store.steampowered.com/app/952060) | Resident Evil 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4](https://store.steampowered.com/app/254700) | Resident Evil 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE5](https://store.steampowered.com/app/21690) | Resident Evil 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE6](https://store.steampowered.com/app/221040) | Resident Evil 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7](https://store.steampowered.com/app/418370) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R](https://store.steampowered.com/app/222480) | Resident Evil: Revelations | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R2](https://store.steampowered.com/app/287290) | Resident Evil: Revelations 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:V](https://store.steampowered.com/app/1196590) | Resident Evil Village | - | - | -- -- -- | -- -- -- | -- -- --
| [REZ](https://store.steampowered.com/app/339340) | Resident Evil Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:30AC](https://store.steampowered.com/app/586200) | Street Fighter 30th Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SF5](https://store.steampowered.com/app/310950) | Street Fighter V | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider](https://store.steampowered.com/app/235210) | Strider (2014 video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [UMVC3](https://store.steampowered.com/app/357190) | Ultimate Marvel vs. Capcom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [UmbrellaCorps](https://store.steampowered.com/app/390340) | Umbrella Corps | - | - | -- -- -- | -- -- -- | -- -- --
| **Cig** | **Cloud Imperium Games**
| [StarCitizen](https://en.wikipedia.org/wiki/Star_Citizen) | Star Citizen | - | - | -- -- -- | -- -- -- | -- -- --
| **Cryptic** | **Cryptic**
| [CO](https://store.steampowered.com/app/9880) | Champions Online | open | read | -- -- -- | -- -- -- | -- -- --
| [STO](https://store.steampowered.com/app/9900) | Star Trek Online | open | read | -- -- -- | -- -- -- | -- -- --
| [NVW](https://store.steampowered.com/app/109600) | Neverwinter | open | read | -- -- -- | -- -- -- | -- -- --
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
| [BioShock](https://store.steampowered.com/app/7670) | BioShock | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShockR](https://store.steampowered.com/app/409710) | BioShock Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2](https://store.steampowered.com/app/8850) | BioShock 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2R](https://store.steampowered.com/app/409720) | BioShock 2 Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock:Inf](https://store.steampowered.com/app/8870) | BioShock Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| **Firaxis** | **Firaxis Games**
| **Frictional** | **HPL Engine**
| [P:O](https://store.steampowered.com/app/22180) | Penumbra: Overture | - | - | -- -- -- | -- -- -- | -- -- --
| [P:BP](https://store.steampowered.com/app/22120) | Penumbra: Black Plague | - | - | -- -- -- | -- -- -- | -- -- --
| [P:R](https://store.steampowered.com/app/22140) | Penumbra: Requiem | - | - | -- -- -- | -- -- -- | -- -- --
| [A:TDD](https://store.steampowered.com/app/57300) | Amnesia: The Dark Descent | - | - | -- -- -- | -- -- -- | -- -- --
| [A:AMFP](https://store.steampowered.com/app/239200) | Amnesia: A Machine for Pigs | - | - | -- -- -- | -- -- -- | -- -- --
| [SOMA](https://store.steampowered.com/app/282140) | SOMA | - | - | -- -- -- | -- -- -- | -- -- --
| [A:R](https://store.steampowered.com/app/999220) | Amnesia: Rebirth | - | - | -- -- -- | -- -- -- | -- -- --
| **Frontier** | **Frontier Developments**
| [LW](https://store.steampowered.com/app/447780) | LostWinds | - | - | -- -- -- | -- -- -- | -- -- --
| [LW2](https://store.steampowered.com/app/447800) | LostWinds 2: Winter of the Melodias | - | - | -- -- -- | -- -- -- | -- -- --
| [ED](https://store.steampowered.com/app/359320) | Elite: Dangerous | - | - | -- -- -- | -- -- -- | -- -- --
| [PC](https://store.steampowered.com/app/493340) | Planet Coaster | - | - | -- -- -- | -- -- -- | -- -- --
| [JW](https://store.steampowered.com/app/648350) | Jurassic World Evolution | - | - | -- -- -- | -- -- -- | -- -- --
| [PZ](https://store.steampowered.com/app/703080) | Planet Zoo | - | - | -- -- -- | -- -- -- | -- -- --
| [JW2](https://store.steampowered.com/app/1244460) | Jurassic World Evolution 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:22](https://store.steampowered.com/app/1708520) | F1 Manager 2022 | - | - | -- -- -- | -- -- -- | -- -- --
| [W4K:CG](https://store.steampowered.com/app/1611910) | Warhammer 40,000: Chaos Gate - Daemonhunters | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:23](https://store.steampowered.com/app/2287220) | F1 Manager 2023 | - | - | -- -- -- | -- -- -- | -- -- --
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
| [EdenEternal](https://en.wikipedia.org/wiki/Eden_Eternal) | Eden Eternal | - | - | -- -- -- | -- -- -- | -- -- --
| [ElShaddai:AotM](https://en.wikipedia.org/wiki/El_Shaddai:_Ascension_of_the_Metatron) | El Shaddai: Ascension of the Metatron | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerUpHeroes](https://en.wikipedia.org/wiki/PowerUp_Heroes) | PowerUp Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Ragnarok2:LotS](https://en.wikipedia.org/wiki/Ragnarok_Online_2:_Legend_of_the_Second) | Ragnarok Online II: Legend of the Second | - | - | -- -- -- | -- -- -- | -- -- --
| [Rift](https://en.wikipedia.org/wiki/Rift_(video_game)) | Rift | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith](https://en.wikipedia.org/wiki/Rocksmith) | Rocksmith | - | - | -- -- -- | -- -- -- | -- -- --
| [YarsRevenge](https://en.wikipedia.org/wiki/Yar%27s_Revenge) | Yar's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [Epic Mickey 2: The Power of Two](XXX) | Epic Mickey 2: The Power of Two | - | - | -- -- -- | -- -- -- | -- -- --
| [Pirate101](https://en.wikipedia.org/wiki/Pirate101) | Pirate101 | - | - | -- -- -- | -- -- -- | -- -- --
| [Defiance](https://en.wikipedia.org/wiki/Defiance_(video_game)) | Defiance | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocksmith2014](https://en.wikipedia.org/wiki/Rocksmith_2014) | Rocksmith 2014 | - | - | -- -- -- | -- -- -- | -- -- --
| [GitS:SAC](https://en.wikipedia.org/wiki/Ghost_in_the_Shell:_Stand_Alone_Complex_-_First_Assault_Online) | Ghost in the Shell: Stand Alone Complex - First Assault Online | - | - | -- -- -- | -- -- -- | -- -- --
| [MapleStory2](https://en.wikipedia.org/wiki/MapleStory_2) | MapleStory 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [OblivionR](https://en.wikipedia.org/wiki/The_Elder_Scrolls_IV:_Oblivion_Remastered) | The Elder Scrolls IV: Oblivion Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| **ID** | **ID**
| [Q](https://store.steampowered.com/app/2310) | Quake | - | - | -- -- -- | -- -- -- | -- -- --
| [Q2](https://store.steampowered.com/app/2320) | Quake II | - | - | -- -- -- | -- -- -- | -- -- --
| [Q3](https://store.steampowered.com/app/0) | Quake III Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://store.steampowered.com/app/9050) | Doom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:L](https://store.steampowered.com/app/282440) | Quake Live | - | - | -- -- -- | -- -- -- | -- -- --
| [R](https://store.steampowered.com/app/9200) | Rage | - | - | -- -- -- | -- -- -- | -- -- --
| [D](https://store.steampowered.com/app/0) | Doom (2016) | - | - | -- -- -- | -- -- -- | -- -- --
| [D:VFR](https://store.steampowered.com/app/650000) | Doom VFR | - | - | -- -- -- | -- -- -- | -- -- --
| [R2](https://store.steampowered.com/app/548570) | Rage 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D:E](https://store.steampowered.com/app/0) | Doom Eternal | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:C](https://store.steampowered.com/app/611500) | Quake Champions | - | - | -- -- -- | -- -- -- | -- -- --
| **IW** | **Infinity Ward**
| [COD](https://store.steampowered.com/app/2620) | Call of Duty | - | - | -- -- -- | -- -- -- | -- -- --
| [COD2](https://store.steampowered.com/app/2630) | Call of Duty 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [COD4](https://store.steampowered.com/app/7940) | Call of Duty 4: Modern Warfare | - | - | -- -- -- | -- -- -- | -- -- --
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
| [PP](https://www.myabandonware.com/game/phm-pegasus-hr) | PHM Pegasus | - | - | -- -- -- | -- -- -- | -- -- --
| [MM](https://store.steampowered.com/app/529890) | Maniac Mansion | - | - | -- -- -- | -- -- -- | -- -- --
| [SF](https://www.myabandonware.com/game/strike-fleet-j2) | Strike Fleet | - | - | -- -- -- | -- -- -- | -- -- --
| [B1942](https://www.myabandonware.com/game/battlehawks-1942-eu) | Battlehawks 1942 | - | - | -- -- -- | -- -- -- | -- -- --
| [ZMatAM](https://store.steampowered.com/app/559070) | Zak McKracken and the Alien Mindbenders | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatLC:TAG](https://www.myabandonware.com/game/indiana-jones-and-the-last-crusade-the-action-game-2fd) | Indiana Jones and the Last Crusade: The Action Game | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatLC](https://store.steampowered.com/app/32310) | Indiana Jones and the Last Crusade: The Graphic Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [TFH](https://www.myabandonware.com/game/their-finest-hour-the-battle-of-britain-sb) | Their Finest Hour | - | - | -- -- -- | -- -- -- | -- -- --
| [TFM:V1](https://www.myabandonware.com/game/their-finest-missions-volume-one-7i9) | Their Finest Missions: Volume One | - | - | -- -- -- | -- -- -- | -- -- --
| [L](https://store.steampowered.com/app/32340) | Loom | - | - | -- -- -- | -- -- -- | -- -- --
| [M](https://www.myabandonware.com/game/masterblazer-17d) | Masterblazer | - | - | -- -- -- | -- -- -- | -- -- --
| [NS](https://www.abandonwaredos.com/abandonware-game.php?abandonware=Night+Shift&gid=1553) | Night Shift | - | - | -- -- -- | -- -- -- | -- -- --
| [SWotL](https://www.myabandonware.com/game/secret-weapons-of-the-luftwaffe-1xo) | Secret Weapons of the Luftwaffe | - | - | -- -- -- | -- -- -- | -- -- --
| [MI2:LR](https://store.steampowered.com/app/32460) | Monkey Island 2: LeChuck's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatFoA](https://store.steampowered.com/app/6010) | Indiana Jones and the Fate of Atlantis | - | - | -- -- -- | -- -- -- | -- -- --
| [DotT](https://store.steampowered.com/app/388210) | Day of the Tentacle | - | - | -- -- -- | -- -- -- | -- -- --
| [ZAMN](https://store.steampowered.com/app/1137970) | Zombies Ate My Neighbors | - | - | -- -- -- | -- -- -- | -- -- --
| [SaMHtR](https://store.steampowered.com/app/355170) | Sam & Max Hit the Road | - | - | -- -- -- | -- -- -- | -- -- --
| [SWC](https://www.myabandonware.com/game/the-software-toolworks-star-wars-chess-2dk) | Star Wars Chess | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TF](https://store.steampowered.com/app/355250) | Star Wars: TIE Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [GP](https://store.steampowered.com/app/1137970) | Ghoul Patrol | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:DF](https://store.steampowered.com/app/32400) | Star Wars: Dark Forces | - | - | -- -- -- | -- -- -- | -- -- --
| [FT](https://store.steampowered.com/app/228360) | Full Throttle | - | - | -- -- -- | -- -- -- | -- -- --
| [TD](https://store.steampowered.com/app/6040) | The Dig | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RA2](https://store.steampowered.com/app/456540) | Star Wars: Rebel Assault II: The Hidden Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [IJaHDA](https://www.myabandonware.com/game/indiana-jones-and-his-desktop-adventures-3lf) | Indiana Jones and His Desktop Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [A](https://www.gog.com/en/game/afterlife) | Afterlife | - | - | -- -- -- | -- -- -- | -- -- --
| [MatRotM](https://www.myabandonware.com/game/mortimer-and-the-riddles-of-the-medallion-3na) | Mortimer and the Riddles of the Medallion | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:SotE](https://store.steampowered.com/app/560170) | Star Wars: Shadows of the Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:YS](https://www.myabandonware.com/game/star-wars-yoda-stories-bcn) | Star Wars: Yoda Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [O](https://store.steampowered.com/app/559620) | Outlaws | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:XvT](https://store.steampowered.com/app/361690) | Star Wars: X-Wing vs. TIE Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK:DF2](https://store.steampowered.com/app/32380) | Star Wars Jedi Knight: Dark Forces II | - | - | -- -- -- | -- -- -- | -- -- --
| [MSW](https://www.myabandonware.com/game/star-wars-monopoly-jal) | Monopoly Star Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [TCoMI](https://store.steampowered.com/app/730820) | The Curse of Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK:MotS](https://store.steampowered.com/app/32390) | Star Wars Jedi Knight: Mysteries of the Sith | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:R](https://store.steampowered.com/app/441550) | Star Wars: Rebellion | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:BtM](https://archive.org/details/btm-cd-1) | Star Wars: Behind the Magic | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:DW](https://www.myabandonware.com/game/star-wars-droidworks-3qo) | Star Wars: DroidWorks | - | - | -- -- -- | -- -- -- | -- -- --
| [GF](https://store.steampowered.com/app/316790) | Grim Fandango | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RS](https://store.steampowered.com/app/455910) | Star Wars: Rogue Squadron | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:XA](https://store.steampowered.com/app/361670) | Star Wars: X-Wing Alliance | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:TPM](https://www.myabandonware.com/game/star-wars-episode-i-the-phantom-menace-lv8) | Star Wars Episode I: The Phantom Menace | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:R](https://store.steampowered.com/app/808910) | Star Wars Episode I: Racer | - | - | -- -- -- | -- -- -- | -- -- --
| [SW1:TGF](https://www.myabandonware.com/game/star-wars-episode-i-the-gungan-frontier-3qq) | Star Wars Episode I: The Gungan Frontier | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:YCAC](https://www.myabandonware.com/game/star-wars-yoda-s-challenge-activity-center-3qt) | Star Wars: Yoda's Challenge Activity Center | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:PD](https://www.myabandonware.com/game/star-wars-pit-droids-403) | Star Wars: Pit Droids | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatIM](https://store.steampowered.com/app/904540) | Indiana Jones and the Infernal Machine | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:FC](https://archive.org/details/star-wars-force-commander-windows-10-compatible) | Star Wars: Force Commander | - | - | -- -- -- | -- -- -- | -- -- --
| [EfMI](https://store.steampowered.com/app/730830) | Escape from Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:S](https://store.steampowered.com/app/32350) | Star Wars: Starfighter | - | - | -- -- -- | -- -- -- | -- -- --
| [SWGB](https://store.steampowered.com/app/356500) | Star Wars: Galactic Battlegrounds | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK2:JO](https://store.steampowered.com/app/6030) | Star Wars Jedi Knight II: Jedi Outcast | - | - | -- -- -- | -- -- -- | -- -- --
| [IJatET](https://store.steampowered.com/app/560430) | Indiana Jones and the Emperor's Tomb | - | - | -- -- -- | -- -- -- | -- -- --
| [SWG](https://en.wikipedia.org/wiki/Star_Wars_Galaxies) | Star Wars Galaxies (closed) | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:KotOR](https://store.steampowered.com/app/32370) | Star Wars: Knights of the Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [SWJK:JA](https://store.steampowered.com/app/6020) | Star Wars Jedi Knight: Jedi Academy | - | - | -- -- -- | -- -- -- | -- -- --
| [AaD](https://store.steampowered.com/app/6090) | Armed and Dangerous | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:B](https://store.steampowered.com/app/1237980) | Star Wars: Battlefront | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:KotOR2](https://store.steampowered.com/app/208580) | Star Wars Knights of the Old Republic II: The Sith Lords | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:RC](https://store.steampowered.com/app/6000) | Star Wars: Republic Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:B2](https://store.steampowered.com/app/6060) | Star Wars: Battlefront II | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:EaW](https://store.steampowered.com/app/32470) | Star Wars: Empire at War | - | - | -- -- -- | -- -- -- | -- -- --
| [T:OtR](https://store.steampowered.com/app/6080) | Thrillville: Off the Rails | - | - | -- -- -- | -- -- -- | -- -- --
| [LSW:TCS](https://store.steampowered.com/app/32440) | Lego Star Wars: The Complete Saga | - | - | -- -- -- | -- -- -- | -- -- --
| [LIJ:TOA](https://store.steampowered.com/app/32330) | Lego Indiana Jones: The Original Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TFU](https://store.steampowered.com/app/32430) | Star Wars: The Force Unleashed | - | - | -- -- -- | -- -- -- | -- -- --
| [ToMI](https://store.steampowered.com/app/31170) | Tales of Monkey Island | - | - | -- -- -- | -- -- -- | -- -- --
| [TSoMI:SE](https://store.steampowered.com/app/32360) | The Secret of Monkey Island: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTCW:RH](https://store.steampowered.com/app/32420) | Star Wars: The Clone Wars - Republic Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [LU](https://store.steampowered.com/app/32410) | Lucidity | - | - | -- -- -- | -- -- -- | -- -- --
| [LIJ2:TAC](https://store.steampowered.com/app/32450) | Lego Indiana Jones 2: The Adventure Continues | - | - | -- -- -- | -- -- -- | -- -- --
| [MI2SE:LCR](https://store.steampowered.com/app/32460) | Monkey Island 2 Special Edition: LeChuck's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TFU2](https://store.steampowered.com/app/32500) | Star Wars: The Force Unleashed II | - | - | -- -- -- | -- -- -- | -- -- --
| [LS3:TCW](https://store.steampowered.com/app/32510) | Lego Star Wars III: The Clone Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [SW:TOR](https://store.steampowered.com/app/1286830) | Star Wars: The Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| **Monolith** | **MonolithTech**
| [FEAR](https://store.steampowered.com/app/21090) | F.E.A.R. | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:EP](https://store.steampowered.com/app/21110) | F.E.A.R.: Extraction Point | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:PM](https://store.steampowered.com/app/21120) | F.E.A.R.: Perseus Mandate | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR2](https://store.steampowered.com/app/16450) | F.E.A.R. 2: Project Origin | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR3](https://store.steampowered.com/app/21100) | F.E.A.R. 3 | - | - | -- -- -- | -- -- -- | -- -- --
| **Mythic** | **Mythic Entertainment**
| [RM](https://en.wikipedia.org/wiki/Magestorm) | Rolemaster: Magestorm | - | - | -- -- -- | -- -- -- | -- -- --
| [AO](https://en.wikipedia.org/wiki/Aliens_Online) | Aliens Online | - | - | -- -- -- | -- -- -- | -- -- --
| [GO](https://en.wikipedia.org/wiki/Godzilla_Online) | Godzilla Online | - | - | -- -- -- | -- -- -- | -- -- --
| [DAoC](https://en.wikipedia.org/wiki/Dark_Age_of_Camelot) | Dark Age of Camelot | - | - | -- -- -- | -- -- -- | -- -- --
| [WAR](https://en.wikipedia.org/wiki/Warhammer_Online:_Age_of_Reckoning) | Warhammer Online: Age of Reckoning | - | - | -- -- -- | -- -- -- | -- -- --
| [UO](https://en.wikipedia.org/wiki/Ultima_Online_expansions#Stygian_Abyss) | Ultima Online: Stygian Abyss | - | - | -- -- -- | -- -- -- | -- -- --
| [DA2](https://en.wikipedia.org/wiki/Dragon_Age_II) | Dragon Age II | - | - | -- -- -- | -- -- -- | -- -- --
| **Nintendo** | **Nintendo**
| [Z:TFH](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Tri_Force_Heroes) | The Legend of Zelda: Tri Force Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [AC:AF](https://en.wikipedia.org/wiki/Animal_Crossing:_Amiibo_Festival) | Animal Crossing: Amiibo Festival | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ](https://en.wikipedia.org/wiki/Star_Fox_Zero) | Star Fox Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [SFG](https://en.wikipedia.org/wiki/Star_Fox_Guard) | Star Fox Guard | - | - | -- -- -- | -- -- -- | -- -- --
| [Z:BotW](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Breath_of_the_Wild) | The Legend of Zelda: Breath of the Wild | - | - | -- -- -- | -- -- -- | -- -- --
| [MK8D](https://en.wikipedia.org/wiki/Mario_Kart_8#Mario_Kart_8_Deluxe) | Mario Kart 8 Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| [CaptainToad:TT](https://en.wikipedia.org/wiki/Captain_Toad:_Treasure_Tracker) | Captain Toad: Treasure Tracker | - | - | -- -- -- | -- -- -- | -- -- --
| [NSMB:UD](https://en.wikipedia.org/wiki/New_Super_Mario_Bros._U_Deluxe) | New Super Mario Bros. U Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| [Pikmin3D](https://en.wikipedia.org/wiki/Pikmin_3_Deluxe) | Pikmin 3 Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| **Origin** | **Origin Systems**
| [U8](https://en.wikipedia.org/wiki/Ultima_VIII:_Pagan) | Pagan: Ultima VIII | - | - | -- -- -- | -- -- -- | -- -- --
| [UO](https://en.wikipedia.org/wiki/Ultima_Online) | Ultima Online | - | - | -- -- -- | -- -- -- | -- -- --
| [U9](https://en.wikipedia.org/wiki/Ultima_IX:_Ascension) | Ultima IX: Ascension | - | - | -- -- -- | -- -- -- | -- -- --
| **Red** | **REDengine**
| [Witcher](https://www.gog.com/en/game/the_witcher) | The Witcher Enhanced Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher2](https://www.gog.com/en/game/the_witcher_2) | The Witcher 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher3](https://www.gog.com/en/game/the_witcher_3_wild_hunt) | The Witcher 3: Wild Hunt | - | - | -- -- -- | -- -- -- | -- -- --
| **Rockstar** | **Rockstar Games**
| [GTA](https://www.rockstargames.com/games/gta) | Grand Theft Auto | - | - | -- -- -- | -- -- -- | -- -- --
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
| [RDR](https://store.steampowered.com/app/2668510/Red_Dead_Redemption/) | Red Dead Redemption | - | - | -- -- -- | -- -- -- | -- -- --
| [LAN](https://store.steampowered.com/app/110800/LA_Noire/) | L.A. Noire | - | - | -- -- -- | -- -- -- | -- -- --
| [MP3](https://store.steampowered.com/app/12120/Grand_Theft_Auto_San_Andreas/) | Max Payne 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA5](https://store.steampowered.com/app/271590/Grand_Theft_Auto_V/) | Grand Theft Auto V | - | - | -- -- -- | -- -- -- | -- -- --
| [GTAO](https://www.xbox.com/en-US/games/store/grand-theft-auto-online-xbox-series-xs/9NKC1Z4Z92VN) | Grand Theft Auto Online | - | - | -- -- -- | -- -- -- | -- -- --
| [LAN:VR](https://store.steampowered.com/app/722230/LA_Noire_The_VR_Case_Files/) | L.A. Noire: The VR Case Files | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR2](https://store.steampowered.com/app/1174180/Red_Dead_Redemption_2/) | Red Dead Redemption 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RDO](https://store.steampowered.com/app/1404210/Red_Dead_Online/) | Red Dead Online | - | - | -- -- -- | -- -- -- | -- -- --
| **Ubisoft** | **Ubisoft**
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
| **X2K** | **Rockstar Games**
| **Xbox** | **Xbox Game Studios**
| [AxiomVerge](https://store.steampowered.com/app/332200/Axiom_Verge/) | Axiom Verge | - | - | -- -- -- | -- -- -- | -- -- --
| [StardewValley](https://store.steampowered.com/app/413150/Stardew_Valley/) | Stardew Valley | - | - | -- -- -- | -- -- -- | -- -- --
| [Celeste](https://store.steampowered.com/app/504230/Celeste/) | Celeste | - | - | -- -- -- | -- -- -- | -- -- --
| [AxiomVerge2](https://store.steampowered.com/app/946030/Axiom_Verge_2/) | Axiom Verge 2 | - | - | -- -- -- | -- -- -- | -- -- --
