from __future__ import annotations
import os
from enum import Enum, Flag
from decimal import Decimal
from numpy import ndarray, array, empty
from openstk.core.drawing import Point, Rectangle
from openstk.core.typex import *

# types
# type Vector2 = ndarray
type Vector3 = ndarray
# type Vector4 = ndarray
# type Matrix4x4 = ndarray
# type Quaternion = ndarray

# A character's gender identity.
@RType('StardewValley.Gender')
@RAssembly('StardewValley.GameData')
class Gender(Enum):
    Male = 0
    Female = 1
    Undefined = 2
# A season of the year.
@RType('StardewValley.Season')
class Season(Enum):
    # The spring season.
    Spring = 0
    # The summer season.
    Summer = 1
    # The fall season.
    Fall = 2
    # The winter season.
    Winter = 3

# A data entry which specifies item data to create.
class ISpawnItemData:
    # The item(s) to create. This can be either a qualified item ID, or an item query like <c>ALL_ITEMS</c>.
    itemId: str
    # A list of random item IDs to choose from, using the same format as <see cref='P:StardewValley.GameData.ISpawnItemData.ItemId' />. If set, <see cref='P:StardewValley.GameData.ISpawnItemData.ItemId' /> is ignored.
    randomItemId: list[str]
    # The maximum number of item stacks to produce, or <c>null</c> to include all stacks produced by <see cref='P:StardewValley.GameData.ISpawnItemData.ItemId' /> or <see cref='P:StardewValley.GameData.ISpawnItemData.RandomItemId' />.
    maxItems: int
    # The minimum stack size for the item to create, or <c>-1</c> to keep the default value.
    # A value in the <see cref='P:StardewValley.GameData.ISpawnItemData.MinStack' /> to <see cref='P:StardewValley.GameData.ISpawnItemData.MaxStack' /> range is chosen randomly. If the maximum is lower than the minimum, the stack is set to <see cref='P:StardewValley.GameData.ISpawnItemData.MinStack' />.
    minStack: int
    # The maximum stack size for the item to create, or <c>-1</c> to match <see cref='P:StardewValley.GameData.ISpawnItemData.MinStack' />.
    # <inheritdoc cref='P:StardewValley.GameData.ISpawnItemData.MinStack' select='/Remarks' />
    maxStack: int
    # The quality of the item to create. One of <c>0</c> (normal), <c>1</c> (silver), <c>2</c> (gold), <c>4</c> (iridium), or <c>-1</c> (keep the quality as-is).
    quality: int
    # For objects only, the internal name to set (or <c>null</c> for the item's name in data). This should usually be null.
    objectInternalName: str
    # For objects only, a tokenizable string for the display name to show (or <c>null</c> for the item's default display name). See remarks on <c>Object.displayNameFormat</c>.
    objectDisplayName: str
    # For objects only, a tint color to apply to the sprite. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.
    objectColor: str
    # For tool items only, the initial upgrade level, or <c>-1</c> to keep the default value.
    toolUpgradeLevel: int
    # Whether to add the crafting/cooking recipe for the item, instead of the item itself.
    isRecipe: bool
    # Changes to apply to the result of <see cref='P:StardewValley.GameData.ISpawnItemData.MinStack' /> and <see cref='P:StardewValley.GameData.ISpawnItemData.MaxStack' />.
    stackModifiers: list[QuantityModifier]
    # How multiple <see cref='P:StardewValley.GameData.ISpawnItemData.StackModifiers' /> should be combined.
    stackModifierMode: QuantityModifier.QuantityModifierMode
    # Changes to apply to the <see cref='P:StardewValley.GameData.ISpawnItemData.Quality' />.
    # These operate on the numeric quality values (i.e. <c>0</c> = normal, <c>1</c> = silver, <c>2</c> = gold, and <c>4</c> = iridium). For example, silver × 2 is gold.
    qualityModifiers: list[QuantityModifier]
    # How multiple <see cref='P:StardewValley.GameData.ISpawnItemData.QualityModifiers' /> should be combined.
    qualityModifierMode: QuantityModifier.QuantityModifierMode
    # Custom metadata to add to the created item's <c>modData</c> field for mod use.
    modData: dict[str, str]
    # A game state query which indicates whether an item produced from the other fields should be returned (e.g. to filter results from item queries like <c>ALL_ITEMS</c>). Defaults to always true.
    perItemCondition: str
# An audio change to apply to the game's sound bank.
# This describes an override applied to the sound bank. The override is applied permanently for the current game session, even if it's later removed from the data asset. Overriding a cue will reset all values to the ones specified.
@RType()
class AudioCueData:
    _fields_ = [
        # A unique cue ID, used when playing the sound in-game. The ID should only contain alphanumeric/underscore/dot characters. For custom audio cues, this should be prefixed with your mod ID like <c>Example.ModId_AudioName</c>.
        ('id', 'string'),
        # A list of file paths (not asset names) from which to load the audio. These can be absolute paths or relative to the game's <c>Content</c> folder. Each file can be <c>.ogg</c> or <c>.wav</c>. If you list multiple paths, a random one will be chosen each time it's played.
        ('[Optional] filePaths', 'List<string>'),
        # The audio category, which determines which volume slider in the game options applies. This should be one of <c>Default</c>, <c>Music</c>, <c>Sound</c>, <c>Ambient</c>, or <c>Footsteps</c>. Defaults to <c>Default</c>.
        ('[Optional] category', 'string'),
        # <para>Whether the audio should be streamed from disk when it's played, instead of being loaded into memory ahead of time. This is only possible for Ogg Vorbis (<c>.ogg</c>) files, which otherwise will be decompressed in-memory on load.</para>
        # <para>This is a tradeoff between memory usage and performance, so you should consider which value is best for each audio cue:</para>
        # <list type='bullet'>
        #   <item><description><c>true</c>: Reduces memory usage when the audio cue isn't active, but increases performance impact when it's played. Playing the audio multiple times will multiply the memory and performance impact while they're active, since each play will stream a new instance. Recommended for longer audio cues (like music or ambient noise), or cues that are rarely used in a specific scenario (e.g. a sound that only plays once in an event).</description></item>
        #   <item><description><c>false</c>: Increases memory usage (since it's fully loaded into memory), but reduces performance impact when it's played. It can be played any number of times without affecting memory or performance (it'll just play the cached audio). Recommended for sound effects, or short audio cues that are played occasionally.</description></item>
        # </list>
        ('[Optional] streamedVorbis', 'bool'),
        # Whether the audio cue loops continuously until stopped.
        ('[Optional] looped', 'bool'),
        # Whether to apply a reverb effect to the audio.
        ('[Optional] useReverb', 'bool'),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
    ]
# The data for an item to create, used in data assets like <see cref='T:StardewValley.GameData.Machines.MachineData' /> or <see cref='T:StardewValley.GameData.Shops.ShopData' />.
# @RType()
class GenericSpawnItemData(ISpawnItemData):
    # The backing field for <see cref='P:StardewValley.GameData.GenericSpawnItemData.Id' />.
    _idImpl: str = None
    # An ID for this entry within the current list (not the item itself, which is <see cref='P:StardewValley.GameData.GenericSpawnItemData.ItemId' />). This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.
    @property
    def id(self) -> str: #Optional
        if self._idImpl != None: return self._idImpl
        if self.itemId != None: return self.itemId if not self.isRecipe else self.itemId + ' (Recipe)'
        if ((1 if len(self.randomItemId) > 0 else 0) if self.randomItemId != None else 0) == 0: return '???'
        return '|'.join(self.randomItemId) if not self.isRecipe else '|'.join(self.randomItemId) + ' (Recipe)'
    @id.setter
    def id(self, value: str): self._idImpl = value
    _fields_ = [
        # An ID for this entry within the current list (not the item itself, which is <see cref='P:StardewValley.GameData.GenericSpawnItemData.ItemId' />). This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.
        ('[Optional] #id', 'string'),
        # [inheritdoc]
        ('[Optional] itemId', 'string'),
        # [inheritdoc]
        ('[Optional] randomItemId', 'List<string>'),
        # [inheritdoc]
        ('[Optional] maxItems', 'int?'),
        # [inheritdoc]
        ('[Optional] minStack', 'int', -1),
        # [inheritdoc]
        ('[Optional] maxStack', 'int', -1),
        # [inheritdoc]
        ('[Optional] quality', 'int', -1),
        # [inheritdoc]
        ('[Optional] objectInternalName', 'string'),
        # [inheritdoc]
        ('[Optional] objectDisplayName', 'string'),
        # [inheritdoc]
        ('[Optional] objectColor', 'string'),
        # [inheritdoc]
        ('[Optional] toolUpgradeLevel', 'int', -1),
        # [inheritdoc]
        ('[Optional] isRecipe', 'bool'),
        # [inheritdoc]
        ('[Optional] stackModifiers', 'List<QuantityModifier>'),
        # [inheritdoc]
        ('[Optional] stackModifierMode', 'Enum<QuantityModifier.QuantityModifierMode>'),
        # [inheritdoc]
        ('[Optional] qualityModifiers', 'List<QuantityModifier>'),
        # [inheritdoc]
        ('[Optional] qualityModifierMode', 'Enum<QuantityModifier.QuantityModifierMode>'),
        # [inheritdoc]
        ('[Optional] modData', 'Dictionary<string, string>'),
        # [inheritdoc]
        ('[Optional] perItemCondition', 'string'),
    ]
# The data for an item to create with support for a game state query, used in data assets like <see cref='T:StardewValley.GameData.Machines.MachineData' /> or <see cref='T:StardewValley.GameData.Shops.ShopData' />.
# @RType()
class GenericSpawnItemDataWithCondition(GenericSpawnItemData):
    _fields_ = [
        # A game state query which indicates whether the item should be added. Defaults to always added.
        ('[Optional] condition', 'string'),
    ]
# An incoming phone call that the player can receive when they have a telephone.
class IncomingPhoneCallData:
    _fields_ = [
        # If set, a game state query which indicates whether to trigger this phone call.
        # Whether a player receives this call depends on two fields: <see cref='F:StardewValley.GameData.IncomingPhoneCallData.TriggerCondition' /> is checked on the host player before sending the call to all players, then <see cref='F:StardewValley.GameData.IncomingPhoneCallData.RingCondition' /> is checked on each player to determine whether the phone rings for them.
        ('[Optional] triggerCondition', 'string'),
        # If set, a game state query which indicates whether the phone will ring when this call is received.
        # [inheritdoc]
        ('[Optional] ringCondition', 'string'),
        # The internal name of the NPC making the call. If specified, that NPC's name and portrait will be shown.
        # To show a portrait and NPC name, you must specify either <see cref='F:StardewValley.GameData.IncomingPhoneCallData.FromNpc' /> or <see cref='F:StardewValley.GameData.IncomingPhoneCallData.FromPortrait' />; otherwise a simple dialogue with no portrait/name will be shown.
        ('[Optional] fromNpc', 'string'),
        # If set, overrides the portrait shown based on <see cref='F:StardewValley.GameData.IncomingPhoneCallData.FromNpc' />.
        ('[Optional] fromPortrait', 'string'),
        # If set, overrides the NPC display name shown based on <see cref='F:StardewValley.GameData.IncomingPhoneCallData.FromNpc' />.
        ('[Optional] fromDisplayName', 'string'),
        # A tokenizable string for the call text.
        ('[Optional] dialogue', 'string'),
        # Whether to ignore the base chance of receiving a call for this call.
        ('[Optional] ignoreBaseChance', 'bool'),
        # If set, marks this as a simple dialogue box without an NPC name and portrait, with lines split into multiple boxes by this substring. For example, using <c>#</c> will split <c>Box A#Box B#Box C</c> into three consecutive dialogue boxes.
        # You should leave this null in most cases, and use the regular dialogue format in <see cref='F:StardewValley.GameData.IncomingPhoneCallData.Dialogue' /> to split lines if needed. This is mainly intended to support some older vanilla phone calls.
        ('[Optional] simpleDialogueSplitBy', 'string'),
        # The maximum number of times this phone call can be received, or -1 for no limit.
        ('[Optional] maxCalls', 'int', 1),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
    ]
# The data for a jukebox track.
class JukeboxTrackData:
    _fields_ = [
        # A tokenizable string for the track's display name, or <c>null</c> to use the ID (i.e. cue name).
        ('name', 'string'),
        # Whether this track is available. This can be <c>true</c> (always available), <c>false</c> (never available), or <c>null</c> (available if the player has heard it).
        ('[Optional] available', 'bool?'),
        # A list of alternative track IDs. Any tracks matching one of these IDs will use this entry.
        ('[Optional] alternativeTrackIds', 'List<string>'),
    ]
# An item which is otherwise unobtainable if lost, so it can appear in the crow's lost items shop.
class LostItem:
    _fields_ = [
        # A unique string ID for this entry in this list.
        ('id', 'string'),
        # The qualified item ID to add to the shop.
        ('itemId', 'string'),
        # The mail flag required to add this item.
        # The number added to the shop is the number of players which match this field minus the number of the item which exist in the world. If you specify multiple criteria fields, only one is applied in the order <see cref='F:StardewValley.GameData.LostItem.RequireMailReceived' /> and then <see cref='F:StardewValley.GameData.LostItem.RequireEventSeen' />.
        ('[Optional] requireMailReceived', 'string'),
        # The event ID that must be seen to add this item.
        # <inheritdoc cref='F:StardewValley.GameData.LostItem.RequireMailReceived' path='/remarks' />
        ('[Optional] requireEventSeen', 'string'),
    ]
# The metadata for a mannequin which can be placed in the world and used to store and display clothing.
class MannequinData:
    _fields_ = [
        # A tokenizable string for the item's translated display name.
        ('displayName', 'string'),
        # A tokenizable string for the item's translated description.
        ('description', 'string'),
        # The asset name for the texture containing the item sprite, or <c>null</c> for <c>TileSheets/Mannequins</c>.
        ('texture', 'string'),
        # The asset name for the texture containing the placed world sprite.
        ('farmerTexture', 'string'),
        # The sprite's index in the <see cref='F:StardewValley.GameData.MannequinData.Texture' /> spritesheet.
        ('sheetIndex', 'int'),
        # For clothing with gender variants, whether to display the male (true) or female (false) variant.
        ('displaysClothingAsMale', 'bool', True),
        # Whether to enable rare Easter egg 'cursed' behavior.
        ('[Optional] cursed', 'bool'),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
    ]
# The metadata for a custom farm layout which can be selected by players.
@RType()
class ModFarmType:
    _fields_ = [
        # A key which uniquely identifies this farm type. The ID should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_FarmType.</c>
        ('id', 'string'),
        # Where to get the translatable farm name and description. This must be a key in the form <c>{asset name}:{key}</c>; for example, <c>Strings/UI:Farm_Description</c> will get it from the <c>Farm_Description</c> entry in the <c>Strings/UI</c> file. The translated text must be in the form <c>{name}_{description}</c>.
        ('tooltipStringPath', 'string'),
        # The map asset name relative to the game's <c>Content/Maps</c> folder.
        ('mapName', 'string'),
        # The asset name for a 22x20 pixel icon texture, shown on the 'New Game' and co-op join screens. Defaults to the standard farm type's icon.
        ('[Optional] iconTexture', 'string'),
        # The asset name for a 131x61 pixel texture that's drawn over the farm area in the in-game world map. Defaults to the standard farm type's texture.
        ('[Optional] worldMapTexture', 'string'),
        # Whether monsters should spawn by default on this farm map. This affects the initial value of the advanced option during save creation, which the player can change.
        ('[Optional] spawnMonstersByDefault', 'bool'),
        # Mod-specific metadata for the farm type.
        ('[Optional] modData', 'Dictionary<string, string>'),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
    ]
# The metadata for a custom language which can be selected by players.
@RType()
class ModLanguage:
    _fields_ = [
        # A key which uniquely identifies this language. The ID should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_Language.</c>
        ('id', 'string'),
        # The language code for this localization. This should ideally be an ISO 639-1 code, with only letters and hyphens.
        ('languageCode', 'string'),
        # The asset name for a 174x78 pixel texture containing the button of the language for language selection menu. The top half of the sprite is the default state, while the bottom half is the hover state.
        ('buttonTexture', 'string'),
        # Whether the language uses the game's default fonts. Set to false to enable a custom font via <see cref='F:StardewValley.GameData.ModLanguage.FontFile' /> and <see cref='F:StardewValley.GameData.ModLanguage.FontPixelZoom' />.
        ('useLatinFont', 'bool'),
        # If <see cref='F:StardewValley.GameData.ModLanguage.UseLatinFont' /> is false, the asset name for the custom BitMap font.
        ('[Optional] fontFile', 'string'),
        # If <see cref='F:StardewValley.GameData.ModLanguage.UseLatinFont' /> is false, a factor by which to multiply the font size. The recommended baseline is 1.5, but you can adjust it to make your text smaller or bigger in-game.
        ('[Optional] fontPixelZoom', 'float'),
        # Whether to shift the font up by four pixels (multiplied by the <see cref='F:StardewValley.GameData.ModLanguage.FontPixelZoom' /> if applicable), to better align languages with larger characters like Chinese and Japanese.
        ('[Optional] fontApplyYOffset', 'bool'),
        # The line spacing value used by the game's <c>smallFont</c> font.
        ('[Optional] smallFontLineSpacing', 'int', 26),
        # Whether the social tab and gift log will use gender-specific translations (like the vanilla Portuguese language).
        ('[Optional] useGenderedCharacterTranslations', 'bool'),
        # The string to use as the thousands separator (like <c>','</c> for <c>5,000,000</c>).
        ('[Optional] numberComma', 'string', ','),
        # A string which describes the in-game time format, with tokens replaced by in-game values. For example, <c>[HOURS_12]:[MINUTES] [AM_PM]</c> would show '12:00 PM' at noon.
        # The valid tokens are:
        # <list type='bullet'>
        #   <item><description><c>[HOURS_12]</c>: hours in 12-hour format, where midnight and noon are both '12'.</description></item>
        #   <item><description><c>[HOURS_12_0]</c>: hours in 12-hour format, where midnight and noon are both '0'.</description></item>
        #   <item><description><c>[HOURS_24]</c>: hours in 24-hour format, where midnight is '0' and noon is '12'.</description></item>
        #   <item><description><c>[HOURS_24_0]</c>: hours in 24-hour format with zero-padding, where midnight is '00' and noon is '12'.</description></item>
        #   <item><description><c>[MINUTES]</c>: minutes with zero-padding.</description></item>
        #   <item><description><c>[AM_PM]</c>: the localized text for 'am' or 'pm'. The game shows 'pm' between noon and 11:59pm inclusively, else 'am'.</description></item>
        # </list>
        ('timeFormat', 'string'),
        # A string which describes the in-game time format. Equivalent to <see cref='F:StardewValley.GameData.ModLanguage.TimeFormat' />, but used for the in-game clock.
        ('clockTimeFormat', 'string'),
        # A string which describes the in-game date format as shown in the in-game clock, with tokens replaced by in-game values. For example, <c>[DAY_OF_WEEK] [DAY_OF_MONTH]</c> would show <c>Mon 1</c>. 
        # The valid tokens are:
        # <list type='bullet'>
        #   <item><description><c>[DAY_OF_WEEK]</c>: the translated, abbreviated day of week.</description></item>
        #   <item><description><c>[DAY_OF_MONTH]</c>: the numerical day of the month.</description></item>
        # </list>
        ('clockDateFormat', 'string'),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
    ]
# The metadata for a custom floor or wallpaper item.
@RType()
class ModWallpaperOrFlooring:
    _fields_ = [
        # A key which uniquely identifies this wallpaper or flooring. This should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_WallpaperName</c>.
        ('id', 'string'),
        # The asset name which contains 32x32 pixel (flooring) or 16x48 pixel (wallpaper) sprites. The tilesheet must be 256 pixels wide, but can have any number of flooring/wallpaper rows.
        ('texture', 'string'),
        # Whether this is a flooring tilesheet; else it's a wallpaper tilesheet.
        ('isFlooring', 'bool'),
        # The number of flooring or wallpaper sprites in the tilesheet.
        ('count', 'int'),
    ]
# The data for an Adventurer's Guild monster eradication goal.
class MonsterSlayerQuestData:
    _fields_ = [
        # A tokenizable string for the goal's display name, shown on the board in the Adventurer's Guild.
        ('displayName', 'string'),
        # A list of monster IDs that are counted towards the <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.Count' />.
        ('targets', 'List<string>'),
        # The total number of monsters matching <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.Targets' /> which must be defeated to complete this goal.
        ('count', 'int'),
        # The qualified item ID for the item that can be collected from Gil when this goal is completed. There's no reward item if omitted.
        ('[Optional] rewardItemId', 'string'),
        # The price of the <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardItemId' /> in Marlon's shop, or <c>-1</c> to disable buying it from Marlon.
        ('[Optional] rewardItemPrice', 'int', -1),
        # A tokenizable string for custom dialogue from Gil shown before collecting the rewards, if any.
        # If <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardDialogueFlag' /> isn't set, then this dialogue will be shown each time the reward menus is opened until the player collects the <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardItemId' /> (if any).
        ('[Optional] rewardDialogue', 'string'),
        # A mail flag ID which indicates whether the player has seen the <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardDialogue' />.
        # This doesn't send a letter; see <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail' /> or <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll' /> for that.
        ('[Optional] rewardDialogueFlag', 'string'),
        # The mail flag ID to set for the current player when this goal is completed, if any.
        # This doesn't send a letter; see <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail' /> or <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll' /> for that.
        ('[Optional] rewardFlag', 'string'),
        # The mail flag ID to set for all players when this goal is completed, if any.
        # This doesn't send a letter; see <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail' /> or <see cref='F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll' /> for that.
        ('[Optional] rewardFlagAll', 'string'),
        # The mail letter ID to add to the current player's mailbox tomorrow, if set.
        ('[Optional] rewardMail', 'string'),
        # The mail letter ID to add to all players' mailboxes tomorrow, if set.
        ('[Optional] rewardMailAll', 'string'),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
    ]
class MusicContext(Enum):
    Default = 0
    # Confusingly, <see cref='F:StardewValley.GameData.MusicContext.SubLocation' /> has a higher MusicContext value than <see cref='F:StardewValley.GameData.MusicContext.Default' />, but is used when figuring out what song to play in split-screen.
    # Songs with this value are prioritized above ambient noises, but below other instances' default songs -- so this should be used for things like specialized ambient
    # music.
    SubLocation = 1
    MusicPlayer = 2
    Event = 3
    MiniGame = 4
    ImportantSplitScreenMusic = 5
    MAX = 6
# The metadata for a festival like the Night Market which replaces an in-game location for a period of time, which the player can enter/leave anytime, and which doesn't affect the passage of time.
class PassiveFestivalData:
    _fields_ = [
        # A tokenizable string for the display name shown on the calendar.
        ('displayName', 'string'),
        # A game state query which indicates whether the festival is enabled (subject to the other fields like <see cref='F:StardewValley.GameData.PassiveFestivalData.StartDay' /> and <see cref='F:StardewValley.GameData.PassiveFestivalData.EndDay' />). Defaults to always enabled.
        ('condition', 'string'),
        # Whether the festival appears on the calendar, using the same icon as the Night Market. Default true.
        ('[Optional] showOnCalendar', 'bool', True),
        # The season when the festival becomes active.
        ('season', '#Season'),
        # The day of month when the festival becomes active.
        ('startDay', 'int'),
        # The last day of month when the festival is active.
        ('endDay', 'int'),
        # The time of day when the festival opens each day.
        ('startTime', 'int'),
        # A tokenizable string for the in-game toast notification shown when the festival begins each day.
        ('startMessage', 'string'),
        # If true, the in-game notification for festival start will only play on the first day
        ('[Optional] onlyShowMessageOnFirstDay', 'bool'),
        # The locations to swap for the duration of the festival, where the key is the original location's internal name and the value is the new location's internal name.
        # Despite the field name, this swaps the full locations, not the location's map asset.
        ('[Optional] mapReplacements', 'Dictionary<string, string>'),
        # A C# method which applies custom logic when the day starts.
        # This must be specified in the form <c>{full type name}: {method name}</c>. The method must be static, take zero arguments, and return void.
        ('[Optional] dailySetupMethod', 'string'),
        # A C# method which applies custom logic overnight after the last day of the festival.
        # This must be specified in the form <c>{full type name}: {method name}</c>. The method must be static, take zero arguments, and return void.
        ('[Optional] cleanupMethod', 'string'),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
    ]
# Indicates when a seed/sapling can be planted in a location.
class PlantableResult(Enum):
    # The seed/sapling can be planted if the location normally allows it.
    Default = 0
    # The seed/sapling can be planted here, regardless of whether the location normally allows it.
    Allow = 1
    # The seed/sapling can't be planted here, regardless of whether the location normally allows it.
    Deny = 2
# As part of <see cref='T:StardewValley.GameData.PlantableRule' />, indicates which cases the rule applies to.
class PlantableRuleContext(Flag):
    # This rule applies when planting into the ground.
    Ground = 1
    # This rule applies when planting in a garden pot.
    GardenPot = 2
    # This rule always applies.
    Any = GardenPot | Ground # 0x00000003
# As part of assets like <see cref='T:StardewValley.GameData.Crops.CropData' /> or <see cref='T:StardewValley.GameData.WildTrees.WildTreeData' />, indicates when a seed or sapling can be planted in a location.
class PlantableRule:
    _fields_ = [
        # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries for vanilla items, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.
        ('id', 'string'),
        # A game state query which indicates whether this entry applies. Default true.
        ('[Optional] condition', 'string'),
        # When this rule should be applied.
        # Note that this doesn't allow bypassing built-in restrictions (e.g. trees can't be planted in garden pots regardless of the plantable location rules).
        ('[Optional] plantedIn', 'PlantableRuleContext', PlantableRuleContext.Any),
        # Indicates when the seed or sapling can be planted in a location if this entry is selected.
        ('result', 'PlantableResult'),
        # If this rule prevents planting the seed or sapling, the tokenizable string to show to the player (or <c>null</c> to show a generic message).
        ('[Optional] deniedMessage', 'string'),
    ]
    # Get whether this rule should be applied.
    # <param name='isGardenPot'>Whether the seed or sapling is being planted in a garden pot (else the ground).</param>
    def shouldApplyWhen(self, isGardenPot: bool) -> bool: return PlantableRuleContext(2 if isGardenPot else 1) in self.plantedIn
# As part of another entry like <see cref='T:StardewValley.GameData.Machines.MachineData' /> or <see cref='T:StardewValley.GameData.Shops.ShopData' />, a change to apply to a numeric quantity.
@RType()
class QuantityModifier:
    _fields_ = [
        # An ID for this modifier. This only needs to be unique within the current modifier list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ModifierName</c>.
        ('id', 'string'),
        # A game state query which indicates whether this change should be applied. Item-only tokens are valid for this check, and will check the input (not output) item. Defaults to always true.
        ('[Optional] condition', 'string'),
        # The type of change to apply.
        ('modification', '^ModificationType'),
        # The operand to apply to the target value (e.g. the multiplier if <see cref='F:StardewValley.GameData.QuantityModifier.Modification' /> is set to <see cref='F:StardewValley.GameData.QuantityModifier.ModificationType.Multiply' />).
        ('[Optional] amount', 'float'),
        # A list of random amounts to choose from, using the same format as <see cref='F:StardewValley.GameData.QuantityModifier.Amount' />. If set, <see cref='F:StardewValley.GameData.QuantityModifier.Amount' /> is ignored.
        ('[Optional] randomAmount', 'List<float>'),
    ]
    # Apply the change to a target value.
    # <param name='value'>The current target value.</param>
    # <param name='modification'>The type of change to apply.</param>
    # <param name='amount'>The operand to apply to the target value (e.g. the multiplier if <paramref name='modification' /> is set to <see cref='F:StardewValley.GameData.QuantityModifier.ModificationType.Multiply' />).</param>
    @staticmethod
    def apply(value: float, modification: ModificationType, amount: float) -> float:
        match modification:
            case ModificationType.Add: return value + amount
            case ModificationType.Subtract: return value - amount
            case ModificationType.Multiply: return value * amount
            case ModificationType.Divide: return value / amount
            case ModificationType.Set: return amount
            case _: return value,
    # The type of change to apply for a <see cref='T:StardewValley.GameData.QuantityModifier' />.
    @RType()
    class ModificationType(Enum):
        # Add a number to the current value.
        Add = 0
        # Subtract a number from the current value.
        Subtract = 1
        # Multiply the current value by a number.
        Multiply = 2
        # Divide the current value by a number.
        Divide = 3
        # Overwrite the current value with a number.
        Set = 4
    # Indicates how multiple quantity modifiers are combined.
    @RType()
    class QuantityModifierMode(Enum):
        # Apply each modifier to the result of the previous one. For example, two modifiers which double a value will quadruple it.
        Stack = 0
        # Apply the modifier which results in the lowest value.
        Minimum = 1
        # Apply the modifier which results in the highest value.
        Maximum = 2
# As part of <see cref='T:StardewValley.GameData.FarmAnimals.FarmAnimalData' /> or <see cref='T:StardewValley.GameData.Machines.MachineData' />, a game state counter to increment.
@RType()
class StatIncrement:
    # The backing field for <see cref='P:StardewValley.GameData.StatIncrement.Id' />.
    _idImpl: str = None
    # A unique string ID for this entry within the current animal's list.
    @property
    def id(self) -> str: return self._idImpl or self.statName
    @id.setter
    def id(self, value: str): self._idImpl = value
    _fields_ = [
        # A unique string ID for this entry within the current animal's list.
        ('[Optional] #id', 'string'),
        # The qualified or unqualified item ID for the item to match.
        # You can specify any combination of <see cref='P:StardewValley.GameData.StatIncrement.RequiredItemId' /> and <see cref='P:StardewValley.GameData.StatIncrement.RequiredTags' />. The input item must match all specified fields; if none are specified, this conversion will always match.
        ('[Optional] requiredItemId', 'string'),
        # A comma-delimited list of context tags required on the main input item. The stat is only incremented if the item has all of these. You can negate a tag with <c>!</c> (like <c>bone_item,!fossil_item</c> for bone items that aren't fossils). Defaults to always enabled.
        # [inheritdoc cref='P:StardewValley.GameData.StatIncrement.RequiredItemId' select='Remarks']
        ('[Optional] requiredTags', 'List<string>'),
        # The name of the stat counter field on <c>Game1.stats</c>.
        ('statName', 'string'),
    ]
# A cosmetic sprite to show temporarily, with optional effects and animation.
class TemporaryAnimatedSpriteDefinition:
    _fields_ = [
        # The unique string ID for this entry in the list.
        ('id', 'string'),
        # A game state query which indicates whether to add this temporary sprite.
        ('[Optional] condition', 'string'),
        # The asset name for the texture under the game's <c>Content</c> folder for the animated sprite.
        ('texture', 'string'),
        # The pixel area for the first animated frame within the <see cref='F:StardewValley.GameData.TemporaryAnimatedSpriteDefinition.Texture' />.
        ('sourceRect', 'Rectangle'),
        # The millisecond duration for each frame in the animation.
        ('[Optional] interval', 'float', 100.),
        # The number of frames in the animation.
        ('[Optional] frames', 'int', 1),
        # The number of times to repeat the animation.
        ('[Optional] loops', 'int'),
        # A pixel offset applied to the sprite, relative to the top-left corner of the machine's collision box.
        ('[Optional] positionOffset', 'Vector2', array([0., 0.])),
        ('[Optional] flicker', 'bool'),
        # Whether to flip the sprite horizontally when it's drawn.
        ('[Optional] flip', 'bool'),
        # The tile Y position to use in the layer depth calculation, which affects which sprite is drawn on top if two sprites overlap.
        ('[Optional] sortOffset', 'float'),
        ('[Optional] alphaFade', 'float'),
        # A multiplier applied to the sprite size (in addition to the normal 4× pixel zoom).
        ('[Optional] scale', 'float', 1.),
        ('[Optional] scaleChange', 'float'),
        # The rotation to apply to the sprite when drawn, measured in radians.
        ('[Optional] rotation', 'float'),
        ('[Optional] rotationChange', 'float'),
        # A tint color to apply to the sprite. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.
        ('[Optional] color', 'string'),
    ]
# An action that's performed when a trigger is called and its conditions are met.
class TriggerActionData:
    _fields_ = [
        # A unique string ID for this action in the global list.
        ('id', 'string'),
        # When the action should be checked. This must be a space-delimited list of registered trigger types like <c>DayStarted</c>.
        ('trigger', 'string'),
        # If set, a game state query which indicates whether the action should run when the trigger runs.
        ('[Optional] condition', 'string'),
        # If set, a game state query which indicates that the action should be marked applied when this condition matches. This happens before <see cref='F:StardewValley.GameData.TriggerActionData.Condition' />, <see cref='F:StardewValley.GameData.TriggerActionData.Action' />, and <see cref='F:StardewValley.GameData.TriggerActionData.Actions' /> are applied.
        # This allows optimizing cases where the action will never be applied, to avoid parsing the <see cref='F:StardewValley.GameData.TriggerActionData.Condition' /> each time.
        ('[Optional] skipPermanentlyCondition', 'string'),
        # Whether to only run this action for the main player.
        ('[Optional] hostOnly', 'bool'),
        # The single action to perform.
        # <see cref='F:StardewValley.GameData.TriggerActionData.Action' /> and <see cref='F:StardewValley.GameData.TriggerActionData.Actions' /> can technically be used together, but generally you should pick one or the other.
        ('[Optional] action', 'string'),
        # The actions to perform.
        # [inheritdoc cref='F:StardewValley.GameData.TriggerActionData.Action' path='/remarks']
        ('[Optional] actions', 'List<string>'),
        # Custom fields ignored by the base game, for use by mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
        # Whether to mark the action applied when it's applied. If false, the action can repeat immediately when the same trigger is raised, and queries like <c>PLAYER_HAS_RUN_TRIGGER_ACTION</c> will return false for it.
        ('[Optional] markActionApplied', 'bool', True),
    ]
# The data for a trinket item.
class TrinketData:
    _fields_ = [
        # A tokenizable string for the item display name.
        ('displayName', 'string'),
        # A tokenizable string for the item description.
        ('description', 'string'),
        # The asset name for the texture containing the item sprite. This should contain a grid of 16x16 sprites.
        ('texture', 'string'),
        # The sprite index for this trinket within the <see cref='F:StardewValley.GameData.TrinketData.Texture' />.
        ('sheetIndex', 'int'),
        # The full name of the C# <c>TrinketEffect</c> subclass which implements the trinket behavior. This can safely be a mod class.
        ('trinketEffectClass', 'string'),
        # Whether this trinket can be spawned randomly (e.g. in mine treasure chests).
        ('[Optional] dropsNaturally', 'bool', True),
        # Whether players can re-roll this trinket's stats using an anvil.
        ('[Optional] canBeReforged', 'bool', True),
        # Custom fields which may be used by the <see cref='F:StardewValley.GameData.TrinketData.TrinketEffectClass' /> or mods.
        ('[Optional] customFields', 'Dictionary<string, string>'),
        # A lookup of arbitrary <c>modData</c> values to attach to the trinket when it's constructed.
        ('[Optional] modData', 'Dictionary<string, string>'),
    ]

class BigCraftables:
    @RType()
    class BigCraftableData:
        _fields_ = [
            # The internal item name.
            ('name', 'string'),
            # A tokenizable string for the item's translated display name.
            ('displayName', 'string'),
            # A tokenizable string for the item's translated description.
            ('description', 'string'),
            # The price when sold by the player. This is not the price when bought from a shop.
            ('[Optional] price', 'int'),
            # How the item can be picked up. The possible values are 0 (pick up with any tool), 1 (destroyed if hit with an axe/hoe/pickaxe, or picked up with any other tool), or 2 (can't be removed once placed).
            ('[Optional] fragility', 'int'),
            # Whether the item can be placed outdoors.
            ('[Optional] canBePlacedOutdoors', 'bool', True),
            # Whether the item can be placed indoors.
            ('[Optional] canBePlacedIndoors', 'bool', True),
            # Whether this is a lamp and should produce light when dark.
            ('[Optional] isLamp', 'bool'),
            # The asset name for the texture containing the item's sprite, or <c>null</c> for <c>TileSheets/Craftables</c>.
            ('[Optional] texture', 'string'),
            # The sprite's index in the spritesheet.
            ('spriteIndex', 'int'),
            # The custom context tags to add for this item (in addition to the tags added automatically based on the other object data).
            ('[Optional] contextTags', 'List<string>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]

class Buffs:
    # As part of <see cref='T:StardewValley.GameData.Buffs.BuffData' /> or <see cref='T:StardewValley.GameData.Objects.ObjectBuffData' />, the attribute values to add to the player's stats.
    @RType()
    class BuffAttributesData:
        _fields_ = [
            # The buff to the player's combat skill level.
            ('[Optional] combatLevel', 'float'),
            # The buff to the player's farming skill level.
            ('[Optional] farmingLevel', 'float'),
            # The buff to the player's fishing skill level.
            ('[Optional] fishingLevel', 'float'),
            # The buff to the player's mining skill level.
            ('[Optional] miningLevel', 'float'),
            # The buff to the player's luck skill level.
            ('[Optional] luckLevel', 'float'),
            # The buff to the player's foraging skill level.
            ('[Optional] foragingLevel', 'float'),
            # The buff to the player's max stamina.
            ('[Optional] maxStamina', 'float'),
            # The buff to the player's magnetic radius.
            ('[Optional] magneticRadius', 'float'),
            # The buff to the player's walk speed.
            ('[Optional] speed', 'float'),
            # The buff to the player's defense.
            ('[Optional] defense', 'float'),
            # The buff to the player's attack power.
            ('[Optional] attack', 'float'),
            # The combined multiplier applied to the player's attack power.
            ('[Optional] attackMultiplier', 'float'),
            # The combined buff to the player's resistance to negative effects.
            ('[Optional] immunity', 'float'),
            # The combined multiplier applied to monster knockback when hit by the player's weapon.
            ('[Optional] knockbackMultiplier', 'float'),
            # The combined multiplier applied to the player's weapon swing speed.
            ('[Optional] weaponSpeedMultiplier', 'float'),
            # The combined multiplier applied to the player's critical hit chance.
            ('[Optional] criticalChanceMultiplier', 'float'),
            # The combined multiplier applied to the player's critical hit damage.
            ('[Optional] criticalPowerMultiplier', 'float'),
            # The combined multiplier applied to the player's weapon accuracy.
            ('[Optional] weaponPrecisionMultiplier', 'float'),
        ]
    # A predefined buff which can be applied in-game.
    @RType()
    class BuffData:
        _fields_ = [
            # A tokenizable string for the translated buff name.
            ('displayName', 'string'),
            # A tokenizable string for the translated buff description.
            ('[Optional] description', 'string'),
            # Whether this buff counts as a debuff, so its duration should be halved when wearing a Sturdy Ring.
            ('[Optional] isDebuff', 'bool'),
            # The glow color to apply to the player, if any.
            ('[Optional] glowColor', 'string'),
            # The buff duration in milliseconds, or <c>-2</c> for a buff that should last all day.
            ('duration', 'int'),
            # The maximum buff duration in milliseconds. If specified and larger than <see cref='F:StardewValley.GameData.Buffs.BuffData.Duration' />, a random value between <see cref='F:StardewValley.GameData.Buffs.BuffData.Duration' /> and <see cref='F:StardewValley.GameData.Buffs.BuffData.MaxDuration' /> will be selected for each buff.
            ('[Optional] maxDuration', 'int', -1),
            # The texture to load for the buff icon.
            ('iconTexture', 'string'),
            # The sprite index for the buff icon within the <see cref='F:StardewValley.GameData.Buffs.BuffData.IconTexture' />.
            ('iconSpriteIndex', 'int'),
            # The custom buff attributes to apply, if any.
            ('[Optional] effects', 'StardewValley.GameData.Buffs.BuffAttributesData'),
            # The trigger actions to run when the buff is applied to the player.
            ('[Optional] actionsOnApply', 'List<string>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]

class Buildings:
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, a tile which the player can click to trigger an <c>Action</c> map tile property.
    @RType()
    class BuildingActionTile:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.
            ('id', 'string'),
            # The tile position, relative to the building's top-left corner tile.
            ('tile', 'Point'),
            # The tokenizable string for the action to perform, excluding the <c>Action</c> prefix. For example, <c>'Dialogue Hi there @!'</c> to show a message box like <c>'Hi there {player name}!'</c>.
            ('action', 'string'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, an input/output inventory that can be accessed from a tile on the building exterior.
    @RType()
    class BuildingChest:
        _fields_ = [
            # A key for this chest, referenced from the <see cref='F:StardewValley.GameData.Buildings.BuildingData.ItemConversions' /> field. Each chest must have a unique name within one building's chest list (but they don't need to be globally unique).
            ('id', 'string'),
            # The inventory type.
            ('type', '#BuildingChestType'),
            # The sound to play once when the player clicks the chest.
            ('[Optional] sound', 'string'),
            # A tokenizable string to show when the player tries to add an item to the chest when it isn't a supported item.
            ('[Optional] invalidItemMessage', 'string'),
            # An extra condition that must be met before <see cref='F:StardewValley.GameData.Buildings.BuildingChest.InvalidItemMessage' /> is shown.
            ('[Optional] invalidItemMessageCondition', 'string'),
            # A tokenizable string to show when the player tries to add an item to the chest when they don't have enough in their inventory.
            ('[Optional] invalidCountMessage', 'string'),
            # A tokenizable string to show when the player tries to add an item to the chest when the chest has no more room to accept it.
            ('[Optional] chestFullMessage', 'string'),
            # The chest's position on the building exterior, measured in tiles from the top-left corner of the building. This affects the position of the 'item ready to collect' bubble. If omitted, the bubble is disabled.
            ('[Optional] displayTile', 'Vector2', array([-1., -1.])),
            # If <see cref='F:StardewValley.GameData.Buildings.BuildingChest.DisplayTile' /> is set, the chest's tile height.
            ('[Optional] displayHeigh', 'float'),
        ]
    # The inventory type for a building chest.
    @RType()
    class BuildingChestType(Enum):
        # A normal chest which can both provide output and accept input.
        Chest = 0
        # Provides items for the player to collect. Clicking the tile will do nothing (if empty), grab the item directly (if it only contains one item), else show a grab-only inventory UI.
        Collect = 1
        # Lets the player add items for the building to process.
        Load = 2
    # The data for a building which can be constructed by players.
    @RType()
    class BuildingData:
        _fields_ = [
            # A tokenizable string for the display name (e.g. shown in the construction menu).
            ('name', 'string'),
            # If set, a tokenizable string for the display name which represents the general building type, like 'Coop' for a Deluxe Coop. If omitted, this defaults to the <see cref='F:StardewValley.GameData.Buildings.BuildingData.Name' />.
            ('[Optional] nameForGeneralType', 'string'),
            # A tokenizable string for the description (e.g. shown in the construction menu).
            ('description', 'string'),
            # The asset name for the texture under the game's <c>Content</c> folder.
            ('texture', 'string'),
            # The appearances which can be selected from the construction menu (like stone vs plank cabins), if any, in addition to the default appearance based on <see cref='F:StardewValley.GameData.Buildings.BuildingData.Texture' />.
            ('[Optional] skins', 'List<#BuildingSkin>', []),
            # Whether to draw an automatic shadow along the bottom edge of the building's sprite.
            ('[Optional] drawShadow', 'bool',  True),
            # The tile position relative to the top-left corner of the building where the upgrade sign will be placed when Robin is building an upgrade. Defaults to approximately (5, 1) if the building interior type is a shed, else (0, 0).
            ('[Optional] upgradeSignTile', 'Vector2', array([-1., -1.])),
            # The pixel height of the upgrade sign when Robin is building an upgrade.
            ('[Optional] upgradeSignHeight', 'float'),
            # The building's width and height when constructed, measured in tiles.
            ('[Optional] size', 'Point', array([1, 1])),
            # Whether the building should become semi-transparent when the player is behind it.
            ('[Optional] fadeWhenBehind', 'bool', True),
            # If set, the building's pixel area within the <see cref='F:StardewValley.GameData.Buildings.BuildingData.Texture' />. Defaults to the entire texture.
            ('[Optional] sourceRect', 'Rectangle', Rectangle.empty),
            # A pixel offset to apply each season. This is applied to the <see cref='F:StardewValley.GameData.Buildings.BuildingData.SourceRect' /> position by multiplying the offset by 0 (spring), 1 (summer), 2 (fall), or 3 (winter). Default 0, so all seasons use the same source rect.
            ('[Optional] seasonOffset', 'Point', Point.empty),
            # A pixel offset applied to the building sprite's placement in the world.
            ('[Optional] drawOffset', 'Vector2', empty(2)),
            # A Y tile offset applied when figuring out render layering. For example, a value of 2.5 will treat the building as if it was 2.5 tiles further up the screen for the purposes of layering.
            ('[Optional] sortTileOffset', 'float'),
            #   If set, an ASCII text block which indicates which of the building's tiles the players can walk onto, where each character can be <c>X</c> (blocked) or <c>O</c> (passable). Defaults to all tiles blocked. For example, a stable covers a 4x2 tile area with the front two tiles passable:
            #   <code>
            #     XXXX
            #     XOOX
            #   </code>
            ('[Optional] collisionMap', 'string'),
            # The extra tiles to treat as part of the building when placing it through a construction menu, if any. For example, the farmhouse uses this to make sure the stairs are clear.
            ('[Optional] additionalPlacementTiles', 'List<#BuildingPlacementTile>'),
            # If set, the full name of the C# type to instantiate for the building instance. Defaults to a generic <c>StardewValley.Building</c> instance. Note that using a non-vanilla building type will cause a crash when trying to write the building to the save file.
            ('[Optional] buildingType', 'string'),
            # The NPC from whom you can request construction. The vanilla values are <c>Robin</c> and <c>Wizard</c>, but you can specify a different name if a mod opens a construction menu for them. Defaults to <c>Robin</c>. If omitted, it won't appear in any menu.
            ('[Optional] builder', 'string', 'Robin'),
            # If set, a game state query which indicates whether the building should be available in the construction menu. Defaults to always available.
            ('[Optional] buildCondition', 'string'),
            # The number of days needed to complete construction (e.g. 1 for a building completed the next day). If set to 0, construction finishes instantly.
            ('[Optional] buildDays', 'int'),
            # The gold cost to construct the building.
            ('[Optional] buildCost', 'int'),
            # The materials you must provide to start construction.
            ('[Optional] buildMaterials', 'List<#BuildingMaterial>'),
            # The ID of the building for which this is an upgrade, or omit to allow constructing it as a new building. For example, the Big Coop sets this to 'Coop'. Any numbers of buildings can be an upgrade for the same building, in which case the player can choose one upgrade path.
            ('[Optional] buildingToUpgrade', 'string'),
            # Whether the building is magical. This changes the carpenter menu to a mystic theme while this building's blueprint is selected, and completes the construction instantly when placed.
            ('[Optional] magicalConstruction', 'bool'),
            # A pixel offset to apply to the building sprite when drawn in the construction menu.
            ('[Optional] buildMenuDrawOffset', 'Point', Point.empty),
            # The position of the door that can be clicked to warp into the building interior. This is measured in tiles relative to the top-left corner tile. Defaults to disabled.
            ('[Optional] humanDoor', 'Point', array([-1, -1])),
            # If set, the position and size of the door that animals use to enter/exit the building, if the building interior is an animal location. This is measured in tiles relative to the top-left corner tile. Defaults to disabled.
            ('[Optional] animalDoor', 'Rectangle', array([-1, -1, 0, 0])),
            # The duration of the open animation for the <see cref='F:StardewValley.GameData.Buildings.BuildingData.AnimalDoor' />, measured in milliseconds. If omitted, the door switches to the open state instantly.
            ('[Optional] animalDoorOpenDuration', 'float'),
            # If set, the sound which is played once each time the animal door is opened. Disabled by default.
            ('[Optional] animalDoorOpenSound', 'string'),
            # The duration of the close animation for the <see cref='F:StardewValley.GameData.Buildings.BuildingData.AnimalDoor' />, measured in milliseconds. If omitted, the door switches to the closed state instantly.
            ('[Optional] animalDoorCloseDuration', 'float'),
            # If set, the sound which is played once each time the animal door is closed. Disabled by default.
            ('[Optional] animalDoorCloseSound', 'string'),
            # If set, the name of the existing global location to treat as the building's interior, like <c>FarmHouse</c> and <c>Greenhouse</c> for their respective buildings. If omitted, each building will have its own location instance.
            #   <para>Each location can only be used by one building. If the location is already in use (e.g. because the player has two of this building), each subsequent building will use the <see cref='F:StardewValley.GameData.Buildings.BuildingData.IndoorMap' /> and <see cref='F:StardewValley.GameData.Buildings.BuildingData.IndoorMapType' /> instead. For example, the first greenhouse will use the global <c>Greenhouse</c> location, and any subsequent greenhouse will use a separate instanced location.</para>
            #   <para>The non-instanced location must already be in <c>Game1.locations</c>.</para>
            ('[Optional] nonInstancedIndoorLocation', 'string'),
            # The name of the map asset under <c>Content/Maps</c> to load for the building interior (like <c>'Shed'</c> for the <c>Content/Maps/Shed</c> map).
            ('[Optional] indoorMap', 'string'),
            # If set, the full name of the C# <c>GameLocation</c> subclass which will manage the building's interior location. This must be one of the vanilla types to avoid a crash when saving. Defaults to a generic <c>GameLocation</c>.
            ('[Optional] indoorMapType', 'string'),
            # The maximum number of animals who can live in this building.
            ('[Optional] maxOccupants', 'int', 20),
            # A list of building IDs whose animals to allow in this building too. For example, <c>[ 'Barn', 'Coop' ]</c> will allow barn and coop animals in this building. Default none.
            ('[Optional] validOccupantTypes', 'List<string>', []),
            # Whether animals can get pregnant and produce offspring in this building.
            ('[Optional] allowAnimalPregnancy', 'bool'),
            # When applied as an upgrade to an existing building, the placed items in its interior to move when transitioning to the new map.
            ('[Optional] indoorItemMoves', 'List<#IndoorItemMove>'),
            # The items to place in the building interior when it's constructed or upgraded.
            ('[Optional] indoorItems', 'List<#IndoorItemAdd>'),
            # A list of mail IDs to send to all players when the building is constructed for the first time.
            ('[Optional] addMailOnBuild', 'List<string>'),
            # A list of custom properties applied to the building, which can optionally be overridden per-skin in the <see cref='F:StardewValley.GameData.Buildings.BuildingData.Skins' /> field.
            ('[Optional] metadata', 'Dictionary<string, string>', []),
            # A lookup of arbitrary <c>modData</c> values to attach to the building when it's constructed.
            ('[Optional] modData', 'Dictionary<string, string>', []),
            # The amount of hay that can be stored in this building. If built on the farm, this works just like silos and contributes to the farm's available hay.
            ('[Optional] hayCapacity', 'int'),
            # The input/output inventories that can be accessed from a tile on the building exterior. The allowed items are defined by the <see cref='F:StardewValley.GameData.Buildings.BuildingData.ItemConversions' /> field.
            ('[Optional] chests', 'List<#BuildingChest>'),
            # The default tile action if the clicked tile isn't in <see cref='F:StardewValley.GameData.Buildings.BuildingData.ActionTiles' />.
            ('[Optional] defaultAction', 'string'),
            # The number of extra tiles around the building for which it may add tile properties via <see cref='F:StardewValley.GameData.Buildings.BuildingData.TileProperties' />, but without hiding tile properties from the underlying ground that aren't overwritten by the building data.
            ('[Optional] additionalTilePropertyRadius', 'int'),
            # If true, terrain feature flooring can be placed underneath, and when the building is placed, it will not destroy flooring beneath it.
            ('[Optional] allowsFlooringUnderneath', 'bool', True),
            # A list of tiles which the player can click to trigger an <c>Action</c> map tile property.
            ('[Optional] actionTiles', 'List<#BuildingActionTile>', []),
            # The map tile properties to set.
            ('[Optional] tileProperties', 'List<#BuildingTileProperty>', []),
            # The output items produced when an input item is converted.
            ('[Optional] itemConversions', 'List<#BuildingItemConversion>'),
            # A list of textures to draw over or behind the building, with support for conditions and animations.
            ('[Optional] drawLayers', 'List<#BuildingDrawLayer>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
            # A cached representation of <see cref='F:StardewValley.GameData.Buildings.BuildingData.ActionTiles' />.
            ('_actionTiles', 'Dictionary<Point, string>'),
            # A cached representation of <see cref='F:StardewValley.GameData.Buildings.BuildingData.CollisionMap' />.
            ('_collisionMap', 'Dictionary<Point, bool>'),
            # A cached representation of <see cref='F:StardewValley.GameData.Buildings.BuildingData.TileProperties' />.
            ('_tileProperties', 'Dictionary<string, Dictionary<Point, Dictionary<string, string>>>'),
        ]
        # Get whether a tile is passable based on the <see cref='F:StardewValley.GameData.Buildings.BuildingData.CollisionMap' />.
        # <param name='relativeX'>The tile X position relative to the top-left corner of the building.</param>
        # <param name='relativeY'>The tile Y position relative to the top-left corner of the building.</param>
        def isTilePassable(self, relativeX: int, relativeY: int) -> bool:
            if self.collisionMap == None: return relativeX < 0 or relativeX >= self.size.X or relativeY < 0 or relativeY >= self.size.Y
            key = Point(relativeX, relativeY)
            if self._collisionMap == None:
                self._collisionMap = []
                if self.collisionMap != None:
                    strArray = self.collisionMap.strip().split('\n')
                    for y in range(len(strArray)):
                        str = strArray[y].strip()
                        for index in range(len(str)):
                            self._collisionMap[Point(index, y)] = str[index] == 'X'
            return key not in self._collisionMap or not self._collisionMap[key]
        # Get the action to add at a given position based on <see cref='F:StardewValley.GameData.Buildings.BuildingData.ActionTiles' />.
        # <param name='relativeX'>The tile X position relative to the top-left corner of the building.</param>
        # <param name='relativeY'>The tile Y position relative to the top-left corner of the building.</param>
        def getActionAtTile(self, relativeX: int, relativeY: int) -> str:
            key = Point(relativeX, relativeY)
            if self._actionTiles == None:
                self._actionTiles = []
                for actionTile in self.actionTiles: self._actionTiles[actionTile.tile] = actionTile.action
            if key in self._actionTiles:
                defaultAction = self._actionTiles[key]
                if relativeX < 0 or relativeX >= self.size.x or relativeY < 0 or relativeY >= self.size.y: return None
                defaultAction = self.defaultAction
            return defaultAction
        # Get whether a tile property should be added based on <see cref='F:StardewValley.GameData.Buildings.BuildingData.TileProperties' />.
        # <param name='relativeX'>The tile X position relative to the top-left corner of the building's bounding box.</param>
        # <param name='relativeY'>The tile Y position relative to the top-left corner of the building's bounding box.</param>
        # <param name='propertyName'>The property name to check.</param>
        # <param name='layerName'>The layer name to check.</param>
        # <param name='propertyValue'>The property value that should be set.</param>
        def hasPropertyAtTile(self, relativeX: int, relativeY: int, propertyName: str, layerName: str, propertyValue: str) -> bool:
            # if self._tileProperties == None:
            #     self._tileProperties = []
            #     for tileProperty in self.tileProperties:
            #         if (!_tileProperties.TryGetValue(tileProperty.Layer, out var dictionary1)) _tileProperties[tileProperty.Layer] = dictionary1 = []', 'string'),
            #         for (var y = tileProperty.TileArea.Y; y < tileProperty.TileArea.Bottom; ++y)
            #             for (var x = tileProperty.TileArea.X; x < tileProperty.TileArea.Right; ++x) {
            #                 var key = new Point(x, y)', 'string'),
            #                 if (!dictionary1.TryGetValue(key, out var dictionary2)) dictionary1[key] = dictionary2 = []', 'string'),
            #                 dictionary2[tileProperty.Name] = tileProperty.Value', 'string'),
            #             }
            #     }
            # }
            # if (!_tileProperties.TryGetValue(layerName, out var dictionary3) || !dictionary3.TryGetValue(new Point(relativeX, relativeY), out var dictionary4) || !dictionary4.TryGetValue(propertyName, out var str)) return false', 'string'),
            # propertyValue = str', 'string'),
            return True
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, a texture to draw over or behind the building.
    @RType()
    class BuildingDrawLayer:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_DrawLayerId</c>.
            ('id', 'string'),
            # The asset name of the texture to draw. Defaults to the building's <see cref='F:StardewValley.GameData.Buildings.BuildingData.Texture' /> field.
            ('[Optional] texture', 'string'),
            # The pixel area within the texture to draw. If the overlay is animated via <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount' />, this is the area of the first frame.
            ('sourceRect', 'Rectangle', Rectangle.empty),
            # The tile position at which to draw the top-left corner of the texture, relative to the building's top-left corner tile.
            ('drawPosition', 'Vector2'),
            # Whether to draw the texture behind the building sprite (i.e. underlay) instead of over it.
            ('[Optional] drawInBackground', 'bool'),
            # A Y tile offset applied when figuring out render layering. For example, a value of 2.5 will treat the texture as if it was 2.5 tiles further up the screen for the purposes of layering.
            ('[Optional] sortTileOffset', 'float'),
            # The name of a chest defined in the <see cref='F:StardewValley.GameData.Buildings.BuildingData.Chests' /> field which must contain items. If it's empty, this overlay won't be rendered. Default none.
            ('[Optional] onlyDrawIfChestHasContents', 'string'),
            # The number of milliseconds each animation frame is displayed on-screen before switching to the next, if <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount' /> is more than one.
            ('[Optional] frameDuration', 'int', 90),
            # The number of animation frames to render. If this is more than one, the building will be animated automatically based on <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow' /> and <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameDuration' />.
            ('[Optional] frameCount', 'int', 1),
            # The number of animation frames per row in the spritesheet.
            #   For each frame, the <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect' /> will be offset by its width to the right up to <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow' /> - 1 times, and then down by its height.
            #   For example, if you set <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount' /> to 6 and <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow' /> to 3, the building will expect the frames to be laid out like this in the spritesheet (where frame 1 matches <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect' />):
            #   <code>
            #     1 2 3
            #     4 5 6
            #   </code>
            ('[Optional] framesPerRow', 'int', -1),
            # A pixel offset applied to the draw layer when the animal door is open. While the door is opening, the percentage open is applied to the offset (e.g. 50% open = 50% offset).
            ('[Optional] animalDoorOffset', 'Point', Point.empty),
        ]
        # Get the parsed <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect' /> adjusted for the current game time, accounting for <see cref='F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount' />.
        # <param name='time'>The total milliseconds elapsed since the game started.</param>
        def getSourceRect(self, time: int) -> Rectangle:
            sourceRect = self.sourceRect
            time /= self.frameDuration
            time %= self.frameCount
            if self.framesPerRow < 0: sourceRect.x += sourceRect.width * time
            else: sourceRect.x += sourceRect.width * (time % self.framesPerRow); sourceRect.y += sourceRect.height * (time / self.framesPerRow)
            return sourceRect
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, an output item produced when an input item is converted.
    @RType()
    class BuildingItemConversion:
        _fields_ = [
            # A unique identifier for this entry. This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.
            ('id', 'string'),
            # A list of context tags to match against an input item. An item must have all of these tags to be accepted.
            ('requiredTags', 'List<string>'),
            # The number of the input item to consume.
            ('[Optional] requiredCount', 'int', 1),
            # The maximum number of the input item which can be processed each day. Each conversion rule has its own separate maximum (e.g. if you have two rules each with a max of 1, then you can convert one of each daily). Set to -1 to allow unlimited conversions.
            ('[Optional] maxDailyConversions', 'int', 1),
            # The name of the inventory defined in <see cref='F:StardewValley.GameData.Buildings.BuildingData.Chests' /> from which to take input items.
            ('sourceChest', 'string'),
            # The name of the inventory defined in <see cref='F:StardewValley.GameData.Buildings.BuildingData.Chests' /> in which to store output items.
            ('destinationChest', 'string'),
            # The output items produced when an input item is converted.
            ('producedItems', 'List<#GenericSpawnItemDataWithCondition>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, the materials needed to construct a building.
    @RType()
    class BuildingMaterial:
        # A key which uniquely identifies the building material.
        @property
        def id(self) -> str: return self.itemId
        _fields_ = [
            # A key which uniquely identifies the building material.
            ('[Ignore] #id', 'string'),
            # The required item ID (qualified or unqualified).
            ('itemId', 'string'),
            # The number of the item required.
            ('amount', 'int'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, a tile to treat as part of the building when placing it through a construction menu.
    @RType()
    class BuildingPlacementTile:
        _fields_ = [
            # The tile positions relative to the top-left corner of the building.
            ('tileArea', 'Rectangle'),
            # Whether this area allows tiles that would normally not be buildable, so long as they are passable. For example, this is used to ensure that an entrance is accessible.
            ('[Optional] onlyNeedsToBePassable', 'bool'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, an appearance which can be selected from the construction menu (like stone vs plank cabins).
    @RType()
    class BuildingSkin:
        _fields_ = [
            # A key which uniquely identifies the skin. The ID should only contain alphanumeric/underscore/dot characters. For custom skins, it should be prefixed with your mod ID like <c>Example.ModId_SkinName</c>.
            ('id', 'string'),
            # A tokenizable string for the skin's translated name.
            ('[Optional] name', 'string'),
            # If set, a tokenizable string for the skin's display name which represents the general building type, like 'Coop' for a Deluxe Coop. If omitted, this defaults to the <see cref='F:StardewValley.GameData.Buildings.BuildingSkin.Name' />.
            ('[Optional] nameForGeneralType', 'string'),
            # A tokenizable string for the skin's translated description.
            ('[Optional] description', 'string'),
            # The asset name for the texture under the game's <c>Content</c> folder.
            ('texture', 'string'),
            # If set, a game state query which indicates whether the skin should be available to apply. Defaults to always available.
            ('[Optional] condition', 'string'),
            # If set, overrides <see cref='F:StardewValley.GameData.Buildings.BuildingData.BuildDays' />.
            ('[Optional] buildDays', 'int?'),
            # If set, overrides <see cref='F:StardewValley.GameData.Buildings.BuildingData.BuildCost' />.
            ('[Optional] buildCost', 'int?'),
            # If set, overrides <see cref='F:StardewValley.GameData.Buildings.BuildingData.BuildMaterials' />.
            ('[Optional] buildMaterials', 'List<#BuildingMaterial>'),
            # Whether this skin should be shown as a separate building option in the construction menu.
            ('[Optional] showAsSeparateConstructionEntry', 'bool'),
            # Equivalent to the <see cref='F:StardewValley.GameData.Buildings.BuildingData.Metadata' /> field on the building. Properties defined in this field are added to the building's metadata when this skin is active, overwriting the previous property with the same name if applicable.
            ('[Optional] metadata', 'Dictionary<string, string>', []),
        ]
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, a map tile property to set.
    @RType()
    class BuildingTileProperty:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.
            ('id', 'string'),
            # The tile property name to set.
            ('name', 'string'),
            # The tile property value to set.
            ('[Optional] value', 'string'),
            # The name of the map layer whose tiles to change.
            ('layer', 'string'),
            # The tiles to which to add the property.
            ('tileArea', 'Rectangle'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, an item to place in the building interior when it's constructed or upgraded.
    @RType()
    class IndoorItemAdd:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.
            ('id', 'string'),
            # The qualified item ID for the item to place.
            ('itemId', 'string'),
            # The tile position at which to place the item.
            ('tile', 'Point'),
            # Whether to prevent the player from destroying, picking up, or moving the item.
            ('[Optional] indestructible', 'bool'),
            # Whether to remove any item on the target tile, except for another instance of <see cref='F:StardewValley.GameData.Buildings.IndoorItemAdd.ItemId' />. The previous contents of the tile will be moved into the lost and found if applicable.
            ('[Optional] clearTile', 'bool', True),
        ]
    # As part of <see cref='T:StardewValley.GameData.Buildings.BuildingData' />, a placed item in its interior to move when transitioning to an upgraded map.
    @RType()
    class IndoorItemMove:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.
            ('id', 'string'),
            # The tile position on which any item will be moved.
            ('[Optional] source', 'Point'),
            # The tile position to which to move the item.
            ('[Optional] destination', 'Point'),
            # The tile size of the area to move. If this is multiple tiles, the <see cref='F:StardewValley.GameData.Buildings.IndoorItemMove.Source' /> and <see cref='F:StardewValley.GameData.Buildings.IndoorItemMove.Destination' /> specify the top-left coordinate of the area.
            ('[Optional] size', 'Point', array([1, 1])),
            # If set, an item on this spot won't be moved if its item ID matches this one.
            ('[Optional] unlessItemId', 'string'),
        ]

class Buldles:
    class BundleData:
        # A unique ID for this entry.
        @property
        def id(self) -> str: return self.name
        _fields_ = [
            # A unique ID for this entry.
            ('[Ignore] #id', 'string'),
            ('name', 'string'),
            ('index', 'int'),
            ('sprite', 'string'),
            ('color', 'string'),
            ('items', 'string'),
            ('[Optional] pick', 'int', -1),
            ('[Optional] requiredItems', 'int', -1),
            ('reward', 'string'),
        ]
    class BundleSetData:
        _fields_ = [
            # A unique ID for this entry.
            ('id', 'string'),
            ('bundles', 'List<BundleData>', []),
        ]
    class RandomBundleData:
         # A unique ID for this entry.
        @property
        def id(self) -> str: return self.areaName
        _fields_ = [
            # A unique ID for this entry.
            ('#id', 'string'),
            ('areaName', 'string'),
            ('keys', 'string'),
            ('[Optional] bundleSets', 'List<BundleSetData>', []),
            ('[Optional] bundles', 'List<BundleData>', []),
        ]

class Characters:
    # How an NPC's birthday is shown on the calendar.
    @RType()
    class CalendarBehavior(Enum):
        # They always appear on the calendar.
        AlwaysShown = 0
        # Until the player meets them, they don't appear on the calendar.
        HiddenUntilMet = 1
        # They never appear on the calendar.
        HiddenAlways = 2
    # How an NPC appears in the end-game perfection slide show.
    @RType()
    class EndSlideShowBehavior(Enum):
        # The NPC doesn't appear in the slide show.
        Hidden = 0
        # The NPC is added to the main group of NPCs which walk across the screen.
        MainGroup = 1
        # The NPC is added to the trailing group of NPCs which follow the main group.
        TrailingGroup = 2
    # The general age of an NPC.
    @RType()
    class NpcAge(Enum):
        Adult = 0
        Teen = 1
        Child = 2
    # The language spoken by an NPC.
    @RType()
    class NpcLanguage(Enum):
        # The default language understood by the player.
        Default = 0
        # The Dwarvish language, which the player can only understand after finding the Dwarvish Translation Guide.
        Dwarvish = 1
    # A measure of a character's general politeness.
    @RType()
    class NpcManner(Enum):
        Neutral = 0
        Polite = 1
        Rude = 2
    # A measure of a character's overall optimism.
    @RType()
    class NpcOptimism(Enum):
        Positive = 0
        Negative = 1
        Neutral = 2
    # A measure of a character's comfort with social situations.
    @RType()
    class NpcSocialAnxiety(Enum):
        Outgoing = 0
        Shy = 1
        Neutral = 2
    # How an NPC is shown on the social tab when unlocked.
    @RType()
    class SocialTabBehavior(Enum):
        # Until the player meets them, their name on the social tab is replaced with '???'.
        UnknownUntilMet = 0
        # They always appear on the social tab (including their name).
        AlwaysShown = 1
        # Until the player meets them, they don't appear on the social tab.
        HiddenUntilMet = 2
        # They never appear on the social tab.
        HiddenAlways = 3
class Characters:
    @RType()
    class CharacterAppearanceData:
        _fields_ = [
            # An ID for this entry within the appearance list. This only needs to be unique within the current list.
            ('id', 'string'),
            # A game state query which indicates whether this entry applies. Default true.
            ('[Optional] condition', 'string'),
            # The season when this appearance applies, or <c>null</c> for any season.
            ('[Optional] season', '#Season?'),
            # Whether the appearance can be used when the NPC is indoors.
            ('[Optional] indoors', 'bool', True),
            # Whether the appearance can be used when the NPC is outdoors.
            ('[Optional] outdoors', 'bool', True),
            # The asset name for the portrait texture, or null for the default portrait.
            ('[Optional] portrait', 'string'),
            # The asset name for the sprite texture, or null for the default sprite.
            ('[Optional] sprite', 'string'),
            # Whether this is island beach attire worn at the resort.
            # This is mutually exclusive: NPCs will never wear it in other contexts if it's true, and will never wear it as island attire if it's false.
            ('[Optional] isIslandAttire', 'bool'),
            # The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.
            ('[Optional] precedence', 'int'),
            # If multiple entries with the same <see cref='F:StardewValley.GameData.Characters.CharacterAppearanceData.Precedence' /> apply, the relative weight to use when randomly choosing one.
            # See remarks on <see cref='F:StardewValley.GameData.Characters.CharacterData.Appearance' />.
            ('[Optional] weight', 'int', 1),
        ]
    # The content data for an NPC.
    @RType()
    class CharacterData :
        _fields_ = [
            # A tokenizable string for the NPC's display name.
            ('displayName', 'string'),
            # The season when the NPC was born.
            ('[Optional] birthSeason', '#Season?'),
            # The day when the NPC was born.
            ('[Optional] birthDay', 'int'),
            # The region of the world in which the NPC lives (one of <c>Desert</c>, <c>Town</c>, or <c>Other</c>).
            # For example, only <c>Town</c> NPCs are counted for the introductions quest, can be selected as a secret santa for the Feast of the Winter Star, or get a friendship boost from the Luau.
            ('[Optional] homeRegion', 'string', 'Other'),
            # The language spoken by the NPC.
            ('[Optional] language', 'NpcLanguage'),
            # The character's gender identity.
            ('[Optional] gender', 'Gender', Gender.Undefined),
            # The general age of the NPC.
            # This affects generated dialogue lines (e.g. a child might say 'stupid' and an adult might say 'depressing'), generic dialogue (e.g. a child might respond to dumpster diving with 'Eww... What are you doing?' and a teen would say 'Um... Why are you digging in the trash?'), and the gift they choose as Feast of the Winter Star gift-giver. Children are also excluded from item delivery quests.
            ('[Optional] age', 'NpcAge'),
            # A measure of the character's general politeness.
            # This affects some generic dialogue lines.
            ('[Optional] manner', 'NpcManner'),
            # A measure of the character's comfort with social situations.
            # This affects some generic dialogue lines.
            ('[Optional] socialAnxiety', 'NpcSocialAnxiety', Characters.NpcSocialAnxiety.Neutral),
            # A measure of the character's overall optimism.
            ('[Optional] optimism', 'NpcOptimism', Characters.NpcOptimism.Neutral),
            # Whether the NPC has dark skin, which affects the chance of children with the player having dark skin too.
            ('[Optional] isDarkSkinned', 'bool'),
            # Whether players can date and marry this NPC.
            ('[Optional] canBeRomanced', 'bool'),
            # Unused.
            ('[Optional] loveInterest', 'string'),
            # How the NPC's birthday is shown on the calendar.
            ('[Optional] calendar', 'CalendarBehavior'),
            # How the NPC is shown on the social tab.
            ('[Optional] socialTab', 'SocialTabBehavior'),
            # A game state query which indicates whether to enable social features (like birthdays, gift giving, friendship, and an entry in the social tab). Defaults to true (except for monsters, horses, pets, and Junimos).
            ('[Optional] canSocialize', 'string'),
            # Whether players can give gifts to this NPC. Default true.
            # The NPC must also be social per <see cref='F:StardewValley.GameData.Characters.CharacterData.CanSocialize' /> and have an entry in <c>Data/NPCGiftTastes</c> to receive gifts, regardless of this value.
            ('[Optional] canReceiveGifts', 'bool', True),
            # Whether this NPC can show a speech bubble greeting nearby players or NPCs, and or be greeted by other NPCs. Default true.
            ('[Optional] canGreetNearbyCharacters', 'bool', True),
            # Whether this NPC can comment on items that a player sold to a shop which then resold it to them, or <c>null</c> to allow it if their <see cref='F:StardewValley.GameData.Characters.CharacterData.HomeRegion' /> is <c>Town</c>.
            # The NPC must also be social per <see cref='F:StardewValley.GameData.Characters.CharacterData.CanSocialize' /> to allow it, regardless of this value.
            ('[Optional] canCommentOnPurchasedShopItems', 'bool?'),
            # A game state query which indicates whether the NPC can visit Ginger Island once the resort is unlocked.
            ('[Optional] canVisitIsland', 'string'),
            # Whether to include this NPC in the introductions quest, or <c>null</c> to include them if their <see cref='F:StardewValley.GameData.Characters.CharacterData.HomeRegion' /> is <c>Town</c>.
            ('[Optional] introductionsQuest', 'bool?'),
            # A game state query which indicates whether this NPC can give item delivery quests, or <c>null</c> to allow it if their <see cref='F:StardewValley.GameData.Characters.CharacterData.HomeRegion' /> is <c>Town</c>.
            # The NPC must also be social per <see cref='F:StardewValley.GameData.Characters.CharacterData.CanSocialize' /> to be included, regardless of this value.
            ('[Optional] itemDeliveryQuests', 'string'),
            # Whether to include this NPC when checking whether the player has max friendships with every NPC for the perfection score.
            # The NPC must also be social per <see cref='F:StardewValley.GameData.Characters.CharacterData.CanSocialize' /> to be counted, regardless of this value.
            ('[Optional] perfectionScore', 'bool', True),
            # How the NPC appears in the end-game perfection slide show.
            ('[Optional] endSlideShow', 'EndSlideShowBehavior', Characters.EndSlideShowBehavior.MainGroup),
            # A game state query which indicates whether the player will need to adopt children with this spouse, instead of either the player or NPC giving birth. If null, defaults to true for same-gender and false for opposite-gender spouses.
            ('[Optional] spouseAdopts', 'string'),
            # A game state query which indicates whether the spouse will ask to have children. Defaults to true.
            ('[Optional] spouseWantsChildren', 'string'),
            # A game state query which indicates whether the spouse will get jealous when the player gifts items to another NPC of the same gender when it's not their birthday. Defaults to true.
            ('[Optional] spouseGiftJealousy', 'string'),
            # The friendship change when <see cref='F:StardewValley.GameData.Characters.CharacterData.SpouseGiftJealousy' /> applies.
            ('[Optional] spouseGiftJealousyFriendshipChange', 'int', -30),
            # The NPC's spouse room in the farmhouse when the player marries them.
            ('[Optional] spouseRoom', 'CharacterSpouseRoomData'),
            # The NPC's patio area on the farm when the player marries them, if any.
            ('[Optional] spousePatio', 'CharacterSpousePatioData'),
            # The floor IDs which the NPC might randomly apply to the farmhouse when married, or an empty list to choose from the vanilla floors.
            ('[Optional] spouseFloors', 'List<string>', []),
            # The wallpaper IDs which the NPC might randomly apply to the farmhouse when married, or an empty list to choose from the vanilla wallpapers.
            ('[Optional] spouseWallpapers', 'List<string>', []),
            # The friendship point change if this NPC sees a player rummaging through trash.
            ('[Optional] dumpsterDiveFriendshipEffect', 'int', -25),
            # The emote ID to show above the NPC's head when they see a player rummaging through trash.
            ('[Optional] dumpsterDiveEmote', 'int?'),
            # The NPC's closest friends and family, where the key is the NPC name and the value is an optional tokenizable string for the name to use in dialogue text (like 'mom').
            # This affects generic dialogue for revealing likes and dislikes to family members, and may affect <c>inlaw_{NPC}</c> dialogues. This isn't necessarily comprehensive.
            ('[Optional] friendsAndFamily', 'Dictionary<string, string>', []),
            # Whether the NPC can be asked to dance at the Flower Dance festival. This can be true (can be asked even if not romanceable), false (can never ask), or null (true if romanceable).
            ('[Optional] flowerDanceCanDance', 'bool?'),
            # At the Winter Star festival, the possible gifts this NPC can give to players.
            # If this doesn't return a match, a generic gift is selected based on <see cref='F:StardewValley.GameData.Characters.CharacterData.Age' />.
            ('[Optional] winterStarGifts', 'List<#GenericSpawnItemDataWithCondition>', []),
            # A game state query which indicates whether this NPC can give and receive gifts at the Feast of the Winter Star, or <c>null</c> to allow it if their <see cref='F:StardewValley.GameData.Characters.CharacterData.HomeRegion' /> is <c>Town</c>.
            ('[Optional] winterStarParticipant', 'string'),
            # A game state query which indicates whether the NPC should be added to the world, checked when loading a save and when ending each day. This only affects whether the NPC is added when missing; returning false won't remove an NPC that's already been added.
            ('[Optional] unlockConditions', 'string'),
            # Whether to add this NPC to the world automatically when they're missing and the <see cref='F:StardewValley.GameData.Characters.CharacterData.UnlockConditions' /> match.
            ('[Optional] spawnIfMissing', 'bool', True),
            # The possible locations for the NPC's default map. The first matching entry is used.
            ('[Optional] home', 'List<CharacterHomeData>'),
            # The <strong>last segment</strong> of the NPC's portrait and sprite asset names when not set via <see cref='F:StardewValley.GameData.Characters.CharacterData.Appearance' />. For example, set to <c>'Abigail'</c> to use <c>Portraits/Abigail</c> and <c>Characters/Abigail</c> respectively. Defaults to the internal NPC name.
            ('[Optional] textureName', 'string'),
            # The sprite and portrait texture to use, if set.
            # <para>The appearances are sorted by <see cref='F:StardewValley.GameData.Characters.CharacterAppearanceData.Precedence' />, then filtered to those whose fields match. If multiple matching appearances have the highest precedence, one entry is randomly chosen based on their relative weight. This randomization is stable per day, so the NPC always makes the same choice until the next day.</para>
            # <para>If a portrait/sprite can't be loaded (or no appearances match), the NPC will use the default asset based on <see cref='F:StardewValley.GameData.Characters.CharacterData.TextureName' />.</para>
            ('[Optional] appearance', 'List<CharacterAppearanceData>', []),
            # The pixel area in the character's sprite texture to show as their mug shot in contexts like the calendar or social menu, or <c>null</c> for the first sprite in the spritesheet.
            # This should be approximately 16x24 pixels for best results.
            ('[Optional] mugShotSourceRect', 'Rectangle?'),
            # The pixel size of the individual sprites in their world sprite spritesheet.
            ('[Optional] size', 'Point', Point(16, 32)),
            # Whether the chest on the NPC's world sprite puffs in and out as they breathe.
            ('[Optional] breather', 'bool', True),
            # The pixel area within the spritesheet which expands and contracts to simulate breathing, relative to the top-left corner of the source rectangle for their current sprite, or <c>null</c> to calculate it automatically.
            ('[Optional] breathChestRect', 'Rectangle?'),
            # The pixel offset to apply to the NPC's <see cref='F:StardewValley.GameData.Characters.CharacterData.BreathChestPosition' /> when drawn over the NPC, or <c>null</c> for the default offset.
            ('[Optional] breathChestPosition', 'Point?'),
            # The shadow to draw, or <c>null</c> to apply the default options.
            ('[Optional] shadow', 'CharacterShadowData'),
            # A pixel offset to apply to the character's default emote position.
            ('[Optional] emoteOffset', 'Point', Point.empty),
            # The portrait indexes which should shake when displayed.
            ('[Optional] shakePortraits', 'List<int>', []),
            # The sprite index within the <see cref='F:StardewValley.GameData.Characters.CharacterData.TextureName' /> to use when kissing a player.
            ('[Optional] kissSpriteIndex', 'int', 28),
            # Whether the character is facing right (true) or left (false) in their <see cref='F:StardewValley.GameData.Characters.CharacterData.KissSpriteIndex' />. The sprite will be flipped as needed to face the player.
            ('[Optional] kissSpriteFacingRight', 'bool', True),
            # For the hidden gift log emote, the cue ID for the sound played when clicking the sprite. Defaults to <c>drumkit6</c>.
            # The hidden gift log emote happens when clicking on a character's sprite in the profile menu after earning enough hearts.
            ('[Optional] hiddenProfileEmoteSound', 'string'),
            # For the hidden gift log emote, how long the animation plays measured in milliseconds. Defaults to 4000 (4 seconds).
            # See remarks on <see cref='F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound' />.
            ('[Optional] hiddenProfileEmoteDuration', 'int', -1),
            # For the hidden gift log emote, the index within the NPC's world sprite spritesheet at which the animation starts. If omitted for a vanilla NPC, the game plays a default animation specific to that NPC; if omitted for a custom NPC, the game just shows them walking while facing down.
            # See remarks on <see cref='F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound' />.
            ('[Optional] hiddenProfileEmoteStartFrame', 'int', -1),
            # For the hidden gift log emote, the number of frames in the animation. The first frame corresponds to <see cref='F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame' />, and each subsequent frame will use the next sprite in the spritesheet. This has no effect if <see cref='F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame' /> isn't set.
            # See remarks on <see cref='F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound' />.
            ('[Optional] hiddenProfileEmoteFrameCount', 'int', 1),
            # For the hidden gift log emote, how long each animation frame is shown on-screen before switching to the next one, measured in milliseconds. This has no effect if <see cref='F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame' /> isn't set.
            # See remarks on <see cref='F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound' />.
            ('[Optional] hiddenProfileEmoteFrameDuration', 'float', 200.),
            # The former NPC names which may appear in save data.
            # If a NPC in save data has a name which (a) matches one of these values and (b) doesn't match the name of a loaded NPC, its data will be loaded into this NPC instead. If that happens, this will also update other references like friendship and spouse data.
            ('[Optional] formerCharacterNames', 'List<string>', []),
            # The NPC's index in the <c>Maps/characterSheet</c> tilesheet, if applicable. This is used for placing vanilla NPCs in festivals from the map; custom NPCs should use the <c>{layer}_additionalCharacters</c> field in the festival data instead.
            ('[Optional] festivalVanillaActorIndex', 'int', -1),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Characters.CharacterData' />, a possible location for the NPC's default map.
    @RType()
    class CharacterHomeData:
        _fields_ = [
            # An ID for this entry within the home list. This only needs to be unique within the current list.
            ('id', 'string'),
            # A game state query which indicates whether this entry applies. Default true.
            ('[Optional] condition', 'string'),
            # The internal name for the home location where this NPC spawns and returns each day.
            ('location', 'string'),
            # The tile position within the home location where this NPC spawns and returns each day.
            ('tile', 'Point', Point.empty),
            # The default direction the NPC faces when they start each day. The possible values are <c>down</c>, <c>left</c>, <c>right</c>, and <c>up</c>.
            ('[Optional] direction', 'string', 'up'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Characters.CharacterData' />, configures how the NPC's shadow should be rendered.
    @RType()
    class CharacterShadowData:
        _fields_ = [
            # Whether the shadow should be drawn.
            ('[Optional] visible', 'bool', True),
            # A pixel offset applied to the shadow position.
            ('[Optional] offset', 'Point', Point.empty),
            # The scale at which to draw the shadow.
            # This is a multiplier applied to the default shadow scale, which can change based on factors like whether the NPC is jumping. For example, <c>0.5</c> means half the size it'd be drawn if you didn't specify a scale.
            ('[Optional] scale', 'float', 1.),
        ]
    # As part of <see cref='T:StardewValley.GameData.Characters.CharacterData' />, the data about the NPC's patio area on the farm when the player marries them.
    @RType()
    class CharacterSpousePatioData:
        # The default value for <see cref='F:StardewValley.GameData.Characters.CharacterSpousePatioData.MapSourceRect' />.
        defaultMapSourceRect: Rectangle = Rectangle(0, 0, 4, 4)
        _fields_ = [
            # The asset name within the content <c>Maps</c> folder which contains the patio. Defaults to <c>spousePatios</c>.
            ('[Optional] mapAsset', 'string'),
            # The tile area within the <see cref='F:StardewValley.GameData.Characters.CharacterSpousePatioData.MapAsset' /> containing the spouse's patio. This must be a 4x4 tile area or smaller.
            ('[Optional] mapSourceRect', 'Rectangle', defaultMapSourceRect),
            # The spouse's animation frames when they're in the patio. Each frame is a tuple containing the [0] frame index and [1] optional duration in milliseconds (default 100). If omitted or empty, the NPC won't be animated.
            ('[Optional] spriteAnimationFrames', 'List<int[]>'),
            # The pixel offset to apply to the NPC's sprite when they're animated in the patio.
            ('[Optional] spriteAnimationPixelOffset', 'Point'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Characters.CharacterData' />, the data about the NPC's spouse room in the farmhouse when the player marries them.
    @RType()
    class CharacterSpouseRoomData:
        # The default value for <see cref='F:StardewValley.GameData.Characters.CharacterSpouseRoomData.MapSourceRect' />.
        defaultMapSourceRect: Rectangle = Rectangle(0, 0, 6, 9)
        _fields_ = [
            # The asset name within the content <c>Maps</c> folder which contains the spouse room. Defaults to <c>spouseRooms</c>.
            ('[Optional] mapAsset', 'string'),
            # The tile area within the <see cref='F:StardewValley.GameData.Characters.CharacterSpouseRoomData.MapAsset' /> containing the spouse's room.
            ('[Optional] mapSourceRect', 'Rectangle', defaultMapSourceRect),
        ]

class Crafting:
    # A clothing item that can be tailored from ingredients using Emily's sewing machine.
    class TailorItemRecipe:
        _fields_ = [
            # The context tags for the first item required, or <c>null</c> for none required. If this lists multiple context tags, an item must match all of them.
            ('[Optional] firstItemTags', 'List<string>'),
            # The context tags for the second item required, or <c>null</c> for none required. If this lists multiple context tags, an item must match all of them.
            ('[Optional] secondItemTags', 'List<string>'),
            # Whether tailoring the item destroys the item matched by <see cref='F:StardewValley.GameData.Crafting.TailorItemRecipe.SecondItemTags' />.
            ('[Optional] spendRightIteme', 'bool', True),
            # The item ID to produce by default.
            # Ignored if <see cref='F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIds' /> has any values, or for female players if <see cref='F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIdFeminine' /> is set.
            ('[Optional] craftedItemId', 'string'),
            # The item IDs to produce by default.
            # Ignored for female players if <see cref='F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIdFeminine' /> is set.
            ('[Optional] craftedItemIds', 'List<string>'),
            # If set, the item ID to produce if the player is female.
            ('[Optional] craftedItemIdFeminine', 'string'),
            # A unique identifier for this entry. This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.
            ('[Optional] #id', 'string'),
        ]
        # The backing field for <see cref='P:StardewValley.GameData.Crafting.TailorItemRecipe.Id' />.
        _idImpl:str = None
        # A unique identifier for this entry. This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.
        @property
        def id(self) -> str:
            if self._idImpl: return self._idImpl
            return string.Join(',', self.craftedItemIds) if ((1 if self.craftedItemIds.any() else 0) if self.craftedItemIds != None else 0) != 0 else self.craftedItemId
        @id.setter
        def id(self, value: str) -> None: self._idImpl = value

class Crops:
    # The metadata for a crop that can be planted.
    class CropData:
        _fields_ = [
            # The seasons in which this crop can grow.
            ('seasons', 'List<#Season>', []),
            # The number of days in each visual step of growth before the crop is harvestable.
            ('daysInPhase', 'List<int>', []),
            # The number of days before the crop regrows after harvesting, or -1 if it can't regrow.
            ('[Optional] regrowDays', 'int', -1),
            # Whether this is a raised crop on a trellis that can't be walked through.
            ('[Optional] isRaised', 'bool'),
            # Whether this crop can be planted near water for a unique paddy dirt texture, faster growth time, and auto-watering.
            ('[Optional] isPaddyCrop', 'bool'),
            # Whether this crop needs to be watered to grow.
            ('[Optional] needsWatering', 'bool', True),
            # The rules which override which locations the crop can be planted in, if applicable. These don't override more specific checks (e.g. crops needing to be planted in dirt).
            ('[Optional] plantableLocationRules', 'List<#PlantableRule>'),
            # The unqualified item ID produced when this crop is harvested.
            ('[Optional] harvestItemId', 'string'),
            # The minimum number of <see cref='F:StardewValley.GameData.Crops.CropData.HarvestItemId' /> to harvest.
            ('[Optional] harvestMinStack', 'int'), 1,
            # The maximum number of <see cref='F:StardewValley.GameData.Crops.CropData.HarvestItemId' /> to harvest, before <see cref='F:StardewValley.GameData.Crops.CropData.ExtraHarvestChance' /> and <see cref='F:StardewValley.GameData.Crops.CropData.HarvestMaxIncreasePerFarmingLevel' /> are applied.
            ('[Optional] harvestMaxStack', 'int', 1),
            # The number of extra harvests to produce per farming level. This is rounded down to the nearest integer and added to <see cref='F:StardewValley.GameData.Crops.CropData.HarvestMaxStack' />.
            ('[Optional] harvestMaxIncreasePerFarmingLevel', 'float'),
            # The probability that harvesting the crop will produce extra harvest items, as a value between 0 (never) and 0.9 (nearly always). This is repeatedly rolled until it fails, then the number of successful rolls is added to the produced count.
            ('[Optional] extraHarvestChance', 'double'),
            # How the crop can be harvested.
            ('[Optional] harvestMethod', 'HarvestMethod'),
            # If set, the minimum quality of the harvest crop.
            # These fields set a constraint that's applied after the quality is calculated normally, they don't affect the initial quality logic.
            ('[Optional] harvestMinQuality', 'int'),
            # If set, the maximum quality of the harvest crop.
            # [inheritdoc cref='F:StardewValley.GameData.Crops.CropData.HarvestMinQuality' path='/remarks' />
            ('[Optional] harvestMaxQuality', 'int?'),
            # The tint colors that can be applied to the crop sprite, if any. If multiple colors are listed, one is chosen at random for each crop. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).
            ('[Optional] tintColors', 'List<string>', []),
            # The asset name for the crop texture under the game's <c>Content</c> folder.
            ('texture', 'string'),
            # The index of this crop in the <see cref='F:StardewValley.GameData.Crops.CropData.Texture' /> (one crop per row).
            ('spriteIndex', 'int'),
            # Whether the player can ship 300 of this crop's harvest item to unlock the monoculture achievement.
            ('countForMonoculture', 'bool'),
            # Whether the player must ship 15 of this crop's harvest item to unlock the polyculture achievement.
            ('countForPolyculture', 'bool'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
        # Get the <see cref='F:StardewValley.GameData.Crops.CropData.Texture' /> if different from the default name.
        # <param name='defaultName'>The default asset name.</param>
        def getCustomTextureName(defaultName: str) -> str: return None if not self.texture or (self.texture == defaultName) else self.texture

    # Indicates how a crop can be harvested.
    class HarvestMethod(Enum):
        # The crop is harvested by hand.
        Grab = 0
        # The crop is harvested using a scythe.
        Scythe = 1

class FarmAnimials:
    # As part of <see cref='T:StardewValley.GameData.FarmAnimals.FarmAnimalData' />, a possible variant for a farm animal.
    class AlternatePurchaseAnimals:
        _fields_ = [
            # A unique string ID for this entry within the current animal's list.
            ('id', 'string'),
            # A game state query which indicates whether this variant entry is available. Default always enabled.
            ('[Optional] condition', 'string'),
            # A list of animal IDs to spawn instead of the main ID field. If multiple are listed, one is chosen at random on purchase.
            ('animalIds', 'List<string>'),
        ]
    # The metadata for a farm animal which can be bought from Marnie's ranch.
    class FarmAnimalData:
        _fields_ = [
            # A tokenizable string for the animal type's display name.
            ('[Optional] displayName', 'string'),
            # The ID for the main building type that houses this animal. The animal will also be placeable in buildings whose <see cref='F:StardewValley.GameData.Buildings.BuildingData.ValidOccupantTypes' /> field contains this value.
            ('[Optional] house', 'string'),
            # The default gender for the animal type. This only affects the text shown after purchasing the animal.
            ('[Optional] gender', 'FarmAnimalGender'),
            # Half the cost to purchase the animal (the actual price is double this value), or a negative value to disable purchasing this animal type. Default -1.
            ('[Optional] purchasePrice', 'int', -1),
            # The price when the player sells the animal, before it's adjusted for the animal's friendship towards the player.
            # The actual sell price will be this value multiplied by a number between 0.3 (zero friendship) and 1.3 (max friendship).
            ('[Optional] sellPrice', 'int'),
            # The asset name for the icon texture to show in shops.
            ('[Optional] shopTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopTexture' /> to draw. This should be 32 pixels wide and 16 high. Ignored if <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopTexture' /> isn't set.
            ('[Optional] shopSourceRect', 'Rectangle'),
            # A tokenizable string for the display name shown in the shop menu. Defaults to the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DisplayName' /> field.
            ('[Optional] shopDisplayName', 'string'),
            # A tokenizable string for the tooltip description shown in the shop menu. Defaults to none.
            ('[Optional] shopDescription', 'string'),
            # A tokenizable string which overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopDescription' /> if the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.RequiredBuilding' /> isn't built. Defaults to none.
            ('[Optional] shopMissingBuildingDescription', 'string'),
            # The building that needs to be built on the farm for this animal to be available to purchase. Buildings that are upgraded from this building are valid too. Default none.
            ('[Optional] requiredBuilding', 'string'),
            # A game state query which indicates whether the farm animal is available in the shop menu. Default always unlocked.
            ('[Optional] unlockCondition', 'string'),
            # The possible variants for this farm animal (e.g. chickens can be Brown Chicken, Blue Chicken, or White Chicken). When the animal is purchased, of the available variants is chosen at random.
            ('[Optional] alternatePurchaseTypes', 'List<AlternatePurchaseAnimals>'),
            # A list of the object IDs that can be placed in the incubator or ostrich incubator to hatch this animal. If <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.House' /> doesn't match the current building, the entry will be ignored. Default none.
            ('[Optional] eggItemIds', 'List<string>'),
            # How long eggs incubate before they hatch, in in-game minutes. Defaults to 9000 minutes.
            ('[Optional] incubationTime', 'int', -1),
            # An offset applied to the incubator's sprite index when it's holding an egg for this animal.
            ('[Optional] incubatorParentSheetOffset', 'int', 1),
            # A tokenizable string for the message shown when entering the building after the egg hatched. Defaults to the text '???'.
            ('[Optional] birthText', 'string'),
            # The number of days until a freshly purchased/born animal becomes an adult and begins producing items.
            ('[Optional] daysToMature', 'int', 1),
            # Whether an animal can produce a child (regardless of gender).
            ('[Optional] canGetPregnant', 'bool'),
            # The number of days between item productions. For example, setting 1 will produce an item every other day.
            ('[Optional] daysToProduce', 'int', 1),
            # How produced items are collected from the animal.
            ('[Optional] harvestType', 'FarmAnimalHarvestType'),
            # The tool name with which produced items can be collected from the animal, if the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestType' /> is set to <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalHarvestType.HarvestWithTool' />. The values recognized by the vanilla tools are <c>Milk Pail</c> and <c>Shears</c>. Default none.
            ('[Optional] harvestTool', 'string'),
            # The items produced by the animal when it's an adult, if <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceMinimumFriendship' /> does not match.
            ('[Optional] produceItemIds', 'List<FarmAnimalProduce>', []),
            # The items produced by the animal when it's an adult, if <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceMinimumFriendship' /> matches.
            ('[Optional] deluxeProduceItemIds', 'List<FarmAnimalProduce>', []),
            # Whether an item is produced on the day the animal becomes an adult (like sheep).
            ('[Optional] poduceOnMature', 'bool'),
            # The minimum friendship points needed to reduce the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DaysToProduce' /> by one. Defaults to no reduction.
            ('[Optional] friendshipForFasterProduce', 'int', -1),
            # The minimum friendship points needed to produce the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds' />.
            ('[Optional] deluxeProduceMinimumFriendship', 'int', 200),
            # A divisor which reduces the probability of producing <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds' />. Lower values produce deluxe items more often.
            #   This is applied using this formula:
            #   <code>
            #     if happiness &gt; 200: happiness_modifier = happiness * 1.5
            #     else if happiness &gt; 100: happiness_modifier = 0
            #     else happiness_modifier = happiness - 100
            #     ((friendship + happiness_modifier) / DeluxeProduceCareDivisor) + (daily_luck * DeluxeProduceLuckMultiplier)
            #   </code>
            #   For example, given a friendship of 102 and happiness of 150, the probability with the default field values will be <c>((102 + 0) / 1200) + (daily_luck * 0) = (102 / 1200) = 0.085</c> or 8.5%.
            ('[Optional] deluxeProduceCareDivisor', 'float', 1200.),
            # A multiplier which increases the bonus from daily luck on the probability of producing <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds' />.
            # See remarks on <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceCareDivisor' />.
            ('[Optional] deluxeProduceLuckMultiplier', 'float'),
            # Whether players can feed this animal a golden cracker to double its normal output.
            ('[Optional] canEatGoldenCrackers', 'stboolring', True),
            # The internal ID of a profession which makes it easier to befriend this animal. Defaults to none.
            ('[Optional] professionForHappinessBoost', 'int', -1),
            # The internal ID of a profession which increases the chance of higher-quality produce.
            ('[Optional] professionForQualityBoost', 'int', -1),
            # The internal ID of a profession which reduces the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DaysToProduce' /> by one. Defaults to none.
            ('[Optional] professionForFasterProduce', 'int', -1),
            # The audio cue ID for the sound produced by the animal (e.g. when pet). Default none.
            ('[Optional] sound', 'string'),
            # If set, overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Sound' /> when the animal is a baby. Has no effect if <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Sound' /> isn't set.
            ('[Optional] babySound', 'string'),
            # If set, the asset name for the animal's spritesheet. Defaults to <c>Animals/{ID}</c>, like Animals/Goat for a goat.
            ('[Optional] texture', 'string'),
            # If set, overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture' /> when the animal doesn't currently have an item ready to collect (like the sheep's sheared sprite).
            ('[Optional] harvestedTexture', 'string'),
            # If set, overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture' /> and <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestedTexture' /> when the animal is a baby.
            ('[Optional] babyTexture', 'string'),
            # When the animal is facing left, whether to use a flipped version of their right-facing sprite.
            ('[Optional] useFlippedRightForLeft', 'bool'),
            # The pixel width of the animal's sprite (before in-game pixel zoom is applied).
            ('[Optional] spriteWidth', 'int', 16),
            # The pixel height of the animal's sprite (before in-game pixel zoom is applied).
            ('[Optional] spriteHeight', 'int', 16),
            # Whether the animal has two frames for the randomized 'unique' animation instead of one.
            # <para>If false, the unique sprite frames are indexes 13 (down), 14 (right), 12 (left if <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.UseFlippedRightForLeft' /> is false), and 15 (up).</para>
            # <para>If true, the unique sprite frames are indexes 16 (down), 18 (right), 22 (left), and 20 (up).</para>
            ('[Optional] useDoubleUniqueAnimationFrames', 'bool'),
            # The sprite index to display when sleeping.
            ('[Optional] sleepFrame', 'int', 12),
            # A pixel offset to apply to emotes drawn over the farm animal.
            ('[Optional] emoteOffset', 'Point', Point.empty),
            # A pixel offset to apply to the farm animal's sprite while it's swimming.
            ('[Optional] swimOffset', 'Point', Point(0, 112)),
            # The possible alternate appearances, if any. A skin is chosen at random when the animal is purchased or hatched based on the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalSkin.Weight' /> field. The default appearance (e.g. using <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture' />) is automatically an available skin with a weight of 1.
            ('[Optional] skins', 'List<FarmAnimalSkin>'),
            # The shadow to draw when a baby animal is swimming, or <c>null</c> to apply <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShadowWhenBaby' />.
            ('[Optional] shadowWhenBabySwims', 'FarmAnimalShadowData'),
            # The shadow to draw for a baby animal, or <c>null</c> to apply <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Shadow' />.
            ('[Optional] shadowWhenBaby', 'FarmAnimalShadowData'),
            # The shadow to draw when an adult animal is swimming, or <c>null</c> to apply <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShadowWhenAdult' />.
            ('[Optional] shadowWhenAdultSwims', 'FarmAnimalShadowData'),
            # The shadow to draw for an adult animal, or <c>null</c> to apply <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Shadow' />.
            ('[Optional] shadowWhenAdult', 'FarmAnimalShadowData'),
            # The shadow to draw if a more specific shadow field doesn't apply, or <c>null</c> to apply the default options.
            ('[Optional] shadow', 'FarmAnimalShadowData'),
            # Whether animals on the farm can swim in water once they've been pet. Default false.
            ('[Optional] canSwim', 'bool'),
            # Whether baby animals can follow nearby adults. This only applies for animals whose <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.House' /> field is <c>Coop</c>. Default false.
            ('[Optional] babiesFollowAdults', 'bool'),
            # The amount of grass eaten by this animal each day.
            ('[Optional] grassEatAmount', 'int', 2),
            # An amount which affects the daily reduction in happiness if the animal wasn't pet, or didn't have a heater in winter.
            ('[Optional] happinessDrain', 'int'),
            # The animal sprite's tile size in the world when the player is clicking to pet them, if the animal is facing up or down. This can be a fractional value like 1.75.
            ('[Optional] upDownPetHitboxTileSize', 'Vector2', array([1., 1.])),
            # The animal sprite's tile size in the world when the player is clicking to pet them, if the animal is facing left or right. This can be a fractional value like 1.75.
            ('[Optional] leftRightPetHitboxTileSize', 'Vector2', array([1., 1.])),
            # Overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.UpDownPetHitboxTileSize' /> when the animal is a baby.
            ('[Optional] babyUpDownPetHitboxTileSize', 'Vector2', array([1., 1.])),
            # Overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.LeftRightPetHitboxTileSize' /> when the animal is a baby.
            ('[Optional] babyLeftRightPetHitboxTileSize', 'Vector2', array([1., 1.])),
            # The game stat counters to increment when the animal produces an item, if any.
            ('[Optional] statToIncrementOnProduce', 'List<#StatIncrement>'),
            # Whether to show the farm animal in the credit scene on the summit after the player achieves perfection.
            ('[Optional] showInSummitCredits', 'bool'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
        # Get the options to apply when drawing the animal's shadow, if any.
        # <param name='isBaby'>Whether the animal is a baby.</param>
        # <param name='isSwimming'>Whether the animal is swimming.</param>
        def getShadow(self, isBaby: bool, isSwimming: bool) -> FarmAnimalShadowData: return \
            (self.shadowWhenBaby or self.shadow if not isSwimming else self.shadowWhenBabySwims or self.shadowWhenBaby or self.shadow) if isBaby else \
            (self.shadowWhenAdult or self.shadow if not isSwimming else self.shadowWhenAdultSwims or self.shadowWhenAdult or self.shadow)
    # The default gender for a farm animal type.
    class FarmAnimalGender(Enum):
        # The farm animal is always female.
        Female = 0
        # The farm animal is always male.
        Male = 1
        # The gender of each animal is randomized when it's purchased.
        MaleOrFemale = 2
    # How produced items are collected from an animal.
    class FarmAnimalHarvestType(Enum):
        # The item is placed on the ground in the animal's home building overnight.
        DropOvernight = 0
        # The item is collected from the animal directly based on the <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestTool' /> field.
        HarvestWithTool = 1
        # The farm animal digs it up with an animation like pigs finding truffles.
        DigUp = 2
    # As part of <see cref='T:StardewValley.GameData.FarmAnimals.FarmAnimalData' />, an item that can be produced by the animal when it's an adult.
    class FarmAnimalProduce:
        _fields_ = [
            # An ID for this entry within the produce list. This only needs to be unique within the current list.
            ('[Optional] id', 'string'),
            # A game state query which indicates whether this item can be produced now. Defaults to always true.
            ('[Optional] condition', 'string'),
            # The minimum friendship points with the animal needed to produce this item.
            ('[Optional] minimumFriendship', 'int'),
            # The <strong>unqualified</strong> object ID of the item to produce.
            ('itemId', 'string'),
        ]
    # As part of <see cref='T:StardewValley.GameData.FarmAnimals.FarmAnimalData' />, configures how the animal's shadow should be rendered.
    class FarmAnimalShadowData:
        _fields_ = [
            # Whether the shadow should be drawn.
            ('[Optional] visible', 'bool', True),
            # A pixel offset applied to the shadow position.
            ('[Optional] offset', 'Point?'),
            # The scale at which to draw the shadow, or <c>null</c> to apply the default logic.
            ('[Optional] scale', 'float?'),
        ]
    # As part of <see cref='T:StardewValley.GameData.FarmAnimals.FarmAnimalData' />, an alternate appearance for a farm animal.
    class FarmAnimalSkin:
        _fields_ = [
            # A key which uniquely identifies the skin for this animal type. The ID should only contain alphanumeric/underscore/dot characters. For custom skins, this should be prefixed with your mod ID like <c>Example.ModId_SkinName</c>.
            ('id', 'string'),
            # A multiplier for the probability to choose this skin when an animal is purchased. For example, <c>2</c> will double the chance this skin is selected relative to skins with the default <c>1</c>.
            ('[Optional] weight', 'float', 1.),
            # If set, overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture' />.
            ('[Optional] texture', 'string'),
            # If set, overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestedTexture' />.
            ('[Optional] harvestedTexture', 'string'),
            # If set, overrides <see cref='F:StardewValley.GameData.FarmAnimals.FarmAnimalData.BabyTexture' />.
            ('[Optional] babyTexture', 'string'),
        ]

class Fences:
    # The metadata for a placeable fence item.
    class FenceData:
        _fields_ = [
            # The initial health points for a fence when it's first placed, which affects how quickly it degrades. A fence loses 1/1440 points per in-game minute (roughly 0.04 points per hour or 0.5 points for a 12-hour day).
            ('health', 'int'),
            # The minimum amount added to the health when a fence is repaired by a player.
            # Repairing a fence sets its health to <c>2 � (<see cref='F:StardewValley.GameData.Fences.FenceData.Health' /> + Random(<see cref='F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMinimum' />, <see cref='F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMaximum' />))</c>.
            ('[Optional] repairHealthAdjustmentMinimum', 'float'),
            # The maximum amount added to the health when a fence is repaired by a player.
            # See remarks on <see cref='F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMinimum' />.
            ('[Optional] repairHealthAdjustmentMaximum', 'float'),
            # The asset name for the texture when the fence is placed. For example, the vanilla fences use individual tilesheets like <c>LooseSprites\Fence1</c> (wood fence).
            ('texture', 'string'),
            # The audio cue ID played when the fence is placed or repairs (e.g. axe used by Wood Fence).
            ('placementSound', 'string'),
            # The audio cue ID played when the fence is broken or picked up by the player. Defaults to <see cref='F:StardewValley.GameData.Fences.FenceData.PlacementSound' />.
            ('[Optional] removalSound', 'string'),
            # A list of tool IDs which can be used to break the fence, matching the keys in the <c>Data\Tools</c> asset.
            # A tool must match <see cref='F:StardewValley.GameData.Fences.FenceData.RemovalToolIds' /> <strong>or</strong> <see cref='F:StardewValley.GameData.Fences.FenceData.RemovalToolTypes' /> to be a valid removal tool. If both lists are null or empty, all tools can remove the fence.
            ('[Optional] removalToolIds', 'List<string>', []),
            # A list of tool class full names which can be used to break the fence, like <c>StardewValley.Tools.Axe</c>.
            # [inheritdoc cref='F:StardewValley.GameData.Fences.FenceData.RemovalToolIds' path='/remarks' />
            ('[Optional] removalToolTypes', 'List<string>', []),
            # The type of cosmetic debris particles to 'splash' from the tile when the fence is broken. The defined values are <c>0</c> (copper), <c>2</c> (iron), <c>4</c> (coal), <c>6</c> (gold), <c>8</c> (coins), <c>10</c> (iridium), <c>12</c> (wood), <c>14</c> (stone), <c>32</c> (big stone), and <c>34</c> (big wood). Default <c>14</c> (stone).
            ('[Optional] removalDebrisType', 'int', 14),
            # When an item like a torch is placed on the fence, the pixel offset to apply to its draw position.
            ('[Optional] heldObjectDrawOffset', 'Vector2', array([0., -20.])),
            # The X pixel offset to apply when the fence is oriented horizontally, with only one connected fence on the right. This fully replaces the X value specified by <see cref='F:StardewValley.GameData.Fences.FenceData.HeldObjectDrawOffset' /> when it's applied.
            ('[Optional] leftEndHeldObjectDrawX', 'float', -1.),
            # Equivalent to <see cref='F:StardewValley.GameData.Fences.FenceData.LeftEndHeldObjectDrawX' />, but when there's only one connected fence on the left.
            ('[Optional] rightEndHeldObjectDrawX', 'float'),
        ]

class FishPond:
    # The fish data for a Fish Pond building.
    @RType()
    class FishPondData:
        _fields_ = [
            # A unique identifier for the entry. The ID should only contain alphanumeric/underscore/dot characters. For custom fish pond entries, this should be prefixed with your mod ID like <c>Example.ModId_Fish.</c>
            ('id', 'string'),
            # The context tags for the fish item to configure. If this lists multiple context tags, an item must match all of them. If an item matches multiple entries, the first entry which matches is used.
            ('requiredTags', 'List<string>'),
            # The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.
            ('[Optional] precedence', 'int'),
            # The maximum number of fish which can be added to this pond.
            # This cannot exceed the global maximum of 10.
            ('[Optional] maxPopulation', 'int', -1),
            # The number of days needed to raise the population by one if there's enough room in the fish pond, or <c>-1</c> to choose a number automatically based on the fish value.
            ('[Optional] spawnTime', 'int', -1),
            # The minimum daily chance that this fish pond checks for output on a given day, as a value between 0 (never) and 1 (always).
            # The actual probability is lerped between <see cref='F:StardewValley.GameData.FishPonds.FishPondData.BaseMinProduceChance' /> and <see cref='F:StardewValley.GameData.FishPonds.FishPondData.BaseMaxProduceChance' /> based on the fish pond's population. If the min chance is 95+%, it's treated as the actual probability without lerping. If this check passes, output is only produced if one of the <see cref='F:StardewValley.GameData.FishPonds.FishPondData.ProducedItems' /> passes its checks too.
            ('[Optional] baseMinProduceChance', 'float', 0.15),
            # The maximum daily chance that this fish pond checks for output on a given day, as a value between 0 (never) and 1 (always).
            # [inheritdoc cref='F:StardewValley.GameData.FishPonds.FishPondData.BaseMinProduceChance' path='/remarks' />
            ('[Optional] baseMaxProduceChance', 'float', 0.95),
            # The custom water color to set, if applicable.
            ('[Optional] waterColor', 'List<#FishPondWaterColor>'),
            # The items that can be produced by the fish pond. When a fish pond is ready to produce output, it will check each entry in the list and take the first one that matches. If no entry matches, no output is produced.
            ('[Optional] producedItems', 'List<#FishPondReward>'),
            # The rules which determine when the fish pond population can grow, and the quests that must be completed to do so.
            ('[Optional] populationGates', 'Dictionary<int, List<string>>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.FishPonds.FishPondData' />, an item that can be produced by the fish pond.
    @RType()
    class FishPondReward(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # The minimum population needed before this output becomes available.
            ('[Optional] requiredPopulation', 'int'),
            # The percentage chance that this output is selected, as a value between 0 (never) and 1 (always). If multiple items pass, only the first one will be produced.
            ('[Optional] chance', 'float', 1.),
            # The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.
            ('[Optional] precedence', 'int'),
        ]
    # As part of <see cref='T:StardewValley.GameData.FishPonds.FishPondData' />, a color to apply to the water if its fields match.
    @RType()
    class FishPondWaterColor:
        _fields_ = [
            ('id', 'string'),
            # A tint color to apply to the water. This can be <c>CopyFromInput</c> (to use the input item's color), a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.
            ('color', 'string'),
            # The minimum population before this color applies.
            ('[Optional] minPopulation', 'int', 1),
            # The minimum population gate that was unlocked, or 0 for any value.
            ('[Optional] minUnlockedPopulationGate', 'int'),
            # A game state query which indicates whether this color should be applied. Defaults to always added.
            ('[Optional] condition', 'string'),
        ]

class FloorsAndPaths:
    # When drawing adjacent flooring items across multiple tiles, how the flooring sprite for each tile is selected.
    class FloorPathConnectType(Enum):
        # For normal floors, intended to cover large square areas. This uses some logic to draw inner corners.
        Default = 0
        # For floors intended to be drawn as narrow paths. These are drawn without any consideration for inner corners.
        Path = 1
        # For floors that have a decorative corner. Use <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.CornerSize' /> to change the size of this corner.
        CornerDecorated = 2
        # For floors that don't connect. When placed, one of the tiles is randomly selected.
        Random = 3
    # The metadata for a craftable floor or path item.
    class FloorPathData:
        _fields_ = [
            # A key which uniquely identifies this floor/path. The ID should only contain alphanumeric/underscore/dot characters. For vanilla floors and paths, this matches the spritesheet index in the <c>TerrainFeatures/Flooring</c> spritesheet; for custom floors and paths, this should be prefixed with your mod ID like <c>Example.ModId_FloorName.</c>
            ('id', 'string'),
            # The unqualified item ID for the corresponding object-type item.
            ('itemId', 'string'),
            # The asset name for the texture when the item is placed.
            ('texture', 'string'),
            # The top-left pixel position for the sprite within the <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture' /> spritesheet.
            ('corner', 'Point'),
            # Equivalent to <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture' />, but applied if the current location is in winter. Defaults to <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture' />.
            ('winterTexture', 'string'),
            # Equivalent to <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Corner' />, but used if <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.WinterTexture' /> is applied. Defaults to <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Corner' />.
            ('winterCorner', 'Point'),
            # The audio cue ID played when the item is placed (e.g. <c>axchop</c> used by Wood Floor).
            ('placementSound', 'string'),
            # The audio cue ID played when the item is picked up. Defaults to <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.PlacementSound' />.
            ('[Optional] removalSound', 'string'),
            # The type of cosmetic debris particles to 'splash' from the tile when the item is picked up. The defined values are <c>0</c> (copper), <c>2</c> (iron), <c>4</c> (coal), <c>6</c> (gold), <c>8</c> (coins), <c>10</c> (iridium), <c>12</c> (wood), <c>14</c> (stone), <c>32</c> (big stone), and <c>34</c> (big wood). Default <c>14</c> (stone).
            ('[Optional] removalDebrisType', 'int', 14),
            # The audio cue ID played when the player steps on the tile (e.g. <c>woodyStep</c> used by Wood Floor).
            ('footstepSound', 'string'),
            # When drawing adjacent flooring items across multiple tiles, how the flooring sprite for each tile is selected.
            ('[Optional] connectType', 'FloorPathConnectType'),
            # The type of shadow to draw under the tile sprite.
            ('[Optional] shadowType', 'FloorPathShadowType'),
            # The pixel size of the decorative inner corner when the <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathData.ConnectType' /> field is set to <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathConnectType.CornerDecorated' /> or <see cref='F:StardewValley.GameData.FloorsAndPaths.FloorPathConnectType.Default' />.
            ('[Optional] cornerSize', 'int', 4),
            # The speed boost applied to the player, on the farm only, when they're walking on paths of this type. Negative values are ignored. Set to <c>-1</c> to use the default for vanilla paths.
            ('[Optional] farmSpeedBuff', 'float', -1.),
        ]
    # How the shadow under a floor or path tile sprite should be drawn.
    class FloorPathShadowType(Enum):
        # Don't draw a shadow.
        None_ = 0
        # Draw a shadow under the entire tile.
        Square = 1
        # Draw a shadow that follows the lines of the path sprite.
        Contoured = 2

class FruitTrees:
    # Metadata for a fruit tree type.
    class FruitTreeData:
        _fields_ = [
            # The rules which override which locations the tree can be planted in, if applicable. These don't override more specific checks (e.g. not being plantable on stone).
            ('[Optional] plantableLocationRules', 'List<#PlantableRule>'),
            # A tokenizable string for the fruit tree display name, like 'Cherry' for a cherry tree.
            # This shouldn't include 'tree', which will be added automatically as needed.
            ('displayName', 'string'),
            # The seasons in which this tree bears fruit.
            ('seasons', 'List<#Season>'),
            # The fruit to produce. The first matching entry will be produced.
            ('fruit', 'List<FruitTreeFruitData>'),
            # The asset name for the texture for the tree's spritesheet.
            ('texture', 'string'),
            # The row index within the <see cref='P:StardewValley.GameData.FruitTrees.FruitTreeData.Texture' /> for the tree's sprites.
            ('textureSpriteRow', 'string'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.FruitTrees.FruitTreeData' />, a possible item to produce as fruit.
    class FruitTreeFruitData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # If set, the specific season when this fruit can be produced. For more complex conditions, see <see cref='P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition' />.
            ('[Optional] season', '#Season?'),
            # The probability that the item will be produced, as a value between 0 (never) and 1 (always).
            ('[Optional] chance ', 'float', 1.),
        ]

class GarbageCans:
    # The data for in-game garbage cans.
    class GarbageCanData:
        _fields_ = [
            # The default probability that any item will be found when searching a garbage can, unless overridden by <see cref='F:StardewValley.GameData.GarbageCans.GarbageCanEntryData.BaseChance' />.
            ('defaultBaseChance', 'float', 0.2),
            # The items to try before <see cref='F:StardewValley.GameData.GarbageCans.GarbageCanData.GarbageCans' /> and <see cref='F:StardewValley.GameData.GarbageCans.GarbageCanData.AfterAll' />, subject to the garbage can's base chance.
            ('beforeAll', 'List<GarbageCanItemData>'),
            # The items to try if neither <see cref='F:StardewValley.GameData.GarbageCans.GarbageCanData.BeforeAll' /> nor <see cref='F:StardewValley.GameData.GarbageCans.GarbageCanData.GarbageCans' /> returned a value.
            ('afterAll', 'List<GarbageCanItemData>'),
            # The metadata for specific garbage can IDs.
            ('garbageCans', 'Dictionary<string, GarbageCanEntryData>'),
        ]
    # Metadata for a specific in-game garbage can.
    class GarbageCanEntryData:
        _fields_ = [
            # The probability that any item will be found when the garbage can is searched, or <c>-1</c> to use <see cref='F:StardewValley.GameData.GarbageCans.GarbageCanData.DefaultBaseChance' />.
            ('[Optional] baseChance', 'float', -1.),
            # The items that may be found by rummaging in the garbage can.
            ('items', 'List<GarbageCanItemData>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.GarbageCans.GarbageCanData' />, an item that can be found by rummaging in the garbage can.
    # Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.
    class GarbageCanItemData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # Whether to check this item even if the <see cref='F:StardewValley.GameData.GarbageCans.GarbageCanEntryData.BaseChance' /> didn't pass.
            ('[Optional] ignoreBaseChance', 'bool'),
            # Whether to treat this item as a 'mega success' if it's selected, which plays a special <c>crit</c> sound and bigger animation.
            ('[Optional] isMegaSuccess', 'bool'),
            # Whether to treat this item as an 'double mega success' if it's selected, which plays an explosion sound and dramatic animation.
            ('[Optional] isDoubleMegaSuccess', 'bool'),
            # Whether to add the item to the player's inventory directly, opening an item grab menu if they don't have room in their inventory. If false, the item will be dropped on the ground next to the garbage can instead.
            ('[Optional] addToInventoryDirectly', 'bool'),
            # Whether to splits stacks into multiple debris items, instead of a single item with a stack size.
            ('[Optional] createMultipleDebris', 'bool'),
        ]

class GiantCrops:
    # A custom giant crop that may spawn in-game.
    class GiantCropData:
        _fields_ = [
            # The qualified or unqualified harvest item ID of the crops from which this giant crop can grow. If multiple giant crops have the same item ID, the first one whose <see cref='F:StardewValley.GameData.GiantCrops.GiantCropData.Chance' /> matches will be used.
            ('fromItemId', 'string'),
            # The items to produce when this giant crop is broken. All matching items will be produced.
            ('harvestItems', 'List<GiantCropHarvestItemData>'),
            # The asset name for the texture containing the giant crop's sprite.
            ('texture', 'string'),
            # The top-left pixel position of the sprite within the <see cref='F:StardewValley.GameData.GiantCrops.GiantCropData.Texture' />. Defaults to (0, 0).
            ('[Optional] texturePosition', 'Point'),
            # The area in tiles occupied by the giant crop. This affects both its sprite size (which should be 16 pixels per tile) and the grid of crops needed for it to grow. Note that giant crops are drawn with an extra tile's height.
            ('[Optional] tileSize', 'Point', Point(3, 3)),
            # The health points that must be depleted to break the giant crop. The number of points depleted per axe chop depends on the axe power level.
            ('[Optional] health', 'int', 3),
            # The percentage chance a given grid of crops will grow into the giant crop each night, as a value between 0 (never) and 1 (always).
            ('[Optional] chance', 'float', 0.01),
            # A game state query which indicates whether the giant crop can be selected. Defaults to always enabled.
            ('[Optional] condition', 'string'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.GiantCrops.GiantCropData' />, a possible item to produce when it's harvested.
    class GiantCropHarvestItemData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # The probability that the item will be produced, as a value between 0 (never) and 1 (always).
            ('[Optional] chance', 'float', 1.),
            # Whether to drop this item only for the Shaving enchantment (true), only when the giant crop is broken (false), or both (null).
            ('[Optional] forShavingEnchantment', 'bool?'),
            # If set, the minimum stack size when this item is dropped due to the Shaving enchantment, scaled to the tool's power level.
            #  <para>This value is multiplied by the health deducted by the tool hit which triggered the enchantment. For example, an iridium tool that reduced the giant crop's health by 3 points will produce three times this value per hit.</para>
            #  <para>If the scaled min and max are both set, the stack size is randomized between them. If only one is set, it's applied as a limit after the generic fields. If neither is set, the generic fields are applied as usual without scaling.</para>
            ('[Optional] scaledMinStackWhenShaving', 'int?', int(2)),
            # If set, the maximum stack size when this item is dropped due to the Shaving enchantment, scaled to the tool's power level.
            # [inheritdoc cref='P:StardewValley.GameData.GiantCrops.GiantCropHarvestItemData.ScaledMinStackWhenShaving' path='/remarks' />
            ('[Optional] scaledMaxStackWhenShaving', 'int?', int(2)),
        ]

class HomeRenovations:
    # A renovation which can be applied to customize the player's farmhouse after the second farmhouse upgrade.
    class HomeRenovation:
        _fields_ = [
            # A translation key in the form <c>{asset name}:{key}</c>. The translation text should contain three slash-delimited fields: the translated display name, translated description, and the action message shown to ask the player which area to renovate.
            ('textStrings', 'string'),
            # The animation to play when the renovation is applied. The possible values are <c>destroy</c> or <c>build</c>. Any other value defaults to <c>build</c>.
            ('animationType', 'string'),
            # Whether to prevent the player from applying the renovations if there are any players, NPCs, items, etc within the target area.
            ('checkForObstructions', 'bool'),
            # A price to charge for this renovation (default free). Negative values will act as a refund the player (typically used when reverting a renovation).
            ('[Optional] price', 'int'),
            # A unique string ID which links this renovation to its counterpart add/remove renovation. Add/remove renovations for the same room should have the same ID.
            ('[Optional] roomId', 'string'),
            # The criteria that must match for the renovation to appear as an option.
            ('requirements', 'List<RenovationValue>'),
            # The actions to perform after the renovation is applied.
            ('renovateActions', 'List<RenovationValue>'),
            # The tile areas within the farmhouse where the renovation can be placed.
            ('[Optional] rectGroups', 'List<RectGroup>'),
            # A dynamic area to add to the <see cref='F:StardewValley.GameData.HomeRenovations.HomeRenovation.RectGroups' /> field, if any. The only supported value is <c>crib</c>, which is the farmhouse area containing the cribs.
            ('[Optional] specialRect', 'string'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.HomeRenovations.RectGroup' />, a tile area within the farmhouse.
    class Rect:
        _fields_ = [
            # The top-left tile X position.
            ('x', 'int'),
            # The top-left tile Y position.
            ('y', 'int'),
            # The area width in tiles.
            ('width', 'int'),
            # The area height in tiles.
            ('height', 'int'),
        ]
    # As part of <see cref='T:StardewValley.GameData.HomeRenovations.HomeRenovation' />, the farmhouse areas where a renovation can be applied.
    class RectGroup:
        _fields_ = [
            # The tile areas within the farmhouse where the renovation can be applied.
            ('rects', 'List<Rect>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.HomeRenovations.HomeRenovation' />, a renovation requirement or action.
    class RenovationValue:
        _fields_ = [
            # The requirement or action type. This can be <c>Mail</c> (check/change a mail flag for the current player) or <c>Value</c> (check/set a C# field on the farmhouse instance).
            ('type', 'string'),
            # The mail flag (if <see cref='F:StardewValley.GameData.HomeRenovations.RenovationValue.Type' /> is <c>Mail</c>) or field name (if <see cref='F:StardewValley.GameData.HomeRenovations.RenovationValue.Type' /> is <c>Value</c>) to check or set.
            ('key', 'string'),
            # The effect of this field depends on whether this is used in <see cref='F:StardewValley.GameData.HomeRenovations.HomeRenovation.Requirements' /> or <see cref='F:StardewValley.GameData.HomeRenovations.HomeRenovation.RenovateActions' />, and the value of <see cref='F:StardewValley.GameData.HomeRenovations.RenovationValue.Type' />:
            # <list type='bullet'>
            #   <item><description>
            #     For a renovation requirement:
            #     <list type='bullet'>
            #       <item><description>If the <see cref='F:StardewValley.GameData.HomeRenovations.RenovationValue.Type' /> is <c>Mail</c>, either <c>'0'</c> (player must not have the flag) or <c>'1'</c> (player must have it).</description></item>
            #       <item><description>If the <see cref='F:StardewValley.GameData.HomeRenovations.RenovationValue.Type' /> is <c>Value</c>, the required field value. This can be prefixed with <c>!</c> to require any value <em>except</em> this one.</description></item>
            #     </list>
            #   </description></item>
            #   <item><description>
            #     For a renovate action:
            #     <list type='bullet'>
            #       <item><description>If the <see cref='F:StardewValley.GameData.HomeRenovations.RenovationValue.Type' /> is <c>Mail</c>, either <c>'0'</c> (remove the mail flag) or <c>'1'</c> (add it).</description></item>
            #       <item><description>If the <see cref='F:StardewValley.GameData.HomeRenovations.RenovationValue.Type' /> is <c>Value</c>, either the integer value to set, or the exact string <c>'selected'</c> to set it to the index of the applied renovation.</description></item>
            #     </list>
            #   </description></item>
            # </list>
            ('value', 'string'),
        ]

class LocationContexts:
    # A world area which groups multiple in-game locations with shared settings and metadata.
    class LocationContextData:
        _fields_ = [
            # The season which is always active for locations within this context. For example, setting <see cref='F:StardewValley.Season.Summer' /> will make it always summer there regardless of the calendar season. If not set, the calendar season applies.
            ('[Optional] seasonOverride', '#Season?'),
            # The cue ID for the music to play when the player is in the location, unless overridden by a <c>Music</c> map property. Despite the name, this has a higher priority than the seasonal music fields like <see cref='!:SpringMusic' />. Ignored if omitted.
            ('[Optional] defaultMusic', 'string'),
            # A game state query which returns whether the <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.DefaultMusic' /> field should be applied (if more specific music isn't playing). Defaults to always true.
            ('[Optional] defaultMusicCondition', 'string'),
            # When the player warps and the music changes, whether to silence the music and play the ambience (if any) until the next warp. This is similar to the default valley locations.
            ('[Optional] defaultMusicDelayOneScreen', 'bool', True),
            # A list of cue IDs to play before noon unless it's raining, there's a <c>Music</c> map property, or the context has a <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.DefaultMusic' /> value. If multiple values are specified, the game will play one per day in sequence.
            ('[Optional] music', 'List<Locations.LocationMusicData>', []),
            # The cue ID for the background ambience to before dark, when there's no music active. Defaults to none.
            ('[Optional] dayAmbience', 'string'),
            # The cue ID for the background ambience to after dark, when there's no music active. Defaults to none.
            ('[Optional] nightAmbience', 'string'),
            # Whether to play random ambience sounds when outdoors depending on factors like the season and time of day (e.g. birds and crickets). This is unrelated to the <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.DayAmbience' /> and <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.NightAmbience' /> fields.
            ('[Optional] playRandomAmbientSounds', 'bool', True),
            # Whether a rain totem can be used to force rain in this context tomorrow.
            ('[Optional] allowRainTotem', 'bool', True),
            # If set, using a rain totem within the context changes the weather in the given context instead.
            # This is ignored if <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.AllowRainTotem' /> is false.
            ('[Optional] rainTotemAffectsContext', 'string'),
            # The weather rules to apply for locations in this context (ignored if <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.CopyWeatherFromLocation' /> is set). Defaults to always sunny. If multiple are specified, the first matching weather is applied.
            ('[Optional] weatherConditions', 'List<WeatherCondition>', []),
            # The ID of the location context from which to inherit weather, if any. If this is set, the <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.WeatherConditions' /> field is ignored.
            ('[Optional] copyWeatherFromLocation', 'string'),
            # <para>When the player gets knocked out in combat, the locations where they can wake up. If multiple locations match, the first match will be used. If none match, the player will wake up at Harvey's clinic.</para>
            # <para>If the selected location has a standard event with the exact key <c>PlayerKilled</c>, that event will play when the player wakes up and the game will apply the lost items or gold logic. The game won't track this event, so it'll repeat each time the player is revived. If there's no such event, the player will wake up without an event, and no items or gold will be lost.</para>
            ('[Optional] reviveLocations', 'List<ReviveLocation>'),
            # When the player passes out (due to exhaustion or at 2am) in this context, the maximum amount of gold lost. If set to <c>-1</c>, uses the same value as the default context.
            ('[Optional] maxPassOutCost', 'int', -1),
            # When the player passes out (due to exhaustion or at 2am) in this context, the possible letters to add to their mailbox (if they haven't received it before).
            # If multiple letters are valid, one will be chosen randomly (unless one of them specifies <see cref='F:StardewValley.GameData.LocationContexts.PassOutMailData.SkipRandomSelection' />).
            ('[Optional] passOutMail', 'List<PassOutMailData>'),
            # When the player passes out (due to exhaustion or at 2am), the locations where they can wake up.
            # <para>If multiple locations match, the first match will be used. If none match, the player will wake up in their bed at home.</para>
            # <para>The selected location must either have a bed or the <c>AllowWakeUpWithoutBed: true</c> map property, otherwise the player will be warped home instead.</para>
            ('[Optional] passOutLocations', 'List<ReviveLocation>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.LocationContexts.LocationContextData' />, a letter added to the player's mailbox when they pass out (due to exhaustion or at 2am).
    class PassOutMailData:
        _fields_ = [
            # A unique string ID for this entry within the current location context.
            ('id', 'string'),
            # A game state query which indicates whether this entry is active. Defaults to always true.
            ('[Optional] condition', 'string'),
            # The letter ID to add.
            # <para>The game will look for an existing letter ID in the <c>Data/mail</c> asset in this order (where <c>{billed}</c> is <c>Billed</c> if they lost gold or <c>NotBilled</c> otherwise, and <c>{gender}</c> is <c>Female</c> or <c>Male</c>): <c>{letter id}_{billed}_{gender}</c>, <c>{letter id}_{billed}</c>, <c>{letter id}</c>. If no match is found, the game will send <c>passedOut2</c> instead.</para>
            # <para>If the mail ID starts with <c>passedOut</c>, <c>{0}</c> in the letter text will be replaced with the gold amount lost, and the letter won't appear on the collections tab.</para>
            ('mail', 'string'),
            # The maximum amount of gold lost. This is applied after the context's <see cref='F:StardewValley.GameData.LocationContexts.LocationContextData.MaxPassOutCost' /> (i.e. the context's value is used to calculate the random amount, then this field caps the result). Defaults to unlimited.
            ('[Optional] maxPassOutCost', 'int', -1),
            # When multiple mail entries match, whether to send this one instead of choosing one randomly.
            ('[Optional] skipRandomSelection', 'bool'),
        ]
    # As part of <see cref='T:StardewValley.GameData.LocationContexts.LocationContextData' />, the locations where a player wakes up after passing out or getting knocked out.
    class ReviveLocation:
        _fields_ = [
            # A unique string ID for this entry within the current location context.
            ('id', 'string'),
            # A game state query which indicates whether this entry is active. Defaults to always applied.
            ('[Optional] condition', 'string'),
            # The internal location name.
            ('location', 'string'),
            # The tile position within the location.
            ('position', 'Point'),
        ]
    # As part of <see cref='T:StardewValley.GameData.LocationContexts.LocationContextData' />, a weather rule to apply for locations in this context.
    class WeatherCondition:
        _fields_ = [
            # A unique string ID for this entry within the current location context.
            ('string Id', 'string'),
            # A game state query which indicates whether to apply the weather. Defaults to always applied.
            ('[Optional] string Condition', 'string'),
            # The weather ID to set.
            ('string Weather', 'string'),
        ]

class Locations:
    # As part of <see cref='T:StardewValley.GameData.Locations.LocationData' />, an item that can be found by digging an artifact dig spot.
    # Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.
    class ArtifactSpotDropData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # A probability that this item will be found, as a value between 0 (never) and 1 (always).
            ('[Optional] chance', 'double', 1.0),
            # Whether the item may drop twice if the player is using a hoe with the Generous enchantment.
            ('[Optional] applyGenerousEnchantment', 'bool', True),
            # Whether to split the dropped item stack into multiple floating debris that each have a stack size of one.
            ('[Optional] oneDebrisPerDrop', 'bool', True),
            # The order in which this drop should be checked, where 0 is the default value used by most drops. Drops within each precedence group are checked in the order listed.
            ('[Optional] precedence', 'int'),
            # Whether to continue searching for more items after this item is dropped, so the artifact spot may drop multiple items.
            ('[Optional] continueOnDrop', 'bool'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Locations.LocationData' />, the data to use to create a location.
    class CreateLocationData:
        _fields_ = [
            # The asset name for the map to use for this location.
            ('mapPath', 'string'),
            # The full name of the C# location class to create. This must be one of the vanilla types to avoid a crash when saving. Defaults to a generic <c>StardewValley.GameLocation</c>.
            # Whether this location is always synchronized to farmhands in multiplayer, even if they're not in the location. Any location which allows building cabins <strong>must</strong> have this enabled to avoid breaking game logic.
            ('[Optional] alwaysActive', 'bool'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Locations.LocationData' />, a distinct fish area within the location which may have its own fish (via <see cref='P:StardewValley.GameData.Locations.SpawnFishData.FishAreaId' />) or crab pot catches.
    class FishAreaData:
        _fields_ = [
            # A tokenizable string for the translated area name, if any.
            ('[Optional] displayName', 'string'),
            # If set, the tile area within the location where the crab pot must be placed.
            ('[Optional] position', 'Rectangle?'),
            # The fish types that can be caught with crab pots in this area.
            # These will be matched against field index 4 in <c>Data/Fish</c> for crab pot fish. If this list is null or empty, it'll default to <c>freshwater</c>.
            ('[Optional] crabPotFishTypes', 'List<string>', []),
            # The chance that crab pots will find junk instead of a fish in this area, if the player doesn't have the Mariner profession.
            ('[Optional] crabPotJunkChance', 'float', 0.2),
        ]
    # The data for a location to add to the game.
    class LocationData:
        _fields_ = [
            # A tokenizable string for the translated location name. This is used anytime the location name is shown in-game for base game logic or mods. If omitted, the location will default to its internal name (i.e. the key in <c>Data/AdditionalLocationData</c>).
            ('[Optional] displayName', 'string'),
            # The default tile position where the player should be placed when they arrive in the location, if arriving from a warp that didn't specify a tile position.
            ('[Optional] defaultArrivalTile', 'Point?'),
            # Whether NPCs should ignore this location when pathfinding between locations.
            ('[Optional] excludeFromNpcPathfinding', 'bool'),
            # If set, the location will be created automatically when the save is loaded using this data.
            ('[Optional] createOnLoad', 'CreateLocationData'),
            # The former location names which may appear in save data.
            # If a location in save data has a name which (a) matches one of these values and (b) doesn't match the name of a loaded location, its data will be loaded into this location instead.
            ('[Optional] formerLocationNames', 'List<string>', []),
            # Whether crops and trees can be planted and grown here by default, unless overridden by their plantable rules. If omitted, defaults to <c>true</c> on the farm and <c>false</c> elsewhere.
            ('[Optional] canPlantHere', 'bool?'),
            # Whether green rain trees and debris can spawn here by default.
            ('[Optional] canHaveGreenRainSpawns', 'bool', True),
            # The items that can be found when digging artifact spots in the location.
            #   <para>The items that can be dug up in a location are decided by combining this field with the one from the <c>Default</c> entry, sorting them by <see cref='P:StardewValley.GameData.Locations.ArtifactSpotDropData.Precedence' />, and taking the first drop whose fields match. Items with the same precedence are checked in the order listed.</para>
            #   <para>For consistency, vanilla artifact drops prefer using these precedence values:</para>
            #   <list type='bullet'>
            #     <item><description>-1000: location items which should override the global priority items (e.g. fossils on Ginger Island);</description></item>
            #     <item><description>-100: global priority items (e.g. Qi Beans);</description></item>
            #     <item><description>0: normal items;</description></item>
            #     <item><description>100: global fallback items (e.g. clay).</description></item>
            #   </list>
            ('[Optional] artifactSpots', 'List<ArtifactSpotDropData>', []),
            # The distinct fishing areas within the location.
            # These can be referenced by <see cref='F:StardewValley.GameData.Locations.LocationData.Fish' /> via <see cref='P:StardewValley.GameData.Locations.SpawnFishData.FishAreaId' />, and determine which fish are collected by crab pots.
            ('[Optional] fishAreas', 'Dictionary<string, FishAreaData>', []),
            # The items that can be found by fishing in the location.
            #   <para>The items to catch in a location are decided by combining this field with the one from the <c>Default</c> entry, sorting them by <see cref='P:StardewValley.GameData.Locations.SpawnFishData.Precedence' />, and taking the first fish whose fields match. Items with the same precedence are shuffled randomly.</para>
            #   <para>For consistency, vanilla fish prefer precedence values in these ranges:</para>
            #   <list type='bullet'>
            #     <item><description>-1100 to -1000: global priority items (e.g. Qi Beans);</description></item>
            #     <item><description>-200 to -100: unique location items (e.g. legendary fish or secret items);</description></item>
            #     <item><description>-50 to -1: normal high-priority items;</description></item>
            #     <item><description>0: normal items;</description></item>
            #     <item><description>1 to 100: normal low-priority items;</description></item>
            #     <item><description>1000+: global fallback items (e.g. trash).</description></item>
            #   </list>
            ('[Optional] fish', 'List<SpawnFishData>', []),
            # The forage objects that can spawn in the location.
            ('[Optional] forage', 'List<SpawnForageData>', []),
            # The minimum number of weeds to spawn in a day.
            ('[Optional] minDailyWeeds', 'int', 2),
            # The maximum number of weeds to spawn in a day.
            ('[Optional] maxDailyWeeds', 'int', 5),
            # A multiplier applied to the number of weeds spawned on the first day of the year.
            ('[Optional] firstDayWeedMultiplier', 'int', 15),
            # The minimum forage to try spawning in one day, if the location has fewer than <see cref='F:StardewValley.GameData.Locations.LocationData.MaxSpawnedForageAtOnce' /> forage.
            ('[Optional] minDailyForageSpawn', 'int', 1),
            # The maximum forage to try spawning in one day, if the location has fewer than <see cref='F:StardewValley.GameData.Locations.LocationData.MaxSpawnedForageAtOnce' /> forage.
            ('[Optional] maxDailyForageSpawn', 'int', 4),
            # The maximum number of spawned forage that can be present at once on the map before they stop spawning.
            ('[Optional] maxSpawnedForageAtOnce', 'int', 6),
            # The probability that digging a tile will produce clay, as a value between 0 (never) and 1 (always).
            ('[Optional] chanceForClay', 'double', 0.03),
            # The music to play when the player enters the location (subject to the other fields like <see cref='F:StardewValley.GameData.Locations.LocationData.MusicContext' />).
            # The first matching entry is used. If none match, falls back to <see cref='F:StardewValley.GameData.Locations.LocationData.MusicDefault' />.s
            ('[Optional] music', 'List<LocationMusicData>', []),
            # The music to play if none of the options in <see cref='F:StardewValley.GameData.Locations.LocationData.Music' /> matched.
            # If this is null, falls back to the <c>Music</c> map property (if set).
            ('[Optional] musicDefault', 'string'),
            # The music context for this location. The recommended values are <c>Default</c> or <c>SubLocation</c>.
            ('[Optional] musicContext', 'GameData.MusicContext'),
            # Whether to ignore the <c>Music</c> map property when it's raining in this location.
            ('[Optional] musicIgnoredInRain', 'bool'),
            # Whether to ignore the <c>Music</c> map property when it's spring in this location.
            ('[Optional] musicIgnoredInSpring', 'strbooling'),
            # Whether to ignore the <c>Music</c> map property when it's summer in this location.
            ('[Optional] musicIgnoredInSummer', 'bool'),
            # Whether to ignore the <c>Music</c> map property when it's fall in this location.
            ('[Optional] musicIgnoredInFall', 'bool'),
            # Whether to ignore the <c>Music</c> map property when it's fall and windy weather in this location.
            ('[Optional] musicIgnoredInFallDebris', 'bool'),
            # Whether to ignore the <c>Music</c> map property when it's winter in this location.
            ('[Optional] musicIgnoredInWinter', 'bool'),
            # Whether to use the same music behavior as Pelican Town's music: it will start playing after the day music has finished, and will continue playing while the player travels through indoor areas, but will stop when entering another outdoor area that isn't marked with the same <c>Music</c> map property and <c>MusicIsTownTheme</c> data field.
            ('[Optional] musicIsTownTheme', 'bool'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Locations.LocationData' />, a music cue to play when the player enters the location (subject to the other fields like <see cref='F:StardewValley.GameData.Locations.LocationData.MusicContext' />).
    class LocationMusicData:
        # The backing field for <see cref='P:StardewValley.GameData.Locations.LocationMusicData.Id' />.
        _idImpl: str = None
        # A unique string ID for this track within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_TrackName</c>. Defaults to <see cref='P:StardewValley.GameData.Locations.LocationMusicData.Track' /> if omitted.
        @property
        def id(self) -> str: return self._idImpl or self.track
        @id.setter
        def id(self, value: str) -> None: self._idImpl = value
        _fields_ = [
            # A unique string ID for this track within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_TrackName</c>. Defaults to <see cref='P:StardewValley.GameData.Locations.LocationMusicData.Track' /> if omitted.
            ('[Optional] #id', 'string'),
            # The audio track ID to play, or <c>null</c> to stop music.
            ('track', 'string'),
            # A game state query which indicates whether the music should be played. Defaults to true.
            ('[Optional] condition', 'string'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Locations.LocationData' />, an item that can be found by fishing in the location.
    #   Fish spawns have a few special constraints:
    #   <list type='bullet'>
    #     <item><description>Only one fish can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</description></item>
    #     <item><description>This must return an item of type <c>StardewValley.Object</c> or one of its subclasses.</description></item>
    #     <item><description>Entries using an item query (instead of an item ID) are ignored for the fishing TV channel hints.</description></item>
    #   </list>
    class SpawnFishData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # The probability that the fish will spawn, as a value between 0 (never) and 1 (always).
            ('[Optional] chance', 'float', 1.),
            # If set, the specific season when the fish should apply. For more complex conditions, see <see cref='P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition' />.
            ('[Optional] season', '#Season?'),
            # If set, the fish area (as defined by <see cref='F:StardewValley.GameData.Locations.LocationData.FishAreas' /> in which the fish can be caught. If omitted, it can be caught in all areas.
            ('[Optional] fishAreaId', 'string'),
            # If set, the tile area within the location where the bobber must land to catch the fish.
            ('[Optional] bobberPosition', 'Rectangle?'),
            # If set, the tile area within the location where the player must be standing to catch the fish.
            ('[Optional] playerPosition', 'Rectangle?'),
            # The minimum fishing level needed for the fish to appear.
            ('[Optional] minFishingLevel', 'int'),
            # The minimum distance from the shore (measured in tiles) at which the fish can be caught, where zero is water directly adjacent to shore.
            ('[Optional] minDistanceFromShore', 'int'),
            # The maximum distance from the shore (measured in tiles) at which the fish can be caught, where zero is water directly adjacent to shore, or -1 for no maximum.
            ('[Optional] maxDistanceFromShore', 'int', -1),
            # Whether to increase the <see cref='P:StardewValley.GameData.Locations.SpawnFishData.Chance' /> by an amount equal to the player's daily luck.
            ('[Optional] applyDailyLuck', 'bool'),
            # A flat increase to the spawn chance when the player has the Curiosity Lure equipped, or <c>-1</c> to apply the default behavior. This affects both the <see cref='P:StardewValley.GameData.Locations.SpawnFishData.Chance' /> field and the <c>Data\Fish</c> chance, if applicable.
            ('[Optional] curiosityLureBuff', 'float', -1.),
            # A flat increase to the spawn chance when the player has a specific bait equipped which targets this fish.
            ('[Optional] specificBaitBuff', 'float'),
            # A multiplier applied to the spawn chance when the player has a specific bait equipped which targets this fish.
            ('[Optional] specificBaitMultiplier', 'float', 1.66),
            # The maximum number of times this fish can be caught by each player.
            ('[Optional] catchLimit', 'int', -1),
            # Whether the player can catch this fish using a training rod. This can be <c>true</c> (always allowed), <c>false</c> (never allowed), or <c>null</c> (apply default logic, i.e. allowed for difficulty ratings under 50).
            ('[Optional] canUseTrainingRod', 'bool?'),
            # Whether this is a 'boss fish' in the fishing minigame. This shows a crowned fish sprite in the minigame, multiplies the XP gained by five, and hides it from the F.I.B.S. TV channel.
            ('[Optional] isBossFish', 'bool'),
            # The mail flag to set for the current player when this fish is successfully caught.
            ('[Optional] setFlagOnCatch', 'string'),
            # Whether the player must fish with Magic Bait for this fish to spawn.
            ('[Optional] requireMagicBait', 'bool'),
            # The order in which this fish should be checked, where 0 is the default value used by most fish. Fish within each precedence group are shuffled randomly.
            ('[Optional] precedence', 'int'),
            # Whether to ignore any fish requirements listed for the ID in <c>Data/Fish</c>.
            # The <c>Data/Fish</c> requirements are ignored regardless of this field for non-object (<c>(O)</c>)-type items, or objects with an ID not listed in <c>Data/Fish</c>.
            ('[Optional] ignoreFishDataRequirements', 'bool'),
            # Whether this fish can be spawned in another location via the <c>LOCATION_FISH</c> item query.
            ('[Optional] canBeInherited', 'bool', True),
            # Changes to apply to the <see cref='P:StardewValley.GameData.Locations.SpawnFishData.Chance' />.
            ('[Optional] chanceModifiers', 'List<#QuantityModifier>'),
            # How multiple <see cref='P:StardewValley.GameData.Locations.SpawnFishData.ChanceModifiers' /> should be combined.
            ('[Optional] chanceModifierMode', '#QuantityModifier.QuantityModifierMode'),
            # How much to increase the <see cref='P:StardewValley.GameData.Locations.SpawnFishData.Chance' /> per player's Luck level
            ('[Optional] chanceBoostPerLuckLevel', 'float'),
            # If true, the chance roll will use a seed value based on the number of fish caught.
            ('[Optional] useFishCaughtSeededRandom', 'bool'),
        ]
        # Get the probability that the fish will spawn, adjusted for modifiers and equipment.
        # <param name='hasCuriosityLure'>Whether the player has the Curiosity Lure equipped.</param>
        # <param name='dailyLuck'>The player's daily luck value.</param>
        # <param name='luckLevel'>The player's current luck level.</param>
        # <param name='applyModifiers'>Apply quantity modifiers to the given value.</param>
        # <param name='isTargetedWithBait'>Whether the player has a specific bait equipped which targets this fish.</param>
        # <returns>Returns a value between 0 (never) and 1 (always).</returns>
        def getChance(self, hasCuriosityLure: bool, dailyLuck: float, luckLevel: int, applyModifiers: callable, isTargetedWithBait: bool = False) -> float:
            num = self.chance
            if hasCuriosityLure and self.curiosityLureBuff > 0.0: num += self.curiosityLureBuff
            if self.applyDailyLuck: num += dailyLuck
            if ((1 if len(self.chanceModifiers) > 0 else 0) if self.chanceModifiers else 0) != 0: num = applyModifiers(num, self.chanceModifiers, self.chanceModifierMode)
            if isTargetedWithBait: num = num * self.specificBaitMultiplier + self.specificBaitBuff
            return num + self.chanceBoostPerLuckLevel * luckLevel
    # As part of <see cref='T:StardewValley.GameData.Locations.LocationData' />, a forage object that can spawn in the location.
    #   Forage spawns have a few special constraints:
    #   <list type='bullet'>
    #     <item><description>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</description></item>
    #     <item><description>If this returns a null or non-<c>StardewValley.Object</c> item, the game will skip that spawn opportunity (and log a warning for a non-null invalid item type).</description></item>
    #     <item><description>The <see cref='P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition' /> field is checked once right before spawning forage, to build the list of possible forage spawns. It's not checked again for each forage spawn; use the <see cref='P:StardewValley.GameData.Locations.SpawnForageData.Chance' /> instead for per-spawn probability.</description></item>
    #   </list>
    class SpawnForageData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # The probability that the forage will spawn if it's selected, as a value between 0 (never) and 1 (always). If this check fails, that spawn opportunity will be skipped.
            ('[Optional] chance', 'double', 1.0),
            # If set, the specific season when the forage should apply. For more complex conditions, see <see cref='P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition' />.
            ('[Optional] season', '#Season?'),
        ]

class Machines:
    # The behavior and metadata for a machine which takes input, produces output, or both.
    class MachineData:
        _fields_ = [
            # Whether to force adding the <c>machine_input</c> context tag, which indicates the machine can accept input.
            # If false, this will be set automatically if any <see cref='F:StardewValley.GameData.Machines.MachineData.OutputRules' /> use the <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine' /> trigger.
            ('[Optional] hasInput', 'bool'),
            # Whether to force adding the <c>machine_output</c> context tag, which indicates the machine can produce output.
            # If false, this will be set automatically if there are <see cref='F:StardewValley.GameData.Machines.MachineData.OutputRules' />.
            ('[Optional] hasOutput', 'bool'),
            # A C# method invoked when the player interacts with the machine while it doesn't have output ready to harvest.
            # <strong>This is an advanced field. Most machines shouldn't use this.</strong> This must be specified in the form <c>{full type name}: {method name}</c> (like <c>StardewValley.Object, Stardew Valley: SomeInteractMethod</c>). The method must be static, take three arguments (<c>Object machine, GameLocation location, Farmer player</c>), and return a boolean indicating whether the interaction succeeded.
            ('[Optional] interactMethod', 'string'),
            # The rules which define how to process input items and produce output.
            ('[Optional] outputRules', 'List<MachineOutputRule>'),
            # A list of extra items required before <see cref='F:StardewValley.GameData.Machines.MachineData.OutputRules' /> will be checked. If specified, every listed item must be present in the player, hopper, or chest inventory (depending how the machine is being loaded).
            ('[Optional] additionalConsumedItems', 'List<MachineItemAdditionalConsumedItems>'),
            # A list of cases when the machine should be paused, so the timer on any item being produced doesn't decrement.
            ('[Optional] preventTimePass', 'List<MachineTimeBlockers>'),
            # Changes to apply to the processing time before output is ready.
            # If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double processing time will quadruple it).
            ('[Optional] readyTimeModifiers', 'List<#QuantityModifier>'),
            # How multiple <see cref='F:StardewValley.GameData.Machines.MachineData.ReadyTimeModifiers' /> should be combined.
            ('[Optional] readyTimeModifierMode', '#QuantityModifier.QuantityModifierMode'),
            # A tokenizable string for the message shown in a toaster notification if the player tries to input an item that isn't accepted by the machine.
            ('[Optional] invalidItemMessage', 'string'),
            # An extra condition that must be met before <see cref='F:StardewValley.GameData.Machines.MachineData.InvalidItemMessage' /> is shown.
            ('[Optional] invalidItemMessageCondition', 'string'),
            # A tokenizable string for the message shown in a toaster notification if the input inventory doesn't contain this item, unless overridden by <see cref='F:StardewValley.GameData.Machines.MachineOutputRule.InvalidCountMessage' /> under <see cref='F:StardewValley.GameData.Machines.MachineData.OutputRules' />.
            #   This can use extra tokens:
            #   <list type='bullet'>
            #     <item><description><c>[ItemCount]</c>: the number of remaining items needed. For example, if you're holding three and need five, <c>[ItemCount]</c> will be replaced with 2.</description></item>
            #   </list>
            ('[Optional] invalidCountMessage', 'string'),
            # The cosmetic effects to show when an item is loaded into the machine.
            ('[Optional] loadEffects', 'List<MachineEffects>'),
            # The cosmetic effects to show while the machine is processing an input, based on the <see cref='F:StardewValley.GameData.Machines.MachineData.WorkingEffectChance' />.
            ('[Optional] workingEffects', 'List<MachineEffects>'),
            # The percentage chance to apply <see cref='F:StardewValley.GameData.Machines.MachineData.WorkingEffects' /> each time the day starts or the in-game clock changes, as a value between 0 (never) and 1 (always).
            ('[Optional] workingEffectChance', 'float', 0.33),
            # Whether the player can drop a new item into the machine before it's done processing the last one (like the crystalarium). The previous item will be lost.
            ('[Optional] allowLoadWhenFull', 'bool'),
            # Whether the machine sprite should bulge in &amp; out while it's processing an item.
            ('[Optional] wobbleWhileWorking', 'bool', True),
            # A light emitted while the machine is processing an item.
            ('[Optional] lightWhileWorking', 'MachineLight'),
            # Whether to show the next sprite in the machine's spritesheet while it's processing an item.
            ('[Optional] showNextIndexWhileWorking', 'bool'),
            # Whether to show the next sprite in the machine's spritesheet while it has an output ready to collect.
            ('[Optional] showNextIndexWhenReady', 'bool'),
            # Whether the player can add fairy dust to speed up the machine.
            ('[Optional] allowFairyDust', 'bool', True),
            # Whether this machine acts as an incubator when placed in a building, so players can incubate eggs in it.
            # This is used by the incubator and ostrich incubator. The game logic assumes there's only one such machine in each building, so this generally shouldn't be used by custom machines that can be built in a vanilla barn or coop.
            ('[Optional] isIncubator', 'bool'),
            # Whether the machine should only produce output overnight. If it finishes processing during the day, it'll pause until its next day update.
            ('[Optional] onlyCompleteOvernight', 'bool'),
            # A game state query which indicates whether the machine should be emptied overnight, so any current output will be lost. Defaults to always false.
            ('[Optional] clearContentsOvernightCondition', 'string'),
            # The game stat counters to increment when an item is placed in the machine.
            ('[Optional] statsToIncrementWhenLoaded', 'List<#StatIncrement>'),
            # The game stat counters to increment when the processed output is collected.
            ('[Optional] statsToIncrementWhenHarvested', 'List<#StatIncrement>'),
            # A list of (skillName) (amount), e.g. Farming 7 Fishing 5 
            ('[Optional] experienceGainOnHarvest', 'string'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Machines.MachineData' />, a cosmetic effect shown when an item is loaded into the machine or while it's processing an input.
    class MachineEffects:
        _fields_ = [
            # A unique string ID for this effect in this list.
            ('id', 'string'),
            # A game state query which indicates whether to add this temporary sprite.
            ('[Optional] condition', 'string'),
            # The audio to play.
            ('[Optional] sounds', 'List<MachineSoundData>'),
            # The number of milliseconds for which each frame in <see cref='F:StardewValley.GameData.Machines.MachineEffects.Frames' /> is kept on-screen.
            ('[Optional] interval', 'int', 100),
            # The animation to apply to the machine sprite, specified as a list of offsets relative to the base sprite index. Default none.
            ('[Optional] frames', 'List<int>'),
            # A duration in milliseconds during which the machine sprite should shake. Default none.
            ('[Optional] shakeDuration', 'int', -1),
            # The temporary animated sprites to show.
            ('[Optional] temporarySprites', 'List<#TemporaryAnimatedSpriteDefinition>'),
        ]
    # As part of a <see cref='T:StardewValley.GameData.Machines.MachineData' />, an extra item required before the machine starts.
    class MachineItemAdditionalConsumedItems:
        _fields_ = [
            # The qualified or unqualified item ID for the required item.
            ('itemId', 'string'),
            # The required stack size for the item matching <see cref='F:StardewValley.GameData.Machines.MachineItemAdditionalConsumedItems.ItemId' />.
            ('[Optional] requiredCount', 'int', 1),
            # If set, overrides the machine's main <see cref='F:StardewValley.GameData.Machines.MachineData.InvalidCountMessage' />.
            ('invalidCountMessage', 'string'),
        ]
    # As part of a <see cref='T:StardewValley.GameData.Machines.MachineData' />, an item produced by this machine.
    # Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.
    class MachineItemOutput(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # Machine-specific data provided to the machine logic, if applicable.
            # For vanilla machines, this is used by casks to set the <c>AgingMultiplier</c> for each item.
            ('[Optional] customData', 'Dictionary<string, string>'),
            # A C# method which produces the item to output.
            # <para><strong>This is an advanced field. Most machines shouldn't use this.</strong> This must be specified in the form <c>{full type name}: {method name}</c> (like <c>StardewValley.Object, Stardew Valley: OutputSolarPanel</c>). The method must be static, take five arguments (<c>Object machine, GameLocation location, Farmer player, Item? inputItem, bool probe</c>), and return the <c>Item</c> instance to output. If this method returns null, the machine won't output anything.</para>
            # <para>If set, the other fields which change the output item (like <see cref='P:StardewValley.GameData.ISpawnItemData.ItemId' /> or <see cref='P:StardewValley.GameData.Machines.MachineItemOutput.CopyColor' />) are ignored.</para>
            ('[Optional] outputMethod', 'string'),
            # Whether to inherit the color of the input item if it was a <c>ColoredObject</c>. This mainly affects roe.
            ('[Optional] copyColor', 'bool'),
            # Whether to inherit the price of the input item, before modifiers like <see cref='P:StardewValley.GameData.Machines.MachineItemOutput.PriceModifiers' /> are applied. This is ignored if the input or output aren't both object (<c>(O)</c>)-type.
            ('[Optional] copyPrice', 'bool'),
            # Whether to inherit the quality of the input item, before modifiers like <see cref='P:StardewValley.GameData.GenericSpawnItemData.QualityModifiers' /> are applied.
            ('[Optional] copyQuality', 'bool'),
            # The produced item's preserved item type, if applicable. This sets the equivalent flag on the output item. The valid values are <c>Jelly</c>, <c>Juice</c>, <c>Pickle</c>, <c>Roe</c> or <c>AgedRoe</c>, and <c>Wine</c>. Defaults to none.
            ('[Optional] preserveType', 'string'),
            # The produced item's preserved unqualified item ID, if applicable. For example, blueberry wine has its preserved item ID set to the blueberry ID. This can be set to <c>DROP_IN</c> to use the input item's ID. Default none.
            ('[Optional] preserveId', 'string'),
            # An amount by which to increment the machine's spritesheet index while it's processing this output. This stacks with <see cref='F:StardewValley.GameData.Machines.MachineData.ShowNextIndexWhileWorking' /> or <see cref='F:StardewValley.GameData.Machines.MachineData.ShowNextIndexWhenReady' />.
            ('[Optional] incrementMachineParentSheetIndex', 'int'),
            # Changes to apply to the item price. This is ignored if the output isn't object (<c>(O)</c>)-type.
            ('[Optional] priceModifiers', 'List<#QuantityModifier>'),
            # How multiple <see cref='P:StardewValley.GameData.Machines.MachineItemOutput.PriceModifiers' /> should be combined.
            ('[Optional] priceModifierMode', '#QuantityModifier.QuantityModifierMode'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Machines.MachineData' />, a light effect shown around the machine.
    class MachineLight:
        _fields_ = [
            # The radius of the light emitted.
            ('[Optional] radius', 'float', 1.),
            # A tint color to apply to the light. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.
            ('[Optional] color', 'string'),
        ]
    # As part of a <see cref='T:StardewValley.GameData.Machines.MachineData' />, a rule which define how to process input items and produce output.
    class MachineOutputRule:
        _fields_ = [
            # A unique identifier for this item within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_Parsnips</c>.
            ('id', 'string'),
            # The rules for when this output rule can be applied.
            ('triggers', 'List<MachineOutputTriggerRule>'),
            # If multiple <see cref='F:StardewValley.GameData.Machines.MachineOutputRule.OutputItem' /> entries match, whether to use the first match instead of choosing one randomly.
            ('[Optional] useFirstValidOutput', 'bool'),
            # The items produced by this output rule. If multiple entries match, one will be selected randomly unless you specify <see cref='F:StardewValley.GameData.Machines.MachineOutputRule.UseFirstValidOutput' />.
            ('[Optional] outputItem', 'List<MachineItemOutput>'),
            # The number of in-game minutes until the output is ready to collect.
            # If both days and minutes are specified, days are used. If neither are specified, the item will be ready instantly.
            ('[Optional] minutesUntilReady', 'int', -1),
            # The number of in-game days until the output is ready to collect.
            # <inheritdoc cref='F:StardewValley.GameData.Machines.MachineOutputRule.MinutesUntilReady' select='/Remarks' />
            ('[Optional] daysUntilReady', 'int', -1),
            # If set, overrides the machine's main <see cref='F:StardewValley.GameData.Machines.MachineData.InvalidCountMessage' />.
            ('[Optional] invalidCountMessage', 'string'),
            # Whether to regenerate the output right before the player collects it, and return the new item instead of what was originally created by the rule.
            # This is specialized to support bee houses. If the new item is null, the original item is returned instead.
            ('[Optional] recalculateOnCollect', 'bool'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Machines.MachineData' />, indicates when a machine should start producing output.
    class MachineOutputTrigger(Flag):
        # The machine is never triggered automatically.
        None_ = 0
        # Apply this rule when an item is placed into the machine.
        ItemPlacedInMachine = 1
        # Apply this rule when the machine's previous output is collected. An output-collected rule won't require or consume the input items, and the input item will be the previous output.
        OutputCollected = 2
        # Apply this rule when the machine is put down. For example, the worm bin uses this to start as soon as it's put down.
        MachinePutDown = 4
        # Apply this rule when a new day starts, if it isn't already processing output. For example, the soda machine does this.
        DayUpdate = 8
class Machines:
    # As part of a <see cref='T:StardewValley.GameData.Machines.MachineOutputRule' />, indicates when the output rule can be applied.
    class MachineOutputTriggerRule:
        # The backing field for <see cref='P:StardewValley.GameData.Machines.MachineOutputTriggerRule.Id' />.
        _idImpl: str = None
        # A unique identifier for this item within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_Parsnips</c>.
        @property
        def id(self) -> str: return self._idImpl or self.trigger.toString()
        @id.setter
        def id(self, value: str) -> None: self._idImpl = value
        _fields_ = [
            # A unique identifier for this item within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_Parsnips</c>.
            ('[Optional] #id', 'string'),
            # When this output rule should apply.
            ('[Optional] trigger', 'MachineOutputTrigger', Machines.MachineOutputTrigger.ItemPlacedInMachine),
            # The qualified or unqualified item ID for the item to match, if the trigger is <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine' /> or <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected' />.
            # You can specify any combination of <see cref='P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId' />, <see cref='P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredTags' />, and <see cref='P:StardewValley.GameData.Machines.MachineOutputTriggerRule.Condition' />. The input item must match all specified fields; if none are specified, this conversion will always match.
            ('[Optional] requiredItemId', 'string'),
            # The context tags to match against input items, if the trigger is <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine' /> or <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected' />. An item must match all of the listed tags to select this rule. You can negate a tag with ! (like <c>!fossil_item</c> to exclude fossils).
            # [inheritdoc cref='P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId' select='Remarks' />
            ('[Optional] requiredTags', 'List<string>'),
            # The required stack size for the input item, if the trigger is <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine' /> or <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected' />.
            ('[Optional] requiredCount', 'int', 1),
            # A game state query which indicates whether a given input should be matched (if the other requirements are matched too). Item-only tokens are valid for this check if the trigger is <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine' /> or <see cref='F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected' />. Defaults to always true.
            # [inheritdoc cref='P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId' select='Remarks' />
            ('[Optional] condition', 'string'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Machines.MachineData' />, an audio cue to play.
    class MachineSoundData:
        _fields_ = [
            # The audio cue ID to play.
            ('id', 'string'),
            # The number of milliseconds until the sound should play.
            ('[Optional] delay', 'int'),
        ]
    # As part of a <see cref='T:StardewValley.GameData.Machines.MachineTimeBlockers' />, indicates when the machine should be paused.
    class MachineTimeBlockers(Enum):
        # Pause when placed in an outside location.
        Outside = 0
        # Pause when placed in an inside location.
        Inside = 1
        # Pause in spring.
        Spring = 2
        # Pause in summer.
        Summer = 3
        # Pause in fall.
        Fall = 4
        # Pause in winter.
        Winter = 5
        # Pause on sunny days.
        Sun = 6
        # Pause on rainy days.
        Rain = 7
        # Always pause the machine. This is used in specialized cases where the timer is handled by advanced machine logic.
        Always = 8

class MakeoverOutfits:
    # A hat, shirt, or pants that should be equipped on the player as part of a <see cref='T:StardewValley.GameData.MakeoverOutfits.MakeoverOutfit' />.
    class MakeoverItem:
        _fields_ = [
            # A unique ID for this entry within the list.
            ('id', 'string'),
            # The qualified item ID for the hat, shirt, or pants to equip.
            ('itemId', 'string'),
            # A tint color to apply to the item. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.
            ('[Optional] color', 'string'),
            # The player gender for which the outfit part applies, or <c>null</c> for any gender.
            ('[Optional] gender', 'Gender?'),
        ]
        # Get whether this item applies to the given player gender.
        # <param name='gender'>The player gender to check.</param>
        def matchesGender(self, gender: Gender) -> bool:
            if not self.gender: return True
            return self.gender == gender and self.gender
    # An outfit that can be selected at the Desert Festival makeover booth.
    class MakeoverOutfit:
        _fields_ = [
            # A unique string ID for this entry within the outfit list.
            ('id', 'string'),
            # The hat, shirt, and pants that makes up the outfit. Each item is added to the appropriate equipment slot based on its type.
            # An item can be omitted to leave the player's current item unchanged (e.g. shirt + pants without a hat). If there are multiple items of the same type, the first matching one is applied.
            ('outfitParts', 'List<MakeoverItem>'),
            # The player gender for which the outfit applies, or <c>null</c> for any gender.
            ('[Optional] gender', 'Gender?'),
        ]

class Minecarts:
    # As part of <see cref='T:StardewValley.GameData.Minecarts.MinecartNetworkData' />, a minecart destination which can be used by players.
    class MinecartDestinationData:
        _fields_ = [
            # A unique string ID for this destination within the network.
            ('id', 'string'),
            # A tokenizable string for the destination name shown in the minecart menu. You can use the location's display name with the <c>LocationName</c> token (like <c>[LocationName Desert]</c> for the desert).
            ('displayName', 'string'),
            # A game state query which indicates whether this minecart destination is available. Defaults to always available.
            ('[Optional] condition', 'string'),
            # The gold price that must be paid to go to this destination, if any.
            ('[Optional] price', 'int'),
            # A localizable string for the message to show when purchasing a ticket, if applicable. Defaults to <see cref='F:StardewValley.GameData.Minecarts.MinecartNetworkData.BuyTicketMessage' />.
            ('[Optional] buyTicketMessage', 'string'),
            # The unique name for the location to warp to.
            ('targetLocation', 'string'),
            # The destination tile position within the location.
            ('targetTile', 'Point'),
            # The direction the player should face after arrival (one of <c>down</c>, <c>left</c>, <c>right</c>, or <c>up</c>).
            ('[Optional] targetDirection', 'string'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # The data for a network of minecarts, which are enabled together.
    class MinecartNetworkData:
        _fields_ = [
            # A game state query which indicates whether this minecart network is unlocked.
            ('[Optional] unlockCondition', 'string'),
            # A localizable string for the message to show if the network is locked.
            ('[Optional] lockedMessage', 'string'),
            # A localizable string for the message to show when selecting a destination.
            ('[Optional] chooseDestinationMessage', 'string'),
            # A localizable string for the message to show when purchasing a ticket, if applicable.
            ('[Optional] buyTicketMessage', 'string'),
            # The destinations which the player can travel to from any minecart in this network.
            ('destinations', 'List<MinecartDestinationData>'),
        ]

class Movies:
    # As part of <see cref='T:StardewValley.GameData.Movies.SpecialResponses' />, a possible dialogue to show.
    class CharacterResponse:
        _fields_ = [
            # <para>For <see cref='F:StardewValley.GameData.Movies.SpecialResponses.DuringMovie' />, the <see cref='F:StardewValley.GameData.Movies.MovieScene.ResponsePoint' /> used to decide whether it should be shown during a scene.</para>
            # <para>For <see cref='F:StardewValley.GameData.Movies.SpecialResponses.BeforeMovie' /> or <see cref='F:StardewValley.GameData.Movies.SpecialResponses.AfterMovie' />, this field is ignored.</para>
            ('[Optional] responsePoint', 'string'),
            # <para>For <see cref='F:StardewValley.GameData.Movies.SpecialResponses.DuringMovie' />, an optional event script to run before the <see cref='F:StardewValley.GameData.Movies.CharacterResponse.Text' /> is shown.</para>
            # <para>For <see cref='F:StardewValley.GameData.Movies.SpecialResponses.BeforeMovie' /> or <see cref='F:StardewValley.GameData.Movies.SpecialResponses.AfterMovie' />, this field is ignored.</para>
            ('[Optional] script', 'string'),
            # The translated dialogue text to show.
            ('[Optional] text', 'string'),
        ]
    # The metadata for a concession which can be purchased at the movie theater.
    class ConcessionItemData:
        _fields_ = [
            # A key which uniquely identifies this concession. This should only contain alphanumeric/underscore/dot characters. For custom concessions, this should be prefixed with your mod ID like <c>Example.ModId_ConcessionName</c>.
            ('id', 'string'),
            # The internal name for the concession item.
            ('name', 'string'),
            # The tokenizable string for the item's translated display name.
            ('displayName', 'string'),
            # The tokenizable string for the item's translated description.
            ('description', 'string'),
            # The gold price to purchase the concession.
            ('price', 'int'),
            # The asset name for the texture containing the concession's sprite.
            ('texture', 'string'),
            # The index within the <see cref='F:StardewValley.GameData.Movies.ConcessionItemData.Texture' /> for the concession sprite, where 0 is the top-left icon.
            ('spriteIndex', 'int'),
            # A list of tags which describe the concession, which can be matched by <see cref='T:StardewValley.GameData.Movies.ConcessionTaste' /> fields.
            ('[Optional] itemTags', 'List<string>'),
        ]
    # The metadata for concession tastes for one or more NPCs.
    class ConcessionTaste:
        # A unique ID for this entry.
        @property
        def id(self) -> str: return self.name
        _fields_ = [
            # A unique ID for this entry.
            ('[Ignore] #id', 'string'),
            # The internal NPC name for which to set tastes, or <c>'*'</c> to apply to all NPCs.
            ('name', 'string'),
            # The concessions loved by the matched NPCs.
            # This can be one of...
            # <list type='bullet'>
            #   <item><description>the <see cref='F:StardewValley.GameData.Movies.ConcessionItemData.Name' /> for a specific concession;</description></item>
            #   <item><description>or a tag to match in <see cref='F:StardewValley.GameData.Movies.ConcessionItemData.ItemTags' />.</description></item>
            # </list>
            ('[Optional] lovedTags', 'List<string>'),
            # The concessions liked by matched NPCs.
            # See remarks on <see cref='P:StardewValley.GameData.Movies.ConcessionTaste.LovedTags' />.
            ('[Optional] likedTags', 'List<string>'),
            # The concessions liked by matched NPCs.
            # See remarks on <see cref='P:StardewValley.GameData.Movies.ConcessionTaste.DislikedTags' />.
            ('[Optional] dislikedTags', 'List<string>'),
        ]
    # Metadata for how an NPC can react to movies.
    class MovieCharacterReaction:
        @property
        def id(self) -> str: return self.npcName
        _fields_ = [
            # A unique ID for this entry.
            ('[Ignore] #id', 'string'),
            # The internal name of the NPC for which to define reactions.
            ('npcName', 'string'),
            # The possible movie reactions for this NPC.
            ('[Optional] reactions', 'List<MovieReaction>'),
        ]
    class MovieCranePrizeData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # The rarity list to update. This can be 1 (common), 2 (rare), or 3 (deluxe).
            ('[Optional] rarity', 'int', 1),
        ]
    # The metadata for a movie that can play at the movie theater.
    class MovieData:
        _fields_ = [
            # A key which uniquely identifies this movie. This should only contain alphanumeric/underscore/dot characters. For custom movies, this should be prefixed with your mod ID like <c>Example.ModId_MovieName</c>.
            ('[Optional] id', 'string'),
            # The seasons when the movie plays, or none to allow any season.
            ('[Optional] seasons', 'List<#Season>'),
            # If set, the movie is available when <c>{year} % <see cref='F:StardewValley.GameData.Movies.MovieData.YearModulus' /> == <see cref='F:StardewValley.GameData.Movies.MovieData.YearRemainder' /></c> (where <c>{year}</c> is the number of years since the movie theater was built and {remainder} defaults to zero). For example, a modulus of 2 with remainder 1 is shown in the second year and every other year thereafter.
            ('[Optional] yearModulus', 'int?'),
            # [inheritdoc cref='F:StardewValley.GameData.Movies.MovieData.YearModulus' />
            ('[Optional] yearRemainder', 'int?'),
            # The asset name for the movie poster and screen images, or <c>null</c> to use <c>LooseSprites\Movies</c>.
            # This must be a spritesheet with one 490�128 pixel row per movie. A 13�19 area in the top-left corner of the row should contain the movie poster. With a 16-pixel offset from the left edge, there should be two rows of five 90�61 pixel movie screen images, with a six-pixel gap between each image. (The movie doesn't need to use all of the image slots.)
            ('[Optional] texture', 'string'),
            # The sprite index within the <see cref='F:StardewValley.GameData.Movies.MovieData.Texture' /> for this movie poster and screen images.
            ('sheetIndex', 'int'),
            # A tokenizable string for the translated movie title.
            ('title', 'string'),
            # A tokenizable string for the translated movie description, shown when interacting with the movie poster.
            ('description', 'string'),
            # A list of tags which describe the genre or other metadata, which can be matched by <see cref='F:StardewValley.GameData.Movies.MovieReaction.Tag' />.
            ('[Optional] tags', 'List<string>'),
            # The prizes that can be grabbed in the crane game while this movie is playing (in addition to the default items).
            ('[Optional] cranePrizes', 'List<MovieCranePrizeData>', []),
            # The prize rarity lists whose default items to clear when this movie is playing, so they're only taken from <see cref='F:StardewValley.GameData.Movies.MovieData.CranePrizes' />.
            ('[Optional] clearDefaultCranePrizeGroups', 'List<int>', []),
            # The scenes to show when watching the movie.
            ('scenes', 'List<MovieScene>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Movies.MovieCharacterReaction' />, a possible reactions to movies matching a tag.
    class MovieReaction:
        _fields_ = [
            # <para>A pattern which determines which movies this reaction can apply to.</para>
            # <para>This can be any of the following:</para>
            # <list type='bullet'>
            #   <item><description><c>'*'</c> to match any movie.</description></item>
            #   <item><description>A tag to match any movie which has that tag in its <see cref='F:StardewValley.GameData.Movies.MovieData.Tags' /> list.</description></item>
            #   <item><description>An ID to match any movie with that <see cref='F:StardewValley.GameData.Movies.MovieData.Id' /> value.</description></item>
            #   <item><description>How much the NPC enjoys this movie, based on the <see cref='F:StardewValley.GameData.Movies.MovieReaction.Response' /> for matched entries. This performs a two-pass check: any <see cref='T:StardewValley.GameData.Movies.MovieReaction' /> entry which matches with a non-response <see cref='F:StardewValley.GameData.Movies.MovieReaction.Tag' /> is used to determine the NPC's response, defaulting to <c>like</c>. The result is then checked against this value.</description></item>
            # </list>
            ('tag', 'string'),
            # How much the NPC enjoys the movie (one of <c>love</c>, <c>like</c>, or <c>dislike</c>).
            ('[Optional] response', 'string', 'like'),
            # A list of internal NPC names. If this isn't empty, at least one of these NPCs must be present in the theater for this reaction to apply.
            ('[Optional] whitelist', 'List<string>', []),
            # If set, possible dialogue from the NPC during the movie.
            ('[Optional] specialResponses', 'SpecialResponses'),
            # A key which uniquely identifies this movie reaction. This should only contain alphanumeric/underscore/dot characters. For custom movie reactions, this should be prefixed with your mod ID like <c>Example.ModId_ReactionName</c>.
            ('id', 'string', ''),
        ]
        # Whether this movie reaction should apply to a given movie.
        # <param name='movieData'>The movie data to match.</param>
        # <param name='moviePatrons'>The internal names for NPCs watching the movie.</param>
        # <param name='otherValidTags'>The other tags to match via <see cref='F:StardewValley.GameData.Movies.MovieReaction.Tag' />.</param>
        def shouldApplyToMovie(self, movieData: MovieData, moviePatrons: list[str], otherValidTags: list[str]) -> bool:
            if self.ehitelist:
                if not moviePatrons: return False
                for str in self.whitelist:
                    if str not in moviePatrons: return False
            return self.tag == movieData.id or tag in movieData.tags or self.tag == '*' or tag in otherValidTags
    # As part of <see cref='T:StardewValley.GameData.Movies.MovieData' />, a scene to show when watching the movie.
    class MovieScene:
        _fields_ = [
            # The screen index within the movie's spritesheet row.
            # See remarks on <see cref='F:StardewValley.GameData.Movies.MovieData.SheetIndex' /> for the expected sprite layout.
            ('[Optional] image', 'int', -1),
            # If set, the audio cue ID for the music to play while the scene is shown. Default none.
            ('[Optional] music', 'string'),
            # If set, the audio cue ID for a sound effect to play when the scene starts. Default none.
            ('[Optional] sound', 'string'),
            # The number of milliseconds to wait after the scene starts before showing the <see cref='F:StardewValley.GameData.Movies.MovieScene.Text' />, <see cref='F:StardewValley.GameData.Movies.MovieScene.Script' />, and <see cref='F:StardewValley.GameData.Movies.MovieScene.Image' />.
            ('[Optional] messageDelay', 'int', 500),
            # If set, a tokenizable string for the custom event script to run for any custom audio, images, etc.
            ('[Optional] script', 'string'),
            # If set, a tokenizable string for the text to show in a message box while the scene plays. The scene will pause until the player closes it.
            ('[Optional] text', 'string'),
            # Whether to shake the movie screen image for the duration of the scene.
            ('[Optional] shake', 'bool'),
            # If set, an optional hook where NPCs may interject a reaction dialogue via <see cref='F:StardewValley.GameData.Movies.CharacterResponse.ResponsePoint' />.
            ('[Optional] responsePoint', 'string'),
            # A key which uniquely identifies this movie scene. This should only contain alphanumeric/underscore/dot characters. For custom movie scenes, this should be prefixed with your mod ID like <c>Example.ModId_MovieScene</c>.
            ('id', 'string'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Movies.MovieReaction' />, possible dialogue from the NPC during the movie.
    class SpecialResponses:
        _fields_ = [
            # The dialogue to show when the player interacts with the NPC in the theater lobby before the movie starts, if any.
            ('[Optional] beforeMovie', 'CharacterResponse'),
            # The dialogue to show during the movie based on the <see cref='F:StardewValley.GameData.Movies.CharacterResponse.ResponsePoint' />, if any.
            ('[Optional] duringMovie', 'CharacterResponse'),
            # The dialogue to show when the player interacts with the NPC in the theater lobby after the movie ends, if any.
            ('[Optional] afterMovie', 'CharacterResponse'),
        ]

class Museum:
    # As part of <see cref='T:StardewValley.GameData.Museum.MuseumRewards' />, an item that must be donated to complete this reward group.
    class MuseumDonationRequirement:
        _fields_ = [
            # The context tag for the items to require.
            ('tag', 'string'),
            # The minimum number of items matching the <see cref='F:StardewValley.GameData.Museum.MuseumDonationRequirement.Tag' /> that must be donated.
            ('count', 'int'),
        ]
    # The data for a set of artifacts that can be donated to the museum, and the resulting reward.
    class MuseumRewards:
        _fields_ = [
            # <para>The items that must be donated to complete this reward group. The player must fulfill every entry in the list to unlock the reward. For example, an entry with the tag <c>forage_item</c> and count 2 will require donating any two forage items.</para>
            # <para>Special case: an entry with the exact values <c>Tag: '', Count: -1</c> passes if the museum is complete (i.e. the player has donated the max number of items). </para>
            ('targetContextTags', 'List<MuseumDonationRequirement>'),
            # The qualified item ID for the item given to the player when they donate all required items for this group. There's no reward item if omitted.
            ('[Optional] rewardItemId', 'string'),
            # The stack size for the <see cref='F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId' /> item (if the item supports stacking).
            ('[Optional] rewardItemCount', 'int', 1),
            # Whether to mark the <see cref='F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId' /> item as a special permanent item, which can't be destroyed/dropped and can only be collected once.
            ('[Optional] rewardItemIsSpecial', 'bool'),
            # Whether to give the player a cooking/crafting recipe which produces the <see cref='F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId' /> item, instead of the item itself. Ignored if the item type can't be cooked/crafted (i.e. non-object-type items).
            ('[Optional] rewardItemIsRecipe', 'bool'),
            # The actions to perform when the reward is collected. For example, this is used for the rusty key unlock at 60 donations.
            ('[Optional] rewardActions', 'List<string>'),
            # Whether to add the ID value to the player's received mail. This is used to track whether the player has collected the reward, and should almost always be true. If this and <see cref='F:StardewValley.GameData.Museum.MuseumRewards.RewardItemIsSpecial' /> are both false, the player will be able to collect the reward infinite times.
            ('[Optional] flagOnCompletion', 'bool'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]

class Objects:
    # As part of <see cref='T:StardewValley.GameData.Objects.ObjectData' />, a buff to set when this item is eaten.
    class ObjectBuffData:
        # The backing field for <see cref='P:StardewValley.GameData.Objects.ObjectBuffData.Id' />.
        _idImpl: str = None
        @property
        def id(self) -> str: return self._idImpl or self.buffId
        @id.setter
        def id(self, value: str) -> None: self._idImpl = value
        _fields_ = [
            # The backing field for <see cref='P:StardewValley.GameData.Objects.ObjectBuffData.Id' />.
            ('[Optional] #id', 'string'),
            # The buff ID to apply, or <c>null</c> to use <c>food</c> or <c>drink</c> depending on the item data.
            ('[Optional] buffId', 'string'),
            # The texture to load for the buff icon, or <c>null</c> for the default icon based on the <see cref='P:StardewValley.GameData.Objects.ObjectBuffData.BuffId' /> and <see cref='P:StardewValley.GameData.Objects.ObjectBuffData.CustomAttributes' />.
            ('[Optional] iconTexture', 'string'),
            # The sprite index for the buff icon within the <see cref='P:StardewValley.GameData.Objects.ObjectBuffData.IconTexture' />.
            ('[Optional] iconSpriteIndex', 'int'),
            # The buff duration measured in in-game minutes, or <c>-2</c> for a buff that should last all day, or (if <see cref='P:StardewValley.GameData.Objects.ObjectBuffData.BuffId' /> is set) omit it to use the duration in <c>Data/Buffs</c>.
            ('[Optional] duration', 'int'),
            # Whether this buff counts as a debuff, so its duration should be halved when wearing a Sturdy Ring.
            ('[Optional] isDebuff', 'bool'),
            # The glow color to apply to the player, if any.
            ('[Optional] glowColor', 'string'),
            # The custom buff attributes to apply, if any.
            ('[Optional] customAttributes', 'Buffs.BuffAttributesData'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # The data for an object-type item.
    class ObjectData:
        _fields_ = [
            # The internal item name.
            ('name', 'string'),
            # A tokenizable string for the item's translated display name.
            ('displayName', 'string'),
            # A tokenizable string for the item's translated description.
            ('description', 'string'),
            # The item's general type, like <c>Arch</c> (artifact) or <c>Minerals</c>.
            ('type', 'string'),
            # The item category, usually matching a constant like <c>Object.flowersCategory</c>.
            ('category', 'int'),
            # The price when sold by the player. This is not the price when bought from a shop.
            ('[Optional] price', 'int'),
            # The asset name for the texture containing the item's sprite, or <c>null</c> for <c>Maps/springobjects</c>.
            ('[Optional] texture', 'string'),
            # The sprite's index in the spritesheet.
            ('spriteIndex', 'int'),
            # When drawn as a colored object, whether to apply the color to the next sprite in the spritesheet and draw that over the main sprite. If false, the color is applied to the main sprite instead.
            ('[Optional] colorOverlayFromNextIndex', 'bool'),
            # A numeric value that determines how much energy (edibility � 2.5) and health (edibility � 1.125) is restored when this item is eaten. An item with an edibility of -300 can't be eaten, values from -299 to -1 reduce health and energy, and zero can be eaten but doesn't change health/energy.
            # This is ignored for rings.
            ('[Optional] edibility', 'int', -300),
            # Whether to drink the item instead of eating it.
            # Ignored if the item isn't edible per <see cref='F:StardewValley.GameData.Objects.ObjectData.Edibility' />.
            ('[Optional] isDrink', 'bool'),
            # The buffs to apply to the player when this item is eaten, if any.
            # Ignored if the item isn't edible per <see cref='F:StardewValley.GameData.Objects.ObjectData.Edibility' />.
            ('[Optional] buffs', 'List<ObjectBuffData>'),
            # If set, the item will drop a default item when broken as a geode. If <see cref='F:StardewValley.GameData.Objects.ObjectData.GeodeDrops' /> is set too, there's a 50% chance of choosing a value from that list instead.
            ('[Optional] geodeDropsDefaultItems', 'bool'),
            # The items that can be dropped when this item is broken open as a geode.
            ('[Optional] geodeDrops', 'List<ObjectGeodeDropData>'),
            # If this is an artifact (i.e. <see cref='F:StardewValley.GameData.Objects.ObjectData.Type' /> is <c>Arch</c>), the chance that it can be found by digging artifact spots in each location.
            ('[Optional] artifactSpotChances', 'Dictionary<string, float>'),
            # Whether this item can be given to NPCs as a gift by default.
            # This doesn't override non-gift behavior (e.g. receiving quest items) or specific exclusions (e.g. only Pierre will accept Pierre's Missing Stocklist).
            ('[Optional] canBeGivenAsGift', 'bool', True),
            # Whether this item can be trashed by players by default.
            # This doesn't override specific exclusions (e.g. quest items can't be trashed).
            ('[Optional] canBeTrashed', 'bool', True),
            # Whether to exclude this item from the fishing collection and perfection score.
            ('[Optional] excludeFromFishingCollection', 'bool'),
            # Whether to exclude this item from the shipping collection and perfection score.
            ('[Optional] excludeFromShippingCollection', 'bool'),
            # Whether to exclude this item from shops when selecting random items to sell.
            ('[Optional] excludeFromRandomSale', 'bool'),
            # The custom context tags to add for this item (in addition to the tags added automatically based on the other object data).
            ('[Optional] contextTags', 'List<string>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Objects.ObjectData' />, an item that can be found by breaking the item as a geode.
    # Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.
    class ObjectGeodeDropData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # A probability that this item will be found, as a value between 0 (never) and 1 (always).
            ('[Optional] chance', 'double', 1.0),
            # The mail flag to set for the current player when this item is picked up by the player.
            ('[Optional] setFlagOnPickup', 'string'),
            # The order in which this drop should be checked, where 0 is the default value used by most drops. Drops within each precedence group are checked in the order listed.
            ('[Optional] precedence', 'int'),
        ]

class Pants:
    # The metadata for a pants item that can be equipped by players.
    class PantsData:
        _fields_ = [
            # The pants' internal name.
            ('name', 'string', 'Pants'),
            # A tokenizable string for the pants' display name.
            ('displayName', 'string', '[LocalizedText Strings\\Pants:Pants_Name]'),
            # A tokenizable string for the pants' description.
            ('description', 'string', '[LocalizedText Strings\\Pants:Pants_Description]'),
            # The price when purchased from shops.
            ('[Optional] price', 'int', 50),
            # The asset name for the texture containing the pants' sprite, or <c>null</c> for <c>Characters/Farmer/pants</c>.
            ('[Optional] texture', 'string'),
            # The sprite's index in the spritesheet.
            ('spriteIndex', 'int'),
            # The default pants color.
            ('[Optional] defaultColor', 'string', '255 235 203'),
            # Whether the pants can be dyed.
            ('[Optional] canBeDyed', 'bool'),
            # Whether the pants continuously shift colors. This overrides <see cref='F:StardewValley.GameData.Pants.PantsData.DefaultColor' /> and <see cref='F:StardewValley.GameData.Pants.PantsData.CanBeDyed' /> if set.
            ('[Optional] isPrismatic', 'bool'),
            # Whether the pants can be selected on the customization screen.
            ('[Optional] canChooseDuringCharacterCustomization', 'bool'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]

class Pets:
    # As part of <see cref='T:StardewValley.GameData.Pets.PetBehavior' />, the animation frames to play while the state is active.
    class PetAnimationFrame:
        _fields_ = [
            # The frame index in the animation. This should be an incremental number starting at 0.
            ('frame', 'int'),
            # The millisecond duration for which the frame should be kept on-screen before continuing to the next frame.
            ('duration', 'int'),
            # Whether to play the footstep sound for the tile under the pet when the frame starts.
            ('[Optional] hitGround', 'bool'),
            # Whether the pet should perform a small hop when the frame starts, including a 'dwop' sound.
            ('[Optional] jump', 'bool'),
            # The audio cue ID for the sound to play when the animation starts or loops. If set to the exact string <c>BARK</c>, the <see cref='F:StardewValley.GameData.Pets.PetData.BarkSound' /> or <see cref='F:StardewValley.GameData.Pets.PetBreed.BarkOverride' /> is used. Defaults to none.
            ('[Optional] sound', 'string'),
            # When set, the <see cref='F:StardewValley.GameData.Pets.PetAnimationFrame.Sound' /> is only audible if the pet is within this many tiles past the border of the screen. Default -1 (no distance check).
            ('[Optional] soundRangeFromBorder', 'int', -1),
            # When set, the <see cref='F:StardewValley.GameData.Pets.PetAnimationFrame.Sound' /> is only audible if the pet is within this many tiles of the player. Default -1 (no distance check).
            ('[Optional] soundRange', 'int', -1),
            # Whether to mute the <see cref='F:StardewValley.GameData.Pets.PetAnimationFrame.Sound' /> when the 'mute animal sounds' option is set.
            ('[Optional] int SoundIsVoice', 'int'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Pets.PetBehavior' />, what to do when the last animation frame is reached while the behavior is still active.
    class PetAnimationLoopMode(Enum):
        # Equivalent to <see cref='F:StardewValley.GameData.Pets.PetAnimationLoopMode.Loop' />.
        None_ = 0
        # Restart the animation from the first frame.
        Loop = 1
        # Keep the last frame visible until the animation ends.
        Hold = 2
    # As part of <see cref='T:StardewValley.GameData.Pets.PetData' />, a state in the pet's possible actions and behaviors.
    class PetBehavior:
        _fields_ = [
            # A unique string ID for the state. This only needs to be unique within the pet type (e.g. cats and dogs can have different behaviors with the same name).
            ('id', 'string'),
            # Whether to constrain the pet's facing direction to left and right while the state is active.
            ('[Optional] isSideBehavior', 'bool'),
            # Whether to point the pet in a random direction at the start of this state. If set, this overrides <see cref='F:StardewValley.GameData.Pets.PetBehavior.Direction' />.
            ('[Optional] randomizeDirection', 'bool'),
            # The specific direction to face at the start of this state (one of <c>left</c>, <c>right</c>, <c>up</c>, or <c>down</c>), unless overridden by <see cref='F:StardewValley.GameData.Pets.PetBehavior.RandomizeDirection' />.
            ('[Optional] direction', 'string'),
            # Whether to walk in the pet's facing direction.
            ('[Optional] walkInDirection', 'bool'),
            # Overrides the pet's <see cref='F:StardewValley.GameData.Pets.PetData.MoveSpeed' /> while this state is active, or <c>-1</c> to inherit it.
            ('[Optional] moveSpeed', 'int', -1),
            # The audio cue ID for the sound to play when the state starts. If set to the exact string <c>BARK</c>, the <see cref='F:StardewValley.GameData.Pets.PetData.BarkSound' /> or <see cref='F:StardewValley.GameData.Pets.PetBreed.BarkOverride' /> is used. Defaults to none.
            ('[Optional] soundOnStart', 'string'),
            # When set, the <see cref='F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart' /> is only audible if the pet is within this many tiles past the border of the screen. Default -1 (no distance check).
            ('[Optional] soundRangeFromBorder', 'int', -1),
            # When set, the <see cref='F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart' /> is only audible if the pet is within this many tiles of the player. Default -1 (no distance check).
            ('[Optional] soundRange', 'int', -1),
            # Whether to mute the <see cref='F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart' /> when the 'mute animal sounds' option is set.
            ('[Optional] soundIsVoice', 'bool'),
            # The millisecond duration for which to shake the pet when the state starts.
            ('[Optional] shake', 'int'),
            # The animation frames to play while this state is active.
            ('[Optional] animation', 'List<PetAnimationFrame>'),
            # What to do when the last animation frame is reached while the behavior is still active.
            ('[Optional] loopMode', 'PetAnimationLoopMode'),
            # The minimum number of times to play the animation, or <c>-1</c> to disable repeating the animation.
            # Both <see cref='F:StardewValley.GameData.Pets.PetBehavior.AnimationMinimumLoops' /> and <see cref='F:StardewValley.GameData.Pets.PetBehavior.AnimationMaximumLoops' /> must be set to have any effect. The game will choose an inclusive random value between them.
            ('[Optional] animationMinimumLoops', 'int', -1),
            # The maximum number of times to play the animation, or <c>-1</c> to disable repeating the animation.
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetBehavior.AnimationMinimumLoops' />.
            ('[Optional] animationMaximumLoops', 'int', -1),
            # The possible behavior transitions to start when the current behavior's animation ends. If multiple transitions are listed, one is selected at random.
            ('[Optional] animationEndBehaviorChanges', 'List<PetBehaviorChanges>'),
            # The millisecond duration until the pet transitions to a behavior in the <see cref='F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges' /> field, if set. This overrides <see cref='F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration' /> and <see cref='F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration' />.
            ('[Optional] duration', 'int', -1),
            # The minimum millisecond duration until the pet transitions to a behavior in the <see cref='F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges' /> field, if set. This is ignored if <see cref='F:StardewValley.GameData.Pets.PetBehavior.Duration' /> is set.
            # Both <see cref='F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration' /> and <see cref='F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration' /> must have a non-negative value to take effect.
            ('[Optional] minimumDuration', 'int', -1),
            # The maximum millisecond duration until the pet transitions to a behavior in the <see cref='F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges' /> field, if set. This is ignored if <see cref='F:StardewValley.GameData.Pets.PetBehavior.Duration' /> is set.
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration' />.
            ('[Optional] maximumDurations', 'int', -1),
            # The possible behavior transitions to start when the <see cref='F:StardewValley.GameData.Pets.PetBehavior.Duration' /> or <see cref='F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration' /> + <see cref='F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration' /> values are reached. If multiple transitions are listed, one is selected at random.
            ('[Optional] timeoutBehaviorChanges', 'List<PetBehaviorChanges>'),
            # The possible behavior transitions to start when the player is within two tiles of the pet. If multiple transitions are listed, one is selected at random.
            ('[Optional] playerNearbyBehaviorChanges', 'List<PetBehaviorChanges>'),
            # The probability at the start of each frame that the pet will transition to a behavior in the <see cref='F:StardewValley.GameData.Pets.PetBehavior.RandomBehaviorChanges' /> field, if set. Specified as a value between 0 (never) and 1 (always).
            ('[Optional] randomBehaviorChangeChance', 'float'),
            # The possible behavior transitions to start, based on a <see cref='F:StardewValley.GameData.Pets.PetBehavior.RandomBehaviorChangeChance' /> check at the start of each frame. If multiple transitions are listed, one is selected at random.
            ('[Optional] randomBehaviorChanges', 'List<PetBehaviorChanges>'),
            # The possible behavior transitions to start when the pet lands after jumping. If multiple transitions are listed, one is selected at random.
            ('[Optional] jumpLandBehaviorChanges', 'List<PetBehaviorChanges>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Pets.PetBehavior' />, a possible behavior transition that can be started.
    class PetBehaviorChanges:
        _fields_ = [
            # The option's weight when randomly choosing a behavior, relative to other behaviors in the list (e.g. 2 is twice as likely as 1).
            ('[Optional] weight', 'float', 1.),
            # Whether the transition can only happen if the pet is outside.
            ('[Optional] outsideOnly', 'bool'),
            # The name of the behavior to start if the pet is facing up.
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior' />.
            ('[Optional] upBehavior', 'string'),
            # The name of the behavior to start if the pet is facing down.
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior' />.
            ('[Optional] downBehavior', 'string'),
            # The name of the behavior to start if the pet is facing left.
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior' />.
            ('[Optional] leftBehavior', 'string'),
            # The name of the behavior to start if the pet is facing right.
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior' />.
            ('[Optional] rightBehavior', 'string'),
            # The name of the behavior to start, if no directional behavior applies.
            # The pet will check for a behavior matching its facing direction first (like <see cref='F:StardewValley.GameData.Pets.PetBehaviorChanges.UpBehavior' />), then try the <see cref='F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior' />. If none are specified, the current behavior will continue unchanged.
            ('[Optional] behavior', 'string'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Pets.PetData' />, a cosmetic breed which can be selected in the character customization menu when creating a save.
    class PetBreed:
        _fields_ = [
            # A key which uniquely identifies the pet breed. The ID should only contain alphanumeric/underscore/dot characters. For custom breeds, this should be prefixed with your mod ID like <c>Example.ModId_BreedName.</c>
            ('id', 'string'),
            # The asset name for the breed spritesheet for the pet's in-game sprite. This should be 128 pixels wide, and 256 (cat) or 288 (dog) pixels high.
            ('texture', 'string'),
            # The asset name for the breed icon texture, shown on the character customization screen and in-game menu. This should be a 16x16 pixel icon.
            ('iconTexture', 'string'),
            # The icon's pixel area within the <see cref='F:StardewValley.GameData.Pets.PetBreed.IconTexture' />.
            ('iconSourceRect', 'Rectangle', Rectangle.empty),
            # Whether this pet can be chosen as a starter pet at character creation
            ('[Optional] canBeChosenAtStart', 'bool', True),
            # Whether this pet can be adopted from Marnie once she starts offering pets.
            ('[Optional] canBeAdoptedFromMarnie', 'bool', True),
            # The price this pet costs in Marnie's shop
            ('[Optional] adoptionPrice', 'int', 40000),
            # Overrides the pet's <see cref='F:StardewValley.GameData.Pets.PetData.BarkSound' /> field for this breed, if set.
            ('[Optional] barkOverride', 'string'),
            # The pitch applied to the pet's bark sound, measured as a decimal value relative to 1.
            ('[Optional] voicePitch', 'float', 1.),
        ]
    # The metadata for a pet type that can be selected by the player.
    class PetData:
        _fields_ = [
            # A tokenizable string for the pet type's display name (like 'cat'), which can be used in dialogue.
            ('displayName', 'string'),
            # The cue ID for the pet's occasional 'bark' sound.
            ('barkSound', 'string'),
            # The cue ID for the sound which the pet makes when you pet it.
            ('contentSound', 'string'),
            # The number of milliseconds until the ContentSound is repeated once. This is used by the dog, who pants twice when pet. Defaults to disabled.
            ('[Optional] repeatContentSoundAfter', 'int', -1),
            # A pixel offset to apply to the emote position over the pet sprite.
            ('[Optional] emoteOffset', 'Point'),
            # The pixel offset for the pet when shown in events like Marnie's adoption event.
            ('[Optional] eventOffset', 'Point'),
            # The location containing the event which lets the player adopt this pet, if they've selected it as their preferred type.
            ('[Optional] adoptionEventLocation', 'string', 'Farm'),
            # The event ID in the <see cref='F:StardewValley.GameData.Pets.PetData.AdoptionEventLocation' /> which lets the player adopt this pet, if they've selected it as their preferred type.
            # If set, this forces the event to play after 20 days if the event's preconditions haven't been met yet.
            ('[Optional] adoptionEventId', 'string'),
            # How to render the pet during the summit perfection slide-show.
            # If this isn't set, the pet won't be shown in the slide-show.
            ('summitPerfectionEvent', 'PetSummitPerfectionEventData'),
            # How quickly the pet can move.
            ('[Optional] moveSpeed', 'int', 2),
            # The percentage chance that the pet sleeps on the player's bed at night, as a decimal value between 0 (never) and 1 (always).
            # The chances are checked in this order: <see cref='F:StardewValley.GameData.Pets.PetData.SleepOnBedChance' />, <see cref='F:StardewValley.GameData.Pets.PetData.SleepNearBedChance' />, and <see cref='F:StardewValley.GameData.Pets.PetData.SleepOnRugChance' />. The first match is used. If none match, the pet will choose a random empty spot in the farmhouse; if there's no empty spot, it'll sleep next to its pet bowl outside.
            ('[Optional] sleepOnBedChance', 'float', 0.05),
            # The percentage chance that the pet sleeps at the foot of the player's bed at night, as a decimal value between 0 (never) and 1 (always).
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetData.SleepOnBedChance' />.
            ('[Optional] sleepNearBedChance', 'float', 0.3),
            # The percentage chance that the pet sleeps on a random rug at night, as a decimal value between 0 (never) and 1 (always).
            # See remarks on <see cref='F:StardewValley.GameData.Pets.PetData.SleepOnBedChance' />.
            ('[Optional] sleepOnRugChance', 'float', 0.5),
            # The pet's possible actions and behaviors, defined as the states in a state machine. Essentially the pet will be in one state at any given time, which also determines which state they can transition to next. For example, a cat can transition from <c>Walk </c>to <c>BeginSitDown</c>, but it can't skip instantly from <c>Walk</c> to <c>SitDownLick</c>.
            ('behaviors', 'List<PetBehavior>'),
            # The percentage chance that the pet will try to give a gift when pet each day.
            ('[Optional] giftChance', 'float', 0.2),
            # The list of gifts that this pet can give if the gift chance roll is successful, chosen by weight similar to the pet behaviors.
            ('[Optional] gifts', 'List<PetGift>', []),
            # The cosmetic breeds which can be selected in the character customization menu when creating a save.
            ('breeds', 'List<PetBreed>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
        # Get the breed from <see cref='F:StardewValley.GameData.Pets.PetData.Breeds' /> to use for a given ID.
        # <param name='breedId'>The preferred pet breed ID.</param>
        # <param name='allowNull'>Whether to return null if the ID isn't found. If false, default to the first breed in the list instead.</param>
        def getBreedById(self, breedId: str, allowNull: bool = False) -> PetBreed:
            for breed in self.breeds:
                if breed.id == breedId: return breed
            return self.breeds[0] if not allowNull else None
    # The item spawn info for a pet gift.
    class PetGift(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # The friendship level that this pet must be at before it can give this gift. Defaults to 1000 (max friendship)
            ('[Optional] minimumFriendshipThreshold', 'int', 1000),
            # The item's weight when randomly choosing a item, relative to other items in the list (e.g. 2 is twice as likely as 1).
            ('[Optional] weight', 'float', 1.),
        ]
    # As part of <see cref='T:StardewValley.GameData.Pets.PetData' />, how to render the pet during the summit perfection slide-show.
    class PetSummitPerfectionEventData:
        _fields_ = [
            # The source rectangle within the texture to draw.
            ('sourceRect', 'Rectangle'),
            # The number of frames to show starting from the <see cref='F:StardewValley.GameData.Pets.PetSummitPerfectionEventData.SourceRect' />.
            ('animationLength', 'int'),
            # Whether to flip the pet sprite left-to-right.
            ('[Optional] flipped', 'bool'),
            # The motion to apply to the pet sprite.
            ('motion', 'Vector2'),
            # Whether to apply the 'ping pong' effect to the pet sprite animation.
            ('[Optional] pingPong', 'bool'),
        ]

class Powers:
    # The content data for powers in the powers sub menu.
    class PowersData:
        _fields_ = [
            # A tokenizable string for the power's display name.
            ('displayName', 'string'),
            # A tokenizable string for the power's description.
            ('[Optional] description = ', 'string', ''),
            # The asset name for the power's icon texture.
            ('texturePath', 'string'),
            # The top-left pixel coordinate of the 16x16 sprite icon to show in the powers menu.
            ('texturePosition', 'Point'),
            # If set, a game state query which indicates whether the power has been unlocked. Defaults to always unlocked.
            ('unlockedCondition', 'string'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, object>'),
        ]

class Shirts:
    # The metadata for a shirt item that can be equipped by players.
    class ShirtData:
        _fields_ = [
            # The shirt's internal name.
            ('[Optional] name', 'string', 'Shirt'),
            # A tokenizable string for the shirt's display name.
            ('[Optional] displayName', 'string', '[LocalizedText Strings\\Shirts:Shirt_Name]'),
            # A tokenizable string for the shirt's description.
            ('[Optional] description', 'string', '[LocalizedText Strings\\Shirts:Shirt_Description]'),
            # The price when purchased from shops.
            ('[Optional] price', 'int', 50),
            # The asset name for the texture containing the shirt's sprite, or <c>null</c> for <c>Characters/Farmer/shirts</c>.
            ('[Optional] texture', 'string'),
            # The sprite's index in the spritesheet.
            ('spriteIndex', 'int'),
            # The default shirt color.
            ('[Optional] defaultColor', 'string'),
            # Whether the shirt can be dyed.
            ('[Optional] canBeDyed', 'bool'),
            # Whether the shirt continuously shift colors. This overrides <see cref='F:StardewValley.GameData.Shirts.ShirtData.DefaultColor' /> and <see cref='F:StardewValley.GameData.Shirts.ShirtData.CanBeDyed' /> if set.
            ('[Optional] isPrismatic', 'bool'),
            # Whether the shirt has sleeves.
            ('[Optional] hasSleeves', 'bool', True),
            # Whether the shirt can be selected on the customization screen.
            ('[Optional] canChooseDuringCharacterCustomization', 'strboolng'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]

class Shops:
    # How a shop stock limit is applied in multiplayer.
    class LimitedStockMode(Enum):
        # The limit applies to every player in the world. For example, if limited to one and a player bought it, no other players can buy one.
        Global = 0
        # Each player has a separate limit. For example, if limited to one, each player could buy one.
        Player = 1
        # Ignore the limit. This is used for items that adjust their own stock via code (e.g. by checking mail).
        None_ = 2
    # Metadata for an in-game shop at which the player can buy and sell items.
    class ShopData:
        _fields_ = [
            # The currency in which all items in the shop should be priced. The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems).
            # For item trading, see <see cref='P:StardewValley.GameData.Shops.ShopItemData.TradeItemId' /> instead.
            ('[Optional] currency', 'int'),
            # How to draw stack size numbers in the shop list by default.
            # This is overridden in some special cases (e.g. recipes never show a stack count).
            ('[Optional] stackSizeVisibility', 'Shops.StackSizeVisibility?'),
            # The sound to play when the shop menu is opened.
            ('[Optional] openSound', 'string'),
            # The sound to play when an item is purchased normally.
            ('[Optional] purchaseSound', 'string'),
            # The repeating sound to play when accumulating a stack to purchase (e.g. by holding right-click on PC).
            ('[Optional] purchaseRepeatSound', 'string'),
            # The default value for <see cref='P:StardewValley.GameData.Shops.ShopItemData.ApplyProfitMargins' />, if set. This can be true (always apply it), false (never apply it), or null (apply to certain items like saplings). This is applied before any quantity modifiers. Default null.
            ('[Optional] applyProfitMargins', 'bool?'),
            # Changes to apply to the sell price for all items in the shop, unless <see cref='P:StardewValley.GameData.Shops.ShopItemData.IgnoreShopPriceModifiers' /> is <c>true</c>. These stack with <see cref='P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers' />.
            # If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the price will quadruple it).
            ('[Optional] priceModifiers', 'List<#QuantityModifier>'),
            # How multiple <see cref='F:StardewValley.GameData.Shops.ShopData.PriceModifiers' /> should be combined. This only affects that specific field, it won't affect price modifiers under <see cref='F:StardewValley.GameData.Shops.ShopData.Items' />.
            ('[Optional] priceModifierMode', '#QuantityModifier.QuantityModifierMode'),
            # The NPCs who can run the shop. If the <c>Action OpenShop</c> property specifies the <c>[owner tile area]</c> argument, at least one of the listed NPCs must be within that area; else if the <c>[owner tile area]</c> argument was omitted, the first entry in the list is used. The selected NPC's portrait will be shown in the shop UI.
            ('[Optional] owners', 'List<ShopOwnerData>'),
            # The visual theme to apply to the shop UI, or <c>null</c> for the default theme.
            ('[Optional] visualTheme', 'List<ShopThemeData>'),
            # A list of context tags for items which the player can sell to this shop. Default none.
            ('[Optional] salableItemTags', 'List<string>'),
            # The items to add to the shop inventory.
            ('[Optional] items', 'List<ShopItemData>', []),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Shops.ShopOwnerData' />, a possible dialogue that can be shown in the shop UI.
    class ShopDialogueData:
        _fields_ = [
            # An ID for this dialogue. This only needs to be unique within the current dialogue list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_DialogueName</c>.
            ('id', 'string'),
            # A game state query which indicates whether the dialogue should be available. Defaults to always available.
            ('[Optional] condition', 'string'),
            # A tokenizable string for the dialogue text to show. The resulting text is parsed using the dialogue format.
            ('[Optional] dialogue', 'string'),
            # A list of random dialogues to choose from, using the same format as <see cref='F:StardewValley.GameData.Shops.ShopDialogueData.Dialogue' />. If set, <see cref='F:StardewValley.GameData.Shops.ShopDialogueData.Dialogue' /> is ignored.
            ('[Optional] randomDialogue', 'List<string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Shops.ShopData' />, an item to add to the shop inventory.
    class ShopItemData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # The actions to perform when the item is purchased.
            ('[Optional] actionsOnPurchase', 'List<string>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
            # The qualified or unqualified item ID which must be traded to purchase this item.
            # If both <see cref='P:StardewValley.GameData.Shops.ShopItemData.TradeItemId' /> and <see cref='P:StardewValley.GameData.Shops.ShopItemData.Price' /> are specified, the player will need to provide both to get the item.
            ('[Optional] tradeItemId', 'string'),
            # The number of <see cref='P:StardewValley.GameData.Shops.ShopItemData.TradeItemId' /> needed to purchase this item.
            ('[Optional] tradeItemAmount', 'int', 1),
            # The gold price to purchase the item from the shop. Defaults to the item's normal price, or zero if <see cref='P:StardewValley.GameData.Shops.ShopItemData.TradeItemId' /> is specified.
            # If both <see cref='P:StardewValley.GameData.Shops.ShopItemData.TradeItemId' /> and <see cref='P:StardewValley.GameData.Shops.ShopItemData.Price' /> are specified, the player will need to provide both to get the item.
            ('[Optional] price', 'int', -1),
            # Whether to multiply the price by the game's profit margins, which reduces the price on easier difficulty settings. This can be true (always apply it), false (never apply it), or null (apply to certain items like saplings). This is applied before any quantity modifiers. Default null.
            ('[Optional] applyProfitMargins', 'bool?'),
            # The number of times the item can be purchased in one day. Default unlimited.
            # If the stack is more than one (e.g. via <see cref='P:StardewValley.GameData.GenericSpawnItemData.MinStack' />), each purchase still counts as one. For example, a stock limit of 5 and a stack size of 10 means the player can purchase 5 sets of 10, for a total of 50 items.
            ('[Optional] availableStock', 'int', -1),
            # If <see cref='P:StardewValley.GameData.Shops.ShopItemData.AvailableStock' /> is set, how the limit is applied in multiplayer. This has no effect on recipes.
            ('[Optional] availableStockLimit', 'LimitedStockMode'),
            # Whether to avoid adding this item to the shop if it would duplicate one that was already added. If the item is randomized, this will choose a value that hasn't already been added to the shop if possible.
            ('[Optional] avoidRepeat', 'bool'),
            # If this data produces an object and <see cref='P:StardewValley.GameData.Shops.ShopItemData.Price' /> is -1, whether to use the raw price in <c>Data/Objects</c> instead of the calculated sell-to-player price.
            ('[Optional] useObjectDataPrice', 'bool'),
            # Whether to ignore the <see cref='F:StardewValley.GameData.Shops.ShopData.PriceModifiers' /> for the shop. This has no effect on the item's <see cref='P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers' />. Default false.
            ('[Optional] ignoreShopPriceModifiers', 'bool'),
            # Changes to apply to the <see cref='P:StardewValley.GameData.Shops.ShopItemData.Price' />. These stack with <see cref='F:StardewValley.GameData.Shops.ShopData.PriceModifiers' />.
            # If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the price will quadruple it).
            ('[Optional] priceModifiers', 'List<#QuantityModifier>'),
            # How multiple <see cref='P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers' /> should be combined.
            ('[Optional] priceModifierMode', '#QuantityModifier.QuantityModifierMode'),
            # Changes to apply to the <see cref='P:StardewValley.GameData.Shops.ShopItemData.AvailableStock' />.
            # If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the available stock will quadruple it).
            ('[Optional] availableStockModifiers', 'List<#QuantityModifier>'),
            # How multiple <see cref='P:StardewValley.GameData.Shops.ShopItemData.AvailableStockModifiers' /> should be combined.
            ('[Optional] availableStockModifierMode', '#QuantityModifier.QuantityModifierMode'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Shops.ShopData' />, an NPC who can run the shop.
    class ShopOwnerData:
        _fields_ = [
            # A game state query which indicates whether this owner entry is available. Defaults to always available.
            ('[Optional] condition', 'string'),
            # The internal name of the NPC to show in the shop menu portrait, or the asset name of the portrait spritesheet to display, or an empty string to disable the portrait. Omit to use the NPC matched via <see cref='P:StardewValley.GameData.Shops.ShopOwnerData.Name' /> if any.
            ('[Optional] portrait', 'string'),
            # The dialogues to show if this entry is selected. Each day one dialogue will be randomly chosen to show in the shop UI. Defaults to a generic dialogue (if this is <c>null</c>) or hides the dialogue (if this is set but none matched).
            ('[Optional] dialogues', 'List<ShopDialogueData>'),
            # If <see cref='F:StardewValley.GameData.Shops.ShopOwnerData.Dialogues' /> has multiple matching entries, whether to re-randomize which one is selected each time the shop is opened (instead of once per day).
            ('[Optional] randomizeDialogueOnOpen', 'bool', True),
            # If set, a 'shop is closed'-style message to show instead of opening the shop.
            ('[Optional] closedMessage', 'string'),
            # An ID for this entry within the shop. This only needs to be unique within the current shop's owner list. Defaults to <see cref='P:StardewValley.GameData.Shops.ShopOwnerData.Name' />.
            ('[Optional] #id', 'string'),
            # This field is case-sensitive.
            ('#name', 'string'),
            # How this entry matches NPCs.
            ('[Ignore] type', 'ShopOwnerType'),
        ]
        # An ID for this entry within the shop. This only needs to be unique within the current shop's owner list. Defaults to <see cref='P:StardewValley.GameData.Shops.ShopOwnerData.Name' />.
        # The backing field for <see cref='P:StardewValley.GameData.Shops.ShopOwnerData.Id' />.
        _idImpl: str = None
        @property
        def id(self) -> str: return self._idImpl or self.name
        @id.setter
        def id(self, value: str) -> None: self._idImpl = value
        # The backing field for <see cref='P:StardewValley.GameData.Shops.ShopOwnerData.Name' />.
        _nameImpl: str = None
        # One of...
        # <list type='bullet'>
        #   <item><description>the internal name for the NPC who must be in range to use this entry;</description></item>
        #   <item><description><see cref='F:StardewValley.GameData.Shops.ShopOwnerType.AnyOrNone' /> to use this entry regardless of whether an NPC is in range;</description></item>
        #   <item><description><see cref='F:StardewValley.GameData.Shops.ShopOwnerType.Any' /> to use this entry if any NPC is in range;</description></item>
        #   <item><description><see cref='F:StardewValley.GameData.Shops.ShopOwnerType.None' /> to use this entry if no NPC is in range.</description></item>
        # </list>
        # This field is case-sensitive.
        @property
        def name(self) -> str: self._nameImpl
        @name.setter
        def name(self, value: str) -> None:
            pass
            # if Enum.TryParse(value, true, out ShopOwnerType result) and Enum.IsDefined(typeof(ShopOwnerType), result): self._nameImpl = result.toString(); self.type = result
            # else: self._nameImpl = value; self.type = ShopOwnerType.NamedNpc
        # Get whether an NPC name matches this entry.
        # <param name='npcName'>The NPC name to check.</param>
        def isValid(self, npcName: str) -> bool:
            match self.type:
                case ShopOwnerType.Any: return not string.IsNullOrWhiteSpace(npcName)
                case ShopOwnerType.AnyOrNone: return True
                case ShopOwnerType.None_: return string.IsNullOrWhiteSpace(npcName)
                case _: return self.name == npcName
    # Specifies how a shop owner entry matches NPCs.
    class ShopOwnerType(Enum):
        # The entry matches an NPC whose name is the entry's name.
        NamedNpc = 0
        # The entry matches any NPC.
        Any = 1
        # The entry matches regardless of whether an NPC is present.
        AnyOrNone = 2
        # The entry matches only if no NPC is present.
        None_ = 3
    # A visual theme to apply to the UI, or <c>null</c> for the default theme.
    class ShopThemeData:
        _fields_ = [
            # A game state query which indicates whether this theme should be applied. Defaults to always applied.
            ('[Optional] condition', 'string'),
            # The name of the texture to load for the shop window border, or <c>null</c> for the default shop texture.
            ('[Optional] windowBorderTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.WindowBorderTexture' /> for the shop window border, or <c>null</c> for the default shop texture. This should be an 18x18 pixel area.
            ('[Optional] windowBorderSourceRect', 'Rectangle?'),
            # The name of the texture to load for the NPC portrait background, or <c>null</c> for the default shop texture.
            ('[Optional] portraitBackgroundTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.PortraitBackgroundTexture' /> for the NPC portrait background, or <c>null</c> for the default shop texture. This should be a 74x47 pixel area.
            ('[Optional] portraitBackgroundSourceRect', 'Rectangle?'),
            # The name of the texture to load for the NPC dialogue background, or <c>null</c> for the default shop texture.
            ('[Optional] dialogueBackgroundTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.DialogueBackgroundTexture' /> for the NPC dialogue background, or <c>null</c> for the default shop texture. This should be a 60x60 pixel area.
            ('[Optional] dialogueBackgroundSourceRect', 'Rectangle?'),
            # The sprite text color for the dialogue text, or <c>null</c> for the default color. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).
            ('[Optional] dialogueColor', 'string'),
            # The sprite text shadow color for the dialogue text shadow, or <c>null</c> for the default color.
            ('[Optional] dialogueShadowColor', 'string'),
            # The name of the texture to load for the item row background, or <c>null</c> for the default shop texture.
            ('[Optional] itemRowBackgroundTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.ItemRowBackgroundTexture' /> for the item row background, or <c>null</c> for the default shop texture. This should be a 15x15 pixel area.
            ('[Optional] itemRowBackgroundSourceRect', 'Rectangle?'),
            # The color tint to apply to the item row background when the cursor is hovering over it, or <c>White</c> for no tint, or <c>null</c> for the default color. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).
            ('[Optional] itemRowBackgroundHoverColor', 'string'),
            # The sprite text color for the item text, or <c>null</c> for the default color. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).
            ('[Optional] itemRowTextColor', 'string'),
            # The name of the texture to load for the box behind the item icons, or <c>null</c> for the default shop texture.
            ('[Optional] itemIconBackgroundTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.ItemIconBackgroundTexture' /> for the item icon background, or <c>null</c> for the default shop texture. This should be an 18x18 pixel area.
            ('[Optional] itemIconBackgroundSourceRect', 'Rectangle?'),
            # The name of the texture to load for the scroll up icon, or <c>null</c> for the default shop texture.
            ('[Optional] scrollUpTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.ScrollUpTexture' /> for the scroll up icon, or <c>null</c> for the default shop texture. This should be an 11x12 pixel area.
            ('[Optional] scrollUpSourceRect', 'Rectangle?'),
            # The name of the texture to load for the scroll down icon, or <c>null</c> for the default shop texture.
            ('[Optional] scrollDownTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.ScrollDownTexture' /> for the scroll down icon, or <c>null</c> for the default shop texture. This should be an 11x12 pixel area.
            ('[Optional] scrollDownSourceRect', 'Rectangle?'),
            # The name of the texture to load for the scrollbar foreground texture, or <c>null</c> for the default shop texture.
            ('[Optional] scrollBarFrontTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.ScrollBarFrontTexture' /> for the scroll foreground, or <c>null</c> for the default shop texture. This should be a 6x10 pixel area.
            ('[Optional] scrollBarFrontSourceRect', 'Rectangle?'),
            # The name of the texture to load for the scrollbar background texture, or <c>null</c> for the default shop texture.
            ('[Optional] scrollBarBackTexture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.Shops.ShopThemeData.ScrollBarBackTexture' /> for the scroll background, or <c>null</c> for the default shop texture. This should be a 6x6 pixel area.
            ('[Optional] scrollBarBackSourceRect', 'Rectangle?'),
        ]
    # How to draw stack size numbers in the shop list.
    class StackSizeVisibility(Enum):
        # Always hide the stack size.
        Hide = 0
        # Always draw the stack size.
        Show = 1
        # Draw the stack size if more than one.
        ShowIfMultiple = 2

class SpecialOrders:
    # The period for which a special order is valid.
    class QuestDuration(Enum):
        # The order is valid until the end of this week.
        Week = 0
        # The order is valid until the end of this month.
        Month = 1
        # The order is valid until the end of the next weeks.
        TwoWeeks = 2
        # The order is valid until the end of tomorrow.
        TwoDays = 3
        # The order is valid until the end of after tomorrow.
        ThreeDays = 4
        # The valid is valid until the end of today.
        OneDay = 5
    # As part of <see cref='T:StardewValley.GameData.SpecialOrders.SpecialOrderData' />, a randomized token which can be referenced by other special order fields.
    # See remarks on <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements' /> for usage details.
    class RandomizedElement:
        _fields_ = [
            # The token name used to reference it.
            ('name', 'string'),
            # The possible values to randomly choose from. If multiple values match, one is chosen randomly.
            ('values', 'List<RandomizedElementItem>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.SpecialOrders.RandomizedElement' />, a possible value for the token.
    class RandomizedElementItem:
        _fields_ = [
            # A set of hardcoded tags that check conditions like the season, received mail, etc.
            ('[Optional] requiredTags', 'string', ''),
            # The token value to set if this item is selected.
            ('value', 'string', ''),
        ]
    class SpecialOrderData:
        _fields_ = [
            # The translated display name for the special order.
            # Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Name]</c>.
            ('name', 'string'),
            # The internal name of the NPC requesting the special order.
            ('requester', 'string'),
            # How long the player has to complete the special order.
            ('duration', 'QuestDuration'),
            # Whether the special order can be chosen again if the player has previously completed it.
            ('[Optional] repeatable', 'bool'),
            # A set of hardcoded tags that check conditions like the season, received mail, etc. Most code should use <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderData.Condition' /> instead.
            ('[Optional] requiredTags', 'string', ''),
            # A game state query which indicates whether this special order can be given.
            ('[Optional] condition', 'string', ''),
            # The order type (one of <c>Qi</c> or an empty string).
            # Setting this to <c>Qi</c> enables some custom game logic for Qi's challenges.
            ('[Optional] orderType', 'string', ''),
            # An arbitrary rule ID that can be checked by game or mod logic to enable special behavior while this order is active.
            ('[Optional] specialRule', 'string', ''),
            # The translated description text for the special order.
            # Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Text]</c>. This can contain <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements' /> tokens.
            ('text', 'string'),
            # If set, an unqualified item ID to remove everywhere in the world when this special order ends.
            ('[Optional] itemToRemoveOnEnd', 'string'),
            # If set, a mail ID to remove from all players when this special order ends.
            ('[Optional] mailToRemoveOnEnd', 'string'),
            # The randomized tokens which can be referenced by other special order fields.
            # 
            # <para>These can be used in some special order fields (noted in their code docs) in the form <c>{Name}</c> (like <c>{FishType}</c>), which returns the element's value.</para>
            # <para>If a randomized element selects an item, you can use the <c>{Name:ValueType}</c> form (like <c>{FishType:Text}</c>) to get a value related to the selected item:</para>
            # <list type='bullet'>
            #   <item><description><c>Text</c>: the item's translated display name.</description></item>
            #   <item><description><c>TextPlural</c>: equivalent to <c>Text</c> but pluralized if possible.</description></item>
            #   <item><description><c>TextPluralCapitalized</c>: equivalent to <c>Text</c> but pluralized if possible and its first letter capitalized.</description></item>
            #   <item><description><c>Tags</c>: a context tag which identifies the item, like <c>id_o_128</c> for a pufferfish.</description></item>
            #   <item><description><c>Price</c>: for objects only, the gold price for selling this item to a store (all other item types will have the value <c>1</c>).</description></item>
            # </list>
            ('[Optional] randomizedElements', 'List<RandomizedElement>'),
            # The objectives which must be achieved to complete this special order.
            ('objectives', 'List<SpecialOrderObjectiveData>'),
            # The rewards given to the player when they complete this special order.
            ('rewards', 'List<SpecialOrderRewardData>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.SpecialOrders.SpecialOrderData' />, an objective that must be achieved to complete the special order.
    class SpecialOrderObjectiveData:
        _fields_ = [
            # The name of the C# class which handles the logic for this objective.
            # The class must be in the <c>StardewValley</c> namespace, and its name must end with <c>Objective</c> (without including it in this field). For example, <c>'Gift'</c> will match the <c>StardewValley.GiftObjective</c> type.
            ('type', 'string'),
            # The translated description text for the objective.
            # Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Objective_0_Text]</c>. This can contain <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements' /> tokens.
            ('text', 'string'),
            # The number related to the objective.
            # This can contain <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements' /> tokens.
            ('requiredCount', 'string'),
            # The arbitrary data values understood by the C# class identified by <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderObjectiveData.Type' />. These may or may not allow <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements' /> tokens, depending on the class.
            ('data', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.SpecialOrders.SpecialOrderData' />, a reward given to the player when they complete this special order..
    class SpecialOrderRewardData:
        _fields_ = [
            # The name of the C# class which handles the logic for this reward.
            # The class must be in the <c>StardewValley</c> namespace, and its name must end with <c>Reward</c> (without including it in this field). For example, <c>'Money'</c> will match the <c>StardewValley.MoneyReward</c> type.
            ('type', 'string'),
            # The arbitrary data values understood by the C# class identified by <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderRewardData.Type' />. These may or may not allow <see cref='F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements' /> tokens, depending on the class.
            ('data', 'Dictionary<string, string>'),
        ]

class Tools:
    # The behavior and metadata for a tool that can be equipped by players.
    class ToolData:
        _fields_ = [
            # The name for the C# class to construct within the <c>StardewValley.Tools</c> namespace. This must be a subclass of <c>StardewValley.Tool</c>.
            ('className', 'string'),
            # The tool's internal name.
            ('name', 'string'),
            # The number of attachment slots to set, or <c>-1</c> to keep the default value.
            ('[Optional] attachmentSlots', 'int', -1),
            # The sale price for the tool in shops.
            ('[Optional] salePrice', 'int', -1),
            # A tokenizable string for the tool's display name.
            ('displayName', 'string'),
            # A tokenizable string for the tool's description.
            ('description', 'string'),
            # The asset name for the texture containing the tool's sprite.
            ('texture', 'string'),
            # The index within the <see cref='F:StardewValley.GameData.Tools.ToolData.Texture' /> for the animation sprites, where 0 is the top icon.
            ('spriteIndex', 'int'),
            # The index within the <see cref='F:StardewValley.GameData.Tools.ToolData.Texture' /> for the item icon, or <c>-1</c> to use the <see cref='F:StardewValley.GameData.Tools.ToolData.SpriteIndex' />.
            ('[Optional] menuSpriteIndex', 'int', -1),
            # The tool's initial upgrade level, or <c>-1</c> to keep the default value.
            ('[Optional] upgradeLevel', 'int', -1),
            # If set, the item ID for a tool which can be upgraded into this one using the default upgrade rules based on <see cref='F:StardewValley.GameData.Tools.ToolData.UpgradeLevel' />. This is prepended to <see cref='F:StardewValley.GameData.Tools.ToolData.UpgradeFrom' />.
            ('[Optional] conventionalUpgradeFrom', 'string'),
            # A list of items which the player can upgrade into this at Clint's shop.
            ('[Optional] upgradeFrom', 'List<ToolUpgradeData>'),
            # Whether the player can lose this tool when they die.
            ('[Optional] canBeLostOnDeath', 'bool'),
            # The class properties to set when creating the tool.
            ('[Optional] setProperties', 'Dictionary<string, string>'),
            # The <c>modData</c> values to set when the tool is created.
            ('[Optional] modData', 'Dictionary<string, string>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Tools.ToolData' />, the requirements to upgrade items into a tool.
    class ToolUpgradeData:
        _fields_ = [
            # A game state query which indicates whether this upgrade is available. Default always enabled.
            ('[Optional] condition', 'string'),
            # The gold price to upgrade the tool, or <c>-1</c> to use <see cref='F:StardewValley.GameData.Tools.ToolData.SalePrice' />.
            ('[Optional] price', 'int', -1),
            # If set, the item ID for the tool that must be in the player's inventory for the upgrade to appear. The tool will be destroyed when the upgrade is accepted.
            ('[Optional] requireToolId', 'string'),
            # If set, the item ID for an extra item that must be traded to upgrade the tool (for example, copper bars for many copper tools).
            ('[Optional] tradeItemId', 'string'),
            # The number of <see cref='F:StardewValley.GameData.Tools.ToolUpgradeData.TradeItemId' /> required.
            ('[Optional] tradeItemAmount', 'int', 1),
        ]

class Weapons:
    # The metadata for a weapon that can be used by players.
    class WeaponData:
        _fields_ = [
            # The internal weapon name.
            ('name', 'string'),
            # A tokenizable string for the weapon's translated display name.
            ('displayName', 'string'),
            # A tokenizable string for the weapon's translated description.
            ('description', 'string'),
            # The minimum base damage caused by hitting a monster with this weapon.
            ('minDamage', 'int'),
            # The maximum base damage caused by hitting a monster with this weapon.
            ('maxDamage', 'int'),
            # How far the target is pushed when hit, as a multiplier relative to a base weapon like the Rusty Sword (e.g. 1.5 for 150% of Rusty Sword's weight).
            ('[Optional] knockback', 'float', 1.),
            # How fast the player can swing the weapon. Each point of speed is worth 40ms of swing time relative to 0. This stacks with the player's weapon speed.
            ('[Optional] speed', 'int'),
            # Reduces the chance that a strike will miss.
            ('[Optional] precision', 'int'),
            # Reduces damage received by the player.
            ('[Optional] defense', 'int'),
            # The weapon type. One of <c>0</c> (stabbing sword), <c>1</c> (dagger), <c>2</c> (club or hammer), or <c>3</c> (slashing sword).
            ('type', 'int'),
            # The base mine level used to determine when this weapon appears in mine containers.
            ('[Optional] mineBaseLevel', 'int', -1),
            # The min mine level used to determine when this weapon appears in mine containers.
            ('[Optional] mineMinLevel', 'int', -1),
            # Slightly increases the area of effect.
            ('[Optional] areaOfEffect', 'int'),
            # The chance of a critical hit, as a decimal value between 0 (never) and 1 (always).
            ('[Optional] critChance', 'float', 0.02),
            # A multiplier applied to the base damage for a critical hit.
            ('[Optional] critMultiplier', 'float', 3.),
            # Whether the player can lose this weapon when they die.
            ('[Optional] canBeLostOnDeath', 'bool', True),
            # The asset name for the texture containing the weapon's sprite.
            ('texture', 'string'),
            # The index within the <see cref='F:StardewValley.GameData.Weapons.WeaponData.Texture' /> for the weapon sprite, where 0 is the top-left icon.
            ('spriteIndex', 'int'),
            # The projectiles fired when the weapon is used, if any. The continue along their path until they hit a monster and cause damage. One projectile will fire for each entry in the list. This doesn't apply for slingshots, which have hardcoded projectile logic.
            ('[Optional] projectiles', 'List<WeaponProjectile>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.Weapons.WeaponData' />, a projectile fired when the weapon is used.
    class WeaponProjectile:
        _fields_ = [
            # A key which uniquely identifies the projectile within its weapon's data. The ID should only contain alphanumeric/underscore/dot characters. For custom projectiles, this should be prefixed with your mod ID like <c>Example.ModId_ProjectileId.</c>
            ('id', 'string'),
            # The amount of damage caused when they hit a monster.
            ('[Optional] damage', 'int', 10),
            # Whether the projectile explodes when it collides with something.
            ('[Optional] explodes', 'bool'),
            # The number of times the projectile can bounce off walls before being destroyed.
            ('[Optional] bounces', 'int'),
            # The maximum tile distance the projectile can travel.
            ('[Optional] maxDistance', 'int', 4),
            # The speed at which the projectile moves.
            ('[Optional] velocity', 'int', 10),
            # The rotation velocity.
            ('[Optional] rotationVelocity', 'int', 32),
            # The length of the tail which trails behind the main projectile.
            ('[Optional] tailLength', 'int', 1),
            # The sound played when the projectile is fired.
            ('[Optional] fireSound', 'string', ''),
            # The sound played when the projectile bounces off a wall.
            ('[Optional] bounceSound', 'string', ''),
            # The sound played when the projectile collides with something.
            ('[Optional] collisionSound', 'string', ''),
            # The minimum value for a random offset applied to the direction of the project each time it's fired. If both fields are zero, it's always shot at the 90� angle matching the player's facing direction.
            ('[Optional] minAngleOffset', 'float'),
            # The maximum value for <see cref='F:StardewValley.GameData.Weapons.WeaponProjectile.MinAngleOffset' />.
            ('[Optional] maxAngleOffset', 'float'),
            # The sprite index in <c>TileSheets/Projectiles</c> to draw for this projectile.
            ('[Optional] spriteIndex', 'int', 11),
            # The item to shoot. If set, this overrides <see cref='F:StardewValley.GameData.Weapons.WeaponProjectile.SpriteIndex' />.
            ('[Optional] item', 'GameData.GenericSpawnItemData'),
        ]

class Weddings:
    # As part of <see cref='T:StardewValley.GameData.Weddings.WeddingData' />, an NPC which should attend wedding events.
    class WeddingAttendeeData:
        _fields_ = [
            # The internal name for the NPC.
            ('id', 'string'),
            # A game state query which indicates whether the NPC should attend. Defaults to always attend.
            ('[Optional] condition', 'string'),
            # The NPC's tile position and facing direction when they attend. This uses the same format as field index 2 in an event script.
            ('setup', 'string'),
            # The event script to run during the celebration, like <c>faceDirection Pierre 3 true</c> which makes Pierre turn to face left. This can contain any number of slash-delimited script commands.
            ('[Optional] celebration', 'string'),
            # Whether to add this NPC regardless of their <see cref='F:StardewValley.GameData.Characters.CharacterData.UnlockConditions' />.
            ('[Optional] ignoreUnlockConditions', 'bool'),
        ]
    class WeddingData:
        _fields_ = [
            # A tokenizable string for the event script which plays the wedding.
            # The key is the internal name of the NPC or unique ID of the player being married, else <c>default</c> for the default script which automatically handles marrying either an NPC or player.
            ('eventScript', 'Dictionary<string, string>'),
            # The other NPCs which should attend wedding events (unless they're the spouse), indexed by <see cref='F:StardewValley.GameData.Weddings.WeddingAttendeeData.Id' />.
            ('attendees', 'Dictionary<string, WeddingAttendeeData>'),
        ]

class WildTrees:
    # Metadata for a non-fruit tree type.
    class WildTreeData:
        _fields_ = [
            # The tree textures to show in game. The first matching texture will be used.
            ('textures', 'List<WildTreeTextureData>'),
            # The qualified or unqualified item ID for the seed item.
            ('seedItemId', 'string'),
            # Whether the seed can be planted by the player. If false, it can only be spawned automatically via map properties.
            ('[Optional] seedPlantable', 'bool', True),
            # The percentage chance each day that the tree will grow to the next stage without tree fertilizer, as a value from 0 (will never grow) to 1 (will grow every day).
            ('[Optional] growthChance', 'float', 0.2),
            # Overrides <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.GrowthChance' /> when tree fertilizer is applied.
            ('[Optional] fertilizedGrowthChance', 'float', 1.),
            # The percentage chance each day that the tree will plant a seed on a nearby tile, as a value from 0 (never) to 1 (always). This only applied in locations where trees drop seeds (e.g. farms in vanilla).
            ('[Optional] seedSpreadChance', 'float', 0.15),
            # The percentage chance each day that the tree will produce a seed that will drop when the tree is shaken, as a value from 0 (never) to 1 (always).
            ('[Optional] seedOnShakeChance', 'float', 0.05),
            # The percentage chance that a seed will drop when the player chops down the tree, as a value from 0 (never) to 1 (always).
            ('[Optional] seedOnChopChance', 'float', 0.75),
            # Whether to drop wood when the player chops down the tree.
            ('[Optional] dropWoodOnChop', 'bool', True),
            # Whether to drop hardwood when the player chops down the tree, if they have the Lumberjack profession.
            ('[Optional] dropHardwoodOnLumberChop', 'bool', True),
            # Whether shaking or chopping the tree causes cosmetic leaves to drop from tree and produces a leaf rustle sound. When a leaf drops, the game will use one of the four leaf sprites in the tree's spritesheet in the slot left of the stump sprite.
            ('[Optional] isLeafy', 'bool', True),
            # Whether <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.IsLeafy' /> also applies in winter.
            ('[Optional] isLeafyInWinter', 'bool'),
            # Whether <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.IsLeafy' /> also applies in fall.
            ('[Optional] isLeafyInFall', 'bool', True),
            # The rules which override which locations the tree can be planted in, if applicable. These don't override more specific checks (e.g. not being plantable on stone).
            ('[Optional] plantableLocationRules', 'List<#PlantableRule>'),
            # Whether the tree can grow in winter (subject to <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.GrowthChance' /> or <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.FertilizedGrowthChance' />).
            ('[Optional] growsInWinter', 'bool'),
            # Whether the tree is reduced to a stump in winter and regrows in spring, like the vanilla mushroom tree.
            ('[Optional] isStumpDuringWinter', 'bool'),
            # Whether woodpeckers can spawn on the tree.
            ('[Optional] allowWoodpeckers', 'bool', True),
            # Whether to render a different tree sprite when the tree hasn't been shaken that day.
            # [inheritdoc cref='F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenSeedReady' path='/remarks' />
            ('[Optional] useAlternateSpriteWhenNotShaken', 'bool'),
            # Whether to render a different tree sprite when it has a seed ready. If true, the tree spritesheet should be double-width with the alternate textures on the right.
            # If <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenNotShaken' /> or <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenSeedReady' /> is true, the tree spritesheet should be double-width with the alternate textures on the right. If both are true, the same alternate texture is used for both.
            ('[Optional] useAlternateSpriteWhenSeedReady', 'bool'),
            # The color of the cosmetic wood chips when chopping the tree. This can be...
            # <list type='bullet'>
            #   <item><description>a MonoGame property name (like <c>SkyBlue</c>);</description></item>
            #   <item><description>an RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>);</description></item>
            #   <item><description>an 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>);</description></item>
            #   <item><description>or a debris type code: <c>12</c> (brown/woody), <c>10000</c> (white), <c>100001</c> (light green), <c>100002</c> (light blue), <c>100003</c> (red), <c>100004</c> (yellow), <c>100005</c> (black), <c>100006</c> (gray), <c>100007</c> (charcoal / dim gray).</description></item>
            # </list>
            # Defaults to brown/woody.
            ('[Optional] debrisColor', 'string'),
            # When a seed is dropped subject to <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.SeedOnShakeChance' />, the item to drop instead of <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.SeedItemId' />. If this is empty or none match, the <see cref='F:StardewValley.GameData.WildTrees.WildTreeData.SeedItemId' /> will be dropped instead.
            ('[Optional] seedDropItems', 'List<WildTreeSeedDropItemData>'),
            # The additional items to drop when the tree is chopped.
            ('[Optional] chopItems', 'List<WildTreeChopItemData>'),
            # The items produced by tapping the tree when it's fully grown. If multiple items can be produced, the first available one is selected.
            ('[Optional] tapItems', 'List<WildTreeTapItemData>'),
            # The items produced by shaking the tree when it's fully grown.
            ('[Optional] shakeItems', 'List<WildTreeItemData>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
            # Whether this tree grows moss or not
            ('[Optional] growsMoss', 'bool'),
        ]
        # Get whether trees of this type can be tapped in any season.
        def canBeTapped(self) -> bool: return self.tapItems and len(self.tapItems) > 0
    # The growth state for a tree.
    # These mainly exist to make content edits more readable. Most code should use the constants like <c>Tree.seedStage</c>, which have the same values.
    class WildTreeGrowthStage(Enum):
        Seed = 0
        Sprout = 1
        Sapling = 2
        Bush = 3
        Tree = 5
    # As part of <see cref='T:StardewValley.GameData.WildTrees.WildTreeData' />, a possible item to produce.
    class WildTreeItemData(GenericSpawnItemDataWithCondition):
        _fields_ = [
            # If set, the specific season when this data should apply. For more complex conditions, see <see cref='P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition' />.
            ('[Optional] season', '#Season?'),
            # The probability that the item will be produced, as a value between 0 (never) and 1 (always).
            ('[Optional] chance', 'float', 1),
        ]
    # As part of <see cref='T:StardewValley.GameData.WildTrees.WildTreeData' />, a possible item to drop when the tree is chopped down.
    class WildTreeChopItemData(WildTreeItemData):
        _fields_ = [
            # The minimum growth stage at which to produce this item.
            ('[Optional] minSize', 'WildTreeGrowthStage?'),
            # The maximum growth stage at which to produce this item.
            ('[Optional] maxSize', 'WildTreeGrowthStage?'),
            # Whether to drop this item if the item is a stump (true), not a stump (false), or both (null).
            ('[Optional] forStump', 'bool?', False),
        ]
        # Get whether the given tree growth stage is valid for <see cref='P:StardewValley.GameData.WildTrees.WildTreeChopItemData.MinSize' /> and <see cref='P:StardewValley.GameData.WildTrees.WildTreeChopItemData.MaxSize' />.
        # <param name='size'>The tree growth stage.</param>
        # <param name='isStump'>Whether the tree is a stump.</param>
        def isValidForGrowthStage(self, size: int, isStump: bool) -> bool:
            if size == 4: size = 3
            # var nullable2 = MinSize.HasValue ? new int?((int)MinSize.GetValueOrDefault()) : new int?()
            # if (size < nullable2.GetValueOrDefault() & nullable2.HasValue) return False
            # nullable2 = MaxSize.HasValue ? new int?((int)MaxSize.GetValueOrDefault()) : new int?()
            # if (size > nullable2.GetValueOrDefault() & nullable2.HasValue) return False
            # if (ForStump.HasValue) if (!(ForStump.GetValueOrDefault() == isStump & ForStump.HasValue)) return False
            return True
    # As part of <see cref='T:StardewValley.GameData.WildTrees.WildTreeData' />, a possible item to produce when dropping the tree seed.
    class WildTreeSeedDropItemData(WildTreeItemData):
        _fields_ = [
            # If this item is dropped, whether to continue as if it hadn't been dropped for the remaining drop candidates.
            ('[Optional] continueOnDrop', 'bool'),
        ]
    # As part of <see cref='T:StardewValley.GameData.WildTrees.WildTreeData' />, a possible item to produce for tappers on the tree.
    class WildTreeTapItemData(WildTreeItemData):
        _fields_ = [
            # If set, the group only applies if the previous item produced by the tapper matches one of these qualified or unqualified item IDs (including <c>null</c> for the initial tap).
            ('[Optional] previousItemId', 'List<string>'),
            # The number of days before the tapper is ready to empty.
            ('daysUntilReady', 'int'),
            # Changes to apply to the result of <see cref='P:StardewValley.GameData.WildTrees.WildTreeTapItemData.DaysUntilReady' />.
            ('[Optional] daysUntilReadyModifiers', 'List<#QuantityModifier>'),
            # How multiple <see cref='P:StardewValley.GameData.WildTrees.WildTreeTapItemData.DaysUntilReadyModifiers' /> should be combined.
            ('[Optional] daysUntilReadyModifierMode', '#QuantityModifier.QuantityModifierMode'),
        ]
    # As part of <see cref='T:StardewValley.GameData.WildTrees.WildTreeData' />, a possible spritesheet to use for the tree.
    class WildTreeTextureData:
        _fields_ = [
            # A game state query which indicates whether this spritesheet should be applied for a tree. Defaults to always enabled.
            # This condition is checked when a tree's texture is loaded. Once it's loaded, the conditions won't be rechecked until the next day.
            ('[Optional] condition', 'string'),
            # If set, the specific season when this texture should apply. For more complex conditions, see <see cref='F:StardewValley.GameData.WildTrees.WildTreeTextureData.Condition' />.
            ('[Optional] season', '#Season?'),
            # The asset name for the tree's spritesheet.
            ('texture', 'string'),
        ]

class WorldMaps:
    # An area within a larger <see cref='T:StardewValley.GameData.WorldMaps.WorldMapRegionData' /> to draw onto the world map. This can provide textures, tooltips, and world positioning data.
    @RType()
    class WorldMapAreaData:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_AreaId</c>.
            ('id', 'string'),
            # If set, a game state query which checks whether the area should be applied. Defaults to always applied.
            ('[Optional] condition', 'string'),
            # The pixel area within the map which is covered by this area.
            ('[Optional] pixelArea', 'Rectangle'),
            # If set, a tokenizable string for the scroll text shown at the bottom of the map when the player is in the location. Defaults to none.
            ('[Optional] scrollText', 'string'),
            # The image overlays to apply to the map.
            ('[Optional] textures', 'List<WorldMapTextureData>', []),
            # The tooltips to show when hovering over parts of this area on the world map.
            ('[Optional] tooltips', 'List<WorldMapTooltipData>', []),
            # The in-world locations and tile coordinates to match to this map area.
            ('[Optional] worldPositions', 'List<WorldMapAreaPositionData>', []),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]
    # As part of <see cref='T:StardewValley.GameData.WorldMaps.WorldMapAreaData' />, a set of in-game locations and tile positions to match to the area.
    @RType()
    class WorldMapAreaPositionData:
        # The backing field for <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.Id' />.
        _idImpl: str = None
        # An ID for this entry within the list. This only needs to be unique within the current position list. Defaults to <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.LocationName' />, if set.
        @property
        def id(self) -> str:
            if self._idImpl: return self._idImpl
            if self.locationName: return self.locationName
            return next(iter(self.LocationNames), None) or self.locationContext
        @id.setter
        def id(self, value: str) -> None: self._idImpl = value
        _fields_ = [
             # If set, the smaller areas within this position which show a different scroll text.
            ('[Optional] scrollTextZones', 'List<WorldMapAreaPositionScrollTextZoneData>', []),
            # An ID for this entry within the list. This only needs to be unique within the current position list. Defaults to <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.LocationName' />, if set.
            ('[Optional] #id', 'string'),
            # If set, a game state query which checks whether this position should be applied. Defaults to always applied.
            ('[Optional] condition', 'string'),
            # The location context in which this world position applies.
            ('[Optional] locationContext', 'string'),
            # The location name to which this world position applies. Any location within the mines and the Skull Cavern will be <c>Mines</c> and <c>SkullCave</c> respectively, and festivals use the map asset name (e.g. <c>Town-EggFestival</c>).
            ('[Optional] locationName', 'string'),
            # A list of location names in which this world position applies (see <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.LocationName' /> for details).
            ('[Optional] locationNames', 'List<string>', []),
            # The tile area for the zone within the in-game location, or an empty rectangle for the entire map.
            # <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea' /> and <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.MapPixelArea' /> are used to calculate the position of a player within the map view, given their real position in-game. For example, let's say an area has tile positions (0, 0) through (10, 20), and map pixel positions (200, 200) through (300, 400). If the player is standing on tile (5, 10) in-game (in the exact middle of the location), the game would place their marker at pixel (250, 300) on the map (in the exact middle of the map area).
            ('[Optional] tileArea', 'Rectangle'),
            # The tile area within which the player is considered to be within the zone, even if they're beyond the <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea' />. Positions outside the <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea' /> will be snapped to the nearest valid position.
            ('[Optional] extendedTileArea', 'Rectangle?'),
            # The pixel coordinates for the image area on the map.
            # See remarks on <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea' />.
            ('[Optional] mapPixelArea', 'Rectangle'),
            # A tokenizable string for the scroll text shown at the bottom of the map when the player is in this area. Defaults to <see cref='F:StardewValley.GameData.WorldMaps.WorldMapAreaData.ScrollText' />.
            ('[Optional] scrollText', 'string'),
        ]
    # As part of <see cref='T:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData' />, a smaller area within this position which shows a different scroll text.
    @RType()
    class WorldMapAreaPositionScrollTextZoneData:
        _fields_ = [
            # An ID for this entry within the list. This only needs to be unique within the current position list.
            ('id', 'string'),
            # The pixel coordinates for the image area on the map.
            ('[Optional] tileArea', 'Rectangle'),
            # A tokenizable string for the scroll text shown at the bottom of the map when the player is in this area. Defaults to <see cref='P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.ScrollText' />.
            ('[Optional] scrollText', 'string'),
        ]
    # A large-scale part of the world like the Valley, containing all the areas drawn together as part of the combined map view.
    @RType()
    class WorldMapRegionData:
        _fields_ = [
            # The base texture to draw as the base texture, if any. The first matching texture is applied.
            ('baseTexture', 'List<WorldMapTextureData>', []),
            # Maps neighbor IDs for controller support in fields like <see cref='F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor' /> to the specific values to use. This allows using simplified IDs like <c>Beach/FishShop</c> instead of <c>Beach/FishShop_DefaultHours, Beach/FishShop_ExtendedHours</c>. Aliases cannot be recursive.
            ('[Optional] mapNeighborIdAliases', 'Dictionary<string, string>', {}), #OrdinalIgnoreCase
            # The areas to draw on top of the <see cref='F:StardewValley.GameData.WorldMaps.WorldMapRegionData.BaseTexture' />. These can provide tooltips, scroll text, and character marker positioning data.
            ('mapAreas', 'List<WorldMapAreaData>', []),
        ]
    # As part of a larger <see cref='T:StardewValley.GameData.WorldMaps.WorldMapAreaData' />, an image overlay to apply to the map.
    @RType()
    class WorldMapTextureData:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_OverlayId</c>.
            ('id', 'string'),
            # If set, a game state query which checks whether the overlay should be applied. Defaults to always applied.
            ('[Optional] condition', 'string'),
            # The asset name for the texture to draw when the area is applied to the map.
            ('[Optional] texture', 'string'),
            # The pixel area within the <see cref='F:StardewValley.GameData.WorldMaps.WorldMapTextureData.Texture' /> to draw, or an empty rectangle to draw the entire image.
            ('[Optional] sourceRect', 'Rectangle'),
            # The pixel area within the map area to draw the texture to. If this is an empty rectangle, defaults to the entire map (for a base texture) or <see cref='F:StardewValley.GameData.WorldMaps.WorldMapAreaData.PixelArea' /> (for a map area texture).
            ('[Optional] mapPixelArea', 'Rectangle'),
        ]
    # A tooltip shown when hovering over parts of a larger <see cref='T:StardewValley.GameData.WorldMaps.WorldMapAreaData' /> on the world map.
    @RType()
    class WorldMapTooltipData:
        _fields_ = [
            # A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_TooltipId.</c>
            ('id', 'string'),
            # If set, a game state query which checks whether the tooltip should be visible. Defaults to always visible.
            ('[Optional] condition', 'string'),
            # If set, a game state query which checks whether the area is known by the player, so the <see cref='F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.Text' /> is shown as-is. If this is false, the tooltip text is replaced with '???'. Defaults to always known.
            ('[Optional] knownCondition', 'string'),
            # The pixel area within the map which can be hovered to show this tooltip, or an empty rectangle if it covers the entire area.
            ('[Optional] pixelArea', 'Rectangle'),
            # A tokenizable string for the tooltip shown when the mouse is over the area.
            ('text', 'string'),
            # The tooltip to the left of this one for controller navigation.
            # This should be the area and tooltip ID, formatted like <c>areaId/tooltipId</c> (not case-sensitive). If there are multiple possible neighbors, they can be specified in comma-delimited form like <c>areaId/tooltipId, areaId/tooltipId, ...</c>; the first one which exists will be used.
            ('leftNeighbor', 'string'),
            # The tooltip to the right of this one for controller navigation.
            # [inheritdoc cref='F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor' path='/remarks' />
            ('rightNeighbor', 'string'),
            # The tooltip above this one for controller navigation.
            # [inheritdoc cref='F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor' path='/remarks' />
            ('upNeighbor', 'string'),
            # The tooltip below this one for controller navigation.
            # [inheritdoc cref='F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor' path='/remarks' />
            ('downNeighbor', 'string'),
        ]