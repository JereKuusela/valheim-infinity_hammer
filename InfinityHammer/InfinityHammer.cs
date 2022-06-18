using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
namespace InfinityHammer;
[BepInPlugin("valheim.jerekuusela.infinity_hammer", "Infinity Hammer", "1.16.0.0")]
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("valheim.jerekuusela.server_devcommands", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("valheim.jerekuusela.world_edit_commands", BepInDependency.DependencyFlags.SoftDependency)]
public class InfinityHammer : BaseUnityPlugin {
#nullable disable
  public static ManualLogSource Log;
  public void Awake() {
    Log = Logger;
    Harmony harmony = new("valheim.jerekuusela.infinity_hammer");
    harmony.PatchAll();
    Settings.Init(Config);
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
    new HammerCommand();
    new HammerLocationCommand();
    new HammerBlueprintCommand();
    new HammerConfigCommand();
    new HammerMoveCommand();
    new HammerOffsetCommand();
    new HammerPlaceCommand();
    new HammerRepairCommand();
    new HammerRotateCommand();
    new HammerScaleCommand();
    new HammerStackCommand();
    new HammerUndoCommand();
    new HammerFreezeCommand();
    new HammerGridCommand();
    new HammerSaveCommand();
    new HammerCommandCommand();
  }
}


[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
public class FejdStartupStart {
  static void Postfix() {
    if (CommandWrapper.ServerDevcommands != null && CommandWrapper.WorldEditCommands != null) {
      Console.instance.TryRunCommand("alias hammer_terrain hammer_command terrain from=x,z,y angle=a");
      Console.instance.TryRunCommand("alias hammer_object hammer_command object center=x,z,y");
      Console.instance.TryRunCommand("alias hammer_slope hammer_command terrain to=x,z,y slope rect=$");
    }
  }
}
