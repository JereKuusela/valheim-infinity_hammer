using System;
using System.Collections.Generic;
using HarmonyLib;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class HammerStackCommand
{
  private static void Execute(GameObject obj, Vector3 delta, Vector3Int min, Vector3Int max)
  {
    Undo.StartCreating();
    var player = Helper.GetPlayer();
    var pos = obj.transform.position;
    var rot = obj.transform.rotation;
    var existingObject = obj != player.m_placementGhost;
    var piece = player.GetSelectedPiece();
    for (var x = min.x; x <= max.x; x++)
    {
      for (var y = min.y; y <= max.y; y++)
      {
        for (var z = min.z; z <= max.z; z++)
        {
          // The first object already exists.
          if (existingObject && x == 0 && y == 0 && z == 0) continue;
          var position = pos;
          position += rot * Vector3.right * x * delta.x;
          position += rot * Vector3.up * y * delta.y;
          position += rot * Vector3.forward * z * delta.z;
          OverridePlacement.OverridePosition = position;
          OverridePlacement.OverrideRotation = rot;
          player.PlacePiece(piece);
        }
      }
    }
    OverridePlacement.OverridePosition = null;
    OverridePlacement.OverrideRotation = null;
    Undo.FinishCreating();
    // For existing objects, nothing was initially selected so makes sense to clear the selection.
    if (existingObject)
      Hammer.Clear();
  }
  private static List<string>? AC(int index)
  {
    if (index == 0) return ParameterInfo.Create("Amount of objects to be placed (number or min-max).");
    if (index == 1) return ParameterInfo.Create("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
    return null;
  }
  private static string Description(string direction) => $"[amount] [step=auto] - Places multiple objects towards the {direction} direction.";
  private static void Command(string name, string description, Func<Range<int>, Range<Vector3Int>> action)
  {
    AutoComplete.Register(name, AC);
    Helper.Command(name, Description(description), (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      Hammer.Equip();
      var target = SelectTarget();
      var amount = ParseValue(args[1]);
      var size = HammerHelper.TryParseSize(target, args.Args, 2);
      var direction = Parse.Direction(args.Args, 3);
      var delta = direction * size;
      var range = action(amount);
      Execute(target, delta, range.Min, range.Max);
    });
  }
  public HammerStackCommand()
  {
    Command("hammer_stack_left", "left", amount => new(new(-amount.Min, 0, 0), new(-amount.Max, 0, 0)));
    Command("hammer_stack_right", "right", amount => new(new(0, amount.Min, 0), new(0, amount.Max, 0)));
    Command("hammer_stack_down", "down", amount => new(new(0, -amount.Min, 0), new(0, -amount.Max, 0)));
    Command("hammer_stack_up", "up", amount => new(new(0, amount.Min, 0), new(0, amount.Max, 0)));
    Command("hammer_stack_backward", "backward", amount => new(new(0, 0, -amount.Min), new(0, 0, -amount.Max)));
    Command("hammer_stack_forward", "forward", amount => new(new(0, 0, amount.Min), new(0, 0, amount.Max)));
    AutoComplete.Register("hammer_stack", (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.FRU("Amounts", subIndex);
      if (index == 1) return ParameterInfo.FRU("Step size (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size)", subIndex);
      return null;
    });
    Helper.Command("hammer_stack", "[forward,up,right] [step=auto,auto,auto] - Places multiple objects next to each other.", (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      Hammer.Equip();
      var target = SelectTarget();
      var amount = ParseValues(args[1]);
      var size = HammerHelper.TryParseSizesZYX(target, args.Args, 2);
      var direction = Parse.Direction(args.Args, 3);
      var delta = direction * size;
      Execute(target, delta, amount.Min, amount.Max);
    });
  }
  private static Range<Vector3Int> ParseValues(string values)
  {
    var split = Parse.Split(values);
    var z = ParseValue(split[0]);
    var x = split.Length > 1 ? ParseValue(split[1]) : new(0);
    var y = split.Length > 2 ? ParseValue(split[2]) : new(0);
    Vector3Int min = new(x.Min, y.Min, z.Min);
    Vector3Int max = new(x.Max, y.Max, z.Max);
    return new(min, max);
  }
  // Default parsing doesn't work because 0 should be the minimum for a single value.
  private static Range<int> ParseValue(string value)
  {
    var split = value.Split('-');
    if (split.Length == 1)
      return new(0, Parse.Int(split[0]));
    return Parse.IntRange(value);
  }
  private static GameObject SelectTarget()
  {
    var player = Helper.GetPlayer();
    if (player.m_placementGhost && player.m_placementGhost.TryGetComponent<Piece>(out var piece) && !piece.m_repairPiece)
      return player.m_placementGhost;
    var hovered = Selector.GetHovered(player, Configuration.Range, [], Configuration.IgnoredIds);
    if (hovered == null) return player.m_placementGhost;
    ObjectSelection sel = new(hovered.Obj, false, null);
    Selection.CreateGhost(sel);
    return hovered.Obj.gameObject;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class OverridePlacement
{
  public static Vector3? OverridePosition = null;
  public static Quaternion? OverrideRotation = null;
  static bool Prefix(Player __instance)
  {
    if (!__instance.m_placementGhost) return true;
    if (OverridePosition.HasValue) __instance.m_placementGhost.transform.position = OverridePosition.Value;
    if (OverrideRotation.HasValue) __instance.m_placementGhost.transform.rotation = OverrideRotation.Value;
    return !OverridePosition.HasValue && !OverrideRotation.HasValue;
  }
}
