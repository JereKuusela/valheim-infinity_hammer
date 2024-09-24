using System;
using System.Collections.Generic;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;
using WorldEditCommands;
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
    var name = Utils.GetPrefabName(obj.gameObject);
    if (view && view.m_syncInitialScale)
    {
      if (scale.x == scale.y && scale.y == scale.z)
        HammerHelper.Message(terminal, $"Selected {name} (size {scale.y:P0}).");
      else
        HammerHelper.Message(terminal, $"Selected {name} (scale {scale:F2}).");
    }
    else
      HammerHelper.Message(terminal, $"Selected {name}.");
  }
  public HammerSelect()
  {
    List<string> named = [
      "freeze", "pick", "scale", "level", "stars", "connect", "health", "type", "include", "ignore", "id", "data",
    ];
    if (InfinityHammer.StructureTweaks)
    {
      List<string> namedStructure = [
        "growth", "wear", "show", "collision", "interact", "fall", "restrict"
      ];
      named.AddRange(namedStructure);
    }
    List<string> Wears = [
      "default",
      "broken",
      "damaged",
      "healthy"
    ];
    List<string> Growths = [
      "big",
      "big_bad",
      "default",
      "small",
      "small_bad"
    ];
    List<string> Falls = [
      "off",
      "solid",
      "terrain"
    ];
    List<string> False = [
      "false",
    ];
    named.Sort();
    AutoComplete.Register("hammer", (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.ObjectIds;
      return named;
    }, new() {
      { "scale", (int index) => ParameterInfo.Scale("scale", "Size of the object (if the object can be scaled).", index) },
      { "level", (int index) => ParameterInfo.Create("Level.") },
      { "stars", (int index) => ParameterInfo.Create("Stars.") },
      { "text", (int index) => ParameterInfo.Create("Text.") },
      { "health", (int index) => ParameterInfo.Create("Health.") },
      { "freeze", (int index) => ParameterInfo.Create("Freezes in the place.") },
      { "pick", (int index) => ParameterInfo.Create("Picks up the selection.") },
      { "connect", (int index) => ParameterInfo.Create("Selects whole building.") },
      { "wear", (int index) => Wears },
      { "fall", (int index) => Falls },
      { "growth", (int index) => Growths },
      { "show", (int index) => False },
      { "collision", (int index) => False },
      { "interact", (int index) => False },
      { "restrict", (int index) => False },
      { "include", (int index) => ParameterInfo.ObjectIds },
      { "ignore", (int index) => ParameterInfo.ObjectIds },
      { "id", (int index) => ParameterInfo.ObjectIds },
      { "type", (int index) => ParameterInfo.Components },
      { "data", (int index) => DataLoading.DataKeys },
    });
    Helper.Command("hammer", "[object id] - Selects the object to be placed (the hovered object by default).", (args) =>
    {
      HammerParameters pars = new(args);
      ZNetView[] views = [];
      if (pars.Radius != null)
        views = Selector.GetNearby(pars.Included, pars.Components, pars.Ignored, pars.Position, pars.Radius, pars.Height);
      else if (pars.Width != null && pars.Depth != null)
        views = Selector.GetNearby(pars.Included, pars.Components, pars.Ignored, pars.Position, pars.Angle, pars.Width, pars.Depth, pars.Height);
      else if (args.Length > 1 && !args[1].Contains("=") && !pars.Connect && !pars.Pick && !pars.Freeze)
      {
        var obj = ZNetScene.instance.GetPrefab(args[1]) ?? throw new InvalidOperationException("Object not found.");
        views = [obj.GetComponent<ZNetView>()];
      }
      else
      {
        var hovered = Selector.GetHovered(Configuration.Range, pars.Included, pars.Ignored) ?? throw new InvalidOperationException("Nothing is being hovered.");
        if (pars.Connect)
          views = Selector.GetConnected(hovered, pars.Included, pars.Ignored);
        else
          views = [hovered];
      }
      if (views.Length == 0) return;
      HammerHelper.Init();
      DataEntry? extraData = pars.Data == null ? null : DataHelper.Get(pars.Data);
      if (pars.Health.HasValue)
      {
        extraData ??= new();
        extraData.Set(ZDOVars.s_health, pars.Health.Value);
      }
      if (pars.Level.HasValue)
      {
        extraData ??= new();
        extraData.Set(ZDOVars.s_level, pars.Level.Value);
      }
      if (pars.Growth != Growth.Default)
      {
        extraData ??= new();
        extraData.Set(Hash.Growth, GrowthNumber(pars.Growth));
        extraData.Set(ZDOVars.s_plantTime, DateTime.MaxValue.Ticks / 2L);
      }
      if (pars.Wear != Wear.Default)
      {
        extraData ??= new();
        extraData.Set(Hash.Wear, WearNumber(pars.Wear));
      }
      if (pars.Fall != Fall.Default)
      {
        extraData ??= new();
        extraData.Set(Hash.Fall, FallNumber(pars.Fall));
      }
      if (pars.Wear != Wear.Default)
      {
        extraData ??= new();
        extraData.Set(Hash.Wear, WearNumber(pars.Wear));
      }
      if (!pars.Collision)
      {
        extraData ??= new();
        extraData.Set(Hash.Collision, false);
      }
      if (!pars.Show)
      {
        extraData ??= new();
        extraData.Set(Hash.Render, false);
      }
      if (!pars.Restrict)
      {
        extraData ??= new();
        extraData.Set(Hash.Restrict, false);
      }
      if (!pars.Interact)
      {
        extraData ??= new();
        extraData.Set(Hash.Interact, false);
      }
      if (pars.Text != null)
      {
        extraData ??= new();
        extraData.Set(Hash.Text, pars.Text);
      }
      ObjectSelection selection = views.Length == 1 ? new(views[0], pars.Pick, pars.Scale, extraData) : new(views, pars.Pick, pars.Scale, extraData);
      var ghost = Selection.CreateGhost(selection);
      if (pars.Freeze) Position.Freeze(views.Length > 0 ? views[0].transform.position : Helper.GetPlayer().transform.position);
      if (pars.Pick)
      {
        UndoHelper.BeginAction();
        foreach (var view in views)
          UndoHelper.AddRemoveAction(view.GetZDO());
        foreach (var view in views)
          HammerHelper.RemoveZDO(view.GetZDO());
        UndoHelper.EndAction();
      }
      PrintSelected(args.Context, ghost);
    });
  }
}
