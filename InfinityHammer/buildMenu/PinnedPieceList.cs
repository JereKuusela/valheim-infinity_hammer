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
  internal static int PinIndex(Piece piece)
  {
    if (!piece || !piece.TryGetComponent<BuildMenuTool>(out var button)) return -1;
    return button.tool?.Name switch { "Pipette" => 0, "Building pipette" => 1, "Area pipette" => 2, _ => -1 };
  }
  public void GetAvailablePiecesWithTag(int tag, PieceTable buildTool, IList<Piece> pieces)
  {
    original.GetAvailablePiecesWithTag(tag, buildTool, pieces);
    Piece? first = null, second = null, third = null;
    // Availability already enforces equipment and command permissions.
    foreach (var piece in buildTool.m_availablePieces)
      switch (PinIndex(piece))
      {
        case 0: first = piece; break;
        case 1: second = piece; break;
        case 2: third = piece; break;
      }
    for (var i = pieces.Count - 1; i >= 0; i--)
      if (PinIndex(pieces[i]) >= 0) pieces.RemoveAt(i);
    if (third) pieces.Insert(0, third!);
    if (second) pieces.Insert(0, second!);
    if (first) pieces.Insert(0, first!);
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
      if (!button || PinnedPieceList.PinIndex(button.Piece) < 0 || !table.m_availablePieces.Contains(button.Piece)) continue;
      if (button.gameObject.activeSelf) continue;
      button.gameObject.SetActive(true);
      changed = true;
    }
    if (changed) __instance.ConfigureButtonNavigation();
  }
}
