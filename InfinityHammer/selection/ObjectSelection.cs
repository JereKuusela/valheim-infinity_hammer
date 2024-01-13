using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;

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
  public override void Destroy()
  {
    base.Destroy();
    UnityEngine.Object.Destroy(Wrapper);
  }

  public ObjectSelection(ZNetView view, bool singleUse, Vector3? scale)
  {
    if (view.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    Wrapper = new GameObject();
    Wrapper.SetActive(false);

    var zdo = view.GetZDO();
    var prefabHash = zdo == null ? view.GetPrefabName().GetStableHashCode() : zdo.GetPrefab();
    ZDOData data = zdo == null ? new() : new(zdo);

    SingleUse = singleUse;
    SelectedPrefab = HammerHelper.SafeInstantiate(view, Wrapper);
    SelectedPrefab.transform.position = Vector3.zero;
    Objects.Add(new(prefabHash, view.m_syncInitialScale, data));
    if (zdo != null)
      PlaceRotation.Set(SelectedPrefab);
    // Reset for zoop bounds check.
    SelectedPrefab.transform.rotation = Quaternion.identity;
    if (scale.HasValue)
      SelectedPrefab.transform.localScale = scale.Value;
    Scaling.Set(SelectedPrefab);
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

    SingleUse = singleUse;
    Objects.Add(new(prefabHash, view.m_syncInitialScale, new()));
    Scaling.Set(SelectedPrefab);
  }
  public ObjectSelection(IEnumerable<ZNetView> views, bool singleUse, Vector3? scale)
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
      ZDOData data = new(view.GetZDO());
      var obj = HammerHelper.ChildInstantiate(view, SelectedPrefab);
      obj.transform.position = view.transform.position;
      obj.transform.rotation = view.transform.rotation;
      SetExtraInfo(obj, "", data);
      Objects.Add(new(view.GetZDO().GetPrefab(), view.m_syncInitialScale, data));
      if (view == views.First() || Configuration.AllSnapPoints)
        AddSnapPoints(obj);
    }
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
    foreach (var item in bp.Objects)
    {
      try
      {
        var prefab = ZNetScene.instance.GetPrefab(item.Prefab);
        if (!prefab) throw new InvalidOperationException($"Prefab {item.Prefab} not found.");
        var view = prefab.GetComponent<ZNetView>();
        var obj = HammerHelper.ChildInstantiate(view, SelectedPrefab);
        obj.transform.localPosition = item.Pos;
        obj.transform.localRotation = item.Rot;
        obj.transform.localScale = item.Scale;
        ZDOData data = new(item.Data);
        SetExtraInfo(obj, item.ExtraInfo, data);
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
    foreach (var position in bp.SnapPoints)
      CreateSnapPoint(position);

    piece.m_clipEverything = HammerHelper.CountSnapPoints(SelectedPrefab) == 0;
    Scaling.Set(SelectedPrefab);
  }
  private void CreateSnapPoint(Vector3 pos)
  {
    GameObject snapObj = new()
    {
      name = "_snappoint",
      layer = LayerMask.NameToLayer("piece"),
      tag = "snappoint",
    };
    snapObj.SetActive(false);
    snapObj.transform.parent = SelectedPrefab.transform;
    snapObj.transform.localPosition = pos;
  }
  public void Mirror()
  {
    var i = 0;
    foreach (Transform tr in SelectedPrefab.transform)
    {
      var prefab = i < Objects.Count ? Objects[i].Prefab : 0;
      i += 1;
      if (HammerHelper.IsSnapPoint(tr.gameObject))
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
      Postprocess(SelectedPrefab, GetData());
      if (HammerHelper.CountSnapPoints(SelectedPrefab) == 0)
        CreateSnapPoint(Vector3.zero);
    }
    else
    {
      var i = 0;
      foreach (Transform tr in SelectedPrefab.transform)
      {
        if (HammerHelper.IsSnapPoint(tr.gameObject)) continue;
        Postprocess(tr.gameObject, GetData(i++));
      }
    }
  }

  private void Postprocess(GameObject obj, ZDOData? zdo)
  {
    if (zdo == null) return;
    SetLevel(obj, zdo.GetInt(Hash.Level, -1));
    SetGrowth(obj, zdo.GetInt(Hash.Growth, -1));
    SetWear(obj, zdo.GetInt(Hash.Wear, -1));
    SetText(obj, zdo.GetString(Hash.Text, ""));
  }

  private static void SetLevel(GameObject obj, int level)
  {
    if (level == -1) return;
    if (obj.GetComponent<Character>() is not { } character) return;
    if (obj.GetComponentInChildren<LevelEffects>() is not { } effect) return;
    effect.m_character = character;
    effect.SetupLevelVisualization(level);
  }
  private static float Convert(int value, float defaultValue)
  {
    if (value == 0) return 0.1f;
    if (value == 1) return 0.5f;
    if (value == 2) return 1f;
    return defaultValue;
  }
  private static void SetWear(GameObject obj, int wear)
  {
    if (wear == -1) return;
    if (obj.GetComponent<WearNTear>() is not { } wearNTear) return;
    wearNTear.SetHealthVisual(Convert(wear, 1f), false);
  }
  private static void SetText(GameObject obj, string text)
  {
    if (obj.GetComponent<Sign>() is not { } sign) return;
    sign.m_textWidget.text = text;
  }
  private static void SetGrowth(GameObject obj, int growth)
  {
    if (growth == -1) return;
    if (obj.GetComponent<Plant>() is not { } plant) return;
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
  protected static void SetExtraInfo(GameObject obj, string extraInfo, ZDOData data)
  {
    if (obj.TryGetComponent<Sign>(out var sign))
    {
      if (extraInfo == "")
        extraInfo = data.GetString(Hash.Text, extraInfo);
      else
        data.Set(Hash.Text, extraInfo);
      sign.m_textWidget.text = extraInfo;
    }
    if (obj.GetComponent<TeleportWorld>() && extraInfo != "")
    {
      data.Set(Hash.Tag, extraInfo);
    }
    if (obj.GetComponent<Tameable>() && extraInfo != "")
    {
      data.Set(Hash.TamedName, extraInfo);
    }
    if (obj.TryGetComponent<ItemStand>(out var itemStand))
    {
      var split = extraInfo.Split(':');
      var name = split[0];
      var variant = Parse.Int(split, 1, 0);
      var quality = Parse.Int(split, 2, 1);
      if (extraInfo == "")
      {
        name = data.GetString(Hash.Item, name);
        variant = data.GetInt(Hash.Variant, variant);
        quality = data.GetInt(Hash.Quality, quality);
      }
      else
      {
        data.Set(Hash.Item, name);
        data.Set(Hash.Variant, variant);
        data.Set(Hash.Quality, quality);
      }
      itemStand.SetVisualItem(name, variant, quality);
    }
    if (obj.TryGetComponent<ArmorStand>(out var armorStand))
    {
      var split = extraInfo.Split(':');
      var pose = Parse.Int(split, 0, 0);
      if (extraInfo == "")
        pose = data.GetInt(Hash.Pose, pose);
      else
        data.Set(Hash.Pose, pose);
      armorStand.m_pose = pose;
      armorStand.m_poseAnimator.SetInteger("Pose", pose);
      SetItemHack.Hack = true;
      for (var i = 0; i < armorStand.m_slots.Count; i++)
      {
        var name = Parse.String(split, i * 2 + 2, "");
        var variant = Parse.Int(split, i * 2 + 3, 0);
        if (extraInfo == "")
        {
          name = data.GetString($"{i}_item", name);
          variant = data.GetInt($"{i}_variant", variant);
        }
        else
        {
          data.Set($"{i}_item", name);
          data.Set($"{i}_variant", variant);
        }
        armorStand.SetVisualItem(i, name, variant);
      }
      SetItemHack.Hack = false;
    }
  }
  private void CountObjects()
  {
    if (Objects.Count < 2) return;
    SelectedPrefab.name = $"Multiple ({HammerHelper.CountActiveChildren(SelectedPrefab)})";
    var piece = SelectedPrefab.GetComponent<Piece>();
    if (!piece) piece = SelectedPrefab.AddComponent<Piece>();
    piece.m_clipEverything = HammerHelper.CountSnapPoints(SelectedPrefab) == 0;
    piece.m_name = SelectedPrefab.name;
    Dictionary<int, int> counts = Objects.GroupBy(obj => obj.Prefab).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());
    var topKeys = counts.OrderBy(kvp => kvp.Value).Reverse().ToArray();
    if (topKeys.Length <= 5)
      piece.m_description += "\n" + string.Join("\n", topKeys.Select(kvp => $"{ZNetScene.instance.GetPrefab(kvp.Key).name}: {kvp.Value}"));
    else
    {
      piece.m_description += "\n" + string.Join("\n", topKeys.Take(4).Select(kvp => $"{ZNetScene.instance.GetPrefab(kvp.Key).name}: {kvp.Value}"));
      piece.m_description += $"\n{topKeys.Length - 4} other types: {topKeys.Skip(4).Sum(kvp => kvp.Value)}";
    }
  }
  private void AddSnapPoints(GameObject obj)
  {
    foreach (Transform tr in obj.transform)
    {
      if (!tr || !HammerHelper.IsSnapPoint(tr.gameObject)) continue;
      CreateSnapPoint(tr.position);
    }
  }
  public override ZDOData GetData(int index = 0)
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
  public void UpdateZDOs(ZDOData data)
  {
    Objects.ForEach(obj => obj.Data.Add(data));
  }
  public override GameObject GetPrefab(GameObject obj)
  {
    if (Objects.Count == 1)
    {
      var name = Utils.GetPrefabName(obj);
      DataHelper.Init(name, obj.transform, GetData(0));
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
      Undo.CreateObject(obj);
    }
    else
    {
      HandleMultiple(HammerHelper.GetPlacementGhost());
      UnityEngine.Object.Destroy(obj);
    }
  }
  private void HandleMultiple(GameObject ghost)
  {
    Undo.StartTracking();
    var children = HammerHelper.GetChildren(ghost);
    ValheimRAFT.Handle(children);
    for (var i = 0; i < children.Count; i++)
    {
      var ghostChild = children[i];
      var hash = GetPrefab(i);
      if (ValheimRAFT.IsRaft(hash)) continue;
      var prefab = ZNetScene.instance.GetPrefab(hash);
      if (prefab)
      {
        DataHelper.Init(hash, ghostChild.transform, GetData(i));
        var childObj = UnityEngine.Object.Instantiate(prefab, ghostChild.transform.position, ghostChild.transform.rotation);
        PostProcessPlaced(childObj);
      }
    }
    Undo.StopTracking();
  }

  public GameObject AddObject(ZNetView view, Vector3 pos)
  {
    if (Objects.Count == 1)
      ToMulti();
    var obj = HammerHelper.SafeInstantiate(view, SelectedPrefab);
    obj.transform.rotation = view.transform.rotation;
    obj.transform.localPosition = pos;
    if (Configuration.AllSnapPoints) AddSnapPoints(obj);
    Objects.Add(new SelectedObject(Objects[0].Prefab, Objects[0].Scalable, Objects[0].Data));
    return obj;
  }
  private void ToMulti()
  {
    var obj = SelectedPrefab;
    SelectedPrefab = new GameObject();
    // Prevents children from disappearing.
    SelectedPrefab.SetActive(false);
    SelectedPrefab.transform.position = obj.transform.position;
    SelectedPrefab.transform.rotation = obj.transform.rotation;
    obj.transform.parent = SelectedPrefab.transform;
    obj.transform.localScale = Vector3.one;
    obj.SetActive(true);
    AddSnapPoints(obj);
    if (obj.TryGetComponent<Piece>(out var piece))
    {
      var name2 = Utils.GetPrefabName(obj);
      var prefab = ZNetScene.instance.GetPrefab(name2);
      if (prefab && !prefab.GetComponent<Piece>())
        UnityEngine.Object.Destroy(piece);
    }
  }
  public void RemoveObject(GameObject obj)
  {
    if (Objects.Count == 1)
      return;
    obj.SetActive(false);
    obj.transform.parent = null;
    UnityEngine.Object.Destroy(obj);
    Objects.RemoveAt(Objects.Count - 1);
    if (Objects.Count == 1)
      ToSingle();
  }
  private void ToSingle()
  {
    var obj = SelectedPrefab.transform.GetChild(0).gameObject;
    obj.SetActive(false);
    HammerHelper.EnsurePiece(obj);
    obj.transform.parent = null;
    UnityEngine.Object.Destroy(SelectedPrefab);
    SelectedPrefab = obj;
    Objects = Objects.Take(1).ToList();
  }
  public override void Activate()
  {
    base.Activate();
    Scaling.Set(SelectedPrefab);
  }
}