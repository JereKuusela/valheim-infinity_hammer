
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    var ghost = Player.m_localPlayer?.m_placementGhost;
    if (!ghost) return false;
    // Ghost won't have netview so the selected piece must be used.
    // This technically also works for the build window if other mods add scalable objects there.
    var view = Player.m_localPlayer?.GetSelectedPiece()?.GetComponent<ZNetView>();
    return view && view.m_syncInitialScale;
  }
  public static void UpdatePlacement() {
    var ghost = Player.m_localPlayer?.m_placementGhost;
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
  public static void SetPieceScale(Piece obj) {
    var view = obj.m_nview;
    if (view && view.m_syncInitialScale)
      view.SetLocalScale(Scale);
  }
}

public static class Offset {
  public static Vector3 Value = Vector3.zero;
  public static void SetX(float value) {
    Value.x = value;
  }
  public static void SetY(float value) {
    Value.y = value;
  }
  public static void SetZ(float value) {
    Value.z = value;
  }
  public static void MoveLeft(float value) {
    Value.x -= value;
  }
  public static void MoveRight(float value) {
    Value.x += value;
  }
  public static void MoveDown(float value) {
    Value.y -= value;
  }
  public static void MoveUp(float value) {
    Value.y += value;
  }
  public static void MoveBackward(float value) {
    Value.z -= value;
  }
  public static void MoveForward(float value) {
    Value.z += value;
  }
  public static void Set(Vector3 value) {
    Value = value;
  }
  public static void Move(Vector3 value) {
    Value += value;
  }
  public static void UpdatePlacement() {
    var ghost = Player.m_localPlayer?.m_placementGhost;
    if (!ghost) return;
    var rotation = ghost.transform.rotation;
    ghost.transform.position += rotation * Vector3.right * Value.x;
    ghost.transform.position += rotation * Vector3.up * Value.y;
    ghost.transform.position += rotation * Vector3.forward * Value.z;
  }
  public static void Print(Terminal terminal) {
    if (Settings.DisableOffsetMessages) return;
    Helper.AddMessage(terminal, $"Offset set to forward: {Value.z.ToString("F1", CultureInfo.InvariantCulture)}, up: {Value.y.ToString("F1", CultureInfo.InvariantCulture)}, right: {Value.x.ToString("F1", CultureInfo.InvariantCulture)}.");
  }
}
public static class Rotating {
  public static void UpdatePlacementRotation(GameObject obj) {
    if (!Settings.CopyRotation) return;
    var player = Player.m_localPlayer;
    if (!player) return;
    var rotation = obj.transform.rotation;
    player.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);
    GizmoWrapper.SetRotation(rotation);
  }
  public static void RotateX(float value) {
    var player = Player.m_localPlayer;
    if (!player) return;
    GizmoWrapper.RotateX(value);
  }
  public static void RotateY(float value) {
    var player = Player.m_localPlayer;
    if (!player) return;
    player.m_placeRotation = Mathf.RoundToInt(((player.m_placeRotation * 22.5f) + value) / 22.5f);
    GizmoWrapper.RotateY(value);
  }
  public static void RotateZ(float value) {
    var player = Player.m_localPlayer;
    if (!player) return;
    GizmoWrapper.RotateZ(value);
  }
}
