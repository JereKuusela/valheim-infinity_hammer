using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;

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
    var toolName = HammerHelper.GetArgs("tool", args);
    if (ToolManager.TryGetToolData(toolName, out _, out var equipment))
      Hammer.EquipByTool(equipment);
    else if (!Hammer.HasAny())
      Hammer.Equip();
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
    Hammer.SelectRepair();
    Selection.CreateGhost(new ToolSelection(tool));
    PlaceRotation.Set(Quaternion.identity);
    HammerHelper.Message(args.Context, $"Selected tool {tool.Name}.");
  }
}