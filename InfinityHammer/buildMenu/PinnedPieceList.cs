using System.Collections.Generic;
using HarmonyLib;
namespace InfinityHammer;

// Native lists follow the available set order, so tools are moved to their configured slots.
// Infinity Hammer pages are already ordered by HammerPieceList.
internal sealed class PinnedPieceList(IPieceList original) : IPieceList
{
  private readonly ByMaterialPieceList? materials = original as ByMaterialPieceList;
  public string DisplayName => original.DisplayName;
  public bool ShowTags => original.ShowTags;
  public bool CanCustomizeTags => original.CanCustomizeTags;
  public int TagCount => original.TagCount;
  public int TagSeparatorIndex => original.TagSeparatorIndex;
  public string GetTagDisplayName(int index) => original.GetTagDisplayName(index);
  public int GetTagIdByIndex(int index) => original.GetTagIdByIndex(index);
  public void UpdateAvailableTags(PieceTable buildTool) => original.UpdateAvailableTags(buildTool);
  public void GetAvailablePiecesWithTag(int tag, PieceTable buildTool, IList<Piece> pieces)
  {
    original.GetAvailablePiecesWithTag(tag, buildTool, pieces);
    if (Hammer.IsInfinityHammer(buildTool)) return;
    var equipment = CustomMenu.PieceTableToEquipment(buildTool);
    var tools = ToolManager.Get(equipment);
    MoveMenuButtons(tools, buildTool, pieces);
    if (tools.Count == 0) return;
    if (tag != -1) AddTaggedTools(tag, buildTool, pieces);
    List<(int index, BuildMenuTool piece)> pinned = [];
    for (var i = pieces.Count - 1; i >= 0; i--)
    {
      if (pieces[i] is not BuildMenuTool button || button.tool == null) continue;
      var index = tools.IndexOf(button.tool);
      if (index < 0) continue;
      pieces.RemoveAt(i);
      if (tag == -1 && !button.tool.ShowInAll) continue;
      pinned.Add((index, button));
    }
    if (pinned.Count == 0) return;
    pinned.Sort((a, b) => a.index.CompareTo(b.index));
    // Same slot logic as AddTools: explicit index, otherwise right after the previous tool.
    var previous = equipment == "hammer" ? 0 : pieces.Count - 1;
    foreach (var (_, piece) in pinned)
    {
      var slot = piece.tool!.GetSlot(pieces, previous);
      if (slot < 0) continue;
      pieces.Insert(slot, piece);
      previous = slot;
    }
  }
  // Navigation buttons (like back when browsing other build tools) are first in every tag.
  private static void MoveMenuButtons(List<Tool> tools, PieceTable buildTool, IList<Piece> pieces)
  {
    for (var i = pieces.Count - 1; i >= 0; i--)
    {
      if (pieces[i] is BuildMenuTool { tool: { } tool } && !tools.Contains(tool))
        pieces.RemoveAt(i);
    }
    foreach (var piece in buildTool.m_availablePieces)
    {
      if (piece is BuildMenuTool { tool: { } tool } && !tools.Contains(tool))
        pieces.Insert(0, piece);
    }
  }
  // Native filters never match tools. Without own tags, tools placed next to a piece follow it (hidden later if missing).
  private void AddTaggedTools(int tag, PieceTable buildTool, IList<Piece> pieces)
  {
    string? material = null;
    if (materials != null)
    {
      var requirement = materials.GetTagById(tag);
      if (requirement != null && requirement.m_resItem) material = requirement.m_resItem.gameObject.name.ToLowerInvariant();
    }
    foreach (var piece in buildTool.m_availablePieces)
    {
      if (piece is not BuildMenuTool { tool: { } tool } || pieces.Contains(piece)) continue;
      var add = materials != null
        ? tool.Materials == null ? tool.FollowsPiece : material != null && tool.Materials.Contains(material)
        : tool.Usage == 0 && tool.FollowsPiece;
      if (add) pieces.Add(piece);
    }
  }
}

[HarmonyPatch(typeof(BuildUi), nameof(BuildUi.UpdateSearch))]
internal static class SearchToolVisibility
{
  private static void Postfix(BuildUi __instance)
  {
    var table = Player.m_localPlayer ? Player.m_localPlayer.m_buildPieces : null;
    if (!table) return;
    var pipettes = ToolManager.Get("hammer");
    var changed = false;
    foreach (var button in __instance.m_pieceButtons)
    {
      if (!button || button.Piece is not BuildMenuTool { tool: { } tool }) continue;
      var visible = button.gameObject.activeSelf;
      if (pipettes.Contains(tool) && table.m_availablePieces.Contains(button.Piece)) visible = true;
      if (tool.FollowsPiece && !IsTargetVisible(__instance, tool)) visible = false;
      if (visible == button.gameObject.activeSelf) continue;
      button.gameObject.SetActive(visible);
      changed = true;
    }
    if (changed) __instance.ConfigureButtonNavigation();
  }
  private static bool IsTargetVisible(BuildUi ui, Tool tool)
  {
    foreach (var button in ui.m_pieceButtons)
    {
      if (button && button.Piece && Tool.IsNamed(button.Piece, tool.TargetPiece))
        return button.gameObject.activeSelf;
    }
    return false;
  }
}
