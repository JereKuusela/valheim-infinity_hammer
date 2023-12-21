using System;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;

public class BaseSelection
{
  protected static readonly GameObject SnapObj = new()
  {
    name = "_snappoint",
    layer = LayerMask.NameToLayer("piece"),
    tag = "snappoint",
  };

#nullable disable
  ///<summary>Copy of the selected entity. Only needed for the placement ghost because armor and item stands have a different model depending on their state.</summary>
  protected GameObject SelectedObject = null;
#nullable enable
  public bool SingleUse = false;
  public string ExtraDescription = "";
  public virtual bool IsTool => false;
  public virtual bool TerrainGrid => false;
  public virtual bool Continuous => false;
  public virtual bool PlayerHeight => false;
  public virtual float MaxPlaceDistance(float value) => Configuration.Range > 0f ? Configuration.Range : value;
  public Piece GetSelectedPiece() => SelectedObject ? SelectedObject.GetComponent<Piece>() : null!;
  public void Destroy() => UnityEngine.Object.Destroy(SelectedObject);

  public virtual void Postprocess(Vector3? scale)
  {
    if (scale.HasValue && Scaling.IsScalingSupported(SelectedObject))
      SetScale(scale.Value);
    Scaling.Get().SetScale(SelectedObject.transform.localScale);
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public void SetScale(Vector3 scale)
  {
    if (SelectedObject)
      SelectedObject.transform.localScale = scale;
  }
  protected void Postprocess(GameObject obj, ZDOData? zdo)
  {
    if (zdo == null) return;
    SetLevel(obj, zdo.GetInt(Hash.Level, -1));
    SetGrowth(obj, zdo.GetInt(Hash.Growth, -1));
    SetWear(obj, zdo.GetInt(Hash.Wear, -1));
    SetText(obj, zdo.GetString(Hash.Text, ""));
  }
  protected static void ResetColliders(GameObject obj, GameObject original)
  {
    var colliders = obj.GetComponentsInChildren<Collider>();
    var originalColliders = original.GetComponentsInChildren<Collider>();
    if (colliders.Length != originalColliders.Length)
    {
      InfinityHammer.Log.LogWarning("Object has a different amount of colliders than the original: Unable to reset colliders.");
      return;
    }
    for (var i = 0; i < colliders.Length; i++)
    {
      colliders[i].enabled = originalColliders[i].enabled;
      colliders[i].isTrigger = originalColliders[i].isTrigger;
    }
  }
  private static void SetLevel(GameObject obj, int level)
  {
    if (level == -1) return;
    if (obj.GetComponent<Character>() is not { } character) return;
    if (obj.GetComponentInChildren<LevelEffects>() is not { } effect) return;
    effect.m_character = character;
    effect.SetupLevelVisualization(level);
  }
  private static float Convert(int value, float defaultValue)
  {
    if (value == 0) return 0.1f;
    if (value == 1) return 0.5f;
    if (value == 2) return 1f;
    return defaultValue;
  }
  private static void SetWear(GameObject obj, int wear)
  {
    if (wear == -1) return;
    if (obj.GetComponent<WearNTear>() is not { } wearNTear) return;
    wearNTear.SetHealthVisual(Convert(wear, 1f), false);
  }
  private static void SetText(GameObject obj, string text)
  {
    if (obj.GetComponent<Sign>() is not { } sign) return;
    sign.m_textWidget.text = text;
  }
  private static void SetGrowth(GameObject obj, int growth)
  {
    if (growth == -1) return;
    if (obj.GetComponent<Plant>() is not { } plant) return;
    var healthy = growth == 0;
    var unhealthy = growth == 1;
    var healthyGrown = growth == 2;
    var unhealthyGrown = growth == 3;
    if (plant.m_healthyGrown)
    {
      plant.m_healthy.SetActive(healthy);
      plant.m_unhealthy.SetActive(unhealthy);
      plant.m_healthyGrown.SetActive(healthyGrown);
      plant.m_unhealthyGrown.SetActive(unhealthyGrown);
    }
    else
    {
      plant.m_healthy.SetActive(healthy || healthyGrown);
      plant.m_unhealthy.SetActive(unhealthy || unhealthyGrown);
    }
  }
  protected static void SetExtraInfo(GameObject obj, string extraInfo, ZDOData data)
  {
    if (obj.TryGetComponent<Sign>(out var sign))
    {
      if (extraInfo == "")
        extraInfo = data.GetString(Hash.Text, extraInfo);
      else
        data.Set(Hash.Text, extraInfo);
      sign.m_textWidget.text = extraInfo;
    }
    if (obj.GetComponent<TeleportWorld>() && extraInfo != "")
    {
      data.Set(Hash.Tag, extraInfo);
    }
    if (obj.GetComponent<Tameable>() && extraInfo != "")
    {
      data.Set(Hash.TamedName, extraInfo);
    }
    if (obj.TryGetComponent<ItemStand>(out var itemStand))
    {
      var split = extraInfo.Split(':');
      var name = split[0];
      var variant = Parse.Int(split, 1, 0);
      var quality = Parse.Int(split, 2, 1);
      if (extraInfo == "")
      {
        name = data.GetString(Hash.Item, name);
        variant = data.GetInt(Hash.Variant, variant);
        quality = data.GetInt(Hash.Quality, quality);
      }
      else
      {
        data.Set(Hash.Item, name);
        data.Set(Hash.Variant, variant);
        data.Set(Hash.Quality, quality);
      }
      itemStand.SetVisualItem(name, variant, quality);
    }
    if (obj.TryGetComponent<ArmorStand>(out var armorStand))
    {
      var split = extraInfo.Split(':');
      var pose = Parse.Int(split, 0, 0);
      if (extraInfo == "")
        pose = data.GetInt(Hash.Pose, pose);
      else
        data.Set(Hash.Pose, pose);
      armorStand.m_pose = pose;
      armorStand.m_poseAnimator.SetInteger("Pose", pose);
      SetItemHack.Hack = true;
      for (var i = 0; i < armorStand.m_slots.Count; i++)
      {
        var name = Parse.String(split, i * 2 + 2, "");
        var variant = Parse.Int(split, i * 2 + 3, 0);
        if (extraInfo == "")
        {
          name = data.GetString($"{i}_item", name);
          variant = data.GetInt($"{i}_variant", variant);
        }
        else
        {
          data.Set($"{i}_item", name);
          data.Set($"{i}_variant", variant);
        }
        armorStand.SetVisualItem(i, name, variant);
      }
      SetItemHack.Hack = false;
    }
  }
  public virtual ZDOData GetData(int index = 0) => new();
  public virtual int GetPrefab(int index = 0) => 0;
  public virtual bool IsScalingSupported() => false;
  public virtual void UpdateZDOs(Action<ZDOData> action)
  {
  }
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
    BindCommand.SetMode("");
  }
  public virtual void Deactivate()
  {

  }
}
