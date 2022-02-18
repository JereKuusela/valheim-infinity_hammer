using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace InfinityHammer {
  [BepInPlugin("valheim.jerekuusela.infinity_hammer", "Infinity Hammer", "1.7.0.0")]
  [BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("valheim.jerekuusela.server_devcommands", BepInDependency.DependencyFlags.SoftDependency)]
  public class InfinityHammer : BaseUnityPlugin {
    public static ManualLogSource Log;
    public static bool IsServerDevcommands => ServerDevcommands != null;
    public static Assembly ServerDevcommands = null;
    public void Awake() {
      Log = Logger;
      Harmony harmony = new Harmony("valheim.jerekuusela.infinity_hammer");
      harmony.PatchAll();
      Settings.Init(Config);
    }

    public void Start() {
      if (Chainloader.PluginInfos.TryGetValue("valheim.jerekuusela.server_devcommands", out var info))
        ServerDevcommands = info.Instance.GetType().Assembly;
    }
  }

  [HarmonyPatch(typeof(Terminal), "InitTerminal")]
  public class SetCommands {
    public static void Postfix() {
      new HammerCommand();
    }
  }
}
