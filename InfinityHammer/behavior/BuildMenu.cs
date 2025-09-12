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

[HarmonyPatch]
public class TakeOverBuildMenu
{

  [HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Vector2Int))]
  [HarmonyPrefix]
  [HarmonyPriority(Priority.Low)]
  public static bool HandleSetSelectedPiece(Player __instance, Vector2Int p)
  {
    // Just something else to not trigger continuous tools instantly.
    __instance.m_placePressedTime = -9998f;
    var wasInstant = HandleInstantTool(__instance.m_buildPieces, p);
    return !wasInstant;
  }
  // Instant tools don't get selected when activated.
  private static bool HandleInstantTool(PieceTable pt, Vector2Int p)
  {
    var piece = pt.GetPiece(p);
    if (piece && piece.TryGetComponent<BuildMenuTool>(out var menuTool) && menuTool.tool != null)
    {
      var tool = menuTool.tool;
      if (tool.Instant) Console.instance.TryRunCommand(tool.GetCommand());
      return tool.Instant;
    }
    return false;
  }

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.LeftPiece))]
  [HarmonyPostfix]
  private static void HandleLeftPiece(PieceTable __instance) => ActivatePiece(__instance);

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.RightPiece))]
  [HarmonyPostfix]
  private static void HandleRightPiece(PieceTable __instance) => ActivatePiece(__instance);

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpPiece))]
  [HarmonyPostfix]
  private static void HandleUpPiece(PieceTable __instance) => ActivatePiece(__instance);

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.DownPiece))]
  [HarmonyPostfix]
  private static void HandleDownPiece(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetCategory))]
  [HarmonyPostfix]
  private static void HandleSetCategory(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.PrevCategory))]
  [HarmonyPostfix]
  private static void HandlePrevCategory(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.NextCategory))]
  [HarmonyPostfix]
  private static void HandleNextCategory(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetSelected))]
  [HarmonyPostfix]
  private static void HandleSetSelected(PieceTable __instance) => ActivatePiece(__instance);

  private static void ActivatePiece(PieceTable pt)
  {
    var index = pt.GetSelectedIndex();
    var piece = pt.GetPiece(pt.GetSelectedCategory(), index);
    if (!piece)
      Selection.Clear();
    else if (piece.TryGetComponent<BuildMenuTool>(out var menuTool) && menuTool.tool != null)
    {
      var tool = menuTool.tool;
      Console.instance.TryRunCommand($"tool {tool.Name}");
    }
    else if (piece.GetComponent<ZNetView>())
      Selection.CreateGhost(new ObjectSelection(piece, false));
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