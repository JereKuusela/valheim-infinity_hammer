using System.Collections.Generic;
using System.IO;
using System;
using BepInEx;
using BepInEx.Configuration;
using Service;
using ServerDevcommands;
namespace InfinityHammer;

public partial class Configuration
{
#nullable disable

  public static ConfigEntry<bool> configSaveBlueprintData;
  public static bool SaveBlueprintData => configSaveBlueprintData.Value;
  public static ConfigEntry<string> configBlueprintCenterPiece;
  public static string BlueprintCenterPiece => configBlueprintCenterPiece.Value;
  public static ConfigEntry<string> configBlueprintSnapPiece;
  public static string BlueprintSnapPiece => configBlueprintSnapPiece.Value;
  public static ConfigEntry<bool> configSaveSimplerBlueprints;
  public static bool SimplerBlueprints => configSaveSimplerBlueprints.Value;
  public static ConfigEntry<bool> configUseBlueprintChance;
  public static bool UseBlueprintChance => configUseBlueprintChance.Value;
  public static ConfigEntry<bool> configIncludeTerrainHeight;
  public static bool IncludeTerrainHeight => configIncludeTerrainHeight.Value;
  public static ConfigEntry<bool> configIncludeTerrainPaint;
  public static bool IncludeTerrainPaint => configIncludeTerrainPaint.Value;
  public static ConfigEntry<string> configBlueprintTerrainHeightOffset;
  public static float BlueprintTerrainHeightOffset => Parse.Float(configBlueprintTerrainHeightOffset.Value);
  public static ConfigEntry<string> configBlueprintTerrainPaintOffset;
  public static float BlueprintTerrainPaintOffset => Parse.Float(configBlueprintTerrainPaintOffset.Value);
  public static ConfigEntry<string> configBlueprintTerrainHeightSmooth;
  public static float BlueprintTerrainHeightSmooth => ClampBlueprintSmooth(Parse.Float(configBlueprintTerrainHeightSmooth.Value));
  public static ConfigEntry<string> configBlueprintTerrainPaintSmooth;
  public static float BlueprintTerrainPaintSmooth => ClampBlueprintSmooth(Parse.Float(configBlueprintTerrainPaintSmooth.Value));
  public static ConfigEntry<string> configBlueprintTerrainNodeSpacing;
  public static float BlueprintTerrainNodeSpacing => Math.Max(0f, Parse.Float(configBlueprintTerrainNodeSpacing.Value));
  public static ConfigEntry<string> configBlueprintPaintNodeSpacing;
  public static float BlueprintPaintNodeSpacing => Math.Max(0f, Parse.Float(configBlueprintPaintNodeSpacing.Value));
  public static ConfigEntry<string> configBlueprintFolder;
  public static string BlueprintGlobalFolder => Path.Combine("BepInEx", "config", configBlueprintFolder.Value);
  public static string BlueprintLocalFolder => Path.Combine(Paths.ConfigPath, configBlueprintFolder.Value);

  public static ConfigEntry<bool> configSaveBlueprintsToProfile;
  public static bool SaveBlueprintsToProfile => configSaveBlueprintsToProfile.Value;
  public static ConfigEntry<string> configSavedObjectData;
  public static HashSet<string> SavedObjectData = [];
  private static void UpdateSavedObjectData()
  {
    SavedObjectData = ParseHashList(configSavedObjectData.Value);
  }
  private static float ClampBlueprintSmooth(float value)
  {
    if (value < 0f) return 0f;
    if (value > 1f) return 1f;
    return value;
  }
  private static void InitBlueprint(ConfigWrapper wrapper)
  {
    var section = "6. Blueprints";
    configBlueprintFolder = wrapper.Bind(section, "Blueprint folder", "PlanBuild", "Folder relative to the config folder.");
    configSaveBlueprintsToProfile = wrapper.Bind(section, "Save blueprints to profile", false, "If enabled, blueprints are saved to the profile folder instead of base Valheim folder.");
    configSaveBlueprintData = wrapper.Bind(section, "Save data to blueprints", true, "If enabled, object data values are saved to blueprints.");
    configSaveSimplerBlueprints = wrapper.Bind(section, "Save simpler blueprints", false, "If enabled, only mandatory information is saved.");
    configSavedObjectData = wrapper.Bind(section, "Save object data blueprints", "", "Object ids that save extra data if save data is disabled.");
    configSavedObjectData.SettingChanged += (s, e) => UpdateSavedObjectData();
    configUseBlueprintChance = wrapper.Bind(section, "Use blueprint chance", false, "If enabled, chance field is checked from the blueprint.");
    configIncludeTerrainHeight = wrapper.Bind(section, "Include terrain height", false, "If enabled, hammer area selection captures terrain height.");
    configIncludeTerrainPaint = wrapper.Bind(section, "Include terrain paint", false, "If enabled, hammer area selection captures terrain paint.");
    configBlueprintTerrainHeightOffset = wrapper.Bind(section, "Terrain height offset", "0", "Radius/size offset in meters used when capturing terrain height.");
    configBlueprintTerrainPaintOffset = wrapper.Bind(section, "Terrain paint offset", "0", "Radius/size offset in meters used when capturing terrain paint.");
    configBlueprintTerrainHeightSmooth = wrapper.Bind(section, "Terrain height smooth", "0", "How gradually blueprint terrain height changes are applied. 0 = exact, 1 = fully gradual.");
    configBlueprintTerrainPaintSmooth = wrapper.Bind(section, "Terrain paint smooth", "0", "How gradually blueprint terrain paint changes are applied. 0 = exact, 1 = fully gradual.");
    configBlueprintTerrainNodeSpacing = wrapper.Bind(section, "Terrain height spacing", "0", "Distance between captured terrain nodes in meters. Use 0 to auto-detect from the terrain heightmap scale.");
    configBlueprintPaintNodeSpacing = wrapper.Bind(section, "Terrain paint spacing", "0", "Distance between captured terrain paint nodes in meters. Use 0 to auto-detect from the terrain heightmap scale.");
    configBlueprintCenterPiece = wrapper.Bind(section, "Blueprint center piece", "", "Piece name that is used as the center point when saving a blueprint.");
    configBlueprintSnapPiece = wrapper.Bind(section, "Blueprint snap piece", "", "Piece name that is used as the snap point when saving a blueprint.");
    UpdateSavedObjectData();
  }
}