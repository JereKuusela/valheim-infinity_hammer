using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace InfinityHammer {
  [BepInPlugin("valheim.jerekuusela.infinity_hammer", "Infinity Hammer", "1.6.0.0")]
  [BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("valheim.jerekuusela.dev", BepInDependency.DependencyFlags.SoftDependency)]
  public class InfinityHammer : BaseUnityPlugin {
    public static ManualLogSource Log;
    public static bool IsDev => DEV != null;
    public static Assembly DEV = null;
    public void Awake() {
      Log = Logger;
      Harmony harmony = new Harmony("valheim.jerekuusela.infinity_hammer");
      harmony.PatchAll();
      Settings.Init(Config);
    }

    public void Start() {
      if (Chainloader.PluginInfos.TryGetValue("valheim.jerekuusela.dev", out var info)) {
        if (info.Metadata.Version.Major == 1 && info.Metadata.Version.Minor < 8) {
          Log.LogWarning($"Server devcommands v{info.Metadata.Version.Major}.{info.Metadata.Version.Minor} is outdated. Please update for better compatibility");
          return;
        }
        DEV = info.Instance.GetType().Assembly;
      }
    }
  }

  [HarmonyPatch(typeof(Terminal), "InitTerminal")]
  public class SetCommands {
    public static void Postfix() {
      new HammerCommand();
    }
  }
}
