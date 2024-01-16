using System;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;
public class HammerRotateCommand
{
  private static float ParseArgs(string[] values, Func<GameObject, bool> isSquare)
  {
    float amount;
    if (values[1].Contains("random"))
    {
      var ghost = HammerHelper.GetPlacementGhost();
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
      amount = Parse.Direction(values, 2) * Parse.Float(values[1], 0f);
    return amount;
  }
  public HammerRotateCommand()
  {
    AutoComplete.Register("hammer_rotate_x", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_x", "[degrees/random/number*random] - Rotates around the X axis.", (args) =>
    {
      if (args.Length < 2) return;
      var amount = ParseArgs(args.Args, HammerHelper.IsSquareX);
      PlaceRotation.RotateX(amount);
    });
    AutoComplete.Register("hammer_rotate_y", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_y", "[degrees/random/number*random] - Rotates around the Y axis.", (args) =>
    {
      if (args.Length < 2) return;
      var amount = ParseArgs(args.Args, HammerHelper.IsSquareY);
      PlaceRotation.RotateY(amount);
    });
    AutoComplete.Register("hammer_rotate_z", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Degrees to rotate. <color=yellow>step*random</color> randomly rotates within a given precision or <color=yellow>random</color> for 90/180 degrees depending on the object shape.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_rotate_z", "[degrees/random/number*random] - Rotates around the Z axis.", (args) =>
    {
      if (args.Length < 2) return;
      var amount = ParseArgs(args.Args, HammerHelper.IsSquareZ);
      PlaceRotation.RotateZ(amount);
    });
    AutoComplete.Register("hammer_rotate", (int index) =>
   {
     if (index == 0) return ParameterInfo.Create("Direction (1 or -1) to rotate.)");
     return null;
   });
    new Terminal.ConsoleCommand("hammer_rotate", "[direction] - Simulates normal rotating.", (args) =>
    {
      if (args.Length < 2) return;
      var player = Helper.GetPlayer();
      if (Parse.Direction(args.Args, 1) > 0)
        PlaceRotation.RotateY(-22.5f);
      else
        PlaceRotation.RotateY(22.5f);
    });
  }

}
