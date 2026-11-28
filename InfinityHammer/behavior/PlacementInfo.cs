using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;

[HarmonyPatch]
public static class PlacementInfo
{
  private static GameObject? Ghost;
  private static Transform? Prefab;
  private static List<Func<IEnumerable<string>>> Rules = [];
  private static Vector3 Position;
  private static Quaternion Rotation;
  private static Player.PlacementStatus Status;
  private static string Text = "";
  private static int SolidMask = 0;
  private static int RoofMask = 0;
  private static int SpaceMask = 0;
  private static readonly Collider[] Colliders = new Collider[128];

  [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateCrosshair)), HarmonyPostfix]
  static void UpdateCrosshair(Hud __instance, Player player)
  {
    if (!player.InPlaceMode() || Configuration.HidePlacementInfo || Hud.IsPieceSelectionVisible() || Selection.Get() is ToolSelection) return;
    var text = Get(player);
    if (text != "") __instance.m_hoverName.text = text;
  }

  private static string Get(Player player)
  {
    var ghost = player.m_placementGhost;
    if (!ghost) return "";
    if (ghost != Ghost)
    {
      Ghost = ghost;
      var prefab = ZNetScene.instance.GetPrefab(Utils.GetPrefabName(ghost)) ?? ghost;
      Prefab = prefab.transform;
      Rules = GetRules(prefab, ghost.transform);
      Position = Vector3.positiveInfinity;
      Text = "";
    }
    var tr = ghost!.transform;
    var status = player.m_placementStatus;
    if (tr.position == Position && tr.rotation == Rotation && status == Status) return Text;
    Position = tr.position;
    Rotation = tr.rotation;
    Status = status;
    var lines = Rules.SelectMany(rule => rule());
    var token = StatusToken(status);
    if (token != "") lines = lines.Prepend(Game(token));
    Text = string.Join("\n", lines);
    return Text;
  }

  private static string StatusToken(Player.PlacementStatus status) => status switch
  {
    Player.PlacementStatus.NoBuildZone => "$msg_nobuildzone",
    Player.PlacementStatus.BlockedbyPlayer => "$msg_blocked",
    Player.PlacementStatus.PrivateZone => "$msg_privatezone",
    Player.PlacementStatus.MoreSpace => "$msg_needspace",
    Player.PlacementStatus.NoTeleportArea => "$msg_noteleportarea",
    Player.PlacementStatus.Invalid => "$msg_invalidplacement",
    Player.PlacementStatus.NoRayHits => "$msg_invalidplacement",
    Player.PlacementStatus.ExtensionMissingStation => "$msg_extensionmissingstation",
    Player.PlacementStatus.WrongBiome => "$msg_wrongbiome",
    Player.PlacementStatus.NeedCultivated => "$msg_needcultivated",
    Player.PlacementStatus.NeedDirt => "$msg_needdirt",
    Player.PlacementStatus.NotInDungeon => "$msg_notindungeon",
    Player.PlacementStatus.DeepSnow => "$msg_snowtoodeep",
    Player.PlacementStatus.NoSnow => "$msg_nosnow",
    _ => "",
  };

  private static List<Func<IEnumerable<string>>> GetRules(GameObject prefab, Transform ghost)
  {
    List<Func<IEnumerable<string>>> rules = [];
    var piece = prefab.GetComponent<Piece>();
    var biomeChecked = piece && piece.m_onlyInBiome != Heightmap.Biome.None;
    if (prefab.TryGetComponent(out Beehive beehive)) rules.Add(() => Beehive(beehive, ghost, biomeChecked));
    if (prefab.TryGetComponent(out Fermenter fermenter)) rules.Add(() => Shelter(Point(fermenter.m_roofCheckPoint, ghost), 0.7f, "$piece_fermenter_needroof", "$piece_fermenter_exposed"));
    if (prefab.TryGetComponent(out Bed bed)) rules.Add(() => Shelter(Point(bed.m_spawnPoint, ghost), 0.8f, "$msg_bedneedroof", "$msg_bedtooexposed"));
    if (prefab.TryGetComponent(out CraftingStation station)) rules.Add(() => Station(station, ghost));
    if (prefab.TryGetComponent(out Fireplace fireplace)) rules.Add(() => Fireplace(fireplace, ghost));
    if (prefab.TryGetComponent(out Smelter smelter)) rules.Add(() => Smelter(smelter, ghost));
    if (prefab.TryGetComponent(out Windmill windmill)) rules.Add(() => Windmill(windmill, ghost));
    if (prefab.TryGetComponent(out Plant plant)) rules.Add(() => Plant(plant, ghost, biomeChecked));
    if (piece && piece.m_comfort > 0 && ghost.TryGetComponent(out Piece ghostPiece)) rules.Add(() => Comfort(ghostPiece));
    return rules;
  }

  private static Vector3 Point(Transform child, Transform ghost) => ghost.TransformPoint(Prefab!.InverseTransformPoint(child.position));

  private static string Game(string token) => Localization.instance.Localize(token);
  private static string Detail(string value) => " (" + value + ")";
  private static string Needs(float cover, float limit) => Detail($"cover {Percent(cover)}, needs {Limit(limit)} {Red("↑")}");
  private static string Yellow(string value) => "<color=yellow>" + value + "</color>";
  private static string Red(string value) => "<color=red>" + value + "</color>";
  private static string Green(string value) => "<color=green>" + value + "</color>";
  private static string Percent(float value) => Yellow(Mathf.FloorToInt(value * 100f) + "%");
  private static string Limit(float value) => Yellow(Mathf.RoundToInt(value * 100f) + "%");

  private static string BiomeName(Heightmap.Biome biome) => Localization.instance.Localize("$biome_" + biome.ToString().ToLower());
  private static string Biomes(Heightmap.Biome biomes)
  {
    var names = Enum.GetValues(typeof(Heightmap.Biome)).Cast<Heightmap.Biome>()
      .Where(b => b != Heightmap.Biome.None && ((int)b & ((int)b - 1)) == 0 && (biomes & b) != 0)
      .Select(BiomeName).ToList();
    if (names.Count < 2) return string.Join("", names);
    return string.Join(", ", names.Take(names.Count - 1)) + " or " + names.Last();
  }

  private static IEnumerable<string> Shelter(Vector3 point, float limit, string roofText, string exposedText)
  {
    Cover.GetCoverForPoint(point, out var cover, out var roof);
    if (!roof) yield return Game(roofText);
    if (cover < limit) yield return Game(exposedText) + Needs(cover, limit);
  }

  private static IEnumerable<string> Beehive(Beehive obj, Transform ghost, bool biomeChecked)
  {
    var biome = Heightmap.FindBiome(ghost.position);
    if (!biomeChecked && (biome & obj.m_biome) == 0) yield return Game(obj.m_areaText) + Detail($"{BiomeName(biome)}, needs {Biomes(obj.m_biome)}");
    if (obj.m_maxCover <= 0f) yield break;
    Cover.GetCoverForPoint(Point(obj.m_coverPoint, ghost), out var cover, out _);
    if (cover >= obj.m_maxCover) yield return Game(obj.m_freespaceText) + Detail($"cover {Percent(cover)}, max {Limit(obj.m_maxCover)} {Red("↓")}");
  }

  private static IEnumerable<string> Station(CraftingStation obj, Transform ghost)
  {
    if (obj.m_craftRequireRoof)
    {
      foreach (var line in Shelter(Point(obj.m_roofCheckPoint, ghost), 0.7f, "$msg_stationneedroof", "$msg_stationtooexposed"))
        yield return line;
    }
    if (obj.m_craftRequireFire && !EffectArea.IsPointPlus025InsideBurningArea(ghost.position)) yield return Game("$msg_needfire");
  }

  private static IEnumerable<string> Fireplace(Fireplace obj, Transform ghost)
  {
    var pos = ghost.position;
    var point = pos + Vector3.up * obj.m_coverCheckOffset;
    if (obj.m_enabledObjectLow && obj.m_enabledObjectHigh)
    {
      Cover.GetCoverForPoint(point, out var cover, out var roof);
      if (!roof) yield return "Rain puts the fire out, needs a roof";
      if (cover < 0.7f) yield return "Wind puts the fire out" + Needs(cover, 0.7f);
    }
    if (obj.m_disableCoverCheck) yield break;
    if (SolidMask == 0) SolidMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain");
    var blocked = Heightmap.GetHeight(pos, out var height) && height > pos.y + obj.m_checkTerrainOffset;
    blocked |= Physics.Raycast(point, Vector3.up, 0.5f, SolidMask);
    blocked |= SmokeBlocked(obj.m_smokeSpawner, ghost);
    if (blocked) yield return "Smoke is blocked, the fire goes out";
  }

  private static IEnumerable<string> Smelter(Smelter obj, Transform ghost)
  {
    if (obj.m_requiresRoof && !Cover.IsUnderRoof(Point(obj.m_roofCheckPoint, ghost))) yield return Game("$piece_smelter_reqroof");
    if (SmokeBlocked(obj.m_smokeSpawner, ghost)) yield return "Smoke is blocked";
  }

  private static bool SmokeBlocked(SmokeSpawner? smoke, Transform ghost) => smoke && Physics.CheckSphere(Point(smoke!.transform, ghost), smoke.m_testRadius, smoke.m_testMask.value);

  private static IEnumerable<string> Windmill(Windmill obj, Transform ghost)
  {
    Cover.GetCoverForPoint(Point(obj.m_propeller, ghost), out var cover, out _);
    yield return "Wind power: " + Percent(1f - cover) + Detail("cover " + Percent(cover));
  }

  private static IEnumerable<string> Plant(Plant obj, Transform ghost, bool biomeChecked)
  {
    var pos = ghost.position;
    var biome = Heightmap.FindBiome(pos);
    if (!biomeChecked && (biome & obj.m_biome) == 0) yield return Game("$piece_plant_wrongbiome") + Detail($"{BiomeName(biome)}, grows in {Biomes(obj.m_biome)}");
    if (!obj.m_tolerateHeat && biome == Heightmap.Biome.AshLands && !ShieldGenerator.IsInsideShield(pos)) yield return Game("$piece_plant_toohot") + Detail("needs a shield");
    if (!obj.m_tolerateCold && (biome == Heightmap.Biome.DeepNorth || biome == Heightmap.Biome.Mountain) && !ShieldGenerator.IsInsideShield(pos)) yield return Game("$piece_plant_toocold") + Detail("needs a shield");
    if (RoofMask == 0) RoofMask = LayerMask.GetMask("Default", "static_solid", "piece");
    if (Physics.Raycast(pos, Vector3.up, 100f, RoofMask)) yield return Game("$piece_plant_nosun");
    if (!HaveGrowSpace(obj, ghost)) yield return Game("$piece_plant_nospace") + Detail(Yellow(obj.m_growRadius.ToString("0.#") + " m"));
  }

  private static bool HaveGrowSpace(Plant obj, Transform ghost)
  {
    var pos = ghost.position;
    if (SpaceMask == 0) SpaceMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "piece_nonsolid");
    var count = Physics.OverlapSphereNonAlloc(pos, obj.m_growRadius, Colliders, SpaceMask);
    for (var i = 0; i < count; i++)
    {
      if (Colliders[i].transform.IsChildOf(ghost)) continue;
      var plant = Colliders[i].GetComponent<Plant>();
      if (!plant || plant.GetStatus() == global::Plant.Status.Healthy) return false;
    }
    if (obj.m_growRadiusVines <= 0f) return true;
    count = Physics.OverlapSphereNonAlloc(pos, obj.m_growRadiusVines, Colliders, SpaceMask);
    for (var i = 0; i < count; i++)
    {
      if (Colliders[i].GetComponentInParent<Vine>()) return false;
    }
    return true;
  }

  private static IEnumerable<string> Comfort(Piece ghost)
  {
    var pos = ghost.transform.position;
    var before = SE_Rested.CalculateComfortLevel(true, pos);
    int Value(Piece piece) => piece == ghost ? piece.m_comfort : piece.GetComfort();
    List<Piece> pieces = [.. SE_Rested.GetNearbyComfortPieces(pos), ghost];
    pieces.Sort((x, y) =>
    {
      if (x.m_comfortGroup != y.m_comfortGroup) return x.m_comfortGroup.CompareTo(y.m_comfortGroup);
      if (Value(x) != Value(y)) return Value(y).CompareTo(Value(x));
      return y.m_name.CompareTo(x.m_name);
    });
    var after = 2;
    for (var i = 0; i < pieces.Count; i++)
    {
      var piece = pieces[i];
      var prev = i > 0 ? pieces[i - 1] : null;
      if (prev && ((piece.m_comfortGroup != Piece.ComfortGroup.None && piece.m_comfortGroup == prev!.m_comfortGroup) || piece.m_name == prev!.m_name)) continue;
      after += Value(piece);
    }
    var text = $"{Game("$se_rested_comfort")}: {Yellow(before.ToString())} → {Yellow(after.ToString())}";
    if (after > before) text += " " + Green("↑");
    yield return text;
  }
}
