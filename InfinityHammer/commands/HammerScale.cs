using System;
using Service;
namespace InfinityHammer;
public class HammerScaleCommand {
  private static void Scale(string amountStr, string direction, Action<float> action) {
    var amount = Parse.Direction(direction) * Parse.TryFloat(amountStr, 1f);
    action(amount);
  }
  private static void CommandAxis(string name, string axis, Func<ToolScaling, Action<float>> action, bool isCommand) {
    name = $"{name}_{axis}";
    if (isCommand) name += "_cmd";
    CommandWrapper.Register(name, (int index) => {
      if (index == 0) return CommandWrapper.Info("Amount.");
      return null;
    });
    Helper.Command(name, $"[amount] - Sets the scale of {axis} axis (if the object supports it).", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      if (!isCommand && Selection.Type == SelectionType.Command) return;
      if (isCommand && Selection.Type != SelectionType.Command) return;
      if (!Helper.GetPlayer().InPlaceMode()) return;
      var direction = args.Length > 2 ? args[2] : "";
      Scale(args[1], direction, action(Scaling.Get()));
      if (!isCommand)
        Scaling.PrintScale(args.Context);
    });
  }
  private static void Command(string name, bool isCommand) {
    if (isCommand) name += "_cmd";
    CommandWrapper.Register(name, (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.XZY("Amount of scale.", subIndex);
      return null;
    });
    Helper.Command(name, "[amount or x,z,y] - Sets the scale (if the object supports it).", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      if (!isCommand && Selection.Type == SelectionType.Command) return;
      if (isCommand && Selection.Type != SelectionType.Command) return;
      if (!Helper.GetPlayer().InPlaceMode()) return;
      var scaling = Scaling.Get();
      var scale = Parse.TryScale(Parse.Split(args[1])) * Parse.Direction(args.Args, 2);
      scaling.SetScale(scale);
      if (!isCommand)
        Scaling.PrintScale(args.Context);
    });
  }
  public HammerScaleCommand() {
    var name = "hammer_scale";
    CommandAxis(name, "x", (scale) => scale.SetScaleX, false);
    CommandAxis(name, "y", (scale) => scale.SetScaleY, false);
    CommandAxis(name, "z", (scale) => scale.SetScaleZ, false);
    CommandAxis(name, "x", (scale) => scale.SetScaleX, true);
    CommandAxis(name, "y", (scale) => scale.SetScaleY, true);
    CommandAxis(name, "z", (scale) => scale.SetScaleZ, true);
    Command(name, false);
    Command(name, true);
  }
}
