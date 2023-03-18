using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

public class BuildMenuTool : Piece
{
  public ToolData tool = new();
}
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
public static class UpdateAvailable
{
  private static Piece Build(ToolData tool)
  {
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuTool>();
    piece.tool = tool;
    piece.m_description = tool.description;
    piece.m_name = tool.name;
    piece.m_icon = Helper.FindSprite(tool.icon);
    return piece;
  }
  static void Postfix(PieceTable __instance)
  {
    if (!Configuration.IsCheats) return;
    List<ToolData>? commands = null;
    int tab = 0;
    int index = 0;
    if (Hammer.HasTool(Helper.GetPlayer(), Equipment.Hammer))
    {
      commands = ToolManager.HammerTools;
      tab = Configuration.HammerMenuTab;
      index = Configuration.HammerMenuIndex;
    }
    if (Hammer.HasTool(Helper.GetPlayer(), Equipment.Hoe))
    {
      commands = ToolManager.HoeTools;
      tab = Configuration.HoeMenuTab;
      index = Configuration.HoeMenuIndex;
    }
    if (commands == null) return;
    if (__instance.m_availablePieces.Count <= tab) return;
    var pieces = __instance.m_availablePieces[tab];
    index = Math.Min(index, pieces.Count);
    foreach (var command in commands.Reverse<ToolData>())
      pieces.Insert(index, Build(command));
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece))]
public class RunBuildMenuCommands
{
  public static bool InstantCommand = false;

  [HarmonyPriority(Priority.Low)]
  public static bool Prefix(Player __instance, Vector2Int p)
  {
    var piece = __instance.GetPiece(p);
    if (piece && piece.TryGetComponent<BuildMenuTool>(out var menuTool))
    {
      var tool = menuTool.tool;
      if (tool.instant) Console.instance.TryRunCommand(tool.command);
      else
      {
        if (Hammer.HasTool(Helper.GetPlayer(), Equipment.Hammer))
          Console.instance.TryRunCommand($"hammer_tool {tool.name}");
        if (Hammer.HasTool(Helper.GetPlayer(), Equipment.Hoe))
          Console.instance.TryRunCommand($"hoe_tool {tool.name}");
        __instance.m_buildPieces.SetSelected(p);
      }
      return false;
    }
    return true;
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
public class ReplaceModifierKeys
{

  [HarmonyPriority(Priority.High)]
  static void Prefix(ref string text)
  {
    if (text.StartsWith("bind ", StringComparison.OrdinalIgnoreCase)) return;
    if (text.StartsWith("alias ", StringComparison.OrdinalIgnoreCase)) return;
    text = text.Replace(ToolManager.CmdMod1, Configuration.ModifierKey1());
    text = text.Replace(ToolManager.CmdMod2, Configuration.ModifierKey2());
  }
}
