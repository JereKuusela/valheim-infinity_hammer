
using System;
using System.Collections.Generic;
using System.Linq;
using InfinityHammer;
using UnityEngine;

namespace Service;
public enum ObjectType
{
  All,
  Character,
  Structure
}
public class Hovered
{
  public ZNetView Obj;
  public int Index;
  public Hovered(ZNetView obj, int index)
  {
    Obj = obj;
    Index = index;
  }
}
public static class Selector
{
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZNetView view) => view && IsValid(view.GetZDO());
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZDO zdo) => zdo != null && zdo.IsValid();

  ///<summary>Returns the hovered object.</summary>
  public static ZNetView? GetHovered(float range, List<string> ignoredIds)
  {
    var hovered = GetHovered(Player.m_localPlayer, range, ignoredIds);
    if (hovered == null) return null;
    return hovered.Obj;
  }

  public static Hovered? GetHovered(Player obj, float maxDistance, List<string> ignoredIds, bool allowOtherPlayers = false)
  {
    var ignoredPrefabs = GetIgnoredPrefabs(ignoredIds);
    var raycast = Math.Max(maxDistance + 5f, 50f);
    var mask = LayerMask.GetMask(new string[]
    {
      "item",
      "piece",
      "piece_nonsolid",
      "Default",
      "static_solid",
      "Default_small",
      "character",
      "character_net",
      "terrain",
      "vehicle",
      "character_trigger" // Added to remove spawners with ESP mod.
    });
    var hits = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, raycast, mask);
    Array.Sort<RaycastHit>(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
    foreach (var hit in hits)
    {
      if (Vector3.Distance(hit.point, obj.m_eye.position) >= maxDistance) continue;
      var netView = hit.collider.GetComponentInParent<ZNetView>();
      if (!IsValid(netView)) continue;
      if (ignoredPrefabs.Contains(netView.GetZDO().GetPrefab())) continue;
      if (hit.collider.GetComponent<EffectArea>()) continue;
      var player = netView.GetComponentInChildren<Player>();
      if (player == obj) continue;
      if (!allowOtherPlayers && player) continue;
      var mineRock = netView.GetComponent<MineRock5>();
      var index = 0;
      if (mineRock)
        index = mineRock.GetAreaIndex(hit.collider);
      return new(netView, index);
    }
    return null;
  }
  private static float GetX(float x, float y, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * y;
  private static float GetY(float x, float y, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
  public static bool Within(Vector3 position, Vector3 center, float angle, float width, float depth, float height)
  {
    var dx = position.x - center.x;
    var dz = position.z - center.z;
    var distanceX = GetX(dx, dz, angle);
    var distanceZ = GetY(dx, dz, angle);
    if (center.y - position.y > 1000f) return false;
    if (position.y - center.y > (height == 0f ? 1000f : height)) return false;
    if (Mathf.Abs(distanceX) > width) return false;
    if (Mathf.Abs(distanceZ) > depth) return false;
    return true;
  }
  public static bool Within(Vector3 position, Vector3 center, float radius, float height)
  {
    return Utils.DistanceXZ(position, center) <= radius && center.y - position.y < 1000f && position.y - center.y <= (height == 0f ? 1000f : height);
  }
  private static bool IsIncluded(string id, string name)
  {
    if (id.StartsWith("*", StringComparison.Ordinal) && id.EndsWith("*", StringComparison.Ordinal))
      return name.Contains(id.Substring(1, id.Length - 3));
    if (id.StartsWith("*", StringComparison.Ordinal)) return name.EndsWith(id.Substring(1), StringComparison.Ordinal);
    if (id.EndsWith("*", StringComparison.Ordinal)) return name.StartsWith(id.Substring(0, id.Length - 2), StringComparison.Ordinal);
    return id == name;
  }
  private static HashSet<int> GetIgnoredPrefabs(List<string> ids)
  {
    HashSet<int> prefabs = new();
    foreach (var id in ids)
      prefabs.UnionWith(GetIgnoredPrefabs(id));
    return prefabs;
  }
  private static HashSet<int> GetIgnoredPrefabs(string id)
  {
    if (id == "") return new();
    id = id.ToLower();
    return ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => IsIncluded(id, prefab.name.ToLower()))
      .Select(prefab => prefab.name.GetStableHashCode())
      .ToHashSet();
  }
  public static HashSet<int> GetPrefabs(string id)
  {
    id = id.ToLower();
    IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
    values = values.Where(prefab => prefab.name != "Player");
    if (id == "*" || id == "")
      values = values.Where(prefab => !prefab.name.StartsWith("_", StringComparison.Ordinal));
    else if (id.Contains("*"))
      values = values.Where(prefab => IsIncluded(id, prefab.name.ToLower()));
    else
      values = values.Where(prefab => prefab.name.ToLower() == id);
    return values.Select(prefab => prefab.name.GetStableHashCode()).ToHashSet();
  }
  public static ZNetView[] GetNearby(string id, ObjectType type, List<string> ignoredIds, Vector3 center, float radius, float height)
  {
    var ignoredPrefabs = GetIgnoredPrefabs(ignoredIds);
    var checker = (Vector3 pos) => Within(pos, center, radius, height);
    return GetNearby(id, type, ignoredPrefabs, checker);
  }
  public static ZNetView[] GetNearby(string id, ObjectType type, List<string> ignoredIds, Vector3 center, float angle, float width, float depth, float height)
  {
    var ignoredPrefabs = GetIgnoredPrefabs(ignoredIds);
    var checker = (Vector3 pos) => Within(pos, center, angle, width, depth, height);
    return GetNearby(id, type, ignoredPrefabs, checker);
  }
  public static ZNetView[] GetNearby(string id, ObjectType type, HashSet<int> ignoredPrefabs, Func<Vector3, bool> checker)
  {
    var codes = GetPrefabs(id);
    var scene = ZNetScene.instance;
    var objects = ZNetScene.instance.m_instances.Values;
    var views = objects.Where(IsValid);
    views = views.Where(view => codes.Contains(view.GetZDO().GetPrefab()));
    views = views.Where(view => !ignoredPrefabs.Contains(view.GetZDO().GetPrefab()));
    views = views.Where(view => checker(view.GetZDO().GetPosition()));
    if (type == ObjectType.Structure)
      views = views.Where(view => scene.GetPrefab(view.GetZDO().GetPrefab()).GetComponent<Piece>());
    if (type == ObjectType.Character)
      views = views.Where(view => scene.GetPrefab(view.GetZDO().GetPrefab()).GetComponent<Character>());
    var objs = views.ToArray();
    if (objs.Length == 0) throw new InvalidOperationException("Nothing is nearby.");
    return objs;
  }

  ///<summary>Returns connected WearNTear objects.</summary>
  public static ZNetView[] GetConnected(ZNetView baseView, List<string> ignoredIds)
  {
    var ignoredPrefabs = GetIgnoredPrefabs(ignoredIds);
    if (ValheimRAFT.IsInRaft(baseView)) return ValheimRAFT.GetConnectedRaft(baseView, ignoredPrefabs);
    var baseWear = baseView.GetComponent<WearNTear>();
    if (baseWear == null) throw new InvalidOperationException("Connected doesn't work for this object.");
    HashSet<ZNetView> views = new() { baseView };
    Queue<WearNTear> todo = new();
    todo.Enqueue(baseWear);
    while (todo.Count > 0)
    {
      var wear = todo.Dequeue();
      if (wear.m_colliders == null) wear.SetupColliders();
      foreach (var boundData in wear.m_bounds)
      {
        var boxes = Physics.OverlapBoxNonAlloc(boundData.m_pos, boundData.m_size, WearNTear.m_tempColliders, boundData.m_rot, WearNTear.m_rayMask);
        for (int i = 0; i < boxes; i++)
        {
          var collider = WearNTear.m_tempColliders[i];
          if (collider.isTrigger || collider.attachedRigidbody != null || wear.m_colliders.Contains(collider)) continue;
          var wear2 = collider.GetComponentInParent<WearNTear>();
          if (!wear2 || !IsValid(wear2.m_nview)) continue;
          if (ignoredPrefabs.Contains(wear2.m_nview.GetZDO().GetPrefab())) continue;
          if (views.Contains(wear2.m_nview)) continue;
          views.Add(wear2.m_nview);
          todo.Enqueue(wear2);
        }
      }
    }
    return views.ToArray();
  }
}