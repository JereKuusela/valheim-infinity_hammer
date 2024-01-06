using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
// Code related to repairing objects.
namespace InfinityHammer;
[HarmonyPatch(typeof(Player), nameof(Player.Repair))]
public class Repair
{
  public static bool IsRepairing = false;
  public static bool Repaired = false;

  private static bool RepairCharacter(ZNetView obj)
  {
    var character = obj.GetComponent<Character>();
    if (!character) return false;
    obj.ClaimOwnership();
    var zdo = obj.GetZDO();
    var currentHealth = zdo.GetFloat(ZDOVars.s_health, character.GetMaxHealth());
    var maxHealth = Configuration.OverwriteHealth > 0f ? Configuration.OverwriteHealth : character.GetMaxHealth();
    var newHealth = Configuration.OverwriteHealth > 0f ? Configuration.OverwriteHealth * 1.000001f : character.GetMaxHealth();
    zdo.Set(ZDOVars.s_maxHealth, maxHealth);
    var heal = newHealth - currentHealth;
    if (heal != 0f)
    {
      // Max health resets on awake if health is equal to max.
      zdo.Set(ZDOVars.s_health, newHealth);
      DamageText.instance.ShowText(heal > 0 ? DamageText.TextType.Heal : DamageText.TextType.Weak, character.GetTopPoint(), Mathf.Abs(heal));
      return true;
    }
    return false;
  }
  private static bool RepairPlayer(ZNetView obj)
  {
    var player = obj.GetComponent<Player>();
    if (!player) return false;
    var heal = player.GetMaxHealth() - player.GetHealth();
    if (heal == 0f) return false;
    player.Heal(heal, true);
    return true;
  }
  public static bool RepairStructure(ZNetView obj)
  {
    obj.ClaimOwnership();
    var wearNTear = obj.GetComponent<WearNTear>();
    if (!wearNTear || Time.time - wearNTear.m_lastRepair < 1f) return false;
    var result = RepairShared(obj, wearNTear.m_health);
    if (result)
    {
      wearNTear.m_lastRepair = Time.time;
      obj.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", [obj.GetZDO().GetFloat("health", wearNTear.m_health)]);
    }
    return result;
  }
  private static bool RepairDestructible(ZNetView obj)
  {
    var destructible = obj.GetComponent<Destructible>();
    if (!destructible) return false;
    return RepairShared(obj, destructible.m_health);
  }
  private static bool RepairTreeBase(ZNetView obj)
  {
    var treeBase = obj.GetComponent<TreeBase>();
    if (!treeBase) return false;
    return RepairShared(obj, treeBase.m_health);
  }
  private static bool RepairTreeLog(ZNetView obj)
  {
    var treeLog = obj.GetComponent<TreeLog>();
    if (!treeLog) return false;
    return RepairShared(obj, treeLog.m_health);
  }
  private static bool RepairMineRock(ZNetView obj, int index)
  {
    var mineRock = obj.GetComponent<MineRock5>();
    if (!mineRock) return false;
    var area = mineRock.GetHitArea(index);
    obj.ClaimOwnership();
    var zdo = obj.GetZDO();
    var max = Configuration.OverwriteHealth > 0f ? Configuration.OverwriteHealth : mineRock.m_health;
    var heal = max - area.m_health;
    if (heal != 0f)
    {
      area.m_health = max;
      mineRock.SaveHealth();
      DamageText.instance.ShowText(heal > 0 ? DamageText.TextType.Heal : DamageText.TextType.Weak, area.m_collider.bounds.center, Mathf.Abs(heal));
      return true;
    }
    var missing = mineRock.m_hitAreas.Find(area => area.m_health <= 0f);
    if (missing != null)
    {
      missing.m_health = max;
      mineRock.SaveHealth();
      mineRock.UpdateMesh();
      DamageText.instance.ShowText(DamageText.TextType.Heal, missing.m_collider.bounds.center, max);
      return true;
    }
    return false;
  }

  private static bool RepairShared(ZNetView obj, float maxHealth)
  {
    obj.ClaimOwnership();
    var zdo = obj.GetZDO();
    var max = Configuration.OverwriteHealth > 0f ? Configuration.OverwriteHealth : maxHealth;
    var heal = max - zdo.GetFloat("health", maxHealth);
    if (heal == 0f) return false;
    zdo.Set("health", max);
    DamageText.instance.ShowText(heal > 0 ? DamageText.TextType.Heal : DamageText.TextType.Weak, obj.transform.position, Mathf.Abs(heal));
    return true;
  }
  private static bool RepairInArea(ZDO zdo, float radius)
  {
    if (radius == 0) return false;
    var position = zdo.m_position;
    var prefab = zdo.m_prefab;
    var toRepair = ZNetScene.instance.m_instances.Values.Where(view =>
      view
      && view.IsValid()
      && view.GetZDO().m_prefab == prefab
      && Vector3.Distance(position, view.GetZDO().m_position) < radius
    ).ToArray();
    var repaired = false;
    foreach (var obj in toRepair)
      repaired |= RepairObject(obj, 0);
    return repaired;
  }
  private static bool RepairObject(ZNetView obj, int index)
  {
    var repaired = false;
    if (RepairPlayer(obj))
      repaired = true;
    if (obj.GetComponent<Player>())
      return repaired;
    if (RepairStructure(obj))
      repaired = true;
    if (RepairCharacter(obj))
      repaired = true;
    if (RepairDestructible(obj))
      repaired = true;
    if (RepairTreeBase(obj))
      repaired = true;
    if (RepairTreeLog(obj))
      repaired = true;
    if (RepairMineRock(obj, index))
      repaired = true;
    return repaired;
  }

  private static bool RepairAnything(Player player)
  {
    var range = Configuration.Range > 0f ? Configuration.Range : player.m_maxPlaceDistance;
    var hovered = Selector.GetHovered(player, range, [], [], true);
    if (hovered == null) return false;
    var obj = hovered.Obj;
    var repaired = RepairObject(obj, hovered.Index);
    if (!repaired) return false;
    var piece = obj.GetComponent<Piece>();
    var name = piece ? piece.m_name : Utils.GetPrefabName(obj.gameObject);
    // Copy paste from the game code.
    piece?.m_placeEffect.Create(obj.transform.position, obj.transform.rotation, null, 1f, -1);
    player.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_repaired", [name]), 0, null);
    var tool = player.GetRightItem();
    if (tool != null)
    {
      player.FaceLookDirection();
      player.m_zanim.SetTrigger(tool.m_shared.m_attack.m_attackAnimation);
      player.UseStamina(tool.m_shared.m_attack.m_attackStamina);
      if (tool.m_shared.m_useDurability)
        tool.m_durability -= tool.m_shared.m_useDurabilityDrain;
    }
    return true;
  }
  public static void Prefix()
  {
    HideEffects.Active = true;
    IsRepairing = true;
    Repaired = false;
  }
  public static void Postfix(Player __instance)
  {
    if (!__instance.InPlaceMode()) return;
    if (!Repaired && Configuration.RepairAnything)
      Repaired = RepairAnything(__instance);
    if (Repaired) Hammer.PostProcessTool(__instance);
  }
  public static void Finalizer()
  {
    IsRepairing = false;
    HideEffects.Active = false;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateWearNTearHover))]
public class UnlockRepairDistance
{
  public static void Prefix(Player __instance, ref float __state)
  {
    __state = __instance.m_maxPlaceDistance;
    if (Configuration.Range > 0f)
      __instance.m_maxPlaceDistance = Configuration.Range;
  }
  public static void Postfix(Player __instance, float __state)
  {
    __instance.m_maxPlaceDistance = __state;
  }

}

[HarmonyPatch(typeof(Character), nameof(Character.UseStamina))]
public class CheckRepair
{
  static void Finalizer()
  {
    if (Repair.IsRepairing) Repair.Repaired = true;
  }
}

[HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Repair))]
public class AdvancedRepair
{
  public static bool Prefix(WearNTear __instance, ref bool __result)
  {
    if (!Repair.IsRepairing || !Configuration.Enabled || !__instance.m_nview || Configuration.OverwriteHealth == 0f) return true;
    __result = Repair.RepairStructure(__instance.m_nview);
    return false;
  }
}
