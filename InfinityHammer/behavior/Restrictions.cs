using System;
using HarmonyLib;

namespace InfinityHammer {

  ///<summary>Disables the resource check.</summary>
  [HarmonyPatch(typeof(Player), "HaveRequirements", new Type[] { typeof(Piece), typeof(Player.RequirementMode) })]
  public class HaveRequirements {
    public static bool Prefix(ref bool __result) {
      if (Settings.NoBuildCost) {
        __result = true;
        return false;
      }
      return true;
    }
  }
  ///<summary>Removes resource usage.</summary>
  [HarmonyPatch(typeof(Player), "ConsumeResources")]
  public class ConsumeResources {
    public static bool Prefix() => !Settings.NoBuildCost;
  }
  [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
  public class UnlockPlacement {
    public static void Postfix(Player __instance) => Placing.ForceValidPlacement(__instance);
  }
  [HarmonyPatch(typeof(Location), "IsInsideNoBuildLocation")]
  public class IsInsideNoBuildLocation {
    public static bool Prefix(ref bool __result) {
      if (Settings.IgnoreNoBuild) {
        __result = false;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(PrivateArea), "CheckAccess")]
  public class CheckAccess {
    public static bool Prefix(ref bool __result) {
      if (Settings.IgnoreWards) {
        __result = true;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
  public class CheckCanRemovePiece {
    public static bool Prefix(ref bool __result) {
      if (Settings.NoBuildCost) {
        __result = true;
        return false;
      }
      return true;
    }
  }
}