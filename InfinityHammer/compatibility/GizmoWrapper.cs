using System;
using System.Reflection;
using UnityEngine;
namespace InfinityHammer;
public static class GizmoWrapper {
  private static Assembly? Comfy = null;
  private static Assembly? Reloaded = null;
  private static BindingFlags PrivateBinding = BindingFlags.Static | BindingFlags.NonPublic;
  private static Type ComfyType() => Comfy!.GetType("Gizmo.ComfyGizmo");
  private static object Get(string field) => ComfyType().GetField(field, PrivateBinding).GetValue(null);
  private static void SetField(string field, int value) => ComfyType().GetField(field, PrivateBinding).SetValue(null, value);
  public static void InitComfy(Assembly assembly) {
    Comfy = assembly;
  }
  public static void InitReloaded(Assembly assembly) {
    Reloaded = assembly;
  }
  static bool Prefix() => !Selection.IsCommand();
  private static void SetComfyRotation(Quaternion rotation) {
    if (Comfy == null) return;
    var euler = rotation.eulerAngles;
    SetField("_xRot", ComfySnap(euler.x));
    SetField("_yRot", ComfySnap(euler.y));
    SetField("_zRot", ComfySnap(euler.z));
    Player.m_localPlayer.UpdatePlacement(true, 0f);
  }
  private static void ComfyRotationX(float rotation, int previous = 0) {
    if (Comfy == null) return;
    SetField("_xRot", previous + ComfySnap(rotation));
    Player.m_localPlayer.UpdatePlacement(true, 0f);
  }
  private static void ComfyRotationY(float rotation, int previous = 0) {
    if (Comfy == null) return;
    SetField("_yRot", previous + ComfySnap(rotation));
    Player.m_localPlayer.UpdatePlacement(true, 0f);
  }
  private static void ComfyRotationZ(float rotation, int previous = 0) {
    if (Comfy == null) return;
    ComfyType().GetField("_zRot", PrivateBinding).SetValue(null, previous + ComfySnap(rotation));
    Player.m_localPlayer.UpdatePlacement(true, 0f);
  }
  private static void ComfyRotateX(float rotation) {
    if (Comfy == null) return;
    var previous = (int)Get("_xRot");
    ComfyRotationX(rotation, previous);
    Player.m_localPlayer.UpdatePlacement(true, 0f);
  }
  private static void ComfyRotateY(float rotation) {
    if (Comfy == null) return;
    var previous = (int)Get("_yRot");
    ComfyRotationY(rotation, previous);
    Player.m_localPlayer.UpdatePlacement(true, 0f);
  }
  private static void ComfyRotateZ(float rotation) {
    if (Comfy == null) return;
    var previous = (int)Get("_zRot");
    ComfyRotationZ(rotation, previous);
    Player.m_localPlayer.UpdatePlacement(true, 0f);
  }
  private static int ComfySnap(float rotation) {
    if (Comfy == null) return 0;
    var snapAngle = (float)ComfyType().GetField("_snapAngle", PrivateBinding).GetValue(null);
    return (int)Math.Round(rotation / snapAngle);
  }
  private static void ReloadedRotateX(float rotation) {
    var gizmo = GetReloaded();
    if (!gizmo) return;
    var rot = gizmo.transform.rotation.eulerAngles;
    rot.x += rotation;
    gizmo.transform.rotation = Quaternion.Euler(rot);
  }
  private static void ReloadedRotateY(float rotation) {
    var gizmo = GetReloaded();
    if (!gizmo) return;
    var rot = gizmo.transform.rotation.eulerAngles;
    rot.y += rotation;
    gizmo.transform.rotation = Quaternion.Euler(rot);
  }
  private static void ReloadedRotateZ(float rotation) {
    var gizmo = GetReloaded();
    if (!gizmo) return;
    var rot = gizmo.transform.rotation.eulerAngles;
    rot.z += rotation;
    gizmo.transform.rotation = Quaternion.Euler(rot);
  }
  private static GameObject GetReloaded() {
#nullable disable
    if (Reloaded == null) return null;
#nullable enable
    var gizmo = GameObject.Find("GizmoRoot(Clone)");
    if (gizmo) return gizmo;
    var player = Player.m_localPlayer;
    if (player) {
      // Gizmo needs these to ensure that it is initialized properly.
      player.UpdatePlacementGhost(false);
      player.UpdatePlacement(false, 0);
    }
    return GameObject.Find("GizmoRoot(Clone)");
  }
  private static void SetReloadedRotation(Quaternion rotation) {
    var gizmo = GetReloaded();
    if (gizmo) gizmo.transform.rotation = rotation;
  }
  public static void SetRotation(Quaternion rotation) {
    SetComfyRotation(rotation);
    SetReloadedRotation(rotation);
  }
  public static void RotateX(float rotation) {
    ComfyRotateX(rotation);
    ReloadedRotateX(rotation);
  }
  public static void RotateY(float rotation) {
    ComfyRotateY(rotation);
    ReloadedRotateY(rotation);
  }
  public static void RotateZ(float rotation) {
    ComfyRotateZ(rotation);
    ReloadedRotateZ(rotation);
  }
}
