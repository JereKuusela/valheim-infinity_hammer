

using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;

public static class Snapping
{
  public static bool IsSnapPoint(GameObject obj) => obj && obj.CompareTag("snappoint");
  public static bool IsSnapPoint(Transform tr) => IsSnapPoint(tr.gameObject);
  public static List<GameObject?> GetChildren(GameObject? obj)
  {
    List<GameObject?> children = [];
    foreach (Transform tr in obj.transform)
    {
      if (IsSnapPoint(tr)) continue;
      children.Add(tr.gameObject);
    }
    return children;
  }
  public static List<GameObject> GetSnapPoints(GameObject? obj)
  {
    List<GameObject> snapPoints = [];
    foreach (Transform tr in obj.transform)
    {
      if (IsSnapPoint(tr)) snapPoints.Add(tr.gameObject);
    }
    return snapPoints;
  }
  public static int CountActiveChildren(GameObject obj)
  {
    var count = 0;
    foreach (Transform tr in obj.transform)
    {
      if (tr.gameObject.activeSelf && !IsSnapPoint(tr.gameObject)) count++;
    }
    return count;
  }
  public static int CountSnapPoints(GameObject obj)
  {
    var count = 0;
    foreach (Transform tr in obj.transform)
    {
      if (IsSnapPoint(tr.gameObject)) count++;
    }
    return count;
  }

  public static void CreateSnapPoint(GameObject? parent, Vector3 pos, string name)
  {
    GameObject snapObj = new()
    {
      name = name,
      layer = LayerMask.NameToLayer("piece"),
      tag = "snappoint",
    };
    snapObj.SetActive(false);
    snapObj.transform.parent = parent.transform;
    snapObj.transform.localPosition = pos;
    snapObj.transform.localRotation = Quaternion.identity;
  }
  public static void CreateSnapPoints(GameObject? obj, List<Vector3> points)
  {
    int counter = 0;
    foreach (var point in points)
      CreateSnapPoint(obj, point, $"Snap {++counter}");
  }

  public static void RegenerateSnapPoints(GameObject? obj)
  {
    RemoveSnapPoints(obj);
    GenerateSnapPoints(obj);
  }
  public static void RemoveSnapPoints(GameObject? obj)
  {
    foreach (Transform tr in obj.transform)
    {
      if (IsSnapPoint(tr)) Object.Destroy(tr.gameObject);
    }
  }
  public static void BuildSnaps(GameObject obj)
  {
    if (!Configuration.Dimensions.TryGetValue(Utils.GetPrefabName(obj).ToLower(), out var extends))
      return;
    var size = extends / 2;
    var useEdges = Configuration.Snapping == SnappingMode.Edges || Configuration.Snapping == SnappingMode.All;
    // Assume origin point at bottom center.
    List<Vector3> edges = useEdges ? [new(size.x, size.y, 0), new(-size.x, size.y, 0), new(0, extends.y, 0), new(0, 0, 0), new(0, size.y, size.z), new(0, size.y, -size.z)] : [];
    var useCorners = Configuration.Snapping == SnappingMode.Corners || Configuration.Snapping == SnappingMode.All;
    if (size.x < 0.5f) size.x = 0.0f;
    if (size.y < 0.5f) size.y = 0.0f;
    if (size.z < 0.5f) size.z = 0.0f;
    List<Vector3> corners = useCorners ? [new(size.x, 0, size.z), new(size.x, 0, -size.z), new(size.x, extends.y, size.z), new(size.x, extends.y, -size.z), new(-size.x, 0, size.z), new(-size.x, 0, -size.z), new(-size.x, extends.y, size.z), new(-size.x, extends.y, -size.z)] : [];
    List<Vector3> points = UniquePoints([.. edges, .. corners]);
    CreateSnapPoints(obj.gameObject, points);
  }
  public static void GenerateSnapPoints(GameObject? obj) => CreateSnapPoints(obj, GenerateSnapPoints(GetChildren(obj)));
  public static List<Vector3> GenerateSnapPoints(List<GameObject?> objects)
  {
    if (objects.Count == 0) return [];
    if (Configuration.Snapping == SnappingMode.Off)
    {
      List<Vector3> snapPoints = [];
      GetSnapPoints(objects[0], snapPoints);
      return snapPoints;
    }
    var points = FindOuterPoints(SearchSnapPoints(objects));
    if (points.Count == 0) return points;
    if (Configuration.Snapping == SnappingMode.Corners) return CornerSnap(points);
    if (Configuration.Snapping == SnappingMode.Edges) return EdgeSnap(points);
    return points;
  }

  private static List<Vector3> SearchSnapPoints(List<GameObject?> objects)
  {
    List<Vector3> snapPoints = [];
    foreach (var child in objects)
    {
      if (IsSnapPoint(child)) continue;
      GetSnapPoints(child, snapPoints);
    }
    return snapPoints;
  }
  private static void GetSnapPoints(GameObject? obj, List<Vector3> snapPoints)
  {
    for (int c = 0; c < obj.transform.childCount; c++)
    {
      var snap = obj.transform.GetChild(c);
      if (IsSnapPoint(snap))
      {
        var pos = obj.transform.localPosition + obj.transform.localRotation * snap.localPosition;
        snapPoints.Add(pos);
      }
    }
  }

  private static List<Vector3> FindOuterPoints(List<Vector3> snapPoints)
  {
    if (snapPoints.Count == 0) return [];
    float left = float.MaxValue;
    float right = float.MinValue;
    float front = float.MaxValue;
    float back = float.MinValue;
    float top = float.MinValue;
    float bottom = float.MaxValue;
    List<Vector3> lefts = [];
    List<Vector3> rights = [];
    List<Vector3> fronts = [];
    List<Vector3> backs = [];
    List<Vector3> tops = [];
    List<Vector3> bottoms = [];
    foreach (var pos in snapPoints)
    {
      if (Helper.Approx(pos.x, left))
      {
        lefts.Add(pos);
      }
      else if (pos.x < left)
      {
        left = pos.x;
        lefts = [pos];
      }

      if (Helper.Approx(pos.x, right))
      {
        rights.Add(pos);
      }
      else if (pos.x > right)
      {
        right = pos.x;
        rights = [pos];
      }

      if (Helper.Approx(pos.z, front))
      {
        fronts.Add(pos);
      }
      else if (pos.z < front)
      {
        front = pos.z;
        fronts = [pos];
      }

      if (Helper.Approx(pos.z, back))
      {
        backs.Add(pos);
      }
      else if (pos.z > back)
      {
        back = pos.z;
        backs = [pos];
      }

      if (Helper.Approx(pos.y, top))
      {
        tops.Add(pos);
      }
      else if (pos.y > top)
      {
        top = pos.y;
        tops = [pos];
      }

      if (Helper.Approx(pos.y, bottom))
      {
        bottoms.Add(pos);
      }
      else if (pos.y < bottom)
      {
        bottom = pos.y;
        bottoms = [pos];
      }
    }
    return UniquePoints([.. lefts, .. rights, .. fronts, .. backs, .. tops, .. bottoms]);
  }

  private static List<Vector3> CornerSnap(List<Vector3> snapPoints)
  {
    var center = FindCenter(snapPoints);
    var points = snapPoints.Select(p => System.Tuple.Create(p - center, p)).ToList();
    var corner1 = points.OrderBy(p => p.Item1.x + p.Item1.y + p.Item1.z).First().Item2;
    var corner2 = points.OrderBy(p => p.Item1.x + p.Item1.y - p.Item1.z).First().Item2;
    var corner3 = points.OrderBy(p => p.Item1.x - p.Item1.y + p.Item1.z).First().Item2;
    var corner4 = points.OrderBy(p => p.Item1.x - p.Item1.y - p.Item1.z).First().Item2;
    var corner5 = points.OrderBy(p => -p.Item1.x + p.Item1.y + p.Item1.z).First().Item2;
    var corner6 = points.OrderBy(p => -p.Item1.x + p.Item1.y - p.Item1.z).First().Item2;
    var corner7 = points.OrderBy(p => -p.Item1.x - p.Item1.y + p.Item1.z).First().Item2;
    var corner8 = points.OrderBy(p => -p.Item1.x - p.Item1.y - p.Item1.z).First().Item2;
    return UniquePoints([corner1, corner2, corner3, corner4, corner5, corner6, corner7, corner8]);
  }
  // Goal is to get the farthest point on each edge. This is not perfect but should work for most cases.
  private static List<Vector3> EdgeSnap(List<Vector3> snapPoints)
  {
    // First to collect furthest points on each edge.
    float left = float.MaxValue;
    float right = float.MinValue;
    float front = float.MaxValue;
    float back = float.MinValue;
    float top = float.MinValue;
    float bottom = float.MaxValue;
    List<Vector3> leftPoints = [];
    List<Vector3> rightPoints = [];
    List<Vector3> frontPoints = [];
    List<Vector3> backPoints = [];
    List<Vector3> topPoints = [];
    List<Vector3> bottomPoints = [];
    foreach (var pos in snapPoints)
    {
      if (Helper.Approx(pos.x, left))
        leftPoints.Add(pos);
      else if (pos.x < left)
      {
        left = pos.x;
        leftPoints = [pos];
      }

      if (Helper.Approx(pos.x, right))
        rightPoints.Add(pos);
      else if (pos.x > right)
      {
        right = pos.x;
        rightPoints = [pos];
      }

      if (Helper.Approx(pos.z, front))
        frontPoints.Add(pos);
      else if (pos.z < front)
      {
        front = pos.z;
        frontPoints = [pos];
      }

      if (Helper.Approx(pos.z, back))
        backPoints.Add(pos);
      else if (pos.z > back)
      {
        back = pos.z;
        backPoints = [pos];
      }

      if (Helper.Approx(pos.y, top))
        topPoints.Add(pos);
      else if (pos.y > top)
      {
        top = pos.y;
        topPoints = [pos];
      }

      if (Helper.Approx(pos.y, bottom))
        bottomPoints.Add(pos);
      else if (pos.y < bottom)
      {
        bottom = pos.y;
        bottomPoints = [pos];
      }
    }

    Vector3 leftPoint = FindCenter(leftPoints);
    Vector3 rightPoint = FindCenter(rightPoints);
    Vector3 frontPoint = FindCenter(frontPoints);
    Vector3 backPoint = FindCenter(backPoints);
    Vector3 topPoint = FindCenter(topPoints);
    Vector3 bottomPoint = FindCenter(bottomPoints);
    List<Vector3> points = [leftPoint, rightPoint, frontPoint, backPoint, topPoint, bottomPoint];
    // For two dimensional structures like walls two edges would get snap points middle of the wall.
    // So makes sense to ignore those edges.
    Bounds bounds = new();
    foreach (var pos in points)
      bounds.Encapsulate(pos);
    bool ignoreX = Helper.Approx(bounds.size.x, 0);
    bool ignoreZ = Helper.Approx(bounds.size.z, 0);
    bool ignoreY = Helper.Approx(bounds.size.y, 0);

    if (ignoreX) return UniquePoints([frontPoint, backPoint, topPoint, bottomPoint]);
    if (ignoreZ) return UniquePoints([leftPoint, rightPoint, topPoint, bottomPoint]);
    if (ignoreY) return UniquePoints([leftPoint, rightPoint, frontPoint, backPoint]);
    return UniquePoints([leftPoint, rightPoint, frontPoint, backPoint, topPoint, bottomPoint]);
  }

  private static List<Vector3> UniquePoints(List<Vector3> points)
  {
    // Very inefficient but not many snap points.
    List<Vector3> unique = [];
    foreach (var pos in points)
    {
      if (unique.Any(p => Helper.Approx(p.x, pos.x) && Helper.Approx(p.y, pos.y) && Helper.Approx(p.z, pos.z))) continue;
      unique.Add(pos);
    }
    return unique;
  }

  private static Vector3 FindCenter(List<Vector3> snapPoints)
  {
    Vector3 center = Vector3.zero;
    foreach (var pos in snapPoints)
      center += pos;
    center /= snapPoints.Count;
    return snapPoints.OrderBy(p => (p - center).sqrMagnitude).First();
  }
}