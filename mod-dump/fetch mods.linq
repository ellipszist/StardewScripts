<Query Kind="Program">
  <Reference>&lt;ProgramFilesX86&gt;\Steam\steamapps\common\Stardew Valley\smapi-internal\SMAPI.Toolkit.CoreInterfaces.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Steam\steamapps\common\Stardew Valley\smapi-internal\SMAPI.Toolkit.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <NuGetReference>Pathoschild.FluentNexus</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Pathoschild.FluentNexus</Namespace>
  <Namespace>Pathoschild.FluentNexus.Models</Namespace>
  <Namespace>Pathoschild.Http.Client</Namespace>
  <Namespace>StardewModdingAPI</Namespace>
  <Namespace>StardewModdingAPI.Toolkit</Namespace>
  <Namespace>StardewModdingAPI.Toolkit.Framework.Clients.Wiki</Namespace>
  <Namespace>StardewModdingAPI.Toolkit.Framework.ModScanning</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

/*

See documentation at https://github.com/Pathoschild/StardewScripts.

*/
#load "Utilities/ConsoleHelper.linq"
#load "Utilities/FileHelper.linq"
#load "Utilities/IncrementalProgressBar.linq"
#load "Utilities/ModDumpManager.linq"

/*********
** Configuration
*********/
/// <summary>The mod site clients from which to fetch mods.</summary>
readonly IModSiteClient[] ModSites = new IModSiteClient[]
{
	new CurseForgeApiClient(
		apiKey: null
	),
	new ModDropApiClient(
		username: null,
		password: null
	),
	new NexusApiClient(
		apiKey: "",
		appName: "Pathoschild",
		appVersion: "1.0.0"
	)
};

/// <summary>The path in which to store cached data.</summary>
readonly ModDumpManager ModDump = new(
	rootPath: @"C:\dev\mod-dump",
	resetUnpacked: false
);

/// <summary>The mods folder to which mods are copied when you click 'install mod'.</summary>
readonly string InstallModsToPath = @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods (test)";

/// <summary>Which mods to refetch from the mod sites (or <c>null</c> to not refetch any).</summary>
readonly Func<IModSiteClient, Task<int[]>> FetchMods =
	null;
	//site => site.GetModsUpdatedSinceAsync(new DateTimeOffset(new DateTime(2024, 04, 25), TimeSpan.Zero)); // since last run
	//site => site.GetModsUpdatedSinceAsync(DateTimeOffset.UtcNow - TimeSpan.FromDays(14));
	//site => site.GetPossibleModIdsAsync(startFrom: null);

/// <summary>Mods to ignore when validating mods or compiling statistics.</summary>
readonly ModSearch[] IgnoreForAnalysis = [
	/*********
	** CurseForge mods
	*********/
	#region CurseForge mods
	// mods marked abandoned
	..ModSearch.ForSiteIds(ModSite.CurseForge,
		308067, // Custom Furniture
		393984, // Easy Prairie King
		309743, // Pelican TTS
		308058, // Plan Importer
		308062, // Portraiture
		306738, // PyTK
		310789, // Scale Up
		307726, // Seed Bag
		310737, // Ship From Inventory
		307654, // The Harp of Yoba
		306750  // TMXL Map Toolkit
	),

	// mods which include a copy of another mod for some reason
	new(ModSite.CurseForge, 877227, manifestId: "Platonymous.PlatoUI"), // Plato Warp Menu

	// mod packs
	..ModSearch.ForSiteIds(ModSite.CurseForge,
		953780, // The First Modpack
		987052  // Morning Dew
	),
	
	#endregion


	/*********
	** ModDrop mods
	*********/
	#region ModDrop mods
	// mod translations
	..ModSearch.ForSiteIds(ModSite.ModDrop,
		1264767, // Always Raining in the Valley (es)
		1258337, // East Scarp (es)
		1190770, // Extra Fish Information (es)
		1258338, // Juliet and Jessie the Joja Clerks (es)
		1258750, // Lavril (es)
		1258746, // Mister Ginger Cat NPC (es)
		1267948, // Nagito - Custom NPC (es)
		1264764, // Ridgeside Village (es)
		1190547, // Stardew Valley Expanded (es)
		1264765, // The Ranch Expansion Marnie and Jas (es)
		1264766, // The Robin Romance Mod (es)
		1258745  // Tristan (es)
	),

	// reposts
	..ModSearch.ForSiteIds(ModSite.ModDrop,
		509776, // Object Progress Bars
		509780  // Running Late
	),

	// deleted mods (ModDrop keeps a readonly version of the deleted page)
	..ModSearch.ForSiteIds(ModSite.ModDrop,
		1287339 // MARGO - Modular Gameplay Overhaul
	),

	// special cases
	..ModSearch.ForSiteIds(ModSite.ModDrop,
		580803, // PPJA Home of Abandoned Mods - CFR Conversions
		624116, // Sprint Sprint Sprint, replaced by Sprint Sprint
		1034925 // Better Tappers, duplicate mod page
	),

	// mods which include a copy of another mod for some reason
	new(ModSite.ModDrop, 1240025, manifestId: "Cherry.ExpandedPreconditionsUtility"), // Little Witch Academia Constanze NPC
	new(ModSite.ModDrop, 1240025, manifestId: "Cherry.ShopTileFramework"),            // Little Witch Academia Constanze NPC
	new(ModSite.ModDrop, 1240025, manifestId: "Pathoschild.ContentPatcher"),          // Little Witch Academia Constanze NPC
	new(ModSite.ModDrop, 1240025, manifestId: "Platonymous.CustomMusic"),             // Little Witch Academia Constanze NPC
	new(ModSite.ModDrop, 1163121, manifestId: "Cherry.ExpandedPreconditionsUtility"), // Little Witch Academia Constanze NPC - (SVE)
	new(ModSite.ModDrop, 1163121, manifestId: "Cherry.ShopTileFramework"),            // Little Witch Academia Constanze NPC - (SVE)
	new(ModSite.ModDrop, 1163121, manifestId: "Pathoschild.ContentPatcher"),          // Little Witch Academia Constanze NPC - (SVE)
	new(ModSite.ModDrop, 1163121, manifestId: "Platonymous.CustomMusic"),             // Little Witch Academia Constanze NPC - (SVE)
	new(ModSite.ModDrop, 1565408, manifestId: "CJBok.CheatsMenu"),                    // "Stardew Valley Expanded Pro" mod pack
	new(ModSite.ModDrop, 1565408, manifestId: "Pathoschild.ContentPatcher"),          // "Stardew Valley Expanded Pro" mod pack
	new(ModSite.ModDrop, 1565408, manifestId: "Esca.FarmTypeManager"),                // "Stardew Valley Expanded Pro" mod pack
	new(ModSite.ModDrop, 1565408, manifestId: "CaptainSully.InfiniteWateringCan"),    // "Stardew Valley Expanded Pro" mod pack
	new(ModSite.ModDrop, 1565408, manifestId: "FlashShifter.SVECode"),                // "Stardew Valley Expanded Pro" mod pack
	#endregion


	/*********
	** ModDrop files
	*********/
	#region ModDrop files
	// broken manifest
	new(ModSite.ModDrop, 580762, 711129), // A Toned Down Stardew Valley, missing comma

	// legacy pre-standardization content packs (SI = Seasonal Immersion)
	new(ModSite.ModDrop, 811243, 820685), // Dutch Farm Buildings, for SI

	// mods which include a copy of another mod for some reason
	new(ModSite.ModDrop, 793920, manifestId: "Platonymous.PlatoUI"), // Plato Warp Menu
	#endregion


	/*********
	** Nexus mods
	*********/
	#region Nexus mods
	..ModSearch.ForSiteIds(ModSite.Nexus,
		// mod translations
		11463, // Always Raining in the Valley (es)
		18036, // Alternate Textures (fr)
		17337, // Ancient History - A Museum Expansion mod (es)
		7932,  // Animals Need Water (fr)
		16289, // Better Juninos (fr)
		18968, // Better Juninos (uk)
		20406, // Better Juninos (vh)
		20767, // Bigger Backpack (es)
		21548, // Bigger Backpack (vh)
		19337, // Buff Framework - Better Together - SVE Spouse Buffs (vh)
		11417, // Bug Net (fr)
		14960, // Bus Locations (fr)
		15665, // Cape Stardew (es)
		14724, // Child Age Up (id)
		5879,  // Child Age Up (zh)
		18168, // Child to NPC (ru/uk)
		14119, // CJB Cheats Menu (es)
		17430, // CJB Cheats Menu (vi)
		17649, // CJB Cheats Menu (vi)
		17657, // CJB Cheats Menu (vi)
		4305,  // Climates of Ferngill (pt)
		4197,  // Companion NPCs (pt)
		15932, // Convenient Inventory (pt)
		14723, // Cooking Skill (ru)
		17789, // Crop Harvest Bubbles (pt)
		9920,  // Crop Regrowth and Perennial Crops (pt)
		18035, // Customize Anywhere (fr)
		12968, // Custom NPC Belos (id)
		5811,  // Custom NPC Riley (de)
		14548, // Custom NPC Riley (tr)
		11851, // Custom Spouse Patio Redux (zh)
		20428, // Daily Tasks Report (ru)
		21168, // Deluxe Grabber Redux (zh)
		15007, // Deluxe Journal (fr)
		22509, // Distant Lands - A Small Witch Swamp Expansion (es)
		15908, // Downtown Zuzu (fr)
		18857, // Downtown Zuzu (id)
		11090, // Downtown Zuzu (it)
		9901,  // Downtown Zuzu (ru)
		18313, // Downtown Zuzu (th)
		20556, // Dusty Overhaul - SVE (es)
		5396,  // Dwarvish (pt)
		5428,  // Dwarvish (zh)
		10626, // East Scarp (es)
		8784,  // East Scarpe (pt)
		10967, // Extra Fish Information (fr)
		15717, // Extended Minecart (tr)
		17027, // Farm Helper (es)
		12045, // Farmer Helper (ru)
		12097, // Farmer Helper (tr)
		18034, // Fashion Sense (fr)
		13106, // Festival of the Mundane (zh)
		13165, // Fishing Trawler (vi)
		15286, // Fireworks Festival (zh)
		6157,  // Garden Village Shops (ru)
		6500,  // Garden Village Shops (ru)
		18721, // Generic Mod Config Menu (vi)
		18722, // Greenhouse Sprinklers (vi)
		9874,  // Happy Birthday (fr)
		4693,  // Happy Birthday (pt)
		6693,  // Happy Birthday (pt)
		9117,  // Happy Birthday (ru)
		6111,  // Immersive Characters - Shane (es)
		12399, // Instant Tool Upgrades (tr)
		17798, // Joys of Efficiency (vi)
		10685, // Juliet and Jessie the Joja Clerks (es)
		8946,  // Junimo Dialog (pt)
		11282, // Lavril (es)
		15624, // LewdDew Valley (zh)
		13866, // Line Sprinklers (fr)
		17797, // Loan Mod (vi)
		9143,  // Lookup Anything (id)
		22518, // Lookup Anything (id)
		18723, // Lookup Anything (vi)
		10720, // Loved Labels (pl)
		20568, // Loved Labels (pt)
		18253, // Love Festival (ru)
		22788, // Love Festival (th)
		18150, // Love Festival (tr)
		4339,  // Lunar Disturbances (pt)
		7082,  // Lunar Disturbances (pt)
		4265,  // Magic (pt)
		18747, // Magic (vi)
		18746, // Mana Bar (vi)
		15183, // Mermaid Island (es)
		10804, // Mister Ginger Cat NPC (es)
		16659, // Mobile Catalogues (vi)
		10307, // Mobile Phone (pt)
		16658, // Mobile Phone (vi)
		11844, // Mobile Phone (zh)
		20109, // More Lively Sewer Overhaul (ru)
		15180, // More New Fish (es)
		16987, // More Rings (ru)
		18252, // Multiple Spouses (th)
		10224, // Multiple Spouses (zh)
		14478, // Never Ending Adventure - NPC Mateo (es)
		6295,  // Nice Messages (ru)
		8928,  // Multiple Spouse Dialogs (tr)
		15327, // New Years Eve (tr)
		5551,  // NPC Adventures (ru)
		8767,  // NPC Adventures (tr)
		13369, // NPC Map Locations (vi)
		17659, // NPC Map Locations (vi)
		14437, // NPC Map Locations (zh)
		14878, // Ornithologist's Guild (ru)
		8696,  // Personal Effects Redux (pt)
		14821, // Personal Effects Redux (pt)
		13244, // PPJA (vi)
		5329,  // Prismatic Tools (pt)
		20395, // Prismatic Tools (multiple languages)
		11407, // Producer Framework Mod (fr)
		18010, // Resource Storage (pt)
		8030,  // Ridgeside Village (es)
		9942,  // Ridgeside Village (fr)
		19377, // Ridgeside Village (th)
		18829, // Ridgeside Village (vi)
		8170,  // Riley (de)
		10349, // Robin Romance (es)
		16432, // Rodney - a new NPC for East Scarp
		6096,  // Sailor Moon Hairstyles Clothing and Kimono (zh)
		16399, // Self Service (pt)
		17658, // Self Service (vi)
		20993, // Self Service for 1.6 (pt)
		21102, // Self Service for 1.6 (tr)
		18378, // Shiko NPC (id)
		18662, // Shopping Show (vi)
		18453, // Show Birthday (pt)
		14373, // Socializing Skill (vi)
		19242, // Socializing Skill (vi)
		11140, // Spouses in Ginger Island (zh)
		20244, // Stardew Notifications (tr)
		18691, // Stardew Realty (pt)
		5259,  // Stardew Valley Expanded (de)
		5272,  // Stardew Valley Expanded (es)
		5509,  // Stardew Valley Expanded (es)
		5901,  // Stardew Valley Expanded (fr)
		8411,  // Stardew Valley Expanded (fr)
		12867, // Stardew Valley Expanded (fr)
		5788,  // Stardew Valley Expanded (ja)
		4206,  // Stardew Valley Expanded (pt)
		6332,  // Stardew Valley Expanded (tr)
		8143,  // Stardew Valley Expanded (zh)
		19243, // Survivalist Skill (vi)
		10221, // The Ranch Expansion Marnie and Jas (es)
		17727, // Time Before Harvest Enhanced (vi)
		8312,  // Town School Functions (tr)
		6356,  // Town School Functions (zh)
		17666, // Tree Transplant (vi)
		10785, // Tristan (es)
		14398, // Tristan (es)
		7556,  // UI Info Suite (fr)
		15208, // UI Info Suite 2 (vi)
		13389, // UI Info Suite (vi)
		6637,  // Underground Secrets (ru)
		22366, // Visit Mount Vapius (es)
		17684, // What Are You Missing (vi)

		// reposts
		22479, // Auto-Eat
		20781, // Crop Harvest Bubbles
		20783, // Crop Variation
		21428, // Crop Watering Bubbles
		22678, // Dialogue Display Framework
		12920, // Extra Map Layers (version for Android by original author, with same mod ID)
		20910, // Farm Cave Framework
		20726, // Fish Spot Bait
		11297, // Friends Forever
		20702, // Friends Forever
		20723, // Gift Rejection
		21766, // Help Wanted
		21286, // Informant
		21635, // Junimos Accept Cash
		21285, // Like a Duck to Water
		21068, // Mailbox Menu
		12729, // Many Enchantments
		16921, // More Random Edition
		19817, // Multiplayer for Mobile
		22257, // Napalm Mummies
		12369, // Never Ending Adventure asd Circle of Thorns - NPCs Mateo and Hector
		19291, // Night Owl Repacked
		20869, // No Fence Decay
		20706, // Plant and Fertilize All
		1427,  // Prairie King Made Easy
		10916, // Qi Exchanger
		887,   // Reseed
		9350,  // Reset Terrain Features
		1363,  // Save Anywhere
		8386,  // Save Anywhere
		9128,  // Shop Tile Framework
		22275, // Show Item Quality
		22167, // Sprinkler Mod
		22763, // Tilemap Challenge
		1077,  // UI Mod Suite
		19879, // Virtual Keyboard
		21054, // Yet Another Jump Mod

		// files to drop into another mod's folder
		18729, // Bun's Datable Jodi Portraits (replaces files in Datable Jodi)
		14360, // Facelift for CC's Horse Plus (replaces files in CC's Horse Plus)

		// newer versions uploaded to a new page for some reason
		3941,  // Daily Planner
		8876,  // Map Editor (replaced by Map Editor Extended with the same mod ID)
		2676,  // PokeMania
		444,   // Save Anywhere (replaced by Save Anywhere Redux at Nexus:8386 with the same mod ID)
		3294,  // Sprint Sprint Sprint (replaced by Sprint Sprint)

		// other
		19079, // Lusif1's NPC Template (not a mod itself, instructions + template for creating a mod)
		17262, // Stardrop Quick Start (not a mod itself, just has dependencies)
		19905  // XNB Archive (not a mod)
	),
	#endregion


	/*********
	** Nexus files
	*********/
	#region Nexus files
	// broken manifests
	new(ModSite.Nexus, 1632, 10352),  // Birthstone Plants, missing comma
	new(ModSite.Nexus, 4686, 19998),  // Clint Removes Apron
	new(ModSite.Nexus, 19007, 78060), // Cooler Inverted Emotes, missing quote
	new(ModSite.Nexus, 4608, 22971),  // DC Burget Krobus for CP
	new(ModSite.Nexus, 10800, 49796), // Dodo's Dwarf replacement
	new(ModSite.Nexus, 30, 279),      // Enemy Health Bars, Storm mod
	new(ModSite.Nexus, 17346, 73218), // Ghosties Wildfood Retexture
	new(ModSite.Nexus, 18027, 75111), // Guinea Pig Pet for Alternative Textures
	new(ModSite.Nexus, 17279, 73016), // Helpful Spouses (pt)
	new(ModSite.Nexus, 17705, 74245), // HxW Fairy Garden Furniture - Gnome Pack
	new(ModSite.Nexus, 2602, 10660),  // katkatpixels Portrait Overhauls, missing UniqueID field in ContentPackFor
	new(ModSite.Nexus, 5202, 22886),  // Minecraft Mobs as Rarecrows, missing quote
	new(ModSite.Nexus, 237, 929),     // No Soil Decay, invalid version "0.0.0"
	new(ModSite.Nexus, 10463, 48639), // Ouranio Recordings Music Pack, Custom Music pack with a SMAPI manifest
	new(ModSite.Nexus, 7600, 36539),  // Pink Tools Recolor, missing quotes in update keys
	new(ModSite.Nexus, 19572, 79639), // Prettier Cherry Tree, empty file
	new(ModSite.Nexus, 19824, 80426), // Roxullinar Cyanillar Fusionillar, various issues
	new(ModSite.Nexus, 16448, 70244), // S
	new(ModSite.Nexus, 366, 2949),    // Siv's Marriage Mod, invalid version "0.0.0"
	new(ModSite.Nexus, 1048, 3757),   // SmartMod, invalid version "0.0.0"
	new(ModSite.Nexus, 6284, 28109),  // Upgraded Seed Maker Fantasy Crops Addon, missing comma
	new(ModSite.Nexus, 18388, 76292), // Van NPC
	new(ModSite.Nexus, 5881, 26283),  // Void Pendant Replacer, UpdateKeys has {} instead of []
	new(ModSite.Nexus, 5558, 24942),  // Zen Garden Desert Obelisk, unescaped quote in string

	// utility mods that are part of a larger mod
	new(ModSite.Nexus, 2677, 14752), // Always On Server for Multiplayer > Server Connection Reset
	new(ModSite.Nexus, 2364, 9477),  // Even More Secret Woods > Bush Reset
	new(ModSite.Nexus, 1008, 3858),  // Hope's Farmer Customization Mods > Hope's Character Customization Mods Improved [Demiacle.ExtraHair]
	new(ModSite.Nexus, 3355, 14167), // Village Map Mod > Village Console Commands
	new(ModSite.Nexus, 20250, manifestId: "MindMeltMax.SAMLTest"), // demo for main mod

	// legacy pre-standardization content packs (ALL = Advanced Location Loader, SI = Seasonal Immersion)
	new(ModSite.Nexus, 3713, 15421), // BathHouse Apartment for ALL
	new(ModSite.Nexus, 3713, 15423), // BathHouse Apartment for ALL
	new(ModSite.Nexus, 1032, 5771),  // Bus Interior Restored for ALL
	new(ModSite.Nexus, 1980, 7425),  // Earth and Water Obelisks for SI
	new(ModSite.Nexus, 1980, 7426),  // Earth and Water Obelisks for SI
	new(ModSite.Nexus, 1980, 7427),  // Earth and Water Obelisks for SI
	new(ModSite.Nexus, 1980, 7428),  // Earth and Water Obelisks for SI
	new(ModSite.Nexus, 1980, 7429),  // Earth and Water Obelisks for SI
	new(ModSite.Nexus, 1980, 7430),  // Earth and Water Obelisks for SI
	new(ModSite.Nexus, 806, 5996),   // Expanded Crevices for ALL
	new(ModSite.Nexus, 588, 3033),   // Extended Cellar for ALL
	new(ModSite.Nexus, 588, 3083),   // Extended Cellar for ALL
	new(ModSite.Nexus, 1467, 5656),  // F-SV Stable for SI
	new(ModSite.Nexus, 1014, 3650),  // Jungle Temple for ALL
	new(ModSite.Nexus, 864, 3149),   // Orbitz for ALL
	new(ModSite.Nexus, 904, 3208),   // Organized Corrosion Detection for ALL
	new(ModSite.Nexus, 928, 4752),   // Seasonal Vanilla Buildings for SI
	new(ModSite.Nexus, 835, 3030),   // VIP Visual Improvement Program for ALL
	new(ModSite.Nexus, 835, 3207),   // VIP Visual Improvement Program for ALL
	new(ModSite.Nexus, 835, 5994),   // VIP Visual Improvement Program for ALL
	new(ModSite.Nexus, 1593, 5998),  // Wax Key for ALL

	// mods which include a copy of another mod for some reason
	new(ModSite.Nexus, 11228, manifestId: "cat.betterfruittrees"),              // Better Fruit Trees
	new(ModSite.Nexus, 8097, manifestId: "Paritee.BetterFarmAnimalVariety"),    // Cotton the Sweetest Shopkeeper
	new(ModSite.Nexus, 3496, manifestId: "Esca.FarmTypeManager"),               // Farm Extended
	new(ModSite.Nexus, 6029, manifestId: "Cherry.ToolUpgradeCosts"),            // Hardew Valley
	new(ModSite.Nexus, 6029, manifestId: "jahangmar.LevelingAdjustment"),       // Hardew Valley
	new(ModSite.Nexus, 6029, manifestId: "jahangmar.LevelingAdjustment2"),      // Hardew Valley
	new(ModSite.Nexus, 8563, manifestId: "spacechase0.CustomNPCFixes"),         // Harvest Valley Farm
	new(ModSite.Nexus, 16426, manifestId: "alja.CCCB"),                         // Little Harder Community Center Bundles
	new(ModSite.Nexus, 13248, manifestId: "Stashek.FishingRodRecolor"),         // Main Questline Redux (PMC CCJ)
	new(ModSite.Nexus, 21954, manifestId: "alja.CCCB"),                         // Moonshine's Moderate Community Center Challange
	new(ModSite.Nexus, 1692, manifestId: "Platonymous.CustomElementHandler"),   // New NPC Alec
	new(ModSite.Nexus, 1692, manifestId: "Platonymous.CustomFarming"),          // New NPC Alec
	new(ModSite.Nexus, 1692, manifestId: "Platonymous.CustomFurniture"),        // New NPC Alec
	new(ModSite.Nexus, 1692, manifestId: "Platonymous.CustomNPC"),              // New NPC Alec
	new(ModSite.Nexus, 1128, manifestId: "Advize.GetDressed"),                  // New Shirts and 2 new Skirts
	new(ModSite.Nexus, 5384, manifestId: "Platonymous.PlatoUI"),                // Plato Warp Menu
	new(ModSite.Nexus, 11929, manifestId: "Paritee.BetterFarmAnimalVariety"),   // -RU- Dark Club
	new(ModSite.Nexus, 16904, manifestId: "RukaBravo.portrait.mod.vanilla"),    // RukaBravo's portrait mod (actually loads Portraiture.dll)
	new(ModSite.Nexus, 12069, manifestId: "Paritee.BetterFarmAnimalVariety"),   // -RU- Nude Farmer and Swimsuits
	new(ModSite.Nexus, 20593, manifestId: "Esca.FarmTypeManager"),              // Standard Farm Expanded
	new(ModSite.Nexus, 9509, manifestId: "jahangmar.LevelingAdjustment"),       // Stardew VallEasy - Easy Gold Everywhere
	new(ModSite.Nexus, 9509, manifestId: "jahangmar.LevelingAdjustment2"),      // Stardew VallEasy - Easy Gold Everywhere
	new(ModSite.Nexus, 2426, manifestId: "Ilyaki.ArtifactSystemFixed"),         // Unofficial Balance Patch
	new(ModSite.Nexus, 2426, manifestId: "BetterQuarry"),                       // Unofficial Balance Patch
	new(ModSite.Nexus, 2426, manifestId: "Nishtra.MiningAtTheFarm"),            // Unofficial Balance Patch
	new(ModSite.Nexus, 2426, manifestId: "KevinConnors.ProfessionAdjustments"), // Unofficial Balance Patch

	// downloads which replace files in other mods
	new(ModSite.Nexus, 19236, 78568), // Anime Catboy Portrait Mod - Wizard SVE
	new(ModSite.Nexus, 21191, 86404), // Auto Fish - Deutsch
	new(ModSite.Nexus, 19426, 79169), // Bun's Dateable (and Marry) Caroline Portraits
	new(ModSite.Nexus, 19698, 80076), // Vanilla Portrait Frame

	// special cases
	new(ModSite.Nexus, 12824, fileId: 79850),                             // Marry Morris (pre-1.6 version is C#, but newer versions are content packs)
	new(ModSite.Nexus, 15564, manifestId: "JefGrizli.RedrawPelicanTownC") // Redraw Pelican Town (C# component uploaded to both #14928 and #15564, so link it to the first one)
	#endregion
];

/// <summary>The <see cref="IgnoreForAnalysis"/> entries indexed by mod site/ID, like <c>"Nexus:2400"</c>.</summary>
private IDictionary<string, ModSearch[]> IgnoreForAnalysisBySiteId;


/*********
** Script
*********/
async Task Main()
{
	// build optimized mod search lookup
	this.IgnoreForAnalysisBySiteId = this.IgnoreForAnalysis
		.GroupBy(p => new { p.Site, p.SiteId })
		.ToDictionary(p => $"{p.Key.Site}:{p.Key.SiteId}", p => p.ToArray());

	// fetch compatibility list
	ConsoleHelper.Print("Fetching wiki compatibility list...");
	WikiModList compatList = await new ModToolkit().GetWikiCompatibilityListAsync();

	// fetch mods
	HashSet<string> unpackMods = new HashSet<string>();
	if (this.FetchMods != null)
	{
		ConsoleHelper.Print("Initializing clients...");
		foreach (var site in this.ModSites)
			await site.AuthenticateAsync();

		ConsoleHelper.Print("Fetching mods...");
		foreach (IModSiteClient modSite in this.ModSites)
		{
			// get mod IDs
			int[] modIds;
			while (true)
			{
				try
				{
					modIds = await this.FetchMods(modSite);
					break;
				}
				catch (RateLimitedException ex)
				{
					this.LogAndAwaitRateLimit(ex, modSite.SiteKey);
					continue;
				}
			}

			// fetch mods
			int[] imported = await this.ImportMods(modSite, modIds);
			foreach (int id in imported)
				unpackMods.Add(Path.Combine(modSite.SiteKey.ToString(), id.ToString()));
		}
	}

	// unpack fetched files
	ConsoleHelper.Print($"Unpacking mod folders...");
	this.ModDump.UnpackModDownloads();

	// read mod data
	ConsoleHelper.Print($"Reading mod folders...");
	ParsedMod[] mods = this.ModDump.ReadMods().ToArray();

	// add launch button
	new Hyperlinq(
		() => Process.Start(
			fileName: Path.Combine(this.InstallModsToPath, "..", "StardewModdingAPI.exe"),
			arguments: @$"--mods-path ""{Path.GetFileName(this.InstallModsToPath)}"""
		),
		"launch SMAPI"
	).Dump("actions");

	// run analyses
	ConsoleHelper.Print($"Running analyses...");
	{
		var notOnWiki = this.GetModsNotOnWiki(mods, compatList).ToArray();
		if (notOnWiki.Length > 0)
		{
			notOnWiki.Dump("SMAPI mods not on the wiki");
			new Lazy<dynamic>(() => Util.WithStyle(string.Join("\n", notOnWiki.Select(p => ((Lazy<string>)p.WikiEntry).Value)), "font-family: monospace;")).Dump("SMAPI mods not on the wiki (wiki format)");
		}
		else
			"none".Dump("SMAPI mods not on the wiki");
	}
	this.GetInvalidMods(mods).Dump("Mods marked invalid by SMAPI toolkit (except blacklist)");
	this.GetInvalidIgnoreModEntries(mods).Dump($"{nameof(IgnoreForAnalysis)} values which don't match any local mod");
	this.GetModTypes(mods).Dump("mod types");
	this.GetContentPatcherVersionUsage(mods).Dump("Content Patcher packs by format version");
}


/*********
** Common queries
*********/
/// <summary>Get SMAPI mods which aren't listed on the wiki compatibility list.</summary>
/// <param name="mods">The mods to check.</param>
/// <param name="compatList">The mod data from the wiki compatibility list.</param>
IEnumerable<dynamic> GetModsNotOnWiki(IEnumerable<ParsedMod> mods, WikiModList compatList)
{
	// fetch mods on the wiki
	ISet<string> manifestIDs = new HashSet<string>(compatList.Mods.SelectMany(p => p.ID), StringComparer.InvariantCultureIgnoreCase);
	IDictionary<ModSite, ISet<int>> siteIDs = new Dictionary<ModSite, ISet<int>>
	{
		[ModSite.CurseForge] = new HashSet<int>(compatList.Mods.Where(p => p.CurseForgeID.HasValue).Select(p => p.CurseForgeID.Value)),
		[ModSite.ModDrop] = new HashSet<int>(compatList.Mods.Where(p => p.ModDropID.HasValue).Select(p => p.ModDropID.Value)),
		[ModSite.Nexus] = new HashSet<int>(compatList.Mods.Where(p => p.NexusID.HasValue).Select(p => p.NexusID.Value))
	};

	// fetch report
	return (
		from mod in mods
		from folder in mod.ModFolders
		orderby mod.Name

		where
			folder.ModType == ModType.Smapi
			&& !string.IsNullOrWhiteSpace(folder.ModID)
			&& !this.ShouldIgnoreForAnalysis(mod.Site, mod.ID, folder.ID, folder.ModID)

		let wikiHasManifestId = manifestIDs.Contains(folder.ModID)
		let wikiHasSiteId = siteIDs[mod.Site].Contains(mod.ID)

		where (!wikiHasManifestId || !wikiHasSiteId)

		let manifest = folder.RawFolder.Manifest
		let names = this.GetModNames(folder, mod)
		let authorNames = this.GetAuthorNames(manifest, mod)
		let githubRepo = this.GetGitHubRepo(manifest, mod)
		let customSourceUrl = githubRepo == null
			? this.GetCustomSourceUrl(mod)
			: null

		let isModInstalled = Directory.Exists(Path.Combine(this.InstallModsToPath, folder.RawFolder.Directory.Name))

		let missingLabels = (new[] { !wikiHasManifestId ? "manifest ID" : null, !wikiHasSiteId ? "site ID" : null }).Where(p => p is not null).ToArray()

		select new
		{
			SitePage = new Hyperlinq(mod.PageUrl, $"{mod.Site}:{mod.ID}"),
			SiteName = mod.Name,
			SiteAuthor = mod.AuthorLabel != null && mod.AuthorLabel != mod.Author
				? $"{mod.Author}\n({mod.AuthorLabel})"
				: mod.Author,
			SiteVersion = SemanticVersion.TryParse(mod.Version, out ISemanticVersion siteVersion) ? siteVersion.ToString() : mod.Version,
			FileName = folder.DisplayName,
			FileCategory = folder.Type,
			folder.ModID,
			folder.ModVersion,
			Missing = Util.WithStyle(
				string.Join(", ", missingLabels),
				missingLabels.Length == 1 ? "color: red" : "" // highlight mods that are partly missing, which usually means outdated info
			),
			Actions = isModInstalled
				? "installed"
				: (object)Util.OnDemand(
					"install mod",
					() => new object[] // returning an array allows collapsing the log in the LINQPad output
					{
						Util.WithStyle(
							Util.VerticalRun(this.InstallMod(folder)),
							"font-style: monospace; font-size: 0.9em;"
						)
					}
				),
			Metadata = Util.OnDemand("expand", () => new
			{
				FileId = folder.ID,
				FileType = folder.ModType,
				UpdateKeys = Util.OnDemand("expand", () => manifest.UpdateKeys),
				Manifest = Util.OnDemand("expand", () => manifest),
				Mod = Util.OnDemand("expand", () => mod),
				Folder = Util.OnDemand("expand", () => folder)
			}),
			WikiEntry = new Lazy<string>(() => // can't be in Metadata since it's accessed by the main script
				"{{#invoke:SMAPI compatibility|entry\n"
				+ $"  |name    = {string.Join(", ", names)}\n"
				+ $"  |author  = {string.Join(", ", authorNames)}\n"
				+ $"  |id      = {manifest?.UniqueID}\n"
				+ (mod.Site == ModSite.CurseForge ? $"  |curse   = {mod.ID}\n" : "")
				+ (mod.Site == ModSite.ModDrop ? $"  |moddrop = {mod.ID}\n" : "")
				+ $"  |nexus   = {(mod.Site == ModSite.Nexus ? mod.ID.ToString() : "")}\n"
				+ $"  |github  = {githubRepo}\n"
				+ (customSourceUrl != null
					? $"  |source  = {customSourceUrl}\n"
					: ""
				)
				+ "}}"
			)
		}
	)
	.ToArray();
}

/// <summary>Copy a mod into the mods folder indicated by <see cref="InstallModsToPath"/>.</summary>
/// <param name="folder">The mod folder to install.</param>
private IEnumerable<object> InstallMod(ParsedFile folder)
{
	const string traceStyle = "opacity: 0.5";
	const string errorStyle = "color: red; font-weight: bold;";
	const string successStyle = "color: green;";

	// get paths
	DirectoryInfo fromDir = folder.RawFolder.Directory;
	DirectoryInfo toDir = new DirectoryInfo(Path.Combine(this.InstallModsToPath, fromDir.Name));
	yield return Util.WithStyle($"Installing mod:\n  - from: {fromDir.FullName};\n  - to: {toDir.FullName}.", traceStyle);
	if (toDir.Exists)
	{
		yield return Util.WithStyle($"Target mod folder already exists.", errorStyle);
		yield break;
	}

	// copy mod
	foreach (FileInfo file in fromDir.GetFiles("*", SearchOption.AllDirectories))
	{
		string relativePath = Path.GetRelativePath(fromDir.FullName, file.FullName);
		string toPath = Path.Combine(toDir.FullName, relativePath);

		Directory.CreateDirectory(Path.GetDirectoryName(toPath));
		File.Copy(file.FullName, toPath);
	}

	yield return Util.WithStyle("Done!", successStyle);
}

/// <summary>Get mods which the SMAPI toolkit marked as invalid or unparseable.</summary>
/// <param name="mods">The mods to check.</param>
IEnumerable<dynamic> GetInvalidMods(IEnumerable<ParsedMod> mods)
{
	return (
		from mod in mods

		let invalid = mod.ModFolders
			.Where(folder =>
				folder.ModType == ModType.Invalid
				&& folder.ModError != ModParseError.ManifestMissing // ignore non-mod files
				&& folder.ModError != ModParseError.EmptyFolder // contains only non-mod files (e.g. replacement PNG assets)
				&& !this.ShouldIgnoreForAnalysis(mod.Site, mod.ID, folder.ID, folder.ModID)
			)
			.ToArray()

		where invalid.Any()
		select new
		{
			mod.Name,
			mod.Author,
			mod.Version,
			mod.Updated,
			SitePage = new Hyperlinq(mod.PageUrl, $"{mod.Site}:{mod.ID}"),
			Data = new Lazy<object>(() => mod),
			InvalidFile = invalid.Select(parsedFile => new
			{
				parsedFile.ID,
				parsedFile.Type,
				parsedFile.DisplayName,
				parsedFile.Version,
				parsedFile.ModType,
				parsedFile.ModError,
				Data = new Lazy<object>(() => parsedFile),
				Manifest = new Lazy<string>(() =>
				{
					FileInfo file = new FileInfo(Path.Combine(parsedFile.RawFolder.Directory.FullName, "manifest.json"));
					return file.Exists
						? File.ReadAllText(file.FullName)
						: "<file not found>";
				}),
				ManifestError = new Lazy<string>(() => $"{parsedFile.RawFolder.ManifestParseError}\n{parsedFile.RawFolder.ManifestParseErrorText}"),
				FileList = new Lazy<string>(() => this.BuildFileList(parsedFile.RawFolder.Directory))
			})
		}
	)
	.ToArray();
}

/// <summary>Get entries in <see cref="IgnoreModsForValidation" /> which don't match any of the given mods.</summary>
/// <param name="mods">The mods to check.</param>
IEnumerable<dynamic> GetInvalidIgnoreModEntries(IEnumerable<ParsedMod> mods)
{
	// index known mods
	IDictionary<string, ParsedMod> modsByKey = mods.ToDictionary(mod =>$"{mod.Site}:{mod.ID}", StringComparer.OrdinalIgnoreCase);

	// show unknown entries
	var invalid = new List<(ModSearch Entry, string Reason, ParsedMod Mod)>();
	foreach (var pair in this.IgnoreForAnalysisBySiteId)
	{
		(string key, ModSearch[] entries) = pair;

		// fetch mod
		if (!modsByKey.TryGetValue(key, out ParsedMod mod))
		{
			foreach (var entry in entries)
				invalid.Add((entry, "Site ID not found", mod));
			continue;
		}

		// match against mod folders
		HashSet<int> fileIds = new(mod.Files.Select(p => p.ID));
		foreach (var entry in entries)
		{
			if (entry.FileId.HasValue && !fileIds.Contains(entry.FileId.Value))
				invalid.Add((entry, "File ID not found", mod));
			else if (!mod.ModFolders.Any(folder => entry.Matches(site: mod.Site, siteId: mod.ID, fileId: folder.ID, manifestId: folder.ModID)))
				invalid.Add((entry, "Mod folder data not matched", mod));
		}
	}

	return invalid
		.Select(p => new { p.Entry.Site, p.Entry.SiteId, p.Entry.FileId, p.Entry.ManifestId, Reason = p.Reason, Mod = new Lazy<ParsedMod>(() => p.Mod), Entry = new Lazy<ModSearch>(() => p.Entry) })
		.OrderBy(p => p.Site)
		.ThenBy(p => p.SiteId)
		.ThenBy(p => p.FileId);
}

/// <summary>Get the number of mods by type.</summary>
/// <param name="mods">The mods to check.</param>
IDictionary<string, int> GetModTypes(IEnumerable<ParsedMod> mods)
{
	const int minPerGroup = 100;

	// get mod id => name lookup
	IDictionary<string, string> namesById = mods
		.SelectMany(p => p.ModFolders)
		.Select(p => new { Id = p.ModID?.Trim(), Name = p.ModDisplayName })
		.Where(p => !string.IsNullOrWhiteSpace(p.Id) && !string.IsNullOrWhiteSpace(p.Name))
		.GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
		.ToDictionary(p => p.Key, p => p.First().Name, StringComparer.OrdinalIgnoreCase);
	if (namesById.ContainsKey("Paritee.BetterFarmAnimalVariety"))
		namesById["Paritee.BetterFarmAnimalVariety"] = "Better Farm Animal Variety"; // match format used in stats without the "Paritee's" prefix

	// get type priority for ID conflicts
	static int GetPriority(string type)
	{
		return type switch
		{
			"SMAPI" => 4,
			"content pack (Content Patcher)" => 3,
			_ when (type?.StartsWith("content pack") == true) => 2,
			"XNB" => 1,
			_ => -1
		};
	}

	// get count by type key
	var typesByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	foreach (ParsedMod mod in mods)
	{
		// get all the mods available to download from this page (including downloads which contain multiple submods)
		var submods =
			(
				from folder in mod.ModFolders
				where folder.ModType is not (ModType.Ignored or ModType.Invalid)

				let manifest = folder.RawFolder.Manifest
				let contentPackFor = manifest?.ContentPackFor?.UniqueID
				let type = folder.ModType switch
				{
					ModType.Smapi => "SMAPI",
					ModType.ContentPack => !string.IsNullOrWhiteSpace(contentPackFor) && namesById.TryGetValue(contentPackFor.Trim(), out string name)
						? $"content pack ({name})"
						: $"content pack ({contentPackFor?.Trim()})",
					ModType.Xnb => "XNB",
					_ => $"other ({folder.ModType})"
				}

				orderby GetPriority(type)
				select (folder, manifest, type)
			);
		if (!submods.Any())
			continue;

		// count submods by type
		bool hasNonXnb = submods.Any(p => p.type != "XNB");
		foreach ((ParsedFile folder, IManifest manifest, string type) in submods)
		{
			// special case: if the mod has both XNB and non-XNB components, ignore the XNB ones (they're generally old/alternative versions)
			if (type == "XNB" && hasNonXnb)
				continue;

			// get tracking key
			string key = !string.IsNullOrWhiteSpace(manifest?.UniqueID)
				? manifest.UniqueID.Trim()
				: $"{type}:{mod.Site}:{mod.ID}";

			// ignore duplicates by ID
			// (Each player can only install one mod with a given ID. If two mods have the same ID, we assume they're equivalent and count them once in priority order.)
			if (typesByKey.TryGetValue(key, out string prevType) && GetPriority(type) <= GetPriority(prevType))
				continue;

			// set type
			typesByKey[key] = type;
		}
	}

	// get counts
	var counts = typesByKey
		.GroupBy(p => p.Value, StringComparer.OrdinalIgnoreCase)
		.OrderByDescending(p => p.Key == "SMAPI")
		.ThenByDescending(p => p.Key == "content pack (Content Patcher)")
		.ThenByDescending(p => p.Key == "XNB")
		.ThenBy(p => p.Key)
		.ToDictionary(p => p.Key, p => p.Count(), StringComparer.OrdinalIgnoreCase);

	// merge content packs with < min usages
	{
		int mergedSum = 0;

		foreach (var pair in counts.Where(p => p.Value < minPerGroup).ToArray())
		{
			if (pair.Key.StartsWith("content pack ("))
			{
				mergedSum += pair.Value;
				counts.Remove(pair.Key);
			}
		}

		if (mergedSum > 0)
			counts[$"content pack (<{minPerGroup} usages)"] = mergedSum;
	}

	return counts;
}

/// <summary>Get the number of unique content packs by Content Patcher version.</summary>
/// <param name="mods">The mods to check.</param>
IDictionary<string, int> GetContentPatcherVersionUsage(IEnumerable<ParsedMod> mods)
{
	// get unique versions by content pack ID
	var modVersions = new Dictionary<string, ISemanticVersion>(StringComparer.OrdinalIgnoreCase);
	foreach (ParsedMod mod in mods)
	{
		foreach (ParsedFile folder in mod.ModFolders)
		{
			// parse manifest
			IManifest manifest = folder.RawFolder.Manifest;
			string id = manifest?.UniqueID?.Trim();
			string contentPackFor = manifest?.ContentPackFor?.UniqueID?.Trim();
			if (string.IsNullOrWhiteSpace(id) || !string.Equals(contentPackFor, "Pathoschild.ContentPatcher", StringComparison.OrdinalIgnoreCase))
				continue;

			// skip if content.json doesn't exist
			FileInfo contentFile = new FileInfo(Path.Combine(folder.RawFolder.Directory.FullName, "content.json"));
			if (!contentFile.Exists)
				continue;

			// extract format version
			ISemanticVersion format = null;
			try
			{
				var template = new { Format = "" };
				var rawContent = JsonConvert.DeserializeAnonymousType(File.ReadAllText(contentFile.FullName), template);
				if (!SemanticVersion.TryParse(rawContent?.Format, out format))
					continue;

				format = new SemanticVersion(format.MajorVersion, format.MinorVersion, 0);
			}
			catch (JsonException)
			{
				continue; // ignore invalid content.json
			}

			// track latest version
			if (!modVersions.TryGetValue(id, out ISemanticVersion prevVersion) || format.IsNewerThan(prevVersion))
				modVersions[id] = format;
		}
	}

	// get counts
	return modVersions
		.OrderBy(p => p.Value.MajorVersion)
		.ThenBy(p => p.Value.MinorVersion)
		.ThenBy(p => p.Value.PatchVersion)
		.GroupBy(p => p.Value.ToString())
		.ToDictionary(p => p.Key.ToString(), p => p.Count());
}

/// <summary>Get all mods which depend on the given mod.</summary>
/// <param name="parsedMods">The mods to check.</param>
/// <param name="modID">The dependency mod ID.</param>
IEnumerable<ModFolder> GetModsDependentOn(IEnumerable<ParsedMod> parsedMods, string modID)
{
	foreach (ParsedMod mod in parsedMods)
	{
		foreach (ModFolder folder in mod.ModFolders.Select(p => p.RawFolder))
		{
			bool dependency =
				folder.Manifest?.Dependencies?.Any(p => p.UniqueID?.Equals(modID, StringComparison.InvariantCultureIgnoreCase) == true) == true
				|| folder.Manifest?.ContentPackFor?.UniqueID?.Equals(modID, StringComparison.InvariantCultureIgnoreCase) == true;
			if (dependency)
				yield return folder;
		}
	}
}


/*********
** Implementation
*********/
/// <summary>Import data for matching mods.</summary>
/// <param name="modSite">The mod site API client.</param>
/// <param name="modIds">The mod IDs to try fetching.</param>
/// <returns>Returns the imported mod IDs.</returns>
async Task<int[]> ImportMods(IModSiteClient modSite, int[] modIds)
{
	Stopwatch timer = Stopwatch.StartNew();

	// get mod IDs
	if (!modIds.Any())
		return modIds;

	// fetch mods
	var progress = new IncrementalProgressBar(modIds.Length).Dump();
	foreach (int id in modIds)
	{
		// update progress
		progress.Increment();
		progress.Caption = $"Fetching {modSite.SiteKey} > mod {id} ({progress.Percent}%)";

		// fetch
		await this.ImportMod(modSite, id);
	}

	timer.Stop();
	progress.Caption = $"Fetched {modIds.Length} updated mods from {modSite.SiteKey} ({progress.Percent}%) in {ConsoleHelper.GetFormattedTime(timer.Elapsed)}";
	return modIds;
}

/// <summary>Import data for a mod.</summary>
/// <param name="modSite">The mod site API client.</param>
/// <param name="id">The unique mod ID.</param>
async Task ImportMod(IModSiteClient modSite, int id)
{
	while (true)
	{
		try
		{
			// fetch mod data
			GenericMod mod;
			while (true)
			{
				try
				{
					mod = await modSite.GetModAsync(id);
					break;
				}
				catch (KeyNotFoundException)
				{
					ConsoleHelper.Print($"Skipped mod {id} (HTTP 404).", Severity.Warning);
					return;
				}
				catch (RateLimitedException ex)
				{
					this.LogAndAwaitRateLimit(ex, modSite.SiteKey);
					continue;
				}
			}

			// save to cache
			while (true)
			{
				try
				{
					await this.DownloadAndCacheModDataAsync(modSite.SiteKey, mod, getDownloadLinks: async file => await modSite.GetDownloadUrlsAsync(mod, file));
					break;
				}
				catch (RateLimitedException ex)
				{
					this.LogAndAwaitRateLimit(ex, modSite.SiteKey);
					continue;
				}
			}
		}
		catch (Exception ex)
		{
			new { error = ex, response = ex is ApiException apiEx ? await apiEx.Response.AsString() : null }.Dump("error occurred");
			string choice = ConsoleHelper.GetChoice("What do you want to do? [r]etry, [s]kip, [a]bort", "r", "s", "a");
			if (choice == "r")
				continue; // retry
			else if (choice == "s")
				return; // skip
			else if (choice == "a")
				throw; // abort
			else
				throw new NotSupportedException($"Invalid choice: '{choice}'", ex);
		}
		break;
	}
}

/// <summary>Write mod data to the cache directory and download the available files.</summary>
/// <param name="siteKey">The mod site from which to fetch.</param>
/// <param name="mod">The mod data to save.</param>
/// <param name="getDownloadLinks">Get the download URLs for a specific file. If this returns multiple URLs, the first working one will be used.</param>
async Task DownloadAndCacheModDataAsync(ModSite siteKey, GenericMod mod, Func<GenericFile, Task<Uri[]>> getDownloadLinks)
{
	// reset cache folder
	this.ModDump.DeleteMod(siteKey, mod.ID.ToString());
	this.ModDump.SaveModData(mod);

	// save files
	using (WebClient downloader = new WebClient())
	{
		foreach (GenericFile file in mod.Files)
		{
			// download file from first working CDN
			FileInfo localFile = new FileInfo(this.ModDump.GetModDownloadPath(siteKey, mod.ID.ToString(), file));
			Queue<Uri> sources = new Queue<Uri>(await getDownloadLinks(file));
			while (true)
			{
				if (!sources.Any())
				{
					ConsoleHelper.Print($"File {mod.ID} > {file.ID} has no download sources available.", Severity.Error);
					ConsoleHelper.Print("You can optionally download it yourself:", Severity.Error);
					ConsoleHelper.Print($"   download from: {mod.PageUrl}", Severity.Error);
					ConsoleHelper.Print($"   download to:   {localFile.FullName}", Severity.Error);
					while (true)
					{
						if (ConsoleHelper.GetChoice("Do you want to [u]se a manually downloaded file or [s]kip this file?", "u", "s") == "u")
						{
							if (!File.Exists(localFile.FullName))
							{
								ConsoleHelper.Print($"No file found at {localFile.FullName}.", Severity.Error);
								continue;
							}

							ConsoleHelper.Print($"Using manually downloaded file.", Severity.Info);
						}
						else
							ConsoleHelper.Print($"Skipped file.", Severity.Info);
						break;
					}
					break;
				}

				Uri url = sources.Dequeue();
				try
				{
					downloader.DownloadFile(url, localFile.FullName);
					break;
				}
				catch (Exception ex)
				{
					string errorPhrase = $"Failed downloading mod {mod.ID} > file {file.ID} from {url}";
					if (sources.Any())
						ConsoleHelper.Print($"{errorPhrase}. Trying next CDN...\n{ex}", Severity.Warning);
					else
						ConsoleHelper.Print($"{errorPhrase}.\n{ex}", Severity.Error);
					Console.WriteLine();
				}
			}
		}
	}
}

/// <summary>Get the human-readable mod names for the compatibility list.</summary>
/// <param name="folder">The downloaded mod folder.</param>
/// <param name="mod">The mod metadata.</param>
private string[] GetModNames(ParsedFile folder, ParsedMod mod)
{
	// get possible names
	string[] names = new[] { folder.ModDisplayName?.Trim(), mod.Name?.Trim() }
		.Where(name => !string.IsNullOrWhiteSpace(name))
		.OrderBy(name => name)
		.Distinct(StringComparer.InvariantCultureIgnoreCase)
		.ToArray();

	// if both names are equivalent except for spacing and capitalization (e.g. SomeModName vs Some Mod Name), use the longer version
	if (names.Length == 2 && names[0].Replace(" ", "").ToLower() == names[1].Replace(" ", "").ToLower())
	{
		names = names
			.OrderByDescending(p => p.Length)
			.Take(1)
			.ToArray();
	}
	
	return names;
}

/// <summary>Get the human-readable mod author names for the compatibility list.</summary>
/// <param name="manifest">The downloaded mod manifest file.</param>
/// <param name="mod">The mod metadata.</param>
private string[] GetAuthorNames(IManifest manifest, ParsedMod mod)
{
	return new[] { manifest?.Author?.Trim(), mod.AuthorLabel?.Trim() ?? mod.Author?.Trim() }
		.SelectMany(field => field?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
		.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
		.Distinct(StringComparer.OrdinalIgnoreCase)
		.ToArray();
}

/// <summary>Get the GitHub repository name for a mod, if available.</summary>
/// <param name="manifest">The downloaded mod manifest file.</param>
/// <param name="mod">The mod metadata.</param>
private string GetGitHubRepo(IManifest manifest, ParsedMod mod)
{
	// from update key
	foreach (string rawUpdateKey in manifest?.UpdateKeys ?? Array.Empty<string>())
	{
		string updateKey = rawUpdateKey?.Trim();
		if (updateKey?.StartsWith("GitHub", StringComparison.OrdinalIgnoreCase) == true)
		{
			Match match = Regex.Match(updateKey, @"^GitHub\s*:\s*(.+)");
			if (match.Success)
				return match.Groups[1].Value;
		}
	}

	// from mod description
	string description = this.TryGetModDescription(mod);
	if (!string.IsNullOrWhiteSpace(description))
	{
		MatchCollection matches = Regex.Matches(description, @"github\.com/([a-z0-9_\-\.]+/[a-z0-9_\-\.]+)", RegexOptions.IgnoreCase);
		foreach (Match match in matches)
		{
			string repo = match.Groups[1].Value;

			if (repo.StartsWith("Pathoschild/", StringComparison.OrdinalIgnoreCase))
				continue; // this is usually not the mod's source link (e.g. links to the Content Patcher docs, or copy/pasted without changing the link)

			return repo;
		}
	}

	// none found
	return null;
}

/// <summary>Get the custom source code URL for a mod, if available.</summary>
/// <param name="mod">The mod metadata.</param>
private string GetCustomSourceUrl(ParsedMod mod)
{
	// from mod description
	string description = this.TryGetModDescription(mod);
	if (!string.IsNullOrWhiteSpace(description))
	{
		Match match = Regex.Match(description, @"(gitlab\.com/[a-z0-9_\-\.]+/[a-z0-9_\-\.]+|sourceforge\.net/p/[a-z0-9_\-\.]+)", RegexOptions.IgnoreCase);
		if (match.Success)
			return $"https://{match.Groups[1].Value}";
	}

	// none found
	return null;
}

/// <summary>Get the raw mod page description, if available.</summary>
/// <param name="mod">The mod metadata.</param>
private string TryGetModDescription(ParsedMod mod)
{
	// ModDrop
	{
		if (mod.RawData.TryGetValue("desc", out object rawValue) && rawValue is string description)
			return description;
	}

	// Nexus
	{
		if (mod.RawData.TryGetValue("Description", out object rawValue) && rawValue is string description)
			return description;
	}

	return null;
}

/// <summary>Build a human-readable file list for a directory path.</summary>
/// <param name="root">The directory for which to build a file list.</param>
public string BuildFileList(DirectoryInfo root)
{
	static IEnumerable<string> BuildEntries(FileSystemInfo entry, string indent = "")
	{
		// yield current
		string icon = entry is DirectoryInfo ? "📁" : "🗎";
		yield return $"{indent}{icon} {entry.Name}";

		// yield children
		if (entry is DirectoryInfo dir)
		{
			foreach (FileSystemInfo child in dir.EnumerateFileSystemInfos().OrderByDescending(p => p is FileInfo))
			{
				foreach (var subEntry in BuildEntries(child, $"{indent}    "))
					yield return subEntry;
			}
		}
	}

	return string.Join("\n", BuildEntries(root));
}

/// <summary>Get whether a given mod and file ID should be ignored when validating mods.</summary>
/// <param name="site">The mod site.</param>
/// <param name="siteId">The mod ID on the mod site.</param>
/// <param name="fileId">The file ID on the mod site.</param>
/// <param name="manifestId">The mod's manifest ID, if available.</param>
private bool ShouldIgnoreForAnalysis(ModSite site, int siteId, int fileId, string manifestId)
{
	return
		this.IgnoreForAnalysisBySiteId.TryGetValue($"{site}:{siteId}", out ModSearch[] entries)
		&& entries.Any(search => search.Matches(site: site, siteId: siteId, fileId: fileId, manifestId: manifestId));
}

/// <summary>Log a human-readable summary for a rate limit exception, and pause the thread until the rate limit is refreshed.</summary>
/// <param name="ex">The rate limit exception.</param>
/// <param name="site">The mod site whose rate limit was exceeded.</param>
private void LogAndAwaitRateLimit(RateLimitedException ex, ModSite site)
{
	TimeSpan resumeDelay = ex.TimeUntilRetry;
	DateTime resumeTime = DateTime.Now + resumeDelay;

	ConsoleHelper.Print($"{site} rate limit exhausted: {ex.RateLimitSummary}; resuming in {ConsoleHelper.GetFormattedTime(resumeDelay)} ({resumeTime:HH:mm:ss} local time).");
	Thread.Sleep(resumeDelay);
}

/// <summary>Matches a mod which should be ignored when validating mod data or compiling statistics.</summary>
class ModSearch
{
	/// <summary>The site which hosts the mod.</summary>
	public ModSite Site { get; }

	/// <summary>The mod's page ID in the site.</summary>
	public int SiteId { get; }

	/// <summary>The uploaded file ID, or <c>null</c> for any value.</summary>
	public int? FileId { get; }

	/// <summary>The mod's manifest ID, or <c>null</c> for any value.</summary>
	public string ManifestId { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="site">The site which hosts the mod.</param>
	/// <param name="siteId">The mod's page ID in the site.</param>
	/// <param name="fileId">The uploaded file ID, or <c>null</c> for any value.</param>
	/// <param name="manifestId">The mod's manifest ID, or <c>null</c> for any value.</param>
	public ModSearch(ModSite site, int siteId, int? fileId = null, string manifestId = null)
	{
		this.Site = site;
		this.SiteId = siteId;
		this.FileId = fileId;
		this.ManifestId = manifestId;
	}

	/// <summary>Get whether a given mod and file ID should be ignored when validating mods.</summary>
	/// <param name="site">The mod site.</param>
	/// <param name="siteId">The mod ID on the mod site.</param>
	/// <param name="fileId">The file ID on the mod site.</param>
	/// <param name="manifestId">The mod's manifest ID, if available.</param>
	public bool Matches(ModSite site, int siteId, int fileId, string manifestId)
	{
		return
			this.Site == site
			&& this.SiteId == siteId
			&& (this.FileId == null || this.FileId == fileId)
			&& (this.ManifestId == null || this.ManifestId == manifestId);
	}

	/// <summary>Get a set of mod search models for the same site.</summary>
	/// <param name="site">The mod site.</param>
	/// <param name="siteIds">The mod IDs on the mod site.</param>
	public static IEnumerable<ModSearch> ForSiteIds(ModSite site, params int[] siteIds)
	{
		return siteIds.Select(id => new ModSearch(site, id));
	}
}

/// <summary>An exception raised when API client exceeds the rate limits for an API.</summary>
class RateLimitedException : Exception
{
	/*********
	** Accessors
	*********/
	/// <summary>The amount of time to wait until it's safe to retry the request.</summary>
	public TimeSpan TimeUntilRetry { get; }

	/// <summary>A human-readable of current rate limit values, if available.</summary>
	public string RateLimitSummary { get; }


	/*********
	** Accessors
	*********/
	public RateLimitedException(TimeSpan timeUntilRetry, string rateLimitSummary)
		: base("Rate limits have been exceeded for this API.")
	{
		this.TimeUntilRetry = timeUntilRetry;
		this.RateLimitSummary = rateLimitSummary;
	}
}

/// <summary>A client which fetches mods from a particular mod site.</summary>
interface IModSiteClient
{
	/*********
	** Accessors
	*********/
	/// <summary>The identifier for this mod site used in update keys.</summary>
	ModSite SiteKey { get; }


	/*********
	** Methods
	*********/
	/// <summary>Authenticate with the mod site if needed.</summary>
	Task AuthenticateAsync();

	/// <summary>Get all mod IDs likely to exist. This may return IDs for mods which don't exist, but should return the most accurate possible range to reduce API queries.</summary>
	/// <param name="startFrom">The minimum mod ID to include.</param>
	/// <param name="endWith">The maximum mod ID to include.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	Task<int[]> GetPossibleModIdsAsync(int? startFrom = null, int? endWith = null);

	/// <summary>Get all mod IDs updated since the given date.</summary>
	/// <param name="startFrom">The minimum date from which to start fetching.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	Task<int[]> GetModsUpdatedSinceAsync(DateTimeOffset startFrom);

	/// <summary>Get a mod from the mod site API.</summary>
	/// <param name="id">The mod ID to fetch.</param>
	/// <exception cref="KeyNotFoundException">The mod site has no mod with that ID.</exception>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	Task<GenericMod> GetModAsync(int id);

	/// <summary>Get the download URLs for a file. If this returns multiple URLs, they're assumed to be mirrors and the first working URL will be used.</summary>
	/// <param name="mod">The mod for which to get download URLs.</param>
	/// <param name="file">The file for which to get download URLs.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	Task<Uri[]> GetDownloadUrlsAsync(GenericMod mod, GenericFile file);
}

/// <summary>A client which fetches mods from the CurseForge API.</summary>
class CurseForgeApiClient : IModSiteClient
{
	/*********
	** Fields
	*********/
	/// <summary>The CurseForge game ID for Stardew Valley.</summary>
	private int GameId = 669;

	/// <summary>A regex pattern which matches a version number in a CurseForge mod file name.</summary>
	private readonly Regex VersionInNamePattern = new Regex(@"^(?:.+? | *)v?(\d+\.\d+(?:\.\d+)?(?:-.+?)?) *(?:\.(?:zip|rar|7z))?$", RegexOptions.Compiled);

	/// <summary>The mod data fetched as part of a previous call.</summary>
	private readonly IDictionary<int, GenericMod> Cache = new Dictionary<int, GenericMod>();

	/// <summary>The CurseForge API client.</summary>
	private readonly IClient CurseForge;


	/*********
	** Accessors
	*********/
	/// <summary>The identifier for this mod site used in update keys.</summary>
	public ModSite SiteKey { get; } = ModSite.CurseForge;


	/*********
	** Public methods
	*********/
	/// <summary>Construct an instance.</summary>
	/// <param name="apiKey">The API key with which to authenticate.</param>
	public CurseForgeApiClient(string apiKey)
	{
		this.CurseForge = new FluentClient("https://api.curseforge.com/v1")
			.AddDefault(req => req.WithHeader("x-api-key", apiKey));
	}

	/// <summary>Authenticate with the mod site if needed.</summary>
	public Task AuthenticateAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>Get all mod IDs likely to exist. This may return IDs for mods which don't exist, but should return the most accurate possible range to reduce API queries.</summary>
	/// <param name="startFrom">The minimum mod ID to include.</param>
	/// <param name="endWith">The maximum mod ID to include.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public Task<int[]> GetPossibleModIdsAsync(int? startFrom = null, int? endWith = null)
	{
		return this.GetModsUpdatedSinceAsync(DateTimeOffset.MinValue);
	}

	/// <summary>Get all mod IDs updated since the given date.</summary>
	/// <param name="startFrom">The minimum date from which to start fetching.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public async Task<int[]> GetModsUpdatedSinceAsync(DateTimeOffset startFrom)
	{
		ISet<int> modIds = new HashSet<int>();

		const int pageSize = 50; // max page size allowed by CurseForge
		int page = 0;
		while (true)
		{
			// fetch data
			JObject response = await this.CurseForge
				.GetAsync("mods/search")
				.WithArguments(new
				{
					gameId = this.GameId,
					index = page,
					pageSize = pageSize,
					sortField = 3, // Last Updated
					sortOrder = "desc"
				})
				.AsRawJsonObject();

			// handle results
			bool reachedEnd = response.Count < pageSize;
			foreach (JObject rawMod in response["data"])
			{
				// parse mod
				GenericMod mod = this.Parse(rawMod);

				// check if we found all the mods we need
				if (modIds.Contains(mod.ID))
				{
					reachedEnd = true; // CurseForge starts repeating mods if we go past the end
					break;
				}
				if (mod.Updated < startFrom)
				{
					reachedEnd = true;
					break;
				}

				// add to list
				this.Cache[mod.ID] = mod;
				modIds.Add(mod.ID);
			}

			// handle pagination
			if (reachedEnd)
				break;
			page++;
		}

		return modIds.OrderBy(id => id).ToArray();
	}

	/// <summary>Get a mod from the mod site API.</summary>
	/// <param name="id">The mod ID to fetch.</param>
	/// <exception cref="KeyNotFoundException">The mod site has no mod with that ID.</exception>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public async Task<GenericMod> GetModAsync(int id)
	{
		if (this.Cache.TryGetValue(id, out GenericMod mod))
			return mod;

		JObject rawMod = await this.CurseForge
			.GetAsync($"addon/{id}")
			.AsRawJsonObject();

		return this.Cache[id] = this.Parse(rawMod);
	}

	/// <summary>Get the download URLs for a file. If this returns multiple URLs, they're assumed to be mirrors and the first working URL will be used.</summary>
	/// <param name="mod">The mod for which to get download URLs.</param>
	/// <param name="file">The file for which to get download URLs.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public Task<Uri[]> GetDownloadUrlsAsync(GenericMod mod, GenericFile file)
	{
		return Task.FromResult(
			GetDownloadUrls(file).Select(url => new Uri(url)).ToArray()
		);
	}


	/*********
	** Private methods
	*********/
	/// <summary>Get the download URLs for a CurseForge file.</summary>
	/// <param name="file">The file for which to get download URLs.</param>
	private IEnumerable<string> GetDownloadUrls(GenericFile file)
	{
		// API download URL
		string apiDownload = (string)file.RawData["downloadUrl"];
		if (apiDownload is not null)
			yield return apiDownload;

		// build CDN URL manually
		// The API doesn't always return a download URL.
		string fileId = file.ID.ToString();
		yield return $"https://mediafilez.forgecdn.net/files/{fileId.Substring(0, 4)}/{fileId.Substring(4)}/{file.FileName.Replace(' ', '+')}";
	}

	/// <summary>Parse raw mod data from the CurseForge API.</summary>
	/// <param name="rawMod">The raw mod data.</param>
	private GenericMod Parse(JObject rawMod)
	{
		// get author names
		string[] authorNames = rawMod["authors"].AsEnumerable().Select(p => p["name"].Value<string>()).ToArray();

		// get last updated
		DateTimeOffset lastUpdated;
		{
			DateTime created = rawMod["dateCreated"].Value<DateTime>();
			DateTime modified = rawMod["dateModified"].Value<DateTime>();
			DateTime released = rawMod["dateReleased"].Value<DateTime>();

			DateTime latest = created;
			if (modified > latest)
				latest = modified;
			if (released > latest)
				latest = released;

			lastUpdated = new DateTimeOffset(latest, TimeSpan.Zero);
		}

		// get files
		List<GenericFile> files = new List<GenericFile>();
		foreach (JObject rawFile in rawMod["latestFiles"].AsEnumerable())
		{
			string displayName = rawFile["displayName"].Value<string>();
			string fileName = rawFile["fileName"].Value<string>();

			files.Add(new GenericFile(
				id: rawFile["id"].Value<int>(),
				type: rawFile["releaseType"].Value<int>() == 1 ? GenericFileType.Main : GenericFileType.Optional, // FileReleaseType: 1=release, 2=beta, 3=alpha
				displayName: displayName,
				fileName: fileName,
				version: this.GetFileVersion(displayName, fileName),
				rawData: rawFile
			));
		}

		// get model
		JObject rawModWithoutFiles = (JObject)rawMod.DeepClone();
		rawModWithoutFiles.Property("latestFiles").Remove();

		return new GenericMod(
			site: ModSite.CurseForge,
			id: rawMod["id"].Value<int>(),
			name: rawMod["name"].Value<string>(),
			author: authorNames.FirstOrDefault(),
			authorLabel: authorNames.Length > 1 ? string.Join(", ", authorNames) : null,
			pageUrl: rawMod["links"]["websiteUrl"].Value<string>(),
			version: null,
			updated: lastUpdated,
			rawData: rawModWithoutFiles,
			files: files.ToArray()
		);
	}

	/// <summary>Get a raw version string for a mod file, if available.</summary>
	/// <param name="displayName">The file's display name.</param>
	/// <param name="fileName">The filename.</param>
	private string GetFileVersion(string displayName, string fileName)
	{
		Match match = this.VersionInNamePattern.Match(displayName);
		if (!match.Success)
			match = this.VersionInNamePattern.Match(fileName);

		return match.Success
			? match.Groups[1].Value
			: null;
	}
}

/// <summary>A client which fetches mods from the ModDrop API.</summary>
class ModDropApiClient : IModSiteClient
{
	/*********
	** Fields
	*********/
	/// <summary>The ModDrop game ID for Stardew Valley.</summary>
	private int GameId = 27;

	/// <summary>The mod data fetched as part of a previous call.</summary>
	private IDictionary<int, GenericMod> Cache = new Dictionary<int, GenericMod>();

	/// <summary>The ModDrop API client.</summary>
	private IClient ModDrop = new FluentClient("https://www.moddrop.com/api");

	/// <summary>The username with which to log in, if any.</summary>
	private readonly string Username;

	/// <summary>The password with which to log in, if any.</summary>
	private readonly string Password;


	/*********
	** Accessors
	*********/
	/// <summary>The identifier for this mod site used in update keys.</summary>
	public ModSite SiteKey { get; } = ModSite.ModDrop;


	/*********
	** Public methods
	*********/
	/// <summary>Construct an instance.</summary>
	/// <param name="username">The username with which to log in, if any.</param>
	/// <param name="password">The password with which to log in, if any.</param>
	public ModDropApiClient(string username, string password)
	{
		this.Username = username;
		this.Password = password;
	}

	/// <summary>Authenticate with the mod site if needed.</summary>
	public async Task AuthenticateAsync()
	{
		if (this.Username == null || this.Password == null)
			return;

		var response = await this.ModDrop
			.PostAsync("v1/auth/login")
			.WithBasicAuthentication(this.Username, this.Password)
			.AsRawJsonObject();

		string apiToken = response["apiToken"].Value<string>();
		if (string.IsNullOrEmpty(apiToken))
			throw new InvalidOperationException($"Authentication with the ModDrop API failed:\n{response.ToString()}");

		this.ModDrop.AddDefault(p => p.WithHeader("Authorization", apiToken));
	}

	/// <summary>Get all mod IDs likely to exist. This may return IDs for mods which don't exist, but should return the most accurate possible range to reduce API queries.</summary>
	/// <param name="startFrom">The minimum mod ID to include.</param>
	/// <param name="endWith">The maximum mod ID to include.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public Task<int[]> GetPossibleModIdsAsync(int? startFrom = null, int? endWith = null)
	{
		return this.GetModsUpdatedSinceAsync(DateTimeOffset.MinValue);
	}

	/// <summary>Get all mod IDs updated since the given date.</summary>
	/// <param name="startFrom">The minimum date from which to start fetching.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public async Task<int[]> GetModsUpdatedSinceAsync(DateTimeOffset startFrom)
	{
		ISet<int> modIds = new HashSet<int>();

		int offset = 0;
		while (true)
		{
			// fetch data
			JObject response = await this.ModDrop
				.GetAsync("v1/mods/search")
				.WithArguments(new
				{
					gameid = this.GameId,
					start = offset,
					order = "updated", // 'updated' or 'published'
					includeFiles = true
				})
				.AsRawJsonObject();

			// handle results
			int total = response["total"].Value<int>();
			JObject[] mods = response["mods"].Values<JObject>().ToArray();
			bool reachedEnd = mods.Length == 0 || offset + mods.Length >= total;

			foreach (JObject rawMod in mods)
			{
				// parse mod
				GenericMod mod = this.Parse(rawMod);

				// check if we found all the mods we need
				if (mod.Updated < startFrom)
				{
					reachedEnd = true;
					break;
				}

				// add to list
				this.Cache[mod.ID] = mod;
				modIds.Add(mod.ID);
			}

			// handle pagination
			if (reachedEnd)
				break;
			offset += mods.Length;
		}

		return modIds.OrderBy(id => id).ToArray();
	}

	/// <summary>Get a mod from the mod site API.</summary>
	/// <param name="id">The mod ID to fetch.</param>
	/// <exception cref="KeyNotFoundException">The mod site has no mod with that ID.</exception>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public async Task<GenericMod> GetModAsync(int id)
	{
		if (this.Cache.TryGetValue(id, out GenericMod mod))
			return mod;

		JObject rawMod = await this.ModDrop
			.GetAsync($"mods/data/{id}")
			.AsRawJsonObject();

		return this.Cache[id] = this.Parse(rawMod["mods"][$"{id}"].Value<JObject>());
	}

	/// <summary>Get the download URLs for a file. If this returns multiple URLs, they're assumed to be mirrors and the first working URL will be used.</summary>
	/// <param name="mod">The mod for which to get download URLs.</param>
	/// <param name="file">The file for which to get download URLs.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public async Task<Uri[]> GetDownloadUrlsAsync(GenericMod mod, GenericFile file)
	{
		try
		{
			var response = await this.ModDrop
				.PostAsync($"v1/mod-{mod.ID}/file-{file.ID}/download")
				.AsRawJsonObject();

			return new[]
			{
				new Uri(response["url"].Value<string>())
			};
		}
		catch (Exception ex)
		{
			string error = $"Can't fetch download URL for \"{mod.Name}\" (#{mod.ID}) > file \"{file.DisplayName} {file.Version}\" (#{file.ID}).";
			if (ex is ApiException apiEx)
				error += $"\n\nHTTP {apiEx.Response.Status}: {await apiEx.Response.AsString()}";
			error += $"\n\n{ex.ToString()}";

			ConsoleHelper.Print(error, Severity.Error);
			return new Uri[0];
		}
	}


	/*********
	** Private methods
	*********/
	/// <summary>Parse raw mod data from the ModDrop API.</summary>
	/// <param name="rawMod">The raw mod data.</param>
	private GenericMod Parse(JObject rawEntry)
	{
		JObject rawMod = rawEntry["mod"].Value<JObject>();
		JObject[] rawFiles = rawEntry["files"].Values<JObject>().ToArray();

		// get author names
		string author = rawMod["userName"].Value<string>()?.Trim();
		string authorLabel = rawMod["authorName"].Value<string>()?.Trim();
		if (author.Equals(authorLabel, StringComparison.InvariantCultureIgnoreCase))
			authorLabel = null;

		// get last updated
		DateTimeOffset lastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(rawMod["dateUpdated"].Value<long>());
		{
			DateTimeOffset published = DateTimeOffset.FromUnixTimeMilliseconds(rawMod["datePublished"].Value<long>());
			if (published > lastUpdated)
				lastUpdated = published;
		}

		// get files
		List<GenericFile> files = new List<GenericFile>();
		foreach (JObject rawFile in rawFiles)
		{
			try
			{
				if (rawFile["isOld"].Value<bool>() || rawFile["isDeleted"].Value<bool>() || rawFile["isHidden"].Value<bool>())
					continue;

				int id = rawFile["id"].Value<int>();
				string title = rawFile["title"]?.Value<string>();
				string version = rawFile["version"]?.Value<string>();
				string fileName = rawFile["fileName"].Value<string>();
				bool isMain = !rawFile["isPreRelease"].Value<bool>() && !rawFile["isAlternative"].Value<bool>();

				files.Add(new GenericFile(
					id: id,
					type: isMain ? GenericFileType.Main : GenericFileType.Optional,
					displayName: title,
					fileName: fileName,
					version: version,
					rawData: rawFile
				));
			}
			catch (Exception ex)
			{
				new { mod = new Lazy<JObject>(() => rawMod), files = new Lazy<JObject[]>(() => rawFiles), file = new Lazy<JObject>(() => rawFile) }.Dump();
				throw;
			}
		}

		// get model
		return new GenericMod(
			site: ModSite.ModDrop,
			id: rawMod["id"].Value<int>(),
			name: rawMod["title"].Value<string>(),
			author: author,
			authorLabel: authorLabel,
			pageUrl: rawMod["pageUrl"].Value<string>(),
			version: null,
			updated: lastUpdated,
			rawData: rawMod,
			files: files.ToArray()
		);
	}
}

/// <summary>A client which fetches mods from the Nexus Mods API.</summary>
class NexusApiClient : IModSiteClient
{
	/*********
	** Fields
	*********/
	/// <summary>The Nexus Mods game key for Stardew Valley.</summary>
	private readonly string GameKey = "stardewvalley";

	/// <summary>The underlying FluentNexus API client.</summary>
	private NexusClient Nexus;


	/*********
	** Accessors
	*********/
	/// <summary>The identifier for this mod site used in update keys.</summary>
	public ModSite SiteKey { get; } = ModSite.Nexus;


	/*********
	** Public methods
	*********/
	/// <summary>Construct an instance.</summary>
	/// <param name="apiKey">The Nexus API key with which to authenticate.</param>
	/// <param name="appName">An arbitrary name for the app/script using the client, reported to the Nexus Mods API and used in the user agent.</param>
	/// <param name="appVersion">An arbitrary version number for the <paramref name="appName" /> (ideally a semantic version).</param>
	public NexusApiClient(string apiKey, string appName, string appVersion)
	{
		this.Nexus = new NexusClient(apiKey, appName, appVersion);
	}

	/// <summary>Authenticate with the mod site if needed.</summary>
	public Task AuthenticateAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>Get all mod IDs likely to exist. This may return IDs for mods which don't exist, but should return the most accurate possible range to reduce API queries.</summary>
	/// <param name="startFrom">The minimum mod ID to include.</param>
	/// <param name="endWith">The maximum mod ID to include.</param>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public async Task<int[]> GetPossibleModIdsAsync(int? startFrom = null, int? endWith = null)
	{
		try
		{
			int minID = Math.Max(1, startFrom ?? 1);
			int maxID = endWith ?? (await this.Nexus.Mods.GetLatestAdded(this.GameKey)).Max(p => p.ModID);

			if (minID > maxID)
				return new int[0];

			return Enumerable.Range(minID, maxID - minID + 1).ToArray();
		}
		catch (ApiException ex) when (ex.Status == (HttpStatusCode)429)
		{
			throw await this.GetRateLimitExceptionAsync();
		}
	}

	/// <summary>Get all mod IDs updated since the given date.</summary>
	/// <param name="startFrom">The minimum date from which to start fetching.</param>
	public async Task<int[]> GetModsUpdatedSinceAsync(DateTimeOffset startFrom)
	{
		// calculate update period
		string updatePeriod = null;
		{
			DateTimeOffset now = DateTimeOffset.UtcNow;
			TimeSpan duration = now - startFrom;

			if (duration.TotalDays <= 1)
				updatePeriod = "1d";
			else if (duration.TotalDays <= 7)
				updatePeriod = "1w";
			else if (startFrom >= now.AddMonths(-1))
				updatePeriod = "1m";
			else
				throw new NotSupportedException($"The given date ({startFrom}) can't be used with {this.GetType().Name} because it exceeds the maximum update period of 28 days for the Nexus API.");
		}

		List<int> modIDs = new List<int>();
		foreach (ModUpdate mod in await this.Nexus.Mods.GetUpdated(this.GameKey, updatePeriod))
		{
			if (mod.LatestFileUpdate >= startFrom)
				modIDs.Add(mod.ModID);
		}
		return modIDs.ToArray();
	}

	/// <summary>Get a mod from the mod site API.</summary>
	/// <param name="id">The mod ID to fetch.</param>
	/// <exception cref="KeyNotFoundException">The mod site has no mod with that ID.</exception>
	/// <exception cref="RateLimitedException">The API client has exceeded the API's rate limits.</exception>
	public async Task<GenericMod> GetModAsync(int id)
	{
		try
		{
			// fetch mod data
			Mod nexusMod;
			try
			{
				nexusMod = await this.Nexus.Mods.GetMod(this.GameKey, id);
			}
			catch (ApiException ex) when (ex.Status == HttpStatusCode.NotFound)
			{
				throw new KeyNotFoundException($"There is no Nexus mod with ID {id}");
			}

			// fetch file data
			ModFile[] nexusFiles = nexusMod.Status == ModStatus.Published
				? (await this.Nexus.ModFiles.GetModFiles(this.GameKey, id, FileCategory.Main, FileCategory.Optional)).Files
				: new ModFile[0];

			// create file models
			GenericFile[] files = nexusFiles
				.Select(file =>
				{
					GenericFileType type = file.Category switch
					{
						FileCategory.Main => GenericFileType.Main,
						FileCategory.Optional => GenericFileType.Optional,
						_ => throw new InvalidOperationException($"Unknown file category from Nexus ({file.Category}) for mod id {id}")
					};
					return new GenericFile(id: file.FileID, type: type, displayName: file.Name, fileName: file.FileName, version: file.FileVersion, rawData: file);
				})
				.ToArray();

			// create mod model
			GenericMod mod;
			{
				string author = nexusMod.User?.Name ?? nexusMod.Author;
				string authorLabel = nexusMod.Author != null && !nexusMod.Author.Equals(author, StringComparison.InvariantCultureIgnoreCase)
					? nexusMod.Author
					: null;

				mod = new GenericMod(
					site: ModSite.Nexus,
					id: nexusMod.ModID,
					name: nexusMod.Name,
					author: author,
					authorLabel: authorLabel,
					pageUrl: $"https://www.nexusmods.com/stardewvalley/mods/{nexusMod.ModID}",
					version: nexusMod.Version,
					updated: nexusMod.Updated,
					rawData: nexusMod,
					files: files
				);
			}

			return mod;
		}
		catch (ApiException ex) when (ex.Status == (HttpStatusCode)429)
		{
			throw await this.GetRateLimitExceptionAsync();
		}
	}

	/// <summary>Get the download URLs for a file. If this returns multiple URLs, they're assumed to be mirrors and the first working URL will be used.</summary>
	/// <param name="mod">The mod for which to get download URLs.</param>
	/// <param name="file">The file for which to get download URLs.</param>
	public async Task<Uri[]> GetDownloadUrlsAsync(GenericMod mod, GenericFile file)
	{
		try
		{
			ModFileDownloadLink[] downloadLinks = await this.Nexus.ModFiles.GetDownloadLinks(this.GameKey, mod.ID, file.ID);
			return downloadLinks.Select(p => p.Uri).ToArray();
		}
		catch (ApiException ex) when (ex.Status == (HttpStatusCode)429)
		{
			throw await this.GetRateLimitExceptionAsync();
		}
	}


	/*********
	** Private methods
	*********/
	/// <summary>Get an exception indicating that rate limits have been exceeded.</summary>
	private async Task<RateLimitedException> GetRateLimitExceptionAsync()
	{
		IRateLimitManager rateLimits = await this.Nexus.GetRateLimits();
		TimeSpan unblockTime = rateLimits.GetTimeUntilRenewal();
		throw new RateLimitedException(unblockTime, this.GetRateLimitSummary(rateLimits));
	}

	/// <summary>Get a human-readable summary for the current rate limits.</summary>
	/// <param name="meta">The current rate limits.</param>
	private string GetRateLimitSummary(IRateLimitManager meta)
	{
		return $"{meta.DailyRemaining}/{meta.DailyLimit} daily resetting in {ConsoleHelper.GetFormattedTime(meta.DailyReset - DateTimeOffset.UtcNow)}, {meta.HourlyRemaining}/{meta.HourlyLimit} hourly resetting in {ConsoleHelper.GetFormattedTime(meta.HourlyReset - DateTimeOffset.UtcNow)}";
	}

	/// <summary>Get a human-readable formatted time span.</summary>
	/// <param name="span">The time span to format.</param>
	private string GetFormattedTime(TimeSpan span)
	{
		int hours = (int)span.TotalHours;
		int minutes = (int)span.TotalMinutes - (hours * 60);
		return $"{hours:00}:{minutes:00}";
	}
}
