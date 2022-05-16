using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;
public class Range<T> {
  public T Min;
  public T Max;
  public Range(T value) {
    Min = value;
    Max = value;
  }
  public Range(T min, T max) {
    Min = min;
    Max = max;
  }
}
public static class Helper {
  public static GameObject GetPlacementGhost() {
    var player = GetPlayer();
    if (!player.m_placementGhost) throw new InvalidOperationException("Not currently placing anything.");
    return player.m_placementGhost;
  }
  public static void RemoveZDO(ZDO zdo) {
    if (!IsValid(zdo)) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.GetMyID());
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view))
      ZNetScene.instance.Destroy(view.gameObject);
    else
      ZDOMan.instance.DestroyZDO(zdo);
  }

  public static void CopyData(ZDO from, ZDO to) {
    to.m_floats = from.m_floats;
    to.m_vec3 = from.m_vec3;
    to.m_quats = from.m_quats;
    to.m_ints = from.m_ints;
    to.m_longs = from.m_longs;
    to.m_strings = from.m_strings;
    to.m_byteArrays = from.m_byteArrays;
    to.IncreseDataRevision();
  }

  public static float ParseFloat(string value, float defaultValue = 0) {
    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
    return defaultValue;
  }
  public static int ParseInt(string value, int defaultValue = 0) {
    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)) return result;
    return defaultValue;
  }
  private static Range<string> ParseRange(string arg) {
    var range = arg.Split('-').ToList();
    if (range.Count > 1 && range[0] == "") {
      range[0] = "-" + range[1];
      range.RemoveAt(1);
    }
    if (range.Count > 2 && range[1] == "") {
      range[1] = "-" + range[2];
      range.RemoveAt(2);
    }
    if (range.Count == 1) return new(range[0]);
    else return new(range[0], range[1]);

  }
  public static Range<int> ParseIntRange(string value, int defaultValue = 0) {
    var range = ParseRange(value);
    return ZeroIntRange(new(ParseInt(range.Min, defaultValue), ParseInt(range.Max, defaultValue)));
  }
  public static Range<int> ZeroIntRange(Range<int> range) {
    if (range.Max == range.Min) {
      if (range.Max > 0) {
        range.Max--;
        range.Min = 0;
      }
      if (range.Max < 0) {
        range.Max = 0;
        range.Min++;
      }
    }
    return range;
  }
  public static Vector3 ParseZYX(string value) {
    var vector = Vector3.zero;
    var split = value.Split(',');
    if (split.Length > 0) vector.z = Helper.ParseFloat(split[0]);
    if (split.Length > 1) vector.y = Helper.ParseFloat(split[1]);
    if (split.Length > 2) vector.x = Helper.ParseFloat(split[2]);
    return vector;
  }
  public static Range<Vector3Int> ParseZYXRange(string value) {
    var min = Vector3Int.zero;
    var max = Vector3Int.zero;
    var split = value.Split(',');
    if (split.Length > 0) {
      var range = Helper.ParseIntRange(split[0]);
      min.z = range.Min;
      max.z = range.Max;
    }
    if (split.Length > 1) {
      var range = Helper.ParseIntRange(split[1]);
      min.y = range.Min;
      max.y = range.Max;
    }
    if (split.Length > 2) {
      var range = Helper.ParseIntRange(split[2]);
      min.x = range.Min;
      max.x = range.Max;
    }
    return new(min, max);
  }
  public static Vector3Int ParseXYZInt(string value) {
    var vector = Vector3Int.zero;
    var split = value.Split(',');
    if (split.Length > 0) vector.x = Helper.ParseInt(split[0]);
    if (split.Length > 1) vector.y = Helper.ParseInt(split[1]);
    if (split.Length > 2) vector.z = Helper.ParseInt(split[2]);
    return vector;
  }
  public static float ParseDirection(string value) {
    var direction = ParseFloat(value, 1);
    if (direction > 0) return 1f;
    return -1f;
  }

  public static float ParseDirection(string[] args, int index) {
    if (args.Length <= index) return 1f;
    var direction = ParseFloat(args[index], 1);
    if (direction > 0) return 1f;
    return -1f;
  }

  public static float ParseMultiplier(string value) {
    var multiplier = 1f;
    var split = value.Split('*');
    foreach (var str in split) multiplier *= Helper.ParseFloat(str, 1f);
    return multiplier;
  }
  public static float TryParseMultiplier(string[] args, int index, float defaultValue = 1f) {
    if (args.Length <= index) return defaultValue;
    return ParseMultiplier(args[index]);
  }
  ///<summary>Parses a size which can be a constant number or based on the ghost size.</summary>
  public static Vector3 ParseSize(GameObject ghost, string value) {
    var multiplier = ParseMultiplier(value);
    var size = Vector3.one;
    if (value.Contains("auto")) {
      if (!ghost) throw new InvalidOperationException("Error: No placement ghost.");
      if (!Bounds.Get.TryGetValue(Utils.GetPrefabName(ghost), out size))
        throw new InvalidOperationException("Error: Missing object dimensions. Try placing the object to fix the issue.");
    }
    return multiplier * size;
  }
  ///<summary>Parses a size which can be a constant number or based on the ghost size.</summary>
  public static Vector3 TryParseSize(GameObject ghost, string[] args, int index, string defaltValue = "auto") {
    var value = defaltValue;
    if (args.Length > index) value = args[index];
    return ParseSize(ghost, value);
  }
  ///<summary>Parses a size which can be a constant number or based on the ghost size.</summary>
  public static Vector3 TryParseSizesZYX(GameObject ghost, string[] args, int index, string defaltValue = "auto") {
    var value = defaltValue;
    if (args.Length > index) value = args[index];
    var split = value.Split(',');
    return new(TryParseSize(ghost, split, 2, defaltValue).x, TryParseSize(ghost, split, 1, defaltValue).y, TryParseSize(ghost, split, 0, defaltValue).z);
  }

  ///<summary>Returns whether the ghost is square on x-axis.</summary>
  public static bool IsSquareX(GameObject ghost) {
    if (!ghost) return false;
    var size = Bounds.Get[Utils.GetPrefabName(ghost)];
    return size.y - size.z < 0.01f;
  }
  ///<summary>Returns whether the ghost is square on y-axis.</summary>
  public static bool IsSquareY(GameObject ghost) {
    if (!ghost) return false;
    var size = Bounds.Get[Utils.GetPrefabName(ghost)];
    return size.x - size.z < 0.01f;
  }
  ///<summary>Returns whether the ghost is square on z-axis.</summary>
  public static bool IsSquareZ(GameObject ghost) {
    if (!ghost) return false;
    var size = Bounds.Get[Utils.GetPrefabName(ghost)];
    return size.x - size.y < 0.01f;
  }

  public static void AddError(Terminal context, string message, bool priority = true) {
    AddMessage(context, $"Error: {message}", priority);
  }
  public static void AddMessage(Terminal context, string message, bool priority = true) {
    context.AddString(message);
    var hud = MessageHud.instance;
    if (!hud || Settings.DisableMessages) return;
    if (priority) {
      var items = hud.m_msgQeue.ToArray();
      hud.m_msgQeue.Clear();
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
      foreach (var item in items)
        hud.m_msgQeue.Enqueue(item);
      hud.m_msgQueueTimer = 10f;
    } else {
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
    }
  }

  public static Hovered? GetHovered(Player obj, float maxDistance, HashSet<string>? blacklist, bool allowOtherPlayers = false) {
    var raycast = Math.Max(maxDistance + 5f, 50f);
    var hits = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, raycast, obj.m_interactMask);
    Array.Sort<RaycastHit>(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
    foreach (var hit in hits) {
      if (Vector3.Distance(hit.point, obj.m_eye.position) >= maxDistance) continue;
      var netView = hit.collider.GetComponentInParent<ZNetView>();
      if (!IsValid(netView)) continue;
      if (blacklist != null && blacklist.Contains(Utils.GetPrefabName(netView.gameObject).ToLower())) continue;
      var player = netView.GetComponentInChildren<Player>();
      if (player == obj) continue;
      if (!allowOtherPlayers && player) continue;
      var mineRock = netView.GetComponent<MineRock5>();
      var index = 0;
      if (mineRock)
        index = mineRock.GetAreaIndex(hit.collider);
      return new(netView, index);
    }
    return null;
  }
  public static Player GetPlayer() {
    var player = Player.m_localPlayer;
    if (!player) throw new InvalidOperationException("Error: No player.");
    return player;
  }
  ///<summary>Initializing the copy as inactive is the best way to avoid any script errors. ZNet stuff also won't run.</summary>
  public static GameObject SafeInstantiate(GameObject obj) {
    obj.SetActive(false);
    var ret = UnityEngine.Object.Instantiate(obj);
    obj.SetActive(true);
    return ret;
  }
  public static GameObject SafeInstantiate(string name, GameObject parent) {
    var obj = ZNetScene.instance.GetPrefab(name);
    if (!obj) throw new InvalidOperationException($"Missing object {name}.");
    obj.SetActive(false);
    var ret = UnityEngine.Object.Instantiate(obj, parent.transform);
    obj.SetActive(true);
    return ret;
  }
  ///<summary>Initializing the copy as inactive is the best way to avoid any script errors. ZNet stuff also won't run.</summary>
  public static GameObject SafeInstantiateLocation(ZoneSystem.ZoneLocation location, int? seed) {
    foreach (var view in location.m_netViews)
      view.gameObject.SetActive(true);
    if (seed.HasValue) {
      var state = UnityEngine.Random.state;
      UnityEngine.Random.InitState(seed.Value);
      foreach (var random in location.m_randomSpawns)
        random.Randomize();
      UnityEngine.Random.state = state;
    }
    return SafeInstantiate(location.m_prefab);
  }
  ///<summary>Removes scripts that try to run (for example placement needs only the model and Piece component).</summary>
  public static void CleanObject(GameObject obj) {
    if (!obj || !Settings.Enabled) return;
    // Creature behavior.
    UnityEngine.Object.Destroy(obj.GetComponent<CharacterDrop>());
    UnityEngine.Object.Destroy(obj.GetComponent<BaseAI>());
    UnityEngine.Object.Destroy(obj.GetComponent<MonsterAI>());
    UnityEngine.Object.Destroy(obj.GetComponent<Character>());
    UnityEngine.Object.Destroy(obj.GetComponent<Tameable>());
    UnityEngine.Object.Destroy(obj.GetComponent<Procreation>());
    UnityEngine.Object.Destroy(obj.GetComponent<Growup>());
    UnityEngine.Object.Destroy(obj.GetComponent<FootStep>());
    UnityEngine.Object.Destroy(obj.GetComponent<Humanoid>());
    // Destructible behavior.
    UnityEngine.Object.Destroy(obj.GetComponent<TreeLog>());
    UnityEngine.Object.Destroy(obj.GetComponent<TreeBase>());
    UnityEngine.Object.Destroy(obj.GetComponent<MineRock>());
    UnityEngine.Object.Destroy(obj.GetComponent<Windmill>());
    UnityEngine.Object.Destroy(obj.GetComponent<SpawnArea>());
    UnityEngine.Object.Destroy(obj.GetComponent<CreatureSpawner>());
    UnityEngine.Object.Destroy(obj.GetComponent<TombStone>());
    UnityEngine.Object.Destroy(obj.GetComponent<MineRock5>());
    // Other
    UnityEngine.Object.Destroy(obj.GetComponent<HoverText>());
    UnityEngine.Object.Destroy(obj.GetComponent<Aoe>());
    UnityEngine.Object.Destroy(obj.GetComponentInChildren<MusicLocation>());
    UnityEngine.Object.Destroy(obj.GetComponentInChildren<SpawnArea>());
  }

  ///<summary>Placement requires the Piece component.</summary>
  public static void EnsurePiece(GameObject obj) {
    if (obj.GetComponent<Piece>()) return;
    var piece = obj.AddComponent<Piece>();
    var proxy = obj.GetComponent<LocationProxy>();
    if (proxy && proxy.m_instance)
      piece.m_name = Utils.GetPrefabName(proxy.m_instance);
    else
      piece.m_name = Utils.GetPrefabName(obj);
    piece.m_clipEverything = true;
  }
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZNetView view) => view && IsValid(view.GetZDO());
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZDO zdo) => zdo != null && zdo.IsValid();
  public static void CheatCheck() {
    if (!Settings.IsCheats) throw new InvalidOperationException("This command is disabled.");
  }
  public static void ArgsCheck(Terminal.ConsoleEventArgs args, int amount, string message) {
    if (args.Length < amount) throw new InvalidOperationException(message);
  }
  public static void Command(string name, string description, Terminal.ConsoleEvent action, Terminal.ConsoleOptionsFetcher? fetcher = null) {
    new Terminal.ConsoleCommand(name, description, Helper.Catch(action), optionsFetcher: fetcher);
  }
  public static Terminal.ConsoleEvent Catch(Terminal.ConsoleEvent action) =>
    (args) => {
      try {
        if (!Player.m_localPlayer) throw new InvalidOperationException("Player not found.");
        action(args);
      } catch (InvalidOperationException e) {
        Helper.AddError(args.Context, e.Message);
      }
    };
}
public class Hovered {
  public ZNetView Obj;
  public int Index;
  public Hovered(ZNetView obj, int index) {
    Obj = obj;
    Index = index;
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.Message))]
public class ReplaceMessage {
  public static string Message = "";
  public static void Prefix(ref string msg) {
    if (Message != "") {
      msg = Message;
      Message = "";
    }
  }
}
