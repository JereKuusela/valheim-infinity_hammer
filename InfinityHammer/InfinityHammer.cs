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
public class InfinityHammer : BaseUnityPlugin {
  public const string GUID = "infinity_hammer";
  public const string NAME = "Infinity Hammer";
  const string LEGACY_GUID = "valheim.jerekuusela.infinity_hammer";
  public const string VERSION = "1.18";
  ServerSync.ConfigSync ConfigSync = new(GUID) {
    DisplayName = NAME,
    CurrentVersion = VERSION,
  };
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public void Awake() {
    var legacyConfig = Path.Combine(Path.GetDirectoryName(Config.ConfigFilePath), $"{LEGACY_GUID}.cfg");
    var config = Path.Combine(Path.GetDirectoryName(Config.ConfigFilePath), $"{GUID}.cfg");
    if (File.Exists(legacyConfig)) {
      if (File.Exists(config))
        File.Delete(legacyConfig);
      else
        File.Move(legacyConfig, config);
    }
    Log = Logger;
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    CommandWrapper.Init();
    ConfigWrapper wrapper = new("hammer_config", Config, ConfigSync);
    Configuration.Init(wrapper);
    SetupWatcher();
  }
  private void OnDestroy() {
    Config.Save();
  }

  private void SetupWatcher() {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(Config.ConfigFilePath), Path.GetFileName(Config.ConfigFilePath));
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  private void ReadConfigValues(object sender, FileSystemEventArgs e) {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try {
      Log.LogDebug("ReadConfigValues called");
      Config.Reload();
    } catch {
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
    }
  }

  public void Start() {
    if (Chainloader.PluginInfos.TryGetValue("com.rolopogo.gizmo.comfy", out var info))
      GizmoWrapper.InitComfy(info.Instance.GetType().Assembly);
    if (Chainloader.PluginInfos.TryGetValue("m3to.mods.GizmoReloaded", out info))
      GizmoWrapper.InitReloaded(info.Instance.GetType().Assembly);
  }

  public void LateUpdate() {
    Ruler.Update();
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands {
  static void Postfix() {
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
    new HammerUndoCommand();
    new HammerFreezeCommand();
    new HammerGridCommand();
    new HammerSaveCommand();
    new HammerCommand();
    new HoeCommand();
    new HammerAddCommand();
    new HoeAddCommand();
    new HammerRemoveCommand();
    new HoeRemoveCommand();
    new HammerMirrorCommand();
    new HammerListCommand();
    new HoeListCommand();
    new HammerShapeCommand();
  }
}

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
public class FejdStartupStart {
  static void Postfix() {
    var pars = "from=x,z,y circle=r angle=a rect=w,d";
    if (CommandWrapper.ServerDevcommands != null) {
      Console.instance.TryRunCommand($"alias hammer_area hammer_command hammer {pars} height=h");
    }
    if (CommandWrapper.ServerDevcommands != null && CommandWrapper.WorldEditCommands != null) {
      Console.instance.TryRunCommand($"alias hammer_terrain hammer_command terrain {pars}");
      Console.instance.TryRunCommand($"alias hammer_object hammer_command object {pars} height=h");
      Console.instance.TryRunCommand($"alias hoe_terrain hoe_command terrain {pars}");
      Console.instance.TryRunCommand($"alias hoe_object hoe_command object {pars} height=h");
      Console.instance.TryRunCommand("alias hoe_slope hoe_command terrain to=x,z,y slope rect=$");
    }
  }
}
