using Service;
using UnityEngine;
namespace InfinityHammer;

public class ToolList
{
  public ToolList(string name, Tool tool)
  {
    Helper.Command(name, " [index to clipboard] - Lists commands from the menu.", (args) => Execute(args, tool));
    CommandWrapper.Register(name, (int index, int subIndex) =>
    {
      if (index == 0) return CommandWrapper.Info("Index to copy.");
      return null;
    });
  }
  protected static void Execute(Terminal.ConsoleEventArgs args, Tool tool)
  {
    var commands = CommandManager.Get(tool);
    var index = -1;
    if (args.Length > 1)
    {
      index = Parse.TryInt(args.Args, 1, index);
      GUIUtility.systemCopyBuffer = CommandManager.Get(tool, index);
    }
    for (var i = 0; i < commands.Count; i++)
    {
      var color = index == i ? "green" : "yellow";
      args.Context.AddString($"<color={color}>{i}:</color> {commands[i]}");
    }
  }
}
public class HammerListCommand : ToolList
{
  public static string Name = "hammer_list";
  public static Tool Tool = Tool.Hammer;
  public HammerListCommand() : base(Name, Tool)
  {
  }
}
public class HoeListCommand : ToolList
{
  public static string Name = "hoe_list";
  public static Tool Tool = Tool.Hoe;
  public HoeListCommand() : base(Name, Tool)
  {
  }
}
