using System.Collections.Generic;
using InfinityTools;
using UnityEngine;
namespace InfinityHammer;

internal static class ToolMenuPieces
{
  private class Table
  {
    private readonly Dictionary<Tool, BuildMenuTool> Pieces = [];
    // Build menu keeps references, so can only clear pieces when it is rebuilt.
    private bool Dirty = false;
    public void Invalidate() => Dirty = true;
    public BuildMenuTool Get(Tool tool)
    {
      if (Dirty) Purge();
      if (!Pieces.TryGetValue(tool, out var piece) || !piece)
      {
        var obj = new GameObject("_IH_Tool_" + tool.Name);
        obj.SetActive(false);
        piece = obj.AddComponent<BuildMenuTool>();
        piece.tool = tool;
        Pieces[tool] = piece;
      }
      piece.m_description = tool.Description;
      piece.m_name = tool.Name;
      piece.m_icon = tool.Icon;
      return piece;
    }
    private void Purge()
    {
      foreach (var piece in Pieces.Values)
        if (piece) Object.Destroy(piece.gameObject);
      Pieces.Clear();
      Dirty = false;
    }
  }
  // Each build table owns its tool pieces so that they can't affect each other.
  private static readonly Dictionary<PieceTable, Table> Tables = [];
  internal static BuildMenuTool Get(PieceTable pt, Tool tool)
  {
    if (!Tables.TryGetValue(pt, out var table))
      Tables[pt] = table = new Table();
    return table.Get(tool);
  }
  public static void Invalidate()
  {
    foreach (var table in Tables.Values)
      table.Invalidate();
  }
}
