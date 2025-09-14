using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using InfinityTools;
using Service;
namespace InfinityHammer;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("bruce.valheim.comfymods.gizmo", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("server_devcommands", "1.95")]
[BepInDependency("world_edit_commands", "1.67")]
public class InfinityHammer : BaseUnityPlugin
{
  public const string GUID = "infinity_hammer";
  public const string NAME = "Infinity Hammer";
  public const string VERSION = "1.73.10";
  public static bool StructureTweaks = false;
#nullable disable
  public static ConfigWrapper Wrapper;
#nullable enable
  public void Awake()
  {
    Log.Init(Logger);
    new Harmony(GUID).PatchAll();
    Wrapper = new("hammer_config", Config);
    Configuration.Init(Wrapper);
    var migrate = File.Exists(Config.ConfigFilePath.Replace("infinity_hammer", "infinity_tools"));
    if (migrate) Migrate();

    try
    {
      SetupWatcher();
      ToolManager.CreateFile();
      ToolManager.SetupWatcher();
      ToolManager.FromFile();
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
  }
  public void LateUpdate()
  {
    Ruler.Update();
  }
}

[HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
public class ChatAwake
{

  private static bool Initialized = false;
  static void CreateAlias()
  {
    if (Initialized) return;
    Initialized = true;
    var pars = "from=<x>,<z>,<y> circle=<r>-<r2> angle=<a> rect=<w>-<w2>,<d>";
    var parsSpawn = "from=<x>,<z>,<y> radius=<r>-<r2>";
    var parsTo = "to=<x>,<z>,<y> circle=<r>-<r2> rect=<w>-<w2>,<d>";
    var sub = ServerDevcommands.Settings.Substitution;
    Console.instance.TryRunCommand($"alias tool_terrain terrain {pars}");
    Console.instance.TryRunCommand($"alias t_t tool tool_terrain");
    Console.instance.TryRunCommand($"alias tool_object object {pars} height=<h> ignore=<ignore> id=<include>");
    Console.instance.TryRunCommand($"alias t_o tool tool_object");
    Console.instance.TryRunCommand($"alias tool_spawn spawn_object {sub} {parsSpawn}");
    Console.instance.TryRunCommand($"alias t_s tool tool_spawn");

    Console.instance.TryRunCommand($"alias tool_terrain_to terrain {parsTo}");
    // Bit pointless but kept for legacy.
    Console.instance.TryRunCommand($"alias tool_slope tool_terrain_to slope");

    Console.instance.TryRunCommand($"alias tool_area hammer {pars} height=<h> ignore=<ignore> id=<include>");

  }
  static void Postfix()
  {
    CreateAlias();
    InfinityHammer.Wrapper.Bind();
  }
}
