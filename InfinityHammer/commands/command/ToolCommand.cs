using System.Linq;
using UnityEngine;
namespace InfinityHammer;

public class ToolCommand {
  public ToolCommand(string name, Tool tool) {
    Helper.Command(name, "[command] - Executes command at the targeted position.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) => null);
    CommandWrapper.AddSpecialCommand(name);
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Tool tool) {
    Helper.ArgsCheck(args, 2, "Missing the command.");
    Hammer.Equip(tool);
    CommandParameters pars = new CommandParameters(args.Args.Skip(1).ToArray());
    Selection.Set(pars.Name, pars.Description, pars.Command, pars.Icon);
    GizmoWrapper.SetRotation(Quaternion.identity);
    Ruler.Create(pars.ToRuler());
    Helper.AddMessage(args.Context, $"Selected command {pars.Name}.");
  }
}
public class HammerCommand : ToolCommand {
  public static string Name = "hammer_command";
  public static Tool Tool = Tool.Hammer;
  public HammerCommand() : base(Name, Tool) {
  }
}
public class HoeCommand : ToolCommand {
  public static string Name = "hoe_command";
  public static Tool Tool = Tool.Hoe;
  public HoeCommand() : base(Name, Tool) {
  }
}
