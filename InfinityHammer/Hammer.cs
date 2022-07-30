using System;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;
public enum Tool {
  Hammer,
  Hoe
}
public static class Hammer {

  public static bool AllLocationsObjects = false;
  public static bool RandomLocationDamage = false;

  public static void CopyState(ZNetView view, int index = 0) {
    if (!Configuration.CopyState || !view) return;
    var zdo = view.GetZDO();
    if (zdo == null || !zdo.IsValid()) return;
    var data = Selection.GetData(index);
    if (data == null) return;
    Helper.CopyData(data, zdo);
  }
  public static void RemoveSelection() {
    Selection.Clear();
    if (Configuration.UnfreezeOnSelect) Position.Unfreeze();
  }
  public static bool IsTool(string name, Tool tool) => tool == Tool.Hammer ? Configuration.HammerTools.Contains(name.ToLower()) : Configuration.HoeTools.Contains(name.ToLower());
  public static bool IsTool(GameObject obj, Tool tool) => obj && IsTool(Utils.GetPrefabName(obj), tool);
  public static bool IsTool(ItemDrop.ItemData item, Tool tool) => item != null && IsTool(item.m_dropPrefab, tool);
  public static bool HasTool(Player player, Tool tool) => player && IsTool(player.GetRightItem(), tool);
  public static void Equip(Tool tool) {
    var player = Helper.GetPlayer();
    if (HasTool(player, tool)) return;
    var inventory = player.GetInventory();
    var item = inventory.m_inventory.Find(item => IsTool(item, tool));
    if (item == null) {
      throw new InvalidOperationException($"Unable to find the tool {tool.ToString()}.");
    };
    player.EquipItem(item);
  }
  public static void Clear() {
    var player = Player.m_localPlayer;
    if (!player) return;
    player.SetSelectedPiece(new(0, 0));
  }
  public static void Place() {
    var player = Player.m_localPlayer;
    if (!player) return;
    player.m_placePressedTime = Time.time;
    player.m_lastToolUseTime = 0f;
    player.UpdatePlacement(true, 0f);
  }

  ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
  public static void FixData(ZNetView obj) {
    var zdo = obj.GetZDO();
    var character = obj.GetComponent<Character>();
    if (character) {
      // SetLevel would also overwrite the health (when copying a creature with a custom health).
      var level = zdo.GetInt(Hash.Level, 1);
      character.m_level = level;
      zdo.Set(Hash.Level, level);
      if (character.m_onLevelSet != null) character.m_onLevelSet(character.m_level);
      character.SetTamed(zdo.GetBool(Hash.Tamed, false));
    }
    obj.GetComponentInChildren<ItemDrop>()?.Load();
    obj.GetComponentInChildren<ArmorStand>()?.UpdateVisual();
    obj.GetComponentInChildren<VisEquipment>()?.UpdateVisuals();
    obj.GetComponentInChildren<ItemStand>()?.UpdateVisual();
    obj.GetComponentInChildren<CookingStation>()?.UpdateCooking();
    obj.GetComponentInChildren<LocationProxy>()?.SpawnLocation();
    obj.GetComponentInChildren<Sign>()?.UpdateText();
    obj.GetComponentInChildren<Door>()?.UpdateState();
  }
  ///<summary>Replaces LocationProxy with the actual location.</summary>
  public static void SpawnLocation(ZNetView view) {
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
    foreach (var zdo in UndoHelper.Objects) {
      if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var obj))
        PostProcessPlaced(obj.gameObject);
    }
    CustomizeSpawnLocation.RandomDamage = null;
    CustomizeSpawnLocation.AllViews = false;
  }
  ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
  public static void PostProcessPlaced(GameObject obj) {
    var view = obj.GetComponent<ZNetView>();
    if (!Configuration.Enabled || !view) return;
    var zdo = view.GetZDO();
    var piece = obj.GetComponent<Piece>();
    if (piece) {
      piece.m_canBeRemoved = true;
      // Creator data is only interesting for actual targets. Dummy components will have these both as false.
      if (piece.m_randomTarget || piece.m_primaryTarget) {
        if (Configuration.NoCreator) {
          zdo.Set(Hash.Creator, 0L);
          piece.m_creator = 0;
        } else
          piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
      }
    }
    var character = obj.GetComponent<Character>();
    if (Configuration.OverwriteHealth > 0f) {
      if (character)
        zdo.Set(Hash.MaxHealth, Configuration.OverwriteHealth);
      if (obj.GetComponent<TreeLog>() || obj.GetComponent<WearNTear>() || obj.GetComponent<Destructible>() || obj.GetComponent<TreeBase>() || character)
        zdo.Set(Hash.Health, Configuration.OverwriteHealth);
      var mineRock = obj.GetComponent<MineRock5>();
      if (mineRock) {
        foreach (var area in mineRock.m_hitAreas) area.m_health = Configuration.OverwriteHealth;
        mineRock.SaveHealth();
      }
    }
    if (!zdo.GetBool(Hash.Render, true)) {
      var renderers = view.GetComponentsInChildren<Renderer>();
      foreach (var renderer in renderers)
        renderer.enabled = false;
    }
    if (!zdo.GetBool(Hash.Collision, true)) {
      var colliders = view.GetComponentsInChildren<Collider>();
      if (zdo.GetPrefab() == Hash.Portal) {
        foreach (var collider in colliders)
          collider.enabled = false;

      } else {
        foreach (var collider in colliders)
          collider.isTrigger = true;
      }
    }
    FixData(view);
  }

  ///<summary>Restores durability and stamina to counter the usage.</summary>
  public static void PostProcessTool(Player obj) {
    var item = obj.GetRightItem();
    if (item == null) return;
    if (Configuration.NoStaminaCost)
      obj.UseStamina(-item.m_shared.m_attack.m_attackStamina);
    if (Configuration.NoDurabilityLoss && item.m_shared.m_useDurability)
      item.m_durability += item.m_shared.m_useDurabilityDrain;
  }
}


[HarmonyPatch(typeof(EffectList), nameof(EffectList.Create))]
public class DisableEffects {
  public static bool Active = false;
  static bool Prefix() => !Active || !Configuration.RemoveEffects;
}

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
public class UnfreezeOnUnequip {
  static void Prefix(Humanoid __instance, ItemDrop.ItemData item) {
    if (__instance != Player.m_localPlayer || item == null) return;
    if (Configuration.UnfreezeOnUnequip) Position.Unfreeze();
  }
}
