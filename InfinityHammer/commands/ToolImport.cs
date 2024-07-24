using InfinityHammer;
using ServerDevcommands;

namespace InfinityTools;

public class ToolImportCommand
{
  public ToolImportCommand()
  {
    Helper.Command("tool_import", "[hammer/hoe/etc[data] - Imports a new tool.", Execute);
    AutoComplete.Register("tool_import", (int index, int subIndex) =>
    {
      if (index == 0) return [.. ToolManager.Tools.Keys];
      return ParameterInfo.Create("Tool data");
    });
  }
  protected static void Execute(Terminal.ConsoleEventArgs args)
  {
    Helper.ArgsCheck(args, 2, "Missing the equipment.");
    Helper.ArgsCheck(args, 3, "Missing the tool data.");
    var equipment = args[1];
    var data = string.Join(" ", args.Args, 2, args.Length - 2);
    var result = ToolManager.Import(equipment, data);
    HammerHelper.Message(args.Context, $"Imported tool {result.name} to {equipment}.");
  }
}