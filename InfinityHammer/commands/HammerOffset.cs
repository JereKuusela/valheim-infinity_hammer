using UnityEngine;

namespace InfinityHammer {
  public class HammerOffsetCommand {
    public HammerOffsetCommand() {
      CommandWrapper.Register("hammer_offset_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Sets the X offset.");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset_x", "[value] - Sets the X offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetX(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_offset_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Sets the Y offset.");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset_y", "[value] - Sets the Y offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetY(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_offset_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Sets the Z offset.");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset_z", "[value] - Sets the Z offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetZ(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_offset", (int index, int subIndex) => {
        if (index == 0) return CommandWrapper.XYZ("Sets the offset.", subIndex);
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset", "[x,y,z] - Sets the offset.", delegate (Terminal.ConsoleEventArgs args) {
        var value = Vector3.zero;
        if (args.Length > 1) {
          var split = args[1].Split(',');
          if (split.Length > 0) value.x = Helper.ParseFloat(split[0], 1f);
          if (split.Length > 1) value.y = Helper.ParseFloat(split[1], 1f);
          if (split.Length > 2) value.z = Helper.ParseFloat(split[2], 1f);
        }
        Offset.Set(value);
        Offset.Print(args.Context);
      });
    }
  }
}
