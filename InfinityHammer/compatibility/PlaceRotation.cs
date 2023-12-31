using System;
using System.Reflection;
using HarmonyLib;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;
public static class PlaceRotation
{
  public static Assembly? Comfy = null;
  public static Assembly? Reloaded = null;
  public static void Set(GameObject obj)
  {
    var player = Helper.GetPlayer();
    var rotation = obj.transform.rotation;
    player.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);
    if (Comfy != null) ComfySet(obj.GetComponent<Piece>());
    if (Reloaded != null) SetReloaded(obj.transform.rotation);
  }
  public static void Set(Quaternion rotation)
  {
    var player = Helper.GetPlayer();
    player.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);
    if (Comfy == null && Reloaded == null) return;
    var ghost = HammerHelper.GetPlacementGhost();
    if (!ghost) return;
    ghost.transform.rotation = rotation;
    if (Comfy != null) ComfySet(ghost.GetComponent<Piece>());
    if (Reloaded != null) SetReloaded(ghost.transform.rotation);
  }
  public static void RotateX(float rotation)
  {
    Rotate(new Vector3(rotation, 0f, 0f));
  }
  public static void RotateY(float rotation)
  {
    Rotate(new Vector3(0f, rotation, 0f));
  }
  public static void RotateZ(float rotation)
  {
    Rotate(new Vector3(0f, 0f, rotation));
  }
  private static void Rotate(Vector3 rotation) => Rotate(Quaternion.Euler(rotation));
  private static void Rotate(Quaternion rotation)
  {
    var ghost = HammerHelper.GetPlacementGhost();
    if (!ghost) return;
    Set(ghost.transform.rotation * rotation);
  }
  private static Type ComfyType() => Comfy!.GetType("ComfyGizmo.RotationManager");
  private static void ComfySet(Piece piece) => AccessTools.Method(ComfyType(), "MatchPieceRotation").Invoke(null, [piece]);
  private static GameObject GetReloaded()
  {
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
}
