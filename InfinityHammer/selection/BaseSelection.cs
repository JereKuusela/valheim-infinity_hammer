using System.IO;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;

public class BaseSelection
{
  protected static BaseSelection? ActiveSelection = null;
#nullable disable
  ///<summary>Copy of the selected entity. Only needed for the placement ghost because armor and item stands have a different model depending on their state.</summary>
  protected GameObject SelectedPrefab;
#nullable enable
  public bool SingleUse = false;
  public virtual bool IsTool => false;
  public virtual bool TerrainGrid => false;
  public virtual bool Continuous => false;
  public virtual bool PlayerHeight => false;
  public virtual float DungeonRoomSnapMultiplier => 1f;
  public virtual float SnapMultiplier => 1f;
  public virtual float MaxPlaceDistance(float value) => Configuration.Range > 0f ? Configuration.Range : value;
  public Piece GetSelectedPiece() => SelectedPrefab ? SelectedPrefab.GetComponent<Piece>() : null!;
  public virtual void Destroy() => Object.Destroy(SelectedPrefab);

  public virtual DataEntry? GetData(int index = 0) => null;
  public virtual int GetPrefab(int index = 0) => 0;
  public virtual bool IsScalingSupported() => false;
  public virtual GameObject GetPrefab(GameObject obj) => obj;
  public virtual void AfterPlace(GameObject obj) { }

  public void SetScale(Vector3 value)
  {
    if (SelectedPrefab && IsScalingSupported())
      SelectedPrefab.transform.localScale = value;
  }
  ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
  public static void PostProcessPlaced(GameObject obj)
  {
    var view = obj.GetComponent<ZNetView>();
    if (!Configuration.Enabled || !view) return;
    if (obj.TryGetComponent<Piece>(out var piece))
    {
      piece.m_canBeRemoved = true;
      NoCreator.Set(view, piece);

    }
    CustomHealth.SetHealth(view, false);
    if (obj.TryGetComponent<DungeonGenerator>(out var dg))
    {
      var zdo = view.GetZDO();
      var data = zdo.GetByteArray(ZDOVars.s_roomData);
      var rooms = zdo.GetInt(ZDOVars.s_rooms);
      if (rooms == 0 && data == null)
        dg.Generate(ZoneSystem.SpawnMode.Full);
    }
  }


  public virtual void Activate()
  {
    ActiveSelection?.Deactivate();
    BindCommand.SetMode("");
    ActiveSelection = this;
  }
  public virtual void Deactivate()
  {
    Hammer.SelectRepairIfEmpty();
  }

}
