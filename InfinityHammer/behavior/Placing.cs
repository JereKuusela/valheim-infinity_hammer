using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
// Code related to adding objects.
namespace InfinityHammer;
///<summary>Overrides the piece selection.</summary>
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPiece))]
public class GetSelectedPiece {
  public static bool Prefix(ref Piece __result) {
    if (Selection.Ghost)
      __result = Selection.Ghost.GetComponent<Piece>();
    if (__result) return false;
    return true;
  }
}

///<summary>Selecting a piece normally removes the override.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece))]
public class SetSelectedPiece {
  public static void Prefix(Player __instance) {
    Hammer.RemoveSelection();
    __instance.SetupPlacementGhost();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PlacePiece {
  static GameObject GetPrefab(Piece obj) {
    var type = Selection.Type;
    if (type == SelectionType.Default) return obj.gameObject;
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return obj.gameObject;
    var name = Utils.GetPrefabName(ghost);

    if (type == SelectionType.Object)
      return ZNetScene.instance.GetPrefab(name);
    if (type == SelectionType.Location)
      return ZoneSystem.instance.m_locationProxyPrefab;
    if (type == SelectionType.Multiple) {
      var dummy = new GameObject();
      dummy.name = "Blueprint";
      return dummy;
    }
    return obj.gameObject;
  }
  static void Postprocess(GameObject obj) {
    Helper.EnsurePiece(obj);
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return;
    if (Selection.Type == SelectionType.Multiple) {
      UndoHelper.StartTracking();
      for (var i = 0; i < ghost.transform.childCount; i++) {
        var ghostObj = ghost.transform.GetChild(i).gameObject;
        var name = Utils.GetPrefabName(ghostObj);
        var prefab = ZNetScene.instance.GetPrefab(name);
        if (prefab) {
          var childObj = UnityEngine.Object.Instantiate(prefab, ghostObj.transform.position, ghostObj.transform.rotation);
          var childView = childObj.GetComponent<ZNetView>();
          Hammer.CopyState(childView, i);
          Hammer.PostProcessPlaced(childObj);
          Scaling.SetPieceScale(childView, ghostObj);
        }
      }
      UndoHelper.StopTracking();
      UnityEngine.Object.Destroy(obj);
      return;
    }
    var view = obj.GetComponent<ZNetView>();
    // Hoe adds pieces too.
    if (!view) return;
    var piece = obj.GetComponent<Piece>();
    if (Selection.Type == SelectionType.Location && obj.GetComponent<LocationProxy>()) {
      UndoHelper.StartTracking();
      Hammer.SpawnLocation(view);
      UndoHelper.StopTracking();
    } else {
      Hammer.CopyState(view);
      Hammer.PostProcessPlaced(piece.gameObject);
      Scaling.SetPieceScale(view, ghost);
      UndoHelper.CreateObject(piece.gameObject);
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Callvirt,
                  AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject))))
          .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate<Func<Piece, GameObject>>(GetPrefab).operand)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_1),
              new CodeMatch(OpCodes.Ret))
          .Advance(-1)
          .Insert(new CodeInstruction(OpCodes.Ldloc_3),
            new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate<Action<GameObject>>(Postprocess).operand))
          .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PostProcessToolOnPlace {
  public static void Postfix(Player __instance, ref bool __result) {
    if (__result) Hammer.PostProcessTool(__instance);
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class UnlockBuildDistance {
  public static void Prefix(Player __instance, ref float __state) {
    __state = __instance.m_maxPlaceDistance;
    if (Settings.BuildRange > 0f)
      __instance.m_maxPlaceDistance = Settings.BuildRange;
  }
  public static void Postfix(Player __instance, float __state) {
    __instance.m_maxPlaceDistance = __state;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
public class SetupPlacementGhost {
  public static void Postfix(Player __instance) {
    if (!__instance.m_placementGhost) return;
    // Ensures that the scale is reseted when selecting objects from the build menu.
    Scaling.SetScale(__instance.m_placementGhost.transform.localScale);
    Helper.CleanObject(__instance.m_placementGhost);
    // When copying an existing object, the copy is inactive.
    // So the ghost must be manually activated while disabling ZNet stuff.
    if (__instance.m_placementGhost && !__instance.m_placementGhost.activeSelf) {
      ZNetView.m_forceDisableInit = true;
      __instance.m_placementGhost.SetActive(true);
      ZNetView.m_forceDisableInit = false;
    }
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class UpdatePlacementGhost {
  static void Postfix(Player __instance) {
    Scaling.UpdatePlacement();
    var marker = __instance.m_placementMarkerInstance;
    if (marker) {
      // Max 2 to only affect default game markers.
      for (var i = 0; i < marker.transform.childCount && i < 2; i++)
        marker.transform.GetChild(i).gameObject.SetActive(!Settings.HidePlacementMarker);
    }
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public class CustomizeSpawnLocation {
  public static bool? RandomDamage = null;
  public static bool AllViews = false;
  static void Customize() {
    if (RandomDamage.HasValue) {
      WearNTear.m_randomInitialDamage = RandomDamage.Value;
    }
    if (AllViews) {
      var data = Selection.GetData();
      if (data != null) {
        var location = ZoneSystem.instance.GetLocation(data.GetInt("location", 0));
        if (location != null) {
          foreach (var view in location.m_netViews)
            view.gameObject.SetActive(true);
        }

      }
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
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
