namespace InfinityHammer;

public static class Hashes
{
  public static readonly int XRaySteamName = "steamName".GetStableHashCode();
  public static readonly int XRaySteamID = "steamID".GetStableHashCode();
  public static readonly int XRayCreatedID = "xray_created".GetStableHashCode();
  public static readonly int HasFields = "HasFields".GetStableHashCode();
  public static readonly int HasFieldsStaticTarget = "HasFieldsStaticTarget".GetStableHashCode();
  public static readonly int StaticTargetPrimaryTarget = "StaticTarget.m_primaryTarget".GetStableHashCode();
  public static readonly int StaticTargetRandomTarget = "StaticTarget.m_randomTarget".GetStableHashCode();
  public static readonly int HasFieldsStaticPhysics = "HasFieldsStaticPhysics".GetStableHashCode();
  public static readonly int StaticPhysicsFall = "StaticPhysics.m_fall".GetStableHashCode();
  public static readonly int StaticPhysicsPushUp = "StaticPhysics.m_pushUp".GetStableHashCode();
  public static readonly int HashFieldsDestructible = "HasFieldsDestructible".GetStableHashCode();
  public static readonly int HashFieldsMineRock = "HasFieldsMineRock".GetStableHashCode();
  public static readonly int HashFieldsMineRock5 = "HasFieldsMineRock5".GetStableHashCode();
  public static readonly int HashFieldsTreeBase = "HasFieldsTreeBase".GetStableHashCode();
  public static readonly int HashFieldsTreeLog = "HasFieldsTreeLog".GetStableHashCode();
  public static readonly int HashFieldsWearNTear = "HasFieldsWearNTear".GetStableHashCode();
  public static readonly int ToolTierDestructible = "Destructible.m_minToolTier".GetStableHashCode();
  public static readonly int ToolTierMineRock = "MineRock.m_minToolTier".GetStableHashCode();
  public static readonly int ToolTierMineRock5 = "MineRock5.m_minToolTier".GetStableHashCode();
  public static readonly int ToolTierTreeBase = "TreeBase.m_minToolTier".GetStableHashCode();
  public static readonly int ToolTierTreeLog = "TreeLog.m_minToolTier".GetStableHashCode();
  public static readonly int HealthMineRock = "MineRock.m_health".GetStableHashCode();
  public static readonly int MaxHealth = "WearNTear.m_health".GetStableHashCode();
  public static readonly int Growth = "Growth".GetStableHashCode();
  public static readonly int Wear = "Wear".GetStableHashCode();
  public static readonly int Fall = "Fall".GetStableHashCode();
  public static readonly int Collision = "Collision".GetStableHashCode();
  public static readonly int Render = "Render".GetStableHashCode();
  public static readonly int Restrict = "Restrict".GetStableHashCode();
  public static readonly int Interact = "Interact".GetStableHashCode();
  public static readonly int Text = "Text".GetStableHashCode();
  public static readonly int BuildingSkillLevel = "BuildingSkillLevel".GetStableHashCode();
}
