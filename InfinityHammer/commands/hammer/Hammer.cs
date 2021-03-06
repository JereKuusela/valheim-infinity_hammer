using System;
using System.Collections.Generic;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class HammerSelect {
  private static int WearNumber(Wear wear) {
    if (wear == Wear.Broken) return 0;
    if (wear == Wear.Damaged) return 1;
    if (wear == Wear.Healthy) return 2;
    return -1;
  }
  private static int GrowthNumber(Growth growth) {
    if (growth == Growth.Healthy) return 0;
    if (growth == Growth.Unhealthy) return 1;
    if (growth == Growth.HealthyGrown) return 2;
    if (growth == Growth.UnhealthyGrown) return 3;
    return -1;
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
      "scale", "radius", "level", "stars", "connected", "from", "health", "type",
    };
    if (CommandWrapper.StructureTweaks != null) {
      List<string> namedStructure = new() {
        "growth", "wear", "show", "collision", "interact"
      };
      named.AddRange(namedStructure);
    }
    List<string> Wears = new() {
      "default",
      "broken",
      "damaged",
      "healthy"
    };
    List<string> Growths = new() {
      "big",
      "big_bad",
      "default",
      "small",
      "small_bad"
    };
    List<string> ObjectTypes = new() {
      "creature",
      "structure"
    };
    List<string> False = new() {
      "false",
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
      { "type", (int index) => ObjectTypes },
      { "wear", (int index) => Wears },
      { "growth", (int index) => Growths },
      { "show", (int index) => False },
      { "collision", (int index) => False },
      { "interact", (int index) => False },
    });
    Helper.Command("hammer", "[object id or radius] - Selects the object to be placed (the hovered object by default).", (args) => {
      Helper.EnabledCheck();
      Hammer.Equip(Tool.Hammer);
      HammerParameters pars = new(args);
      GameObject selected;
      if (pars.Radius.HasValue)
        selected = Selection.Set(Selector.GetNearby(pars.ObjectType, pars.Position, pars.Radius.Value, pars.Height));
      else if (pars.Width.HasValue && pars.Depth.HasValue)
        selected = Selection.Set(Selector.GetNearby(pars.ObjectType, pars.Position, pars.Angle, pars.Width.Value, pars.Depth.Value, pars.Height));
      else if (args.Length > 1 && !args[1].Contains("=") && args[1] != "connect")
        selected = Selection.Set(args[1]);
      else {
        var hovered = Selector.GetHovered(Configuration.SelectRange, Configuration.SelectBlacklist);
        if (pars.Connect)
          selected = Selection.Set(Selector.GetConnected(hovered));
        else
          selected = Selection.Set(hovered);
      }
      if (pars.Health.HasValue)
        UpdateZDOs(zdo => zdo.Set(Hash.Health, pars.Health.Value));
      if (pars.Level.HasValue)
        UpdateZDOs(zdo => zdo.Set(Hash.Level, pars.Level.Value));
      if (pars.Growth != Growth.Default) {
        UpdateZDOs(zdo => zdo.Set(Hash.Growth, GrowthNumber(pars.Growth)));
        UpdateZDOs(zdo => zdo.Set(Hash.PlantTime, DateTime.MaxValue.Ticks / 2L));
      }
      if (pars.Wear != Wear.Default)
        UpdateZDOs(zdo => zdo.Set(Hash.Wear, WearNumber(pars.Wear)));
      if (!pars.Collision)
        UpdateZDOs(zdo => zdo.Set(Hash.Collision, false));
      if (!pars.Show)
        UpdateZDOs(zdo => zdo.Set(Hash.Render, false));
      if (!pars.Interact)
        UpdateZDOs(zdo => zdo.Set(Hash.Interact, false));
      if (pars.Text != null)
        UpdateZDOs(zdo => zdo.Set(Hash.Text, pars.Text));
      Selection.Postprocess(pars.Scale);
      PrintSelected(args.Context, selected);
    }, CommandWrapper.ObjectIds);
  }
}
