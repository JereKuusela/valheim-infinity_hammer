using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace InfinityHammer;
public class HammerSelect {
  ///<summary>Returns the hovered object.</summary>
  private static ZNetView GetHovered() {
    var range = Configuration.SelectRange == 0 ? Player.m_localPlayer.m_maxInteractDistance : Configuration.SelectRange;
    var hovered = Helper.GetHovered(Player.m_localPlayer, range, Configuration.SelectBlacklist);
    if (hovered == null) throw new InvalidOperationException("Nothing is being hovered.");
    return hovered.Obj;
  }
  private static float GetX(float x, float y, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * y;
  private static float GetY(float x, float y, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
  private static bool Within(Vector3 position, Vector3 center, float angle, float width, float depth, float height) {
    var dx = position.x - center.x;
    var dz = position.z - center.z;
    var distanceX = GetX(dx, dz, angle);
    var distanceZ = GetY(dx, dz, angle);
    if (position.y - center.y > (height == 0f ? 1000f : height)) return false;
    if (Mathf.Abs(distanceX) > width) return false;
    if (Mathf.Abs(distanceZ) > depth) return false;
    return true;
  }
  private static bool Within(Vector3 position, Vector3 center, float radius, float height) {
    return Utils.DistanceXZ(position, center) <= radius && position.y - center.y <= (height == 0f ? 1000f : height);
  }
  private static bool ValidName(string name) => name != "Player" && !name.StartsWith("_", StringComparison.Ordinal);
  private static ZNetView[] GetNearby(Func<Vector3, bool> checker) {
    var scene = ZNetScene.instance.m_instances.Values;
    var views = scene.Where(view => view.GetZDO() != null && view.GetZDO().IsValid()).Where(view => ValidName(Utils.GetPrefabName(view.gameObject))).Where(view => checker(view.GetZDO().GetPosition())).ToArray();
    if (views.Length == 0) throw new InvalidOperationException("Nothing is nearby.");
    return views;
  }
  private static ZNetView[] GetConnected(ZNetView baseView) {
    var baseWear = baseView.GetComponent<WearNTear>();
    if (baseWear == null) throw new InvalidOperationException("Connected doesn't work for this object.");
    HashSet<ZNetView> views = new() { baseView };
    Queue<WearNTear> todo = new();
    todo.Enqueue(baseWear);
    while (todo.Count > 0) {
      var wear = todo.Dequeue();
      if (wear.m_colliders == null) wear.SetupColliders();
      foreach (var boundData in wear.m_bounds) {
        var boxes = Physics.OverlapBoxNonAlloc(boundData.m_pos, boundData.m_size, WearNTear.m_tempColliders, boundData.m_rot, WearNTear.m_rayMask);
        for (int i = 0; i < boxes; i++) {
          var collider = WearNTear.m_tempColliders[i];
          if (collider.isTrigger || collider.attachedRigidbody != null || wear.m_colliders.Contains(collider)) continue;
          var wear2 = collider.GetComponentInParent<WearNTear>();
          if (!wear2 || !wear2.m_nview) continue;
          if (views.Contains(wear2.m_nview)) continue;
          views.Add(wear2.m_nview);
          todo.Enqueue(wear2);
        }
      }
    }
    return views.ToArray();
  }
  private static void PrintSelected(Terminal terminal, GameObject obj) {
    if (Configuration.DisableSelectMessages) return;
    var scale = obj.transform.localScale;
    var view = obj.GetComponent<ZNetView>();
    var name = Utils.GetPrefabName(obj);
    if (view && view.m_syncInitialScale) {
      if (scale.x == scale.y && scale.y == scale.z)
        Helper.AddMessage(terminal, $"Selected {name} (size {scale.y.ToString("P0")}).");
      else
        Helper.AddMessage(terminal, $"Selected {name} (scale {scale.ToString("F2")}).");
    } else
      Helper.AddMessage(terminal, $"Selected {name}.");
  }
  private void UpdateZDOs(Action<ZDO> action) {
    for (var i = 0; i < Selection.Objects.Count; i++) {
      var zdo = Selection.Objects[i].Data;
      zdo ??= new ZDO();
      action(zdo);
      Selection.Objects[i].Data = zdo;
    }
  }
  public HammerSelect() {
    List<string> named = new() {
      "scale", "radius", "Level", "stars", "connected", "from", "health"
    };
    named.Sort();
    CommandWrapper.Register("hammer", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return named;
    }, new() {
      { "scale", (int index) => CommandWrapper.Scale("scale", "Size of the object (if the object can be scaled).", index) },
      { "level", (int index) => CommandWrapper.Info("Level.") },
      { "stars", (int index) => CommandWrapper.Info("Stars.") },
      { "from", (int index) => CommandWrapper.Info("Position.") },
      { "text", (int index) => CommandWrapper.Info("Text.") },
      { "health", (int index) => CommandWrapper.Info("Health.") },
      { "connect", (int index) => CommandWrapper.Info("piece") },
    });
    Helper.Command("hammer", "[object id or radius] - Selects the object to be placed (the hovered object by default).", (args) => {
      Helper.EnabledCheck();
      Hammer.Equip(Tool.Hammer);
      HammerParameters pars = new(args);
      GameObject selected;
      if (pars.Radius.HasValue)
        selected = Selection.Set(GetNearby(pos => Within(pos, pars.Position, pars.Radius.Value, pars.Height)), pars.Scale);
      else if (pars.Width.HasValue && pars.Depth.HasValue)
        selected = Selection.Set(GetNearby(pos => Within(pos, pars.Position, pars.Angle, pars.Width.Value, pars.Depth.Value, pars.Height)), pars.Scale);
      else if (args.Length > 1 && !args[1].Contains("=") && args[1] != "connect")
        selected = Selection.Set(args[1], pars.Scale);
      else {
        var hovered = GetHovered();
        if (pars.Connect)
          selected = Selection.Set(GetConnected(hovered), pars.Scale);
        else
          selected = Selection.Set(hovered, pars.Scale);
      }
      if (pars.Health.HasValue)
        UpdateZDOs(zdo => zdo.Set("health", pars.Health.Value));
      if (pars.Level.HasValue)
        UpdateZDOs(zdo => zdo.Set("level", pars.Level.Value));
      if (pars.Text != null)
        UpdateZDOs(zdo => zdo.Set("text", pars.Text));
      PrintSelected(args.Context, selected);
    }, CommandWrapper.ObjectIds);
  }
}
