using System;
using UnityEngine;
namespace InfinityHammer;
public class HammerRotateCommand {
  private static float Parse(Terminal terminal, string[] values, Func<GameObject, bool> isSquare) {
    var amount = 0f;
    if (values[1].Contains("random")) {
      var ghost = Helper.GetPlacementGhost();
      System.Random rng = new();
      var multiplier = (int)Helper.ParseMultiplier(values[1]);
      if (multiplier == 1) {
        var square = isSquare(ghost);
        if (square) amount = 90f * rng.Next(0, 4);
        else amount = 180f * rng.Next(0, 2);
      } else amount = 360f / multiplier * rng.Next(0, multiplier + 1);
    } else
      amount = Helper.ParseDirection(values, 2) * Helper.ParseFloat(values[1], 0f);
    return amount;
  }
  public HammerRotateCommand() {
    CommandWrapper.Register("hammer_rotate_x", (int index) => {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_x", "[degrees/random/number*random] [direction=1] - Rotates around the X axis.", (args) => {
      if (args.Length < 2) return;
      var amount = Parse(args.Context, args.Args, Helper.IsSquareX);
      Rotating.RotateX(amount);
    });
    CommandWrapper.Register("hammer_rotate_y", (int index) => {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_y", "[degrees/random/number*random] [direction=1] - Rotates around the Y axis.", (args) => {
      if (args.Length < 2) return;
      var amount = Parse(args.Context, args.Args, Helper.IsSquareY);
      Rotating.RotateY(amount);
    });
    CommandWrapper.Register("hammer_rotate_z", (int index) => {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_z", "[degrees/random/number*random] [direction=1] - Rotates around the Z axis.", (args) => {
      if (args.Length < 2) return;
      var amount = Parse(args.Context, args.Args, Helper.IsSquareZ);
      Rotating.RotateZ(amount);
    });
  }
}
