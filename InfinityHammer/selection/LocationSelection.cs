using System;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;


public class LocationSelection : BaseSelection
{
  public SelectedObject Object;

  public LocationSelection(ZoneSystem.ZoneLocation location, int seed)
  {
    if (location == null) throw new InvalidOperationException("Location not found.");
    if (!location.m_prefab) throw new InvalidOperationException("Invalid location");
    SelectedObject = HammerHelper.SafeInstantiateLocation(location, Hammer.AllLocationsObjects ? null : seed);
    HammerHelper.EnsurePiece(SelectedObject);
    ZDOData data = new();
    data.Set(Hash.Location, location.m_prefabName.GetStableHashCode());
    data.Set(Hash.Seed, seed);
    Object = new(location.m_prefabName, false, data);
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public override ZDOData GetData(int index = 0) => Object.Data;
  public override int GetPrefab(int index = 0) => Object.Prefab.GetStableHashCode();
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

  ///<summary>Replaces LocationProxy with the actual location.</summary>
  private static void SpawnLocation()
  {
  }
}