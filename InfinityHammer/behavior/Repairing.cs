using HarmonyLib;
using UnityEngine;

// Code related to repairing objects.
namespace InfinityHammer {



  [HarmonyPatch(typeof(Player), "Repair")]
  public class Repair {
    public static bool IsRepairing = false;
    public static bool Repaired = false;

    private static bool RepairCharacter(ZNetView obj) {
      var character = obj.GetComponent<Character>();
      if (!character || character.IsPlayer()) return false;
      obj.ClaimOwnership();
      var zdo = obj.GetZDO();
      var current = zdo.GetFloat("health", character.GetMaxHealth());
      var max = Settings.OverwriteHealth > 0f ? Settings.OverwriteHealth : character.GetMaxHealth();
      zdo.Set("max_health", max);
      var heal = max - current;
      if (heal != 0f) {
        // Max health resets on awake if health is equal to max.
        zdo.Set("health", max * 1.000001f);
        DamageText.instance.ShowText(heal > 0 ? DamageText.TextType.Heal : DamageText.TextType.Weak, character.GetTopPoint(), Mathf.Abs(heal));
        return true;
      }
      if (Settings.RepairTaming) {
        Taming.SetTame(character, !character.IsTamed());
        if (character.IsTamed())
          ReplaceMessage.Message = "Tamed " + Localization.instance.Localize(character.m_name);
        else
          ReplaceMessage.Message = "Untamed " + Localization.instance.Localize(character.m_name);
        return true;
      }
      return false;
    }
    private static bool RepairPlayer(ZNetView obj) {
      var player = obj.GetComponent<Player>();
      if (!player) return false;
      var heal = player.GetMaxHealth() - player.GetHealth();
      if (heal == 0f) return false;
      player.Heal(heal, true);
      return true;
    }
    public static bool RepairStructure(ZNetView obj) {
      var wearNTear = obj.GetComponent<WearNTear>();
      if (!wearNTear || Time.time - wearNTear.m_lastRepair < 1f) return false;
      var result = RepairShared(obj, wearNTear.m_health);
      if (result) {
        wearNTear.m_lastRepair = Time.time;
        obj.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", new object[] { obj.GetZDO().GetFloat("health", wearNTear.m_health) });
      }
      return result;
    }
    private static bool RepairDestructible(ZNetView obj) {
      var destructible = obj.GetComponent<Destructible>();
      if (!destructible) return false;
      return RepairShared(obj, destructible.m_health);
    }
    private static bool RepairTreeBase(ZNetView obj) {
      var treeBase = obj.GetComponent<TreeBase>();
      if (!treeBase) return false;
      return RepairShared(obj, treeBase.m_health);
    }
    private static bool RepairTreeLog(ZNetView obj) {
      var treeLog = obj.GetComponent<TreeLog>();
      if (!treeLog) return false;
      return RepairShared(obj, treeLog.m_health);
    }
    private static bool RepairMineRock(ZNetView obj, int index) {
      var mineRock = obj.GetComponent<MineRock5>();
      if (!mineRock) return false;
      var area = mineRock.GetHitArea(index);
      obj.ClaimOwnership();
      var zdo = obj.GetZDO();
      var max = Settings.OverwriteHealth > 0f ? Settings.OverwriteHealth : mineRock.m_health;
      var heal = max - area.m_health;
      if (heal != 0f) {
        area.m_health = max;
        mineRock.SaveHealth();
        DamageText.instance.ShowText(heal > 0 ? DamageText.TextType.Heal : DamageText.TextType.Weak, area.m_collider.bounds.center, Mathf.Abs(heal));
        return true;
      }
      var missing = mineRock.m_hitAreas.Find(area => area.m_health <= 0f);
      if (missing != null) {
        missing.m_health = max;
        mineRock.SaveHealth();
        mineRock.UpdateMesh();
        DamageText.instance.ShowText(DamageText.TextType.Heal, missing.m_collider.bounds.center, max);
        return true;
      }
      return false;
    }

    private static bool RepairShared(ZNetView obj, float maxHealth) {
      obj.ClaimOwnership();
      var zdo = obj.GetZDO();
      var max = Settings.OverwriteHealth > 0f ? Settings.OverwriteHealth : maxHealth;
      var heal = max - zdo.GetFloat("health", maxHealth);
      if (heal == 0f) return false;
      zdo.Set("health", max);
      DamageText.instance.ShowText(heal > 0 ? DamageText.TextType.Heal : DamageText.TextType.Weak, obj.transform.position, Mathf.Abs(heal));
      return true;
    }


    private static bool RepairAnything(Player player) {
      var hovered = Helper.GetHovered(player, player.m_maxPlaceDistance, true);
      if (hovered == null) return false;
      var obj = hovered.Obj;
      var repaired = false;
      if (RepairStructure(obj))
        repaired = true;
      if (RepairCharacter(obj))
        repaired = true;
      if (RepairPlayer(obj))
        repaired = true;
      if (RepairDestructible(obj))
        repaired = true;
      if (RepairTreeBase(obj))
        repaired = true;
      if (RepairTreeLog(obj))
        repaired = true;
      if (RepairMineRock(obj, hovered.Index))
        repaired = true;
      if (!repaired) return false;
      var piece = obj.GetComponent<Piece>();
      var name = piece ? piece.m_name : Utils.GetPrefabName(obj.gameObject);
      // Copy paste from the game code.
      piece?.m_placeEffect.Create(obj.transform.position, obj.transform.rotation, null, 1f, -1);
      player.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_repaired", new string[] { name }), 0, null);
      var tool = player.GetRightItem();
      if (tool != null) {
        player.FaceLookDirection();
        player.m_zanim.SetTrigger(tool.m_shared.m_attack.m_attackAnimation);
        player.UseStamina(tool.m_shared.m_attack.m_attackStamina);
        if (tool.m_shared.m_useDurability)
          tool.m_durability -= tool.m_shared.m_useDurabilityDrain;
      }
      return true;
    }
    public static void Prefix(Player __instance) {
      DisableEffects.Active = true;
      IsRepairing = true;
      Repaired = false;
    }
    public static void Postfix(Player __instance) {
      DisableEffects.Active = false;
      IsRepairing = false;
      if (!__instance.InPlaceMode()) return;
      if (!Repaired && Settings.RepairAnything)
        Repaired = RepairAnything(__instance);
      if (Repaired) Hammer.PostProcessTool(__instance);
    }
  }


  [HarmonyPatch(typeof(Player), "UpdateWearNTearHover")]
  public class UnlockRepairDistance {
    public static void Prefix(Player __instance, ref float __state) {
      __state = __instance.m_maxPlaceDistance;
      if (Settings.RepairRange > 0f)
        __instance.m_maxPlaceDistance = Settings.RepairRange;
    }
    public static void Postfix(Player __instance, float __state) {
      __instance.m_maxPlaceDistance = __state;
    }

  }

  [HarmonyPatch(typeof(Character), "UseStamina")]
  public class CheckRepair {
    public static void Prefix() {
      if (Repair.IsRepairing) Repair.Repaired = true;
    }
  }

  [HarmonyPatch(typeof(WearNTear), "Repair")]
  public class AdvancedRepair {
    public static bool Prefix(WearNTear __instance, ref bool __result) {
      if (!Settings.Enabled || !__instance.m_nview) return true;
      __result = Repair.RepairStructure(__instance.m_nview);
      return false;
    }
  }
}