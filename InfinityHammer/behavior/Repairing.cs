using HarmonyLib;
using Service;
using UnityEngine;
using System;

// Code related to repairing objects.
namespace InfinityHammer {



  [HarmonyPatch(typeof(Player), "Repair")]
  public class Repair {
    public static bool IsRepairing = false;
    public static bool Repaired = false;

    private static bool RepairCharacter(ZNetView obj) {
      var character = obj.GetComponent<Character>();
      if (character && character.GetHealthPercentage() < 1f) {
        obj.ClaimOwnership();
        if (Settings.OverwriteHealth > 0f)
          obj.GetZDO().Set("max_health", Settings.OverwriteHealth);
        character.Heal(float.MaxValue, true);
        return true;
      }
      return false;
    }
    private static bool RepairStructure(ZNetView obj) {
      var wearNTear = obj.GetComponent<WearNTear>();
      // Repair behavior is changed elsewhere.
      if (wearNTear && wearNTear.Repair())
        return true;
      return false;
    }


    private static bool RepairAnything(Player player) {
      var obj = Helper.GetHovered(player);
      if (!obj) return false;
      var repaired = false;
      if (RepairStructure(obj))
        repaired = true;
      if (RepairCharacter(obj))
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
      IsRepairing = true;
      Repaired = false;
    }
    public static void Postfix(Player __instance) {
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
  public class SetRepairHealth {
    public static bool Prefix(WearNTear __instance) {
      if (Settings.OverwriteHealth == 0) return true;
      var netView = __instance.m_nview;
      if (!netView.IsValid()) return true;
      netView.ClaimOwnership();
      // Copypaste from RPC_Repair.
      netView.GetZDO().Set("health", Settings.OverwriteHealth);
      netView.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", new object[] { Settings.OverwriteHealth });
      return false;
    }
  }
}