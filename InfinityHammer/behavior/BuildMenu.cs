using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

public class BuildMenuCommand : Piece {
  public string Command = "";
}
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
public static class UpdateAvailable {
  static void Postfix(PieceTable __instance) {
    if (!Hammer.HasTool(Helper.GetPlayer())) return;
    var pieces = __instance.m_availablePieces.FirstOrDefault();
    if (pieces == null) return;
    var sprite = pieces.FirstOrDefault()?.m_icon;
    foreach (var cmd in Settings.Commands) {
      GameObject obj = new();
      var piece = obj.AddComponent<BuildMenuCommand>();
      piece.Command = cmd;
      piece.m_description = cmd;
      piece.m_name = "Command";
      piece.m_icon = sprite;
      var args = cmd.Split(' ');
      foreach (var arg in args) {
        var split = arg.Split('=');
        var name = split[0].ToLower();
        if (split.Length < 2) continue;
        if (name == "cmd_name") piece.m_name = split[1].Replace("_", " ");
        if (name == "cmd_desc") piece.m_description = split[1].Replace("_", " ");
        if (name == "cmd_icon") piece.m_description = split[1].Replace("_", " ");
      }
      pieces.Insert(1, piece);
    }
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece))]
public class RunBuildMenuCommands {
  public static void Postfix(Player __instance) {
    var piece = __instance.GetSelectedPiece();
    if (piece.GetComponent<BuildMenuCommand>() is { } cmd) Console.instance.TryRunCommand(cmd.Command);
  }
}

