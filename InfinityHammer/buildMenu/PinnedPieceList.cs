using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
namespace InfinityHammer;

// Native lists follow the available set order, so tools are moved to their configured slots.
// Infinity Hammer pages are already ordered by HammerPieceList.
internal sealed class PinnedPieceList(IPieceList original, bool reorder) : IPieceList
{
  private readonly ByMaterialPieceList? materials = original as ByMaterialPieceList;
  // Tool categories/materials that no piece uses, shown before the native tags.
  private readonly List<(int id, string name, string display)> extraTags = [];
  public string DisplayName => original.DisplayName;
  public bool ShowTags => original.ShowTags;
  public bool CanCustomizeTags => original.CanCustomizeTags;
  public int TagCount => extraTags.Count + original.TagCount;
  public int TagSeparatorIndex => original.TagSeparatorIndex < 0 ? -1 : original.TagSeparatorIndex + extraTags.Count;
  public string GetTagDisplayName(int index) => index < extraTags.Count ? extraTags[index].display : original.GetTagDisplayName(index - extraTags.Count);
  public int GetTagIdByIndex(int index) => index < extraTags.Count ? extraTags[index].id : original.GetTagIdByIndex(index - extraTags.Count);
  public void UpdateAvailableTags(PieceTable buildTool)
  {
    original.UpdateAvailableTags(buildTool);
    extraTags.Clear();
    if (Hammer.IsInfinityHammer(buildTool) || !reorder) return;
    HashSet<int> existing = [.. Enumerable.Range(0, original.TagCount).Select(original.GetTagIdByIndex)];
    foreach (var piece in buildTool.m_availablePieces)
    {
      if (piece is not BuildMenuTool { tool: { } tool }) continue;
      var names = materials != null ? tool.Materials : tool.Categories;
      if (names == null) continue;
      foreach (var name in names)
      {
        var (key, display) = materials != null ? ResolveMaterial(name) : (name.ToLowerInvariant(), name);
        var id = key.GetStableHashCode();
        if (existing.Add(id)) extraTags.Add((id, key, display));
      }
    }
  }
  private static (string name, string display) ResolveMaterial(string name)
  {
    var item = ObjectDB.instance ? ObjectDB.instance.m_items.FirstOrDefault(i => i && i.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) : null;
    if (item && item!.TryGetComponent<ItemDrop>(out var drop)) return (item.name, drop.m_itemData.m_shared.m_name);
    return (name, name);
  }
  private string? GetExtraTag(int tag)
  {
    foreach (var extra in extraTags)
      if (extra.id == tag) return extra.name;
    return null;
  }
  public void GetAvailablePiecesWithTag(int tag, PieceTable buildTool, IList<Piece> pieces)
  {
    if (reorder) Fill(tag, buildTool, pieces);
    else original.GetAvailablePiecesWithTag(tag, buildTool, pieces);
    CustomMenu.EnsureIcons(pieces);
  }
  private void Fill(int tag, PieceTable buildTool, IList<Piece> pieces)
  {
    var extra = GetExtraTag(tag);
    // Native lists would fail to resolve the unknown tag.
    if (extra == null)
      original.GetAvailablePiecesWithTag(tag, buildTool, pieces);
    else
    {
      foreach (var piece in buildTool.m_availablePieces)
        if (piece.m_repairPiece || piece.m_removePiece) pieces.Add(piece);
    }
    if (Hammer.IsInfinityHammer(buildTool)) return;
    var equipment = CustomMenu.PieceTableToEquipment(buildTool);
    var tools = ToolManager.Get(equipment);
    MoveMenuButtons(tools, buildTool, pieces);
    if (tools.Count == 0) return;
    if (tag != -1) AddTaggedTools(tag, extra, buildTool, pieces);
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
  private void AddTaggedTools(int tag, string? extra, PieceTable buildTool, IList<Piece> pieces)
  {
    var material = extra;
    if (material == null && materials != null)
    {
      var requirement = materials.GetTagById(tag);
      if (requirement != null && requirement.m_resItem) material = requirement.m_resItem.gameObject.name;
    }
    foreach (var piece in buildTool.m_availablePieces)
    {
      if (piece is not BuildMenuTool { tool: { } tool } || pieces.Contains(piece)) continue;
      bool add;
      if (materials != null)
        add = tool.Materials == null ? tool.FollowsPiece : material != null && tool.Materials.Contains(material);
      else if (extra != null)
        add = tool.Categories?.Contains(extra) == true;
      else
        add = tool.Usage == 0 && tool.Categories == null && tool.FollowsPiece;
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
