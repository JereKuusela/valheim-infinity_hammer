using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;

public class BaseSelection
{
  protected static BaseSelection? ActiveSelection = null;
  protected static readonly GameObject SnapObj = new()
  {
    name = "_snappoint",
    layer = LayerMask.NameToLayer("piece"),
    tag = "snappoint",
  };

#nullable disable
  ///<summary>Copy of the selected entity. Only needed for the placement ghost because armor and item stands have a different model depending on their state.</summary>
  protected GameObject SelectedPrefab = null;
#nullable enable
  public bool SingleUse = false;
  public string ExtraDescription = "";
  public virtual bool IsTool => false;
  public virtual bool TerrainGrid => false;
  public virtual bool Continuous => false;
  public virtual bool PlayerHeight => false;
  public virtual float MaxPlaceDistance(float value) => Configuration.Range > 0f ? Configuration.Range : value;
  public Piece GetSelectedPiece() => SelectedPrefab ? SelectedPrefab.GetComponent<Piece>() : null!;
  public void Destroy() => Object.Destroy(SelectedPrefab);
  public void SetScale(Vector3 scale)
  {
    if (!IsScalingSupported())
      scale = Vector3.one;
    if (SelectedPrefab)
      SelectedPrefab.transform.localScale = scale;
    Scaling.Set(scale);
    var player = Helper.GetPlayer();
    if (player.m_placementGhost)
      player.m_placementGhost.transform.localScale = scale;
  }
  public virtual ZDOData GetData(int index = 0) => new();
  public virtual int GetPrefab(int index = 0) => 0;
  public virtual bool IsScalingSupported() => false;
  public virtual GameObject GetPrefab(GameObject obj) => obj;
  public virtual void AfterPlace(GameObject obj) { }


  ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
  public static void PostProcessPlaced(GameObject obj)
  {
    var view = obj.GetComponent<ZNetView>();
    if (!Configuration.Enabled || !view) return;
    var zdo = view.GetZDO();
    var piece = obj.GetComponent<Piece>();
    if (piece)
    {
      piece.m_canBeRemoved = true;
      // Creator data is only interesting for actual targets. Dummy components will have these both as false.
      if (piece.m_randomTarget || piece.m_primaryTarget)
      {
        if (Configuration.NoCreator)
        {
          zdo.Set(Hash.Creator, 0L);
          piece.m_creator = 0;
        }
        else
          piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
      }
    }
    var character = obj.GetComponent<Character>();
    if (Configuration.OverwriteHealth > 0f)
    {
      if (character)
        zdo.Set(Hash.MaxHealth, Configuration.OverwriteHealth);
      if (obj.GetComponent<TreeLog>() || obj.GetComponent<WearNTear>() || obj.GetComponent<Destructible>() || obj.GetComponent<TreeBase>() || character)
        zdo.Set(Hash.Health, Configuration.OverwriteHealth);
      var mineRock = obj.GetComponent<MineRock5>();
      if (mineRock)
      {
        foreach (var area in mineRock.m_hitAreas) area.m_health = Configuration.OverwriteHealth;
        mineRock.SaveHealth();
      }
    }
    if (obj.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (zdo.GetByteArray(ZDOVars.s_roomData) == null)
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
  }
}
