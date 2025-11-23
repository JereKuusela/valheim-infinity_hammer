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
  static void Prefix(Player __instance)
  {
    HideEffects.Active = true;
    UndoHelper.BeginAction();
    if (!Configuration.RepairAnything) return;
    if (!__instance.InPlaceMode()) return;
    // Normal pieces handled by the game.
    if (__instance.GetHoveringPiece()) return;
    RepairAnything(__instance);
  }
  static void Finalizer()
  {
    UndoHelper.EndAction();
    HideEffects.Active = false;
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
  private static bool RepairMineRock(ZNetView obj, MineRock mineRock, int index)
  {
    var zdo = obj.GetZDO();
    var targetArea = mineRock.GetHitArea(index)?.bounds.center ?? Player.m_localPlayer.transform.position;
    int? missing = null;
    float minDistance = float.MaxValue;
    for (var i = 0; i < mineRock.m_hitAreas.Length; i++)
    {
      var hash = ("Health" + i).GetStableHashCode();
      if (zdo.GetFloat(hash, 1f) > 0f) continue;
      var distance = Vector3.Distance(mineRock.m_hitAreas[i].bounds.center, targetArea);
      if (distance < minDistance)
      {
        missing = i;
        minDistance = distance;
      }
    }
    if (missing != null)
    {
      var hash = ("Health" + missing).GetStableHashCode();
      zdo.RemoveFloat(hash);
      DamageText.instance.ShowText(DamageText.TextType.Heal, mineRock.m_hitAreas[missing.Value].bounds.center, GetMaxHealth(mineRock.m_health));
      mineRock.UpdateVisability();
      return true;
    }
    return false;
  }
  private static bool RepairMineRock(MineRock5 mineRock, int index)
  {
    var targetArea = mineRock.GetHitArea(index)?.m_collider.bounds.center ?? Player.m_localPlayer.transform.position;
    var max = Configuration.OverwriteHealth > 0f ? Configuration.OverwriteHealth : mineRock.m_health;
    MineRock5.HitArea? missing = null;
    float minDistance = float.MaxValue;
    foreach (var area in mineRock.m_hitAreas)
    {
      if (area.m_health > 0f) continue;
      var distance = Vector3.Distance(area.m_collider.bounds.center, targetArea);
      if (distance < minDistance)
      {
        missing = area;
        minDistance = distance;
      }
    }
    if (missing != null)
    {
      missing.m_health = max;
      mineRock.SaveHealth();
      mineRock.UpdateMesh();
      DamageText.instance.ShowText(DamageText.TextType.Heal, missing.m_collider.bounds.center, GetMaxHealth(mineRock.m_health));
      return true;
    }
    return false;
  }
  private static float GetMaxHealth(float defaultValue) => Configuration.Invulnerability == InvulnerabilityMode.Off ? Configuration.OverwriteHealth == 0f ? defaultValue : Configuration.OverwriteHealth : float.PositiveInfinity;
  private static bool RepairShared(ZNetView obj)
  {
    obj.ClaimOwnership();
    var change = CustomHealth.SetHealth(obj, true);
    if (change == 0f) return false;
    DamageText.instance.ShowText(DamageText.TextType.Heal, obj.transform.position, change);
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
    if (RepairShared(obj))
      return true;
    if (obj.TryGetComponent(out MineRock5 mineRock5))
      return RepairMineRock(mineRock5, index);
    if (obj.TryGetComponent(out MineRock mineRock))
      return RepairMineRock(obj, mineRock, index);
    return false;
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
    var hasCustomHealth = zdo.GetFloat(Hashes.MaxHealth) != 0f;
    if (!customHealth && !hasCustomHealth) return true;
    __result = Repair.RepairStructure(__instance.m_nview);
    return false;
  }
}
