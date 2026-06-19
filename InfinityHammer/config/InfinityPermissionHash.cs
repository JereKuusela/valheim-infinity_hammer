
namespace InfinityHammer;

public static class InfinityPermissionHash
{
  public const string Section = "infinityhammer";

  public static readonly int NoCost = "no_cost".GetStableHashCode();

  public static readonly int IgnoreWards = "ignore_wards".GetStableHashCode();

  public static readonly int IgnoreNoBuild = "ignore_no_build".GetStableHashCode();

  public static readonly int AllowInDungeons = "allow_in_dungeons".GetStableHashCode();

  public static readonly int IgnoreOtherRestrictions = "ignore_other_restrictions".GetStableHashCode();

  public static readonly int RemoveAnything = "remove_anything".GetStableHashCode();

  public static readonly int DisableLoot = "disable_loot".GetStableHashCode();

  public static readonly int RepairAnything = "repair_anything".GetStableHashCode();

  public static readonly int NoCreator = "no_creator".GetStableHashCode();

  public static readonly int NoTarget = "no_target".GetStableHashCode();

  public static readonly int NoPhysics = "no_physics".GetStableHashCode();

  public static readonly int NoRemove = "no_remove".GetStableHashCode();

  public static readonly int OverwriteHealth = "overwrite_health".GetStableHashCode();

  public static readonly int Invulnerability = "invulnerability".GetStableHashCode();

  public static readonly int Range = "range".GetStableHashCode();

  public static readonly int ToolsEnabled = "tools_enabled".GetStableHashCode();
}