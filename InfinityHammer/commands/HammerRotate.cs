using System;
namespace InfinityHammer;
public class HammerRotateCommand {
  public HammerRotateCommand() {
    CommandWrapper.Register("hammer_rotate_x", (int index) => {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_x", "[degrees] [direction=1] - Rotates around the X axis.", (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) return;
      Rotating.RotateX(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
    });
    CommandWrapper.Register("hammer_rotate_y", (int index) => {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate. <color=yellow>step*random</color> randomly rotates with a given step size or <color=yellow>random</color> for 90/180 degrees depending on the shape.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_y", "[degrees] [direction=1] - Rotates around the Y axis.", (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) return;
      var amount = 0f;
      if (args[1].Contains("random")) {
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var rng = new Random();
        var multiplier = (int)Helper.ParseMultiplier(args[1]);
        if (multiplier == 1) {
          var isSquare = Helper.IsSquare(ghost);
          if (isSquare) amount = 90f * rng.Next(0, 4);
          else amount = 180f * rng.Next(0, 2);
        } else amount = 360f / multiplier * rng.Next(0, multiplier + 1);
      } else
        amount = Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f);
      Rotating.RotateY(amount);
    });
    CommandWrapper.Register("hammer_rotate_z", (int index) => {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_z", "[degrees] [direction=1] - Rotates around the Z axis.", (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) return;
      Rotating.RotateZ(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
    });
  }
}
