using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using Service;
using UnityEngine;
namespace InfinityHammer;

public partial class Configuration
{
#nullable disable
  public static ConfigEntry<string> configVersion;
  public static ConfigEntry<string> configPlanBuildFolder;
  public static string PlanBuildFolder => configPlanBuildFolder.Value;
  public static ConfigEntry<string> configBuildShareFolder;
  public static string BuildShareFolder => configBuildShareFolder.Value;
  public static ConfigEntry<string> configIgnoredRemoveIds;
  public static List<string> RemoveIds = new();
  public static ConfigEntry<string> configIgnoredIds;
  public static List<string> IgnoredIds = new();
  public static ConfigEntry<string> configHammerTools;
  public static HashSet<string> HammerTools = new();
  public static ConfigEntry<string> configHoeTools;
  public static HashSet<string> HoeTools = new();
  public static ConfigEntry<string> configMirrorFlip;
  public static HashSet<string> MirrorFlip = new();
  public static ConfigEntry<string> configDimensions;
  public static Dictionary<string, Vector3> Dimensions = new();
  public static ConfigEntry<int> hammerMenuTab;
  public static int HammerMenuTab => hammerMenuTab.Value;
  public static ConfigEntry<int> hammerMenuIndex;
  public static int HammerMenuIndex => hammerMenuIndex.Value;
  public static ConfigEntry<int> hoeMenuTab;
  public static int HoeMenuTab => hoeMenuTab.Value;
  public static ConfigEntry<int> hoeMenuIndex;
  public static int HoeMenuIndex => hoeMenuIndex.Value;
  public static ConfigEntry<string> commandDefaultSize;
  public static float CommandDefaultSize => ConfigWrapper.TryParseFloat(commandDefaultSize);
  public static ConfigEntry<bool> configEnabled;
  public static bool Enabled => configEnabled.Value;
#nullable enable
  private static List<string> ParseList(string value) => value.Split(',').Select(s => s.Trim().ToLower()).Where(s => s != "").ToList();
  private static HashSet<string> ParseHashList(string value) => value.Split(',').Select(s => s.Trim().ToLower()).Where(s => s != "").ToHashSet();

  private static Dictionary<string, Vector3> ParseSize(string value) => value.Split('|').Select(s => s.Trim().ToLower()).Where(s => s != "")
    .Select(s => s.Split(',')).Where(split => split.Length == 4).ToDictionary(split => split[0], split => Parse.TryVectorXZY(split, 1));

  private static void UpdateTools()
  {
    HammerTools = ParseHashList(configHammerTools.Value);
    HoeTools = ParseHashList(configHoeTools.Value);
  }
  private static void UpdateMirrorFlip()
  {
    MirrorFlip = ParseHashList(configMirrorFlip.Value);
  }
  private static void UpdateDimensions()
  {
    if (configDimensions.Value == "") configDimensions.Value = DimensionValues.Default;
    Dimensions = ParseSize(configDimensions.Value);
  }
  private static string Format(float f) => f.ToString("0.##", CultureInfo.InvariantCulture);
  public static void SetDimension(string value, Vector3 size)
  {
    Dimensions[value.ToLower()] = size;
    configDimensions.Value = string.Join("|", Dimensions.Select(kvp => $"{kvp.Key},{Format(kvp.Value.x)},{Format(kvp.Value.z)},{Format(kvp.Value.y)}"));
  }

  private static void InitOther(ConfigWrapper wrapper)
  {
    var section = "5. Other";
    configEnabled = wrapper.Bind(section, "Enabled", true, "Whether this mod is enabled at all.");
    configDimensions = wrapper.Bind(section, "Dimensions", "", "Object dimensions.");
    configDimensions.SettingChanged += (s, e) => UpdateDimensions();
    UpdateDimensions();
    configIgnoredIds = wrapper.BindList(section, "Ignored ids", "", "Object ids separated by , that are ignored by this mod.");
    configIgnoredIds.SettingChanged += (s, e) =>
    {
      IgnoredIds = ParseList(configIgnoredIds.Value);
      RemoveIds = ParseList(configIgnoredRemoveIds.Value);
      RemoveIds.AddRange(IgnoredIds);
    };
    IgnoredIds = ParseList(configIgnoredIds.Value);
    configIgnoredRemoveIds = wrapper.BindList(section, "Ignored remove ids", "", "Additional ids that are ignored when removing anything.");
    configIgnoredRemoveIds.SettingChanged += (s, e) =>
    {
      RemoveIds = ParseList(configIgnoredRemoveIds.Value);
      RemoveIds.AddRange(IgnoredIds);
    };
    RemoveIds = ParseList(configIgnoredRemoveIds.Value);
    RemoveIds.AddRange(IgnoredIds);
    configVersion = wrapper.Bind(section, "Version", InfinityHammer.ConfigExists ? "" : InfinityHammer.VERSION, "Version of this config.");
    configVersion.SettingChanged += (s, e) =>
    {
      if (configVersion.Value != InfinityHammer.VERSION)
        configVersion.Value = InfinityHammer.VERSION;
    };
    configHammerTools = wrapper.BindList(section, "Hammer tools", "hammer", "List of hammers.");
    configHammerTools.SettingChanged += (s, e) => UpdateTools();
    configHoeTools = wrapper.Bind(section, "Hoe tools", "hoe", "List of hoes.");
    configHoeTools.SettingChanged += (s, e) => UpdateTools();
    UpdateTools();
    configMirrorFlip = wrapper.Bind(section, "Mirror flip", "woodwall", "Object ids that get flipped instead of rotated when mirrored.");
    configMirrorFlip.SettingChanged += (s, e) => UpdateMirrorFlip();
    UpdateMirrorFlip();
    configPlanBuildFolder = wrapper.Bind(section, "Plan Build folder", "BepInEx/config/PlanBuild", "Folder relative to the Valheim.exe.");
    configBuildShareFolder = wrapper.Bind(section, "Build Share folder", "BuildShare/Builds", "Folder relative to the Valheim.exe.");

    commandDefaultSize = wrapper.Bind(section, "Command default size", "10", "Default size for commands.");
    commandDefaultSize.SettingChanged += (s, e) =>
    {
      Scaling.Command.SetScaleX(CommandDefaultSize);
      Scaling.Command.SetScaleZ(CommandDefaultSize);
    };
    Scaling.Command.SetScaleX(CommandDefaultSize);
    Scaling.Command.SetScaleZ(CommandDefaultSize);
    Scaling.Command.SetScaleY(0f);

    hammerMenuTab = wrapper.Bind(section, "Hammer menu tab", 0, "Index of the menu tab.");
    hammerMenuIndex = wrapper.Bind(section, "Hammer menu index", 1, "Index on the menu.");
    hoeMenuTab = wrapper.Bind(section, "Hoe menu tab", 0, "Index of the menu tab.");
    hoeMenuIndex = wrapper.Bind(section, "Hoe menu index", 5, "Index on the menu.");
  }
}