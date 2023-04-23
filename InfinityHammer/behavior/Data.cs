using UnityEngine;
namespace InfinityHammer;

public class DataHelper
{
  public static void Init(string name, Transform tr, ZDO? zdo = null) => Init(name.GetStableHashCode(), tr, zdo);
  public static void Init(int hash, Transform tr, ZDO? zdo = null) => Init(hash, tr.position, tr.rotation, tr.localScale, zdo);
  public static void Init(ZDO zdo) => Init(zdo.GetPrefab(), zdo.GetPosition(), zdo.GetRotation(), zdo.GetVec3("scale", Vector3.one), zdo);
  public static void Init(int hash, Vector3 pos, Quaternion rot, Vector3 scale, ZDO? zdo = null)
  {
    Clear();
    if (!ZNetScene.instance.m_namedPrefabs.TryGetValue(hash, out var obj)) return;
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    if (zdo == null && (!view.m_syncInitialScale || scale == Vector3.one)) return;
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos);
    if (zdo != null) Copy(zdo, ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot;
    ZNetView.m_initZDO.m_type = view.m_type;
    ZNetView.m_initZDO.m_distant = view.m_distant;
    ZNetView.m_initZDO.m_persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = hash;
    Console.instance.AddString("Scale: " + scale);
    if (view.m_syncInitialScale)
      ZNetView.m_initZDO.Set("scale", scale);
    ZNetView.m_initZDO.m_dataRevision = 1;
  }
  public static void Clear()
  {
    ZNetView.m_initZDO = null;
  }

  public static void Copy(ZDO from, ZDO to)
  {
    to.m_floats = from.m_floats;
    to.m_vec3 = from.m_vec3;
    to.m_quats = from.m_quats;
    to.m_ints = from.m_ints;
    to.m_longs = from.m_longs;
    to.m_strings = from.m_strings;
    to.m_byteArrays = from.m_byteArrays;
    to.IncreseDataRevision();
  }
}