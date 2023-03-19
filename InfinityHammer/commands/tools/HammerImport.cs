namespace InfinityHammer;

public class HammerImportCommand
{
  public HammerImportCommand()
  {
    Helper.Command("hammer_import", "[hammer/hoe/etc] [tool] - Imports a new tool.", Execute);
    CommandWrapper.Register("hammer_import", (int index, int subIndex) => null);
    CommandWrapper.AddCompositeCommand("hammer_import");
  }
  protected static void Execute(Terminal.ConsoleEventArgs args)
  {
    Helper.ArgsCheck(args, 2, "Missing the equipment.");
    Helper.ArgsCheck(args, 3, "Missing the tool.");
    var equipment = args[1];
    var tool = string.Join(" ", args.Args, 2, args.Length - 1);
    ToolManager.Import(equipment, tool);
    Helper.AddMessage(args.Context, $"Imported tool {tool} to {equipment.ToString()}.");
  }
}
