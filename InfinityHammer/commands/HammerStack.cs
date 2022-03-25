using HarmonyLib;
using UnityEngine;

namespace InfinityHammer {
  public class HammerStackCommand {
    private static void Execute(Vector3 delta, Vector3Int min, Vector3Int max) {
      var ghostPosition = Player.m_localPlayer.m_placementGhost.transform.position;
      for (var x = min.x; x <= max.x; x++) {
        for (var y = min.y; y <= max.y; y++) {
          for (var z = min.z; z <= max.z; z++) {
            var position = ghostPosition;
            position.x += x * delta.x;
            position.y += y * delta.y;
            position.z += z * delta.z;
            OverridePlacement.Override = position;
            Hammer.Place();
          }
        }
      }
      OverridePlacement.Override = null;
    }
    public HammerStackCommand() {
      CommandWrapper.Register("hammer_stack_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Amount of objects to be placed (number or min-max).");
        if (index == 1) return CommandWrapper.Info("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 2) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_stack_x", "[amount] [step=auto] [direction=1] - Stacks multiple objects next to each other (forward / backward).", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          Helper.AddMessage(args.Context, "Error: Missing the amount.");
          return;
        }
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.ParseIntRange(args[1]);
        var size = Helper.ParseSizes(ghost, args.Args, 2).x;
        var direction = Helper.ParseDirection(args.Args, 3);
        var delta = new Vector3(direction * size, 0f, 0f);
        Execute(delta, new Vector3Int(amount.Min, 0, 0), new Vector3Int(amount.Max, 0, 0));
      });
      CommandWrapper.Register("hammer_stack_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Amount of objects to be placed (number or min-max).");
        if (index == 1) return CommandWrapper.Info("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 2) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_stack_y", "[amount] [step=auto] [direction=1] - Stacks multiple objects next to each other (up / down).", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          Helper.AddMessage(args.Context, "Error: Missing the amount.");
          return;
        }
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.ParseIntRange(args[1]);
        var size = Helper.ParseSizes(ghost, args.Args, 2).y;
        var direction = Helper.ParseDirection(args.Args, 3);
        var delta = new Vector3(0f, direction * size, 0f);
        Execute(delta, new Vector3Int(0, amount.Min, 0), new Vector3Int(0, amount.Max, 0));
      });
      CommandWrapper.Register("hammer_stack_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Amount of objects to be placed (number or min-max).");
        if (index == 1) return CommandWrapper.Info("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 2) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_stack_z", "[amount] [step=auto] [direction=1] - Stacks multiple objects next to each other (left / right).", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          Helper.AddMessage(args.Context, "Error: Missing the amount.");
          return;
        }
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.ParseIntRange(args[1]);
        var size = Helper.ParseSizes(ghost, args.Args, 2).z;
        var direction = Helper.ParseDirection(args.Args, 3);
        var delta = new Vector3(0f, 0f, direction * size);
        Execute(delta, new Vector3Int(0, 0, amount.Min), new Vector3Int(0, 0, amount.Max));
      });
      CommandWrapper.Register("hammer_stack", (int index, int subIndex) => {
        if (index == 0) return CommandWrapper.XYZ("Amounts.", subIndex);
        if (index == 1) return CommandWrapper.XYZ("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).", subIndex);
        if (index == 2) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_stack", "[x,y,z] [step=auto,auto,auto] [direction=1] - Stacks multiple objects next to each other.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          Helper.AddMessage(args.Context, "Error: Missing the amount.");
          return;
        }
        var ghost = Helper.GetPlacementGhost(args.Context);
        if (!ghost) return;
        var amount = Helper.ParseXYZRange(args[1]);
        var size = Helper.ParseSizes(ghost, args.Args, 2);
        var direction = Helper.ParseDirection(args.Args, 3);
        var delta = direction * size;
        Execute(delta, amount.Min, amount.Max);
      });
    }
  }

  [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
  public class OverridePlacement {
    public static Vector3? Override = null;
    public static bool Prefix(Player __instance) {
      if (!__instance.m_placementGhost) return true;
      if (Override.HasValue) __instance.m_placementGhost.transform.position = Override.Value;
      return !Override.HasValue;
    }
  }
}
