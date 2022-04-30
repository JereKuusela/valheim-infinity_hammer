using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;
public static class Hammer {
  ///<summary>Copy of the selected entity. Only needed for the placement ghost because armor and item stands have a different model depending on their state.</summary>
  public static GameObject? GhostPrefab = null;
  ///<summary>Copy of the state.</summary>
  private static ZDO? State = null;

  public static void CopyState(Piece obj) {
    if (State == null || !Settings.CopyState || !obj.m_nview) return;
    var zdo = obj.m_nview.GetZDO();
    if (!zdo.IsValid()) return;
    Helper.CopyData(State.Clone(), zdo);
  }

  private static bool IsBuildPiece(Player player, GameObject obj)
    => player.m_buildPieces.m_pieces.Any(piece => Utils.GetPrefabName(obj) == Utils.GetPrefabName(piece));

  ///<summary>Sets the sample object while ensuring it has the needed Piece component.</summary>
  public static bool Set(Player player, GameObject obj, ZDO? state) {
    if (!player || !obj) return false;
    if (obj.GetComponent<Player>()) return false;
    if (!Settings.AllObjects && !IsBuildPiece(player, obj)) return false;
    RemoveSelection();
    if (Settings.CopyState) {
      GhostPrefab = Helper.SafeInstantiate(obj);
      State = state == null ? null : state.Clone();
    } else {
      var basePrefab = ZNetScene.instance.GetPrefab(Utils.GetPrefabName(obj));
      GhostPrefab = Helper.SafeInstantiate(basePrefab);
      GhostPrefab.transform.localScale = obj.transform.localScale;
      State = null;
    }
    Helper.EnsurePiece(GhostPrefab);
    player.SetupPlacementGhost();
    return true;
  }
  public static void RemoveSelection() {
    if (GhostPrefab) ZNetScene.instance.Destroy(GhostPrefab);
    GhostPrefab = null;
    State = null;
    if (Settings.UnfreezeOnSelect) Position.Unfreeze();
  }
  public static void Equip() {
    var player = Player.m_localPlayer;
    if (!Settings.AutoEquip || !player) return;
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
  }
  ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
  public static void PostProcessPlaced(Piece piece) {
    if (!Settings.Enabled || !piece.m_nview) return;
    CopyState(piece);
    piece.m_canBeRemoved = true;
    Scaling.SetPieceScale(piece);
    var zdo = piece.m_nview.GetZDO();
    // Creator data is only interesting for actual targets. Dummy components will have these both as false.
    if (piece.m_randomTarget || piece.m_primaryTarget) {
      if (Settings.NoCreator)
        zdo.Set("creator", 0L);
      else
        piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
    }
    var character = piece.GetComponent<Character>();
    if (Settings.OverwriteHealth > 0f) {
      if (character)
        zdo.Set("max_health", Settings.OverwriteHealth);
      if (piece.GetComponent<TreeLog>() || piece.GetComponent<WearNTear>() || piece.GetComponent<Destructible>() || piece.GetComponent<TreeBase>() || character)
        zdo.Set("health", Settings.OverwriteHealth);
      var mineRock = piece.GetComponent<MineRock5>();
      if (mineRock) {
        foreach (var area in mineRock.m_hitAreas) area.m_health = Settings.OverwriteHealth;
        mineRock.SaveHealth();
      }
    }
    FixData(piece.m_nview);
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
