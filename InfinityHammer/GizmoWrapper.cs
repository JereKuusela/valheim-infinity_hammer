using System;
using System.Reflection;
using UnityEngine;

namespace InfinityHammer {
  public static class GizmoWrapper {
    private static Assembly Comfy = null;
    private static BindingFlags PrivateBinding = BindingFlags.Static | BindingFlags.NonPublic;
    private static Type Type() => Comfy.GetType("Gizmo.ComfyGizmo");
    public static void InitComfy(Assembly assembly) {
      Comfy = assembly;
    }
    private static void SetComfyRotation(Quaternion rotation) {
      if (Comfy == null) return;
      var euler = rotation.eulerAngles;
      var snapAngle = (float)Type().GetField("_snapAngle", PrivateBinding).GetValue(null);
      Type().GetField("_xRot", PrivateBinding).SetValue(null, (int)Math.Round(euler.x / snapAngle));
      Type().GetField("_yRot", PrivateBinding).SetValue(null, (int)Math.Round(euler.y / snapAngle));
      Type().GetField("_zRot", PrivateBinding).SetValue(null, (int)Math.Round(euler.z / snapAngle));
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
    public static void SetRotation(Player player, Quaternion rotation) {
      SetComfyRotation(rotation);
    }
  }
}
