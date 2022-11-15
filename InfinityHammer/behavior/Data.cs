
using Service;
using UnityEngine;
namespace InfinityHammer;

public class DataHelper
{
  public static void Init(string name, Transform tr, ZDO? zdo = null) => Init(name.GetStableHashCode(), tr, zdo);
  public static void Init(int hash, Transform tr, ZDO? zdo = null)
  {
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(tr.position);
    if (zdo != null) Copy(zdo, ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = tr.rotation;
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(hash, out var obj))
    {
      if (obj.TryGetComponent<ZNetView>(out var view))
      {
        ZNetView.m_initZDO.m_type = view.m_type;
        ZNetView.m_initZDO.m_distant = view.m_distant;
        ZNetView.m_initZDO.m_persistent = view.m_persistent;
        ZNetView.m_initZDO.m_prefab = hash;
        if (view.m_syncInitialScale)
          ZNetView.m_initZDO.Set("scale", tr.lossyScale);
      }
    }
    else InfinityHammer.Log.LogWarning("Failed to find prefab for the zdo.");
    ZNetView.m_initZDO.m_dataRevision = 1;
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

  ///<summary>Updates visuals, etc.</summary>
  public static void Fix(ZNetView obj)
  {
    var zdo = obj.GetZDO();
    var character = obj.GetComponent<Character>();
    if (character)
    {
      // SetLevel would also overwrite the health (when copying a creature with a custom health).
      var level = zdo.GetInt(Hash.Level, 1);
      character.m_level = level;
      zdo.Set(Hash.Level, level);
      if (character.m_onLevelSet != null) character.m_onLevelSet(character.m_level);
      character.SetTamed(zdo.GetBool(Hash.Tamed, false));
    }
    obj.GetComponentInChildren<ItemDrop>()?.Load();
    obj.GetComponentInChildren<ArmorStand>()?.UpdateVisual();
    obj.GetComponentInChildren<VisEquipment>()?.UpdateVisuals();
    obj.GetComponentInChildren<ItemStand>()?.UpdateVisual();
    obj.GetComponentInChildren<CookingStation>()?.UpdateCooking();
    obj.GetComponentInChildren<LocationProxy>()?.SpawnLocation();
    obj.GetComponentInChildren<Sign>()?.UpdateText();
    obj.GetComponentInChildren<Door>()?.UpdateState();
  }

}