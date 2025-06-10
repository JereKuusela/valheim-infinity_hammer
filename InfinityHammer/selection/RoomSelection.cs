using System;
using System.Linq;
using Data;
using Service;
using SoftReferenceableAssets;
using UnityEngine;
using WorldEditCommands;

namespace InfinityHammer;


public class RoomSelection : BaseSelection
{
  // Unity doesn't run scripts for inactive objects.
  // So an inactive object is used to store the selected object.
  // This mimics the ZNetScene.m_namedPrefabs behavior.
  private readonly GameObject Wrapper;
  private readonly SelectedObject Object;
  public override float DungeonRoomSnapMultiplier => 100f;
  public override void Destroy()
  {
    base.Destroy();
    UnityEngine.Object.Destroy(Wrapper);
  }
  private readonly bool PlaceEmptyRoom;

  public RoomSelection(int hash, bool emptyRoom)
  {
    PlaceEmptyRoom = emptyRoom;
    var room = DungeonDB.m_instance.GetRoom(hash);
    if (room == null) throw new InvalidOperationException("Room not found.");
    Wrapper = new GameObject();
    Wrapper.SetActive(false);
    SelectedPrefab = HammerHelper.SafeInstantiateRoom(room, emptyRoom, Wrapper);
    Object = new(hash, false, null, SelectedPrefab);
    var connections = SelectedPrefab.GetComponentsInChildren<RoomConnection>(includeInactive: false).ToList();
    var snaps = connections.Select(c => c.transform.position).ToList();
    Snapping.CreateSnapPoints(SelectedPrefab, snaps);
  }
  public override DataEntry? GetData(int index = 0) => Object.Data;
  public override int GetPrefab(int index = 0) => Object.Prefab;
  public override GameObject GetPrefab(GameObject obj)
  {
    var dummy = new GameObject
    {
      name = "Blueprint"
    };
    dummy.AddComponent<Piece>();
    return dummy;
  }

  public override void AfterPlace(GameObject obj)
  {
    var pos = obj.transform.position;
    var rot = obj.transform.rotation;
    var dg = DungeonRooms.FindClosestDg(pos);
    if (dg == null)
    {
      Log.Error("DungeonGenerator not found.");
      return;
    }
    var roomData = DungeonDB.m_instance.GetRoom(Object.Prefab);
    if (roomData == null)
    {
      Log.Error("Room not found.");
      return;
    }
    roomData.m_prefab.Load();
    Room component = roomData.m_prefab.Asset.GetComponent<Room>();
    ZNetView[] objects = Utils.GetEnabledComponentsInChildren<ZNetView>(roomData.m_prefab.Asset);
    RandomSpawn[] randomSpawns = Utils.GetEnabledComponentsInChildren<RandomSpawn>(roomData.m_prefab.Asset);
    for (int i = 0; i < randomSpawns.Length; i++)
      randomSpawns[i].Prepare();
    int seed = (int)pos.x * 4271 + (int)pos.y * 9187 + (int)pos.z * 2134;
    UnityEngine.Random.State state = UnityEngine.Random.state;
    UnityEngine.Random.InitState(seed);
    Vector3 position = component.transform.position;
    Quaternion quaternion = Quaternion.Inverse(component.transform.rotation);
    UnityEngine.Random.InitState(seed);
    foreach (RandomSpawn randomSpawn in randomSpawns)
    {
      Vector3 point = quaternion * (randomSpawn.gameObject.transform.position - position);
      Vector3 pos2 = pos + rot * point;
      randomSpawn.Randomize(pos2, null, dg);
    }
    UndoHelper.BeginSubAction();
    if (!PlaceEmptyRoom)
    {
      foreach (var znetView in objects)
      {
        if (!znetView.gameObject.activeSelf) continue;
        var basePos = quaternion * (znetView.gameObject.transform.position - position);
        var objPos = pos + rot * basePos;
        Quaternion baseRot = quaternion * znetView.gameObject.transform.rotation;
        Quaternion objRot = rot * baseRot;
        var gameObject = UnityEngine.Object.Instantiate(znetView.gameObject, objPos, objRot);
        GameObjectExtentions.HoldReferenceTo(gameObject, roomData.m_prefab);
      }
    }
    foreach (var znetView in objects)
      znetView.gameObject.SetActive(false);
    UndoHelper.AddEditAction(dg.m_nview.GetZDO());
    DungeonRooms.AddRoom(dg.m_nview, Object.Prefab, obj.transform.position, obj.transform.rotation);
    UnityEngine.Object.Instantiate(roomData.m_prefab.m_loadedAsset, pos, rot, dg.transform);
    UndoHelper.EndSubAction();
    UnityEngine.Random.state = state;
    foreach (var randomSpawn in randomSpawns)
      randomSpawn.Reset();
    foreach (var znetView in objects)
      znetView.gameObject.SetActive(true);
    roomData.m_prefab.Release();
  }
  public override void Activate()
  {
    base.Activate();
    Hammer.SelectEmpty();
  }
}