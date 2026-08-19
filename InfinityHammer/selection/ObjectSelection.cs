using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;
using WorldEditCommands;
using System.Text.RegularExpressions;

namespace InfinityHammer;

// This is quite messy because single and multiple objects behave differently.
// But they have to be the same because selection is changed when zooping.
public partial class ObjectSelection : BaseSelection
{
  // Unity doesn't run scripts for inactive objects.
  // So an inactive object is used to store the selected object.
  // This mimics the ZNetScene.m_namedPrefabs behavior.
  private readonly GameObject Wrapper;
  public List<SelectedObject> Objects = [];
  public TerrainHeight? TerrainHeightInfo;
  public TerrainPaint? TerrainPaintInfo;
  private string SelectionBaseDescription = "";
  public override void Destroy()
  {
    base.Destroy();
    UnityEngine.Object.Destroy(Wrapper);
    Objects.Clear();
    TerrainHeightInfo = null;
    TerrainPaintInfo = null;
    SelectedPrefab = null;
  }

  private void SetTerrainState(TerrainHeight? terrainHeightInfo, TerrainPaint? terrainPaintInfo)
  {
    TerrainHeightInfo = terrainHeightInfo;
    TerrainPaintInfo = terrainPaintInfo;
  }

  public ObjectSelection(ZNetView view, bool singleUse, Vector3? scale, DataEntry? extraData, TerrainHeight? terrainHeightInfo = null, TerrainPaint? terrainPaintInfo = null)
  {
    if (view.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    Wrapper = new GameObject();
    Wrapper.SetActive(false);

    var zdo = view.GetZDO();
    var prefabHash = zdo == null ? view.GetPrefabName().GetStableHashCode() : zdo.GetPrefab();
    DataEntry? data = zdo == null ? extraData : DataHelper.Merge(new(zdo), extraData);

    SingleUse = singleUse;
    SelectedPrefab = HammerHelper.SafeInstantiate(view, Wrapper);
    SelectedPrefab.transform.position = Vector3.zero;
    UpdateVisuals(SelectedPrefab, data);
    Objects.Add(new(prefabHash, IsScalable(view), data));
    if (zdo != null)
      PlaceRotation.Set(SelectedPrefab);
    // Reset for zoop bounds check.
    SelectedPrefab.transform.rotation = Quaternion.identity;
    if (scale.HasValue)
      SelectedPrefab.transform.localScale = scale.Value;
    Scaling.Set(SelectedPrefab);
    var hasSnaps = Snapping.GetSnapPoints(SelectedPrefab).Count > 0;
    if (Configuration.Snapping != SnappingMode.Off && !hasSnaps)
      Snapping.BuildSnaps(SelectedPrefab);

    SetTerrainState(terrainHeightInfo, terrainPaintInfo);
    UpdateSelectionDescription();
  }
  // This is for compatibility. Many mods don't expect a cleaned up ghost.
  // So when selecting from the build menu, the ghost doesn't have to be cleaned up.
  public ObjectSelection(Piece piece, bool singleUse)
  {
    Wrapper = new GameObject();
    Wrapper.SetActive(false);
    var view = piece.GetComponent<ZNetView>();
    var prefabHash = view.GetPrefabName().GetStableHashCode();
    SelectedPrefab = UnityEngine.Object.Instantiate(view.gameObject, Wrapper.transform);
    SelectedPrefab.name = view.name;

    SingleUse = singleUse;
    Objects.Add(new(prefabHash, IsScalable(view), null));
    Scaling.Set(SelectedPrefab);
  }
  public ObjectSelection(IEnumerable<ZNetView> views, bool singleUse, Vector3? scale, DataEntry? extraData, TerrainHeight? terrainHeightInfo = null, TerrainPaint? terrainPaintInfo = null)
  {
    Wrapper = new GameObject();
    Wrapper.SetActive(false);

    SingleUse = singleUse;
    SelectedPrefab = new GameObject();
    SelectedPrefab.transform.SetParent(Wrapper.transform);
    if (scale.HasValue)
      SelectedPrefab.transform.localScale = scale.Value;
    SelectedPrefab.name = $"Multiple ({views.Count()})";
    SelectedPrefab.transform.position = views.First().transform.position;
    foreach (var view in views)
    {
      DataEntry? data = DataHelper.Merge(new(view.GetZDO()), extraData);
      var obj = HammerHelper.ChildInstantiate(view, SelectedPrefab);
      obj.transform.position = view.transform.position;
      obj.transform.rotation = view.transform.rotation;
      UpdateVisuals(obj, data);
      Objects.Add(new(view.GetZDO().GetPrefab(), IsScalable(view), data));
    }
    SelectedPrefab.transform.position = Vector3.zero;
    Snapping.GenerateSnapPoints(SelectedPrefab);
    CountObjects();
    PlaceRotation.Set(SelectedPrefab);
    Scaling.Set(SelectedPrefab);

    SetTerrainState(terrainHeightInfo, terrainPaintInfo);
    UpdateSelectionDescription();
  }


  public ObjectSelection(Terminal terminal, Blueprint bp, Vector3 scale)
  {
    Wrapper = new GameObject();
    Wrapper.SetActive(false);

    SelectedPrefab = new GameObject();
    SelectedPrefab.transform.SetParent(Wrapper.transform);
    SelectedPrefab.name = bp.Name;
    SelectedPrefab.transform.localScale = scale;
    SelectedPrefab.transform.position = Helper.GetPlayer().transform.position;
    var piece = SelectedPrefab.AddComponent<Piece>();
    piece.m_name = bp.Name;
    piece.m_description = bp.Description;
    if (piece.m_description == "")
      piece.m_description = "Center: " + bp.CenterPiece;
    SelectionBaseDescription = piece.m_description;
    var centerPieceExists = false;
    foreach (var item in bp.Objects)
    {
      if (item.Prefab == bp.CenterPiece)
        centerPieceExists = true;
      if (Configuration.UseBlueprintChance && item.Chance != 1f && UnityEngine.Random.value > item.Chance) continue;
      try
      {
        var prefab = ZNetScene.instance.GetPrefab(item.Prefab);
        if (!prefab) throw new InvalidOperationException($"Prefab {item.Prefab} not found.");
        var view = prefab.GetComponent<ZNetView>();
        var obj = HammerHelper.ChildInstantiate(view, SelectedPrefab);
        obj.transform.localPosition = item.Pos;
        obj.transform.localRotation = item.Rot;
        obj.transform.localScale = item.Scale;
        DataEntry? data = item.Data == null || item.Data == "" ? ReadExtraInfo(obj, item.ExtraInfo) : DataHelper.Get(item.Data);
        UpdateVisuals(obj, data);
        Objects.Add(new SelectedObject(item.Prefab.GetStableHashCode(), IsScalable(view), data));
      }
      catch (Exception e)
      {
        HammerHelper.Message(terminal, $"Warning: {e.Message}");
      }
    }
    // Might be good to have a proper loading for single item blueprints, but this works for now.
    if (Objects.Count == 1)
      ToSingle();

    // Snapping not needed when the user is using a specific center point.
    if (!centerPieceExists)
    {
      if (bp.SnapPoints.Count == 0)
        Snapping.GenerateSnapPoints(SelectedPrefab);
      else
        Snapping.CreateSnapPoints(SelectedPrefab, bp.SnapPoints);
    }

    piece.m_clipEverything = Snapping.CountSnapPoints(SelectedPrefab) == 0;
    Scaling.Set(SelectedPrefab);

    var terrainHeightInfo = bp.TerrainHeight?.Clone();
    var terrainPaintInfo = bp.TerrainPaint?.Clone();
    SetTerrainState(terrainHeightInfo, terrainPaintInfo);
    UpdateSelectionDescription();
  }

  private string BuildTerrainSummaryDescription()
  {
    var lines = new List<string>();

    if (TerrainHeightInfo != null)
      lines.Add($"Terrain height radius: {HammerHelper.Format(TerrainHeightInfo.GetRadius())}");
    if (TerrainPaintInfo != null)
      lines.Add($"Terrain paint radius: {HammerHelper.Format(TerrainPaintInfo.GetRadius())}");

    return string.Join("\n", lines);
  }

  private static string BuildObjectCountDescription(Dictionary<int, int> counts)
  {
    var topKeys = counts.OrderBy(kvp => kvp.Value).Reverse().ToArray();
    if (topKeys.Length <= 5)
      return string.Join("\n", topKeys.Select(kvp => $"{ZNetScene.instance.GetPrefab(kvp.Key).name}: {kvp.Value}"));

    var description = string.Join("\n", topKeys.Take(4).Select(kvp => $"{ZNetScene.instance.GetPrefab(kvp.Key).name}: {kvp.Value}"));
    description += $"\n{topKeys.Length - 4} other types: {topKeys.Skip(4).Sum(kvp => kvp.Value)}";
    return description;
  }

  private void UpdateSelectionDescription()
  {
    var piece = SelectedPrefab.GetComponent<Piece>();
    if (!piece)
      return;

    var terrainSummary = BuildTerrainSummaryDescription();
    var lines = string.IsNullOrEmpty(SelectionBaseDescription) ? new List<string>() : SelectionBaseDescription.Split('\n').ToList();
    if (!string.IsNullOrEmpty(terrainSummary))
      lines.Add(terrainSummary);

    piece.m_description = string.Join("\n", lines);
  }

  public void Mirror()
  {
    var i = 0;
    foreach (Transform tr in SelectedPrefab.transform)
    {
      var prefab = i < Objects.Count ? Objects[i].Prefab : 0;
      i += 1;
      if (Snapping.IsSnapPoint(tr.gameObject))
      {
        prefab = 0;
        i -= 1;
      }
      tr.localPosition = new(tr.localPosition.x, tr.localPosition.y, -tr.localPosition.z);

      var angles = tr.localEulerAngles;
      angles = new(angles.x, -angles.y, angles.z);
      if (Configuration.MirrorFlip.Contains(prefab))
        angles.y += 180;
      tr.localRotation = Quaternion.Euler(angles);
    }
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public void Postprocess()
  {
    if (Objects.Count == 1)
    {
      if (Snapping.CountSnapPoints(SelectedPrefab) == 0)
        Snapping.CreateSnapPoint(SelectedPrefab, Vector3.zero, "Center");
    }
  }


  private static float Convert(int value, float defaultValue)
  {
    if (value == 0) return 0.1f;
    if (value == 1) return 0.5f;
    if (value == 2) return 1f;
    return defaultValue;
  }
  private static void SetWear(WearNTear wearNTear, int wear)
  {
    if (wear == -1) return;
    wearNTear.SetHealthVisual(Convert(wear, 1f), false);
  }
  private static void SetGrowth(Plant plant, int growth)
  {
    if (growth == -1) return;
    var healthy = growth == 0;
    var unhealthy = growth == 1;
    var healthyGrown = growth == 2;
    var unhealthyGrown = growth == 3;
    if (plant.m_healthyGrown)
    {
      plant.m_healthy.SetActive(healthy);
      plant.m_unhealthy.SetActive(unhealthy);
      plant.m_healthyGrown.SetActive(healthyGrown);
      plant.m_unhealthyGrown.SetActive(unhealthyGrown);
    }
    else
    {
      plant.m_healthy.SetActive(healthy || healthyGrown);
      plant.m_unhealthy.SetActive(unhealthy || unhealthyGrown);
    }
  }
  protected static DataEntry? ReadExtraInfo(GameObject obj, string extraInfo)
  {
    if (extraInfo == "") return null;

    // new planbuild format can have an escaped string, try to deserialize
    if (extraInfo.StartsWith("\"") && extraInfo.EndsWith("\""))
    {
      extraInfo = Regex.Unescape(extraInfo.Substring(1, extraInfo.Length - 2));
    }

    DataEntry data = new();
    if (obj.TryGetComponent<Sign>(out var sign))
    {
      data.Set(ZDOVars.s_text, extraInfo);
      sign.m_textWidget.text = extraInfo;
    }
    if (obj.GetComponent<TeleportWorld>())
    {
      data.Set(ZDOVars.s_tag, extraInfo);
    }
    if (obj.GetComponent<Tameable>())
    {
      data.Set(ZDOVars.s_tamedName, extraInfo);
    }
    // Fish don't work with piece, see PreventFishBecomingPieces patch.
    if (obj.TryGetComponent<ItemDrop>(out var _) && !obj.TryGetComponent<Fish>(out var _))
    {
      data.Set(ZDOVars.s_piece, 1);
    }
    if (obj.TryGetComponent<ItemStand>(out var _))
    {
      var split = extraInfo.Split(':');
      var name = split[0];
      var variant = Parse.Int(split, 1, 0);
      var quality = Parse.Int(split, 2, 1);
      var orientation = Parse.Int(split, 3, 0);
      data.Set(ZDOVars.s_item, name);
      data.Set(ZDOVars.s_variant, variant);
      data.Set(ZDOVars.s_quality, quality);
      if (split.Length > 3)
      {
        data.Set(ZDOVars.s_type, orientation);
      }
    }
    if (obj.TryGetComponent<ArmorStand>(out var armorStand))
    {
      var split = extraInfo.Split(':');
      var pose = Parse.Int(split, 0, 0);
      data.Set(ZDOVars.s_pose, pose);
      for (var i = 0; i < armorStand.m_slots.Count; i++)
      {
        var name = Parse.String(split, i * 2 + 2, "");
        var variant = Parse.Int(split, i * 2 + 3, 0);
        if (name == "") continue;
        data.Set(StringExtensionMethods.GetStableHashCode($"{i}_item"), name);
        data.Set(StringExtensionMethods.GetStableHashCode($"{i}_variant"), variant);
      }
    }
    return data;
  }
  protected static void UpdateVisuals(GameObject obj, DataEntry? data)
  {
    if (data == null) return;
    Dictionary<string, string> pars = [];
    if (data.TryGetString(pars, ZDOVars.s_text, out var signText) && obj.TryGetComponent<Sign>(out var sign))
    {
      sign.m_textWidget.text = signText;
    }
    if (data.TryGetString(pars, ZDOVars.s_item, out var item) && obj.TryGetComponent<ItemStand>(out var itemStand))
    {
      var variant = data.TryGetInt(pars, ZDOVars.s_variant, out var v) ? v : 0;
      var quality = data.TryGetInt(pars, ZDOVars.s_quality, out var q) ? q : 1;
      var orientation = data.TryGetInt(pars, ZDOVars.s_type, out var t) ? t : 0;
      itemStand.SetVisualItem(item, variant, quality, orientation);
    }
    if (obj.TryGetComponent<ArmorStand>(out var armorStand))
    {
      armorStand.m_pose = data.TryGetInt(pars, ZDOVars.s_pose, out var pose) ? pose : 0;
      armorStand.m_poseAnimator.SetInteger("Pose", pose);
      SetItemHack.Hack = true;
      for (var i = 0; i < armorStand.m_slots.Count; i++)
      {
        var name = data.TryGetString(pars, StringExtensionMethods.GetStableHashCode($"{i}_item"), out var s) ? s : "";
        var variant = data.TryGetInt(pars, StringExtensionMethods.GetStableHashCode($"{i}_variant"), out var v) ? v : 0;
        if (name != "")
          armorStand.SetVisualItem(i, name, variant);
      }
      SetItemHack.Hack = false;
    }
    if (obj.TryGetComponent<Character>(out var character))
    {
      if (data.TryGetFloat(pars, ZDOVars.s_health, out var health))
      {
        data.Set(ZDOVars.s_maxHealth, health);
        data.Set(ZDOVars.s_health, health * 1.000001f);
      }
      if (data.TryGetInt(pars, ZDOVars.s_level, out var level) && level > 1 && obj.TryGetComponent<LevelEffects>(out var effect))
      {
        effect.m_character = character;
        effect.SetupLevelVisualization(level);
      }
    }

    if (data.TryGetInt(pars, Hashes.Wear, out var wear) && obj.TryGetComponent<WearNTear>(out var wearNTear))
    {
      SetWear(wearNTear, wear);
    }
    if (data.TryGetInt(pars, Hashes.Growth, out var growth) && obj.TryGetComponent<Plant>(out var plant))
    {
      SetGrowth(plant, growth);
    }
  }
  private void CountObjects()
  {
    if (Objects.Count < 2) return;
    SelectedPrefab.name = $"Multiple ({Snapping.CountActiveChildren(SelectedPrefab)})";
    var piece = SelectedPrefab.GetComponent<Piece>();
    if (!piece) piece = SelectedPrefab.AddComponent<Piece>();
    piece.m_clipEverything = Snapping.CountSnapPoints(SelectedPrefab) == 0;
    piece.m_name = SelectedPrefab.name;
    Dictionary<int, int> counts = Objects.GroupBy(obj => obj.Prefab).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());
    SelectionBaseDescription = BuildObjectCountDescription(counts);
  }
  public override DataEntry? GetData(int index = 0)
  {
    if (Objects.Count <= index) throw new InvalidOperationException("Invalid index.");
    return Objects[index].Data;
  }
  public override int GetPrefab(int index = 0)
  {
    if (Objects.Count <= index) throw new InvalidOperationException("Invalid index.");
    return Objects[index].Prefab;
  }
  public override bool IsScalingSupported() => Objects.All(obj => obj.Scalable);
  public override GameObject GetPrefab(GameObject obj)
  {
    UndoHelper.BeginSubAction();
    if (Objects.Count == 1)
    {
      var name = Utils.GetPrefabName(obj);
      var tr = HammerHelper.GetPlacementGhost().transform;
      var zdo = DataHelper.Init(StringExtensionMethods.GetStableHashCode(name), tr, GetData(0));
      if (zdo != null)
        DungeonRooms.Reposition(zdo, tr);
      return ZNetScene.instance.GetPrefab(name);
    }
    var dummy = new GameObject
    {
      name = "Blueprint"
    };
    dummy.AddComponent<Piece>();
    return dummy;
  }
  public override void AfterPlace(GameObject obj)
  {
    if (Objects.Count == 1)
    {
      var view = obj.GetComponent<ZNetView>();
      // Hoe adds pieces too.
      if (!view) return;
      view.m_body?.WakeUp();
      PostProcessPlaced(obj);
      ApplyTerrainChanges(obj.transform.position, obj.transform.rotation);
    }
    else
    {
      HandleMultiple(HammerHelper.GetPlacementGhost());
      UnityEngine.Object.Destroy(obj);
    }

    UndoHelper.EndSubAction();
  }

  private void ApplyTerrainChanges(Vector3 placementPosition, Quaternion placementRotation)
  {
    if (TerrainHeightInfo == null && TerrainPaintInfo == null) return;
    var terrainRadius = TerrainInfo.ResolveRadius(TerrainHeightInfo, TerrainPaintInfo);

    var heightRotation = TerrainHeightInfo == null ? Quaternion.identity : GetRelativeYawRotation(placementRotation, TerrainHeightInfo.FirstNodeRotation);
    var paintRotation = TerrainPaintInfo == null ? Quaternion.identity : GetRelativeYawRotation(placementRotation, TerrainPaintInfo.FirstNodeRotation);

    // Get terrain compilers around the placement position
    var compilers = Terrain.GetCompilers(placementPosition, new(terrainRadius));

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var index = z * max + x;

          // Apply height changes using optimized lookup
          var nearestHeight = TerrainHeightInfo?.FindNearest(nodePos, placementPosition, heightRotation);
          if (nearestHeight != null)
          {
            if (index < compiler.m_hmap.m_heights.Count)
            {
              var altitude = nearestHeight.Value + placementPosition.y;
              compiler.m_levelDelta[index] += altitude - compiler.m_hmap.m_heights[index];
              compiler.m_smoothDelta[index] = 0f;
              compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
            }
          }

          // Apply paint changes using optimized lookup
          var paintWorldPos = nodePos;
          var nearestPaint = TerrainPaintInfo?.FindNearest(paintWorldPos, placementPosition, paintRotation);
          if (nearestPaint != null)
          {
            if (index < compiler.m_paintMask.Length)
            {
              compiler.m_paintMask[index] = nearestPaint.Value;
              compiler.m_modifiedPaint[index] = true;
            }
          }
        }
      }
    }

    foreach (var compiler in compilers)
      Terrain.Save(compiler);
    ClutterSystem.instance?.ResetGrass(placementPosition, terrainRadius);
  }

  private static Quaternion GetRelativeYawRotation(Quaternion placementRotation, Quaternion originalRotation)
  {
    var rotationDifference = placementRotation * Quaternion.Inverse(originalRotation);
    var forward = rotationDifference * Vector3.forward;
    forward.y = 0f;
    if (forward.sqrMagnitude <= 0.0001f)
      return Quaternion.identity;
    return Quaternion.LookRotation(forward.normalized, Vector3.up);
  }

  private static Vector3 VertexToWorld(Heightmap hmap, int x, int z)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2) * hmap.m_scale;
    vector.z += (z - hmap.m_width / 2) * hmap.m_scale;
    return vector;
  }
  private static bool IsScalable(ZNetView view)
  {
    if (view.m_syncInitialScale) return true;
    if (Configuration.ScaleZSyncObjects && view.GetComponent<ZSyncTransform>()) return true;
    return false;
  }
  private void HandleMultiple(GameObject ghost)
  {
    var children = Snapping.GetChildren(ghost);
    for (var i = 0; i < children.Count; i++)
    {
      var ghostChild = children[i];
      var hash = GetPrefab(i);
      var prefab = ZNetScene.instance.GetPrefab(hash);
      if (prefab)
      {
        var zdo = DataHelper.Init(hash, ghostChild.transform, GetData(i));
        if (zdo != null)
          DungeonRooms.Reposition(zdo, ghostChild.transform);
        var childObj = UnityEngine.Object.Instantiate(prefab, ghostChild.transform.position, ghostChild.transform.rotation);
        PostProcessPlaced(childObj);
      }
      if (i == 0)
        ApplyTerrainChanges(ghostChild.transform.position, ghostChild.transform.rotation);
    }
  }

  public GameObject AddObject(ZNetView view, Vector3 pos)
  {
    if (Objects.Count == 1)
      ToMulti();
    var obj = HammerHelper.ChildInstantiate(view, SelectedPrefab);
    obj.transform.rotation = view.transform.rotation;
    obj.transform.localPosition = pos;
    if (Configuration.Snapping != SnappingMode.Off)
      Snapping.RegenerateSnapPoints(SelectedPrefab);
    Objects.Add(new SelectedObject(Objects[0].Prefab, Objects[0].Scalable, Objects[0].Data));
    return obj;
  }
  private void ToMulti()
  {
    var obj = SelectedPrefab;
    SelectedPrefab = new GameObject();
    SelectedPrefab.transform.SetParent(Wrapper.transform);
    SelectedPrefab.transform.position = obj.transform.position;
    SelectedPrefab.transform.rotation = obj.transform.rotation;
    obj.transform.SetParent(SelectedPrefab.transform);
    obj.transform.localScale = Vector3.one;
    if (obj.TryGetComponent<Piece>(out var piece))
    {
      var prefab = ZNetScene.instance.GetPrefab(Objects[0].Prefab);
      if (prefab && !prefab.GetComponent<Piece>())
        UnityEngine.Object.Destroy(piece);
    }
  }
  public void RemoveObject(GameObject obj)
  {
    if (Objects.Count == 1)
      return;
    // Must be deactivated so that destroy doesn't activate it.
    obj.SetActive(false);
    obj.transform.SetParent(null);
    UnityEngine.Object.Destroy(obj);
    Objects.RemoveAt(Objects.Count - 1);
    if (Objects.Count == 1)
      ToSingle();
    else if (Configuration.Snapping != SnappingMode.Off)
      Snapping.RegenerateSnapPoints(SelectedPrefab);
  }
  private void ToSingle()
  {
    var obj = SelectedPrefab.transform.GetChild(0).gameObject;
    HammerHelper.EnsurePiece(obj);
    // Must transfer parent directly to prevent self-activation.
    obj.transform.SetParent(Wrapper.transform);
    // Must be deactivated so that destroy doesn't activate it.
    SelectedPrefab.SetActive(false);
    UnityEngine.Object.Destroy(SelectedPrefab);
    SelectedPrefab = obj;
    Objects = [.. Objects.Take(1)];
  }
  public override void Activate()
  {
    base.Activate();
    Scaling.Set(SelectedPrefab);
  }
}