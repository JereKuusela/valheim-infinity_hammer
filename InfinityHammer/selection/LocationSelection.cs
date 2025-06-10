using System;
using System.Collections.Generic;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;
using WorldEditCommands;

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

    Wrapper = new GameObject();
    Wrapper.SetActive(false);
    SelectedPrefab = HammerHelper.SafeInstantiateLocation(location, Hammer.AllLocationsObjects ? null : seed, Wrapper);

    DataEntry data = new();
    data.Set(ZDOVars.s_location, location.Hash);
    data.Set(ZDOVars.s_seed, seed);
    Object = new(location.Hash, false, data, Wrapper);
  }
  public override DataEntry? GetData(int index = 0) => Object.Data;
  public override int GetPrefab(int index = 0) => Object.Prefab;
  public override GameObject GetPrefab(GameObject obj) => ZoneSystem.instance.m_locationProxyPrefab;

  public override void AfterPlace(GameObject obj)
  {
    UndoHelper.BeginSubAction();
    var view = obj.GetComponent<ZNetView>();
    HammerHelper.RemoveZDO(view.GetZDO());
    var data = GetData();
    if (data == null) return;
    Dictionary<string, string> pars = [];
    if (!data.TryGetInt(pars, ZDOVars.s_location, out var prefab)) return;
    if (!data.TryGetInt(pars, ZDOVars.s_seed, out var seed)) return;
    var location = ZoneSystem.instance.GetLocation(prefab);
    var ghost = HammerHelper.GetPlacementGhost();
    var position = ghost.transform.position;
    var rotation = ghost.transform.rotation;
    CustomizeSpawnLocation.AllViews = Hammer.AllLocationsObjects;
    CustomizeSpawnLocation.RandomDamage = Hammer.RandomLocationDamage;
    ZoneSystem.instance.SpawnLocation(location, seed, position, rotation, ZoneSystem.SpawnMode.Full, []);
    foreach (var zdo in UndoHelper.GetSpawned())
    {
      if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var spawned))
        PostProcessPlaced(spawned.gameObject);
    }
    CustomizeSpawnLocation.RandomDamage = null;
    CustomizeSpawnLocation.AllViews = false;
    UndoHelper.EndSubAction();
  }
  public override void Activate()
  {
    base.Activate();
    Hammer.SelectEmpty();
  }
}