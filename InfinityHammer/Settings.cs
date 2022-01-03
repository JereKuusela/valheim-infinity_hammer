using BepInEx.Configuration;

namespace InfinityHammer {
  public partial class Settings {
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
    public static ConfigEntry<bool> configEnabled;
    public static bool Enabled => configEnabled.Value;

    public static void Init(ConfigFile config) {
      var section = "General";
      configEnabled = config.Bind(section, "Enabled", true, "Whether this mod is enabled at all.");
      section = "Powers";
      configNoBuildCost = config.Bind(section, "No build cost", true, "Removes build costs and requirements.");
      configIgnoreWards = config.Bind(section, "Ignore wards", true, "Ignores ward restrictions.");
      configIgnoreNoBuild = config.Bind(section, "Ignore no build", true, "Ignores no build areas.");
      configNoStaminaCost = config.Bind(section, "No stamina cost", true, "Removes hammer stamina usage.");
      configNoDurabilityLoss = config.Bind(section, "No durability loss", true, "Removes hammer durability usage.");
      configAllObjects = config.Bind(section, "All objects", true, "Allows placement of non-default objects.");
      configCopyState = config.Bind(section, "Copy state", true, "Copies object's internal state.");
      configAllowInDungeons = config.Bind(section, "Allow in dungeons", true, "Allows building in dungeons.");
      configRemoveAnything = config.Bind(section, "Remove anything", false, "Allows removing anything (use at your own risk).");
      configIgnoreOtherRestrictions = config.Bind(section, "Ignore other restrictions", true, "Ignores any other restrictions (material, biome, etc.)");
    }
  }
}
