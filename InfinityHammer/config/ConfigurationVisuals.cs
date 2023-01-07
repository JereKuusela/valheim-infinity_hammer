using BepInEx.Configuration;
using HarmonyLib;
using Service;

namespace InfinityHammer;
public partial class Configuration
{

#nullable disable
  // Visuals.
  public static ConfigEntry<bool> configRemoveEffects;
  public static bool RemoveEffects => configRemoveEffects.Value && Enabled;
  public static ConfigEntry<bool> configHidePlacementMarker;
  public static bool HidePlacementMarker => configHidePlacementMarker.Value && Enabled;
  public static ConfigEntry<bool> configHideSupportColor;
  public static bool HideSupportColor => configHideSupportColor.Value && Enabled;
#nullable enable
  public static void InitVisuals(ConfigWrapper wrapper)
  {
    Wrapper = wrapper;
    var section = "3. Visual";
    configRemoveEffects = wrapper.Bind(section, "Remove effects", false, "Removes visual effects of building, etc.");
    configHidePlacementMarker = wrapper.Bind(section, "No placement marker", false, "Hides the yellow placement marker (also affects Gizmo mod).");
    configHideSupportColor = wrapper.Bind(section, "No support color", false, "Hides the color that shows support.");
  }
}

[HarmonyPatch(typeof(EffectList), nameof(EffectList.Create))]
public class RemoveEffects
{
  public static bool Active = false;
  static bool Prefix() => !Active || !Configuration.RemoveEffects;
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
  static bool Prefix() => !Configuration.HideSupportColor;
}