using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
using WorldEditCommands;
// Code related to repairing objects.
namespace InfinityHammer;
[HarmonyPatch(typeof(Player), nameof(Player.Repair))]
public class Repair
{
  static void Prefix(Player __instance, ItemDrop.ItemData toolItem)
  {
    HideEffects.Active = true;
    Hammer.RemoveToolCosts(toolItem);
    UndoHelper.BeginAction();
    if (!Configuration.RepairAnything) return;
    if (!__instance.InPlaceMode()) return;
    // Normal pieces handled by the game.
    if (__instance.GetHoveringPiece()) return;
    RepairAnything(__instance);
  }
  static void Finalizer(ItemDrop.ItemData toolItem)
  {
    UndoHelper.EndAction();
    HideEffects.Active = false;
    Hammer.RestoreToolCosts(toolItem);
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
    var wearNTear = obj.GetComponent<WearNTear>();
    if (!wearNTear || Time.time - wearNTear.m_lastRepair < 1f) return false;
    var result = RepairShared(obj);
    if (result)
    {
      wearNTear.m_lastRepair = Time.time;
      obj.InvokeRPC(ZNetView.Everybody, "RPC_HealthChanged", [obj.GetZDO().GetFloat(ZDOVars.s_health, wearNTear.m_health)]);
    }
    return result;
  }
  private static bool RepairMineRock(ZNetView obj, MineRock5 mineRock, int index)
  {
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

  private static bool RepairShared(ZNetView obj)
  {
    obj.ClaimOwnership();
    var change = CustomHealth.SetHealth(obj, true);
    if (change == 0f) return false;
    if (change == float.PositiveInfinity || change == float.NegativeInfinity)
      DamageText.instance.ShowText(DamageText.TextType.Heal, obj.transform.position, change);
    else
      DamageText.instance.ShowText(change > 0 ? DamageText.TextType.Heal : DamageText.TextType.Weak, obj.transform.position, Mathf.Abs(change));
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
    if (obj.GetComponent<Player>())
      return RepairPlayer(obj);
    if (Configuration.Invulnerability == InvulnerabilityMode.Off && obj.TryGetComponent(out MineRock5 mineRock))
      return RepairMineRock(obj, mineRock, index);
    return RepairShared(obj);
  }

  private static bool RepairAnything(Player player)
  {
    var range = Configuration.Range > 0f ? Configuration.Range : player.m_maxPlaceDistance;
    var hovered = Selector.GetHovered(player, range, [], [], true);
    if (hovered == null) return false;
    var obj = hovered.Obj;
    UndoHelper.AddEditAction(obj.GetZDO());
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
      player.UseEitr(tool.m_shared.m_attack.m_attackEitr);
      if (tool.m_shared.m_useDurability)
        tool.m_durability -= tool.m_shared.m_useDurabilityDrain;
    }
    return true;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateWearNTearHover))]
public class UnlockRepairDistance
{
  static void Prefix(Player __instance, ref float __state)
  {
    __state = __instance.m_maxPlaceDistance;
    if (Configuration.Range > 0f)
      __instance.m_maxPlaceDistance = Configuration.Range;
  }
  static void Postfix(Player __instance, float __state)
  {
    __instance.m_maxPlaceDistance = __state;
  }

}

[HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Repair))]
public class AdvancedRepair
{
  public static bool Prefix(WearNTear __instance, ref bool __result)
  {
    if (!Configuration.Enabled || !__instance.m_nview) return true;
    var zdo = __instance.m_nview.GetZDO();
    var customHealth = Configuration.OverwriteHealth > 0f || Configuration.Invulnerability != InvulnerabilityMode.Off;
    var hasCustomHealth = zdo.GetFloat(CustomHealth.HashMaxHealth) != 0f;
    if (!customHealth && !hasCustomHealth) return true;
    __result = Repair.RepairStructure(__instance.m_nview);
    return false;
  }
}
