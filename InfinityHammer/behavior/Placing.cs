using HarmonyLib;
using Service;

// Code related to adding objects.
namespace InfinityHammer {

  public static class Placing {

    ///<summary>Removes placement checks.</summary>
    public static void ForceValidPlacement(Player obj) {
      if (obj.m_placementGhost == null) return;
      if (obj.m_placementStatus == Player.PlacementStatus.NotInDungeon) {
        if (!Settings.AllowInDungeons) return;
      } else if (obj.m_placementStatus == Player.PlacementStatus.NoBuildZone) {
        if (!Settings.IgnoreNoBuild) return;
      } else if (obj.m_placementStatus == Player.PlacementStatus.PrivateZone) {
        if (!Settings.IgnoreWards) return;
      } else if (!Settings.IgnoreOtherRestrictions) return;
      obj.m_placementStatus = Player.PlacementStatus.Valid;
      obj.SetPlacementGhostValid(true);
    }
  }

  ///<summary>Overrides the piece selection.</summary>
  [HarmonyPatch(typeof(PieceTable), "GetSelectedPiece")]
  public class GetSelectedPiece {
    public static bool Prefix(ref Piece __result) {
      __result = Hammer.GetPrefab()?.GetComponent<Piece>();
      if (__result) return false;
      return true;
    }
  }

  ///<summary>Selecting a piece normally removes the override.</summary>
  [HarmonyPatch(typeof(Player), "SetSelectedPiece")]
  public class SetSelectedPiece {
    public static void Prefix(Player __instance) {
      Hammer.RemoveSelection();
      __instance.SetupPlacementGhost();
    }
  }

  [HarmonyPatch(typeof(Player), "PlacePiece")]
  public class PlacePiece {
    public static void Prefix() {
      DisableEffects.Active = true;
    }
    public static void Postfix(bool __result) {
      DisableEffects.Active = false;
      if (__result && Piece.m_allPieces.Count > 0) {
        var added = Piece.m_allPieces[Piece.m_allPieces.Count - 1];
        Hammer.PostProcessPlaced(added);
        if (Settings.EnableUndo)
          UndoManager.Add(new UndoPlace(added.m_nview));
      }
    }
  }
  [HarmonyPatch(typeof(Player), "PlacePiece")]
  public class PostProcessToolOnPlace {
    public static void Postfix(Player __instance, ref bool __result) {
      if (__result) Hammer.PostProcessTool(__instance);
    }
  }

  [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
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

  [HarmonyPatch(typeof(Player), "SetupPlacementGhost")]
  public class SetupPlacementGhost {
    public static void Prefix() {
      Hammer.UseSelectedObject = true;
    }
    public static void Postfix(Player __instance) {
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
  [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
  public class UpdatePlacementGhost {
    public static void Postfix() => Scaling.UpdatePlacementScale();
  }
}