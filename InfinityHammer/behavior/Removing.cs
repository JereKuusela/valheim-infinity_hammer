using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
// Code related to removing objects.
namespace InfinityHammer;

[HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
public class UnlockRemoveDistance {
  public static void Prefix(Player __instance, ref float __state) {
    __state = __instance.m_maxPlaceDistance;
    if (Configuration.RemoveRange > 0f)
      __instance.m_maxPlaceDistance = Configuration.RemoveRange;
  }
  public static void Postfix(Player __instance, float __state) {
    __instance.m_maxPlaceDistance = __state;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
public class RemovePiece {
  public static bool Removing = false;
  public static List<ZDO> RemovedObjects = new();

  public static void SetRemovedObject(ZNetView obj) {
    RemovedObjects = new() { obj.GetZDO() };
  }
  public static void AddRemovedObject(ZNetView obj) {
    RemovedObjects.Add(obj.GetZDO());
  }

  private static bool RemoveAnything(Player obj) {
    var hovered = Selector.GetHovered(obj, obj.m_maxPlaceDistance, Configuration.RemoveBlacklist);
    if (hovered == null) return false;
    obj.m_removeEffects.Create(hovered.Obj.transform.position, Quaternion.identity, null, 1f, -1);
    SetRemovedObject(hovered.Obj);
    Remove(hovered.Obj);
    var tool = obj.GetRightItem();
    if (tool != null) {
      obj.FaceLookDirection();
      obj.m_zanim.SetTrigger(tool.m_shared.m_attack.m_attackAnimation);
    }
    return true;
  }
  private static void Remove(ZNetView obj) {
    obj.GetComponent<CharacterDrop>()?.OnDeath();
    obj.GetComponent<Piece>()?.DropResources();
    Helper.RemoveZDO(obj.GetZDO());
  }
  private static void RemoveInArea(ZDO zdo, float radius) {
    if (radius == 0f) return;
    var position = zdo.m_position;
    var prefab = zdo.m_prefab;
    var toRemove = ZNetScene.instance.m_instances.Values.Where(view =>
      view
      && view.IsValid()
      && view.GetZDO().m_prefab == prefab
      && Vector3.Distance(position, view.GetZDO().m_position) < radius
      && view.GetZDO() != zdo
    ).ToArray();
    foreach (var obj in toRemove) {
      AddRemovedObject(obj);
      Remove(obj);
    }
  }
  public static bool Prefix(Player __instance, ref bool __result) {
    DisableEffects.Active = true;
    Removing = true;
    PreventPieceDrops.Active = Configuration.DisableLoot;
    PreventCreaturerops.Active = Configuration.DisableLoot;
    if (Configuration.RemoveAnything) {
      __result = RemoveAnything(__instance);
      return false;
    }
    return true;
  }
  static void Finalizer() {
    if (RemovedObjects.Count > 0) {
      RemoveInArea(RemovedObjects[0], Configuration.RemoveArea);
      UndoWrapper.Remove(RemovedObjects);
    }
    RemovedObjects.Clear();
    DisableEffects.Active = false;
    PreventPieceDrops.Active = false;
    PreventCreaturerops.Active = false;
    Removing = false;
  }
}

[HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
public class PreventPieceDrops {
  public static bool Active = false;
  static bool Prefix() => !Active;
}
[HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.OnDeath))]
public class PreventCreaturerops {
  public static bool Active = false;
  static bool Prefix() => !Active;
}
[HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
public class PostProcessToolOnRemove {
  public static void Postfix(Player __instance, ref bool __result) {
    if (__result) Hammer.PostProcessTool(__instance);
  }
}

///<summary>Game code doesn't give direct access to the removed object.</summary>
[HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
public class AccessTargetedObject {
  public static void Prefix(Piece __instance) {
    if (RemovePiece.Removing && Selector.IsValid(__instance.m_nview))
      RemovePiece.SetRemovedObject(__instance.m_nview);
  }
}
