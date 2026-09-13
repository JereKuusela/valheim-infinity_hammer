using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace InfinityHammer;

// Caches preview materials created for placement ghosts to eliminate native memory leaks.
[HarmonyPatch]
public static class TrackPreviewMaterials
{
  private static readonly Dictionary<Material, Material> Cache = [];

  static IEnumerable<MethodBase> TargetMethods()
  {
    var method = AccessTools.Method(typeof(Player), nameof(Player.CleanupGhostMaterials));
    yield return method.MakeGenericMethod(typeof(MeshRenderer));
    yield return method.MakeGenericMethod(typeof(SkinnedMeshRenderer));
  }

  static bool Prefix(Player __instance, GameObject ghost, MethodBase __originalMethod)
  {
    if (!ghost) return false;

    var rendererType = __originalMethod.GetGenericArguments()[0];
    var renderers = ghost.GetComponentsInChildren(rendererType);

    foreach (Renderer renderer in renderers)
    {
      if (!renderer || renderer.sharedMaterial == null) continue;

      var sharedMaterials = renderer.sharedMaterials;

      for (int j = 0; j < sharedMaterials.Length; j++)
      {
        var original = sharedMaterials[j];
        if (!original) continue;

        if (!Cache.TryGetValue(original, out var cleaned) || !cleaned)
        {
          cleaned = new Material(original);
          if (cleaned.HasProperty("_RippleDistance"))
          {
            __instance.m_ghostRippleDistance[cleaned] = cleaned.GetFloat("_RippleDistance");
          }
          cleaned.SetFloat("_ValueNoise", 0f);
          cleaned.SetFloat("_TriplanarLocalPos", 1f);
          Cache[original] = cleaned;
        }
        else if (cleaned.HasProperty("_RippleDistance") && !__instance.m_ghostRippleDistance.ContainsKey(cleaned))
        {
          __instance.m_ghostRippleDistance[cleaned] = cleaned.GetFloat("_RippleDistance");
        }

        sharedMaterials[j] = cleaned;
      }
      if (sharedMaterials.Length > 0)
        renderer.sharedMaterials = sharedMaterials;
      renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    return false;
  }
}
