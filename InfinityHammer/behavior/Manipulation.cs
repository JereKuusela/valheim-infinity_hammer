
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;
[HarmonyPatch(typeof(ZNetView), "Awake")]
public static class Bounds {
  private static int[] IgnoredLayers = new[] { LayerMask.NameToLayer("character_trigger"), LayerMask.NameToLayer("viewblock"), LayerMask.NameToLayer("pathblocker") };

  public static Dictionary<string, Vector3> Get = new();
  public static void Postfix(ZNetView __instance) {
    var name = __instance.GetPrefabName();
    if (Get.ContainsKey(name)) return;
    if (__instance.transform.rotation != Quaternion.identity) return;
    var colliders = __instance.GetComponentsInChildren<Collider>().Where(collider => !IgnoredLayers.Contains(collider.gameObject.layer)).ToArray();
    if (colliders.Length == 0) {
      Get[name] = Vector3.zero;
      return;
    }
    var bounds = colliders[0].bounds;
    foreach (var collider in colliders)
      bounds.Encapsulate(collider.bounds);
    Get[name] = bounds.size;
  }
}
public static class Scaling {
  public static Vector3 Scale = Vector3.one;
  public static void ScaleUp() {
    Scale *= (1f + Settings.ScaleStep);
  }
  public static void ScaleDown() {
    Scale /= (1f + Settings.ScaleStep);
  }
  public static void SetScale(float value) {
    Scale = value * Vector3.one;
  }
  public static void SetScale(Vector3 value) {
    Scale = value;
  }
  private static bool IsScalingSupported() {
    var player = Helper.GetPlayer();
    var ghost = player.m_placementGhost;
    if (!ghost) return false;
    // Ghost won't have netview so the selected piece must be used.
    // This technically also works for the build window if other mods add scalable objects there.
    var view = player.GetSelectedPiece()?.GetComponent<ZNetView>();
    return view && view != null && view.m_syncInitialScale;
  }
  public static void UpdatePlacement() {
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (Settings.Enabled && ghost && IsScalingSupported())
      ghost.transform.localScale = Scale;
  }
  public static void PrintScale(Terminal terminal) {
    if (Settings.DisableScaleMessages) return;
    if (IsScalingSupported())
      Helper.AddMessage(terminal, $"Scale set to {Scale.y.ToString("P0")}.");
    else
      Helper.AddMessage(terminal, "Selected object doesn't support scaling.");
  }
  public static void SetPieceScale(ZNetView view) {
    if (view && view.m_syncInitialScale)
      view.SetLocalScale(Scale);
  }
}


[HarmonyPatch(typeof(Player), nameof(Player.PieceRayTest))]
public class FreezePlacementMarker {
  static Vector3 CurrentNormal;
  static Vector3 CurrentPoint;
  static bool CurrentSuccess = false;
  static void Postfix(ref Vector3 point, ref Vector3 normal, ref Piece piece, ref Heightmap heightmap, ref Collider waterSurface, ref bool __result) {
    if (__result && Grid.Enabled) {
      point = Grid.Apply(point, heightmap ? Vector3.up : normal);
      if (heightmap) {
        // +2 meters so that floors and small objects will be hit by the collision check.
        point.y = ZoneSystem.instance.GetGroundHeight(point) + 2f;
        if (Physics.Raycast(point, Vector3.down, out var raycastHit, 50f, Player.m_localPlayer.m_placeRayMask)) {
          point = raycastHit.point;
          normal = raycastHit.normal;
        }
      }
    }
    if (Position.Override.HasValue) {
      point = CurrentPoint;
      normal = CurrentNormal;
      __result = CurrentSuccess;
#nullable disable
      piece = null;
      heightmap = null;
      waterSurface = null;
#nullable enable
    } else {
      CurrentPoint = point;
      CurrentNormal = normal;
      CurrentSuccess = __result;
    }
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class OverridePlacementGhost {
  ///<summary>Then override snapping and other modifications for the final result (and some rules are checked too).</summary>
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
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

public static class Grid {
  public static bool Enabled => Precision != 0f;
  private static float Precision;
  private static Vector3 Center;
  public static Vector3 Apply(Vector3 point, Vector3 normal) {
    if (!Enabled) return point;
    var rotation = Quaternion.FromToRotation(Vector3.up, normal);
    point = rotation * point;
    var center = rotation * Center;
    point.x = center.x + Mathf.Round((point.x - center.x) / Precision) * Precision;
    point.z = center.z + Mathf.Round((point.z - center.z) / Precision) * Precision;
    return Quaternion.Inverse(rotation) * point;
  }
  public static void Set(float precision, Vector3 center) {
    if (Precision == precision) {
      Precision = 0f;
    } else {
      Center = center;
      Precision = precision;
    }
  }
}
public static class Position {
  public static Vector3? Override = null;
  public static Vector3 Offset = Vector3.zero;
  public static void ToggleFreeze() {
    if (Override.HasValue)
      Unfreeze();
    else
      Freeze();
  }
  public static void Freeze() {
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return;
    Override = ghost.transform.position;
  }
  public static void Unfreeze() {
    Override = null;
    if (Settings.ResetOffsetOnUnfreeze) Offset = Vector3.zero;
  }
  public static Vector3 Apply(Vector3 point) {
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return point;
    if (Override.HasValue)
      point = Override.Value;
    var rotation = ghost.transform.rotation;
    point += rotation * Vector3.right * Offset.x;
    point += rotation * Vector3.up * Offset.y;
    point += rotation * Vector3.forward * Offset.z;
    return point;
  }
  public static void SetX(float value) {
    Offset.x = value;
  }
  public static void SetY(float value) {
    Offset.y = value;
  }
  public static void SetZ(float value) {
    Offset.z = value;
  }
  public static void MoveLeft(float value) {
    Offset.x -= value;
  }
  public static void MoveRight(float value) {
    Offset.x += value;
  }
  public static void MoveDown(float value) {
    Offset.y -= value;
  }
  public static void MoveUp(float value) {
    Offset.y += value;
  }
  public static void MoveBackward(float value) {
    Offset.z -= value;
  }
  public static void MoveForward(float value) {
    Offset.z += value;
  }
  public static void Set(Vector3 value) {
    Offset = value;
  }
  public static void Move(Vector3 value) {
    Offset += value;
  }

  public static void Print(Terminal terminal) {
    if (Settings.DisableOffsetMessages) return;
    Helper.AddMessage(terminal, $"Offset set to forward: {Offset.z.ToString("F1", CultureInfo.InvariantCulture)}, up: {Offset.y.ToString("F1", CultureInfo.InvariantCulture)}, right: {Offset.x.ToString("F1", CultureInfo.InvariantCulture)}.");
  }
}
public static class Rotating {
  public static void UpdatePlacementRotation(GameObject obj) {
    if (!Settings.CopyRotation) return;
    var player = Helper.GetPlayer();
    var rotation = obj.transform.rotation;
    player.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);
    GizmoWrapper.SetRotation(rotation);
  }
  public static void RotateX(float value) {
    Helper.GetPlayer();
    GizmoWrapper.RotateX(value);
  }
  public static void RotateY(float value) {
    var player = Helper.GetPlayer();
    player.m_placeRotation = Mathf.RoundToInt(((player.m_placeRotation * 22.5f) + value) / 22.5f);
    GizmoWrapper.RotateY(value);
  }
  public static void RotateZ(float value) {
    Helper.GetPlayer();
    GizmoWrapper.RotateZ(value);
  }
}
