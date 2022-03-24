using UnityEngine;

namespace InfinityHammer {
  public class HammerMoveCommand {
    public HammerMoveCommand() {
      CommandWrapper.Register("hammer_move_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the X offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_x", "[value=auto] [direction=1] - Moves the X offset.", delegate (Terminal.ConsoleEventArgs args) {
        var amount = 0f;
        if (args.Length < 2 || args[1].Contains("auto")) {
          amount = Bounds.Get[Utils.GetPrefabName(Player.m_localPlayer.m_placementGhost)].x;
          amount *= Helper.ParseMultiplier(args.Args, 1);
        } else
          amount = Helper.ParseFloat(args[1], 0f);
        Offset.MoveX(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the Y offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_y", "[value=auto] [direction=1] - Moves the Y offset.", delegate (Terminal.ConsoleEventArgs args) {
        var amount = 0f;
        if (args.Length < 2 || args[1].Contains("auto")) {
          amount = Bounds.Get[Utils.GetPrefabName(Player.m_localPlayer.m_placementGhost)].y;
          amount *= Helper.ParseMultiplier(args.Args, 1);
        } else
          amount = Helper.ParseFloat(args[1], 0f);
        Offset.MoveY(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the Z offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_z", "[value=auto] [direction=1] - Moves the Z offset.", delegate (Terminal.ConsoleEventArgs args) {
        var amount = 0f;
        if (args.Length < 2 || args[1].Contains("auto")) {
          amount = Bounds.Get[Utils.GetPrefabName(Player.m_localPlayer.m_placementGhost)].z;
          amount *= Helper.ParseMultiplier(args.Args, 1);
        } else
          amount = Helper.ParseFloat(args[1], 0f);
        Offset.MoveZ(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move", (int index) => {
        if (index < 3) return CommandWrapper.XYZ("Moves the offset.", index);
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move", "[x,y,z] - Moves the offset.", delegate (Terminal.ConsoleEventArgs args) {
        var value = Vector3.zero;
        if (args.Length > 1) {
          var split = args[1].Split(',');
          if (split.Length > 0) value.x = Helper.ParseFloat(split[0], 1f);
          if (split.Length > 1) value.y = Helper.ParseFloat(split[1], 1f);
          if (split.Length > 2) value.z = Helper.ParseFloat(split[2], 1f);
        }
        Offset.Move(value);
        Offset.Print(args.Context);
      });
    }
  }
}
