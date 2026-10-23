using HarmonyLib;
namespace InfinityHammer;

[HarmonyPatch]
public static class Restrictions
{
  ///<summary>Disables the resource check.</summary>
  [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements), typeof(Piece), typeof(Player.RequirementMode)), HarmonyPrefix]
  static bool HaveRequirements(ref bool __result)
  {
    if (Configuration.NoResourceCost)
    {
      __result = true;
      return false;
    }
    return true;
  }

  ///<summary>Removes resource usage.</summary>
  [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources)), HarmonyPrefix]
  static bool ConsumeResources() => !Configuration.NoResourceCost;

  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyPrefix]
  static void UnlockPlacementPrefix(Player __instance)
  {
    if (!__instance.m_placementGhost) return;
    var piece = __instance.m_placementGhost.GetComponent<Piece>();
    if (Configuration.AllowInDungeons) piece.m_allowedInDungeons = true;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyFinalizer]
  static void UnlockPlacementFinalizer(Player __instance)
  {
    if (!__instance.m_placementGhost) return;
    if (!__instance.m_placementGhost.activeSelf) return;
    // These three are handled by other settings in other places.
    var status = __instance.m_placementStatus;
    if (status == Player.PlacementStatus.NoBuildZone) return;
    if (status == Player.PlacementStatus.NotInDungeon) return;
    if (status == Player.PlacementStatus.PrivateZone) return;
    if (!Configuration.IgnoreOtherRestrictions) return;
    __instance.m_placementStatus = Player.PlacementStatus.Valid;
    __instance.SetPlacementGhostValid(true);
  }

  [HarmonyPatch(typeof(Location), nameof(Location.IsInsideNoBuildLocation)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
  static bool IsInsideNoBuildLocation(bool result) => !Configuration.IgnoreNoBuild && result;

  [HarmonyPatch(typeof(Location), nameof(Location.IsInsideActiveBossDungeon)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
  static bool IsInsideActiveBossDungeon(bool result) => !Configuration.IgnoreNoBuild && result;

  [HarmonyPatch(typeof(PrivateArea), nameof(PrivateArea.CheckAccess)), HarmonyPrefix, HarmonyPriority(Priority.Last)]
  static bool CheckAccess(ref bool __result)
  {
    if (Configuration.IgnoreWards) __result = true;
    return !Configuration.IgnoreWards;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.CheckCanRemovePiece)), HarmonyPrefix, HarmonyPriority(Priority.Last)]
  static bool CheckCanRemovePiece(ref bool __result)
  {
    if (Configuration.NoResourceCost) __result = true;
    return !Configuration.NoResourceCost;
  }
}