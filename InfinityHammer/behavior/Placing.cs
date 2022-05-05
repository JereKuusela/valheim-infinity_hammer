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
    if (Hammer.GhostPrefab)
      __result = Hammer.GhostPrefab.GetComponent<Piece>();
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
  private static bool AddedPiece = false;
  private static int PreviousPieces = 0;
  public static void Prefix(ref Piece piece) {
    DisableEffects.Active = true;
    AddedPiece = false;
    PreviousPieces = Piece.m_allPieces.Count;
    if (!Hammer.GhostPrefab) return;
    var name = Utils.GetPrefabName(piece.gameObject);

    var basePrefab = ZNetScene.instance.GetPrefab(name);
    if (!basePrefab && ZoneSystem.instance.GetLocation(name) != null) {
      basePrefab = ZoneSystem.instance.m_locationProxyPrefab;
    }
    if (!basePrefab) {
      basePrefab = new GameObject();
      basePrefab.name = "__Ghost__";
    }
    var basePiece = basePrefab.GetComponent<Piece>();
    if (!basePiece) {
      AddedPiece = true;
      // Not all prefabs have the piece component. So add it temporarily.
      Helper.EnsurePiece(basePrefab);
      basePiece = basePrefab.GetComponent<Piece>();
    }
    // When copying, some objects like armor and item stands will have a different model depending on their items.
    // To avoid these model changes being copied, use the base prefab.
    piece = basePiece;
  }

  public static void Postfix(ref Piece piece, Player __instance, bool __result) {
    DisableEffects.Active = false;
    // Revert the adding of Piece component.
    if (AddedPiece) ObjectDB.Destroy(piece);
    // Restore the actual selection.
    if (Hammer.GhostPrefab)
      piece = Hammer.GhostPrefab.GetComponent<Piece>();
    if (__result && Piece.m_allPieces.Count > PreviousPieces) {
      var added = Piece.m_allPieces[Piece.m_allPieces.Count - 1];
      if (Utils.GetPrefabName(added.gameObject) == "__Ghost__") {
        if (!Hammer.GhostPrefab) return;
        UndoHelper.StartTracking();
        var ghost = __instance.m_placementGhost;
        for (var i = 0; i < ghost.transform.childCount; i++) {
          var obj = ghost.transform.GetChild(i).gameObject;
          var name = Utils.GetPrefabName(obj);
          var prefab = ZNetScene.instance.GetPrefab(name);
          if (prefab) {
            UnityEngine.Object.Instantiate(prefab, obj.transform.position, obj.transform.rotation);
          }
        }
        UndoHelper.StopTracking();
        UnityEngine.Object.Destroy(added);
        return;
      }
      // Hoe adds pieces too.
      if (!added.m_nview) return;
      if (added.GetComponent<LocationProxy>()) {
        UndoHelper.StartTracking();
        Hammer.SpawnLocation(added);
        UndoHelper.StopTracking();
      } else {
        Hammer.PostProcessPlaced(added);
        UndoHelper.CreateObject(added.gameObject);
      }
    }
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
    if (AllViews && Hammer.State != null) {
      var location = ZoneSystem.instance.GetLocation(Hammer.State.GetInt("location", 0));
      if (location != null) {
        foreach (var view in location.m_netViews)
          view.gameObject.SetActive(true);
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
