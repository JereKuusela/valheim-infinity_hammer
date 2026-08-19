using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Service;
namespace InfinityHammer;

public class BlueprintObject(string name, Vector3 pos, Quaternion rot, Vector3 scale, string info, string data, float chance)
{
  public string Prefab = name;
  public Vector3 Pos = pos;
  public Quaternion Rot = rot.normalized;
  public string Data = data;
  public Vector3 Scale = scale;
  public float Chance = chance;
  public string ExtraInfo = info;
}
public class Blueprint
{
  public string Name = "";
  public string Description = "";
  public string Creator = "";
  public string Category = "";
  public Vector3 Coordinates = Vector3.zero;
  public Vector3 Rotation = Vector3.zero;
  public string CenterPiece = Configuration.BlueprintCenterPiece;
  public List<BlueprintObject> Objects = [];
  public List<Vector3> SnapPoints = [];
  public TerrainHeight? TerrainHeight = null;
  public TerrainPaint? TerrainPaint = null;
  public float Radius = 0f;
  public Vector3 Center(string centerPiece)
  {
    if (centerPiece != "")
      CenterPiece = centerPiece;
    Bounds bounds = new();
    var y = float.MaxValue;
    Quaternion rot = Quaternion.identity;
    foreach (var obj in Objects)
    {
      y = Mathf.Min(y, obj.Pos.y);
      bounds.Encapsulate(obj.Pos);
    }

    Vector3 center = new(bounds.center.x, y, bounds.center.z);
    foreach (var obj in Objects)
    {
      if (obj.Prefab == CenterPiece)
      {
        center = obj.Pos;
        rot = Quaternion.Inverse(obj.Rot);
        break;
      }
    }
    Radius = Utils.LengthXZ(bounds.extents);
    foreach (var obj in Objects)
      obj.Pos -= center;
    SnapPoints = SnapPoints.Select(p => p - center).ToList();
    if (TerrainHeight != null)
    {
      TerrainHeight.FirstNodePosition -= center;
      for (int x = 0; x < TerrainHeight.Width; x++)
      {
        for (int z = 0; z < TerrainHeight.Height; z++)
        {
          if (TerrainHeight.Heights[x, z].HasValue)
            TerrainHeight.Heights[x, z] -= center.y;
        }
      }
    }
    if (TerrainPaint != null)
      TerrainPaint.FirstNodePosition -= center;
    if (rot != Quaternion.identity)
    {
      foreach (var obj in Objects)
      {
        obj.Pos = rot * obj.Pos;
        obj.Rot = rot * obj.Rot;
      }
      SnapPoints = SnapPoints.Select(p => rot * p).ToList();
      if (TerrainHeight != null)
      {
        TerrainHeight.FirstNodePosition = rot * TerrainHeight.FirstNodePosition;
        var terrainRotation = rot * TerrainHeight.FirstNodeRotation;
        TerrainHeight.FirstNodeRotation = Quaternion.Euler(0f, terrainRotation.eulerAngles.y, 0f);
      }
      if (TerrainPaint != null)
      {
        TerrainPaint.FirstNodePosition = rot * TerrainPaint.FirstNodePosition;
        var paintTerrainRotation = rot * TerrainPaint.FirstNodeRotation;
        TerrainPaint.FirstNodeRotation = Quaternion.Euler(0f, paintTerrainRotation.eulerAngles.y, 0f);
      }
    }
    return center;
  }
}