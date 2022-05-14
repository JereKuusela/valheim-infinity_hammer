using System;
using UnityEngine;
namespace InfinityHammer;
public class HammerScaleCommand {
  private static void Command(string direction, Action action) {
    CommandWrapper.RegisterEmpty($"hammer_scale_{direction}");
    Helper.Command($"hammer_scale_{direction}", $"Scales {direction} the selection (if the object supports it).", (args) => {
      CheckStep();
      action();
      Scaling.PrintScale(args.Context);
    });
  }
  private static void CheckStep() {
    if (Settings.ScaleStep <= 0f)
      throw new InvalidOperationException("Invalid step size on the mod configuration.");
  }
  public HammerScaleCommand() {
    Command("up", Scaling.ScaleUp);
    Command("down", Scaling.ScaleDown);
    CommandWrapper.Register("hammer_scale", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.Scale("Sets the size (if the object supports it).", subIndex);
      return null;
    });
    Helper.Command("hammer_scale", "[scale=1] - Sets the size (if the object supports it).", (args) => {
      CheckStep();
      if (args.Length < 2)
        Scaling.SetScale(1f);
      else if (args[1].Contains(",")) {
        var scale = Vector3.one;
        var split = args[1].Replace("scale=", "").Split(',');
        if (split.Length > 0) scale.x = Helper.ParseFloat(split[0], 1f);
        if (split.Length > 1) scale.y = Helper.ParseFloat(split[1], 1f);
        if (split.Length > 2) scale.z = Helper.ParseFloat(split[2], 1f);
        if (scale.x == 0f) scale.x = 1f;
        if (scale.y == 0f) scale.y = 1f;
        if (scale.z == 0f) scale.z = 1f;
        Scaling.SetScale(scale);
      } else
        Scaling.SetScale(Helper.ParseFloat(args[1].Replace("scale=", ""), 1f));
      Scaling.PrintScale(args.Context);
    });
  }
}
