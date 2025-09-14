using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using HarmonyLib;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;

[HarmonyPatch]
public class HammerMark
{
  [HarmonyPatch(typeof(Player), nameof(Player.UpdateWearNTearHover)), HarmonyPrefix]
  static bool HighlightSelected()
  {
    foreach (var wtr in MarkedPieces)
      wtr.Highlight();
    return MarkedPieces.Count == 0;
  }
  public static List<WearNTear> MarkedPieces = [];

  private static void PrintMarked(Terminal terminal, GameObject obj)
  {
    if (Configuration.DisableSelectMessages) return;
    var name = Utils.GetPrefabName(obj.gameObject);
    HammerHelper.Message(terminal, $"Marked {name}.");
  }

  public static void ClearMarked()
  {
    MarkedPieces.Clear();
  }

  public static ZNetView[] GetMarkedAsViews()
  {
    return [.. MarkedPieces
      .Where(piece => piece && piece.GetComponent<ZNetView>())
      .Select(piece => piece.GetComponent<ZNetView>())];
  }

  public HammerMark()
  {
    List<string> named = ["connect", "include", "ignore"];
    named.Sort();
    AutoComplete.Register("hammer_mark", (index, subIndex) =>
    {
      if (index == 0) return ParameterInfo.ObjectIds;
      return named;
    }, new() {
      { "connect", index => ParameterInfo.Create("Selects whole building.") },
      { "include", index => ParameterInfo.ObjectIds },
      { "ignore", index => ParameterInfo.ObjectIds },
    });
    AutoComplete.Register("hammer_mark", (int index, int subIndex) => ParameterInfo.None);
    Helper.Command("hammer_mark", "- Marks the selected object for later selection.", (args) =>
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

      foreach (var view in views)
      {
        var wtr = view.GetComponent<WearNTear>();
        if (wtr && !MarkedPieces.Contains(wtr))
        {
          MarkedPieces.Add(wtr);
          PrintMarked(args.Context, view.gameObject);
        }
      }

      HammerHelper.Message(args.Context, $"Total marked pieces: {MarkedPieces.Count}");
    });
  }
}