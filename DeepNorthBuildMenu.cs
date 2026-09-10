using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

// Keep IH's category ordering in Deep North's first native tab. Other tools use vanilla tags.
internal sealed class HammerPieceList(IPieceList original) : IPieceList
{
  private PieceTable? table;
  private bool Custom => table && Hammer.IsInfinityHammer(table);
  public string DisplayName => original.DisplayName;
  public bool ShowTags => Custom || original.ShowTags;
  public bool CanCustomizeTags => !Custom && original.CanCustomizeTags;
  public int TagCount => Custom ? table!.m_categories.Count : original.TagCount;
  public int TagSeparatorIndex => Custom ? -1 : original.TagSeparatorIndex;
  public string GetTagDisplayName(int index) => Custom ? table!.m_categoryLabels[index] : original.GetTagDisplayName(index);
  public int GetTagIdByIndex(int index) => Custom ? (int)table!.m_categories[index] : original.GetTagIdByIndex(index);
  public void UpdateAvailableTags(PieceTable buildTool)
  {
    table = buildTool;
    if (!Custom) original.UpdateAvailableTags(buildTool);
  }
  public void GetAvailablePiecesWithTag(int tag, PieceTable buildTool, IList<Piece> pieces)
  {
    if (!Hammer.IsInfinityHammer(buildTool))
    {
      original.GetAvailablePiecesWithTag(tag, buildTool, pieces);
      return;
    }
    var added = new HashSet<Piece>();
    foreach (var category in buildTool.m_categories)
    {
      if (tag != -1 && tag != (int)category) continue;
      foreach (var piece in buildTool.m_availablePiecesByCategory[(int)category])
        if (piece && added.Add(piece)) pieces.Add(piece);
    }
  }
}

[HarmonyPatch]
public static class DeepNorthBuildMenu
{
  [HarmonyPatch(typeof(BuildUi), nameof(BuildUi.Awake)), HarmonyPostfix]
  private static void InstallPieceList(BuildUi __instance)
  {
    for (var i = 0; i < __instance.m_pieceLists.Count; i++)
    {
      if (__instance.m_pieceLists[i] is PinnedPieceList) continue;
      var original = __instance.m_pieceLists[i];
      if (i == 0) original = new HammerPieceList(original);
      __instance.m_pieceLists[i] = new PinnedPieceList(original);
    }
  }

  // Synthetic tool buttons are commands, not placeable prefab names. In particular,
  // navigation must not immediately close the new page in native OnSelectPiece.
  [HarmonyPatch(typeof(BuildUi), nameof(BuildUi.OnSelectPiece)), HarmonyPrefix]
  private static bool SelectTool(BuildUi __instance, Piece piece)
  {
    var player = Player.m_localPlayer;
    if (!player || !player.m_buildPieces || !piece || !piece.TryGetComponent<BuildMenuTool>(out var menuTool) || menuTool.tool == null)
      return true;
    var pt = player.m_buildPieces;
    if (!TryGetIndex(pt, piece, out var index, out var category)) return false;
    var tool = menuTool.tool;
    var revision = Hammer.MenuRevision;
    var selection = Selection.Get();
    player.m_placePressedTime = -9998f;
    pt.m_selectedCategory = category;
    Console.instance.TryRunCommand(tool.Instant ? TakeOverBuildMenu.GetInstantCommand(tool, player) : $"tool {tool.Name}");
    if (revision != Hammer.MenuRevision) return false;
    if (!tool.Instant || selection != Selection.Get())
      pt.m_selectedPiece[(int)category] = index;
    if (!ZInput.IsTouchActive() || ZInput.HasDoubleTapped())
    {
      player.PlayButtonSound();
      Hud.CloseBuildUi();
    }
    return false;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Piece)), HarmonyPrefix]
  private static void ClearCommandSelection(Player __instance, Piece __0)
  {
    var piece = __0;
    if (__instance != Player.m_localPlayer || !piece || piece.GetComponent<BuildMenuTool>() || !__instance.m_buildPieces) return;
    if (!TryGetIndex(__instance.m_buildPieces, piece, out var index, out var category)) return;
    if (Selection.Get().GetSelectedPiece() == null) return;
    Selection.Clear();
    // The native method skips SetupPlacementGhost when the slot is unchanged.
    __instance.m_buildPieces.m_selectedCategory = category;
    __instance.m_buildPieces.m_selectedPiece[(int)category] = index;
    __instance.SetupPlacementGhost();
  }

  // Native lookup compares names and returns Piece.m_category. IH can put the same
  // repair/back piece in several tabs; use the actual containing tab and reference.
  internal static bool TryGetIndex(PieceTable table, Piece piece, out Vector2Int index, out Piece.PieceCategory category)
  {
    foreach (var tab in table.m_categories)
    {
      if ((int)tab < 0 || (int)tab >= table.m_availablePiecesByCategory.Count) continue;
      var i = table.m_availablePiecesByCategory[(int)tab].IndexOf(piece);
      if (i < 0) continue;
      index = new(i % PieceTable.m_gridWidth, i / PieceTable.m_gridWidth);
      category = tab;
      return true;
    }
    // Hoe/cultivator tables can have no declared tabs. Availability still has
    // category lists, and native BuildUi displays those pieces from the set.
    for (var tab = 0; tab < table.m_availablePiecesByCategory.Count; tab++)
    {
      if (table.m_categories.Contains((Piece.PieceCategory)tab)) continue;
      var i = table.m_availablePiecesByCategory[tab].IndexOf(piece);
      if (i < 0) continue;
      index = new(i % PieceTable.m_gridWidth, i / PieceTable.m_gridWidth);
      category = (Piece.PieceCategory)tab;
      return true;
    }
    index = Vector2Int.zero;
    category = 0;
    return false;
  }

  [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetPieceIndex)), HarmonyPrefix]
  private static bool FindSyntheticPiece(PieceTable __instance, Piece __0, ref Vector2Int index, ref Piece.PieceCategory category, ref bool __result)
  {
    if (!__0 || (!Hammer.IsInfinityHammer(__instance) && !__0.GetComponent<BuildMenuTool>())) return true;
    __result = TryGetIndex(__instance, __0, out index, out category);
    return false;
  }
}
