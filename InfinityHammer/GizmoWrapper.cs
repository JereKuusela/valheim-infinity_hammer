using System;
using System.Reflection;
using UnityEngine;
namespace InfinityHammer;
public static class GizmoWrapper {
  private static Assembly Comfy = null;
  private static BindingFlags PrivateBinding = BindingFlags.Static | BindingFlags.NonPublic;
  private static Type Type() => Comfy.GetType("Gizmo.ComfyGizmo");
  private static object Get(string field) => Type().GetField(field, PrivateBinding).GetValue(null);
  private static void SetField(string field, int value) => Type().GetField(field, PrivateBinding).SetValue(null, value);
  public static void InitComfy(Assembly assembly) {
    Comfy = assembly;
  }
  private static void SetComfyRotation(Quaternion rotation) {
    if (Comfy == null) return;
    var euler = rotation.eulerAngles;
    SetField("_xRot", ComfySnap(euler.x));
    SetField("_yRot", ComfySnap(euler.y));
    SetField("_zRot", ComfySnap(euler.z));
  }
  private static void ComfyRotationX(float rotation, int previous = 0) {
    if (Comfy == null) return;
    SetField("_xRot", previous + ComfySnap(rotation));
  }
  private static void ComfyRotationY(float rotation, int previous = 0) {
    if (Comfy == null) return;
    SetField("_yRot", previous + ComfySnap(rotation));
  }
  private static void ComfyRotationZ(float rotation, int previous = 0) {
    if (Comfy == null) return;
    Type().GetField("_zRot", PrivateBinding).SetValue(null, previous + ComfySnap(rotation));
  }
  private static void ComfyRotateX(float rotation) {
    if (Comfy == null) return;
    var previous = (int)Get("_xRot");
    ComfyRotationX(rotation, previous);
  }
  private static void ComfyRotateY(float rotation) {
    if (Comfy == null) return;
    var previous = (int)Get("_yRot");
    ComfyRotationY(rotation, previous);
  }
  private static void ComfyRotateZ(float rotation) {
    if (Comfy == null) return;
    var previous = (int)Get("_zRot");
    ComfyRotationZ(rotation, previous);
  }
  private static int ComfySnap(float rotation) {
    if (Comfy == null) return 0;
    var snapAngle = (float)Type().GetField("_snapAngle", PrivateBinding).GetValue(null);
    return (int)Math.Round(rotation / snapAngle);
  }
  // Reloaded currently doesn't work so not in use.
  private static void SetReloadedRotation(Player player, Quaternion rotation) {
    var gizmo = GameObject.Find("GizmoRoot(Clone)");
    if (!gizmo) {
      // Gizmo needs these to ensure that it is initialized properly.
      player.UpdatePlacementGhost(false);
      player.UpdatePlacement(false, 0);
    }
    gizmo = GameObject.Find("GizmoRoot(Clone)");
    if (gizmo)
      gizmo.transform.rotation = rotation;
  }
  public static void SetRotation(Quaternion rotation) {
    SetComfyRotation(rotation);
  }
  public static void RotateX(float rotation) {
    ComfyRotateX(rotation);
  }
  public static void RotateY(float rotation) {
    ComfyRotateY(rotation);
  }
  public static void RotateZ(float rotation) {
    ComfyRotateZ(rotation);
  }
}
