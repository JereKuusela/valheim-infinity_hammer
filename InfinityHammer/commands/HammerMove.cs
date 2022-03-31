namespace InfinityHammer;
public class HammerMoveCommand {
  public HammerMoveCommand() {
    CommandWrapper.Register("hammer_move_left", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters towards the left direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_move_left", "[value=auto] [direction=1] - Moves the placement towards the left direction.", (Terminal.ConsoleEventArgs args) => {
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.TryParseSize(ghost, args.Args, 1).x;
      Offset.MoveLeft(Helper.ParseDirection(args.Args, 2) * amount);
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_move_right", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters towards the right direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_move_right", "[value=auto] [direction=1] - Moves the placement towards the right direction.", (Terminal.ConsoleEventArgs args) => {
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.TryParseSize(ghost, args.Args, 1).x;
      Offset.MoveRight(Helper.ParseDirection(args.Args, 2) * amount);
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_move_down", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters towards the up direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_move_down", "[value=auto] [direction=1] - Moves the placement towards the up direction.", (Terminal.ConsoleEventArgs args) => {
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.TryParseSize(ghost, args.Args, 1).y;
      Offset.MoveDown(Helper.ParseDirection(args.Args, 2) * amount);
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_move_up", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters towards the up direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_move_up", "[value=auto] [direction=1] - Moves the placement towards the up direction.", (Terminal.ConsoleEventArgs args) => {
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.TryParseSize(ghost, args.Args, 1).y;
      Offset.MoveUp(Helper.ParseDirection(args.Args, 2) * amount);
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_move_backward", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters towards the backward direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_move_backward", "[value=auto] [direction=1] - Moves the placement towards the backward direction.", (Terminal.ConsoleEventArgs args) => {
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.TryParseSize(ghost, args.Args, 1).z;
      Offset.MoveBackward(Helper.ParseDirection(args.Args, 2) * amount);
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_move_forward", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters towards the forward direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_move_forward", "[value=auto] [direction=1] - Moves the placement towards the forward direction.", (Terminal.ConsoleEventArgs args) => {
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.TryParseSize(ghost, args.Args, 1).z;
      Offset.MoveForward(Helper.ParseDirection(args.Args, 2) * amount);
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_move", (int index) => {
      if (index < 3) return CommandWrapper.DirectionZYX("Meters to move the placement", index);
      return null;
    });
    new Terminal.ConsoleCommand("hammer_move", "[forward,up,right] - Moves the placement.", (Terminal.ConsoleEventArgs args) => {
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.TryParseSizesZYX(ghost, args.Args, 1, "0");
      Offset.Move(amount);
      Offset.Print(args.Context);
    });
  }
}
