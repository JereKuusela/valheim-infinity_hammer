using HarmonyLib;

// Code related to adding objects.
namespace InfinityHammer {
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
    public static void Prefix(ref Piece piece, ref bool __state) {
      DisableEffects.Active = true;
      __state = false;
      if (!Hammer.GhostPrefab) return;
      var basePrefab = ZNetScene.instance.GetPrefab(Utils.GetPrefabName(piece.gameObject));
      var basePiece = basePrefab.GetComponent<Piece>();
      if (!basePiece) {
        // Not all prefabs have the piece component. So add it temporarily.
        __state = true;
        Helper.EnsurePiece(basePrefab);
        basePiece = basePrefab.GetComponent<Piece>();
      }
      // When copying, some objects like armor and item stands will have a different model depending on their items.
      // To avoid these model changes being copied, use the base prefab.
      piece = basePiece;
    }

    public static void Postfix(ref Piece piece, bool __state, bool __result) {
      DisableEffects.Active = false;
      // Revert the adding of Piece component.
      if (__state) ObjectDB.Destroy(piece);
      // Restore the actual selection.
      if (Hammer.GhostPrefab) piece = Hammer.GhostPrefab.GetComponent<Piece>();
      if (__result && Piece.m_allPieces.Count > 0) {
        var added = Piece.m_allPieces[Piece.m_allPieces.Count - 1];
        Hammer.PostProcessPlaced(added);
        UndoWrapper.Place(added.m_nview);
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
    public static void Postfix(Player __instance) {
      Scaling.UpdatePlacement();
      Offset.UpdatePlacement();
      var marker = __instance.m_placementMarkerInstance;
      if (marker) {
        // Max 2 to only affect default game markers.
        for (var i = 0; i < marker.transform.childCount && i < 2; i++)
          marker.transform.GetChild(i).gameObject.SetActive(!Settings.HidePlacementMarker);
      }
    }
  }
}