using System;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

public class RulerParameters
{
  public float? Radius;
  public float? Ring;
  public float? Width;
  public float? Grid;
  public float? Depth;
  public float? Height;
  public bool RotateWithPlayer;
  public bool IsTargeted;
}
public enum RulerShape
{
  Circle,
  Ring,
  Grid,
  Square,
  Rectangle,
}
public class Ruler
{
  public static GameObject? Projector = null;
  public static GameObject? Circle = null;
  public static GameObject? Ring = null;
  public static GameObject? Rectangle = null;
  public static GameObject? Square = null;
  public static GameObject? Grid = null;
  private static CircleProjector? BaseProjector = null;
  public static void SanityCheckShape()
  {
    if (Ruler.Shape == RulerShape.Circle && !Circle) Ruler.Shape = RulerShape.Ring;
    if (Ruler.Shape == RulerShape.Ring && !Ring) Ruler.Shape = RulerShape.Square;
    if (Ruler.Shape == RulerShape.Square && !Square) Ruler.Shape = RulerShape.Grid;
    if (Ruler.Shape == RulerShape.Grid && !Grid) Ruler.Shape = RulerShape.Rectangle;
    if (Ruler.Shape == RulerShape.Rectangle && !Rectangle) Ruler.Shape = RulerShape.Circle;
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
  public static void Update()
  {
    var player = Player.m_localPlayer;
    if (Projector == null || !player) return;
    var ghost = player.m_placementGhost;
    var ptr = player.transform;
    var gtr = ghost.transform;
    Projector.SetActive(ghost);
    if (!ghost) return;
    var scale = Scaling.Command;
    if (IsTargeted)
      Projector.transform.position = (ptr.position + gtr.position) / 2f;
    else
      Projector.transform.position = gtr.position;
    var angle = gtr.rotation.eulerAngles.y;
    if (RotateWithPlayer)
      angle = Vector3.SignedAngle(Vector3.forward, Utils.DirectionXZ(gtr.position - ptr.position), Vector3.up);
    Projector.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    if (IsTargeted)
    {
      var distance = Utils.DistanceXZ(ptr.position, gtr.position) / 2f;
      scale.SetScaleZ(distance);
    }
    Ruler.SanityCheckShape();
    if (Circle != null)
    {
      Circle.SetActive(Shape == RulerShape.Circle || Shape == RulerShape.Ring);
      var proj = Circle.GetComponent<CircleProjector>();
      if (Circle.activeSelf)
      {
        proj.m_radius = scale.X;
        proj.m_nrOfSegments = Math.Max(3, (int)(proj.m_radius * 4));
      }
      else if (proj.m_nrOfSegments > 0)
      {
        proj.m_nrOfSegments = 0;
        proj.CreateSegments();
      }
    }
    if (Circle != null && Ring != null)
    {
      Ring.SetActive(Shape == RulerShape.Ring);
      var proj = Ring.GetComponent<CircleProjector>();
      if (Ring.activeSelf)
      {
        proj.m_radius = scale.Z;
        proj.m_nrOfSegments = Math.Max(3, (int)(proj.m_radius * 4));
      }
      else if (proj.m_nrOfSegments > 0)
      {
        proj.m_nrOfSegments = 0;
        proj.CreateSegments();
      }
    }
    if (Square != null)
    {
      Square.SetActive(Shape == RulerShape.Square || Shape == RulerShape.Grid);
      var proj = Square.GetComponent<RectangleProjector>();
      if (Square.activeSelf)
      {
        proj.m_width = scale.X;
        proj.m_depth = scale.X;
        proj.m_nrOfSegments = Math.Max(3, (int)((proj.m_depth + proj.m_width) * 2));
      }
      else if (proj.m_nrOfSegments > 0)
      {
        proj.m_nrOfSegments = 0;
        proj.CreateSegments();
      }
    }
    if (Square != null && Grid != null)
    {
      Grid.SetActive(Shape == RulerShape.Grid);
      var proj = Grid.GetComponent<RectangleProjector>();
      if (Grid.activeSelf)
      {
        proj.m_width = scale.Z;
        proj.m_depth = scale.Z;
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
        proj.m_nrOfSegments = Math.Max(3, (int)((proj.m_depth + proj.m_width) * 2));
      }
      else if (proj.m_nrOfSegments > 0)
      {
        proj.m_nrOfSegments = 0;
        proj.CreateSegments();
      }
    }
  }
  public static float Height => UseHeight ? Scaling.Command.Y : 0f;

  private static string DescriptionScale()
  {
    var scale = Scaling.Command;
    var height = UseHeight ? $", h: {Format(scale.Y)}" : "";
    if (Shape == RulerShape.Rectangle)
      return $"w: {Format(scale.X)}, d: {Format(scale.Z)}" + height;

    if (Shape == RulerShape.Square)
      return $"w: {Format(scale.X)}" + height;

    if (Shape == RulerShape.Circle)
      return $"r: {Format(scale.X)}" + height;
    if (Shape == RulerShape.Ring)
    {
      var min = Mathf.Min(scale.X, scale.Z);
      var max = Mathf.Max(scale.X, scale.Z);
      return $"r: {Format(min)}-{Format(max)}" + height;
    }
    if (Shape == RulerShape.Grid)
    {
      var min = Mathf.Min(scale.X, scale.Z);
      var max = Mathf.Max(scale.X, scale.Z);
      return $"w: {Format(min)}-{Format(max)}" + height;
    }
    return "";
  }
  private static string DescriptionPosition()
  {
    if (Projector == null) return "";
    var pos = Projector.transform.position;
    return $"x: {Format(pos.x)}, z: {Format(pos.z)}, y: {Format(pos.y)}";
  }
  public static string Description()
  {
    if (!Selection.IsCommand() || Projector == null) return "";
    if (Hud.instance.m_pieceSelectionWindow.activeSelf) return "";
    var lines = new[] { DescriptionScale(), DescriptionPosition() };
    return string.Join("\n", lines.Where(s => s != ""));
  }
  private static string Format(float f) => f.ToString("F1", CultureInfo.InvariantCulture);

  private static GameObject InitializeGameObject(RulerParameters pars)
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
  public static void InitializeProjector(RulerParameters pars, GameObject obj)
  {
    if (BaseProjector == null)
      BaseProjector = GetBaseProjector();
    RotateWithPlayer = pars.RotateWithPlayer;
    IsTargeted = pars.IsTargeted;
    var scale = Scaling.Command;
    if (pars.Width.HasValue)
    {
      Square = CreateChild(obj);
      var proj = Square.AddComponent<RectangleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_mask = BaseProjector.m_mask;
      proj.m_nrOfSegments = 3;
      scale.SetScaleX(pars.Width.Value);
    }
    if (pars.Grid.HasValue)
    {
      Grid = CreateChild(obj);
      var proj = Grid.AddComponent<RectangleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_mask = BaseProjector.m_mask;
      proj.m_nrOfSegments = 3;
      scale.SetScaleZ(pars.Grid.Value);
    }
    if (pars.Width.HasValue && pars.Depth.HasValue)
    {
      Rectangle = CreateChild(obj);
      var proj = Rectangle.AddComponent<RectangleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_mask = BaseProjector.m_mask;
      proj.m_nrOfSegments = 3;
      scale.SetScaleX(pars.Width.Value);
      scale.SetScaleZ(pars.Depth.Value);
    }
    if (pars.Radius.HasValue)
    {
      Circle = CreateChild(obj);
      var proj = Circle.AddComponent<CircleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_mask = BaseProjector.m_mask;
      proj.m_nrOfSegments = 3;
      scale.SetScaleX(pars.Radius.Value);
    }
    if (pars.Ring.HasValue)
    {
      Ring = CreateChild(obj);
      var proj = Ring.AddComponent<CircleProjector>();
      proj.m_prefab = BaseProjector.m_prefab;
      proj.m_mask = BaseProjector.m_mask;
      proj.m_nrOfSegments = 3;
      scale.SetScaleZ(pars.Ring.Value);
    }
    UseHeight = pars.Height.HasValue;
    if (pars.Height.HasValue)
    {
      scale.SetScaleY(pars.Height.Value);
    }
  }
  public static void Create(RulerParameters pars)
  {
    Remove();
    if (pars.Radius == null && pars.Width == null && pars.Depth == null) return;
    InitializeProjector(pars, InitializeGameObject(pars));
  }

  public static void Remove()
  {
    if (Projector != null)
      UnityEngine.Object.Destroy(Projector);
    Projector = null;
    Circle = null;
    Ring = null;
    Grid = null;
    Rectangle = null;
    Square = null;
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
