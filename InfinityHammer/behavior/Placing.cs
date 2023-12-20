using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;
// Code related to adding objects.
namespace InfinityHammer;


[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PlacePiece
{
  private static bool Clear = false;
  static void Prefix()
  {
    HideEffects.Active = true;
    Clear = Selection.Get().SingleUse;
  }
  static void Finalizer(bool __result)
  {
    HideEffects.Active = false;
    DataHelper.Clear();
    if (__result && Clear)
    {
      Selection.Clear();
      Hammer.Clear();
    }
  }
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
              new CodeMatch(OpCodes.Ldloc_2))
          .Advance(1)
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(GetPrefab).operand))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_1),
              new CodeMatch(OpCodes.Ret))
          .Advance(-1)
          .Insert(new CodeInstruction(OpCodes.Ldloc_3),
            new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(Postprocess).operand))
          .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
public class HoldUse
{
  // Needed to prevent instant usage when selecting continuous command.
  public static bool Selecting = true;
  static void CheckHold()
  {
    if (!ZInput.GetButton("Attack") && !ZInput.GetButton("JoyPlace"))
      Selecting = false;
    if (Selecting || !Selection.Get().Continuous) return;
    var player = Player.m_localPlayer;
    if (ZInput.GetButton("Attack") || ZInput.GetButton("JoyPlace"))
      player.m_placePressedTime = Time.time;
    if (Time.time - player.m_lastToolUseTime > player.m_placeDelay / 4f)
      player.m_lastToolUseTime = 0f;
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Ldstr,
                  "Attack"))
          .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(CheckHold).operand)
          .Insert(new CodeInstruction(OpCodes.Ldstr, "Attack"))
          .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PostProcessToolOnPlace
{
  static void Postfix(Player __instance, ref bool __result)
  {
    if (__result) Hammer.PostProcessTool(__instance);
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

[HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
public class SetupPlacementGhost
{
  public static void Postfix(Player __instance)
  {
    if (!__instance.m_placementGhost) return;
    // Ensures that the scale is reseted when selecting objects from the build menu.
    Scaling.Build.SetScale(__instance.m_placementGhost.transform.localScale);
    // When copying an existing object, the copy is inactive.
    // So the ghost must be manually activated while disabling ZNet stuff.
    if (__instance.m_placementGhost && !__instance.m_placementGhost.activeSelf)
    {
      ZNetView.m_forceDisableInit = true;
      __instance.m_placementGhost.SetActive(true);
      ZNetView.m_forceDisableInit = false;
    }
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public class CustomizeSpawnLocation
{
  public static bool? RandomDamage = null;
  public static bool AllViews = false;
  static void Customize()
  {
    if (RandomDamage.HasValue)
    {
      WearNTear.m_randomInitialDamage = RandomDamage.Value;
    }
    if (AllViews)
    {
      var data = Selection.Get().GetData();
      if (data != null)
      {
        var location = ZoneSystem.instance.GetLocation(data.GetInt(Hash.Location, 0));
        if (location != null)
        {
          foreach (var view in location.m_netViews)
            view.gameObject.SetActive(true);
        }

      }
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Stsfld,
                  AccessTools.Field(typeof(WearNTear), nameof(WearNTear.m_randomInitialDamage))))
          .Advance(1)
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate<Action>(Customize).operand))
          .InstructionEnumeration();
  }
}
