using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;
public enum SelectedType {
  Default,
  Object,
  Location,
  Multiple,
  Command,
}

public class SelectedObject {
  public string Prefab = "";
  public ZDO? Data;
  public bool Scalable;
  public SelectedObject(string name, bool scalable, ZDO? data) {
    Prefab = name;
    Scalable = scalable;
    Data = data == null ? data : data.Clone();
  }
}

public static class Selection {
  public static Dictionary<string, Selected> Selections = new();
  public static Selected? Get() => Selections.TryGetValue(Helper.GetTool(), out var selection) ? selection : null;
  public static Selected GetOrAdd() {
    var tool = Helper.GetTool();
    if (Selections.ContainsKey(tool))
      return Selections[tool];
    Selected selected = new();
    Selections[tool] = selected;
    return selected;
  }

#nullable disable
  public static GameObject Ghost => Get()?.Ghost;
#nullable enable
  public static String Command => Get()?.Command ?? "";
  public static SelectedType Type => Get()?.Type ?? SelectedType.Default;
  public static RulerParameters RulerParameters => Get()?.RulerParameters ?? new();
  public static List<SelectedObject> Objects => Get()?.Objects ?? new();

  public static void Clear() => Get()?.Clear();
  public static void Mirror() => Get()?.Mirror();
  public static void Postprocess(Vector3? scale) => Get()?.Postprocess(scale);
  public static ZDO? GetData(int index = 0) => Get()?.GetData(index);
  public static bool IsCommand() => Get()?.Type == SelectedType.Command;
  public static GameObject Set(string name) => GetOrAdd().Set(name);
  public static GameObject Set(ZNetView view) => GetOrAdd().Set(view);
  public static GameObject Set(IEnumerable<ZNetView> views) => GetOrAdd().Set(views);
  public static GameObject Set(RulerParameters ruler, string name, string description, string command, Sprite? icon) => GetOrAdd().Set(ruler, name, description, command, icon);
  public static GameObject Set(ZoneSystem.ZoneLocation location, int seed) => GetOrAdd().Set(location, seed);
  public static GameObject Set(Terminal terminal, Blueprint bp, Vector3 scale) => GetOrAdd().Set(terminal, bp, scale);
}

public class Selected {

#nullable disable
  ///<summary>Copy of the selected entity. Only needed for the placement ghost because armor and item stands have a different model depending on their state.</summary>
  public GameObject Ghost = null;
#nullable enable
  public SelectedType Type = SelectedType.Default;
  public List<SelectedObject> Objects = new();
  public string Command = "";
  public RulerParameters RulerParameters = new();
  public ZDO? GetData(int index = 0) {
    if (Objects.Count <= index) return null;
    return Objects[index].Data?.Clone();
  }
  public void Clear() {
    if (Ghost) ZNetScene.instance.Destroy(Ghost);
    Ghost = null;
    Type = SelectedType.Default;
    RulerParameters = new();
    Ruler.Remove();
    Command = "";
    Objects.Clear();
  }
  public GameObject Set(string name) {
    var prefab = ZNetScene.instance.GetPrefab(name);
    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Configuration.AllObjects && !Helper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    Clear();
    Type = SelectedType.Object;
    Ghost = Helper.SafeInstantiate(prefab);
    Objects.Add(new(name, prefab.GetComponent<ZNetView>().m_syncInitialScale, null));
    return Ghost;
  }
  private static void ResetColliders(GameObject obj, GameObject original) {
    var colliders = obj.GetComponentsInChildren<Collider>();
    var originalColliders = original.GetComponentsInChildren<Collider>();
    if (colliders.Length != originalColliders.Length) {
      InfinityHammer.Log.LogWarning("Object has a different amount of colliders than the original: Unable to reset colliders.");
      return;
    }
    for (var i = 0; i < colliders.Length; i++) {
      colliders[i].enabled = originalColliders[i].enabled;
      colliders[i].isTrigger = originalColliders[i].isTrigger;
    }
  }
  private static void SetScale(GameObject obj, Vector3? scale) {
    if (obj.GetComponent<ZNetView>() is not { } view) return;
    if (view.m_syncInitialScale)
      obj.transform.localScale = scale ?? view.gameObject.transform.localScale;
  }
  private static void SetLevel(GameObject obj, int level) {
    if (level == -1) return;
    if (obj.GetComponent<Character>() is not { } character) return;
    if (obj.GetComponentInChildren<LevelEffects>() is not { } effect) return;
    effect.m_character = character;
    effect.SetupLevelVisualization(level);
  }
  private static float Convert(int value, float defaultValue) {
    if (value == 0) return 0.1f;
    if (value == 1) return 0.5f;
    if (value == 2) return 1f;
    return defaultValue;
  }
  private static void SetWear(GameObject obj, int wear) {
    if (wear == -1) return;
    if (obj.GetComponent<WearNTear>() is not { } wearNTear) return;
    wearNTear.SetHealthVisual(Convert(wear, 1f), false);
  }
  private static void SetGrowth(GameObject obj, int growth) {
    if (growth == -1) return;
    if (obj.GetComponent<Plant>() is not { } plant) return;
    var healthy = growth == 0;
    var unhealthy = growth == 1;
    var healthyGrown = growth == 2;
    var unhealthyGrown = growth == 3;
    if (plant.m_healthyGrown) {
      plant.m_healthy.SetActive(healthy);
      plant.m_unhealthy.SetActive(unhealthy);
      plant.m_healthyGrown.SetActive(healthyGrown);
      plant.m_unhealthyGrown.SetActive(unhealthyGrown);
    } else {
      plant.m_healthy.SetActive(healthy || healthyGrown);
      plant.m_unhealthy.SetActive(unhealthy || unhealthyGrown);
    }
  }
  private void Postprocess(GameObject obj, ZDO? zdo, Vector3? scale) {
    SetScale(obj, scale);
    if (zdo == null) return;
    SetLevel(obj, zdo.GetInt(Hash.Level, -1));
    SetGrowth(obj, zdo.GetInt(Hash.Growth, -1));
    SetWear(obj, zdo.GetInt(Hash.Wear, -1));
  }
  public void Postprocess(Vector3? scale) {
    if (Type == SelectedType.Object) {
      Postprocess(Ghost, GetData(), scale);
      Scaling.Get().SetScale(Ghost.transform.localScale);
      Helper.EnsurePiece(Ghost);
    }
    if (Type == SelectedType.Multiple) {
      var i = 0;
      foreach (Transform tr in Ghost.transform)
        Postprocess(tr.gameObject, GetData(i++), scale);
    }
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public GameObject Set(ZNetView view) {
    var name = Utils.GetPrefabName(view.gameObject);
    var originalPrefab = ZNetScene.instance.GetPrefab(name);
    var prefab = Configuration.CopyState ? view.gameObject : originalPrefab;
    var data = Configuration.CopyState ? view.GetZDO() : null;

    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Configuration.AllObjects && !Helper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    Clear();
    Type = SelectedType.Object;
    Ghost = Helper.SafeInstantiate(prefab);
    ResetColliders(Ghost, originalPrefab);
    Objects.Add(new(name, view.m_syncInitialScale, data));
    Rotating.UpdatePlacementRotation(view.gameObject);
    return Ghost;
  }
  public GameObject Set(IEnumerable<ZNetView> views) {
    if (views.Count() == 1)
      return Set(views.First());
    Clear();
    Ghost = new GameObject();
    // Prevents children from disappearing.
    Ghost.SetActive(false);
    Ghost.name = "Multiple";
    Ghost.transform.position = views.First().transform.position;
    var piece = Ghost.AddComponent<Piece>();
    piece.m_name = "Multiple";
    piece.m_description = "";
    ZNetView.m_forceDisableInit = true;
    foreach (var view in views) {
      var name = Utils.GetPrefabName(view.gameObject);
      var originalPrefab = ZNetScene.instance.GetPrefab(name);
      var prefab = Configuration.CopyState ? view.gameObject : originalPrefab;
      var data = Configuration.CopyState ? view.GetZDO() : null;
      var obj = Helper.SafeInstantiate(prefab, Ghost);
      obj.SetActive(true);
      obj.transform.position = view.transform.position;
      obj.transform.rotation = view.transform.rotation;
      ResetColliders(obj, originalPrefab);
      var zdo = SetData(obj, "", data);
      Objects.Add(new SelectedObject(name, view.m_syncInitialScale, zdo));
    }
    ZNetView.m_forceDisableInit = false;
    Type = SelectedType.Multiple;
    Rotating.UpdatePlacementRotation(Ghost);
    return Ghost;
  }
  public void Mirror() {
    foreach (Transform item in Ghost.transform) {
      item.localPosition = new(-item.localPosition.x, item.localPosition.y, item.localPosition.z);
      var angles = item.localEulerAngles;
      item.localRotation = Quaternion.Euler(angles.x, -angles.y, angles.z);
    }
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public GameObject Set(RulerParameters ruler, string name, string description, string command, Sprite? icon) {
    Clear();
    var player = Helper.GetPlayer();
    RulerParameters = ruler;
    Command = command.Replace("hammer_command ", "").Replace("hoe_command ", "");
    Ghost = new GameObject();
    Ghost.name = name;
    var piece = Ghost.AddComponent<Piece>();
    piece.m_name = name;
    piece.m_description = description.Replace("\\n", "\n");
    piece.m_icon = icon;
    piece.m_clipEverything = true;
    Helper.GetPlayer().SetupPlacementGhost();
    Type = SelectedType.Command;
    Ruler.Create(ruler);
    return Ghost;
  }
  public GameObject Set(ZoneSystem.ZoneLocation location, int seed) {
    if (location == null) throw new InvalidOperationException("Location not found.");
    if (!location.m_prefab) throw new InvalidOperationException("Invalid location");
    Clear();
    Ghost = Helper.SafeInstantiateLocation(location, Hammer.AllLocationsObjects ? null : seed);
    Helper.EnsurePiece(Ghost);
    ZDO data = new();
    data.Set(Hash.Location, location.m_prefab.name.GetStableHashCode());
    data.Set(Hash.Seed, seed);
    Objects.Add(new(location.m_prefab.name, false, data));
    Helper.GetPlayer().SetupPlacementGhost();
    Type = SelectedType.Location;
    return Ghost;
  }
  private static ZDO? SetData(GameObject obj, string data, ZDO? zdo) {
    if (obj.GetComponent<Sign>() is { } sign) {
      zdo ??= new();
      if (data == "")
        data = zdo.GetString(Hash.Text, data);
      else
        zdo.Set(Hash.Text, data);
      sign.m_textWidget.text = data;
    }
    if (obj.GetComponent<TeleportWorld>() && data != "") {
      zdo ??= new();
      zdo.Set(Hash.Tag, data);
    }
    if (obj.GetComponent<Tameable>() && data != "") {
      zdo ??= new();
      zdo.Set(Hash.TamedName, data);
    }
    if (obj.GetComponent<ItemStand>() is { } itemStand) {
      zdo ??= new();
      var split = data.Split(':');
      var name = split[0];
      var variant = Parse.TryInt(split, 1, 0);
      if (data == "") {
        name = zdo.GetString(Hash.Item, name);
        variant = zdo.GetInt(Hash.Variant, variant);
      } else {
        zdo.Set(Hash.Item, name);
        zdo.Set(Hash.Variant, variant);
      }
      itemStand.SetVisualItem(name, variant);
    }
    if (obj.GetComponent<ArmorStand>() is { } armorStand) {
      zdo ??= new();
      var split = data.Split(':');
      var pose = Parse.TryInt(split, 0, 0);
      if (data == "")
        pose = zdo.GetInt(Hash.Pose, pose);
      else
        zdo.Set(Hash.Pose, pose);
      armorStand.m_pose = pose;
      armorStand.m_poseAnimator.SetInteger("Pose", pose);
      SetItemHack.Hack = true;
      for (var i = 0; i < armorStand.m_slots.Count; i++) {
        var name = Parse.TryString(split, i * 2 + 2, "");
        var variant = Parse.TryInt(split, i * 2 + 3, 0);
        if (data == "") {
          name = zdo.GetString($"{i}_item", name);
          variant = zdo.GetInt($"{i}_variant", variant);
        } else {
          zdo.Set($"{i}_item", name);
          zdo.Set($"{i}_variant", variant);
        }
        armorStand.SetVisualItem(i, name, variant);
      }
      SetItemHack.Hack = false;

    }
    return zdo;
  }
  public GameObject Set(Terminal terminal, Blueprint bp, Vector3 scale) {
    Clear();
    Ghost = new GameObject();
    // Prevents children from disappearing.
    Ghost.SetActive(false);
    Ghost.name = bp.Name;
    Ghost.transform.localScale = scale;
    var piece = Ghost.AddComponent<Piece>();
    piece.m_name = bp.Name;
    piece.m_description = bp.Description;
    ZNetView.m_forceDisableInit = true;
    foreach (var item in bp.Objects) {
      try {
        var obj = Helper.SafeInstantiate(item.Prefab, Ghost);
        obj.SetActive(true);
        obj.transform.localPosition = item.Pos;
        obj.transform.localRotation = item.Rot;
        obj.transform.localScale = item.Scale;
        item.Data = SetData(obj, item.ExtraInfo, item.Data);
        Objects.Add(new SelectedObject(item.Prefab, obj.GetComponent<ZNetView>().m_syncInitialScale, item.Data));

      } catch (InvalidOperationException e) {
        Helper.AddMessage(terminal, $"Warning: {e.Message}");
      }
    }
    foreach (var position in bp.SnapPoints) {
      GameObject obj = new() {
        name = "_snappoint",
        layer = LayerMask.NameToLayer("piece"),
        tag = "snappoint"
      };
      obj.SetActive(false);
      UnityEngine.Object.Instantiate(obj, position, Quaternion.identity, Ghost.transform);
    }
    ZNetView.m_forceDisableInit = false;
    Scaling.Get().SetScale(Ghost.transform.localScale);
    Helper.GetPlayer().SetupPlacementGhost();
    Type = SelectedType.Multiple;
    return Ghost;
  }
}
///<summary>Removes resource usage.</summary>
[HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetItem))]
public class SetItemHack {
  public static bool Hack = false;

  static void SetItem(VisEquipment obj, VisSlot slot, string name, int variant) {
    switch (slot) {
      case VisSlot.HandLeft:
        obj.m_leftItem = name;
        obj.m_leftItemVariant = variant;
        return;
      case VisSlot.HandRight:
        obj.m_rightItem = name;
        return;
      case VisSlot.BackLeft:
        obj.m_leftBackItem = name;
        obj.m_leftBackItemVariant = variant;
        return;
      case VisSlot.BackRight:
        obj.m_rightBackItem = name;
        return;
      case VisSlot.Chest:
        obj.m_chestItem = name;
        return;
      case VisSlot.Legs:
        obj.m_legItem = name;
        return;
      case VisSlot.Helmet:
        obj.m_helmetItem = name;
        return;
      case VisSlot.Shoulder:
        obj.m_shoulderItem = name;
        obj.m_shoulderItemVariant = variant;
        return;
      case VisSlot.Utility:
        obj.m_utilityItem = name;
        return;
      case VisSlot.Beard:
        obj.m_beardItem = name;
        return;
      case VisSlot.Hair:
        obj.m_hairItem = name;
        return;
    }
  }
  static bool Prefix(VisEquipment __instance, VisSlot slot, string name, int variant) {
    if (Hack)
      SetItem(__instance, slot, name, variant);
    return !Hack;
  }
}