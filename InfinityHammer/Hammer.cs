using System;
using HarmonyLib;
using InfinityTools;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;

public static class Hammer
{
  public static bool AllLocationsObjects = false;
  public static bool RandomLocationDamage = false;

  public static void Clear()
  {
    Selection.Clear();
    var player = Helper.GetPlayer();
    player.SetSelectedPiece(new Vector2Int(0, 0));
    player.SetupPlacementGhost();
  }
  public static void Place()
  {
    var player = Player.m_localPlayer;
    if (!player) return;
    player.m_placePressedTime = Time.time;
    player.m_lastToolUseTime = 0f;
    player.UpdatePlacement(true, 0f);
  }
  public static void SelectEmpty()
  {
    var build = Helper.GetPlayer().m_buildPieces;
    if (build)
      build.m_selectedPiece[(int)build.GetSelectedCategory()] = new Vector2Int(-1, -1);
  }
  public static void SelectRepair()
  {
    var build = Helper.GetPlayer().m_buildPieces;
    if (build)
      build.m_selectedPiece[(int)build.GetSelectedCategory()] = new Vector2Int(0, 0);
  }
  public static void SelectRepairIfEmpty()
  {
    var build = Helper.GetPlayer().m_buildPieces;
    if (build && build.m_selectedPiece[(int)build.GetSelectedCategory()].x == -1)
      build.m_selectedPiece[(int)build.GetSelectedCategory()] = new Vector2Int(0, 0);
  }

  private static bool OriginalUseDurability = false;
  private static float OriginalUseStamina = 0f;
  private static float OriginalUseEitr = 0f;
  private static bool OriginalCanRemoveFeasts = false;
  private static bool OriginalCanRemovePieces = false;

  public static void AddRemoveAnything(ItemDrop.ItemData item)
  {
    if (item == null || !Configuration.RemoveAnything || !item.m_shared.m_buildPieces) return;
    OriginalCanRemoveFeasts = item.m_shared.m_buildPieces.m_canRemoveFeasts;
    OriginalCanRemovePieces = item.m_shared.m_buildPieces.m_canRemovePieces;
    item.m_shared.m_buildPieces.m_canRemoveFeasts = true;
    item.m_shared.m_buildPieces.m_canRemovePieces = true;
  }
  public static void RestoreRemoveAnything(ItemDrop.ItemData item)
  {
    if (item == null || !Configuration.RemoveAnything || !item.m_shared.m_buildPieces) return;
    item.m_shared.m_buildPieces.m_canRemoveFeasts = OriginalCanRemoveFeasts;
    item.m_shared.m_buildPieces.m_canRemovePieces = OriginalCanRemovePieces;
  }
  public static void RemoveToolCosts(ItemDrop.ItemData item)
  {
    if (item == null || !Configuration.NoCost) return;
    OriginalUseDurability = item.m_shared.m_useDurability;
    OriginalUseStamina = item.m_shared.m_attack.m_attackStamina;
    OriginalUseEitr = item.m_shared.m_attack.m_attackEitr;
    item.m_shared.m_useDurability = false;
    item.m_shared.m_attack.m_attackStamina = 0f;
    item.m_shared.m_attack.m_attackEitr = 0f;
  }
  public static void RestoreToolCosts(ItemDrop.ItemData item)
  {
    if (item == null || !Configuration.NoCost) return;
    item.m_shared.m_useDurability = OriginalUseDurability;
    item.m_shared.m_attack.m_attackStamina = OriginalUseStamina;
    item.m_shared.m_attack.m_attackEitr = OriginalUseEitr;
  }
  public static bool IsHammer(string name) => ToolManager.Tools.ContainsKey(name.ToLower());
  public static bool IsHammer(GameObject obj) => obj && IsHammer(Utils.GetPrefabName(obj));
  public static bool IsHammer(ItemDrop.ItemData item) => item != null && IsHammer(item.m_dropPrefab);
  public static bool HasHammer(Player player) => player && IsHammer(player.GetRightItem());
  public static void Equip()
  {
    var player = Helper.GetPlayer();
    if (HasHammer(player)) return;
    var inventory = player.GetInventory();
    var item = inventory.m_inventory.Find(IsHammer) ?? throw new InvalidOperationException($"Unable to find the hammer.");

    player.EquipItem(item);
  }

  public static bool Is(ItemDrop.ItemData item) => item != null && item.m_shared.m_buildPieces != null;

  public static bool HasAny()
  {
    var player = Helper.GetPlayer();
    return player && Is(player.GetRightItem());
  }
  public static string Get()
  {
    var player = Helper.GetPlayer();
    if (!player) return "";
    var item = player.GetRightItem();
    if (item == null) return "";
    return Utils.GetPrefabName(item.m_dropPrefab).ToLower();
  }
}



[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
public class UnfreezeOnUnequip
{
  static void Prefix(Humanoid __instance, ItemDrop.ItemData item)
  {
    if (__instance != Player.m_localPlayer || item == null) return;
    if (Configuration.UnfreezeOnUnequip) Position.Unfreeze();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class DisableSnapCycleWhenTyping
{
  static void Prefix(Player __instance, ref int __state)
  {
    __state = __instance.m_manualSnapPoint;
  }
  static void Postfix(Player __instance, ref int __state)
  {
    if (!__instance.TakeInput()) __instance.m_manualSnapPoint = __state;
  }
}