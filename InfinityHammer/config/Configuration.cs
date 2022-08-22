using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using Service;
using UnityEngine;

namespace InfinityHammer;
public partial class Configuration {
#nullable disable
  public static bool IsCheats => Enabled && ((ZNet.instance && ZNet.instance.IsServer()) || Console.instance.IsCheatsEnabled());

  public static ConfigEntry<bool> configNoBuildCost;
  public static bool NoBuildCost => configNoBuildCost.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreWards;
  public static bool IgnoreWards => configIgnoreWards.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreNoBuild;
  public static bool IgnoreNoBuild => configIgnoreNoBuild.Value && IsCheats;
  public static ConfigEntry<bool> configNoStaminaCost;
  public static bool NoStaminaCost => configNoStaminaCost.Value && IsCheats;
  public static ConfigEntry<bool> configNoDurabilityLoss;
  public static bool NoDurabilityLoss => configNoDurabilityLoss.Value && IsCheats;
  public static ConfigEntry<bool> configAllObjects;
  public static bool AllObjects => configAllObjects.Value && IsCheats;
  public static ConfigEntry<bool> configCopyState;
  public static bool CopyState => configCopyState.Value && IsCheats;
  public static ConfigEntry<bool> configAllowInDungeons;
  public static bool AllowInDungeons => configAllowInDungeons.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreOtherRestrictions;
  public static bool IgnoreOtherRestrictions => configIgnoreOtherRestrictions.Value && IsCheats;
  public static ConfigEntry<bool> configRemoveAnything;
  public static bool RemoveAnything => configRemoveAnything.Value && IsCheats;
  public static ConfigEntry<bool> configDisableMessages;
  public static bool DisableMessages => configDisableMessages.Value;
  public static ConfigEntry<bool> configDisableSelectMessages;
  public static bool DisableSelectMessages => configDisableSelectMessages.Value;
  public static ConfigEntry<bool> configDisableOffsetMessages;
  public static bool DisableOffsetMessages => configDisableOffsetMessages.Value;
  public static ConfigEntry<bool> configDisableScaleMessages;
  public static bool DisableScaleMessages => configDisableScaleMessages.Value;
  public static ConfigEntry<bool> configChatOutput;
  public static bool ChatOutput => configChatOutput.Value;
  public static ConfigEntry<bool> configDisableLoot;
  public static bool DisableLoot => configDisableLoot.Value && IsCheats;
  public static ConfigEntry<bool> configRepairAnything;
  public static bool RepairAnything => configRepairAnything.Value && IsCheats;
  public static ConfigEntry<bool> configEnableUndo;
  public static bool EnableUndo => configEnableUndo.Value && IsCheats;
  public static ConfigEntry<bool> configNoCreator;
  public static bool NoCreator => configNoCreator.Value && IsCheats;
  public static ConfigEntry<bool> configResetOffsetOnUnfreeze;
  public static bool ResetOffsetOnUnfreeze => configResetOffsetOnUnfreeze.Value;
  public static ConfigEntry<bool> configUnfreezeOnUnequip;
  public static bool UnfreezeOnUnequip => configUnfreezeOnUnequip.Value;
  public static ConfigEntry<bool> configUnfreezeOnSelect;
  public static bool UnfreezeOnSelect => configUnfreezeOnSelect.Value;
  public static ConfigEntry<string> configOverwriteHealth;
  public static float OverwriteHealth => IsCheats ? InfiniteHealth ? 1E30f : Helper.ParseFloat(configOverwriteHealth.Value, 0f) : 0f;
  public static ConfigEntry<string> configPlanBuildFolder;
  public static string PlanBuildFolder => configPlanBuildFolder.Value;
  public static ConfigEntry<string> configBuildShareFolder;
  public static string BuildShareFolder => configBuildShareFolder.Value;
  public static ConfigEntry<bool> configInfiniteHealth;
  public static bool InfiniteHealth => configInfiniteHealth.Value && IsCheats;
  public static ConfigEntry<bool> configCopyRotation;
  public static bool CopyRotation => configCopyRotation.Value && Enabled;
  public static ConfigEntry<string> configRemoveArea;
  public static float RemoveArea => Enabled ? Helper.ParseFloat(configRemoveArea.Value, 0f) : 0f;
  public static ConfigEntry<string> configSelectRange;
  public static float SelectRange => Enabled ? Helper.ParseFloat(configSelectRange.Value, 0f) : 0f;
  public static ConfigEntry<string> configRemoveRange;
  public static float RemoveRange => IsCheats ? Helper.ParseFloat(configRemoveRange.Value, 0f) : 0f;
  public static ConfigEntry<string> configRepairRange;
  public static float RepairRange => IsCheats ? Helper.ParseFloat(configRepairRange.Value, 0f) : 0f;
  public static ConfigEntry<string> configBuildRange;
  public static float BuildRange => IsCheats ? Helper.ParseFloat(configBuildRange.Value, 0f) : 0f;
  public static ConfigEntry<bool> configRemoveEffects;
  public static bool RemoveEffects => configRemoveEffects.Value && Enabled;
  public static ConfigEntry<bool> configRepairTaming;
  public static bool RepairTaming => configRepairTaming.Value && IsCheats;
  public static ConfigEntry<bool> configShowCommandValues;
  public static bool AlwaysShowCommand => configShowCommandValues.Value;
  public static ConfigEntry<bool> configHidePlacementMarker;
  public static bool HidePlacementMarker => configHidePlacementMarker.Value && Enabled;
  public static ConfigEntry<bool> configEnabled;
  public static bool Enabled => configEnabled.Value;
  public static ConfigEntry<bool> configServerDevcommandsUndo;
  public static bool ServerDevcommandsUndo => configServerDevcommandsUndo.Value;
  public static ConfigEntry<string> configRemoveBlacklist;
  public static HashSet<string> RemoveBlacklist = new();
  public static ConfigEntry<string> configSelectBlacklist;
  public static HashSet<string> SelectBlacklist = new();
  public static ConfigEntry<string> configHammerTools;
  public static HashSet<string> HammerTools = new();
  public static ConfigEntry<string> configHoeTools;
  public static HashSet<string> HoeTools = new();
  public static ConfigEntry<string> configMirrorFlip;
  public static HashSet<string> MirrorFlip = new();
  public static ConfigEntry<string> configDimensions;
  public static Dictionary<string, Vector3> Dimensions = new();
  public static ConfigWrapper Wrapper;
#nullable enable
  private static HashSet<string> ParseList(string value) => value.Split(',').Select(s => s.Trim().ToLower()).Where(s => s != "").ToHashSet();

  private static Dictionary<string, Vector3> ParseSize(string value) => value.Split('|').Select(s => s.Trim().ToLower()).Where(s => s != "")
    .Select(s => s.Split(',')).Where(split => split.Length == 4).ToDictionary(split => split[0], split => Parse.TryVectorXZY(split, 1));

  private static void UpdateTools() {
    HammerTools = ParseList(configHammerTools.Value);
    HoeTools = ParseList(configHoeTools.Value);
  }
  private static void UpdateMirrorFlip() {
    MirrorFlip = ParseList(configMirrorFlip.Value);
  }
  private static void UpdateDimensions() {
    Dimensions = ParseSize(configDimensions.Value);
  }
  private static string Format(float f) => f.ToString("0.##", CultureInfo.InvariantCulture);
  public static void SetDimension(string value, Vector3 size) {
    Dimensions[value.ToLower()] = size;
    configDimensions.Value = string.Join("|", Dimensions.Select(kvp => $"{kvp.Key},{Format(kvp.Value.x)},{Format(kvp.Value.z)},{Format(kvp.Value.y)}"));
  }
  public static void Init(ConfigWrapper wrapper) {
    var defaultDimensions = "abomination,9.07,9.2,6.93|abomination_ragdoll,8.76,9.12,7.18|acorn,0.3,0.27,0.18|amber,0.27,0.38,0.14|amberpearl,0.15,0.15,0.15|ancientseed,0.52,0.5,0.49|armorbronzechest,0.61,0.63,0.16|armorbronzelegs,0.61,0.63,0.16|armorfenringchest,0.61,0.63,0.16|armorfenringlegs,0.61,0.63,0.16|armorironchest,0.61,0.63,0.16|armorironlegs,0.61,0.63,0.16|armorleatherchest,0.61,0.63,0.16|armorleatherlegs,0.61,0.63,0.16|armorpaddedcuirass,0.64,0.63,0.28|armorpaddedgreaves,0.7,0.73,0.24|armorragschest,0.61,0.63,0.16|armorragslegs,0.61,0.63,0.16|armorrootchest,0.61,0.63,0.16|armorrootlegs,0.7,0.73,0.24|armorstand,0.55,0.3,1.93|armorstand_female,1.87,0.54,1.93|armorstand_male,1.88,0.54,1.93|armortrollleatherchest,0.61,0.63,0.16|armortrollleatherlegs,0.61,0.63,0.16|armorwolfchest,0.61,0.63,0.16|armorwolflegs,0.61,0.63,0.16|arrowfrost,0.15,1.72,0.15|arrowneedle,0.1,1.82,0.08|arrowpoison,0.15,1.72,0.15|atgeirblackmetal,0.3,2.89,0.1|atgeirbronze,0.21,3,0.18|atgeiriron,0.21,3,0.18|axeblackmetal,0.33,0.94,0.08|axebronze,0.25,0.89,0.04|axeflint,0.32,1,0.07|axeiron,0.25,0.89,0.04|axestone,0.32,0.92,0.04|barley,0.22,0.22,0.71|barleyflour,0.39,0.63,0.7|barleywinebase,0.47,0.47,0.2|barrell,0.84,0.85,1.1|bat,0.8,0.8,0.8|bat_melee,0.25,0.89,0.04|battleaxe,0.4,1.7,0.1|battleaxecrystal,0.76,1.77,0.14|bed,1.2,2.8,0.4|beech_log,0.88,0.88,16.96|beech_log_half,0.8,0.8,7.31|beech_sapling,0.2,0.2,1|beech_small1,0.2,0.2,2|beech_small2,0.2,0.2,2|beech_stub,1.79,1.79,1.67|beechseeds,0.3,0.27,0.18|birch_log,1.02,1.02,12.7|birch_log_half,0.93,0.93,5.74|birch_sapling,0.2,0.2,1.6|birch1,4.71,8.11,19.23|birch1_aut,4.71,8.11,19.23|birch2,6.28,9.45,18.63|birch2_aut,6.28,9.45,18.63|birchstub,1.58,1.51,1.7|blackmetal,0.3,0.62,0.12|blackmetalscrap,0.37,0.66,0.23|blacksoup,0.45,0.45,0.21|blastfurnace,3.57,3.79,5.2|blob,1.56,1.56,1.56|blobelite,2.4,2.4,2.4|blobtar,1.56,1.56,1.56|bloodbag,0.41,0.4,0.16|bloodpudding,0.53,0.41,0.18|blueberries,0.3,0.27,0.31|blueberrybush,1,1,1.09|boar,1,1,1.4|boar_piggy,0.6,0.6,1|boar_ragdoll,0.54,1.55,0.91|boarjerky,0.32,0.34,0.06|bombooze,0.28,0.28,0.28|bonefragments,0.48,0.5,0.09|bonemass_aoe,0,0,0|bonepilespawner,2,1.98,1.01|bonfire,4.31,4.37,1.03|bossstone_bonemass,6.17,1.87,7.44|bossstone_dragonqueen,9.69,1.92,5.56|bossstone_theelder,3,1.66,10.52|bossstone_yagluth,6.11,2.85,9.29|bow,0.68,1.54,0.14|bow_projectile_needle,0.1,1.82,0.08|bowdraugrfang,0.53,1.6,0.11|bowfinewood,0.28,1.26,0.08|bowhuntsman,0.38,1.51,0.15|bread,0.45,0.91,0.27|breaddough,0.39,0.52,0.19|bronze,0.3,0.62,0.12|bucket,0.72,0.72,0.67|bush01,0.4,0.4,0.91|bush01_heath,0.4,0.4,0.91|bush02_en,0.8,0.8,2.96|capedeerhide,0.61,0.63,0.16|capelinen,0.61,0.63,0.16|capelox,0.61,0.63,0.16|capeodin,0.61,0.63,0.16|capetest,0.61,0.63,0.16|capetrollhide,0.61,0.63,0.16|capewolf,0.61,0.63,0.16|cargocrate,0.76,0.76,0.76|carrot,0.36,0.11,0.11|carrotseeds,0.3,0.27,0.18|carrotsoup,0.37,0.37,0.16|cart,1.72,3.25,1.38|castlekit_braided_box01,0.76,0.76,0.76|castlekit_brazier,0.75,0.75,0.7|castlekit_groundtorch,0.23,0.24,1.48|castlekit_groundtorch_green,0.23,0.24,1.48|castlekit_groundtorch_unlit,0.12,0.13,1.41|castlekit_metal_groundtorch_unlit,0.14,0.15,1.7|cauldron_ext1_spice,2.18,0.1,1.89|cauldron_ext3_butchertable,1.2,1.2,0.95|cauldron_ext4_pots,2,0.63,1.9|caverock_ice_pillar_wall,9.25,3.24,11.4|caverock_ice_stalagmite,1.36,1.33,4.82|caverock_ice_stalagmite_broken,1.36,1.33,1.48|caverock_ice_stalagtite,0.94,0.94,3.39|caverock_ice_stalagtite_falling,0.37,0.38,1.69|chain,0.12,0.12,0.67|charcoal_kiln,4.7,5.28,3.12|chest,1.05,0.78,0.86|cloth_hanging_door,2,0.15,4|cloth_hanging_door_double,4,0.15,4|cloth_hanging_long,2,0.15,8|cloudberrybush,0.74,0.74,0.74|coal,0.31,0.53,0.33|coal_pile,1.46,1.49,1.03|coins,0.32,0.36,0.11|cookeddeermeat,0.28,0.43,0.09|cookedmeat,0.26,0.61,0.26|cookedwolfmeat,0.39,0.96,0.24|copperore,0.45,0.63,0.33|crow,1,1,1|cryptkey,0.31,0.83,0.07|crystal,0.84,0.27,0.23|crystal_wall_1x1,1,0.3,1|cultivator,0.28,1.5,0.33|darkwood_arch,2.23,0.4,2.2|darkwood_beam,2,0.4,0.4|darkwood_beam4x4,4,0.4,0.4|darkwood_decowall,1,0.1,2|darkwood_gate,2.02,0.45,4.02|darkwood_pole,0.4,0.4,2|darkwood_pole4,0.4,0.4,4|darkwood_roof_icorner,2.4,2.4,1.37|darkwood_roof_icorner_45,2.38,2.38,2.24|darkwood_roof_ocorner,2.74,2.74,1.35|darkwood_roof_ocorner_45,2.52,2.52,2.3|deathsquito,1,1,1|deer,1.6,1.6,1.78|deer_ragdoll,0.52,1.96,1.69|deerhide,0.44,2.46,0.33|deermeat,0.28,0.43,0.09|deerstew,0.5,0.5,0.15|dragon,18.03,5.38,11.35|dragonegg,1,1,1.31|dragoneggcup,1,1,0.43|dragontear,0.7,0.7,0.7|draugr,0.8,0.8,2.2|draugr_axe,0.32,1.11,0.06|draugr_bow,0.45,1.42,0.06|draugr_elite,1,1,2.5|draugr_elite_ragdoll,1.63,0.33,2.1|draugr_ragdoll,1.48,0.3,1.91|draugr_ranged,0.8,0.8,2.2|draugr_ranged_ragdoll,1.48,0.3,1.91|draugr_sword,0.32,1.11,0.06|dungeon_forestcrypt_door,2.28,0.5,3|dungeon_sunkencrypt_irongate,2,0.2,3.29|eikthyr,2.91,5.55,3.71|eikthyr_ragdoll,1.25,4.28,3.61|elderbark,0.59,1.7,0.12|entrails,0.47,0.45,0.18|evilheart_forest,0.75,0.89,1|evilheart_swamp,0.75,0.89,1|eyescream,0.3,0.3,0.3|feathers,0.65,0.56,0.19|fenring,1.61,1.61,3.23|fenring_attack_flames_aoe,1.63,5.45,4.06|fenring_cultist,1,1,2.5|fenring_cultist_ragdoll,1.55,1.41,3.19|fenring_ragdoll,1.55,1.41,3.19|fermenter,1.6,1.55,2.2|finewood,0.4,2.07,0.33|fircone,0.34,0.34,0.86|fire_pit,1.4,1.4,0.2|firtree,0.8,0.8,10.89|firtree_log,0.62,0.62,7.75|firtree_log_half,0.57,0.57,3.45|firtree_oldlog,1.26,7.05,1.57|firtree_sapling,0.2,0.2,1|firtree_small,0.4,0.4,5.45|firtree_small_dead,0.8,0.8,10.89|firtree_stub,1.53,1.36,0.49|fish1,0.14,1.03,0.46|fish2,0.19,1.71,0.61|fish3,0.33,1.74,0.72|fish4_cave,0.17,0.87,0.36|fishingrod,0.28,3.69,0.12|fishingrodfloat,0.3,0.3,0.3|fishwraps,0.38,0.57,0.21|fistfenrirclaw,0.45,0.57,0.2|flametalore,0.68,0.74,0.55|forge,1.8,1,0.91|forge_ext1,1.76,1.59,1.91|forge_ext2,0.84,0.9,0.84|forge_ext3,0.75,1.05,1.41|forge_ext4,0.9,0.9,1.4|forge_ext5,0.95,0.94,1|forge_ext6,2.5,0.2,0.3|freezegland,0.41,0.4,0.16|ghost,1,1,2|glowingmushroom,1.49,1.37,1.53|goblin,0.8,0.8,1.45|goblin_bed,1.2,2.9,0.7|goblin_dragdoll,1.99,0.3,1.39|goblin_stairs,2,2.03,1.12|goblin_stepladder,1,2.16,2.16|goblinarcher,0.8,0.8,1.45|goblinbrute,1.6,1.6,3.5|goblinbrute_attack,0.2,0.96,0.03|goblinbrute_ragdoll,1.99,2.09,3.45|goblinbrute_rageattack,0.2,0.96,0.03|goblinbrute_taunt,0.2,0.96,0.03|goblinclub,0.15,1.2,0.15|goblinking,15.7,4.35,8.9|goblinking_ragdoll,9.81,9.89,6.14|goblinshaman,0.8,0.8,1.45|goblinshaman_headdress_antlers,1,1,1|goblinshaman_headdress_feathers,1,1,1|goblinshaman_protect_aoe,0,0,0|goblinshaman_ragdoll,1.96,0.4,1.42|goblinshaman_staff_bones,1,1,1|goblinshaman_staff_feathers,1,1,1|goblinspear,0.16,2.36,0.16|goblinsword,0.2,0.96,0.03|goblintorch,0.15,0.88,0.15|goblintotem,0.39,0.3,0.74|greydwarf_elite,1.28,1.28,2.62|greydwarf_elite_ragdoll,1.63,1.4,2.87|greydwarf_ragdoll,1.08,0.93,1.9|greydwarf_root,1.77,1.3,1.59|greydwarf_shaman_ragdoll,1.72,1.36,2.07|greydwarfeye,0.3,0.35,0.21|greyling_ragdoll,0.94,0.8,1.65|guard_stone,1.03,0.78,1.61|guard_stone_test,1.55,1.3,2|gucksack,2.41,2.79,3.92|gucksack_small,1.21,1.4,1.96|haldor,1,1,1.63|hammer,0.26,0.44,0.1|hanging_hairstrands,0.2,0.2,2.2|hatchling,4,2.02,2.3|hatchling_ragdoll,4.7,2.88,2.88|healthupgrade_bonemass,0.75,0.89,1|healthupgrade_gdking,0.75,0.89,1|hearth,4,3,0.6|heathrockpillar,9.86,12.54,44.38|heathrockpillar_frac,9.86,12.54,44.38|helmetbronze,0.21,0.28,0.19|helmetdrake,0.23,0.49,0.28|helmetdverger,0.2,0.27,0.17|helmetfenring,0.61,0.63,0.16|helmetiron,0.21,0.28,0.26|helmetleather,0.21,0.28,0.27|helmetmidsummercrown,0.39,0.44,0.32|helmetodin,0.46,0.46,0.2|helmetpadded,0.39,0.4,0.46|helmetroot,0.21,0.28,0.19|helmettrollleather,0.46,0.46,0.2|highstone,4.32,2.24,6.65|highstone_frac,4.32,2.24,6.65|hoe,0.31,1.26,0.25|honey,0.45,0.59,0.14|hugeroot1,29.27,11.8,10.07|ice_floor,4.7,4.66,2.1|ice_floor_fractured,4.7,4.66,2.1|ice_rock1,24,15.55,29.14|ice_rock1_frac,24,15.55,29.14|ice1,8.35,8,0.69|iceblocker,2,2,3|incinerator,2.36,1.27,10.19|iron_grate,2.01,0.2,2.99|iron_wall_1x1,1,0.1,1|itemstand,0.5,0.09,0.55|itemstandh,0.2,0.2,0.04|jute_carpet,3.57,2.56,0.05|karve,7.68,10.12,8.82|knifeblackmetal,0.13,0.54,0.03|knifebutcher,0.07,0.61,0.02|knifechitin,0.17,0.5,0.04|knifecopper,0.14,0.38,0.03|knifeflint,0.16,0.38,0.03|knifesilver,0.14,0.54,0.03|leatherscraps,0.64,0.55,0.15|leech,1.6,1.63,1.6|leech_cave,1.6,1.63,1.6|leviathan,69.97,60.41,45.02|loot_chest_stone,2.02,0.86,0.54|loot_chest_wood,1.64,0.75,0.74|lox,3,6.56,3.5|lox_calf,2,2,2|lox_ragdoll,3.8,6.82,3.04|loxcalf_ragdoll,1.52,2.73,1.22|loxpelt,0.85,4.1,0.62|loxpie,0.57,0.57,0.25|loxpieuncooked,0.5,0.5,0.18|macebronze,0.15,0.84,0.18|maceiron,0.15,1.05,0.18|maceneedle,0.54,1.25,0.45|macesilver,0.44,0.95,0.15|marker01,0.93,0.96,1.65|marker02,0.93,0.96,1.65|meadbasefrostresist,0.47,0.47,0.2|meadbasehealthmedium,0.47,0.47,0.2|meadbasehealthminor,0.47,0.47,0.2|meadbasepoisonresist,0.47,0.47,0.2|meadbasestaminamedium,0.47,0.47,0.2|meadbasestaminaminor,0.47,0.47,0.2|meadbasetasty,0.47,0.47,0.2|meadfrostresist,0.15,0.37,0.42|meadhealthmedium,0.27,0.25,0.44|meadhealthminor,0.17,0.16,0.28|meadpoisonresist,0.14,0.25,0.44|meadstaminamedium,0.14,0.25,0.44|meadstaminaminor,0.12,0.21,0.32|meadtasty,0.14,0.34,0.37|mincemeatsauce,0.55,0.55,0.1|minerock_copper,3.27,3.48,2.88|minerock_iron,3.27,3.48,2.88|minerock_meteorite,2.17,2.35,2.03|minerock_obsidian,1.99,2.16,1.85|minerock_stone,3.27,3.48,2.88|minerock_tin,2.29,1.8,1.38|mountainkit_brazier,0.75,0.75,0.7|mountainkit_chair,0.55,0.57,1.21|mountainkit_table,2.53,2.46,0.78|mountainkit_wood_gate,3,0.5,4.52|mudpile,2.9,3.18,1.46|mudpile_beacon,2.9,3.18,1.46|mudpile_frac,2.9,3.18,1.39|mudpile_old,2.9,3.18,1.39|mudpile2,5.29,5.31,5.01|mudpile2_frac,5.29,5.31,5.01|mushroom,0.36,0.33,0.37|mushroomblue,0.3,0.27,0.31|mushroomyellow,0.3,0.27,0.31|necktail,0.22,0.65,0.27|necktailgrilled,0.22,0.65,0.27|needle,0.13,1.05,0.16|oak_log,2.64,2.64,13.78|oak_log_half,2.4,2.4,6.29|oak_sapling,0.18,0.18,0.9|oakstub,4.2,3.97,2.9|obsidian,0.53,0.49,0.15|odin,1.12,1,2.02|old_wood_roof_icorner,2.35,2.35,1.19" +
      "|old_wood_roof_ocorner,2.47,2.47,1.27|old_wood_roof_top,2,2.21,0.91|old_wood_wall_roof,2.27,0.4,1.31|onion,0.26,0.25,0.28|onionsoup,0.5,0.5,0.2|ooze,0.45,0.41,0.33|pickable_barley,0.8,0.8,1.45|pickable_barley_wild,0.8,0.8,1.45|pickable_bogironore,0.45,0.63,0.33|pickable_branch,1.12,3.29,1.12|pickable_carrot,0.28,0.28,0.89|pickable_dandelion,0.48,0.48,0.48|pickable_fishingrod,0.28,3.69,0.12|pickable_flax,0.6,0.6,1.45|pickable_flax_wild,0.8,0.8,1.45|pickable_flint,1,1,1|pickable_forestcryptremains01,1.03,0.86,0.44|pickable_forestcryptremains02,1.03,0.86,0.44|pickable_forestcryptremains03,1.03,0.86,0.44|pickable_forestcryptremains04,0.81,1.88,0.5|pickable_hairstrands01,0.3,0.3,2.25|pickable_hairstrands02,0.45,0.45,2.1|pickable_meatpile,0.8,0.8,0.35|pickable_meteorite,0.84,0.84,0.84|pickable_mountaincavecrystal,0.46,0.43,0.78|pickable_mountaincaveobsidian,0.5,0.5,0.5|pickable_mushroom,0.96,0.96,0.96|pickable_mushroom_blue,0.6,0.6,0.6|pickable_mushroom_yellow,0.6,0.6,0.6|pickable_obsidian,0.94,0.94,0.94|pickable_onion,0.28,0.28,1|pickable_seedcarrot,0.48,0.48,1.08|pickable_seedonion,0.6,0.6,2.15|pickable_seedturnip,0.4,0.4,1.04|pickable_stone,1,1,1|pickable_surtlingcorestand,0.4,0.46,1.4|pickable_tar,0.74,0.81,0.54|pickable_tarbig,1.9,2.1,1.53|pickable_thistle,0.4,0.38,1.52|pickable_tin,0.55,0.38,0.35|pickable_turnip,0.28,0.28,1|pickaxeantler,0.36,0.86,0.09|pickaxebronze,0.57,0.87,0.07|pickaxeiron,0.86,0.83,0.04|pickaxestone,0.36,0.86,0.09|piece_banner01,0.3,1.41,0.3|piece_banner02,0.3,1.41,0.3|piece_banner03,0.3,1.41,0.3|piece_banner04,0.3,1.41,0.3|piece_banner05,0.3,1.41,0.3|piece_banner06,0.3,1.41,0.3|piece_banner07,0.3,1.41,0.3|piece_banner08,0.3,1.41,0.3|piece_banner09,0.3,1.41,0.3|piece_banner10,0.3,1.41,0.3|piece_banner11,0.3,1.41,0.3|piece_bathtub,3.17,3.81,2.73|piece_bed02,2.3,3.44,1.5|piece_beehive,1.34,1.3,1.58|piece_bench01,2.41,0.65,0.55|piece_brazierceiling01,0.97,0.91,1.96|piece_brazierfloor01,0.75,0.75,0.9|piece_cartographytable,3.93,1.95,1.39|piece_cauldron,0.94,0.94,1.4|piece_chair,0.54,0.5,0.53|piece_chair02,0.55,0.94,1.2|piece_chair03,0.55,0.57,1.21|piece_chest,1.84,0.95,1.07|piece_chest_blackmetal,2.06,1.35,0.92|piece_chest_treasure,1.16,0.76,0.52|piece_cloth_hanging_door,0.2,1.13,0.2|piece_cookingstation,1.73,0.78,1.28|piece_cookingstation_iron,3.6,1,2.08|piece_gift1,0.59,0.31,0.27|piece_gift2,0.71,0.37,0.32|piece_gift3,0.89,0.47,0.4|piece_groundtorch,0.23,0.24,1.48|piece_groundtorch_blue,0.23,0.24,1.48|piece_groundtorch_green,0.23,0.24,1.48|piece_groundtorch_wood,0.12,0.13,1.41|piece_jackoturnip,0.61,0.61,0.61|piece_logbench01,2.39,0.58,0.57|piece_maypole,1.22,4.13,5.1|piece_oven,3,2,2.37|piece_sharpstakes,1.8,1.51,0.84|piece_spinningwheel,2.04,2.67,2.27|piece_stonecutter,3.23,1.5,1.81|piece_table,2.45,1.22,0.79|piece_table_oak,6.45,1.85,0.79|piece_table_round,2.53,2.46,0.8|piece_throne01,1.4,0.84,2.49|piece_throne02,1.43,0.88,2.95|piece_walltorch,0.45,0.31,0.95|piece_workbench,3.01,1.1,1.03|piece_workbench_ext1,0.81,0.72,1.01|piece_workbench_ext2,2.01,0.6,1.75|piece_workbench_ext3,2.11,1.09,1.16|piece_workbench_ext4,1.5,0.3,1.3|piece_xmastree,0.2,0.2,2.72|pinecone,0.4,0.4,0.64|pinetree,9.44,8.96,21.3|pinetree_01,0.62,0.62,24.43|pinetree_01_stub,1.2,1.07,0.46|pinetree_log_half,0.57,0.57,3.45|pinetree_log_halfold,0.8,0.8,6.36|pinetree_logold,0.88,0.88,14.65|pinetree_sapling,0.2,0.2,1|player_ragdoll,1.39,0.36,1.87|player_ragdoll_old,0.92,0.36,1.89|player_tombstone,0.5,0.39,1.16|playerunarmed,0.15,1.2,0.15|portal,4.64,4.46,4.09|portal_wood,4.23,1.18,3.29|pukeberries,0.3,0.31,0.18|queenbee,0.23,0.75,0.24|queensjam,0.27,0.26,0.31|raft,6.09,6.91,6.56|raspberry,0.2,0.22,0.27|raspberrybush,1,1,1.09|rawmeat,0.26,0.61,0.26|rock_3,2.1,1.89,2.08|rock_3_frac,2.1,1.89,2.08|rock_4,2.29,1.8,1.38|rock_4_plains,2.29,1.8,1.38|rock_destructible_test,3.27,3.48,2.88|rock1_mountain,24,15.55,29.14|rock1_mountain_frac,24,15.55,29.14|rock2_heath,14.58,17.57,14.48|rock2_heath_frac,14.58,17.57,14.48|rock2_mountain,19.44,19.31,23.42|rock2_mountain_frac,19.44,19.31,23.42|rock3_mountain,19.2,12.68,24.87|rock3_mountain_frac,19.2,12.68,24.87|rock3_silver,19.2,12.68,24.87|rock3_silver_frac,19.2,12.68,24.87|rock4_coast,27.75,24.16,16.84|rock4_coast_frac,27.75,24.16,16.84|rock4_copper,27.75,24.16,16.84|rock4_copper_frac,27.75,24.16,16.84|rock4_forest,27.75,24.16,16.84|rock4_forest_frac,27.75,24.16,16.84|rock4_heath,27.75,24.16,16.84|rock4_heath_frac,27.75,24.16,16.84|rockfinger,12.39,33.06,39.33|rockfinger_frac,12.39,33.06,39.33|rockfingerbroken_frac,12.39,14.45,32.73|rockformation1,31.55,36.81,57.95|rockthumb,15.99,23.3,37.88|rockthumb_frac,15.99,23.3,37.88|root,1.05,0.9,0.38|root07,1.22,0.44,1.61|rottenmeat,0.84,0.44,0.43|roundlog,2.22,0.55,0.52|ruby,0.23,0.36,0.1|rug_deer,2,3,0.05|rug_fur,2.56,3.57,0.05|rug_wolf,2,3,0.05|saddlelox,1.02,1.65,1.13|sapling_barley,0.8,0.8,1|sapling_carrot,0.36,0.36,1|sapling_flax,0.6,0.6,1|sapling_onion,0.36,0.36,1|sapling_seedcarrot,0.36,0.36,1|sapling_seedonion,0.36,0.36,1|sapling_seedturnip,0.36,0.36,1|sapling_turnip,0.36,0.36,1|sausages,0.61,0.78,0.13|seagal,1,1,1|serpent,2,32.26,2.24|serpentmeat,1.25,1.34,0.75|serpentmeatcooked,1.25,1.34,0.75|serpentscale,0.52,0.75,0.1|serpentstew,0.55,0.55,0.2|shaman_attack_aoe,1.99,3.67,1|shaman_heal_aoe,0,0,0|sharpeningstone,0.11,0.71,0.71|shieldbanded,0.99,0.99,0.16|shieldblackmetal,1.04,1.02,0.2|shieldblackmetaltower,0.8,1.46,0.21|shieldbonetower,0.87,1.44,0.32|shieldbronzebuckler,0.71,0.71,0.11|shieldironbuckler,0.56,0.56,0.08|shieldironsquare,0.68,0.16,0.98|shieldirontower,0.71,1.37,0.19|shieldknight,0.87,0.87,0.09|shieldserpentscale,0.57,1.44,0.27|shieldsilver,0.71,0.77,0.23|shieldwood,0.83,0.83,0.11|shieldwoodtower,0.75,1.5,0.15|ship_construction,11.76,26.91,14.64|shocklatesmoothie,0.18,0.18,0.32|shrub_2,1.14,1.14,1.14|shrub_2_heath,1.14,1.14,1.14|silver,0.3,0.62,0.12|silvernecklace,0.37,0.69,0.06|silverore,0.45,0.63,0.33|silvervein,17.17,11.13,3.23|silvervein_frac,17.17,11.13,3.23|skeleton,0.8,0.8,1.85|skeleton_bow,0.45,1.42,0.06|skeleton_mace,0.34,1.38,0.34|skeleton_noarcher,0.8,0.8,1.85|skeleton_poison,0.8,0.8,2.2|skeleton_sword,0.32,1.11,0.06|skull1,0.31,0.31,0.31|skull2,4.64,4.64,4.64|sledgecheat,0.55,1.36,0.19|sledgeiron,0.55,1.36,0.19|sledgestagbreaker,0.54,1.36,0.33|smelter,3.03,2.99,4.24|spawner_draugrpile,2.66,2.98,2.36|spawner_greydwarfnest,2,2,4.6|spearbronze,0.16,2.43,0.05|spearchitin,0.16,2.36,0.16|spearelderbark,0.13,2.31,0.1|spearflint,0.16,2.36,0.16|spearwolffang,0.16,2.36,0.16|stake_wall,2,0.57,3.47|staminaupgrade_greydwarf,0.7,0.7,0.7|staminaupgrade_troll,0.7,0.7,0.7|staminaupgrade_wraith,0.7,0.7,0.7|statueevil,1.9,2.26,5.19|stone,0.53,0.5,0.32|stone_arch,2,1,1|stone_floor,4,4,1|stone_floor_2x2,2,2,1|stone_pile,1.67,1.54,0.92|stone_pillar,1,1,2|stone_stair,1.97,1.98,1.06|stone_wall_1x1,1,1,1|stone_wall_2x1,2,1,1|stone_wall_4x2,4,1,2|stoneblock_fracture,4,4,1|stonechest,2.02,0.86,0.54|stonegolem,2,2,4.5|stonegolem_ragdoll,6.65,1.06,4.06|stubbe,7.21,4.94,3.73|stubbe_spawner,7.34,4.94,4.09|sunken_crypt_gate,2,0.2,3.02|surtling,0.6,0.6,1.28|surtlingcore,0.22,0.22,0.22|swamptree1,4.54,5.26,16.42|swamptree1_stub,4.45,5.26,3.06|swamptree2,14.07,16.5,36.22|swamptree2_darkland,14.07,16.5,36.22|swamptree2_log,18.76,48.29,22|swordblackmetal,0.19,1.29,0.06|swordbronze,0.14,0.91,0.05|swordcheat,0.2,1.21,0.03|swordiron,0.2,1.21,0.03|swordironfire,0.2,1.21,0.03|swordsilver,0.2,1.21,0.03|tankard,0.18,0.18,0.25|tankardanniversary,0.36,0.53,0.46|tankardodin,0.26,0.34,0.51|tar,0.47,0.52,0.34|tarliquid,64,64,40|tarlump1,27.75,24.16,8.42|tarlump1_frac,27.75,24.16,8.42|tentaroot,0.8,0.8,3|tentaroot_attack,0.32,1.11,0.06|thistle,0.32,0.28,0.91|thunderstone,0.68,0.23,0.23|tinore,0.3,0.35,0.28|tolroko_flyer,2.49,3.99,1.35|torch,0.15,0.88,0.15|trailership,9.16,21.57,11.65|trainingdummy,0.8,0.8,1.85|treasure_pile,1.5,1.51,0.27|treasure_stack,0.25,0.27,0.54|treasurechest_blackforest,1.64,0.75,0.74|treasurechest_fcrypt,2.02,0.86,0.54|treasurechest_forestcrypt,2.02,0.86,0.54|treasurechest_heath,1.64,0.75,0.74|treasurechest_meadows,1.64,0.75,0.74|treasurechest_mountaincave,2.42,1.04,0.65|treasurechest_plains_stone,2.02,0.86,0.54|treasurechest_sunkencrypt,2.02,0.86,0.54|treasurechest_swamp,1.64,0.75,0.74|troll,3.8,3.8,7.58|troll_ragdoll,6.04,2.62,7.2|trophyabomination,2.82,2.76,0.9|trophyblob,0.65,0.46,0.83|trophyboar,0.25,0.49,0.54|trophybonemass,3.08,3.01,1.33|trophycultist,0.6,0.6,0.9|trophydeathsquito,0.48,0.19,0.74|trophydeer,0.58,0.8,0.54|trophydragonqueen,0.93,1.59,1.29|trophydraugr,0.35,0.49,0.48|trophydraugrelite,0.35,0.49,0.48|trophydraugrfem,0.35,0.49,0.48|trophyeikthyr,2.3,2.92,1.57|trophyfenring,0.56,1.78,0.61|trophyfrosttroll,1.24,1.69,1.69|trophygoblin,0.23,0.32,0.24|trophygoblinbrute,1.95,1.49,0.89|trophygoblinking,4.61,3.15,2.88|trophygoblinshaman,0.62,0.45,0.21|trophygreydwarf,0.32,0.62,0.42|trophygreydwarfbrute,0.36,0.85,0.41|trophygreydwarfshaman,1.64,1.49,0.34|trophygrowth,0.46,0.51,0.25|trophyhatchling,0.3,0.27,0.5|trophyneck,0.39,0.66,0.65|trophyserpent,4.58,3.85,3.13|trophyskeleton,0.19,0.34,0.24|trophyskeletonpoison,0.26,0.4,0.36|trophysurtling,0.23,0.32,0.24|trophytheelder,0.66,1.33,0.89|trophyulv,1.34,1.21,0.47|trophywolf,0.28,0.31,0.42|trophywraith,0.99,1.72,0.2|turnip,0.28,0.28,0.58|turnipstew,0.47,0.47,0.2|ulv,1,1.67,1.5|ulv_ragdoll,1.54,2.48,1.35|vegvisirshard_bonemass,0.2,0.1,0.2|vfx_barrle_destroyed,0.81,0.82,1.14|vfx_destroyed_karve,7.81,10.1,8.82|vfx_destroyed_raft,6.31,6.42,7.29|vfx_destroyed_vikingship,8.49,22.68,11.7|vfx_greydwarfnest_destroyed,9.44,8.32,5.73|vfx_stonegolem_death,4.15,1.46,4.78|vikingship,9.16,21.57,11.65|vines,1,0.3,1|widestone,6.59,4.08,2.2|widestone_frac,6.59,4.08,2.2|windmill,5.66,5.6,8.64|wishbone,0.72,0.99,0.26|witheredbone,0.41,1.98,0.37|wolf,1.3,1.3,1.5|wolf_cub,0.8,0.8,1|wolf_ragdoll,0.7,1.84,1.29|wolfclaw,0.83,0.28,0.12|wolffang,0.11,0.17,0.44|wolfhairbundle,0.68,0.42,0.27|wolfjerky,0.34,0.51,0.08|wolfmeat,0.39,0.96,0.24|wolfmeatskewer,0.15,0.77,0.17|wolfpelt,0.37,2.05,0.27" +
      "|wood_beam,2,0.4,0.4|wood_beam_1,1,0.4,0.4|wood_beam_26,2.27,0.4,1.54|wood_beam_45,2.43,0.4,2.43|wood_core_stack,1.99,2.22,1.32|wood_door,2,0.5,2.02|wood_fence,2.69,0.29,2.08|wood_fine_stack,1.9,1.9,0.9|wood_floor,2,2,0.13|wood_floor_1x1,1,1,0.13|wood_gate,1.98,0.5,3|wood_ledge,1,0.6,0.05|wood_log_26,2.23,0.5,1.45|wood_log_45,2.35,0.5,2.35|wood_pole,0.4,0.4,1|wood_pole_log,0.4,0.4,2|wood_pole_log_4,0.4,0.4,4|wood_pole2,0.4,0.4,2|wood_roof_icorner,2.35,2.35,1.19|wood_roof_icorner_45,2.19,2.19,2.08|wood_roof_ocorner,2.47,2.47,1.27|wood_roof_ocorner_45,2.43,2.43,2.13|wood_stack,2.46,2.31,1.3|wood_stair,2,2.03,1.12|wood_stepladder,1,2.16,2.16|wood_wall_log,2,0.5,0.5|wood_wall_log_4x0.5,4,0.5,0.5|wood_wall_roof,2.27,0.4,1.54|wood_wall_roof_45,2.43,0.4,2.43|wood_wall_roof_45_upsidedown,2.43,0.42,2.43|wood_wall_roof_top_45,2.43,0.41,2.43|wood_wall_roof_upsidedown,2.27,0.42,1.54|wood_window,1.15,0.36,1.05|woodiron_beam,2,0.3,0.3|woodiron_pole,0.3,0.3,2|wraith,1,1,2.53|wraith_melee,0.25,0.89,0.04|yagluthdrop,0.5,0.5,0.5|ymirremains,0.48,0.53,0.23";
    Wrapper = wrapper;
    var section = "1. General";
    configEnabled = wrapper.Bind(section, "Enabled", true, "Whether this mod is enabled at all.");
    configHammerTools = wrapper.BindList(section, "Hammer tools", "hammer", "List of hammers.");
    configHammerTools.SettingChanged += (s, e) => UpdateTools();
    configHoeTools = wrapper.Bind(section, "Hoe tools", "hoe", "List of hoes.");
    configHoeTools.SettingChanged += (s, e) => UpdateTools();
    UpdateTools();
    configMirrorFlip = wrapper.Bind(section, "Mirror flip", "woodwall", "Object ids that get flipped instead of rotated when mirrored.");
    configMirrorFlip.SettingChanged += (s, e) => UpdateMirrorFlip();
    UpdateMirrorFlip();
    configDimensions = wrapper.Bind(section, "Dimensions", "", "Object dimensions.");
    configDimensions.SettingChanged += (s, e) => UpdateDimensions();
    UpdateDimensions();
    if (configDimensions.Value == "") configDimensions.Value = defaultDimensions;
    if (CommandWrapper.ServerDevcommands != null)
      configServerDevcommandsUndo = wrapper.Bind(section, "Server Devcommands undo", true, "If disabled, uses Infinity Hammer's own undo system even if Server Devcommands is installed.");
    configPlanBuildFolder = wrapper.Bind(section, "Plan Build folder", "BepInEx/config/PlanBuild", "Folder relative to the Valheim.exe.");
    configBuildShareFolder = wrapper.Bind(section, "Build Share folder", "BuildShare/Builds", "Folder relative to the Valheim.exe.");
    if (CommandWrapper.ServerDevcommands != null)
      InitBinds(wrapper);
    section = "3. Powers";
    configRemoveArea = wrapper.Bind(section, "Remove area", "0", "Removes same objects within the radius.");
    configSelectRange = wrapper.Bind(section, "Select range", "50", "Range for selecting objects.");
    configRemoveRange = wrapper.Bind(section, "Remove range", "0", "Range for removing objects (0 = default).");
    configRepairRange = wrapper.Bind(section, "Repair range", "0", "Range for repairing objects (0 = default).");
    configBuildRange = wrapper.Bind(section, "Build range", "0", "Range for placing objects (0 = default)");
    configRepairTaming = wrapper.Bind(section, "Repair taming", false, "Repairing full health creatures tames/untames them.");
    configShowCommandValues = wrapper.Bind(section, "Show command values", false, "Always shows the command in the tool descriptions.");
    configRemoveEffects = wrapper.Bind(section, "Remove effects", false, "Removes visual effects of building, etc.");
    configEnableUndo = wrapper.Bind(section, "Enable undo", true, "Enabled undo and redo for placing/removing.");
    configCopyRotation = wrapper.Bind(section, "Copy rotation", true, "Copies rotation of the selected object.");
    configNoBuildCost = wrapper.Bind(section, "No build cost", true, "Removes build costs and requirements.");
    configIgnoreWards = wrapper.Bind(section, "Ignore wards", true, "Ignores ward restrictions.");
    configIgnoreNoBuild = wrapper.Bind(section, "Ignore no build", true, "Ignores no build areas.");
    configNoStaminaCost = wrapper.Bind(section, "No stamina cost", true, "Removes hammer stamina usage.");
    configNoDurabilityLoss = wrapper.Bind(section, "No durability loss", true, "Removes hammer durability usage.");
    configAllObjects = wrapper.Bind(section, "All objects", true, "Allows placement of non-default objects.");
    configCopyState = wrapper.Bind(section, "Copy state", true, "Copies object's internal state.");
    configAllowInDungeons = wrapper.Bind(section, "Allow in dungeons", true, "Allows building in dungeons.");
    configRemoveAnything = wrapper.Bind(section, "Remove anything", false, "Allows removing anything.");
    configDisableLoot = wrapper.Bind(section, "Disable loot", false, "Prevents creatures and structures dropping loot when removed with the hammer.");
    configRepairAnything = wrapper.Bind(section, "Repair anything", false, "Allows reparing anything.");
    configOverwriteHealth = wrapper.Bind(section, "Overwrite health", "0", "Overwrites the health of built or repaired objects.");
    configInfiniteHealth = wrapper.Bind(section, "Infinite health", false, "Sets the Overwrite health to 1E30.");
    configNoCreator = wrapper.Bind(section, "No creator", false, "Build without setting the creator.");
    configUnfreezeOnSelect = wrapper.Bind(section, "Unfreeze on select", false, "Removes the placement freeze when selecting a new object.");
    configResetOffsetOnUnfreeze = wrapper.Bind(section, "Reset offset on unfreeze", true, "Removes the placement offset when unfreezing the placement.");
    configUnfreezeOnUnequip = wrapper.Bind(section, "Unfreeze on unequip", true, "Removes the placement freeze when unequipping the hammer.");
    configHidePlacementMarker = wrapper.Bind(section, "No placement marker", false, "Hides the yellow placement marker (also affects Gizmo mod).");
    configIgnoreOtherRestrictions = wrapper.Bind(section, "Ignore other restrictions", true, "Ignores any other restrictions (material, biome, etc.)");
    section = "4. Items";
    configRemoveBlacklist = wrapper.BindList(section, "Remove blacklist", "", "Object ids separated by , that can't be removed.");
    configRemoveBlacklist.SettingChanged += (s, e) => RemoveBlacklist = ParseList(configRemoveBlacklist.Value);
    RemoveBlacklist = ParseList(configRemoveBlacklist.Value);
    configSelectBlacklist = wrapper.BindList(section, "Select blacklist", "", "Object ids separated by , that can't be selected.");
    configSelectBlacklist.SettingChanged += (s, e) => SelectBlacklist = ParseList(configSelectBlacklist.Value);
    SelectBlacklist = ParseList(configSelectBlacklist.Value);
    section = "5. Messages";
    configDisableMessages = wrapper.Bind(section, "Disable messages", false, "Disables all messages from this mod.");
    configDisableOffsetMessages = wrapper.Bind(section, "Disable offset messages", false, "Disables messages from changing placement offset.");
    configDisableScaleMessages = wrapper.Bind(section, "Disable scale messages", false, "Disables messages from changing the scale.");
    configDisableSelectMessages = wrapper.Bind(section, "Disable select messages", false, "Disables messages from selecting objects.");
    configChatOutput = wrapper.Bind(section, "Chat output", false, "Sends messages to the chat window from bound keys.");
    InitCommands(wrapper);
  }
}
