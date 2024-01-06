using System;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;
public class HammerMeasureCommand
{
  private static readonly int[] IgnoredLayers = [LayerMask.NameToLayer("character_trigger"), LayerMask.NameToLayer("viewblock"), LayerMask.NameToLayer("pathblocker")];
  public static void CheckWithCollider(GameObject obj)
  {
    if (obj.transform.rotation != Quaternion.identity) return;
    var name = Utils.GetPrefabName(obj);
    if (Configuration.Dimensions.ContainsKey(name.ToLower())) return;
    if (!ZNetScene.instance.GetPrefab(name)) return;
    var colliders = obj.GetComponentsInChildren<Collider>().Where(c => !IgnoredLayers.Contains(c.gameObject.layer)).ToArray();
    if (colliders.Length == 0) return;
    var bounds = colliders[0].bounds;
    foreach (var c in colliders)
      bounds.Encapsulate(c.bounds);
    Configuration.SetDimension(name, bounds.size);
  }
  public HammerMeasureCommand()
  {
    AutoComplete.RegisterEmpty("hammer_measure");
    Helper.Command("hammer_measure", "Tries to measure all structures.", (args) =>
    {
      HammerHelper.CheatCheck();
      ZNetView.m_forceDisableInit = true;
      foreach (var prefab in ZNetScene.instance.m_prefabs)
      {
        if (prefab.name == "Player") continue;
        if (prefab.name.StartsWith("_", StringComparison.Ordinal)) continue;
        if (prefab.name.StartsWith("fx_", StringComparison.Ordinal)) continue;
        if (prefab.name.StartsWith("sfx_", StringComparison.Ordinal)) continue;
        if (prefab.GetComponentInChildren<Ragdoll>()) continue;
        if (prefab.GetComponentInChildren<ItemDrop>()) continue;
        if (!prefab.GetComponentInChildren<Collider>()) continue;
        var obj = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        CheckWithCollider(obj);
        UnityEngine.Object.Destroy(obj);
      }
      ZNetView.m_forceDisableInit = false;
      HammerHelper.Message(args.Context, "Objects measured.");
    });
  }
}
