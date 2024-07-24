using System.Linq;
using InfinityHammer;
using ServerDevcommands;
using UnityEngine;
namespace InfinityTools;

public class ToolCommand
{
  public ToolCommand()
  {
    Helper.Command("tool", "[tool] - Selects a tool to be used when placed.", Execute);
    AutoComplete.Register("tool", (int index, int subIndex) => ToolManager.GetAll().Select(tool => tool.Name).ToList());
  }
  protected static void Execute(Terminal.ConsoleEventArgs args)
  {
    Helper.ArgsCheck(args, 2, "Missing the tool name.");
    if (!Hammer.HasAny())
      Hammer.Equip();
    var toolName = string.Join(" ", args.Args, 1, args.Length - 1);
    if (!ToolManager.TryGetTool(Hammer.Get(), toolName, out var tool))
    {
      ToolData data = new()
      {
        name = "Command",
        command = toolName,
        description = toolName,
      };
      tool = new(data);
    }
    InfinityHammer.Hammer.Clear();
    Selection.CreateGhost(new ToolSelection(tool));
    PlaceRotation.Set(Quaternion.identity);
    HammerHelper.Message(args.Context, $"Selected tool {tool.Name}.");
  }
}