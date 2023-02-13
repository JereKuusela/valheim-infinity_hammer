using System;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class HammerRotateCommand
{
  private static float ParseArgs(Terminal terminal, string[] values, Func<GameObject, bool> isSquare)
  {
    var amount = 0f;
    if (values[1].Contains("random"))
    {
      var ghost = Helper.GetPlacementGhost();
      System.Random rng = new();
      var multiplier = (int)Parse.Multiplier(values[1]);
      if (multiplier == 1)
      {
        var square = isSquare(ghost);
        if (square) amount = 90f * rng.Next(0, 4);
        else amount = 180f * rng.Next(0, 2);
      }
      else amount = 360f / multiplier * rng.Next(0, multiplier + 1);
    }
    else
      amount = Parse.Direction(values, 2) * Helper.ParseFloat(values[1], 0f);
    return amount;
  }
  public HammerRotateCommand()
  {
    CommandWrapper.Register("hammer_rotate_x", (int index) =>
    {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_x", "[degrees/random/number*random] - Rotates around the X axis.", (args) =>
    {
      if (args.Length < 2) return;
      var amount = ParseArgs(args.Context, args.Args, Helper.IsSquareX);
      Rotating.RotateX(amount);
    });
    CommandWrapper.Register("hammer_rotate_y", (int index) =>
    {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_y", "[degrees/random/number*random] - Rotates around the Y axis.", (args) =>
    {
      if (args.Length < 2) return;
      var amount = ParseArgs(args.Context, args.Args, Helper.IsSquareY);
      Rotating.RotateY(amount);
    });
    CommandWrapper.Register("hammer_rotate_z", (int index) =>
    {
      if (index == 0) return CommandWrapper.Info("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_z", "[degrees/random/number*random] - Rotates around the Z axis.", (args) =>
    {
      if (args.Length < 2) return;
      var amount = ParseArgs(args.Context, args.Args, Helper.IsSquareZ);
      Rotating.RotateZ(amount);
    });
    CommandWrapper.Register("hammer_rotate", (int index) =>
    {
      if (index == 0) return CommandWrapper.Info("Direction (1 or -1) to rotate.)");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate", "[direction] - Simulates normal rotating.", (args) =>
    {
      if (args.Length < 2) return;
      var player = Helper.GetPlayer();
      if (args.TryParameterInt(1) > 0)
        Helper.GetPlayer().m_placeRotation--;
      else
        Helper.GetPlayer().m_placeRotation++;
    });
  }
}
