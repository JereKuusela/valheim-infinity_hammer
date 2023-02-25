using UnityEngine;
namespace InfinityHammer;

public class ToolCommand
{
  public ToolCommand(string name, Equipment tool)
  {
    Helper.Command(name, "[command] - Executes the command at the targeted position.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) => null);
    CommandWrapper.AddCompositeCommand(name);
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Equipment tool)
  {
    Helper.ArgsCheck(args, 2, "Missing the command.");
    Hammer.Equip(tool);
    var command = string.Join(" ", args.Args);
    command = command.Replace("hammer_command ", "").Replace("hoe_command ", "");
    CommandParameters pars = new CommandParameters(command, false);
    Selection.Set(pars.ToRuler(), pars.Name, pars.Description, pars.Command, pars.Continuous, pars.Icon);
    AddExtraInfo.ShowId = pars.IsId;
    GizmoWrapper.SetRotation(Quaternion.identity);
    Helper.AddMessage(args.Context, $"Selected command {pars.Name}.");
  }
}
public class HammerCommand : ToolCommand
{
  public static string Name = "hammer_command";
  public static Equipment Tool = Equipment.Hammer;
  public HammerCommand() : base(Name, Tool)
  {
  }
}
public class HoeCommand : ToolCommand
{
  public static string Name = "hoe_command";
  public static Equipment Tool = Equipment.Hoe;
  public HoeCommand() : base(Name, Tool)
  {
  }
}
