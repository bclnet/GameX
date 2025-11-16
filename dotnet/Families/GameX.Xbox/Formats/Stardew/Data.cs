using System.Collections.Generic;

namespace GameX.Xbox.Formats.Stardew;

#region GameData

public class BigCraftableData {
    /// <summary>The internal item name.</summary>
    public string Name;
    /// <summary>A tokenizable string for the item's translated display name.</summary>
    public string DisplayName;
    /// <summary>A tokenizable string for the item's translated description.</summary>
    public string Description;
    /// <summary>The price when sold by the player. This is not the price when bought from a shop.</summary>
    public int? Price;
    /// <summary>How the item can be picked up. The possible values are 0 (pick up with any tool), 1 (destroyed if hit with an axe/hoe/pickaxe, or picked up with any other tool), or 2 (can't be removed once placed).</summary>
    public int? Fragility;
    /// <summary>Whether the item can be placed outdoors.</summary>
    public bool? CanBePlacedOutdoors = true;
    /// <summary>Whether the item can be placed indoors.</summary>
    public bool? CanBePlacedIndoors = true;
    /// <summary>Whether this is a lamp and should produce light when dark.</summary>
    public bool? IsLamp;
    /// <summary>The asset name for the texture containing the item's sprite, or <c>null</c> for <c>TileSheets/Craftables</c>.</summary>
    public string Texture;
    /// <summary>The sprite's index in the spritesheet.</summary>
    public int SpriteIndex;
    /// <summary>The custom context tags to add for this item (in addition to the tags added automatically based on the other object data).</summary>
    public List<string> ContextTags;
    /// <summary>Custom fields ignored by the base game, for use by mods.</summary>
    public Dictionary<string, string> CustomFields;
}

#endregion
