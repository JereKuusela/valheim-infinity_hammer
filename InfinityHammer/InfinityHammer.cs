using BepInEx;
using HarmonyLib;

namespace InfinityHammer {
  [BepInPlugin("valheim.jerekuusela.infinity_hammer", "Infinity Hammer", "1.3.0.0")]
  [BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("valheim.jerekuusela.dev", BepInDependency.DependencyFlags.SoftDependency)]
  public class InfinityHammer : BaseUnityPlugin {
    public void Awake() {
      Harmony harmony = new Harmony("valheim.jerekuusela.infinity_hammer");
      harmony.PatchAll();
      Settings.Init(Config);
    }
  }

  [HarmonyPatch(typeof(Terminal), "InitTerminal")]
  public class SetCommands {
    public static void Postfix() {
      new HammerCommand();
    }
  }
}
