using HarmonyLib;
using Service;

// Code related to adding objects.
namespace InfinityHammer {

  ///<summary>Overrides the piece selection.</summary>
  [HarmonyPatch(typeof(PieceTable), "GetSelectedPiece")]
  public class GetSelectedPiece {
    public static bool Prefix(ref Piece __result) => !Hammer.ReplacePiece(ref __result);
  }

  ///<summary>Selecting a piece normally removes the override.</summary>
  [HarmonyPatch(typeof(Player), "SetSelectedPiece")]
  public class SetSelectedPiece {
    public static void Prefix(Player __instance) => Hammer.Remove(__instance);
  }

  [HarmonyPatch(typeof(Player), "PlacePiece")]
  public class PlacePiece {
    public static void Prefix(ref Piece piece) => Hammer.CleanPlacePrefab(ref piece);
    public static void Postfix(bool __result) {
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
    public static void Postfix(Player __instance) => Hammer.PostProcessPlacementGhost(__instance.m_placementGhost);
  }
}