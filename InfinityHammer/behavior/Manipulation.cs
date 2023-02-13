
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;


[HarmonyPatch(typeof(Player), nameof(Player.FindClosestSnapPoints))]
public class DisableSnapsWhenFrozen
{
  static bool Prefix() => !Position.Override.HasValue;
}

[HarmonyPatch(typeof(Player), nameof(Player.PieceRayTest))]
public class FreezePlacementMarker
{
  static Vector3 CurrentNormal = Vector3.up;
  static void Postfix(ref Vector3 point, ref Vector3 normal, ref Piece piece, ref Heightmap heightmap, ref Collider waterSurface, ref bool __result)
  {
    if (__result && Grid.Enabled)
    {
      point = Grid.Apply(point, heightmap ? Vector3.up : normal);
      if (heightmap)
      {
        // +2 meters so that floors and small objects will be hit by the collision check.
        point.y = ZoneSystem.instance.GetGroundHeight(point) + 2f;
        if (Physics.Raycast(point, Vector3.down, out var raycastHit, 50f, Player.m_localPlayer.m_placeRayMask))
        {
          point = raycastHit.point;
          normal = raycastHit.normal;
        }
      }
    }
    if (Position.Override.HasValue)
    {
      point = Position.Override.Value;
      normal = CurrentNormal;
      __result = true;
#nullable disable
      piece = null;
      heightmap = null;
      waterSurface = null;
#nullable enable
    }
    else
    {
      CurrentNormal = normal;
    }
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class OverridePlacementGhost
{
  ///<summary>Then override snapping and other modifications for the final result (and some rules are checked too).</summary>
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Call,
                  AccessTools.Method(typeof(Location), nameof(Location.IsInsideNoBuildLocation))))
          .Advance(-2)
          // If-branches require using ops from the IsInsideBuildLocation so just duplicate the used ops afterwards.
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate<Action<GameObject>>(
                 (GameObject ghost) => ghost.transform.position = Position.Apply(ghost.transform.position)).operand),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Player), nameof(Player.m_placementGhost)))
          )
          .InstructionEnumeration();
  }
}

public static class Grid
{
  public static bool Enabled => Precision != 0f;
  private static float Precision;
  private static Vector3 Center;
  public static Vector3 Apply(Vector3 point, Vector3 normal)
  {
    if (!Enabled) return point;
    var rotation = Quaternion.FromToRotation(Vector3.up, normal);
    point = rotation * point;
    var center = rotation * Center;
    point.x = center.x + Mathf.Round((point.x - center.x) / Precision) * Precision;
    point.z = center.z + Mathf.Round((point.z - center.z) / Precision) * Precision;
    return Quaternion.Inverse(rotation) * point;
  }
  public static void Toggle(float precision, Vector3 center)
  {
    if (Precision == precision) Precision = 0f;
    else Set(precision, center);
  }
  public static void Set(float precision, Vector3 center)
  {
    Center = center;
    Precision = precision;
  }
}
public static class Position
{
  public static Vector3? Override = null;
  public static Vector3 Offset = Vector3.zero;
  public static void ToggleFreeze()
  {
    if (Override.HasValue)
      Unfreeze();
    else
      Freeze();
  }
  public static void Freeze(Vector3 position)
  {
    Override = position;
  }
  public static void Freeze()
  {
    var player = Helper.GetPlayer();
    var ghost = player.m_placementGhost;
    Override = ghost ? Deapply(ghost.transform.position) : player.transform.position;
  }
  public static void Unfreeze()
  {
    Override = null;
    if (Configuration.ResetOffsetOnUnfreeze) Offset = Vector3.zero;
  }
  public static Vector3 Apply(Vector3 point)
  {
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return point;
    if (Override.HasValue)
      point = Override.Value;
    var rotation = ghost.transform.rotation;
    if (Configuration.PreciseCommands && Selection.IsCommand())
      rotation = Quaternion.identity;
    point += rotation * Vector3.right * Offset.x;
    point += rotation * Vector3.up * Offset.y;
    point += rotation * Vector3.forward * Offset.z;
    return point;
  }
  public static Vector3 Deapply(Vector3 point)
  {
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return point;
    if (Override.HasValue)
      point = Override.Value;
    var rotation = ghost.transform.rotation;
    if (Configuration.PreciseCommands && Selection.IsCommand())
      rotation = Quaternion.identity;
    point -= rotation * Vector3.right * Offset.x;
    point -= rotation * Vector3.up * Offset.y;
    point -= rotation * Vector3.forward * Offset.z;
    return point;
  }
  public static void SetX(float value)
  {
    Offset.x = value;
  }
  public static void SetY(float value)
  {
    Offset.y = value;
  }
  public static void SetZ(float value)
  {
    Offset.z = value;
  }
  public static void MoveLeft(float value)
  {
    if (Configuration.PreciseCommands && Selection.IsCommand()) value = Mathf.Max(value, 1f);
    Offset.x -= value;
  }
  public static void MoveRight(float value)
  {
    if (Configuration.PreciseCommands && Selection.IsCommand()) value = Mathf.Max(value, 1f);
    Offset.x += value;
  }
  public static void MoveDown(float value)
  {
    Offset.y -= value;
  }
  public static void MoveUp(float value)
  {
    Offset.y += value;
  }
  public static void MoveBackward(float value)
  {
    if (Configuration.PreciseCommands && Selection.IsCommand()) value = Mathf.Max(value, 1f);
    Offset.z -= value;
  }
  public static void MoveForward(float value)
  {
    if (Configuration.PreciseCommands && Selection.IsCommand()) value = Mathf.Max(value, 1f);
    Offset.z += value;
  }
  public static void Set(Vector3 value)
  {
    Offset = value;
  }
  public static void Move(Vector3 value)
  {
    Offset += value;
  }

  public static void Print(Terminal terminal)
  {
    if (Configuration.DisableOffsetMessages) return;
    Helper.AddMessage(terminal, $"Offset set to forward: {Offset.z.ToString("F1", CultureInfo.InvariantCulture)}, up: {Offset.y.ToString("F1", CultureInfo.InvariantCulture)}, right: {Offset.x.ToString("F1", CultureInfo.InvariantCulture)}.");
  }
}
public static class Rotating
{
  public static void UpdatePlacementRotation(GameObject obj)
  {
    var player = Helper.GetPlayer();
    var rotation = obj.transform.rotation;
    player.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);
    GizmoWrapper.SetRotation(rotation);
  }
  public static void RotateX(float value)
  {
    Helper.GetPlayer();
    GizmoWrapper.RotateX(value);
  }
  public static void RotateY(float value)
  {
    var player = Helper.GetPlayer();
    player.m_placeRotation = Mathf.RoundToInt(((player.m_placeRotation * 22.5f) + value) / 22.5f);
    GizmoWrapper.RotateY(value);
  }
  public static void RotateZ(float value)
  {
    Helper.GetPlayer();
    GizmoWrapper.RotateZ(value);
  }
}
