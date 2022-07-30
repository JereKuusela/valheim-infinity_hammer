using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

public class BuildMenuCommand : Piece {
  public string Command = "";
}
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
public static class UpdateAvailable {
  private static Piece Build(string command) {
    CommandParameters pars = new(command.Split(' '), true);
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuCommand>();
    piece.Command = command;
    piece.m_description = pars.Description;
    piece.m_name = pars.Name;
    piece.m_icon = pars.Icon;
    return piece;
  }
  static void Postfix(PieceTable __instance) {
    if (!Configuration.IsCheats) return;
    List<string>? commands = null;
    int tab = 0;
    int index = 0;
    if (Hammer.HasTool(Helper.GetPlayer(), Tool.Hammer)) {
      commands = Configuration.HammerCommands;
      tab = Configuration.HammerMenuTab;
      index = Configuration.HammerMenuIndex;
    }
    if (Hammer.HasTool(Helper.GetPlayer(), Tool.Hoe)) {
      commands = Configuration.HoeCommands;
      tab = Configuration.HoeMenuTab;
      index = Configuration.HoeMenuIndex;
    }
    if (commands == null) return;
    if (__instance.m_availablePieces.Count <= tab) return;
    var pieces = __instance.m_availablePieces[tab];
    index = Math.Min(index, pieces.Count);
    foreach (var command in commands.Reverse<string>())
      pieces.Insert(index, Build(command));
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece))]
public class RunBuildMenuCommands {
  public static void Postfix(Player __instance) {
    var piece = __instance.GetSelectedPiece();
    if (piece && piece.GetComponent<BuildMenuCommand>() is { } cmd) Console.instance.TryRunCommand(cmd.Command);
  }
}

