using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;
public static class Hammer {

  public static bool AllLocationsObjects = false;
  public static bool RandomLocationDamage = false;

  public static void CopyState(ZNetView view, int index = 0) {
    if (!Settings.CopyState || !view) return;
    var zdo = view.GetZDO();
    if (zdo == null || !zdo.IsValid()) return;
    var data = Selection.GetData(index);
    if (data == null) return;
    Helper.CopyData(data, zdo);
  }
  public static void RemoveSelection() {
    Selection.Clear();
    if (Settings.UnfreezeOnSelect) Position.Unfreeze();
  }
  public static void Equip() {
    var player = Helper.GetPlayer();
    if (!Settings.AutoEquip) return;
    if (player.GetRightItem()?.m_dropPrefab?.gameObject.name == "Hammer") return;
    var inventory = player.GetInventory();
    var hammer = inventory.m_inventory.Find(item => item.m_dropPrefab.gameObject.name == "Hammer");
    if (hammer == null) return;

    player.EquipItem(hammer);
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
      var level = zdo.GetInt("level", 1);
      character.m_level = level;
      zdo.Set("level", level);
      if (character.m_onLevelSet != null) character.m_onLevelSet(character.m_level);
      character.SetTamed(zdo.GetBool("tamed", false));
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
    var prefab = data.GetInt("location", 0);
    var seed = data.GetInt("seed", 0);
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
    if (!Settings.Enabled || !view) return;
    var zdo = view.GetZDO();
    var piece = obj.GetComponent<Piece>();
    if (piece) {
      piece.m_canBeRemoved = true;
      // Creator data is only interesting for actual targets. Dummy components will have these both as false.
      if (piece.m_randomTarget || piece.m_primaryTarget) {
        if (Settings.NoCreator)
          zdo.Set("creator", 0L);
        else
          piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
      }
    }
    var character = obj.GetComponent<Character>();
    if (Settings.OverwriteHealth > 0f) {
      if (character)
        zdo.Set("max_health", Settings.OverwriteHealth);
      if (obj.GetComponent<TreeLog>() || obj.GetComponent<WearNTear>() || obj.GetComponent<Destructible>() || obj.GetComponent<TreeBase>() || character)
        zdo.Set("health", Settings.OverwriteHealth);
      var mineRock = obj.GetComponent<MineRock5>();
      if (mineRock) {
        foreach (var area in mineRock.m_hitAreas) area.m_health = Settings.OverwriteHealth;
        mineRock.SaveHealth();
      }
    }
    FixData(view);
  }

  ///<summary>Restores durability and stamina to counter the usage.</summary>
  public static void PostProcessTool(Player obj) {
    var item = obj.GetRightItem();
    if (item == null) return;
    if (Settings.NoStaminaCost)
      obj.UseStamina(-item.m_shared.m_attack.m_attackStamina);
    if (Settings.NoDurabilityLoss && item.m_shared.m_useDurability)
      item.m_durability += item.m_shared.m_useDurabilityDrain;
  }
}


[HarmonyPatch(typeof(EffectList), nameof(EffectList.Create))]
public class DisableEffects {
  public static bool Active = false;
  static bool Prefix() => !Active || !Settings.RemoveEffects;
}

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
public class UnfreezeOnUnequip {
  static void Prefix(Humanoid __instance, ItemDrop.ItemData item) {
    if (__instance != Player.m_localPlayer || item == null) return;
    if (Settings.UnfreezeOnUnequip) Position.Unfreeze();
  }
}
