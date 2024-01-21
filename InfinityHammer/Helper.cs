using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;
public static class HammerHelper
{
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
  public static GameObject SafeInstantiateLocation(ZoneSystem.ZoneLocation location, int? seed, GameObject parent)
  {
    foreach (var view in location.m_netViews)
      view.gameObject.SetActive(true);
    if (seed.HasValue)
    {
      var state = UnityEngine.Random.state;
      UnityEngine.Random.InitState(seed.Value);
      foreach (var random in location.m_randomSpawns)
        random.Randomize();
      UnityEngine.Random.state = state;
    }
    return SafeInstantiate(location.m_prefab, location.m_prefab.transform.rotation, parent.transform);
  }
  // Rot is needed if the object is reseted with the original.
  private static GameObject SafeInstantiate(GameObject obj, Quaternion rot, Transform parent)
  {
    ResetHighlight(obj);
    var ret = UnityEngine.Object.Instantiate(obj, Vector3.zero, rot, parent);
    CleanObject(ret);
    EnsurePiece(ret);
    return ret;
  }
  public static bool IsSnapPoint(GameObject obj) => obj && obj.CompareTag("snappoint");
  public static List<GameObject> GetChildren(GameObject obj)
  {
    List<GameObject> children = [];
    foreach (Transform tr in obj.transform)
    {
      if (IsSnapPoint(tr.gameObject)) continue;
      children.Add(tr.gameObject);
    }
    return children;
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
    DisableComponents<Character>(obj);
    DisableComponents<MonsterAI>(obj);
    DisableComponents<AnimalAI>(obj);
    DisableComponents<BaseAI>(obj);
    DisableComponents<Tameable>(obj);
    DisableComponents<Procreation>(obj);
    DisableComponents<Growup>(obj);
    DisableComponents<FootStep>(obj);
    DisableComponents<RandomFlyingBird>(obj);
    DisableComponents<Fish>(obj);
    DisableComponents<CharacterAnimEvent>(obj);
    DisableComponents<TreeLog>(obj);
    DisableComponents<TreeBase>(obj);
    DisableComponents<Windmill>(obj);
    DisableComponents<SpawnArea>(obj);
    DisableComponents<CreatureSpawner>(obj);
    DisableComponents<TombStone>(obj);
    DisableComponents<MineRock5>(obj);
    DisableComponents<MineRock>(obj);
    DisableComponents<HoverText>(obj);
    DisableComponents<StaticPhysics>(obj);
    DisableComponents<Aoe>(obj);
    DisableComponents<DungeonGenerator>(obj);
    DisableComponents<MusicLocation>(obj);
    DisableComponents<SpawnArea>(obj);

  }
  private static void DisableComponents<T>(GameObject obj) where T : MonoBehaviour
  {
    foreach (MonoBehaviour component in obj.GetComponentsInChildren<T>())
      component.enabled = false;
  }
  public static bool IsBuildPiece(GameObject obj)
      => Player.m_localPlayer.m_buildPieces.m_pieces.Any(piece => Utils.GetPrefabName(obj) == Utils.GetPrefabName(piece));
  ///<summary>Placement requires the Piece component.</summary>
  public static void EnsurePiece(GameObject obj)
  {
    if (obj.GetComponent<Piece>()) return;
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
  }

  public static int CountActiveChildren(GameObject obj)
  {
    var count = 0;
    foreach (Transform tr in obj.transform)
    {
      if (tr.gameObject.activeSelf && !IsSnapPoint(tr.gameObject)) count++;
    }
    return count;
  }
  public static int CountSnapPoints(GameObject obj)
  {
    var count = 0;
    foreach (Transform tr in obj.transform)
    {
      if (IsSnapPoint(tr.gameObject)) count++;
    }
    return count;
  }

  public static bool IsDown(string key)
  {
    if (key.StartsWith("-", StringComparison.OrdinalIgnoreCase))
      return Enum.TryParse<KeyCode>(key.Substring(1), true, out var code) && !Input.GetKey(code);
    else
      return Enum.TryParse<KeyCode>(key, true, out var code) && Input.GetKey(code);
  }
  private static Dictionary<string, int> PrefabNames = [];
  public static Sprite? FindSprite(string name)
  {
    if (!ZNetScene.instance) return null;
    if (PrefabNames.Count == 0)
    {
      PrefabNames = ZNetScene.instance.m_namedPrefabs.GroupBy(kvp => kvp.Value.name.ToLower()).ToDictionary(kvp => kvp.Key, kvp => kvp.First().Key);
    }

    name = name.ToLower();
    Sprite? sprite;
    if (PrefabNames.TryGetValue(name, out var hash))
    {
      var prefab = ZNetScene.instance.GetPrefab(hash);
      sprite = prefab?.GetComponent<Piece>()?.m_icon;
      if (sprite) return sprite;
      sprite = prefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_icons.FirstOrDefault();
      if (sprite) return sprite;
    }
    var effect = ObjectDB.instance.m_StatusEffects.Find(se => se.name.ToLower() == name);
    sprite = effect?.m_icon;
    if (sprite) return sprite;
    var skill = Player.m_localPlayer.m_skills.m_skills.Find(skill => skill.m_skill.ToString().ToLower() == name);
    sprite = skill?.m_icon;
    if (sprite) return sprite;
    return null;
  }
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
