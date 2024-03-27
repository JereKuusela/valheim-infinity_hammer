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

  public static ConfigEntry<bool> configResetOffsetOnUnfreeze;
  public static bool ResetOffsetOnUnfreeze => configResetOffsetOnUnfreeze.Value;
  public static ConfigEntry<bool> configUnfreezeOnUnequip;
  public static bool UnfreezeOnUnequip => configUnfreezeOnUnequip.Value;
  public static ConfigEntry<bool> configAllSnapPoints;
  public static bool AllSnapPoints => configAllSnapPoints.Value;
  public static ConfigEntry<bool> configUnfreezeOnSelect;
  public static bool UnfreezeOnSelect => configUnfreezeOnSelect.Value;
  public static ConfigEntry<string> configOverwriteHealth;
  public static float OverwriteHealth => IsCheats ? Parse.Float(configOverwriteHealth.Value) : 0f;
  public static ConfigEntry<string> configInvulnerability;
  public static string Invulnerability => IsCheats ? configInvulnerability.Value : InvulnerabilityMode.Off;
  public static ConfigEntry<string> configRemoveArea;
  public static float RemoveArea => Enabled ? Parse.Float(configRemoveArea.Value) : 0f;
  public static ConfigEntry<string> configRange;
  public static float Range => IsCheats ? Parse.Float(configRange.Value) : 0f;
  public static ConfigEntry<bool> configSaveBlueprintData;
  public static bool SaveBlueprintData => configSaveBlueprintData.Value;
  public static ConfigEntry<bool> configSaveSimplerBlueprints;
  public static bool SimplerBlueprints => configSaveSimplerBlueprints.Value;
  public static ConfigEntry<bool> configUseBlueprintChance;
  public static bool UseBlueprintChance => configUseBlueprintChance.Value;
  public static ConfigEntry<string> configSavedObjectData;
  public static HashSet<string> SavedObjectData = [];
  private static void UpdateSavedObjectData()
  {
    SavedObjectData = ParseHashList(configSavedObjectData.Value);
  }

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

    configNoCreator = wrapper.Bind(section, "No creator", false, "Reduces save data by not setting the creator id.");
    configNoPrimaryTarget = wrapper.Bind(section, "No primary target", false, "Removes the primary target status. Requires World Edit Commands mod on the server.");
    configNoSecondaryTarget = wrapper.Bind(section, "No secondary target", false, "Removes the secondary target status. Requires World Edit Commands mod on the server.");
    configUnfreezeOnSelect = wrapper.Bind(section, "Unfreeze on select", true, "Removes the placement freeze when selecting a new object.");
    configResetOffsetOnUnfreeze = wrapper.Bind(section, "Reset offset on unfreeze", true, "Removes the placement offset when unfreezing the placement.");
    configUnfreezeOnUnequip = wrapper.Bind(section, "Unfreeze on unequip", true, "Removes the placement freeze when unequipping the hammer.");
    configAllSnapPoints = wrapper.Bind(section, "Snap points for all objects", false, "If enabled, multi selection creates snap points for every object.");
    configIgnoreOtherRestrictions = wrapper.Bind(section, "Ignore other restrictions", true, "Ignores any other restrictions (material, biome, etc.)");
    configSaveBlueprintData = wrapper.Bind(section, "Save data to blueprints", true, "If enabled, object data values are saved to blueprints.");
    configSaveSimplerBlueprints = wrapper.Bind(section, "Save simpler blueprints", false, "If enabled, only mandatory information is saved.");
    configSavedObjectData = wrapper.Bind(section, "Save object data blueprints", "", "Object ids that save extra data if save data is disabled.");
    configSavedObjectData.SettingChanged += (s, e) => UpdateSavedObjectData();
    configUseBlueprintChance = wrapper.Bind(section, "Use blueprint chance", false, "If enabled, chance field is checked from the blueprint.");
    UpdateSavedObjectData();
    InitVisuals(wrapper);
    InitBinds(wrapper);
    section = "4. Messages";
    configDisableMessages = wrapper.Bind(section, "Disable messages", false, "Disables all messages from this mod.");
    configDisableOffsetMessages = wrapper.Bind(section, "Disable offset messages", false, "Disables messages from changing placement offset.");
    configDisableScaleMessages = wrapper.Bind(section, "Disable scale messages", false, "Disables messages from changing the scale.");
    configDisableSelectMessages = wrapper.Bind(section, "Disable select messages", false, "Disables messages from selecting objects.");

    InitOther(wrapper);
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