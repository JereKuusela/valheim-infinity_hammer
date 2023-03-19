using UnityEngine;
namespace InfinityHammer;

public class HammerToolCommand
{
  public HammerToolCommand()
  {
    Helper.Command("hammer_tool", "[tool] - Executes the tool at the targeted position.", Execute);
    CommandWrapper.Register("hammer_tool", (int index, int subIndex) => null);
    CommandWrapper.AddCompositeCommand("hammer_tool");
  }
  protected static void Execute(Terminal.ConsoleEventArgs args)
  {
    Helper.ArgsCheck(args, 2, "Missing the tool name.");
    if (!Hammer.HasAnyTool())
      Hammer.Equip();
    var toolName = string.Join(" ", args.Args, 1, args.Length - 1);
    if (!ToolManager.TryGetTool(Hammer.GetTool(), toolName, out var tool))
    {
      tool = new();
      tool.name = "Command";
      tool.command = toolName;
      tool.description = tool.command;
    }
    Selection.Set(tool);
    GizmoWrapper.SetRotation(Quaternion.identity);
    Helper.AddMessage(args.Context, $"Selected command {tool.name}.");
  }
}