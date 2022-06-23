using System.Linq;
namespace InfinityHammer;

public class ToolAdd {
  public ToolAdd(string name, Tool tool) {
    Helper.Command(name, "[command] - Adds command to the menu.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) => null);
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Tool tool) {
    Helper.ArgsCheck(args, 2, "Missing the command.");
    var command = string.Join(" ", args.Args.Skip(1));
    if (tool == Tool.Hammer) Configuration.AddHammerCommand(command);
    if (tool == Tool.Hoe) Configuration.AddHoeCommand(command);
    Helper.AddMessage(args.Context, $"Added command {command} to {tool.ToString()}.");
  }
}
public class HammerAddCommand : ToolAdd {
  public static string Name = "hammer_add";
  public static Tool Tool = Tool.Hammer;
  public HammerAddCommand() : base(Name, Tool) {
  }
}
public class HoeAddCommand : ToolAdd {
  public static string Name = "hoe_add";
  public static Tool Tool = Tool.Hoe;
  public HoeAddCommand() : base(Name, Tool) {
  }
}
