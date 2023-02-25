namespace InfinityHammer;

public class ToolAdd
{
  public ToolAdd(string name, Equipment tool)
  {
    Helper.Command(name, "[command] - Adds command to the menu.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) => null);
    CommandWrapper.AddCompositeCommand(name);
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Equipment tool)
  {
    Helper.ArgsCheck(args, 2, "Missing the command.");
    var command = string.Join(" ", args.Args, 1, args.Length - 1);
    CommandManager.Add(tool, command);
    Helper.AddMessage(args.Context, $"Added command {command} to {tool.ToString()}.");
  }
}
public class HammerAddCommand : ToolAdd
{
  public static string Name = "hammer_add";
  public static Equipment Tool = Equipment.Hammer;
  public HammerAddCommand() : base(Name, Tool)
  {
  }
}
public class HoeAddCommand : ToolAdd
{
  public static string Name = "hoe_add";
  public static Equipment Tool = Equipment.Hoe;
  public HoeAddCommand() : base(Name, Tool)
  {
  }
}
