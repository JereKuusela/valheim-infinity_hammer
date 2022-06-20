using System;
using Service;
namespace InfinityHammer;
public class ToolScale {
  private static void Scale(string[] args, Action<float, float> action) {
    if (args[1].EndsWith("%", StringComparison.Ordinal)) {
      var amount = Parse.Direction(args, 2) * Parse.TryFloat(args[1].Substring(0, args[1].Length - 1), 0.05f);
      action(0f, amount);
    } else {
      var amount = Parse.Direction(args, 2) * Parse.TryFloat(args[1], 1f);
      action(amount, 0f);
    }
  }
  private static void Command(string name, Tool tool, string axis, Action<float, float> action) {
    CommandWrapper.Register($"{name}_{axis}", (int index) => {
      if (index == 0) return CommandWrapper.Info("Flat amount or percentage to scale.");
      return null;
    });
    Helper.Command($"{name}_{axis}", $"[percentage] - Scales the {axis} axis (if the object supports it).", (args) => {
      if (!Hammer.HasTool(Helper.GetPlayer(), Tool.Hammer)) return;
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      Scale(args.Args, action);
      Scaling.PrintScale(args.Context, tool);
    });
  }
  public ToolScale(string name, Tool tool) {
    var scale = Scaling.Get(tool);
    Command(name, tool, "x", scale.ScaleX);
    Command(name, tool, "y", scale.ScaleY);
    Command(name, tool, "z", scale.ScaleZ);
    CommandWrapper.Register(name, (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.Info("Flat amount or percentage to scale. If missing, resets the scale.");
      return null;
    });
    Helper.Command(name, "[amount] - Scales the selection (if the object supports it).", (args) => {
      if (!Hammer.HasTool(Helper.GetPlayer(), Tool.Hammer)) return;
      if (args.Length < 2)
        scale.SetScale(1f);
      else {
        Scale(args.Args, scale.Scale);
      }
      Scaling.PrintScale(args.Context, tool);
    });
  }
}
public class HammerScale : ToolScale {
  public HammerScale() : base("hammer_scale", Tool.Hammer) {
  }
}

public class HoeScale : ToolScale {
  public HoeScale() : base("hoe_scale", Tool.Hoe) {
  }
}