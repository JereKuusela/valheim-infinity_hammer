using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;

namespace InfinityHammer;

[HarmonyPatch]
public static class DungeonRooms
{
  public static void AddRoom(ZNetView obj, int hash, Vector3 pos, Quaternion rot)
  {
    var zdo = obj.GetZDO();
    var rooms = zdo.GetInt(ZDOVars.s_rooms);
    var data = zdo.GetByteArray(ZDOVars.s_roomData);
    if (rooms > 0)
      AddRoom(zdo, rooms, hash, pos, rot);
    else if (data != null)
      AddRoom(zdo, data, hash, pos, rot);
  }
  private static void AddRoom(ZDO zdo, int rooms, int hash, Vector3 pos, Quaternion rot)
  {
    zdo.Set(ZDOVars.s_rooms, rooms + 1);
    zdo.Set(StringExtensionMethods.GetStableHashCode("room" + rooms), hash);
    zdo.Set(StringExtensionMethods.GetStableHashCode("room" + rooms + "_pos"), pos);
    zdo.Set(StringExtensionMethods.GetStableHashCode("room" + rooms + "_rot"), rot);
  }
  private static void AddRoom(ZDO zdo, byte[] data, int hash, Vector3 pos, Quaternion rot)
  {
    DungeonGenerator.saveStream.SetLength(0L);
    BinaryReader binaryReader = new(new MemoryStream(data));
    int rooms = binaryReader.ReadInt32();
    DungeonGenerator.saveWriter.Write(rooms + 1);
    for (int i = 0; i < rooms; i++)
    {
      DungeonGenerator.saveWriter.Write(binaryReader.ReadInt32());
      Utils.Write(DungeonGenerator.saveWriter, Utils.ReadVector3(binaryReader));
      Utils.Write(DungeonGenerator.saveWriter, Utils.ReadQuaternion(binaryReader));
    }
    DungeonGenerator.saveWriter.Write(hash);
    Utils.Write(DungeonGenerator.saveWriter, pos);
    Utils.Write(DungeonGenerator.saveWriter, rot);
    zdo.Set(ZDOVars.s_roomData, DungeonGenerator.saveStream.ToArray());
  }
  public static void RemoveRoom(ZNetView obj, int index)
  {
    var zdo = obj.GetZDO();
    var rooms = zdo.GetInt(ZDOVars.s_rooms);
    if (rooms > 0)
      Remove(zdo, rooms, index);
    var data = zdo.GetByteArray(ZDOVars.s_roomData);
    if (data != null)
      Remove(zdo, data, index);

    if (obj.transform.childCount < 1) return;
    var child = obj.transform.GetChild(index);
    if (child)
    {
      child.parent = null;
      Object.Destroy(child.gameObject);
    }
  }
  private static void Remove(ZDO zdo, int rooms, int index)
  {
    for (var i = index + 1; i < rooms; i++)
    {
      var sourceHash = StringExtensionMethods.GetStableHashCode("room" + i);
      var targetHash = StringExtensionMethods.GetStableHashCode("room" + (i - 1));
      zdo.Set(targetHash, zdo.GetInt(sourceHash));
      sourceHash = StringExtensionMethods.GetStableHashCode("room" + i + "_pos");
      targetHash = StringExtensionMethods.GetStableHashCode("room" + (i - 1) + "_pos");
      zdo.Set(targetHash, zdo.GetVec3(sourceHash, Vector3.zero));
      sourceHash = StringExtensionMethods.GetStableHashCode("room" + i + "_rot");
      targetHash = StringExtensionMethods.GetStableHashCode("room" + (i - 1) + "_rot");
      zdo.Set(targetHash, zdo.GetQuaternion(sourceHash, Quaternion.identity));
    }
    zdo.Set(ZDOVars.s_rooms, rooms - 1);
    zdo.RemoveInt(StringExtensionMethods.GetStableHashCode("room" + rooms));
    zdo.RemoveVec3(StringExtensionMethods.GetStableHashCode("room" + rooms + "_pos"));
    zdo.RemoveQuaternion(StringExtensionMethods.GetStableHashCode("room" + rooms + "_rot"));
  }
  private static void Remove(ZDO zdo, byte[] data, int index)
  {
    DungeonGenerator.saveStream.SetLength(0L);
    BinaryReader binaryReader = new(new MemoryStream(data));
    int rooms = binaryReader.ReadInt32();
    DungeonGenerator.saveWriter.Write(rooms - 1);
    for (int i = 0; i < rooms; i++)
    {
      var room = binaryReader.ReadInt32();
      var pos = Utils.ReadVector3(binaryReader);
      var rot = Utils.ReadQuaternion(binaryReader);
      if (i == index) continue;
      DungeonGenerator.saveWriter.Write(room);
      Utils.Write(DungeonGenerator.saveWriter, pos);
      Utils.Write(DungeonGenerator.saveWriter, rot);
    }
    zdo.Set(ZDOVars.s_roomData, DungeonGenerator.saveStream.ToArray());
  }
  public static void Reposition(ZDO zdo, Transform target)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
    if (!prefab || !prefab.GetComponent<DungeonGenerator>()) return;
    var trs = target.GetComponentsInChildren<Room>().Select(r => r.transform).ToList();
    var data = zdo.GetByteArray(ZDOVars.s_roomData);
    var rooms = zdo.GetInt(ZDOVars.s_rooms);
    if (rooms > 0)
      Reposition(zdo, rooms, trs);
    else if (data != null)
      Reposition(zdo, data, trs);
  }
  private static void Reposition(ZDO zdo, int rooms, List<Transform> trs)
  {
    for (var i = 0; i < rooms; i++)
    {
      var posHash = StringExtensionMethods.GetStableHashCode("room" + i + "_pos");
      var rotHash = StringExtensionMethods.GetStableHashCode("room" + i + "_rot");
      if (trs.Count <= i)
      {
        zdo.Set(posHash, zdo.GetPosition());
        zdo.Set(rotHash, zdo.GetRotation());
        continue;
      }
      zdo.Set(posHash, trs[i].position);
      zdo.Set(rotHash, trs[i].rotation);
    }
  }
  private static void Reposition(ZDO zdo, byte[] data, List<Transform> trs)
  {
    DungeonGenerator.saveStream.SetLength(0L);
    BinaryReader binaryReader = new(new MemoryStream(data));
    int rooms = binaryReader.ReadInt32();
    DungeonGenerator.saveWriter.Write(rooms);
    for (int i = 0; i < rooms; i++)
    {
      DungeonGenerator.saveWriter.Write(binaryReader.ReadInt32());
      if (trs.Count <= i)
      {
        Utils.Write(DungeonGenerator.saveWriter, zdo.GetPosition());
        Utils.Write(DungeonGenerator.saveWriter, zdo.GetRotation());
        continue;
      }
      Utils.Write(DungeonGenerator.saveWriter, trs[i].position);
      Utils.Write(DungeonGenerator.saveWriter, trs[i].rotation);

    }
    zdo.Set(ZDOVars.s_roomData, DungeonGenerator.saveStream.ToArray());
  }
  public static DungeonGenerator? FindClosestDg(Vector3 pos) => Dgs.OrderBy(dg =>
  {
    var distance = Vector3.Distance(pos, dg.transform.position);
    foreach (Transform tr in dg.transform)
    {
      if (!tr.TryGetComponent(out Room room)) continue;
      foreach (var c in room.GetConnections())
      {
        var d = Vector3.Distance(pos, c.transform.position);
        if (d < distance) distance = d;
      }
    }
    return distance;
  }).FirstOrDefault();



  private static readonly List<DungeonGenerator> Dgs = [];
  [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Awake)), HarmonyPostfix]
  static void DungeonGeneratorAwake(DungeonGenerator __instance) => Dgs.Add(__instance);
  [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.OnDestroy)), HarmonyPostfix]
  static void DungeonGeneratorDestroy(DungeonGenerator __instance) => Dgs.Remove(__instance);

  [HarmonyPatch(typeof(Piece), nameof(Piece.GetSnapPoints), typeof(Vector3), typeof(float), typeof(List<Transform>), typeof(List<Piece>)), HarmonyPrefix]
  static void GetSnapPointsRadius(ref float radius)
  {
    var multiplier = Selection.Get().SnapMultiplier;
    radius *= multiplier;
  }

  [HarmonyPatch(typeof(Piece), nameof(Piece.GetSnapPoints), typeof(Vector3), typeof(float), typeof(List<Transform>), typeof(List<Piece>)), HarmonyPostfix]
  static void GetSnapPoints(Vector3 point, float radius, List<Transform> points)
  {
    var multiplier = Selection.Get().DungeonRoomSnapMultiplier;
    foreach (var dg in Dgs)
    {
      foreach (Transform tr in dg.transform)
      {
        if (!tr.TryGetComponent(out Room room)) continue;
        foreach (var c in room.GetConnections())
        {
          // Rooms are big so longer snap distance works better.
          if (Vector3.Distance(point, c.transform.position) <= radius * multiplier)
            points.Add(c.transform);
        }
      }
    }
  }
}