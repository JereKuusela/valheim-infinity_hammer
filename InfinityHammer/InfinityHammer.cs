using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
namespace InfinityHammer;
[BepInPlugin("valheim.jerekuusela.infinity_hammer", "Infinity Hammer", "1.11.0.0")]
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("valheim.jerekuusela.server_devcommands", BepInDependency.DependencyFlags.SoftDependency)]
public class InfinityHammer : BaseUnityPlugin {
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

[HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
public class PreventDoubleZNetView {
  static bool Prefix(ZNetView __instance) {
    if (ZNetView.m_forceDisableInit || ZDOMan.instance == null) return true;
    if (ZNetView.m_useInitZDO && ZNetView.m_initZDO == null) {
      ZLog.LogWarning($"Preventing double ZNetView for {__instance.gameObject.name}. Recommended to remove these objects.");
      UnityEngine.Object.Destroy(__instance);
      return false;
    }
    return true;
  }
}