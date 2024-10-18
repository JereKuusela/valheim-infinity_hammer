using System.Runtime.Remoting.Messaging;

namespace InfinityHammer;

public class CustomHealth
{
  private static readonly int HashFields = "HasFields".GetStableHashCode();
  private static readonly int HashFieldsDestructible = "HasFieldsDestructible".GetStableHashCode();
  private static readonly int HashFieldsMineRock5 = "HasFieldsMineRock5".GetStableHashCode();
  private static readonly int HashFieldsTreeBase = "HasFieldsTreeBase".GetStableHashCode();
  private static readonly int HashFieldsTreeLog = "HasFieldsTreeLog".GetStableHashCode();
  private static readonly int HashToolTierDestructible = "Destructible.m_minToolTier".GetStableHashCode();
  private static readonly int HashToolTierMineRock5 = "MineRock5.m_minToolTier".GetStableHashCode();
  private static readonly int HashToolTierTreeBase = "TreeBase.m_minToolTier".GetStableHashCode();
  private static readonly int HashToolTierTreeLog = "TreeLog.m_minToolTier".GetStableHashCode();
  private static readonly int HashFieldsWearNTear = "HasFieldsWearNTear".GetStableHashCode();
  public static readonly int HashMaxHealth = "WearNTear.m_health".GetStableHashCode();

  public static float SetHealth(ZNetView obj, bool isRepair)
  {
    var zdo = obj.GetZDO();
    if (Configuration.Invulnerability == InvulnerabilityMode.Off && Configuration.OverwriteHealth == 0f)
      return SetDefaultHealth(obj, zdo, isRepair);
    else if (Configuration.Invulnerability == InvulnerabilityMode.Off || Configuration.Invulnerability == InvulnerabilityMode.Legacy)
      return SetCustomHealth(obj, zdo);
    else
      return SetInfiniteHealth(obj, zdo);

  }
  private static float SetDefaultHealth(ZNetView obj, ZDO zdo, bool isRepair)
  {
    if (!isRepair) return 0f;
    var change = 0f;
    if (obj.TryGetComponent(out Character character))
      change += SetDefaultHealth(zdo, character);
    if (obj.TryGetComponent(out Destructible destructible))
      change += SetDefaultHealth(zdo, destructible);
    if (obj.TryGetComponent(out TreeBase treeBase))
      change += SetDefaultHealth(zdo, treeBase);
    if (obj.TryGetComponent(out TreeLog treeLog))
      change += SetDefaultHealth(zdo, treeLog);
    if (obj.TryGetComponent(out MineRock5 mineRock))
      change += SetDefaultHealth(zdo, mineRock);
    if (obj.TryGetComponent(out WearNTear wear))
      change += SetDefaultHealth(zdo, wear);
    return change;
  }
  private static float SetDefaultHealth(ZDO zdo, Destructible destructible)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, destructible.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    if (zdo.GetInt(HashToolTierDestructible, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierDestructible);
      destructible.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<Destructible>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return destructible.m_health - prev;
  }
  private static float SetDefaultHealth(ZDO zdo, TreeBase treeBase)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeBase.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    if (zdo.GetInt(HashToolTierTreeBase, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierTreeBase);
      treeBase.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<TreeBase>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return treeBase.m_health - prev;
  }
  private static float SetDefaultHealth(ZDO zdo, TreeLog treeLog)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeLog.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    if (zdo.GetInt(HashToolTierTreeLog, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierTreeLog);
      treeLog.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<TreeLog>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return treeLog.m_health - prev;
  }
  private static float SetDefaultHealth(ZDO zdo, MineRock5 mineRock)
  {
    var change = 0f;
    foreach (var area in mineRock.m_hitAreas)
    {
      change += mineRock.m_health - area.m_health;
      area.m_health = mineRock.m_health;
    };
    mineRock.SaveHealth();
    if (zdo.GetInt(HashToolTierMineRock5, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierMineRock5);
      mineRock.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<MineRock5>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return change;
  }
  private static float SetDefaultHealth(ZDO zdo, WearNTear wear)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, wear.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    var maxHealth = zdo.GetFloat(HashMaxHealth);
    if (maxHealth != 0f)
    {
      wear.m_health = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<WearNTear>().m_health;
      zdo.RemoveFloat(HashMaxHealth);
    }
    UpdateVisual(zdo, wear);
    return maxHealth < 0f ? float.NegativeInfinity : wear.m_health - prev;
  }
  private static float SetDefaultHealth(ZDO zdo, Character character)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, zdo.GetFloat(ZDOVars.s_maxHealth, character.m_health));
    zdo.RemoveFloat(ZDOVars.s_health);
    character.SetupMaxHealth();
    return zdo.GetFloat(ZDOVars.s_maxHealth) - prev;
  }


  private static float SetCustomHealth(ZNetView obj, ZDO zdo)
  {
    var change = 0f;
    var value = Configuration.Invulnerability == InvulnerabilityMode.Legacy ? 1E30f : Configuration.OverwriteHealth;
    if (value <= 0f) return 0f;
    if (obj.TryGetComponent(out Character character))
      change += SetCustomHealth(zdo, character, value);
    if (obj.TryGetComponent(out Destructible destructible))
      change += SetCustomHealth(zdo, destructible, value);
    if (obj.TryGetComponent(out TreeBase treeBase))
      change += SetCustomHealth(zdo, treeBase, value);
    if (obj.TryGetComponent(out TreeLog treeLog))
      change += SetCustomHealth(zdo, treeLog, value);
    if (obj.TryGetComponent(out MineRock5 mineRock))
      change += SetCustomHealth(zdo, mineRock, value);
    if (obj.TryGetComponent(out WearNTear wear))
      change += SetCustomHealth(zdo, wear, value);
    return change;
  }
  private static float SetCustomHealth(ZDO zdo, Destructible destructible, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, destructible.m_health);
    zdo.Set(ZDOVars.s_health, health);
    if (zdo.GetInt(HashToolTierDestructible, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierDestructible);
      destructible.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<Destructible>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return health - prev;
  }
  private static float SetCustomHealth(ZDO zdo, TreeBase treeBase, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeBase.m_health);
    zdo.Set(ZDOVars.s_health, health);
    if (zdo.GetInt(HashToolTierTreeBase, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierTreeBase);
      treeBase.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<TreeBase>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return health - prev;
  }
  private static float SetCustomHealth(ZDO zdo, TreeLog treeLog, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeLog.m_health);
    zdo.Set(ZDOVars.s_health, health);
    if (zdo.GetInt(HashToolTierTreeLog, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierTreeLog);
      treeLog.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<TreeLog>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return health - prev;
  }
  private static float SetCustomHealth(ZDO zdo, MineRock5 mineRock, float health)
  {
    var change = 0f;
    foreach (var area in mineRock.m_hitAreas)
    {
      change += health - area.m_health;
      area.m_health = health;
    };
    mineRock.SaveHealth();
    if (zdo.GetInt(HashToolTierMineRock5, -1) > -1)
    {
      zdo.RemoveInt(HashToolTierMineRock5);
      mineRock.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<MineRock5>()?.m_minToolTier ?? 0;
      return float.NegativeInfinity;
    }
    return change;
  }
  private static float SetCustomHealth(ZDO zdo, WearNTear wear, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, wear.m_health);
    zdo.Set(ZDOVars.s_health, health);
    var maxHealth = zdo.GetFloat(HashMaxHealth);
    if (maxHealth != 0f)
    {
      wear.m_health = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<WearNTear>().m_health;
      zdo.RemoveFloat(HashMaxHealth);
    }
    zdo.RemoveFloat(Hash.BuildingSkillLevel);
    UpdateVisual(zdo, wear);
    return maxHealth < 0f ? float.NegativeInfinity : health - prev;
  }
  private static float SetCustomHealth(ZDO zdo, Character character, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, zdo.GetFloat(ZDOVars.s_maxHealth, character.m_health));
    zdo.Set(ZDOVars.s_maxHealth, health);
    // Creatures setup max health if current health equals max health.
    zdo.Set(ZDOVars.s_health, health * 1.000001f);
    return health - prev;
  }


  private static float SetInfiniteHealth(ZNetView obj, ZDO zdo)
  {
    var changed = false;
    if (obj.TryGetComponent(out Character character))
      changed |= SetInfiniteHealth(zdo, character);
    if (obj.TryGetComponent(out Destructible destructible))
      changed |= SetInfiniteHealth(zdo, destructible);
    if (obj.TryGetComponent(out TreeBase treeBase))
      changed |= SetInfiniteHealth(zdo, treeBase);
    if (obj.TryGetComponent(out TreeLog treeLog))
      changed |= SetInfiniteHealth(zdo, treeLog);
    if (obj.TryGetComponent(out MineRock5 mineRock))
      changed |= SetInfiniteHealth(zdo, mineRock);
    if (obj.TryGetComponent(out WearNTear wear))
      changed |= SetInfiniteHealth(zdo, wear);
    return changed ? float.PositiveInfinity : 0f;
  }

  private static bool SetInfiniteHealth(ZDO zdo, Destructible destructible)
  {
    var changed = zdo.GetInt(HashToolTierDestructible) != int.MaxValue / 2;
    zdo.RemoveFloat(ZDOVars.s_health);
    zdo.Set(HashFields, true);
    zdo.Set(HashFieldsDestructible, true);
    zdo.Set(HashToolTierDestructible, int.MaxValue / 2);
    destructible.m_minToolTier = int.MaxValue / 2;
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, TreeBase treeBase)
  {
    var changed = zdo.GetInt(HashToolTierTreeBase) != int.MaxValue / 2;
    zdo.RemoveFloat(ZDOVars.s_health);
    zdo.Set(HashFields, true);
    zdo.Set(HashFieldsTreeBase, true);
    zdo.Set(HashToolTierTreeBase, int.MaxValue / 2);
    treeBase.m_minToolTier = int.MaxValue / 2;
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, TreeLog treeLog)
  {
    var changed = zdo.GetInt(HashToolTierTreeLog) != int.MaxValue / 2;
    zdo.RemoveFloat(ZDOVars.s_health);
    zdo.Set(HashFields, true);
    zdo.Set(HashFieldsTreeLog, true);
    zdo.Set(HashToolTierTreeLog, int.MaxValue / 2);
    treeLog.m_minToolTier = int.MaxValue / 2;
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, MineRock5 mineRock)
  {
    var changed = zdo.GetInt(HashToolTierMineRock5) != int.MaxValue / 2;
    zdo.Set(HashFields, true);
    zdo.Set(HashFieldsMineRock5, true);
    zdo.Set(HashToolTierMineRock5, int.MaxValue / 2);
    foreach (var area in mineRock.m_hitAreas)
      area.m_health = mineRock.m_health;
    mineRock.SaveHealth();
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, WearNTear wear)
  {
    // Structures ignore damage when below zero health.
    zdo.Set(ZDOVars.s_health, -1f);
    // Max health must be equal or less than health to prevent repair.
    var maxHealth = -1f;
    if (Configuration.Invulnerability == InvulnerabilityMode.Worn)
      maxHealth = -2f;
    if (Configuration.Invulnerability == InvulnerabilityMode.Damaged)
      maxHealth = -4f;
    var changed = zdo.GetFloat(HashMaxHealth) != maxHealth;
    zdo.Set(HashFields, true);
    zdo.Set(HashFieldsWearNTear, true);
    zdo.Set(HashMaxHealth, maxHealth);
    zdo.RemoveFloat(Hash.BuildingSkillLevel);
    wear.m_health = maxHealth;
    UpdateVisual(zdo, wear);
    return changed;
  }
  // Creatures setup max health if current health equals max health.
  // Creatures are automatically killed if less than zero health.
  private static bool SetInfiniteHealth(ZDO zdo, Character character)
  {
    var changed = zdo.GetFloat(ZDOVars.s_maxHealth) != 1E30f;
    zdo.Set(ZDOVars.s_health, 1E30f * 1.000001f);
    zdo.Set(ZDOVars.s_maxHealth, 1E30f);
    return changed;
  }


  private static void UpdateVisual(ZDO zdo, WearNTear wear)
  {
    wear.m_healthPercentage = zdo.GetFloat(ZDOVars.s_health, wear.m_health) / wear.m_health;
    wear.SetHealthVisual(wear.m_healthPercentage, false);
  }
}