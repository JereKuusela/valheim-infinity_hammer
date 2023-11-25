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
    Selection.Set(tool);
    GizmoWrapper.Set(Quaternion.identity);
    Helper.AddMessage(args.Context, $"Selected tool {tool.Name}.");
  }
}