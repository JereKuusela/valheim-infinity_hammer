using System;
using System.Collections.Generic;
using HarmonyLib;
using InfinityTools;
using Service;
using UnityEngine;
namespace InfinityHammer;

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

  private static Piece BuildMenuButton()
  {
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuTool>();
    var toolData = new ToolData()
    {
      name = "Menu",
      description = "Open custom menu",
      icon = "\u2630",
      command = "hammer_menu",
      instant = true
    };
    piece.tool = new Tool(toolData);
    piece.m_description = "Open custom menu";
    piece.m_name = "Menu";
    piece.m_icon = piece.tool.Icon;
    return piece;
  }

  private static void AddMenuButtonToTabs(PieceTable __instance)
  {
    // Add menu button to the start of each tab
    for (int i = 0; i < __instance.m_availablePieces.Count; i++)
    {
      var pieces = __instance.m_availablePieces[i];
      if (pieces.Count > 0)
      {
        var menuButton = BuildMenuButton();
        // Set the category for the menu button
        menuButton.m_category = (Piece.PieceCategory)i;
        pieces.Insert(0, menuButton);
      }
    }
  }

  static bool Prefix(PieceTable __instance)
  {
    if (!Configuration.IsCheats) return true;
    if (!Configuration.ToolsEnabled) return true;
    return CustomBuildMenu.BuildOriginal(__instance);
  }
  static void Postfix(PieceTable __instance)
  {
    if (!Configuration.IsCheats) return;
    if (!Configuration.ToolsEnabled) return;
    if (HammerMenuCommand.CurrentMode != MenuMode.Default)
    {
      CustomBuildMenu.HandleCustomMenuMode(__instance);
      return;
    }


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
    if (false && Configuration.ShowMenuButton)
      AddMenuButtonToTabs(__instance);
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
    var pt = __instance.m_buildPieces;
    if (!pt) return true;
    var piece = pt.GetPiece(p);
    if (piece && piece.TryGetComponent<BuildMenuTool>(out var menuTool) && menuTool.tool != null)
    {
      // Just something else to not trigger continuous tools instantly.
      __instance.m_placePressedTime = -9998f;
      var tool = menuTool.tool;
      if (!tool.Instant) return true;
      var previousSelection = Selection.Get();
      Console.instance.TryRunCommand(tool.GetCommand());
      // Bit of a hack, but if the command changed the selection then it should be selected on the build menu.
      if (previousSelection != Selection.Get())
        pt.m_selectedPiece[(int)pt.GetSelectedCategory()] = p;
      return false;
    }
    return true;
  }

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.LeftPiece)), HarmonyPostfix]
  private static void HandleLeftPiece(PieceTable __instance) => ActivatePiece(__instance);

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.RightPiece)), HarmonyPostfix]
  private static void HandleRightPiece(PieceTable __instance) => ActivatePiece(__instance);

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpPiece)), HarmonyPostfix]
  private static void HandleUpPiece(PieceTable __instance) => ActivatePiece(__instance);

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.DownPiece)), HarmonyPostfix]
  private static void HandleDownPiece(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetCategory)), HarmonyPostfix]
  private static void HandleSetCategory(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.PrevCategory)), HarmonyPostfix]
  private static void HandlePrevCategory(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.NextCategory)), HarmonyPostfix]
  private static void HandleNextCategory(PieceTable __instance) => ActivatePiece(__instance);
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetSelected)), HarmonyPostfix]
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
      if (!tool.Instant)
        Console.instance.TryRunCommand($"tool {tool.Name}");
      // Tool command resets the selection to empty so restore it.
      pt.m_selectedPiece[(int)pt.GetSelectedCategory()] = index;
    }
    else if (piece.GetComponent<ZNetView>())
      Selection.CreateGhost(new ObjectSelection(piece, false));
    else
      Selection.Clear();
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