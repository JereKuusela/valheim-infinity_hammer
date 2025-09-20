using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;
using WorldEditCommands;

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
  public TerrainData? TerrainInfo;
  public float TerrainRadius = 0f;
  public override void Destroy()
  {
    base.Destroy();
    UnityEngine.Object.Destroy(Wrapper);
    Objects.Clear();
    TerrainInfo = null;
    SelectedPrefab = null;
  }

  public ObjectSelection(ZNetView view, bool singleUse, Vector3? scale, DataEntry? extraData)
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
    Objects.Add(new(prefabHash, view.m_syncInitialScale, data));
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
    Objects.Add(new(prefabHash, view.m_syncInitialScale, null));
    Scaling.Set(SelectedPrefab);
  }
  public ObjectSelection(IEnumerable<ZNetView> views, bool singleUse, Vector3? scale, DataEntry? extraData)
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
      Objects.Add(new(view.GetZDO().GetPrefab(), view.m_syncInitialScale, data));
    }
    SelectedPrefab.transform.position = Vector3.zero;
    Snapping.GenerateSnapPoints(SelectedPrefab);
    CountObjects();
    PlaceRotation.Set(SelectedPrefab);
    Scaling.Set(SelectedPrefab);
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
        Objects.Add(new SelectedObject(item.Prefab.GetStableHashCode(), view.m_syncInitialScale, data));
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
  }

  public void SetTerrainData(TerrainData terrainInfo, float terrainRadius)
  {
    TerrainInfo = terrainInfo;
    TerrainRadius = terrainRadius;
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
    if (obj.TryGetComponent<ItemStand>(out var itemStand))
    {
      var split = extraInfo.Split(':');
      var name = split[0];
      var variant = Parse.Int(split, 1, 0);
      var quality = Parse.Int(split, 2, 1);
      data.Set(ZDOVars.s_item, name);
      data.Set(ZDOVars.s_variant, variant);
      data.Set(ZDOVars.s_quality, quality);
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

    if (data.TryGetInt(pars, Hash.Wear, out var wear) && obj.TryGetComponent<WearNTear>(out var wearNTear))
    {
      SetWear(wearNTear, wear);
    }
    if (data.TryGetInt(pars, Hash.Growth, out var growth) && obj.TryGetComponent<Plant>(out var plant))
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
    var topKeys = counts.OrderBy(kvp => kvp.Value).Reverse().ToArray();
    if (topKeys.Length <= 5)
      piece.m_description = string.Join("\n", topKeys.Select(kvp => $"{ZNetScene.instance.GetPrefab(kvp.Key).name}: {kvp.Value}"));
    else
    {
      piece.m_description = string.Join("\n", topKeys.Take(4).Select(kvp => $"{ZNetScene.instance.GetPrefab(kvp.Key).name}: {kvp.Value}"));
      piece.m_description += $"\n{topKeys.Length - 4} other types: {topKeys.Skip(4).Sum(kvp => kvp.Value)}";
    }
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
    if (TerrainInfo == null) return;

    // Calculate rotation difference specifically along Y-axis in degrees
    var originalRotation = TerrainInfo.FirstNodeRotation;

    // Calculate the rotation difference by finding the quaternion that transforms from original to placement
    var rotationDifference = placementRotation * Quaternion.Inverse(originalRotation);

    // Extract the Y-axis rotation from the difference quaternion
    var yRotationDifference = rotationDifference.eulerAngles.y;

    // Convert to signed angle (-180 to +180 degrees)
    if (yRotationDifference > 180f)
      yRotationDifference -= 360f;

    // Convert to radians for the terrain lookup
    var rotation = Mathf.Deg2Rad * yRotationDifference;

    // Get terrain compilers around the placement position
    var compilers = Terrain.GetCompilers(placementPosition, new(TerrainRadius));

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
          var nearestHeight = TerrainInfo.FindNearestHeight(nodePos, placementPosition, rotation);
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
          //paintWorldPos.x += 0.5f;
          //paintWorldPos.z += 0.5f;
          var nearestPaint = TerrainInfo.FindNearestPaint(paintWorldPos, placementPosition, rotation);
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
    ClutterSystem.instance?.ResetGrass(placementPosition, TerrainRadius);
  }

  private static Vector3 VertexToWorld(Heightmap hmap, int x, int z)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2) * hmap.m_scale;
    vector.z += (z - hmap.m_width / 2) * hmap.m_scale;
    return vector;
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