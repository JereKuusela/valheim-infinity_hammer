using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace InfinityHammer;

// These materials belong only to a live placement ghost. The inactive selection
// template and the placed world objects keep their own, untouched materials.
public sealed class PreviewMaterials : MonoBehaviour
{
  private readonly HashSet<Material> owned = [];

  public static void Prepare(GameObject obj)
  {
    // Include inactive children: a later preview clone may activate them.
    foreach (var component in obj.GetComponentsInChildren<RandomMaterialValues>(true))
    {
      component.CancelInvoke();
      component.enabled = false;
    }
  }

  public void OnDestroy()
  {
    foreach (var material in owned)
      if (material) UnityEngine.Object.Destroy(material);
    owned.Clear();
  }

  public static void CleanupMeshMaterials(Player player, GameObject ghost) => Cleanup<MeshRenderer>(player, ghost);
  public static void CleanupSkinnedMaterials(Player player, GameObject ghost) => Cleanup<SkinnedMeshRenderer>(player, ghost);

  private static void Cleanup<T>(Player player, GameObject ghost) where T : Renderer
  {
    if (!Configuration.Enabled || player != Player.m_localPlayer)
    {
      player.CleanupGhostMaterials<T>(ghost);
      return;
    }
    // The native cleanup replaces active renderers' shared-material arrays with
    // newly allocated materials. Compare before/after rather than destroying
    // every material found on a ghost (some may still be shared assets).
    var renderers = ghost.GetComponentsInChildren<T>();
    HashSet<Material> originals = [];
    foreach (var renderer in renderers)
      foreach (var material in renderer.sharedMaterials)
        if (material) originals.Add(material);
    var owner = ghost.GetComponent<PreviewMaterials>() ?? ghost.AddComponent<PreviewMaterials>();
    try
    {
      player.CleanupGhostMaterials<T>(ghost);
    }
    finally
    {
      foreach (var renderer in renderers)
      {
        if (!renderer) continue;
        foreach (var material in renderer.sharedMaterials)
          if (material && !originals.Contains(material)) owner.owned.Add(material);
      }
    }
  }
}

// Wrap the two concrete calls, avoiding patches to shared generic method bodies.
[HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
public static class TrackPreviewMaterials
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    int mesh = 0, skinned = 0;
    var native = AccessTools.Method(typeof(Player), nameof(Player.CleanupGhostMaterials));
    var meshMethod = native.MakeGenericMethod(typeof(MeshRenderer));
    var skinnedMethod = native.MakeGenericMethod(typeof(SkinnedMeshRenderer));
    foreach (var instruction in instructions)
    {
      if (instruction.Calls(meshMethod))
      {
        instruction.opcode = OpCodes.Call;
        instruction.operand = AccessTools.Method(typeof(PreviewMaterials), nameof(PreviewMaterials.CleanupMeshMaterials));
        mesh++;
      }
      else if (instruction.Calls(skinnedMethod))
      {
        instruction.opcode = OpCodes.Call;
        instruction.operand = AccessTools.Method(typeof(PreviewMaterials), nameof(PreviewMaterials.CleanupSkinnedMaterials));
        skinned++;
      }
      yield return instruction;
    }
    if (mesh != 1 || skinned != 1)
      throw new InvalidOperationException("Infinity Hammer: expected both native ghost material cleanup calls.");
  }
}
