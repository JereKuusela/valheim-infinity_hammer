using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;
public class HammerStackCommand {
  private static void Execute(Vector3 delta, Vector3Int min, Vector3Int max) {
    UndoHelper.StartCreating();
    var ghostPosition = Player.m_localPlayer.m_placementGhost.transform.position;
    var ghostRotation = Player.m_localPlayer.m_placementGhost.transform.rotation;
    for (var x = min.x; x <= max.x; x++) {
      for (var y = min.y; y <= max.y; y++) {
        for (var z = min.z; z <= max.z; z++) {
          var position = ghostPosition;
          position += ghostRotation * Vector3.right * x * delta.x;
          position += ghostRotation * Vector3.up * y * delta.y;
          position += ghostRotation * Vector3.forward * z * delta.z;
          OverridePlacement.OverridePosition = position;
          OverridePlacement.OverrideRotation = ghostRotation;
          Hammer.Place();
        }
      }
    }
    OverridePlacement.OverridePosition = null;
    OverridePlacement.OverrideRotation = null;
    UndoHelper.FinishCreating();
  }
  private static List<string> AutoComplete(int index) {
    if (index == 0) return CommandWrapper.Info("Amount of objects to be placed (number or min-max).");
    if (index == 1) return CommandWrapper.Info("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
    if (index == 2) return CommandWrapper.Info("Direction (default 1 or -1).");
    return null;
  }
  private static string Description(string direction) => $"[amount] [step=auto] [direction=1] - Places multiple objects towards the {direction} direction.";
  public HammerStackCommand() {
    CommandWrapper.Register("hammer_stack_left", AutoComplete);
    new Terminal.ConsoleCommand("hammer_stack_left", Description("left"), (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, "Error: Missing the amount.");
        return;
      }
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.ParseIntRange(args[1]);
      var size = Helper.TryParseSize(ghost, args.Args, 2).x;
      var direction = Helper.ParseDirection(args.Args, 3);
      Vector3 delta = new(-direction * size, 0f, 0f);
      Execute(delta, new(amount.Min, 0, 0), new(amount.Max, 0, 0));
    });
    CommandWrapper.Register("hammer_stack_right", AutoComplete);
    new Terminal.ConsoleCommand("hammer_stack_right", Description("right"), (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, "Error: Missing the amount.");
        return;
      }
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.ParseIntRange(args[1]);
      var size = Helper.TryParseSize(ghost, args.Args, 2).x;
      var direction = Helper.ParseDirection(args.Args, 3);
      var delta = new Vector3(direction * size, 0f, 0f);
      Execute(delta, new Vector3Int(amount.Min, 0, 0), new Vector3Int(amount.Max, 0, 0));
    });
    CommandWrapper.Register("hammer_stack_down", AutoComplete);
    new Terminal.ConsoleCommand("hammer_stack_down", Description("down"), (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, "Error: Missing the amount.");
        return;
      }
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.ParseIntRange(args[1]);
      var size = Helper.TryParseSize(ghost, args.Args, 2).y;
      var direction = Helper.ParseDirection(args.Args, 3);
      Vector3 delta = new(0f, -direction * size, 0f);
      Execute(delta, new(0, amount.Min, 0), new(0, amount.Max, 0));
    });
    CommandWrapper.Register("hammer_stack_up", AutoComplete);
    new Terminal.ConsoleCommand("hammer_stack_up", Description("up"), (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, "Error: Missing the amount.");
        return;
      }
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.ParseIntRange(args[1]);
      var size = Helper.TryParseSize(ghost, args.Args, 2).y;
      var direction = Helper.ParseDirection(args.Args, 3);
      Vector3 delta = new(0f, direction * size, 0f);
      Execute(delta, new(0, amount.Min, 0), new(0, amount.Max, 0));
    });
    CommandWrapper.Register("hammer_stack_backward", AutoComplete);
    new Terminal.ConsoleCommand("hammer_stack_backward", Description("backward"), (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, "Error: Missing the amount.");
        return;
      }
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.ParseIntRange(args[1]);
      var size = Helper.TryParseSize(ghost, args.Args, 2).z;
      var direction = Helper.ParseDirection(args.Args, 3);
      Vector3 delta = new(0f, 0f, -direction * size);
      Execute(delta, new(0, 0, amount.Min), new(0, 0, amount.Max));
    });
    CommandWrapper.Register("hammer_stack_forward", AutoComplete);
    new Terminal.ConsoleCommand("hammer_stack_forward", Description("forward"), (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, "Error: Missing the amount.");
        return;
      }
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.ParseIntRange(args[1]);
      var size = Helper.TryParseSize(ghost, args.Args, 2).z;
      var direction = Helper.ParseDirection(args.Args, 3);
      Vector3 delta = new(0f, 0f, direction * size);
      Execute(delta, new(0, 0, amount.Min), new(0, 0, amount.Max));
    });
    CommandWrapper.Register("hammer_stack", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.FRU("Amounts", subIndex);
      if (index == 1) return CommandWrapper.FRU("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size)", subIndex);
      if (index == 2) return CommandWrapper.Info("Direction (default 1 or -1).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_stack", "[forward,up,right] [step=auto,auto,auto] [direction=1] - Places multiple objects next to each other.", (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, "Error: Missing the amount.");
        return;
      }
      var ghost = Helper.GetPlacementGhost(args.Context);
      if (!ghost) return;
      var amount = Helper.ParseZYXRange(args[1]);
      var size = Helper.TryParseSizesZYX(ghost, args.Args, 2);
      var direction = Helper.ParseDirection(args.Args, 3);
      var delta = direction * size;
      Execute(delta, amount.Min, amount.Max);
    });
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class OverridePlacement {
  public static Vector3? OverridePosition = null;
  public static Quaternion? OverrideRotation = null;
  public static bool Prefix(Player __instance) {
    if (!__instance.m_placementGhost) return true;
    if (OverridePosition.HasValue) __instance.m_placementGhost.transform.position = OverridePosition.Value;
    if (OverrideRotation.HasValue) __instance.m_placementGhost.transform.rotation = OverrideRotation.Value;
    return !OverridePosition.HasValue && !OverrideRotation.HasValue;
  }
}
