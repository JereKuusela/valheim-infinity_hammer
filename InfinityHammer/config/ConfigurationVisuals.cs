using BepInEx.Configuration;
using HarmonyLib;
using Service;

namespace InfinityHammer;

public partial class Configuration
{

#nullable disable
  // Visuals.
  public static ConfigEntry<bool> hideEffects;
  public static bool HideEffects => hideEffects.Value && Enabled;
  public static ConfigEntry<bool> hidePlacementMarker;
  public static bool HidePlacementMarker => hidePlacementMarker.Value && Enabled;
  public static ConfigEntry<bool> hideSupportColor;
  public static bool HideSupportColor => hideSupportColor.Value && Enabled;
  public static ConfigEntry<bool> hidePieceHealth;
  public static bool HidePieceHealth => hidePieceHealth.Value && Enabled;
#nullable enable
  public static void InitVisuals(ConfigWrapper wrapper)
  {
    Wrapper = wrapper;
    var section = "2. Visual";
    hideEffects = wrapper.Bind(section, "No effects", false, "Hides visual effects of building, repairing and destroying.");
    hidePlacementMarker = wrapper.Bind(section, "No placement marker", false, "Hides the yellow placement marker (also affects Gizmo mod).");
    hideSupportColor = wrapper.Bind(section, "No support indicator", false, "Hides the color that shows support.");
    hidePieceHealth = wrapper.Bind(section, "No health indicator", false, "Hides the piece health bar.");
  }
}

[HarmonyPatch]
public static class Visuals
{
  public static bool HideEffectsActive = false;

  [HarmonyPatch(typeof(EffectList), nameof(EffectList.Create)), HarmonyPrefix]
  static bool HideEffects() => !HideEffectsActive || !Configuration.HideEffects;

  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyPostfix]
  static void HidePlacementMarker(Player __instance)
  {
    var marker = __instance.m_placementMarkerInstance;
    if (marker)
    {
      // Max 2 to only affect default game markers.
      for (var i = 0; i < marker.transform.childCount && i < 2; i++)
        marker.transform.GetChild(i).gameObject.SetActive(!Configuration.HidePlacementMarker);
    }
  }

  // Bit hacky, but some things reuse the highlight system and then HideSupportColor shouldn't apply.
  [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Highlight)), HarmonyPrefix]
  static bool HideSupportColor() => !Configuration.HideSupportColor || HammerMark.MarkedPieces.Count > 0 || Ruler.IsActive;

  [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateCrosshair)), HarmonyPostfix]
  static void HidePieceHealth(Hud __instance)
  {
    if (Configuration.HidePieceHealth)
      __instance.m_pieceHealthRoot.gameObject.SetActive(false);
  }
}