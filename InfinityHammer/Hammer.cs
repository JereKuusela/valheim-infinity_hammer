using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InfinityTools;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;

public static class Hammer
{
  public static bool AllLocationsObjects = false;
  public static bool RandomLocationDamage = false;

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
      build.m_selectedPiece[(int)build.m_selectedCategory] = new(-1, -1);
  }
  public static void SelectRepair()
  {
    var build = Helper.GetPlayer().m_buildPieces;
    if (build)
      build.m_selectedPiece[(int)build.m_selectedCategory] = new(0, 0);
  }
  public static void SelectRepairIfEmpty()
  {
    var build = Helper.GetPlayer().m_buildPieces;
    foreach (var category in build.m_categories)
      if (build.m_selectedPiece[(int)category].x == -1)
        build.m_selectedPiece[(int)category] = new(0, 0);
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
  public static void OpenBuildMenu()
  {
    var player = Helper.GetPlayer();
    if (EquipInfinityHammer())
    {
      // Some mods trigger when place mode is set. So make sure it is retriggered if the hammer is already equipped.
      player.SetPlaceMode(player.m_buildPieces);
    }
    var pt = player.m_buildPieces;
    if (pt)
    {
      pt.m_selectedCategory = pt.m_categories.Count > 0 ? pt.m_categories[0] : 0;
      pt.m_selectedPiece[(int)pt.m_selectedCategory] = new(0, 0);
    }
    Hud.instance.m_pieceSelectionWindow.SetActive(true);
    Hud.instance.m_closePieceSelection = 0;
    Hud.instance.UpdateBuild(Player.m_localPlayer, true);
  }
  public static bool IsInfinityHammer(ItemDrop.ItemData item) => item != null && item.m_customData != null && item.m_customData.ContainsKey("infinity_hammer");
  public static bool IsInfinityHammer(PieceTable pt) => pt && pt.name == "_InfinityHammerPieceTable";
  public static bool EquipInfinityHammer()
  {
    var player = Helper.GetPlayer();
    var rightItem = player.GetRightItem();
    if (rightItem != null && rightItem.m_customData.ContainsKey("infinity_hammer")) return true;
    var inventory = player.GetInventory();
    var infinityHammer = inventory.m_inventory.Find(IsInfinityHammer);
    if (infinityHammer == null)
    {
      var freeSlot = inventory.FindEmptySlot(true);
      var data = new Dictionary<string, string>();
      data["infinity_hammer"] = "true";
      if (!inventory.AddItem("Hammer", 1, 100f, freeSlot, false, 1, 0, player.GetPlayerID(), Game.instance.GetPlayerProfile().GetName(), data, 0, true))
        throw new InvalidOperationException("Unable to add the hammer to inventory.");
    }
    infinityHammer = inventory.m_inventory.Find(item => item != null && item.m_customData.ContainsKey("infinity_hammer"));
    if (infinityHammer == null) throw new InvalidOperationException("Unable to find the hammer in inventory.");
    player.EquipItem(infinityHammer);
    return false;
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
    if (IsInfinityHammer(item)) return "infinity_hammer";
    if (item == null) return "";
    return Utils.GetPrefabName(item.m_dropPrefab).ToLower();
  }
}


[HarmonyPatch]
public class CustomHammer
{

  private static Sprite? cachedSprite = null;
  [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetIcon)), HarmonyPostfix]
  public static Sprite GetIcon(Sprite result, ItemDrop.ItemData __instance)
  {
    if (!Hammer.IsInfinityHammer(__instance)) return result;
    if (cachedSprite == null) cachedSprite = SpriteHelper.FindSprite("+🛠️");
    return cachedSprite ?? result;
  }

  [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverName)), HarmonyPostfix]
  public static string GetHoverName(string result, ItemDrop __instance)
  {
    var item = __instance.m_itemData;
    return Hammer.IsInfinityHammer(item) ? "Infinity Hammer" : result;
  }

  private static PieceTable? cachedPieceTable = null;

  [HarmonyPatch(typeof(Player), nameof(Player.SetPlaceMode)), HarmonyPrefix]
  public static void SetPlaceModePrefix(Player __instance, ref PieceTable buildPieces)
  {
    var item = __instance.GetRightItem();
    if (!Hammer.IsInfinityHammer(item)) return;
    if (HammerMenuCommand.CurrentMode == MenuMode.Builds && HammerMenuCommand.CurrentFilter != "")
    {
      var items = CustomMenu.GetBuildItems();
      var build = items.FirstOrDefault(i => i.name.Equals(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase));
      if (build != null)
      {
        buildPieces = build.m_itemData.m_shared.m_buildPieces;
        return;
      }
    }
    if (cachedPieceTable == null)
    {
      var obj = new GameObject("_InfinityHammerPieceTable");
      UnityEngine.Object.DontDestroyOnLoad(obj);
      var pt = obj.AddComponent<PieceTable>();
      pt.m_canRemoveFeasts = true;
      pt.m_canRemovePieces = true;
      pt.m_skill = Skills.SkillType.None;
      cachedPieceTable = pt;
    }
    buildPieces = cachedPieceTable;
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