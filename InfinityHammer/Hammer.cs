using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace InfinityHammer {
  public static class Hammer {
    ///<summary>Copy of the selected entity. Only needed for the placement ghost because armor and item stands have a different model depending on their state.</summary>
    private static GameObject Selected = null;
    ///<summary>Copy of the state.</summary>
    private static ZDO State = null;
    ///<summary>The actual named prefab used for creating the object.</summary>
    private static GameObject Prefab = null;

    ///<summary>Hack to only return the selected when creating the placement ghost.</summary>
    public static bool UseSelectedObject = false;
    public static GameObject GetPrefab() {
      if (UseSelectedObject) {
        UseSelectedObject = false;
        return Selected ?? Prefab;
      }
      return Prefab;
    }

    public static void CopyState(Piece obj) {
      if (State == null || !Settings.CopyState || !obj.m_nview) return;
      var zdo = obj.m_nview.GetZDO();
      if (!zdo.IsValid()) return;
      Helper.CopyData(State.Clone(), zdo);
    }

    private static bool IsBuildPiece(Player player, GameObject obj)
      => player.m_buildPieces.m_pieces.Any(piece => Utils.GetPrefabName(obj) == Utils.GetPrefabName(piece));

    ///<summary>Sets the sample object while ensuring it has the needed Piece component.</summary>
    public static bool Set(Player player, GameObject obj, ZDO state) {
      if (!player || !obj) return false;
      if (obj.GetComponent<Player>()) return false;
      if (!Settings.AllObjects && !IsBuildPiece(player, obj)) return false;
      Scaling.SetScale(obj.transform.localScale);
      RemoveSelection();
      State = null;
      Prefab = ZNetScene.instance.GetPrefab(Utils.GetPrefabName(obj));
      if (Settings.CopyState) {
        obj.GetComponent<WearNTear>()?.ResetHighlight();
        // Initializing the copy as inactive is the best way to avoid any script errors.
        // ZNet stuff also won't run.
        obj.SetActive(false);
        Selected = Object.Instantiate(obj);
        obj.SetActive(true);
        State = state == null ? null : state.Clone();
      }
      if (Selected && !Selected.GetComponent<Piece>()) {
        var piece = Selected.AddComponent<Piece>();
        piece.m_name = Utils.GetPrefabName(piece.gameObject);
        piece.m_clipEverything = true;
      }
      if (Prefab && !Prefab.GetComponent<Piece>()) {
        var piece = Prefab.AddComponent<Piece>();
        piece.m_name = Utils.GetPrefabName(piece.gameObject);
        piece.m_clipEverything = true;
      }
      player.SetupPlacementGhost();
      return true;
    }
    public static void RemoveSelection() {
      if (Selected) ZNetScene.instance.Destroy(Selected);
      Selected = null;
      State = null;
      Prefab = null;
    }

    ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
    public static void PostProcessPlaced(Piece piece) {
      if (!Settings.Enabled) return;
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
      if (character) {
        // SetLevel would also overwrite the health (when copying a creature with a custom health).
        var level = zdo.GetInt("level", 1);
        character.m_level = level;
        zdo.Set("level", level);
        if (character.m_onLevelSet != null) character.m_onLevelSet(character.m_level);
        character.SetTamed(zdo.GetBool("tamed", false));
      }
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
      piece.GetComponentInChildren<ArmorStand>()?.UpdateVisual();
      piece.GetComponentInChildren<VisEquipment>()?.UpdateVisuals();
      piece.GetComponentInChildren<ItemStand>()?.UpdateVisual();
      piece.GetComponentInChildren<CookingStation>()?.UpdateCooking();
      piece.GetComponentInChildren<LocationProxy>()?.SpawnLocation();
      piece.GetComponentInChildren<Sign>()?.UpdateText();
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


  [HarmonyPatch(typeof(EffectList), "Create")]
  public class DisableEffects {
    public static bool Active = false;
    public static bool Prefix() => !Active || !Settings.RemoveEffects;
  }
}