using System;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;

public class Scaling()
{
  private static readonly ScalingData PieceScaling = new(true, false, true, Vector3.one);
  private static readonly ScalingData ToolScaling = new(false, true, false, new(10f, 0f, 10f));
  public static void Set(Vector3 value) => Get().SetScale(value);
  public static void Set(GameObject value) => Get().SetScale(value.transform.localScale);
  public static void Print(Terminal terminal) => Get().Print(terminal);
  public static ScalingData Get() => Selection.Get().IsTool ? ToolScaling : PieceScaling;
  public static ScalingData Get(bool tool) => tool ? ToolScaling : PieceScaling;
  public static Vector3 Value => Get().Vec3;
}


public class ScalingData(bool sanityY, bool minXZ, bool printChanges, Vector3 value)
{
  private readonly bool OnlyPositiveHeight = sanityY;
  private readonly bool MinXZ = minXZ;
  private Vector3 Value = value;
  // Unsnapped accumulator so repeated zooming doesn't lose precision to snapping.
  private Vector3 RawValue = value;
  public Vector3 Vec3 => Value;
  private readonly bool PrintChanges = printChanges;
  public float X => MinXZ ? Mathf.Max(0.25f, Value.x) : Value.x;
  public float Y => Value.y;
  public float Z => MinXZ ? Mathf.Max(0.25f, Value.z) : Value.z;
  public void Print(Terminal terminal)
  {
    if (!PrintChanges) return;
    if (Configuration.DisableScaleMessages) return;
    if (X != Y || X != Z)
      HammerHelper.Message(terminal, $"Scale set to X: {X:P0}, Z: {Z:P0}, Y: {Y:P0}.");
    else
      HammerHelper.Message(terminal, $"Scale set to {Y:P0}.");
  }
  public void SetPrecisionXZ(float min, float precision)
  {
    RawValue.x = min + precision * Mathf.Floor((RawValue.x - min) / precision);
    RawValue.z = min + precision * Mathf.Floor((RawValue.z - min) / precision);
    Value.x = RawValue.x;
    Value.z = RawValue.z;
    AfterScaling();
  }
  public void Zoom(float amount) => TryCommit(RawValue + new Vector3(amount, amount, amount));
  public void ZoomPercentage(float percentage) => TryCommitPercentage(RawValue * (1f + percentage));
  public void ZoomX(float amount) => TryCommit(RawValue + new Vector3(amount, 0f, 0f));
  public void ZoomXPercentage(float percentage) => TryCommitPercentage(new(RawValue.x * (1f + percentage), RawValue.y, RawValue.z));
  public void ZoomY(float amount) => TryCommit(RawValue + new Vector3(0f, amount, 0f));
  public void ZoomYPercentage(float percentage) => TryCommitPercentage(new(RawValue.x, RawValue.y * (1f + percentage), RawValue.z));
  public void ZoomZ(float amount) => TryCommit(RawValue + new Vector3(0f, 0f, amount));
  public void ZoomZPercentage(float percentage) => TryCommitPercentage(new(RawValue.x, RawValue.y, RawValue.z * (1f + percentage)));
  private void TryCommit(Vector3 candidate)
  {
    if (!IsValid(candidate)) return;
    RawValue = candidate;
    Value = candidate;
    AfterScaling();
  }
  private void TryCommitPercentage(Vector3 candidate)
  {
    // Percentage is limited at precision, must reject if already at limit to avoid loss of uniformal scaling.
    if ((candidate.x < RawValue.x && AtLimit(Value.x)) ||
        (candidate.y < RawValue.y && AtLimit(Value.y)) ||
        (candidate.z < RawValue.z && AtLimit(Value.z))) return;

    if (!IsValid(candidate)) return;
    RawValue = candidate;
    Value = SnapToPrecision(candidate);
    AfterScaling();
  }
  private bool IsValid(Vector3 value)
  {
    if (value.x <= 0f || value.z <= 0f) return false;
    return !OnlyPositiveHeight || value.y > 0f;
  }
  private bool AtLimit(float value)
  {
    var precision = Configuration.ScalePrecision;
    if (precision <= 0f) return false;
    return OnlyPositiveHeight ? value <= precision : Mathf.Abs(value) <= precision;
  }
  private static float SnapToPrecision(float value)
  {
    var precision = Configuration.ScalePrecision;
    if (precision <= 0f) return value;
    var sign = Mathf.Sign(value);
    return sign * Mathf.Max(precision, Mathf.Round(Mathf.Abs(value) / precision) * precision);
  }
  private static Vector3 SnapToPrecision(Vector3 value) => new(
    SnapToPrecision(value.x),
    SnapToPrecision(value.y),
    SnapToPrecision(value.z));
  public void SetScale(float value)
  {
    RawValue = value * Vector3.one;
    Value = RawValue;
    AfterScaling();
  }
  public void SetScaleX(float value)
  {
    RawValue.x = value;
    Value.x = value;
    AfterScaling();
  }
  public void SetScaleY(float value)
  {
    RawValue.y = value;
    Value.y = value;
    AfterScaling();
  }
  public void SetScaleZ(float value)
  {
    RawValue.z = value;
    Value.z = value;
    AfterScaling();
  }
  public void SetScale(Vector3 value)
  {
    RawValue = value;
    Value = RawValue;
    AfterScaling();
  }
  private void AfterScaling()
  {
    var player = Helper.GetPlayer();
    if (player.m_placementGhost)
      player.m_placementGhost.transform.localScale = Value;
  }
}
