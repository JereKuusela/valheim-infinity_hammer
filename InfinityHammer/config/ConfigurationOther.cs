using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;

public partial class Configuration
{
#nullable disable
  public static ConfigEntry<string> configDefaultCenterPiece;
  public static string DefaultCenterPiece => configDefaultCenterPiece.Value;
  public static ConfigEntry<string> configBlueprintFolder;
  public static string BlueprintGlobalFolder => Path.Combine("BepInEx", "config", configBlueprintFolder.Value);
  public static string BlueprintLocalFolder => Path.Combine(Paths.ConfigPath, configBlueprintFolder.Value);

  public static ConfigEntry<bool> configSaveBlueprintsToProfile;
  public static bool SaveBlueprintsToProfile => configSaveBlueprintsToProfile.Value;
  public static ConfigEntry<string> configIgnoredRemoveIds;
  public static List<string> RemoveIds = [];
  public static ConfigEntry<string> configIgnoredIds;
  public static List<string> IgnoredIds = [];
  public static ConfigEntry<string> configHammerTools;
  public static HashSet<string> HammerTools = [];
  public static ConfigEntry<string> configMirrorFlip;
  public static HashSet<string> MirrorFlip = [];
  public static ConfigEntry<string> configDimensions;
  public static Dictionary<string, Vector3> Dimensions = [];
  public static ConfigEntry<string> commandDefaultSize;
  public static float CommandDefaultSize => ConfigWrapper.TryParseFloat(commandDefaultSize);
  public static ConfigEntry<bool> configEnabled;
  public static bool Enabled => configEnabled.Value;
#nullable enable
  private static List<string> ParseList(string value) => value.Split(',').Select(s => s.Trim().ToLower()).Where(s => s != "").ToList();
  private static HashSet<string> ParseHashList(string value) => value.Split(',').Select(s => s.Trim().ToLower()).Where(s => s != "").ToHashSet();

  private static Dictionary<string, Vector3> ParseSize(string value) => value.Split('|').Select(s => s.Trim().ToLower()).Where(s => s != "")
    .Select(s => s.Split(',')).Where(split => split.Length == 4).ToDictionary(split => split[0], split => Parse.VectorXZY(split, 1));

  private static void UpdateTools()
  {
    HammerTools = ParseHashList(configHammerTools.Value);
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
    configHammerTools = wrapper.BindList(section, "Hammer tools", "hammer", "List of hammers.");
    configHammerTools.SettingChanged += (s, e) => UpdateTools();
    UpdateTools();
    configMirrorFlip = wrapper.Bind(section, "Mirror flip", "woodwall", "Object ids that get flipped instead of rotated when mirrored.");
    configMirrorFlip.SettingChanged += (s, e) => UpdateMirrorFlip();
    UpdateMirrorFlip();
    configBlueprintFolder = wrapper.Bind(section, "Blueprint folder", "PlanBuild", "Folder relative to the config folder.");
    configSaveBlueprintsToProfile = wrapper.Bind(section, "Save blueprints to profile", false, "If enabled, blueprints are saved to the profile folder instead of base Valheim folder.");
    configDefaultCenterPiece = wrapper.Bind(section, "Blueprint center piece", "piece_bpcenterpoint", "Default center piece for blueprints.");

    commandDefaultSize = wrapper.Bind(section, "Command default size", "10", "Default size for commands.");
    commandDefaultSize.SettingChanged += (s, e) =>
    {
      Scaling.Command.SetScaleX(CommandDefaultSize);
      Scaling.Command.SetScaleZ(CommandDefaultSize);
    };
    Scaling.Command.SetScaleX(CommandDefaultSize);
    Scaling.Command.SetScaleZ(CommandDefaultSize);
    Scaling.Command.SetScaleY(0f);
  }
}