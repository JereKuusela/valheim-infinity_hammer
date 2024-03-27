using Service;

namespace InfinityHammer;

public class CustomHealth
{

  public static bool SetHealth(ZNetView obj)
  {
    var zdo = obj.GetZDO();
    if (Configuration.Invulnerability == InvulnerabilityMode.Off || Configuration.Invulnerability == InvulnerabilityMode.Legacy)
      return SetCustomHealth(obj, zdo);
    else
      return SetInfiniteHealth(obj, zdo);

  }
  private static bool SetCustomHealth(ZNetView obj, ZDO zdo)
  {
    var changed = false;
    var value = Configuration.Invulnerability == InvulnerabilityMode.Legacy ? 1E30f : Configuration.OverwriteHealth;
    if (value <= 0f) return false;
    if (obj.GetComponent<Character>())
    {
      changed |= zdo.GetFloat(ZDOVars.s_maxHealth) != value;
      zdo.Set(ZDOVars.s_maxHealth, value);
      // Creatures setup max health if current health equals max health.
      zdo.Set(ZDOVars.s_health, value * 1.000001f);
    }
    if (obj.GetComponent<Destructible>() || obj.GetComponent<TreeBase>() || obj.GetComponent<TreeLog>())
    {
      changed |= zdo.GetFloat(ZDOVars.s_health) != value;
      zdo.Set(ZDOVars.s_health, value);
    }
    if (obj.TryGetComponent<MineRock5>(out var mineRock))
    {
      foreach (var area in mineRock.m_hitAreas)
      {
        changed |= area.m_health != value;
        area.m_health = value;
      };
      mineRock.SaveHealth();
    }
    if (obj.TryGetComponent<WearNTear>(out var wear))
    {
      changed |= zdo.GetFloat(ZDOVars.s_health) != value;
      zdo.Set(ZDOVars.s_health, value);
      zdo.RemoveFloat(Hash.BuildingSkillLevel);
      wear.m_healthPercentage = value / wear.m_health;
      wear.SetHealthVisual(wear.m_healthPercentage, false);
    }
    return changed;
  }
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
  private static readonly int HashMaxHealth = "WearNTear.m_health".GetStableHashCode();


  private static bool SetInfiniteHealth(ZNetView obj, ZDO zdo)
  {
    var changed = false;
    // Destructibles, mine rocks and trees have tool tier which can be used for invulnerability.
    if (obj.GetComponent<Destructible>())
    {
      changed |= zdo.GetInt(HashToolTierDestructible) != int.MaxValue / 2;
      zdo.Set(HashFields, true);
      zdo.Set(HashFieldsDestructible, true);
      zdo.Set(HashToolTierDestructible, int.MaxValue / 2);
      obj.LoadFields();
    }
    if (obj.GetComponent<MineRock5>())
    {
      changed |= zdo.GetInt(HashToolTierMineRock5) != int.MaxValue / 2;
      zdo.Set(HashFields, true);
      zdo.Set(HashFieldsMineRock5, true);
      zdo.Set(HashToolTierMineRock5, int.MaxValue / 2);
      obj.LoadFields();
    }
    if (obj.GetComponent<TreeBase>())
    {
      changed |= zdo.GetInt(HashToolTierTreeBase) != int.MaxValue / 2;
      zdo.Set(HashFields, true);
      zdo.Set(HashFieldsTreeBase, true);
      zdo.Set(HashToolTierTreeBase, int.MaxValue / 2);
      obj.LoadFields();
    }
    if (obj.GetComponent<TreeLog>())
    {
      changed |= zdo.GetInt(HashToolTierTreeLog) != int.MaxValue / 2;
      zdo.Set(HashFields, true);
      zdo.Set(HashFieldsTreeLog, true);
      zdo.Set(HashToolTierTreeLog, int.MaxValue / 2);
      obj.LoadFields();
    }
    if (obj.TryGetComponent<WearNTear>(out var wear))
    {
      // Structures ignore damage when below zero health.
      zdo.Set(ZDOVars.s_health, -1f);
      // Max health must be equal or less than health to prevent repair.
      var maxHealth = -1f;
      if (Configuration.Invulnerability == InvulnerabilityMode.Worn)
        maxHealth = -2f;
      if (Configuration.Invulnerability == InvulnerabilityMode.Damaged)
        maxHealth = -4f;
      changed |= zdo.GetFloat(HashMaxHealth) != maxHealth;
      zdo.Set(HashFields, true);
      zdo.Set(HashFieldsWearNTear, true);
      zdo.Set(HashMaxHealth, maxHealth);
      zdo.RemoveFloat(Hash.BuildingSkillLevel);
      obj.LoadFields();
      wear.m_healthPercentage = -1 / maxHealth;
      wear.SetHealthVisual(wear.m_healthPercentage, false);
    }
    // Creatures setup max health if current health equals max health.
    // Creatures are automatically killed if less than zero health.
    if (obj.GetComponent<Character>())
    {
      changed |= zdo.GetFloat(ZDOVars.s_maxHealth) != 1E30f;
      zdo.Set(ZDOVars.s_health, 1E30f * 1.000001f);
      zdo.Set(ZDOVars.s_maxHealth, 1E30f);
    }
    return changed;
  }
}