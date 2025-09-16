using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using ServerDevcommands;
using Service;
using UnityEngine;
using UnityEngine.Animations;
namespace InfinityHammer;

public static class HammerHelper
{
  public static string GetArgs(string cmd, Terminal.ConsoleEventArgs args) => args.FullLine.Substring(cmd.Length + 1);
  public static string Format(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
  public static string PrintXZY(Vector3 vec) => $"{Format(vec.x)},{Format(vec.z)},{Format(vec.y)}";
  public static string PrintYXZ(Vector3 vec) => $"{Format(vec.y)},{Format(vec.x)},{Format(vec.z)}";
  public static GameObject GetPlacementGhost()
  {
    var player = Helper.GetPlayer();
    if (!player.m_placementGhost) throw new InvalidOperationException("Not currently placing anything.");
    return player.m_placementGhost;
  }
  public static string GetTool()
  {
    var player = Helper.GetPlayer();
    var item = player.GetRightItem();
    if (Hammer.IsInfinityHammer(item))
      return "infinity_hammer";
    return item?.m_dropPrefab?.name ?? "";
  }
  public static void RemoveZDO(ZDO zdo)
  {
    if (!Selector.IsValid(zdo)) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.m_sessionID);
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view))
      ZNetScene.instance.Destroy(view.gameObject);
    else
      ZDOMan.instance.DestroyZDO(zdo);
  }

  ///<summary>Parses a size which can be a constant number or based on the ghost size.</summary>
  public static Vector3 ParseSize(GameObject ghost, string value)
  {
    var multiplier = Parse.Multiplier(value);
    var size = Vector3.one;
    if (value.Contains("auto"))
    {
      if (!ghost) throw new InvalidOperationException("No placement ghost.");
      if (!Configuration.Dimensions.TryGetValue(Utils.GetPrefabName(ghost).ToLower(), out size))
        throw new InvalidOperationException("Missing object dimensions. Try placing the object to fix the issue.");
    }
    return multiplier * size;
  }
  ///<summary>Parses a size which can be a constant number or based on the ghost size.</summary>
  public static Vector3 TryParseSize(GameObject ghost, string[] args, int index, string defaltValue = "auto")
  {
    var value = defaltValue;
    if (args.Length > index) value = args[index];
    return ParseSize(ghost, value);
  }
  ///<summary>Parses a size which can be a constant number or based on the ghost size.</summary>
  public static Vector3 TryParseSizesZYX(GameObject ghost, string[] args, int index, string defaltValue = "auto")
  {
    var value = defaltValue;
    if (args.Length > index) value = args[index];
    var split = value.Split(',');
    return new(TryParseSize(ghost, split, 2, defaltValue).x, TryParseSize(ghost, split, 1, defaltValue).y, TryParseSize(ghost, split, 0, defaltValue).z);
  }
  ///<summary>Returns whether the ghost is square on x-axis.</summary>
  public static bool IsSquareX(GameObject ghost)
  {
    if (!ghost) return false;
    var size = Configuration.Dimensions[Utils.GetPrefabName(ghost).ToLower()];
    return size.y - size.z < 0.01f;
  }
  ///<summary>Returns whether the ghost is square on y-axis.</summary>
  public static bool IsSquareY(GameObject ghost)
  {
    if (!ghost) return false;
    var size = Configuration.Dimensions[Utils.GetPrefabName(ghost).ToLower()];
    return size.x - size.z < 0.01f;
  }
  ///<summary>Returns whether the ghost is square on z-axis.</summary>
  public static bool IsSquareZ(GameObject ghost)
  {
    if (!ghost) return false;
    var size = Configuration.Dimensions[Utils.GetPrefabName(ghost).ToLower()];
    return size.x - size.y < 0.01f;
  }
  public static void Message(Terminal instance, string message)
  {
    if (Configuration.DisableMessages) return;
    Helper.AddMessage(instance, message, true);
  }
  public static GameObject SafeInstantiate(ZNetView view, GameObject parent)
  {
    var obj = view.gameObject;
    // Some data changes can remove all colliders from the obj, so fallback to the original prefab if needed.
    var colliders = view.GetComponentsInChildren<Collider>();
    var activeColliders = colliders.Where(collider => collider.enabled && !collider.isTrigger).ToArray();
    if (activeColliders.Length == 0)
    {
      var hash = view.GetZDO() == null ? view.GetPrefabName().GetStableHashCode() : view.GetZDO().GetPrefab();
      obj = ZNetScene.instance.GetPrefab(hash);
    }
    return SafeInstantiate(obj, view.transform.rotation, parent.transform);
  }
  public static GameObject ChildInstantiate(ZNetView view, GameObject parent)
  {
    var obj = view.gameObject;
    ResetHighlight(obj);
    var ret = UnityEngine.Object.Instantiate(obj, parent.transform);
    CleanObject(ret);
    return ret;
  }
  public static GameObject SafeInstantiateRoom(DungeonDB.RoomData room, bool emptyRoom, GameObject parent)
  {
    room.m_prefab.Load();
    ZNetView[] views = Utils.GetEnabledComponentsInChildren<ZNetView>(room.m_prefab.Asset);
    if (emptyRoom)
    {
      foreach (var view in views)
        view.gameObject.SetActive(false);
    }
    var ret = SafeInstantiate(room.m_prefab.Asset, room.m_prefab.Asset.transform.rotation, parent.transform);
    if (emptyRoom)
    {
      foreach (var view in views)
        view.gameObject.SetActive(true);
    }
    room.m_prefab.Release();
    return ret;
  }
  public static GameObject SafeInstantiateLocation(ZoneSystem.ZoneLocation location, int? seed, GameObject parent)
  {
    if (!location.m_prefab.IsValid)
    {
      var obj = new GameObject();
      EnsurePiece(obj);
      obj.name = location.m_prefab.Name;
      return obj;
    }
    location.m_prefab.Load();
    ZNetView[] netViews = Utils.GetEnabledComponentsInChildren<ZNetView>(location.m_prefab.Asset);
    RandomSpawn[] randomSpawns = Utils.GetEnabledComponentsInChildren<RandomSpawn>(location.m_prefab.Asset);
    foreach (var view in netViews)
      view.gameObject.SetActive(true);
    if (seed.HasValue)
    {
      var state = UnityEngine.Random.state;
      UnityEngine.Random.InitState(seed.Value);
      foreach (var random in randomSpawns)
      {
        random.Prepare();
        random.Randomize(Vector3.zero);
      }
      UnityEngine.Random.state = state;
    }
    var ret = SafeInstantiate(location.m_prefab.Asset, location.m_prefab.Asset.transform.rotation, parent.transform);
    location.m_prefab.Release();
    return ret;
  }
  // Rot is needed if the object is reseted with the original.
  private static GameObject SafeInstantiate(GameObject obj, Quaternion rot, Transform parent)
  {
    ResetHighlight(obj);
    var ret = UnityEngine.Object.Instantiate(obj, Vector3.zero, rot, parent);
    CleanObject(ret);
    EnsurePiece(ret);
    ret.name = obj.name;
    return ret;
  }
  private static void ResetHighlight(GameObject obj)
  {
    // Must be done for the original because m_oldMaterials is not copied.
    if (obj.TryGetComponent<WearNTear>(out var wear))
      wear.ResetHighlight();
  }
  ///<summary>Removes scripts that try to run (for example placement needs only the model and Piece component).</summary>
  private static void CleanObject(GameObject obj)
  {
    // These are confirmed to cause issues.
    DestroyComponents<RandomFlyingBird>(obj);
    DestroyComponents<DungeonGenerator>(obj);
    DestroyComponents<MusicLocation>(obj);
    DisableComponents<Fish>(obj);

    // Could be disables but destroying maybe better for performance.
    DestroyComponents<FootStep>(obj);
    DestroyComponents<Growup>(obj);
    DestroyComponents<Windmill>(obj);
    DestroyComponents<MineRock>(obj);

    DestroyComponents<MineRock5>(obj);
    DestroyComponents<CharacterAnimEvent>(obj);
    DestroyComponents<TreeLog>(obj);
    DestroyComponents<TreeBase>(obj);
    DestroyComponents<CreatureSpawner>(obj);
    DestroyComponents<TombStone>(obj);
    DestroyComponents<Aoe>(obj);
    DestroyComponents<SpawnArea>(obj);
    DestroyComponents<Procreation>(obj);
    DestroyComponents<StaticPhysics>(obj);
    DestroyComponents<Tameable>(obj);
    DestroyComponents<Catapult>(obj);

    // Many things rely on Character so better just undo the Awake.
    var c = obj.GetComponent<Character>();
    if (c)
      Character.s_characters.Remove(c);
  }
  private static void DisableComponents<T>(GameObject obj) where T : MonoBehaviour
  {
    // Disable is enough to prevent Start.
    foreach (MonoBehaviour component in obj.GetComponentsInChildren<T>())
      component.enabled = false;
  }
  private static void DestroyComponents<T>(GameObject obj) where T : MonoBehaviour
  {
    // DestroyImmediate is needed to prevent Awake.
    // However some scripts rely on each other, so it's better to disable them if possible.
    foreach (MonoBehaviour component in obj.GetComponentsInChildren<T>())
      UnityEngine.Object.DestroyImmediate(component);

  }
  public static bool IsBuildPiece(GameObject obj)
      => Player.m_localPlayer.m_buildPieces.m_pieces.Any(piece => Utils.GetPrefabName(obj) == Utils.GetPrefabName(piece));
  ///<summary>Placement requires the Piece component.</summary>
  public static void EnsurePiece(GameObject obj)
  {
    if (obj.GetComponent<Piece>())
    {
      // Some unobtainable objects have Piece but no colliders, making them impossible to place.
      var colliders = obj.GetComponentsInChildren<Collider>().Where(collider => collider.enabled && !collider.isTrigger).ToArray();
      if (colliders.Length == 0)
      {
        obj.GetComponent<Piece>().m_clipEverything = true;
      }
      return;
    }
    var piece = obj.AddComponent<Piece>();
    var proxy = obj.GetComponent<LocationProxy>();
    if (proxy && proxy.m_instance)
      piece.m_name = Utils.GetPrefabName(proxy.m_instance);
    else
      piece.m_name = Utils.GetPrefabName(obj);
    piece.m_clipEverything = true;
    piece.m_randomTarget = false;
    piece.m_primaryTarget = false;
  }
  public static void CheatCheck()
  {
    if (!Configuration.IsCheats) throw new InvalidOperationException("This command is disabled.");
  }
  public static void EnabledCheck()
  {
    if (!Configuration.Enabled) throw new InvalidOperationException("Infinity Hammer is disabled.");
  }
  public static void Init()
  {
    EnabledCheck();
    Hammer.Equip();
    Hammer.SelectEmpty();
  }


  public static bool IsDown(string key)
  {
    var tag = false;
    var checkDown = true;
    if (key.StartsWith("-<"))
    {
      tag = true;
      checkDown = false;
      key = key.Substring(2, key.Length - 3);
    }
    else if (key.StartsWith("<"))
    {
      tag = true;
      key = key.Substring(1, key.Length - 2);
    }
    else if (key.StartsWith("-"))
    {
      checkDown = false;
      key = key.Substring(1, key.Length - 1);
    }
    if (tag)
    {
      if (key == "mod1")
      {
        key = Configuration.ModifierKey1();
        tag = false;
      }
      else if (key == "mod2")
      {
        key = Configuration.ModifierKey2();
        tag = false;
      }
      else if (key == "alt")
      {
        key = "AltPlace";
      }
    }
    if (tag)
      return ZInput.instance.TryGetButtonState(key, b => b.Held == checkDown);
    return Enum.TryParse<KeyCode>(key, true, out var code) && (Input.GetKey(code) == checkDown);
  }

  public static Sprite? FindSprite(string name) => SpriteHelper.FindSprite(name);
}
[HarmonyPatch(typeof(Player), nameof(Player.Message))]
public class ReplaceMessage
{
  public static string Message = "";
  public static void Prefix(ref string msg)
  {
    if (Message != "")
    {
      msg = Message;
      Message = "";
    }
  }
}
[HarmonyPatch(typeof(MaterialMan), nameof(MaterialMan.UnregisterRenderers))]
public class UnregisterRenderers
{
  // ItemStyles use this and may cause issues for armor and item stands.
  static bool Prefix(MaterialMan __instance, GameObject gameObject) => __instance.m_blocks.ContainsKey(gameObject.GetInstanceID());
}

[HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.MakePiece))]
public class PreventFishBecomingPieces
{
  // Vanilla automatically turns placed item drops to pieces.
  // This removes body which breaks fishes (that have Piece for some reasoon).
  // Also prevent pointless error message from missing Piece.
  static bool Prefix(ItemDrop __instance) => __instance.GetComponent<Piece>() && !__instance.GetComponent<Fish>();
}

[HarmonyPatch(typeof(Fish), nameof(Fish.Awake))]
public class PreventFishBeingPiece
{
  // Good idea also auto clean up fish that have gone wrong.
  static void Postfix(Fish __instance)
  {
    var view = __instance.m_nview;
    if (!view || !view.IsValid()) return;
    view.GetZDO().Set(ZDOVars.s_piece, false);
  }
}

// For some reason, vanilla messes up the object name.
[HarmonyPatch(typeof(MineRock5), nameof(MineRock5.Awake))]
public class MineRock5_NameFix
{
  static void Prefix(MineRock5 __instance, ref string __state)
  {
    __state = __instance.name;
  }
  static void Postfix(MineRock5 __instance, string __state)
  {
    __instance.name = __state;
  }
}