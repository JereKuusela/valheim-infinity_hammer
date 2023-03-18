namespace InfinityHammer;

public class ToolImport
{
  public ToolImport(string name, Equipment tool)
  {
    Helper.Command(name, "[command] - Imports a new tool.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) => null);
    CommandWrapper.AddCompositeCommand(name);
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Equipment equipment)
  {
    Helper.ArgsCheck(args, 2, "Missing the tool.");
    var tool = string.Join(" ", args.Args, 1, args.Length - 1);
    ToolManager.Import(equipment, tool);
    Helper.AddMessage(args.Context, $"Imported tool {tool} to {equipment.ToString()}.");
  }
}
public class HammerImportCommand : ToolImport
{
  public static string Name = "hammer_import";
  public static Equipment Tool = Equipment.Hammer;
  public HammerImportCommand() : base(Name, Tool)
  {
  }
}
public class HoeImportCommand : ToolImport
{
  public static string Name = "hoe_import";
  public static Equipment Tool = Equipment.Hoe;
  public HoeImportCommand() : base(Name, Tool)
  {
  }
}
