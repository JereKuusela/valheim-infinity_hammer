namespace InfinityHammer {
  public class HammerMoveCommand {
    public HammerMoveCommand() {
      CommandWrapper.Register("hammer_move_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the X offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_x", "[value=auto] [direction=1] - Moves the X offset.", delegate (Terminal.ConsoleEventArgs args) {
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.TryParseSize(ghost, args.Args, 1).x;
        Offset.MoveX(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the Y offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_y", "[value=auto] [direction=1] - Moves the Y offset.", delegate (Terminal.ConsoleEventArgs args) {
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.TryParseSize(ghost, args.Args, 1).y;
        Offset.MoveY(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the Z offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_z", "[value=auto] [direction=1] - Moves the Z offset.", delegate (Terminal.ConsoleEventArgs args) {
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.TryParseSize(ghost, args.Args, 1).z;
        Offset.MoveZ(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move", (int index) => {
        if (index < 3) return CommandWrapper.XYZ("Moves the offset.", index);
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move", "[x,y,z] - Moves the offset.", delegate (Terminal.ConsoleEventArgs args) {
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.TryParseSizes(ghost, args.Args, 1, "0,0,0");
        Offset.Move(amount);
        Offset.Print(args.Context);
      });
    }
  }
}
