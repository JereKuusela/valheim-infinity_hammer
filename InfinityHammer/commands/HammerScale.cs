using UnityEngine;
namespace InfinityHammer;
public class HammerScaleCommand {
  public HammerScaleCommand() {
    CommandWrapper.RegisterEmpty("hammer_scale_up");
    new Terminal.ConsoleCommand("hammer_scale_up", "Scales up the selection (if the object supports it).", (Terminal.ConsoleEventArgs args) => {
      if (Settings.ScaleStep <= 0f) return;
      Scaling.ScaleUp();
      Scaling.PrintScale(args.Context);
    });
    CommandWrapper.RegisterEmpty("hammer_scale_down");
    new Terminal.ConsoleCommand("hammer_scale_down", "Scales down the selection (if the object supports it).", (Terminal.ConsoleEventArgs args) => {
      if (Settings.ScaleStep <= 0f) return;
      Scaling.ScaleDown();
      Scaling.PrintScale(args.Context);
    });
    CommandWrapper.Register("hammer_scale", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.Scale("Sets the size (if the object supports it).", subIndex);
      return null;
    });
    new Terminal.ConsoleCommand("hammer_scale", "[scale=1] - Sets the size (if the object supports it).", (Terminal.ConsoleEventArgs args) => {
      if (Settings.ScaleStep <= 0f) return;
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
