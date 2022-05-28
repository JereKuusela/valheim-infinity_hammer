using System;
using System.Collections.Generic;
using UnityEngine;
namespace InfinityHammer;
public enum SelectionType {
  Object,
  Location,
  Blueprint,
  Default
}
public class SelectionObject {
  public string Prefab = "";
  public Vector3 Scale;
  public ZDO? Data;
  public SelectionObject(string name, Vector3 scale, ZDO? data) {
    Prefab = name;
    Scale = scale;
    Data = data;
  }
}
public static class Selection {
  public static SelectionType Type = SelectionType.Default;
  public static List<SelectionObject> Objects = new();
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
    Objects.Clear();
  }
  public static GameObject Set(string name) {
    var prefab = ZNetScene.instance.GetPrefab(name);
    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Settings.AllObjects && !Helper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    Clear();
    Type = SelectionType.Object;
    Ghost = Helper.SafeInstantiate(prefab);
    Selection.Objects.Add(new(name, Vector3.one, null));
    Helper.EnsurePiece(Ghost);
    Helper.GetPlayer().SetupPlacementGhost();
    return Ghost;
  }
  public static GameObject Set(ZNetView view) {
    var name = Utils.GetPrefabName(view.gameObject);
    var prefab = Settings.CopyState ? view.gameObject : ZNetScene.instance.GetPrefab(name);
    var data = Settings.CopyState ? view.GetZDO() : null;

    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Settings.AllObjects && !Helper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    Clear();
    Type = SelectionType.Object;
    Ghost = Helper.SafeInstantiate(prefab);
    Ghost.transform.localScale = view.gameObject.transform.localScale;
    Objects.Add(new(name, view.gameObject.transform.localScale, data));
    Helper.EnsurePiece(Ghost);
    Helper.GetPlayer().SetupPlacementGhost();
    Rotating.UpdatePlacementRotation(view.gameObject);
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
    Type = SelectionType.Blueprint;
    return Ghost;
  }
}