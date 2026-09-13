using HarmonyLib;
namespace InfinityHammer;

[HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost)), HarmonyPriority(Priority.Last)]
public class Player_ManualUpdate
{
  public static CircleProjector? Projector = null;
  public static void Update()
  {
    var player = Player.m_localPlayer;
    if (!player || !player.m_placementGhost || !player.m_placementGhost.activeInHierarchy) return;
    if (Projector && Projector.gameObject.activeInHierarchy)
      Projector.Update();
  }
  static void Postfix(Player __instance)
  {
    if (__instance != Player.m_localPlayer) return;
    Projector = null;
    if (!__instance.m_placementGhost) return;
    Projector = __instance.m_placementGhost.GetComponentInChildren<CircleProjector>(true);
    // IH drives this projector after placement transforms, exactly once per
    // visible frame. Disable the automatic Unity Update to avoid doing both.
    if (Projector) Projector.enabled = false;
  }
}
