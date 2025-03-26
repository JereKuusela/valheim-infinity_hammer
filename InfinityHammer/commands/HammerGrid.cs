using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;
public class HammerGridCommand
{
  public HammerGridCommand()
  {
    AutoComplete.Register("hammer_grid", (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.Create("Grid precision in meters.");
      if (index == 1) return ParameterInfo.XZY("Grid center point. If not given, uses the current placement position.", subIndex);
      return ParameterInfo.None;
    });
    Helper.Command("hammer_grid", " [precision] [center=current] - Restricts possible placement coordinates. Using the same command removes the restriction.", (args) =>
    {
      var ghost = HammerHelper.GetPlacementGhost();
      var precision = args.Length < 2 ? 0f : Parse.Float(args[1], 0f);
      var center = args.Length < 3 ? ghost.transform.position : Parse.VectorXZY(args[2]);
      Grid.Toggle(precision, center);
      HammerHelper.Message(args.Context, Grid.Enabled ? $"Grid {precision} enabled." : "Grid disabled.");
    });
  }
}
