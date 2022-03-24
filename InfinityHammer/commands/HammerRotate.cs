namespace InfinityHammer {
  public class HammerRotateCommand {
    public HammerRotateCommand() {
      CommandWrapper.Register("hammer_rotate_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Rotates around the X axis.");
        if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_rotate_x", "[value] [direction=1] - Rotates around the X axis.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Rotating.RotateX(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
      });
      CommandWrapper.Register("hammer_rotate_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Rotates around the Y axis.");
        if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_rotate_y", "[value] [direction=1] - Rotates around the Y axis.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Rotating.RotateY(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
      });
      CommandWrapper.Register("hammer_rotate_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Rotates around the Z axis.");
        if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_rotate_z", "[value] [direction=1] - Rotates around the Z axis.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Rotating.RotateZ(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
      });
    }
  }
}
