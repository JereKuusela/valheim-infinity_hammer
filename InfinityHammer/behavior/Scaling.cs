using UnityEngine;
namespace InfinityHammer;
public class ToolScaling {
  public Vector3 Value = Vector3.one;
  public float X => Value.x;
  public float Y => Value.y;
  public float Z => Value.z;
  public void Scale(float amount, float percentage) {
    Value += new Vector3(amount, amount, amount);
    if (percentage < 0f) Value /= (1f - percentage);
    else Value *= (1f + percentage);
  }
  public void ScaleX(float amount, float percentage) {
    Value.x += amount;
    if (percentage < 0f) Value.x /= (1f - percentage);
    else Value.x *= (1f + percentage);
  }
  public void ScaleY(float amount, float percentage) {
    Value.y += amount;
    if (percentage < 0f) Value.y /= (1f - percentage);
    else Value.y *= (1f + percentage);
  }

  public void ScaleZ(float amount, float percentage) {
    Value.z += amount;
    if (percentage < 0f) Value.z /= (1f - percentage);
    else Value.z *= (1f + percentage);
  }
  public void SetScale(float value) {
    Value = value * Vector3.one;
  }
  public void SetScaleX(float value) {
    Value.x = value;
  }
  public void SetScaleY(float value) {
    Value.y = value;
  }
  public void SetScaleZ(float value) {
    Value.z = value;
  }
  public void SetScale(Vector3 value) {
    Value = value;
  }
}

public static class Scaling {
  public static ToolScaling Build = new();
  public static ToolScaling Command = new();
  public static ToolScaling Get() => Selection.Type == SelectionType.Command ? Scaling.Command : Scaling.Build;
  private static bool IsScalingSupported() {
    var player = Helper.GetPlayer();
    var ghost = player.m_placementGhost;
    if (!ghost) return false;
    if (Selection.Type == SelectionType.Command) return true;
    // Ghost won't have netview so the selected piece must be used.
    // This technically also works for the build window if other mods add scalable objects there.
    var view = player.GetSelectedPiece()?.GetComponent<ZNetView>();
    return view && view != null && view.m_syncInitialScale;
  }
  public static void Set(GameObject ghost) {
    if (Configuration.Enabled && ghost && IsScalingSupported())
      ghost.transform.localScale = Build.Value; ;
  }
  public static void PrintScale(Terminal terminal) {
    if (Configuration.DisableScaleMessages) return;
    var scale = Get();
    if (IsScalingSupported())
      Helper.AddMessage(terminal, $"Scale set to {scale.Value.y.ToString("P0")}.");
    else
      Helper.AddMessage(terminal, "Selected object doesn't support scaling.");
  }
  public static void SetPieceScale(ZNetView view, GameObject ghost) {
    if (view && view.m_syncInitialScale)
      view.SetLocalScale(ghost.transform.localScale);
  }
}