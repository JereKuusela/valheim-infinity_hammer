using System.Collections.Generic;
using UnityEngine;
namespace InfinityHammer;
public class BlueprintObject {
  public string Prefab = "";
  public Vector3 Pos;
  public Quaternion Rot;
  public Vector3 Scale;
  public ZDO? Data;
  public BlueprintObject(string name, Vector3 pos, Quaternion rot, Vector3 scale, ZDO? data) {
    Prefab = name;
    Pos = pos;
    Rot = rot.normalized;
    Scale = scale;
    Data = data;
  }
}
public class Blueprint {
  public string Name = "";
  public string Description = "";
  public string Creator = "";
  public List<BlueprintObject> Objects = new();
  public List<Vector3> SnapPoints = new();
}