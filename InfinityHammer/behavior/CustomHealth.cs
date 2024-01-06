namespace InfinityHammer;

public class CustomHealth
{

  public static void SetHealth(ZNetView obj)
  {
    var zdo = obj.GetZDO();
    if (Configuration.Invulnerability == InvulnerabilityMode.Off)
      SetCustomHealth(obj, zdo);
    else
      SetInfiniteHealth(obj, zdo);

  }
  private static void SetCustomHealth(ZNetView obj, ZDO zdo)
  {
    var value = Configuration.OverwriteHealth;
    if (obj.GetComponent<Character>())
    {
      zdo.Set(ZDOVars.s_maxHealth, value);
      // Creatures setup max health if current health equals max health.
      zdo.Set(ZDOVars.s_health, value * 1.000001f);
    }
    if (obj.GetComponent<Destructible>() || obj.GetComponent<TreeBase>() || obj.GetComponent<TreeLog>())
      zdo.Set(ZDOVars.s_health, value);
    if (obj.TryGetComponent<MineRock5>(out var mineRock))
    {
      foreach (var area in mineRock.m_hitAreas) area.m_health = value;
      mineRock.SaveHealth();
    }
    if (obj.TryGetComponent<WearNTear>(out var wear))
    {
      zdo.Set(ZDOVars.s_health, value);
      wear.m_healthPercentage = value / wear.m_health;
      wear.SetHealthVisual(wear.m_healthPercentage, false);
    }
  }
  private static void SetInfiniteHealth(ZNetView obj, ZDO zdo)
  {

    // Destructibles, mine rocks and trees have tool tier which can be used for invulnerability.
    if (obj.GetComponent<Destructible>())
    {
      zdo.Set("HasFields", true);
      zdo.Set("HasFieldsDestructible", true);
      zdo.Set("Destructible.m_minToolTier", int.MaxValue / 2);
      obj.LoadFields();
    }
    if (obj.GetComponent<MineRock5>())
    {
      zdo.Set("HasFields", true);
      zdo.Set("HasFieldsMineRock5", true);
      zdo.Set("MineRock5.m_minToolTier", int.MaxValue / 2);
      obj.LoadFields();
    }
    if (obj.GetComponent<TreeBase>())
    {
      zdo.Set("HasFields", true);
      zdo.Set("HasFieldsTreeBase", true);
      zdo.Set("TreeBase.m_minToolTier", int.MaxValue / 2);
      obj.LoadFields();
    }
    if (obj.GetComponent<TreeLog>())
    {
      zdo.Set("HasFields", true);
      zdo.Set("HasFieldsTreeLog", true);
      zdo.Set("TreeLog.m_minToolTier", int.MaxValue / 2);
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
      zdo.Set("HasFields", true);
      zdo.Set("HasFieldsWearNTear", true);
      zdo.Set("WearNTear.m_health", maxHealth);
      obj.LoadFields();
      wear.m_healthPercentage = -1 / maxHealth;
      wear.SetHealthVisual(wear.m_healthPercentage, false);
    }
    // Creatures setup max health if current health equals max health.
    // Creatures are automatically killed if less than zero health.
    if (obj.GetComponent<Character>())
    {
      zdo.Set(ZDOVars.s_health, 1E30f * 1.000001f);
      zdo.Set(ZDOVars.s_maxHealth, 1E30f);
    }
  }
}