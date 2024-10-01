using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;
using WorldEditCommands;
// Code related to removing objects.
namespace InfinityHammer;

[HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
public class UnlockRemoveDistance
{
  public static void Prefix(Player __instance, ref float __state)
  {
    __state = __instance.m_maxPlaceDistance;
    if (Configuration.Range > 0f)
      __instance.m_maxPlaceDistance = Configuration.Range;
  }
  public static void Postfix(Player __instance, float __state)
  {
    __instance.m_maxPlaceDistance = __state;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
public class RemovePiece
{
  public static bool Prefix(Player __instance, ref bool __result)
  {
    UndoHelper.BeginAction();
    HideEffects.Active = true;
    PreventPieceDrops.Active = Configuration.DisableLoot;
    PreventCreaturerops.Active = Configuration.DisableLoot;
    if (Configuration.RemoveAnything)
    {
      __result = RemoveAnything(__instance);
      return false;
    }
    return true;
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(
        useEnd: false,
        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZNetScene), "get_instance")))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemovePiece), nameof(HandleRemoved))))
      .InstructionEnumeration();
  }
  static void Finalizer()
  {
    UndoHelper.EndAction();
    HideEffects.Active = false;
    PreventPieceDrops.Active = false;
    PreventCreaturerops.Active = false;
  }
  private static void HandleRemoved(Piece piece)
  {
    var zdo = piece.m_nview.GetZDO();
    UndoHelper.AddRemoveAction(zdo);
    RemoveInArea(zdo, Configuration.RemoveArea);
  }

  private static bool RemoveAnything(Player obj)
  {
    var hovered = Selector.GetHovered(obj, obj.m_maxPlaceDistance, [], Configuration.RemoveIds);
    if (hovered == null) return false;
    obj.m_removeEffects.Create(hovered.Obj.transform.position, Quaternion.identity, null, 1f, -1);
    if (Remove(hovered))
      RemoveInArea(hovered.Obj.GetZDO(), Configuration.RemoveArea);
    var tool = obj.GetRightItem();
    if (tool != null)
    {
      obj.FaceLookDirection();
      obj.m_zanim.SetTrigger(tool.m_shared.m_attack.m_attackAnimation);
    }
    return true;
  }
  private static bool Remove(Hovered hovered)
  {
    var obj = hovered.Obj;
    if (obj.GetComponent<DungeonGenerator>() && hovered.Index > -1)
    {
      if (obj.transform.childCount > 1)
        UndoHelper.AddEditAction(obj.GetZDO());
      else
        UndoHelper.AddRemoveAction(obj.GetZDO());
      DungeonRooms.RemoveRoom(obj, hovered.Index);
      if (obj.transform.childCount == 0)
      {
        HammerHelper.RemoveZDO(obj.GetZDO());
        return true;
      }
      return false;
    }
    Remove(obj);
    return true;
  }
  private static void Remove(ZNetView obj)
  {
    obj.GetComponent<CharacterDrop>()?.OnDeath();
    obj.GetComponent<Piece>()?.DropResources();
    UndoHelper.AddRemoveAction(obj.GetZDO());
    HammerHelper.RemoveZDO(obj.GetZDO());
  }

  private static void RemoveInArea(ZDO zdo, float radius)
  {
    if (radius == 0f) return;
    var position = zdo.GetPosition();
    var prefab = zdo.m_prefab;
    var toRemove = ZNetScene.instance.m_instances.Values.Where(view =>
      view
      && view.IsValid()
      && view.GetZDO().m_prefab == prefab
      && Vector3.Distance(position, view.GetZDO().m_position) < radius
      && view.GetZDO() != zdo
    ).ToArray();
    foreach (var obj in toRemove)
      Remove(obj);
  }
}

[HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
public class PreventPieceDrops
{
  public static bool Active = false;
  static bool Prefix() => !Active;
}
[HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.OnDeath))]
public class PreventCreaturerops
{
  public static bool Active = false;
  static bool Prefix() => !Active;
}
