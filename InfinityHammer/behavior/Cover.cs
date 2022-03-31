
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace InfinityHammer;
public static class CoverCheck {
  public static Vector3 GetCoverPoint(CraftingStation obj) => obj.m_roofCheckPoint.position;
  public static Vector3 GetCoverPoint(Fermenter obj) => obj.m_roofCheckPoint.position;
  public static Vector3 GetCoverPoint(Beehive obj) => obj.m_coverPoint.position;
  public static Vector3 GetCoverPoint(Fireplace obj) => obj.transform.position + Vector3.up * obj.m_coverCheckOffset;
  public static Vector3 GetCoverPoint(Bed obj) => obj.GetSpawnPoint();
  public static Vector3 GetCoverPoint(Windmill obj) => obj.m_propeller.transform.position;
  public static bool ChecksCover(Fireplace obj) => obj.m_enabledObjectLow != null && obj.m_enabledObjectHigh != null;
  public const float CoverRayCastLength = 30f;
  public const float CoverRaycastStart = 0.5f;
  public const float CoverBedLimit = 0.8f;
  public const float CoverCraftingStationLimit = 0.7f;
  public const float CoverFermenterLimit = 0.7f;
  public const float CoverFireplaceLimit = 0.7f;
  public const string FORMAT = "0.##";
  public static string String(string value, string color = "white") => "<color=" + color + ">" + value + "</color>";
  public static string Percent(double value, string color = "white") => String(value.ToString("P0"), color);
  public static string JoinLines(IEnumerable<string> lines) => string.Join(". ", lines.Where(line => line != ""));
  public static string CurrentCover = "";

  public static void CheckCover(GameObject obj) {
    CurrentCover = "";
    GetCover(obj.GetComponent<Beehive>());
    GetCover(obj.GetComponent<Fermenter>());
    GetCover(obj.GetComponent<Fireplace>());
    GetCover(obj.GetComponent<Bed>());
    GetCover(obj.GetComponent<CraftingStation>());
    GetCover(obj.GetComponent<Windmill>());
  }

  public static void GetCover(Beehive obj) {
    if (obj) GetCover(GetCoverPoint(obj), obj.m_maxCover, false, false);
  }
  public static void GetCover(Fermenter obj) {
    if (obj) GetCover(GetCoverPoint(obj), CoverFermenterLimit);
  }
  public static void GetCover(Fireplace obj) {
    if (obj) GetCover(GetCoverPoint(obj), CoverFireplaceLimit, false);
  }
  public static void GetCover(Bed obj) {
    if (obj) GetCover(GetCoverPoint(obj), CoverBedLimit);
  }
  public static void GetCover(CraftingStation obj) {
    if (obj) GetCover(GetCoverPoint(obj), CoverCraftingStationLimit);
  }
  public static void GetCover(Windmill obj) {
    if (obj) GetCover(GetCoverPoint(obj), 0, false);
  }
  public static void GetCover(Vector3 position, float limit, bool checkRoof = true, bool minLimit = true) {
    var lines = new List<string>();
    Cover.GetCoverForPoint(position, out var percent, out var roof);
    var text = $"{Percent(percent)} cover";
    if (limit > 0) {
      var pastLimit = minLimit ? percent < limit : percent > limit;
      text += " (" + Percent(limit, pastLimit ? "yellow" : "white") + ")";
    }
    lines.Add(text);
    if (checkRoof && !roof)
      lines.Add(String("Missing roof", "yellow"));
    CurrentCover = JoinLines(lines);
  }
}
/*[HarmonyPatch(typeof(Hud), nameof(Hud.SetupPieceInfo))]
public class AddCoverText {
  public static void Postfix(Hud __instance, Piece piece) {
    if (!piece) return;
    if (!Player.m_localPlayer) return;
    if (!Player.m_localPlayer.m_placementGhost) return;
    CoverCheck.CheckCover(Player.m_localPlayer.m_placementGhost);
    __instance.m_pieceDescription.text += " " + CoverCheck.CurrentCover;
  }
}*/