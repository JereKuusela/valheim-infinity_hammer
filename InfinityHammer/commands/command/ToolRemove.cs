using Service;
namespace InfinityHammer;

public class ToolRemove {
  public ToolRemove(string name, Tool tool) {
    Helper.Command(name, "[index or start of the command] - Removes commands from the menu.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index) => {
      if (index == 0) return CommandWrapper.Info("Index or start of the command.");
      return null;
    });
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Tool tool) {
    Helper.ArgsCheck(args, 2, "Missing the index or start of the command.");
    var index = Parse.TryInt(args.Args, 1, -1);
    if (index > -1) {
      var cmd = CommandManager.Get(tool, index);
      CommandManager.Remove(tool, index);
      Helper.AddMessage(args.Context, $"Removed command {cmd} from {tool}.");
    } else {
      var command = string.Join(" ", args.Args, 1, args.Length - 1);
      var removed = CommandManager.Remove(tool, command);
      Helper.AddMessage(args.Context, $"Removed {removed} commands from {tool}.");
    }
  }
}
public class HammerRemoveCommand : ToolRemove {
  public static string Name = "hammer_remove";
  public static Tool Tool = Tool.Hammer;
  public HammerRemoveCommand() : base(Name, Tool) {
  }
}
public class HoeRemoveCommand : ToolRemove {
  public static string Name = "hoe_remove";
  public static Tool Tool = Tool.Hoe;
  public HoeRemoveCommand() : base(Name, Tool) {
  }
}
