using BepInEx.Configuration;
using HarmonyLib;
using InfinityTools;
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

[HarmonyPatch(typeof(EffectList), nameof(EffectList.Create))]
public class HideEffects
{
  public static bool Active = false;
  static bool Prefix() => !Active || !Configuration.HideEffects;
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class HidePlacementMarker
{
  static void Postfix(Player __instance)
  {
    var marker = __instance.m_placementMarkerInstance;
    if (marker)
    {
      // Max 2 to only affect default game markers.
      for (var i = 0; i < marker.transform.childCount && i < 2; i++)
        marker.transform.GetChild(i).gameObject.SetActive(!Configuration.HidePlacementMarker);
    }
  }
}

[HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Highlight))]
public class HideSupportColor
{
  // Bit hacky, but some things reuse the highlight system and then HideSupportColor shouldn't apply.
  static bool Prefix() => !Configuration.HideSupportColor || HammerMark.MarkedPieces.Count > 0 || Ruler.IsActive;
}
[HarmonyPatch(typeof(Hud), nameof(Hud.UpdateCrosshair))]
public class HidePieceHealth
{
  static void Postfix(Hud __instance)
  {
    if (Configuration.HidePieceHealth)
      __instance.m_pieceHealthRoot.gameObject.SetActive(false);
  }
}