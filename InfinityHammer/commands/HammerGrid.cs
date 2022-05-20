namespace InfinityHammer;
public class HammerGridCommand {
  public HammerGridCommand() {
    CommandWrapper.Register("hammer_grid", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.Info("Grid precision in meters.");
      if (index == 1) return CommandWrapper.XYZ("Grid center point. If not given, uses the current placement position.", subIndex);
      return null;
    });
    Helper.Command("hammer_grid", " [precision] [center=current] - Restricts possible placement coordinates. Using the same command removes the restriction.", (args) => {
      var ghost = Helper.GetPlacementGhost();
      var precision = args.Length < 2 ? 0f : Helper.ParseFloat(args[1], 0f);
      var center = args.Length < 3 ? ghost.transform.position : Helper.ParseXYZ(args[2]);
      Grid.Set(precision, center);
      Helper.AddMessage(args.Context, Grid.Enabled ? $"Grid {precision} enabled." : "Grid disabled.");
    });
  }
}
