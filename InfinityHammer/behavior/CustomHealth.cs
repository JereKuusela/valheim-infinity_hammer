namespace InfinityHammer;

public class CustomHealth
{

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
    if (obj.TryGetComponent(out MineRock mineRock))
      change += SetDefaultHealth(zdo, mineRock);
    if (obj.TryGetComponent(out MineRock5 mineRock5))
      change += SetDefaultHealth(zdo, mineRock5);
    if (obj.TryGetComponent(out WearNTear wear))
      change += SetDefaultHealth(zdo, wear);
    return change;
  }
  private static float SetDefaultHealth(ZDO zdo, Destructible destructible)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, destructible.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    if (RemoveToolTier(destructible, zdo))
      return float.NegativeInfinity;
    return destructible.m_health - prev;
  }
  private static float SetDefaultHealth(ZDO zdo, TreeBase treeBase)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeBase.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    if (RemoveToolTier(treeBase, zdo))
      return float.NegativeInfinity;
    return treeBase.m_health - prev;
  }
  private static float SetDefaultHealth(ZDO zdo, TreeLog treeLog)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeLog.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    if (RemoveToolTier(treeLog, zdo))
      return float.NegativeInfinity;
    return treeLog.m_health - prev;
  }
  private static float SetDefaultHealth(ZDO zdo, MineRock mineRock)
  {
    var original = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<MineRock>();
    if (!original) return 0f;
    var change = original.m_health - mineRock.m_health;
    mineRock.m_health = original.m_health;

    zdo.RemoveFloat(Hashes.HealthMineRock);
    var repair = RemoveCurrentHealth(mineRock, zdo);

    if (RemoveToolTier(mineRock, zdo))
      return float.NegativeInfinity;
    return change != 0f ? change : repair;
  }
  private static float SetDefaultHealth(ZDO zdo, MineRock5 mineRock)
  {
    var change = 0f;
    foreach (var area in mineRock.m_hitAreas)
    {
      change += mineRock.m_health - area.m_health;
      area.m_health = mineRock.m_health;
    }
    mineRock.SaveHealth();
    if (RemoveToolTier(mineRock, zdo))
      return float.NegativeInfinity;
    return change;
  }
  private static float SetDefaultHealth(ZDO zdo, WearNTear wear)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, wear.m_health);
    zdo.RemoveFloat(ZDOVars.s_health);
    var maxHealth = zdo.GetFloat(Hashes.MaxHealth);
    if (maxHealth != 0f)
    {
      wear.m_health = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<WearNTear>().m_health;
      zdo.RemoveFloat(Hashes.MaxHealth);
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
    if (obj.TryGetComponent(out MineRock mineRock))
      change += SetCustomHealth(zdo, mineRock, value);
    if (obj.TryGetComponent(out MineRock5 mineRock5))
      change += SetCustomHealth(zdo, mineRock5, value);
    if (obj.TryGetComponent(out WearNTear wear))
      change += SetCustomHealth(zdo, wear, value);
    return change;
  }
  private static float SetCustomHealth(ZDO zdo, Destructible destructible, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, destructible.m_health);
    zdo.Set(ZDOVars.s_health, health);
    if (RemoveToolTier(destructible, zdo))
      return float.NegativeInfinity;
    return health - prev;
  }
  private static float SetCustomHealth(ZDO zdo, TreeBase treeBase, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeBase.m_health);
    zdo.Set(ZDOVars.s_health, health);
    if (RemoveToolTier(treeBase, zdo))
      return float.NegativeInfinity;
    return health - prev;
  }
  private static float SetCustomHealth(ZDO zdo, TreeLog treeLog, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, treeLog.m_health);
    zdo.Set(ZDOVars.s_health, health);
    if (RemoveToolTier(treeLog, zdo))
      return float.NegativeInfinity;
    return health - prev;
  }
  private static float SetCustomHealth(ZDO zdo, MineRock mineRock, float health)
  {
    zdo.Set(Hashes.HasFields, true);
    zdo.Set(Hashes.HashFieldsMineRock, true);
    // To avoid bloating data, update the default health.
    zdo.Set(Hashes.HealthMineRock, health);
    var change = health - mineRock.m_health;
    mineRock.m_health = health;
    var repair = RemoveCurrentHealth(mineRock, zdo);
    if (RemoveToolTier(mineRock, zdo))
      return float.NegativeInfinity;
    return change != 0f ? change : repair;
  }
  private static float SetCustomHealth(ZDO zdo, MineRock5 mineRock, float health)
  {
    var change = 0f;
    foreach (var area in mineRock.m_hitAreas)
    {
      // Check to not restore removed pieces.
      if (area.m_health <= 0f) continue;
      change += health - area.m_health;
      area.m_health = health;
    }
    // Data is saved for all pieces regardless, so no point to try avoiding data bloat.
    mineRock.SaveHealth();
    if (RemoveToolTier(mineRock, zdo))
      return float.NegativeInfinity;
    return change;
  }
  private static float SetCustomHealth(ZDO zdo, WearNTear wear, float health)
  {
    var prev = zdo.GetFloat(ZDOVars.s_health, wear.m_health);
    zdo.Set(ZDOVars.s_health, health);
    var maxHealth = zdo.GetFloat(Hashes.MaxHealth);
    if (maxHealth != 0f)
    {
      wear.m_health = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<WearNTear>().m_health;
      zdo.RemoveFloat(Hashes.MaxHealth);
    }
    zdo.RemoveFloat(Hashes.BuildingSkillLevel);
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
    if (obj.TryGetComponent(out MineRock mineRock))
      changed |= SetInfiniteHealth(zdo, mineRock);
    if (obj.TryGetComponent(out MineRock5 mineRock5))
      changed |= SetInfiniteHealth(zdo, mineRock5);
    if (obj.TryGetComponent(out WearNTear wear))
      changed |= SetInfiniteHealth(zdo, wear);
    return changed ? float.PositiveInfinity : 0f;
  }

  private static bool SetInfiniteHealth(ZDO zdo, Destructible destructible)
  {
    var changed = zdo.GetInt(Hashes.ToolTierDestructible) != int.MaxValue / 2;
    zdo.RemoveFloat(ZDOVars.s_health);
    zdo.Set(Hashes.HasFields, true);
    zdo.Set(Hashes.HashFieldsDestructible, true);
    zdo.Set(Hashes.ToolTierDestructible, int.MaxValue / 2);
    destructible.m_minToolTier = int.MaxValue / 2;
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, TreeBase treeBase)
  {
    var changed = zdo.GetInt(Hashes.ToolTierTreeBase) != int.MaxValue / 2;
    zdo.RemoveFloat(ZDOVars.s_health);
    zdo.Set(Hashes.HasFields, true);
    zdo.Set(Hashes.HashFieldsTreeBase, true);
    zdo.Set(Hashes.ToolTierTreeBase, int.MaxValue / 2);
    treeBase.m_minToolTier = int.MaxValue / 2;
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, TreeLog treeLog)
  {
    var changed = zdo.GetInt(Hashes.ToolTierTreeLog) != int.MaxValue / 2;
    zdo.RemoveFloat(ZDOVars.s_health);
    zdo.Set(Hashes.HasFields, true);
    zdo.Set(Hashes.HashFieldsTreeLog, true);
    zdo.Set(Hashes.ToolTierTreeLog, int.MaxValue / 2);
    treeLog.m_minToolTier = int.MaxValue / 2;
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, MineRock mineRock)
  {
    var changed = zdo.GetInt(Hashes.ToolTierMineRock) != int.MaxValue / 2;
    var change = RemoveCurrentHealth(mineRock, zdo);
    mineRock.m_minToolTier = int.MaxValue / 2;
    zdo.Set(Hashes.HasFields, true);
    zdo.Set(Hashes.HashFieldsMineRock, true);
    zdo.Set(Hashes.ToolTierMineRock, int.MaxValue / 2);
    return changed || change != 0f;
  }
  private static bool SetInfiniteHealth(ZDO zdo, MineRock5 mineRock)
  {
    var changed = zdo.GetInt(Hashes.ToolTierMineRock5) != int.MaxValue / 2;
    zdo.Set(Hashes.HasFields, true);
    zdo.Set(Hashes.HashFieldsMineRock5, true);
    zdo.Set(Hashes.ToolTierMineRock5, int.MaxValue / 2);
    mineRock.m_minToolTier = int.MaxValue / 2;
    foreach (var area in mineRock.m_hitAreas)
    {
      if (area.m_health <= 0f) continue;
      area.m_health = int.MaxValue / 2;
    }
    mineRock.SaveHealth();
    return changed;
  }
  private static bool SetInfiniteHealth(ZDO zdo, WearNTear wear)
  {
    var currentMaxHealth = zdo.GetFloat(Hashes.MaxHealth);
    // Structures ignore damage when below zero health.
    zdo.Set(ZDOVars.s_health, -1f);
    // Max health must be equal or less than health to prevent repair.
    var maxHealth = -1f;
    if (Configuration.Invulnerability == InvulnerabilityMode.Worn)
      maxHealth = -2f;
    if (Configuration.Invulnerability == InvulnerabilityMode.Damaged)
      maxHealth = -4f;
    if (Configuration.PreserveWear && Configuration.Invulnerability != InvulnerabilityMode.Off)
    {
      if (currentMaxHealth < 0f) // already invulnerable, preserve current type of invulnerability
        maxHealth = currentMaxHealth;
      else
      {
        // not invulnerable, convert to equivalent wear level
        // but only damaged pieces, the other should respect InvulnerabilityMode
        if (wear.GetHealthPercentage() <= 0.25f)
          maxHealth = -4f;
        else if (wear.GetHealthPercentage() <= 0.75f)
          maxHealth = -2f;
      }
    }

    var changed = currentMaxHealth != maxHealth;
    zdo.Set(Hashes.HasFields, true);
    zdo.Set(Hashes.HashFieldsWearNTear, true);
    zdo.Set(Hashes.MaxHealth, maxHealth);
    zdo.RemoveFloat(Hashes.BuildingSkillLevel);
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

  private static float RemoveCurrentHealth(MineRock mineRock, ZDO zdo)
  {
    if (mineRock.m_hitAreas == null) return 0f;
    var change = 0f;
    for (var i = 0; i < mineRock.m_hitAreas.Length; i++)
    {
      var hash = "Health" + i.ToString();
      var health = zdo.GetFloat(hash);
      // Check to not restore removed pieces.
      if (health <= 0f) continue;
      change += mineRock.m_health - health;
      zdo.RemoveFloat(hash);
    }
    return change;
  }
  private static bool RemoveToolTier(MineRock5 obj, ZDO zdo)
  {
    if (zdo.GetInt(Hashes.ToolTierMineRock5, -1) < 0) return false;
    zdo.RemoveInt(Hashes.ToolTierMineRock5);
    obj.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<MineRock5>()?.m_minToolTier ?? 0;
    return true;
  }
  private static bool RemoveToolTier(MineRock obj, ZDO zdo)
  {
    if (zdo.GetInt(Hashes.ToolTierMineRock, -1) < 0) return false;
    zdo.RemoveInt(Hashes.ToolTierMineRock);
    obj.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<MineRock>()?.m_minToolTier ?? 0;
    return true;
  }
  private static bool RemoveToolTier(TreeBase obj, ZDO zdo)
  {
    if (zdo.GetInt(Hashes.ToolTierTreeBase, -1) < 0) return false;
    zdo.RemoveInt(Hashes.ToolTierTreeBase);
    obj.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<TreeBase>()?.m_minToolTier ?? 0;
    return true;
  }
  private static bool RemoveToolTier(TreeLog obj, ZDO zdo)
  {
    if (zdo.GetInt(Hashes.ToolTierTreeLog, -1) < 0) return false;
    zdo.RemoveInt(Hashes.ToolTierTreeLog);
    obj.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<TreeLog>()?.m_minToolTier ?? 0;
    return true;
  }
  private static bool RemoveToolTier(Destructible obj, ZDO zdo)
  {
    if (zdo.GetInt(Hashes.ToolTierDestructible, -1) < 0) return false;
    zdo.RemoveInt(Hashes.ToolTierDestructible);
    obj.m_minToolTier = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<Destructible>()?.m_minToolTier ?? 0;
    return true;
  }
}