using System;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using InfinityHammer;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityTools;

public class RulerParameters
{
  public bool Radius;
  public bool Ring;
  public bool Width;
  public bool Grid;
  public bool Depth;
  public bool Height;
  public bool RotateWithPlayer;
  public bool IsTargeted;
  public bool IsId;
}
public enum RulerShape
{
  Circle,
  Ring,
  Frame,
  Square,
  Rectangle,
}
public class Ruler
{
  public static RulerShape Shape = RulerShape.Circle;
  public static bool IsActive => Projector != null;
  public static GameObject? Projector = null;
  public static GameObject? Circle = null;
  public static GameObject? Ring = null;
  public static GameObject? Rectangle = null;
  public static GameObject? Square = null;
  public static GameObject? Frame = null;
  public static void SanityCheckShape()
  {
    if (Shape == RulerShape.Circle && (!Circle || !Configuration.ShapeCircle)) Shape = RulerShape.Ring;
    if (Shape == RulerShape.Ring && (!Ring || !Configuration.ShapeRing)) Shape = RulerShape.Square;
    if (Shape == RulerShape.Square && (!Square || !Configuration.ShapeSquare)) Shape = RulerShape.Frame;
    if (Shape == RulerShape.Frame && (!Frame || !Configuration.ShapeFrame)) Shape = RulerShape.Rectangle;
    if (Shape == RulerShape.Rectangle && (!Rectangle || !Configuration.ShapeRectangle)) Shape = RulerShape.Circle;
  }
  private static void SetTerrainGrid(float width, float height)
  {
    var decimalsW = width - Mathf.Floor(width);
    var decimalsH = height - Mathf.Floor(height);
    var centerX = 0.25f < decimalsW && decimalsW < 0.75f;
    var centerZ = 0.25f < decimalsH && decimalsH < 0.75f;
    Vector3 center = new Vector3(centerX ? 0.0f : 0.5f, 0f, centerZ ? 0.0f : 0.5f);
    Grid.SetPreciseMode(center);
  }
  private static void BuildCircle(GameObject obj, float radius)
  {
    var proj = obj.GetComponent<CircleRuler>();
    proj.Visible = obj.activeSelf;
    proj.Radius = radius;
  }
  private static void BuildRectangle(GameObject obj, float width, float depth)
  {
    var proj = obj.GetComponent<RectangleRuler>();
    proj.Visible = obj.activeSelf;
    proj.Width = width;
    proj.Depth = depth;
  }
  public static void Update()
  {
    var player = Player.m_localPlayer;
    if (Projector == null || !player) return;
    if (Selection.Get() is not ToolSelection selection) return;
    var ghost = player.m_placementGhost;
    var ptr = player.transform;
    Projector.SetActive(ghost);
    if (!ghost) return;
    var gtr = ghost.transform;
    var scale = Scaling.Get();
    var tool = selection.Tool;
    if (tool.IsTargetEdge)
    {
      Vector3 pos = new((ptr.position.x + gtr.position.x) / 2f, gtr.position.y, (ptr.position.z + gtr.position.z) / 2f);
      Projector.transform.position = pos;
    }
    else
      Projector.transform.position = gtr.position;
    var angle = gtr.rotation.eulerAngles.y;
    if (tool.RotateWithPlayer)
      angle = Vector3.SignedAngle(Vector3.forward, Utils.DirectionXZ(gtr.position - ptr.position), Vector3.up);
    if (selection.TerrainGrid)
      angle = 0;
    Projector.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    if (tool.IsTargetEdge)
    {
      var distance = Utils.DistanceXZ(ptr.position, gtr.position) / 2f;
      scale.SetScaleZ(distance);
    }
    SanityCheckShape();
    var shape = Shape;
    if (selection.TerrainGrid) scale.SetPrecisionXZ(0.5f, 0.5f);
    if (Circle != null)
    {
      Circle.SetActive(shape == RulerShape.Circle || shape == RulerShape.Ring);
      BuildCircle(Circle, scale.X);
      if (Circle.activeSelf)
      {
        if (selection.TerrainGrid)
          SetTerrainGrid(scale.X, scale.X);
      }
    }
    if (Circle != null && Ring != null)
    {
      Ring.SetActive(shape == RulerShape.Ring);
      BuildCircle(Ring, scale.Z);
    }
    if (Square != null)
    {
      Square.SetActive(shape == RulerShape.Square || shape == RulerShape.Frame);
      BuildRectangle(Square, scale.X, scale.X);
      if (Square.activeSelf)
      {
        if (selection.TerrainGrid)
          SetTerrainGrid(scale.X, scale.X);
      }
    }
    if (Square != null && Frame != null)
    {
      Frame.SetActive(shape == RulerShape.Frame);
      BuildRectangle(Frame, scale.Z, scale.Z);
      if (Frame.activeSelf)
      {
        if (selection.TerrainGrid)
          SetTerrainGrid(scale.Z, scale.Z);
      }
    }
    if (Rectangle != null)
    {
      Rectangle.SetActive(shape == RulerShape.Rectangle);
      BuildRectangle(Rectangle, scale.X, scale.Z);
      if (Rectangle.activeSelf)
      {
        if (selection.TerrainGrid)
          SetTerrainGrid(scale.X, scale.Z);
      }
    }
    if (tool.Highlight)
    {
      if (shape == RulerShape.Circle)
        HighlightCircle(Projector.transform.position, scale.X, scale.Y);
      if (shape == RulerShape.Ring)
        HighlightRing(Projector.transform.position, scale.X, scale.Z, scale.Y);
      if (shape == RulerShape.Square)
        HighlightRectangle(Projector.transform.position, angle, scale.X, scale.X, scale.Y);
      if (shape == RulerShape.Frame)
        HighlightFrame(Projector.transform.position, angle, scale.X, scale.Z, scale.Y);
      if (shape == RulerShape.Rectangle)
        HighlightRectangle(Projector.transform.position, angle, scale.X, scale.Z, scale.Y);

    }
    BaseRuler.SnapToGround = selection.Tool.SnapGround;
    BaseRuler.Offset = selection.Tool.Height ? scale.Y : 0f;
  }

  public static string DescriptionScale(ToolSelection selection)
  {
    if (Projector == null) return "";
    var tool = selection.Tool;
    var scale = Scaling.Get();
    var height = tool.Height ? $", h: {Format2(scale.Y)}" : "";
    var shape = Shape;
    if (shape == RulerShape.Rectangle)
      return $"w: {Format2(scale.X)}, d: {Format2(scale.Z)}" + height;

    if (shape == RulerShape.Square)
      return $"w: {Format2(scale.X)}" + height;

    if (shape == RulerShape.Circle)
      return $"r: {Format2(scale.X)}" + height;
    if (shape == RulerShape.Ring)
    {
      var min = Mathf.Min(scale.X, scale.Z);
      var max = Mathf.Max(scale.X, scale.Z);
      return $"r: {Format2(min)}-{Format2(max)}" + height;
    }
    if (shape == RulerShape.Frame)
    {
      var min = Mathf.Min(scale.X, scale.Z);
      var max = Mathf.Max(scale.X, scale.Z);
      return $"w: {Format2(min)}-{Format2(max)}" + height;
    }
    return "";
  }
  public static string DescriptionPosition()
  {
    if (Projector == null) return "";
    var pos = Projector.transform.position;
    return $"x: {Format1(pos.x)}, z: {Format1(pos.z)}, y: {Format1(pos.y)}";
  }
  private static string Format1(float f) => f.ToString("F1", CultureInfo.InvariantCulture);
  private static string Format2(float f) => f.ToString("F2", CultureInfo.InvariantCulture);

  private static GameObject InitializeGameObject()
  {
    Projector = new() { layer = LayerMask.NameToLayer("character_trigger") };
    return Projector;
  }
  private static GameObject CreateCircle(GameObject obj)
  {
    var go = CreateChild(obj);
    go.AddComponent<CircleRuler>();
    return go;
  }
  private static GameObject CreateRectangle(GameObject obj)
  {
    var go = CreateChild(obj);
    go.AddComponent<RectangleRuler>();
    return go;
  }
  private static GameObject CreateChild(GameObject obj)
  {
    var go = new GameObject();
    go.transform.SetParent(obj.transform);
    go.transform.localPosition = Vector3.zero;
    go.transform.localRotation = Quaternion.identity;
    return go;
  }
  public static void Constrain(Range<float?> size, Range<float?> height)
  {
    var scale = Scaling.Get();
    if (size.Min.HasValue && size.Max.HasValue)
    {
      scale.SetScaleX(Mathf.Clamp(scale.X, size.Min.Value, size.Max.Value));
      scale.SetScaleZ(Mathf.Clamp(scale.Z, size.Min.Value, size.Max.Value));
    }
    if (height.Min.HasValue && height.Max.HasValue)
    {
      scale.SetScaleY(Mathf.Clamp(scale.Y, height.Min.Value, height.Max.Value));
    }
  }
  public static void InitializeProjector(Tool tool, GameObject obj)
  {
    if (tool.Width)
      Square = CreateRectangle(obj);
    if (tool.Frame)
      Frame = CreateRectangle(obj);
    if (tool.Width && tool.Depth)
      Rectangle = CreateRectangle(obj);
    if (tool.Radius)
      Circle = CreateCircle(obj);
    if (tool.Ring)
      Ring = CreateCircle(obj);
  }
  public static void Create(Tool tool)
  {
    Remove();
    if (!tool.Radius && !tool.Width && !tool.Depth) return;
    InitializeProjector(tool, InitializeGameObject());
  }

  public static void Remove()
  {
    if (Projector != null) UnityEngine.Object.Destroy(Projector);
    Projector = null;
    if (Circle != null) UnityEngine.Object.Destroy(Circle);
    Circle = null;
    if (Ring != null) UnityEngine.Object.Destroy(Ring);
    Ring = null;
    if (Frame != null) UnityEngine.Object.Destroy(Frame);
    Frame = null;
    if (Rectangle != null) UnityEngine.Object.Destroy(Rectangle);
    Rectangle = null;
    if (Square != null) UnityEngine.Object.Destroy(Square);
    Square = null;
  }

  private static void HighlightCircle(Vector3 center, float radius, float height)
  {
    Range<float> r = new(radius);
    foreach (var wtr in WearNTear.s_allInstances)
    {
      // Removing can make the instances invalid.
      if (!wtr || !wtr.m_nview || wtr.m_nview.GetZDO() == null) continue;
      var pos = wtr.m_nview.GetZDO().GetPosition();
      if (Selector.Within(pos, center, r, height))
        wtr.Highlight();
    }
  }
  private static void HighlightRing(Vector3 center, float radius1, float radius2, float height)
  {
    var min = Mathf.Min(radius1, radius2);
    var max = Mathf.Max(radius1, radius2);
    Range<float> radius = new(min, max);
    foreach (var wtr in WearNTear.s_allInstances)
    {
      // Removing can make the instances invalid.
      if (!wtr || !wtr.m_nview || wtr.m_nview.GetZDO() == null) continue;
      var pos = wtr.m_nview.GetZDO().GetPosition();
      if (Selector.Within(pos, center, radius, height))
        wtr.Highlight();
    }
  }
  private static void HighlightRectangle(Vector3 center, float angle, float width, float depth, float height)
  {
    Range<float> w = new(width);
    Range<float> d = new(depth);
    foreach (var wtr in WearNTear.s_allInstances)
    {
      // Removing can make the instances invalid.
      if (!wtr || !wtr.m_nview || wtr.m_nview.GetZDO() == null) continue;
      var pos = wtr.m_nview.GetZDO().GetPosition();
      if (Selector.Within(pos, center, angle, w, d, height))
        wtr.Highlight();
    }
  }
  private static void HighlightFrame(Vector3 center, float angle, float size1, float size2, float height)
  {
    var min = Mathf.Min(size1, size2);
    var max = Mathf.Max(size1, size2);
    Range<float> size = new(min);
    foreach (var wtr in WearNTear.s_allInstances)
    {
      // Removing can make the instances invalid.
      if (!wtr || !wtr.m_nview || wtr.m_nview.GetZDO() == null) continue;
      var pos = wtr.m_nview.GetZDO().GetPosition();
      if (Selector.Within(pos, center, angle, size, size, height))
        wtr.Highlight();
    }
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateWearNTearHover))]
public class UpdateWearNTearHover
{
  static bool Prefix() => !Ruler.IsActive;
}

[HarmonyPatch(typeof(Hud), nameof(Hud.SetupPieceInfo))]
public class AddExtraInfo
{
  private static string DescriptionHover()
  {
    if (Ruler.Projector) return "";
    var hovered = Selector.GetHovered(InfinityHammer.Configuration.Range, [], InfinityHammer.Configuration.IgnoredIds);
    var name = hovered == null ? "" : Utils.GetPrefabName(hovered.gameObject);
    return $"id: {name}";
  }
  private static string Description(ToolSelection selection)
  {
    if (Hud.instance.m_pieceSelectionWindow.activeSelf) return "";
    var lines = new[] { DescriptionHover(), Ruler.DescriptionScale(selection), Ruler.DescriptionPosition() };
    return string.Join("\n", lines.Where(s => s != ""));
  }
  static Vector2? DefaultOffset;
  static Vector2? ToolOffset;
  static void Prefix(Hud __instance)
  {
    var tr = __instance.m_pieceDescription.rectTransform;
    if (DefaultOffset == null) DefaultOffset = tr.offsetMin;
    if (ToolOffset == null) ToolOffset = new(tr.offsetMin.x, -160);
    // Not sure if this is needed, but might prevent some extra size calculations.
    if (tr.offsetMin != DefaultOffset.Value)
      tr.offsetMin = DefaultOffset.Value;
  }
  static void Postfix(Hud __instance, Piece piece)
  {
    if (!piece) return;
    if (Selection.Get() is not ToolSelection selection) return;
    if (ToolOffset == null) return;
    var tr = __instance.m_pieceDescription.rectTransform;
    if (tr.offsetMin != ToolOffset.Value)
      tr.offsetMin = ToolOffset.Value;
    if (Hud.instance.m_pieceSelectionWindow.activeSelf) return;
    var text = Description(selection);
    if (__instance.m_pieceDescription.text != "") text = "\n" + text;
    __instance.m_pieceDescription.text += text;
  }
}