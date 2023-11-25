using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Service;
namespace InfinityHammer;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("server_devcommands", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("world_edit_commands", BepInDependency.DependencyFlags.SoftDependency)]
public class InfinityHammer : BaseUnityPlugin
{
  public const string GUID = "infinity_hammer";
  public const string NAME = "Infinity Hammer";
  public const string VERSION = "1.44";
  public static bool StructureTweaks = false;
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public static bool ConfigExists = false;
  public void Awake()
  {
    ConfigExists = File.Exists(Config.ConfigFilePath);
    Log = Logger;
    new Harmony(GUID).PatchAll();
    CommandWrapper.Init();
    ConfigWrapper wrapper = new("hammer_config", Config);
    Configuration.Init(wrapper);
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
      Log.LogDebug("ReadConfigValues called");
      Config.Reload();
    }
    catch
    {
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
    }
  }

  public void Start()
  {
    if (Chainloader.PluginInfos.TryGetValue("com.rolopogo.gizmo.comfy", out var info))
      GizmoWrapper.InitComfy(info.Instance.GetType().Assembly);
    if (Chainloader.PluginInfos.TryGetValue("m3to.mods.GizmoReloaded", out info))
      GizmoWrapper.InitReloaded(info.Instance.GetType().Assembly);
    StructureTweaks = Chainloader.PluginInfos.ContainsKey("structure_tweaks");
  }

  public void LateUpdate()
  {
    Ruler.Update();
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands
{
  static void Postfix()
  {
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
    new HammerToolCommand();
    new HammerGridCommand();
    new HammerSaveCommand();
    new HammerImportCommand();
    new HammerMirrorCommand();
    new HammerShapeCommand();
    new HammerZoopCommand();
    new HammerMeasureCommand();
    new HammerPosCommand();
  }
}

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
public class FejdStartupStart
{
  static void Create()
  {
    var pars = "from=x,z,y circle=r1-r2 angle=a rect=w1-w2,d";
    var parsSpawn = "from=x,z,y radius=r1-r2";
    var parsTo = "terrain to=tx,tz,ty circle=r1-r2 rect=w1-w2,d";
    var sub = CommandWrapper.Substitution();
    Console.instance.TryRunCommand($"alias hammer_terrain hammer_tool terrain {pars}");
    Console.instance.TryRunCommand($"alias hammer_object hammer_tool object {pars} height=h ignore=ignore");
    Console.instance.TryRunCommand($"alias hammer_spawn hammer_tool spawn_object {sub} {parsSpawn}");

    Console.instance.TryRunCommand($"alias hammer_terrain_to hammer_shape rectangle;hammer_tool {parsTo}");
    Console.instance.TryRunCommand($"alias hammer_slope hammer_terrain_to slope; hammer_scale_x_cmd {sub}");

  }
  static void Postfix()
  {
    if (CommandWrapper.ServerDevcommands != null)
    {
      var pars = "from=x,z,y circle=r angle=a rect=w,d";
      Console.instance.TryRunCommand($"alias hammer_area hammer_tool hammer {pars} height=h");
    }
    if (CommandWrapper.ServerDevcommands != null && CommandWrapper.WorldEditCommands != null)
    {
      Create();
    }
  }
}
