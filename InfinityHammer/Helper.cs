using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;

namespace InfinityHammer {
  public static class Helper {

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

    public static void AddMessage(Terminal context, string message, bool priority = true) {
      context.AddString(message);
      var hud = MessageHud.instance;
      if (!hud) return;
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

    public static Hovered GetHovered(Player obj, float maxDistance, HashSet<string> blacklist, bool allowOtherPlayers = false) {
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
        return new Hovered() {
          Obj = netView,
          Index = index
        };
      }
      return null;
    }
    ///<summary>Initializing the copy as inactive is the best way to avoid any script errors. ZNet stuff also won't run.</summary>
    public static GameObject SafeInstantiate(GameObject obj) {
      obj.SetActive(false);
      var ret = UnityEngine.Object.Instantiate(obj);
      obj.SetActive(true);
      return ret;
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
    }

    ///<summary>Placement requires the Piece component.</summary>
    public static void EnsurePiece(GameObject obj) {
      if (obj.GetComponent<Piece>()) return;
      var piece = obj.AddComponent<Piece>();
      piece.m_name = Utils.GetPrefabName(obj);
      piece.m_clipEverything = true;
    }
    ///<summary>Helper to check object validity.</summary>
    public static bool IsValid(ZNetView view) => view && IsValid(view.GetZDO());
    ///<summary>Helper to check object validity.</summary>
    public static bool IsValid(ZDO zdo) => zdo != null && zdo.IsValid();
  }
  public class Hovered {
    public ZNetView Obj;
    public int Index;
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
}