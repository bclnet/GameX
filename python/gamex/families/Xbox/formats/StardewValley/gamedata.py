from __future__ import annotations
import os
from enum import Enum, Flag
from numpy import ndarray, array
from gamex.families.Xbox.formats.binary import Binary_Xnb

# types
# type Vector2 = ndarray
type Vector3 = ndarray
# type Vector4 = ndarray
# type Matrix4x4 = ndarray
# type Quaternion = ndarray

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
class GenericSpawnItemData(ISpawnItemData):
    # The backing field for <see cref='P:StardewValley.GameData.GenericSpawnItemData.Id' />.
    _idImpl: str = None
    # An ID for this entry within the current list (not the item itself, which is <see cref='P:StardewValley.GameData.GenericSpawnItemData.ItemId' />). This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.
    @property
    def id(self) -> str: #Optional
        if self._idImpl != None: return _idImpl
        if self.itemId != None: return itemId if not self.isRecipe else itemId + ' (Recipe)'
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
class GenericSpawnItemDataWithCondition(GenericSpawnItemData):
    _base_ = 'GenericSpawnItemData'
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
        ('[Optional] numberComma', 'string', ","),
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
        ('season', 'Season'),
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
        ('[Optional] plantedIn', 'PlantableRuleContext', 'PlantableRuleContext.Any'),
        # Indicates when the seed or sapling can be planted in a location if this entry is selected.
        ('result', 'PlantableResult'),
        # If this rule prevents planting the seed or sapling, the tokenizable string to show to the player (or <c>null</c> to show a generic message).
        ('[Optional] deniedMessage', 'string'),
    ]
    # Get whether this rule should be applied.
    # <param name='isGardenPot'>Whether the seed or sapling is being planted in a garden pot (else the ground).</param>
    def shouldApplyWhen(self, isGardenPot: bool) -> bool: return PlantableRuleContext(2 if isGardenPot else 1) in self.plantedIn
PlantableRule._fields_[2] = ('[Optional] plantedIn', 'PlantableRuleContext', PlantableRuleContext.Any)
# As part of another entry like <see cref='T:StardewValley.GameData.Machines.MachineData' /> or <see cref='T:StardewValley.GameData.Shops.ShopData' />, a change to apply to a numeric quantity.
class QuantityModifier:
    _fields_ = [
        # An ID for this modifier. This only needs to be unique within the current modifier list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ModifierName</c>.
        ('id', 'string'),
        # A game state query which indicates whether this change should be applied. Item-only tokens are valid for this check, and will check the input (not output) item. Defaults to always true.
        ('[Optional] condition', 'string'),
        # The type of change to apply.
        ('modification', 'ModificationType'),
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
    class QuantityModifierMode(Enum):
        # Apply each modifier to the result of the previous one. For example, two modifiers which double a value will quadruple it.
        Stack = 0
        # Apply the modifier which results in the lowest value.
        Minimum = 1
        # Apply the modifier which results in the highest value.
        Maximum = 2
# As part of <see cref='T:StardewValley.GameData.FarmAnimals.FarmAnimalData' /> or <see cref='T:StardewValley.GameData.Machines.MachineData' />, a game state counter to increment.
class StatIncrement:
    # The backing field for <see cref='P:StardewValley.GameData.StatIncrement.Id' />.
    _idImpl: str = None
    # A unique string ID for this entry within the current animal's list.
    @property
    def id(self) -> str: return self._idImpl or self.statName
    @id.setter
    def id(self, value: str): self._idImpl = value #Optional
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
    # As part of <see cref="T:StardewValley.GameData.Buffs.BuffData" /> or <see cref="T:StardewValley.GameData.Objects.ObjectBuffData" />, the attribute values to add to the player's stats.
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
            # The maximum buff duration in milliseconds. If specified and larger than <see cref="F:StardewValley.GameData.Buffs.BuffData.Duration" />, a random value between <see cref="F:StardewValley.GameData.Buffs.BuffData.Duration" /> and <see cref="F:StardewValley.GameData.Buffs.BuffData.MaxDuration" /> will be selected for each buff.
            ('[Optional] maxDuration', 'int', -1),
            # The texture to load for the buff icon.
            ('iconTexture', 'string'),
            # The sprite index for the buff icon within the <see cref="F:StardewValley.GameData.Buffs.BuffData.IconTexture" />.
            ('iconSpriteIndex', 'int'),
            # The custom buff attributes to apply, if any.
            ('[Optional] effects', 'StardewValley.GameData.Buffs.BuffAttributesData'),
            # The trigger actions to run when the buff is applied to the player.
            ('[Optional] actionsOnApply', 'List<string>'),
            # Custom fields ignored by the base game, for use by mods.
            ('[Optional] customFields', 'Dictionary<string, string>'),
        ]