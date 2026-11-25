using System.Collections.Generic;
using HarmonyLib;
namespace InfinityHammer;

// A view decoration only: never add synthetic tools to saved favorites/history.
internal sealed class PinnedPieceList(IPieceList original) : IPieceList
{
  public string DisplayName => original.DisplayName;
  public bool ShowTags => original.ShowTags;
  public bool CanCustomizeTags => original.CanCustomizeTags;
  public int TagCount => original.TagCount;
  public int TagSeparatorIndex => original.TagSeparatorIndex;
  public string GetTagDisplayName(int index) => original.GetTagDisplayName(index);
  public int GetTagIdByIndex(int index) => original.GetTagIdByIndex(index);
  public void UpdateAvailableTags(PieceTable buildTool) => original.UpdateAvailableTags(buildTool);
  // Returns the hammer tool index, or -1. Only Infinity Hammer tables pin; other tables place tools via AddTools.
  internal static int PinIndex(Piece piece, PieceTable table)
  {
    if (!piece || !Hammer.IsInfinityHammer(table)) return -1;
    if (!piece.TryGetComponent<BuildMenuTool>(out var button) || button.tool == null) return -1;
    return ToolManager.Get("hammer").IndexOf(button.tool);
  }
  public void GetAvailablePiecesWithTag(int tag, PieceTable buildTool, IList<Piece> pieces)
  {
    original.GetAvailablePiecesWithTag(tag, buildTool, pieces);
    // Availability already enforces equipment and command permissions.
    List<(int index, Piece piece)> pinned = [];
    var defaultTag = buildTool.m_categories.Count > 0 ? (int)buildTool.m_categories[0] : 0;
    foreach (var piece in buildTool.m_availablePieces)
    {
      var index = PinIndex(piece, buildTool);
      if (index >= 0 && piece.GetComponent<BuildMenuTool>().tool!.IsVisibleIn(tag, defaultTag)) pinned.Add((index, piece));
    }
    for (var i = pieces.Count - 1; i >= 0; i--)
      if (PinIndex(pieces[i], buildTool) >= 0) pieces.RemoveAt(i);
    pinned.Sort((a, b) => a.index.CompareTo(b.index));
    // Same slot logic as AddTools: explicit index, otherwise right after the previous tool.
    var previous = 0;
    foreach (var (_, piece) in pinned)
    {
      var slot = System.Math.Max(0, System.Math.Min(piece.GetComponent<BuildMenuTool>().tool!.Index ?? previous + 1, pieces.Count));
      pieces.Insert(slot, piece);
      previous = slot;
    }
  }
}

[HarmonyPatch(typeof(BuildUi), nameof(BuildUi.UpdateSearch))]
internal static class KeepPipettesVisible
{
  private static void Postfix(BuildUi __instance)
  {
    var table = Player.m_localPlayer ? Player.m_localPlayer.m_buildPieces : null;
    if (!table) return;
    var changed = false;
    foreach (var button in __instance.m_pieceButtons)
    {
      if (!button || PinnedPieceList.PinIndex(button.Piece, table) < 0 || button.Piece.GetComponent<BuildMenuTool>().tool!.Categories != null || !table.m_availablePieces.Contains(button.Piece)) continue;
      if (button.gameObject.activeSelf) continue;
      button.gameObject.SetActive(true);
      changed = true;
    }
    if (changed) __instance.ConfigureButtonNavigation();
  }
}
