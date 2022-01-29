using HarmonyLib;
using Service;
using UnityEngine;

// Code related to removing objects.
namespace InfinityHammer {

  [HarmonyPatch(typeof(Player), "RemovePiece")]
  public class UnlockRemoveDistance {
    public static void Prefix(Player __instance, ref float __state) {
      __state = __instance.m_maxPlaceDistance;
      if (Settings.RemoveRange > 0f)
        __instance.m_maxPlaceDistance = Settings.RemoveRange;
    }
    public static void Postfix(Player __instance, float __state) {
      __instance.m_maxPlaceDistance = __state;
    }
  }

  [HarmonyPatch(typeof(Player), "RemovePiece")]
  public class RemovePiece {
    public static bool Removing = false;
    public static UndoData Target;

    public static void SetTarget(ZNetView obj) {
      Target = UndoHelper.CreateDate(obj);
    }

    private static bool RemoveAnything(Player obj) {
      var hovered = Helper.GetHovered(obj, obj.m_maxPlaceDistance);
      if (hovered == null) return false;
      obj.m_removeEffects.Create(hovered.Obj.transform.position, Quaternion.identity, null, 1f, -1);
      SetTarget(hovered.Obj);
      hovered.Obj.GetComponent<CharacterDrop>()?.OnDeath();
      hovered.Obj.GetComponent<Piece>()?.DropResources();
      Helper.RemoveZDO(hovered.Obj.GetZDO());
      var tool = obj.GetRightItem();
      if (tool != null) {
        obj.FaceLookDirection();
        obj.m_zanim.SetTrigger(tool.m_shared.m_attack.m_attackAnimation);
      }
      return true;
    }
    private static void End(bool result) {
      DisableEffects.Active = false;
      if (result && Target != null && Settings.EnableUndo)
        UndoManager.Add(new UndoRemove(Target));
      Removing = false;
      Target = null;
      PreventPieceDrops.Active = false;
      PreventCreaturerops.Active = false;
    }
    public static bool Prefix(Player __instance, ref bool __result) {
      DisableEffects.Active = true;
      Removing = true;
      PreventPieceDrops.Active = Settings.DisableLoot;
      PreventCreaturerops.Active = Settings.DisableLoot;
      if (Settings.RemoveAnything) {
        __result = RemoveAnything(__instance);
        End(__result);
        return false;
      }
      return true;
    }
    public static void Postfix(ref bool __result) => End(__result);
  }

  [HarmonyPatch(typeof(Piece), "DropResources")]
  public class PreventPieceDrops {
    public static bool Active = false;
    public static bool Prefix() => !Active;
  }
  [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
  public class PreventCreaturerops {
    public static bool Active = false;
    public static bool Prefix() => !Active;
  }
  [HarmonyPatch(typeof(Player), "RemovePiece")]
  public class PostProcessToolOnRemove {
    public static void Postfix(Player __instance, ref bool __result) {
      if (__result) Hammer.PostProcessTool(__instance);
    }
  }

  ///<summary>Game code doesn't give direct access to the removed object.</summary>
  [HarmonyPatch(typeof(Piece), "CanBeRemoved")]
  public class AccessTargetedObject {
    public static void Prefix(Piece __instance) {
      if (RemovePiece.Removing)
        RemovePiece.SetTarget(__instance.m_nview);
    }
  }
}