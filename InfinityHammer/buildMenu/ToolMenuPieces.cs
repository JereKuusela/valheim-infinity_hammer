using System.Collections.Generic;
using InfinityTools;
using UnityEngine;
namespace InfinityHammer;

// BuildUi keeps buttons when availability counts are unchanged. Keep the Piece
// identity stable too; otherwise visible buttons point at discarded entries.
internal static class ToolMenuPieces
{
  private static readonly Dictionary<Tool, BuildMenuTool> pieces = [];
  internal static BuildMenuTool Get(Tool tool)
  {
    if (!pieces.TryGetValue(tool, out var piece) || !piece)
    {
      var obj = new GameObject("_IH_Tool_" + tool.Name);
      obj.SetActive(false);
      piece = obj.AddComponent<BuildMenuTool>();
      piece.tool = tool;
      pieces[tool] = piece;
    }
    piece.m_description = tool.Description;
    piece.m_name = tool.Name;
    piece.m_icon = tool.Icon;
    return piece;
  }
  internal static void Clear()
  {
    foreach (var piece in pieces.Values)
      if (piece) Object.Destroy(piece.gameObject);
    pieces.Clear();
  }
}
