using System;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;

public static class Hammer
{

  public static bool AllLocationsObjects = false;
  public static bool RandomLocationDamage = false;

  public static void RemoveSelection()
  {
    Selection.Clear();
  }
  public static bool IsHammer(string name) => Configuration.HammerTools.Contains(name.ToLower()) || ToolManager.Tools.ContainsKey(name.ToLower());
  public static bool IsHammer(GameObject obj) => obj && IsHammer(Utils.GetPrefabName(obj));
  public static bool IsHammer(ItemDrop.ItemData item) => item != null && IsHammer(item.m_dropPrefab);
  public static bool HasHammer(Player player) => player && IsHammer(player.GetRightItem());
  public static void Equip()
  {
    var player = Helper.GetPlayer();
    if (HasHammer(player)) return;
    var inventory = player.GetInventory();
    var item = inventory.m_inventory.Find(item => IsHammer(item));
    if (item == null)
    {
      throw new InvalidOperationException($"Unable to find the hammer.");
    };
    player.EquipItem(item);
  }
  public static bool IsTool(ItemDrop.ItemData item) => item != null && item.m_shared.m_buildPieces != null;

  public static bool HasAnyTool()
  {
    var player = Helper.GetPlayer();
    if (!player) return false;
    return IsTool(player.GetRightItem());
  }
  public static string GetTool()
  {
    var player = Helper.GetPlayer();
    if (!player) return "";
    var item = player.GetRightItem();
    if (item == null) return "";
    return Utils.GetPrefabName(item.m_dropPrefab).ToLower();
  }
  public static void Clear()
  {
    var player = Player.m_localPlayer;
    if (!player) return;
    player.SetSelectedPiece(new(0, 0));
  }
  public static void Place()
  {
    var player = Player.m_localPlayer;
    if (!player) return;
    player.m_placePressedTime = Time.time;
    player.m_lastToolUseTime = 0f;
    player.UpdatePlacement(true, 0f);
  }
  ///<summary>Replaces LocationProxy with the actual location.</summary>
  public static void SpawnLocation(ZNetView view)
  {
    Helper.RemoveZDO(view.GetZDO());
    var data = Selection.GetData();
    if (data == null) return;
    var prefab = data.GetInt(Hash.Location, 0);
    var seed = data.GetInt(Hash.Seed, 0);
    var location = ZoneSystem.instance.GetLocation(prefab);
    var ghost = Helper.GetPlacementGhost();
    var position = ghost.transform.position;
    var rotation = ghost.transform.rotation;
    CustomizeSpawnLocation.AllViews = AllLocationsObjects;
    CustomizeSpawnLocation.RandomDamage = RandomLocationDamage;
    ZoneSystem.instance.SpawnLocation(location, seed, position, rotation, ZoneSystem.SpawnMode.Full, new());
    foreach (var zdo in Undo.Objects)
    {
      if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var obj))
        PostProcessPlaced(obj.gameObject);
    }
    CustomizeSpawnLocation.RandomDamage = null;
    CustomizeSpawnLocation.AllViews = false;
  }

  ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
  public static void PostProcessPlaced(GameObject obj)
  {
    var view = obj.GetComponent<ZNetView>();
    if (!Configuration.Enabled || !view) return;
    var zdo = view.GetZDO();
    var piece = obj.GetComponent<Piece>();
    if (piece)
    {
      piece.m_canBeRemoved = true;
      // Creator data is only interesting for actual targets. Dummy components will have these both as false.
      if (piece.m_randomTarget || piece.m_primaryTarget)
      {
        if (Configuration.NoCreator)
        {
          zdo.Set(Hash.Creator, 0L);
          piece.m_creator = 0;
        }
        else
          piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
      }
    }
    var character = obj.GetComponent<Character>();
    if (Configuration.OverwriteHealth > 0f)
    {
      if (character)
        zdo.Set(Hash.MaxHealth, Configuration.OverwriteHealth);
      if (obj.GetComponent<TreeLog>() || obj.GetComponent<WearNTear>() || obj.GetComponent<Destructible>() || obj.GetComponent<TreeBase>() || character)
        zdo.Set(Hash.Health, Configuration.OverwriteHealth);
      var mineRock = obj.GetComponent<MineRock5>();
      if (mineRock)
      {
        foreach (var area in mineRock.m_hitAreas) area.m_health = Configuration.OverwriteHealth;
        mineRock.SaveHealth();
      }
    }
    if (obj.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (zdo.GetInt("rooms") == 0)
        dg.Generate(ZoneSystem.SpawnMode.Full);
    }
  }

  ///<summary>Restores durability and stamina to counter the usage.</summary>
  public static void PostProcessTool(Player obj)
  {
    var item = obj.GetRightItem();
    if (item == null) return;
    if (Configuration.NoCost)
      obj.UseStamina(-item.m_shared.m_attack.m_attackStamina);
    if (Configuration.NoCost && item.m_shared.m_useDurability)
      item.m_durability += item.m_shared.m_useDurabilityDrain;
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
