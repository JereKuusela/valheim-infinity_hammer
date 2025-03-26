using System.Collections.Generic;
using BepInEx.Configuration;
using ServerDevcommands;
using Service;

namespace InfinityHammer;
public partial class Configuration
{
#nullable disable
  public static bool IsCheats => Enabled && ((ZNet.instance && ZNet.instance.IsServer()) || Console.instance.IsCheatsEnabled());
  public static ConfigEntry<bool> configNoCost;
  public static bool NoCost => configNoCost.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreWards;
  public static bool IgnoreWards => configIgnoreWards.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreNoBuild;
  public static bool IgnoreNoBuild => configIgnoreNoBuild.Value && IsCheats;
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
  public static ConfigEntry<bool> configDisableLoot;
  public static bool DisableLoot => configDisableLoot.Value && IsCheats;
  public static ConfigEntry<bool> configRepairAnything;
  public static bool RepairAnything => configRepairAnything.Value && IsCheats;
  public static ConfigEntry<bool> configNoCreator;
  public static bool NoCreator => configNoCreator.Value && IsCheats;
  public static ConfigEntry<bool> configNoPrimaryTarget;
  public static bool NoPrimaryTarget => configNoPrimaryTarget.Value && IsCheats;
  public static ConfigEntry<bool> configNoSecondaryTarget;
  public static bool NoSecondaryTarget => configNoSecondaryTarget.Value && IsCheats;
  public static ConfigEntry<string> configZoopMagic;
  public static string ZoopMagic => configZoopMagic.Value;

  public static ConfigEntry<bool> configResetOffsetOnUnfreeze;
  public static bool ResetOffsetOnUnfreeze => configResetOffsetOnUnfreeze.Value;
  public static ConfigEntry<bool> configUnfreezeOnUnequip;
  public static bool UnfreezeOnUnequip => configUnfreezeOnUnequip.Value;
  public static ConfigEntry<bool> configUnfreezeOnSelect;
  public static bool UnfreezeOnSelect => configUnfreezeOnSelect.Value;
  public static ConfigEntry<string> configOverwriteHealth;
  public static float OverwriteHealth => IsCheats ? Parse.Float(configOverwriteHealth.Value) : 0f;
  public static ConfigEntry<string> configInvulnerability;
  public static string Invulnerability => IsCheats ? configInvulnerability.Value : InvulnerabilityMode.Off;
  public static ConfigEntry<string> configSnapping;
  public static string Snapping => configSnapping.Value;
  public static ConfigEntry<string> configRemoveArea;
  public static float RemoveArea => Enabled ? Parse.Float(configRemoveArea.Value) : 0f;
  public static ConfigEntry<string> configRange;
  public static float Range => IsCheats ? Parse.Float(configRange.Value) : 0f;
  public static ConfigEntry<bool> configPlaceEmptyRooms;
  public static bool PlaceEmptyRooms => configPlaceEmptyRooms.Value;
  
  public static ConfigEntry<bool> configBlueprintNoChestContent;
  public static bool BlueprintNoChestContent => configBlueprintNoChestContent.Value;

  public static ConfigEntry<bool> configBlueprintNoFuel;
  public static bool BlueprintNoFuel => configBlueprintNoFuel.Value;

  public static ConfigEntry<bool> configBlueprintItemHolderReduceData;
  public static bool BlueprintItemHolderReduceData => configBlueprintItemHolderReduceData.Value;
  
  
 
 /*
 public static ConfigEntry<string> configEnableBuildPlayer;
 public static ConfigEntry<string> configEnableBuildPiece;
 public static ConfigEntry<string> configEnablePlaceable;
 public static ConfigEntry<string> configEnableCompfort;
 public static ConfigEntry<string> configEnableLightSource;
 public static ConfigEntry<string> configEnableHoverable;
 public static ConfigEntry<string> configEnableTextReceiver;
 public static ConfigEntry<string> configEnableInteractable;
 public static ConfigEntry<string> configEnableObjectHolder;
 public static ConfigEntry<string> configEnableContainerPiece;
 public static ConfigEntry<string> configEnableCraftingStation;
 public static ConfigEntry<string> configEnableFuel;
 public static ConfigEntry<string> configEnablePickable;
 public static ConfigEntry<string> configEnableCultivated;
 public static ConfigEntry<string> configEnableDestroyableTerrain;
 public static ConfigEntry<string> configEnableFractured;
 public static ConfigEntry<string> configEnableHoverableResourceNode;
 public static ConfigEntry<string> configEnableNonStatic;
 public static ConfigEntry<string> configEnableCreature;
 public static ConfigEntry<string> configEnableTameable;
 public static ConfigEntry<string> configEnableVehicle;
 public static ConfigEntry<string> configEnableAnimated;
 public static ConfigEntry<string> configEnableSpecialInterface;
 public static ConfigEntry<string> configEnableIndestructible;
 public static ConfigEntry<string> configEnableCustomNotVanilla;
 public static string  EnableBuildPlayer => configEnableBuildPlayer.Value;
 public static string    EnableBuildPiece => configEnableBuildPiece.Value;
 public static string  EnablePlaceable => configEnablePlaceable.Value;
 public static string    EnableCompfort => configEnableCompfort.Value;
 public static string  EnableLightSource => configEnableLightSource.Value;
 public static string    EnableHoverable => configEnableHoverable.Value;
 public static string  EnableTextReceiver => configEnableTextReceiver.Value;
 public static string    EnableInteractable => configEnableInteractable.Value;
 public static string  EnableObjectHolder => configEnableObjectHolder.Value;
 public static string    EnableContainerPiece => configEnableContainerPiece.Value;
 public static string  EnableCraftingStation => configEnableCraftingStation.Value;
 public static string    EnableFuel => configEnableFuel.Value;
 public static string  EnablePickable => configEnablePickable.Value;
 public static string    EnableCultivated => configEnableCultivated.Value;
 public static string  EnableDestroyableTerrain => configEnableDestroyableTerrain.Value;
 public static string    EnableFractured => configEnableFractured.Value;
 public static string  EnableHoverableResourceNode => configEnableHoverableResourceNode.Value;
 public static string    EnableNonStatic => configEnableNonStatic.Value;
 public static string  EnableCreature => configEnableCreature.Value;
 public static string    EnableTameable => configEnableTameable.Value;
 public static string  EnableVehicle => configEnableVehicle.Value;
 public static string    EnableAnimated => configEnableAnimated.Value;
 public static string  EnableSpecialInterface => configEnableSpecialInterface.Value;
 public static string    EnableIndestructible => configEnableIndestructible.Value;
 public static string  EnableCustomNotVanilla => configEnableCustomNotVanilla.Value;
 */

 public static ConfigWrapper Wrapper;

#nullable enable

  public static void Init(ConfigWrapper wrapper)
  {
    Wrapper = wrapper;
    var section = "1. General";
    configRemoveArea = wrapper.Bind(section, "Remove area", "0", "Removes same objects within the radius.");
    configRange = wrapper.Bind(section, "Hammer range", "50", "Range for actions.");
    configNoCost = wrapper.Bind(section, "No cost", false, "Removes durability, resource and stamina costs.");
    configIgnoreWards = wrapper.Bind(section, "Ignore wards", true, "Ignores ward restrictions.");
    configIgnoreNoBuild = wrapper.Bind(section, "Ignore no build", true, "Ignores no build areas.");
    configAllowInDungeons = wrapper.Bind(section, "Allow in dungeons", true, "Allows building in dungeons.");
    configRemoveAnything = wrapper.Bind(section, "Remove anything", false, "Allows removing anything.");
    configDisableLoot = wrapper.Bind(section, "Disable loot", false, "Prevents creatures and structures dropping loot when removed with the hammer.");
    configRepairAnything = wrapper.Bind(section, "Repair anything", false, "Allows reparing anything.");
    configOverwriteHealth = wrapper.Bind(section, "Overwrite health", "0", "Overwrites the health of built or repaired objects.");
    configInvulnerability = wrapper.Bind(section, "Set invulnerability", InvulnerabilityMode.Off, new ConfigDescription("Built objects are invulnerable.", new AcceptableValueList<string>(InvulnerabilityMode.Off, InvulnerabilityMode.On, InvulnerabilityMode.Damaged, InvulnerabilityMode.Worn, InvulnerabilityMode.Legacy)));
    configZoopMagic = wrapper.Bind(section, "Zoop magic mode", ZoopMagicMode.Off, new ConfigDescription("Zoop magic mode.", new AcceptableValueList<string>(ZoopMagicMode.Off, ZoopMagicMode.Mild, ZoopMagicMode.Wild)));

    configNoCreator = wrapper.Bind(section, "No creator", false, "Reduces save data by not setting the creator id.");
    configNoPrimaryTarget = wrapper.Bind(section, "No primary target", false, "Removes the primary target status. Requires World Edit Commands mod on the server.");
    configNoSecondaryTarget = wrapper.Bind(section, "No secondary target", false, "Removes the secondary target status. Requires World Edit Commands mod on the server.");
    configUnfreezeOnSelect = wrapper.Bind(section, "Unfreeze on select", true, "Removes the placement freeze when selecting a new object.");
    configResetOffsetOnUnfreeze = wrapper.Bind(section, "Reset offset on unfreeze", true, "Removes the placement offset when unfreezing the placement.");
    configUnfreezeOnUnequip = wrapper.Bind(section, "Unfreeze on unequip", true, "Removes the placement freeze when unequipping the hammer.");
    configSnapping = wrapper.Bind(section, "Snap points", SnappingMode.Corners, new ConfigDescription("Automatic snap points.", new AcceptableValueList<string>(SnappingMode.Off, SnappingMode.Edges, SnappingMode.Corners, SnappingMode.All)));
    configIgnoreOtherRestrictions = wrapper.Bind(section, "Ignore other restrictions", true, "Ignores any other restrictions (material, biome, etc.)");
    configPlaceEmptyRooms = wrapper.Bind(section, "Place empty rooms", false, "hammer_room command places rooms without their contents.");
    InitVisuals(wrapper);
    InitBinds(wrapper);
    section = "4. Messages";
    configDisableMessages = wrapper.Bind(section, "Disable messages", false, "Disables all messages from this mod.");
    configDisableOffsetMessages = wrapper.Bind(section, "Disable offset messages", false, "Disables messages from changing placement offset.");
    configDisableScaleMessages = wrapper.Bind(section, "Disable scale messages", false, "Disables messages from changing the scale.");
    configDisableSelectMessages = wrapper.Bind(section, "Disable select messages", false, "Disables messages from selecting objects.");
    section = "8. Json Blueprint Tests";
    configBlueprintNoChestContent = wrapper.Bind(section, "Disable Chest Content Save", false, "Doesnt Save chest contents when saving blueprints.");
    configBlueprintNoFuel  = wrapper.Bind(section, "Disable Fuel Save", false, "Doesnt Save the content of ItemHolders when saving blueprints.");
    configBlueprintItemHolderReduceData = wrapper.Bind(section, "Reduced ItemHolder Data", false, "Does nothing atm.");
      
    /*configEnableBuildPlayer = wrapper.Bind(section,  "Enable BuildPlayer",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude BuildPlayer Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableBuildPiece = wrapper.Bind(section,  "Enable BuildPiece",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude BuildPiece Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnablePlaceable = wrapper.Bind(section,  "Enable Placeable",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Placeable Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableCompfort = wrapper.Bind(section,  "Enable Compfort",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Compfort Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableLightSource = wrapper.Bind(section,  "Enable LightSource",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude LightSource Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableHoverable = wrapper.Bind(section,  "Enable Hoverable",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Hoverable Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableTextReceiver = wrapper.Bind(section,  "Enable TextReceiver",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude TextReceiver Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableInteractable = wrapper.Bind(section,  "Enable Interactable",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Interactable Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableObjectHolder = wrapper.Bind(section,  "Enable ObjectHolder",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude ObjectHolder Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableContainerPiece = wrapper.Bind(section,  "Enable ContainerPiece",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude ContainerPiece Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableCraftingStation = wrapper.Bind(section,  "Enable CraftingStation",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude CraftingStation Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableFuel = wrapper.Bind(section,  "Enable Fuel",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Fuel Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnablePickable = wrapper.Bind(section,  "Enable Pickable",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Pickable Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableCultivated = wrapper.Bind(section,  "Enable Cultivated",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Cultivated Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableDestroyableTerrain = wrapper.Bind(section,  "Enable DestroyableTerrain",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude DestroyableTerrain Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableFractured = wrapper.Bind(section,  "Enable Fractured",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Fractured Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableHoverableResourceNode = wrapper.Bind(section,  "Enable HoverableResourceNode",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude HoverableResourceNode Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableNonStatic = wrapper.Bind(section,  "Enable NonStatic",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude NonStatic Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableCreature = wrapper.Bind(section,  "Enable Creature",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Creature Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableTameable = wrapper.Bind(section,  "Enable Tameable",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Tameable Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableVehicle = wrapper.Bind(section,  "Enable Vehicle",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Vehicle Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableAnimated = wrapper.Bind(section,  "Enable Animated",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Animated Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableSpecialInterface = wrapper.Bind(section,  "Enable SpecialInterface",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude SpecialInterface Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableIndestructible = wrapper.Bind(section,  "Enable Indestructible",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude Indestructible Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    configEnableCustomNotVanilla = wrapper.Bind(section,  "Enable CustomNotVanilla",CategorySettingsStrings.Include,  new ConfigDescription("Includes, excludes or forces to exclude CustomNotVanilla Pieces", new AcceptableValueList<string>(CategorySettingsStrings.Include, CategorySettingsStrings.Exclude,CategorySettingsStrings.ForceExclude)));   
    */
    
      
    InitOther(wrapper);
    InitTools(wrapper);
    InitBlueprint(wrapper);
  }

}

public static class InvulnerabilityMode
{
  public const string Off = "Off";
  public const string On = "On";
  public const string Damaged = "Damaged";
  public const string Worn = "Worn";
  public const string Legacy = "Legacy";
}
public static class SnappingMode
{
  public const string All = "All";
  public const string Corners = "Corners";
  public const string Edges = "Edges";
  public const string Off = "Off";
}
public static class ZoopMagicMode
{
  public const string Off = "Off";
  public const string Mild = "Mild";
  public const string Wild = "Wild";
}