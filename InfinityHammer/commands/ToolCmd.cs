using InfinityHammer;
using ServerDevcommands;
using UnityEngine;
namespace InfinityTools;

public class ToolCmdCommand
{
  public ToolCmdCommand()
  {
    Helper.Command("tool_cmd", "[command] - Selects a command to be used when placed.", Execute);
    AutoComplete.RegisterEmpty("tool_cmd");
    AutoComplete.Offsets["tool_cmd"] = 0;
  }
  protected static void Execute(Terminal.ConsoleEventArgs args)
  {
    Helper.ArgsCheck(args, 2, "Missing the command.");
    if (!Hammer.HasAny())
      Hammer.Equip();
    var toolName = string.Join(" ", args.Args, 1, args.Length - 1);
    Tool tool = new(new()
    {
      name = "Command",
      command = toolName,
      description = toolName,
    });
    InfinityHammer.Hammer.Clear();
    Selection.CreateGhost(new ToolSelection(tool));
    PlaceRotation.Set(Quaternion.identity);
    HammerHelper.Message(args.Context, $"Selected command {tool.Name}.");
  }
}