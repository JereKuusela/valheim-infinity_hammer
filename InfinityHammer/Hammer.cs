using System.Linq;
using UnityEngine;

namespace InfinityHammer {
  public static class Hammer {
    ///<summary>The entity sampled by the hammer (can be an actual entity or a named prefab)</summary>
    private static GameObject Sample = null;

    ///<summary>Current scale (separate variable to not affect the sample).</summary>
    private static Vector3 Scale = Vector3.one;
    ///<summary>ZDO of actual entities.</summary>
    public static ZDO SampleZDO = null;

    ///<summary>Copies ZDO data</summary>
    public static void CopyState(Piece obj) {
      if (SampleZDO == null || !Settings.CopyState || !obj.m_nview) return;
      var zdo = obj.m_nview.GetZDO();
      if (!SampleZDO.IsValid() || !zdo.IsValid()) return;
      var clone = SampleZDO.Clone();
      Helper.CopyData(clone, zdo);
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
      Scale = obj.transform.localScale;
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
    public static void PostProcessPlaced(Piece piece) {
      if (!Sample) return;
      CopyState(piece);
      piece.m_canBeRemoved = true;
      piece.m_nview?.SetLocalScale(Scale);
      var zdo = piece.m_nview.GetZDO();
      if (Settings.NoCreator)
        zdo.Set("creator", 0L);
      else
        piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
      if (Settings.OverwriteHealth > 0f) {
        var character = piece.GetComponent<Character>();
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
      obj.transform.localScale = Scale;
    }
    ///<summary>Copies the selected object rotation.</summary>
    public static void Rotate(GameObject obj) {
      if (!Settings.CopyRotation) return;
      var player = Player.m_localPlayer;
      if (!player) return;
      var rotation = obj.transform.rotation;
      player.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);

      var gizmo = GameObject.Find("GizmoRoot(Clone)");
      if (!gizmo) {
        // Gizmo needs these to ensure that it is initialized properly.
        player.UpdatePlacementGhost(false);
        player.UpdatePlacement(false, 0);
      }
      gizmo = GameObject.Find("GizmoRoot(Clone)");
      if (gizmo)
        gizmo.transform.rotation = rotation;
    }
    public static GameObject ScaleUp() {
      Scale *= (1f + Settings.ScaleStep);
      return UpdateScale();
    }
    public static GameObject ScaleDown() {
      Scale /= (1f + Settings.ScaleStep);
      return UpdateScale();
    }
    public static GameObject SetScale(float value) {
      Scale = value * Vector3.one;
      return UpdateScale();
    }
    public static GameObject SetScale(Vector3 value) {
      Scale = value;
      return UpdateScale();
    }
    private static GameObject UpdateScale() {
      var ghost = Player.m_localPlayer?.m_placementGhost;
      if (!ghost) return null;
      var view = Player.m_localPlayer?.GetSelectedPiece()?.GetComponent<ZNetView>();
      if (view && view.m_syncInitialScale) {
        ghost.transform.localScale = Scale;
        return ghost;
      }
      return null;
    }
  }
}