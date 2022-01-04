using System.Globalization;
using UnityEngine;
using System;

namespace InfinityHammer {
  public static class Helper {

    public static void RemoveZDO(ZDO zdo) {
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

    public static Hovered GetHovered(Player obj) {
      var hits = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, 50f, obj.m_interactMask);
      Array.Sort<RaycastHit>(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
      foreach (var hit in hits) {
        if (Vector3.Distance(hit.point, obj.m_eye.position) >= obj.m_maxPlaceDistance) continue;
        var netView = hit.collider.GetComponentInParent<ZNetView>();
        if (!netView) continue;
        if (netView.GetComponentInChildren<Player>()) continue;
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
  }
  public class Hovered {
    public ZNetView Obj;
    public int Index;
  }
}