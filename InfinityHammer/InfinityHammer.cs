using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using InfinityTools;
using ServerDevcommands;
using Service;
namespace InfinityHammer;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("bruce.valheim.comfymods.gizmo", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("server_devcommands", "1.99")]
[BepInDependency("world_edit_commands", "1.67")]
public class InfinityHammer : BaseUnityPlugin
{
  public const string GUID = "infinity_hammer";
  public const string NAME = "Infinity Hammer";
  public const string VERSION = "1.79.10";
  public static bool StructureTweaks = false;
#nullable disable
  public static ConfigWrapper Wrapper;
#nullable enable
  public void Awake()
  {
    new Harmony(GUID).PatchAll();
    Wrapper = new("hammer_config", Config);
    Configuration.Init(Wrapper);
    var migrate = File.Exists(Config.ConfigFilePath.Replace("infinity_hammer", "infinity_tools"));
    if (migrate) Migrate();

    try
    {
      SetupWatcher();
    }
    catch
    {
      //
    }
  }
#pragma warning disable IDE0051
  private void OnDestroy()
  {
#pragma warning restore IDE0051
    Config.Save();
  }
  private void Migrate()
  {
    ConfigFile oldConfig = new(Config.ConfigFilePath.Replace("infinity_hammer", "infinity_tools"), false);
    ConfigWrapper legacyWrapper = new("tool_config", oldConfig);
    ConfigurationLegacy.Init(legacyWrapper);
    Configuration.configShapeCircle.Value = ConfigurationLegacy.configShapeCircle.Value;
    Configuration.configShapeRing.Value = ConfigurationLegacy.configShapeRing.Value;
    Configuration.configShapeSquare.Value = ConfigurationLegacy.configShapeSquare.Value;
    Configuration.configShapeRectangle.Value = ConfigurationLegacy.configShapeRectangle.Value;
    Configuration.configShapeFrame.Value = ConfigurationLegacy.configShapeFrame.Value;
    Configuration.configShowCommandValues.Value = ConfigurationLegacy.configShowCommandValues.Value;
    Configuration.commandHeightAmount.Value = ConfigurationLegacy.commandHeightAmount.Value;
    Configuration.commandModifier1.Value = ConfigurationLegacy.commandModifier1.Value;
    Configuration.commandModifier2.Value = ConfigurationLegacy.commandModifier2.Value;
    Configuration.shapeKey.Value = ConfigurationLegacy.shapeKey.Value;
    Configuration.commandRadius.Value = ConfigurationLegacy.commandRadius.Value;
    Configuration.commandRotate.Value = ConfigurationLegacy.commandRotate.Value;
    Configuration.commandDepth.Value = ConfigurationLegacy.commandDepth.Value;
    Configuration.commandHeight.Value = ConfigurationLegacy.commandHeight.Value;
    Config.Save();
    File.Delete(Config.ConfigFilePath.Replace("infinity_hammer", "infinity_tools"));
  }
  private void SetupWatcher()
  {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(Config.ConfigFilePath), Path.GetFileName(Config.ConfigFilePath))
    {
      NotifyFilter = NotifyFilters.Size
    };
    watcher.Changed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  private void ReadConfigValues(object sender, FileSystemEventArgs e)
  {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try
    {
      Config.Reload();
    }
    catch
    {
      Log.Error($"There was an issue loading your {Config.ConfigFilePath}");
      Log.Error("Please check your config entries for spelling and format!");
    }
  }

  public void Start()
  {
    if (Chainloader.PluginInfos.TryGetValue("bruce.valheim.comfymods.gizmo", out var info))
      PlaceRotation.Comfy = info.Instance.GetType().Assembly;
    if (Chainloader.PluginInfos.TryGetValue("m3to.mods.GizmoReloaded", out info))
      PlaceRotation.Reloaded = info.Instance.GetType().Assembly;
    StructureTweaks = Chainloader.PluginInfos.ContainsKey("structure_tweaks");
    PermissionApi.Subscribe(UpdateBuildMenu);
  }
  public void LateUpdate()
  {
    Ruler.Update();
    if (Player_ManualUpdate.Projector)
      Player_ManualUpdate.Projector.Update();
  }

  private void UpdateBuildMenu()
  {
    Player.m_localPlayer?.UpdateAvailablePiecesList();
  }
}


[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyPriority(Priority.HigherThanNormal)]
public class Initialize
{
  private static bool Initialized = false;
  static void Postfix()
  {
    if (Initialized) return;
    Initialized = true;
    new HammerAddPieceComponentsCommand();
    new HammerSelect();
    new HammerLocationCommand();
    new HammerBlueprintCommand();
    new HammerMoveCommand();
    new HammerOffsetCommand();
    new HammerPlaceCommand();
    new HammerRepairCommand();
    new HammerRotateCommand();
    new HammerScaleCommand();
    new HammerZoomCommand();
    new HammerStackCommand();
    new HammerFreezeCommand();
    new HammerGridCommand();
    new HammerSaveCommand();
    new HammerMirrorCommand();
    new HammerZoopCommand();
    new HammerMeasureCommand();
    new HammerPosCommand();
    new HammerMenuCommand();
    new ToolShapeCommand();
    new ToolCommand();
    new ToolImportCommand();
    new ToolExportCommand();
    new ToolCmdCommand();
    new HammerRoomCommand();
    new HammerMark();
    ToolManager.Initialize();
    InfinityHammer.Wrapper.Bind();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost)), HarmonyPriority(Priority.Last)]
public class Player_ManualUpdate
{
  public static CircleProjector? Projector = null;
  static void Postfix(Player __instance)
  {
    if (__instance.m_placementGhost)
      Projector = __instance.m_placementGhost.GetComponentInChildren<CircleProjector>(true);
    else
      Projector = null;
  }
}