using UnityEngine;
namespace InfinityHammer;
public class HammerOffsetCommand {
  public HammerOffsetCommand() {
    CommandWrapper.Register("hammer_offset_x", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters in the right / left direction.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_offset_x", "[value=0] - Sets the right / left offset.", (Terminal.ConsoleEventArgs args) => {
      Offset.SetX(Helper.ParseFloat(args[1], 0f));
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_offset_y", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters in the up / down direction.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_offset_y", "[value=0] - Sets the up / down offset.", (Terminal.ConsoleEventArgs args) => {
      Offset.SetY(Helper.ParseFloat(args[1], 0f));
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_offset_z", (int index) => {
      if (index == 0) return CommandWrapper.Info("Meters in the forward / backward direction.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_offset_z", "[value=0] - Sets the forward / backward offset.", (Terminal.ConsoleEventArgs args) => {
      Offset.SetZ(Helper.ParseFloat(args[1], 0f));
      Offset.Print(args.Context);
    });
    CommandWrapper.Register("hammer_offset", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.FRU("Sets the offset", subIndex);
      return null;
    });
    new Terminal.ConsoleCommand("hammer_offset", "[forward,up,right=0,0,0] - Sets the offset.", (Terminal.ConsoleEventArgs args) => {
      var value = Vector3.zero;
      if (args.Length > 1) value = Helper.ParseZYX(args[1]);
      Offset.Set(value);
      Offset.Print(args.Context);
    });
  }
}
