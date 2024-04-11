using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Service;
namespace InfinityHammer;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("bruce.valheim.comfymods.gizmo", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("server_devcommands", "1.76")]
public class InfinityHammer : BaseUnityPlugin
{
  public const string GUID = "infinity_hammer";
  public const string NAME = "Infinity Hammer";
  public const string VERSION = "1.52";
  public static bool StructureTweaks = false;
#nullable disable
  public static ManualLogSource Log;
  public static ConfigWrapper Wrapper;
#nullable enable
  public static bool ConfigExists = false;
  public void Awake()
  {
    ConfigExists = File.Exists(Config.ConfigFilePath);
    Log = Logger;
    new Harmony(GUID).PatchAll();
    Wrapper = new("hammer_config", Config);
    Configuration.Init(Wrapper);
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
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
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
  }
}

[HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
public class ChatAwake
{
  static void Postfix()
  {
    InfinityHammer.Wrapper.Bind();
  }
}
