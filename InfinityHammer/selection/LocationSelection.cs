using System;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;


public class LocationSelection : BaseSelection
{
  // Unity doesn't run scripts for inactive objects.
  // So an inactive object is used to store the selected object.
  // This mimics the ZNetScene.m_namedPrefabs behavior.
  private readonly GameObject Wrapper;
  private readonly SelectedObject Object;
  public override void Destroy()
  {
    base.Destroy();
    UnityEngine.Object.Destroy(Wrapper);
  }


  public LocationSelection(ZoneSystem.ZoneLocation location, int seed)
  {
    if (location == null) throw new InvalidOperationException("Location not found.");
    if (!location.m_prefab) throw new InvalidOperationException("Invalid location");

    Wrapper = new GameObject();
    Wrapper.SetActive(false);
    SelectedPrefab = HammerHelper.SafeInstantiateLocation(location, Hammer.AllLocationsObjects ? null : seed, Wrapper);

    ZDOData data = new();
    var hash = location.m_prefabName.GetStableHashCode();
    data.Set(Hash.Location, hash);
    data.Set(Hash.Seed, seed);
    Object = new(hash, false, data);
  }
  public override ZDOData GetData(int index = 0) => Object.Data;
  public override int GetPrefab(int index = 0) => Object.Prefab;
  public override GameObject GetPrefab(GameObject obj) => ZoneSystem.instance.m_locationProxyPrefab;

  public override void AfterPlace(GameObject obj)
  {
    Undo.StartTracking();
    var view = obj.GetComponent<ZNetView>();
    HammerHelper.RemoveZDO(view.GetZDO());
    var data = GetData();
    if (data == null) return;
    var prefab = data.GetInt(Hash.Location, 0);
    var seed = data.GetInt(Hash.Seed, 0);
    var location = ZoneSystem.instance.GetLocation(prefab);
    var ghost = HammerHelper.GetPlacementGhost();
    var position = ghost.transform.position;
    var rotation = ghost.transform.rotation;
    CustomizeSpawnLocation.AllViews = Hammer.AllLocationsObjects;
    CustomizeSpawnLocation.RandomDamage = Hammer.RandomLocationDamage;
    ZoneSystem.instance.SpawnLocation(location, seed, position, rotation, ZoneSystem.SpawnMode.Full, []);
    foreach (var zdo in Undo.Objects)
    {
      if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var spawned))
        PostProcessPlaced(spawned.gameObject);
    }
    CustomizeSpawnLocation.RandomDamage = null;
    CustomizeSpawnLocation.AllViews = false;
    Undo.StopTracking();
  }
  public override void Activate()
  {
    base.Activate();
    Hammer.SelectEmpty();
  }
}