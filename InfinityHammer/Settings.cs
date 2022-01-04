using System.Collections.Generic;
using BepInEx.Configuration;
using Service;

namespace InfinityHammer {
  public class Settings {
    private static bool IsCheats => Enabled && ((ZNet.instance && ZNet.instance.IsServer()) || Console.instance.IsCheatsEnabled());

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
    public static ConfigEntry<bool> configEnableUndo;
    public static bool EnableUndo => configEnableUndo.Value && IsCheats;
    public static ConfigEntry<bool> configNoCreator;
    public static bool NoCreator => configNoCreator.Value && IsCheats;
    public static ConfigEntry<bool> configInfiniteHealth;
    public static bool InfiniteHealth => configInfiniteHealth.Value && IsCheats;
    public static ConfigEntry<bool> configCopyRotation;
    public static bool CopyRotation => configCopyRotation.Value;
    public static ConfigEntry<string> configUndoLimit;
    public static int UndoLimit => (int)Helper.ParseFloat(configUndoLimit.Value, 0f);
    public static ConfigEntry<string> configSelectRange;
    public static float SelectRange => Helper.ParseFloat(configSelectRange.Value, 0f);
    public static ConfigEntry<string> configRemoveRange;
    public static float RemoveRange => IsCheats ? Helper.ParseFloat(configRemoveRange.Value, 0f) : 0f;
    public static ConfigEntry<string> configBuildRange;
    public static float BuildRange => IsCheats ? Helper.ParseFloat(configBuildRange.Value, 0f) : 0f;
    public static ConfigEntry<string> configScaleStep;
    public static float ScaleStep => IsCheats ? Helper.ParseFloat(configScaleStep.Value, 0f) : 0f;
    public static ConfigEntry<bool> configEnabled;
    public static bool Enabled => configEnabled.Value;

    public static void Init(ConfigFile config) {
      var section = "General";
      configEnabled = config.Bind(section, "Enabled", true, "Whether this mod is enabled at all.");
      section = "Powers";
      configSelectRange = config.Bind(section, "Select range", "50", "Range for selecting objects.");
      configRemoveRange = config.Bind(section, "Remove range", "0", "Range for removing objects (0 = default).");
      configBuildRange = config.Bind(section, "Build range", "0", "Range for placing objects (0 = default)");
      configEnableUndo = config.Bind(section, "Enable undo", true, "Enabled undo and redo for placing/removing.");
      configCopyRotation = config.Bind(section, "Copy rotation", true, "Copies rotation of the selected object.");
      configNoBuildCost = config.Bind(section, "No build cost", true, "Removes build costs and requirements.");
      configIgnoreWards = config.Bind(section, "Ignore wards", true, "Ignores ward restrictions.");
      configIgnoreNoBuild = config.Bind(section, "Ignore no build", true, "Ignores no build areas.");
      configNoStaminaCost = config.Bind(section, "No stamina cost", true, "Removes hammer stamina usage.");
      configNoDurabilityLoss = config.Bind(section, "No durability loss", true, "Removes hammer durability usage.");
      configAllObjects = config.Bind(section, "All objects", true, "Allows placement of non-default objects.");
      configCopyState = config.Bind(section, "Copy state", true, "Copies object's internal state.");
      configAllowInDungeons = config.Bind(section, "Allow in dungeons", true, "Allows building in dungeons.");
      configRemoveAnything = config.Bind(section, "Remove anything", false, "Allows removing anything.");
      configInfiniteHealth = config.Bind(section, "Max health", false, "Built and repaired objects are set to very high health.");
      configNoCreator = config.Bind(section, "No creator", false, "Build without setting the creator (ignored by enemies).");
      configIgnoreOtherRestrictions = config.Bind(section, "Ignore other restrictions", true, "Ignores any other restrictions (material, biome, etc.)");
      configScaleStep = config.Bind(section, "Scaling step", "0.05", "How much each scale up/down affects the size");
      configUndoLimit = config.Bind(section, "Max undo steps", "50", "How many undo actions are stored.");
      configUndoLimit.SettingChanged += (s, e) => UndoManager.MaxSteps = UndoLimit;
    }

    public static List<string> Options = new List<string>() {
      "enabled", "select_range", "remove_range", "build_range", "enable_undo", "copy_rotation", "no_build_cost",
      "ignore_wards", "ignore_no_build", "no_stamina_cost", "no_durability_loss", "all_objects", "copy_state",
      "allow_in_dungeons", "remove_anything", "ignore_other_restrictions", "scaling_step", "max_undo_steps", "no_creator",
      "infinite_health"
    };
    private static string State(bool value) => value ? "enabled" : "disabled";
    private static void Toggle(Terminal context, ConfigEntry<bool> setting, string name, bool reverse = false) {
      setting.Value = !setting.Value;
      Helper.AddMessage(context, $"{name} {State(reverse ? !setting.Value : setting.Value)}.");

    }
    public static void UpdateValue(Terminal context, string key, string value) {
      if (key == "enabled") Toggle(context, configEnabled, "Infinity Hammer");
      if (key == "infinite_health") Toggle(context, configInfiniteHealth, "Infinite health");
      if (key == "enable_undo") Toggle(context, configEnableUndo, "Undo");
      if (key == "copy_rotation") Toggle(context, configCopyRotation, "Copy rotation");
      if (key == "no_build_cost") Toggle(context, configNoBuildCost, "Build costs", true);
      if (key == "ignore_wards") Toggle(context, configIgnoreWards, "Building inside wards", true);
      if (key == "ignore_no_build") Toggle(context, configIgnoreNoBuild, "No build areas", true);
      if (key == "no_stamina_cost") Toggle(context, configNoStaminaCost, "Hammer stamina cost", true);
      if (key == "no_durability_loss") Toggle(context, configNoDurabilityLoss, "Hammer durability cost", true);
      if (key == "all_objects") Toggle(context, configAllObjects, "All objects");
      if (key == "copy_state") Toggle(context, configCopyState, "Copy state");
      if (key == "allow_in_dungeons") Toggle(context, configAllowInDungeons, "Building in dungeons");
      if (key == "remove_anything") Toggle(context, configRemoveAnything, "Removing anything");
      if (key == "ignore_other_restrictions") Toggle(context, configIgnoreOtherRestrictions, "Other build restrictions", true);
      if (key == "no_creator") Toggle(context, configNoCreator, "Creator", true);
      if (key == "select_range") {
        configSelectRange.Value = value;
        Helper.AddMessage(context, $"Select range set to {value} meters.");
      }
      if (key == "remove_range") {
        configRemoveRange.Value = value;
        Helper.AddMessage(context, $"Remove range set to {value} meters.");
      }
      if (key == "build_range") {
        configBuildRange.Value = value;
        Helper.AddMessage(context, $"Build range set to {value} meters.");
      }
      if (key == "scaling_step") {
        configScaleStep.Value = value;
        Helper.AddMessage(context, $"Scaling step set to {value}%.");
      }
      if (key == "max_undo_steps") {
        configUndoLimit.Value = value;
        Helper.AddMessage(context, $"Max undo steps set to {value}%.");
      }
    }
  }
}
