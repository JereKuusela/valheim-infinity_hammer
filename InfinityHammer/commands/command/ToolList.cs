namespace InfinityHammer;

public class ToolList {
  public ToolList(string name, Tool tool) {
    Helper.Command(name, " - Lists commands from the menu.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) => null);
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Tool tool) {
    var commands = Configuration.GetCommands(tool);
    for (var i = 0; i < commands.Count; i++) {
      args.Context.AddString($"<color=yellow>{i}:</color> {commands[i]}");
    }
  }
}
public class HammerListCommand : ToolList {
  public static string Name = "hammer_list";
  public static Tool Tool = Tool.Hammer;
  public HammerListCommand() : base(Name, Tool) {
  }
}
public class HoeListCommand : ToolList {
  public static string Name = "hoe_list";
  public static Tool Tool = Tool.Hoe;
  public HoeListCommand() : base(Name, Tool) {
  }
}
