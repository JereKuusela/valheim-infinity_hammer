using System;
using UnityEngine;
namespace InfinityHammer;
public class ToolScaling {
  public Vector3 Value = Vector3.one;
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
  public void SetScale(Vector3 value) {
    Value = value;
  }
}

public static class Scaling {
  private static ToolScaling HammerScale = new();
  private static ToolScaling HoeScale = new();
  public static ToolScaling? Get() {
    var player = Helper.GetPlayer();
    if (Hammer.HasTool(player, Tool.Hammer)) return HammerScale;
    if (Hammer.HasTool(player, Tool.Hoe)) return HoeScale;
    return null;
  }
  public static ToolScaling Get(Tool tool) {
    if (tool == Tool.Hammer) return HammerScale;
    if (tool == Tool.Hoe) return HoeScale;
    throw new NotImplementedException();
  }
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
    var scale = Get();
    if (scale == null) return;
    if (Settings.Enabled && ghost && IsScalingSupported())
      ghost.transform.localScale = scale.Value;
  }
  public static void PrintScale(Terminal terminal, Tool tool) {
    if (Settings.DisableScaleMessages) return;
    var scale = Get(tool);
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