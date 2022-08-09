using System.Linq;
using UnityEngine;
namespace InfinityHammer;
public class ToolScaling {
  private bool SanityY;
  public ToolScaling(bool sanityY) {
    SanityY = sanityY;
  }
  public Vector3 Value = Vector3.one;
  public float X => Value.x;
  public float Y => Value.y;
  public float Z => Value.z;
  private void Sanity() {
    Value.x = Mathf.Max(0f, Value.x);
    if (SanityY)
      Value.y = Mathf.Max(0f, Value.y);
    Value.z = Mathf.Max(0f, Value.z);
  }
  public void Zoom(float amount, float percentage) {
    Value += new Vector3(amount, amount, amount);
    if (percentage < 0f) Value /= (1f - percentage);
    else Value *= (1f + percentage);
    Sanity();
  }
  public void ZoomX(float amount, float percentage) {
    Value.x += amount;
    if (percentage < 0f) Value.x /= (1f - percentage);
    else Value.x *= (1f + percentage);
    Sanity();
  }
  public void ZoomY(float amount, float percentage) {
    Value.y += amount;
    if (percentage < 0f) Value.y /= (1f - percentage);
    else Value.y *= (1f + percentage);
    Sanity();
  }

  public void ZoomZ(float amount, float percentage) {
    Value.z += amount;
    if (percentage < 0f) Value.z /= (1f - percentage);
    else Value.z *= (1f + percentage);
    Sanity();
  }
  public void SetScale(float value) {
    Value = value * Vector3.one;
    Sanity();
  }
  public void SetScaleX(float value) {
    Value.x = value;
    Sanity();
  }
  public void SetScaleY(float value) {
    Value.y = value;
    Sanity();
  }
  public void SetScaleZ(float value) {
    Value.z = value;
    Sanity();
  }
  public void SetScale(Vector3 value) {
    Value = value;
    Sanity();
  }
}

public static class Scaling {
  public static ToolScaling Build = new(true);
  public static ToolScaling Command = new(false);
  public static ToolScaling Get() => Selection.IsCommand() ? Scaling.Command : Scaling.Build;
  private static bool IsScalingSupported() {
    var player = Helper.GetPlayer();
    var ghost = player.m_placementGhost;
    if (!ghost) return false;
    if (Selection.Type == SelectedType.Command) return true;
    if (Selection.Type == SelectedType.Multiple) return Selection.Objects.All(obj => obj.Scalable);
    // Ghost won't have netview so the selected piece must be used.
    // This technically also works for the build window if other mods add scalable objects there.
    var view = player.GetSelectedPiece()?.GetComponent<ZNetView>();
    return view && view != null && view.m_syncInitialScale;
  }
  public static void Set(GameObject ghost) {
    if (Configuration.Enabled && ghost && IsScalingSupported() && Selection.Type != SelectedType.Command)
      ghost.transform.localScale = Build.Value;
  }
  public static void PrintScale(Terminal terminal) {
    if (Configuration.DisableScaleMessages) return;
    var scaling = Get();
    if (IsScalingSupported()) {
      if (scaling.X != scaling.Y || scaling.X != scaling.Z)
        Helper.AddMessage(terminal, $"Scale set to X: {scaling.X.ToString("P0")}, Z: {scaling.Z.ToString("P0")}, Y: {scaling.Y.ToString("P0")}.");
      else
        Helper.AddMessage(terminal, $"Scale set to {scaling.Y.ToString("P0")}.");

    } else
      Helper.AddMessage(terminal, "Selected object doesn't support scaling.");
  }
  public static void SetPieceScale(ZNetView view, GameObject ghost) {
    if (view && view.m_syncInitialScale)
      view.SetLocalScale(ghost.transform.localScale);
  }
}