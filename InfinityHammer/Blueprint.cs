using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace InfinityHammer;

public interface IBlueprintObject
{
  string Prefab { get; set; }
  Vector3 Pos { get; set; }
  Quaternion Rot { get; set; }
  string Data { get; set; }
  Vector3 Scale { get; set; }
  float Chance { get; set; }
  string ExtraInfo { get; set; }
}

[Serializable]
public class BlueprintObject(
  string name,
  Vector3 pos,
  Quaternion rot,
  Vector3 scale,
  string info,
  string data,
  float chance) : IBlueprintObject
{
  public string prefab = name;
  public Vector3 pos = pos;
  public Quaternion rot = rot.normalized;
  public string data = data;
  public Vector3 scale = scale;
  public float chance = chance;
  public string extraInfo = info;

  public virtual string Prefab
  {
    get => prefab;
    set => prefab = value ?? throw new ArgumentNullException(nameof(value));
  }

  public virtual Vector3 Pos
  {
    get => pos;
    set => pos = value;
  }

  public virtual Quaternion Rot
  {
    get => rot;
    set => rot = value;
  }

  public virtual string Data
  {
    get => data;
    set => data = value ?? throw new ArgumentNullException(nameof(value));
  }

  public virtual Vector3 Scale
  {
    get => scale;
    set => scale = value;
  }

  public virtual float Chance
  {
    get => chance;
    set => chance = value;
  }

  public virtual string ExtraInfo
  {
    get => extraInfo;
    set => extraInfo = value ?? throw new ArgumentNullException(nameof(value));
  }
}

public interface IBlueprint
{
  string Name { get; set; }
  string Description { get; set; }
  string Creator { get; set; }
  Vector3 Coordinates { get; set; }
  Vector3 Rotation { get; set; }
  string CenterPiece { get; set; }
  // List<BlueprintObject> Objects { get; set; }
  List<Vector3> SnapPoints { get; set; }
  float Radius { get; set; }
  Vector3 Center(string centerPiece);
}

public class Blueprint : IBlueprint
{
  public virtual string Name
  {
    get => name;
    set => name = value ?? throw new ArgumentNullException(nameof(value));
  }

  public virtual string Description
  {
    get => description;
    set => description = value ?? throw new ArgumentNullException(nameof(value));
  }

  public virtual string Creator
  {
    get => creator;
    set => creator = value ?? throw new ArgumentNullException(nameof(value));
  }

  public virtual Vector3 Coordinates
  {
    get => coordinates;
    set => coordinates = value;
  }

  public virtual Vector3 Rotation
  {
    get => rotation;
    set => rotation = value;
  }

  public virtual string CenterPiece
  {
    get => centerPiece;
    set => centerPiece = value ?? throw new ArgumentNullException(nameof(value));
  }

  
  public virtual List<Vector3> SnapPoints
  {
    get => snapPoints;
    set => snapPoints = value ?? throw new ArgumentNullException(nameof(value));
  }

  public virtual float Radius
  {
    get => radius;
    set => radius = value;
  }

  public string name = "";
  public string description = "";
  public string creator = "";
  public Vector3 coordinates = Vector3.zero;
  public Vector3 rotation = Vector3.zero;
  public string centerPiece = Configuration.BlueprintCenterPiece;
  public List<BlueprintObject> Objects = [];
  public List<Vector3> snapPoints = [];
  public float radius = 0f;
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
    if (rot != Quaternion.identity)
    {
      foreach (var obj in Objects)
      {
        obj.Pos = rot * obj.Pos;
        obj.Rot = rot * obj.Rot;
      }
      SnapPoints = SnapPoints.Select(p => rot * p).ToList();
    }
    return center;
  }
}