using System.Linq;
using InfinityHammer;
using ServerDevcommands;
using UnityEngine;

namespace InfinityTools;

public class ToolExportCommand
{
  public ToolExportCommand()
  {
    Helper.Command("tool_export", "[name] - Exports a tool", Execute);
    AutoComplete.Register("tool_export", (int index, int subIndex) => ToolManager.GetAll().Select(tool => tool.Name).ToList());
  }
  protected static void Execute(Terminal.ConsoleEventArgs args)
  {
    Helper.ArgsCheck(args, 2, "Missing the tool name.");
    var tool = string.Join(" ", args.Args, 1, args.Length - 1);
    var equipment = Hammer.Get();
    var result = ToolManager.Export(equipment, tool);
    if (string.IsNullOrEmpty(result))
    {
      HammerHelper.Message(args.Context, $"Unable to find the tool {tool}.");
      return;
    }
    GUIUtility.systemCopyBuffer = result;
    HammerHelper.Message(args.Context, $"Exported tool {tool} to the clipboard.");
  }
}