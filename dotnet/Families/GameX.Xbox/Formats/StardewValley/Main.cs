using GameX.Xbox.Formats.StardewValley.GameData;
using System.IO;
using static GameX.Xbox.Formats.Binary_Xnb;

namespace GameX.Xbox.Formats.StardewValley;

public static class X {
    public static void Register() {
        //Add(new TypeReader<string>("BmFont.XmlSourceReader", "String", r => r.ReadLV7UString()));
        //{
        //    Add(new TypeReader<GenericSpawnItemData>("GenericSpawnItemData", "StardewValley.GameData.GenericSpawnItemData", r => new GenericSpawnItemData()));
        //    Add(new TypeReader<GenericSpawnItemDataWithCondition>("GenericSpawnItemDataWithCondition", "StardewValley.GameData.GenericSpawnItemDataWithCondition", r => new GenericSpawnItemDataWithCondition()));
        //    Add(new TypeReader<QuantityModifier>("QuantityModifier", "StardewValley.GameData.QuantityModifier", r => new QuantityModifier()));
        //    Add(new TypeReader<QuantityModifier.ModificationType>("ModificationType", "StardewValley.GameData.QuantityModifier.ModificationType", r => new QuantityModifier.ModificationType()));
        //    Add(new TypeReader<QuantityModifier.QuantityModifierMode>("QuantityModifier", "StardewValley.GameData.QuantityModifier.QuantityModifierMode", r => new QuantityModifier.QuantityModifierMode()));
        //}
        //A
        //Add(new TypeReader<Buildings.BuildingData>("Buildings.BuildingData", "StardewValley.GameData.Buildings.BuildingData", r => new Buildings.BuildingData()));
        //{
        //    Add(new TypeReader<Buildings.BuildingActionTile>("Buildings.BuildingActionTile", "StardewValley.GameData.Buildings.BuildingActionTile", r => new Buildings.BuildingActionTile()));
        //    Add(new TypeReader<Buildings.BuildingChest>("Buildings.BuildingChest", "StardewValley.GameData.Buildings.BuildingChest", r => new Buildings.BuildingChest()));
        //    Add(new TypeReader<Buildings.BuildingChestType>("Buildings.BuildingChestType", "StardewValley.GameData.Buildings.BuildingChestType", r => new Buildings.BuildingChestType()));
        //    Add(new TypeReader<Buildings.BuildingDrawLayer>("Buildings.BuildingDrawLayer", "StardewValley.GameData.Buildings.BuildingDrawLayer", r => new Buildings.BuildingDrawLayer()));
        //    Add(new TypeReader<Buildings.BuildingItemConversion>("Buildings.BuildingItemConversion", "StardewValley.GameData.Buildings.BuildingItemConversion", r => new Buildings.BuildingItemConversion()));
        //    Add(new TypeReader<Buildings.BuildingMaterial>("Buildings.BuildingMaterial", "StardewValley.GameData.Buildings.BuildingMaterial", r => new Buildings.BuildingMaterial()));
        //    Add(new TypeReader<Buildings.BuildingPlacementTile>("Buildings.BuildingPlacementTile", "StardewValley.GameData.Buildings.BuildingPlacementTile", r => new Buildings.BuildingPlacementTile()));
        //    Add(new TypeReader<Buildings.BuildingSkin>("Buildings.BuildingSkin", "StardewValley.GameData.Buildings.BuildingSkin", r => new Buildings.BuildingSkin()));
        //    Add(new TypeReader<Buildings.BuildingTileProperty>("Buildings.BuildingTileProperty", "StardewValley.GameData.Buildings.BuildingTileProperty", r => new Buildings.BuildingTileProperty()));
        //    Add(new TypeReader<Buildings.IndoorItemAdd>("Buildings.IndoorItemAdd", "StardewValley.GameData.Buildings.IndoorItemAdd", r => new Buildings.IndoorItemAdd()));
        //    Add(new TypeReader<Buildings.IndoorItemMove>("Buildings.IndoorItemMove", "StardewValley.GameData.Buildings.IndoorItemMove", r => new Buildings.IndoorItemMove()));
        //}
        //Add(new TypeReader<Characters.CharacterData>("Characters.CharacterData", "StardewValley.GameData.Characters.CharacterData", r => new Characters.CharacterData()));
        //{
        //    Add(new TypeReader<Characters.CharacterAppearanceData>("Characters.CharacterAppearanceData", "StardewValley.GameData.Characters.CharacterAppearanceData", r => new Characters.CharacterAppearanceData()));
        //    Add(new TypeReader<Characters.CharacterHomeData>("Characters.CharacterHomeData", "StardewValley.GameData.Characters.CharacterHomeData", r => new Characters.CharacterHomeData()));
        //    Add(new TypeReader<Characters.CharacterShadowData>("Characters.CharacterShadowData", "StardewValley.GameData.Characters.CharacterShadowData", r => new Characters.CharacterShadowData()));
        //    Add(new TypeReader<Characters.CharacterSpousePatioData>("Characters.CharacterSpousePatioData", "StardewValley.GameData.Characters.CharacterSpousePatioData", r => new Characters.CharacterSpousePatioData()));
        //    Add(new TypeReader<Characters.CharacterSpouseRoomData>("Characters.CharacterSpouseRoomData", "StardewValley.GameData.Characters.CharacterSpouseRoomData", r => new Characters.CharacterSpouseRoomData()));
        //    // enum
        //    Add(new TypeReader<Characters.CalendarBehavior>("Characters.CalendarBehavior", "StardewValley.GameData.Characters.CalendarBehavior", r => new Characters.CalendarBehavior()));
        //    Add(new TypeReader<Characters.EndSlideShowBehavior>("Characters.EndSlideShowBehavior", "StardewValley.GameData.Characters.EndSlideShowBehavior", r => new Characters.EndSlideShowBehavior()));
        //    Add(new TypeReader<Characters.NpcAge>("Characters.NpcAge", "StardewValley.GameData.Characters.NpcAge", r => new Characters.NpcAge()));
        //    Add(new TypeReader<Characters.NpcLanguage>("Characters.NpcLanguage", "StardewValley.GameData.Characters.NpcLanguage", r => new Characters.NpcLanguage()));
        //    Add(new TypeReader<Characters.NpcManner>("Characters.NpcManner", "StardewValley.GameData.Characters.NpcManner", r => new Characters.NpcManner()));
        //    Add(new TypeReader<Characters.NpcOptimism>("Characters.NpcOptimism", "StardewValley.GameData.Characters.NpcOptimism", r => new Characters.NpcOptimism()));
        //    Add(new TypeReader<Characters.NpcSocialAnxiety>("Characters.NpcSocialAnxiety", "StardewValley.GameData.Characters.NpcSocialAnxiety", r => new Characters.NpcSocialAnxiety()));
        //    Add(new TypeReader<Characters.SocialTabBehavior>("Characters.SocialTabBehavior", "StardewValley.GameData.Characters.SocialTabBehavior", r => new Characters.SocialTabBehavior()));
        //}
        //Add(new TypeReader<Movies.ConcessionItemData>("Movies.ConcessionItemData", "StardewValley.GameData.Movies.ConcessionItemData", r => new Movies.ConcessionItemData()));
        //Add(new TypeReader<Movies.ConcessionTaste>("Movies.ConcessionTaste", "StardewValley.GameData.Movies.ConcessionTaste", r => new Movies.ConcessionTaste()));
        //Add(new TypeReader<Crops.CropData>("Crops.CropData", "StardewValley.GameData.Crops.CropData", r => new Crops.CropData()));
        //Add(new TypeReader<FarmAnimals.FarmAnimalData>("FarmAnimals.FarmAnimalData", "StardewValley.GameData.FarmAnimals.FarmAnimalData", r => new FarmAnimals.FarmAnimalData()));
        //Add(new TypeReader<Fences.FenceData>("Fences.FenceData", "StardewValley.GameData.Fences.FenceData", r => new Fences.FenceData()));
        //Add(new TypeReader<FishPonds.FishPondData>("FishPonds.FishPondData", "StardewValley.GameData.FishPonds.FishPondData", r => new FishPonds.FishPondData()));
        //Add(new TypeReader<FloorsAndPaths.FloorPathData>("FloorsAndPaths.FloorPathData", "StardewValley.GameData.FloorsAndPaths.FloorPathData", r => new FloorsAndPaths.FloorPathData()));
        //Add(new TypeReader<FruitTrees.FruitTreeData>("FruitTrees.FruitTreeData", "StardewValley.GameData.FruitTrees.FruitTreeData", r => new FruitTrees.FruitTreeData()));
        //Add(new TypeReader<GarbageCans.GarbageCanData>("GarbageCans.GarbageCanData", "StardewValley.GameData.GarbageCans.GarbageCanData", r => new GarbageCans.GarbageCanData()));
        //Add(new TypeReader<GiantCrops.GiantCropData>("GiantCrops.GiantCropData", "StardewValley.GameData.GiantCrops.GiantCropData", r => new GiantCrops.GiantCropData()));
        //Add(new TypeReader<HomeRenovations.HomeRenovation>("HomeRenovations.HomeRenovation", "StardewValley.GameData.HomeRenovations.HomeRenovation", r => new HomeRenovations.HomeRenovation()));
        //Add(new TypeReader<IncomingPhoneCallData>("IncomingPhoneCallData", "StardewValley.GameData.IncomingPhoneCallData", r => new IncomingPhoneCallData()));
        //Add(new TypeReader<JukeboxTrackData>("JukeboxTrackData", "StardewValley.GameData.JukeboxTrackData", r => new JukeboxTrackData()));
        //Add(new TypeReader<LocationContexts.LocationContextData>("LocationContexts.LocationContextData", "StardewValley.GameData.LocationContexts.LocationContextData", r => new LocationContexts.LocationContextData()));
        //Add(new TypeReader<Locations.LocationData>("Locations.LocationData", "StardewValley.GameData.Locations.LocationData", r => new Locations.LocationData()));
        //Add(new TypeReader<LostItem>("LostItem", "StardewValley.GameData.LostItem", r => new LostItem()));
        //Add(new TypeReader<Machines.MachineData>("Machines.MachineData", "StardewValley.GameData.Machines.MachineData", r => new Machines.MachineData()));
        //Add(new TypeReader<MakeoverOutfits.MakeoverOutfit>("MakeoverOutfits.MakeoverOutfit", "StardewValley.GameData.MakeoverOutfits.MakeoverOutfit", r => new MakeoverOutfits.MakeoverOutfit()));
        //Add(new TypeReader<MannequinData>("MannequinData", "StardewValley.GameData.MannequinData", r => new MannequinData()));
        //Add(new TypeReader<Minecarts.MinecartNetworkData>("Minecarts.MinecartNetworkData", "StardewValley.GameData.Minecarts.MinecartNetworkData", r => new Minecarts.MinecartNetworkData()));
        //Add(new TypeReader<MonsterSlayerQuestData>("MonsterSlayerQuestData", "StardewValley.GameData.MonsterSlayerQuestData", r => new MonsterSlayerQuestData()));
        //Add(new TypeReader<Movies.MovieData>("Movies.MovieData", "StardewValley.GameData.Movies.MovieData", r => new Movies.MovieData()));
        //Add(new TypeReader<Movies.MovieCharacterReaction>("Movies.MovieCharacterReaction", "StardewValley.GameData.Movies.MovieCharacterReaction", r => new Movies.MovieCharacterReaction()));
        //Add(new TypeReader<Museum.MuseumRewards>("Museum.MuseumRewards", "StardewValley.GameData.Museum.MuseumRewards", r => new Museum.MuseumRewards()));
        //Add(new TypeReader<Objects.ObjectData>("Objects.ObjectData", "StardewValley.GameData.Objects.ObjectData", r => new Objects.ObjectData()));
        //Add(new TypeReader<Pants.PantsData>("Pants.PantsData", "StardewValley.GameData.Pants.PantsData", r => new Pants.PantsData()));
        //Add(new TypeReader<PassiveFestivalData>("PassiveFestivalData", "StardewValley.GameData.PassiveFestivalData", r => new PassiveFestivalData()));
        //Add(new TypeReader<Pets.PetData>("Pets.PetData", "StardewValley.GameData.Pets.PetData", r => new Pets.PetData()));
        //Add(new TypeReader<Powers.PowersData>("Powers.PowersData", "StardewValley.GameData.Powers.PowersData", r => new Powers.PowersData()));
        //Add(new TypeReader<Bundles.RandomBundleData>("Bundles.RandomBundleData", "StardewValley.GameData.Bundles.RandomBundleData", r => new Bundles.RandomBundleData()));
        //Add(new TypeReader<Shirts.ShirtData>("Shirts.ShirtData", "StardewValley.GameData.Shirts.ShirtData", r => new Shirts.ShirtData()));
        //Add(new TypeReader<Shops.ShopData>("Shops.ShopData", "StardewValley.GameData.Shops.ShopData", r => new Shops.ShopData()));
        //Add(new TypeReader<SpecialOrders.SpecialOrderData>("SpecialOrders.SpecialOrderData", "StardewValley.GameData.SpecialOrders.SpecialOrderData", r => new SpecialOrders.SpecialOrderData()));
        //Add(new TypeReader<Crafting.TailorItemRecipe>("Crafting.TailorItemRecipe", "StardewValley.GameData.Crafting.TailorItemRecipe", r => new Crafting.TailorItemRecipe()));
        //Add(new TypeReader<Tools.ToolData>("Tools.ToolData", "StardewValley.GameData.Tools.ToolData", r => new Tools.ToolData()));
        //Add(new TypeReader<TriggerActionData>("TriggerActionData", "StardewValley.GameData.TriggerActionData", r => new TriggerActionData()));
        //Add(new TypeReader<TrinketData>("TrinketData", "StardewValley.GameData.TrinketData", r => new TrinketData()));
        //Add(new TypeReader<Weapons.WeaponData>("Weapons.WeaponData", "StardewValley.GameData.Weapons.WeaponData", r => new Weapons.WeaponData()));
        //Add(new TypeReader<Weddings.WeddingData>("Weddings.WeddingData", "StardewValley.GameData.Weddings.WeddingData", r => new Weddings.WeddingData()));
        //Add(new TypeReader<WildTrees.WildTreeData>("WildTrees.WildTreeData", "StardewValley.GameData.WildTrees.WildTreeData", r => new WildTrees.WildTreeData()));
        //Add(new TypeReader<WorldMaps.WorldMapRegionData>("WorldMaps.WorldMapRegion", "StardewValley.GameData.WorldMaps.WorldMapRegionData", r => new WorldMaps.WorldMapRegionData()));
    }
}