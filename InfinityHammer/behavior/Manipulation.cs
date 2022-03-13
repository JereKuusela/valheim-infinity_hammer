
using UnityEngine;
namespace InfinityHammer {

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
    public static void MoveX(float value) {
      Value.x += value;
    }
    public static void MoveY(float value) {
      Value.y += value;
    }
    public static void MoveZ(float value) {
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
      ghost.transform.position += rotation * Vector3.forward * Value.x;
      ghost.transform.position += rotation * Vector3.right * Value.z;
      ghost.transform.position += rotation * Vector3.up * Value.y;
    }
    public static void Print(Terminal terminal) {
      Helper.AddMessage(terminal, $"Offset set to {Value.ToString("F1")}.");
    }
  }
  public static class Rotating {
    public static void UpdatePlacementRotation(GameObject obj) {
      if (!Settings.CopyRotation) return;
      var player = Player.m_localPlayer;
      if (!player) return;
      var rotation = obj.transform.rotation;
      player.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);

      var gizmo = GameObject.Find("GizmoRoot(Clone)");
      if (!gizmo) {
        // Gizmo needs these to ensure that it is initialized properly.
        player.UpdatePlacementGhost(false);
        player.UpdatePlacement(false, 0);
      }
      gizmo = GameObject.Find("GizmoRoot(Clone)");
      if (gizmo) {
        if (InfinityHammer.IsComfyGizmo) gizmo.transform.localRotation = rotation;
        else gizmo.transform.rotation = rotation;
      }
    }
  }
}