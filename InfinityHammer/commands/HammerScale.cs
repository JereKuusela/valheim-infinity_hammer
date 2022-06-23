using System;
using System.Linq;
using Service;
namespace InfinityHammer;
public class HammerScale {
  private static void Scale(string[] args, Action<float, float> action) {
    if (args[0].EndsWith("%", StringComparison.Ordinal)) {
      var amount = Parse.Direction(args, 1) * Parse.TryFloat(args[0].Substring(0, args[0].Length - 1), 5f) / 100f;
      action(0f, amount);
    } else {
      var amount = Parse.Direction(args, 1) * Parse.TryFloat(args[0], 1f);
      action(amount, 0f);
    }
  }
  private static void Command(string name, string axis, Func<ToolScaling, Action<float, float>> action) {
    CommandWrapper.Register($"{name}_{axis}", (int index) => {
      if (index == 0) return new() { "all", "build", "command" };
      if (index == 1) return CommandWrapper.Info("Flat amount or percentage to scale.");
      return null;
    });
    Helper.Command($"{name}_{axis}", $"[all/build/command] [percentage] - Scales the {axis} axis (if the object supports it).", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the type parameter (all, build or command).");
      Helper.ArgsCheck(args, 3, "Missing the amount.");
      if (args[1] == "build" && Selection.Type == SelectionType.Command) return;
      if (args[1] == "command" && Selection.Type != SelectionType.Command) return;
      Scale(args.Args.Skip(2).ToArray(), action(Scaling.Get()));
      Scaling.PrintScale(args.Context);
    });
  }
  public HammerScale() {
    var name = "hammer_scale";
    Command(name, "x", (scale) => scale.ScaleX);
    Command(name, "y", (scale) => scale.ScaleY);
    Command(name, "z", (scale) => scale.ScaleZ);
    CommandWrapper.Register(name, (int index, int subIndex) => {
      if (index == 0) return new() { "all", "build", "command" };
      if (index == 1) return CommandWrapper.Info("Flat amount or percentage to scale. If missing, resets the scale.");
      return null;
    });
    Helper.Command(name, "[all/build/command] [amount] - Scales the selection (if the object supports it).", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the type parameter (all, build or command).");
      if (args[1] == "build" && Selection.Type == SelectionType.Command) return;
      if (args[1] == "command" && Selection.Type != SelectionType.Command) return;
      var scale = Scaling.Get();
      if (args.Length < 3)
        scale.SetScale(1f);
      else {
        Scale(args.Args.Skip(2).ToArray(), scale.Scale);
      }
      Scaling.PrintScale(args.Context);
    });
  }
}
