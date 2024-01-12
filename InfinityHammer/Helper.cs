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
    var hash = view.GetZDO() == null ? view.GetPrefabName().GetStableHashCode() : view.GetZDO().GetPrefab();
    var originalPrefab = ZNetScene.instance.GetPrefab(hash);
    var colliders = view.GetComponentsInChildren<Collider>();
    var originalColliders = originalPrefab.GetComponentsInChildren<Collider>();
    // Some data changes can remove all colliders from the obj, so fallback to the original prefab if colliders can't be reseted.
    var obj = colliders.Length == originalColliders.Length ? view.gameObject : originalPrefab;

    var ret = SafeInstantiate(obj, parent.transform);

    if (colliders.Length == originalColliders.Length)
    {
      colliders = ret.GetComponentsInChildren<Collider>();
      for (var i = 0; i < colliders.Length; i++)
      {
        colliders[i].enabled = originalColliders[i].enabled;
        colliders[i].isTrigger = originalColliders[i].isTrigger;
      }
    }
    return ret;
  }
  public static GameObject ChildInstantiate(ZNetView view, GameObject parent)
  {
    var obj = view.gameObject;
    var ret = UnityEngine.Object.Instantiate(obj, parent.transform);
    CleanObject(ret);
    return ret;
  }
  ///<summary>Initializing the copy as inactive is the best way to avoid any script errors. ZNet stuff also won't run.</summary>
  public static GameObject SafeInstantiateLocation(ZoneSystem.ZoneLocation location, int? seed)
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
    return SafeInstantiate(location.m_prefab);
  }
  private static GameObject SafeInstantiate(GameObject obj)
  {
    var ret = UnityEngine.Object.Instantiate(obj);
    CleanObject(ret);
    return ret;
  }
  private static GameObject SafeInstantiate(GameObject obj, Transform parent)
  {
    var ret = UnityEngine.Object.Instantiate(obj, parent);
    CleanObject(ret);
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
  ///<summary>Removes scripts that try to run (for example placement needs only the model and Piece component).</summary>
  public static void CleanObject(GameObject obj)
  {
    if (obj.TryGetComponent<WearNTear>(out var wear) && wear.m_oldMaterials != null)
      wear.ResetHighlight();
    // Creature behavior.
    if (obj.TryGetComponent<Character>(out var character)) character.enabled = false;
    UnityEngine.Object.Destroy(obj.GetComponent<CharacterDrop>());
    UnityEngine.Object.Destroy(obj.GetComponent<MonsterAI>());
    UnityEngine.Object.Destroy(obj.GetComponent<AnimalAI>());
    UnityEngine.Object.Destroy(obj.GetComponent<BaseAI>());
    UnityEngine.Object.Destroy(obj.GetComponent<Tameable>());
    UnityEngine.Object.Destroy(obj.GetComponent<Procreation>());
    UnityEngine.Object.Destroy(obj.GetComponent<Growup>());
    UnityEngine.Object.Destroy(obj.GetComponent<FootStep>());
    UnityEngine.Object.Destroy(obj.GetComponent<RandomFlyingBird>());
    UnityEngine.Object.Destroy(obj.GetComponent<Fish>());
    UnityEngine.Object.Destroy(obj.GetComponentInChildren<CharacterAnimEvent>());
    // Destructible behavior.
    UnityEngine.Object.Destroy(obj.GetComponent<TreeLog>());
    UnityEngine.Object.Destroy(obj.GetComponent<TreeBase>());
    UnityEngine.Object.Destroy(obj.GetComponent<MineRock>());
    UnityEngine.Object.Destroy(obj.GetComponent<Windmill>());
    UnityEngine.Object.Destroy(obj.GetComponent<SpawnArea>());
    UnityEngine.Object.Destroy(obj.GetComponent<CreatureSpawner>());
    UnityEngine.Object.Destroy(obj.GetComponent<TombStone>());
    UnityEngine.Object.Destroy(obj.GetComponent<MineRock5>());
    UnityEngine.Object.Destroy(obj.GetComponent<MineRock>());
    // Other
    UnityEngine.Object.Destroy(obj.GetComponent<HoverText>());
    UnityEngine.Object.Destroy(obj.GetComponent<StaticPhysics>());
    UnityEngine.Object.Destroy(obj.GetComponent<Aoe>());
    UnityEngine.Object.Destroy(obj.GetComponent<DungeonGenerator>());
    UnityEngine.Object.Destroy(obj.GetComponentInChildren<MusicLocation>());
    UnityEngine.Object.Destroy(obj.GetComponentInChildren<SpawnArea>());
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
    Hammer.SelectRepair();
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
