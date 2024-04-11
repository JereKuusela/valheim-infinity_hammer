using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
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
      "freeze", "pick", "scale", "level", "stars", "connect", "health", "type",
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
      { "type", (int index) => ParameterInfo.Components }
    });
    Helper.Command("hammer", "[object id] - Selects the object to be placed (the hovered object by default).", (args) =>
    {
      HammerParameters pars = new(args);
      ZNetView[] views = [];
      if (pars.Radius != null)
        views = Selector.GetNearby([], pars.Components, Configuration.IgnoredIds, pars.Position, pars.Radius, pars.Height);
      else if (pars.Width != null && pars.Depth != null)
        views = Selector.GetNearby([], pars.Components, Configuration.IgnoredIds, pars.Position, pars.Angle, pars.Width, pars.Depth, pars.Height);
      else if (args.Length > 1 && !args[1].Contains("=") && !pars.Connect && !pars.Pick && !pars.Freeze)
      {
        var obj = ZNetScene.instance.GetPrefab(args[1]) ?? throw new InvalidOperationException("Object not found.");
        views = [obj.GetComponent<ZNetView>()];
      }
      else
      {
        var hovered = Selector.GetHovered(Configuration.Range, [], Configuration.IgnoredIds) ?? throw new InvalidOperationException("Nothing is being hovered.");
        if (pars.Connect)
          views = Selector.GetConnected(hovered, [], Configuration.IgnoredIds);
        else
          views = [hovered];
      }
      HammerHelper.Init();
      ObjectSelection selection = views.Length == 1 ? new(views[0], pars.Pick, pars.Scale) : new(views, pars.Pick, pars.Scale);
      ZDOData extraData = new();
      if (pars.Health.HasValue)
      {
        if (selection.GetSelectedPiece().GetComponent<Character>())
        {
          extraData.Set(ZDOVars.s_health, pars.Health.Value * 1.000001f);
          extraData.Set(ZDOVars.s_maxHealth, pars.Health.Value);
        }
        else extraData.Set(ZDOVars.s_health, pars.Health.Value);
      }
      if (pars.Level.HasValue)
        extraData.Set(ZDOVars.s_level, pars.Level.Value);
      if (pars.Growth != Growth.Default)
      {
        extraData.Set(Hash.Growth, GrowthNumber(pars.Growth));
        extraData.Set(ZDOVars.s_plantTime, DateTime.MaxValue.Ticks / 2L);
      }
      if (pars.Wear != Wear.Default)
        extraData.Set(Hash.Wear, WearNumber(pars.Wear));
      if (pars.Fall != Fall.Default)
        extraData.Set(Hash.Fall, FallNumber(pars.Fall));
      if (pars.Wear != Wear.Default)
        extraData.Set(Hash.Wear, WearNumber(pars.Wear));
      if (!pars.Collision)
        extraData.Set(Hash.Collision, false);
      if (!pars.Show)
        extraData.Set(Hash.Render, false);
      if (!pars.Restrict)
        extraData.Set(Hash.Restrict, false);
      if (!pars.Interact)
        extraData.Set(Hash.Interact, false);
      if (pars.Text != null)
        extraData.Set(ZDOVars.s_text, pars.Text);
      selection.UpdateZDOs(extraData);
      selection.Postprocess();
      var ghost = Selection.CreateGhost(selection);
      if (pars.Freeze) Position.Freeze(views.Length > 0 ? views[0].transform.position : Helper.GetPlayer().transform.position);
      if (pars.Pick)
      {
        Undo.AddRemoveStep(views);
        foreach (var view in views)
          HammerHelper.RemoveZDO(view.GetZDO());
      }
      PrintSelected(args.Context, ghost);
    });
  }
}
