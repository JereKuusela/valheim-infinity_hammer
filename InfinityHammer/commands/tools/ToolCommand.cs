using UnityEngine;
namespace InfinityHammer;

public class ToolCommand
{
  public ToolCommand(string name, Equipment tool)
  {
    Helper.Command(name, "[tool] - Executes the tool at the targeted position.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) => null);
    CommandWrapper.AddCompositeCommand(name);
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Equipment equipment)
  {
    Helper.ArgsCheck(args, 2, "Missing the tool name.");
    Hammer.Equip(equipment);
    var toolName = string.Join(" ", args.Args);
    if (!ToolManager.TryGetTool(equipment, toolName, out var tool))
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
public class HammerTool : ToolCommand
{
  public static string Name = "hammer_tool";
  public static Equipment Tool = Equipment.Hammer;
  public HammerTool() : base(Name, Tool)
  {
  }
}
public class HoeTool : ToolCommand
{
  public static string Name = "hoe_tool";
  public static Equipment Tool = Equipment.Hoe;
  public HoeTool() : base(Name, Tool)
  {
  }
}
