using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

public class BuildMenuCommand : Piece
{
  public string Command = "";
}
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
public static class UpdateAvailable
{
  private static Piece Build(string command)
  {
    CommandParameters pars = new(command, Configuration.AlwaysShowCommand);
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuCommand>();
    piece.Command = command;
    piece.m_description = pars.Description;
    piece.m_name = pars.Name;
    piece.m_icon = pars.Icon;
    return piece;
  }
  static void Postfix(PieceTable __instance)
  {
    if (!Configuration.IsCheats) return;
    List<string>? commands = null;
    int tab = 0;
    int index = 0;
    if (Hammer.HasTool(Helper.GetPlayer(), Tool.Hammer))
    {
      commands = CommandManager.HammerCommands;
      tab = Configuration.HammerMenuTab;
      index = Configuration.HammerMenuIndex;
    }
    if (Hammer.HasTool(Helper.GetPlayer(), Tool.Hoe))
    {
      commands = CommandManager.HoeCommands;
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
public class RunBuildMenuCommands
{
  public static bool InstantCommand = false;

  [HarmonyPriority(Priority.Low)]
  public static bool Prefix(Player __instance, Vector2Int p)
  {
    var piece = __instance.GetPiece(p);
    if (piece && piece.GetComponent<BuildMenuCommand>() is { } cmd)
    {
      InstantCommand = false;
      Console.instance.TryRunCommand(cmd.Command);
      if (!InstantCommand)
      {
        __instance.m_buildPieces.SetSelected(p);
      }
      return false;
    }
    return true;
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
    text = text.Replace(CommandParameters.CmdMod1, Configuration.ModifierKey1());
    text = text.Replace(CommandParameters.CmdMod2, Configuration.ModifierKey2());
  }
}
[HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
public class RemoveCmdParameters
{

  [HarmonyPriority(Priority.Low)]
  static void Prefix(ref string text)
  {
    if (text.StartsWith("hammer_command ", StringComparison.OrdinalIgnoreCase)) return;
    if (text.StartsWith("hoe_command ", StringComparison.OrdinalIgnoreCase)) return;
    if (text.StartsWith("hammer_add ", StringComparison.OrdinalIgnoreCase)) return;
    if (text.StartsWith("hoe_add ", StringComparison.OrdinalIgnoreCase)) return;
    text = CommandParameters.RemoveCmdParameters(text);
    RunBuildMenuCommands.InstantCommand = true;
  }
}