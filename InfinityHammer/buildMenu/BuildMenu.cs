using System;
using System.Globalization;
using HarmonyLib;
using InfinityTools;
using ServerDevcommands;
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
  static bool Prefix(PieceTable __instance)
  {
    if (Hammer.IsInfinityHammer(__instance))
    {
      CustomBuildMenu.HandleCustomMenuMode(__instance);
      return false;
    }
    return true;
  }
  [HarmonyPriority(Priority.Low)]
  static void Postfix(PieceTable __instance)
  {
    if (!Configuration.ToolsEnabled) return;
    if (Hammer.IsInfinityHammer(__instance) && HammerMenuCommand.CurrentMode != MenuMode.Menu) return;
    CustomMenu.AddTools(__instance);
    if (HammerMenuCommand.CurrentMode == MenuMode.Builds && HammerMenuCommand.CurrentFilter != "")
    {
      var player = Helper.GetPlayer();
      var item = player.GetRightItem();
      if (!Hammer.IsInfinityHammer(item)) return;
      var back = CustomMenu.BackButton();
      foreach (var tab in __instance.m_availablePieces)
      {
        tab.Insert(0, back);
      }
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
      Console.instance.TryRunCommand(GetInstantCommand(tool, __instance));
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
    var category = pt.GetSelectedCategory();
    var index = pt.GetSelectedIndex();
    var piece = pt.GetPiece(category, index);
    if (!piece)
      Selection.Clear();
    else if (piece.TryGetComponent<BuildMenuTool>(out var menuTool) && menuTool.tool != null)
    {
      var tool = menuTool.tool;
      if (!tool.Instant)
        Console.instance.TryRunCommand($"tool {tool.Name}");
      // Tool command resets the selection to empty so restore it.
      pt.m_selectedPiece[(int)category] = index;
    }
    else if (piece.GetComponent<ZNetView>())
      Selection.CreateGhost(new ObjectSelection(piece, false));
    else
      Selection.Clear();
  }

  private static string GetInstantCommand(Tool tool, Player player)
  {
    var command = tool.GetCommand();
    var position = GetHoveredPosition(player);
    command = command.Replace("<x>", position.x.ToString(CultureInfo.InvariantCulture));
    command = command.Replace("<y>", position.y.ToString(CultureInfo.InvariantCulture));
    command = command.Replace("<z>", position.z.ToString(CultureInfo.InvariantCulture));
    if (command.Contains("<id>"))
    {
      var hovered = Selector.GetHovered(Configuration.Range, [], Configuration.IgnoredIds);
      if (hovered != null)
        command = command.Replace("<id>", Utils.GetPrefabName(hovered.gameObject));
    }
    return command;
  }

  private static Vector3 GetHoveredPosition(Player player)
  {
    var range = Configuration.Range > 0f ? Configuration.Range : 50f;
    var ray = new Ray(GameCamera.instance.transform.position, GameCamera.instance.transform.forward);
    if (Physics.Raycast(ray, out var hit, range, player.m_placeRayMask))
      return hit.point;
    else
      return player.transform.position;
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