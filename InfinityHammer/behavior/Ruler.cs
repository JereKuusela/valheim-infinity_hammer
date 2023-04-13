using System;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;

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
  public static bool IsActive => Projector != null;
  public static GameObject? Projector = null;
  public static GameObject? Circle = null;
  public static GameObject? HeightCircle = null;
  public static GameObject? Ring = null;
  public static GameObject? Rectangle = null;
  public static GameObject? Square = null;
  public static GameObject? Frame = null;
  private static CircleProjector? BaseProjector = null;
  public static void SanityCheckShape()
  {
    if (Ruler.Shape == RulerShape.Circle && (!Circle || !Configuration.ShapeCircle)) Ruler.Shape = RulerShape.Ring;
    if (Ruler.Shape == RulerShape.Ring && (!Ring || !Configuration.ShapeRing)) Ruler.Shape = RulerShape.Square;
    if (Ruler.Shape == RulerShape.Square && (!Square || !Configuration.ShapeSquare)) Ruler.Shape = RulerShape.Frame;
    if (Ruler.Shape == RulerShape.Frame && (!Frame || !Configuration.ShapeFrame)) Ruler.Shape = RulerShape.Rectangle;
    if (Ruler.Shape == RulerShape.Rectangle && (!Rectangle || !Configuration.ShapeRectangle)) Ruler.Shape = RulerShape.Circle;
  }
  private static CircleProjector GetBaseProjector()
  {
    var workbench = ZNetScene.instance.GetPrefab("piece_workbench");
    if (!workbench) throw new InvalidOperationException("Error: Unable to find the workbench object.");
    BaseProjector = workbench.GetComponentInChildren<CircleProjector>();
    return BaseProjector;
  }
  private static bool RotateWithPlayer = false;
  private static bool UseHeight = false;
  private static bool IsTargeted = false;
  public static RulerShape Shape = RulerShape.Circle;
  private static void SetGrid(float width, float height)
  {
    if (!Configuration.PreciseCommands) return;
    var centerX = Math.Floor(width / 0.5) % 2 == 0;
    var centerZ = Math.Floor(height / 0.5) % 2 == 0;
    Vector3 center = new Vector3(centerX ? 0f : 0.5f, 0f, centerZ ? 0f : 0.5f);
    Grid.SetPreciseMode(center);
  }
  private static void BuildCircle(GameObject obj, float radius)
  {
    var proj = obj.GetComponent<CircleProjector>();
    if (obj.activeSelf)
    {
      proj.m_radius = radius;
      proj.m_nrOfSegments = Math.Max(3, (int)(proj.m_radius * 4));
    }
    else if (proj.m_nrOfSegments > 0)
    {
      proj.m_nrOfSegments = 0;
      proj.CreateSegments();
    }
  }
  public static void Update()
  {
    var player = Player.m_localPlayer;
    if (Projector == null || !player) return;
    var ghost = player.m_placementGhost;
    var ptr = player.transform;
    Projector.SetActive(ghost);
    if (!ghost) return;
    var gtr = ghost.transform;
    var scale = Scaling.Command;
    if (IsTargeted)
      Projector.transform.position = (ptr.position + gtr.position) / 2f;
    else
      Projector.transform.position = gtr.position;
    var angle = gtr.rotation.eulerAngles.y;
    if (RotateWithPlayer)
      angle = Vector3.SignedAngle(Vector3.forward, Utils.DirectionXZ(gtr.position - ptr.position), Vector3.up);
    if (Configuration.PreciseCommands)
      angle = 0;
    Projector.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    if (IsTargeted)
    {
      var distance = Utils.DistanceXZ(ptr.position, gtr.position) / 2f;
      scale.SetScaleZ(distance);
    }
    Ruler.SanityCheckShape();
    if (Configuration.PreciseCommands) scale.SetPrecisionXZ(0.25f, 0.5f);
    if (Circle != null)
    {
      Circle.SetActive(Shape == RulerShape.Circle || Shape == RulerShape.Ring);
      BuildCircle(Circle, scale.X);
      if (Circle.activeSelf)
      {
        SetGrid(scale.X, scale.X);
        HighlightCircle(Projector.transform.position, scale.X, scale.Y);
      }
    }
    if (Circle != null && HeightCircle != null)
    {
      HeightCircle.SetActive(Circle.activeSelf && scale.Y != 0);
      if (HeightCircle.activeSelf)
        HeightCircle.transform.localPosition = new Vector3(0f, scale.Y, 0f);
      BuildCircle(HeightCircle, scale.X);
    }
    if (Circle != null && Ring != null)
    {
      Ring.SetActive(Shape == RulerShape.Ring);
      BuildCircle(Ring, scale.Z);
    }
    if (Square != null)
    {
      Square.SetActive(Shape == RulerShape.Square || Shape == RulerShape.Frame);
      var proj = Square.GetComponent<RectangleProjector>();
      if (Square.activeSelf)
      {
        proj.m_width = scale.X;
        proj.m_depth = scale.X;
        SetGrid(scale.X, scale.X);
        proj.m_nrOfSegments = Math.Max(3, (int)((proj.m_depth + proj.m_width) * 2));
        HighlightRectangle(Projector.transform.position, angle, scale.X, scale.X, scale.Y);
      }
      else if (proj.m_nrOfSegments > 0)
      {
        proj.m_nrOfSegments = 0;
        proj.CreateSegments();
      }
    }
    if (Square != null && Frame != null)
    {
      Frame.SetActive(Shape == RulerShape.Frame);
      var proj = Frame.GetComponent<RectangleProjector>();
      if (Frame.activeSelf)
      {
        proj.m_width = scale.Z;
        proj.m_depth = scale.Z;
        SetGrid(scale.Z, scale.Z);
        proj.m_nrOfSegments = Math.Max(3, (int)((proj.m_depth + proj.m_width) * 2));
      }
      else if (proj.m_nrOfSegments > 0)
      {
        proj.m_nrOfSegments = 0;
        proj.CreateSegments();
      }
    }
    if (Rectangle != null)
    {
      Rectangle.SetActive(Shape == RulerShape.Rectangle);
      var proj = Rectangle.GetComponent<RectangleProjector>();
      if (Rectangle.activeSelf)
      {
        proj.m_width = scale.X;
        proj.m_depth = scale.Z;
        SetGrid(scale.X, scale.Z);
        proj.m_nrOfSegments = Math.Max(3, (int)((proj.m_depth + proj.m_width) * 2));
        HighlightRectangle(Projector.transform.position, angle, scale.X, scale.Z, scale.Y);
      }
      else if (proj.m_nrOfSegments > 0)
      {
        proj.m_nrOfSegments = 0;
        proj.CreateSegments();
      }
    }
  }
  public static float Height => UseHeight ? Scaling.Command.Y : 0f;
  public static bool SnapToGround => true;

  private static string DescriptionScale()
  {
    var scale = Scaling.Command;
    var height = UseHeight ? $", h: {Format2(scale.Y)}" : "";
    if (Shape == RulerShape.Rectangle)
      return $"w: {Format2(scale.X)}, d: {Format2(scale.Z)}" + height;

    if (Shape == RulerShape.Square)
      return $"w: {Format2(scale.X)}" + height;

    if (Shape == RulerShape.Circle)
      return $"r: {Format2(scale.X)}" + height;
    if (Shape == RulerShape.Ring)
    {
      var min = Mathf.Min(scale.X, scale.Z);
      var max = Mathf.Max(scale.X, scale.Z);
      return $"r: {Format2(min)}-{Format2(max)}" + height;
    }
    if (Shape == RulerShape.Frame)
    {
      var min = Mathf.Min(scale.X, scale.Z);
      var max = Mathf.Max(scale.X, scale.Z);
      return $"w: {Format2(min)}-{Format2(max)}" + height;
    }
    return "";
  }
  private static string DescriptionPosition()
  {
    if (Projector == null) return "";
    var pos = Projector.transform.position;
    return $"x: {Format1(pos.x)}, z: {Format1(pos.z)}, y: {Format1(pos.y)}";
  }
  public static string Description()
  {
    if (!Selection.IsCommand() || Projector == null) return "";
    if (Hud.instance.m_pieceSelectionWindow.activeSelf) return "";
    var lines = new[] { DescriptionScale(), DescriptionPosition() };
    return string.Join("\n", lines.Where(s => s != ""));
  }
  private static string Format1(float f) => f.ToString("F1", CultureInfo.InvariantCulture);
  private static string Format2(float f) => f.ToString("F2", CultureInfo.InvariantCulture);

  private static GameObject InitializeGameObject()
  {
    Projector = new();
    Projector.layer = LayerMask.NameToLayer("character_trigger");
    return Projector;
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
    var scale = Scaling.Command;
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
  public static void InitializeProjector(RulerParameters pars, GameObject obj)
  {
    if (BaseProjector == null)
      BaseProjector = GetBaseProjector();
    RotateWithPlayer = pars.RotateWithPlayer;
    IsTargeted = pars.IsTargeted;
    var scale = Scaling.Command;
    if (pars.Width)
    {
      Square = CreateChild(obj);
      var proj = Square.AddComponent<RectangleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_nrOfSegments = 3;
    }
    if (pars.Grid)
    {
      Frame = CreateChild(obj);
      var proj = Frame.AddComponent<RectangleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_nrOfSegments = 3;
    }
    if (pars.Width && pars.Depth)
    {
      Rectangle = CreateChild(obj);
      var proj = Rectangle.AddComponent<RectangleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_nrOfSegments = 3;
    }
    if (pars.Radius)
    {
      Circle = CreateChild(obj);
      var proj = Circle.AddComponent<CircleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_nrOfSegments = 3;
      HeightCircle = CreateChild(obj);
      proj = HeightCircle.AddComponent<CircleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_nrOfSegments = 3;
    }
    if (pars.Ring)
    {
      Ring = CreateChild(obj);
      var proj = Ring.AddComponent<CircleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_nrOfSegments = 3;
    }
    UseHeight = pars.Height;
  }
  public static void Create(RulerParameters pars)
  {
    Remove();
    if (!pars.Radius && !pars.Width && pars.Depth) return;
    InitializeProjector(pars, InitializeGameObject());
  }

  public static void Remove()
  {
    if (Projector != null)
      UnityEngine.Object.Destroy(Projector);
    Projector = null;
    Circle = null;
    HeightCircle = null;
    Ring = null;
    Frame = null;
    Rectangle = null;
    Square = null;
  }

  private static void HighlightCircle(Vector3 center, float radius, float height)
  {
    foreach (var wtr in WearNTear.m_allInstances)
    {
      var pos = wtr.m_nview.GetZDO().GetPosition();
      if (Selector.Within(pos, center, radius, height))
        wtr.Highlight();
    }
  }
  private static void HighlightRectangle(Vector3 center, float angle, float width, float depth, float height)
  {
    foreach (var wtr in WearNTear.m_allInstances)
    {
      var pos = wtr.m_nview.GetZDO().GetPosition();
      if (Selector.Within(pos, center, angle, width, depth, height))
        wtr.Highlight();
    }
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetPlaceMode))]
public class InitializeRuler
{
  static void Postfix()
  {
    Ruler.Create(Selection.RulerParameters);
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateWearNTearHover))]
public class UpdateWearNTearHover
{
  static bool Prefix() => !Ruler.IsActive;
}
