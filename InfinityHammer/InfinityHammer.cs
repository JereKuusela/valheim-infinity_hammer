using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace InfinityHammer {
  [BepInPlugin("valheim.jerekuusela.infinity_hammer", "Infinity Hammer", "1.11.0.0")]
  [BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("valheim.jerekuusela.server_devcommands", BepInDependency.DependencyFlags.SoftDependency)]
  public class InfinityHammer : BaseUnityPlugin {
    public static ManualLogSource Log;
    public static bool IsServerDevcommands = false;
    public static Assembly ServerDevcommands = null;
    public void Awake() {
      Log = Logger;
      Harmony harmony = new Harmony("valheim.jerekuusela.infinity_hammer");
      harmony.PatchAll();
      Settings.Init(Config);
    }

    public void Start() {
      if (Chainloader.PluginInfos.TryGetValue("valheim.jerekuusela.server_devcommands", out var info)) {
        if (info.Metadata.Version.Major == 1 && info.Metadata.Version.Minor < 12) {
          Log.LogWarning($"Server devcommands v{info.Metadata.Version.Major}.{info.Metadata.Version.Minor} is outdated. Please update for better compatibility!");
        } else {
          IsServerDevcommands = true;
          ServerDevcommands = info.Instance.GetType().Assembly;
        }
      }
      if (Chainloader.PluginInfos.TryGetValue("com.rolopogo.gizmo.comfy", out info))
        GizmoWrapper.InitComfy(info.Instance.GetType().Assembly);
    }
  }

  [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
  public class SetCommands {
    static void Postfix() {
      new HammerAddPieceComponentsCommand();
      new HammerCommand();
      new HammerConfigCommand();
      new HammerMoveCommand();
      new HammerOffsetCommand();
      new HammerPlaceCommand();
      new HammerRepairCommand();
      new HammerRotateCommand();
      new HammerScaleCommand();
      new HammerSetupBindsCommand();
      new HammerStackCommand();
      new HammerUndoCommand();
    }
  }
}
