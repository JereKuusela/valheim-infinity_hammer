using System;
using UnityEngine;
namespace InfinityHammer;
public class HammerOffsetCommand
{
  private static void Command(string name, string direction, Action<float> action)
  {
    CommandWrapper.Register($"hammer_offset_{name}", (int index) =>
    {
      if (index == 0) return CommandWrapper.Info($"Meters in the {direction} direction.");
      return null;
    });
    Helper.Command($"hammer_offset_{name}", $"[value=0] - Sets the {direction} offset.", (args) =>
    {
      action(Helper.ParseFloat(args[1], 0f));
      Position.Print(args.Context);
    });
  }
  public HammerOffsetCommand()
  {
    Command("x", "right / left", Position.SetX);
    Command("y", "up / down", Position.SetY);
    Command("z", "forward / backward", Position.SetZ);
    CommandWrapper.Register("hammer_offset", (int index, int subIndex) =>
    {
      if (index == 0) return CommandWrapper.FRU("Sets the offset", subIndex);
      return null;
    });
    new Terminal.ConsoleCommand("hammer_offset", "[forward,up,right=0,0,0] - Sets the offset.", (args) =>
    {
      var value = Vector3.zero;
      if (args.Length > 1) value = Helper.ParseZYX(args[1]);
      Position.Set(value);
      Position.Print(args.Context);
    });
  }
}
