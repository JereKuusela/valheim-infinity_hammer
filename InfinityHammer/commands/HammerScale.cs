using System;
using Service;
namespace InfinityHammer;
public class HammerScaleCommand {
  private static void Command(string direction, Action<float> action) {
    CommandWrapper.Register($"hammer_scale_{direction}", (int index) => {
      if (index == 0) return CommandWrapper.Info("Percentage to scale.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    Helper.Command($"hammer_scale_{direction}", $"[percentage] [direction=1] - Scales the {direction} axis (if the object supports it).", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      var amount = Parse.Direction(args.Args, 2) * Parse.TryFloat(args[1], 0.05f);
      action(amount);
      Scaling.PrintScale(args.Context);
    });
  }
  public HammerScaleCommand() {
    Command("x", Scaling.ScaleX);
    Command("y", Scaling.ScaleY);
    Command("z", Scaling.ScaleZ);
    CommandWrapper.Register("hammer_scale", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.Info("Percentage to scale. If missing, resets the scale.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    Helper.Command("hammer_scale", "[amount] [direction=1] - Scales the selection (if the object supports it).", (args) => {
      if (args.Length < 2)
        Scaling.SetScale(1f);
      else {
        var amount = Parse.Direction(args.Args, 2) * Parse.TryFloat(args[1], 0.05f);
        Scaling.Scale(amount);
      }
      Scaling.PrintScale(args.Context);
    });
  }
}
