using System;
using System.Globalization;
using HarmonyLib;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;

public class BuildMenuTool : Piece
{
  public Tool? tool;
}
[HarmonyPatch]
public class TakeOverBuildMenu
{
  private static bool Selecting;
  private static bool Activated;

  // Native selection calls SetBuildCategory which rebuilds the unchanged piece table.
  [HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Piece)), HarmonyPrefix]
  static void SelectPiecePrefix()
  {
    Selecting = true;
    Activated = false;
  }
  [HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Piece)), HarmonyPostfix]
  static void SelectPiecePostfix(Player __instance, bool __result)
  {
    Selecting = false;
    // Native skips SetSelected when the slot is unchanged.
    if (__result && !Activated)
    {
      ActivatePiece(__instance.m_buildPieces);
      __instance.SetupPlacementGhost();
    }
  }
  [HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Piece)), HarmonyFinalizer]
  static void SelectPieceFinalizer() => Selecting = false;

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable)), HarmonyPrefix]
  static bool UpdateAvailablePrefix(PieceTable __instance)
  {
    if (Selecting) return false;
    if (Hammer.IsInfinityHammer(__instance))
    {
      CustomBuildMenu.HandleCustomMenuMode(__instance);
      return false;
    }
    return true;
  }
  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable)), HarmonyPostfix, HarmonyPriority(Priority.Low)]
  static void UpdateAvailablePostfix(PieceTable __instance)
  {
    if (Selecting || !Player.m_localPlayer || __instance != Player.m_localPlayer.m_buildPieces) return;
    // Native UI only refreshes when the piece count changes.
    if (Hud.instance) Hud.instance.m_buildUi.m_availablePieceCount = -1;
    if (!Configuration.ToolsEnabled) return;
    CustomMenu.AddTools(__instance);
    if (Hammer.IsInfinityHammer(__instance) && HammerMenuCommand.CurrentMode != MenuMode.Menu) return;
    if (HammerMenuCommand.CurrentMode == MenuMode.Builds && HammerMenuCommand.CurrentFilter != "")
    {
      var player = Helper.GetPlayer();
      var item = player.GetRightItem();
      if (!Hammer.IsInfinityHammer(item)) return;
      var back = CustomMenu.BackButton();
      foreach (var tab in __instance.m_availablePiecesByCategory)
      {
        tab.Insert(0, back);
        __instance.m_availablePieces.Add(back);
        __instance.m_enabledPieces.Add(back);
      }
    }
  }

  [HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Vector2Int))]
  [HarmonyPrefix]
  [HarmonyPriority(Priority.Low)]
  public static bool HandleSetSelectedPiece(Player __instance, Vector2Int p)
  {
    var pt = __instance.m_buildPieces;
    if (__instance != Player.m_localPlayer || !pt) return true;
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

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetSelected)), HarmonyPostfix]
  private static void HandleSetSelected(PieceTable __instance) => ActivatePiece(__instance);

  private static void ActivatePiece(PieceTable pt)
  {
    if (!Player.m_localPlayer || Player.m_localPlayer.m_buildPieces != pt) return;
    Activated = true;
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

  internal static string GetInstantCommand(Tool tool, Player player)
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