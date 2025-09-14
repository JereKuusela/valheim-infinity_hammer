using System.Collections.Generic;
using System.Reflection.Emit;
using Data;
using HarmonyLib;
using UnityEngine;
// Code related to adding objects.
namespace InfinityHammer;

[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PlacePiece
{
  static void Prefix()
  {
    HideEffects.Active = true;
  }
  static void Finalizer()
  {
    HideEffects.Active = false;
    DataHelper.CleanUp();
    if (Selection.Get().SingleUse)
      Hammer.SelectRepair();
  }
  // Parameter is the selected piece which doesn't have the correct transformation.
  static GameObject GetPrefab(GameObject obj) => Configuration.Enabled ? Selection.Get().GetPrefab(obj) : obj;

  static void Postprocess(GameObject obj)
  {
    if (!Configuration.Enabled) return;
    Selection.Get().AfterPlace(obj);
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Callvirt))
          .Advance(1)
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(GetPrefab).operand))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ret))
          .Insert(new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(Postprocess).operand))
          .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
public class HoldUse
{

  static void Prefix(Player __instance, ref ItemDrop.ItemData __state)
  {
    __state = __instance.GetRightItem();
    Hammer.RemoveToolCosts(__state);
    Hammer.AddRemoveAnything(__state);
  }
  static Transform RemoveFailSafe(Piece piece) => piece == null ? Player.m_localPlayer.transform : piece.transform;
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Ldfld,
                  AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), nameof(ItemDrop.ItemData.SharedData.m_destroyEffect))))
          .Advance(2)
          .Set(OpCodes.Call, AccessTools.Method(typeof(HoldUse), nameof(RemoveFailSafe)))
          .InstructionEnumeration();
  }
  static void Postfix(Player __instance)
  {
    if (!Selection.Get().Continuous) return;
    if (__instance.m_placePressedTime != -9999f) return;
    if (!ZInput.GetButton("Attack") && !ZInput.GetButton("JoyPlace"))
    {
      // Just something else to not trigger anymore.
      __instance.m_placePressedTime = -9998f;
      return;
    }
    if (Time.time - __instance.m_lastToolUseTime < __instance.m_placeDelay / 4f) return;
    __instance.m_placePressedTime = Time.time;
    __instance.m_lastToolUseTime = 0f;

  }
  static void Finalizer(ItemDrop.ItemData __state)
  {
    Hammer.RestoreToolCosts(__state);
    Hammer.RestoreRemoveAnything(__state);
  }
}


[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class UnlockBuildDistance
{
  public static void Prefix(Player __instance, ref float __state)
  {
    __state = __instance.m_maxPlaceDistance;
    var selection = Selection.Get();
    if (selection != null)
      __instance.m_maxPlaceDistance = selection.MaxPlaceDistance(__instance.m_maxPlaceDistance);
  }
  public static void Postfix(Player __instance, float __state)
  {
    __instance.m_maxPlaceDistance = __state;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public class CustomizeSpawnLocation
{
  public static bool? RandomDamage = null;
  public static bool AllViews = false;
  static void Customize(ZNetView[] views)
  {
    if (RandomDamage.HasValue)
    {
      WearNTear.m_randomInitialDamage = RandomDamage.Value;
    }
    if (AllViews)
    {
      foreach (var view in views)
        view.gameObject.SetActive(true);
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: true,
              new CodeMatch(
                  OpCodes.Stsfld,
                  AccessTools.Field(typeof(WearNTear), nameof(WearNTear.m_randomInitialDamage))))
          .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
          .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(Customize).operand))
          .InstructionEnumeration();
  }
}
