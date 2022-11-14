using System;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;
public static class Helper
{
  public static GameObject GetPlacementGhost()
  {
    var player = GetPlayer();
    if (!player.m_placementGhost) throw new InvalidOperationException("Not currently placing anything.");
    return player.m_placementGhost;
  }
  public static string GetTool()
  {
    var player = GetPlayer();
    var item = player.GetRightItem();
    return item?.m_dropPrefab?.name ?? "";
  }
  public static void RemoveZDO(ZDO zdo)
  {
    if (!Selector.IsValid(zdo)) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.GetMyID());
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view))
      ZNetScene.instance.Destroy(view.gameObject);
    else
      ZDOMan.instance.DestroyZDO(zdo);
  }

  public static float ParseFloat(string value, float defaultValue = 0)
  {
    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
    return defaultValue;
  }
  public static int ParseInt(string value, int defaultValue = 0)
  {
    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)) return result;
    return defaultValue;
  }
  private static Range<string> ParseRange(string arg)
  {
    var range = arg.Split('-').ToList();
    if (range.Count > 1 && range[0] == "")
    {
      range[0] = "-" + range[1];
      range.RemoveAt(1);
    }
    if (range.Count > 2 && range[1] == "")
    {
      range[1] = "-" + range[2];
      range.RemoveAt(2);
    }
    if (range.Count == 1) return new(range[0]);
    else return new(range[0], range[1]);

  }
  public static Range<int> ParseIntRange(string value, int defaultValue = 0)
  {
    var range = ParseRange(value);
    return ZeroIntRange(new(ParseInt(range.Min, defaultValue), ParseInt(range.Max, defaultValue)));
  }
  public static Range<int> ZeroIntRange(Range<int> range)
  {
    if (range.Max == range.Min)
    {
      if (range.Max > 0)
      {
        range.Max--;
        range.Min = 0;
      }
      if (range.Max < 0)
      {
        range.Max = 0;
        range.Min++;
      }
    }
    return range;
  }
  public static Vector3 ParseZYX(string value)
  {
    var vector = Vector3.zero;
    var split = value.Split(',');
    if (split.Length > 0) vector.z = Helper.ParseFloat(split[0]);
    if (split.Length > 1) vector.y = Helper.ParseFloat(split[1]);
    if (split.Length > 2) vector.x = Helper.ParseFloat(split[2]);
    return vector;
  }
  public static Vector3 ParseXYZ(string value) => ParseXYZ(value, Vector3.zero);
  public static Vector3 ParseXYZ(string value, Vector3 defaultValue)
  {
    var vector = defaultValue;
    var split = value.Split(',');
    if (split.Length > 0) vector.x = Helper.ParseFloat(split[0]);
    if (split.Length > 1) vector.y = Helper.ParseFloat(split[1]);
    if (split.Length > 2) vector.z = Helper.ParseFloat(split[2]);
    return vector;
  }
  public static Range<Vector3Int> ParseZYXRange(string value)
  {
    var min = Vector3Int.zero;
    var max = Vector3Int.zero;
    var split = value.Split(',');
    if (split.Length > 0)
    {
      var range = Helper.ParseIntRange(split[0]);
      min.z = range.Min;
      max.z = range.Max;
    }
    if (split.Length > 1)
    {
      var range = Helper.ParseIntRange(split[1]);
      min.y = range.Min;
      max.y = range.Max;
    }
    if (split.Length > 2)
    {
      var range = Helper.ParseIntRange(split[2]);
      min.x = range.Min;
      max.x = range.Max;
    }
    return new(min, max);
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

  public static void AddError(Terminal context, string message, bool priority = true)
  {
    AddMessage(context, $"Error: {message}", priority);
  }
  public static void AddMessage(Terminal context, string message, bool priority = true)
  {
    if (context == Console.instance || Configuration.ChatOutput)
      context.AddString(message);
    var hud = MessageHud.instance;
    if (!hud || Configuration.DisableMessages) return;
    if (priority)
    {
      var items = hud.m_msgQeue.ToArray();
      hud.m_msgQeue.Clear();
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
      foreach (var item in items)
        hud.m_msgQeue.Enqueue(item);
      hud.m_msgQueueTimer = 10f;
    }
    else
    {
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
    }
  }

  public static Player GetPlayer()
  {
    var player = Player.m_localPlayer;
    if (!player) throw new InvalidOperationException("No player.");
    return player;
  }
  ///<summary>Initializing the copy as inactive is the best way to avoid any script errors. ZNet stuff also won't run.</summary>
  public static GameObject SafeInstantiate(GameObject obj)
  {
    var wear = obj.GetComponent<WearNTear>();
    var highlight = wear && wear.m_oldMaterials != null;
    if (highlight)
      wear.ResetHighlight();
    obj.SetActive(false);
    var ret = UnityEngine.Object.Instantiate(obj);
    Helper.CleanObject(ret);
    obj.SetActive(true);
    if (highlight)
      wear.Highlight();
    return ret;
  }
  public static GameObject SafeInstantiate(GameObject obj, GameObject parent)
  {
    var wear = obj.GetComponent<WearNTear>();
    var highlight = wear && wear.m_oldMaterials != null;
    if (highlight)
      wear.ResetHighlight();
    obj.SetActive(false);
    var ret = UnityEngine.Object.Instantiate(obj, parent.transform);
    Helper.CleanObject(ret);
    obj.SetActive(true);
    if (highlight)
      wear.Highlight();
    return ret;
  }
  public static GameObject SafeInstantiate(string name, GameObject parent)
  {
    var obj = ZNetScene.instance.GetPrefab(name);
    if (!obj) throw new InvalidOperationException($"Missing object {name}.");
    return SafeInstantiate(obj, parent);
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
  public static bool IsSnapPoint(GameObject obj) => obj.CompareTag("snappoint");
  ///<summary>Removes scripts that try to run (for example placement needs only the model and Piece component).</summary>
  public static void CleanObject(GameObject obj)
  {
    if (!obj || !Configuration.Enabled) return;
    // Creature behavior.
    if (obj.GetComponent<Character>() is { } character) character.enabled = false;
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
  }
  public static void CheatCheck()
  {
    if (!Configuration.IsCheats) throw new InvalidOperationException("This command is disabled.");
  }
  public static void EnabledCheck()
  {
    if (!Configuration.Enabled) throw new InvalidOperationException("Infinity Hammer is disabled.");
  }
  public static void ArgsCheck(Terminal.ConsoleEventArgs args, int amount, string message)
  {
    if (args.Length < amount) throw new InvalidOperationException(message);
  }
  public static void Command(string name, string description, Terminal.ConsoleEvent action, Terminal.ConsoleOptionsFetcher? fetcher = null)
  {
    new Terminal.ConsoleCommand(name, description, Helper.Catch(action), optionsFetcher: fetcher);
  }
  public static Terminal.ConsoleEvent Catch(Terminal.ConsoleEvent action) =>
    (args) =>
    {
      try
      {
        if (!Player.m_localPlayer) throw new InvalidOperationException("Player not found.");
        action(args);
      }
      catch (InvalidOperationException e)
      {
        Helper.AddError(args.Context, e.Message);
      }
    };

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
      if (Helper.IsSnapPoint(tr.gameObject)) count++;
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
