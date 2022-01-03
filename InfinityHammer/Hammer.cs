using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace InfinityHammer {
  public static class Hammer {
    ///<summary>The entity sampled by the hammer (can be an actual entity or a named prefab)</summary>
    private static GameObject Sample = null;
    ///<summary>ZDO of actual entities.</summary>
    private static ZDO SampleZDO = null;

    public static void ResetIfSelected(ZDO zdo) {
      if (SampleZDO == zdo) Remove(Player.m_localPlayer);
    }

    ///<summary>Copies ZDO data</summary>
    public static void CopyState(Piece obj) {
      if (SampleZDO == null || !Settings.CopyState) return;
      var zdo = obj.m_nview.GetZDO();
      if (!SampleZDO.IsValid() || !zdo.IsValid()) return;
      var clone = SampleZDO.Clone();
      zdo.m_floats = clone.m_floats;
      zdo.m_vec3 = clone.m_vec3;
      zdo.m_quats = clone.m_quats;
      zdo.m_ints = clone.m_ints;
      zdo.m_longs = clone.m_longs;
      zdo.m_strings = clone.m_strings;
      zdo.m_byteArrays = clone.m_byteArrays;
      zdo.IncreseDataRevision();
    }
    ///<summary>Replaces the placement piece with a clean prefab (prevents attached objects being copied).</summary>
    public static void CleanPlacePrefab(ref Piece piece) {
      if (Sample == null) return;
      var prefab = ZNetScene.instance.GetPrefab(Utils.GetPrefabName(Sample));
      piece = prefab.GetComponent<Piece>();
      if (!piece)
        piece = prefab.AddComponent<Piece>();
    }
    private static bool IsBuildPiece(Player player, GameObject obj)
      => player.m_buildPieces.m_pieces.Any(piece => Utils.GetPrefabName(obj) == Utils.GetPrefabName(piece));
    ///<summary>Sets the sample object while ensuring it has the needed Piece component.</summary>
    public static bool Set(Player player, GameObject obj, ZDO state) {
      if (!player || !obj) return false;
      if (obj.GetComponent<Player>()) return false;
      if (!Settings.AllObjects && !IsBuildPiece(player, obj)) return false;
      // Without state, the clean should be used immediatelly (so itemstands won't show the item).
      if (!Settings.CopyState) obj = ZNetScene.instance.GetPrefab(Utils.GetPrefabName(obj));
      if (!obj) return false;
      var piece = obj.GetComponent<Piece>();
      if (!piece) {
        piece = obj.AddComponent<Piece>();
        piece.m_clipEverything = true;
      }
      Sample = obj;
      SampleZDO = state;
      player.SetupPlacementGhost();
      return true;
    }
    ///<summary>Removes the sample object.</summary>
    public static void Remove(Player player) {
      Sample = null;
      SampleZDO = null;
      player?.SetupPlacementGhost();
    }
    ///<summary>Overrides the given piece with the sample object. Returns if overridden.</summary>
    public static bool ReplacePiece(ref Piece piece) {
      if (Sample) {
        piece = Sample.GetComponent<Piece>();
        if (piece)
          return true;
      }
      return false;
    }

    ///<summary>Copies state and ensures visuals are updated for the placed object.</summary>
    public static void PostProcessPlaced() {
      if (!Sample) return;
      if (Piece.m_allPieces.Count == 0) return;
      var added = Piece.m_allPieces[Piece.m_allPieces.Count - 1];
      CopyState(added);
      var piece = added.GetComponent<Piece>();
      if (piece)
        piece.m_canBeRemoved = true;
      var armorStand = added.GetComponentInChildren<ArmorStand>();
      if (armorStand)
        armorStand.UpdateVisual();
      var visEquipment = added.GetComponentInChildren<VisEquipment>();
      if (visEquipment)
        visEquipment.UpdateVisuals();
      var itemStand = added.GetComponentInChildren<ItemStand>();
      if (itemStand)
        itemStand.UpdateVisual();
      var cookingStation = added.GetComponentInChildren<CookingStation>();
      if (cookingStation)
        cookingStation.UpdateCooking();
      var locationProxy = added.GetComponentInChildren<LocationProxy>();
      if (locationProxy)
        locationProxy.SpawnLocation();
      var sign = added.GetComponentInChildren<Sign>();
      if (sign)
        sign.UpdateText();
    }

    ///<summary>Removes placement checks.</summary>
    public static void ForceValidPlacement(Player obj) {
      if (obj.m_placementGhost == null) return;
      if (obj.m_placementStatus == Player.PlacementStatus.NotInDungeon) {
        if (!Settings.AllowInDungeons) return;
      } else if (obj.m_placementStatus == Player.PlacementStatus.NoBuildZone) {
        if (!Settings.IgnoreNoBuild) return;
      } else if (obj.m_placementStatus == Player.PlacementStatus.PrivateZone) {
        if (!Settings.IgnoreWards) return;
      } else if (!Settings.IgnoreOtherRestrictions) return;
      obj.m_placementStatus = Player.PlacementStatus.Valid;
      obj.SetPlacementGhostValid(true);
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

    ///<summary>Disables problematic scripts and sets free placement.</summary>
    public static void PostProcessPlacementGhost(GameObject obj) {
      if (!obj || !Sample) return;
      var baseAI = obj.GetComponent<BaseAI>();
      var monsterAI = obj.GetComponent<MonsterAI>();
      var humanoid = obj.GetComponent<Humanoid>();
      var character = obj.GetComponent<Character>();
      var tombStone = obj.GetComponent<TombStone>();
      if (baseAI) baseAI.enabled = false;
      if (monsterAI) monsterAI.enabled = false;
      if (humanoid) humanoid.enabled = false;
      if (character) character.enabled = false;
      if (tombStone) tombStone.enabled = false;
    }

    ///<summary>Removes any object...</summary>
    public static void RemoveAnything(Player obj) {
      var hits = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, 50f, obj.m_interactMask);
      Array.Sort<RaycastHit>(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
      foreach (var hit in hits) {
        if (Vector3.Distance(hit.point, obj.m_eye.position) >= obj.m_maxPlaceDistance) continue;
        var netView = hit.collider.GetComponentInParent<ZNetView>();
        if (!netView) continue;
        if (netView.GetComponentInChildren<Player>()) continue;
        netView.ClaimOwnership();
        obj.m_removeEffects.Create(netView.transform.position, Quaternion.identity, null, 1f, -1);
        ZNetScene.instance.Destroy(netView.gameObject);
        ItemDrop.ItemData rightItem = obj.GetRightItem();
        if (rightItem != null) {
          obj.FaceLookDirection();
          obj.m_zanim.SetTrigger(rightItem.m_shared.m_attack.m_attackAnimation);
        }
        PostProcessTool(obj);
        break;
      }
    }
  }

  ///<summary>Overrides the piece selection.</summary>
  [HarmonyPatch(typeof(PieceTable), "GetSelectedPiece")]
  public class GetSelectedPiece {
    public static bool Prefix(ref Piece __result) => !Hammer.ReplacePiece(ref __result);
  }

  ///<summary>Selecting a piece normally removes the override.</summary>
  [HarmonyPatch(typeof(Player), "SetSelectedPiece")]
  public class SetSelectedPiece {
    public static void Prefix(Player __instance) => Hammer.Remove(__instance);
  }

  [HarmonyPatch(typeof(Player), "PlacePiece")]
  public class PlacePiece {
    public static void Prefix(ref Piece piece) => Hammer.CleanPlacePrefab(ref piece);
    public static void Postfix(Player __instance, bool __result) {
      if (__result) {
        Hammer.PostProcessPlaced();
        Hammer.PostProcessTool(__instance);
      }
    }
  }

  [HarmonyPatch(typeof(Player), "RemovePiece")]
  public class RemovePiece {
    public static void Postfix(Player __instance, bool __result) {
      if (__result) Hammer.PostProcessTool(__instance);
      else if (Settings.RemoveAnything) Hammer.RemoveAnything(__instance);
    }
  }
  [HarmonyPatch(typeof(Player), "SetupPlacementGhost")]
  public class SetupPlacementGhost {
    public static void Postfix(Player __instance) => Hammer.PostProcessPlacementGhost(__instance.m_placementGhost);
  }
  ///<summary>Resets the sample if it's removed.</summary>
  [HarmonyPatch(typeof(ZNetScene), "OnZDODestroyed")]
  public class OnZDODestroyed {
    public static void Prefix(ZDO zdo) => Hammer.ResetIfSelected(zdo);
  }
}