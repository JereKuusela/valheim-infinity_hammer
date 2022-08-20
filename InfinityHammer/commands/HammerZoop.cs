using System;
using Service;

namespace InfinityHammer;
public class HammerZoopCommand {
  private static void Command(string direction, Action<string> action, Action<string> reverse) {
    CommandWrapper.Register($"hammer_zoop_{direction}", (int index) => {
      if (index == 0) return CommandWrapper.Info($"Meters towards the {direction} direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      return null;
    });
    Helper.Command($"hammer_zoop_{direction}", $"[value=auto] - Zoops towards the {direction} direction.", (args) => {
      Helper.CheatCheck();
      var ghost = Helper.GetPlacementGhost();
      var value = "auto";
      if (args.Args.Length > 1)
        value = args[1];
      if (Parse.Direction(args.Args, 2) < 0)
        reverse(value);
      else
        action(value);
    });
  }
  public HammerZoopCommand() {
    Command("left", Selection.ZoopLeft, Selection.ZoopRight);
    Command("right", Selection.ZoopRight, Selection.ZoopLeft);
    Command("down", Selection.ZoopDown, Selection.ZoopUp);
    Command("up", Selection.ZoopUp, Selection.ZoopDown);
    Command("backward", Selection.ZoopBackward, Selection.ZoopForward);
    Command("forward", Selection.ZoopForward, Selection.ZoopBackward);
  }
}
