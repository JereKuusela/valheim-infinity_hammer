using System;
using System.Collections.Generic;
using HarmonyLib;
using InfinityHammer;
using UnityEngine;
namespace InfinityTools;

public class BuildMenuTool : Piece
{
  public Tool? tool;
}
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
public static class UpdateAvailable
{
  private static Piece Build(Tool tool)
  {
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuTool>();
    piece.tool = tool;
    piece.m_description = tool.Description;
    piece.m_name = tool.Name;
    piece.m_icon = tool.Icon;
    return piece;
  }
  static void Postfix(PieceTable __instance)
  {
    if (!Configuration.IsCheats) return;
    if (!Configuration.ToolsEnabled) return;
    var hammer = Hammer.Get();
    List<Tool> tools = ToolManager.Get(hammer);
    int tab = 0;
    Dictionary<int, int> indices = [];
    foreach (var tool in tools)
    {
      tab = tool.TabIndex ?? tab;
      if (__instance.m_availablePieces.Count <= tab) return;
      if (!indices.ContainsKey(tab))
        indices[tab] = hammer == "hammer" ? 0 : __instance.m_availablePieces[tab].Count - 1;
      var index = tool.Index ?? indices[tab] + 1;
      var pieces = __instance.m_availablePieces[tab];
      index = Math.Min(index, pieces.Count);
      pieces.Insert(index, Build(tool));
      indices[tab] = index;
    }
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Vector2Int))]
public class RunBuildMenuCommands
{
  [HarmonyPriority(Priority.Low)]
  public static bool Prefix(Player __instance, Vector2Int p)
  {
    var piece = __instance.GetPiece(p);
    if (piece && piece.TryGetComponent<BuildMenuTool>(out var menuTool) && menuTool.tool != null)
    {
      var tool = menuTool.tool;
      if (tool.Instant) Console.instance.TryRunCommand(tool.Command);
      else
      {
        InfinityHammer.HoldUse.Selecting = true;
        Console.instance.TryRunCommand($"tool {tool.Name}");
        var pieces = __instance.m_buildPieces;
        // Must be set directly because SetSelected triggers object selection.
        pieces.m_selectedPiece[(int)pieces.m_selectedCategory] = p;
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