using GameX.Xbox.Formats.Xna;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace GameX.Xbox.Formats.StardewValley.GameData;

/// <summary>A character's gender identity.</summary>
[RType("StardewValley.Gender"), RAssembly("StardewValley.GameData")]
public enum Gender {
    Male,
    Female,
    Undefined,
}
/// <summary>A season of the year.</summary>
[RType("StardewValley.Season")]
public enum Season {
    /// <summary>The spring season.</summary>
    Spring,
    /// <summary>The summer season.</summary>
    Summer,
    /// <summary>The fall season.</summary>
    Fall,
    /// <summary>The winter season.</summary>
    Winter,
}

/// <summary>A data entry which specifies item data to create.</summary>
public interface ISpawnItemData {
    /// <summary>The item(s) to create. This can be either a qualified item ID, or an item query like <c>ALL_ITEMS</c>.</summary>
    string ItemId { get; set; }
    /// <summary>A list of random item IDs to choose from, using the same format as <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" />. If set, <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> is ignored.</summary>
    List<string> RandomItemId { get; set; }
    /// <summary>The maximum number of item stacks to produce, or <c>null</c> to include all stacks produced by <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> or <see cref="P:StardewValley.GameData.ISpawnItemData.RandomItemId" />.</summary>
    int? MaxItems { get; set; }
    /// <summary>The minimum stack size for the item to create, or <c>-1</c> to keep the default value.</summary>
    /// <remarks>A value in the <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" /> to <see cref="P:StardewValley.GameData.ISpawnItemData.MaxStack" /> range is chosen randomly. If the maximum is lower than the minimum, the stack is set to <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" />.</remarks>
    int MinStack { get; set; }
    /// <summary>The maximum stack size for the item to create, or <c>-1</c> to match <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" />.</summary>
    /// <remarks><inheritdoc cref="P:StardewValley.GameData.ISpawnItemData.MinStack" select="/Remarks" /></remarks>
    int MaxStack { get; set; }
    /// <summary>The quality of the item to create. One of <c>0</c> (normal), <c>1</c> (silver), <c>2</c> (gold), <c>4</c> (iridium), or <c>-1</c> (keep the quality as-is).</summary>
    int Quality { get; set; }
    /// <summary>For objects only, the internal name to set (or <c>null</c> for the item's name in data). This should usually be null.</summary>
    string ObjectInternalName { get; set; }
    /// <summary>For objects only, a tokenizable string for the display name to show (or <c>null</c> for the item's default display name). See remarks on <c>Object.displayNameFormat</c>.</summary>
    string ObjectDisplayName { get; set; }
    /// <summary>For objects only, a tint color to apply to the sprite. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
    string ObjectColor { get; set; }
    /// <summary>For tool items only, the initial upgrade level, or <c>-1</c> to keep the default value.</summary>
    int ToolUpgradeLevel { get; set; }
    /// <summary>Whether to add the crafting/cooking recipe for the item, instead of the item itself.</summary>
    bool IsRecipe { get; set; }
    /// <summary>Changes to apply to the result of <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" /> and <see cref="P:StardewValley.GameData.ISpawnItemData.MaxStack" />.</summary>
    List<QuantityModifier> StackModifiers { get; set; }
    /// <summary>How multiple <see cref="P:StardewValley.GameData.ISpawnItemData.StackModifiers" /> should be combined.</summary>
    QuantityModifier.QuantityModifierMode StackModifierMode { get; set; }
    /// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.ISpawnItemData.Quality" />.</summary>
    /// <remarks>These operate on the numeric quality values (i.e. <c>0</c> = normal, <c>1</c> = silver, <c>2</c> = gold, and <c>4</c> = iridium). For example, silver � 2 is gold.</remarks>
    List<QuantityModifier> QualityModifiers { get; set; }
    /// <summary>How multiple <see cref="P:StardewValley.GameData.ISpawnItemData.QualityModifiers" /> should be combined.</summary>
    QuantityModifier.QuantityModifierMode QualityModifierMode { get; set; }
    /// <summary>Custom metadata to add to the created item's <c>modData</c> field for mod use.</summary>
    Dictionary<string, string> ModData { get; set; }
    /// <summary>A game state query which indicates whether an item produced from the other fields should be returned (e.g. to filter results from item queries like <c>ALL_ITEMS</c>). Defaults to always true.</summary>
    string PerItemCondition { get; set; }
}
/// <summary>An audio change to apply to the game's sound bank.</summary>
/// <remarks>This describes an override applied to the sound bank. The override is applied permanently for the current game session, even if it's later removed from the data asset. Overriding a cue will reset all values to the ones specified.</remarks>
[RType]
public class AudioCueData {
    /// <summary>A unique cue ID, used when playing the sound in-game. The ID should only contain alphanumeric/underscore/dot characters. For custom audio cues, this should be prefixed with your mod ID like <c>Example.ModId_AudioName</c>.</summary>
    public string Id;
    /// <summary>A list of file paths (not asset names) from which to load the audio. These can be absolute paths or relative to the game's <c>Content</c> folder. Each file can be <c>.ogg</c> or <c>.wav</c>. If you list multiple paths, a random one will be chosen each time it's played.</summary>
    [Optional] public List<string> FilePaths;
    /// <summary>The audio category, which determines which volume slider in the game options applies. This should be one of <c>Default</c>, <c>Music</c>, <c>Sound</c>, <c>Ambient</c>, or <c>Footsteps</c>. Defaults to <c>Default</c>.</summary>
    [Optional] public string Category;
    /// <summary>
    ///   <para>Whether the audio should be streamed from disk when it's played, instead of being loaded into memory ahead of time. This is only possible for Ogg Vorbis (<c>.ogg</c>) files, which otherwise will be decompressed in-memory on load.</para>
    ///   <para>This is a tradeoff between memory usage and performance, so you should consider which value is best for each audio cue:</para>
    ///   <list type="bullet">
    ///     <item><description><c>true</c>: Reduces memory usage when the audio cue isn't active, but increases performance impact when it's played. Playing the audio multiple times will multiply the memory and performance impact while they're active, since each play will stream a new instance. Recommended for longer audio cues (like music or ambient noise), or cues that are rarely used in a specific scenario (e.g. a sound that only plays once in an event).</description></item>
    ///     <item><description><c>false</c>: Increases memory usage (since it's fully loaded into memory), but reduces performance impact when it's played. It can be played any number of times without affecting memory or performance (it'll just play the cached audio). Recommended for sound effects, or short audio cues that are played occasionally.</description></item>
    ///   </list>
    /// </summary>
    [Optional] public bool StreamedVorbis;
    /// <summary>Whether the audio cue loops continuously until stopped.</summary>
    [Optional] public bool Looped;
    /// <summary>Whether to apply a reverb effect to the audio.</summary>
    [Optional] public bool UseReverb;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
}
/// <summary>The data for an item to create, used in data assets like <see cref="T:StardewValley.GameData.Machines.MachineData" /> or <see cref="T:StardewValley.GameData.Shops.ShopData" />.</summary>
[RType]
public class GenericSpawnItemData : ISpawnItemData {
    /// <summary>The backing field for <see cref="P:StardewValley.GameData.GenericSpawnItemData.Id" />.</summary>
    string _IdImpl;
    /// <summary>An ID for this entry within the current list (not the item itself, which is <see cref="P:StardewValley.GameData.GenericSpawnItemData.ItemId" />). This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.</summary>
    [Optional]
    public string Id {
        get {
            if (_IdImpl != null) return _IdImpl;
            if (ItemId != null) return !IsRecipe ? ItemId : ItemId + " (Recipe)";
            if ((RandomItemId != null ? (RandomItemId.Count > 0 ? 1 : 0) : 0) == 0) return "???";
            return !IsRecipe ? string.Join("|", RandomItemId) : string.Join("|", RandomItemId) + " (Recipe)";
        }
        set => _IdImpl = value;
    }
    /// <inheritdoc />
    [Optional] public string ItemId { get; set; }
    /// <inheritdoc />
    [Optional] public List<string> RandomItemId { get; set; }
    /// <inheritdoc />
    [Optional] public int? MaxItems { get; set; }
    /// <inheritdoc />
    [Optional] public int MinStack { get; set; } = -1;
    /// <inheritdoc />
    [Optional] public int MaxStack { get; set; } = -1;
    /// <inheritdoc />
    [Optional] public int Quality { get; set; } = -1;
    /// <inheritdoc />
    [Optional] public string ObjectInternalName { get; set; }
    /// <inheritdoc />
    [Optional] public string ObjectDisplayName { get; set; }
    /// <inheritdoc />
    [Optional] public string ObjectColor { get; set; }
    /// <inheritdoc />
    [Optional] public int ToolUpgradeLevel { get; set; } = -1;
    /// <inheritdoc />
    [Optional] public bool IsRecipe { get; set; }
    /// <inheritdoc />
    [Optional] public List<QuantityModifier> StackModifiers { get; set; }
    /// <inheritdoc />
    [Optional] public QuantityModifier.QuantityModifierMode StackModifierMode { get; set; }
    /// <inheritdoc />
    [Optional] public List<QuantityModifier> QualityModifiers { get; set; }
    /// <inheritdoc />
    [Optional] public QuantityModifier.QuantityModifierMode QualityModifierMode { get; set; }
    /// <inheritdoc />
    [Optional] public Dictionary<string, string> ModData { get; set; }
    /// <inheritdoc />
    [Optional] public string PerItemCondition { get; set; }
}
/// <summary>The data for an item to create with support for a game state query, used in data assets like <see cref="T:StardewValley.GameData.Machines.MachineData" /> or <see cref="T:StardewValley.GameData.Shops.ShopData" />.</summary>
[RType]
public class GenericSpawnItemDataWithCondition : GenericSpawnItemData {
    /// <summary>A game state query which indicates whether the item should be added. Defaults to always added.</summary>
    [Optional] public string Condition { get; set; }
}
/// <summary>An incoming phone call that the player can receive when they have a telephone.</summary>
[RType]
public class IncomingPhoneCallData {
    /// <summary>If set, a game state query which indicates whether to trigger this phone call.</summary>
    /// <remarks>Whether a player receives this call depends on two fields: <see cref="F:StardewValley.GameData.IncomingPhoneCallData.TriggerCondition" /> is checked on the host player before sending the call to all players, then <see cref="F:StardewValley.GameData.IncomingPhoneCallData.RingCondition" /> is checked on each player to determine whether the phone rings for them.</remarks>
    [Optional] public string TriggerCondition;
    /// <summary>If set, a game state query which indicates whether the phone will ring when this call is received.</summary>
    /// <inheritdoc cref="F:StardewValley.GameData.IncomingPhoneCallData.TriggerCondition" path="/remarks" />
    [Optional] public string RingCondition;
    /// <summary>The internal name of the NPC making the call. If specified, that NPC's name and portrait will be shown.</summary>
    /// <remarks>To show a portrait and NPC name, you must specify either <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromNpc" /> or <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromPortrait" />; otherwise a simple dialogue with no portrait/name will be shown.</remarks>
    [Optional] public string FromNpc;
    /// <summary>If set, overrides the portrait shown based on <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromNpc" />.</summary>
    [Optional] public string FromPortrait;
    /// <summary>If set, overrides the NPC display name shown based on <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromNpc" />.</summary>
    [Optional] public string FromDisplayName;
    /// <summary>A tokenizable string for the call text.</summary>
    [Optional] public string Dialogue;
    /// <summary>Whether to ignore the base chance of receiving a call for this call.</summary>
    [Optional] public bool IgnoreBaseChance;
    /// <summary>If set, marks this as a simple dialogue box without an NPC name and portrait, with lines split into multiple boxes by this substring. For example, using <c>#</c> will split <c>Box A#Box B#Box C</c> into three consecutive dialogue boxes.</summary>
    /// <remarks>You should leave this null in most cases, and use the regular dialogue format in <see cref="F:StardewValley.GameData.IncomingPhoneCallData.Dialogue" /> to split lines if needed. This is mainly intended to support some older vanilla phone calls.</remarks>
    [Optional] public string SimpleDialogueSplitBy;
    /// <summary>The maximum number of times this phone call can be received, or -1 for no limit.</summary>
    [Optional] public int MaxCalls = 1;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
}
/// <summary>The data for a jukebox track.</summary>
[RType]
public class JukeboxTrackData {
    /// <summary>A tokenizable string for the track's display name, or <c>null</c> to use the ID (i.e. cue name).</summary>
    public string Name;
    /// <summary>Whether this track is available. This can be <c>true</c> (always available), <c>false</c> (never available), or <c>null</c> (available if the player has heard it).</summary>
    [Optional] public bool? Available;
    /// <summary>A list of alternative track IDs. Any tracks matching one of these IDs will use this entry.</summary>
    [Optional] public List<string> AlternativeTrackIds;
}
/// <summary>An item which is otherwise unobtainable if lost, so it can appear in the crow's lost items shop.</summary>
[RType]
public class LostItem {
    /// <summary>A unique string ID for this entry in this list.</summary>
    public string Id;
    /// <summary>The qualified item ID to add to the shop.</summary>
    public string ItemId;
    /// <summary>The mail flag required to add this item.</summary>
    /// <remarks>The number added to the shop is the number of players which match this field minus the number of the item which exist in the world. If you specify multiple criteria fields, only one is applied in the order <see cref="F:StardewValley.GameData.LostItem.RequireMailReceived" /> and then <see cref="F:StardewValley.GameData.LostItem.RequireEventSeen" />.</remarks>
    [Optional] public string RequireMailReceived;
    /// <summary>The event ID that must be seen to add this item.</summary>
    /// <remarks><inheritdoc cref="F:StardewValley.GameData.LostItem.RequireMailReceived" path="/remarks" /></remarks>
    [Optional] public string RequireEventSeen;
}
/// <summary>The metadata for a mannequin which can be placed in the world and used to store and display clothing.</summary>
[RType]
public class MannequinData {
    /// <summary>A tokenizable string for the item's translated display name.</summary>
    public string DisplayName;
    /// <summary>A tokenizable string for the item's translated description.</summary>
    public string Description;
    /// <summary>The asset name for the texture containing the item sprite, or <c>null</c> for <c>TileSheets/Mannequins</c>.</summary>
    public string Texture;
    /// <summary>The asset name for the texture containing the placed world sprite.</summary>
    public string FarmerTexture;
    /// <summary>The sprite's index in the <see cref="F:StardewValley.GameData.MannequinData.Texture" /> spritesheet.</summary>
    public int SheetIndex;
    /// <summary>For clothing with gender variants, whether to display the male (true) or female (false) variant.</summary>
    public bool DisplaysClothingAsMale = true;
    /// <summary>Whether to enable rare Easter egg 'cursed' behavior.</summary>
    [Optional] public bool Cursed;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
}
/// <summary>The metadata for a custom farm layout which can be selected by players.</summary>
[RType]
public class ModFarmType {
    /// <summary>A key which uniquely identifies this farm type. The ID should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_FarmType.</c></summary>
    public string Id;
    /// <summary>Where to get the translatable farm name and description. This must be a key in the form <c>{asset name}:{key}</c>; for example, <c>Strings/UI:Farm_Description</c> will get it from the <c>Farm_Description</c> entry in the <c>Strings/UI</c> file. The translated text must be in the form <c>{name}_{description}</c>.</summary>
    public string TooltipStringPath;
    /// <summary>The map asset name relative to the game's <c>Content/Maps</c> folder.</summary>
    public string MapName;
    /// <summary>The asset name for a 22x20 pixel icon texture, shown on the 'New Game' and co-op join screens. Defaults to the standard farm type's icon.</summary>
    [Optional] public string IconTexture;
    /// <summary>The asset name for a 131x61 pixel texture that's drawn over the farm area in the in-game world map. Defaults to the standard farm type's texture.</summary>
    [Optional] public string WorldMapTexture;
    /// <summary>Whether monsters should spawn by default on this farm map. This affects the initial value of the advanced option during save creation, which the player can change.</summary>
    [Optional] public bool SpawnMonstersByDefault;
    /// <summary>Mod-specific metadata for the farm type.</summary>
    [Optional] public Dictionary<string, string> ModData;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
}
/// <summary>The metadata for a custom language which can be selected by players.</summary>
[RType]
public class ModLanguage {
    /// <summary>A key which uniquely identifies this language. The ID should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_Language.</c></summary>
    public string Id;
    /// <summary>The language code for this localization. This should ideally be an ISO 639-1 code, with only letters and hyphens.</summary>
    public string LanguageCode;
    /// <summary>The asset name for a 174x78 pixel texture containing the button of the language for language selection menu. The top half of the sprite is the default state, while the bottom half is the hover state.</summary>
    public string ButtonTexture;
    /// <summary>Whether the language uses the game's default fonts. Set to false to enable a custom font via <see cref="F:StardewValley.GameData.ModLanguage.FontFile" /> and <see cref="F:StardewValley.GameData.ModLanguage.FontPixelZoom" />.</summary>
    public bool UseLatinFont;
    /// <summary>If <see cref="F:StardewValley.GameData.ModLanguage.UseLatinFont" /> is false, the asset name for the custom BitMap font.</summary>
    [Optional] public string FontFile;
    /// <summary>If <see cref="F:StardewValley.GameData.ModLanguage.UseLatinFont" /> is false, a factor by which to multiply the font size. The recommended baseline is 1.5, but you can adjust it to make your text smaller or bigger in-game.</summary>
    [Optional] public float FontPixelZoom;
    /// <summary>Whether to shift the font up by four pixels (multiplied by the <see cref="F:StardewValley.GameData.ModLanguage.FontPixelZoom" /> if applicable), to better align languages with larger characters like Chinese and Japanese.</summary>
    [Optional] public bool FontApplyYOffset;
    /// <summary>The line spacing value used by the game's <c>smallFont</c> font.</summary>
    [Optional] public int SmallFontLineSpacing = 26;
    /// <summary>Whether the social tab and gift log will use gender-specific translations (like the vanilla Portuguese language).</summary>
    [Optional] public bool UseGenderedCharacterTranslations;
    /// <summary>The string to use as the thousands separator (like <c>","</c> for <c>5,000,000</c>).</summary>
    [Optional] public string NumberComma = ",";
    /// <summary>A string which describes the in-game time format, with tokens replaced by in-game values. For example, <c>[HOURS_12]:[MINUTES] [AM_PM]</c> would show "12:00 PM" at noon.</summary>
    /// <remarks>
    ///   The valid tokens are:
    ///   <list type="bullet">
    ///     <item><description><c>[HOURS_12]</c>: hours in 12-hour format, where midnight and noon are both "12".</description></item>
    ///     <item><description><c>[HOURS_12_0]</c>: hours in 12-hour format, where midnight and noon are both "0".</description></item>
    ///     <item><description><c>[HOURS_24]</c>: hours in 24-hour format, where midnight is "0" and noon is "12".</description></item>
    ///     <item><description><c>[HOURS_24_0]</c>: hours in 24-hour format with zero-padding, where midnight is "00" and noon is "12".</description></item>
    ///     <item><description><c>[MINUTES]</c>: minutes with zero-padding.</description></item>
    ///     <item><description><c>[AM_PM]</c>: the localized text for "am" or "pm". The game shows "pm" between noon and 11:59pm inclusively, else "am".</description></item>
    ///   </list>
    /// </remarks>
    public string TimeFormat;
    /// <summary>A string which describes the in-game time format. Equivalent to <see cref="F:StardewValley.GameData.ModLanguage.TimeFormat" />, but used for the in-game clock.</summary>
    public string ClockTimeFormat;
    /// <summary>A string which describes the in-game date format as shown in the in-game clock, with tokens replaced by in-game values. For example, <c>[DAY_OF_WEEK] [DAY_OF_MONTH]</c> would show <c>Mon 1</c>. </summary>
    /// <remarks>
    ///   The valid tokens are:
    ///   <list type="bullet">
    ///     <item><description><c>[DAY_OF_WEEK]</c>: the translated, abbreviated day of week.</description></item>
    ///     <item><description><c>[DAY_OF_MONTH]</c>: the numerical day of the month.</description></item>
    ///   </list>
    /// </remarks>
    public string ClockDateFormat;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
}
/// <summary>The metadata for a custom floor or wallpaper item.</summary>
[RType]
public class ModWallpaperOrFlooring {
    /// <summary>A key which uniquely identifies this wallpaper or flooring. This should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_WallpaperName</c>.</summary>
    public string Id;
    /// <summary>The asset name which contains 32x32 pixel (flooring) or 16x48 pixel (wallpaper) sprites. The tilesheet must be 256 pixels wide, but can have any number of flooring/wallpaper rows.</summary>
    public string Texture;
    /// <summary>Whether this is a flooring tilesheet; else it's a wallpaper tilesheet.</summary>
    public bool IsFlooring;
    /// <summary>The number of flooring or wallpaper sprites in the tilesheet.</summary>
    public int Count;
}
/// <summary>The data for an Adventurer's Guild monster eradication goal.</summary>
[RType]
public class MonsterSlayerQuestData {
    /// <summary>A tokenizable string for the goal's display name, shown on the board in the Adventurer's Guild.</summary>
    public string DisplayName;
    /// <summary>A list of monster IDs that are counted towards the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.Count" />.</summary>
    public List<string> Targets;
    /// <summary>The total number of monsters matching <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.Targets" /> which must be defeated to complete this goal.</summary>
    public int Count;
    /// <summary>The qualified item ID for the item that can be collected from Gil when this goal is completed. There's no reward item if omitted.</summary>
    [Optional] public string RewardItemId;
    /// <summary>The price of the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardItemId" /> in Marlon's shop, or <c>-1</c> to disable buying it from Marlon.</summary>
    [Optional] public int RewardItemPrice = -1;
    /// <summary>A tokenizable string for custom dialogue from Gil shown before collecting the rewards, if any.</summary>
    /// <remarks>If <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardDialogueFlag" /> isn't set, then this dialogue will be shown each time the reward menus is opened until the player collects the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardItemId" /> (if any).</remarks>
    [Optional] public string RewardDialogue;
    /// <summary>A mail flag ID which indicates whether the player has seen the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardDialogue" />.</summary>
    /// <remarks>This doesn't send a letter; see <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail" /> or <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll" /> for that.</remarks>
    [Optional] public string RewardDialogueFlag;
    /// <summary>The mail flag ID to set for the current player when this goal is completed, if any.</summary>
    /// <remarks>This doesn't send a letter; see <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail" /> or <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll" /> for that.</remarks>
    [Optional] public string RewardFlag;
    /// <summary>The mail flag ID to set for all players when this goal is completed, if any.</summary>
    /// <remarks>This doesn't send a letter; see <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail" /> or <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll" /> for that.</remarks>
    [Optional] public string RewardFlagAll;
    /// <summary>The mail letter ID to add to the current player's mailbox tomorrow, if set.</summary>
    [Optional] public string RewardMail;
    /// <summary>The mail letter ID to add to all players' mailboxes tomorrow, if set.</summary>
    [Optional] public string RewardMailAll;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
}
[RType]
public enum MusicContext {
    Default,
    /// <remarks>
    /// Confusingly, <see cref="F:StardewValley.GameData.MusicContext.SubLocation" /> has a higher MusicContext value than <see cref="F:StardewValley.GameData.MusicContext.Default" />, but is used when figuring out what song to play in split-screen.
    /// Songs with this value are prioritized above ambient noises, but below other instances' default songs -- so this should be used for things like specialized ambient
    /// music.
    /// </remarks>
    SubLocation,
    MusicPlayer,
    Event,
    MiniGame,
    ImportantSplitScreenMusic,
    MAX,
}
/// <summary>The metadata for a festival like the Night Market which replaces an in-game location for a period of time, which the player can enter/leave anytime, and which doesn't affect the passage of time.</summary>
[RType]
public class PassiveFestivalData {
    /// <summary>A tokenizable string for the display name shown on the calendar.</summary>
    public string DisplayName;
    /// <summary>A game state query which indicates whether the festival is enabled (subject to the other fields like <see cref="F:StardewValley.GameData.PassiveFestivalData.StartDay" /> and <see cref="F:StardewValley.GameData.PassiveFestivalData.EndDay" />). Defaults to always enabled.</summary>
    public string Condition;
    /// <summary>Whether the festival appears on the calendar, using the same icon as the Night Market. Default true.</summary>
    [Optional] public bool ShowOnCalendar = true;
    /// <summary>The season when the festival becomes active.</summary>
    public Season Season;
    /// <summary>The day of month when the festival becomes active.</summary>
    public int StartDay;
    /// <summary>The last day of month when the festival is active.</summary>
    public int EndDay;
    /// <summary>The time of day when the festival opens each day.</summary>
    public int StartTime;
    /// <summary>A tokenizable string for the in-game toast notification shown when the festival begins each day.</summary>
    public string StartMessage;
    /// <summary>If true, the in-game notification for festival start will only play on the first day</summary>
    [Optional] public bool OnlyShowMessageOnFirstDay;
    /// <summary>The locations to swap for the duration of the festival, where the key is the original location's internal name and the value is the new location's internal name.</summary>
    /// <remarks>Despite the field name, this swaps the full locations, not the location's map asset.</remarks>
    [Optional] public Dictionary<string, string> MapReplacements;
    /// <summary>A C# method which applies custom logic when the day starts.</summary>
    /// <remarks>This must be specified in the form <c>{full type name}: {method name}</c>. The method must be static, take zero arguments, and return void.</remarks>
    [Optional] public string DailySetupMethod;
    /// <summary>A C# method which applies custom logic overnight after the last day of the festival.</summary>
    /// <remarks>This must be specified in the form <c>{full type name}: {method name}</c>. The method must be static, take zero arguments, and return void.</remarks>
    [Optional] public string CleanupMethod;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
}
/// <summary>Indicates when a seed/sapling can be planted in a location.</summary>
[RType]
public enum PlantableResult {
    /// <summary>The seed/sapling can be planted if the location normally allows it.</summary>
    Default,
    /// <summary>The seed/sapling can be planted here, regardless of whether the location normally allows it.</summary>
    Allow,
    /// <summary>The seed/sapling can't be planted here, regardless of whether the location normally allows it.</summary>
    Deny,
}
/// <summary>As part of <see cref="T:StardewValley.GameData.PlantableRule" />, indicates which cases the rule applies to.</summary>
[Flags, RType]
public enum PlantableRuleContext {
    /// <summary>This rule applies when planting into the ground.</summary>
    Ground = 1,
    /// <summary>This rule applies when planting in a garden pot.</summary>
    GardenPot = 2,
    /// <summary>This rule always applies.</summary>
    Any = GardenPot | Ground, // 0x00000003
}
/// <summary>As part of assets like <see cref="T:StardewValley.GameData.Crops.CropData" /> or <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, indicates when a seed or sapling can be planted in a location.</summary>
[RType]
public class PlantableRule {
    /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries for vanilla items, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
    public string Id;
    /// <summary>A game state query which indicates whether this entry applies. Default true.</summary>
    [Optional] public string Condition;
    /// <summary>When this rule should be applied.</summary>
    /// <remarks>Note that this doesn't allow bypassing built-in restrictions (e.g. trees can't be planted in garden pots regardless of the plantable location rules).</remarks>
    [Optional] public PlantableRuleContext PlantedIn = PlantableRuleContext.Any;
    /// <summary>Indicates when the seed or sapling can be planted in a location if this entry is selected.</summary>
    public PlantableResult Result;
    /// <summary>If this rule prevents planting the seed or sapling, the tokenizable string to show to the player (or <c>null</c> to show a generic message).</summary>
    [Optional] public string DeniedMessage;
    /// <summary>Get whether this rule should be applied.</summary>
    /// <param name="isGardenPot">Whether the seed or sapling is being planted in a garden pot (else the ground).</param>
    public bool ShouldApplyWhen(bool isGardenPot) => PlantedIn.HasFlag((PlantableRuleContext)(isGardenPot ? 2 : 1));
}
/// <summary>As part of another entry like <see cref="T:StardewValley.GameData.Machines.MachineData" /> or <see cref="T:StardewValley.GameData.Shops.ShopData" />, a change to apply to a numeric quantity.</summary>
[RType]
public class QuantityModifier {
    /// <summary>An ID for this modifier. This only needs to be unique within the current modifier list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ModifierName</c>.</summary>
    public string Id;
    /// <summary>A game state query which indicates whether this change should be applied. Item-only tokens are valid for this check, and will check the input (not output) item. Defaults to always true.</summary>
    [Optional] public string Condition;
    /// <summary>The type of change to apply.</summary>
    public ModificationType Modification;
    /// <summary>The operand to apply to the target value (e.g. the multiplier if <see cref="F:StardewValley.GameData.QuantityModifier.Modification" /> is set to <see cref="F:StardewValley.GameData.QuantityModifier.ModificationType.Multiply" />).</summary>
    [Optional] public float Amount;
    /// <summary>A list of random amounts to choose from, using the same format as <see cref="F:StardewValley.GameData.QuantityModifier.Amount" />. If set, <see cref="F:StardewValley.GameData.QuantityModifier.Amount" /> is ignored.</summary>
    [Optional] public List<float> RandomAmount;
    /// <summary>Apply the change to a target value.</summary>
    /// <param name="value">The current target value.</param>
    /// <param name="modification">The type of change to apply.</param>
    /// <param name="amount">The operand to apply to the target value (e.g. the multiplier if <paramref name="modification" /> is set to <see cref="F:StardewValley.GameData.QuantityModifier.ModificationType.Multiply" />).</param>
    public static float Apply(float value, ModificationType modification, float amount) {
        return modification switch {
            ModificationType.Add => value + amount,
            ModificationType.Subtract => value - amount,
            ModificationType.Multiply => value * amount,
            ModificationType.Divide => value / amount,
            ModificationType.Set => amount,
            _ => value,
        };
    }
    /// <summary>The type of change to apply for a <see cref="T:StardewValley.GameData.QuantityModifier" />.</summary>
    [RType]
    public enum ModificationType {
        /// <summary>Add a number to the current value.</summary>
        Add,
        /// <summary>Subtract a number from the current value.</summary>
        Subtract,
        /// <summary>Multiply the current value by a number.</summary>
        Multiply,
        /// <summary>Divide the current value by a number.</summary>
        Divide,
        /// <summary>Overwrite the current value with a number.</summary>
        Set,
    }
    /// <summary>Indicates how multiple quantity modifiers are combined.</summary>
    [RType]
    public enum QuantityModifierMode {
        /// <summary>Apply each modifier to the result of the previous one. For example, two modifiers which double a value will quadruple it.</summary>
        Stack,
        /// <summary>Apply the modifier which results in the lowest value.</summary>
        Minimum,
        /// <summary>Apply the modifier which results in the highest value.</summary>
        Maximum,
    }
}
/// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" /> or <see cref="T:StardewValley.GameData.Machines.MachineData" />, a game state counter to increment.</summary>
[RType]
public class StatIncrement {
    /// <summary>The backing field for <see cref="P:StardewValley.GameData.StatIncrement.Id" />.</summary>
    string _idImpl;
    /// <summary>A unique string ID for this entry within the current animal's list.</summary>
    [Optional] public string Id { get => _idImpl ?? StatName; set => _idImpl = value; }
    /// <summary>The qualified or unqualified item ID for the item to match.</summary>
    /// <remarks>You can specify any combination of <see cref="P:StardewValley.GameData.StatIncrement.RequiredItemId" /> and <see cref="P:StardewValley.GameData.StatIncrement.RequiredTags" />. The input item must match all specified fields; if none are specified, this conversion will always match.</remarks>
    [Optional] public string RequiredItemId;
    /// <summary>A comma-delimited list of context tags required on the main input item. The stat is only incremented if the item has all of these. You can negate a tag with <c>!</c> (like <c>bone_item,!fossil_item</c> for bone items that aren't fossils). Defaults to always enabled.</summary>
    /// <inheritdoc cref="P:StardewValley.GameData.StatIncrement.RequiredItemId" select="Remarks" />
    [Optional] public List<string> RequiredTags;
    /// <summary>The name of the stat counter field on <c>Game1.stats</c>.</summary>
    public string StatName;
}
/// <summary>A cosmetic sprite to show temporarily, with optional effects and animation.</summary>
[RType]
public class TemporaryAnimatedSpriteDefinition {
    /// <summary>The unique string ID for this entry in the list.</summary>
    public string Id;
    /// <summary>A game state query which indicates whether to add this temporary sprite.</summary>
    [Optional] public string Condition;
    /// <summary>The asset name for the texture under the game's <c>Content</c> folder for the animated sprite.</summary>
    public string Texture;
    /// <summary>The pixel area for the first animated frame within the <see cref="F:StardewValley.GameData.TemporaryAnimatedSpriteDefinition.Texture" />.</summary>
    public Rectangle SourceRect;
    /// <summary>The millisecond duration for each frame in the animation.</summary>
    [Optional] public float Interval = 100f;
    /// <summary>The number of frames in the animation.</summary>
    [Optional] public int Frames = 1;
    /// <summary>The number of times to repeat the animation.</summary>
    [Optional] public int Loops;
    /// <summary>A pixel offset applied to the sprite, relative to the top-left corner of the machine's collision box.</summary>
    [Optional] public Vector2 PositionOffset = Vector2.Zero;
    [Optional] public bool Flicker;
    /// <summary>Whether to flip the sprite horizontally when it's drawn.</summary>
    [Optional] public bool Flip;
    /// <summary>The tile Y position to use in the layer depth calculation, which affects which sprite is drawn on top if two sprites overlap.</summary>
    [Optional] public float SortOffset;
    [Optional] public float AlphaFade;
    /// <summary>A multiplier applied to the sprite size (in addition to the normal 4� pixel zoom).</summary>
    [Optional] public float Scale = 1f;
    [Optional] public float ScaleChange;
    /// <summary>The rotation to apply to the sprite when drawn, measured in radians.</summary>
    [Optional] public float Rotation;
    [Optional] public float RotationChange;
    /// <summary>A tint color to apply to the sprite. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
    [Optional] public string Color;
}
/// <summary>An action that's performed when a trigger is called and its conditions are met.</summary>
[RType]
public class TriggerActionData {
    /// <summary>A unique string ID for this action in the global list.</summary>
    public string Id;
    /// <summary>When the action should be checked. This must be a space-delimited list of registered trigger types like <c>DayStarted</c>.</summary>
    public string Trigger;
    /// <summary>If set, a game state query which indicates whether the action should run when the trigger runs.</summary>
    [Optional] public string Condition;
    /// <summary>If set, a game state query which indicates that the action should be marked applied when this condition matches. This happens before <see cref="F:StardewValley.GameData.TriggerActionData.Condition" />, <see cref="F:StardewValley.GameData.TriggerActionData.Action" />, and <see cref="F:StardewValley.GameData.TriggerActionData.Actions" /> are applied.</summary>
    /// <remarks>This allows optimizing cases where the action will never be applied, to avoid parsing the <see cref="F:StardewValley.GameData.TriggerActionData.Condition" /> each time.</remarks>
    [Optional] public string SkipPermanentlyCondition;
    /// <summary>Whether to only run this action for the main player.</summary>
    [Optional] public bool HostOnly;
    /// <summary>The single action to perform.</summary>
    /// <remarks><see cref="F:StardewValley.GameData.TriggerActionData.Action" /> and <see cref="F:StardewValley.GameData.TriggerActionData.Actions" /> can technically be used together, but generally you should pick one or the other.</remarks>
    [Optional] public string Action;
    /// <summary>The actions to perform.</summary>
    /// <inheritdoc cref="F:StardewValley.GameData.TriggerActionData.Action" path="/remarks" />
    [Optional] public List<string> Actions;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
    /// <summary>Whether to mark the action applied when it's applied. If false, the action can repeat immediately when the same trigger is raised, and queries like <c>PLAYER_HAS_RUN_TRIGGER_ACTION</c> will return false for it.</summary>
    [Optional] public bool MarkActionApplied = true;
}
/// <summary>The data for a trinket item.</summary>
[RType]
public class TrinketData {
    /// <summary>A tokenizable string for the item display name.</summary>
    public string DisplayName;
    /// <summary>A tokenizable string for the item description.</summary>
    public string Description;
    /// <summary>The asset name for the texture containing the item sprite. This should contain a grid of 16x16 sprites.</summary>
    public string Texture;
    /// <summary>The sprite index for this trinket within the <see cref="F:StardewValley.GameData.TrinketData.Texture" />.</summary>
    public int SheetIndex;
    /// <summary>The full name of the C# <c>TrinketEffect</c> subclass which implements the trinket behavior. This can safely be a mod class.</summary>
    public string TrinketEffectClass;
    /// <summary>Whether this trinket can be spawned randomly (e.g. in mine treasure chests).</summary>
    [Optional] public bool DropsNaturally = true;
    /// <summary>Whether players can re-roll this trinket's stats using an anvil.</summary>
    [Optional] public bool CanBeReforged = true;
    /// <summary>Custom fields which may be used by the <see cref="F:StardewValley.GameData.TrinketData.TrinketEffectClass" /> or mods.</summary>
    [Optional] public Dictionary<string, string> CustomFields;
    /// <summary>A lookup of arbitrary <c>modData</c> values to attach to the trinket when it's constructed.</summary>
    [Optional] public Dictionary<string, string> ModData;
}

public class BigCraftables {
    [RType]
    public class BigCraftableData {
        /// <summary>The internal item name.</summary>
        public string Name;
        /// <summary>A tokenizable string for the item's translated display name.</summary>
        public string DisplayName;
        /// <summary>A tokenizable string for the item's translated description.</summary>
        public string Description;
        /// <summary>The price when sold by the player. This is not the price when bought from a shop.</summary>
        [Optional] public int Price;
        /// <summary>How the item can be picked up. The possible values are 0 (pick up with any tool), 1 (destroyed if hit with an axe/hoe/pickaxe, or picked up with any other tool), or 2 (can't be removed once placed).</summary>
        [Optional] public int Fragility;
        /// <summary>Whether the item can be placed outdoors.</summary>
        [Optional] public bool CanBePlacedOutdoors = true;
        /// <summary>Whether the item can be placed indoors.</summary>
        [Optional] public bool CanBePlacedIndoors = true;
        /// <summary>Whether this is a lamp and should produce light when dark.</summary>
        [Optional] public bool IsLamp;
        /// <summary>The asset name for the texture containing the item's sprite, or <c>null</c> for <c>TileSheets/Craftables</c>.</summary>
        [Optional] public string Texture;
        /// <summary>The sprite's index in the spritesheet.</summary>
        public int SpriteIndex;
        /// <summary>The custom context tags to add for this item (in addition to the tags added automatically based on the other object data).</summary>
        [Optional] public List<string> ContextTags;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
}

public class Buffs {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buffs.BuffData" /> or <see cref="T:StardewValley.GameData.Objects.ObjectBuffData" />, the attribute values to add to the player's stats.</summary>
    [RType]
    public class BuffAttributesData {
        /// <summary>The buff to the player's combat skill level.</summary>
        [Optional] public float CombatLevel;
        /// <summary>The buff to the player's farming skill level.</summary>
        [Optional] public float FarmingLevel;
        /// <summary>The buff to the player's fishing skill level.</summary>
        [Optional] public float FishingLevel;
        /// <summary>The buff to the player's mining skill level.</summary>
        [Optional] public float MiningLevel;
        /// <summary>The buff to the player's luck skill level.</summary>
        [Optional] public float LuckLevel;
        /// <summary>The buff to the player's foraging skill level.</summary>
        [Optional] public float ForagingLevel;
        /// <summary>The buff to the player's max stamina.</summary>
        [Optional] public float MaxStamina;
        /// <summary>The buff to the player's magnetic radius.</summary>
        [Optional] public float MagneticRadius;
        /// <summary>The buff to the player's walk speed.</summary>
        [Optional] public float Speed;
        /// <summary>The buff to the player's defense.</summary>
        [Optional] public float Defense;
        /// <summary>The buff to the player's attack power.</summary>
        [Optional] public float Attack;
        /// <summary>The combined multiplier applied to the player's attack power.</summary>
        [Optional] public float AttackMultiplier;
        /// <summary>The combined buff to the player's resistance to negative effects.</summary>
        [Optional] public float Immunity;
        /// <summary>The combined multiplier applied to monster knockback when hit by the player's weapon.</summary>
        [Optional] public float KnockbackMultiplier;
        /// <summary>The combined multiplier applied to the player's weapon swing speed.</summary>
        [Optional] public float WeaponSpeedMultiplier;
        /// <summary>The combined multiplier applied to the player's critical hit chance.</summary>
        [Optional] public float CriticalChanceMultiplier;
        /// <summary>The combined multiplier applied to the player's critical hit damage.</summary>
        [Optional] public float CriticalPowerMultiplier;
        /// <summary>The combined multiplier applied to the player's weapon accuracy.</summary>
        [Optional] public float WeaponPrecisionMultiplier;
    }
    /// <summary>A predefined buff which can be applied in-game.</summary>
    [RType]
    public class BuffData {
        /// <summary>A tokenizable string for the translated buff name.</summary>
        public string DisplayName;
        /// <summary>A tokenizable string for the translated buff description.</summary>
        [Optional] public string Description;
        /// <summary>Whether this buff counts as a debuff, so its duration should be halved when wearing a Sturdy Ring.</summary>
        [Optional] public bool IsDebuff;
        /// <summary>The glow color to apply to the player, if any.</summary>
        [Optional] public string GlowColor;
        /// <summary>The buff duration in milliseconds, or <c>-2</c> for a buff that should last all day.</summary>
        public int Duration;
        /// <summary>The maximum buff duration in milliseconds. If specified and larger than <see cref="F:StardewValley.GameData.Buffs.BuffData.Duration" />, a random value between <see cref="F:StardewValley.GameData.Buffs.BuffData.Duration" /> and <see cref="F:StardewValley.GameData.Buffs.BuffData.MaxDuration" /> will be selected for each buff.</summary>
        [Optional] public int MaxDuration = -1;
        /// <summary>The texture to load for the buff icon.</summary>
        public string IconTexture;
        /// <summary>The sprite index for the buff icon within the <see cref="F:StardewValley.GameData.Buffs.BuffData.IconTexture" />.</summary>
        public int IconSpriteIndex;
        /// <summary>The custom buff attributes to apply, if any.</summary>
        [Optional] public BuffAttributesData Effects;
        /// <summary>The trigger actions to run when the buff is applied to the player.</summary>
        [Optional] public List<string> ActionsOnApply;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
}

public class Buildings {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a tile which the player can click to trigger an <c>Action</c> map tile property.</summary>
    [RType]
    public class BuildingActionTile {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
        public string Id;
        /// <summary>The tile position, relative to the building's top-left corner tile.</summary>
        public Point Tile;
        /// <summary>The tokenizable string for the action to perform, excluding the <c>Action</c> prefix. For example, <c>"Dialogue Hi there @!"</c> to show a message box like <c>"Hi there {player name}!"</c>.</summary>
        public string Action;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an input/output inventory that can be accessed from a tile on the building exterior.</summary>
    [RType]
    public class BuildingChest {
        /// <summary>A key for this chest, referenced from the <see cref="F:StardewValley.GameData.Buildings.BuildingData.ItemConversions" /> field. Each chest must have a unique name within one building's chest list (but they don't need to be globally unique).</summary>
        public string Id;
        /// <summary>The inventory type.</summary>
        public BuildingChestType Type;
        /// <summary>The sound to play once when the player clicks the chest.</summary>
        [Optional] public string Sound;
        /// <summary>A tokenizable string to show when the player tries to add an item to the chest when it isn't a supported item.</summary>
        [Optional] public string InvalidItemMessage;
        /// <summary>An extra condition that must be met before <see cref="F:StardewValley.GameData.Buildings.BuildingChest.InvalidItemMessage" /> is shown.</summary>
        [Optional] public string InvalidItemMessageCondition;
        /// <summary>A tokenizable string to show when the player tries to add an item to the chest when they don't have enough in their inventory.</summary>
        [Optional] public string InvalidCountMessage;
        /// <summary>A tokenizable string to show when the player tries to add an item to the chest when the chest has no more room to accept it.</summary>
        [Optional] public string ChestFullMessage;
        /// <summary>The chest's position on the building exterior, measured in tiles from the top-left corner of the building. This affects the position of the 'item ready to collect' bubble. If omitted, the bubble is disabled.</summary>
        [Optional] public Vector2 DisplayTile = new(-1f, -1f);
        /// <summary>If <see cref="F:StardewValley.GameData.Buildings.BuildingChest.DisplayTile" /> is set, the chest's tile height.</summary>
        [Optional] public float DisplayHeight;
    }
    /// <summary>The inventory type for a building chest.</summary>
    [RType]
    public enum BuildingChestType {
        /// <summary>A normal chest which can both provide output and accept input.</summary>
        Chest,
        /// <summary>Provides items for the player to collect. Clicking the tile will do nothing (if empty), grab the item directly (if it only contains one item), else show a grab-only inventory UI.</summary>
        Collect,
        /// <summary>Lets the player add items for the building to process.</summary>
        Load,
    }
    /// <summary>The data for a building which can be constructed by players.</summary>
    [RType]
    public class BuildingData {
        /// <summary>A tokenizable string for the display name (e.g. shown in the construction menu).</summary>
        public string Name;
        /// <summary>If set, a tokenizable string for the display name which represents the general building type, like 'Coop' for a Deluxe Coop. If omitted, this defaults to the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Name" />.</summary>
        [Optional] public string NameForGeneralType;
        /// <summary>A tokenizable string for the description (e.g. shown in the construction menu).</summary>
        public string Description;
        /// <summary>The asset name for the texture under the game's <c>Content</c> folder.</summary>
        public string Texture;
        /// <summary>The appearances which can be selected from the construction menu (like stone vs plank cabins), if any, in addition to the default appearance based on <see cref="F:StardewValley.GameData.Buildings.BuildingData.Texture" />.</summary>
        [Optional] public List<BuildingSkin> Skins = [];
        /// <summary>Whether to draw an automatic shadow along the bottom edge of the building's sprite.</summary>
        [Optional] public bool DrawShadow = true;
        /// <summary>The tile position relative to the top-left corner of the building where the upgrade sign will be placed when Robin is building an upgrade. Defaults to approximately (5, 1) if the building interior type is a shed, else (0, 0).</summary>
        [Optional] public Vector2 UpgradeSignTile = new(-1f, -1f);
        /// <summary>The pixel height of the upgrade sign when Robin is building an upgrade.</summary>
        [Optional] public float UpgradeSignHeight;
        /// <summary>The building's width and height when constructed, measured in tiles.</summary>
        [Optional] public Point Size = new(1, 1);
        /// <summary>Whether the building should become semi-transparent when the player is behind it.</summary>
        [Optional] public bool FadeWhenBehind = true;
        /// <summary>If set, the building's pixel area within the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Texture" />. Defaults to the entire texture.</summary>
        [Optional] public Rectangle SourceRect = Rectangle.Empty;
        /// <summary>A pixel offset to apply each season. This is applied to the <see cref="F:StardewValley.GameData.Buildings.BuildingData.SourceRect" /> position by multiplying the offset by 0 (spring), 1 (summer), 2 (fall), or 3 (winter). Default 0, so all seasons use the same source rect.</summary>
        [Optional] public Point SeasonOffset = Point.Empty;
        /// <summary>A pixel offset applied to the building sprite's placement in the world.</summary>
        [Optional] public Vector2 DrawOffset = Vector2.Zero;
        /// <summary>A Y tile offset applied when figuring out render layering. For example, a value of 2.5 will treat the building as if it was 2.5 tiles further up the screen for the purposes of layering.</summary>
        [Optional] public float SortTileOffset;
        /// <summary>
        ///   If set, an ASCII text block which indicates which of the building's tiles the players can walk onto, where each character can be <c>X</c> (blocked) or <c>O</c> (passable). Defaults to all tiles blocked. For example, a stable covers a 4x2 tile area with the front two tiles passable:
        ///   <code>
        ///     XXXX
        ///     XOOX
        ///   </code>
        /// </summary>
        [Optional] public string CollisionMap;
        /// <summary>The extra tiles to treat as part of the building when placing it through a construction menu, if any. For example, the farmhouse uses this to make sure the stairs are clear.</summary>
        [Optional] public List<BuildingPlacementTile> AdditionalPlacementTiles;
        /// <summary>If set, the full name of the C# type to instantiate for the building instance. Defaults to a generic <c>StardewValley.Building</c> instance. Note that using a non-vanilla building type will cause a crash when trying to write the building to the save file.</summary>
        [Optional] public string BuildingType;
        /// <summary>The NPC from whom you can request construction. The vanilla values are <c>Robin</c> and <c>Wizard</c>, but you can specify a different name if a mod opens a construction menu for them. Defaults to <c>Robin</c>. If omitted, it won't appear in any menu.</summary>
        [Optional] public string Builder = "Robin";
        /// <summary>If set, a game state query which indicates whether the building should be available in the construction menu. Defaults to always available.</summary>
        [Optional] public string BuildCondition;
        /// <summary>The number of days needed to complete construction (e.g. 1 for a building completed the next day). If set to 0, construction finishes instantly.</summary>
        [Optional] public int BuildDays;
        /// <summary>The gold cost to construct the building.</summary>
        [Optional] public int BuildCost;
        /// <summary>The materials you must provide to start construction.</summary>
        [Optional] public List<BuildingMaterial> BuildMaterials;
        /// <summary>The ID of the building for which this is an upgrade, or omit to allow constructing it as a new building. For example, the Big Coop sets this to "Coop". Any numbers of buildings can be an upgrade for the same building, in which case the player can choose one upgrade path.</summary>
        [Optional] public string BuildingToUpgrade;
        /// <summary>Whether the building is magical. This changes the carpenter menu to a mystic theme while this building's blueprint is selected, and completes the construction instantly when placed.</summary>
        [Optional] public bool MagicalConstruction;
        /// <summary>A pixel offset to apply to the building sprite when drawn in the construction menu.</summary>
        [Optional] public Point BuildMenuDrawOffset = Point.Empty;
        /// <summary>The position of the door that can be clicked to warp into the building interior. This is measured in tiles relative to the top-left corner tile. Defaults to disabled.</summary>
        [Optional] public Point HumanDoor = new(-1, -1);
        /// <summary>If set, the position and size of the door that animals use to enter/exit the building, if the building interior is an animal location. This is measured in tiles relative to the top-left corner tile. Defaults to disabled.</summary>
        [Optional] public Rectangle AnimalDoor = new(-1, -1, 0, 0);
        /// <summary>The duration of the open animation for the <see cref="F:StardewValley.GameData.Buildings.BuildingData.AnimalDoor" />, measured in milliseconds. If omitted, the door switches to the open state instantly.</summary>
        [Optional] public float AnimalDoorOpenDuration;
        /// <summary>If set, the sound which is played once each time the animal door is opened. Disabled by default.</summary>
        [Optional] public string AnimalDoorOpenSound;
        /// <summary>The duration of the close animation for the <see cref="F:StardewValley.GameData.Buildings.BuildingData.AnimalDoor" />, measured in milliseconds. If omitted, the door switches to the closed state instantly.</summary>
        [Optional] public float AnimalDoorCloseDuration;
        /// <summary>If set, the sound which is played once each time the animal door is closed. Disabled by default.</summary>
        [Optional] public string AnimalDoorCloseSound;
        /// <summary>If set, the name of the existing global location to treat as the building's interior, like <c>FarmHouse</c> and <c>Greenhouse</c> for their respective buildings. If omitted, each building will have its own location instance.</summary>
        /// <remarks>
        ///   <para>Each location can only be used by one building. If the location is already in use (e.g. because the player has two of this building), each subsequent building will use the <see cref="F:StardewValley.GameData.Buildings.BuildingData.IndoorMap" /> and <see cref="F:StardewValley.GameData.Buildings.BuildingData.IndoorMapType" /> instead. For example, the first greenhouse will use the global <c>Greenhouse</c> location, and any subsequent greenhouse will use a separate instanced location.</para>
        ///   <para>The non-instanced location must already be in <c>Game1.locations</c>.</para>
        /// </remarks>
        [Optional] public string NonInstancedIndoorLocation;
        /// <summary>The name of the map asset under <c>Content/Maps</c> to load for the building interior (like <c>"Shed"</c> for the <c>Content/Maps/Shed</c> map).</summary>
        [Optional] public string IndoorMap;
        /// <summary>If set, the full name of the C# <c>GameLocation</c> subclass which will manage the building's interior location. This must be one of the vanilla types to avoid a crash when saving. Defaults to a generic <c>GameLocation</c>.</summary>
        [Optional] public string IndoorMapType;
        /// <summary>The maximum number of animals who can live in this building.</summary>
        [Optional] public int MaxOccupants = 20;
        /// <summary>A list of building IDs whose animals to allow in this building too. For example, <c>[ "Barn", "Coop" ]</c> will allow barn and coop animals in this building. Default none.</summary>
        [Optional] public List<string> ValidOccupantTypes = [];
        /// <summary>Whether animals can get pregnant and produce offspring in this building.</summary>
        [Optional] public bool AllowAnimalPregnancy;
        /// <summary>When applied as an upgrade to an existing building, the placed items in its interior to move when transitioning to the new map.</summary>
        [Optional] public List<IndoorItemMove> IndoorItemMoves;
        /// <summary>The items to place in the building interior when it's constructed or upgraded.</summary>
        [Optional] public List<IndoorItemAdd> IndoorItems;
        /// <summary>A list of mail IDs to send to all players when the building is constructed for the first time.</summary>
        [Optional] public List<string> AddMailOnBuild;
        /// <summary>A list of custom properties applied to the building, which can optionally be overridden per-skin in the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Skins" /> field.</summary>
        [Optional] public Dictionary<string, string> Metadata = [];
        /// <summary>A lookup of arbitrary <c>modData</c> values to attach to the building when it's constructed.</summary>
        [Optional] public Dictionary<string, string> ModData = [];
        /// <summary>The amount of hay that can be stored in this building. If built on the farm, this works just like silos and contributes to the farm's available hay.</summary>
        [Optional] public int HayCapacity;
        /// <summary>The input/output inventories that can be accessed from a tile on the building exterior. The allowed items are defined by the <see cref="F:StardewValley.GameData.Buildings.BuildingData.ItemConversions" /> field.</summary>
        [Optional] public List<BuildingChest> Chests;
        /// <summary>The default tile action if the clicked tile isn't in <see cref="F:StardewValley.GameData.Buildings.BuildingData.ActionTiles" />.</summary>
        [Optional] public string DefaultAction;
        /// <summary>The number of extra tiles around the building for which it may add tile properties via <see cref="F:StardewValley.GameData.Buildings.BuildingData.TileProperties" />, but without hiding tile properties from the underlying ground that aren't overwritten by the building data.</summary>
        [Optional] public int AdditionalTilePropertyRadius;
        /// <summary>If true, terrain feature flooring can be placed underneath, and when the building is placed, it will not destroy flooring beneath it.</summary>
        [Optional] public bool AllowsFlooringUnderneath = true;
        /// <summary>A list of tiles which the player can click to trigger an <c>Action</c> map tile property.</summary>
        [Optional] public List<BuildingActionTile> ActionTiles = [];
        /// <summary>The map tile properties to set.</summary>
        [Optional] public List<BuildingTileProperty> TileProperties = [];
        /// <summary>The output items produced when an input item is converted.</summary>
        [Optional] public List<BuildingItemConversion> ItemConversions;
        /// <summary>A list of textures to draw over or behind the building, with support for conditions and animations.</summary>
        [Optional] public List<BuildingDrawLayer> DrawLayers;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
        /// <summary>A cached representation of <see cref="F:StardewValley.GameData.Buildings.BuildingData.ActionTiles" />.</summary>
        Dictionary<Point, string> _actionTiles;
        /// <summary>A cached representation of <see cref="F:StardewValley.GameData.Buildings.BuildingData.CollisionMap" />.</summary>
        Dictionary<Point, bool> _collisionMap;
        /// <summary>A cached representation of <see cref="F:StardewValley.GameData.Buildings.BuildingData.TileProperties" />.</summary>
        Dictionary<string, Dictionary<Point, Dictionary<string, string>>> _tileProperties;
        /// <summary>Get whether a tile is passable based on the <see cref="F:StardewValley.GameData.Buildings.BuildingData.CollisionMap" />.</summary>
        /// <param name="relativeX">The tile X position relative to the top-left corner of the building.</param>
        /// <param name="relativeY">The tile Y position relative to the top-left corner of the building.</param>
        public bool IsTilePassable(int relativeX, int relativeY) {
            if (CollisionMap == null) return relativeX < 0 || relativeX >= Size.X || relativeY < 0 || relativeY >= Size.Y;
            var key = new Point(relativeX, relativeY);
            if (_collisionMap == null) {
                _collisionMap = [];
                if (CollisionMap != null) {
                    var strArray = CollisionMap.Trim().Split('\n');
                    for (var y = 0; y < strArray.Length; ++y) {
                        var str = strArray[y].Trim();
                        for (var index = 0; index < str.Length; ++index)
                            _collisionMap[new Point(index, y)] = str[index] == 'X';
                    }
                }
            }
            return !_collisionMap.TryGetValue(key, out var flag) || !flag;
        }
        /// <summary>Get the action to add at a given position based on <see cref="F:StardewValley.GameData.Buildings.BuildingData.ActionTiles" />.</summary>
        /// <param name="relativeX">The tile X position relative to the top-left corner of the building.</param>
        /// <param name="relativeY">The tile Y position relative to the top-left corner of the building.</param>
        public string GetActionAtTile(int relativeX, int relativeY) {
            var key = new Point(relativeX, relativeY);
            if (_actionTiles == null) {
                _actionTiles = [];
                foreach (var actionTile in ActionTiles) _actionTiles[actionTile.Tile] = actionTile.Action;
            }
            if (!_actionTiles.TryGetValue(key, out var defaultAction)) {
                if (relativeX < 0 || relativeX >= Size.X || relativeY < 0 || relativeY >= Size.Y) return null;
                defaultAction = DefaultAction;
            }
            return defaultAction;
        }
        /// <summary>Get whether a tile property should be added based on <see cref="F:StardewValley.GameData.Buildings.BuildingData.TileProperties" />.</summary>
        /// <param name="relativeX">The tile X position relative to the top-left corner of the building's bounding box.</param>
        /// <param name="relativeY">The tile Y position relative to the top-left corner of the building's bounding box.</param>
        /// <param name="propertyName">The property name to check.</param>
        /// <param name="layerName">The layer name to check.</param>
        /// <param name="propertyValue">The property value that should be set.</param>
        public bool HasPropertyAtTile(int relativeX, int relativeY, string propertyName, string layerName, ref string propertyValue) {
            if (_tileProperties == null) {
                _tileProperties = [];
                foreach (var tileProperty in TileProperties) {
                    if (!_tileProperties.TryGetValue(tileProperty.Layer, out var dictionary1)) _tileProperties[tileProperty.Layer] = dictionary1 = [];
                    for (var y = tileProperty.TileArea.Y; y < tileProperty.TileArea.Bottom; ++y)
                        for (var x = tileProperty.TileArea.X; x < tileProperty.TileArea.Right; ++x) {
                            var key = new Point(x, y);
                            if (!dictionary1.TryGetValue(key, out var dictionary2)) dictionary1[key] = dictionary2 = [];
                            dictionary2[tileProperty.Name] = tileProperty.Value;
                        }
                }
            }
            if (!_tileProperties.TryGetValue(layerName, out var dictionary3) || !dictionary3.TryGetValue(new Point(relativeX, relativeY), out var dictionary4) || !dictionary4.TryGetValue(propertyName, out var str)) return false;
            propertyValue = str;
            return true;
        }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a texture to draw over or behind the building.</summary>
    [RType]
    public class BuildingDrawLayer {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_DrawLayerId</c>.</summary>
        public string Id;
        /// <summary>The asset name of the texture to draw. Defaults to the building's <see cref="F:StardewValley.GameData.Buildings.BuildingData.Texture" /> field.</summary>
        [Optional] public string Texture;
        /// <summary>The pixel area within the texture to draw. If the overlay is animated via <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" />, this is the area of the first frame.</summary>
        public Rectangle SourceRect = Rectangle.Empty;
        /// <summary>The tile position at which to draw the top-left corner of the texture, relative to the building's top-left corner tile.</summary>
        public Vector2 DrawPosition;
        /// <summary>Whether to draw the texture behind the building sprite (i.e. underlay) instead of over it.</summary>
        [Optional] public bool DrawInBackground;
        /// <summary>A Y tile offset applied when figuring out render layering. For example, a value of 2.5 will treat the texture as if it was 2.5 tiles further up the screen for the purposes of layering.</summary>
        [Optional] public float SortTileOffset;
        /// <summary>The name of a chest defined in the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Chests" /> field which must contain items. If it's empty, this overlay won't be rendered. Default none.</summary>
        [Optional] public string OnlyDrawIfChestHasContents;
        /// <summary>The number of milliseconds each animation frame is displayed on-screen before switching to the next, if <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" /> is more than one.</summary>
        [Optional] public int FrameDuration = 90;
        /// <summary>The number of animation frames to render. If this is more than one, the building will be animated automatically based on <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow" /> and <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameDuration" />.</summary>
        [Optional] public int FrameCount = 1;
        /// <summary>The number of animation frames per row in the spritesheet.</summary>
        /// <remarks>
        ///   For each frame, the <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect" /> will be offset by its width to the right up to <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow" /> - 1 times, and then down by its height.
        ///   For example, if you set <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" /> to 6 and <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow" /> to 3, the building will expect the frames to be laid out like this in the spritesheet (where frame 1 matches <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect" />):
        ///   <code>
        ///     1 2 3
        ///     4 5 6
        ///   </code>
        /// </remarks>
        [Optional] public int FramesPerRow = -1;
        /// <summary>A pixel offset applied to the draw layer when the animal door is open. While the door is opening, the percentage open is applied to the offset (e.g. 50% open = 50% offset).</summary>
        [Optional] public Point AnimalDoorOffset = Point.Empty;
        /// <summary>Get the parsed <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect" /> adjusted for the current game time, accounting for <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" />.</summary>
        /// <param name="time">The total milliseconds elapsed since the game started.</param>
        public Rectangle GetSourceRect(int time) {
            var sourceRect = SourceRect;
            time /= FrameDuration;
            time %= FrameCount;
            if (FramesPerRow < 0) sourceRect.X += sourceRect.Width * time;
            else { sourceRect.X += sourceRect.Width * (time % FramesPerRow); sourceRect.Y += sourceRect.Height * (time / FramesPerRow); }
            return sourceRect;
        }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an output item produced when an input item is converted.</summary>
    [RType]
    public class BuildingItemConversion {
        /// <summary>A unique identifier for this entry. This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.</summary>
        public string Id;
        /// <summary>A list of context tags to match against an input item. An item must have all of these tags to be accepted.</summary>
        public List<string> RequiredTags;
        /// <summary>The number of the input item to consume.</summary>
        [Optional] public int RequiredCount = 1;
        /// <summary>The maximum number of the input item which can be processed each day. Each conversion rule has its own separate maximum (e.g. if you have two rules each with a max of 1, then you can convert one of each daily). Set to -1 to allow unlimited conversions.</summary>
        [Optional] public int MaxDailyConversions = 1;
        /// <summary>The name of the inventory defined in <see cref="F:StardewValley.GameData.Buildings.BuildingData.Chests" /> from which to take input items.</summary>
        public string SourceChest;
        /// <summary>The name of the inventory defined in <see cref="F:StardewValley.GameData.Buildings.BuildingData.Chests" /> in which to store output items.</summary>
        public string DestinationChest;
        /// <summary>The output items produced when an input item is converted.</summary>
        public List<GenericSpawnItemDataWithCondition> ProducedItems;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, the materials needed to construct a building.</summary>
    [RType]
    public class BuildingMaterial {
        /// <summary>A key which uniquely identifies the building material.</summary>
        [Ignore] public string Id => ItemId;
        /// <summary>The required item ID (qualified or unqualified).</summary>
        public string ItemId;
        /// <summary>The number of the item required.</summary>
        public int Amount;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a tile to treat as part of the building when placing it through a construction menu.</summary>
    [RType]
    public class BuildingPlacementTile {
        /// <summary>The tile positions relative to the top-left corner of the building.</summary>
        public Rectangle TileArea;
        /// <summary>Whether this area allows tiles that would normally not be buildable, so long as they are passable. For example, this is used to ensure that an entrance is accessible.</summary>
        [Optional] public bool OnlyNeedsToBePassable;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an appearance which can be selected from the construction menu (like stone vs plank cabins).</summary>
    [RType]
    public class BuildingSkin {
        /// <summary>A key which uniquely identifies the skin. The ID should only contain alphanumeric/underscore/dot characters. For custom skins, it should be prefixed with your mod ID like <c>Example.ModId_SkinName</c>.</summary>
        public string Id;
        /// <summary>A tokenizable string for the skin's translated name.</summary>
        [Optional] public string Name;
        /// <summary>If set, a tokenizable string for the skin's display name which represents the general building type, like 'Coop' for a Deluxe Coop. If omitted, this defaults to the <see cref="F:StardewValley.GameData.Buildings.BuildingSkin.Name" />.</summary>
        [Optional] public string NameForGeneralType;
        /// <summary>A tokenizable string for the skin's translated description.</summary>
        [Optional] public string Description;
        /// <summary>The asset name for the texture under the game's <c>Content</c> folder.</summary>
        public string Texture;
        /// <summary>If set, a game state query which indicates whether the skin should be available to apply. Defaults to always available.</summary>
        [Optional] public string Condition;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.Buildings.BuildingData.BuildDays" />.</summary>
        [Optional] public int? BuildDays;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.Buildings.BuildingData.BuildCost" />.</summary>
        [Optional] public int? BuildCost;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.Buildings.BuildingData.BuildMaterials" />.</summary>
        [Optional] public List<BuildingMaterial> BuildMaterials;
        /// <summary>Whether this skin should be shown as a separate building option in the construction menu.</summary>
        [Optional] public bool ShowAsSeparateConstructionEntry;
        /// <summary>Equivalent to the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Metadata" /> field on the building. Properties defined in this field are added to the building's metadata when this skin is active, overwriting the previous property with the same name if applicable.</summary>
        [Optional] public Dictionary<string, string> Metadata = [];
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a map tile property to set.</summary>
    [RType]
    public class BuildingTileProperty {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
        public string Id;
        /// <summary>The tile property name to set.</summary>
        public string Name;
        /// <summary>The tile property value to set.</summary>
        [Optional] public string Value;
        /// <summary>The name of the map layer whose tiles to change.</summary>
        public string Layer;
        /// <summary>The tiles to which to add the property.</summary>
        public Rectangle TileArea;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an item to place in the building interior when it's constructed or upgraded.</summary>
    [RType]
    public class IndoorItemAdd {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
        public string Id;
        /// <summary>The qualified item ID for the item to place.</summary>
        public string ItemId;
        /// <summary>The tile position at which to place the item.</summary>
        public Point Tile;
        /// <summary>Whether to prevent the player from destroying, picking up, or moving the item.</summary>
        [Optional] public bool Indestructible;
        /// <summary>Whether to remove any item on the target tile, except for another instance of <see cref="F:StardewValley.GameData.Buildings.IndoorItemAdd.ItemId" />. The previous contents of the tile will be moved into the lost and found if applicable.</summary>
        [Optional] public bool ClearTile = true;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a placed item in its interior to move when transitioning to an upgraded map.</summary>
    [RType]
    public class IndoorItemMove {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
        public string Id;
        /// <summary>The tile position on which any item will be moved.</summary>
        [Optional] public Point Source;
        /// <summary>The tile position to which to move the item.</summary>
        [Optional] public Point Destination;
        /// <summary>The tile size of the area to move. If this is multiple tiles, the <see cref="F:StardewValley.GameData.Buildings.IndoorItemMove.Source" /> and <see cref="F:StardewValley.GameData.Buildings.IndoorItemMove.Destination" /> specify the top-left coordinate of the area.</summary>
        [Optional] public Point Size = new(1, 1);
        /// <summary>If set, an item on this spot won't be moved if its item ID matches this one.</summary>
        [Optional] public string UnlessItemId;
    }
}

public class Bundles {
    [RType]
    public class BundleData {
        /// <summary>A unique ID for this entry.</summary>
        [Ignore] public string Id => Name;
        public string Name;
        public int Index;
        public string Sprite;
        public string Color;
        public string Items;
        [Optional] public int Pick = -1;
        [Optional] public int RequiredItems = -1;
        public string Reward;
    }
    [RType]
    public class BundleSetData {
        /// <summary>A unique ID for this entry.</summary>
        public string Id;
        public List<BundleData> Bundles = [];
    }
    [RType]
    public class RandomBundleData {
        /// <summary>A unique ID for this entry.</summary>
        [Ignore] public string Id => AreaName;
        public string AreaName;
        public string Keys;
        [Optional] public List<BundleSetData> BundleSets = [];
        [Optional] public List<BundleData> Bundles = [];
    }
}

public class Characters {
    /// <summary>How an NPC's birthday is shown on the calendar.</summary>
    [RType]
    public enum CalendarBehavior {
        /// <summary>They always appear on the calendar.</summary>
        AlwaysShown,
        /// <summary>Until the player meets them, they don't appear on the calendar.</summary>
        HiddenUntilMet,
        /// <summary>They never appear on the calendar.</summary>
        HiddenAlways,
    }
    /// <summary>How an NPC appears in the end-game perfection slide show.</summary>
    [RType]
    public enum EndSlideShowBehavior {
        /// <summary>The NPC doesn't appear in the slide show.</summary>
        Hidden,
        /// <summary>The NPC is added to the main group of NPCs which walk across the screen.</summary>
        MainGroup,
        /// <summary>The NPC is added to the trailing group of NPCs which follow the main group.</summary>
        TrailingGroup,
    }
    /// <summary>The general age of an NPC.</summary>
    [RType]
    public enum NpcAge {
        Adult,
        Teen,
        Child,
    }
    /// <summary>The language spoken by an NPC.</summary>
    [RType]
    public enum NpcLanguage {
        /// <summary>The default language understood by the player.</summary>
        Default,
        /// <summary>The Dwarvish language, which the player can only understand after finding the Dwarvish Translation Guide.</summary>
        Dwarvish,
    }
    /// <summary>A measure of a character's general politeness.</summary>
    [RType]
    public enum NpcManner {
        Neutral,
        Polite,
        Rude,
    }
    /// <summary>A measure of a character's overall optimism.</summary>
    [RType]
    public enum NpcOptimism {
        Positive,
        Negative,
        Neutral,
    }
    /// <summary>A measure of a character's comfort with social situations.</summary>
    [RType]
    public enum NpcSocialAnxiety {
        Outgoing,
        Shy,
        Neutral,
    }
    /// <summary>How an NPC is shown on the social tab when unlocked.</summary>
    [RType]
    public enum SocialTabBehavior {
        /// <summary>Until the player meets them, their name on the social tab is replaced with "???".</summary>
        UnknownUntilMet,
        /// <summary>They always appear on the social tab (including their name).</summary>
        AlwaysShown,
        /// <summary>Until the player meets them, they don't appear on the social tab.</summary>
        HiddenUntilMet,
        /// <summary>They never appear on the social tab.</summary>
        HiddenAlways,
    }
    [RType]
    public class CharacterAppearanceData {
        /// <summary>An ID for this entry within the appearance list. This only needs to be unique within the current list.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether this entry applies. Default true.</summary>
        [Optional] public string Condition;
        /// <summary>The season when this appearance applies, or <c>null</c> for any season.</summary>
        [Optional] public Season? Season;
        /// <summary>Whether the appearance can be used when the NPC is indoors.</summary>
        [Optional] public bool Indoors = true;
        /// <summary>Whether the appearance can be used when the NPC is outdoors.</summary>
        [Optional] public bool Outdoors = true;
        /// <summary>The asset name for the portrait texture, or null for the default portrait.</summary>
        [Optional] public string Portrait;
        /// <summary>The asset name for the sprite texture, or null for the default sprite.</summary>
        [Optional] public string Sprite;
        /// <summary>Whether this is island beach attire worn at the resort.</summary>
        /// <remarks>This is mutually exclusive: NPCs will never wear it in other contexts if it's true, and will never wear it as island attire if it's false.</remarks>
        [Optional] public bool IsIslandAttire;
        /// <summary>The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.</summary>
        [Optional] public int Precedence;
        /// <summary>If multiple entries with the same <see cref="F:StardewValley.GameData.Characters.CharacterAppearanceData.Precedence" /> apply, the relative weight to use when randomly choosing one.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.Appearance" />.</remarks>
        [Optional] public int Weight = 1;
    }
    /// <summary>The content data for an NPC.</summary>
    [RType]
    public class CharacterData {
        /// <summary>A tokenizable string for the NPC's display name.</summary>
        public string DisplayName;
        /// <summary>The season when the NPC was born.</summary>
        [Optional] public Season? BirthSeason;
        /// <summary>The day when the NPC was born.</summary>
        [Optional] public int BirthDay;
        /// <summary>The region of the world in which the NPC lives (one of <c>Desert</c>, <c>Town</c>, or <c>Other</c>).</summary>
        /// <remarks>For example, only <c>Town</c> NPCs are counted for the introductions quest, can be selected as a secret santa for the Feast of the Winter Star, or get a friendship boost from the Luau.</remarks>
        [Optional] public string HomeRegion = "Other";
        /// <summary>The language spoken by the NPC.</summary>
        [Optional] public NpcLanguage Language;
        /// <summary>The character's gender identity.</summary>
        [Optional] public Gender Gender = Gender.Undefined;
        /// <summary>The general age of the NPC.</summary>
        /// <remarks>This affects generated dialogue lines (e.g. a child might say "stupid" and an adult might say "depressing"), generic dialogue (e.g. a child might respond to dumpster diving with "Eww... What are you doing?" and a teen would say "Um... Why are you digging in the trash?"), and the gift they choose as Feast of the Winter Star gift-giver. Children are also excluded from item delivery quests.</remarks>
        [Optional] public NpcAge Age;
        /// <summary>A measure of the character's general politeness.</summary>
        /// <remarks>This affects some generic dialogue lines.</remarks>
        [Optional] public NpcManner Manner;
        /// <summary>A measure of the character's comfort with social situations.</summary>
        /// <remarks>This affects some generic dialogue lines.</remarks>
        [Optional] public NpcSocialAnxiety SocialAnxiety = NpcSocialAnxiety.Neutral;
        /// <summary>A measure of the character's overall optimism.</summary>
        [Optional] public NpcOptimism Optimism = NpcOptimism.Neutral;
        /// <summary>Whether the NPC has dark skin, which affects the chance of children with the player having dark skin too.</summary>
        [Optional] public bool IsDarkSkinned;
        /// <summary>Whether players can date and marry this NPC.</summary>
        [Optional] public bool CanBeRomanced;
        /// <summary>Unused.</summary>
        [Optional] public string LoveInterest;
        /// <summary>How the NPC's birthday is shown on the calendar.</summary>
        [Optional] public CalendarBehavior Calendar;
        /// <summary>How the NPC is shown on the social tab.</summary>
        [Optional] public SocialTabBehavior SocialTab;
        /// <summary>A game state query which indicates whether to enable social features (like birthdays, gift giving, friendship, and an entry in the social tab). Defaults to true (except for monsters, horses, pets, and Junimos).</summary>
        [Optional] public string CanSocialize;
        /// <summary>Whether players can give gifts to this NPC. Default true.</summary>
        /// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> and have an entry in <c>Data/NPCGiftTastes</c> to receive gifts, regardless of this value.</remarks>
        [Optional] public bool CanReceiveGifts = true;
        /// <summary>Whether this NPC can show a speech bubble greeting nearby players or NPCs, and or be greeted by other NPCs. Default true.</summary>
        [Optional] public bool CanGreetNearbyCharacters = true;
        /// <summary>Whether this NPC can comment on items that a player sold to a shop which then resold it to them, or <c>null</c> to allow it if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
        /// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> to allow it, regardless of this value.</remarks>
        [Optional] public bool? CanCommentOnPurchasedShopItems;
        /// <summary>A game state query which indicates whether the NPC can visit Ginger Island once the resort is unlocked.</summary>
        [Optional] public string CanVisitIsland;
        /// <summary>Whether to include this NPC in the introductions quest, or <c>null</c> to include them if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
        [Optional] public bool? IntroductionsQuest;
        /// <summary>A game state query which indicates whether this NPC can give item delivery quests, or <c>null</c> to allow it if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
        /// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> to be included, regardless of this value.</remarks>
        [Optional] public string ItemDeliveryQuests;
        /// <summary>Whether to include this NPC when checking whether the player has max friendships with every NPC for the perfection score.</summary>
        /// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> to be counted, regardless of this value.</remarks>
        [Optional] public bool PerfectionScore = true;
        /// <summary>How the NPC appears in the end-game perfection slide show.</summary>
        [Optional] public EndSlideShowBehavior EndSlideShow = EndSlideShowBehavior.MainGroup;
        /// <summary>A game state query which indicates whether the player will need to adopt children with this spouse, instead of either the player or NPC giving birth. If null, defaults to true for same-gender and false for opposite-gender spouses.</summary>
        [Optional] public string SpouseAdopts;
        /// <summary>A game state query which indicates whether the spouse will ask to have children. Defaults to true.</summary>
        [Optional] public string SpouseWantsChildren;
        /// <summary>A game state query which indicates whether the spouse will get jealous when the player gifts items to another NPC of the same gender when it's not their birthday. Defaults to true.</summary>
        [Optional] public string SpouseGiftJealousy;
        /// <summary>The friendship change when <see cref="F:StardewValley.GameData.Characters.CharacterData.SpouseGiftJealousy" /> applies.</summary>
        [Optional] public int SpouseGiftJealousyFriendshipChange = -30;
        /// <summary>The NPC's spouse room in the farmhouse when the player marries them.</summary>
        [Optional] public CharacterSpouseRoomData SpouseRoom;
        /// <summary>The NPC's patio area on the farm when the player marries them, if any.</summary>
        [Optional] public CharacterSpousePatioData SpousePatio;
        /// <summary>The floor IDs which the NPC might randomly apply to the farmhouse when married, or an empty list to choose from the vanilla floors.</summary>
        [Optional] public List<string> SpouseFloors = [];
        /// <summary>The wallpaper IDs which the NPC might randomly apply to the farmhouse when married, or an empty list to choose from the vanilla wallpapers.</summary>
        [Optional] public List<string> SpouseWallpapers = [];
        /// <summary>The friendship point change if this NPC sees a player rummaging through trash.</summary>
        [Optional] public int DumpsterDiveFriendshipEffect = -25;
        /// <summary>The emote ID to show above the NPC's head when they see a player rummaging through trash.</summary>
        [Optional] public int? DumpsterDiveEmote;
        /// <summary>The NPC's closest friends and family, where the key is the NPC name and the value is an optional tokenizable string for the name to use in dialogue text (like 'mom').</summary>
        /// <remarks>This affects generic dialogue for revealing likes and dislikes to family members, and may affect <c>inlaw_{NPC}</c> dialogues. This isn't necessarily comprehensive.</remarks>
        [Optional] public Dictionary<string, string> FriendsAndFamily = [];
        /// <summary>Whether the NPC can be asked to dance at the Flower Dance festival. This can be true (can be asked even if not romanceable), false (can never ask), or null (true if romanceable).</summary>
        [Optional] public bool? FlowerDanceCanDance;
        /// <summary>At the Winter Star festival, the possible gifts this NPC can give to players.</summary>
        /// <remarks>If this doesn't return a match, a generic gift is selected based on <see cref="F:StardewValley.GameData.Characters.CharacterData.Age" />.</remarks>
        [Optional] public List<GenericSpawnItemDataWithCondition> WinterStarGifts = [];
        /// <summary>A game state query which indicates whether this NPC can give and receive gifts at the Feast of the Winter Star, or <c>null</c> to allow it if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
        [Optional] public string WinterStarParticipant;
        /// <summary>A game state query which indicates whether the NPC should be added to the world, checked when loading a save and when ending each day. This only affects whether the NPC is added when missing; returning false won't remove an NPC that's already been added.</summary>
        [Optional] public string UnlockConditions;
        /// <summary>Whether to add this NPC to the world automatically when they're missing and the <see cref="F:StardewValley.GameData.Characters.CharacterData.UnlockConditions" /> match.</summary>
        [Optional] public bool SpawnIfMissing = true;
        /// <summary>The possible locations for the NPC's default map. The first matching entry is used.</summary>
        [Optional] public List<CharacterHomeData> Home;
        /// <summary>The <strong>last segment</strong> of the NPC's portrait and sprite asset names when not set via <see cref="F:StardewValley.GameData.Characters.CharacterData.Appearance" />. For example, set to <c>"Abigail"</c> to use <c>Portraits/Abigail</c> and <c>Characters/Abigail</c> respectively. Defaults to the internal NPC name.</summary>
        [Optional] public string TextureName;
        /// <summary>The sprite and portrait texture to use, if set.</summary>
        /// <remarks>
        ///   <para>The appearances are sorted by <see cref="F:StardewValley.GameData.Characters.CharacterAppearanceData.Precedence" />, then filtered to those whose fields match. If multiple matching appearances have the highest precedence, one entry is randomly chosen based on their relative weight. This randomization is stable per day, so the NPC always makes the same choice until the next day.</para>
        ///   <para>If a portrait/sprite can't be loaded (or no appearances match), the NPC will use the default asset based on <see cref="F:StardewValley.GameData.Characters.CharacterData.TextureName" />.</para>
        /// </remarks>
        [Optional] public List<CharacterAppearanceData> Appearance = [];
        /// <summary>The pixel area in the character's sprite texture to show as their mug shot in contexts like the calendar or social menu, or <c>null</c> for the first sprite in the spritesheet.</summary>
        /// <remarks>This should be approximately 16x24 pixels for best results.</remarks>
        [Optional] public Rectangle? MugShotSourceRect;
        /// <summary>The pixel size of the individual sprites in their world sprite spritesheet.</summary>
        [Optional] public Point Size = new(16, 32);
        /// <summary>Whether the chest on the NPC's world sprite puffs in and out as they breathe.</summary>
        [Optional] public bool Breather = true;
        /// <summary>The pixel area within the spritesheet which expands and contracts to simulate breathing, relative to the top-left corner of the source rectangle for their current sprite, or <c>null</c> to calculate it automatically.</summary>
        [Optional] public Rectangle? BreathChestRect;
        /// <summary>The pixel offset to apply to the NPC's <see cref="F:StardewValley.GameData.Characters.CharacterData.BreathChestPosition" /> when drawn over the NPC, or <c>null</c> for the default offset.</summary>
        [Optional] public Point? BreathChestPosition;
        /// <summary>The shadow to draw, or <c>null</c> to apply the default options.</summary>
        [Optional] public CharacterShadowData Shadow;
        /// <summary>A pixel offset to apply to the character's default emote position.</summary>
        [Optional] public Point EmoteOffset = Point.Empty;
        /// <summary>The portrait indexes which should shake when displayed.</summary>
        [Optional] public List<int> ShakePortraits = [];
        /// <summary>The sprite index within the <see cref="F:StardewValley.GameData.Characters.CharacterData.TextureName" /> to use when kissing a player.</summary>
        [Optional] public int KissSpriteIndex = 28;
        /// <summary>Whether the character is facing right (true) or left (false) in their <see cref="F:StardewValley.GameData.Characters.CharacterData.KissSpriteIndex" />. The sprite will be flipped as needed to face the player.</summary>
        [Optional] public bool KissSpriteFacingRight = true;
        /// <summary>For the hidden gift log emote, the cue ID for the sound played when clicking the sprite. Defaults to <c>drumkit6</c>.</summary>
        /// <remarks>The hidden gift log emote happens when clicking on a character's sprite in the profile menu after earning enough hearts.</remarks>
        [Optional] public string HiddenProfileEmoteSound;
        /// <summary>For the hidden gift log emote, how long the animation plays measured in milliseconds. Defaults to 4000 (4 seconds).</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
        [Optional] public int HiddenProfileEmoteDuration = -1;
        /// <summary>For the hidden gift log emote, the index within the NPC's world sprite spritesheet at which the animation starts. If omitted for a vanilla NPC, the game plays a default animation specific to that NPC; if omitted for a custom NPC, the game just shows them walking while facing down.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
        [Optional] public int HiddenProfileEmoteStartFrame = -1;
        /// <summary>For the hidden gift log emote, the number of frames in the animation. The first frame corresponds to <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame" />, and each subsequent frame will use the next sprite in the spritesheet. This has no effect if <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame" /> isn't set.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
        [Optional] public int HiddenProfileEmoteFrameCount = 1;
        /// <summary>For the hidden gift log emote, how long each animation frame is shown on-screen before switching to the next one, measured in milliseconds. This has no effect if <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame" /> isn't set.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
        [Optional] public float HiddenProfileEmoteFrameDuration = 200f;
        /// <summary>The former NPC names which may appear in save data.</summary>
        /// <remarks>If a NPC in save data has a name which (a) matches one of these values and (b) doesn't match the name of a loaded NPC, its data will be loaded into this NPC instead. If that happens, this will also update other references like friendship and spouse data.</remarks>
        [Optional] public List<string> FormerCharacterNames = [];
        /// <summary>The NPC's index in the <c>Maps/characterSheet</c> tilesheet, if applicable. This is used for placing vanilla NPCs in festivals from the map; custom NPCs should use the <c>{layer}_additionalCharacters</c> field in the festival data instead.</summary>
        [Optional] public int FestivalVanillaActorIndex = -1;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, a possible location for the NPC's default map.</summary>
    [RType]
    public class CharacterHomeData {
        /// <summary>An ID for this entry within the home list. This only needs to be unique within the current list.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether this entry applies. Default true.</summary>
        [Optional] public string Condition;
        /// <summary>The internal name for the home location where this NPC spawns and returns each day.</summary>
        public string Location;
        /// <summary>The tile position within the home location where this NPC spawns and returns each day.</summary>
        public Point Tile = Point.Empty;
        /// <summary>The default direction the NPC faces when they start each day. The possible values are <c>down</c>, <c>left</c>, <c>right</c>, and <c>up</c>.</summary>
        [Optional] public string Direction = "up";
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, configures how the NPC's shadow should be rendered.</summary>
    [RType]
    public class CharacterShadowData {
        /// <summary>Whether the shadow should be drawn.</summary>
        [Optional] public bool Visible = true;
        /// <summary>A pixel offset applied to the shadow position.</summary>
        [Optional] public Point Offset = Point.Empty;
        /// <summary>The scale at which to draw the shadow.</summary>
        /// <remarks>This is a multiplier applied to the default shadow scale, which can change based on factors like whether the NPC is jumping. For example, <c>0.5</c> means half the size it'd be drawn if you didn't specify a scale.</remarks>
        [Optional] public float Scale = 1f;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, the data about the NPC's patio area on the farm when the player marries them.</summary>
    [RType]
    public class CharacterSpousePatioData {
        /// <summary>The default value for <see cref="F:StardewValley.GameData.Characters.CharacterSpousePatioData.MapSourceRect" />.</summary>
        public static readonly Rectangle DefaultMapSourceRect = new(0, 0, 4, 4);
        /// <summary>The asset name within the content <c>Maps</c> folder which contains the patio. Defaults to <c>spousePatios</c>.</summary>
        [Optional] public string MapAsset;
        /// <summary>The tile area within the <see cref="F:StardewValley.GameData.Characters.CharacterSpousePatioData.MapAsset" /> containing the spouse's patio. This must be a 4x4 tile area or smaller.</summary>
        [Optional] public Rectangle MapSourceRect = CharacterSpousePatioData.DefaultMapSourceRect;
        /// <summary>The spouse's animation frames when they're in the patio. Each frame is a tuple containing the [0] frame index and [1] optional duration in milliseconds (default 100). If omitted or empty, the NPC won't be animated.</summary>
        [Optional] public List<int[]> SpriteAnimationFrames;
        /// <summary>The pixel offset to apply to the NPC's sprite when they're animated in the patio.</summary>
        [Optional] public Point SpriteAnimationPixelOffset;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, the data about the NPC's spouse room in the farmhouse when the player marries them.</summary>
    [RType]
    public class CharacterSpouseRoomData {
        /// <summary>The default value for <see cref="F:StardewValley.GameData.Characters.CharacterSpouseRoomData.MapSourceRect" />.</summary>
        public static readonly Rectangle DefaultMapSourceRect = new(0, 0, 6, 9);
        /// <summary>The asset name within the content <c>Maps</c> folder which contains the spouse room. Defaults to <c>spouseRooms</c>.</summary>
        [Optional] public string MapAsset;
        /// <summary>The tile area within the <see cref="F:StardewValley.GameData.Characters.CharacterSpouseRoomData.MapAsset" /> containing the spouse's room.</summary>
        [Optional] public Rectangle MapSourceRect = CharacterSpouseRoomData.DefaultMapSourceRect;
    }
}

public class Crafting {
    /// <summary>A clothing item that can be tailored from ingredients using Emily's sewing machine.</summary>
    [RType]
    public class TailorItemRecipe {
        /// <summary>The backing field for <see cref="P:StardewValley.GameData.Crafting.TailorItemRecipe.Id" />.</summary>
        string _idImpl;
        /// <summary>The context tags for the first item required, or <c>null</c> for none required. If this lists multiple context tags, an item must match all of them.</summary>
        [Optional] public List<string> FirstItemTags;
        /// <summary>The context tags for the second item required, or <c>null</c> for none required. If this lists multiple context tags, an item must match all of them.</summary>
        [Optional] public List<string> SecondItemTags;
        /// <summary>Whether tailoring the item destroys the item matched by <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.SecondItemTags" />.</summary>
        [Optional] public bool SpendRightItem = true;
        /// <summary>The item ID to produce by default.</summary>
        /// <remarks>Ignored if <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIds" /> has any values, or for female players if <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIdFeminine" /> is set.</remarks>
        [Optional] public string CraftedItemId;
        /// <summary>The item IDs to produce by default.</summary>
        /// <remarks>Ignored for female players if <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIdFeminine" /> is set.</remarks>
        [Optional] public List<string> CraftedItemIds;
        /// <summary>If set, the item ID to produce if the player is female.</summary>
        [Optional] public string CraftedItemIdFeminine;
        /// <summary>A unique identifier for this entry. This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.</summary>
        [Optional]
        public string Id {
            get {
                if (_idImpl != null) return _idImpl;
                return (CraftedItemIds != null ? (CraftedItemIds.Any() ? 1 : 0) : 0) != 0 ? string.Join(",", CraftedItemIds) : CraftedItemId;
            }
            set => _idImpl = value;
        }
    }
}

public class Crops {
    /// <summary>The metadata for a crop that can be planted.</summary>
    [RType]
    public class CropData {
        /// <summary>The seasons in which this crop can grow.</summary>
        public List<Season> Seasons = [];
        /// <summary>The number of days in each visual step of growth before the crop is harvestable.</summary>
        public List<int> DaysInPhase = [];
        /// <summary>The number of days before the crop regrows after harvesting, or -1 if it can't regrow.</summary>
        [Optional] public int RegrowDays = -1;
        /// <summary>Whether this is a raised crop on a trellis that can't be walked through.</summary>
        [Optional] public bool IsRaised;
        /// <summary>Whether this crop can be planted near water for a unique paddy dirt texture, faster growth time, and auto-watering.</summary>
        [Optional] public bool IsPaddyCrop;
        /// <summary>Whether this crop needs to be watered to grow.</summary>
        [Optional] public bool NeedsWatering = true;
        /// <summary>The rules which override which locations the crop can be planted in, if applicable. These don't override more specific checks (e.g. crops needing to be planted in dirt).</summary>
        [Optional] public List<PlantableRule> PlantableLocationRules;
        /// <summary>The unqualified item ID produced when this crop is harvested.</summary>
        [Optional] public string HarvestItemId;
        /// <summary>The minimum number of <see cref="F:StardewValley.GameData.Crops.CropData.HarvestItemId" /> to harvest.</summary>
        [Optional] public int HarvestMinStack = 1;
        /// <summary>The maximum number of <see cref="F:StardewValley.GameData.Crops.CropData.HarvestItemId" /> to harvest, before <see cref="F:StardewValley.GameData.Crops.CropData.ExtraHarvestChance" /> and <see cref="F:StardewValley.GameData.Crops.CropData.HarvestMaxIncreasePerFarmingLevel" /> are applied.</summary>
        [Optional] public int HarvestMaxStack = 1;
        /// <summary>The number of extra harvests to produce per farming level. This is rounded down to the nearest integer and added to <see cref="F:StardewValley.GameData.Crops.CropData.HarvestMaxStack" />.</summary>
        [Optional] public float HarvestMaxIncreasePerFarmingLevel;
        /// <summary>The probability that harvesting the crop will produce extra harvest items, as a value between 0 (never) and 0.9 (nearly always). This is repeatedly rolled until it fails, then the number of successful rolls is added to the produced count.</summary>
        [Optional] public double ExtraHarvestChance;
        /// <summary>How the crop can be harvested.</summary>
        [Optional] public HarvestMethod HarvestMethod;
        /// <summary>If set, the minimum quality of the harvest crop.</summary>
        /// <remarks>These fields set a constraint that's applied after the quality is calculated normally, they don't affect the initial quality logic.</remarks>
        [Optional] public int HarvestMinQuality;
        /// <summary>If set, the maximum quality of the harvest crop.</summary>
        /// <inheritdoc cref="F:StardewValley.GameData.Crops.CropData.HarvestMinQuality" path="/remarks" />
        [Optional] public int? HarvestMaxQuality;
        /// <summary>The tint colors that can be applied to the crop sprite, if any. If multiple colors are listed, one is chosen at random for each crop. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).</summary>
        [Optional] public List<string> TintColors = [];
        /// <summary>The asset name for the crop texture under the game's <c>Content</c> folder.</summary>
        public string Texture;
        /// <summary>The index of this crop in the <see cref="F:StardewValley.GameData.Crops.CropData.Texture" /> (one crop per row).</summary>
        public int SpriteIndex;
        /// <summary>Whether the player can ship 300 of this crop's harvest item to unlock the monoculture achievement.</summary>
        public bool CountForMonoculture;
        /// <summary>Whether the player must ship 15 of this crop's harvest item to unlock the polyculture achievement.</summary>
        public bool CountForPolyculture;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
        /// <summary>Get the <see cref="F:StardewValley.GameData.Crops.CropData.Texture" /> if different from the default name.</summary>
        /// <param name="defaultName">The default asset name.</param>
        public string GetCustomTextureName(string defaultName) => string.IsNullOrWhiteSpace(Texture) || !(Texture != defaultName) ? null : Texture;
    }
    /// <summary>Indicates how a crop can be harvested.</summary>
    [RType]
    public enum HarvestMethod {
        /// <summary>The crop is harvested by hand.</summary>
        Grab,
        /// <summary>The crop is harvested using a scythe.</summary>
        Scythe,
    }
}

public class FarmAnimals {
    /// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, a possible variant for a farm animal.</summary>
    [RType]
    public class AlternatePurchaseAnimals {
        /// <summary>A unique string ID for this entry within the current animal's list.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether this variant entry is available. Default always enabled.</summary>
        [Optional] public string Condition;
        /// <summary>A list of animal IDs to spawn instead of the main ID field. If multiple are listed, one is chosen at random on purchase.</summary>
        public List<string> AnimalIds;
    }
    /// <summary>The metadata for a farm animal which can be bought from Marnie's ranch.</summary>
    [RType]
    public class FarmAnimalData {
        /// <summary>A tokenizable string for the animal type's display name.</summary>
        [Optional] public string DisplayName;
        /// <summary>The ID for the main building type that houses this animal. The animal will also be placeable in buildings whose <see cref="F:StardewValley.GameData.Buildings.BuildingData.ValidOccupantTypes" /> field contains this value.</summary>
        [Optional] public string House;
        /// <summary>The default gender for the animal type. This only affects the text shown after purchasing the animal.</summary>
        [Optional] public FarmAnimalGender Gender;
        /// <summary>Half the cost to purchase the animal (the actual price is double this value), or a negative value to disable purchasing this animal type. Default -1.</summary>
        [Optional] public int PurchasePrice = -1;
        /// <summary>The price when the player sells the animal, before it's adjusted for the animal's friendship towards the player.</summary>
        /// <remarks>The actual sell price will be this value multiplied by a number between 0.3 (zero friendship) and 1.3 (max friendship).</remarks>
        [Optional] public int SellPrice;
        /// <summary>The asset name for the icon texture to show in shops.</summary>
        [Optional] public string ShopTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopTexture" /> to draw. This should be 32 pixels wide and 16 high. Ignored if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopTexture" /> isn't set.</summary>
        [Optional] public Rectangle ShopSourceRect;
        /// <summary>A tokenizable string for the display name shown in the shop menu. Defaults to the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DisplayName" /> field.</summary>
        [Optional] public string ShopDisplayName;
        /// <summary>A tokenizable string for the tooltip description shown in the shop menu. Defaults to none.</summary>
        [Optional] public string ShopDescription;
        /// <summary>A tokenizable string which overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopDescription" /> if the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.RequiredBuilding" /> isn't built. Defaults to none.</summary>
        [Optional] public string ShopMissingBuildingDescription;
        /// <summary>The building that needs to be built on the farm for this animal to be available to purchase. Buildings that are upgraded from this building are valid too. Default none.</summary>
        [Optional] public string RequiredBuilding;
        /// <summary>A game state query which indicates whether the farm animal is available in the shop menu. Default always unlocked.</summary>
        [Optional] public string UnlockCondition;
        /// <summary>The possible variants for this farm animal (e.g. chickens can be Brown Chicken, Blue Chicken, or White Chicken). When the animal is purchased, of the available variants is chosen at random.</summary>
        [Optional] public List<AlternatePurchaseAnimals> AlternatePurchaseTypes;
        /// <summary>A list of the object IDs that can be placed in the incubator or ostrich incubator to hatch this animal. If <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.House" /> doesn't match the current building, the entry will be ignored. Default none.</summary>
        [Optional] public List<string> EggItemIds;
        /// <summary>How long eggs incubate before they hatch, in in-game minutes. Defaults to 9000 minutes.</summary>
        [Optional] public int IncubationTime = -1;
        /// <summary>An offset applied to the incubator's sprite index when it's holding an egg for this animal.</summary>
        [Optional] public int IncubatorParentSheetOffset = 1;
        /// <summary>A tokenizable string for the message shown when entering the building after the egg hatched. Defaults to the text "???".</summary>
        [Optional] public string BirthText;
        /// <summary>The number of days until a freshly purchased/born animal becomes an adult and begins producing items.</summary>
        [Optional] public int DaysToMature = 1;
        /// <summary>Whether an animal can produce a child (regardless of gender).</summary>
        [Optional] public bool CanGetPregnant;
        /// <summary>The number of days between item productions. For example, setting 1 will produce an item every other day.</summary>
        [Optional] public int DaysToProduce = 1;
        /// <summary>How produced items are collected from the animal.</summary>
        [Optional] public FarmAnimalHarvestType HarvestType;
        /// <summary>The tool name with which produced items can be collected from the animal, if the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestType" /> is set to <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalHarvestType.HarvestWithTool" />. The values recognized by the vanilla tools are <c>Milk Pail</c> and <c>Shears</c>. Default none.</summary>
        [Optional] public string HarvestTool;
        /// <summary>The items produced by the animal when it's an adult, if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceMinimumFriendship" /> does not match.</summary>
        [Optional] public List<FarmAnimalProduce> ProduceItemIds = [];
        /// <summary>The items produced by the animal when it's an adult, if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceMinimumFriendship" /> matches.</summary>
        [Optional] public List<FarmAnimalProduce> DeluxeProduceItemIds = [];
        /// <summary>Whether an item is produced on the day the animal becomes an adult (like sheep).</summary>
        [Optional] public bool ProduceOnMature;
        /// <summary>The minimum friendship points needed to reduce the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DaysToProduce" /> by one. Defaults to no reduction.</summary>
        [Optional] public int FriendshipForFasterProduce = -1;
        /// <summary>The minimum friendship points needed to produce the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds" />.</summary>
        [Optional] public int DeluxeProduceMinimumFriendship = 200;
        /// <summary>A divisor which reduces the probability of producing <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds" />. Lower values produce deluxe items more often.</summary>
        /// <remarks>
        ///   This is applied using this formula:
        ///   <code>
        ///     if happiness &gt; 200: happiness_modifier = happiness * 1.5
        ///     else if happiness &gt; 100: happiness_modifier = 0
        ///     else happiness_modifier = happiness - 100
        ///     ((friendship + happiness_modifier) / DeluxeProduceCareDivisor) + (daily_luck * DeluxeProduceLuckMultiplier)
        ///   </code>
        ///   For example, given a friendship of 102 and happiness of 150, the probability with the default field values will be <c>((102 + 0) / 1200) + (daily_luck * 0) = (102 / 1200) = 0.085</c> or 8.5%.
        /// </remarks>
        [Optional] public float DeluxeProduceCareDivisor = 1200f;
        /// <summary>A multiplier which increases the bonus from daily luck on the probability of producing <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds" />.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceCareDivisor" />.</remarks>
        [Optional] public float DeluxeProduceLuckMultiplier;
        /// <summary>Whether players can feed this animal a golden cracker to double its normal output.</summary>
        [Optional] public bool CanEatGoldenCrackers = true;
        /// <summary>The internal ID of a profession which makes it easier to befriend this animal. Defaults to none.</summary>
        [Optional] public int ProfessionForHappinessBoost = -1;
        /// <summary>The internal ID of a profession which increases the chance of higher-quality produce.</summary>
        [Optional] public int ProfessionForQualityBoost = -1;
        /// <summary>The internal ID of a profession which reduces the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DaysToProduce" /> by one. Defaults to none.</summary>
        [Optional] public int ProfessionForFasterProduce = -1;
        /// <summary>The audio cue ID for the sound produced by the animal (e.g. when pet). Default none.</summary>
        [Optional] public string Sound;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Sound" /> when the animal is a baby. Has no effect if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Sound" /> isn't set.</summary>
        [Optional] public string BabySound;
        /// <summary>If set, the asset name for the animal's spritesheet. Defaults to <c>Animals/{ID}</c>, like Animals/Goat for a goat.</summary>
        [Optional] public string Texture;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" /> when the animal doesn't currently have an item ready to collect (like the sheep's sheared sprite).</summary>
        [Optional] public string HarvestedTexture;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" /> and <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestedTexture" /> when the animal is a baby.</summary>
        [Optional] public string BabyTexture;
        /// <summary>When the animal is facing left, whether to use a flipped version of their right-facing sprite.</summary>
        [Optional] public bool UseFlippedRightForLeft;
        /// <summary>The pixel width of the animal's sprite (before in-game pixel zoom is applied).</summary>
        [Optional] public int SpriteWidth = 16;
        /// <summary>The pixel height of the animal's sprite (before in-game pixel zoom is applied).</summary>
        [Optional] public int SpriteHeight = 16;
        /// <summary>Whether the animal has two frames for the randomized 'unique' animation instead of one.</summary>
        /// <remarks>
        ///   <para>If false, the unique sprite frames are indexes 13 (down), 14 (right), 12 (left if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.UseFlippedRightForLeft" /> is false), and 15 (up).</para>
        ///   <para>If true, the unique sprite frames are indexes 16 (down), 18 (right), 22 (left), and 20 (up).</para>
        /// </remarks>
        [Optional] public bool UseDoubleUniqueAnimationFrames;
        /// <summary>The sprite index to display when sleeping.</summary>
        [Optional] public int SleepFrame = 12;
        /// <summary>A pixel offset to apply to emotes drawn over the farm animal.</summary>
        [Optional] public Point EmoteOffset = Point.Empty;
        /// <summary>A pixel offset to apply to the farm animal's sprite while it's swimming.</summary>
        [Optional] public Point SwimOffset = new(0, 112);
        /// <summary>The possible alternate appearances, if any. A skin is chosen at random when the animal is purchased or hatched based on the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalSkin.Weight" /> field. The default appearance (e.g. using <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" />) is automatically an available skin with a weight of 1.</summary>
        [Optional] public List<FarmAnimalSkin> Skins;
        /// <summary>The shadow to draw when a baby animal is swimming, or <c>null</c> to apply <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShadowWhenBaby" />.</summary>
        [Optional] public FarmAnimalShadowData ShadowWhenBabySwims;
        /// <summary>The shadow to draw for a baby animal, or <c>null</c> to apply <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Shadow" />.</summary>
        [Optional] public FarmAnimalShadowData ShadowWhenBaby;
        /// <summary>The shadow to draw when an adult animal is swimming, or <c>null</c> to apply <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShadowWhenAdult" />.</summary>
        [Optional] public FarmAnimalShadowData ShadowWhenAdultSwims;
        /// <summary>The shadow to draw for an adult animal, or <c>null</c> to apply <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Shadow" />.</summary>
        [Optional] public FarmAnimalShadowData ShadowWhenAdult;
        /// <summary>The shadow to draw if a more specific shadow field doesn't apply, or <c>null</c> to apply the default options.</summary>
        [Optional] public FarmAnimalShadowData Shadow;
        /// <summary>Whether animals on the farm can swim in water once they've been pet. Default false.</summary>
        [Optional] public bool CanSwim;
        /// <summary>Whether baby animals can follow nearby adults. This only applies for animals whose <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.House" /> field is <c>Coop</c>. Default false.</summary>
        [Optional] public bool BabiesFollowAdults;
        /// <summary>The amount of grass eaten by this animal each day.</summary>
        [Optional] public int GrassEatAmount = 2;
        /// <summary>An amount which affects the daily reduction in happiness if the animal wasn't pet, or didn't have a heater in winter.</summary>
        [Optional] public int HappinessDrain;
        /// <summary>The animal sprite's tile size in the world when the player is clicking to pet them, if the animal is facing up or down. This can be a fractional value like 1.75.</summary>
        [Optional] public Vector2 UpDownPetHitboxTileSize = new(1f, 1f);
        /// <summary>The animal sprite's tile size in the world when the player is clicking to pet them, if the animal is facing left or right. This can be a fractional value like 1.75.</summary>
        [Optional] public Vector2 LeftRightPetHitboxTileSize = new(1f, 1f);
        /// <summary>Overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.UpDownPetHitboxTileSize" /> when the animal is a baby.</summary>
        [Optional] public Vector2 BabyUpDownPetHitboxTileSize = new(1f, 1f);
        /// <summary>Overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.LeftRightPetHitboxTileSize" /> when the animal is a baby.</summary>
        [Optional] public Vector2 BabyLeftRightPetHitboxTileSize = new(1f, 1f);
        /// <summary>The game stat counters to increment when the animal produces an item, if any.</summary>
        [Optional] public List<StatIncrement> StatToIncrementOnProduce;
        /// <summary>Whether to show the farm animal in the credit scene on the summit after the player achieves perfection.</summary>
        [Optional] public bool ShowInSummitCredits;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
        /// <summary>Get the options to apply when drawing the animal's shadow, if any.</summary>
        /// <param name="isBaby">Whether the animal is a baby.</param>
        /// <param name="isSwimming">Whether the animal is swimming.</param>
        public FarmAnimalShadowData GetShadow(bool isBaby, bool isSwimming) => isBaby ? (!isSwimming ? ShadowWhenBaby ?? Shadow : ShadowWhenBabySwims ?? ShadowWhenBaby ?? Shadow) : (!isSwimming ? ShadowWhenAdult ?? Shadow : ShadowWhenAdultSwims ?? ShadowWhenAdult ?? Shadow);
    }
    /// <summary>The default gender for a farm animal type.</summary>
    [RType]
    public enum FarmAnimalGender {
        /// <summary>The farm animal is always female.</summary>
        Female,
        /// <summary>The farm animal is always male.</summary>
        Male,
        /// <summary>The gender of each animal is randomized when it's purchased.</summary>
        MaleOrFemale,
    }
    /// <summary>How produced items are collected from an animal.</summary>
    [RType]
    public enum FarmAnimalHarvestType {
        /// <summary>The item is placed on the ground in the animal's home building overnight.</summary>
        DropOvernight,
        /// <summary>The item is collected from the animal directly based on the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestTool" /> field.</summary>
        HarvestWithTool,
        /// <summary>The farm animal digs it up with an animation like pigs finding truffles.</summary>
        DigUp,
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, an item that can be produced by the animal when it's an adult.</summary>
    [RType]
    public class FarmAnimalProduce {
        /// <summary>An ID for this entry within the produce list. This only needs to be unique within the current list.</summary>
        [Optional] public string Id;
        /// <summary>A game state query which indicates whether this item can be produced now. Defaults to always true.</summary>
        [Optional] public string Condition;
        /// <summary>The minimum friendship points with the animal needed to produce this item.</summary>
        [Optional] public int MinimumFriendship;
        /// <summary>The <strong>unqualified</strong> object ID of the item to produce.</summary>
        public string ItemId;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, configures how the animal's shadow should be rendered.</summary>
    [RType]
    public class FarmAnimalShadowData {
        /// <summary>Whether the shadow should be drawn.</summary>
        [Optional] public bool Visible = true;
        /// <summary>A pixel offset applied to the shadow position.</summary>
        [Optional] public Point? Offset;
        /// <summary>The scale at which to draw the shadow, or <c>null</c> to apply the default logic.</summary>
        [Optional] public float? Scale;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, an alternate appearance for a farm animal.</summary>
    [RType]
    public class FarmAnimalSkin {
        /// <summary>A key which uniquely identifies the skin for this animal type. The ID should only contain alphanumeric/underscore/dot characters. For custom skins, this should be prefixed with your mod ID like <c>Example.ModId_SkinName</c>.</summary>
        public string Id;
        /// <summary>A multiplier for the probability to choose this skin when an animal is purchased. For example, <c>2</c> will double the chance this skin is selected relative to skins with the default <c>1</c>.</summary>
        [Optional] public float Weight = 1f;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" />.</summary>
        [Optional] public string Texture;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestedTexture" />.</summary>
        [Optional] public string HarvestedTexture;
        /// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.BabyTexture" />.</summary>
        [Optional] public string BabyTexture;
    }
}

public class Fences {
    /// <summary>The metadata for a placeable fence item.</summary>
    [RType]
    public class FenceData {
        /// <summary>The initial health points for a fence when it's first placed, which affects how quickly it degrades. A fence loses 1/1440 points per in-game minute (roughly 0.04 points per hour or 0.5 points for a 12-hour day).</summary>
        public int Health;
        /// <summary>The minimum amount added to the health when a fence is repaired by a player.</summary>
        /// <remarks>Repairing a fence sets its health to <c>2 � (<see cref="F:StardewValley.GameData.Fences.FenceData.Health" /> + Random(<see cref="F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMinimum" />, <see cref="F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMaximum" />))</c>.</remarks>
        [Optional] public float RepairHealthAdjustmentMinimum;
        /// <summary>The maximum amount added to the health when a fence is repaired by a player.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMinimum" />.</remarks>
        [Optional] public float RepairHealthAdjustmentMaximum;
        /// <summary>The asset name for the texture when the fence is placed. For example, the vanilla fences use individual tilesheets like <c>LooseSprites\Fence1</c> (wood fence).</summary>
        public string Texture;
        /// <summary>The audio cue ID played when the fence is placed or repairs (e.g. axe used by Wood Fence).</summary>
        public string PlacementSound;
        /// <summary>The audio cue ID played when the fence is broken or picked up by the player. Defaults to <see cref="F:StardewValley.GameData.Fences.FenceData.PlacementSound" />.</summary>
        [Optional] public string RemovalSound;
        /// <summary>A list of tool IDs which can be used to break the fence, matching the keys in the <c>Data\Tools</c> asset.</summary>
        /// <remarks>A tool must match <see cref="F:StardewValley.GameData.Fences.FenceData.RemovalToolIds" /> <strong>or</strong> <see cref="F:StardewValley.GameData.Fences.FenceData.RemovalToolTypes" /> to be a valid removal tool. If both lists are null or empty, all tools can remove the fence.</remarks>
        [Optional] public List<string> RemovalToolIds = [];
        /// <summary>A list of tool class full names which can be used to break the fence, like <c>StardewValley.Tools.Axe</c>.</summary>
        /// <inheritdoc cref="F:StardewValley.GameData.Fences.FenceData.RemovalToolIds" path="/remarks" />
        [Optional] public List<string> RemovalToolTypes = [];
        /// <summary>The type of cosmetic debris particles to 'splash' from the tile when the fence is broken. The defined values are <c>0</c> (copper), <c>2</c> (iron), <c>4</c> (coal), <c>6</c> (gold), <c>8</c> (coins), <c>10</c> (iridium), <c>12</c> (wood), <c>14</c> (stone), <c>32</c> (big stone), and <c>34</c> (big wood). Default <c>14</c> (stone).</summary>
        [Optional] public int RemovalDebrisType = 14;
        /// <summary>When an item like a torch is placed on the fence, the pixel offset to apply to its draw position.</summary>
        [Optional] public Vector2 HeldObjectDrawOffset = new(0.0f, -20f);
        /// <summary>The X pixel offset to apply when the fence is oriented horizontally, with only one connected fence on the right. This fully replaces the X value specified by <see cref="F:StardewValley.GameData.Fences.FenceData.HeldObjectDrawOffset" /> when it's applied.</summary>
        [Optional] public float LeftEndHeldObjectDrawX = -1f;
        /// <summary>Equivalent to <see cref="F:StardewValley.GameData.Fences.FenceData.LeftEndHeldObjectDrawX" />, but when there's only one connected fence on the left.</summary>
        [Optional] public float RightEndHeldObjectDrawX;
    }
}

public class FishPonds {
    /// <summary>The fish data for a Fish Pond building.</summary>
    [RType]
    public class FishPondData {
        /// <summary>A unique identifier for the entry. The ID should only contain alphanumeric/underscore/dot characters. For custom fish pond entries, this should be prefixed with your mod ID like <c>Example.ModId_Fish.</c></summary>
        public string Id;
        /// <summary>The context tags for the fish item to configure. If this lists multiple context tags, an item must match all of them. If an item matches multiple entries, the first entry which matches is used.</summary>
        public List<string> RequiredTags;
        /// <summary>The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.</summary>
        [Optional] public int Precedence;
        /// <summary>The maximum number of fish which can be added to this pond.</summary>
        /// <remarks>This cannot exceed the global maximum of 10.</remarks>
        [Optional] public int MaxPopulation = -1;
        /// <summary>The number of days needed to raise the population by one if there's enough room in the fish pond, or <c>-1</c> to choose a number automatically based on the fish value.</summary>
        [Optional] public int SpawnTime = -1;
        /// <summary>The minimum daily chance that this fish pond checks for output on a given day, as a value between 0 (never) and 1 (always).</summary>
        /// <remarks>The actual probability is lerped between <see cref="F:StardewValley.GameData.FishPonds.FishPondData.BaseMinProduceChance" /> and <see cref="F:StardewValley.GameData.FishPonds.FishPondData.BaseMaxProduceChance" /> based on the fish pond's population. If the min chance is 95+%, it's treated as the actual probability without lerping. If this check passes, output is only produced if one of the <see cref="F:StardewValley.GameData.FishPonds.FishPondData.ProducedItems" /> passes its checks too.</remarks>
        [Optional] public float BaseMinProduceChance = 0.15f;
        /// <summary>The maximum daily chance that this fish pond checks for output on a given day, as a value between 0 (never) and 1 (always).</summary>
        /// <inheritdoc cref="F:StardewValley.GameData.FishPonds.FishPondData.BaseMinProduceChance" path="/remarks" />
        [Optional] public float BaseMaxProduceChance = 0.95f;
        /// <summary>The custom water color to set, if applicable.</summary>
        [Optional] public List<FishPondWaterColor> WaterColor;
        /// <summary>The items that can be produced by the fish pond. When a fish pond is ready to produce output, it will check each entry in the list and take the first one that matches. If no entry matches, no output is produced.</summary>
        [Optional] public List<FishPondReward> ProducedItems;
        /// <summary>The rules which determine when the fish pond population can grow, and the quests that must be completed to do so.</summary>
        [Optional] public Dictionary<int, List<string>> PopulationGates;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.FishPonds.FishPondData" />, an item that can be produced by the fish pond.</summary>
    [RType]
    public class FishPondReward : GameData.GenericSpawnItemDataWithCondition {
        /// <summary>The minimum population needed before this output becomes available.</summary>
        [Optional] public int RequiredPopulation;
        /// <summary>The percentage chance that this output is selected, as a value between 0 (never) and 1 (always). If multiple items pass, only the first one will be produced.</summary>
        [Optional] public float Chance = 1f;
        /// <summary>The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.</summary>
        [Optional] public int Precedence;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.FishPonds.FishPondData" />, a color to apply to the water if its fields match.</summary>
    [RType]
    public class FishPondWaterColor {
        public string Id;
        /// <summary>A tint color to apply to the water. This can be <c>CopyFromInput</c> (to use the input item's color), a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
        public string Color;
        /// <summary>The minimum population before this color applies.</summary>
        [Optional] public int MinPopulation = 1;
        /// <summary>The minimum population gate that was unlocked, or 0 for any value.</summary>
        [Optional] public int MinUnlockedPopulationGate;
        /// <summary>A game state query which indicates whether this color should be applied. Defaults to always added.</summary>
        [Optional] public string Condition;
    }
}

public class FloorsAndPaths {
    /// <summary>When drawing adjacent flooring items across multiple tiles, how the flooring sprite for each tile is selected.</summary>
    [RType]
    public enum FloorPathConnectType {
        /// <summary>For normal floors, intended to cover large square areas. This uses some logic to draw inner corners.</summary>
        Default,
        /// <summary>For floors intended to be drawn as narrow paths. These are drawn without any consideration for inner corners.</summary>
        Path,
        /// <summary>For floors that have a decorative corner. Use <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.CornerSize" /> to change the size of this corner.</summary>
        CornerDecorated,
        /// <summary>For floors that don't connect. When placed, one of the tiles is randomly selected.</summary>
        Random,
    }
    /// <summary>The metadata for a craftable floor or path item.</summary>
    [RType]
    public class FloorPathData {
        /// <summary>A key which uniquely identifies this floor/path. The ID should only contain alphanumeric/underscore/dot characters. For vanilla floors and paths, this matches the spritesheet index in the <c>TerrainFeatures/Flooring</c> spritesheet; for custom floors and paths, this should be prefixed with your mod ID like <c>Example.ModId_FloorName.</c></summary>
        public string Id;
        /// <summary>The unqualified item ID for the corresponding object-type item.</summary>
        public string ItemId;
        /// <summary>The asset name for the texture when the item is placed.</summary>
        public string Texture;
        /// <summary>The top-left pixel position for the sprite within the <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture" /> spritesheet.</summary>
        public Point Corner;
        /// <summary>Equivalent to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture" />, but applied if the current location is in winter. Defaults to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture" />.</summary>
        public string WinterTexture;
        /// <summary>Equivalent to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Corner" />, but used if <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.WinterTexture" /> is applied. Defaults to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Corner" />.</summary>
        public Point WinterCorner;
        /// <summary>The audio cue ID played when the item is placed (e.g. <c>axchop</c> used by Wood Floor).</summary>
        public string PlacementSound;
        /// <summary>The audio cue ID played when the item is picked up. Defaults to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.PlacementSound" />.</summary>
        [Optional] public string RemovalSound;
        /// <summary>The type of cosmetic debris particles to 'splash' from the tile when the item is picked up. The defined values are <c>0</c> (copper), <c>2</c> (iron), <c>4</c> (coal), <c>6</c> (gold), <c>8</c> (coins), <c>10</c> (iridium), <c>12</c> (wood), <c>14</c> (stone), <c>32</c> (big stone), and <c>34</c> (big wood). Default <c>14</c> (stone).</summary>
        [Optional] public int RemovalDebrisType = 14;
        /// <summary>The audio cue ID played when the player steps on the tile (e.g. <c>woodyStep</c> used by Wood Floor).</summary>
        public string FootstepSound;
        /// <summary>When drawing adjacent flooring items across multiple tiles, how the flooring sprite for each tile is selected.</summary>
        [Optional] public FloorPathConnectType ConnectType;
        /// <summary>The type of shadow to draw under the tile sprite.</summary>
        [Optional] public FloorPathShadowType ShadowType;
        /// <summary>The pixel size of the decorative inner corner when the <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.ConnectType" /> field is set to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathConnectType.CornerDecorated" /> or <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathConnectType.Default" />.</summary>
        [Optional] public int CornerSize = 4;
        /// <summary>The speed boost applied to the player, on the farm only, when they're walking on paths of this type. Negative values are ignored. Set to <c>-1</c> to use the default for vanilla paths.</summary>
        [Optional] public float FarmSpeedBuff = -1f;
    }
    /// <summary>How the shadow under a floor or path tile sprite should be drawn.</summary>
    [RType]
    public enum FloorPathShadowType {
        /// <summary>Don't draw a shadow.</summary>
        None,
        /// <summary>Draw a shadow under the entire tile.</summary>
        Square,
        /// <summary>Draw a shadow that follows the lines of the path sprite.</summary>
        Contoured,
    }
}

public class FruitTrees {
    /// <summary>Metadata for a fruit tree type.</summary>
    [RType]
    public class FruitTreeData {
        /// <summary>The rules which override which locations the tree can be planted in, if applicable. These don't override more specific checks (e.g. not being plantable on stone).</summary>
        [Optional] public List<PlantableRule> PlantableLocationRules;
        /// <summary>A tokenizable string for the fruit tree display name, like 'Cherry' for a cherry tree.</summary>
        /// <remarks>This shouldn't include 'tree', which will be added automatically as needed.</remarks>
        public string DisplayName;
        /// <summary>The seasons in which this tree bears fruit.</summary>
        public List<Season> Seasons;
        /// <summary>The fruit to produce. The first matching entry will be produced.</summary>
        public List<FruitTreeFruitData> Fruit;
        /// <summary>The asset name for the texture for the tree's spritesheet.</summary>
        public string Texture;
        /// <summary>The row index within the <see cref="P:StardewValley.GameData.FruitTrees.FruitTreeData.Texture" /> for the tree's sprites.</summary>
        public int TextureSpriteRow;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.FruitTrees.FruitTreeData" />, a possible item to produce as fruit.</summary>
    [RType]
    public class FruitTreeFruitData : GenericSpawnItemDataWithCondition {
        /// <summary>If set, the specific season when this fruit can be produced. For more complex conditions, see <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" />.</summary>
        [Optional] public Season? Season { get; set; }
        /// <summary>The probability that the item will be produced, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public float Chance { get; set; } = 1f;
    }
}

public class GarbageCans {
    /// <summary>The data for in-game garbage cans.</summary>
    [RType]
    public class GarbageCanData {
        /// <summary>The default probability that any item will be found when searching a garbage can, unless overridden by <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanEntryData.BaseChance" />.</summary>
        public float DefaultBaseChance = 0.2f;
        /// <summary>The items to try before <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.GarbageCans" /> and <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.AfterAll" />, subject to the garbage can's base chance.</summary>
        public List<GarbageCanItemData> BeforeAll;
        /// <summary>The items to try if neither <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.BeforeAll" /> nor <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.GarbageCans" /> returned a value.</summary>
        public List<GarbageCanItemData> AfterAll;
        /// <summary>The metadata for specific garbage can IDs.</summary>
        public Dictionary<string, GarbageCanEntryData> GarbageCans;
    }
    /// <summary>Metadata for a specific in-game garbage can.</summary>
    [RType]
    public class GarbageCanEntryData {
        /// <summary>The probability that any item will be found when the garbage can is searched, or <c>-1</c> to use <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.DefaultBaseChance" />.</summary>
        [Optional] public float BaseChance = -1f;
        /// <summary>The items that may be found by rummaging in the garbage can.</summary>
        public List<GarbageCanItemData> Items;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.GarbageCans.GarbageCanData" />, an item that can be found by rummaging in the garbage can.</summary>
    /// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
    [RType]
    public class GarbageCanItemData : GenericSpawnItemDataWithCondition {
        /// <summary>Whether to check this item even if the <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanEntryData.BaseChance" /> didn't pass.</summary>
        [Optional] public bool IgnoreBaseChance { get; set; }
        /// <summary>Whether to treat this item as a 'mega success' if it's selected, which plays a special <c>crit</c> sound and bigger animation.</summary>
        [Optional] public bool IsMegaSuccess { get; set; }
        /// <summary>Whether to treat this item as an 'double mega success' if it's selected, which plays an explosion sound and dramatic animation.</summary>
        [Optional] public bool IsDoubleMegaSuccess { get; set; }
        /// <summary>Whether to add the item to the player's inventory directly, opening an item grab menu if they don't have room in their inventory. If false, the item will be dropped on the ground next to the garbage can instead.</summary>
        [Optional] public bool AddToInventoryDirectly { get; set; }
        /// <summary>Whether to splits stacks into multiple debris items, instead of a single item with a stack size.</summary>
        [Optional] public bool CreateMultipleDebris { get; set; }
    }
}

public class GiantCrops {
    /// <summary>A custom giant crop that may spawn in-game.</summary>
    [RType]
    public class GiantCropData {
        /// <summary>The qualified or unqualified harvest item ID of the crops from which this giant crop can grow. If multiple giant crops have the same item ID, the first one whose <see cref="F:StardewValley.GameData.GiantCrops.GiantCropData.Chance" /> matches will be used.</summary>
        public string FromItemId;
        /// <summary>The items to produce when this giant crop is broken. All matching items will be produced.</summary>
        public List<GiantCropHarvestItemData> HarvestItems;
        /// <summary>The asset name for the texture containing the giant crop's sprite.</summary>
        public string Texture;
        /// <summary>The top-left pixel position of the sprite within the <see cref="F:StardewValley.GameData.GiantCrops.GiantCropData.Texture" />. Defaults to (0, 0).</summary>
        [Optional] public Point TexturePosition;
        /// <summary>The area in tiles occupied by the giant crop. This affects both its sprite size (which should be 16 pixels per tile) and the grid of crops needed for it to grow. Note that giant crops are drawn with an extra tile's height.</summary>
        [Optional] public Point TileSize = new(3, 3);
        /// <summary>The health points that must be depleted to break the giant crop. The number of points depleted per axe chop depends on the axe power level.</summary>
        [Optional] public int Health = 3;
        /// <summary>The percentage chance a given grid of crops will grow into the giant crop each night, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public float Chance = 0.01f;
        /// <summary>A game state query which indicates whether the giant crop can be selected. Defaults to always enabled.</summary>
        [Optional] public string Condition;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.GiantCrops.GiantCropData" />, a possible item to produce when it's harvested.</summary>
    [RType]
    public class GiantCropHarvestItemData : GameData.GenericSpawnItemDataWithCondition {
        /// <summary>The probability that the item will be produced, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public float Chance { get; set; } = 1f;
        /// <summary>Whether to drop this item only for the Shaving enchantment (true), only when the giant crop is broken (false), or both (null).</summary>
        [Optional] public bool? ForShavingEnchantment { get; set; }
        /// <summary>If set, the minimum stack size when this item is dropped due to the Shaving enchantment, scaled to the tool's power level.</summary>
        /// <remarks>
        ///   <para>This value is multiplied by the health deducted by the tool hit which triggered the enchantment. For example, an iridium tool that reduced the giant crop's health by 3 points will produce three times this value per hit.</para>
        ///   <para>If the scaled min and max are both set, the stack size is randomized between them. If only one is set, it's applied as a limit after the generic fields. If neither is set, the generic fields are applied as usual without scaling.</para>
        /// </remarks>
        [Optional] public int? ScaledMinStackWhenShaving { get; set; } = new int?(2);
        /// <summary>If set, the maximum stack size when this item is dropped due to the Shaving enchantment, scaled to the tool's power level.</summary>
        /// <inheritdoc cref="P:StardewValley.GameData.GiantCrops.GiantCropHarvestItemData.ScaledMinStackWhenShaving" path="/remarks" />
        [Optional] public int? ScaledMaxStackWhenShaving { get; set; } = new int?(2);
    }
}

public class HomeRenovations {
    /// <summary>A renovation which can be applied to customize the player's farmhouse after the second farmhouse upgrade.</summary>
    [RType]
    public class HomeRenovation {
        /// <summary>A translation key in the form <c>{asset name}:{key}</c>. The translation text should contain three slash-delimited fields: the translated display name, translated description, and the action message shown to ask the player which area to renovate.</summary>
        public string TextStrings;
        /// <summary>The animation to play when the renovation is applied. The possible values are <c>destroy</c> or <c>build</c>. Any other value defaults to <c>build</c>.</summary>
        public string AnimationType;
        /// <summary>Whether to prevent the player from applying the renovations if there are any players, NPCs, items, etc within the target area.</summary>
        public bool CheckForObstructions;
        /// <summary>A price to charge for this renovation (default free). Negative values will act as a refund the player (typically used when reverting a renovation).</summary>
        [Optional] public int Price;
        /// <summary>A unique string ID which links this renovation to its counterpart add/remove renovation. Add/remove renovations for the same room should have the same ID.</summary>
        [Optional] public string RoomId;
        /// <summary>The criteria that must match for the renovation to appear as an option.</summary>
        public List<RenovationValue> Requirements;
        /// <summary>The actions to perform after the renovation is applied.</summary>
        public List<RenovationValue> RenovateActions;
        /// <summary>The tile areas within the farmhouse where the renovation can be placed.</summary>
        [Optional] public List<RectGroup> RectGroups;
        /// <summary>A dynamic area to add to the <see cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.RectGroups" /> field, if any. The only supported value is <c>crib</c>, which is the farmhouse area containing the cribs.</summary>
        [Optional] public string SpecialRect;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.HomeRenovations.RectGroup" />, a tile area within the farmhouse.</summary>
    [RType]
    public class Rect {
        /// <summary>The top-left tile X position.</summary>
        public int X;
        /// <summary>The top-left tile Y position.</summary>
        public int Y;
        /// <summary>The area width in tiles.</summary>
        public int Width;
        /// <summary>The area height in tiles.</summary>
        public int Height;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.HomeRenovations.HomeRenovation" />, the farmhouse areas where a renovation can be applied.</summary>
    [RType]
    public class RectGroup {
        /// <summary>The tile areas within the farmhouse where the renovation can be applied.</summary>
        public List<Rect> Rects;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.HomeRenovations.HomeRenovation" />, a renovation requirement or action.</summary>
    [RType]
    public class RenovationValue {
        /// <summary>The requirement or action type. This can be <c>Mail</c> (check/change a mail flag for the current player) or <c>Value</c> (check/set a C# field on the farmhouse instance).</summary>
        public string Type;
        /// <summary>The mail flag (if <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Mail</c>) or field name (if <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Value</c>) to check or set.</summary>
        public string Key;
        /// <summary>
        /// The effect of this field depends on whether this is used in <see cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.Requirements" /> or <see cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.RenovateActions" />, and the value of <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" />:
        /// 
        /// <list type="bullet">
        ///   <item><description>
        ///     For a renovation requirement:
        ///     <list type="bullet">
        ///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Mail</c>, either <c>"0"</c> (player must not have the flag) or <c>"1"</c> (player must have it).</description></item>
        ///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Value</c>, the required field value. This can be prefixed with <c>!</c> to require any value <em>except</em> this one.</description></item>
        ///     </list>
        ///   </description></item>
        ///   <item><description>
        ///     For a renovate action:
        ///     <list type="bullet">
        ///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Mail</c>, either <c>"0"</c> (remove the mail flag) or <c>"1"</c> (add it).</description></item>
        ///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Value</c>, either the integer value to set, or the exact string <c>"selected"</c> to set it to the index of the applied renovation.</description></item>
        ///     </list>
        ///   </description></item>
        /// </list>
        /// </summary>
        public string Value;
    }
}

public class LocationContexts {
    /// <summary>A world area which groups multiple in-game locations with shared settings and metadata.</summary>
    [RType]
    public class LocationContextData {
        /// <summary>The season which is always active for locations within this context. For example, setting <see cref="F:StardewValley.Season.Summer" /> will make it always summer there regardless of the calendar season. If not set, the calendar season applies.</summary>
        [Optional] public Season? SeasonOverride;
        /// <summary>The cue ID for the music to play when the player is in the location, unless overridden by a <c>Music</c> map property. Despite the name, this has a higher priority than the seasonal music fields like <see cref="!:SpringMusic" />. Ignored if omitted.</summary>
        [Optional] public string DefaultMusic;
        /// <summary>A game state query which returns whether the <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.DefaultMusic" /> field should be applied (if more specific music isn't playing). Defaults to always true.</summary>
        [Optional] public string DefaultMusicCondition;
        /// <summary>When the player warps and the music changes, whether to silence the music and play the ambience (if any) until the next warp. This is similar to the default valley locations.</summary>
        [Optional] public bool DefaultMusicDelayOneScreen = true;
        /// <summary>A list of cue IDs to play before noon unless it's raining, there's a <c>Music</c> map property, or the context has a <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.DefaultMusic" /> value. If multiple values are specified, the game will play one per day in sequence.</summary>
        [Optional] public List<Locations.LocationMusicData> Music = [];
        /// <summary>The cue ID for the background ambience to before dark, when there's no music active. Defaults to none.</summary>
        [Optional] public string DayAmbience;
        /// <summary>The cue ID for the background ambience to after dark, when there's no music active. Defaults to none.</summary>
        [Optional] public string NightAmbience;
        /// <summary>Whether to play random ambience sounds when outdoors depending on factors like the season and time of day (e.g. birds and crickets). This is unrelated to the <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.DayAmbience" /> and <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.NightAmbience" /> fields.</summary>
        [Optional] public bool PlayRandomAmbientSounds = true;
        /// <summary>Whether a rain totem can be used to force rain in this context tomorrow.</summary>
        [Optional] public bool AllowRainTotem = true;
        /// <summary>If set, using a rain totem within the context changes the weather in the given context instead.</summary>
        /// <remarks>This is ignored if <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.AllowRainTotem" /> is false.</remarks>
        [Optional] public string RainTotemAffectsContext;
        /// <summary>The weather rules to apply for locations in this context (ignored if <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.CopyWeatherFromLocation" /> is set). Defaults to always sunny. If multiple are specified, the first matching weather is applied.</summary>
        [Optional] public List<WeatherCondition> WeatherConditions = [];
        /// <summary>The ID of the location context from which to inherit weather, if any. If this is set, the <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.WeatherConditions" /> field is ignored.</summary>
        [Optional] public string CopyWeatherFromLocation;
        /// <summary>
        ///   <para>When the player gets knocked out in combat, the locations where they can wake up. If multiple locations match, the first match will be used. If none match, the player will wake up at Harvey's clinic.</para>
        ///   <para>If the selected location has a standard event with the exact key <c>PlayerKilled</c>, that event will play when the player wakes up and the game will apply the lost items or gold logic. The game won't track this event, so it'll repeat each time the player is revived. If there's no such event, the player will wake up without an event, and no items or gold will be lost.</para>
        /// </summary>
        [Optional] public List<ReviveLocation> ReviveLocations;
        /// <summary>When the player passes out (due to exhaustion or at 2am) in this context, the maximum amount of gold lost. If set to <c>-1</c>, uses the same value as the default context.</summary>
        [Optional] public int MaxPassOutCost = -1;
        /// <summary>When the player passes out (due to exhaustion or at 2am) in this context, the possible letters to add to their mailbox (if they haven't received it before).</summary>
        /// <remarks>If multiple letters are valid, one will be chosen randomly (unless one of them specifies <see cref="F:StardewValley.GameData.LocationContexts.PassOutMailData.SkipRandomSelection" />).</remarks>
        [Optional] public List<PassOutMailData> PassOutMail;
        /// <summary>When the player passes out (due to exhaustion or at 2am), the locations where they can wake up.</summary>
        /// <remarks>
        ///   <para>If multiple locations match, the first match will be used. If none match, the player will wake up in their bed at home.</para>
        ///   <para>The selected location must either have a bed or the <c>AllowWakeUpWithoutBed: true</c> map property, otherwise the player will be warped home instead.</para>
        /// </remarks>
        [Optional] public List<ReviveLocation> PassOutLocations;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.LocationContexts.LocationContextData" />, a letter added to the player's mailbox when they pass out (due to exhaustion or at 2am).</summary>
    [RType]
    public class PassOutMailData {
        /// <summary>A unique string ID for this entry within the current location context.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether this entry is active. Defaults to always true.</summary>
        [Optional] public string Condition;
        /// <summary>The letter ID to add.</summary>
        /// <remarks>
        ///   <para>The game will look for an existing letter ID in the <c>Data/mail</c> asset in this order (where <c>{billed}</c> is <c>Billed</c> if they lost gold or <c>NotBilled</c> otherwise, and <c>{gender}</c> is <c>Female</c> or <c>Male</c>): <c>{letter id}_{billed}_{gender}</c>, <c>{letter id}_{billed}</c>, <c>{letter id}</c>. If no match is found, the game will send <c>passedOut2</c> instead.</para>
        ///   <para>If the mail ID starts with <c>passedOut</c>, <c>{0}</c> in the letter text will be replaced with the gold amount lost, and the letter won't appear on the collections tab.</para>
        /// </remarks>
        public string Mail;
        /// <summary>The maximum amount of gold lost. This is applied after the context's <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.MaxPassOutCost" /> (i.e. the context's value is used to calculate the random amount, then this field caps the result). Defaults to unlimited.</summary>
        [Optional] public int MaxPassOutCost = -1;
        /// <summary>When multiple mail entries match, whether to send this one instead of choosing one randomly.</summary>
        [Optional] public bool SkipRandomSelection;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.LocationContexts.LocationContextData" />, the locations where a player wakes up after passing out or getting knocked out.</summary>
    [RType]
    public class ReviveLocation {
        /// <summary>A unique string ID for this entry within the current location context.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether this entry is active. Defaults to always applied.</summary>
        [Optional] public string Condition;
        /// <summary>The internal location name.</summary>
        public string Location;
        /// <summary>The tile position within the location.</summary>
        public Point Position;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.LocationContexts.LocationContextData" />, a weather rule to apply for locations in this context.</summary>
    [RType]
    public class WeatherCondition {
        /// <summary>A unique string ID for this entry within the current location context.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether to apply the weather. Defaults to always applied.</summary>
        [Optional] public string Condition;
        /// <summary>The weather ID to set.</summary>
        public string Weather;
    }
}

public class Locations {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, an item that can be found by digging an artifact dig spot.</summary>
    /// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
    [RType]
    public class ArtifactSpotDropData : GenericSpawnItemDataWithCondition {
        /// <summary>A probability that this item will be found, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public double Chance { get; set; } = 1.0;
        /// <summary>Whether the item may drop twice if the player is using a hoe with the Generous enchantment.</summary>
        [Optional] public bool ApplyGenerousEnchantment { get; set; } = true;
        /// <summary>Whether to split the dropped item stack into multiple floating debris that each have a stack size of one.</summary>
        [Optional] public bool OneDebrisPerDrop { get; set; } = true;
        /// <summary>The order in which this drop should be checked, where 0 is the default value used by most drops. Drops within each precedence group are checked in the order listed.</summary>
        [Optional] public int Precedence { get; set; }
        /// <summary>Whether to continue searching for more items after this item is dropped, so the artifact spot may drop multiple items.</summary>
        [Optional] public bool ContinueOnDrop { get; set; }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, the data to use to create a location.</summary>
    [RType]
    public class CreateLocationData {
        /// <summary>The asset name for the map to use for this location.</summary>
        public string MapPath;
        /// <summary>The full name of the C# location class to create. This must be one of the vanilla types to avoid a crash when saving. Defaults to a generic <c>StardewValley.GameLocation</c>.</summary>
        /// <summary>Whether this location is always synchronized to farmhands in multiplayer, even if they're not in the location. Any location which allows building cabins <strong>must</strong> have this enabled to avoid breaking game logic.</summary>
        [Optional] public bool AlwaysActive;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, a distinct fish area within the location which may have its own fish (via <see cref="P:StardewValley.GameData.Locations.SpawnFishData.FishAreaId" />) or crab pot catches.</summary>
    [RType]
    public class FishAreaData {
        /// <summary>A tokenizable string for the translated area name, if any.</summary>
        [Optional] public string DisplayName;
        /// <summary>If set, the tile area within the location where the crab pot must be placed.</summary>
        [Optional] public Rectangle? Position;
        /// <summary>The fish types that can be caught with crab pots in this area.</summary>
        /// <remarks>These will be matched against field index 4 in <c>Data/Fish</c> for crab pot fish. If this list is null or empty, it'll default to <c>freshwater</c>.</remarks>
        [Optional] public List<string> CrabPotFishTypes = [];
        /// <summary>The chance that crab pots will find junk instead of a fish in this area, if the player doesn't have the Mariner profession.</summary>
        [Optional] public float CrabPotJunkChance = 0.2f;
    }
    /// <summary>The data for a location to add to the game.</summary>
    [RType]
    public class LocationData {
        /// <summary>A tokenizable string for the translated location name. This is used anytime the location name is shown in-game for base game logic or mods. If omitted, the location will default to its internal name (i.e. the key in <c>Data/AdditionalLocationData</c>).</summary>
        [Optional] public string DisplayName;
        /// <summary>The default tile position where the player should be placed when they arrive in the location, if arriving from a warp that didn't specify a tile position.</summary>
        [Optional] public Point? DefaultArrivalTile;
        /// <summary>Whether NPCs should ignore this location when pathfinding between locations.</summary>
        [Optional] public bool ExcludeFromNpcPathfinding;
        /// <summary>If set, the location will be created automatically when the save is loaded using this data.</summary>
        [Optional] public CreateLocationData CreateOnLoad;
        /// <summary>The former location names which may appear in save data.</summary>
        /// <remarks>If a location in save data has a name which (a) matches one of these values and (b) doesn't match the name of a loaded location, its data will be loaded into this location instead.</remarks>
        [Optional] public List<string> FormerLocationNames = [];
        /// <summary>Whether crops and trees can be planted and grown here by default, unless overridden by their plantable rules. If omitted, defaults to <c>true</c> on the farm and <c>false</c> elsewhere.</summary>
        [Optional] public bool? CanPlantHere;
        /// <summary>Whether green rain trees and debris can spawn here by default.</summary>
        [Optional] public bool CanHaveGreenRainSpawns = true;
        /// <summary>The items that can be found when digging artifact spots in the location.</summary>
        /// <remarks>
        ///   <para>The items that can be dug up in a location are decided by combining this field with the one from the <c>Default</c> entry, sorting them by <see cref="P:StardewValley.GameData.Locations.ArtifactSpotDropData.Precedence" />, and taking the first drop whose fields match. Items with the same precedence are checked in the order listed.</para>
        ///   <para>For consistency, vanilla artifact drops prefer using these precedence values:</para>
        ///   <list type="bullet">
        ///     <item><description>-1000: location items which should override the global priority items (e.g. fossils on Ginger Island);</description></item>
        ///     <item><description>-100: global priority items (e.g. Qi Beans);</description></item>
        ///     <item><description>0: normal items;</description></item>
        ///     <item><description>100: global fallback items (e.g. clay).</description></item>
        ///   </list>
        /// </remarks>
        [Optional] public List<ArtifactSpotDropData> ArtifactSpots = [];
        /// <summary>The distinct fishing areas within the location.</summary>
        /// <remarks>These can be referenced by <see cref="F:StardewValley.GameData.Locations.LocationData.Fish" /> via <see cref="P:StardewValley.GameData.Locations.SpawnFishData.FishAreaId" />, and determine which fish are collected by crab pots.</remarks>
        [Optional] public Dictionary<string, FishAreaData> FishAreas = [];
        /// <summary>The items that can be found by fishing in the location.</summary>
        /// <remarks>
        ///   <para>The items to catch in a location are decided by combining this field with the one from the <c>Default</c> entry, sorting them by <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Precedence" />, and taking the first fish whose fields match. Items with the same precedence are shuffled randomly.</para>
        ///   <para>For consistency, vanilla fish prefer precedence values in these ranges:</para>
        ///   <list type="bullet">
        ///     <item><description>-1100 to -1000: global priority items (e.g. Qi Beans);</description></item>
        ///     <item><description>-200 to -100: unique location items (e.g. legendary fish or secret items);</description></item>
        ///     <item><description>-50 to -1: normal high-priority items;</description></item>
        ///     <item><description>0: normal items;</description></item>
        ///     <item><description>1 to 100: normal low-priority items;</description></item>
        ///     <item><description>1000+: global fallback items (e.g. trash).</description></item>
        ///   </list>
        /// </remarks>
        [Optional] public List<SpawnFishData> Fish = [];
        /// <summary>The forage objects that can spawn in the location.</summary>
        [Optional] public List<SpawnForageData> Forage = [];
        /// <summary>The minimum number of weeds to spawn in a day.</summary>
        [Optional] public int MinDailyWeeds = 2;
        /// <summary>The maximum number of weeds to spawn in a day.</summary>
        [Optional] public int MaxDailyWeeds = 5;
        /// <summary>A multiplier applied to the number of weeds spawned on the first day of the year.</summary>
        [Optional] public int FirstDayWeedMultiplier = 15;
        /// <summary>The minimum forage to try spawning in one day, if the location has fewer than <see cref="F:StardewValley.GameData.Locations.LocationData.MaxSpawnedForageAtOnce" /> forage.</summary>
        [Optional] public int MinDailyForageSpawn = 1;
        /// <summary>The maximum forage to try spawning in one day, if the location has fewer than <see cref="F:StardewValley.GameData.Locations.LocationData.MaxSpawnedForageAtOnce" /> forage.</summary>
        [Optional] public int MaxDailyForageSpawn = 4;
        /// <summary>The maximum number of spawned forage that can be present at once on the map before they stop spawning.</summary>
        [Optional] public int MaxSpawnedForageAtOnce = 6;
        /// <summary>The probability that digging a tile will produce clay, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public double ChanceForClay = 0.03;
        /// <summary>The music to play when the player enters the location (subject to the other fields like <see cref="F:StardewValley.GameData.Locations.LocationData.MusicContext" />).</summary>
        /// <remarks>The first matching entry is used. If none match, falls back to <see cref="F:StardewValley.GameData.Locations.LocationData.MusicDefault" />.s</remarks>
        [Optional] public List<LocationMusicData> Music = [];
        /// <summary>The music to play if none of the options in <see cref="F:StardewValley.GameData.Locations.LocationData.Music" /> matched.</summary>
        /// <remarks>If this is null, falls back to the <c>Music</c> map property (if set).</remarks>
        [Optional] public string MusicDefault;
        /// <summary>The music context for this location. The recommended values are <c>Default</c> or <c>SubLocation</c>.</summary>
        [Optional] public GameData.MusicContext MusicContext;
        /// <summary>Whether to ignore the <c>Music</c> map property when it's raining in this location.</summary>
        [Optional] public bool MusicIgnoredInRain;
        /// <summary>Whether to ignore the <c>Music</c> map property when it's spring in this location.</summary>
        [Optional] public bool MusicIgnoredInSpring;
        /// <summary>Whether to ignore the <c>Music</c> map property when it's summer in this location.</summary>
        [Optional] public bool MusicIgnoredInSummer;
        /// <summary>Whether to ignore the <c>Music</c> map property when it's fall in this location.</summary>
        [Optional] public bool MusicIgnoredInFall;
        /// <summary>Whether to ignore the <c>Music</c> map property when it's fall and windy weather in this location.</summary>
        [Optional] public bool MusicIgnoredInFallDebris;
        /// <summary>Whether to ignore the <c>Music</c> map property when it's winter in this location.</summary>
        [Optional] public bool MusicIgnoredInWinter;
        /// <summary>Whether to use the same music behavior as Pelican Town's music: it will start playing after the day music has finished, and will continue playing while the player travels through indoor areas, but will stop when entering another outdoor area that isn't marked with the same <c>Music</c> map property and <c>MusicIsTownTheme</c> data field.</summary>
        [Optional] public bool MusicIsTownTheme;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, a music cue to play when the player enters the location (subject to the other fields like <see cref="F:StardewValley.GameData.Locations.LocationData.MusicContext" />).</summary>
    [RType]
    public class LocationMusicData {
        /// <summary>The backing field for <see cref="P:StardewValley.GameData.Locations.LocationMusicData.Id" />.</summary>
        string _idImpl;
        /// <summary>A unique string ID for this track within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_TrackName</c>. Defaults to <see cref="P:StardewValley.GameData.Locations.LocationMusicData.Track" /> if omitted.</summary>
        [Optional] public string Id { get => _idImpl ?? Track; set => _idImpl = value; }
        /// <summary>The audio track ID to play, or <c>null</c> to stop music.</summary>
        public string Track { get; set; }
        /// <summary>A game state query which indicates whether the music should be played. Defaults to true.</summary>
        [Optional] public string Condition;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, an item that can be found by fishing in the location.</summary>
    /// <remarks>
    ///   Fish spawns have a few special constraints:
    ///   <list type="bullet">
    ///     <item><description>Only one fish can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</description></item>
    ///     <item><description>This must return an item of type <c>StardewValley.Object</c> or one of its subclasses.</description></item>
    ///     <item><description>Entries using an item query (instead of an item ID) are ignored for the fishing TV channel hints.</description></item>
    ///   </list>
    /// </remarks>
    [RType]
    public class SpawnFishData : GenericSpawnItemDataWithCondition {
        /// <summary>The probability that the fish will spawn, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public float Chance { get; set; } = 1f;
        /// <summary>If set, the specific season when the fish should apply. For more complex conditions, see <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" />.</summary>
        [Optional] public Season? Season { get; set; }
        /// <summary>If set, the fish area (as defined by <see cref="F:StardewValley.GameData.Locations.LocationData.FishAreas" /> in which the fish can be caught. If omitted, it can be caught in all areas.</summary>
        [Optional] public string FishAreaId { get; set; }
        /// <summary>If set, the tile area within the location where the bobber must land to catch the fish.</summary>
        [Optional] public Rectangle? BobberPosition { get; set; }
        /// <summary>If set, the tile area within the location where the player must be standing to catch the fish.</summary>
        [Optional] public Rectangle? PlayerPosition { get; set; }
        /// <summary>The minimum fishing level needed for the fish to appear.</summary>
        [Optional] public int MinFishingLevel { get; set; }
        /// <summary>The minimum distance from the shore (measured in tiles) at which the fish can be caught, where zero is water directly adjacent to shore.</summary>
        [Optional] public int MinDistanceFromShore { get; set; }
        /// <summary>The maximum distance from the shore (measured in tiles) at which the fish can be caught, where zero is water directly adjacent to shore, or -1 for no maximum.</summary>
        [Optional] public int MaxDistanceFromShore { get; set; } = -1;
        /// <summary>Whether to increase the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" /> by an amount equal to the player's daily luck.</summary>
        [Optional] public bool ApplyDailyLuck { get; set; }
        /// <summary>A flat increase to the spawn chance when the player has the Curiosity Lure equipped, or <c>-1</c> to apply the default behavior. This affects both the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" /> field and the <c>Data\Fish</c> chance, if applicable.</summary>
        [Optional] public float CuriosityLureBuff { get; set; } = -1f;
        /// <summary>A flat increase to the spawn chance when the player has a specific bait equipped which targets this fish.</summary>
        [Optional] public float SpecificBaitBuff { get; set; }
        /// <summary>A multiplier applied to the spawn chance when the player has a specific bait equipped which targets this fish.</summary>
        [Optional] public float SpecificBaitMultiplier { get; set; } = 1.66f;
        /// <summary>The maximum number of times this fish can be caught by each player.</summary>
        [Optional] public int CatchLimit { get; set; } = -1;
        /// <summary>Whether the player can catch this fish using a training rod. This can be <c>true</c> (always allowed), <c>false</c> (never allowed), or <c>null</c> (apply default logic, i.e. allowed for difficulty ratings under 50).</summary>
        [Optional] public bool? CanUseTrainingRod { get; set; }
        /// <summary>Whether this is a 'boss fish' in the fishing minigame. This shows a crowned fish sprite in the minigame, multiplies the XP gained by five, and hides it from the F.I.B.S. TV channel.</summary>
        [Optional] public bool IsBossFish { get; set; }
        /// <summary>The mail flag to set for the current player when this fish is successfully caught.</summary>
        [Optional] public string SetFlagOnCatch { get; set; }
        /// <summary>Whether the player must fish with Magic Bait for this fish to spawn.</summary>
        [Optional] public bool RequireMagicBait { get; set; }
        /// <summary>The order in which this fish should be checked, where 0 is the default value used by most fish. Fish within each precedence group are shuffled randomly.</summary>
        [Optional] public int Precedence { get; set; }
        /// <summary>Whether to ignore any fish requirements listed for the ID in <c>Data/Fish</c>.</summary>
        /// <remarks>The <c>Data/Fish</c> requirements are ignored regardless of this field for non-object (<c>(O)</c>)-type items, or objects with an ID not listed in <c>Data/Fish</c>.</remarks>
        [Optional] public bool IgnoreFishDataRequirements { get; set; }
        /// <summary>Whether this fish can be spawned in another location via the <c>LOCATION_FISH</c> item query.</summary>
        [Optional] public bool CanBeInherited { get; set; } = true;
        /// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" />.</summary>
        [Optional] public List<QuantityModifier> ChanceModifiers { get; set; }
        /// <summary>How multiple <see cref="P:StardewValley.GameData.Locations.SpawnFishData.ChanceModifiers" /> should be combined.</summary>
        [Optional] public QuantityModifier.QuantityModifierMode ChanceModifierMode { get; set; }
        /// <summary>How much to increase the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" /> per player's Luck level</summary>
        [Optional] public float ChanceBoostPerLuckLevel { get; set; }
        /// <summary>If true, the chance roll will use a seed value based on the number of fish caught.</summary>
        [Optional] public bool UseFishCaughtSeededRandom { get; set; }
        /// <summary>Get the probability that the fish will spawn, adjusted for modifiers and equipment.</summary>
        /// <param name="hasCuriosityLure">Whether the player has the Curiosity Lure equipped.</param>
        /// <param name="dailyLuck">The player's daily luck value.</param>
        /// <param name="luckLevel">The player's current luck level.</param>
        /// <param name="applyModifiers">Apply quantity modifiers to the given value.</param>
        /// <param name="isTargetedWithBait">Whether the player has a specific bait equipped which targets this fish.</param>
        /// <returns>Returns a value between 0 (never) and 1 (always).</returns>
        public float GetChance(bool hasCuriosityLure, double dailyLuck, int luckLevel, Func<float, IList<GameData.QuantityModifier>, GameData.QuantityModifier.QuantityModifierMode, float> applyModifiers, bool isTargetedWithBait = false) {
            var num = Chance;
            if (hasCuriosityLure && (double)CuriosityLureBuff > 0.0) num += CuriosityLureBuff;
            if (ApplyDailyLuck) num += (float)dailyLuck;
            if ((ChanceModifiers != null ? (ChanceModifiers.Count > 0 ? 1 : 0) : 0) != 0) num = applyModifiers(num, ChanceModifiers, ChanceModifierMode);
            if (isTargetedWithBait) num = num * SpecificBaitMultiplier + SpecificBaitBuff;
            return num + ChanceBoostPerLuckLevel * luckLevel;
        }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, a forage object that can spawn in the location.</summary>
    /// <remarks>
    ///   Forage spawns have a few special constraints:
    ///   <list type="bullet">
    ///     <item><description>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</description></item>
    ///     <item><description>If this returns a null or non-<c>StardewValley.Object</c> item, the game will skip that spawn opportunity (and log a warning for a non-null invalid item type).</description></item>
    ///     <item><description>The <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" /> field is checked once right before spawning forage, to build the list of possible forage spawns. It's not checked again for each forage spawn; use the <see cref="P:StardewValley.GameData.Locations.SpawnForageData.Chance" /> instead for per-spawn probability.</description></item>
    ///   </list>
    /// </remarks>
    [RType]
    public class SpawnForageData : GenericSpawnItemDataWithCondition {
        /// <summary>The probability that the forage will spawn if it's selected, as a value between 0 (never) and 1 (always). If this check fails, that spawn opportunity will be skipped.</summary>
        [Optional] public double Chance { get; set; } = 1.0;
        /// <summary>If set, the specific season when the forage should apply. For more complex conditions, see <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" />.</summary>
        [Optional] public Season? Season { get; set; }
    }
}

public class Machines {
    /// <summary>The behavior and metadata for a machine which takes input, produces output, or both.</summary>
    [RType]
    public class MachineData {
        /// <summary>Whether to force adding the <c>machine_input</c> context tag, which indicates the machine can accept input.</summary>
        /// <remarks>If false, this will be set automatically if any <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" /> use the <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> trigger.</remarks>
        [Optional] public bool HasInput;
        /// <summary>Whether to force adding the <c>machine_output</c> context tag, which indicates the machine can produce output.</summary>
        /// <remarks>If false, this will be set automatically if there are <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" />.</remarks>
        [Optional] public bool HasOutput;
        /// <summary>A C# method invoked when the player interacts with the machine while it doesn't have output ready to harvest.</summary>
        /// <remarks><strong>This is an advanced field. Most machines shouldn't use this.</strong> This must be specified in the form <c>{full type name}: {method name}</c> (like <c>StardewValley.Object, Stardew Valley: SomeInteractMethod</c>). The method must be static, take three arguments (<c>Object machine, GameLocation location, Farmer player</c>), and return a boolean indicating whether the interaction succeeded.</remarks>
        [Optional] public string InteractMethod;
        /// <summary>The rules which define how to process input items and produce output.</summary>
        [Optional] public List<MachineOutputRule> OutputRules;
        /// <summary>A list of extra items required before <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" /> will be checked. If specified, every listed item must be present in the player, hopper, or chest inventory (depending how the machine is being loaded).</summary>
        [Optional] public List<MachineItemAdditionalConsumedItems> AdditionalConsumedItems;
        /// <summary>A list of cases when the machine should be paused, so the timer on any item being produced doesn't decrement.</summary>
        [Optional] public List<MachineTimeBlockers> PreventTimePass;
        /// <summary>Changes to apply to the processing time before output is ready.</summary>
        /// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double processing time will quadruple it).</remarks>
        [Optional] public List<QuantityModifier> ReadyTimeModifiers;
        /// <summary>How multiple <see cref="F:StardewValley.GameData.Machines.MachineData.ReadyTimeModifiers" /> should be combined.</summary>
        [Optional] public QuantityModifier.QuantityModifierMode ReadyTimeModifierMode;
        /// <summary>A tokenizable string for the message shown in a toaster notification if the player tries to input an item that isn't accepted by the machine.</summary>
        [Optional] public string InvalidItemMessage;
        /// <summary>An extra condition that must be met before <see cref="F:StardewValley.GameData.Machines.MachineData.InvalidItemMessage" /> is shown.</summary>
        [Optional] public string InvalidItemMessageCondition;
        /// <summary>A tokenizable string for the message shown in a toaster notification if the input inventory doesn't contain this item, unless overridden by <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.InvalidCountMessage" /> under <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" />.</summary>
        /// <remarks>
        ///   This can use extra tokens:
        ///   <list type="bullet">
        ///     <item><description><c>[ItemCount]</c>: the number of remaining items needed. For example, if you're holding three and need five, <c>[ItemCount]</c> will be replaced with 2.</description></item>
        ///   </list>
        /// </remarks>
        [Optional] public string InvalidCountMessage;
        /// <summary>The cosmetic effects to show when an item is loaded into the machine.</summary>
        [Optional] public List<MachineEffects> LoadEffects;
        /// <summary>The cosmetic effects to show while the machine is processing an input, based on the <see cref="F:StardewValley.GameData.Machines.MachineData.WorkingEffectChance" />.</summary>
        [Optional] public List<MachineEffects> WorkingEffects;
        /// <summary>The percentage chance to apply <see cref="F:StardewValley.GameData.Machines.MachineData.WorkingEffects" /> each time the day starts or the in-game clock changes, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public float WorkingEffectChance = 0.33f;
        /// <summary>Whether the player can drop a new item into the machine before it's done processing the last one (like the crystalarium). The previous item will be lost.</summary>
        [Optional] public bool AllowLoadWhenFull;
        /// <summary>Whether the machine sprite should bulge in &amp; out while it's processing an item.</summary>
        [Optional] public bool WobbleWhileWorking = true;
        /// <summary>A light emitted while the machine is processing an item.</summary>
        [Optional] public MachineLight LightWhileWorking;
        /// <summary>Whether to show the next sprite in the machine's spritesheet while it's processing an item.</summary>
        [Optional] public bool ShowNextIndexWhileWorking;
        /// <summary>Whether to show the next sprite in the machine's spritesheet while it has an output ready to collect.</summary>
        [Optional] public bool ShowNextIndexWhenReady;
        /// <summary>Whether the player can add fairy dust to speed up the machine.</summary>
        [Optional] public bool AllowFairyDust = true;
        /// <summary>Whether this machine acts as an incubator when placed in a building, so players can incubate eggs in it.</summary>
        /// <remarks>This is used by the incubator and ostrich incubator. The game logic assumes there's only one such machine in each building, so this generally shouldn't be used by custom machines that can be built in a vanilla barn or coop.</remarks>
        [Optional] public bool IsIncubator;
        /// <summary>Whether the machine should only produce output overnight. If it finishes processing during the day, it'll pause until its next day update.</summary>
        [Optional] public bool OnlyCompleteOvernight;
        /// <summary>A game state query which indicates whether the machine should be emptied overnight, so any current output will be lost. Defaults to always false.</summary>
        [Optional] public string ClearContentsOvernightCondition;
        /// <summary>The game stat counters to increment when an item is placed in the machine.</summary>
        [Optional] public List<StatIncrement> StatsToIncrementWhenLoaded;
        /// <summary>The game stat counters to increment when the processed output is collected.</summary>
        [Optional] public List<StatIncrement> StatsToIncrementWhenHarvested;
        /// <summary>A list of (skillName) (amount), e.g. Farming 7 Fishing 5 </summary>
        [Optional] public string ExperienceGainOnHarvest;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, a cosmetic effect shown when an item is loaded into the machine or while it's processing an input.</summary>
    [RType]
    public class MachineEffects {
        /// <summary>A unique string ID for this effect in this list.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether to add this temporary sprite.</summary>
        [Optional] public string Condition;
        /// <summary>The audio to play.</summary>
        [Optional] public List<MachineSoundData> Sounds;
        /// <summary>The number of milliseconds for which each frame in <see cref="F:StardewValley.GameData.Machines.MachineEffects.Frames" /> is kept on-screen.</summary>
        [Optional] public int Interval = 100;
        /// <summary>The animation to apply to the machine sprite, specified as a list of offsets relative to the base sprite index. Default none.</summary>
        [Optional] public List<int> Frames;
        /// <summary>A duration in milliseconds during which the machine sprite should shake. Default none.</summary>
        [Optional] public int ShakeDuration = -1;
        /// <summary>The temporary animated sprites to show.</summary>
        [Optional] public List<TemporaryAnimatedSpriteDefinition> TemporarySprites;
    }
    /// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineData" />, an extra item required before the machine starts.</summary>
    [RType]
    public class MachineItemAdditionalConsumedItems {
        /// <summary>The qualified or unqualified item ID for the required item.</summary>
        public string ItemId;
        /// <summary>The required stack size for the item matching <see cref="F:StardewValley.GameData.Machines.MachineItemAdditionalConsumedItems.ItemId" />.</summary>
        [Optional] public int RequiredCount = 1;
        /// <summary>If set, overrides the machine's main <see cref="F:StardewValley.GameData.Machines.MachineData.InvalidCountMessage" />.</summary>
        public string InvalidCountMessage;
    }
    /// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineData" />, an item produced by this machine.</summary>
    /// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
    [RType]
    public class MachineItemOutput : GameData.GenericSpawnItemDataWithCondition {
        /// <summary>Machine-specific data provided to the machine logic, if applicable.</summary>
        /// <remarks>For vanilla machines, this is used by casks to set the <c>AgingMultiplier</c> for each item.</remarks>
        [Optional] public Dictionary<string, string> CustomData;
        /// <summary>A C# method which produces the item to output.</summary>
        /// <remarks>
        ///   <para><strong>This is an advanced field. Most machines shouldn't use this.</strong> This must be specified in the form <c>{full type name}: {method name}</c> (like <c>StardewValley.Object, Stardew Valley: OutputSolarPanel</c>). The method must be static, take five arguments (<c>Object machine, GameLocation location, Farmer player, Item? inputItem, bool probe</c>), and return the <c>Item</c> instance to output. If this method returns null, the machine won't output anything.</para>
        ///   <para>If set, the other fields which change the output item (like <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> or <see cref="P:StardewValley.GameData.Machines.MachineItemOutput.CopyColor" />) are ignored.</para>
        /// </remarks>
        [Optional] public string OutputMethod { get; set; }
        /// <summary>Whether to inherit the color of the input item if it was a <c>ColoredObject</c>. This mainly affects roe.</summary>
        [Optional] public bool CopyColor { get; set; }
        /// <summary>Whether to inherit the price of the input item, before modifiers like <see cref="P:StardewValley.GameData.Machines.MachineItemOutput.PriceModifiers" /> are applied. This is ignored if the input or output aren't both object (<c>(O)</c>)-type.</summary>
        [Optional] public bool CopyPrice { get; set; }
        /// <summary>Whether to inherit the quality of the input item, before modifiers like <see cref="P:StardewValley.GameData.GenericSpawnItemData.QualityModifiers" /> are applied.</summary>
        [Optional] public bool CopyQuality { get; set; }
        /// <summary>The produced item's preserved item type, if applicable. This sets the equivalent flag on the output item. The valid values are <c>Jelly</c>, <c>Juice</c>, <c>Pickle</c>, <c>Roe</c> or <c>AgedRoe</c>, and <c>Wine</c>. Defaults to none.</summary>
        [Optional] public string PreserveType { get; set; }
        /// <summary>The produced item's preserved unqualified item ID, if applicable. For example, blueberry wine has its preserved item ID set to the blueberry ID. This can be set to <c>DROP_IN</c> to use the input item's ID. Default none.</summary>
        [Optional] public string PreserveId { get; set; }
        /// <summary>An amount by which to increment the machine's spritesheet index while it's processing this output. This stacks with <see cref="F:StardewValley.GameData.Machines.MachineData.ShowNextIndexWhileWorking" /> or <see cref="F:StardewValley.GameData.Machines.MachineData.ShowNextIndexWhenReady" />.</summary>
        [Optional] public int IncrementMachineParentSheetIndex { get; set; }
        /// <summary>Changes to apply to the item price. This is ignored if the output isn't object (<c>(O)</c>)-type.</summary>
        [Optional] public List<QuantityModifier> PriceModifiers { get; set; }
        /// <summary>How multiple <see cref="P:StardewValley.GameData.Machines.MachineItemOutput.PriceModifiers" /> should be combined.</summary>
        [Optional] public QuantityModifier.QuantityModifierMode PriceModifierMode { get; set; }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, a light effect shown around the machine.</summary>
    [RType]
    public class MachineLight {
        /// <summary>The radius of the light emitted.</summary>
        [Optional] public float Radius = 1f;
        /// <summary>A tint color to apply to the light. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
        [Optional] public string Color;
    }
    /// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineData" />, a rule which define how to process input items and produce output.</summary>
    [RType]
    public class MachineOutputRule {
        /// <summary>A unique identifier for this item within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_Parsnips</c>.</summary>
        public string Id;
        /// <summary>The rules for when this output rule can be applied.</summary>
        public List<MachineOutputTriggerRule> Triggers;
        /// <summary>If multiple <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.OutputItem" /> entries match, whether to use the first match instead of choosing one randomly.</summary>
        [Optional] public bool UseFirstValidOutput;
        /// <summary>The items produced by this output rule. If multiple entries match, one will be selected randomly unless you specify <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.UseFirstValidOutput" />.</summary>
        [Optional] public List<MachineItemOutput> OutputItem;
        /// <summary>The number of in-game minutes until the output is ready to collect.</summary>
        /// <remarks>If both days and minutes are specified, days are used. If neither are specified, the item will be ready instantly.</remarks>
        [Optional] public int MinutesUntilReady = -1;
        /// <summary>The number of in-game days until the output is ready to collect.</summary>
        /// <remarks><inheritdoc cref="F:StardewValley.GameData.Machines.MachineOutputRule.MinutesUntilReady" select="/Remarks" /></remarks>
        [Optional] public int DaysUntilReady = -1;
        /// <summary>If set, overrides the machine's main <see cref="F:StardewValley.GameData.Machines.MachineData.InvalidCountMessage" />.</summary>
        [Optional] public string InvalidCountMessage;
        /// <summary>Whether to regenerate the output right before the player collects it, and return the new item instead of what was originally created by the rule.</summary>
        /// <remarks>This is specialized to support bee houses. If the new item is null, the original item is returned instead.</remarks>
        [Optional] public bool RecalculateOnCollect;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, indicates when a machine should start producing output.</summary>
    [Flags, RType]
    public enum MachineOutputTrigger {
        /// <summary>The machine is never triggered automatically.</summary>
        None = 0,
        /// <summary>Apply this rule when an item is placed into the machine.</summary>
        ItemPlacedInMachine = 1,
        /// <summary>Apply this rule when the machine's previous output is collected. An output-collected rule won't require or consume the input items, and the input item will be the previous output.</summary>
        OutputCollected = 2,
        /// <summary>Apply this rule when the machine is put down. For example, the worm bin uses this to start as soon as it's put down.</summary>
        MachinePutDown = 4,
        /// <summary>Apply this rule when a new day starts, if it isn't already processing output. For example, the soda machine does this.</summary>
        DayUpdate = 8,
    }
    /// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineOutputRule" />, indicates when the output rule can be applied.</summary>
    [RType]
    public class MachineOutputTriggerRule {
        /// <summary>The backing field for <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.Id" />.</summary>
        string _idImpl;
        /// <summary>A unique identifier for this item within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_Parsnips</c>.</summary>
        [Optional] public string Id { get => _idImpl ?? Trigger.ToString(); set => _idImpl = value; }
        /// <summary>When this output rule should apply.</summary>
        [Optional] public MachineOutputTrigger Trigger = MachineOutputTrigger.ItemPlacedInMachine;
        /// <summary>The qualified or unqualified item ID for the item to match, if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />.</summary>
        /// <remarks>You can specify any combination of <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId" />, <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredTags" />, and <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.Condition" />. The input item must match all specified fields; if none are specified, this conversion will always match.</remarks>
        [Optional] public string RequiredItemId;
        /// <summary>The context tags to match against input items, if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />. An item must match all of the listed tags to select this rule. You can negate a tag with ! (like <c>!fossil_item</c> to exclude fossils).</summary>
        /// <inheritdoc cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId" select="Remarks" />
        [Optional] public List<string> RequiredTags;
        /// <summary>The required stack size for the input item, if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />.</summary>
        [Optional] public int RequiredCount = 1;
        /// <summary>A game state query which indicates whether a given input should be matched (if the other requirements are matched too). Item-only tokens are valid for this check if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />. Defaults to always true.</summary>
        /// <inheritdoc cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId" select="Remarks" />
        [Optional] public string Condition;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, an audio cue to play.</summary>
    [RType]
    public class MachineSoundData {
        /// <summary>The audio cue ID to play.</summary>
        public string Id;
        /// <summary>The number of milliseconds until the sound should play.</summary>
        [Optional] public int Delay;
    }
    /// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineTimeBlockers" />, indicates when the machine should be paused.</summary>
    [RType]
    public enum MachineTimeBlockers {
        /// <summary>Pause when placed in an outside location.</summary>
        Outside,
        /// <summary>Pause when placed in an inside location.</summary>
        Inside,
        /// <summary>Pause in spring.</summary>
        Spring,
        /// <summary>Pause in summer.</summary>
        Summer,
        /// <summary>Pause in fall.</summary>
        Fall,
        /// <summary>Pause in winter.</summary>
        Winter,
        /// <summary>Pause on sunny days.</summary>
        Sun,
        /// <summary>Pause on rainy days.</summary>
        Rain,
        /// <summary>Always pause the machine. This is used in specialized cases where the timer is handled by advanced machine logic.</summary>
        Always,
    }
}

public class MakeoverOutfits {
    /// <summary>A hat, shirt, or pants that should be equipped on the player as part of a <see cref="T:StardewValley.GameData.MakeoverOutfits.MakeoverOutfit" />.</summary>
    [RType]
    public class MakeoverItem {
        /// <summary>A unique ID for this entry within the list.</summary>
        public string Id;
        /// <summary>The qualified item ID for the hat, shirt, or pants to equip.</summary>
        public string ItemId;
        /// <summary>A tint color to apply to the item. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
        [Optional] public string Color;
        /// <summary>The player gender for which the outfit part applies, or <c>null</c> for any gender.</summary>
        [Optional] public Gender? Gender;
        /// <summary>Get whether this item applies to the given player gender.</summary>
        /// <param name="gender">The player gender to check.</param>
        public bool MatchesGender(Gender gender) {
            if (!Gender.HasValue) return true;
            return Gender.GetValueOrDefault() == gender & Gender.HasValue;
        }
    }
    /// <summary>An outfit that can be selected at the Desert Festival makeover booth.</summary>
    [RType]
    public class MakeoverOutfit {
        /// <summary>A unique string ID for this entry within the outfit list.</summary>
        public string Id;
        /// <summary>The hat, shirt, and pants that makes up the outfit. Each item is added to the appropriate equipment slot based on its type.</summary>
        /// <remarks>An item can be omitted to leave the player's current item unchanged (e.g. shirt + pants without a hat). If there are multiple items of the same type, the first matching one is applied.</remarks>
        public List<MakeoverItem> OutfitParts;
        /// <summary>The player gender for which the outfit applies, or <c>null</c> for any gender.</summary>
        [Optional] public Gender? Gender;
    }
}

public class Minecarts {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Minecarts.MinecartNetworkData" />, a minecart destination which can be used by players.</summary>
    [RType]
    public class MinecartDestinationData {
        /// <summary>A unique string ID for this destination within the network.</summary>
        public string Id;
        /// <summary>A tokenizable string for the destination name shown in the minecart menu. You can use the location's display name with the <c>LocationName</c> token (like <c>[LocationName Desert]</c> for the desert).</summary>
        public string DisplayName;
        /// <summary>A game state query which indicates whether this minecart destination is available. Defaults to always available.</summary>
        [Optional] public string Condition;
        /// <summary>The gold price that must be paid to go to this destination, if any.</summary>
        [Optional] public int Price;
        /// <summary>A localizable string for the message to show when purchasing a ticket, if applicable. Defaults to <see cref="F:StardewValley.GameData.Minecarts.MinecartNetworkData.BuyTicketMessage" />.</summary>
        [Optional] public string BuyTicketMessage;
        /// <summary>The unique name for the location to warp to.</summary>
        public string TargetLocation;
        /// <summary>The destination tile position within the location.</summary>
        public Point TargetTile;
        /// <summary>The direction the player should face after arrival (one of <c>down</c>, <c>left</c>, <c>right</c>, or <c>up</c>).</summary>
        [Optional] public string TargetDirection;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>The data for a network of minecarts, which are enabled together.</summary>
    [RType]
    public class MinecartNetworkData {
        /// <summary>A game state query which indicates whether this minecart network is unlocked.</summary>
        [Optional] public string UnlockCondition;
        /// <summary>A localizable string for the message to show if the network is locked.</summary>
        [Optional] public string LockedMessage;
        /// <summary>A localizable string for the message to show when selecting a destination.</summary>
        [Optional] public string ChooseDestinationMessage;
        /// <summary>A localizable string for the message to show when purchasing a ticket, if applicable.</summary>
        [Optional] public string BuyTicketMessage;
        /// <summary>The destinations which the player can travel to from any minecart in this network.</summary>
        public List<MinecartDestinationData> Destinations;
    }
}

public class Movies {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Movies.SpecialResponses" />, a possible dialogue to show.</summary>
    [RType]
    public class CharacterResponse {
        /// <summary>
        ///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.DuringMovie" />, the <see cref="F:StardewValley.GameData.Movies.MovieScene.ResponsePoint" /> used to decide whether it should be shown during a scene.</para>
        ///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.BeforeMovie" /> or <see cref="F:StardewValley.GameData.Movies.SpecialResponses.AfterMovie" />, this field is ignored.</para>
        /// </summary>
        [Optional] public string ResponsePoint;
        /// <summary>
        ///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.DuringMovie" />, an optional event script to run before the <see cref="F:StardewValley.GameData.Movies.CharacterResponse.Text" /> is shown.</para>
        ///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.BeforeMovie" /> or <see cref="F:StardewValley.GameData.Movies.SpecialResponses.AfterMovie" />, this field is ignored.</para>
        /// </summary>
        [Optional] public string Script;
        /// <summary>The translated dialogue text to show.</summary>
        [Optional] public string Text;
    }
    /// <summary>The metadata for a concession which can be purchased at the movie theater.</summary>
    [RType]
    public class ConcessionItemData {
        /// <summary>A key which uniquely identifies this concession. This should only contain alphanumeric/underscore/dot characters. For custom concessions, this should be prefixed with your mod ID like <c>Example.ModId_ConcessionName</c>.</summary>
        public string Id;
        /// <summary>The internal name for the concession item.</summary>
        public string Name;
        /// <summary>The tokenizable string for the item's translated display name.</summary>
        public string DisplayName;
        /// <summary>The tokenizable string for the item's translated description.</summary>
        public string Description;
        /// <summary>The gold price to purchase the concession.</summary>
        public int Price;
        /// <summary>The asset name for the texture containing the concession's sprite.</summary>
        public string Texture;
        /// <summary>The index within the <see cref="F:StardewValley.GameData.Movies.ConcessionItemData.Texture" /> for the concession sprite, where 0 is the top-left icon.</summary>
        public int SpriteIndex;
        /// <summary>A list of tags which describe the concession, which can be matched by <see cref="T:StardewValley.GameData.Movies.ConcessionTaste" /> fields.</summary>
        [Optional] public List<string> ItemTags;
    }
    /// <summary>The metadata for concession tastes for one or more NPCs.</summary>
    [RType]
    public class ConcessionTaste {
        /// <summary>A unique ID for this entry.</summary>
        [Ignore] public string Id => Name;
        /// <summary>The internal NPC name for which to set tastes, or <c>"*"</c> to apply to all NPCs.</summary>
        public string Name;
        /// <summary>The concessions loved by the matched NPCs.</summary>
        /// <remarks>
        ///   This can be one of...
        ///   <list type="bullet">
        ///     <item><description>the <see cref="F:StardewValley.GameData.Movies.ConcessionItemData.Name" /> for a specific concession;</description></item>
        ///     <item><description>or a tag to match in <see cref="F:StardewValley.GameData.Movies.ConcessionItemData.ItemTags" />.</description></item>
        ///   </list>
        /// </remarks>
        [Optional] public List<string> LovedTags;
        /// <summary>The concessions liked by matched NPCs.</summary>
        /// <remarks>See remarks on <see cref="P:StardewValley.GameData.Movies.ConcessionTaste.LovedTags" />.</remarks>
        [Optional] public List<string> LikedTags;
        /// <summary>The concessions liked by matched NPCs.</summary>
        /// <remarks>See remarks on <see cref="P:StardewValley.GameData.Movies.ConcessionTaste.DislikedTags" />.</remarks>
        [Optional] public List<string> DislikedTags;
    }
    /// <summary>Metadata for how an NPC can react to movies.</summary>
    [RType]
    public class MovieCharacterReaction {
        /// <summary>A unique ID for this entry.</summary>
        [Ignore] public string Id => NPCName;
        /// <summary>The internal name of the NPC for which to define reactions.</summary>
        public string NPCName;
        /// <summary>The possible movie reactions for this NPC.</summary>
        [Optional] public List<MovieReaction> Reactions;
    }
    [RType]
    public class MovieCranePrizeData : GenericSpawnItemDataWithCondition {
        /// <summary>The rarity list to update. This can be 1 (common), 2 (rare), or 3 (deluxe).</summary>
        [Optional] public int Rarity { get; set; } = 1;
    }
    /// <summary>The metadata for a movie that can play at the movie theater.</summary>
    [RType]
    public class MovieData {
        /// <summary>A key which uniquely identifies this movie. This should only contain alphanumeric/underscore/dot characters. For custom movies, this should be prefixed with your mod ID like <c>Example.ModId_MovieName</c>.</summary>
        [Optional] public string Id;
        /// <summary>The seasons when the movie plays, or none to allow any season.</summary>
        [Optional] public List<Season> Seasons;
        /// <summary>If set, the movie is available when <c>{year} % <see cref="F:StardewValley.GameData.Movies.MovieData.YearModulus" /> == <see cref="F:StardewValley.GameData.Movies.MovieData.YearRemainder" /></c> (where <c>{year}</c> is the number of years since the movie theater was built and {remainder} defaults to zero). For example, a modulus of 2 with remainder 1 is shown in the second year and every other year thereafter.</summary>
        [Optional] public int? YearModulus;
        /// <inheritdoc cref="F:StardewValley.GameData.Movies.MovieData.YearModulus" />
        [Optional] public int? YearRemainder;
        /// <summary>The asset name for the movie poster and screen images, or <c>null</c> to use <c>LooseSprites\Movies</c>.</summary>
        /// <remarks>This must be a spritesheet with one 490�128 pixel row per movie. A 13�19 area in the top-left corner of the row should contain the movie poster. With a 16-pixel offset from the left edge, there should be two rows of five 90�61 pixel movie screen images, with a six-pixel gap between each image. (The movie doesn't need to use all of the image slots.)</remarks>
        [Optional] public string Texture;
        /// <summary>The sprite index within the <see cref="F:StardewValley.GameData.Movies.MovieData.Texture" /> for this movie poster and screen images.</summary>
        public int SheetIndex;
        /// <summary>A tokenizable string for the translated movie title.</summary>
        public string Title;
        /// <summary>A tokenizable string for the translated movie description, shown when interacting with the movie poster.</summary>
        public string Description;
        /// <summary>A list of tags which describe the genre or other metadata, which can be matched by <see cref="F:StardewValley.GameData.Movies.MovieReaction.Tag" />.</summary>
        [Optional] public List<string> Tags;
        /// <summary>The prizes that can be grabbed in the crane game while this movie is playing (in addition to the default items).</summary>
        [Optional] public List<MovieCranePrizeData> CranePrizes = [];
        /// <summary>The prize rarity lists whose default items to clear when this movie is playing, so they're only taken from <see cref="F:StardewValley.GameData.Movies.MovieData.CranePrizes" />.</summary>
        [Optional] public List<int> ClearDefaultCranePrizeGroups = [];
        /// <summary>The scenes to show when watching the movie.</summary>
        public List<MovieScene> Scenes;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Movies.MovieCharacterReaction" />, a possible reactions to movies matching a tag.</summary>
    [RType]
    public class MovieReaction {
        /// <summary>
        ///   <para>A pattern which determines which movies this reaction can apply to.</para>
        ///   <para>This can be any of the following:</para>
        ///   <list type="bullet">
        ///     <item><description><c>"*"</c> to match any movie.</description></item>
        ///     <item><description>A tag to match any movie which has that tag in its <see cref="F:StardewValley.GameData.Movies.MovieData.Tags" /> list.</description></item>
        ///     <item><description>An ID to match any movie with that <see cref="F:StardewValley.GameData.Movies.MovieData.Id" /> value.</description></item>
        ///     <item><description>How much the NPC enjoys this movie, based on the <see cref="F:StardewValley.GameData.Movies.MovieReaction.Response" /> for matched entries. This performs a two-pass check: any <see cref="T:StardewValley.GameData.Movies.MovieReaction" /> entry which matches with a non-response <see cref="F:StardewValley.GameData.Movies.MovieReaction.Tag" /> is used to determine the NPC's response, defaulting to <c>like</c>. The result is then checked against this value.</description></item>
        ///   </list>
        /// </summary>
        public string Tag;
        /// <summary>How much the NPC enjoys the movie (one of <c>love</c>, <c>like</c>, or <c>dislike</c>).</summary>
        [Optional] public string Response = "like";
        /// <summary>A list of internal NPC names. If this isn't empty, at least one of these NPCs must be present in the theater for this reaction to apply.</summary>
        [Optional] public List<string> Whitelist = [];
        /// <summary>If set, possible dialogue from the NPC during the movie.</summary>
        [Optional] public SpecialResponses SpecialResponses;
        /// <summary>A key which uniquely identifies this movie reaction. This should only contain alphanumeric/underscore/dot characters. For custom movie reactions, this should be prefixed with your mod ID like <c>Example.ModId_ReactionName</c>.</summary>
        public string Id = "";
        /// <summary>Whether this movie reaction should apply to a given movie.</summary>
        /// <param name="movieData">The movie data to match.</param>
        /// <param name="moviePatrons">The internal names for NPCs watching the movie.</param>
        /// <param name="otherValidTags">The other tags to match via <see cref="F:StardewValley.GameData.Movies.MovieReaction.Tag" />.</param>
        public bool ShouldApplyToMovie(MovieData movieData, IEnumerable<string> moviePatrons, params string[] otherValidTags) {
            if (Whitelist != null) {
                if (moviePatrons == null) return false;
                foreach (var str in Whitelist) if (!moviePatrons.Contains(str)) return false;
            }
            return Tag == movieData.Id || movieData.Tags.Contains(Tag) || Tag == "*" || otherValidTags.Contains(Tag);
        }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Movies.MovieData" />, a scene to show when watching the movie.</summary>
    [RType]
    public class MovieScene {
        /// <summary>The screen index within the movie's spritesheet row.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Movies.MovieData.SheetIndex" /> for the expected sprite layout.</remarks>
        [Optional] public int Image = -1;
        /// <summary>If set, the audio cue ID for the music to play while the scene is shown. Default none.</summary>
        [Optional] public string Music;
        /// <summary>If set, the audio cue ID for a sound effect to play when the scene starts. Default none.</summary>
        [Optional] public string Sound;
        /// <summary>The number of milliseconds to wait after the scene starts before showing the <see cref="F:StardewValley.GameData.Movies.MovieScene.Text" />, <see cref="F:StardewValley.GameData.Movies.MovieScene.Script" />, and <see cref="F:StardewValley.GameData.Movies.MovieScene.Image" />.</summary>
        [Optional] public int MessageDelay = 500;
        /// <summary>If set, a tokenizable string for the custom event script to run for any custom audio, images, etc.</summary>
        [Optional] public string Script;
        /// <summary>If set, a tokenizable string for the text to show in a message box while the scene plays. The scene will pause until the player closes it.</summary>
        [Optional] public string Text;
        /// <summary>Whether to shake the movie screen image for the duration of the scene.</summary>
        [Optional] public bool Shake;
        /// <summary>If set, an optional hook where NPCs may interject a reaction dialogue via <see cref="F:StardewValley.GameData.Movies.CharacterResponse.ResponsePoint" />.</summary>
        [Optional] public string ResponsePoint;
        /// <summary>A key which uniquely identifies this movie scene. This should only contain alphanumeric/underscore/dot characters. For custom movie scenes, this should be prefixed with your mod ID like <c>Example.ModId_MovieScene</c>.</summary>
        public string Id;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Movies.MovieReaction" />, possible dialogue from the NPC during the movie.</summary>
    [RType]
    public class SpecialResponses {
        /// <summary>The dialogue to show when the player interacts with the NPC in the theater lobby before the movie starts, if any.</summary>
        [Optional] public CharacterResponse BeforeMovie;
        /// <summary>The dialogue to show during the movie based on the <see cref="F:StardewValley.GameData.Movies.CharacterResponse.ResponsePoint" />, if any.</summary>
        [Optional] public CharacterResponse DuringMovie;
        /// <summary>The dialogue to show when the player interacts with the NPC in the theater lobby after the movie ends, if any.</summary>
        [Optional] public CharacterResponse AfterMovie;
    }
}

public class Museum {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Museum.MuseumRewards" />, an item that must be donated to complete this reward group.</summary>
    [RType]
    public class MuseumDonationRequirement {
        /// <summary>The context tag for the items to require.</summary>
        public string Tag;
        /// <summary>The minimum number of items matching the <see cref="F:StardewValley.GameData.Museum.MuseumDonationRequirement.Tag" /> that must be donated.</summary>
        public int Count;
    }
    /// <summary>The data for a set of artifacts that can be donated to the museum, and the resulting reward.</summary>
    [RType]
    public class MuseumRewards {
        /// <summary>
        ///   <para>The items that must be donated to complete this reward group. The player must fulfill every entry in the list to unlock the reward. For example, an entry with the tag <c>forage_item</c> and count 2 will require donating any two forage items.</para>
        ///   <para>Special case: an entry with the exact values <c>Tag: "", Count: -1</c> passes if the museum is complete (i.e. the player has donated the max number of items). </para>
        /// </summary>
        public List<MuseumDonationRequirement> TargetContextTags;
        /// <summary>The qualified item ID for the item given to the player when they donate all required items for this group. There's no reward item if omitted.</summary>
        [Optional] public string RewardItemId;
        /// <summary>The stack size for the <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId" /> item (if the item supports stacking).</summary>
        [Optional] public int RewardItemCount = 1;
        /// <summary>Whether to mark the <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId" /> item as a special permanent item, which can't be destroyed/dropped and can only be collected once.</summary>
        [Optional] public bool RewardItemIsSpecial;
        /// <summary>Whether to give the player a cooking/crafting recipe which produces the <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId" /> item, instead of the item itself. Ignored if the item type can't be cooked/crafted (i.e. non-object-type items).</summary>
        [Optional] public bool RewardItemIsRecipe;
        /// <summary>The actions to perform when the reward is collected. For example, this is used for the rusty key unlock at 60 donations.</summary>
        [Optional] public List<string> RewardActions;
        /// <summary>Whether to add the ID value to the player's received mail. This is used to track whether the player has collected the reward, and should almost always be true. If this and <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemIsSpecial" /> are both false, the player will be able to collect the reward infinite times.</summary>
        [Optional] public bool FlagOnCompletion;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
}

public class Objects {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Objects.ObjectData" />, a buff to set when this item is eaten.</summary>
    [RType]
    public class ObjectBuffData {
        /// <summary>The backing field for <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.Id" />.</summary>
        string _idImpl;
        [Optional] public string Id { get => _idImpl ?? BuffId; set => _idImpl = value; }
        /// <summary>The buff ID to apply, or <c>null</c> to use <c>food</c> or <c>drink</c> depending on the item data.</summary>
        [Optional] public string BuffId { get; set; }
        /// <summary>The texture to load for the buff icon, or <c>null</c> for the default icon based on the <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.BuffId" /> and <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.CustomAttributes" />.</summary>
        [Optional] public string IconTexture { get; set; }
        /// <summary>The sprite index for the buff icon within the <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.IconTexture" />.</summary>
        [Optional] public int IconSpriteIndex { get; set; }
        /// <summary>The buff duration measured in in-game minutes, or <c>-2</c> for a buff that should last all day, or (if <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.BuffId" /> is set) omit it to use the duration in <c>Data/Buffs</c>.</summary>
        [Optional] public int Duration { get; set; }
        /// <summary>Whether this buff counts as a debuff, so its duration should be halved when wearing a Sturdy Ring.</summary>
        [Optional] public bool IsDebuff { get; set; }
        /// <summary>The glow color to apply to the player, if any.</summary>
        [Optional] public string GlowColor { get; set; }
        /// <summary>The custom buff attributes to apply, if any.</summary>
        [Optional] public Buffs.BuffAttributesData CustomAttributes { get; set; }
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields { get; set; }
    }
    /// <summary>The data for an object-type item.</summary>
    [RType]
    public class ObjectData {
        /// <summary>The internal item name.</summary>
        public string Name;
        /// <summary>A tokenizable string for the item's translated display name.</summary>
        public string DisplayName;
        /// <summary>A tokenizable string for the item's translated description.</summary>
        public string Description;
        /// <summary>The item's general type, like <c>Arch</c> (artifact) or <c>Minerals</c>.</summary>
        public string Type;
        /// <summary>The item category, usually matching a constant like <c>Object.flowersCategory</c>.</summary>
        public int Category;
        /// <summary>The price when sold by the player. This is not the price when bought from a shop.</summary>
        [Optional] public int Price;
        /// <summary>The asset name for the texture containing the item's sprite, or <c>null</c> for <c>Maps/springobjects</c>.</summary>
        [Optional] public string Texture;
        /// <summary>The sprite's index in the spritesheet.</summary>
        public int SpriteIndex;
        /// <summary>When drawn as a colored object, whether to apply the color to the next sprite in the spritesheet and draw that over the main sprite. If false, the color is applied to the main sprite instead.</summary>
        [Optional] public bool ColorOverlayFromNextIndex;
        /// <summary>A numeric value that determines how much energy (edibility � 2.5) and health (edibility � 1.125) is restored when this item is eaten. An item with an edibility of -300 can't be eaten, values from -299 to -1 reduce health and energy, and zero can be eaten but doesn't change health/energy.</summary>
        /// <remarks>This is ignored for rings.</remarks>
        [Optional] public int Edibility = -300;
        /// <summary>Whether to drink the item instead of eating it.</summary>
        /// <remarks>Ignored if the item isn't edible per <see cref="F:StardewValley.GameData.Objects.ObjectData.Edibility" />.</remarks>
        [Optional] public bool IsDrink;
        /// <summary>The buffs to apply to the player when this item is eaten, if any.</summary>
        /// <remarks>Ignored if the item isn't edible per <see cref="F:StardewValley.GameData.Objects.ObjectData.Edibility" />.</remarks>
        [Optional] public List<ObjectBuffData> Buffs;
        /// <summary>If set, the item will drop a default item when broken as a geode. If <see cref="F:StardewValley.GameData.Objects.ObjectData.GeodeDrops" /> is set too, there's a 50% chance of choosing a value from that list instead.</summary>
        [Optional] public bool GeodeDropsDefaultItems;
        /// <summary>The items that can be dropped when this item is broken open as a geode.</summary>
        [Optional] public List<ObjectGeodeDropData> GeodeDrops;
        /// <summary>If this is an artifact (i.e. <see cref="F:StardewValley.GameData.Objects.ObjectData.Type" /> is <c>Arch</c>), the chance that it can be found by digging artifact spots in each location.</summary>
        [Optional] public Dictionary<string, float> ArtifactSpotChances;
        /// <summary>Whether this item can be given to NPCs as a gift by default.</summary>
        /// <remarks>This doesn't override non-gift behavior (e.g. receiving quest items) or specific exclusions (e.g. only Pierre will accept Pierre's Missing Stocklist).</remarks>
        [Optional] public bool CanBeGivenAsGift = true;
        /// <summary>Whether this item can be trashed by players by default.</summary>
        /// <remarks>This doesn't override specific exclusions (e.g. quest items can't be trashed).</remarks>
        [Optional] public bool CanBeTrashed = true;
        /// <summary>Whether to exclude this item from the fishing collection and perfection score.</summary>
        [Optional] public bool ExcludeFromFishingCollection;
        /// <summary>Whether to exclude this item from the shipping collection and perfection score.</summary>
        [Optional] public bool ExcludeFromShippingCollection;
        /// <summary>Whether to exclude this item from shops when selecting random items to sell.</summary>
        [Optional] public bool ExcludeFromRandomSale;
        /// <summary>The custom context tags to add for this item (in addition to the tags added automatically based on the other object data).</summary>
        [Optional] public List<string> ContextTags;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Objects.ObjectData" />, an item that can be found by breaking the item as a geode.</summary>
    /// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
    [RType]
    public class ObjectGeodeDropData : GenericSpawnItemDataWithCondition {
        /// <summary>A probability that this item will be found, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public double Chance { get; set; } = 1.0;
        /// <summary>The mail flag to set for the current player when this item is picked up by the player.</summary>
        [Optional] public string SetFlagOnPickup { get; set; }
        /// <summary>The order in which this drop should be checked, where 0 is the default value used by most drops. Drops within each precedence group are checked in the order listed.</summary>
        [Optional] public int Precedence { get; set; }
    }
}

public class Pants {
    /// <summary>The metadata for a pants item that can be equipped by players.</summary>
    [RType]
    public class PantsData {
        /// <summary>The pants' internal name.</summary>
        public string Name = "Pants";
        /// <summary>A tokenizable string for the pants' display name.</summary>
        public string DisplayName = "[LocalizedText Strings\\Pants:Pants_Name]";
        /// <summary>A tokenizable string for the pants' description.</summary>
        public string Description = "[LocalizedText Strings\\Pants:Pants_Description]";
        /// <summary>The price when purchased from shops.</summary>
        [Optional] public int Price = 50;
        /// <summary>The asset name for the texture containing the pants' sprite, or <c>null</c> for <c>Characters/Farmer/pants</c>.</summary>
        [Optional] public string Texture;
        /// <summary>The sprite's index in the spritesheet.</summary>
        public int SpriteIndex;
        /// <summary>The default pants color.</summary>
        [Optional] public string DefaultColor = "255 235 203";
        /// <summary>Whether the pants can be dyed.</summary>
        [Optional] public bool CanBeDyed;
        /// <summary>Whether the pants continuously shift colors. This overrides <see cref="F:StardewValley.GameData.Pants.PantsData.DefaultColor" /> and <see cref="F:StardewValley.GameData.Pants.PantsData.CanBeDyed" /> if set.</summary>
        [Optional] public bool IsPrismatic;
        /// <summary>Whether the pants can be selected on the customization screen.</summary>
        [Optional] public bool CanChooseDuringCharacterCustomization;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
}

public class Pets {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetBehavior" />, the animation frames to play while the state is active.</summary>
    [RType]
    public class PetAnimationFrame {
        /// <summary>The frame index in the animation. This should be an incremental number starting at 0.</summary>
        public int Frame;
        /// <summary>The millisecond duration for which the frame should be kept on-screen before continuing to the next frame.</summary>
        public int Duration;
        /// <summary>Whether to play the footstep sound for the tile under the pet when the frame starts.</summary>
        [Optional] public bool HitGround;
        /// <summary>Whether the pet should perform a small hop when the frame starts, including a 'dwop' sound.</summary>
        [Optional] public bool Jump;
        /// <summary>The audio cue ID for the sound to play when the animation starts or loops. If set to the exact string <c>BARK</c>, the <see cref="F:StardewValley.GameData.Pets.PetData.BarkSound" /> or <see cref="F:StardewValley.GameData.Pets.PetBreed.BarkOverride" /> is used. Defaults to none.</summary>
        [Optional] public string Sound;
        /// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetAnimationFrame.Sound" /> is only audible if the pet is within this many tiles past the border of the screen. Default -1 (no distance check).</summary>
        [Optional] public int SoundRangeFromBorder = -1;
        /// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetAnimationFrame.Sound" /> is only audible if the pet is within this many tiles of the player. Default -1 (no distance check).</summary>
        [Optional] public int SoundRange = -1;
        /// <summary>Whether to mute the <see cref="F:StardewValley.GameData.Pets.PetAnimationFrame.Sound" /> when the 'mute animal sounds' option is set.</summary>
        [Optional] public bool SoundIsVoice;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetBehavior" />, what to do when the last animation frame is reached while the behavior is still active.</summary>
    [RType]
    public enum PetAnimationLoopMode {
        /// <summary>Equivalent to <see cref="F:StardewValley.GameData.Pets.PetAnimationLoopMode.Loop" />.</summary>
        None,
        /// <summary>Restart the animation from the first frame.</summary>
        Loop,
        /// <summary>Keep the last frame visible until the animation ends.</summary>
        Hold,
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetData" />, a state in the pet's possible actions and behaviors.</summary>
    [RType]
    public class PetBehavior {
        /// <summary>A unique string ID for the state. This only needs to be unique within the pet type (e.g. cats and dogs can have different behaviors with the same name).</summary>
        public string Id;
        /// <summary>Whether to constrain the pet's facing direction to left and right while the state is active.</summary>
        [Optional] public bool IsSideBehavior;
        /// <summary>Whether to point the pet in a random direction at the start of this state. If set, this overrides <see cref="F:StardewValley.GameData.Pets.PetBehavior.Direction" />.</summary>
        [Optional] public bool RandomizeDirection;
        /// <summary>The specific direction to face at the start of this state (one of <c>left</c>, <c>right</c>, <c>up</c>, or <c>down</c>), unless overridden by <see cref="F:StardewValley.GameData.Pets.PetBehavior.RandomizeDirection" />.</summary>
        [Optional] public string Direction;
        /// <summary>Whether to walk in the pet's facing direction.</summary>
        [Optional] public bool WalkInDirection;
        /// <summary>Overrides the pet's <see cref="F:StardewValley.GameData.Pets.PetData.MoveSpeed" /> while this state is active, or <c>-1</c> to inherit it.</summary>
        [Optional] public int MoveSpeed = -1;
        /// <summary>The audio cue ID for the sound to play when the state starts. If set to the exact string <c>BARK</c>, the <see cref="F:StardewValley.GameData.Pets.PetData.BarkSound" /> or <see cref="F:StardewValley.GameData.Pets.PetBreed.BarkOverride" /> is used. Defaults to none.</summary>
        [Optional] public string SoundOnStart;
        /// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart" /> is only audible if the pet is within this many tiles past the border of the screen. Default -1 (no distance check).</summary>
        [Optional] public int SoundRangeFromBorder = -1;
        /// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart" /> is only audible if the pet is within this many tiles of the player. Default -1 (no distance check).</summary>
        [Optional] public int SoundRange = -1;
        /// <summary>Whether to mute the <see cref="F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart" /> when the 'mute animal sounds' option is set.</summary>
        [Optional] public bool SoundIsVoice;
        /// <summary>The millisecond duration for which to shake the pet when the state starts.</summary>
        [Optional] public int Shake;
        /// <summary>The animation frames to play while this state is active.</summary>
        [Optional] public List<PetAnimationFrame> Animation;
        /// <summary>What to do when the last animation frame is reached while the behavior is still active.</summary>
        [Optional] public PetAnimationLoopMode LoopMode;
        /// <summary>The minimum number of times to play the animation, or <c>-1</c> to disable repeating the animation.</summary>
        /// <remarks>Both <see cref="F:StardewValley.GameData.Pets.PetBehavior.AnimationMinimumLoops" /> and <see cref="F:StardewValley.GameData.Pets.PetBehavior.AnimationMaximumLoops" /> must be set to have any effect. The game will choose an inclusive random value between them.</remarks>
        [Optional] public int AnimationMinimumLoops = -1;
        /// <summary>The maximum number of times to play the animation, or <c>-1</c> to disable repeating the animation.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehavior.AnimationMinimumLoops" />.</remarks>
        [Optional] public int AnimationMaximumLoops = -1;
        /// <summary>The possible behavior transitions to start when the current behavior's animation ends. If multiple transitions are listed, one is selected at random.</summary>
        [Optional] public List<PetBehaviorChanges> AnimationEndBehaviorChanges;
        /// <summary>The millisecond duration until the pet transitions to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges" /> field, if set. This overrides <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" /> and <see cref="F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration" />.</summary>
        [Optional] public int Duration = -1;
        /// <summary>The minimum millisecond duration until the pet transitions to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges" /> field, if set. This is ignored if <see cref="F:StardewValley.GameData.Pets.PetBehavior.Duration" /> is set.</summary>
        /// <remarks>Both <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" /> and <see cref="F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration" /> must have a non-negative value to take effect.</remarks>
        [Optional] public int MinimumDuration = -1;
        /// <summary>The maximum millisecond duration until the pet transitions to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges" /> field, if set. This is ignored if <see cref="F:StardewValley.GameData.Pets.PetBehavior.Duration" /> is set.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" />.</remarks>
        [Optional] public int MaximumDuration = -1;
        /// <summary>The possible behavior transitions to start when the <see cref="F:StardewValley.GameData.Pets.PetBehavior.Duration" /> or <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" /> + <see cref="F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration" /> values are reached. If multiple transitions are listed, one is selected at random.</summary>
        [Optional] public List<PetBehaviorChanges> TimeoutBehaviorChanges;
        /// <summary>The possible behavior transitions to start when the player is within two tiles of the pet. If multiple transitions are listed, one is selected at random.</summary>
        [Optional] public List<PetBehaviorChanges> PlayerNearbyBehaviorChanges;
        /// <summary>The probability at the start of each frame that the pet will transition to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.RandomBehaviorChanges" /> field, if set. Specified as a value between 0 (never) and 1 (always).</summary>
        [Optional] public float RandomBehaviorChangeChance;
        /// <summary>The possible behavior transitions to start, based on a <see cref="F:StardewValley.GameData.Pets.PetBehavior.RandomBehaviorChangeChance" /> check at the start of each frame. If multiple transitions are listed, one is selected at random.</summary>
        [Optional] public List<PetBehaviorChanges> RandomBehaviorChanges;
        /// <summary>The possible behavior transitions to start when the pet lands after jumping. If multiple transitions are listed, one is selected at random.</summary>
        [Optional] public List<PetBehaviorChanges> JumpLandBehaviorChanges;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetBehavior" />, a possible behavior transition that can be started.</summary>
    [RType]
    public class PetBehaviorChanges {
        /// <summary>The option's weight when randomly choosing a behavior, relative to other behaviors in the list (e.g. 2 is twice as likely as 1).</summary>
        [Optional] public float Weight = 1f;
        /// <summary>Whether the transition can only happen if the pet is outside.</summary>
        [Optional] public bool OutsideOnly;
        /// <summary>The name of the behavior to start if the pet is facing up.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
        [Optional] public string UpBehavior;
        /// <summary>The name of the behavior to start if the pet is facing down.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
        [Optional] public string DownBehavior;
        /// <summary>The name of the behavior to start if the pet is facing left.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
        [Optional] public string LeftBehavior;
        /// <summary>The name of the behavior to start if the pet is facing right.</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
        [Optional] public string RightBehavior;
        /// <summary>The name of the behavior to start, if no directional behavior applies.</summary>
        /// <remarks>The pet will check for a behavior matching its facing direction first (like <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.UpBehavior" />), then try the <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />. If none are specified, the current behavior will continue unchanged.</remarks>
        [Optional] public string Behavior;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetData" />, a cosmetic breed which can be selected in the character customization menu when creating a save.</summary>
    [RType]
    public class PetBreed {
        /// <summary>A key which uniquely identifies the pet breed. The ID should only contain alphanumeric/underscore/dot characters. For custom breeds, this should be prefixed with your mod ID like <c>Example.ModId_BreedName.</c></summary>
        public string Id;
        /// <summary>The asset name for the breed spritesheet for the pet's in-game sprite. This should be 128 pixels wide, and 256 (cat) or 288 (dog) pixels high.</summary>
        public string Texture;
        /// <summary>The asset name for the breed icon texture, shown on the character customization screen and in-game menu. This should be a 16x16 pixel icon.</summary>
        public string IconTexture;
        /// <summary>The icon's pixel area within the <see cref="F:StardewValley.GameData.Pets.PetBreed.IconTexture" />.</summary>
        public Rectangle IconSourceRect = Rectangle.Empty;
        /// <summary>Whether this pet can be chosen as a starter pet at character creation</summary>
        [Optional] public bool CanBeChosenAtStart = true;
        /// <summary>Whether this pet can be adopted from Marnie once she starts offering pets.</summary>
        [Optional] public bool CanBeAdoptedFromMarnie = true;
        /// <summary>The price this pet costs in Marnie's shop</summary>
        [Optional] public int AdoptionPrice = 40000;
        /// <summary>Overrides the pet's <see cref="F:StardewValley.GameData.Pets.PetData.BarkSound" /> field for this breed, if set.</summary>
        [Optional] public string BarkOverride;
        /// <summary>The pitch applied to the pet's bark sound, measured as a decimal value relative to 1.</summary>
        [Optional] public float VoicePitch = 1f;
    }
    /// <summary>The metadata for a pet type that can be selected by the player.</summary>
    [RType]
    public class PetData {
        /// <summary>A tokenizable string for the pet type's display name (like "cat"), which can be used in dialogue.</summary>
        public string DisplayName;
        /// <summary>The cue ID for the pet's occasional 'bark' sound.</summary>
        public string BarkSound;
        /// <summary>The cue ID for the sound which the pet makes when you pet it.</summary>
        public string ContentSound;
        /// <summary>The number of milliseconds until the ContentSound is repeated once. This is used by the dog, who pants twice when pet. Defaults to disabled.</summary>
        [Optional] public int RepeatContentSoundAfter = -1;
        /// <summary>A pixel offset to apply to the emote position over the pet sprite.</summary>
        [Optional] public Point EmoteOffset;
        /// <summary>The pixel offset for the pet when shown in events like Marnie's adoption event.</summary>
        [Optional] public Point EventOffset;
        /// <summary>The location containing the event which lets the player adopt this pet, if they've selected it as their preferred type.</summary>
        [Optional] public string AdoptionEventLocation = "Farm";
        /// <summary>The event ID in the <see cref="F:StardewValley.GameData.Pets.PetData.AdoptionEventLocation" /> which lets the player adopt this pet, if they've selected it as their preferred type.</summary>
        /// <remarks>If set, this forces the event to play after 20 days if the event's preconditions haven't been met yet.</remarks>
        [Optional] public string AdoptionEventId;
        /// <summary>How to render the pet during the summit perfection slide-show.</summary>
        /// <remarks>If this isn't set, the pet won't be shown in the slide-show.</remarks>
        public PetSummitPerfectionEventData SummitPerfectionEvent;
        /// <summary>How quickly the pet can move.</summary>
        [Optional] public int MoveSpeed = 2;
        /// <summary>The percentage chance that the pet sleeps on the player's bed at night, as a decimal value between 0 (never) and 1 (always).</summary>
        /// <remarks>The chances are checked in this order: <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnBedChance" />, <see cref="F:StardewValley.GameData.Pets.PetData.SleepNearBedChance" />, and <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnRugChance" />. The first match is used. If none match, the pet will choose a random empty spot in the farmhouse; if there's no empty spot, it'll sleep next to its pet bowl outside.</remarks>
        [Optional] public float SleepOnBedChance = 0.05f;
        /// <summary>The percentage chance that the pet sleeps at the foot of the player's bed at night, as a decimal value between 0 (never) and 1 (always).</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnBedChance" />.</remarks>
        [Optional] public float SleepNearBedChance = 0.3f;
        /// <summary>The percentage chance that the pet sleeps on a random rug at night, as a decimal value between 0 (never) and 1 (always).</summary>
        /// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnBedChance" />.</remarks>
        [Optional] public float SleepOnRugChance = 0.5f;
        /// <summary>The pet's possible actions and behaviors, defined as the states in a state machine. Essentially the pet will be in one state at any given time, which also determines which state they can transition to next. For example, a cat can transition from <c>Walk </c>to <c>BeginSitDown</c>, but it can't skip instantly from <c>Walk</c> to <c>SitDownLick</c>.</summary>
        public List<PetBehavior> Behaviors;
        /// <summary>The percentage chance that the pet will try to give a gift when pet each day.</summary>
        [Optional] public float GiftChance = 0.2f;
        /// <summary>The list of gifts that this pet can give if the gift chance roll is successful, chosen by weight similar to the pet behaviors.</summary>
        [Optional] public List<PetGift> Gifts = [];
        /// <summary>The cosmetic breeds which can be selected in the character customization menu when creating a save.</summary>
        public List<PetBreed> Breeds;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
        /// <summary>Get the breed from <see cref="F:StardewValley.GameData.Pets.PetData.Breeds" /> to use for a given ID.</summary>
        /// <param name="breedId">The preferred pet breed ID.</param>
        /// <param name="allowNull">Whether to return null if the ID isn't found. If false, default to the first breed in the list instead.</param>
        public PetBreed GetBreedById(string breedId, bool allowNull = false) {
            foreach (var breed in Breeds) if (breed.Id == breedId) return breed;
            return !allowNull ? Breeds[0] : null;
        }
    }
    /// <summary>The item spawn info for a pet gift.</summary>
    [RType]
    public class PetGift : GameData.GenericSpawnItemDataWithCondition {
        /// <summary>The friendship level that this pet must be at before it can give this gift. Defaults to 1000 (max friendship)</summary>
        [Optional] public int MinimumFriendshipThreshold { get; set; } = 1000;
        /// <summary>The item's weight when randomly choosing a item, relative to other items in the list (e.g. 2 is twice as likely as 1).</summary>
        [Optional] public float Weight { get; set; } = 1f;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetData" />, how to render the pet during the summit perfection slide-show.</summary>
    [RType]
    public class PetSummitPerfectionEventData {
        /// <summary>The source rectangle within the texture to draw.</summary>
        public Rectangle SourceRect;
        /// <summary>The number of frames to show starting from the <see cref="F:StardewValley.GameData.Pets.PetSummitPerfectionEventData.SourceRect" />.</summary>
        public int AnimationLength;
        /// <summary>Whether to flip the pet sprite left-to-right.</summary>
        [Optional] public bool Flipped;
        /// <summary>The motion to apply to the pet sprite.</summary>
        public Vector2 Motion;
        /// <summary>Whether to apply the 'ping pong' effect to the pet sprite animation.</summary>
        [Optional] public bool PingPong;
    }
}

public class Powers {
    /// <summary>The content data for powers in the powers sub menu.</summary>
    [RType]
    public class PowersData {
        /// <summary>A tokenizable string for the power's display name.</summary>
        public string DisplayName;
        /// <summary>A tokenizable string for the power's description.</summary>
        [Optional] public string Description = "";
        /// <summary>The asset name for the power's icon texture.</summary>
        public string TexturePath;
        /// <summary>The top-left pixel coordinate of the 16x16 sprite icon to show in the powers menu.</summary>
        public Point TexturePosition;
        /// <summary>If set, a game state query which indicates whether the power has been unlocked. Defaults to always unlocked.</summary>
        public string UnlockedCondition;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, object> CustomFields;
    }
}

public class Shirts {
    /// <summary>The metadata for a shirt item that can be equipped by players.</summary>
    [RType]
    public class ShirtData {
        /// <summary>The shirt's internal name.</summary>
        [Optional] public string Name = "Shirt";
        /// <summary>A tokenizable string for the shirt's display name.</summary>
        [Optional] public string DisplayName = "[LocalizedText Strings\\Shirts:Shirt_Name]";
        /// <summary>A tokenizable string for the shirt's description.</summary>
        [Optional] public string Description = "[LocalizedText Strings\\Shirts:Shirt_Description]";
        /// <summary>The price when purchased from shops.</summary>
        [Optional] public int Price = 50;
        /// <summary>The asset name for the texture containing the shirt's sprite, or <c>null</c> for <c>Characters/Farmer/shirts</c>.</summary>
        [Optional] public string Texture;
        /// <summary>The sprite's index in the spritesheet.</summary>
        public int SpriteIndex;
        /// <summary>The default shirt color.</summary>
        [Optional] public string DefaultColor;
        /// <summary>Whether the shirt can be dyed.</summary>
        [Optional] public bool CanBeDyed;
        /// <summary>Whether the shirt continuously shift colors. This overrides <see cref="F:StardewValley.GameData.Shirts.ShirtData.DefaultColor" /> and <see cref="F:StardewValley.GameData.Shirts.ShirtData.CanBeDyed" /> if set.</summary>
        [Optional] public bool IsPrismatic;
        /// <summary>Whether the shirt has sleeves.</summary>
        [Optional] public bool HasSleeves = true;
        /// <summary>Whether the shirt can be selected on the customization screen.</summary>
        [Optional] public bool CanChooseDuringCharacterCustomization;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
}

public class Shops {
    /// <summary>How a shop stock limit is applied in multiplayer.</summary>
    [RType]
    public enum LimitedStockMode {
        /// <summary>The limit applies to every player in the world. For example, if limited to one and a player bought it, no other players can buy one.</summary>
        Global,
        /// <summary>Each player has a separate limit. For example, if limited to one, each player could buy one.</summary>
        Player,
        /// <summary>Ignore the limit. This is used for items that adjust their own stock via code (e.g. by checking mail).</summary>
        None,
    }
    /// <summary>Metadata for an in-game shop at which the player can buy and sell items.</summary>
    [RType]
    public class ShopData {
        /// <summary>The currency in which all items in the shop should be priced. The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems).</summary>
        /// <remarks>For item trading, see <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> instead.</remarks>
        [Optional] public int Currency;
        /// <summary>How to draw stack size numbers in the shop list by default.</summary>
        /// <remarks>This is overridden in some special cases (e.g. recipes never show a stack count).</remarks>
        [Optional] public Shops.StackSizeVisibility? StackSizeVisibility;
        /// <summary>The sound to play when the shop menu is opened.</summary>
        [Optional] public string OpenSound;
        /// <summary>The sound to play when an item is purchased normally.</summary>
        [Optional] public string PurchaseSound;
        /// <summary>The repeating sound to play when accumulating a stack to purchase (e.g. by holding right-click on PC).</summary>
        [Optional] public string PurchaseRepeatSound;
        /// <summary>The default value for <see cref="P:StardewValley.GameData.Shops.ShopItemData.ApplyProfitMargins" />, if set. This can be true (always apply it), false (never apply it), or null (apply to certain items like saplings). This is applied before any quantity modifiers. Default null.</summary>
        [Optional] public bool? ApplyProfitMargins;
        /// <summary>Changes to apply to the sell price for all items in the shop, unless <see cref="P:StardewValley.GameData.Shops.ShopItemData.IgnoreShopPriceModifiers" /> is <c>true</c>. These stack with <see cref="P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers" />.</summary>
        /// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the price will quadruple it).</remarks>
        [Optional] public List<QuantityModifier> PriceModifiers;
        /// <summary>How multiple <see cref="F:StardewValley.GameData.Shops.ShopData.PriceModifiers" /> should be combined. This only affects that specific field, it won't affect price modifiers under <see cref="F:StardewValley.GameData.Shops.ShopData.Items" />.</summary>
        [Optional] public QuantityModifier.QuantityModifierMode PriceModifierMode;
        /// <summary>The NPCs who can run the shop. If the <c>Action OpenShop</c> property specifies the <c>[owner tile area]</c> argument, at least one of the listed NPCs must be within that area; else if the <c>[owner tile area]</c> argument was omitted, the first entry in the list is used. The selected NPC's portrait will be shown in the shop UI.</summary>
        [Optional] public List<ShopOwnerData> Owners;
        /// <summary>The visual theme to apply to the shop UI, or <c>null</c> for the default theme.</summary>
        [Optional] public List<ShopThemeData> VisualTheme;
        /// <summary>A list of context tags for items which the player can sell to this shop. Default none.</summary>
        [Optional] public List<string> SalableItemTags;
        /// <summary>The items to add to the shop inventory.</summary>
        [Optional] public List<ShopItemData> Items = [];
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Shops.ShopOwnerData" />, a possible dialogue that can be shown in the shop UI.</summary>
    [RType]
    public class ShopDialogueData {
        /// <summary>An ID for this dialogue. This only needs to be unique within the current dialogue list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_DialogueName</c>.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether the dialogue should be available. Defaults to always available.</summary>
        [Optional] public string Condition;
        /// <summary>A tokenizable string for the dialogue text to show. The resulting text is parsed using the dialogue format.</summary>
        [Optional] public string Dialogue;
        /// <summary>A list of random dialogues to choose from, using the same format as <see cref="F:StardewValley.GameData.Shops.ShopDialogueData.Dialogue" />. If set, <see cref="F:StardewValley.GameData.Shops.ShopDialogueData.Dialogue" /> is ignored.</summary>
        [Optional] public List<string> RandomDialogue;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Shops.ShopData" />, an item to add to the shop inventory.</summary>
    [RType]
    public class ShopItemData : GameData.GenericSpawnItemDataWithCondition {
        /// <summary>The actions to perform when the item is purchased.</summary>
        [Optional] public List<string> ActionsOnPurchase;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
        /// <summary>The qualified or unqualified item ID which must be traded to purchase this item.</summary>
        /// <remarks>If both <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> and <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" /> are specified, the player will need to provide both to get the item.</remarks>
        [Optional] string TradeItemId { get; set; }
        /// <summary>The number of <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> needed to purchase this item.</summary>
        [Optional] public int TradeItemAmount { get; set; } = 1;
        /// <summary>The gold price to purchase the item from the shop. Defaults to the item's normal price, or zero if <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> is specified.</summary>
        /// <remarks>If both <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> and <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" /> are specified, the player will need to provide both to get the item.</remarks>
        [Optional] public int Price { get; set; } = -1;
        /// <summary>Whether to multiply the price by the game's profit margins, which reduces the price on easier difficulty settings. This can be true (always apply it), false (never apply it), or null (apply to certain items like saplings). This is applied before any quantity modifiers. Default null.</summary>
        [Optional] public bool? ApplyProfitMargins { get; set; }
        /// <summary>The number of times the item can be purchased in one day. Default unlimited.</summary>
        /// <remarks>If the stack is more than one (e.g. via <see cref="P:StardewValley.GameData.GenericSpawnItemData.MinStack" />), each purchase still counts as one. For example, a stock limit of 5 and a stack size of 10 means the player can purchase 5 sets of 10, for a total of 50 items.</remarks>
        [Optional] public int AvailableStock { get; set; } = -1;
        /// <summary>If <see cref="P:StardewValley.GameData.Shops.ShopItemData.AvailableStock" /> is set, how the limit is applied in multiplayer. This has no effect on recipes.</summary>
        [Optional] public LimitedStockMode AvailableStockLimit { get; set; }
        /// <summary>Whether to avoid adding this item to the shop if it would duplicate one that was already added. If the item is randomized, this will choose a value that hasn't already been added to the shop if possible.</summary>
        [Optional] public bool AvoidRepeat { get; set; }
        /// <summary>If this data produces an object and <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" /> is -1, whether to use the raw price in <c>Data/Objects</c> instead of the calculated sell-to-player price.</summary>
        [Optional] public bool UseObjectDataPrice { get; set; }
        /// <summary>Whether to ignore the <see cref="F:StardewValley.GameData.Shops.ShopData.PriceModifiers" /> for the shop. This has no effect on the item's <see cref="P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers" />. Default false.</summary>
        [Optional] public bool IgnoreShopPriceModifiers { get; set; }
        /// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" />. These stack with <see cref="F:StardewValley.GameData.Shops.ShopData.PriceModifiers" />.</summary>
        /// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the price will quadruple it).</remarks>
        [Optional] public List<QuantityModifier> PriceModifiers { get; set; }
        /// <summary>How multiple <see cref="P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers" /> should be combined.</summary>
        [Optional] public QuantityModifier.QuantityModifierMode PriceModifierMode { get; set; }
        /// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.Shops.ShopItemData.AvailableStock" />.</summary>
        /// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the available stock will quadruple it).</remarks>
        [Optional] public List<QuantityModifier> AvailableStockModifiers { get; set; }
        /// <summary>How multiple <see cref="P:StardewValley.GameData.Shops.ShopItemData.AvailableStockModifiers" /> should be combined.</summary>
        [Optional] public QuantityModifier.QuantityModifierMode AvailableStockModifierMode { get; set; }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Shops.ShopData" />, an NPC who can run the shop.</summary>
    [RType]
    public class ShopOwnerData {
        /// <summary>A game state query which indicates whether this owner entry is available. Defaults to always available.</summary>
        [Optional] public string Condition;
        /// <summary>The internal name of the NPC to show in the shop menu portrait, or the asset name of the portrait spritesheet to display, or an empty string to disable the portrait. Omit to use the NPC matched via <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Name" /> if any.</summary>
        [Optional] public string Portrait;
        /// <summary>The dialogues to show if this entry is selected. Each day one dialogue will be randomly chosen to show in the shop UI. Defaults to a generic dialogue (if this is <c>null</c>) or hides the dialogue (if this is set but none matched).</summary>
        [Optional] public List<ShopDialogueData> Dialogues;
        /// <summary>If <see cref="F:StardewValley.GameData.Shops.ShopOwnerData.Dialogues" /> has multiple matching entries, whether to re-randomize which one is selected each time the shop is opened (instead of once per day).</summary>
        [Optional] public bool RandomizeDialogueOnOpen = true;
        /// <summary>If set, a 'shop is closed'-style message to show instead of opening the shop.</summary>
        [Optional] public string ClosedMessage;
        /// <summary>An ID for this entry within the shop. This only needs to be unique within the current shop's owner list. Defaults to <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Name" />.</summary>
        /// <summary>The backing field for <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Id" />.</summary>
        string _idImpl;
        [Optional] public string Id { get => _idImpl ?? Name; set => _idImpl = value; }
        /// <summary>The backing field for <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Name" />.</summary>
        string _nameImpl;
        /// <summary>
        ///   One of...
        ///   <list type="bullet">
        ///     <item><description>the internal name for the NPC who must be in range to use this entry;</description></item>
        ///     <item><description><see cref="F:StardewValley.GameData.Shops.ShopOwnerType.AnyOrNone" /> to use this entry regardless of whether an NPC is in range;</description></item>
        ///     <item><description><see cref="F:StardewValley.GameData.Shops.ShopOwnerType.Any" /> to use this entry if any NPC is in range;</description></item>
        ///     <item><description><see cref="F:StardewValley.GameData.Shops.ShopOwnerType.None" /> to use this entry if no NPC is in range.</description></item>
        ///   </list>
        ///   This field is case-sensitive.
        /// </summary>
        public string Name {
            get => _nameImpl;
            set {
                if (Enum.TryParse(value, true, out ShopOwnerType result) && Enum.IsDefined(typeof(ShopOwnerType), result)) { _nameImpl = result.ToString(); Type = result; }
                else { _nameImpl = value; Type = ShopOwnerType.NamedNpc; }
            }
        }
        /// <summary>How this entry matches NPCs.</summary>
        [Ignore] public ShopOwnerType Type { get; private set; }
        /// <summary>Get whether an NPC name matches this entry.</summary>
        /// <param name="npcName">The NPC name to check.</param>
        public bool IsValid(string npcName) => Type switch {
            ShopOwnerType.Any => !string.IsNullOrWhiteSpace(npcName),
            ShopOwnerType.AnyOrNone => true,
            ShopOwnerType.None => string.IsNullOrWhiteSpace(npcName),
            _ => Name == npcName,
        };
    }
    /// <summary>Specifies how a shop owner entry matches NPCs.</summary>
    [RType]
    public enum ShopOwnerType {
        /// <summary>The entry matches an NPC whose name is the entry's name.</summary>
        NamedNpc,
        /// <summary>The entry matches any NPC.</summary>
        Any,
        /// <summary>The entry matches regardless of whether an NPC is present.</summary>
        AnyOrNone,
        /// <summary>The entry matches only if no NPC is present.</summary>
        None,
    }
    /// <summary>A visual theme to apply to the UI, or <c>null</c> for the default theme.</summary>
    [RType]
    public class ShopThemeData {
        /// <summary>A game state query which indicates whether this theme should be applied. Defaults to always applied.</summary>
        [Optional] public string Condition;
        /// <summary>The name of the texture to load for the shop window border, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string WindowBorderTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.WindowBorderTexture" /> for the shop window border, or <c>null</c> for the default shop texture. This should be an 18x18 pixel area.</summary>
        [Optional] public Rectangle? WindowBorderSourceRect;
        /// <summary>The name of the texture to load for the NPC portrait background, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string PortraitBackgroundTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.PortraitBackgroundTexture" /> for the NPC portrait background, or <c>null</c> for the default shop texture. This should be a 74x47 pixel area.</summary>
        [Optional] public Rectangle? PortraitBackgroundSourceRect;
        /// <summary>The name of the texture to load for the NPC dialogue background, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string DialogueBackgroundTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.DialogueBackgroundTexture" /> for the NPC dialogue background, or <c>null</c> for the default shop texture. This should be a 60x60 pixel area.</summary>
        [Optional] public Rectangle? DialogueBackgroundSourceRect;
        /// <summary>The sprite text color for the dialogue text, or <c>null</c> for the default color. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).</summary>
        [Optional] public string DialogueColor;
        /// <summary>The sprite text shadow color for the dialogue text shadow, or <c>null</c> for the default color.</summary>
        [Optional] public string DialogueShadowColor;
        /// <summary>The name of the texture to load for the item row background, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string ItemRowBackgroundTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ItemRowBackgroundTexture" /> for the item row background, or <c>null</c> for the default shop texture. This should be a 15x15 pixel area.</summary>
        [Optional] public Rectangle? ItemRowBackgroundSourceRect;
        /// <summary>The color tint to apply to the item row background when the cursor is hovering over it, or <c>White</c> for no tint, or <c>null</c> for the default color. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).</summary>
        [Optional] public string ItemRowBackgroundHoverColor;
        /// <summary>The sprite text color for the item text, or <c>null</c> for the default color. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).</summary>
        [Optional] public string ItemRowTextColor;
        /// <summary>The name of the texture to load for the box behind the item icons, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string ItemIconBackgroundTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ItemIconBackgroundTexture" /> for the item icon background, or <c>null</c> for the default shop texture. This should be an 18x18 pixel area.</summary>
        [Optional] public Rectangle? ItemIconBackgroundSourceRect;
        /// <summary>The name of the texture to load for the scroll up icon, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string ScrollUpTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollUpTexture" /> for the scroll up icon, or <c>null</c> for the default shop texture. This should be an 11x12 pixel area.</summary>
        [Optional] public Rectangle? ScrollUpSourceRect;
        /// <summary>The name of the texture to load for the scroll down icon, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string ScrollDownTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollDownTexture" /> for the scroll down icon, or <c>null</c> for the default shop texture. This should be an 11x12 pixel area.</summary>
        [Optional] public Rectangle? ScrollDownSourceRect;
        /// <summary>The name of the texture to load for the scrollbar foreground texture, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string ScrollBarFrontTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollBarFrontTexture" /> for the scroll foreground, or <c>null</c> for the default shop texture. This should be a 6x10 pixel area.</summary>
        [Optional] public Rectangle? ScrollBarFrontSourceRect;
        /// <summary>The name of the texture to load for the scrollbar background texture, or <c>null</c> for the default shop texture.</summary>
        [Optional] public string ScrollBarBackTexture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollBarBackTexture" /> for the scroll background, or <c>null</c> for the default shop texture. This should be a 6x6 pixel area.</summary>
        [Optional] public Rectangle? ScrollBarBackSourceRect;
    }
    /// <summary>How to draw stack size numbers in the shop list.</summary>
    [RType]
    public enum StackSizeVisibility {
        /// <summary>Always hide the stack size.</summary>
        Hide,
        /// <summary>Always draw the stack size.</summary>
        Show,
        /// <summary>Draw the stack size if more than one.</summary>
        ShowIfMultiple,
    }
}

public class SpecialOrders {
    /// <summary>The period for which a special order is valid.</summary>
    [RType]
    public enum QuestDuration {
        /// <summary>The order is valid until the end of this week.</summary>
        Week,
        /// <summary>The order is valid until the end of this month.</summary>
        Month,
        /// <summary>The order is valid until the end of the next weeks.</summary>
        TwoWeeks,
        /// <summary>The order is valid until the end of tomorrow.</summary>
        TwoDays,
        /// <summary>The order is valid until the end of after tomorrow.</summary>
        ThreeDays,
        /// <summary>The valid is valid until the end of today.</summary>
        OneDay,
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.SpecialOrderData" />, a randomized token which can be referenced by other special order fields.</summary>
    /// <remarks>See remarks on <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> for usage details.</remarks>
    [RType]
    public class RandomizedElement {
        /// <summary>The token name used to reference it.</summary>
        public string Name;
        /// <summary>The possible values to randomly choose from. If multiple values match, one is chosen randomly.</summary>
        public List<RandomizedElementItem> Values;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.RandomizedElement" />, a possible value for the token.</summary>
    [RType]
    public class RandomizedElementItem {
        /// <summary>A set of hardcoded tags that check conditions like the season, received mail, etc.</summary>
        [Optional] public string RequiredTags = "";
        /// <summary>The token value to set if this item is selected.</summary>
        public string Value = "";
    }
    [RType]
    public class SpecialOrderData {
        /// <summary>The translated display name for the special order.</summary>
        /// <remarks>Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Name]</c>.</remarks>
        public string Name;
        /// <summary>The internal name of the NPC requesting the special order.</summary>
        public string Requester;
        /// <summary>How long the player has to complete the special order.</summary>
        public QuestDuration Duration;
        /// <summary>Whether the special order can be chosen again if the player has previously completed it.</summary>
        [Optional] public bool Repeatable;
        /// <summary>A set of hardcoded tags that check conditions like the season, received mail, etc. Most code should use <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.Condition" /> instead.</summary>
        [Optional] public string RequiredTags = "";
        /// <summary>A game state query which indicates whether this special order can be given.</summary>
        [Optional] public string Condition = "";
        /// <summary>The order type (one of <c>Qi</c> or an empty string).</summary>
        /// <remarks>Setting this to <c>Qi</c> enables some custom game logic for Qi's challenges.</remarks>
        [Optional] public string OrderType = "";
        /// <summary>An arbitrary rule ID that can be checked by game or mod logic to enable special behavior while this order is active.</summary>
        [Optional] public string SpecialRule = "";
        /// <summary>The translated description text for the special order.</summary>
        /// <remarks>Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Text]</c>. This can contain <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens.</remarks>
        public string Text;
        /// <summary>If set, an unqualified item ID to remove everywhere in the world when this special order ends.</summary>
        [Optional] public string ItemToRemoveOnEnd;
        /// <summary>If set, a mail ID to remove from all players when this special order ends.</summary>
        [Optional] public string MailToRemoveOnEnd;
        /// <summary>The randomized tokens which can be referenced by other special order fields.</summary>
        /// <remarks>
        ///   <para>These can be used in some special order fields (noted in their code docs) in the form <c>{Name}</c> (like <c>{FishType}</c>), which returns the element's value.</para>
        ///   <para>If a randomized element selects an item, you can use the <c>{Name:ValueType}</c> form (like <c>{FishType:Text}</c>) to get a value related to the selected item:</para>
        ///   <list type="bullet">
        ///     <item><description><c>Text</c>: the item's translated display name.</description></item>
        ///     <item><description><c>TextPlural</c>: equivalent to <c>Text</c> but pluralized if possible.</description></item>
        ///     <item><description><c>TextPluralCapitalized</c>: equivalent to <c>Text</c> but pluralized if possible and its first letter capitalized.</description></item>
        ///     <item><description><c>Tags</c>: a context tag which identifies the item, like <c>id_o_128</c> for a pufferfish.</description></item>
        ///     <item><description><c>Price</c>: for objects only, the gold price for selling this item to a store (all other item types will have the value <c>1</c>).</description></item>
        ///   </list>
        /// </remarks>
        [Optional] public List<RandomizedElement> RandomizedElements;
        /// <summary>The objectives which must be achieved to complete this special order.</summary>
        public List<SpecialOrderObjectiveData> Objectives;
        /// <summary>The rewards given to the player when they complete this special order.</summary>
        public List<SpecialOrderRewardData> Rewards;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.SpecialOrderData" />, an objective that must be achieved to complete the special order.</summary>
    [RType]
    public class SpecialOrderObjectiveData {
        /// <summary>The name of the C# class which handles the logic for this objective.</summary>
        /// <remarks>The class must be in the <c>StardewValley</c> namespace, and its name must end with <c>Objective</c> (without including it in this field). For example, <c>"Gift"</c> will match the <c>StardewValley.GiftObjective</c> type.</remarks>
        public string Type;
        /// <summary>The translated description text for the objective.</summary>
        /// <remarks>Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Objective_0_Text]</c>. This can contain <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens.</remarks>
        public string Text;
        /// <summary>The number related to the objective.</summary>
        /// <remarks>This can contain <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens.</remarks>
        public string RequiredCount;
        /// <summary>The arbitrary data values understood by the C# class identified by <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderObjectiveData.Type" />. These may or may not allow <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens, depending on the class.</summary>
        public Dictionary<string, string> Data;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.SpecialOrderData" />, a reward given to the player when they complete this special order..</summary>
    [RType]
    public class SpecialOrderRewardData {
        /// <summary>The name of the C# class which handles the logic for this reward.</summary>
        /// <remarks>The class must be in the <c>StardewValley</c> namespace, and its name must end with <c>Reward</c> (without including it in this field). For example, <c>"Money"</c> will match the <c>StardewValley.MoneyReward</c> type.</remarks>
        public string Type;
        /// <summary>The arbitrary data values understood by the C# class identified by <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderRewardData.Type" />. These may or may not allow <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens, depending on the class.</summary>
        public Dictionary<string, string> Data;
    }
}

public class Tools {
    /// <summary>The behavior and metadata for a tool that can be equipped by players.</summary>
    [RType]
    public class ToolData {
        /// <summary>The name for the C# class to construct within the <c>StardewValley.Tools</c> namespace. This must be a subclass of <c>StardewValley.Tool</c>.</summary>
        public string ClassName;
        /// <summary>The tool's internal name.</summary>
        public string Name;
        /// <summary>The number of attachment slots to set, or <c>-1</c> to keep the default value.</summary>
        [Optional] public int AttachmentSlots = -1;
        /// <summary>The sale price for the tool in shops.</summary>
        [Optional] public int SalePrice = -1;
        /// <summary>A tokenizable string for the tool's display name.</summary>
        public string DisplayName;
        /// <summary>A tokenizable string for the tool's description.</summary>
        public string Description;
        /// <summary>The asset name for the texture containing the tool's sprite.</summary>
        public string Texture;
        /// <summary>The index within the <see cref="F:StardewValley.GameData.Tools.ToolData.Texture" /> for the animation sprites, where 0 is the top icon.</summary>
        public int SpriteIndex;
        /// <summary>The index within the <see cref="F:StardewValley.GameData.Tools.ToolData.Texture" /> for the item icon, or <c>-1</c> to use the <see cref="F:StardewValley.GameData.Tools.ToolData.SpriteIndex" />.</summary>
        [Optional] public int MenuSpriteIndex = -1;
        /// <summary>The tool's initial upgrade level, or <c>-1</c> to keep the default value.</summary>
        [Optional] public int UpgradeLevel = -1;
        /// <summary>If set, the item ID for a tool which can be upgraded into this one using the default upgrade rules based on <see cref="F:StardewValley.GameData.Tools.ToolData.UpgradeLevel" />. This is prepended to <see cref="F:StardewValley.GameData.Tools.ToolData.UpgradeFrom" />.</summary>
        [Optional] public string ConventionalUpgradeFrom;
        /// <summary>A list of items which the player can upgrade into this at Clint's shop.</summary>
        [Optional] public List<ToolUpgradeData> UpgradeFrom;
        /// <summary>Whether the player can lose this tool when they die.</summary>
        [Optional] public bool CanBeLostOnDeath;
        /// <summary>The class properties to set when creating the tool.</summary>
        [Optional] public Dictionary<string, string> SetProperties;
        /// <summary>The <c>modData</c> values to set when the tool is created.</summary>
        [Optional] public Dictionary<string, string> ModData;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Tools.ToolData" />, the requirements to upgrade items into a tool.</summary>
    [RType]
    public class ToolUpgradeData {
        /// <summary>A game state query which indicates whether this upgrade is available. Default always enabled.</summary>
        [Optional] public string Condition;
        /// <summary>The gold price to upgrade the tool, or <c>-1</c> to use <see cref="F:StardewValley.GameData.Tools.ToolData.SalePrice" />.</summary>
        [Optional] public int Price = -1;
        /// <summary>If set, the item ID for the tool that must be in the player's inventory for the upgrade to appear. The tool will be destroyed when the upgrade is accepted.</summary>
        [Optional] public string RequireToolId;
        /// <summary>If set, the item ID for an extra item that must be traded to upgrade the tool (for example, copper bars for many copper tools).</summary>
        [Optional] public string TradeItemId;
        /// <summary>The number of <see cref="F:StardewValley.GameData.Tools.ToolUpgradeData.TradeItemId" /> required.</summary>
        [Optional] public int TradeItemAmount = 1;
    }
}

public class Weapons {
    /// <summary>The metadata for a weapon that can be used by players.</summary>
    [RType]
    public class WeaponData {
        /// <summary>The internal weapon name.</summary>
        public string Name;
        /// <summary>A tokenizable string for the weapon's translated display name.</summary>
        public string DisplayName;
        /// <summary>A tokenizable string for the weapon's translated description.</summary>
        public string Description;
        /// <summary>The minimum base damage caused by hitting a monster with this weapon.</summary>
        public int MinDamage;
        /// <summary>The maximum base damage caused by hitting a monster with this weapon.</summary>
        public int MaxDamage;
        /// <summary>How far the target is pushed when hit, as a multiplier relative to a base weapon like the Rusty Sword (e.g. 1.5 for 150% of Rusty Sword's weight).</summary>
        [Optional] public float Knockback = 1f;
        /// <summary>How fast the player can swing the weapon. Each point of speed is worth 40ms of swing time relative to 0. This stacks with the player's weapon speed.</summary>
        [Optional] public int Speed;
        /// <summary>Reduces the chance that a strike will miss.</summary>
        [Optional] public int Precision;
        /// <summary>Reduces damage received by the player.</summary>
        [Optional] public int Defense;
        /// <summary>The weapon type. One of <c>0</c> (stabbing sword), <c>1</c> (dagger), <c>2</c> (club or hammer), or <c>3</c> (slashing sword).</summary>
        public int Type;
        /// <summary>The base mine level used to determine when this weapon appears in mine containers.</summary>
        [Optional] public int MineBaseLevel = -1;
        /// <summary>The min mine level used to determine when this weapon appears in mine containers.</summary>
        [Optional] public int MineMinLevel = -1;
        /// <summary>Slightly increases the area of effect.</summary>
        [Optional] public int AreaOfEffect;
        /// <summary>The chance of a critical hit, as a decimal value between 0 (never) and 1 (always).</summary>
        [Optional] public float CritChance = 0.02f;
        /// <summary>A multiplier applied to the base damage for a critical hit.</summary>
        [Optional] public float CritMultiplier = 3f;
        /// <summary>Whether the player can lose this weapon when they die.</summary>
        [Optional] public bool CanBeLostOnDeath = true;
        /// <summary>The asset name for the texture containing the weapon's sprite.</summary>
        public string Texture;
        /// <summary>The index within the <see cref="F:StardewValley.GameData.Weapons.WeaponData.Texture" /> for the weapon sprite, where 0 is the top-left icon.</summary>
        public int SpriteIndex;
        /// <summary>The projectiles fired when the weapon is used, if any. The continue along their path until they hit a monster and cause damage. One projectile will fire for each entry in the list. This doesn't apply for slingshots, which have hardcoded projectile logic.</summary>
        [Optional] public List<WeaponProjectile> Projectiles;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.Weapons.WeaponData" />, a projectile fired when the weapon is used.</summary>
    [RType]
    public class WeaponProjectile {
        /// <summary>A key which uniquely identifies the projectile within its weapon's data. The ID should only contain alphanumeric/underscore/dot characters. For custom projectiles, this should be prefixed with your mod ID like <c>Example.ModId_ProjectileId.</c></summary>
        public string Id;
        /// <summary>The amount of damage caused when they hit a monster.</summary>
        [Optional] public int Damage = 10;
        /// <summary>Whether the projectile explodes when it collides with something.</summary>
        [Optional] public bool Explodes;
        /// <summary>The number of times the projectile can bounce off walls before being destroyed.</summary>
        [Optional] public int Bounces;
        /// <summary>The maximum tile distance the projectile can travel.</summary>
        [Optional] public int MaxDistance = 4;
        /// <summary>The speed at which the projectile moves.</summary>
        [Optional] public int Velocity = 10;
        /// <summary>The rotation velocity.</summary>
        [Optional] public int RotationVelocity = 32;
        /// <summary>The length of the tail which trails behind the main projectile.</summary>
        [Optional] public int TailLength = 1;
        /// <summary>The sound played when the projectile is fired.</summary>
        [Optional] public string FireSound = "";
        /// <summary>The sound played when the projectile bounces off a wall.</summary>
        [Optional] public string BounceSound = "";
        /// <summary>The sound played when the projectile collides with something.</summary>
        [Optional] public string CollisionSound = "";
        /// <summary>The minimum value for a random offset applied to the direction of the project each time it's fired. If both fields are zero, it's always shot at the 90� angle matching the player's facing direction.</summary>
        [Optional] public float MinAngleOffset;
        /// <summary>The maximum value for <see cref="F:StardewValley.GameData.Weapons.WeaponProjectile.MinAngleOffset" />.</summary>
        [Optional] public float MaxAngleOffset;
        /// <summary>The sprite index in <c>TileSheets/Projectiles</c> to draw for this projectile.</summary>
        [Optional] public int SpriteIndex = 11;
        /// <summary>The item to shoot. If set, this overrides <see cref="F:StardewValley.GameData.Weapons.WeaponProjectile.SpriteIndex" />.</summary>
        [Optional] public GameData.GenericSpawnItemData Item;
    }
}

public class Weddings {
    /// <summary>As part of <see cref="T:StardewValley.GameData.Weddings.WeddingData" />, an NPC which should attend wedding events.</summary>
    [RType]
    public class WeddingAttendeeData {
        /// <summary>The internal name for the NPC.</summary>
        public string Id;
        /// <summary>A game state query which indicates whether the NPC should attend. Defaults to always attend.</summary>
        [Optional] public string Condition;
        /// <summary>The NPC's tile position and facing direction when they attend. This uses the same format as field index 2 in an event script.</summary>
        public string Setup;
        /// <summary>The event script to run during the celebration, like <c>faceDirection Pierre 3 true</c> which makes Pierre turn to face left. This can contain any number of slash-delimited script commands.</summary>
        [Optional] public string Celebration;
        /// <summary>Whether to add this NPC regardless of their <see cref="F:StardewValley.GameData.Characters.CharacterData.UnlockConditions" />.</summary>
        [Optional] public bool IgnoreUnlockConditions;
    }
    [RType]
    public class WeddingData {
        /// <summary>A tokenizable string for the event script which plays the wedding.</summary>
        /// <remarks>The key is the internal name of the NPC or unique ID of the player being married, else <c>default</c> for the default script which automatically handles marrying either an NPC or player.</remarks>
        public Dictionary<string, string> EventScript;
        /// <summary>The other NPCs which should attend wedding events (unless they're the spouse), indexed by <see cref="F:StardewValley.GameData.Weddings.WeddingAttendeeData.Id" />.</summary>
        public Dictionary<string, WeddingAttendeeData> Attendees;
    }
}

public class WildTrees {
    /// <summary>Metadata for a non-fruit tree type.</summary>
    [RType]
    public class WildTreeData {
        /// <summary>The tree textures to show in game. The first matching texture will be used.</summary>
        public List<WildTreeTextureData> Textures;
        /// <summary>The qualified or unqualified item ID for the seed item.</summary>
        public string SeedItemId;
        /// <summary>Whether the seed can be planted by the player. If false, it can only be spawned automatically via map properties.</summary>
        [Optional] public bool SeedPlantable = true;
        /// <summary>The percentage chance each day that the tree will grow to the next stage without tree fertilizer, as a value from 0 (will never grow) to 1 (will grow every day).</summary>
        [Optional] public float GrowthChance = 0.2f;
        /// <summary>Overrides <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.GrowthChance" /> when tree fertilizer is applied.</summary>
        [Optional] public float FertilizedGrowthChance = 1f;
        /// <summary>The percentage chance each day that the tree will plant a seed on a nearby tile, as a value from 0 (never) to 1 (always). This only applied in locations where trees drop seeds (e.g. farms in vanilla).</summary>
        [Optional] public float SeedSpreadChance = 0.15f;
        /// <summary>The percentage chance each day that the tree will produce a seed that will drop when the tree is shaken, as a value from 0 (never) to 1 (always).</summary>
        [Optional] public float SeedOnShakeChance = 0.05f;
        /// <summary>The percentage chance that a seed will drop when the player chops down the tree, as a value from 0 (never) to 1 (always).</summary>
        [Optional] public float SeedOnChopChance = 0.75f;
        /// <summary>Whether to drop wood when the player chops down the tree.</summary>
        [Optional] public bool DropWoodOnChop = true;
        /// <summary>Whether to drop hardwood when the player chops down the tree, if they have the Lumberjack profession.</summary>
        [Optional] public bool DropHardwoodOnLumberChop = true;
        /// <summary>Whether shaking or chopping the tree causes cosmetic leaves to drop from tree and produces a leaf rustle sound. When a leaf drops, the game will use one of the four leaf sprites in the tree's spritesheet in the slot left of the stump sprite.</summary>
        [Optional] public bool IsLeafy = true;
        /// <summary>Whether <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.IsLeafy" /> also applies in winter.</summary>
        [Optional] public bool IsLeafyInWinter;
        /// <summary>Whether <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.IsLeafy" /> also applies in fall.</summary>
        [Optional] public bool IsLeafyInFall = true;
        /// <summary>The rules which override which locations the tree can be planted in, if applicable. These don't override more specific checks (e.g. not being plantable on stone).</summary>
        [Optional] public List<PlantableRule> PlantableLocationRules;
        /// <summary>Whether the tree can grow in winter (subject to <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.GrowthChance" /> or <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.FertilizedGrowthChance" />).</summary>
        [Optional] public bool GrowsInWinter;
        /// <summary>Whether the tree is reduced to a stump in winter and regrows in spring, like the vanilla mushroom tree.</summary>
        [Optional] public bool IsStumpDuringWinter;
        /// <summary>Whether woodpeckers can spawn on the tree.</summary>
        [Optional] public bool AllowWoodpeckers = true;
        /// <summary>Whether to render a different tree sprite when the tree hasn't been shaken that day.</summary>
        /// <inheritdoc cref="F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenSeedReady" path="/remarks" />
        [Optional] public bool UseAlternateSpriteWhenNotShaken;
        /// <summary>Whether to render a different tree sprite when it has a seed ready. If true, the tree spritesheet should be double-width with the alternate textures on the right.</summary>
        /// <remarks>If <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenNotShaken" /> or <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenSeedReady" /> is true, the tree spritesheet should be double-width with the alternate textures on the right. If both are true, the same alternate texture is used for both.</remarks>
        [Optional] public bool UseAlternateSpriteWhenSeedReady;
        /// <summary>
        ///   The color of the cosmetic wood chips when chopping the tree. This can be...
        ///   <list type="bullet">
        ///     <item><description>a MonoGame property name (like <c>SkyBlue</c>);</description></item>
        ///     <item><description>an RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>);</description></item>
        ///     <item><description>an 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>);</description></item>
        ///     <item><description>or a debris type code: <c>12</c> (brown/woody), <c>10000</c> (white), <c>100001</c> (light green), <c>100002</c> (light blue), <c>100003</c> (red), <c>100004</c> (yellow), <c>100005</c> (black), <c>100006</c> (gray), <c>100007</c> (charcoal / dim gray).</description></item>
        ///    </list>
        ///    Defaults to brown/woody.
        /// </summary>
        [Optional] public string DebrisColor;
        /// <summary>When a seed is dropped subject to <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.SeedOnShakeChance" />, the item to drop instead of <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.SeedItemId" />. If this is empty or none match, the <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.SeedItemId" /> will be dropped instead.</summary>
        [Optional] public List<WildTreeSeedDropItemData> SeedDropItems;
        /// <summary>The additional items to drop when the tree is chopped.</summary>
        [Optional] public List<WildTreeChopItemData> ChopItems;
        /// <summary>The items produced by tapping the tree when it's fully grown. If multiple items can be produced, the first available one is selected.</summary>
        [Optional] public List<WildTreeTapItemData> TapItems;
        /// <summary>The items produced by shaking the tree when it's fully grown.</summary>
        [Optional] public List<WildTreeItemData> ShakeItems;
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
        /// <summary>Whether this tree grows moss or not</summary>
        [Optional] public bool GrowsMoss;
        /// <summary>Get whether trees of this type can be tapped in any season.</summary>
        public bool CanBeTapped() => TapItems != null && TapItems.Count > 0;
    }
    /// <summary>The growth state for a tree.</summary>
    /// <remarks>These mainly exist to make content edits more readable. Most code should use the constants like <c>Tree.seedStage</c>, which have the same values.</remarks>
    [RType]
    public enum WildTreeGrowthStage {
        Seed = 0,
        Sprout = 1,
        Sapling = 2,
        Bush = 3,
        Tree = 5,
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to produce.</summary>
    [RType]
    public class WildTreeItemData : GameData.GenericSpawnItemDataWithCondition {
        /// <summary>If set, the specific season when this data should apply. For more complex conditions, see <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" />.</summary>
        [Optional] public Season? Season { get; set; }
        /// <summary>The probability that the item will be produced, as a value between 0 (never) and 1 (always).</summary>
        [Optional] public float Chance { get; set; } = 1f;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to drop when the tree is chopped down.</summary>
    [RType]
    public class WildTreeChopItemData : WildTreeItemData {
        /// <summary>The minimum growth stage at which to produce this item.</summary>
        [Optional] public WildTreeGrowthStage? MinSize { get; set; }
        /// <summary>The maximum growth stage at which to produce this item.</summary>
        [Optional] public WildTreeGrowthStage? MaxSize { get; set; }
        /// <summary>Whether to drop this item if the item is a stump (true), not a stump (false), or both (null).</summary>
        [Optional] public bool? ForStump { get; set; } = new bool?(false);
        /// <summary>Get whether the given tree growth stage is valid for <see cref="P:StardewValley.GameData.WildTrees.WildTreeChopItemData.MinSize" /> and <see cref="P:StardewValley.GameData.WildTrees.WildTreeChopItemData.MaxSize" />.</summary>
        /// <param name="size">The tree growth stage.</param>
        /// <param name="isStump">Whether the tree is a stump.</param>
        public bool IsValidForGrowthStage(int size, bool isStump) {
            if (size == 4) size = 3;
            var nullable2 = MinSize.HasValue ? new int?((int)MinSize.GetValueOrDefault()) : new int?();
            if (size < nullable2.GetValueOrDefault() & nullable2.HasValue) return false;
            nullable2 = MaxSize.HasValue ? new int?((int)MaxSize.GetValueOrDefault()) : new int?();
            if (size > nullable2.GetValueOrDefault() & nullable2.HasValue) return false;
            if (ForStump.HasValue) if (!(ForStump.GetValueOrDefault() == isStump & ForStump.HasValue)) return false;
            return true;
        }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to produce when dropping the tree seed.</summary>
    [RType]
    public class WildTreeSeedDropItemData : WildTreeItemData {
        /// <summary>If this item is dropped, whether to continue as if it hadn't been dropped for the remaining drop candidates.</summary>
        [Optional] public bool ContinueOnDrop { get; set; }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to produce for tappers on the tree.</summary>
    [RType]
    public class WildTreeTapItemData : WildTreeItemData {
        /// <summary>If set, the group only applies if the previous item produced by the tapper matches one of these qualified or unqualified item IDs (including <c>null</c> for the initial tap).</summary>
        [Optional] public List<string> PreviousItemId { get; set; }
        /// <summary>The number of days before the tapper is ready to empty.</summary>
        public int DaysUntilReady { get; set; }
        /// <summary>Changes to apply to the result of <see cref="P:StardewValley.GameData.WildTrees.WildTreeTapItemData.DaysUntilReady" />.</summary>
        [Optional] public List<QuantityModifier> DaysUntilReadyModifiers { get; set; }
        /// <summary>How multiple <see cref="P:StardewValley.GameData.WildTrees.WildTreeTapItemData.DaysUntilReadyModifiers" /> should be combined.</summary>
        [Optional] public QuantityModifier.QuantityModifierMode DaysUntilReadyModifierMode { get; set; }
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible spritesheet to use for the tree.</summary>
    [RType]
    public class WildTreeTextureData {
        /// <summary>A game state query which indicates whether this spritesheet should be applied for a tree. Defaults to always enabled.</summary>
        /// <remarks>This condition is checked when a tree's texture is loaded. Once it's loaded, the conditions won't be rechecked until the next day.</remarks>
        [Optional] public string Condition;
        /// <summary>If set, the specific season when this texture should apply. For more complex conditions, see <see cref="F:StardewValley.GameData.WildTrees.WildTreeTextureData.Condition" />.</summary>
        [Optional] public Season? Season;
        /// <summary>The asset name for the tree's spritesheet.</summary>
        public string Texture;
    }
}

public class WorldMaps {
    /// <summary>An area within a larger <see cref="T:StardewValley.GameData.WorldMaps.WorldMapRegionData" /> to draw onto the world map. This can provide textures, tooltips, and world positioning data.</summary>
    [RType]
    public class WorldMapAreaData {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_AreaId</c>.</summary>
        public string Id;
        /// <summary>If set, a game state query which checks whether the area should be applied. Defaults to always applied.</summary>
        [Optional] public string Condition;
        /// <summary>The pixel area within the map which is covered by this area.</summary>
        [Optional] public Rectangle PixelArea;
        /// <summary>If set, a tokenizable string for the scroll text shown at the bottom of the map when the player is in the location. Defaults to none.</summary>
        [Optional] public string ScrollText;
        /// <summary>The image overlays to apply to the map.</summary>
        [Optional] public List<WorldMapTextureData> Textures = [];
        /// <summary>The tooltips to show when hovering over parts of this area on the world map.</summary>
        [Optional] public List<WorldMapTooltipData> Tooltips = [];
        /// <summary>The in-world locations and tile coordinates to match to this map area.</summary>
        [Optional] public List<WorldMapAreaPositionData> WorldPositions = [];
        /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
        [Optional] public Dictionary<string, string> CustomFields;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaData" />, a set of in-game locations and tile positions to match to the area.</summary>
    [RType]
    public class WorldMapAreaPositionData {
        /// <summary>If set, the smaller areas within this position which show a different scroll text.</summary>
        [Optional] public List<WorldMapAreaPositionScrollTextZoneData> ScrollTextZones = [];
        /// <summary>The backing field for <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.Id" />.</summary>
        string _idImpl;
        /// <summary>An ID for this entry within the list. This only needs to be unique within the current position list. Defaults to <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.LocationName" />, if set.</summary>
        [Optional]
        public string Id {
            get {
                if (_idImpl != null) return _idImpl;
                if (LocationName != null) return LocationName;
                return (LocationNames?.FirstOrDefault()) ?? LocationContext;
            }
            set => _idImpl = value;
        }
        /// <summary>If set, a game state query which checks whether this position should be applied. Defaults to always applied.</summary>
        [Optional] public string Condition;
        /// <summary>The location context in which this world position applies.</summary>
        [Optional] public string LocationContext;
        /// <summary>The location name to which this world position applies. Any location within the mines and the Skull Cavern will be <c>Mines</c> and <c>SkullCave</c> respectively, and festivals use the map asset name (e.g. <c>Town-EggFestival</c>).</summary>
        [Optional] public string LocationName;
        /// <summary>A list of location names in which this world position applies (see <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.LocationName" /> for details).</summary>
        [Optional] public List<string> LocationNames = [];
        /// <summary>The tile area for the zone within the in-game location, or an empty rectangle for the entire map.</summary>
        /// <remarks><see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" /> and <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.MapPixelArea" /> are used to calculate the position of a player within the map view, given their real position in-game. For example, let's say an area has tile positions (0, 0) through (10, 20), and map pixel positions (200, 200) through (300, 400). If the player is standing on tile (5, 10) in-game (in the exact middle of the location), the game would place their marker at pixel (250, 300) on the map (in the exact middle of the map area).</remarks>
        [Optional] public Rectangle TileArea;
        /// <summary>The tile area within which the player is considered to be within the zone, even if they're beyond the <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" />. Positions outside the <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" /> will be snapped to the nearest valid position.</summary>
        [Optional] public Rectangle? ExtendedTileArea;
        /// <summary>The pixel coordinates for the image area on the map.</summary>
        /// <remarks>See remarks on <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" />.</remarks>
        [Optional] public Rectangle MapPixelArea;
        /// <summary>A tokenizable string for the scroll text shown at the bottom of the map when the player is in this area. Defaults to <see cref="F:StardewValley.GameData.WorldMaps.WorldMapAreaData.ScrollText" />.</summary>
        [Optional] public string ScrollText;
    }
    /// <summary>As part of <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData" />, a smaller area within this position which shows a different scroll text.</summary>
    [RType]
    public class WorldMapAreaPositionScrollTextZoneData {
        /// <summary>An ID for this entry within the list. This only needs to be unique within the current position list.</summary>
        public string Id;
        /// <summary>The pixel coordinates for the image area on the map.</summary>
        [Optional] public Rectangle TileArea;
        /// <summary>A tokenizable string for the scroll text shown at the bottom of the map when the player is in this area. Defaults to <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.ScrollText" />.</summary>
        [Optional] public string ScrollText;
    }
    /// <summary>A large-scale part of the world like the Valley, containing all the areas drawn together as part of the combined map view.</summary>
    [RType]
    public class WorldMapRegionData {
        /// <summary>The base texture to draw as the base texture, if any. The first matching texture is applied.</summary>
        public List<WorldMapTextureData> BaseTexture = [];
        /// <summary>Maps neighbor IDs for controller support in fields like <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" /> to the specific values to use. This allows using simplified IDs like <c>Beach/FishShop</c> instead of <c>Beach/FishShop_DefaultHours, Beach/FishShop_ExtendedHours</c>. Aliases cannot be recursive.</summary>
        [Optional] public Dictionary<string, string> MapNeighborIdAliases = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>The areas to draw on top of the <see cref="F:StardewValley.GameData.WorldMaps.WorldMapRegionData.BaseTexture" />. These can provide tooltips, scroll text, and character marker positioning data.</summary>
        public List<WorldMapAreaData> MapAreas = [];
    }
    /// <summary>As part of a larger <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaData" />, an image overlay to apply to the map.</summary>
    [RType]
    public class WorldMapTextureData {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_OverlayId</c>.</summary>
        public string Id;
        /// <summary>If set, a game state query which checks whether the overlay should be applied. Defaults to always applied.</summary>
        [Optional] public string Condition;
        /// <summary>The asset name for the texture to draw when the area is applied to the map.</summary>
        [Optional] public string Texture;
        /// <summary>The pixel area within the <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTextureData.Texture" /> to draw, or an empty rectangle to draw the entire image.</summary>
        [Optional] public Rectangle SourceRect;
        /// <summary>The pixel area within the map area to draw the texture to. If this is an empty rectangle, defaults to the entire map (for a base texture) or <see cref="F:StardewValley.GameData.WorldMaps.WorldMapAreaData.PixelArea" /> (for a map area texture).</summary>
        [Optional] public Rectangle MapPixelArea;
    }
    /// <summary>A tooltip shown when hovering over parts of a larger <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaData" /> on the world map.</summary>
    [RType]
    public class WorldMapTooltipData {
        /// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_TooltipId.</c></summary>
        public string Id;
        /// <summary>If set, a game state query which checks whether the tooltip should be visible. Defaults to always visible.</summary>
        [Optional] public string Condition;
        /// <summary>If set, a game state query which checks whether the area is known by the player, so the <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.Text" /> is shown as-is. If this is false, the tooltip text is replaced with '???'. Defaults to always known.</summary>
        [Optional] public string KnownCondition;
        /// <summary>The pixel area within the map which can be hovered to show this tooltip, or an empty rectangle if it covers the entire area.</summary>
        [Optional] public Rectangle PixelArea;
        /// <summary>A tokenizable string for the tooltip shown when the mouse is over the area.</summary>
        public string Text;
        /// <summary>The tooltip to the left of this one for controller navigation.</summary>
        /// <remarks>This should be the area and tooltip ID, formatted like <c>areaId/tooltipId</c> (not case-sensitive). If there are multiple possible neighbors, they can be specified in comma-delimited form like <c>areaId/tooltipId, areaId/tooltipId, ...</c>; the first one which exists will be used.</remarks>
        public string LeftNeighbor;
        /// <summary>The tooltip to the right of this one for controller navigation.</summary>
        /// <inheritdoc cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" path="/remarks" />
        public string RightNeighbor;
        /// <summary>The tooltip above this one for controller navigation.</summary>
        /// <inheritdoc cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" path="/remarks" />
        public string UpNeighbor;
        /// <summary>The tooltip below this one for controller navigation.</summary>
        /// <inheritdoc cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" path="/remarks" />
        public string DownNeighbor;
    }
}
