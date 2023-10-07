using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;
public static class GizmoWrapper
{
  private static Assembly? Comfy = null;
  private static Assembly? Reloaded = null;
  private static Type ComfyType() => Comfy!.GetType("Gizmo.ComfyGizmo");
  private static Vector3 ComfyGet() => (Vector3)AccessTools.Field(ComfyType(), "EulerAngles").GetValue(null);
  private static void ComfySet(Vector3 angles)
  {
    AccessTools.Field(ComfyType(), "EulerAngles").SetValue(null, angles);
    AccessTools.Method(ComfyType(), "Rotate").Invoke(null, new object[0]);
  }
  public static void InitComfy(Assembly assembly)
  {
    Comfy = assembly;
  }
  public static void InitReloaded(Assembly assembly)
  {
    Reloaded = assembly;
  }
  private static void ComfyRotation(Quaternion rotation)
  {
    if (Comfy == null) return;
    var euler = rotation.eulerAngles;
    ComfySet(euler);
  }
  private static void ComfyRotate(Vector3 rotation)
  {
    if (Comfy == null) return;
    var angles = ComfyGet();
    angles += rotation;
    ComfySet(angles);
  }
  private static void ComfyRotateX(float rotation) => ComfyRotate(new(rotation, 0f, 0f));
  private static void ComfyRotateY(float rotation) => ComfyRotate(new(0f, rotation, 0f));
  private static void ComfyRotateZ(float rotation) => ComfyRotate(new(0f, 0f, rotation));
  private static void ReloadedRotateX(float rotation)
  {
    var gizmo = GetReloaded();
    if (!gizmo) return;
    var rot = gizmo.transform.rotation.eulerAngles;
    rot.x += rotation;
    gizmo.transform.rotation = Quaternion.Euler(rot);
  }
  private static void ReloadedRotateY(float rotation)
  {
    var gizmo = GetReloaded();
    if (!gizmo) return;
    var rot = gizmo.transform.rotation.eulerAngles;
    rot.y += rotation;
    gizmo.transform.rotation = Quaternion.Euler(rot);
  }
  private static void ReloadedRotateZ(float rotation)
  {
    var gizmo = GetReloaded();
    if (!gizmo) return;
    var rot = gizmo.transform.rotation.eulerAngles;
    rot.z += rotation;
    gizmo.transform.rotation = Quaternion.Euler(rot);
  }
  private static GameObject GetReloaded()
  {
#nullable disable
    if (Reloaded == null) return null;
#nullable enable
    var gizmo = GameObject.Find("GizmoRoot(Clone)");
    if (gizmo) return gizmo;
    var player = Player.m_localPlayer;
    if (player)
    {
      // Gizmo needs these to ensure that it is initialized properly.
      player.UpdatePlacementGhost(false);
      player.UpdatePlacement(false, 0);
    }
    return GameObject.Find("GizmoRoot(Clone)");
  }
  private static void SetReloaded(Quaternion rotation)
  {
    var gizmo = GetReloaded();
    if (gizmo) gizmo.transform.rotation = rotation;
  }
  public static void Set(Quaternion rotation)
  {
    ComfyRotation(rotation);
    SetReloaded(rotation);
  }
  public static void RotateX(float rotation)
  {
    ComfyRotateX(rotation);
    ReloadedRotateX(rotation);
  }
  public static void RotateY(float rotation)
  {
    ComfyRotateY(rotation);
    ReloadedRotateY(rotation);
  }
  public static void RotateZ(float rotation)
  {
    ComfyRotateZ(rotation);
    ReloadedRotateZ(rotation);
  }
}
