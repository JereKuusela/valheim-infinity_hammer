using HarmonyLib;
namespace InfinityHammer;
///<summary>Disables the resource check.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements), new[] { typeof(Piece), typeof(Player.RequirementMode) })]
public class HaveRequirements
{
  public static bool Prefix(ref bool __result)
  {
    if (Configuration.NoCost)
    {
      __result = true;
      return false;
    }
    return true;
  }
}
///<summary>Removes resource usage.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources))]
public class ConsumeResources
{
  static bool Prefix() => !Configuration.NoCost;
}
[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class UnlockPlacement
{
  public static void Prefix(Player __instance)
  {
    if (!__instance.m_placementGhost) return;
    var piece = __instance.m_placementGhost.GetComponent<Piece>();
    if (Configuration.AllowInDungeons) piece.m_allowedInDungeons = true;
  }
  public static void Postfix(Player __instance)
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
}
[HarmonyPatch(typeof(Location), nameof(Location.IsInsideNoBuildLocation)), HarmonyPriority(Priority.Last)]
public class IsInsideNoBuildLocation
{
  public static bool Postfix(bool result)
  {
    if (Configuration.IgnoreNoBuild) return false;
    return result;
  }
}
[HarmonyPatch(typeof(PrivateArea), nameof(PrivateArea.CheckAccess)), HarmonyPriority(Priority.Last)]
public class CheckAccess
{
  public static bool Postfix(bool result)
  {
    if (Configuration.IgnoreWards) return true;
    return result;
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.CheckCanRemovePiece)), HarmonyPriority(Priority.Last)]
public class CheckCanRemovePiece
{
  public static bool Postfix(bool result)
  {
    if (Configuration.NoCost) return true;
    return result;
  }
}