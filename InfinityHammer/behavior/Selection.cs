using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;
public enum SelectionType {
  Default,
  Object,
  Location,
  Multiple,
  Command,
}

public class SelectionObject {
  public string Prefab = "";
  public Vector3 Scale;
  public ZDO? Data;
  public SelectionObject(string name, Vector3 scale, ZDO? data) {
    Prefab = name;
    Scale = scale;
    Data = data == null ? data : data.Clone();
  }
}
public static class Selection {
  public static SelectionType Type = SelectionType.Default;
  public static List<SelectionObject> Objects = new();
  public static string Command = "";
  public static ZDO? GetData(int index = 0) {
    if (Objects.Count <= index) return null;
    return Objects[index].Data?.Clone();
  }
#nullable disable
  ///<summary>Copy of the selected entity. Only needed for the placement ghost because armor and item stands have a different model depending on their state.</summary>
  public static GameObject Ghost = null;
#nullable enable
  public static void Clear() {
    if (Ghost) ZNetScene.instance.Destroy(Ghost);
    Ghost = null;
    Type = SelectionType.Default;
    Ruler.Remove();
    Command = "";
    Objects.Clear();
  }
  public static GameObject Set(string name, Vector3? scale) {
    var prefab = ZNetScene.instance.GetPrefab(name);
    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Configuration.AllObjects && !Helper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    Clear();
    Type = SelectionType.Object;
    Ghost = Helper.SafeInstantiate(prefab);
    if (prefab.GetComponent<ZNetView>().m_syncInitialScale)
      Ghost.transform.localScale = scale ?? Vector3.one;
    Selection.Objects.Add(new(name, Ghost.transform.localScale, null));
    Scaling.Get()?.SetScale(Ghost.transform.localScale);
    Helper.EnsurePiece(Ghost);
    Helper.GetPlayer().SetupPlacementGhost();
    return Ghost;
  }
  public static GameObject Set(ZNetView view, Vector3? scale) {
    var name = Utils.GetPrefabName(view.gameObject);
    var prefab = Configuration.CopyState ? view.gameObject : ZNetScene.instance.GetPrefab(name);
    var data = Configuration.CopyState ? view.GetZDO() : null;

    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Configuration.AllObjects && !Helper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    Clear();
    Type = SelectionType.Object;
    Ghost = Helper.SafeInstantiate(prefab);
    if (view.m_syncInitialScale)
      Ghost.transform.localScale = scale ?? view.gameObject.transform.localScale;
    Objects.Add(new(name, Ghost.transform.localScale, data));
    Scaling.Get()?.SetScale(Ghost.transform.localScale);
    Helper.EnsurePiece(Ghost);
    Helper.GetPlayer().SetupPlacementGhost();
    Rotating.UpdatePlacementRotation(view.gameObject);
    return Ghost;
  }
  public static GameObject Set(IEnumerable<ZNetView> views, Vector3? scale) {
    if (views.Count() == 1)
      return Set(views.First(), scale);
    Clear();
    Ghost = new GameObject();
    // Prevents children from disappearing.
    Ghost.SetActive(false);
    Ghost.name = "Multiple";
    Ghost.transform.position = views.First().transform.position;
    Ghost.transform.rotation = views.First().transform.rotation;
    var piece = Ghost.AddComponent<Piece>();
    piece.m_name = "Multiple";
    piece.m_description = "";
    ZNetView.m_forceDisableInit = true;
    foreach (var item in views) {
      var name = Utils.GetPrefabName(item.gameObject);
      var obj = Helper.SafeInstantiate(name, Ghost);
      obj.SetActive(true);
      obj.transform.position = item.transform.position;
      obj.transform.rotation = item.transform.rotation;
      if (item.m_syncInitialScale)
        obj.transform.localScale = scale ?? item.transform.localScale;
      var zdo = SetData(obj, "", item.GetZDO());
      Objects.Add(new SelectionObject(name, obj.transform.localScale, zdo));
    }
    ZNetView.m_forceDisableInit = false;
    Helper.GetPlayer().SetupPlacementGhost();
    Type = SelectionType.Multiple;
    return Ghost;
  }
  public static void Mirror() {
    foreach (Transform item in Ghost.transform) {
      item.localPosition = new(-item.localPosition.x, item.localPosition.y, item.localPosition.z);
      var angles = item.localEulerAngles;
      item.localRotation = Quaternion.Euler(angles.x, -angles.y, angles.z);
    }
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public static GameObject Set(string name, string description, string command, Sprite? icon) {
    Clear();
    var player = Helper.GetPlayer();
    Command = command;
    Ghost = new GameObject();
    Ghost.name = name;
    var piece = Ghost.AddComponent<Piece>();
    piece.m_name = name;
    piece.m_description = description.Replace("\\n", "\n");
    piece.m_icon = icon;
    piece.m_clipEverything = true;
    Helper.GetPlayer().SetupPlacementGhost();
    Type = SelectionType.Command;
    return Ghost;
  }
  public static GameObject Set(ZoneSystem.ZoneLocation location, int seed) {
    if (location == null) throw new InvalidOperationException("Location not found.");
    if (!location.m_prefab) throw new InvalidOperationException("Invalid location");
    Clear();
    Ghost = Helper.SafeInstantiateLocation(location, Hammer.AllLocationsObjects ? null : seed);
    Helper.EnsurePiece(Ghost);
    ZDO data = new();
    data.Set("location", location.m_prefab.name.GetStableHashCode());
    data.Set("seed", seed);
    Objects.Add(new(location.m_prefab.name, Vector3.one, data));
    Helper.GetPlayer().SetupPlacementGhost();
    Type = SelectionType.Location;
    return Ghost;
  }
  private static ZDO? SetData(GameObject obj, string data, ZDO? zdo) {
    if (obj.GetComponent<Sign>() is { } sign) {
      zdo ??= new();
      if (data == "")
        data = zdo.GetString("text", data);
      else
        zdo.Set("text", data);
      sign.m_textWidget.text = data;
    }
    if (obj.GetComponent<TeleportWorld>() && data != "") {
      zdo ??= new();
      zdo.Set("tag", data);
    }
    if (obj.GetComponent<Tameable>() && data != "") {
      zdo ??= new();
      zdo.Set("TamedName", data);
    }
    if (obj.GetComponent<ItemStand>() is { } itemStand) {
      zdo ??= new();
      var split = data.Split(':');
      var name = split[0];
      var variant = Parse.TryInt(split, 1, 0);
      if (data == "") {
        name = zdo.GetString("item", name);
        variant = zdo.GetInt("item", variant);
      } else {
        zdo.Set("item", name);
        zdo.Set("variant", variant);
      }
      itemStand.SetVisualItem(name, variant);
    }
    if (obj.GetComponent<ArmorStand>() is { } armorStand) {
      zdo ??= new();
      var split = data.Split(':');
      var pose = Parse.TryInt(split, 0, 0);
      if (data == "")
        pose = zdo.GetInt("Pose", pose);
      else
        zdo.Set("pose", pose);
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
  public static GameObject Set(Terminal terminal, Blueprint bp) {
    Clear();
    Ghost = new GameObject();
    // Prevents children from disappearing.
    Ghost.SetActive(false);
    Ghost.name = bp.Name;
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
        Objects.Add(new SelectionObject(item.Prefab, item.Scale, item.Data));

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
      obj.transform.SetParent(obj.transform);
      obj.transform.localPosition = position;
    }
    ZNetView.m_forceDisableInit = false;
    Helper.GetPlayer().SetupPlacementGhost();
    Type = SelectionType.Multiple;
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