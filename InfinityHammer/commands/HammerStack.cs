using System;
using System.Collections.Generic;
using HarmonyLib;
using Service;
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
  private static List<string>? AutoComplete(int index) {
    if (index == 0) return CommandWrapper.Info("Amount of objects to be placed (number or min-max).");
    if (index == 1) return CommandWrapper.Info("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
    return null;
  }
  private static string Description(string direction) => $"[amount] [step=auto] - Places multiple objects towards the {direction} direction.";
  private static void Command(string name, string description, Func<Vector3, float, Vector3> action) {
    CommandWrapper.Register(name, AutoComplete);
    Helper.Command(name, Description(description), (args) => {
      Helper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      var ghost = Helper.GetPlacementGhost();
      var amount = Helper.ParseIntRange(args[1]);
      var size = Helper.TryParseSize(ghost, args.Args, 2);
      var direction = Parse.Direction(args.Args, 3);
      var delta = action(size, direction);
      Execute(delta, new(amount.Min, 0, 0), new(amount.Max, 0, 0));
    });
  }
  public HammerStackCommand() {
    Command("hammer_stack_left", "left", (size, direction) => new(-direction * size.x, 0f, 0f));
    Command("hammer_stack_right", "right", (size, direction) => new(direction * size.x, 0f, 0f));
    Command("hammer_stack_down", "down", (size, direction) => new(0f, -direction * size.y, 0f));
    Command("hammer_stack_up", "up", (size, direction) => new(0f, direction * size.y, 0f));
    Command("hammer_stack_backward", "backward", (size, direction) => new(0f, 0f, -direction * size.z));
    Command("hammer_stack_forward", "forward", (size, direction) => new(0f, 0f, direction * size.z));
    CommandWrapper.Register("hammer_stack", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.FRU("Amounts", subIndex);
      if (index == 1) return CommandWrapper.FRU("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size)", subIndex);
      return null;
    });
    Helper.Command("hammer_stack", "[forward,up,right] [step=auto,auto,auto] - Places multiple objects next to each other.", (args) => {
      Helper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      var ghost = Helper.GetPlacementGhost();
      var amount = Helper.ParseZYXRange(args[1]);
      var size = Helper.TryParseSizesZYX(ghost, args.Args, 2);
      var direction = Parse.Direction(args.Args, 3);
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
