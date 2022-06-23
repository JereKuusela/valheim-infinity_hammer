using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
namespace InfinityHammer;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("server_devcommands", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("world_edit_commands", BepInDependency.DependencyFlags.SoftDependency)]
public class InfinityHammer : BaseUnityPlugin {
  public const string GUID = "infinity_hammer";
  public const string NAME = "Infinity Hammer";
  public const string VERSION = "1.16";
  ServerSync.ConfigSync ConfigSync = new(GUID) {
    DisplayName = NAME,
    CurrentVersion = VERSION,
  };
#nullable disable
  public static ManualLogSource Log;
  public void Awake() {
    Log = Logger;
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    Configuration.Init(ConfigSync, Config);
  }

  public void Start() {
    CommandWrapper.Init();
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
    new HammerScale();
    new HammerStackCommand();
    new HammerUndoCommand();
    new HammerFreezeCommand();
    new HammerGridCommand();
    new HammerSaveCommand();
    new HammerCommand();
    new HoeCommand();
    new HammerAddCommand();
    new HoeAddCommand();
    new HammerMirrorCommand();
  }
}


[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
public class FejdStartupStart {
  static void Postfix() {
    if (CommandWrapper.ServerDevcommands != null && CommandWrapper.WorldEditCommands != null) {
      Console.instance.TryRunCommand("alias hoe_terrain hoe_command terrain from=x,z,y angle=a");
      Console.instance.TryRunCommand("alias hoe_object hoe_command object center=x,z,y");
      Console.instance.TryRunCommand("alias hoe_slope hoe_command terrain to=x,z,y slope rect=$");
    }
  }
}
