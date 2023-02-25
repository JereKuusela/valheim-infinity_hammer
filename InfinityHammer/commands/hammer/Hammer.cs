using System;
using System.Collections.Generic;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class HammerSelect
{
  private static int WearNumber(Wear wear)
  {
    if (wear == Wear.Broken) return 0;
    if (wear == Wear.Damaged) return 1;
    if (wear == Wear.Healthy) return 2;
    return -1;
  }
  private static int GrowthNumber(Growth growth)
  {
    if (growth == Growth.Healthy) return 0;
    if (growth == Growth.Unhealthy) return 1;
    if (growth == Growth.HealthyGrown) return 2;
    if (growth == Growth.UnhealthyGrown) return 3;
    return -1;
  }
  private static int FallNumber(Fall fall)
  {
    if (fall == Fall.Off) return 0;
    if (fall == Fall.Terrain) return 1;
    if (fall == Fall.Solid) return 2;
    return -1;
  }

  private static void PrintSelected(Terminal terminal, GameObject obj)
  {
    if (Configuration.DisableSelectMessages) return;
    var scale = obj.transform.localScale;
    var view = obj.GetComponent<ZNetView>();
    var name = Utils.GetPrefabName(obj);
    if (view && view.m_syncInitialScale)
    {
      if (scale.x == scale.y && scale.y == scale.z)
        Helper.AddMessage(terminal, $"Selected {name} (size {scale.y.ToString("P0")}).");
      else
        Helper.AddMessage(terminal, $"Selected {name} (scale {scale.ToString("F2")}).");
    }
    else
      Helper.AddMessage(terminal, $"Selected {name}.");
  }
  private void UpdateZDOs(Action<ZDO> action)
  {
    for (var i = 0; i < Selection.Objects.Count; i++)
    {
      var zdo = Selection.Objects[i].Data;
      zdo ??= new();
      action(zdo);
      Selection.Objects[i].Data = zdo;
    }
  }
  public HammerSelect()
  {
    List<string> named = new() {
      "freeze", "pick", "scale", "level", "stars", "connect", "health", "type",
    };
    if (InfinityHammer.StructureTweaks)
    {
      List<string> namedStructure = new() {
        "growth", "wear", "show", "collision", "interact", "fall", "restrict"
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
    List<string> Falls = new() {
      "off",
      "solid",
      "terrain"
    };
    List<string> False = new() {
      "false",
    };
    named.Sort();
    CommandWrapper.Register("hammer", (int index, int subIndex) =>
    {
      if (index == 0) return CommandWrapper.ObjectIds();
      return named;
    }, new() {
      { "scale", (int index) => CommandWrapper.Scale("scale", "Size of the object (if the object can be scaled).", index) },
      { "level", (int index) => CommandWrapper.Info("Level.") },
      { "stars", (int index) => CommandWrapper.Info("Stars.") },
      { "text", (int index) => CommandWrapper.Info("Text.") },
      { "health", (int index) => CommandWrapper.Info("Health.") },
      { "freeze", (int index) => CommandWrapper.Info("Freezes in the place.") },
      { "pick", (int index) => CommandWrapper.Info("Picks up the selection.") },
      { "connect", (int index) => CommandWrapper.Info("Selects whole building.") },
      { "type", (int index) => ObjectTypes },
      { "wear", (int index) => Wears },
      { "fall", (int index) => Falls },
      { "growth", (int index) => Growths },
      { "show", (int index) => False },
      { "collision", (int index) => False },
      { "interact", (int index) => False },
      { "restrict", (int index) => False },
    });
    Helper.Command("hammer", "[object id] - Selects the object to be placed (the hovered object by default).", (args) =>
    {
      Helper.EnabledCheck();
      Hammer.Equip(Equipment.Hammer);
      HammerParameters pars = new(args);
      GameObject? selected = null;
      ZNetView[] views = new ZNetView[0];
      if (pars.Radius.HasValue)
        views = Selector.GetNearby("", pars.ObjectType, Configuration.IgnoredIds, pars.Position, pars.Radius.Value, pars.Height);
      else if (pars.Width.HasValue && pars.Depth.HasValue)
        views = Selector.GetNearby("", pars.ObjectType, Configuration.IgnoredIds, pars.Position, pars.Angle, pars.Width.Value, pars.Depth.Value, pars.Height);
      else if (args.Length > 1 && !args[1].Contains("=") && !pars.Connect && !pars.Pick && !pars.Freeze)
        selected = Selection.Set(args[1], pars.Pick);
      else
      {
        var hovered = Selector.GetHovered(Configuration.Range, Configuration.IgnoredIds);
        if (hovered == null) throw new InvalidOperationException("Nothing is being hovered.");
        if (pars.Connect)
          views = Selector.GetConnected(hovered, Configuration.IgnoredIds);
        else
          views = new ZNetView[] { hovered };
      }
      if (selected == null && views.Length > 0)
      {
        selected = Selection.Set(views, pars.Pick);
        if (pars.Pick)
        {
          foreach (var view in views)
          {
            RemovePiece.AddRemovedObject(view);
            Helper.RemoveZDO(view.GetZDO());
          }
          Undo.Remove(RemovePiece.RemovedObjects);
          RemovePiece.RemovedObjects.Clear();
        }
      }
      if (selected == null) return;
      if (pars.Health.HasValue)
        UpdateZDOs(zdo => zdo.Set(Hash.Health, pars.Health.Value));
      if (pars.Level.HasValue)
        UpdateZDOs(zdo => zdo.Set(Hash.Level, pars.Level.Value));
      if (pars.Growth != Growth.Default)
      {
        UpdateZDOs(zdo => zdo.Set(Hash.Growth, GrowthNumber(pars.Growth)));
        UpdateZDOs(zdo => zdo.Set(Hash.PlantTime, DateTime.MaxValue.Ticks / 2L));
      }
      if (pars.Fall != Fall.Default)
        UpdateZDOs(zdo => zdo.Set(Hash.Fall, FallNumber(pars.Fall)));
      if (pars.Wear != Wear.Default)
        UpdateZDOs(zdo => zdo.Set(Hash.Wear, WearNumber(pars.Wear)));
      if (!pars.Collision)
        UpdateZDOs(zdo => zdo.Set(Hash.Collision, false));
      if (!pars.Show)
        UpdateZDOs(zdo => zdo.Set(Hash.Render, false));
      if (!pars.Restrict)
        UpdateZDOs(zdo => zdo.Set(Hash.Restrict, false));
      if (!pars.Interact)
        UpdateZDOs(zdo => zdo.Set(Hash.Interact, false));
      if (pars.Text != null)
        UpdateZDOs(zdo => zdo.Set(Hash.Text, pars.Text));
      Selection.Postprocess(pars.Scale);
      if (pars.Freeze) Position.Freeze(selected.transform.position);
      PrintSelected(args.Context, selected);
    }, CommandWrapper.ObjectIds);
  }
}
