using Service;
using UnityEngine;
namespace InfinityHammer;

public class DataHelper
{
  public static void Init(string name, Transform tr, ZDOData? data = null) => Init(name.GetStableHashCode(), tr, data);
  // Lossy scale is needed for multiple objects like blueprints (the container is scaled).
  public static void Init(int hash, Transform tr, ZDOData? data = null) => Init(hash, tr.position, tr.rotation, tr.lossyScale, data);
  public static void Init(ZDO zdo) => Init(zdo.GetPrefab(), zdo.GetPosition(), zdo.GetRotation(), zdo.GetVec3("scale", Vector3.one), new(zdo));
  public static void Init(int hash, Vector3 pos, Quaternion rot, Vector3 scale, ZDOData? data = null)
  {
    Clear();
    if (!ZNetScene.instance.m_namedPrefabs.TryGetValue(hash, out var obj)) return;
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    if (data == null && (!view.m_syncInitialScale || scale == Vector3.one)) return;
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos, hash);
    data?.Copy(ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot.eulerAngles;
    ZNetView.m_initZDO.Type = view.m_type;
    ZNetView.m_initZDO.Distant = view.m_distant;
    ZNetView.m_initZDO.Persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = hash;
    if (view.m_syncInitialScale)
      ZNetView.m_initZDO.Set("scale", scale);
    ZNetView.m_initZDO.DataRevision = 1;
    ZNetView.m_initZDO.IncreaseDataRevision();
  }
  public static void Clear()
  {
    ZNetView.m_initZDO = null;
  }
}