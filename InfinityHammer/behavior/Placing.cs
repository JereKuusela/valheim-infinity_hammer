using System.Collections.Generic;
using System.Reflection.Emit;
using Data;
using HarmonyLib;
using UnityEngine;
// Code related to adding objects.
namespace InfinityHammer;

[HarmonyPatch]
public static class Placing
{
  [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece)), HarmonyPrefix]
  static void PlacePiecePrefix()
  {
    Visuals.HideEffectsActive = true;
  }
  [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece)), HarmonyFinalizer]
  static void PlacePieceFinalizer()
  {
    Visuals.HideEffectsActive = false;
    DataHelper.CleanUp();
    if (Selection.Get().SingleUse)
      Hammer.SelectRepair();
  }
  // Parameter is the selected piece which doesn't have the correct transformation.
  static GameObject GetPrefab(GameObject obj, Vector3 pos, Quaternion rot) => Configuration.Enabled ? Selection.Get().GetPrefab(obj, pos, rot) : obj;

  static void Postprocess(GameObject obj)
  {
    if (!Configuration.Enabled) return;
    Selection.Get().AfterPlace(obj);
  }

  // While PlacePiece has piece as parameter, most flexible to override the actual game object.
  [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> PlacePieceTranspiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject))))
      .Advance(1)
      .Insert(
        new CodeInstruction(OpCodes.Ldarg_2),
        new CodeInstruction(OpCodes.Ldarg_3),
        new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(GetPrefab).operand))
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_placeEffect))))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Placing), nameof(Postprocess))))
      .InstructionEnumeration();
  }

  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement)), HarmonyPrefix]
  static void HoldUsePrefix(Player __instance, ref ItemDrop.ItemData __state)
  {
    __state = __instance.GetRightItem();
    Hammer.RemoveToolCosts(__state);
    Hammer.AddRemoveAnything(__state);
  }
  static Transform RemoveFailSafe(Piece piece) => piece == null ? Player.m_localPlayer.transform : piece.transform;
  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> HoldUseTranspiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Ldfld,
                  AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), nameof(ItemDrop.ItemData.SharedData.m_destroyEffect))))
          .Advance(2)
          .Set(OpCodes.Call, AccessTools.Method(typeof(Placing), nameof(RemoveFailSafe)))
          .InstructionEnumeration();
  }
  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement)), HarmonyPostfix]
  static void HoldUsePostfix(Player __instance)
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
  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement)), HarmonyFinalizer]
  static void HoldUseFinalizer(ItemDrop.ItemData __state)
  {
    Hammer.RestoreToolCosts(__state);
    Hammer.RestoreRemoveAnything(__state);
  }

  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyPrefix]
  static void UnlockBuildDistancePrefix(Player __instance, ref float __state)
  {
    __state = __instance.m_maxPlaceDistance;
    var selection = Selection.Get();
    if (selection != null)
      __instance.m_maxPlaceDistance = selection.MaxPlaceDistance(__instance.m_maxPlaceDistance);
  }
  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyPostfix]
  static void UnlockBuildDistancePostfix(Player __instance, float __state)
  {
    __instance.m_maxPlaceDistance = __state;
  }

  public static bool? SpawnRandomDamage = null;
  public static bool SpawnAllViews = false;
  static void Customize(ZNetView[] views)
  {
    if (SpawnRandomDamage.HasValue)
    {
      WearNTear.m_randomInitialDamage = SpawnRandomDamage.Value;
    }
    if (SpawnAllViews)
    {
      foreach (var view in views)
        view.gameObject.SetActive(true);
    }
  }
  [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> SpawnLocationTranspiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Stsfld,
                  AccessTools.Field(typeof(WearNTear), nameof(WearNTear.m_randomInitialDamage))))
          .Advance(1)
          .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
          .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(Customize).operand))
          .InstructionEnumeration();
  }
}
